using System;
using System.ComponentModel.DataAnnotations;

namespace SwiftKampusModel.AddmissionApplicant
{
    public class EmploymentDetail
    {
        public int EmploymentDetailId { get; set; }
        public string ApplicantId { get; set; }
        [Required]
        public string EmployeeName { get; set; }
        public string Address { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "From Date")]
        public DateTime FromDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "To Date")]
        public DateTime ToDate { get; set; }
    }
}
