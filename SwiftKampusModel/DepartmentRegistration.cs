namespace SwiftKampusModel
{
    public class DepartmentRegistration
    {
        public int DepartmentRegistrationId { get; set; }
        public string StudentId { get; set; }
        public int SemesterId { get; set; }
        public int SessionId { get; set; }

        public int LevelId { get; set; }

        public virtual Student Students { get; set; }
        public virtual Semester Semester { get; set; }
        public virtual Session Session { get; set; }
        public virtual Level Level { get; set; }
    }
}
