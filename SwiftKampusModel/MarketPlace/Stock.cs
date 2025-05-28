using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwiftKampusModel.MarketPlace
{
    public class Stock
    {
        [Key]
        public int Id { get; set; }

        public int ProductId { get; set; }
        public int QuantityBeforeOrder { get; set; }
        public int QuantityOrder  { get; set; }
        public int SalesQuantity { get; set; }
        public int CurrentQuantity { get; set; }
        public int CollectedQuantity { get; set; }
        public int NotCollectedQuantity { get; set; }
        public Product Product { get; set; }




    }
}
