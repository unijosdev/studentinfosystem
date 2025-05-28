using SwiftKampusModel.Attendance;
using SwiftKampusModel.Employee;
using SwiftKampusModel.Employee.Leave;
using SwiftKampusModel.Misconduct;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SwiftKampusModel
{
    public class Staff : Person
    {
        [Key]
        public string StaffId { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        [Display(Name = "Hire Date")]
        public DateTime? HireDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? ResumptionDate { get; set; }


        public string Designation { get; set; }

        public bool IsActiveStaff { get; set; }
        public string ActiveStatus { get; set; }

        public int? DepartmentId { get; set; }

        // [Required]
        public string StaffRole { get; set; }

        public Department Department { get; set; }

        public ICollection<Course> Courses { get; set; }
        public OfficeAssignment OfficeAssignment { get; set; }

        public ICollection<Executive> Executives { get; set; }
        public ICollection<DeptPosition> DeptPositions { get; set; }
        public ICollection<FacultyPosition> FacultyPositions { get; set; }
        public ICollection<StaffQualification> StaffQualifications { get; set; }
        public ICollection<AssignedCourse> AssignedCourses { get; set; }

        public ICollection<EmployeeAccountDetail> EmployeeAccountDetails { get; set; }
        public ICollection<LeaveApplication> LeaveApplications { get; set; }
        public ICollection<StaffAttendance> StaffAttendance { get; set; }
        public ICollection<StudentDisciplinaryStatus> StudentDisciplinaryStatus { get; set; }


    }

    public class StaffIndexVm
    {
        public string StaffId { get; set; }
        public string FullName { get; set; }
        public string Gender { get; set; }
        public string DeptName { get; set; }
    }
}