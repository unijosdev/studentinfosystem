using SwiftKampusModel.AddmissionApplicant;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SwiftKampusModel.StudentStatusManagement
{
    public class Deferment
    {
        [Key]
        public int DefermentId { get; set; }
        public string StudentId { get; set; }
        public string StartSessionId { get; set; }
        public string EndSessionId { get; set; }
        public string ReasonForDeferment { get; set; }
        public DateTime? ApplicationDateForDerferment { get; set; }
        public string HODCommentForDeferment { get; set; }
        public DateTime? HODCommentDateForDeferment { get; set; }
        public bool? HODApprovalForDeferment { get; set; }
        public string FacultyCommentForDeferment { get; set; }
        public DateTime? FacultyCommentDateForDeferment { get; set; }
        public bool? FacultyApprovalForDerferment { get; set; }
        public string SenateCommentForDeferment { get; set; }
        public bool? SenateApprovalForDeferment { get; set; }
        public DateTime? SenateApprovalDateForDeferment { get; set; }
        public bool IsApplicationPending { get; set; }
        public int ChangeOfCoursePaymentId { get; set; }

        public Student Student { get; set; }
        public Session Session { get; set; }
        public Reabsorption Reabsorption { get; set; }
        public ChangeOfCoursePayment ChangeOfCoursePayment { get; set; }
        public ICollection<DefermentDocument> DefermentDocuments { get; set; }
    }
}
