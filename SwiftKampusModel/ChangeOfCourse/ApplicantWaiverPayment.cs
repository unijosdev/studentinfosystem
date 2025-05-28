using SwiftKampusModel.AddmissionApplicant;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwiftKampusModel.ChangeOfCourse
{
    public class ApplicantWaiverPayment
    {
        public int ApplicantWaiverPaymentId { get; set; }
        public int? ProgrammeId { get; set; }
        public string ApplicantId { get; set; }
        public string ReferenceNo { get; set; }

        [Index(IsUnique = true)]
        [MaxLength(90)]
        public string OrderId { get; set; }
        public int SessionId { get; set; }

        public decimal TotalAmount { get; set; }

        [Display(Name = "Date of Payment")]
        public DateTime PaymentDateTime { get; set; }
        public bool IsPayed { get; set; }
        public bool IsExpired { get; set; }
        public bool IsProcessed { get; set; }
        public bool? IsDownloaded { get; set; }
        public string ProcessedStatus { get; set; }
        public int NoOfSubmit { get; set; }
        public string TransactionMessage { get; set; }
        public string ChangeOfCourseType { get; set; }

        public Session Session { get; set; }
        public Applicant Applicant { get; set; }
        public Programme Programme { get; set; }
    }
}
