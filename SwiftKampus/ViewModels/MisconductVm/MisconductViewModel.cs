using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SwiftKampus.ViewModels.MisconductVm
{
    public class MisconductViewModel
    {
        [Display(Name = "SN")]
        public int MisconductId { get; set; }

        [Display(Name = "Misconduct Name")]
        [Required(ErrorMessage = "Please Enter Misconduct")]
        public string MisconductName { get; set; }
    }
}