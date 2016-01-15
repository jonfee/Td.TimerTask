using System;

namespace KylinService.Data.Model
{
    /// <summary>
    /// 上门/预约服务订单
    /// </summary>
    public class AppointOrderModel
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
        /// 订单的业务ID
        /// </summary>
        public long BusinessID { get; set; }

        /// <summary>
        /// 订单业务类型（如：上门|预约）
        /// </summary>
        public int BusinessType { get; set; }

        /// <summary>
        /// 业务名称
        /// </summary>
        public string BusinessName { get; set; }

        /// <summary>
        /// 报价方式（枚举：下单时报价｜上门时报价等）
        /// </summary>
        public int QuoteWays { get; set; }

        /// <summary>
        /// 付款方类型（枚举：下单方｜服务方）
        /// </summary>
        public int PayerType { get; set; }

        /// <summary>
        /// 实际订单总金额
        /// </summary>
        public decimal ActualOrderAmount { get; set; }

        /// <summary>
        /// 预支付金额
        /// </summary>
        public decimal PrepaidAmount { get; set; }

        /// <summary>
        /// 支付方式
        /// </summary>
        public int PaymentType { get; set; }

        /// <summary>
        /// 下单时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 商家接单时间
        /// </summary>
        public DateTime? ReceivedTime { get; set; }

        /// <summary>
        /// 订单确定时间（即双方达成一致，默认为商家接单时间）
        /// </summary>
        public DateTime? ConfirmTime { get; set; }

        /// <summary>
        /// 支付时间
        /// </summary>
        public DateTime? PaiedTime { get; set; }

        /// <summary>
        /// 服务职员结束服务时间
        /// </summary>
        public DateTime? WorkerFinishTime { get; set; }

        /// <summary>
        /// 用户确定服务结束时间
        /// </summary>
        public DateTime? UserFinishTime { get; set; }

        /// <summary>
        /// 订单状态（上门/预约订单状态枚举不同）
        /// </summary>
        public int Status { get; set; }
    }
}
