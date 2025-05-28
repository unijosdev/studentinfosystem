using EndWellJobApplication.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SwiftKampusModel.AddmissionApplicant
{
    public class ApplicantProgramme
    {
        public int ApplicantProgrammeId { get; set; }

        [Display(Name = "School Programme Name")]
        [StringLength(70, ErrorMessage = "Your Programme name is too long")]
        public string Name { get; set; }

        public virtual ICollection<Applicant> Applicants { get; set; }

        public virtual ICollection<ScreeningRule> ScreeningRules { get; set; }
    }
}