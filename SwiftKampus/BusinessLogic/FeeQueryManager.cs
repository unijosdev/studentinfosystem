using Microsoft.Ajax.Utilities;
using SwiftKampus.Abstractions;
using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampus.ViewModels.Fee_Management;
using SwiftKampusModel;
using SwiftKampusModel.Payment;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace SwiftKampus.BusinessLogic
{
    public class FeeQueryManager : IFeeQueryManager
    {
        private readonly SchoolDbContext _db;

        public FeeQueryManager(SchoolDbContext db)
        {
            _db = db;
        }

        public async Task<List<FeeList>> GetSchoolFeeList(string feeCategory, Student student, PaymentSetting paymentSetting, int sessionId)
            {
            string studentStatus;
            if (student.Session.SessionId.Equals(sessionId) || student.StudentStatus.Equals(StudentStatus.New_Student.ToString()))
            {
                studentStatus = StudentStatus.New_Student.ToString();
            }
            else
            {
                studentStatus = StudentStatus.Returning.ToString();
            }

            if (paymentSetting.AcceptPartPayment)
            {
                return await _db.SchoolFeeTypes.Include(i => i.SchoolProgramme)
                            .AsNoTracking().Where(x => x.FeeCategory.Equals(feeCategory)
                            && x.SchoolProgrammeId.Equals(student.SchoolProgrammeId)
                            && x.Indegine.Equals(student.Indegine)
                            && x.Session.SessionId.Equals(sessionId)
                            && x.StudentType.ToUpper().Equals(studentStatus.ToUpper()))
                            .Select(s => new FeeList()
                            {
                                FeeTypeName = s.FeeCode,
                                Amount = s.Amount,
                                Description = s.FeeName
                            }).ToListAsync();
            }
            return await _db.SchoolFeeTypes.Include(i => i.SchoolProgramme)
                         .AsNoTracking().Where(x => x.SchoolProgrammeId.Equals(student.SchoolProgrammeId)
                         && x.Indegine.Equals(student.Indegine)
                          && x.Session.SessionId.Equals(sessionId)
                         && x.StudentType.ToUpper().Equals(studentStatus.ToUpper()))
                         .Select(s => new FeeList()
                         {
                             FeeTypeName = s.FeeName,
                             Amount = s.Amount,
                             Description = s.Description
                         }).ToListAsync();
        }
        public async Task<List<FeeList>> GetSchoolFeeListByFaculty(string feeCategory, Student student, PaymentSetting paymentSetting, int sessionId, int facultyId)
        {
            if (paymentSetting.AcceptPartPayment)
            {
                return await _db.SchoolFeeTypes.Include(i => i.SchoolProgramme).Include(i => i.Faculty)
                            .AsNoTracking().Where(x => x.FeeCategory.Equals(feeCategory)
                            && x.SchoolProgrammeId.Equals(student.SchoolProgrammeId)
                            && x.Indegine.Equals(student.NationalityStatus)
                            && x.Session.SessionId.Equals(sessionId)
                            && x.Faculty.FacultyId.Equals(facultyId)
                            && x.StudentType.ToUpper().Equals(student.StudentStatus.ToUpper()))
                            .Select(s => new FeeList()
                            {
                                FeeTypeName = s.FeeName,
                                Amount = s.Amount,
                                Description = s.Description
                            }).ToListAsync();
            }
            return await _db.SchoolFeeTypes.Include(i => i.SchoolProgramme)
                            .AsNoTracking().Where(x => x.SchoolProgrammeId.Equals(student.SchoolProgrammeId)
                            && x.Indegine.Equals(student.NationalityStatus)
                            && x.Session.SessionId.Equals(sessionId)
                            && x.Faculty.FacultyId.Equals(facultyId)
                            && x.StudentType.ToUpper().Equals(student.StudentStatus.ToUpper()))
                             .Select(s => new FeeList()
                             {
                                 FeeTypeName = s.FeeName,
                                 Amount = s.Amount,
                                 Description = s.Description
                             }).ToListAsync();
        }

        public async Task<List<FeeList>> GetSchoolFeeListReport(string feeCategory, int SchoolProgrammeId, int SessionId, string studentType)
        {
            if (feeCategory.Equals(SchoolFeeCategory.Acceptance.ToString()))
            {
                var model = await _db.SchoolFeeTypes.Include(i => i.SchoolProgramme).Include(i => i.Faculty)
                            .AsNoTracking().Where(x => x.SchoolProgrammeId.Equals(SchoolProgrammeId)
                            && x.FeeCategory.ToUpper().Equals(feeCategory.ToUpper())
                            && x.Session.SessionId.Equals(SessionId)).ToListAsync();
                /*&& x.Faculty.FacultyId.Equals(FacultyId)*/

                return model.DistinctBy(x => x.FeeName).ToList().Select(s => new FeeList()
                {
                    FeeTypeName = s.FeeName,
                    Amount = s.Amount,
                    Description = s.Description
                }).ToList();
            }
            else
            {
                var model = await _db.SchoolFeeTypes.Include(i => i.SchoolProgramme).Include(i => i.Faculty)
                           .AsNoTracking().Where(x => x.SchoolProgrammeId.Equals(SchoolProgrammeId)
                           && x.FeeCategory.ToUpper().Equals(feeCategory.ToUpper())
                           && x.Session.SessionId.Equals(SessionId)
                           && x.StudentType.Equals(studentType)).ToListAsync();
                /*&& x.Faculty.FacultyId.Equals(FacultyId)*/

                return model.DistinctBy(x => x.FeeName).ToList().Select(s => new FeeList()
                {
                    FeeTypeName = s.FeeName,
                    Amount = s.Amount,
                    Description = s.Description
                }).ToList();
            }
           
        }



        public async Task<List<FeeList>> GetLatePaymentFeeList(int sessionId, int schoolProgrammeId, string feeType, DateTime? checkDate)
        {
            if (checkDate == null)
            {
                checkDate = DateTime.Now;
            }
            var feeList = new List<FeeList>();
            var latePayment = await _db.SchoolFeeSettings.Where(x => x.SessionId.Equals(sessionId)
                                    && x.SchoolProgrammeId.Equals(schoolProgrammeId)
                                    && x.IsActive.Equals(true) && x.FeeCategory.ToUpper().Equals(feeType.ToUpper()))
                                    .FirstOrDefaultAsync();
            if (latePayment != null)
            {
                int dateCompare1 = DateTime.Compare((DateTime)checkDate, latePayment.StartDate);
                if (dateCompare1 > 0)
                {
                    var lateFee = new FeeList
                    {
                        FeeTypeName = "Late Registration",
                        Description = "Fee charges for Late registration",
                        Amount = Convert.ToDecimal(latePayment.FinedAmount)
                    };
                    feeList.Add(lateFee);
                }
            }
            return feeList;
        }

        public async Task<List<SchoolFeePayment>> GetSchoolFeePaymentList(string feeCategory, int sessionId)
        {
            return await _db.SchoolFeePayments.Include(i => i.Students).Include(i => i.Session)
                                .Include(i => i.Students.Programme).Include(i => i.Students.Level)
                                .AsNoTracking().Where(x => x.FeeCategory.Equals(feeCategory) &&
                                x.Status.Equals(true) && x.SessionId.Equals(sessionId)).ToListAsync();
        }

        public async Task<List<FeeList>> GetDepartmentPaymentList(int deptId, int sessionId, int levelId)
        {
            var deptFees = await _db.DepartmentFeeTypes.AsNoTracking().Where(x => x.DepartmentId.Equals(deptId)
                                && x.SessionId.Equals(sessionId) && x.LevelId == levelId).ToListAsync();
            var feeList = new List<FeeList>();
            foreach (var item in deptFees)
            {
                feeList.Add(new FeeList { FeeTypeName = item.FeeName, Amount = item.Amount, Description = item.Description });
            }
            return feeList;
        }

        public string GetServiceType(string feeCategory, string schoolProgramme)
        {
            var serviceTypeId = string.Empty;
            if (feeCategory.Equals(SchoolFeeCategory.Acceptance.ToString()) &&
                (schoolProgramme.Equals(ProgrammeCategory.Masters.ToString()) || schoolProgramme.Equals(ProgrammeCategory.Post_Graduate.ToString()))
                || schoolProgramme.Equals(ProgrammeCategory.Phd.ToString()) || schoolProgramme.Equals(ProgrammeCategory.MBA.ToString()))
            {
                serviceTypeId = RemitaConfigParams.PGACCEPTANCESERVICETYPE;
            }
            else if (feeCategory.ToUpper().Trim().Equals(SchoolFeeCategory.School_Charges.ToString().ToUpper().Trim()) &&
               (schoolProgramme.Equals(ProgrammeCategory.Masters.ToString()) || schoolProgramme.Equals(ProgrammeCategory.Post_Graduate.ToString()))
               || schoolProgramme.Equals(ProgrammeCategory.Phd.ToString()) || schoolProgramme.Equals(ProgrammeCategory.MBA.ToString()))
            {
                serviceTypeId = RemitaConfigParams.PGSCHOOLFEESERVICETYPE;
            }
            if (feeCategory.Equals(SchoolFeeCategory.Acceptance.ToString()) || feeCategory.Equals(SchoolFeeCategory.School_Charges.ToString()) &&
                (schoolProgramme.Equals(ProgrammeCategory.Remedial_Science.ToString())))
            {
                serviceTypeId = RemitaConfigParams.UTILITY;
            }
            else if (feeCategory.Equals(SchoolFeeCategory.Acceptance.ToString()))
            {
                serviceTypeId = RemitaConfigParams.ACCEPTANCESERVICETYPE;
            }
            else if (feeCategory.Equals(SchoolFeeCategory.School_Charges.ToString()))
            {
                serviceTypeId = RemitaConfigParams.SCHOOLFEESERVICETYPE;
            }
            return serviceTypeId;
        }

        public decimal GetFeeAmount(SchoolFeePaymentVm model, List<FeeList> feeList, PaymentSetting paymentSetting)
        {
            var totalAmount = feeList.Sum(s => s.Amount);
            var payingAmount = 0.0m;
            if (paymentSetting.AcceptPartPayment)
            {

            }
            //if (SchoolSetUp.IsPartPaymet  && semester.SemesterName.ToUpper().Equals("FIRST"))
            //{
            //    //var payingPercentage = (decimal)(paymentSetting.FirstPaymentPercentage / 100) * totalAmount;
            //    payingAmount = feeList;
            //}
            //else if (model.SchoolFeePaymentType.Equals(SchoolFeePaymentType.Part_Payment)
            //                && semester.SemesterName.ToUpper().Equals("SECOND"))
            //{
            //    var payingPercentage = (decimal)(paymentSetting.FirstPaymentPercentage / 100) * totalAmount;
            //    var remainingBalance = totalAmount - payingPercentage;
            //    payingAmount = remainingBalance;
            //}
            //else
            //{
            //    payingAmount = totalAmount;
            //}
            return payingAmount;
        }

        public async Task<PaymentSetting> GetPaymentSetting(int sessionId, int schoolProgrammeId, string studentStatus)
        {
            return await _db.PaymentSettings.AsNoTracking().Where(x => x.SessionId.Equals(sessionId)
                    && x.SchoolProgrammeId.Equals(schoolProgrammeId) && x.StudentType.ToUpper().Equals(studentStatus.ToUpper()))
                    .FirstOrDefaultAsync();
        }

        public async Task<PaymentSetting> GetPaymentSetting(int sessionId, int schoolProgrammeId)
        {
            return await _db.PaymentSettings.AsNoTracking().Where(x => x.SessionId.Equals(sessionId)
                    && x.SchoolProgrammeId.Equals(schoolProgrammeId))
                    .FirstOrDefaultAsync();
        }


    }
}