using SwiftKampusModel.AddmissionApplicant;

namespace SwiftKampusModel.Payment
{
    public class PaymentSetting
    {
        public int PaymentSettingId { get; set; }
        public int SessionId { get; set; }
        public int SchoolProgrammeId { get; set; }
        public bool AcceptPartPayment { get; set; }
        public bool ConsiderIndigine { get; set; }
        public bool ConsiderNationality { get; set; }
        public bool ConsiderDepartmentalFee { get; set; }
        public double FirstPaymentPercentage { get; set; }
        public string SchoolFeeType { get; set; }
        public string StudentType { get; set; }
        public Session Session { get; set; }
        public SchoolProgramme SchoolProgramme { get; set; }
    }
}
