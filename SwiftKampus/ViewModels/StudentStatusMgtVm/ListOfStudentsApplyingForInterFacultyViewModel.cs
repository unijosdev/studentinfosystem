using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Unijos.Web.ViewModels.StudentStatus
{
    public class ListOfStudentsApplyingForInterFacultyViewModel
    {
        public int TransferId { get; set; }

        public int TransferProcessId { get; set; }

        [Display(Name = "Matriculation Number")]
        public string  MatriculationNumber { get; set; }

        [Display(Name = "Fullname")]
        public string FullName { get; set; }

        [Display(Name = "Faculty Name")]
        public string FacultyName { get; set; }

        [Display(Name = "Department Name")]
        public string DepartmentName { get; set; }

        [Display(Name = "Programme Name")]
        public string DepartmentOptionName { get; set; }

        [Display(Name = "Selected Faculty Name")]
        public string NewFacultyName { get; set; }

        [Display(Name = "Selected Department Name")]
        public string NewDepartmentName { get; set; }

        [Display(Name = "Selected Programme Name")]
        public string NewDepartmentOptionName { get; set; }
    }
}