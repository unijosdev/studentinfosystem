namespace SwiftKampusModel.Classroom.Quiz
{
    public class QuizLog
    {
        public int QuizLogId { get; set; }
        public string StudentId { get; set; }
        public int TopicId { get; set; }
        public int LevelId { get; set; }
        public int SemesterId { get; set; }
        public int SessionId { get; set; }
        public int ExamTypeId { get; set; }
        public double Score { get; set; }
        public double TotalScore { get; set; }
        public bool ExamTaken { get; set; }
        public virtual Student Student { get; set; }
        public virtual Topic Topic { get; set; }


    }
}
