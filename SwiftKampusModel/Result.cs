using System.ComponentModel.DataAnnotations;

namespace SwiftKampusModel
{
    public class Result
    {
        public int ResultId { get; set; }

        [Display(Name = "Student Number")]
        //[Required(ErrorMessage = "Student Number is required")]
        public string StudentId { get; set; }

        [Display(Name = "Programme Name")]
        [Required(ErrorMessage = "Programme Name is required")]
        public int ProgrammeId { get; set; }

        [Display(Name = "Semester")]
        [Required(ErrorMessage = "Semester Name is required")]
        public int SemesterId { get; set; }

        [Display(Name = "Session")]
        [Required(ErrorMessage = "Session Name is required")]
        public int SessionId { get; set; }

        public int TotalCourseUnit { get; set; }

        public double TotalGradePoint { get; set; }

        public double TotalQualityPoint { get; set; }

        public string LevelName { get; set; }

        public double Gpa
        {
            get
            {
                return TotalQualityPoint / TotalCourseUnit;
            }
        }

        public double Cgpa { get; set; }

        public virtual Student Student { get; set; }
        public virtual Semester Semester { get; set; }
        public virtual Session Sessions { get; set; }
        //public virtual Course Course { get; set; }
        public virtual Programme Programme { get; set; }
    }


    public class ResultVm
    {

        [Display(Name = "Programme Name")]
        [Required(ErrorMessage = "Programme Name is required")]
        public int ProgrammeId { get; set; }

        [Display(Name = "School Programme Name")]
        [Required(ErrorMessage = "School Programme Name is required")]
        public int SchoolProgrammeId { get; set; }

        [Display(Name = "Semester")]
        [Required(ErrorMessage = "Semester Name is required")]
        public int SemesterId { get; set; }

        [Display(Name = "Session")]
        [Required(ErrorMessage = "Session Name is required")]
        public int SessionId { get; set; }

    }

}
