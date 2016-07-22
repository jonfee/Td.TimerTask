using KylinService.Data.Model;
using System;
using System.Linq;
using System.Threading.Tasks;
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
        /// 修改数据-自动取消订单
        /// </summary>
        /// <param name="_legworkOrder"></param>
        /// <returns></returns>
        public async static Task<bool> UpdateOrderTimeout(LegworkOrderModel _legworkOrder)
        {
            using (var db = new DataContext())
            {
                var model = db.Legwork_Order.FirstOrDefault(q => q.OrderID == _legworkOrder.OrderID);
                model.Status = (int)LegworkOrderStatus.Canceled;
                model.CancelTime = DateTime.Now;
                return await db.SaveChangesAsync() > 0;
            }
        }

        /// <summary>
        /// 修改数据-自动确认收货
        /// </summary>
        /// <param name="_legworkOrder"></param>
        /// <returns></returns>
        public async static Task<bool> UpdateAutoConfirmTime(LegworkOrderModel _legworkOrder)
        {
            using (var db = new DataContext())
            {
                var model = db.Legwork_Order.FirstOrDefault(q => q.OrderID == _legworkOrder.OrderID);
                model.Status = (int)LegworkOrderStatus.Complete;
                model.CompleteTime = DateTime.Now;
                return await db.SaveChangesAsync() > 0;
            }
        }

        /// <summary>
        /// 修改数据-未付款自动取消
        /// </summary>
        /// <param name="_legworkOrder"></param>
        /// <returns></returns>
        public async static Task<bool> UpdatePaymentTimeout(LegworkOrderModel _legworkOrder)
        {
            using (var db = new DataContext())
            {
                var model = db.Legwork_Order.FirstOrDefault(q => q.OrderID == _legworkOrder.OrderID);
                model.Status = (int)LegworkOrderStatus.Invalid;
                model.CancelTime = DateTime.Now;
                return await db.SaveChangesAsync() > 0;
            }
        }
    }
}
