using SwiftKampusModel;
using SwiftKampusModel.AddmissionApplicant;

namespace SwiftKampus.ViewModels
{
    public class AllActiveStudentsVM
    {
        public string StudentId { get; set; }
        public string MatricNO { get; set; }
        public string JambNo { get; set; }
        public string Fullname { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Middlename { get; set; }
        public string Gender { get; set; }
        public string PhoneNumber { get; set; }
        public string SchoolCode { get; set; }
        public string SecondaryEmail { get; set; }
        public string DeptOptionNAme { get; set; }
        public string LevelName { get; set; }
        public string DeptName { get; set; }
        public string FacultyName { get; set; }
        public string Nationality { get; set; }
        public string State { get; set; }
        public string LocalGovernment { get; set; }
        public string ModeOfEntry { get; set; }
        public string MaritalStatus { get; set; }
        public string PhysicalStatus { get; set; }
        public string StudentStatus { get; set; }
        public string Session { get; set; }
        public string SchoolFeeCharge { get; set; }
        public int DepartmentId { get; set; }
        public System.DateTime? DateUploaded { get; set; }
        public System.DateTime? DateOfBirth { get; set; }
        public Student student { get; set; }

        public DirectEntryExam directEntryExam { get; set; }
    }

    public class RegistrationReportVm
    {
        public int Id { get; set; }
        public string DepartmentName { get; set; }
        public int UTMERegCount { get; set; }
        public int DERegCount { get; set; }
        public int NotRegistered { get; set; }
        public int TotalRegistered { get; set; }
        public int TotalAdmitted { get; set; }
    }
}