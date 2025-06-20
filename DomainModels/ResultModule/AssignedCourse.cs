using System;

namespace DomainModels.ResultModule;

public class AssignedCourse
{
    public int AssignedCourseId { get; set; }
    public bool IsMainLecture { get; set; }
    public string StaffId { get; set; } = string.Empty;
    public Guid CourseId { get; set; }
    public Guid SessionId { get; set; }
    public Guid SemesterId { get; set; }
    public Staff Staff { get; set; } = default!;
    public Course Course { get; set; } = default!;
    public Semester Semester { get; set; } = default!;
    public Session Session { get; set; } = default!;
}
