using System.ComponentModel.DataAnnotations;

namespace SwiftKampus.ViewModels
{
    public class ResultVm
    {
        public int ResultId { get; set; }

        [Display(Name = "Student Number")]
        [Required(ErrorMessage = "Student Number is required")]
        public int StudentId { get; set; }

        [Display(Name = "Programme Name")]
        [Required(ErrorMessage = "Programme Name is required")]
        public int DepartmentProgrammeId { get; set; }

        [Display(Name = "Semester")]
        [Required(ErrorMessage = "Semester Name is required")]
        public int SemesterId { get; set; }

        [Display(Name = "Session")]
        [Required(ErrorMessage = "Session Name is required")]
        public int SessionId { get; set; }

        public int TotalCourse { get; set; }

        public double TotalGradePoint { get; set; }

        public double TotalQualityPoint { get; set; }

        public double Gpa { get; set; }

        public double Cgpa { get; set; }
    }
}