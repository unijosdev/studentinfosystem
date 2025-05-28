using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwiftKampusModel
{
    public class SupplementaryList
    {
        public int SupplementaryListId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string OtherName { get; set; }

        [Index(IsUnique = true)]
        [MaxLength(20)]
        [Required]
        public string JambRegNo { get; set; }

        public string ReferenceNo { get; set; }

        [Index(IsUnique = true)]
        [MaxLength(50)]
        public string OrderId { get; set; }

        public bool IsPayed { get; set; }
        public DateTime PaymentDate { get; set; }
        public string TransactionMessage { get; set; }
        public string StateOfOrigin { get; set; }
        public string LocalGovtArea { get; set; }
        public string Gender { get; set; }
        public string Age { get; set; }
        public string JambScore { get; set; }
        public string CourseAbbreviation { get; set; }

        public bool IsActive { get; set; }
        public bool IsAdmitted { get; set; }
        [NotMapped]
        public string Hash { get; set; }
        [NotMapped]
        public string ResponseUrl { get; set; }
        public string FullName => LastName + " " + FirstName + " " + OtherName;


        public RemitaPaymentType RemitaPaymentType { get; set; }

    }
}
