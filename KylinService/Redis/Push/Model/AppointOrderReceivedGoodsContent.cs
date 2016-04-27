using System;

namespace KylinService.Redis.Push.Model
{
    /// <summary>
    /// 上门预约订单确认收货后消息推送内容
    /// </summary>
    public class AppointOrderReceivedGoodsContent
    {
        /// <summary>
        /// 订单ID
        /// </summary>
        public long OrderID { get; set; }

        /// <summary>
        /// 订单编号
        /// </summary>
        public string OrderCode { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        public long UserID { get; set; }

        /// <summary>
        /// 商家ID（未抢单前为0，个人服务者抢单后为0）
        /// </summary> 
        public long MerchantID { get; set; }

        /// <summary>
        /// 服务职员ID（未抢单前为0）
        /// </summary>
        public long WorkerID { get; set; }

        /// <summary>
        /// 服务者类型（枚举：商家｜个人服务者）
        /// </summary>
        public int ServerType { get; set; }

        /// <summary>
        /// 订单业务类型（如：上门|预约）
        /// </summary>
        public int BusinessType { get; set; }

        /// <summary>
        /// 用户姓名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 实际订单总金额
        /// </summary>
        public decimal ActualOrderAmount { get; set; }

        /// <summary>
        /// 用户确定服务结束时间
        /// </summary>

        public DateTime UserFinishTime { get; set; }
    }
}
