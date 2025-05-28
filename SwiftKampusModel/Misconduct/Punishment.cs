using System.ComponentModel.DataAnnotations;

namespace SwiftKampusModel.Misconduct
{
    /*
     Any alteration on this enum should result to a corresponding change in StudentDisciplinaryContoller on the create Action
     */
    public enum Punishment
    {
        Select,
        Expulsion,
        Suspension,
        [Display(Name = "One Session Rustication")]
        One_Session_Rustication,
        [Display(Name = "Two Sessions Rustication")]
        Two_Sessions_Rustication,
        [Display(Name = "Pay for vandalized Property")]
        Pay_for_vandalized_Property,
        Others
    }
}
