namespace SwiftKampus.ViewModels
{
    public class BaseVm
    {
        public bool HasPayedSchoolFee { get; set; }
        public bool HasPayedAcceptanceFee { get; set; }
        public bool HasPayedSupplementaryFee { get; set; }
        public bool HasPayedApplicationFee { get; set; }
        public bool HasGraduated { get; set; }
        public string FullName { get; set; }
        public string SchoolProgramme { get; set; }
        public string ProgrammeType { get; set; }
        public string BloodGroup { get; set; }
    }
}