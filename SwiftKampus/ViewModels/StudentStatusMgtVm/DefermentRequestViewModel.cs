using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using SwiftKampusModel.StudentStatusManagement;

namespace Unijos.Web.ViewModels.StudentStatus
{
    public class DefermentRequestViewModel
    {
        public int DefermentReabsorptionId { get; set; }
        public string StudentId { get; set; }
        [Display(Name = "Matriculation Number")]
        public string MatriculationNumber { get; set; }
        [Display(Name = "Fullname")]
        public string FullName { get; set; }
        [Display(Name = "Faculty Name")]
        public string FacultyName { get; set; }
        [Display(Name = "Department Name")]
        public string DepartmentName { get; set; }
        [Display(Name = "Programme Name")]
        public string DepartmentOptionName { get; set; }
        public string StartSessionId { get; set; }
        public string EndSessionId { get; set; }
        [Display(Name = "Reason For Deferment")]
        public string ReasonForDeferment { get; set; }
        [Display(Name = "Application Date")]
        public string ApplicationDateForDerferment { get; set; }
        [Display(Name = "HOD Comment")]
        public string HODCommentForDeferment { get; set; }
        [Display(Name = "HOD Comment Date")]
        public string HODCommentDateForDeferment { get; set; }
        [Display(Name = "Faculty Comment")]
        public string FacultyCommentForDeferment { get; set; }
        [Display(Name = "Faculty Comment Date")]
        public string FacultyCommentDateForDeferment { get; set; }
        [Display(Name = "Senate Comment")]
        public string SenateCommentForDeferment { get; set; }
        [Display(Name = "Senate Approval")]
        public bool? SenateApprovalForDeferment { get; set; }
        [Display(Name = "Senate Approval Date")]
        public string SenateApprovalDateForDeferment { get; set; }
        [Display(Name = "Reason For Extension Duration")]
        public string ReasonForExtensionDuration { get; set; }
        [Display(Name = "HOD Comment on Re-Absorption Request")]
        public string HODCommentForReabsorption { get; set; }
        public string HODCommentDateForReabsorption { get; set; }
        [Display(Name = "Faculty Comment on Re-Absorption Request")]
        public string FacultyCommentForReabsorption { get; set; }
        public string FacultyCommentDateForReabsorption { get; set; }
        [Display(Name = "Senate Comment on Re-Absorption Request")]
        public string SenateCommentForReabsorption { get; set; }
        [Display(Name = "Senate Approval on Re-Absorption")]
        public bool? SenateApprovalForReabsorption { get; set; }
        public string SenateApprovalDateForReabsorption { get; set; }
        public ICollection<DefermentDocument> DefermentDocuments { get; set; }
    }
}