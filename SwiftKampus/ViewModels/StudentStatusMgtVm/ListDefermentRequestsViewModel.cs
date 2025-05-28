using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SwiftKampusModel.StudentStatusManagement;

namespace Unijos.Web.ViewModels.StudentStatus
{
    public class ListDefermentRequestsViewModel
    {
        public int DefermentId { get; set; }
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
        public ICollection<DefermentDocument> DefermentDocuments { get; set; }
    }
}