using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SwiftKampus.ViewModels.ConvocationVm
{
    public class AcademicGownLeaseVM
    {
        public string Id { get; set; } //PaymentItemId
        public string LeaseType { get; set; }
        public double GownHeight { get; set; }
        public int SchoolProgrammeId { get; set; }
        public string StudentId { get; set; }
        public string SchoolProhrammeName { get; set; }
        public int SessionId { get; set; }
    }
}