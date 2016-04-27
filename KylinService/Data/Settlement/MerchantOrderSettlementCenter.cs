using KylinService.Core;
using KylinService.Redis.Push.Model;
using KylinService.SysEnums;
using System;
using System.Collections.Generic;
using System.Linq;
using Td.Kylin.DataCache;
using Td.Kylin.Entity;
using Td.Kylin.EnumLibrary;
using Td.Kylin.Redis;

namespace KylinService.Data.Settlement
{
    /// <summary>
    /// 附近购（商家）订单结算中心
    /// </summary>
    public class MerchantOrderSettlementCenter : SettlementCenter
    {
        /// <summary>
        /// 初始化商家订单结算实例
        /// </summary>
        /// <param name="orderID">需要结算的订单ID</param>
        /// <param name="needProcessOrder">是否需要处理订单</param>
        public MerchantOrderSettlementCenter(long orderID, bool needProcessOrder)
        {
            using (var db = new DataContext())
            {
                var order = db.Merchant_Order.SingleOrDefault(p => p.OrderID == orderID);

                _order = order;
            }

            _needProcessOrder = needProcessOrder;
        }

        /// <summary>
        /// 初始化商家订单结算实例
        /// </summary>
        /// <param name="order">需要结算的订单</param>
        /// <param name="needProcessOrder">是否需要处理订单</param>
        public MerchantOrderSettlementCenter(Merchant_Order order, bool needProcessOrder)
        {
            _order = order;
            _needProcessOrder = needProcessOrder;
        }

        /// <summary>
        /// 当前结算的订单
        /// </summary>
        private Merchant_Order _order;

        /// <summary>
        /// 是否需要处理订单
        /// </summary>
        private bool _needProcessOrder;

        /// <summary>
        /// 结算结果
        /// </summary>
        public override bool Success
        {
            get
            {
                return string.IsNullOrWhiteSpace(ErrorMessage);
            }
        }

        private string _errorMessage = "error";
        /// <summary>
        /// 错误消息
        /// </summary>
        public override string ErrorMessage
        {
            get { return _errorMessage; }
        }

        /// <summary>
        /// 执行结算
        /// </summary>
        public override void Execute()
        {
            #region//检测订单有效性

            if (null == _order)
            {
                _errorMessage = "订单数据不存在！";
                return;
            }

            if (_order.OrderStatus != (int)MerchantOrderStatus.WaitingReceipt)
            {
                _errorMessage = "当前订单状态不适合进行结算处理！";
                return;
            }

            #endregion

            #region //结算处理

            using (var db = new DataContext())
            {
                #region//1、获取用户资金并检测
                var user = db.User_Account.SingleOrDefault(p => p.UserID == _order.UserID);
                if (null == user)
                {
                    _errorMessage = "用户数据异常，未检测到用户信息！";
                    return;
                }
                if (user.FreezeMoney < _order.ActualOrderAmount)
                {
                    _errorMessage = "呼叫程序猿，用户钱跑哪去了？冻结资金结算不了当前订单！";
                    return;
                }
                #endregion

                #region//2、获得商家信息
                var merchant = db.Merchant_Account.SingleOrDefault(p => p.MerchantID == _order.MerchantID);
                if (null == merchant)
                {
                    _errorMessage = "哇哦，商家呢？跑哪去了，不见了？钱给谁？给谁？给谁……";
                    return;
                }
                #endregion

                #region//3、得到区域运营商信息
                //区域（大区域，即运营商区域）
                var areaID = merchant.AreaID;
                var openAreas = CacheCollection.OpenAreaCache.Value();
                var areaLayers = merchant.AreaLayer.Split(new[] { ',', '|' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var area in openAreas)
                {
                    if (areaLayers.Contains(area.AreaID.ToString()))
                    {
                        areaID = area.AreaID;
                        break;
                    }
                }

                var operatorQuery = from op in db.Area_Operator
                                    join ao in db.Area_OperatorRelation
                                    on op.OperatorID equals ao.OperatorID
                                    where ao.AreaID == areaID
                                    select op;
                var operater = operatorQuery.FirstOrDefault();
                if (null == operater)
                {
                    _errorMessage = "启奏陛下，订单所在区域运营商不存在，微臣不敢擅作主张，还请陛下圣断！";
                    return;
                }
                //运营商基本资料
                var operatorProfile = db.Area_OperatorProfile.SingleOrDefault(p => p.OperatorID == operater.OperatorID);
                #endregion

                #region//4、用户货款从冻结金额中结算并写入交易记录、资金明细
                db.User_Account.Attach(user);
                db.Entry(user).Property(p => p.FreezeMoney).IsModified = true;
                user.FreezeMoney -= _order.ActualOrderAmount;
                #endregion

                #region//5、获取用户支付时的交易记录信息（从中获取平台交易流水号）
                var userTradeRecord = db.User_TradeRecords.Where(p => p.DataID == _order.OrderID).FirstOrDefault();

                string mainTransCode = null != userTradeRecord ? userTradeRecord.PlatformTransactionCode : string.Empty;
                if (!string.IsNullOrWhiteSpace(mainTransCode) && mainTransCode.Length == 24)
                {
                    mainTransCode = mainTransCode.Remove(23, 1);
                }
                if (mainTransCode.Length != 23) mainTransCode = IDCreater.Instance.GetPlatformTransactionCode(PlatformTransactionType.BuyProduct, areaID);
                //业务序号
                int transIndex = 0;
                #endregion

                #region//6、货款结算给商家
                string merchantGetSaleTransCode = mainTransCode + (++transIndex);
                db.Merchant_Account.Attach(merchant);
                db.Entry(merchant).Property(p => p.Balance).IsModified = true;
                merchant.Balance = merchant.Balance + _order.ActualOrderAmount;

                //写入商家交易记录
                var merchantSaleTrandsRecords = new Merchant_TradeRecords
                {
                    Amount = _order.ActualOrderAmount,
                    CounterpartyId = _order.UserID,
                    CounterpartyIdentity = (int)CounterpartyIdentity.User,
                    CreateTime = DateTime.Now,
                    DataID = _order.OrderID,
                    LastBalance = merchant.Balance,
                    MerchantID = merchant.MerchantID,
                    PaymentType = (int)OnlinePaymentType.Balance,
                    PlatformTransactionCode = merchantGetSaleTransCode,
                    TradeID = IDCreater.Instance.GetID(),
                    TradeInfo = string.Format("销售商品，订单编号：{0}", _order.OrderCode),
                    TradeNo = string.Empty,
                    TradeType = (int)MerchantTransType.ProductSales
                };
                db.Merchant_TradeRecords.Add(merchantSaleTrandsRecords);

                //写入平台资金流水
                db.Platform_MoneyTransaction.Add(new Platform_MoneyTransaction
                {
                    Amount = merchantSaleTrandsRecords.Amount,
                    AreaID = areaID,
                    CustomAccountID = merchantSaleTrandsRecords.MerchantID,
                    CustomIdentity = (int)CounterpartyIdentity.Merchant,
                    CustomName = merchant.Name,
                    IsMainTransaction = false,
                    LastBalance = merchantSaleTrandsRecords.LastBalance,
                    Remark = string.Format("销售商品，订单编号：{0}", _order.OrderCode),
                    ThirdTransactionCode = string.Empty,
                    TransactionCode = merchantGetSaleTransCode,
                    TransactionTime = DateTime.Now,
                    TransactionType = (int)PlatformTransactionType.SaleProduct
                });
                #endregion
                
                //总抽成
                decimal totalCommissionMoney = new AreaForMerchantCommissionCalculator(areaID, _order.MerchantID, AreaMerchantCommissionOption.MerchantProductOrder, _order.ActualOrderAmount).CommissionMoney;

                //抽成金额>0时
                if (totalCommissionMoney > 0)
                {
                    #region//7、区域运营商抽成

                    #region//商家返佣给运营商
                    merchant.Balance -= totalCommissionMoney;
                    string merchantReturnTransCode = mainTransCode + (++transIndex);
                    //写入商家返佣记录
                    var merchantReturnTrandsRecords = new Merchant_TradeRecords
                    {
                        Amount = -totalCommissionMoney,
                        CounterpartyId = operater.OperatorID,
                        CounterpartyIdentity = (int)CounterpartyIdentity.AreaOperator,
                        CreateTime = DateTime.Now,
                        DataID = _order.OrderID,
                        LastBalance = merchant.Balance,
                        MerchantID = _order.MerchantID,
                        PaymentType = (int)OnlinePaymentType.Balance,
                        PlatformTransactionCode = merchantReturnTransCode,
                        TradeID = IDCreater.Instance.GetID(),
                        TradeInfo = string.Format("销售商品返佣金给运营商，订单编号：{0}", _order.OrderCode),
                        TradeNo = string.Empty,
                        TradeType = (int)MerchantTransType.ReturnCommissionToOperator
                    };
                    db.Merchant_TradeRecords.Add(merchantReturnTrandsRecords);

                    //写入平台资金流水
                    db.Platform_MoneyTransaction.Add(new Platform_MoneyTransaction
                    {
                        Amount = merchantReturnTrandsRecords.Amount,
                        AreaID = areaID,
                        CustomAccountID = merchantReturnTrandsRecords.MerchantID,
                        CustomIdentity = (int)CounterpartyIdentity.Merchant,
                        CustomName = merchant.Name,
                        IsMainTransaction = false,
                        LastBalance = merchantReturnTrandsRecords.LastBalance,
                        Remark = string.Format("销售商品返佣金给运营商，订单编号：{0}", _order.OrderCode),
                        ThirdTransactionCode = string.Empty,
                        TransactionCode = merchantReturnTransCode,
                        TransactionTime = DateTime.Now,
                        TransactionType = (int)PlatformTransactionType.PayCommission
                    });
                    #endregion

                    #region//运营商接收商家返佣
                    db.Area_Operator.Attach(operater);
                    db.Entry(operater).Property(p => p.Balance).IsModified = true;
                    operater.Balance += totalCommissionMoney;
                    string operatorGetReturnCommissionTransCode = mainTransCode + (++transIndex);
                    //写入运营商交易记录
                    var operatorGetReturnCommissionTrandsRecords = new AreaOperator_TradeRecords
                    {
                        Amount = totalCommissionMoney,
                        CounterpartyId = _order.MerchantID,
                        CounterpartyIdentity = (int)CounterpartyIdentity.Merchant,
                        CreateTime = DateTime.Now,
                        DataID = _order.OrderID,
                        LastBalance = operater.Balance,
                        OpeartorID = operater.OperatorID,
                        PaymentType = (int)OnlinePaymentType.Balance,
                        PlatformTransactionCode = operatorGetReturnCommissionTransCode,
                        TradeID = IDCreater.Instance.GetID(),
                        TradeInfo = string.Format("从商家销售商品中抽成，订单编号：{0}", _order.OrderCode),
                        TradeNo = string.Empty,
                        TradeType = (int)OperatorTradeType.CommissionGet
                    };
                    db.AreaOperator_TradeRecords.Add(operatorGetReturnCommissionTrandsRecords);
                    //写入平台资金流水
                    db.Platform_MoneyTransaction.Add(new Platform_MoneyTransaction
                    {
                        Amount = operatorGetReturnCommissionTrandsRecords.Amount,
                        AreaID = areaID,
                        CustomAccountID = operatorGetReturnCommissionTrandsRecords.OpeartorID,
                        CustomIdentity = (int)CounterpartyIdentity.AreaOperator,
                        CustomName = operatorProfile?.CompanyName,
                        IsMainTransaction = false,
                        LastBalance = operatorGetReturnCommissionTrandsRecords.LastBalance,
                        Remark = string.Format("从商家销售商品中抽成，订单编号：{0}", _order.OrderCode),
                        ThirdTransactionCode = string.Empty,
                        TransactionCode = operatorGetReturnCommissionTransCode,
                        TransactionTime = DateTime.Now,
                        TransactionType = (int)PlatformTransactionType.CommissionGet
                    });
                    #endregion

                    #endregion

                    #region//8、运营商返佣金给平台（平台抽成）
                    //平台应抽成金额
                    decimal platformCommissionMoney = new PlatformCommissionCalculator(areaID, PlatformCommissionOption.AreaCommissionByMerchantOrder, totalCommissionMoney).CommissionMoney;

                    //需要抽佣时（抽佣金额>0）才进入
                    if (platformCommissionMoney > 0)
                    {
                        //从余额中支付佣金给平台
                        operater.Balance = operater.Balance - platformCommissionMoney;

                        //本次流水号
                        string operatorPayCommissionTransCode = mainTransCode + (++transIndex);
                        //记录本次操作，写入运营商交易记录
                        var operatorPayCommissionTrandsRecords = new AreaOperator_TradeRecords
                        {
                            Amount = -platformCommissionMoney,
                            CounterpartyId = 0,
                            CounterpartyIdentity = (int)CounterpartyIdentity.Platform,
                            CreateTime = DateTime.Now,
                            DataID = _order.OrderID,
                            LastBalance = operater.Balance,
                            OpeartorID = operater.OperatorID,
                            PaymentType = (int)OnlinePaymentType.Balance,
                            PlatformTransactionCode = operatorPayCommissionTransCode,
                            TradeID = IDCreater.Instance.GetID(),
                            TradeInfo = string.Format("从商家订单抽成后返佣金给平台，订单编号：{0}", _order.OrderCode),
                            TradeNo = string.Empty,
                            TradeType = (int)OperatorTradeType.PayCommission
                        };
                        db.AreaOperator_TradeRecords.Add(operatorPayCommissionTrandsRecords);
                        //写入平台资金流水
                        db.Platform_MoneyTransaction.Add(new Platform_MoneyTransaction
                        {
                            Amount = operatorPayCommissionTrandsRecords.Amount,
                            AreaID = areaID,
                            CustomAccountID = operater.OperatorID,
                            CustomIdentity = (int)CounterpartyIdentity.AreaOperator,
                            CustomName = operatorProfile?.CompanyName,
                            IsMainTransaction = false,
                            LastBalance = operatorPayCommissionTrandsRecords.LastBalance,
                            Remark = string.Format("从商家订单抽成后返佣金给平台，订单编号：{0}", _order.OrderCode),
                            ThirdTransactionCode = string.Empty,
                            TransactionCode = operatorPayCommissionTransCode,
                            TransactionTime = DateTime.Now,
                            TransactionType = (int)PlatformTransactionType.PayCommission
                        });
                    }
                    #endregion
                }
                
                #region//9、更新订单状态
                if (_needProcessOrder)
                {
                    db.Merchant_Order.Attach(_order);
                    db.Entry(_order).Property(p => p.OrderStatus).IsModified = true;
                    db.Entry(_order).Property(p => p.ReceivedTime).IsModified = true;

                    //修改订单状态为已完成
                    _order.OrderStatus = (int)MerchantOrderStatus.Done;
                    _order.ReceivedTime = DateTime.Now;
                }
                #endregion

                #region//10、用户积分奖励
                var pointCalc = new PointCalculator(_order.UserID, UserActivityType.OrderFinish);
                if (pointCalc.CanContinue)
                {
                    int points = pointCalc.Score;

                    //更新用户积分
                    db.Entry(user).Property(p => p.Points).IsModified = true;
                    user.Points += points;

                    //积分获取记录
                    db.User_PointsRecords.Add(new User_PointsRecords
                    {
                        ActivityType = (int)UserActivityType.OrderFinish,
                        CreateTime = DateTime.Now,
                        RecordsID = IDCreater.Instance.GetID(),
                        Remark = string.Format("订单（编号：{0}）交易完成，获得{1}积分。", _order.OrderCode, points),
                        Score = points,
                        UserID = _order.UserID
                    });
                }
                #endregion

                #region//11、用户经验值奖励
                var empiricalCalc = new EmpiricalCalculator(_order.UserID, UserActivityType.OrderFinish);
                if (empiricalCalc.CanContinue)
                {
                    int empirical = empiricalCalc.Score;

                    //更新用户经验值
                    db.Entry(user).Property(p => p.Empirical).IsModified = true;
                    user.Empirical += empirical;

                    //经验值获取记录
                    db.User_EmpiricalRecords.Add(new User_EmpiricalRecords
                    {
                        ActivityType = (int)UserActivityType.OrderFinish,
                        CreateTime = DateTime.Now,
                        RecordsID = IDCreater.Instance.GetID(),
                        Remark = string.Format("订单（编号：{0}）交易完成，获得{1}点经验值。", _order.OrderCode, empirical),
                        Score = empirical,
                        UserID = _order.UserID
                    });
                }
                #endregion

                //返回处理结果
                if (db.SaveChanges() < 1)
                {
                    _errorMessage = "操作失败！";
                    return;
                }
                else
                {
                    _errorMessage = null;

                    #region//12、消息推送
                    try
                    {
                        var pushRedis = Startup.PushRedisConfigs[RedisPushType.MerchantOrderReceivedGoods];

                        if (null != pushRedis)
                        {
                            var msgContent = new MerchantOrderReceivedGoodsContent
                            {
                                ActualOrderAmount = _order.ActualOrderAmount,
                                OrderCode = _order.OrderCode,
                                OrderID = _order.OrderID,
                                ReceivedTime = _order.ReceivedTime ?? DateTime.Now,
                                UserID = _order.UserID,
                                UserName = user.Username,
                                MerchantID = _order.MerchantID
                            };
                            pushRedis.DataBase.ListRightPush(pushRedis.Key, msgContent);
                        }
                    }
                    catch { }
                    #endregion
                }
            }
            #endregion
        }
    }
}
