using SwiftKampusModel.AddmissionApplicant;

namespace SwiftKampusModel.Payment
{
    public class ChangeOfCourseFee
    {
        public int ChangeOfCourseFeeId { get; set; }

        public int SchoolProgrammeId { get; set; }

        public int SessionId { get; set; }

        public decimal ApplicationFee { get; set; }

        public string AmountInWords { get; set; }
        public string ChangeOfCourseType { get; set; }

        public Session Session { get; set; }
        public SchoolProgramme SchoolProgramme { get; set; }
    }
}
