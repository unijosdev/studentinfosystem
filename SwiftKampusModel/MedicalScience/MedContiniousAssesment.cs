namespace SwiftKampusModel.MedicalScience
{
    public class MedContiniousAssesment
    {
        public int MedContiniousAssesmentId { get; set; }
        public int MedResultCaId { get; set; }
        public int SessionId { get; set; } 
        public string StudentId { get; set; }
        public int Score { get; set; }
        public bool Submitted { get; set; }
        public bool IsDeptApproved { get; set; }
        public bool IsFacultyApproved { get; set; }
        public bool IsSenateApproved { get; set; }
        public bool IsAbsentForExam { get; set; }
        public bool IsDisqualified { get; set; }
        public int NoOfSubmission { get; set; }
        public string StaffName { get; set; }
        public string ReasonForReject { get; set; }
        public Student Student { get; set; }
        public Session Session { get; set; }
        public MedResultCa MedResultCa { get; set; }
    }
}
