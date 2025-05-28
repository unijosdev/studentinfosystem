using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwiftKampusModel.MarketPlace
{
    public class Price
    {
        [Key]
        public int Id { get; set; }

        [Display(Name ="Product Name")]
        public int ProductId { get; set; }

        [Display(Name = "Buy Price")]
        public decimal BuyPrice { get; set; }

        [Display(Name = "Sell Price")]
        public decimal SalePrice { get; set; }

        [Display(Name = "Discount")]    
        public decimal Discount { get; set; }

        public int Visible { get; set; }

        public Product Product { get; set; }
    }
}
