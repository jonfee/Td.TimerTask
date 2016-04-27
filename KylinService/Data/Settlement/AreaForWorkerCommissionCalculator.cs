using System;
using Td.Kylin.DataCache;
using Td.Kylin.DataCache.CacheModel;
using Td.Kylin.EnumLibrary;

namespace KylinService.Data.Settlement
{
    /// <summary>
    /// 区域运营商对个人服务者抽成计算
    /// </summary>
    public class AreaForWorkerCommissionCalculator
    {
        /// <summary>
        /// 初始化区域运营商对个人服务者抽成实例
        /// </summary>
        /// <param name="areaID">区域ID</param>
        /// <param name="workerID">工作人员ID</param>
        /// <param name="option">抽成项</param>
        /// <param name="baseAmount">抽成的基准金额</param>
        public AreaForWorkerCommissionCalculator(int areaID, long workerID, AreaWorkerCommissionOption option, decimal baseAmount)
        {
            _areaID = areaID;
            _workerID = workerID;
            _option = option;
            _baseAmount = Math.Abs(baseAmount);
        }

        /// <summary>
        /// 区域ID
        /// </summary>
        private int _areaID;

        /// <summary>
        /// 工作人员ID
        /// </summary>
        private long _workerID;

        /// <summary>
        /// 抽成项
        /// </summary>
        private AreaWorkerCommissionOption _option;

        /// <summary>
        /// 抽成的基准金额
        /// </summary>
        private decimal _baseAmount;

        /// <summary>
        /// 抽成金额
        /// </summary>
        public decimal CommissionMoney
        {
            get
            {
                var money = GetCommissionMoney();
                return Math.Abs(money);
            }
        }

        decimal GetCommissionMoney()
        {
            //平台应抽成金额
            decimal commissionMoney = 0;

            //区域运营商对个人服务者的抽成配置
            var workerCommission = CacheCollection.AreaForPersonalWorkerCommissionCache.Get(_areaID, _workerID, (int)_option);

            //不存在独立个人服务者的抽成配置则从默认抽成中获取
            if (null == workerCommission)
            {
                workerCommission = new Func<AreaForPersonalWorkerCommissionCacheModel>(() =>
                {
                    var defaultWorkerCommission = CacheCollection.AreaDefaultCommissionCache.Get(_areaID, (int)_option);
                    if (null != defaultWorkerCommission)
                    {
                        AreaDefaultCommissionOption comOption = default(AreaDefaultCommissionOption);

                        switch (_option)
                        {
                            case AreaWorkerCommissionOption.WorkerServiceOrder: comOption = AreaDefaultCommissionOption.WorkerServiceOrder; break;
                        }

                        return new AreaForPersonalWorkerCommissionCacheModel
                        {
                            AreaID = defaultWorkerCommission.AreaID,
                            CommissionItem = (int)comOption,
                            CommissionType = defaultWorkerCommission.CommissionType,
                            UserID = _workerID,
                            Value = defaultWorkerCommission.Value
                        };
                    }
                    return null;
                }).Invoke();
            }
            if (null != workerCommission && workerCommission.Value > 0)
            {
                if (workerCommission.CommissionType == (int)CommissionType.FixedAmount)
                {
                    commissionMoney = workerCommission.Value;
                }
                else if (workerCommission.CommissionType == (int)CommissionType.MoneyRate)
                {
                    commissionMoney = Math.Round(_baseAmount * workerCommission.Value * 0.01M, 2, MidpointRounding.ToEven);
                }
            }

            return commissionMoney;
        }
    }
}
