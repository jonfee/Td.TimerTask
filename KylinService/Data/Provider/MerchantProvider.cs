using System.Linq;

namespace KylinService.Data.Provider
{
    public class MerchantProvider
    {
        /// <summary>
        /// 获取商户区域路径
        /// </summary>
        /// <param name="merchantID"></param>
        /// <returns></returns>
        public static string GetMerchantAreaLayer(long merchantID)
        {
            using (var db = new DataContext())
            {
                return db.Merchant_Account.SingleOrDefault(p => p.MerchantID == merchantID)?.AreaLayer;
            }
        }
    }
}
