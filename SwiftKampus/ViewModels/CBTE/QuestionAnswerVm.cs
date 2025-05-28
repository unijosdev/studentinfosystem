using SwiftKampusModel;
using System.ComponentModel.DataAnnotations;

namespace SwiftKampus.ViewModels.CBTE
{
    public class QuestionAnswerVm
    {
        public int QuestionAnswerId { get; set; }

        [Display(Name = "Course Name")]
        [Required(ErrorMessage = "Course Name is required")]
        public int CourseId { get; set; }

        [Display(Name = "LevelId Name")]
        //[Required(ErrorMessage = "LevelId Name is required")]
        public int LevelId { get; set; }

        [Display(Name = "Exam Type")]
        public int ExamTypeId { get; set; }

        [Display(Name = "Question")]
        [Required(ErrorMessage = "Question is required")]
        [DataType(DataType.MultilineText)]
        public string Question { get; set; }

        [Display(Name = "Option 1")]
        //[Required(ErrorMessage = "Option 1 is required")]
        [DataType(DataType.MultilineText)]
        public string Option1 { get; set; }

        [Display(Name = "Option 2")]
        //[Required(ErrorMessage = "Option 2 is required")]
        [DataType(DataType.MultilineText)]
        public string Option2 { get; set; }

        [Display(Name = "Option 3")]
        //[Required(ErrorMessage = "Option 3 is required")]
        [DataType(DataType.MultilineText)]
        public string Option3 { get; set; }

        [Display(Name = "Option 4")]
        // [Required(ErrorMessage = "Option 4 is required")]
        [DataType(DataType.MultilineText)]
        public string Option4 { get; set; }


        [Display(Name = "Answer")]
        [Required(ErrorMessage = "Answer is required")]
        [DataType(DataType.MultilineText)]
        public string Answer { get; set; }

        [Display(Name = "Question Hint")]
        public string QuestionHint { get; set; }

        public QuestionType QuestionType { get; set; }
    }
}