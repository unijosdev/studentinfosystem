using System.ComponentModel.DataAnnotations;

namespace SwiftKampusModel.StudentStatusManagement
{
    public enum ExtensionReason
    {
        [Display(Name = "Health Grounds")]
        Health_Grounds,
        [Display(Name = "Need More Time For Project")]
        Need_More_Time_For_Project
    }
}
