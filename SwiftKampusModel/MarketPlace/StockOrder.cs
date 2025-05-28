using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwiftKampusModel.MarketPlace
{
    public class StockOrder
    {
        [Key]
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int OrderQuantity { get; set; }
        public int Damage { get; set; }
        public string OrderDate { get; set; }
        public int Visible { get; set; }
        public Product Product { get; set; }

    }
}
