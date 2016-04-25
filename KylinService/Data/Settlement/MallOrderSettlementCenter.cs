using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Td.Kylin.Entity;

namespace KylinService.Data.Settlement
{
    /// <summary>
    /// 精品汇（B2C）订单结算中心
    /// </summary>
    public class MallOrderSettlementCenter
    {
        /// <summary>
        /// 初始化B2C订单结算实例
        /// </summary>
        /// <param name="orderID">需要结算的订单ID</param>
        /// <param name="needProcessOrder">是否需要处理订单</param>
        public MallOrderSettlementCenter(long orderID,bool needProcessOrder)
        {
            using (var db = new DataContext())
            {
                var order = db.Mall_Order.SingleOrDefault(p => p.OrderID == orderID);

                Order = order;
            }
        }

        /// <summary>
        /// 初始化B2C订单结算实例
        /// </summary>
        /// <param name="order">需要结算的订单</param>
        /// <param name="needProcessOrder">是否需要处理订单</param>
        public MallOrderSettlementCenter(Mall_Order order, bool needProcessOrder)
        {
            Order = order;
        }

        /// <summary>
        /// 当前结算的订单
        /// </summary>
        public Mall_Order Order { get; private set; }

        /// <summary>
        /// 是否需要处理订单
        /// </summary>
        public bool NeedProcessOrder { get; private set; }

        /// <summary>
        /// 执行结算
        /// </summary>
        public void Execute()
        {
            //获取抽成配置

        }
    }
}
