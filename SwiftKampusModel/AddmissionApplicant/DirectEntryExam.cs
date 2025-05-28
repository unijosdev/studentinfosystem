using System;
using System.ComponentModel.DataAnnotations;

namespace SwiftKampusModel.AddmissionApplicant
{
    public class DirectEntryExam
    {
        public int DirectEntryExamId { get; set; }
        public string UserId { get; set; }

        [Display(Name = "Certificate Type")]
        public string CertificateType { get; set; }

        [Display(Name = "Name of Institution")]
        public string NameOfInstitution { get; set; }

        [Display(Name = "Exam Number")]
        public string ExamNumber { get; set; }

        [Display(Name = "Overall Grade")]
        public string OverallGrade { get; set; }


        [Display(Name = "Exam Date")]
        [DataType(DataType.Date)]
        public DateTime ExamDate { get; set; }
    }
}
