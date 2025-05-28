using System.ComponentModel.DataAnnotations;

namespace SwiftKampusModel.AddmissionApplicant
{
    public class AwardPrice
    {
        public int AwardPriceId { get; set; }
        public string ApplicantId { get; set; }
        [Required]
        [Display(Name = "Awarding Body")]
        public string AwardingBody { get; set; }

        [Display(Name = "Academic Price")]
        public string AcademicPrice { get; set; }

        [Display(Name = "Award Years")]
        public string Year { get; set; }

    }
}
