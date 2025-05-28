namespace SwiftKampusModel.Payment
{
    public class StudentPaymentDetail
    {
        public int StudentPaymentDetailId { get; set; }
        public int SessionId { get; set; }
        public string StudentId { get; set; }
        public string FeeTypeName { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public Student Student { get; set; }
        public Session Session { get; set; }
    }
}
