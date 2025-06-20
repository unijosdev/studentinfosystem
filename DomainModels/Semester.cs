using System;
using DomainModels.ResultModule;

namespace DomainModels;

public class Semester
{
    public Guid SemesterId { get; set; }

    public string SemesterName { get; set; } = string.Empty;

    public ICollection<Course> Courses { get; set; } = [];
    public ICollection<CourseRegistration> CourseRegistrations { get; set; } = [];
    public ICollection<Session> Sessions { get; set; }
    public ICollection<StudentAssignment> StudentAssignments { get; set; }
    public ICollection<DepartmentFeeType> DepartmentFeeTypes { get; set; }
    public ICollection<FacultyFeeType> FacultyFeeTypes { get; set; }
    public ICollection<SchoolFeeType> SchoolFeeTypes { get; set; }
    public ICollection<SchoolFeePayment> SchoolFeePayments { get; set; }
    public ICollection<FacultyFeePayment> FacultyFeePayments { get; set; }
    public ICollection<DepartmentFeePayment> DepartmentFeePayments { get; set; }
    public ICollection<AssignedCourse> AssignedCourses { get; set; }
    public ICollection<TimeTablePeriod> TimeTablePeriods { get; set; }
    public ICollection<ExamTimeTable> ExamTimeTables { get; set; }
    public ICollection<FacultyFeeSetting> FacultyFeeSettings { get; set; }
    public ICollection<DepartmentFeeSetting> DepartmentFeeSettings { get; set; }
    public ICollection<ContinuousAssessment> ContinuousAssessments { get; set; } = [];
    public ICollection<ContinuousAssessmentHistory> ContinuousAssessmentHistories { get; set; } = [];
    public ICollection<Defaulter> Defaulters { get; set; }
    public ICollection<CourseRegistrationSetting> CourseRegSettings { get; set; } = [];
}