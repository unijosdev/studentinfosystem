using SwiftKampusModel.Misconduct;

namespace SwiftKampus.ViewModels.MisconductVm
{
    public class RecallRequestViewModel
    {
        public string StudentId { get; set; }

        public int StudentDisciplinaryStatusId { get; set; }

        public ReasonForRequest ReasonForRequest { get; set; }

        public int DefaulterId { get; set; }

    }
}