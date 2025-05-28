using SwiftKampusModel;
using SwiftKampusModel.Misconduct;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace SwiftKampus.ViewModels.MisconductVm
{
    public class CreateDefaulterViewModel
    {
        public string StudentId { get; set; }
        [Required]
        public int SemesterId { get; set; }
        [Required]
        public int SessionId { get; set; }

        public string FullName { get; set; }

        public string MatriculationNumber { get; set; }

        public ICollection<Misconduct> Misconducts { get; set; }
        public ICollection<Session> Sessions { get; set; }
        public ICollection<Semester> Semesters { get; set; }

        public int DefaulterId { get; set; }

        public Defaulter Defaulter { get; set; }

        [Display(Name = "Misconduct")]
        [Required(ErrorMessage = "Please Select a Misconduct")]
        public int MisconductId { get; set; }

        public int DisciplinaryId { get; set; }

        public HttpPostedFileBase EvidenceFile { get; set; }
    }
}