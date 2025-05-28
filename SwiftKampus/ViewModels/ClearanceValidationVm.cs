using SwiftKampus.Models;
using SwiftKampusModel;
using System.Linq;

namespace SwiftKampus.ViewModels
{
    public class ClearanceValidationVm
    {
        private readonly SchoolDbContext _db;
        public ClearanceValidationVm(SchoolDbContext db, string userId, string schoolProgrammeCode, string modeOfEntry)
        {

            _db = db;
            BioData = CheckBioData(userId);
            OLevelResult = CheckOLevelResult(userId);
            NextOfKin = CheckNextOfKin(userId);
            Address = CheckAddress(userId);
            Sponsors = CheckReferees(userId);

            if (!string.IsNullOrEmpty(modeOfEntry) && modeOfEntry.ToUpper().Equals(ModeOfEntry.DE.ToString()))
            {
                DirectEntryExam = CheckDirectEntrytExam(userId);
            }
            else if (schoolProgrammeCode.Equals(ProgrammeCategory.Masters.ToString())
                    || schoolProgrammeCode.Equals(ProgrammeCategory.Post_Graduate.ToString())
                    || schoolProgrammeCode.Equals(ProgrammeCategory.MBA.ToString())
                    || schoolProgrammeCode.Equals(ProgrammeCategory.Phd.ToString()))
            {
                EmploymentHistory = CheckEmploymentHistory(userId);
                AcademicQualification = CheckAcademicQualification(userId);
                Award = CheckAward(userId);
                RelevanQualification = CheckRelevantQualification(userId);
                Publication = CheckPublication(userId);
                Referee = CheckReferees(userId);
                UploadDocument = CheckUploadDocument(userId);
            }

            if (schoolProgrammeCode.Equals(ProgrammeCategory.Phd.ToString()))
            {              
                Thesis = CheckThesis(userId);
            }           
           
        }


        public bool BioData { get; set; } = false;
        public bool OLevelResult { get; set; } = false;
        public bool NextOfKin { get; set; } = false;
        public bool Address { get; set; } = false;
        public bool Sponsors { get; set; } = false;
        public bool EmploymentHistory { get; set; } = false;
        public bool AcademicQualification { get; set; } = false;
        public bool Award { get; set; } = false;
        public bool RelevanQualification { get; set; } = false;
        public bool Publication { get; set; } = false;
        public bool Referee { get; set; } = false;
        public bool Thesis { get; set; } = false;
        public bool UploadDocument { get; set; } = false;
        public bool DirectEntryExam { get; set; } = false;

        private bool CheckBioData(string userId)
        {
            return _db.Students.Any(x => x.Email.Equals(userId) && x.Passport != null && x.Signature != null);
        }
        private bool CheckOLevelResult(string userId)
        {
            return _db.ApplicantOLevelResults.Any(x => x.ApplicantId.Equals(userId));
        }
        private bool CheckNextOfKin(string userId)
        {
            return _db.NextOfKins.Any(x => x.UserId.Equals(userId));
        }
        private bool CheckAddress(string userId)
        {
            return _db.Addresses.Any(x => x.UserId.Trim().ToUpper().Equals(userId.Trim().ToUpper()));
        }
        private bool CheckEmploymentHistory(string userId)
        {
            return _db.EmploymentDetails.Any(x => x.ApplicantId.Equals(userId));
        }
        private bool CheckAcademicQualification(string userId)
        {
            return _db.AttendedSchools.Any(x => x.ApplicantId.Equals(userId));
        }

        private bool CheckAward(string userId)
        {
            return _db.AwardPrices.Any(x => x.ApplicantId.Equals(userId));
        }
        private bool CheckRelevantQualification(string userId)
        {
            return _db.Qualifications.Any(x => x.UserId.Equals(userId));
        }
        private bool CheckPublication(string userId)
        {
            return _db.Publications.Any(x => x.UserId.Equals(userId));
        }
        private bool CheckReferees(string userId)
        {
            return _db.Referees.Any(x => x.UserId.Equals(userId));
        }
        private bool CheckThesis(string userId)
        {
            return _db.ThesisProposals.Any(x => x.UserId.Equals(userId));
        }
        private bool CheckUploadDocument(string userId)
        {
            return _db.RelevantDocuments.Any(x => x.UserId.Equals(userId));
        }

        private bool CheckDirectEntrytExam(string userId)
        {
            return _db.DirectEntryExams.Any(x => x.UserId.Equals(userId));
        }
    }
}