using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SwiftKampusModel.CBTE
{
    public class ExamType
    {
        public int ExamTypeId { get; set; }

        [Display(Name = "Exam Name")]
        public string ExamName { get; set; }

        public virtual ICollection<ExamSetting> ExamSettings { get; set; }
        public virtual ICollection<QuestionAnswer> QuestionAnswers { get; set; }
        public virtual ICollection<ExamLog> ExamLogs { get; set; }
    }
}
