using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SwiftKampus.ViewModels.ConvocationVm
{
    public class ConvocationListVm
    {
        [Display(Name = "Matriculation NO")]
        public string Matric { get; set; }

        public string studentId { get; set; }

        [Display(Name = "Fullname")]
        public string Fullname { get; set; }

        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Eamil")]
        public string Email { get; set; }

        [Display(Name = "Programme Studied")]
        public string ProgrammeOfStudy { get; set; }

        [Display(Name = "Department")]
        public string Department { get; set; }

        [Display(Name = "Faculty")]
        public string Faculty { get; set; }

        [Display(Name = "Class of Degree")]
        public string ClassOfDegree { get; set; }

        [Display(Name = "Hope to Attending")]
        public bool WillAttend { get; set; }

        [Display(Name = "Lease Gown")]
        public bool LeaseGown { get; set; }
        public bool HasCollected { get; set; }
        public bool HassReturned { get; set; }
    }
}