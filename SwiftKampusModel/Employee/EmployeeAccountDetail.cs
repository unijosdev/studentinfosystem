namespace SwiftKampusModel.Employee
{
    public class EmployeeAccountDetail
    {
        public int EmployeeAccountDetailId { get; set; }
        public string AccountName { get; set; }
        public int AccountNumber { get; set; }
        public string BankName { get; set; }
        public string AccountType { get; set; }
        public bool IsPreffered { get; set; }
        public string StaffId { get; set; }
        public virtual Staff Staff { get; set; }
    }
}