namespace SwiftKampus.ViewModels.CBTE
{
    public class ExamLogVm
    {
        public int ExamLogId { get; set; }
        public string StudentId { get; set; }
        public int CourseId { get; set; }
        public int LevelId { get; set; }
        public int SemesterId { get; set; }
        public int SessionId { get; set; }
        public int ExamTypeId { get; set; }
        public double Score { get; set; }
        public bool ExamTaken { get; set; }
    }
}