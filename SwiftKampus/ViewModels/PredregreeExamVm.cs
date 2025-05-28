using SwiftKampusModel;
using System.Collections.Generic;

namespace SwiftKampus.ViewModels
{
    public class PredregreeExamVm
    {
        public virtual PreDegreeStudent PreDegreeStudent { get; set; }
        public ICollection<PreDegreeExam> PreDegreeExams { get; set; }
    }
}