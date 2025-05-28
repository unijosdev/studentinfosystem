namespace SwiftKampus.ViewModels.Result
{
    public class DeptApprovalVm
    {
        public int CourseId { get; set; }
        public string LevelName { get; set; }
        public string CourseCode { get; set; }
        public string ProgrammeName { get; set; }
        public string SessionName { get; set; }
        public string SemesterName { get; set; }
        public int ContinuousAssessmentId { get; set; }
        public bool IsDeptApproved { get; set; }
        public bool IsFacultyApproved { get; set; }
        public bool IsSenateApproved { get; set; }
        public bool Submitted { get; set; }
        public string ReasonForReject { get; set; }
    }
}