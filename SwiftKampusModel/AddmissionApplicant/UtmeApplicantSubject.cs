using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwiftKampusModel.AddmissionApplicant
{
    public class UtmeApplicantSubject
    {
        public int UtmeApplicantSubjectId { get; set; }
        public string JambRegNo { get; set; }
        public string SubjectName { get; set; }
        public string Score { get; set; }
        public UtmeApplicant UtmeApplicant { get; set; }
    }
}
