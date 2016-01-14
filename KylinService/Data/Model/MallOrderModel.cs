using System;

namespace KylinService.Data.Model
{
    public class MallOrderModel
    {
        ///<summary>
		///订单ID
		///</summary>
		public long OrderID { get; set; }

        ///<summary>
        ///订单类型
        ///</summary>
        public int OrderType { get; set; }

        ///<summary>
        ///订单来源数据ID（如摇一摇抽中记录ID）
        ///</summary>
        public long SourceDataID { get; set; }

        ///<summary>
        ///订单编号
        ///</summary>
        public string OrderCode { get; set; }

        ///<summary>
        ///商品简要信息
        ///</summary>
        public string ProductInfo { get; set; }

        ///<summary>
        ///实际订单总金额（TotalProductAmount-TotalDiscountAmount+TotalDeliveryFee）
        ///</summary>
        public decimal ActualOrderAmount { get; set; }

        ///<summary>
        ///买家用户ID
        ///</summary>
        public long UserID { get; set; }

        ///<summary>
        ///订单状态
        ///</summary>
        public int OrderStatus { get; set; }

        ///<summary>
        ///下单时间
        ///</summary>
        public DateTime CreateTime { get; set; }

        ///<summary>
        ///发货时间
        ///</summary>
        public DateTime? ShipTime { get; set; }

        ///<summary>
        ///必须支付的时间
        ///</summary>
        public DateTime? NeedPayTime { get; set; }
    }
}
