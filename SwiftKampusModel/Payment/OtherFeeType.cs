using SwiftKampusModel.AddmissionApplicant;
using System.Collections.Generic;

namespace SwiftKampusModel.Payment
{
    public class DepartmentFeeType
    {
        public int DepartmentFeeTypeId { get; set; }
        public int DepartmentId { get; set; }
        public int SessionId { get; set; }
        public int? LevelId { get; set; }
        public int SchoolProgrammeId { get; set; }
        public string FeeName { get; set; }
        public decimal Amount { get; set; }
        public string AmountInWords { get; set; }
        public string Description { get; set; }
        public Session Session { get; set; }
        public Department Department { get; set; }
        public Level Level { get; set; }
        public SchoolProgramme SchoolProgramme { get; set; }

        public ICollection<DepartmentFeeSetting> DepartmentFeeSettings { get; set; }


    }

    public class FacultyFeeType
    {
        public int FacultyFeeTypeId { get; set; }
        public int FacultyId { get; set; }
        public int? LevelId { get; set; }
        public int SemesterId { get; set; }
        public int SchoolProgrammeId { get; set; }
        public string FeeName { get; set; }
        public decimal Amount { get; set; }
        public string AmountInWords { get; set; }
        public string Description { get; set; }
        public Semester Semester { get; set; }
        public SchoolProgramme SchoolProgramme { get; set; }
        public Faculty Faculty { get; set; }
        public Level Level { get; set; }
        public ICollection<FacultyFeeSetting> FacultyFeeSettings { get; set; }

    }

}
