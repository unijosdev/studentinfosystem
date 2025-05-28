using System.ComponentModel.DataAnnotations;

namespace SwiftKampusModel.AddmissionApplicant
{
    public class UtmeScreningPolicy
    {
        public int UtmeScreningPolicyId { get; set; }

        public int SessionId { get; set; }

        [Required]
        public double JambPercentage { get; set; }

        [Required]
        public double OLevelPercentage { get; set; }
        public int JambMaximumScore { get; set; }
        public int OLevelMaximumScore { get; set; }

        public Session Session { get; set; }
    }
}
