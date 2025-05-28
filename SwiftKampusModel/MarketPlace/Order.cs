using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwiftKampusModel.MarketPlace
{
    public partial class Order
    {
        public int OrderId { get; set; }
        public int CustomerId { get; set; }
        //public string Username { get; set; }
        //public string Fullname { get; set; }
        //public string LastName { get; set; }
        //public string Address { get; set; }
        //public string City { get; set; }
        //public string State { get; set; }
        //public string PostalCode { get; set; }
        //public string Country { get; set; }
        //public string Phone { get; set; }
        //public string Email { get; set; }
        public decimal Total { get; set; }
        public int DeliverId { get; set; }
        public string OrderDate { get; set; }

        public Customer Customer { get; set; }
        public List<OrderDetail> OrderDetails { get; set; }
        public ICollection<Transaction> Transactions { get; set; }
    }
}
