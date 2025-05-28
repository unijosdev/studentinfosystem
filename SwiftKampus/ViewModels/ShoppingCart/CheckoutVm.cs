using SwiftKampusModel.MarketPlace;
using System.Collections.Generic;

namespace SwiftKampus.ViewModels.ShoppingCart
{
    public class CheckoutVm : RemitaPostVm
    {
        public Order Order { get; set; }
        public List<OrderDetail> OrderDetails { get; set; }
        public string ModeOfPayment { get; set; }
    }
}