using SwiftKampusModel;

namespace Unijos.Web.ViewModels.StudentStatus
{
    public class InterFacultyTransferApplicationViewModel
    {
        public int TransferId { get; set; }

        public Student Student { get; set; }

        public int SessionId { get; set; }

        public int ProgrammeId { get; set; }
        public Programme Programme { get; set; }

        public string ApplicationDate { get; set; }

        public string SessionName { get; set; }

        public string GandCRecommendation { get; set; }

        public string GandCRecommendationDate { get; set; }

        public string HODComment { get; set; }

        public string HODCommentDate { get; set; }

        public bool? EntryRequirementsMet { get; set; }

        public int AcceptableLevelId { get; set; }

        public string NewHODComment { get; set; }

        public string NewHODCommentDate { get; set; }

        public string NewFacultyComment { get; set; }

        public string NewFacultyCommentDate { get; set; }

        public bool? SenateApproval { get; set; }

        public string SenateApprovalDates { get; set; }
    }
}