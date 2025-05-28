using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwiftKampusModel.StudentStatusManagement
{
    public class Reabsorption
    {
        [Key, ForeignKey("Deferment")]
        public int DefermentId { get; set; }
       
        public DateTime? ApplicationDateForReabsorption { get; set; }
        public string HODCommentForReabsorption { get; set; }
        public DateTime? HODCommentDateForReabsorption { get; set; }
        public bool? HODApprovalForReabsorption { get; set; }
        public string FacultyCommentForReabsorption { get; set; }
        public DateTime? FacultyCommentDateForReabsorption { get; set; }
        public bool? FacultyApprovalForReabsorption { get; set; }
        public string SenateCommentForReabsorption { get; set; }
        public bool? SenateApprovalForReabsorption { get; set; }
        public DateTime? SenateApprovalDateForReabsorption { get; set; }
        public bool IsApplicationPending { get; set; }
        public Deferment Deferment { get; set; }
     
    }
}
