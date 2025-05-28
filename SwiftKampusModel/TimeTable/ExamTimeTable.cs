using System.Collections.Generic;

namespace SwiftKampusModel.TimeTable
{
    public class ExamTimeTable
    {
        public int ExamTimeTableId { get; set; }
        public int CourseId { get; set; }
        public int SemesterId { get; set; }
        public int SessionId { get; set; }
        public bool IsPublishTimeTable { get; set; }
        public Course Course { get; set; }
        public virtual Semester Semester { get; set; }
        public virtual Session Session { get; set; }
        public virtual ICollection<ExamTimeTableAllocation> ExamTimeTableAllocations { get; set; }
    }
}