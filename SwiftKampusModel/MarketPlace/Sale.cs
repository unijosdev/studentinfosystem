using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwiftKampusModel.MarketPlace
{
    public class Sale
    {
        [Key]
        public int Id { get; set; }

        public int TransactionId { get; set; }

        public int ProductId { get; set; }

        public decimal Amount { get; set; }

        public int Quantity { get; set; }

        public int TotalAmount { get; set; }

        public decimal Gain { get; set; }

        public decimal Loss { get; set; }

        public Product Product { get; set; }

        //public Transaction Transaction { get; set; }
    }
}
