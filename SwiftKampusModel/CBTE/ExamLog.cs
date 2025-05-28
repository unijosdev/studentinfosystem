namespace SwiftKampusModel.CBTE
{
    public class ExamLog
    {
        public int ExamLogId { get; set; }
        public string StudentId { get; set; }
        public int CourseId { get; set; }
        public int LevelId { get; set; }
        public int SemesterId { get; set; }
        public int SessionId { get; set; }
        public int ExamTypeId { get; set; }
        public double Score { get; set; }
        public double TotalScore { get; set; }
        public bool ExamTaken { get; set; }
        public virtual Student Student { get; set; }

        public virtual Course Course { get; set; }

        public virtual Level Level { get; set; }
        public virtual ExamType ExamType { get; set; }
        public virtual Semester Semester { get; set; }
        public virtual Session Sessions { get; set; }

    }
}
