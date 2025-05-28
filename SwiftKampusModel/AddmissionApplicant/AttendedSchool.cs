using System;
using System.ComponentModel.DataAnnotations;

namespace SwiftKampusModel.AddmissionApplicant
{
    public class AttendedSchool
    {
        public int AttendedSchoolId { get; set; }

        public string ApplicantId { get; set; }

        [Display(Name = "Name of School")]
        [StringLength(70, ErrorMessage = "Your School name is too long")]
        [Required]
        public string SchoolName { get; set; }

        [Display(Name = "School Address")]
        public string SchoolAddress { get; set; }

        [Display(Name = "Qualification Name")]
        public string Degree { get; set; }

        [Display(Name = "Course of Study")]
        [Required]
        public string CourseOfStudy { get; set; }

        [Display(Name = "Class of Degree")]
        [Required]
        public string ClassOfDegree { get; set; }

        [Display(Name = "CGPA Grade")]
        [StringLength(25, ErrorMessage = "Your CGPA is too long")]
        [Required]
        public string ResultGrade { get; set; }

        [Display(Name = "From Date")]
        [DataType(DataType.Date)]
        public DateTime FromDate { get; set; }

        [Display(Name = "To Date")]
        [DataType(DataType.Date)]
        public DateTime ToDate { get; set; }

    }
}