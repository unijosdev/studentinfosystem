namespace SwiftKampusModel.Payment
{
    public class IdCardPaymentSetting
    {
        public int IdCardPaymentSettingId { get; set; }
        public int SessionId { get; set; }
        public decimal Amount { get; set; }
        public string AmountInWords { get; set; }
        public string StudentType { get; set; }
        public Session Session { get; set; }
    }
}
