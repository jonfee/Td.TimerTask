using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KylinService.Data.Model;
using Td.Kylin.DataCache.CacheModel;
using Td.Kylin.Entity;
using Td.Kylin.EnumLibrary;

namespace KylinService.Data.Provider
{
    public static class LegworkOrderProvder
    {
        /// <summary>
        /// 根据订单ID获取数据
        /// </summary>
        /// <param name="OrderID"></param>
        /// <returns></returns>
        public static LegworkOrderModel GetLegworkOrder(long OrderID)
        {
            using (var db = new DataContext())
            {
                return db.Legwork_Order.Select(t => new LegworkOrderModel
                {
                    OrderID = t.OrderID,
                    SubmitTime = t.SubmitTime,
                    ActualDeliveryTime = t.ActualDeliveryTime,
                    OfferAcceptTime = t.OfferAcceptTime,
                    Status = t.Status,
                    OrderCode = t.OrderCode
                }).FirstOrDefault(q => q.OrderID == OrderID);
            }
        }

        /// <summary>
        /// 修改数据
        /// </summary>
        /// <param name="_legworkOrder"></param>
        /// <returns></returns>
        public static int Update(Legwork_Order _legworkOrder)
        {
            using (var db = new DataContext())
            {
                db.Legwork_Order.Attach(_legworkOrder);
                return db.SaveChanges();
            }
        }

    }
}
