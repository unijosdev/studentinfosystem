using SwiftKampusModel.StudentStatusManagement;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Unijos.Web.ViewModels.StudentStatus
{
    public class ExtensionRequestViewModel
    {
        public int ExtensionId { get; set; }

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
        public string ProgrammeName { get; set; }

        public int SessionId { get; set; }

        public string SessionName { get; set; }

        [Display(Name = "Period of extension sought")]
        public string PeriodOfExtensionSought { get; set; }

        [Display(Name = "Reason For Extension")]
        public string ReasonForExtension { get; set; }

        [Display(Name = "Application Date")]
        public string ApplicationDate { get; set; }

        [Display(Name = "HOD Comment")]
        public string HODComment { get; set; }

        [Display(Name = "HOD Comment Date")]
        public string HODCommentDate { get; set; }

        [Display(Name = "Faculty Comment")]
        public string FacultyComment { get; set; }

        [Display(Name = "Faculty Comment Date")]
        public string FacultyCommentDate { get; set; }

        [Display(Name = "Senate Comment")]
        public string SenateComment { get; set; }

        [Display(Name = "Senate Approval")]
        public bool SenateApproval { get; set; }

        [Display(Name = "Senate Approval Date")]
        public string SenateApprovalDate { get; set; }

        public ICollection<ExtensionDocument> ExtensionDocuments { get; set; }
    }
}