namespace SwiftKampus.ViewModels
{
    public class StaffVM
    {
        public string StudentInClass { get; set; }
        public int TotalNumberOfStudent { get; set; }
        public int TotalNumberOfMale { get; set; }
        public int TotalNumberOfFemale { get; set; }
    }

    public class StaffRoleVm
    {
        public string StaffId { get; set; }
        public string[] StaffRoleName { get; set; }
    }
}