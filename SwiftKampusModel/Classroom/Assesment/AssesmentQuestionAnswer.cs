using System.ComponentModel.DataAnnotations;

namespace SwiftKampusModel.Classroom.Assesment
{
    public class AssesmentQuestionAnswer
    {
        public int AssesmentQuestionAnswerId { get; set; }

        [Display(Name = "Course Name")]
        [Required(ErrorMessage = "Course Name is required")]
        public int CourseId { get; set; }

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

        public string QuestionType { get; set; }

        [Display(Name = "Fill in the Gap")]
        public bool IsFillInTheGag
        {
            get
            {
                if (QuestionType.Trim().ToUpper().Equals("BLANKCHOICE"))
                {
                    return true;
                }
                return false;
            }
        }

        [Display(Name = "Multi-choice Answer")]
        public bool IsMultiChoiceAnswer
        {
            get
            {
                if (QuestionType.Trim().ToUpper().Equals("MULTICHOICE"))
                {
                    return true;
                }
                return false;
            }
        }

        public bool IsSingleChoiceAnswer
        {
            get
            {
                if (QuestionType.Trim().ToUpper().Equals("SINGLECHOICE"))
                {
                    return true;
                }
                return false;
            }
        }

        public Course Course { get; set; }
    }
}