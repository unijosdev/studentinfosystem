using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SwiftKampus.ViewModels.ShoppingCart
{
    public class ProductCartVm
    {
        public int ProductId { get; set; }
        public int availableQty { get; set; }
        public int CustomerOrderedCount { get; set; }
        public decimal Price { get; set; }
        public string ProductName { get; set; }
    }
}