using System.ComponentModel.DataAnnotations;

namespace SwiftKampus.ViewModels
{
    public class RejectStudentVm
    {
        public string StudentId { get; set; }

        [Required]
        [DataType(DataType.MultilineText)]
        public string ReasonForReject { get; set; }

        [Required]
        public string LevelOfReject { get; set; }
    }

    public class RejectAdmissionVm
    {
        public string applicantId { get; set; }

        [Required]
        [DataType(DataType.MultilineText)]
        public string ReasonForReject { get; set; }

        [Required]
        public string LevelOfReject { get; set; }
    }

    public class RejectCourseRegVm
    {
        public int CourseRegistrationId { get; set; }

        [Required]
        [DataType(DataType.MultilineText)]
        public string ReasonForReject { get; set; }
    }
}