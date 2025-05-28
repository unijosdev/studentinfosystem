namespace SwiftKampus.ViewModels.MedResultVm
{
    public class MedCaListVm
    {
        public int MedContiniousAssesmentId { get; set; }
        public string MatricNo { get; set; }
        public string StudentFullName { get; set; }
        public string ResultName { get; set; }
        public string SessionName { get; set; }
        public string LevelName { get; set; }
        public string Score { get; set; }
        public string IsAbsent { get; set; }

    }

    public class MedCaListVmApproval
    {
        public int MedContiniousAssesmentId { get; set; }
       public string CategoryName { get; set; }
       public string CaItem { get; set; }
        public string SessionName { get; set; }
        public string LevelName { get; set; }
        public bool IsDeptApproved { get; set; }
        public bool IsFacultyApproved { get; set; }
        public bool IsSenateApproved { get; set; }
        public string ReasonForReject { get; set; }
        public bool Submitted { get; set; }

    }
}