using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwiftKampusModel.MarketPlace
{
    public class MarketPlaceTransaction
    {
        public int Id { get; set; }

        public string TransactionOrderId { get; set; } //Same as remita OrderId

        public int CustomerId { get; set; }

        public int OrderId { get; set; }

        public decimal Amount { get; set; }

        public string ModeOfPayment { get; set; }

        public string TransactionDate { get; set; }

        public string TransactionMessage { get; set; }

        public bool HasMadePayment { get; set; }

        public int TransactionReferenceNumber { get; set; }

        public int DeliverId { get; set; }

        public Customer Customer { get; set; }
        public Order Order { get; set; }
        //public ICollection<Sale> Sales { get; set; }
    }
}
