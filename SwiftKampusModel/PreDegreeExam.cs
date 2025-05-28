using System.ComponentModel.DataAnnotations.Schema;

namespace SwiftKampusModel
{
    public class PreDegreeExam
    {
        public int PreDegreeExamId { get; set; }
        public string RegNo { get; set; }
        public string SubjectName { get; set; }
        public double Score { get; set; }
        //public virtual PreDegreeStudent PreDegreeStudent { get; set; }

    }
}
