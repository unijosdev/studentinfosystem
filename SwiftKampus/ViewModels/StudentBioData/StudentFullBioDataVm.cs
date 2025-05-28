using SwiftKampus.Models;
using SwiftKampusModel;
using SwiftKampusModel.AddmissionApplicant;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;


namespace SwiftKampus.ViewModels.StudentBioData
{
    public class StudentFullBioDataVm
    {
        private readonly SchoolDbContext _db;
        public StudentFullBioDataVm(SchoolDbContext db, Student student)
        {           
            _db = db;
            //var userId = student.Email;
            string userId;
            if (student.Email.Contains("@unijos.edu.ng") && (student.SessionId != 1 && student.SessionId < 20)) //This fectches the biodata of old students (2017/2018 and below)
            {
                userId = student.PrimaryEmail;
                if (userId == null)
                {
                    userId = student.Email;
                }
            }
            else if (student.PrimaryEmail == null)
            {
                userId = student.Email;
            }
            else
            {
                userId = student.Email;
            }


            //var userId = student.Email == null ? student.Email : student.PrimaryEmail; //added to cater for students when they have gotten mat number and email switched
            Student =student;
            var schoolProgrammeCode = Student.SchoolProgramme.ProgrammeCategory;
            OLevelResult = CheckOLevelResult(userId);
            NextOfKin = CheckNextOfKin(userId);
            Address = CheckAddress(userId);

            if (schoolProgrammeCode.Equals(ProgrammeCategory.UnderGraduate.ToString()) || schoolProgrammeCode.Equals(ProgrammeCategory.Institute_Of_Education.ToString()))
            {
                Sponsors = CheckReferees(userId);
                if (!string.IsNullOrEmpty(student.ModeOfEntry) && student.ModeOfEntry.Trim().ToUpper().Equals(ModeOfEntry.DE.ToString()))
                {
                    DeExam = CheckDirectEntrytExam(userId);
                }
                UploadDocument = CheckUploadDocument(userId);
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
        public Student Student { get; set; }
        public DirectEntryExam DeExam { get; set; }
        public List<ApplicantOLevelResult> OLevelResult { get; set; }
        public NextOfKin NextOfKin { get; set; }
        public Address Address { get; set; }
        public List<Referee> Sponsors { get; set; }
        public List<EmploymentDetail> EmploymentHistory { get; set; }
        public List<AttendedSchool> AcademicQualification { get; set; }
        public List<AwardPrice> Award { get; set; }
        public List<Qualification> RelevanQualification { get; set; }
        public List<Publication> Publication { get; set; }
        public List<Referee> Referee { get; set; }
        public List<ThesisProposal> Thesis { get; set; }
        public List<RelevantDocument> UploadDocument { get; set; }


    
        private List<ApplicantOLevelResult> CheckOLevelResult(string userId)
        {
            return _db.ApplicantOLevelResults.Include(i => i.Subject).Where(x => x.ApplicantId.Trim().ToUpper().Equals(userId.Trim().ToUpper())).ToList();
        }
        private NextOfKin CheckNextOfKin(string userId) 
        {
            return _db.NextOfKins.Where(x => x.UserId.Trim().ToUpper().Equals(userId.Trim().ToUpper())).FirstOrDefault();
        }
        private Address CheckAddress(string userId)
        {
            return _db.Addresses.FirstOrDefault(x => x.UserId.Trim().ToUpper().Equals(userId.Trim().ToUpper()));
        }
        private List<EmploymentDetail> CheckEmploymentHistory(string userId)
        {
            return _db.EmploymentDetails.Where(x => x.ApplicantId.Trim().ToUpper().Equals(userId.Trim().ToUpper())).ToList();
        }
        private List<AttendedSchool> CheckAcademicQualification(string userId)
        {
            return _db.AttendedSchools.Where(x => x.ApplicantId.Trim().ToUpper().Equals(userId.Trim().ToUpper())).ToList();
        }

        private List<AwardPrice> CheckAward(string userId)
        {
            return _db.AwardPrices.Where(x => x.ApplicantId.Trim().ToUpper().Equals(userId.Trim().ToUpper())).ToList();
        }
        private List<Qualification> CheckRelevantQualification(string userId)
        {
            return _db.Qualifications.Where(x => x.UserId.Equals(userId)).ToList();
        }
        private List<Publication> CheckPublication(string userId)
        {
            return _db.Publications.Where(x => x.UserId.Trim().ToUpper().Equals(userId.Trim().ToUpper())).ToList();
        }
        private List<Referee> CheckReferees(string userId)
        {
            return _db.Referees.Where(x => x.UserId.Trim().ToUpper().Equals(userId.Trim().ToUpper())).ToList();
        }
        private List<ThesisProposal> CheckThesis(string userId)
        {
            return _db.ThesisProposals.Where(x => x.UserId.Trim().ToUpper().Equals(userId.Trim().ToUpper())).ToList();
        }
        private List<RelevantDocument> CheckUploadDocument(string userId)
        {
            return _db.RelevantDocuments.Where(x => x.UserId.Trim().ToUpper().Equals(userId.Trim().ToUpper())).ToList();
        }
        private DirectEntryExam CheckDirectEntrytExam(string userId)
        {
            return _db.DirectEntryExams.FirstOrDefault(x => x.UserId.Equals(userId));
        }
    }
}