using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwiftKampusModel.Payment
{
    public class ChangeDetailPayment
    {
        public int ChangeDetailPaymentId { get; set; }

        [Display(Name = "Student's Name")]        
        public string StudentId { get; set; }
        public string ApplicantId { get; set; }
        public string StudentCategory { get; set; }
        public string ReferenceNo { get; set; }

        [Index(IsUnique = true)]
        [MaxLength(50)]
        [Required]
        public string OrderId { get; set; }

        [Display(Name = "Fees Type")]
        public int ChangeDetailFeeId { get; set; }

        [Display(Name = "Session")]
        [Required(ErrorMessage = "Session is required")]
        public int SessionId { get; set; }

        [Display(Name = "Total Amount")]
        public decimal TotalAmount { get; set; }

        [Display(Name = "Date of Payment")]
        public DateTime Date { get; set; }

        [Display(Name = "Fee Status")]
        public bool Status { get; set; }

        public string PaymentStatus { get; set; }
        public bool IsExpired { get; set; }

        public Student Students { get; set; }
        public Session Session { get; set; }
        public ChangeDetailFee ChangeDetailFee { get; set; }
    }
}
