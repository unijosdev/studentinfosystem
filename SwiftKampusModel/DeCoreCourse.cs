namespace SwiftKampusModel
{
    public class DeCoreCourse
    {
        public int DeCoreCourseId { get; set; }
        public int ProgrammeId { get; set; }
        public int CourseId { get; set; }
        public int LevelId { get; set; }

        public Programme Programme { get; set; }
        public Course Course { get; set; }
        public Level Level { get; set; }
    }
}
