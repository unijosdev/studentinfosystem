using SwiftKampusModel.AddmissionApplicant;
using System;
using System.ComponentModel.DataAnnotations;

namespace SwiftKampusModel.Payment
{
    public class SchoolFeeSetting
    {
        public int SchoolFeeSettingId { get; set; }
        public int SchoolProgrammeId { get; set; }
        public string FeeCategory { get; set; }
        [Required]
        public double FinedAmount { get; set; }
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }
        public int SessionId { get; set; }
        public bool IsActive { get; set; }
        public Session Session { get; set; }
        public SchoolProgramme SchoolProgramme { get; set; }

    }
    public class FacultyFeeSetting
    {
        public int FacultyFeeSettingId { get; set; }
        public int SchoolFeeSettingId { get; set; }
        public int FacultyFeeTypeId { get; set; }
        public double FinedAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? SemesterId { get; set; }
        public int? SessionId { get; set; }
        public bool IsActive { get; set; }
        public Semester Semester { get; set; }
        public Session Session { get; set; }
        public FacultyFeeType FacultyFeeType { get; set; }
        public SchoolProgramme SchoolProgramme { get; set; }


    }
    public class DepartmentFeeSetting
    {
        public int DepartmentFeeSettingId { get; set; }
        public int SchoolFeeSettingId { get; set; }
        public int DepartmentFeeTypeId { get; set; }
        public double FinedAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? SemesterId { get; set; }
        public int? SessionId { get; set; }
        public bool IsActive { get; set; }
        public virtual Semester Semester { get; set; }
        public virtual Session Session { get; set; }
        public virtual DepartmentFeeType DepartmentFeeType { get; set; }
        public SchoolProgramme SchoolProgramme { get; set; }



    }
}
