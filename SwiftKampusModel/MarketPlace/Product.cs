using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwiftKampusModel.MarketPlace
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Product Name")]
        public string ProductName { get; set; }

        public int ProductCategoryId { get; set; }

        public int Visible { get; set; }

        [Display(Name = "Product Price")]
        public decimal ProductPrice { get; set; }

        public ProductCategory ProductCategory { get; set; }
        public ICollection<StockOrder> StockOrders { get; set; }

    }
}
