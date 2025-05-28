using SwiftKampusModel.AddmissionApplicant;
using SwiftKampusModel.CBTE;
using SwiftKampusModel.Misconduct;
using SwiftKampusModel.Payment;
using SwiftKampusModel.TimeTable;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwiftKampusModel
{
    public class Semester
    {
        [Key]
        public int SemesterId { get; set; }

        [Index(IsUnique = true)]
        [MaxLength(20)]
        public string SemesterName { get; set; }    
           
        public ICollection<Course> Courses { get; set; }
        public ICollection<CourseRegistration> CourseRegistrations { get; set; }
        public ICollection<Session> Sessions { get; set; }
        public ICollection<ExamSetting> ExamSettings { get; set; }
        public ICollection<ExamLog> ExamLogs { get; set; }
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
        public ICollection<ContinuousAssessment> ContinuousAssessments { get; set; }
        public ICollection<ContinuousAssessmentHistory> ContinuousAssessmentHistories { get; set; }
        public ICollection<Defaulter> Defaulters { get; set; }
        public ICollection<CourseRegSetting> CourseRegSettings { get; set; }




    }
}
