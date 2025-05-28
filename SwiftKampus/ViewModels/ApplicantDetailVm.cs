using SwiftKampusModel;
using SwiftKampusModel.AddmissionApplicant;
using System.Collections.Generic;

namespace SwiftKampus.ViewModels
{
    public class ApplicantDetailVm
    {
        public Applicant Applicant { get; set; }
        public ApplicantPayment ApplicantPayment { get; set; }
        public List<ApplicantOLevelResult> ApplicantOLevelResults { get; set; }
        public List<NextOfKin> NextOfKins { get; set; }
        public List<EmploymentDetail> EmploymentDetails { get; set; }
        public List<AttendedSchool> AttendedSchools { get; set; }
        public List<Qualification> Qualifications { get; set; }
        public List<AwardPrice> AwardPrices { get; set; }
        public List<Referee> Referees { get; set; }
        public List<Publication> Publications { get; set; }
        public List<ThesisProposal> ThesisProposals { get; set; }
        public List<RelevantDocument> RelevantDocuments { get; set; }

        public string SubjetCombination { get; set; }
    }
}