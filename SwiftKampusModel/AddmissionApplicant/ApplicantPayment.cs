using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwiftKampusModel.AddmissionApplicant
{
    public class ApplicantPayment
    {
        public int ApplicantPaymentId { get; set; }
        public int SchoolProgrammeId { get; set; }

       // [Index(IsUnique = true)]
        [MaxLength(90)]
        public string ApplicantEmail { get; set; }
        public string ReferenceNo { get; set; }
        public string JambRegNo { get; set; }
        public string FullName { get; set; }

        [Index(IsUnique = true)]
        [MaxLength(90)]
        public string OrderId { get; set; }
        public decimal AmountPayed { get; set; }
        public int SessionId { get; set; }

        public decimal TotalAmount { get; set; }
        public decimal ExpectedAmount { get; set; }

        [Display(Name = "Date of Payment")]
        public DateTime PaymentDateTime { get; set; }
        public bool IsPayed { get; set; }
        public string TransactionMessage { get; set; }
        public SchoolProgramme SchoolProgramme { get; set; }
        public Session Session { get; set; }
    }


}
