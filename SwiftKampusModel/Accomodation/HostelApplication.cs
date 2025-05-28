using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwiftKampusModel.Accomodation
{
    public class HostelApplication
    {
        public int HostelApplicationId { get; set; }
        public string StudentId { get; set; }

        [Index(IsUnique = true)]
        [MaxLength(50)]
        [Required]
        public string OrderId { get; set; }
        public string ReferenceNo { get; set; }

        public int SessionId { get; set; }
        public DateTime PaymentDateTime { get; set; }
        public double ExpectedAmount { get; set; }
        public double AmountPayed { get; set; }
        public bool IsPayed { get; set; }
        public string TransactionMessage { get; set; }
        public virtual Session Session { get; set; }
        public virtual Student Student { get; set; }
    }
}
