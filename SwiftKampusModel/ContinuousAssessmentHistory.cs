using System;

namespace SwiftKampusModel
{
    public class ContinuousAssessmentHistory
    {
        public int ContinuousAssessmentHistoryId { get; set; }
        public int NoOfReject { get; set; }
        public string StudentId { get; set; }
        public int SemesterId { get; set; }
        public int SessionId { get; set; }
        public int CourseId { get; set; }
        public int? ProgrammeId { get; set; }
        public int? LevelId { get; set; }
        public int CourseUnit { get; set; }
        public double CaScore { get; set; }
        public double ExamScore { get; set; }
        public string StaffName { get; set; }
        public double Total { get; set; }
        public string Grading { get; set; }
        public string Remark { get; set; }
        public int GradePoint { get; set; }
        public int QualityPoint { get; set; }
        public string ReasonForReject { get; set; }
        public string RejectedBy { get; set; }
        public DateTime RejectionDate { get; set; }

        public Student Student { get; set; }
        public Semester Semester { get; set; }
        public Session Sessions { get; set; }
        public Course Course { get; set; }
        public Programme Programme { get; set; }
        public Level Level { get; set; }
    }
}
