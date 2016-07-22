using KylinService.Core;
using KylinService.Redis.Push;
using KylinService.Redis.Push.Model;
using KylinService.SysEnums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Td.Kylin.Entity;
using Td.Kylin.EnumLibrary;
using Td.Kylin.Redis;

namespace KylinService.Data.Settlement
{
    /// <summary>
    /// 上门订单结算中心
    /// </summary>
    public class VisitingOrderSettlementCenter : SettlementCenter
    {
        /// <summary>
        /// 初始化上门订单结算实例
        /// </summary>
        /// <param name="orderID">需要结算的订单ID</param>
        /// <param name="needProcessOrder">是否需要处理订单</param>
        public VisitingOrderSettlementCenter(long orderID, bool needProcessOrder)
        {
            using (var db = new DataContext())
            {
                var order = db.KylinService_Order.SingleOrDefault(p => p.OrderID == orderID);

                _order = order;
            }

            _needProcessOrder = needProcessOrder;
        }

        /// <summary>
        /// 初始化上门订单结算实例
        /// </summary>
        /// <param name="order">需要结算的订单</param>
        /// <param name="needProcessOrder">是否需要处理订单</param>
        public VisitingOrderSettlementCenter(KylinService_Order order, bool needProcessOrder)
        {
            _order = order;
            _needProcessOrder = needProcessOrder;
        }

        /// <summary>
        /// 数据上下文
        /// </summary>
        private DataContext _context;

        /// <summary>
        /// 当前结算的订单
        /// </summary>
        private KylinService_Order _order;

        /// <summary>
        /// 订单结算的金额
        /// </summary>
        private decimal _settlementAmount;

        /// <summary>
        /// 当前订单服务受用用户
        /// </summary>
        private User_Account _user;

        /// <summary>
        /// 当前订单服务个人工作人员
        /// </summary>
        private User_Account _worker;

        /// <summary>
        /// 当前订单服务商家
        /// </summary>
        private Merchant_Account _merchant;

        /// <summary>
        /// 当前订单区域运营商
        /// </summary>
        private Area_Operator _operator;

        /// <summary>
        /// 当前订单区域运营商资料
        /// </summary>
        private Area_OperatorProfile _operatorProfile;

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
        /// 用户支付给商家
        /// </summary>
        /// <param name="error"></param>
        void UserPaymentToMerchant()
        {
            #region//1、用户货款从冻结金额中结算
            if (_user.FreezeMoney < _settlementAmount)
            {
                _errorMessage = "呼叫程序猿，用户钱跑哪去了？冻结资金结算不了当前订单！";
                return;
            }
            _user.FreezeMoney -= _settlementAmount;
            #endregion

            #region //2、获取用户支付时的交易记录信息（从中获取平台交易流水号）
            var userTradeRecord = _context.User_TradeRecords.Where(p => p.DataID == _order.OrderID).FirstOrDefault();
            string mainTransCode = null != userTradeRecord ? userTradeRecord.PlatformTransactionCode : string.Empty;
            if (!string.IsNullOrWhiteSpace(mainTransCode) && mainTransCode.Length == 24)
            {
                mainTransCode = mainTransCode.Remove(23, 1);
            }
            if (mainTransCode.Length != 23) mainTransCode = IDCreater.Instance.GetPlatformTransactionCode(PlatformTransactionType.BuyService, _order.AreaID);
            //业务序号
            int transIndex = 0;
            #endregion

            #region //3、商家获得货款并记录
            string merchantGetSaleTransCode = mainTransCode + (++transIndex);
            _merchant.Balance = _merchant.Balance + _settlementAmount;

            //写入商家交易记录
            var merchantSaleTrandsRecords = new Merchant_TradeRecords
            {
                Amount = _settlementAmount,
                CounterpartyId = _order.UserID,
                CounterpartyIdentity = (int)CounterpartyIdentity.User,
                CreateTime = DateTime.Now,
                DataID = _order.OrderID,
                LastBalance = _merchant.Balance,
                MerchantID = _merchant.MerchantID,
                PaymentType = (int)OnlinePaymentType.Balance,
                PlatformTransactionCode = merchantGetSaleTransCode,
                TradeID = IDCreater.Instance.GetID(),
                TradeInfo = string.Format("提供上门服务，订单编号：{0}", _order.OrderCode),
                TradeNo = string.Empty,
                TradeType = (int)MerchantTransType.ServiceSales
            };
            _context.Merchant_TradeRecords.Add(merchantSaleTrandsRecords);

            //写入平台资金流水
            _context.Platform_MoneyTransaction.Add(new Platform_MoneyTransaction
            {
                Amount = merchantSaleTrandsRecords.Amount,
                AreaID = _order.AreaID,
                CustomAccountID = merchantSaleTrandsRecords.MerchantID,
                CustomIdentity = (int)CounterpartyIdentity.Merchant,
                CustomName = _merchant.Name,
                IsMainTransaction = false,
                LastBalance = merchantSaleTrandsRecords.LastBalance,
                Remark = string.Format("提供上门服务，订单编号：{0}", _order.OrderCode),
                ThirdTransactionCode = string.Empty,
                TransactionCode = merchantGetSaleTransCode,
                TransactionTime = DateTime.Now,
                TransactionType = (int)PlatformTransactionType.SaleService
            });
            #endregion

            //总抽成（指商家被抽）
            decimal totalCommissionMoney = new AreaForMerchantCommissionCalculator(_order.AreaID, _order.MerchantID, AreaMerchantCommissionOption.MerchantServiceOrder, _settlementAmount).CommissionMoney;

            if (totalCommissionMoney > 0)
            {
                #region//4、区域运营商抽成

                #region//商家返佣给运营商
                _merchant.Balance -= totalCommissionMoney;
                string merchantReturnTransCode = mainTransCode + (++transIndex);
                //写入商家返佣记录
                var merchantReturnTrandsRecords = new Merchant_TradeRecords
                {
                    Amount = -totalCommissionMoney,
                    CounterpartyId = _operator.OperatorID,
                    CounterpartyIdentity = (int)CounterpartyIdentity.AreaOperator,
                    CreateTime = DateTime.Now,
                    DataID = _order.OrderID,
                    LastBalance = _merchant.Balance,
                    MerchantID = _order.MerchantID,
                    PaymentType = (int)OnlinePaymentType.Balance,
                    PlatformTransactionCode = merchantReturnTransCode,
                    TradeID = IDCreater.Instance.GetID(),
                    TradeInfo = string.Format("上门服务返佣金给运营商，订单编号：{0}", _order.OrderCode),
                    TradeNo = string.Empty,
                    TradeType = (int)MerchantTransType.ReturnCommissionToOperator
                };
                _context.Merchant_TradeRecords.Add(merchantReturnTrandsRecords);

                //写入平台资金流水
                _context.Platform_MoneyTransaction.Add(new Platform_MoneyTransaction
                {
                    Amount = merchantReturnTrandsRecords.Amount,
                    AreaID = _order.AreaID,
                    CustomAccountID = merchantReturnTrandsRecords.MerchantID,
                    CustomIdentity = (int)CounterpartyIdentity.Merchant,
                    CustomName = _merchant.Name,
                    IsMainTransaction = false,
                    LastBalance = merchantReturnTrandsRecords.LastBalance,
                    Remark = string.Format("上门服务返佣金给运营商，订单编号：{0}", _order.OrderCode),
                    ThirdTransactionCode = string.Empty,
                    TransactionCode = merchantReturnTransCode,
                    TransactionTime = DateTime.Now,
                    TransactionType = (int)PlatformTransactionType.PayCommission
                });
                #endregion

                #region//运营商接收商家返佣
                _operator.Balance += totalCommissionMoney;
                string operatorGetReturnCommissionTransCode = mainTransCode + (++transIndex);
                //写入运营商交易记录
                var operatorGetReturnCommissionTrandsRecords = new AreaOperator_TradeRecords
                {
                    Amount = totalCommissionMoney,
                    CounterpartyId = _order.MerchantID,
                    CounterpartyIdentity = (int)CounterpartyIdentity.Merchant,
                    CreateTime = DateTime.Now,
                    DataID = _order.OrderID,
                    LastBalance = _operator.Balance,
                    OpeartorID = _operator.OperatorID,
                    PaymentType = (int)OnlinePaymentType.Balance,
                    PlatformTransactionCode = operatorGetReturnCommissionTransCode,
                    TradeID = IDCreater.Instance.GetID(),
                    TradeInfo = string.Format("从商家上门服务中抽成，订单编号：{0}", _order.OrderCode),
                    TradeNo = string.Empty,
                    TradeType = (int)OperatorTradeType.CommissionGet
                };
                _context.AreaOperator_TradeRecords.Add(operatorGetReturnCommissionTrandsRecords);
                //写入平台资金流水
                _context.Platform_MoneyTransaction.Add(new Platform_MoneyTransaction
                {
                    Amount = operatorGetReturnCommissionTrandsRecords.Amount,
                    AreaID = _order.AreaID,
                    CustomAccountID = operatorGetReturnCommissionTrandsRecords.OpeartorID,
                    CustomIdentity = (int)CounterpartyIdentity.AreaOperator,
                    CustomName = _operatorProfile?.CompanyName,
                    IsMainTransaction = false,
                    LastBalance = operatorGetReturnCommissionTrandsRecords.LastBalance,
                    Remark = string.Format("从商家上门服务中抽成，订单编号：{0}", _order.OrderCode),
                    ThirdTransactionCode = string.Empty,
                    TransactionCode = operatorGetReturnCommissionTransCode,
                    TransactionTime = DateTime.Now,
                    TransactionType = (int)PlatformTransactionType.CommissionGet
                });
                #endregion

                #endregion

                #region//5、运营商返佣金给平台（平台抽成）
                //平台应抽成金额
                decimal platformCommissionMoney = new PlatformCommissionCalculator(_order.AreaID, PlatformCommissionOption.AreaCommissionByMerchantOrder, totalCommissionMoney).CommissionMoney;

                //需要抽佣时（抽佣金额>0）才进入
                if (platformCommissionMoney > 0)
                {
                    //从余额中支付佣金给平台
                    _operator.Balance = _operator.Balance - platformCommissionMoney;

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
                        LastBalance = _operator.Balance,
                        OpeartorID = _operator.OperatorID,
                        PaymentType = (int)OnlinePaymentType.Balance,
                        PlatformTransactionCode = operatorPayCommissionTransCode,
                        TradeID = IDCreater.Instance.GetID(),
                        TradeInfo = string.Format("从商家上门服务订单抽成后返佣金给平台，订单编号：{0}", _order.OrderCode),
                        TradeNo = string.Empty,
                        TradeType = (int)OperatorTradeType.PayCommission
                    };
                    _context.AreaOperator_TradeRecords.Add(operatorPayCommissionTrandsRecords);
                    //写入平台资金流水
                    _context.Platform_MoneyTransaction.Add(new Platform_MoneyTransaction
                    {
                        Amount = operatorPayCommissionTrandsRecords.Amount,
                        AreaID = _order.AreaID,
                        CustomAccountID = _operator.OperatorID,
                        CustomIdentity = (int)CounterpartyIdentity.AreaOperator,
                        CustomName = _operatorProfile?.CompanyName,
                        IsMainTransaction = false,
                        LastBalance = operatorPayCommissionTrandsRecords.LastBalance,
                        Remark = string.Format("从商家上门服务订单抽成后返佣金给平台，订单编号：{0}", _order.OrderCode),
                        ThirdTransactionCode = string.Empty,
                        TransactionCode = operatorPayCommissionTransCode,
                        TransactionTime = DateTime.Now,
                        TransactionType = (int)PlatformTransactionType.PayCommission
                    });
                }
                #endregion
            }
        }

        /// <summary>
        /// 用户支付给个人服务者
        /// </summary>
        /// <param name="error"></param>
        void UserPaymentToWorker()
        {
            #region//1、用户货款从冻结金额中结算
            if (_user.FreezeMoney < _settlementAmount)
            {
                _errorMessage = "呼叫程序猿，用户钱跑哪去了？冻结资金结算不了当前订单！";
                return;
            }
            _user.FreezeMoney -= _settlementAmount;
            #endregion

            #region //2、获取用户支付时的交易记录信息（从中获取平台交易流水号）
            var userTradeRecord = _context.User_TradeRecords.Where(p => p.DataID == _order.OrderID).FirstOrDefault();
            string mainTransCode = null != userTradeRecord ? userTradeRecord.PlatformTransactionCode : string.Empty;
            if (!string.IsNullOrWhiteSpace(mainTransCode) && mainTransCode.Length == 24)
            {
                mainTransCode = mainTransCode.Remove(23, 1);
            }
            if (mainTransCode.Length != 23) mainTransCode = IDCreater.Instance.GetPlatformTransactionCode(PlatformTransactionType.BuyService, _order.AreaID);
            //业务序号
            int transIndex = 0;
            #endregion

            #region //3、个人工作人员获得货款并记录
            string workerGetSaleTransCode = mainTransCode + (++transIndex);
            _worker.Balance = _worker.Balance + _settlementAmount;

            //写入个人服务用户交易记录
            var workerSaleTrandsRecords = new User_TradeRecords
            {
                Amount = _settlementAmount,
                CounterpartyId = _order.UserID,
                CounterpartyIdentity = (int)CounterpartyIdentity.User,
                CreateTime = DateTime.Now,
                DataID = _order.OrderID,
                LastBalance = _worker.Balance,
                UserID = _worker.UserID,
                PaymentType = (int)OnlinePaymentType.Balance,
                PlatformTransactionCode = workerGetSaleTransCode,
                TradeID = IDCreater.Instance.GetID(),
                TradeInfo = string.Format("提供上门服务，订单编号：{0}", _order.OrderCode),
                TradeNo = string.Empty,
                TradeType = (int)UserTransType.GetWorkCommission
            };
            _context.User_TradeRecords.Add(workerSaleTrandsRecords);

            //同步到工作交易记录
            _context.Worker_TradeRecords.Add(new Worker_TradeRecords
            {
                Amount = workerSaleTrandsRecords.Amount,
                CounterpartyId = workerSaleTrandsRecords.CounterpartyId,
                CounterpartyIdentity = workerSaleTrandsRecords.CounterpartyIdentity,
                CreateTime = workerSaleTrandsRecords.CreateTime,
                DataID = workerSaleTrandsRecords.DataID,
                LastBalance = workerSaleTrandsRecords.LastBalance,
                UserID = workerSaleTrandsRecords.UserID,
                PaymentType = workerSaleTrandsRecords.PaymentType,
                PlatformTransactionCode = workerSaleTrandsRecords.PlatformTransactionCode,
                TradeID = IDCreater.Instance.GetID(),
                TradeInfo = workerSaleTrandsRecords.TradeInfo,
                TradeNo = workerSaleTrandsRecords.TradeNo,
                TradeType = (int)WorkerTransType.ServiceSales
            });

            //写入平台资金流水
            _context.Platform_MoneyTransaction.Add(new Platform_MoneyTransaction
            {
                Amount = workerSaleTrandsRecords.Amount,
                AreaID = _order.AreaID,
                CustomAccountID = workerSaleTrandsRecords.UserID,
                CustomIdentity = (int)CounterpartyIdentity.Worker,
                CustomName = _worker.Username,
                IsMainTransaction = false,
                LastBalance = workerSaleTrandsRecords.LastBalance,
                Remark = string.Format("提供上门服务，订单编号：{0}", _order.OrderCode),
                ThirdTransactionCode = string.Empty,
                TransactionCode = workerGetSaleTransCode,
                TransactionTime = DateTime.Now,
                TransactionType = (int)PlatformTransactionType.SaleService
            });
            #endregion

            //总抽成（指商家被抽）
            decimal totalCommissionMoney = new AreaForWorkerCommissionCalculator(_order.AreaID, _order.WorkerID, AreaWorkerCommissionOption.WorkerServiceOrder, _settlementAmount).CommissionMoney;

            if (totalCommissionMoney > 0)
            {
                #region//4、区域运营商抽成

                #region//个人服务者返佣给运营商
                _worker.Balance -= totalCommissionMoney;
                string workerReturnTransCode = mainTransCode + (++transIndex);
                //写入个人服务者返佣记录
                var workerReturnTrandsRecords = new User_TradeRecords
                {
                    Amount = -totalCommissionMoney,
                    CounterpartyId = _operator.OperatorID,
                    CounterpartyIdentity = (int)CounterpartyIdentity.AreaOperator,
                    CreateTime = DateTime.Now,
                    DataID = _order.OrderID,
                    LastBalance = _worker.Balance,
                    UserID = _order.WorkerID,
                    PaymentType = (int)OnlinePaymentType.Balance,
                    PlatformTransactionCode = workerReturnTransCode,
                    TradeID = IDCreater.Instance.GetID(),
                    TradeInfo = string.Format("上门服务返佣金给运营商，订单编号：{0}", _order.OrderCode),
                    TradeNo = string.Empty,
                    TradeType = (int)UserTransType.ReturnCommission
                };
                _context.User_TradeRecords.Add(workerReturnTrandsRecords);

                //同步到工作交易记录
                _context.Worker_TradeRecords.Add(new Worker_TradeRecords
                {
                    Amount = workerReturnTrandsRecords.Amount,
                    CounterpartyId = workerReturnTrandsRecords.CounterpartyId,
                    CounterpartyIdentity = workerReturnTrandsRecords.CounterpartyIdentity,
                    CreateTime = workerReturnTrandsRecords.CreateTime,
                    DataID = workerReturnTrandsRecords.DataID,
                    LastBalance = workerReturnTrandsRecords.LastBalance,
                    UserID = workerReturnTrandsRecords.UserID,
                    PaymentType = workerReturnTrandsRecords.PaymentType,
                    PlatformTransactionCode = workerReturnTrandsRecords.PlatformTransactionCode,
                    TradeID = IDCreater.Instance.GetID(),
                    TradeInfo = workerReturnTrandsRecords.TradeInfo,
                    TradeNo = workerReturnTrandsRecords.TradeNo,
                    TradeType = (int)WorkerTransType.ReturnCommission
                });

                //写入平台资金流水
                _context.Platform_MoneyTransaction.Add(new Platform_MoneyTransaction
                {
                    Amount = workerReturnTrandsRecords.Amount,
                    AreaID = _order.AreaID,
                    CustomAccountID = workerReturnTrandsRecords.UserID,
                    CustomIdentity = (int)CounterpartyIdentity.Worker,
                    CustomName = _worker.Username,
                    IsMainTransaction = false,
                    LastBalance = workerReturnTrandsRecords.LastBalance,
                    Remark = string.Format("上门服务返佣金给运营商，订单编号：{0}", _order.OrderCode),
                    ThirdTransactionCode = string.Empty,
                    TransactionCode = workerReturnTransCode,
                    TransactionTime = DateTime.Now,
                    TransactionType = (int)PlatformTransactionType.PayCommission
                });
                #endregion

                #region//运营商接收个人服务者返佣
                _operator.Balance += totalCommissionMoney;
                string operatorGetReturnCommissionTransCode = mainTransCode + (++transIndex);
                //写入运营商交易记录
                var operatorGetReturnCommissionTrandsRecords = new AreaOperator_TradeRecords
                {
                    Amount = totalCommissionMoney,
                    CounterpartyId = _order.WorkerID,
                    CounterpartyIdentity = (int)CounterpartyIdentity.Worker,
                    CreateTime = DateTime.Now,
                    DataID = _order.OrderID,
                    LastBalance = _operator.Balance,
                    OpeartorID = _operator.OperatorID,
                    PaymentType = (int)OnlinePaymentType.Balance,
                    PlatformTransactionCode = operatorGetReturnCommissionTransCode,
                    TradeID = IDCreater.Instance.GetID(),
                    TradeInfo = string.Format("从个人服务者上门服务中抽成，订单编号：{0}", _order.OrderCode),
                    TradeNo = string.Empty,
                    TradeType = (int)OperatorTradeType.CommissionGet
                };
                _context.AreaOperator_TradeRecords.Add(operatorGetReturnCommissionTrandsRecords);
                //写入平台资金流水
                _context.Platform_MoneyTransaction.Add(new Platform_MoneyTransaction
                {
                    Amount = operatorGetReturnCommissionTrandsRecords.Amount,
                    AreaID = _order.AreaID,
                    CustomAccountID = operatorGetReturnCommissionTrandsRecords.OpeartorID,
                    CustomIdentity = (int)CounterpartyIdentity.AreaOperator,
                    CustomName = _operatorProfile?.CompanyName,
                    IsMainTransaction = false,
                    LastBalance = operatorGetReturnCommissionTrandsRecords.LastBalance,
                    Remark = string.Format("从个人服务者上门服务中抽成，订单编号：{0}", _order.OrderCode),
                    ThirdTransactionCode = string.Empty,
                    TransactionCode = operatorGetReturnCommissionTransCode,
                    TransactionTime = DateTime.Now,
                    TransactionType = (int)PlatformTransactionType.CommissionGet
                });
                #endregion

                #endregion

                #region//5、运营商返佣金给平台（平台抽成）
                //平台应抽成金额
                decimal platformCommissionMoney = new PlatformCommissionCalculator(_order.AreaID, PlatformCommissionOption.AreaCommissionByWorkerOrder, totalCommissionMoney).CommissionMoney;

                //需要抽佣时（抽佣金额>0）才进入
                if (platformCommissionMoney > 0)
                {
                    //从余额中支付佣金给平台
                    _operator.Balance = _operator.Balance - platformCommissionMoney;

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
                        LastBalance = _operator.Balance,
                        OpeartorID = _operator.OperatorID,
                        PaymentType = (int)OnlinePaymentType.Balance,
                        PlatformTransactionCode = operatorPayCommissionTransCode,
                        TradeID = IDCreater.Instance.GetID(),
                        TradeInfo = string.Format("从个人服务者上门服务订单抽成后返佣金给平台，订单编号：{0}", _order.OrderCode),
                        TradeNo = string.Empty,
                        TradeType = (int)OperatorTradeType.PayCommission
                    };
                    _context.AreaOperator_TradeRecords.Add(operatorPayCommissionTrandsRecords);
                    //写入平台资金流水
                    _context.Platform_MoneyTransaction.Add(new Platform_MoneyTransaction
                    {
                        Amount = operatorPayCommissionTrandsRecords.Amount,
                        AreaID = _order.AreaID,
                        CustomAccountID = _operator.OperatorID,
                        CustomIdentity = (int)CounterpartyIdentity.AreaOperator,
                        CustomName = _operatorProfile?.CompanyName,
                        IsMainTransaction = false,
                        LastBalance = operatorPayCommissionTrandsRecords.LastBalance,
                        Remark = string.Format("从个人服务者上门服务订单抽成后返佣金给平台，订单编号：{0}", _order.OrderCode),
                        ThirdTransactionCode = string.Empty,
                        TransactionCode = operatorPayCommissionTransCode,
                        TransactionTime = DateTime.Now,
                        TransactionType = (int)PlatformTransactionType.PayCommission
                    });
                }
                #endregion
            }
        }

        /// <summary>
        /// 商家支付给用户
        /// </summary>
        /// <param name="error"></param>
        void MerchantPaymentToUser()
        {
            #region //1、商户货款从冻结金额中结算
            if (_merchant.FreezeMoney < _settlementAmount)
            {
                _errorMessage = "呼叫程序猿，商户钱跑哪去了？冻结资金结算不了当前订单！";
                return;
            }
            _merchant.FreezeMoney -= _settlementAmount;
            #endregion

            #region //2、获取商家支付时的交易记录信息（从中获取平台交易流水号）
            var merchantTradeRecord = _context.Merchant_TradeRecords.Where(p => p.DataID == _order.OrderID).FirstOrDefault();
            string mainTransCode = null != merchantTradeRecord ? merchantTradeRecord.PlatformTransactionCode : string.Empty;
            if (!string.IsNullOrWhiteSpace(mainTransCode) && mainTransCode.Length == 24)
            {
                mainTransCode = mainTransCode.Remove(23, 1);
            }
            if (mainTransCode.Length != 23) mainTransCode = IDCreater.Instance.GetPlatformTransactionCode(PlatformTransactionType.SaleProduct, _order.AreaID);
            //业务序号
            int transIndex = 0;
            #endregion

            #region //3、用户获得服务中的货款并记录
            string userGetSaleTransCode = mainTransCode + (++transIndex);
            _user.Balance += _settlementAmount;

            //写入个人服务用户交易记录
            var userGetTrandsRecords = new User_TradeRecords
            {
                Amount = _settlementAmount,
                CounterpartyId = _merchant.MerchantID,
                CounterpartyIdentity = (int)CounterpartyIdentity.Merchant,
                CreateTime = DateTime.Now,
                DataID = _order.OrderID,
                LastBalance = _user.Balance,
                UserID = _user.UserID,
                PaymentType = (int)OnlinePaymentType.Balance,
                PlatformTransactionCode = userGetSaleTransCode,
                TradeID = IDCreater.Instance.GetID(),
                TradeInfo = string.Format("在上门服务中收款，订单编号：{0}", _order.OrderCode),
                TradeNo = string.Empty,
                TradeType = (int)UserTransType.Proceeds
            };
            _context.User_TradeRecords.Add(userGetTrandsRecords);

            //写入平台资金流水
            _context.Platform_MoneyTransaction.Add(new Platform_MoneyTransaction
            {
                Amount = userGetTrandsRecords.Amount,
                AreaID = _order.AreaID,
                CustomAccountID = userGetTrandsRecords.UserID,
                CustomIdentity = (int)CounterpartyIdentity.User,
                CustomName = _user.Username,
                IsMainTransaction = false,
                LastBalance = userGetTrandsRecords.LastBalance,
                Remark = string.Format("在上门服务中收款，订单编号：{0}", _order.OrderCode),
                ThirdTransactionCode = string.Empty,
                TransactionCode = userGetSaleTransCode,
                TransactionTime = DateTime.Now,
                TransactionType = (int)PlatformTransactionType.SaleProduct
            });
            #endregion
        }

        /// <summary>
        /// 个人服务者支付给用户
        /// </summary>
        /// <param name="error"></param>
        void WorkerPaymentToUser()
        {
            #region //1、个人工作人员货款从冻结金额中结算
            if (_worker.FreezeMoney < _settlementAmount)
            {
                _errorMessage = "呼叫程序猿，个人服务者钱跑哪去了？冻结资金结算不了当前订单！";
                return;
            }
            _worker.FreezeMoney -= _settlementAmount;
            #endregion

            #region //2、获取个人服务者支付时的交易记录信息（从中获取平台交易流水号）
            var _workerTradeRecord = _context.Worker_TradeRecords.Where(p => p.DataID == _order.OrderID).FirstOrDefault();
            string mainTransCode = null != _workerTradeRecord ? _workerTradeRecord.PlatformTransactionCode : string.Empty;
            if (!string.IsNullOrWhiteSpace(mainTransCode) && mainTransCode.Length == 24)
            {
                mainTransCode = mainTransCode.Remove(23, 1);
            }
            if (mainTransCode.Length != 23) mainTransCode = IDCreater.Instance.GetPlatformTransactionCode(PlatformTransactionType.SaleProduct, _order.AreaID);
            //业务序号
            int transIndex = 0;
            #endregion

            #region //3、用户获得服务中的货款并记录
            string userGetSaleTransCode = mainTransCode + (++transIndex);
            _user.Balance += _settlementAmount;

            //写入个人服务用户交易记录
            var userGetTrandsRecords = new User_TradeRecords
            {
                Amount = _settlementAmount,
                CounterpartyId = _order.WorkerID,
                CounterpartyIdentity = (int)CounterpartyIdentity.Worker,
                CreateTime = DateTime.Now,
                DataID = _order.OrderID,
                LastBalance = _user.Balance,
                UserID = _user.UserID,
                PaymentType = (int)OnlinePaymentType.Balance,
                PlatformTransactionCode = userGetSaleTransCode,
                TradeID = IDCreater.Instance.GetID(),
                TradeInfo = string.Format("在上门服务中收款，订单编号：{0}", _order.OrderCode),
                TradeNo = string.Empty,
                TradeType = (int)UserTransType.Proceeds
            };
            _context.User_TradeRecords.Add(userGetTrandsRecords);

            //写入平台资金流水
            _context.Platform_MoneyTransaction.Add(new Platform_MoneyTransaction
            {
                Amount = userGetTrandsRecords.Amount,
                AreaID = _order.AreaID,
                CustomAccountID = userGetTrandsRecords.UserID,
                CustomIdentity = (int)CounterpartyIdentity.User,
                CustomName = _user.Username,
                IsMainTransaction = false,
                LastBalance = userGetTrandsRecords.LastBalance,
                Remark = string.Format("在上门服务中收款，订单编号：{0}", _order.OrderCode),
                ThirdTransactionCode = string.Empty,
                TransactionCode = userGetSaleTransCode,
                TransactionTime = DateTime.Now,
                TransactionType = (int)PlatformTransactionType.SaleProduct
            });
            #endregion
        }

        /// <summary>
        /// 执行结算
        /// </summary>
        public override void Execute()
        {
            _context = new DataContext();

            #region//检测订单有效性

            if (null == _order)
            {
                _errorMessage = "订单数据不存在！";
                return;
            }
            if (_order.BusinessType != (int)BusinessServiceType.Visiting)
            {
                _errorMessage = "当前订单不是有效的上门服务订单！";
                return;
            }
            if (_order.Status != (int)VisitingServiceOrderStatus.WorkerServiceDone)
            {
                _errorMessage = "当前订单状态不适合进行结算处理！";
                return;
            }

            #endregion

            #region //结算处理

            #region//1、计算订单需要结算的金额
            //线下支付，但下单时有预约金
            if (_order.PaymentType == (int)BusinessServiceOrderPayment.OffLine)
            {
                if (_order.PrepaidAmount > 0) _settlementAmount = _order.PrepaidAmount;
            }
            else if (_order.PaiedTime.HasValue)
            {
                _settlementAmount = _order.ActualOrderAmount;
            }
            else
            {
                throw new Exception("当前订单数据异常（线上支付方式但未检测到付款信息），不能自动确定服务完成！");
            }
            _settlementAmount = Math.Abs(_settlementAmount);
            #endregion

            //需要结算，先结算
            if (_settlementAmount > 0)
            {
                #region//2、检测订单付款方
                //支付方
                BusinessServicePayer payer = default(BusinessServicePayer);
                //业务信息
                var business = _context.KylinService_Business.SingleOrDefault(p => p.BusinessID == _order.BusinessID);
                payer = (BusinessServicePayer)Enum.Parse(typeof(BusinessServicePayer), business.PayerType.ToString());
                #endregion

                #region//3、获取用户资金并检测
                _user = _context.User_Account.SingleOrDefault(p => p.UserID == _order.UserID);
                if (null == _user)
                {
                    _errorMessage = "用户数据异常，未检测到用户信息！";
                    return;
                }
                _context.User_Account.Attach(_user);
                _context.Entry(_user).Property(p => p.Balance).IsModified = true;
                #endregion

                #region //4、检测并获取服务提供方信息
                //服务提供方类型
                BusinessOrderServiceProvider servicerProvider = default(BusinessOrderServiceProvider);
                servicerProvider = (BusinessOrderServiceProvider)Enum.Parse(typeof(BusinessOrderServiceProvider), _order.ServerType.ToString());

                switch (servicerProvider)
                {
                    case BusinessOrderServiceProvider.Merchant:
                        _merchant = _context.Merchant_Account.SingleOrDefault(p => p.MerchantID == _order.MerchantID);
                        break;
                    case BusinessOrderServiceProvider.Person:
                        _worker = _context.User_Account.SingleOrDefault(p => p.UserID == _order.WorkerID);
                        break;
                }
                //如果服务者不存在，则返回错误
                if (_merchant == null && _worker == null)
                {
                    _errorMessage = "启奏陛下，订单的服务提供者最近学会一种新的奇门循土术，突然就不见了，请陛下明察！";
                    return;
                }
                if (_merchant != null)
                {
                    _context.Merchant_Account.Attach(_merchant);
                    _context.Entry(_merchant).Property(p => p.Balance).IsModified = true;
                    _context.Entry(_merchant).Property(p => p.FreezeMoney).IsModified = true;
                }
                if (_worker != null)
                {
                    _context.User_Account.Attach(_worker);
                    _context.Entry(_worker).Property(p => p.Balance).IsModified = true;
                    _context.Entry(_worker).Property(p => p.FreezeMoney).IsModified = true;
                }
                #endregion

                #region //5、获取当前订单所归属区域的运营商
                //区域（大区域，即运营商区域）
                var operatorQuery = from op in _context.Area_Operator
                                    join ao in _context.Area_OperatorRelation
                                    on op.OperatorID equals ao.OperatorID
                                    where ao.AreaID == _order.AreaID
                                    select op;
                _operator = operatorQuery.FirstOrDefault();
                if (null == _operator)
                {
                    _errorMessage = "启奏陛下，订单所在区域运营商不存在，微臣不敢擅作主张，还请陛下圣断！";
                    return;
                }
                _context.Area_Operator.Attach(_operator);
                _context.Entry(_operator).Property(p => p.Balance).IsModified = true;
                //运营商基本资料
                _operatorProfile = _context.Area_OperatorProfile.SingleOrDefault(p => p.OperatorID == _operator.OperatorID);
                #endregion

                #region //6、处理结算及记录
                //6.1 如果付款方为服务者（商），表示服务人员付款给用户，则不需要考虑抽成
                if (payer == BusinessServicePayer.Servicer)
                {
                    //商户接单，商家付
                    if (servicerProvider == BusinessOrderServiceProvider.Merchant)
                    {
                        MerchantPaymentToUser();
                    }
                    //个人服务者接单，个人服务者付
                    else if (servicerProvider == BusinessOrderServiceProvider.Person)
                    {
                        WorkerPaymentToUser();
                    }
                }
                //6.2 如果付款方为用户，表示用户付款给服务提供者，则从服务者（商）抽成
                else if (payer == BusinessServicePayer.Custom)
                {
                    //商户接单，付给商家
                    if (servicerProvider == BusinessOrderServiceProvider.Merchant)
                    {
                        UserPaymentToMerchant();
                    }
                    //个人服务者接单，付给个人服务人员
                    else if (servicerProvider == BusinessOrderServiceProvider.Person)
                    {
                        UserPaymentToWorker();
                    }
                }
                else
                {
                    _errorMessage = "妈呀，即不是服务受用方也不是服务提供方买单，莫非有雷锋慈善家赞助？先不处理了，陛下，你决定吧！";
                    return;
                }
                #endregion
            }

            if (Success)
            {
                #region//7、更新订单状态
                if (_needProcessOrder)
                {
                    _context.KylinService_Order.Attach(_order);
                    _context.Entry(_order).Property(p => p.Status).IsModified = true;
                    _context.Entry(_order).Property(p => p.ReceivedTime).IsModified = true;

                    //修改订单状态为已完成
                    _order.Status = (int)VisitingServiceOrderStatus.UserServiceConfirmDone;
                    _order.ReceivedTime = DateTime.Now;
                }
                #endregion

                #region//8、用户积分奖励
                var pointCalc = new PointCalculator(_order.UserID, UserActivityType.OrderFinish);
                if (pointCalc.CanContinue)
                {
                    int points = pointCalc.Score;

                    //更新用户积分
                    _context.Entry(_user).Property(p => p.Points).IsModified = true;
                    _user.Points += points;

                    //积分获取记录
                    _context.User_PointsRecords.Add(new User_PointsRecords
                    {
                        ActivityType = (int)UserActivityType.OrderFinish,
                        CreateTime = DateTime.Now,
                        RecordsID = IDCreater.Instance.GetID(),
                        Remark = string.Format("上门订单（编号：{0}）交易完成，获得{1}积分。", _order.OrderCode, points),
                        Score = points,
                        UserID = _order.UserID
                    });
                }
                #endregion

                #region//9、用户经验值奖励
                var empiricalCalc = new EmpiricalCalculator(_order.UserID, UserActivityType.OrderFinish);
                if (empiricalCalc.CanContinue)
                {
                    int empirical = empiricalCalc.Score;

                    //更新用户经验值
                    _context.Entry(_user).Property(p => p.Empirical).IsModified = true;
                    _user.Empirical += empirical;

                    //经验值获取记录
                    _context.User_EmpiricalRecords.Add(new User_EmpiricalRecords
                    {
                        ActivityType = (int)UserActivityType.OrderFinish,
                        CreateTime = DateTime.Now,
                        RecordsID = IDCreater.Instance.GetID(),
                        Remark = string.Format("上门订单（编号：{0}）交易完成，获得{1}点经验值。", _order.OrderCode, empirical),
                        Score = empirical,
                        UserID = _order.UserID
                    });
                }
                #endregion

                //返回处理结果
                if (_context.SaveChanges() < 1)
                {
                    _errorMessage = "操作失败！";
                    return;
                }
                else
                {
                    _errorMessage = null;

                    #region//10、消息推送
                    try
                    {
                        var pushRedis = Startup.PushRedisConfigs[RedisPushType.AppointOrderReceivedGoods];

                        if (null != pushRedis)
                        {
                            var msgContent = new AppointOrderReceivedGoodsContent
                            {
                                ActualOrderAmount = _order.ActualOrderAmount,
                                OrderCode = _order.OrderCode,
                                OrderID = _order.OrderID,
                                UserFinishTime = _order.UserFinishTime ?? DateTime.Now,
                                UserID = _order.UserID,
                                UserName = _user.Username,
                                MerchantID = _order.MerchantID,
                                BusinessType = _order.BusinessType,
                                ServerType = _order.ServerType,
                                WorkerID = _order.WorkerID
                            };
                            var pushDb = PushRedisContext.Redis.GetDatabase(pushRedis.DbIndex);

                            if (pushDb != null)
                            {
                                pushDb.ListRightPush(pushRedis.Key, msgContent);
                            }
                        }
                    }
                    catch { }
                    #endregion
                }
            }

            #endregion

            _context.Dispose();
        }
    }
}
