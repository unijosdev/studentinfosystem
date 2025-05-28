using SwiftKampusModel.Misconduct;
using System;

namespace SwiftKampus.ViewModels.MisconductVm
{
    public class RecallRequestListViewModel
    {
        public string StudentId { get; set; }

        public String MatriculationNumber { get; set; }

        public string FullName { get; set; }

        public int RecallRequestId { get; set; }

        public int DefaulterId { get; set; }

        public DateTime SuspensionDate { get; set; }

        public DateTime ApplicationDate { get; set; }

        public bool ApplicationTreated { get; set; }

        public string DisciplineName { get; set; }

        public string ReasonForRequest { get; set; }

        public int RecallSession { get; set; }

        // public StudentRecallRequest StudentRecallRequest { get; set; }

        public StudentDisciplinaryStatus StudentDisciplinaryStatus { get; set; }
    }
}