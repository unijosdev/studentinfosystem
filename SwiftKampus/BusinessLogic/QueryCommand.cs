using Microsoft.AspNet.Identity;
using SwiftKampus.Abstractions;
using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampus.ViewModels;
using SwiftKampusModel;
using SwiftKampusModel.AddmissionApplicant;
using SwiftKampusModel.Payment;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;

namespace SwiftKampus.BusinessLogic
{
    public class QueryCommand : IQueryCommand
    {
        private readonly SchoolDbContext _db;
        public AzureDataLayer DbLayer;

        public QueryCommand(SchoolDbContext db)
        {
            _db = db;
            DbLayer = new AzureDataLayer();
        }

        public int GetCurrentSessionId(int schoolProgrammeId)
        {
            return _db.AssignSessionToSchools.Include(i => i.Session).AsNoTracking()
                        .Where(x => x.ActiveSession.Equals(true)
                        && x.SchoolProgrammeId.Equals(schoolProgrammeId))
                        .Select(s => s.Session.SessionId).FirstOrDefault();
        }

        public int GetCurrentProgrammeSessionId(int ProgrammeId)
        {
            return _db.AssignSessionToProgrammes.Include(i => i.Session).AsNoTracking()
                        .Where(x => x.ActiveSession.Equals(true)
                        && x.ProgrammeId.Equals(ProgrammeId))
                        .Select(s => s.Session.SessionId).FirstOrDefault();
        }
        public string GetCurrentSessionName(int schoolProgrammeId)     
        {
            return _db.AssignSessionToSchools.Include(i => i.Session).AsNoTracking()
                        .Where(x => x.ActiveSession.Equals(true)
                        && x.SchoolProgrammeId.Equals(schoolProgrammeId))
                        .Select(s => s.Session.SessionName).FirstOrDefault();
        }

        public string GetSessionName(int sessionId)
        {
            return _db.Sessions.AsNoTracking()
                        .Where(x =>  x.SessionId.Equals(sessionId))
                        .Select(s => s.SessionName).FirstOrDefault();
        }

        public string GetCurrentSemesterName(int schoolProgrammeId)
        {
            return _db.AssignSemesterToSchools.Include(i => i.Semester).AsNoTracking()
                    .Where(x => x.ActiveSemester.Equals(true)
                    && x.SchoolProgrammeId.Equals(schoolProgrammeId))
                    .Select(s => s.Semester.SemesterName).FirstOrDefault();
        }

        public int GetCurrentSemesterId(int schoolProgrammeId)
        {
            return _db.AssignSemesterToSchools.Include(i => i.Semester).AsNoTracking()
                    .Where(x => x.ActiveSemester.Equals(true)
                    && x.SchoolProgrammeId.Equals(schoolProgrammeId))
                    .Select(s => s.SemesterId).FirstOrDefault();
        }
        public Session GetCurrentSession(int schoolProgrammeId)
        {
            return _db.AssignSessionToSchools.Include(i => i.Session).AsNoTracking()
                    .Where(x => x.ActiveSession.Equals(true)
                    && x.SchoolProgrammeId.Equals(schoolProgrammeId))
                    .Select(s => s.Session)
                    .FirstOrDefault();
        }

        public Semester GetCurrentSemester(int schoolProgrammeId)
        {
            return _db.AssignSemesterToSchools.Include(i => i.Semester).AsNoTracking()
                .Where(x => x.ActiveSemester.Equals(true)
                && x.SchoolProgrammeId.Equals(schoolProgrammeId))
                .Select(s => s.Semester)
                .FirstOrDefault();
        }

        public List<Session> GetCurrentSessionList(int schoolProgrammeId)
        {
            return _db.AssignSessionToSchools.Include(i => i.Session).AsNoTracking()
                    .Where(x => x.ActiveSession.Equals(true)
                    && x.SchoolProgrammeId.Equals(schoolProgrammeId))
                    .Select(s => s.Session)
                    .OrderByDescending(s => s.SessionName).ToList();
        }

        public List<Semester> GetCurrentSemesterList(int schoolProgrammeId)
        {
            return _db.AssignSemesterToSchools.Include(i => i.Semester).AsNoTracking()
                .Where(x => x.ActiveSemester.Equals(true)
                && x.SchoolProgrammeId.Equals(schoolProgrammeId))
                .Select(s => s.Semester).OrderByDescending(o => o.SemesterName)
                .ToList();
        }

        //public bool CheckForFirstSemester(string num)
        //{
        //    var isExist = _db.Semesters.AsNoTracking().Where(x => x.ActiveSemester.Equals(true)
        //               && x.SemesterName.ToUpper().Equals("FIRST")).FirstOrDefault();
        //    if (isExist != null)
        //    {
        //        return true;
        //    }
        //    return false;
        //}
        public int GetLevelByName(string levelName)
        {
            levelName = levelName.Trim().ToUpper();
            return _db.Levels.AsNoTracking().Where(x => x.LevelName.ToUpper().Trim().Equals(levelName))
                        .Select(s => s.LevelId).FirstOrDefault();
        }

        public Level GetLevelById(int levelId)
        {
            return _db.Levels.Find(levelId);
        }

        public SchoolProgramme GetSchoolProgById(int levelId)
        {
            var schprog = _db.Levels.Include(l => l.SchoolProgramme).Where(x => x.LevelId.Equals(levelId)).FirstOrDefault();
            return _db.SchoolProgrammes.Where(p => p.SchoolProgrammeId.Equals(schprog.SchoolProgramme.SchoolProgrammeId)).FirstOrDefault();
        }

        public string GetId()
        {
            return HttpContext.Current.User.Identity.GetUserName();
        }

        public int ConvertToKobo(int value)
        {
            return value * 100;
        }

        public int ConvertToNaira(int value)
        {
            return value / 100;
        }

        public string HashRemitaRequest(string merchantId, string serviceTypeId, string orderId, string amount, string responseUrl, string apiKey)
        {
            string hash_string = merchantId + serviceTypeId + orderId + amount + responseUrl + apiKey;
            System.Security.Cryptography.SHA512Managed sha512 = new System.Security.Cryptography.SHA512Managed();
            Byte[] EncryptedSHA512 = sha512.ComputeHash(System.Text.Encoding.UTF8.GetBytes(hash_string));
            sha512.Clear();
            return BitConverter.ToString(EncryptedSHA512).Replace("-", "").ToLower();
        }

        public string HashRemitedValidate(string orderID, string apiKey, string merchantId)
        {
            string hash_string = orderID + apiKey + merchantId;
            System.Security.Cryptography.SHA512Managed sha512 = new System.Security.Cryptography.SHA512Managed();
            Byte[] EncryptedSHA512 = sha512.ComputeHash(System.Text.Encoding.UTF8.GetBytes(hash_string));
            sha512.Clear();
            return BitConverter.ToString(EncryptedSHA512).Replace("-", "").ToLower();
        }

        public string HashRemitedRePost(string merchantId, string rrr, string apiKey)
        {
            string hash_string = merchantId + rrr + apiKey;
            System.Security.Cryptography.SHA512Managed sha512 = new System.Security.Cryptography.SHA512Managed();
            Byte[] EncryptedSHA512 = sha512.ComputeHash(System.Text.Encoding.UTF8.GetBytes(hash_string));
            sha512.Clear();
            return BitConverter.ToString(EncryptedSHA512).Replace("-", "").ToLower();
        }

        public string HashRrrQuery(string rrr, string apiKey, string merchantId)
        {
            string hash_string = rrr + apiKey + merchantId;
            System.Security.Cryptography.SHA512Managed sha512 = new System.Security.Cryptography.SHA512Managed();
            Byte[] EncryptedSHA512 = sha512.ComputeHash(System.Text.Encoding.UTF8.GetBytes(hash_string));
            sha512.Clear();
            return BitConverter.ToString(EncryptedSHA512).Replace("-", "").ToLower();
        }

        public async Task<double> GetAdmissionGradePoint(string grade)
        {
            return await _db.AdmissionGrades.AsNoTracking()
                                    .Where(x => x.GradeName.Equals(grade))
                                    .Select(s => s.GradePoint).FirstOrDefaultAsync();
        }

        public BaseVm GetPaymentStatus(int sessionId)
        {
            var model = new BaseVm();
            var userId = GetId();

            var checkStudent = _db.Students.Include(i => i.SchoolProgramme).AsNoTracking()
                                .FirstOrDefault(x => x.Email.Trim().ToUpper().Equals(userId.ToUpper().Trim())
                                && x.Active.Equals(true) && x.IsGraduated.Equals(false));

            if (checkStudent != null && HttpContext.Current.User.IsInRole(RoleName.Student))
            {
                if (checkStudent.IsRemedialStudent != null && checkStudent.IsRemedialStudent.Equals(true))
                {
                    var applicatPayment = _db.ApplicantPayments.AsNoTracking()
                                      .Where(x => x.ApplicantEmail.Equals(checkStudent.Email))
                                      .FirstOrDefault();
                    //applicatPayment.IsPayed = true;  //used to update students from rems who sees "pay sceening charges"
                    //applicatPayment.ApplicantEmail = "2018NS1478@unijos.edu.ng";
                    //_db.Entry(applicatPayment).State = EntityState.Modified;
                    //_db.SaveChanges();

                    model.HasPayedApplicationFee = applicatPayment != null && applicatPayment.IsPayed || checkStudent.SessionId == 20 || checkStudent.SessionId == 1 || checkStudent.SessionId == 24; //Added (|| checkStudent.SessionId == 20) part for 2019 rems students
                }
                else
                {
                    model.HasPayedApplicationFee = true;
                }
                var acceptPartPayment = _db.PaymentSettings.AsNoTracking().Where(x => x.SessionId.Equals(sessionId)
                                       && x.SchoolProgrammeId.Equals(checkStudent.SchoolProgramme.SchoolProgrammeId)
                                       && x.StudentType.ToUpper().Equals(checkStudent.StudentStatus.ToUpper()))
                                    .Select(s => s.AcceptPartPayment).FirstOrDefault();

                model.FullName = $"{checkStudent.LastName} {checkStudent.FirstName}";
                model.ProgrammeType = checkStudent.SchoolProgramme.ProgrammeType;
                model.SchoolProgramme = checkStudent.SchoolProgramme.ProgrammeCategory;

                model.BloodGroup = checkStudent.BloodGroup;

                var programmeHasActiveSession =  _db.AssignSessionToProgrammes.Where(a => a.SessionId.Equals(sessionId)).FirstOrDefault();
                var schoolfee = new List<SchoolFeePayment>();

                if (programmeHasActiveSession != null)
                {
                     schoolfee = _db.SchoolFeePayments.AsNoTracking()
                          .Where(x => x.StudentId.Equals(checkStudent.StudentId)
                          && x.SessionId.Equals(sessionId) && x.Status.Equals(true)).ToList();
                }
                else
                {
                    schoolfee = _db.SchoolFeePayments.AsNoTracking()
                      .Where(x => x.StudentId.Equals(checkStudent.StudentId)
                      && x.SessionId.Equals(sessionId) && x.Status.Equals(true)).ToList();
                }
                    
                if (schoolfee.Count() > 0)
                {
                    if (schoolfee.Any(x => x.FeeCategory.Equals(SchoolFeeCategory.Acceptance.ToString())))
                    {
                        model.HasPayedAcceptanceFee = true;
                    }                    
                    if (acceptPartPayment.Equals(false))
                    {
                        if (schoolfee.Any(x => x.FeeCategory.Equals(SchoolFeeCategory.School_Charges.ToString())))
                        {
                            model.HasPayedSchoolFee = true;
                        }
                        model.HasPayedAcceptanceFee = true;
                    }
                    else
                    {
                        if (schoolfee.Any(x => x.FeeCategory.Equals(SchoolFeeCategory.School_Charges.ToString())))
                        {
                            model.HasPayedSchoolFee = true;
                        }
                    }                    
                }
                if (checkStudent.StudentStatus.Equals(StudentStatus.Returning.ToString()))
                {
                    model.HasPayedAcceptanceFee = true;
                }

                if (checkStudent.IsStaff || (checkStudent.IsSchoolarshipStudent != null && checkStudent.IsSchoolarshipStudent.Equals(true)))
                {
                    model.HasPayedSchoolFee = true;
                    model.HasPayedAcceptanceFee = true;
                }

                if (checkStudent.StudentStatus.ToUpper().Equals(StudentStatus.New_Student.ToString().ToUpper()))
                {
                    //First condition(commented) for policy which requires students to pay school charges before biodata when School payment setup set partpayemnt = TRUE
                    //Modified by CIS team to allow students to  do biodata before payment

                    //if (model.HasPayedSchoolFee.Equals(true) && acceptPartPayment.Equals(false)) 
                    if ( acceptPartPayment.Equals(false))
                    {
                        model.HasPayedAcceptanceFee = true;
                    }
                    else
                    {
                        var acceptancefee = _db.SchoolFeePayments.AsNoTracking().FirstOrDefault(
                                         x => x.StudentId.Equals(checkStudent.StudentId)
                                          && x.SessionId.Equals(sessionId) && x.Status.Equals(true)
                                          && x.FeeCategory.Equals(SchoolFeeCategory.Acceptance.ToString()));

                        model.HasPayedAcceptanceFee = acceptancefee != null && acceptancefee.Status;
                    }

                }
                return model;
            }

            // Check for graduated students so they get a custom dashboard
            var checkGraduatedStudent = _db.Students.Include(i => i.SchoolProgramme).AsNoTracking()
                                .FirstOrDefault(x => x.Email.Trim().ToUpper().Equals(userId.ToUpper().Trim())
                                && x.Active.Equals(true) && x.IsGraduated.Equals(true));

            if (checkGraduatedStudent != null && HttpContext.Current.User.IsInRole(RoleName.Student))
            {
                model.HasGraduated = true;

                return model;
            }

            var checkStaff = _db.Staffs.AsNoTracking()
                               .FirstOrDefault(x => x.Email.ToUpper().Equals(userId.ToUpper().Trim())
                               && x.IsActiveStaff.Equals(true));
            if (checkStaff != null)
            {
                model.FullName = $"{checkStaff.LastName} {checkStaff.FirstName}";
                return model;
            }


            var CheckUtmeApplicants = _db.UtmeApplicants.AsNoTracking().Include(i => i.SchoolProgramme)
                             .Where(x => x.Email.ToUpper().Equals(userId.ToUpper().Trim())).ToList();
            var CheckUtmeApplicant = CheckUtmeApplicants.LastOrDefault();
            if (CheckUtmeApplicant != null)
            {
                model.FullName = $"{CheckUtmeApplicant.Surname} {CheckUtmeApplicant.FirstName}";
                model.SchoolProgramme = CheckUtmeApplicant.SchoolProgramme.ProgrammeCategory;
                model.ProgrammeType = CheckUtmeApplicant.SchoolProgramme.ProgrammeType;

                var applicatPayments = _db.ApplicantPayments.AsNoTracking()
                                        .Where(x => x.ApplicantEmail.Trim().ToUpper().Equals(CheckUtmeApplicant.Email.Trim().ToUpper()))
                                        .ToList();

                var applicatPayment = new ApplicantPayment();
                if (applicatPayments.Count() >= 1)
                {
                    //applicatPayment = applicatPayments.Where(x => x.PaymentDateTime.Year.Equals(DateTime.Now.Year)).LastOrDefault(); //Commented to fix 2019 utme issues
                    applicatPayment = applicatPayments.LastOrDefault(); 
                    //applicatPayment = applicatPayments.First();
                }
                else
                {
                    applicatPayment = applicatPayments.Where(x => x.PaymentDateTime.Year.Equals(DateTime.Now.Year)).FirstOrDefault();
                }

                model.HasPayedApplicationFee = applicatPayment != null && applicatPayment.IsPayed;
                return model;
            }

            var CheckApplicant = _db.Applicants.AsNoTracking().Include(i => i.SchoolProgramme)
                              .FirstOrDefault(x => x.ApplicantEmail.ToUpper().Equals(userId.ToUpper().Trim()));

            if (CheckApplicant != null && HttpContext.Current.User.IsInRole(RoleName.Applicant))
            {
                model.FullName = $"{CheckApplicant.LastName} {CheckApplicant.FirstName}";
                model.SchoolProgramme = CheckApplicant.SchoolProgramme.ProgrammeCategory;
                model.ProgrammeType = CheckApplicant.SchoolProgramme.ProgrammeType;
                var applicatPayment = _db.ApplicantPayments.AsNoTracking()
                                        .Where(x => x.ApplicantEmail.Trim().ToUpper().Equals(CheckApplicant.ApplicantEmail.Trim().ToUpper()) /*&& x.SessionId.Equals((int)CheckApplicant.SessionId)*/)
                                        .ToList();
                model.HasPayedApplicationFee = applicatPayment.Count() > 0 && applicatPayment.Any(x => x.IsPayed.Equals(true));
                model.ProgrammeType = CheckApplicant.SchoolProgramme.ProgrammeType;
            }
            return model;
        }

        public void UpdateTransactionLog(RemitaPaymentLog log, RemitaResponse result)
        {
            log.Rrr = result.Rrr;
            log.StatusCode = result.Status;
            log.TransactionMessage = result.Message;
            _db.Entry(log).State = EntityState.Modified;
        }

        public async Task<string> GetUserFullName(string userId)
        {
            var loginDetail = new LoginDetailVm();
            var UtmeApplicant = await _db.UtmeApplicants.AsNoTracking()
                                     .Where(x => x.Email.Trim().ToUpper().Equals(userId.Trim().ToUpper()))
                                     .FirstOrDefaultAsync();
            if (UtmeApplicant != null)
            {
                return UtmeApplicant.FullName;
            }

            var student = await _db.Students.AsNoTracking().Where(x => x.Email.Trim().ToUpper().Equals(userId.Trim().ToUpper()))
                            .FirstOrDefaultAsync();

            if (student != null)
            {

                return student.FullName;
            }
            var staff = await _db.Staffs.AsNoTracking().Where(x => x.Email.Trim().ToUpper().Equals(userId.Trim().ToUpper()))
                               .FirstOrDefaultAsync();
            if (staff != null)
            {
                return staff.FullName;
            }

            var applicant = await _db.Applicants.AsNoTracking()
                                      .Where(x => x.ApplicantEmail.Trim().ToUpper().Equals(userId.Trim().ToUpper()))
                                      .FirstOrDefaultAsync();
            if (applicant != null)
            {
                return applicant.FullName;
            }
            return "";

        }

        public async Task<LoginDetailVm> GetUserDetails(string userId)
        {
            var loginDetail = new LoginDetailVm();
            var student = await _db.Students.Include(i => i.SchoolProgramme).AsNoTracking()
                            .Where(x => x.Email.Equals(userId)).FirstOrDefaultAsync();
            if (student != null)
            {
                loginDetail.Email = student.Email;
                loginDetail.Email = student.PhoneNumber;
                loginDetail.FullName = student.FullName;
                loginDetail.UserId = student.MatricNo;
                loginDetail.UserType = $"{student.SchoolProgramme.FancyName} Student";

                return loginDetail;
            }
            var staff = await _db.Staffs.AsNoTracking().Where(x => x.Email.Equals(userId)).FirstOrDefaultAsync();
            if (staff != null)
            {
                loginDetail.Email = staff.Email;
                loginDetail.FullName = staff.FullName;
                loginDetail.UserId = staff.StaffId;
                loginDetail.UserType = $"{staff.StaffRole} Staff";

                return loginDetail;
            }

            var utmeApplicant = await _db.UtmeApplicants.Include(i => i.SchoolProgramme).AsNoTracking()
                                .Where(x => x.Email.Equals(userId)).FirstOrDefaultAsync();
            if (utmeApplicant != null)
            {
                loginDetail.Email = utmeApplicant.Email;
                loginDetail.FullName = utmeApplicant.FullName;
                loginDetail.UserId = utmeApplicant.JambRegNo;
                loginDetail.UserType = utmeApplicant.SchoolProgramme.FullName;
                return loginDetail;
            }
            var applicant = await _db.Applicants.Include(i => i.SchoolProgramme).AsNoTracking()
                                .Where(x => x.ApplicantEmail.Equals(userId)).FirstOrDefaultAsync();
            if (utmeApplicant != null)
            {
                loginDetail.Email = applicant.ApplicantEmail;
                loginDetail.FullName = applicant.FullName;
                loginDetail.UserId = applicant.ApplicantId;
                loginDetail.UserType = applicant.SchoolProgramme.FullName;
                return loginDetail;
            }
            loginDetail = null;
            return loginDetail;
        }

        public int GetStudentSchoolProgramme(string userId)
        {
            var student = _db.Students.Include(i => i.SchoolProgramme).AsNoTracking()
                          .Where(x => x.Email.Trim().ToUpper().Equals(userId.Trim().ToUpper())).FirstOrDefault();
            if (student != null)
            {
                return student.SchoolProgrammeId;
            }
            return 0;
        }

        public int GetStudentProgramme(string userId)
        {
            var student = _db.Students.Include(i => i.Programme).AsNoTracking()
                          .Where(x => x.Email.Trim().ToUpper().Equals(userId.Trim().ToUpper())).FirstOrDefault();
            if (student != null)
            {
                return (int)student.ProgrammeId;
            }
            return 0;
        }

        public async Task<Tuple<int, double, int>> UserActivityStatistic()
        {
            var loginUser = await _db.Users.AsNoTracking().CountAsync(x => x.IsLogin.Equals(true));
            var allUsers = await _db.Users.AsNoTracking().CountAsync(x => x.EmailConfirmed.Equals(true));
            double val1 = loginUser * 100;
            var percentage = Math.Round(val1 / allUsers, 2);
            // Create a 3-tuple and return it  
            var activityStat = new Tuple<int, double, int>(
            loginUser, percentage, allUsers);
            return activityStat;
        }

        public async Task<List<UnderGraduateRule>> GetUnderGraduateRule(string userId)
        {
            var utmeApplicant = _db.UtmeApplicants.Include(i => i.SchoolProgramme).Include(i => i.Programme)
                                .AsNoTracking().Where(x => x.Email.Equals(userId))
                                .FirstOrDefault();
            if (utmeApplicant != null)
            {
                return await _db.UnderGraduateRules.Include(i => i.SchoolProgramme).Include(i => i.Programme).Include(i => i.Subject)
                                   .AsNoTracking().Where(x => x.SchoolProgramme.SchoolProgrammeId.Equals(utmeApplicant.SchoolProgramme.SchoolProgrammeId)
                                   && x.Programme.ProgrammeId.Equals(utmeApplicant.Programme.ProgrammeId)).ToListAsync();
            }
            var applicant = await _db.Applicants.Include(i => i.SchoolProgramme).Include(i => i.AvailableCourse).AsNoTracking()
                                .Where(x => x.ApplicantEmail.Equals(userId)).FirstOrDefaultAsync();
            if (ProgrammeCategory.Masters.ToString().Equals(applicant.SchoolProgramme.ProgrammeCategory)
                || ProgrammeCategory.Phd.ToString().Equals(applicant.SchoolProgramme.ProgrammeCategory))
            {
                return await _db.UnderGraduateRules.Include(i => i.SchoolProgramme).Include(i => i.Programme).Include(i => i.Subject)
                                   .AsNoTracking().Where(x => x.SchoolProgramme.SchoolProgrammeId.Equals(applicant.SchoolProgramme.SchoolProgrammeId)
                                   && x.Programme.ProgrammeId.Equals(applicant.AvailableCourse.AvailableCourseId)).ToListAsync();
            }
            return await _db.UnderGraduateRules.Include(i => i.SchoolProgramme).Include(i => i.Programme).Include(i => i.Subject)
                              .AsNoTracking().Where(x => x.SchoolProgramme.SchoolProgrammeId.Equals(applicant.SchoolProgramme.SchoolProgrammeId)
                              ).ToListAsync();

        }

        public async Task<List<UnderGraduateRule>> GetUnderGraduateRule(int programmeId, int schoolProgrammeId)
        {

            return await _db.UnderGraduateRules.Include(i => i.SchoolProgramme).Include(i => i.Programme).Include(i => i.Subject)
                               .AsNoTracking().Where(x => x.SchoolProgramme.SchoolProgrammeId.Equals(schoolProgrammeId)
                               && x.Programme.ProgrammeId.Equals(programmeId)).ToListAsync();


        }

        public async Task<SchoolProgramme> GetUndergraduateCurrentSession()
        {
            return await _db.SchoolProgrammes.AsNoTracking()
                    .Where(x => x.ProgrammeCategory.Equals(ProgrammeCategory.UnderGraduate.ToString())
                    && x.ProgrammeType.Equals(ProgrammeType.Full_Time.ToString())).FirstOrDefaultAsync();
        }

        public string GetPreviousSession(string sessionName)
        {
            try
            {
                string[] splitSessionName = sessionName.Trim().Split('/');
                var firstPart = Convert.ToInt32(splitSessionName[0]);
                var secondPart = Convert.ToInt32(splitSessionName[1]);
                return $"{firstPart - 1}/{secondPart - 1}";
            }
            catch (Exception)
            {
                return null;
            }

        }

        public int GetSessionPartName(string sessionName)
        {
            string[] splitSessionName = sessionName.Trim().Split('/');
            var firstPart = Convert.ToInt32(splitSessionName[0]);
            return firstPart;
        }

        public string GetNextSession(string sessionName)
        {
            try
            {
                string[] splitSessionName = sessionName.Trim().Split('/');
                var firstPart = Convert.ToInt32(splitSessionName[0]);
                var secondPart = Convert.ToInt32(splitSessionName[1]);
                return $"{firstPart + 1}/{secondPart + 1}";
            }
            catch (Exception)
            {
                return null;
            }

        }
        public string GetPreviousLevel(string levelName)
        {
            levelName = levelName.Trim();
            if (levelName.Equals("100"))
            {
                return "100";
            }
            if (levelName.Equals("200"))
            {
                return "100";
            }
            if (levelName.Equals("300"))
            {
                return "200";
            }
            if (levelName.Equals("400"))
            {
                return "300";
            }
            if (levelName.Equals("500"))
            {
                return "400";
            }
            if (levelName.Equals("600"))
            {
                return "500";
            }
            return null;
        }

        public string GetNextLevel(string levelName)
        {
            levelName = levelName.Trim();
            if (levelName.Equals("100"))
            {
                return "200";
            }
            if (levelName.Equals("200"))
            {
                return "300";
            }
            if (levelName.Equals("300"))
            {
                return "400";
            }
            if (levelName.Equals("400"))
            {
                return "500";
            }
            if (levelName.Equals("500"))
            {
                return "600";
            }
            if (levelName.Equals("600"))
            {
                return "500";
            }
            return levelName;
        }


        public string GetSessionNameById(int SessionId)
        {
            return _db.Sessions.Where(x => x.SessionId.Equals(SessionId))
                               .Select(s => s.SessionName).FirstOrDefault();
        }
        public int GetSessionIdByName(string sessionName)
        {
            return _db.Sessions.Where(x => x.SessionName.Trim().Equals(sessionName))
                               .Select(s => s.SessionId).FirstOrDefault();
        }

        public string GetPreviousSession(string sessionName, int schoolProgramme)
        {
            try
            {
                //string[] splitSessionName = sessionName.Trim().Split('/');
                //var firstPart = Convert.ToInt32(splitSessionName[0]);
                //var secondPart = Convert.ToInt32(splitSessionName[1]);
                //return $"{firstPart - 1}/{secondPart - 1}";
                // Get the previous session before the active one
                // Find the currently active session for the given school program
                //var activeSession = _db.AssignSessionToSchools
                //    .Include(s => s.Session)
                //    .FirstOrDefault(x => x.ActiveSession == true && x.SchoolProgrammeId == schoolProgramme);

                //if (activeSession?.Session == null)
                //{
                //    return null; // No active session found, return null
                //}

                // Find the previous session based on session name
                var previousSession = _db.AssignSessionToSchools
                    .Include(s => s.Session)
                    .Where(s => s.Session != null && s.SchoolProgrammeId == schoolProgramme
                        && string.Compare(s.Session.SessionName, sessionName) < 0)
                    .OrderByDescending(s => s.Session.SessionName)
                    .FirstOrDefault();

                return previousSession?.Session?.SessionName; // Return previous session name or null

            }
            catch (Exception)
            {
                return null;
            }

        }


        public int GetApplicantStudentId(string userId)
        {
            userId = userId.Trim().ToUpper();
            var utmeStudent = _db.UtmeApplicants.AsNoTracking().Where(x => x.JambRegNo.Trim().ToUpper().Equals(userId) || x.Email.Trim().ToUpper().Equals(userId))
                  .FirstOrDefault();
            if (utmeStudent != null)
            {
                return utmeStudent.SessionId;
            }
            else
            {
                var applicant = _db.Applicants.Where(x => x.ApplicantEmail.Trim().ToUpper().Equals(userId)).FirstOrDefault();
                return (int)applicant.SessionId;
            }
        }

        public byte[] MapUtmeDePicture(string jambNumber)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(HostingEnvironment.MapPath("~/UtmeDePictures"));
            var files = dirInfo.GetFiles().ToList();
              var jambApplicant =  _db.UtmeApplicants.Where(x => x.JambRegNo.ToUpper().Trim().Equals(jambNumber.ToUpper())).FirstOrDefault();


            var filesInDir = dirInfo.GetFiles("*" + jambNumber + "*.*");
            foreach (var item in filesInDir)
            {

                if (jambApplicant != null && jambApplicant.Passport == null)
                {
                    System.Drawing.Image img = System.Drawing.Image.FromFile(item.FullName);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        img.Save(ms, ImageFormat.Jpeg);
                        jambApplicant.Passport = ms.ToArray();
                        _db.Entry(jambApplicant).State = EntityState.Modified;
                    }
                }
            }

             _db.SaveChangesAsync();

            return jambApplicant.Passport;
        }

        public void Dispose()
        {
            _db?.Dispose();
            GC.SuppressFinalize(this);
        }


    }
}