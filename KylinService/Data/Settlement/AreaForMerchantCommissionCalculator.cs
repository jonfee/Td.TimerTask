using System;
using Td.Kylin.DataCache;
using Td.Kylin.DataCache.CacheModel;
using Td.Kylin.EnumLibrary;

namespace KylinService.Data.Settlement
{
    /// <summary>
    /// 区域运营商针对商家抽成计算
    /// </summary>
    public class AreaForMerchantCommissionCalculator
    {
        /// <summary>
        /// 初始化区域运营商针对商家抽成实例
        /// </summary>
        /// <param name="areaID">区域ID</param>
        /// <param name="merchantID">商家ID</param>
        /// <param name="option">抽成项</param>
        /// <param name="baseAmount">抽成的基准金额</param>
        public AreaForMerchantCommissionCalculator(int areaID, long merchantID, AreaMerchantCommissionOption option, decimal baseAmount)
        {
            _areaID = areaID;
            _merchantID = merchantID;
            _option = option;
            _baseAmount = Math.Abs(baseAmount);
        }

        /// <summary>
        /// 区域ID
        /// </summary>
        private int _areaID;

        /// <summary>
        /// 商家ID
        /// </summary>
        private long _merchantID;

        /// <summary>
        /// 抽成项
        /// </summary>
        private AreaMerchantCommissionOption _option;

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

            //区域运营商对商家的抽成配置
            var merchantCommission = CacheCollection.AreaForMerchantCommissionCache.Get(_areaID, _merchantID, (int)_option);

            //不存在独立商家的抽成配置则从默认抽成中获取
            if (null == merchantCommission)
            {
                merchantCommission = new Func<AreaForMerchantCommissionCacheModel>(() =>
                {
                    var defaultMerchantCommission = CacheCollection.AreaDefaultCommissionCache.Get(_areaID, (int)_option);
                    if (null != defaultMerchantCommission)
                    {
                        AreaDefaultCommissionOption comOption = default(AreaDefaultCommissionOption);

                        switch (_option)
                        {
                            case AreaMerchantCommissionOption.MerchantProductOrder: comOption = AreaDefaultCommissionOption.MerchantProductOrder; break;
                            case AreaMerchantCommissionOption.MerchantServiceOrder: comOption = AreaDefaultCommissionOption.MerchantServiceOrder; break;
                        }

                        return new AreaForMerchantCommissionCacheModel
                        {
                            AreaID = defaultMerchantCommission.AreaID,
                            CommissionItem = (int)comOption,
                            CommissionType = defaultMerchantCommission.CommissionType,
                            MerchantID = _merchantID,
                            Value = defaultMerchantCommission.Value
                        };
                    }
                    return null;
                }).Invoke();
            }
            if (null != merchantCommission && merchantCommission.Value > 0)
            {
                if (merchantCommission.CommissionType == (int)CommissionType.FixedAmount)
                {
                    commissionMoney = merchantCommission.Value;
                }
                else if (merchantCommission.CommissionType == (int)CommissionType.MoneyRate)
                {
                    commissionMoney = Math.Round(_baseAmount * merchantCommission.Value * 0.01M, 2, MidpointRounding.ToEven);
                }
            }

            return commissionMoney;
        }
    }
}
