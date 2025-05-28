using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwiftKampusModel.AddmissionApplicant
{
    public class Subject
    {
        [Key]
        public int SubjectId { get; set; }

        [Display(Name = "Subject Code")]
        [Required(ErrorMessage = "Subject Code is required")]
        [Index(IsUnique = true)]
        [MaxLength(20)]
        public string CourseCode { get; set; }

        [Display(Name = "Subject Name")]
        [Required(ErrorMessage = "Subject Name is required")]
        [StringLength(50, ErrorMessage = "Subject Name is too long")]
        public string CourseName { get; set; }


        public ICollection<ApplicantOLevelResult> ApplicantOlevelResluts { get; set; }
    }
}
