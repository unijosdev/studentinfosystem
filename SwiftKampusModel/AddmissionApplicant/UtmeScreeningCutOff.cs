using System.ComponentModel.DataAnnotations;

namespace SwiftKampusModel.AddmissionApplicant
{
    public class UtmeScreeningCutOff
    {
        public int UtmeScreeningCutOffId { get; set; }
        [Required]
        public int? SessionId { get; set; }
        [Required]
        public int? ProgrammeId { get; set; }

        [Range(100, 400)]
        public int CutOffMark { get; set; }
        public Programme Programme { get; set; }
        public Session Session { get; set; }
    }
}
