namespace SwiftKampusModel
{
    public class AssignStudentLevel
    {
        public int AssignStudentLevelId { get; set; }
        public int FacultyId { get; set; }
        public int SessionId { get; set; }
        public bool IsMigrated { get; set; }
        public int NumberOfSuccesfulMigration { get; set; }
        public int NumberOfUnSuccesfulMigration { get; set; }
        public Faculty Faculty { get; set; }
        public Session Session { get; set; }

    }
}
