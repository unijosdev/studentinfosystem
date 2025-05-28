using SwiftKampusModel.Attendance;
using SwiftKampusModel.Payment;
using SwiftKampusModel.TimeTable;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SwiftKampusModel.AddmissionApplicant
{
    public class SchoolProgramme
    {
        public int SchoolProgrammeId { get; set; }
        public int? SessionId { get; set; }

        [Display(Name = "Programme Type")]
        [Required]
        public string ProgrammeType { get; set; }

        [Required]
        public string ProgrammeCategory { get; set; }

        [Display(Name = "Form Active")]
        public bool ActiveSale { get; set; }

        public string Duration { get; set; }
        [Required]
        public string FancyName { get; set; }

        [Required]
        public string SchoolProgrammeCode { get; set; }

        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [DataType(DataType.Date)]
        public DateTime ClosingDate { get; set; }

        public string ImageName { get; set; }

        public string FullName => ProgrammeCategory + " " + ProgrammeType;

        public Session Session { get; set; }
        public ICollection<Applicant> Applicants { get; set; }
        public ICollection<Level> Levels { get; set; }
        public ICollection<AvailableCourse> AvailableCourses { get; set; }
        public ICollection<UnderGraduateRule> UnderGraduateRules { get; set; }
        public ICollection<ApplicantFeeSetting> ApplicantFeeSettings { get; set; }
        public ICollection<Student> Students { get; set; }
        public ICollection<SchoolFeeType> SchoolFeeTypes { get; set; }
        public ICollection<DepartmentFeeType> DepartmentFeeTypes { get; set; }
        public ICollection<FacultyFeeType> FacultyFeeTypes { get; set; }
        public ICollection<Course> Courses { get; set; }
        public ICollection<ChangeOfCourseFee> ChangeOfCourseFees { get; set; }
        public ICollection<AssignSessionToSchool> AssignSessionToSchools { get; set; }
        public ICollection<AssignSemesterToSchool> AssignSemesterToSchools { get; set; }
        public ICollection<TimeTablePeriod> TimeTablePeriods { get; set; }
        public ICollection<StudentAttendance> StudentAttendances { get; set; }
        public ICollection<ClassDegree> ClassDegrees { get; set; }
        public ICollection<DeptResultType> DeptResultTypes { get; set; }
        public ICollection<CourseRegSetting> CourseRegSettings { get; set; }


    }


}
