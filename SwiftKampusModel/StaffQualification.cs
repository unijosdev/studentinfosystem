using System;
using System.ComponentModel.DataAnnotations;

namespace SwiftKampusModel
{
    public class StaffQualification
    {
        public int StaffQualificationId { get; set; }

        [Display(Name = "Qualification Type")]
        [Required(ErrorMessage = "Qualification Type is Required")]
        public string QualificationType { get; set; }

        [Display(Name = "Name of Qualification")]
        [Required(ErrorMessage = "Qualification is Required")]
        public string QualificationName { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date")]
        public DateTime DateObtained { get; set; }

        public string StaffId { get; set; }

        public virtual Staff Staff { get; set; }
    }
}
