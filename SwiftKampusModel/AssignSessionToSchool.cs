using SwiftKampusModel.AddmissionApplicant;
using System.ComponentModel.DataAnnotations;

namespace SwiftKampusModel
{
    public class AssignSessionToSchool
    {
        public int AssignSessionToSchoolId { get; set; }
        public int SessionId { get; set; }
        public int SchoolProgrammeId { get; set; }

        [Display(Name = "Current Session")]
        public bool ActiveSession { get; set; }

        public Session Session { get; set; }
        public SchoolProgramme SchoolProgramme { get; set; }
    }

    public class AssignSemesterToSchool
    {
        public int AssignSemesterToSchoolId { get; set; }
        public int SemesterId { get; set; }
        public int SchoolProgrammeId { get; set; }

        [Display(Name = "Current Semester")]
        public bool ActiveSemester { get; set; }
        public Semester Semester { get; set; }
        public SchoolProgramme SchoolProgramme { get; set; }
    }

    //Active Session for a given Programme
    public class AssignSessionToProgramme
    {
        public int AssignSessionToProgrammeId { get; set; }
        public int SessionId { get; set; }
        public int ProgrammeId { get; set; }

        [Display(Name = "Current Session")]
        public bool ActiveSession { get; set; }

        public Session Session { get; set; }
        public Programme Programme { get; set; }
    }

    //Active Semester for a given Programme
    public class AssignSemesterToProgramme
    {
        public int AssignSemesterToProgrammeId { get; set; }
        public int SemesterId { get; set; }
        public int ProgrammeId { get; set; }

        [Display(Name = "Current Semester")]
        public bool ActiveSemester { get; set; }
        public Semester Semester { get; set; }
        public Programme Programme { get; set; }
    }
}
