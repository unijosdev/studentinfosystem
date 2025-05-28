using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwiftKampusModel.MarketPlace
{
    public class Delivery
    {
        [Key]
        public int Id { get; set; }

        public int TransactionId { get; set; }

        [Display(Name ="Collected By")]
        public string Recipient { get; set; }

        public string DateRecieved { get; set; }

        [Display(Name ="Issuer")]
        public int UserId { get; set; }
    }
}
