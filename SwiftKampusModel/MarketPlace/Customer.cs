using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwiftKampusModel.MarketPlace
{
    public class Customer
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "CutomerName")]
        public string Fullname { get; set; }

        public string Username { get; set; }

        public int OrderId { get; set; }

        public string PhoneNumber { get; set; }

        public string Email { get; set; }

        public string TransactionDate { get; set; }

        public ICollection<Order> Orders { get; set; }
    }
}
