using System.ComponentModel.DataAnnotations;

namespace SwiftKampusModel
{
    public class StudentAssignment
    {
        public int StudentAssignmentId { get; set; }

        [Display(Name = "Student ID")]
        [Required(ErrorMessage = "Your Student ID Number is required")]
        [StringLength(25, ErrorMessage = "Your Student ID is too long")]
        public string StudentId { get; set; }

        [Display(Name = "Semester")]
        [Required(ErrorMessage = "Semester is required")]
        public int SemesterId { get; set; }

        [Display(Name = "Session")]
        [Required(ErrorMessage = "Session is required")]
        public int SessionId { get; set; }

        public int? LevelId { get; set; }

        [Display(Name = "Course Name")]
        [Required(ErrorMessage = "Course Name is required")]
        public int CourseId { get; set; }

        public int? ProgrammeId { get; set; }

        [Display(Name = "Assignment Question")]
        [StringLength(25, ErrorMessage = "Your Assignment question is too long")]
        public string AssignmentQuestion { get; set; }

        [Display(Name = "Answer")]
        [Required(ErrorMessage = "Your Answer  is required")]
        public string AssignmentAnswer { get; set; }

        public virtual Student Student { get; set; }
        public virtual Semester Semester { get; set; }
        public virtual Session Sessions { get; set; }
        public virtual Course Course { get; set; }
        public virtual Programme Programme { get; set; }
        public virtual Level Level { get; set; }
    }
}
