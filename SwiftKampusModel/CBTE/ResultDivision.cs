using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwiftKampusModel.CBTE
{
    public class ResultDivision
    {
        [ForeignKey("Course")]
        [Display(Name = "Course Name")]
        [Required(ErrorMessage = "Course Name is required")]
        public int CourseId { get; set; }

        [Display(Name = "Level Name")]
        [Required(ErrorMessage = "Level Name is required")]
        public int LevelId { get; set; }
    }
}
