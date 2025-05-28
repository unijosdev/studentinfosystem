using System;
using System.ComponentModel.DataAnnotations;

namespace SwiftKampusModel.Misconduct
{
    public class Defaulter
    {
        public int DefaulterId { get; set; }
        public string StudentId { get; set; }

        [Required(ErrorMessage = "Please Select a Misconduct")]
        public int MisconductId { get; set; }
        public int SessionId { get; set; }
        public int SemesterId { get; set; }    
        public DateTime? DefaultDate { get; set; }
        public string EvidenceAdress { get; set; }
        public int? StudentDisciplinaryStatusId { get; set; }
        public bool CaseTreated { get; set; }

        [Display(Name ="Notification")]
        public bool IsNotified { get; set; }

        public Student Student { get; set; }
        public Misconduct Misconduct { get; set; }
        public Semester Semester { get; set; }
        public Session Session { get; set; }
        public StudentDisciplinaryStatus StudentDisciplinaryStatus { get; set; }

    }
}
