namespace SwiftKampus.Models
{
    public static class RoleName
    {
        public const string Admin = "Admin";
        public const string Student = "Student";
        public const string Academic = "Academic";
        public const string None_Academic = "None_Academic";
        public const string SuperAdmin = "SuperAdmin";
        public const string PreDegree = "PreDegree";
        public const string UnderGraduate = "UnderGraduate";
        public const string Hod = "Hod";
        public const string Dean = "Dean";
        public const string Registrar = "Registrar";
        public const string Registry = "Registry";
        public const string Bursar = "Bursar";
        public const string Bursary = "Bursary";
        public const string Librarian = "Librarian";
        public const string DatabaseAdmin = "DatabaseAdmin";
        public const string Applicant = "Applicant";
        public const string TSupport = "TSupport";
        public const string StudentAffairs = "StudentAffairs";
        public const string DStudentAffairs = "Dean StudentAffairs";
        public const string HostelSupervisor = "Hostel Supervisor";
        public const string HostelPorter = "Hostel Porter";
        public const string Security = "Security";
        public const string VC = "VC";
        public const string DVC = "DVC";
        public const string AClearanceOfficer = "AClearanceOfficer";
        public const string DClearanceOfficer = "DClearanceOfficer";
        public const string FClearanceOfficer = "FClearanceOfficer";
        public const string LCordinator = "LCordinator";
        public const string AcademicOfficeSup = "AcademicOfficeSup";
        public const string AcademicOffice = "AcademicOffice";
        public const string Audit = "Audit";
        public const string DAPM_Officer = "DAPM_Officer";
        public const string DAPM_Sup = "DAPM_Sup";
        public const string HodAdmissionOfficer = "HodAdmissionOfficer";    
        public const string PGAdmissionOfficer = "PGAdmissionOfficer";    
        public const string ExamAndRecordDeskOfficer = "Exam And Record Desk Officer";    
        public const string ExamAndRecordSupervisor = "Exam And Record Supervisor";    
        public const string EmailDownload = "Email Download";    
        public const string HealthOfficer = "Health Officer";    
        public const string StudentDisciplinary = "Student Disciplinary";    
        public const string StaffTraningAndDev = "Staff Training And Development";    
        public const string IdCardOperator = "Id Card Operator";    
        public const string IdCardSupervisor = "Id Card Supervisor";
        public const string DeptExaminationOfficer = "Dept. Exam Officer";
        public const string AdmissionUploadOfficer = "Admission Upload Officer";
        public const string AdmissionUploadSupervisor = "Admission Upload Supervisor";
        public const string SundryAndOtherIncomeMgt = "Sundry,And Other Income Mgt";
        public const string StudentDownloadLibrary = "Students Download Library";
        public const string TimetableManagement = "Timetable Management";
        public const string AffiliateSchoolMgt = "Affiliate Sch Mgt";
        public const string ComputerBaseTest = "CBT Management";
        public const string HodRemedial = "HodRemedial";
        public const string DirectorGST = "DirectorGST";

    }

    public static class SchoolSetUp
    {

        public static string CurrentSchoolName = "UNIJOS";
        public static string Description = "UNIJOS";
        public static string Keywords = "The primary mission of the University of Jos is to serve the people of" +
                                            " Nigeria and humanity at large by:Encouraging and promoting a culture of ... ";
        public static string SchoolBanner = "ujlogo.png";
        public static string SchoolLogo = "ujlogo.png";
        public static string SchoolBackgroundImage = "watermark.png";
        public static string StateOfOrigin = "PLATEAU";
        public static bool IsDeptSchoolFee = true;
        public static bool IsPartPaymet = true;

        public static string BlobAddress = "https://unijosdemostorage.blob.core.windows.net/unijosblobstorage/";
    }


    public static class CreditLoadDefault
    {
        public const int CreditLoad = 24;
    }
}