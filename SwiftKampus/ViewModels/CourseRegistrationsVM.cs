namespace SwiftKampus.ViewModels
{
    public class CourseRegistrationsIndexVM
    {
        public string StudentName { get; set; }

        public string CourseName { get; set; }

        public string Session { get; set; }

        public string Semester { get; set; }

        public int CreditLoad { get; set; }

        public string ProgrammeName { get; set; }

        public string LevelName { get; set; }

        public int RegistrationId { get; set; }
    }

    public class CoureRegApprovalVm
    {
        public int CourseRegistrationId { get; set; }
        public string FullName { get; set; }
        public string MatricNo { get; set; }
        public string LevelName { get; set; }
        public string SessionName { get; set; }
        public string SemesterName { get; set; }
        public string ProgrammeName { get; set; }
        public string StudentId { get; set; }
        public string IsApproved { get; set; }
        public bool ApprovalStatus { get; set; }
        public bool IsStudentSubmitted { get; set; }
    }
}