using System;

namespace DomainModels.ResultModule;

public class AssignedCourse
    {
        public int AssignedCourseId { get; set; }
        public string StaffId { get; set; }
        public int CourseId { get; set; }
        public int? SessionId { get; set; }
        public int? SemesterId { get; set; }
        public bool IsMainLecture { get; set; }
        public virtual Staff Staff { get; set; }
        public virtual Course Course { get; set; }
        public virtual Semester Semester { get; set; }
        public virtual Session Session { get; set; }
    }
