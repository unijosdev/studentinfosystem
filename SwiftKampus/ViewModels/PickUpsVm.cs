using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;

namespace SwiftKampus.ViewModels
{
    public class PickUpsVm
    {
        public int TransactionId { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string Phone { get; set; }
        public string CustomerName { get; set; }
        public decimal ProductPrice { get; set; }
        public int ProductCount { get; set; }
        public bool TransactionStatus { get; set; }
        public bool IssueStatus { get; set; }
        public string TransactionReference { get; set; }
    }
}