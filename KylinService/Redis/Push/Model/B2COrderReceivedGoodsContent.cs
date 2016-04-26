using System;

namespace KylinService.Redis.Push.Model
{
    /// <summary>
    /// 精品汇订单确认收货后消息推送内容
    /// </summary>
    public class B2COrderReceivedGoodsContent
    {
        ///<summary>
        ///订单ID
        ///</summary>
        public long OrderID { get; set; }

        /// <summary>
        /// 运营商ID
        /// </summary>
        public long  OperatorID { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        public long UserID { get; set; }

        /// <summary>
        /// 用户名称
        /// </summary>
        public string UserName { get; set; }
        
        ///<summary>
        ///订单编号
        ///</summary>
        public string OrderCode { get; set; }

        ///<summary>
        ///商品简要信息
        ///</summary>
        public string ProductInfo { get; set; }

        ///<summary>
        ///实际订单总金额
        ///</summary>
        public decimal ActualOrderAmount { get; set; }

        ///<summary>
        ///收货时间
        ///</summary>

        public DateTime ReceivedTime { get; set; }
    }
}
