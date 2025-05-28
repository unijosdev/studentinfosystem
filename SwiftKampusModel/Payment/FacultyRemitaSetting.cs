namespace SwiftKampusModel.Payment
{
    public class FacultyRemitaSetting
    {
        public int FacultyRemitaSettingId { get; set; }
        public int FacultyId { get; set; }
        public string ServiceType { get; set; }
        public string MerchantId { get; set; }
        public string ApiKey { get; set; }
        public virtual Faculty Faculty { get; set; }
    }
    public class DepartmentRemitaSetting
    {
        public int DepartmentRemitaSettingId { get; set; }
        public int DepartmentId { get; set; }
        public string ServiceType { get; set; }
        public string MerchantId { get; set; }
        public string ApiKey { get; set; }
        public virtual Department Department { get; set; }
    }
}
