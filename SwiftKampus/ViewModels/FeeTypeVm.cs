using SwiftKampusModel;

namespace SwiftKampus.ViewModels
{
    public class FeeTypeVm
    {
    }
    public class DepartmentFeeTypeVm
    {
        public int DepartmentFeeTypeId { get; set; }
        public int DepartmentId { get; set; }
        public int SessionId { get; set; }
        public int[] LevelId { get; set; }
        public int SchoolProgrammeId { get; set; }
        public string FeeName { get; set; }
        public decimal Amount { get; set; }
        public string AmountInWords { get; set; }
        public string Description { get; set; }

    }

    public class FacultyFeeTypeVm
    {
        public int FacultyFeeTypeId { get; set; }
        public int FacultyId { get; set; }
        public int[] LevelId { get; set; }
        public int SemesterId { get; set; }
        public int SchoolProgrammeId { get; set; }
        public string FeeName { get; set; }
        public decimal Amount { get; set; }
        public string AmountInWords { get; set; }
        public string Description { get; set; }
        // public virtual ICollection<FacultyFeePayment> FacultyFeePayments { get; set; }

    }

    public class SchoolFeeTypeVm
    {
        public int SchoolFeeTypeId { get; set; }
        public SchoolFeeCategory FeeCategory { get; set; }
        public string FacultyOrDept { get; set; }
        public string FeeName { get; set; }
        public string FeeCode { get; set; }
        public string StudentType { get; set; }
        public int SchoolProgrammeId { get; set; }
        public string StudentProgramme { get; set; }
        public decimal Amount { get; set; }
        public string AmountInWords { get; set; }
        public int SessionId { get; set; }
        public string Description { get; set; }
        public string Indegine { get; set; }

    }
}