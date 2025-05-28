using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwiftKampusModel.Payment
{
    public class StudentIdCardPayment
    {
        public int StudentIdCardPaymentId { get; set; }
        public string StudentId { get; set; }
        public string ReferenceNo { get; set; }

        [Index(IsUnique = true)]
        [MaxLength(50)]
        [Required]
        public string OrderId { get; set; } 

        [Display(Name = "Session")]
        [Required(ErrorMessage = "Session is required")]
        public int SessionId { get; set; }

        [Display(Name = "Total Amount")]
        public decimal TotalAmount { get; set; }

        [Display(Name = "Payment Method")]
        public PMode PaymentMode { get; set; }

        [Display(Name = "Date of Payment")]
        public DateTime Date { get; set; }

        [Display(Name = "Fee Status")]
        public bool Status { get; set; }
        public bool IsExpired { get; set; }
        public string PaymentStatus { get; set; }
        public Student Student { get; set; }
        public Session Session { get; set; }
    }
}
