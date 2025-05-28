namespace SwiftKampusModel
{
    public class CarryOverCourse
    {
        public int CarryOverCourseId { get; set; }
        public string StudentId { get; set; }
        public int CourseId { get; set; }
        public int DepartmentId { get; set; }
        public int ProgrammeId { get; set; }

        public int SemesterId { get; set; }
        public int SessionId { get; set; }
        public double Score { get; set; }
        public virtual Student Student { get; set; }
        public virtual Semester Semester { get; set; }
        public virtual Session Sessions { get; set; }
        public virtual Course Course { get; set; }
        public virtual Programme Programme { get; set; }
    }
}