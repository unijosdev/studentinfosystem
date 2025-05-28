namespace SwiftKampusModel
{
    public class AssignCourseToCategory
    {
        public int AssignCourseToCategoryId { get; set; }
        public int CourseId { get; set; }
        public int CourseCategoryId { get; set; }
        public Course Course { get; set; }
        public CourseCategory CourseCategory { get; set; }
    }

    public class AssignCourseToCategoryVm
    {
        public int AssignCourseToCategoryId { get; set; }
        public int[] CourseId { get; set; }
        public int CourseCategoryId { get; set; }
    }
}
