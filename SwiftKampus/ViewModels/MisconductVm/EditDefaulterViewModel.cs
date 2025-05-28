using SwiftKampusModel;
using SwiftKampusModel.Misconduct;
using System.Collections.Generic;
using System.Web;

namespace SwiftKampus.ViewModels.MisconductVm
{
    public class EditDefaulterViewModel
    {
        public int DefaulterId { get; set; }
        public string StudentId { get; set; }
        public int SemesterId { get; set; }
        public int SessionId { get; set; }
        public string FullName { get; set; }

        public string MatriculationNumber { get; set; }

        public string MisconductName { get; set; }

        public ICollection<Misconduct> Misconducts { get; set; }
        public ICollection<Session> Sessions { get; set; }
        public ICollection<Semester> Semesters { get; set; }

        public Defaulter Defaulter { get; set; }

        public int MisconductId { get; set; }

        public int DisciplinaryId { get; set; }

        public HttpPostedFileBase EvidenceFile { get; set; }
    }
}