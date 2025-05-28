using SwiftKampusModel.AddmissionApplicant;
using System;
using System.ComponentModel.DataAnnotations;

namespace SwiftKampusModel
{
    public class CourseRegSetting
    {
        public int CourseRegSettingId { get; set; }
        public int SchoolProgrammeId { get; set; }
        public int SessionId { get; set; }
        public int SemesterId { get; set; }
        [DataType(DataType.Date)]
        public DateTime ClosingDate { get; set; }
        [DataType(DataType.Date)]
        public DateTime OpeningDate { get; set; }
        public bool IsActive { get; set; }
        public bool ActivatePrequisite { get; set; }
        public SchoolProgramme SchoolProgramme { get; set; }
        public Session Session { get; set; }
        public Semester Semester { get; set; }

    }
}
