using System.ComponentModel.DataAnnotations;

namespace SwiftKampus.ViewModels
{
    public class SearchCourseVm
    {
        [Display(Name = "Programme Name")]
        [Required(ErrorMessage = "Programme Name is required")]
        public int ProgrammeId { get; set; }

        [Display(Name = "Class Name")]
        [Required(ErrorMessage = "Class Name cannot be empty")]
        public int LevelId { get; set; }

      
    }
}