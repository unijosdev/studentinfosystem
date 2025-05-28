using System;
using System.ComponentModel.DataAnnotations;

namespace SwiftKampus.ViewModels.MisconductVm
{
    public class DefaulterMisconductViewModel
    {
        public int DefaulterId { get; set; }

        [Display(Name = "Misconduct")]
        public string MisconductName { get; set; }

        [Display(Name = "Incident Date")]
        public DateTime? IncidentDate { get; set; }

        public string FacultyName { get; set; }
               
        public bool Status { get; set; } //Addressed or not Addressed
    }
}