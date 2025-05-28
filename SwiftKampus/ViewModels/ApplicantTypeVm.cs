using System.ComponentModel.DataAnnotations;

namespace SwiftKampus.Controllers
{
    public class ApplicantTypeVm
    {
        public string ApplicantType { get; set; }
        public string TimeType { get; set; }
    }

    public class ApplicantNotificationVm
    {
        [Display(Name ="Session Name")]
        [Required]
        public int SessionId { get; set; }

        [Display(Name = "School Programme")]
        [Required]
        public int SchoolProgrammeId { get; set; }

        [Display(Name = "Email Subject")]
        [Required]
        public string Subject { get; set; }

        [Display(Name = "Faculty")]
        [Required]
        public int FacultyId { get; set; }

        [Display(Name = "Email Body")]
        [Required]
        [DataType(DataType.MultilineText)]
        public string Body { get; set; }
    }
}