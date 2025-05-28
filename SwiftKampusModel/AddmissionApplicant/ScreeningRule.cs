using SwiftKampusModel;
using SwiftKampusModel.AddmissionApplicant;

namespace EndWellJobApplication.Models
{
    public class ScreeningRule
    {
        public int ScreeningRuleId { get; set; }
        public int ApplicantPositionId { get; set; }

        public int OLevelResultPercentage { get; set; }

        public int JambScorePercentage { get; set; }

        public int PricipalYears { get; set; }
        public Qualifications RequiredQualifications { get; set; }
        public int SessionId { get; set; }
        public Session session { get; set; }
        public virtual ApplicantProgramme ApplicantProgramme { get; set; }
    }
}