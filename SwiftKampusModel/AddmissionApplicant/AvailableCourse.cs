using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwiftKampusModel.AddmissionApplicant
{
    public class AvailableCourse
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AvailableCourseId { get; set; }

        public int SchoolProgrammeId { get; set; }

        public int ProgrammeId { get; set; }

        public string ProgrammeName { get; set; }

        public bool IsActive { get; set; }

        public virtual SchoolProgramme SchoolProgramme { get; set; }

        public virtual Programme Programme { get; set; }

        public ICollection<Applicant> Applicants { get; set; }
    }

    public class AvailableCourseVm
    {
        public int AvailableCourseId { get; set; }
        public int SchoolProgrammeId { get; set; }
        public int[] ProgrammeId { get; set; }
        public string ProgrammeName { get; set; }
        public bool IsActive { get; set; }

    }
}
