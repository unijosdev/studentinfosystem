using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SwiftKampusModel.Misconduct
{
    public class Misconduct
    {
        public int MisconductId { get; set; }

        [Required(ErrorMessage = "Please Enter Misconduct")]
        [Display(Name = "Misconduct")]
        public string MisconductName { get; set; }
        public ICollection<Defaulter> Defaulters { get; set; }
    }
}
