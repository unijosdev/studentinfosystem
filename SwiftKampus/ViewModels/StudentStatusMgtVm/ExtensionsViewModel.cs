using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Unijos.Web.ViewModels.StudentStatus
{
    public class ExtensionsViewModel
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
    }
}