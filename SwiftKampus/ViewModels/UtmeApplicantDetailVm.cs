using SwiftKampusModel;
using SwiftKampusModel.AddmissionApplicant;
using System.Collections.Generic;

namespace SwiftKampus.ViewModels
{
    public class UtmeApplicantDetailVm
    {
        public UtmeApplicant UtmeApplicant { get; set; }
        public ApplicantPayment ApplicantPayment { get; set; }
        public ICollection<ApplicantOLevelResult> ApplicantOlevelResult { get; set; }
        public Qualification Qualification { get; set; }
    }
}