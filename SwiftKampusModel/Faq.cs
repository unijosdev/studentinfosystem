using System;
using System.ComponentModel.DataAnnotations;

namespace SwiftKampusModel
{
    public class Faq
    {
        public int FaqId { get; set; }

        [Required]
        [DataType(DataType.MultilineText)]
        public string Question { get; set; }
        [Required]
        public string QuestionBy { get; set; }
        public DateTime AskedDate { get; set; }
        public string Answer { get; set; }
        public string AnswerBy { get; set; }
        public string RepliedDate { get; set; }
    }
}
