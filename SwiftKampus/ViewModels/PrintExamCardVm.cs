using SwiftKampusModel;
using SwiftKampusModel.Payment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SwiftKampus.ViewModels
{
    public class PrintExamCardVm
    {
        public Student Student { get; set; }
        //public SchoolFeePayment SchoolFeePayment { get; set; }
        public string RRR { get; set; }

        public ICollection<CourseRegistration> CourseRegistration { get; set; }
    }
}