namespace SwiftKampusModel
{
    public class CoursePrerequisite
    {
        public int CoursePrerequisiteId { get; set; }
        public int CourseId { get; set; }
        public int PrerequisiteCourseId { get; set; }
        public bool IsSiwes { get; set; }
        public Course Course { get; set; }
    }
}
