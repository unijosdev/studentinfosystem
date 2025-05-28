using System.ComponentModel.DataAnnotations;

namespace StAugustine.ViewModel.CBTE
{
    public class SelectSubjectViewModel
    {
        [Display(Name = "Subject Name")]
        [Required(ErrorMessage = "Subject Name is required")]
        public int SubjectName { get; set; }

        [Display(Name = "LevelId Name")]
        [Required(ErrorMessage = "LevelId Name is required")]
        public int LevelId { get; set; }

        public int ExamTypeId { get; set; }
    }
}