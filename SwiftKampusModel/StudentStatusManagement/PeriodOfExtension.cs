using System.ComponentModel.DataAnnotations;

namespace SwiftKampusModel.StudentStatusManagement
{
    public enum PeriodOfExtension
    {
        [Display(Name = "One Session")]
        One_Session,
        [Display(Name = "Two Sessions")]
        Two_Sessions
    }
}
