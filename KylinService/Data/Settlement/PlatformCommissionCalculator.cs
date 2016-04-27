using System;
using Td.Kylin.DataCache;
using Td.Kylin.DataCache.CacheModel;
using Td.Kylin.EnumLibrary;

namespace KylinService.Data.Settlement
{
    /// <summary>
    /// 平台抽成计算
    /// </summary>
    public class PlatformCommissionCalculator
    {
        /// <summary>
        /// 初始化平台对区域的抽成实例
        /// </summary>
        /// <param name="areaID">区域ID</param>
        /// <param name="option">抽成项</param>
        /// <param name="baseAmount">抽成的基准金额</param>
        public PlatformCommissionCalculator(int areaID, PlatformCommissionOption option, decimal baseAmount)
        {
            _areaID = areaID;
            _option = option;
            _baseAmount = Math.Abs(baseAmount);
        }

        /// <summary>
        /// 区域ID
        /// </summary>
        private int _areaID;

        /// <summary>
        /// 抽成项
        /// </summary>
        private PlatformCommissionOption _option;

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

            //平台对当前区域的抽成配置
            var platformCommission = CacheCollection.PlatformCommissionCache.Get(_areaID, (int)_option);

            //不存在独立区域的抽成配置则从默认抽成中获取
            if (null == platformCommission)
            {
                platformCommission = new Func<PlatformCommissionCacheModel>(() =>
                {
                    var defaultPlatformCommission = CacheCollection.SystemGolbalConfigCache.Get((int)GlobalConfigType.AreaCommission, (int)_option);

                    if (null != defaultPlatformCommission)
                    {
                        decimal _val = 0M;

                        decimal.TryParse(defaultPlatformCommission.Value, out _val);

                        return new PlatformCommissionCacheModel
                        {
                            AreaID = _areaID,
                            CommissionItem = defaultPlatformCommission.ResourceKey,
                            CommissionType = defaultPlatformCommission.ValueUnit,
                            Value = _val
                        };
                    }
                    return null;
                }).Invoke();
            }

            if (null != platformCommission && platformCommission.Value > 0)
            {
                if (platformCommission.CommissionType == (int)CommissionType.FixedAmount)
                {
                    commissionMoney = platformCommission.Value;
                }
                else if (platformCommission.CommissionType == (int)CommissionType.MoneyRate)
                {
                    commissionMoney = Math.Round(_baseAmount * platformCommission.Value * 0.01M,2, MidpointRounding.ToEven);
                }
            }

            return commissionMoney;
        }
    }
}
