using KylinService.Core;
using KylinService.Redis.Push.Model;
using KylinService.SysEnums;
using System;
using System.Collections.Generic;
using System.Linq;
using Td.Kylin.Entity;
using Td.Kylin.EnumLibrary;
using Td.Kylin.Redis;

namespace KylinService.Data.Settlement
{
    /// <summary>
    /// 精品汇（B2C）订单结算中心
    /// </summary>
    public class MallOrderSettlementCenter: SettlementCenter
    {
        /// <summary>
        /// 初始化B2C订单结算实例
        /// </summary>
        /// <param name="orderID">需要结算的订单ID</param>
        /// <param name="needProcessOrder">是否需要处理订单</param>
        public MallOrderSettlementCenter(long orderID, bool needProcessOrder)
        {
            using (var db = new DataContext())
            {
                var order = db.Mall_Order.SingleOrDefault(p => p.OrderID == orderID);

                _order = order;
            }

            _needProcessOrder = needProcessOrder;
        }

        /// <summary>
        /// 初始化B2C订单结算实例
        /// </summary>
        /// <param name="order">需要结算的订单</param>
        /// <param name="needProcessOrder">是否需要处理订单</param>
        public MallOrderSettlementCenter(Mall_Order order, bool needProcessOrder)
        {
            _order = order;
            _needProcessOrder = needProcessOrder;
        }

        /// <summary>
        /// 当前结算的订单
        /// </summary>
        private Mall_Order _order;

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

            if (_order.OrderStatus != (int)B2COrderStatus.WaitingReceipt)
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

                #region//2、得到区域运营商信息
                var operatorQuery = from op in db.Area_Operator
                                    join ao in db.Area_OperatorRelation
                                    on op.OperatorID equals ao.OperatorID
                                    where ao.AreaID == _order.AreaID
                                    select op;
                var operater = operatorQuery.FirstOrDefault();
                #endregion

                #region//3、检测区域运营商是否有效
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
                if (mainTransCode.Length != 23) mainTransCode = IDCreater.Instance.GetPlatformTransactionCode(PlatformTransactionType.BuyProduct, _order.AreaID);
                //业务序号
                int transIndex = 0;
                #endregion

                #region//6、货款结算给区域运营商
                string operatorGetSaleTransCode = mainTransCode + (++transIndex);
                db.Area_Operator.Attach(operater);
                db.Entry(operater).Property(p => p.Balance).IsModified = true;
                operater.Balance = operater.Balance + _order.ActualOrderAmount;

                //写入运营商交易记录
                var operatorSaleTrandsRecords = new AreaOperator_TradeRecords
                {
                    Amount = _order.ActualOrderAmount,
                    CounterpartyId = _order.UserID,
                    CounterpartyIdentity = (int)CounterpartyIdentity.User,
                    CreateTime = DateTime.Now,
                    DataID = _order.OrderID,
                    LastBalance = operater.Balance,
                    OpeartorID = operater.OperatorID,
                    PaymentType = (int)OnlinePaymentType.Balance,
                    PlatformTransactionCode = operatorGetSaleTransCode,
                    TradeID = IDCreater.Instance.GetID(),
                    TradeInfo = string.Format("销售商品“{0}”，订单编号：{1}", _order.ProductInfo, _order.OrderCode),
                    TradeNo = string.Empty,
                    TradeType = (int)OperatorTradeType.SaleProduct
                };
                db.AreaOperator_TradeRecords.Add(operatorSaleTrandsRecords);
                //写入平台资金流水
                db.Platform_MoneyTransaction.Add(new Platform_MoneyTransaction
                {
                    Amount = operatorSaleTrandsRecords.Amount,
                    AreaID = _order.AreaID,
                    CustomAccountID = operatorSaleTrandsRecords.OpeartorID,
                    CustomIdentity = (int)CounterpartyIdentity.AreaOperator,
                    CustomName = operatorProfile?.CompanyName,
                    IsMainTransaction = false,
                    LastBalance = operatorSaleTrandsRecords.LastBalance,
                    Remark = string.Format("销售商品“{0}”，订单编号：{1}", _order.ProductInfo, _order.OrderCode),
                    ThirdTransactionCode = string.Empty,
                    TransactionCode = operatorGetSaleTransCode,
                    TransactionTime = DateTime.Now,
                    TransactionType = (int)PlatformTransactionType.SaleProduct
                });
                #endregion

                #region//7、运营商返佣金给平台（平台抽成）

                //平台应抽成金额
                decimal commissionMoney = new PlatformCommissionCalculator(_order.AreaID, PlatformCommissionOption.B2COrder, _order.ActualOrderAmount).CommissionMoney;

                //需要抽佣时（抽佣金额>0）才进入
                if (commissionMoney > 0)
                {
                    //从余额中支付佣金给平台
                    operater.Balance = operater.Balance - commissionMoney;

                    //本次流水号
                    string operatorPayCommissionTransCode = mainTransCode + (++transIndex);
                    //记录本次操作，写入运营商交易记录
                    var operatorPayCommissionTrandsRecords = new AreaOperator_TradeRecords
                    {
                        Amount = -commissionMoney,
                        CounterpartyId = 0,
                        CounterpartyIdentity = (int)CounterpartyIdentity.Platform,
                        CreateTime = DateTime.Now,
                        DataID = _order.OrderID,
                        LastBalance = operater.Balance,
                        OpeartorID = operater.OperatorID,
                        PaymentType = (int)OnlinePaymentType.Balance,
                        PlatformTransactionCode = operatorPayCommissionTransCode,
                        TradeID = IDCreater.Instance.GetID(),
                        TradeInfo = string.Format("销售商品“{0}”返佣金给平台，订单编号：{1}", _order.ProductInfo, _order.OrderCode),
                        TradeNo = string.Empty,
                        TradeType = (int)OperatorTradeType.PayCommission
                    };
                    db.AreaOperator_TradeRecords.Add(operatorPayCommissionTrandsRecords);
                    //写入平台资金流水
                    db.Platform_MoneyTransaction.Add(new Platform_MoneyTransaction
                    {
                        Amount = operatorPayCommissionTrandsRecords.Amount,
                        AreaID = _order.AreaID,
                        CustomAccountID = operater.OperatorID,
                        CustomIdentity = (int)CounterpartyIdentity.AreaOperator,
                        CustomName = operatorProfile?.CompanyName,
                        IsMainTransaction = false,
                        LastBalance = operatorPayCommissionTrandsRecords.LastBalance,
                        Remark = string.Format("销售商品返佣金给平台，订单编号：{0}", _order.OrderCode),
                        ThirdTransactionCode = string.Empty,
                        TransactionCode = operatorPayCommissionTransCode,
                        TransactionTime = DateTime.Now,
                        TransactionType = (int)PlatformTransactionType.PayCommission
                    });
                }
                #endregion

                #region//8、更新订单状态
                if (_needProcessOrder)
                {
                    db.Mall_Order.Attach(_order);
                    db.Entry(_order).Property(p => p.OrderStatus).IsModified = true;
                    db.Entry(_order).Property(p => p.ReceivedTime).IsModified = true;

                    //修改订单状态为已完成
                    _order.OrderStatus = (int)B2COrderStatus.Done;
                    _order.ReceivedTime = DateTime.Now;
                }
                #endregion

                #region//9、用户积分奖励
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

                #region//10、用户经验值奖励
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

                    #region//11、消息推送
                    try
                    {
                        var pushRedis = Startup.PushRedisConfigs[RedisPushType.B2COrderReceivedGoods];

                        if (null != pushRedis)
                        {
                            var msgContent = new B2COrderReceivedGoodsContent
                            {
                                ActualOrderAmount = _order.ActualOrderAmount,
                                OperatorID = operater.OperatorID,
                                OrderCode = _order.OrderCode,
                                OrderID = _order.OrderID,
                                ProductInfo = _order.ProductInfo,
                                ReceivedTime = _order.ReceivedTime ?? DateTime.Now,
                                UserID = _order.UserID,
                                UserName = user.Username
                            };
                            pushRedis.DataBase.ListRightPush<B2COrderReceivedGoodsContent>(pushRedis.Key, msgContent);
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
