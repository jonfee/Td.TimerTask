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
    public class MerchantOrderSettlementCenter
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

                Order = order;
            }

            NeedProcessOrder = needProcessOrder;
        }

        /// <summary>
        /// 初始化商家订单结算实例
        /// </summary>
        /// <param name="order">需要结算的订单</param>
        /// <param name="needProcessOrder">是否需要处理订单</param>
        public MerchantOrderSettlementCenter(Merchant_Order order, bool needProcessOrder)
        {
            Order = order;
            NeedProcessOrder = needProcessOrder;
        }

        /// <summary>
        /// 当前结算的订单
        /// </summary>
        public Merchant_Order Order { get; private set; }

        /// <summary>
        /// 是否需要处理订单
        /// </summary>
        public bool NeedProcessOrder { get; private set; }

        /// <summary>
        /// 结算结果
        /// </summary>
        public bool Success
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
        public string ErrorMessage
        {
            get { return _errorMessage; }
        }

        /// <summary>
        /// 执行结算
        /// </summary>
        public void Execute()
        {
            #region//检测订单有效性

            if (null == Order)
            {
                _errorMessage = "订单数据不存在！";
                return;
            }

            if (Order.OrderStatus != (int)MerchantOrderStatus.WaitingReceipt)
            {
                _errorMessage = "当前订单状态不适合进行结算处理！";
                return;
            }

            #endregion

            #region //结算处理

            using (var db = new DataContext())
            {
                #region//1、获取用户资金并检测
                var user = db.User_Account.SingleOrDefault(p => p.UserID == Order.UserID);
                if (null == user)
                {
                    _errorMessage = "用户数据异常，未检测到用户信息！";
                    return;
                }
                if (user.FreezeMoney < Order.ActualOrderAmount)
                {
                    _errorMessage = "呼叫程序猿，用户钱跑哪去了？冻结资金结算不了当前订单！";
                    return;
                }
                #endregion

                #region//2、获得商家信息
                var merchant = db.Merchant_Account.SingleOrDefault(p => p.MerchantID == Order.MerchantID);
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
                user.FreezeMoney -= Order.ActualOrderAmount;
                #endregion

                #region//5、获取用户支付时的交易记录信息（从中获取平台交易流水号）
                var userTradeRecord = db.User_TradeRecords.Where(p => p.DataID == Order.OrderID).FirstOrDefault();

                string mainTransCode = null != userTradeRecord ? userTradeRecord.PlatformTransactionCode : string.Empty;
                if (!string.IsNullOrWhiteSpace(mainTransCode) && mainTransCode.Length == 24)
                {
                    mainTransCode = mainTransCode.Remove(23, 1);
                }
                if (mainTransCode.Length != 23) mainTransCode = IDCreater.Instance.GetPlatformTransactionCode(PlatformTransactionType.BuyProduct, merchant.AreaID);
                //业务序号
                int transIndex = 0;
                #endregion

                #region//6、货款结算给商家
                string merchantGetSaleTransCode = mainTransCode + (++transIndex);
                db.Merchant_Account.Attach(merchant);
                db.Entry(merchant).Property(p => p.Balance).IsModified = true;
                merchant.Balance = merchant.Balance + Order.ActualOrderAmount;

                //写入商家交易记录
                var merchantSaleTrandsRecords = new Merchant_TradeRecords
                {
                    Amount = Order.ActualOrderAmount,
                    CounterpartyId = Order.UserID,
                    CounterpartyIdentity = (int)CounterpartyIdentity.User,
                    CreateTime = DateTime.Now,
                    DataID = Order.OrderID,
                    LastBalance = merchant.Balance,
                    MerchantID = merchant.MerchantID,
                    PaymentType = (int)OnlinePaymentType.Balance,
                    PlatformTransactionCode = merchantGetSaleTransCode,
                    TradeID = IDCreater.Instance.GetID(),
                    TradeInfo = string.Format("销售商品，订单编号：{0}", Order.OrderCode),
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
                    Remark = string.Format("销售商品，订单编号：{0}", Order.OrderCode),
                    ThirdTransactionCode = string.Empty,
                    TransactionCode = merchantGetSaleTransCode,
                    TransactionTime = DateTime.Now,
                    TransactionType = (int)PlatformTransactionType.SaleProduct
                });
                #endregion

                #region //7、商家返佣金给运营商  //8、运营商返佣金给平台（平台抽成）
                //总抽成
                decimal totalCommissionMoney = 0;
                //运营商针对商家抽成配置
                var merchantCommission = CacheCollection.AreaForMerchantCommissionCache.Get(areaID, merchant.MerchantID, (int)AreaMerchantCommissionOption.MerchantProductOrder);
                if (null != merchantCommission && merchantCommission.Value > 0)
                {
                    if (merchantCommission.CommissionType == (int)CommissionType.FixedAmount)
                    {
                        totalCommissionMoney = merchantCommission.Value;
                    }
                    else if (merchantCommission.CommissionType == (int)CommissionType.MoneyRate)
                    {
                        totalCommissionMoney = Math.Round(Order.ActualOrderAmount * merchantCommission.Value * 0.01M, MidpointRounding.ToEven);
                    }
                }

                //抽成金额>0时
                if (totalCommissionMoney > 0)
                {
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
                        DataID = Order.OrderID,
                        LastBalance = merchant.Balance,
                        MerchantID = Order.MerchantID,
                        PaymentType = (int)OnlinePaymentType.Balance,
                        PlatformTransactionCode = merchantReturnTransCode,
                        TradeID = IDCreater.Instance.GetID(),
                        TradeInfo = string.Format("销售商品返佣金给运营商，订单编号：{0}", Order.OrderCode),
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
                        Remark = string.Format("销售商品返佣金给运营商，订单编号：{0}", Order.OrderCode),
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
                        CounterpartyId = Order.MerchantID,
                        CounterpartyIdentity = (int)CounterpartyIdentity.Merchant,
                        CreateTime = DateTime.Now,
                        DataID = Order.OrderID,
                        LastBalance = operater.Balance,
                        OpeartorID = operater.OperatorID,
                        PaymentType = (int)OnlinePaymentType.Balance,
                        PlatformTransactionCode = operatorGetReturnCommissionTransCode,
                        TradeID = IDCreater.Instance.GetID(),
                        TradeInfo = string.Format("从商家销售商品中抽成，订单编号：{0}", Order.OrderCode),
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
                        Remark = string.Format("从商家销售商品中抽成，订单编号：{0}", Order.OrderCode),
                        ThirdTransactionCode = string.Empty,
                        TransactionCode = operatorGetReturnCommissionTransCode,
                        TransactionTime = DateTime.Now,
                        TransactionType = (int)PlatformTransactionType.CommissionGet
                    });
                    #endregion

                    #region//8、运营商返佣金给平台（平台抽成）
                    decimal platformCommissionMoney = 0;//平台应抽成金额
                    //平台对当前区域的抽成配置
                    var platformCommission = CacheCollection.PlatformCommissionCache.Get(areaID, (int)PlatformCommissionOption.B2COrder);
                    if (null != platformCommission && platformCommission.Value > 0)
                    {
                        if (platformCommission.CommissionType == (int)CommissionType.FixedAmount)
                        {
                            platformCommissionMoney = platformCommission.Value;
                        }
                        else if (platformCommission.CommissionType == (int)CommissionType.MoneyRate)
                        {
                            platformCommissionMoney = Math.Round(totalCommissionMoney * platformCommission.Value * 0.01M, MidpointRounding.ToEven);
                        }
                    }

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
                            DataID = Order.OrderID,
                            LastBalance = operater.Balance,
                            OpeartorID = operater.OperatorID,
                            PaymentType = (int)OnlinePaymentType.Balance,
                            PlatformTransactionCode = operatorPayCommissionTransCode,
                            TradeID = IDCreater.Instance.GetID(),
                            TradeInfo = string.Format("从商家订单抽成后返佣金给平台，订单编号：{0}", Order.OrderCode),
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
                            Remark = string.Format("从商家订单抽成后返佣金给平台，订单编号：{0}", Order.OrderCode),
                            ThirdTransactionCode = string.Empty,
                            TransactionCode = operatorPayCommissionTransCode,
                            TransactionTime = DateTime.Now,
                            TransactionType = (int)PlatformTransactionType.PayCommission
                        });
                    }
                    #endregion
                }

                #endregion

                #region//9、更新订单状态
                if (NeedProcessOrder)
                {
                    db.Merchant_Order.Attach(Order);
                    db.Entry(Order).Property(p => p.OrderStatus).IsModified = true;
                    db.Entry(Order).Property(p => p.ReceivedTime).IsModified = true;

                    //修改订单状态为已完成
                    Order.OrderStatus = (int)MerchantOrderStatus.Done;
                    Order.ReceivedTime = DateTime.Now;
                }
                #endregion

                #region//10、用户积分奖励
                var pointCalc = new PointCalculator(Order.UserID, UserActivityType.OrderFinish);
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
                        Remark = string.Format("订单（编号：{0}）交易完成，获得{1}积分。", Order.OrderCode, points),
                        Score = points,
                        UserID = Order.UserID
                    });
                }
                #endregion

                #region//11、用户经验值奖励
                var empiricalCalc = new EmpiricalCalculator(Order.UserID, UserActivityType.OrderFinish);
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
                        Remark = string.Format("订单（编号：{0}）交易完成，获得{1}点经验值。", Order.OrderCode, empirical),
                        Score = empirical,
                        UserID = Order.UserID
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
                                ActualOrderAmount = Order.ActualOrderAmount,
                                OrderCode = Order.OrderCode,
                                OrderID = Order.OrderID,
                                ReceivedTime = Order.ReceivedTime ?? DateTime.Now,
                                UserID = Order.UserID,
                                UserName = user.Username,
                                MerchantID = Order.MerchantID,
                                OrderStatus = Order.OrderStatus
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
