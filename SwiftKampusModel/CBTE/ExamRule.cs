using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwiftKampusModel.CBTE
{
    public class ExamRule
    {
        public int ExamRuleId { get; set; }

        [ForeignKey("Course")]
        [Display(Name = "Course Name")]
        [Required(ErrorMessage = "Course Name is required")]
        public int CourseId { get; set; }

        [Display(Name = "Level Name")]
        [Required(ErrorMessage = "Level Name is required")]
        public int LevelId { get; set; }

        public int ResultDivision { get; set; }

        [Display(Name = "Score per Question")]
        [Required(ErrorMessage = "Score per Question is required")]
        public double ScorePerQuestion { get; set; }

        [Display(Name = "Total Question")]
        [Range(1, 100)]
        [Required(ErrorMessage = "Total Question is required")]
        public int TotalQuestion { get; set; }

        [Display(Name = "Duration In Minute")]
        [Required(ErrorMessage = "Maximum Exam Time is required")]
        public int MaximumTime { get; set; }

        public virtual Course Course { get; set; }

        public virtual Level Level { get; set; }
    }


}