using System;

namespace DomainModels.ResultModule;

public class CarryOverCourse
    {
        public Guid CarryOverCourseId { get; set; }
        public double Score { get; set; }

        public string StudentId { get; set; } = string.Empty;
        public Guid CourseId { get; set; }
        public Guid DepartmentId { get; set; }
        public Guid ProgrammeId { get; set; }

        public Guid SemesterId { get; set; }
        public Guid SessionId { get; set; }
        public Student Student { get; set; } = default!;
        public Semester Semester { get; set; } = default!;
        public Session Sessions { get; set; } = default!;
        public Course Course { get; set; } = default!;
        public Programme Programme { get; set; } = default!;
    }
