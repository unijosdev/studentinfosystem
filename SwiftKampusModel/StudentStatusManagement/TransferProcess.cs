using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwiftKampusModel.StudentStatusManagement
{
    public class TransferProcess
    {
        [Key, ForeignKey("Transfer")]
        public int TransferId { get; set; }
        public string GandCRecommendation { get; set; }
        public DateTime? GandCRecommendationDate { get; set; }
        public bool? GandCActed { get; set; }
        public string HODComment { get; set; }
        public bool? HODActted { get; set; }
        public DateTime? HODCommentDate { get; set; }
        public bool? EntryRequirementsMet { get; set; }
        public int AcceptableLevelId { get; set; }
        public string NewHODComment { get; set; }
        public bool? NewHODApproval { get; set; }
        public DateTime? NewHODCommentDate { get; set; }
        public string NewFacultyComment { get; set; }
        public bool? NewFacultyApproval { get; set; }
        public DateTime? NewFacultyCommentDate { get; set; }
        public bool? SenateApproval { get; set; }
        public string SenateComment { get; set; }
        public DateTime? SenateApprovalDates { get; set; }
        public Transfer Transfer { get; set; }

    }
}
