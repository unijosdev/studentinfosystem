namespace SwiftKampusModel.AddmissionApplicant
{
    public class ApplicantFeeSetting
    {
        public int ApplicantFeeSettingId { get; set; }

        public int SchoolProgrammeId { get; set; }

        public int SessionId { get; set; }

        public decimal ApplicationFee { get; set; }

        public string AmountInWords { get; set; }

        public bool IsEmailEnabled { get; set; }
        public bool IsSmsEnabled { get; set; }
        public int AllocatedSms { get; set; }
        public int AllocatedEmail { get; set; }

        public bool MakeActive { get; set; }

        public virtual Session Session { get; set; }
        public virtual SchoolProgramme SchoolProgramme { get; set; }
    }
}
