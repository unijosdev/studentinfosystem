using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SwiftKampus.ViewModels.StudentBioData
{
    public class DirectEntryExamVm
    {
        public int DirectEntryExamId { get; set; }
        public string UserId { get; set; }

        [Display(Name = "Certificate Type")]
        [Required]
        public string CertificateType { get; set; }

        [Display(Name = "Name of Institution")]
        [Required]
        public string NameOfInstitution { get; set; }

        [Display(Name = "Exam Number")]
        public string ExamNumber { get; set; }

        [Display(Name = "Overall Grade")]
        public string OverallGrade { get; set; }


        [Display(Name = "Exam Date")]
        [DataType(DataType.Date)]
        [Required]
        public DateTime ExamDate { get; set; }
    }
}