using SwiftKampusModel.AddmissionApplicant;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SwiftKampusModel.Payment
{
    public class SchoolFeeType
    {
        public int SchoolFeeTypeId { get; set; }
        public int? SessionId { get; set; }
        public int? FacultyId { get; set; }
        public string FeeCategory { get; set; }
        public string FeeCode { get; set; }
        [Required]
        public string FeeName { get; set; }
        public int SchoolProgrammeId { get; set; }
        public decimal Amount { get; set; }
        public string AmountInWords { get; set; }
        public string Description { get; set; }
        public string StudentType { get; set; }
        public string Indegine { get; set; }

        public Session Session { get; set; }
        public SchoolProgramme SchoolProgramme { get; set; }
        public Faculty Faculty { get; set; }

        public ICollection<SchoolFeePayment> SchoolFeePayments { get; set; }
    }
}