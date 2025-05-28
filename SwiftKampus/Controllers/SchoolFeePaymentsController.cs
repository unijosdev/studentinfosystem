using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using OfficeOpenXml;
using Rotativa;
using SwiftKampus.Abstractions;
using SwiftKampus.BusinessLogic;
using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampus.ViewModels;
using SwiftKampus.ViewModels.Fee_Management;
using SwiftKampusModel;
using SwiftKampusModel.Accomodation;
using SwiftKampusModel.Payment;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Net.Http;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json.Linq;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;

namespace SwiftKampus.Controllers
{
    [Authorize]
    [Audit(AuditingLevel = 2)]
    public class SchoolFeePaymentsController : BaseController
    {
        IFeeQueryManager _feeQueryManager;
        IStudentQueryManager StudentQuery { get; }
        List<Semester> allSemesters;
        List<Session> allSessions;

        public SchoolFeePaymentsController(SchoolDbContext db) : base(db)
        {
            StudentQuery = new StudentQueryManager(_db);
            _feeQueryManager = new FeeQueryManager(_db);
            allSessions = GetAllSession();
            allSemesters = GetAllSemesters();
        }
        // GET: SchoolFeePayments
        public async Task<ActionResult> Index()
        {
            var student = await _db.Students.Include(i => i.SchoolProgramme).Include(i => i.Session).Include(i => i.Programme.Department.Faculty)
                                .AsNoTracking().FirstOrDefaultAsync(x => x.Email.Trim().ToUpper().Equals(userId.Trim().ToUpper())
                                && x.Active.Equals(true));
            var schoolFeePayments = await _db.SchoolFeePayments.Include(s => s.Semester)
                                    .Include(s => s.Session).Include(s => s.Students)
                                    .Where(x => x.StudentId.Equals(student.StudentId) && x.Status.Equals(true)).ToListAsync();

            return View(schoolFeePayments);
        }
        public async Task<ActionResult> SchoolPaymentReport()
        {
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            ViewBag.DepartmentId = new SelectList(await _db.Departments.AsNoTracking().ToListAsync(), "DepartmentId", "DeptName");
            ViewBag.LevelId = new SelectList(await _db.Levels.AsNoTracking().ToListAsync(), "LevelId", "LevelName");
            var feeCategory = from SchoolFeeCategory s in Enum.GetValues(typeof(SchoolFeeCategory))
                              select new { ID = s, Name = s.ToString() };
            var studentType = from StudentStatus s in Enum.GetValues(typeof(StudentStatus))
                              select new { ID = s, Name = s.ToString() };

            ViewBag.FeeCategoryId = new MultiSelectList(feeCategory, "Name", "Name");
            ViewBag.StudentType = new MultiSelectList(studentType, "Name", "Name");
            ViewBag.SchoolProgrammeId = new SelectList(await _db.SchoolProgrammes.AsNoTracking().ToListAsync(), "SchoolProgrammeId", "FancyName");
            return View();
        }

        public async Task<ActionResult> GetSchoolPaymentReport(int SchoolProgrammeId, string StudentType, string FeeCategoryId, int? DepartmentId, int? LevelId,
                           int? SessionId, string HasPayed, DateTime? StartDate, DateTime? EndDate)
        {

            bool hasPayed = false || (!string.IsNullOrEmpty(HasPayed) && HasPayed.Equals("True"));
            if (SessionId == null)
            {
                SessionId = _query.GetCurrentSessionId(SchoolProgrammeId);
            }
            var startDate = StartDate != null ? Convert.ToDateTime(StartDate) : new DateTime(2018, 12, 1);
            var endDate = EndDate != null ? Convert.ToDateTime(EndDate) : new DateTime(2022, 12, 1);

            var schoolFee = await _db.SchoolFeePayments.Include(i => i.Students).Include(i => i.Session)
                                    .Include(i => i.Students.Programme.Department).Include(i => i.Students.Level)
                                    .Include(i => i.Students.Session)
                                    .AsNoTracking()
                                    .Where(x => x.FeeCategory.Equals(FeeCategoryId)
                                    && x.Status.Equals(hasPayed) && x.SessionId.Equals((int)SessionId)
                                    && x.Students.SchoolProgrammeId.Equals(SchoolProgrammeId)).ToListAsync();

            schoolFee = schoolFee.Where(x => x.Date.Date >= startDate.Date && x.Date.Date <= endDate.Date).ToList();

            var dept = await _db.Departments.FindAsync(DepartmentId);

            if (StudentType.Equals(StudentStatus.New_Student.ToString()) && FeeCategoryId.Equals(SchoolFeeCategory.School_Charges.ToString()))
            {
                schoolFee = schoolFee.Where(x => x.Students.Session.SessionId.Equals((int)SessionId)).ToList();
            }

            if (StudentType.Equals(StudentStatus.Returning.ToString()) && FeeCategoryId.Equals(SchoolFeeCategory.School_Charges.ToString()))
            {
                schoolFee = schoolFee.Where(x => x.Students.Session.SessionId != (int)SessionId).ToList();
            }

            if (DepartmentId != null & LevelId != null)
            {
                schoolFee = schoolFee.Where(x => x.Students.Programme.Department.DepartmentId.Equals((int)DepartmentId)
                                                 && x.Students.Level.LevelId.Equals((int)LevelId)
                                                 /*&& x.SessionId.Equals((int)SessionId)*/).ToList();
            }
            else if (DepartmentId != null)
            {
                schoolFee = schoolFee.Where(x => x.Students.Programme.Department.DepartmentId.Equals((int)DepartmentId)).ToList();
            }
            else if (LevelId != null)
            {
                schoolFee = schoolFee.Where(x => x.Students.Level.LevelId.Equals((int)LevelId)).ToList();
            }

            var feeType = new List<ReportFeeListVm>();
            var feeList = await _feeQueryManager.GetSchoolFeeListReport(FeeCategoryId, SchoolProgrammeId, (int)SessionId, StudentType);
            foreach (var item in feeList)
            {
                feeType.Add(new ReportFeeListVm
                {
                    FeeTypeName = item.FeeTypeName,
                    Amount = item.Amount,
                    Description = item.Description,
                    TotalAmount = item.Amount * schoolFee.Count()
                });
            }

            var payingamount = feeList.Sum(x => x.Amount);
            var model = new SchoolPaymentFeeListVm
            {
                ReportFeeListVms = feeType,
                OverallTotal = schoolFee.Count() * payingamount,
                TotalStudent = schoolFee.Count()
            };
            return PartialView(model);
        }


        public ActionResult VerifySchoolFee()
        {
            return View();
        }

        public ActionResult VerifyAcceptanceFee()
        {
            return View();
        }

        public async Task<PartialViewResult> StudentLedger(string studentId)
        {
            var schoolPayments = await _db.SchoolFeePayments.Include(i => i.Session).Include(i => i.Students)
                                    .Include(i => i.Students.Programme).Include(i => i.Students.Programme.Department)
                                    .Include(i => i.Students.Programme.Department.Faculty).Include(i => i.Students.Level)
                                    .AsNoTracking().Where(x => x.StudentId.Equals(studentId))
                                    .OrderBy(o => o.SessionId).ToListAsync();
            return PartialView(schoolPayments);
        }

        public async Task<ActionResult> BursaaryValidateTransaction(string studentId, string rrr, string FeeCategory, int? Sessions)
        {
            var message = string.Empty;
            long milliseconds = DateTime.Now.Ticks;

            if (!string.IsNullOrEmpty(studentId) && !string.IsNullOrEmpty(rrr) && !string.IsNullOrEmpty(FeeCategory))
            {
                if (FeeCategory.Equals(PaymentFeeCategory.Acceptance.ToString()) || FeeCategory.Equals(PaymentFeeCategory.School_Charges.ToString()))
                {

                    studentId = studentId.ToUpper().Trim();
                    var student = await _db.Students.Include(i => i.SchoolProgramme)
                                                    .Include(i => i.Session)
                                                    .Include(i => i.Programme.Department)
                                                    .Include(i => i.Level)
                                                    .AsNoTracking().Where(x => x.MatricNo.Trim().ToUpper().Equals(studentId) ||
                                    x.JambRegNo.Trim().ToUpper().Equals(studentId)).FirstOrDefaultAsync();

                    if (student != null)
                    {
                        var transactions = await _db.SchoolFeePayments.Include(x => x.Students).AsNoTracking().Where(x => x.ReferenceNo.Trim().Equals(rrr.Trim())).FirstOrDefaultAsync();

                        if (transactions != null && transactions.Status == true)
                        {
                            message = $"This RRR is already used by {transactions.Students.FullName} - {transactions.Students.JambRegNo} and transaction is successful";
                        }
                        else
                        {
                            //get all student's payment parameters
                            string studentStatus = student.SessionId == (Int32)Sessions || student.StudentStatus == StudentStatus.New_Student.ToString()
                                   ? StudentStatus.New_Student.ToString()
                                   : StudentStatus.Returning.ToString();

                            SchoolFeePaymentVm schFeePayModel = new SchoolFeePaymentVm
                            {
                                FeeCategory = SchoolFeeCategory.School_Charges.ToString(),
                                SessionId = (Int32)Sessions
                            };

                            var paymentSetting = await GetPaymentSettingAsync((Int32)Sessions, student.SchoolProgrammeId, studentStatus);

                            var feeList = await GenerateFeeListAsync(student, schFeePayModel);

                            var hashrrr = _query.HashRrrQuery(rrr, RemitaConfigParams.APIKEY, RemitaConfigParams.MERCHANTID);
                            string posturl = RemitaConfigParams.CHECKSTATUSURL + "/" + RemitaConfigParams.MERCHANTID + "/" + rrr + "/" + hashrrr + "/" + "status.reg";
                            try
                            {
                                string jsondata = new WebClient().DownloadString(posturl);
                                var result = JsonConvert.DeserializeObject<RemitaResponse>(jsondata);

                                if (result.Status.Equals("00") || result.Status.Equals("01"))
                                {

                                    //await CreatePaymentLogAndDetailsAsync(student, confirmPayModel, feeList, paymentSetting);

                                    var schoolFeePayment = new SchoolFeePayment
                                    {
                                        OrderId = $"{SchoolSetUp.CurrentSchoolName}SF{milliseconds}",
                                        FeeCategory = FeeCategory,
                                        Date = DateTime.Now,
                                        SemesterId = _query.GetCurrentSemesterId(student.SchoolProgrammeId),
                                        SessionId = (Int32)Sessions,
                                        StudentId = student.StudentId,
                                        PaidFee = result.Amount,
                                        TotalAmount = SumPaymentsForStudentInSession(student.StudentId, (Int32)Sessions, FeeCategory) != 0 ?
                                                        feeList.Sum(x => x.Amount) - SumPaymentsForStudentInSession(student.StudentId, (Int32)Sessions, FeeCategory) :
                                                        feeList.Sum(x => x.Amount),
                                        //PaymentMode = 0,
                                        IsPartPaymet = true,
                                        LevelId = student.LevelId,
                                        ReferenceNo = result.Rrr,
                                        Status = true,
                                        PaymentStatus = result.Message
                                    };
                                    _db.SchoolFeePayments.Add(schoolFeePayment);
                                    var log = new RemitaPaymentLog
                                    {
                                        OrderId = schoolFeePayment.OrderId,
                                        PaymentName = schoolFeePayment.FeeCategory,
                                        PaymentDate = DateTime.Now,
                                        Amount = schoolFeePayment.TotalAmount.ToString(),
                                        PayerName = student.FullName

                                    };
                                    _db.RemitaPaymentLogs.Add(log);

                                    foreach (var fee in feeList)
                                    {
                                        var studentFeeDetails = new StudentPaymentDetail()
                                        {
                                            StudentId = student.StudentId,
                                            SessionId = schoolFeePayment.SessionId,
                                            FeeTypeName = fee.FeeTypeName,
                                            Amount = fee.Amount,
                                            Description = fee.Description
                                        };
                                        _db.StudentPaymentDetails.Add(studentFeeDetails);

                                    }

                                    message = $"Payment is imported successfully for {student.FullName} with RRR {rrr} and status {result.Message}";
                                    await _db.SaveChangesAsync();

                                }
                                else
                                {
                                    message = $"Payment status for this student is not found RRR {rrr} is {result.Message}";
                                }
                            }
                            catch (Exception ex)
                            {
                                message = $"Remita Network {ex.Message}";
                            }
                        }
                    }
                    else
                    {
                        message = "Student Record is not found";
                    }
                }
            }
            else
            {
                message = "Student Jamb No/Matric-No/PGA Form  or Remita Reference Number is empty";
            }
            ViewBag.Message = message;
            var feeCategory = from PaymentFeeCategory s in Enum.GetValues(typeof(PaymentFeeCategory))
                              select new { ID = s, Name = s.ToString() };
            ViewBag.FeeCategory = new SelectList(feeCategory, "Name", "Name");

            var sessions = await _db.Sessions.OrderByDescending(s => s.SessionName).ToListAsync();
            var sessionName = from s in sessions select new { Name = s.SessionName, Id = s.SessionId };
            ViewBag.Sessions = new SelectList(sessionName, "Id", "Name");

            return View();
        }


        // Use this when students are seeing wrong payment charges, but havent't made payment yet
        [Authorize(Roles = RoleName.Admin + "," + RoleName.TSupport + "," + RoleName.Bursary)]
        public async Task<ActionResult> RemoveTransaction(string studentId, string FeeCategory, int? SessionId)
        {
            var message = string.Empty;

            if (!string.IsNullOrEmpty(studentId) && !string.IsNullOrEmpty(FeeCategory) && SessionId != null)
            {
                studentId = studentId.ToUpper().Trim();
                var student = await _db.Students.AsNoTracking().Where(x => x.MatricNo.Trim().ToUpper().Equals(studentId.Trim().ToUpper()) ||
                                x.JambRegNo.Trim().ToUpper().Equals(studentId.Trim().ToUpper())).Select(x => x.StudentId).FirstOrDefaultAsync();

                if (student == null) message = "No student with such matric/jamb/form number found";

                // for acceptance fee
                if (student != null && FeeCategory.Equals(PaymentFeeCategory.Acceptance.ToString()))
                {
                    var transactions = await _db.SchoolFeePayments.AsNoTracking().Where(x => x.StudentId.Equals(student)
                    && x.FeeCategory.Equals(SchoolFeeCategory.Acceptance.ToString()) && x.SessionId == SessionId).ToListAsync();

                    if (transactions.Count != 0)
                    {
                        foreach (var item in transactions)
                        {
                            var entry = _db.Entry(item);
                            if (entry.State == EntityState.Detached)
                                _db.SchoolFeePayments.Attach(item);
                            _db.SchoolFeePayments.Remove(item);
                        }
                        await _db.SaveChangesAsync();
                        message = "Transaction Removed Successfully";
                    }
                    else
                    {
                        message = "Student Transaction not found";
                    }

                }

                // for school charges
                if (student != null && FeeCategory.Equals(PaymentFeeCategory.School_Charges.ToString()))
                {
                    var transactions = await _db.SchoolFeePayments.AsNoTracking().Where(x => x.StudentId.Equals(student)
                    && x.FeeCategory.Equals(SchoolFeeCategory.School_Charges.ToString()) && x.SessionId == SessionId).ToListAsync();

                    if (transactions.Count != 0)
                    {
                        foreach (var item in transactions)
                        {
                            var entry = _db.Entry(item);
                            if (entry.State == EntityState.Detached)
                                _db.SchoolFeePayments.Attach(item);
                            _db.SchoolFeePayments.Remove(item);
                        }
                        await _db.SaveChangesAsync();
                        message = "Transaction Removed Successfully";
                    }
                    else
                    {
                        message = "Student Transaction not found";
                    }

                }
            }
            else
            {
                message = "Student Id or Fee Category or Payment Session is empty";
            }

            ViewBag.Message = message;
            var feeCategory = from PaymentFeeCategory s in Enum.GetValues(typeof(PaymentFeeCategory))
                              select new { ID = s, Name = s.ToString() };
            ViewBag.FeeCategory = new SelectList(feeCategory, "Name", "Name");

            var currentUndergradSessionId = _query.GetCurrentSessionId(1);
            var sessions = await _db.Sessions.OrderByDescending(s => s.SessionName).ToListAsync();
            var sessionName = from s in sessions select new { Name = s.SessionName, Id = s.SessionId };
            ViewBag.SessionId = new SelectList(sessionName, "Id", "Name", currentUndergradSessionId);

            return View();
        }

        public async Task<ActionResult> RemoveTransactionChangeOfCourse(string studentId)
        {
            var message = string.Empty;

            if (!string.IsNullOrEmpty(studentId))
            {
                studentId = studentId.ToUpper().Trim();
                var student = await _db.Students.AsNoTracking().Where(x => x.MatricNo.Trim().ToUpper().Equals(studentId) ||
                                x.JambRegNo.Trim().ToUpper().Equals(studentId)).Select(x => x.StudentId).FirstOrDefaultAsync();

                if (student != null)
                {
                    var transactions = await _db.ChangeOfCoursePayments.Include(x => x.Student).AsNoTracking().Where(x =>
                        x.Student.StudentId.Equals(student)
                        /*&& x..Equals(SchoolFeeCategory.School_Charges.ToString()*/).FirstOrDefaultAsync();
                    if (transactions != null)
                    {
                        var entry = _db.Entry(transactions);
                        if (entry.State == EntityState.Detached)
                            _db.ChangeOfCoursePayments.Attach(transactions);
                        _db.ChangeOfCoursePayments.Remove(transactions);
                        await _db.SaveChangesAsync();
                        message = "Message Removed Successfully";
                    }
                    else
                    {
                        message = "Student Transaction not found";
                    }

                }
                else
                {
                    message = "Transactions not found";
                }
            }
            else
            {
                message = "Student Id is empty";
            }

            ViewBag.Message = message;

            return View();
        }

        //public async Task<ActionResult> BursaaryValidateTransaction(string studentId, string rrr, string FeeCategory, int? Sessions)
        //{
        //    var message = string.Empty;
        //    long milliseconds = DateTime.Now.Ticks;

        //    if (!string.IsNullOrEmpty(studentId) && !string.IsNullOrEmpty(rrr) && !string.IsNullOrEmpty(FeeCategory))
        //    {
        //        if (FeeCategory.Equals(PaymentFeeCategory.Acceptance.ToString()) || FeeCategory.Equals(PaymentFeeCategory.School_Charges.ToString()))
        //        {

        //            studentId = studentId.ToUpper().Trim();
        //            var student = await _db.Students.Include(i => i.SchoolProgramme)
        //                                            .Include(i => i.Session)
        //                                            .Include(i => i.Programme.Department)
        //                                            .Include(i => i.Level)
        //                                            .AsNoTracking().Where(x => x.MatricNo.Trim().ToUpper().Equals(studentId) ||
        //                            x.JambRegNo.Trim().ToUpper().Equals(studentId)).FirstOrDefaultAsync();

        //            if (student != null)
        //            {
        //                var transactions = await _db.SchoolFeePayments.Include(x => x.Students).AsNoTracking().Where(x => x.ReferenceNo.Trim().Equals(rrr.Trim())).FirstOrDefaultAsync();

        //                if (transactions != null && transactions.Status == true)
        //                {
        //                    message = $"This RRR is already used by {transactions.Students.FullName} - {transactions.Students.JambRegNo} and transaction is successful";
        //                }
        //                else
        //                {
        //                    //get all student's payment parameters
        //                    string studentStatus = student.SessionId == (Int32)Sessions || student.StudentStatus == StudentStatus.New_Student.ToString()
        //                           ? StudentStatus.New_Student.ToString()
        //                           : StudentStatus.Returning.ToString();

        //                    SchoolFeePaymentVm schFeePayModel = new SchoolFeePaymentVm
        //                    {
        //                        FeeCategory = SchoolFeeCategory.School_Charges.ToString(),
        //                        SessionId = (Int32)Sessions
        //                    };

        //                    var paymentSetting = await GetPaymentSettingAsync((Int32)Sessions, student.SchoolProgrammeId, studentStatus);

        //                    var feeList = await GenerateFeeListAsync(student, schFeePayModel);

        //                    var hashrrr = _query.HashRrrQuery(rrr, RemitaConfigParams.APIKEY, RemitaConfigParams.MERCHANTID);
        //                    string posturl = RemitaConfigParams.CHECKSTATUSURL + "/" + RemitaConfigParams.MERCHANTID + "/" + rrr + "/" + hashrrr + "/" + "status.reg";
        //                    try
        //                    {
        //                        string jsondata = new WebClient().DownloadString(posturl);
        //                        var result = JsonConvert.DeserializeObject<RemitaResponse>(jsondata);

        //                        if (result.Status.Equals("00") || result.Status.Equals("01"))
        //                        {

        //                            //await CreatePaymentLogAndDetailsAsync(student, confirmPayModel, feeList, paymentSetting);

        //                            var schoolFeePayment = new SchoolFeePayment
        //                            {
        //                                OrderId = $"{SchoolSetUp.CurrentSchoolName}SF{milliseconds}",
        //                                FeeCategory = FeeCategory,
        //                                Date = DateTime.Now,
        //                                SemesterId = _query.GetCurrentSemesterId(student.SchoolProgrammeId),
        //                                SessionId = (Int32)Sessions,
        //                                StudentId = student.StudentId,
        //                                PaidFee = result.Amount,
        //                                TotalAmount = SumPaymentsForStudentInSession(student.StudentId, (Int32)Sessions, FeeCategory) != 0 ?
        //                                                feeList.Sum(x => x.Amount) - SumPaymentsForStudentInSession(student.StudentId, (Int32)Sessions, FeeCategory) :
        //                                                feeList.Sum(x => x.Amount),
        //                                //PaymentMode = 0,
        //                                IsPartPaymet = true,
        //                                LevelId = student.LevelId,
        //                                ReferenceNo = result.Rrr,
        //                                Status = true,
        //                                PaymentStatus = result.Message
        //                            };
        //                            _db.SchoolFeePayments.Add(schoolFeePayment);
        //                            var log = new RemitaPaymentLog
        //                            {
        //                                OrderId = schoolFeePayment.OrderId,
        //                                PaymentName = schoolFeePayment.FeeCategory,
        //                                PaymentDate = DateTime.Now,
        //                                Amount = schoolFeePayment.TotalAmount.ToString(),
        //                                PayerName = student.FullName

        //                            };
        //                            _db.RemitaPaymentLogs.Add(log);

        //                            foreach (var fee in feeList)
        //                            {
        //                                var studentFeeDetails = new StudentPaymentDetail()
        //                                {
        //                                    StudentId = student.StudentId,
        //                                    SessionId = schoolFeePayment.SessionId,
        //                                    FeeTypeName = fee.FeeTypeName,
        //                                    Amount = fee.Amount,
        //                                    Description = fee.Description
        //                                };
        //                                _db.StudentPaymentDetails.Add(studentFeeDetails);

        //                            }

        //                            message = $"Payment is imported successfully for {student.FullName} with RRR {rrr} and status {result.Message}";
        //                            await _db.SaveChangesAsync();

        //                        }
        //                        else
        //                        {
        //                            message = $"Payment status for this student is not found RRR {rrr} is {result.Message}";
        //                        }
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        message = $"Remita Network {ex.Message}";
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                message = "Student Record is not found";
        //            }
        //        }
        //    }
        //    else
        //    {
        //        message = "Student Jamb No/Matric-No/PGA Form  or Remita Reference Number is empty";
        //    }
        //    ViewBag.Message = message;
        //    var feeCategory = from PaymentFeeCategory s in Enum.GetValues(typeof(PaymentFeeCategory))
        //                      select new { ID = s, Name = s.ToString() };
        //    ViewBag.FeeCategory = new SelectList(feeCategory, "Name", "Name");

        //    var sessions = await _db.Sessions.OrderByDescending(s => s.SessionName).ToListAsync();
        //    var sessionName = from s in sessions select new { Name = s.SessionName, Id = s.SessionId };
        //    ViewBag.Sessions = new SelectList(sessionName, "Id", "Name");

        //    return View();
        //}

        public decimal SumPaymentsForStudentInSession(string studentId, int sessionId, string FeeCategory)
        {
            decimal totalPaidFees = 0;
            // Filter payments for the given studentId and sessionId
            var payments = _db.SchoolFeePayments
                .Where(p => p.StudentId == studentId && p.SessionId == sessionId && p.Status == true && p.FeeCategory.Equals(FeeCategory.ToString())).ToList();

            // Calculate the total sum of paid fees
            if (payments.Count() > 0)
            {
                return totalPaidFees = payments.Sum(p => p.PaidFee);

            }
            else
            {
                return totalPaidFees = 0;
            }

        }

        //For creating students schoool charges paid by NELFUND
        [HttpGet]
        [Authorize(Roles = RoleName.Admin + "," +  RoleName.SuperAdmin )]
        public ActionResult createStdNELFUND(string message)
        {
            ViewBag.Message = message;
            return View();
        }

        [Authorize(Roles = RoleName.Admin + "," + RoleName.SuperAdmin)]
        public async Task<ActionResult> createStdNELFUND(HttpPostedFileBase excelfile)
        {
            if (excelfile == null || excelfile.ContentLength == 0)
            {
                ViewBag.Error = "Please Select a excel file <br/>";
                ViewBag.Message = "You must select an excel file before you click Upload button";
                TempData["Title"] = "Error.";

                return View("Index");
            }
            HttpPostedFileBase file = Request.Files["excelfile"];
            if (excelfile.FileName.EndsWith("xls", StringComparison.CurrentCulture) || excelfile.FileName.EndsWith("xlsx", StringComparison.CurrentCulture))
            {
                string lastrecord = "";
                int count = 0;
                string message = "";
                string fileContentType = file.ContentType;
                byte[] fileBytes = new byte[file.ContentLength];
                var data = file.InputStream.Read(fileBytes, 0, Convert.ToInt32(file.ContentLength));

                var utmeUploadError = new List<StudentUtmeUploadErrorVm>();
                bool hasUtmeUploadError = false;

                // Read data from excel file
                using (var package = new ExcelPackage(file.InputStream))
                {
                    ExcelValidation myExcel = new ExcelValidation();
                    var currentSheet = package.Workbook.Worksheets;
                    var workSheet = currentSheet.First();
                    var noOfCol = workSheet.Dimension.End.Column;
                    var noOfRow = workSheet.Dimension.End.Row;
                    int requiredField = 1;

                    string validCheck = myExcel.ValidateExcel(noOfRow, workSheet, requiredField);
                    if (!validCheck.Equals("Success"))
                    {
                        string[] ssizes = validCheck.Split(' ');
                        string[] myArray = new string[2];
                        for (int i = 0; i < ssizes.Length; i++)
                        {
                            myArray[i] = ssizes[i];
                        }
                        string lineError = $"Line/Row number {myArray[0]}  and column {myArray[1]} is not rightly formatted, Please Check for anomalies ";
                        ViewBag.Message = lineError;
                        return View("ErrorException");
                    }

                    for (int row = 2; row <= noOfRow; row++)
                    {
                        var matNo = workSheet.Cells[row, 1].Value.ToString().Trim();
                        var Session = workSheet.Cells[row, 4].Value.ToString().Trim();
                        var FeeCategory = workSheet.Cells[row, 2].Value.ToString().Trim();
                        var PaidFee = workSheet.Cells[row, 3].Value.ToString().Trim();

                        var session = await _db.Sessions.Where(s => s.SessionName.Trim().Equals(Session.Trim())).FirstOrDefaultAsync();



                        if (!string.IsNullOrEmpty(matNo)   && !string.IsNullOrEmpty(FeeCategory))
                        {
                            if (FeeCategory.Equals(PaymentFeeCategory.Acceptance.ToString()) || FeeCategory.Equals(PaymentFeeCategory.School_Charges.ToString()))
                            {
                                //var message = string.Empty;
                                long milliseconds = DateTime.Now.Ticks;
                                var studentId = matNo.ToUpper().Trim();
                                var student = await _db.Students.Include(i => i.SchoolProgramme)
                                                                .Include(i => i.Session)
                                                                .Include(i => i.Programme.Department)
                                                                .Include(i => i.Level)
                                                                .AsNoTracking().Where(x => x.MatricNo.Trim().ToUpper().Equals(studentId) ||
                                                x.JambRegNo.Trim().ToUpper().Equals(studentId)).FirstOrDefaultAsync();

                                if (student != null)
                                {
                                    
                                    var SessionId = _query.GetCurrentSessionId(student.SchoolProgrammeId);
                                        //get all student's payment parameters
                                        string studentStatus = student.SessionId == (Int32)SessionId || student.StudentStatus == StudentStatus.New_Student.ToString()
                                               ? StudentStatus.New_Student.ToString()
                                               : StudentStatus.Returning.ToString();

                                        SchoolFeePaymentVm schFeePayModel = new SchoolFeePaymentVm
                                        {
                                            FeeCategory = SchoolFeeCategory.School_Charges.ToString(),
                                            SessionId = (Int32)session.SessionId
                                        };

                                        var paymentSetting = await GetPaymentSettingAsync((Int32)session.SessionId, student.SchoolProgrammeId, studentStatus);

                                        var feeList = await GenerateFeeListAsync(student, schFeePayModel);

                                        //var hashrrr = _query.HashRrrQuery(rrr, RemitaConfigParams.APIKEY, RemitaConfigParams.MERCHANTID);
                                        //string posturl = RemitaConfigParams.CHECKSTATUSURL + "/" + RemitaConfigParams.MERCHANTID + "/" + rrr + "/" + hashrrr + "/" + "status.reg";
                                        try
                                        {
                                            //string jsondata = new WebClient().DownloadString(posturl);
                                            //var result = JsonConvert.DeserializeObject<RemitaResponse>(jsondata);

                                            //if (result.Status.Equals("00") || result.Status.Equals("01"))
                                            //{

                                                //await CreatePaymentLogAndDetailsAsync(student, confirmPayModel, feeList, paymentSetting);

                                                var schoolFeePayment = new SchoolFeePayment
                                                {
                                                    OrderId = $"{SchoolSetUp.CurrentSchoolName}SF{milliseconds}",
                                                    FeeCategory = FeeCategory,
                                                    Date = DateTime.Now,
                                                    SemesterId = _query.GetCurrentSemesterId(student.SchoolProgrammeId),
                                                    SessionId = (Int32)session.SessionId,
                                                    StudentId = student.StudentId,
                                                    PaidFee = decimal.Parse(PaidFee),
                                                    TotalAmount = SumPaymentsForStudentInSession(student.StudentId, (Int32)session.SessionId, FeeCategory) != 0 ?
                                                                    feeList.Sum(x => x.Amount) - SumPaymentsForStudentInSession(student.StudentId, (Int32)session.SessionId, FeeCategory) :
                                                                    feeList.Sum(x => x.Amount),
                                                    PaymentMode = PMode.NELFUND,
                                                    IsPartPaymet = false,
                                                    LevelId = student.LevelId,
                                                    ReferenceNo = GenerateReferenceNumber(),
                                                    Status = true,
                                                    PaymentStatus = "Successful",
                                                };
                                                _db.SchoolFeePayments.Add(schoolFeePayment);
                                                var log = new RemitaPaymentLog
                                                {
                                                    OrderId = schoolFeePayment.OrderId,
                                                    PaymentName = schoolFeePayment.FeeCategory,
                                                    PaymentDate = DateTime.Now,
                                                    Amount = schoolFeePayment.TotalAmount.ToString(),
                                                    PayerName = student.FullName

                                                };
                                                _db.RemitaPaymentLogs.Add(log);

                                                foreach (var fee in feeList)
                                                {
                                                    var studentFeeDetails = new StudentPaymentDetail()
                                                    {
                                                        StudentId = student.StudentId,
                                                        SessionId = schoolFeePayment.SessionId,
                                                        FeeTypeName = fee.FeeTypeName,
                                                        Amount = fee.Amount,
                                                        Description = fee.Description
                                                    };
                                                    _db.StudentPaymentDetails.Add(studentFeeDetails);
                                                    count++;
                                                }

                                                message = $"You have successfully created {count} payments";
                                                //await _db.SaveChangesAsync();

                                        }
                                        catch (Exception ex)
                                        {
                                            message = $"Remita Network {ex.Message}";
                                        }
                                }
                                else
                                {
                                    message = "Student Record is not found";
                                }
                            }
                        }
                        else
                        {
                            message = "Student Jamb No/Matric-No/PGA Form  or Remita Reference Number is empty";
                        }


                    }
                    try
                    {
                        await _db.SaveChangesAsync();

                    }
                    catch (Exception ex)
                    {
                        ViewBag.ErrorMessage = ex.Message;
                        return View("ErrorException");
                    }
                    if (hasUtmeUploadError)
                    {
                        ViewBag.ErrorInfo = $"Success!";
                        ViewBag.ErrorMessage = $"You have successfully created {count} records...";
                        return View("ErrorException", utmeUploadError);
                    }
                    message = $"You have successfully created {count} records";
                    ViewBag.Message = message;
                    return RedirectToAction("Index", "Students", new { message });
                }
            }
            ViewBag.Error = $"File type is Incorrect <br/>";
            return View("Index");
        }

        private string GenerateReferenceNumber()
        {
            long milliseconds = DateTime.Now.Ticks;
            // Your logic to generate a reference number
            //return Guid.NewGuid().ToString(); // Example: Generate a GUID
            return $"UJNELFUND{milliseconds}";
        }
        public async Task<ActionResult> ValidateTransaction(string studentId, string rrr, string FeeCategory, int? paymentYear, int? Sessions)
        {
            var message = string.Empty;

            if (!string.IsNullOrEmpty(studentId) && !string.IsNullOrEmpty(rrr) && !string.IsNullOrEmpty(FeeCategory))
            {
                if (FeeCategory.Equals(PaymentFeeCategory.Acceptance.ToString()) || FeeCategory.Equals(PaymentFeeCategory.School_Charges.ToString()))
                {

                    studentId = studentId.ToUpper().Trim();
                    var student = await _db.Students.Include(i => i.SchoolProgramme).AsNoTracking().Where(x => x.MatricNo.Trim().ToUpper().Equals(studentId) ||
                                    x.JambRegNo.Trim().ToUpper().Equals(studentId)).FirstOrDefaultAsync();

                    if (student != null)
                    {
                        var transactions = await _db.SchoolFeePayments.Include(x => x.Students).AsNoTracking().Where(x => x.ReferenceNo.Trim().Equals(rrr.Trim())).FirstOrDefaultAsync();
                        //transactions.StudentId = "UNIJOS637720014229593823";
                        //_db.Entry(transactions).State = EntityState.Modified;
                        //await _db.SaveChangesAsync();
                        if (transactions != null && transactions.Status == true)
                        {
                            message = $"This RRR is already used by {transactions.Students.FullName} - {transactions.Students.JambRegNo} and transaction is successful";
                        }
                        else
                        {
                            var hashrrr = _query.HashRrrQuery(rrr, RemitaConfigParams.APIKEY, RemitaConfigParams.MERCHANTID);
                            string posturl = RemitaConfigParams.CHECKSTATUSURL + "/" + RemitaConfigParams.MERCHANTID + "/" + rrr + "/" + hashrrr + "/" + "status.reg";
                            try
                            {
                                string jsondata = new WebClient().DownloadString(posturl);
                                var result = JsonConvert.DeserializeObject<RemitaResponse>(jsondata);

                                if (result.Status.Equals("00") || result.Status.Equals("01"))
                                {
                                    //var sessionId = _query.GetCurrentSessionId(student.SchoolProgramme.SchoolProgrammeId);

                                    var schoofeepayment = await _db.SchoolFeePayments.Include(i => i.Students).AsNoTracking().Where(x => x.StudentId.Equals(student.StudentId)
                                                            && x.FeeCategory.Trim().ToUpper().Equals(FeeCategory.Trim().ToUpper()) && x.SessionId.Equals((Int32)Sessions))
                                                               .FirstOrDefaultAsync();
                                    if (schoofeepayment != null && result.PaymentDate.Year.Equals(paymentYear)
                                        //&& result.PaymentDate.Month.Equals(schoofeepayment.Date.Month)
                                        && result.Amount >= schoofeepayment.PaidFee
                                        /*&& compareValue((double)schoofeepayment.TotalAmount, (double)result.Amount)*/)
                                    {
                                        schoofeepayment.Status = true;
                                        schoofeepayment.PaidFee = result.Amount;
                                        schoofeepayment.PaymentStatus = result.Message;
                                        schoofeepayment.ReferenceNo = result.Rrr;
                                        _db.Entry(schoofeepayment).State = EntityState.Modified;
                                        await _db.SaveChangesAsync();
                                        message = $"Payment is validated successfully for this RRR {rrr} is {result.Message}";
                                    }
                                    else
                                    {
                                        message = $"Payment Record for this Student is not found RRR {rrr} is {result.Message}";
                                    }
                                }
                                else
                                {
                                    message = $"Payment status for this student is not found RRR {rrr} is {result.Message}";
                                }
                            }
                            catch (Exception ex)
                            {
                                message = $"Remita Network {ex.Message}";
                            }
                        }
                    }
                    else
                    {
                        message = "Student Record is not found";
                    }
                }
                if (FeeCategory.Equals(PaymentFeeCategory.Sales_Of_Forms.ToString()))
                {
                    studentId = studentId.ToUpper().Trim();
                    var applicant = await _db.Applicants.Include(i => i.Session).AsNoTracking().Where(x => x.ApplicantId.Trim().ToUpper().Equals(studentId) ||
                                    x.ApplicantEmail.Trim().ToUpper().Equals(studentId)).FirstOrDefaultAsync();

                    if (applicant != null)
                    {
                        var transactions = await _db.ApplicantPayments.AsNoTracking().Where(x => x.ReferenceNo.Trim().Equals(rrr.Trim())).FirstOrDefaultAsync();
                        if (transactions != null)
                        {
                            message = $"This RRR is already used by {transactions.FullName} - {transactions.ApplicantEmail}";
                        }
                        else
                        {
                            var hashrrr = _query.HashRrrQuery(rrr, RemitaConfigParams.APIKEY, RemitaConfigParams.MERCHANTID);
                            string posturl = RemitaConfigParams.CHECKSTATUSURL + "/" + RemitaConfigParams.MERCHANTID + "/" + rrr + "/" + hashrrr + "/" + "status.reg";
                            try
                            {
                                string jsondata = new WebClient().DownloadString(posturl);
                                var result = JsonConvert.DeserializeObject<RemitaResponse>(jsondata);

                                if (result.Status.Equals("00") || result.Status.Equals("01"))
                                {
                                    var sessionId = applicant.Session.SessionId;
                                    var applicantfeepayment = await _db.ApplicantPayments.AsNoTracking().Where(x => x.ApplicantEmail.Equals(applicant.ApplicantEmail)
                                                            && x.SessionId.Equals(29)).FirstOrDefaultAsync();

                                    if (applicantfeepayment != null && result.PaymentDate.Year.Equals(applicantfeepayment.PaymentDateTime.Year)
                                                                    /*&& result.PaymentDate.Month.Equals(applicantfeepayment.PaymentDateTime.Month)*/
                                                                    //&& result.Amount.Equals(applicantfeepayment.TotalAmount)
                                                                    && result.Amount >= applicantfeepayment.AmountPayed)
                                    {
                                        applicantfeepayment.IsPayed = true;
                                        applicantfeepayment.TransactionMessage = result.Message;
                                        applicantfeepayment.ReferenceNo = result.Rrr;
                                        _db.Entry(applicantfeepayment).State = EntityState.Modified;
                                        await _db.SaveChangesAsync();
                                        message = $"Payment is validated successfully for this RRR {rrr} is {result.Message}";
                                    }
                                    else
                                    {
                                        message = $"Payment Record for this Applicant is not found RRR {rrr} is {result.Message}";
                                    }
                                }
                                else
                                {
                                    message = $"Payment status for this  is not found RRR {rrr} is {result.Message}";
                                }
                            }
                            catch (Exception ex)
                            {
                                message = $"Remita Network {ex.Message}";
                            }
                        }
                    }
                    else
                    {
                        message = "APplicant Record is not found";
                    }
                }
                if (FeeCategory.Equals(PaymentFeeCategory.Accommodation.ToString()))
                {
                    studentId = studentId.ToUpper().Trim();
                    var applicant = await _db.Students.Include(i => i.Session).Where(x => x.MatricNo.Trim().ToUpper().Equals(studentId) ||
                                    x.PrimaryEmail.Trim().ToUpper().Equals(studentId)).FirstOrDefaultAsync();
                    int currentSessionId = _query.GetCurrentSessionId(applicant.SchoolProgrammeId);

                    if (applicant != null)
                    {
                        var transactions = await _db.StudentAccommodationFeePayments.Include(a => a.Student).AsNoTracking().Where(x => x.ReferenceNo.Trim().Equals(rrr.Trim())).FirstOrDefaultAsync();

                        if (transactions != null)
                        {
                            message = $"This RRR is already used by {transactions.Student.FullName} - {transactions.Student.PrimaryEmail}";
                        }
                        else
                        {
                            var hashrrr = _query.HashRrrQuery(rrr, RemitaConfigParams.APIKEY, RemitaConfigParams.MERCHANTID);
                            string posturl = RemitaConfigParams.CHECKSTATUSURL + "/" + RemitaConfigParams.MERCHANTID + "/" + rrr + "/" + hashrrr + "/" + "status.reg";
                            try
                            {
                                string jsondata = new WebClient().DownloadString(posturl);
                                var result = JsonConvert.DeserializeObject<RemitaResponse>(jsondata);

                                if (result.Status.Equals("00") || result.Status.Equals("01"))
                                {
                                    var sessionId = applicant.Session.SessionId;
                                    var applicantfeepayment = await _db.StudentAccommodationFeePayments.Include(a => a.Student).Where(x => x.Student.Email.Trim().ToUpper().Equals(applicant.Email.Trim().ToUpper())
                                                            && x.SessionId.Equals(currentSessionId)).FirstOrDefaultAsync();

                                    //var applicantfeepayment = await _db.AccommodationFeePayments.AsNoTracking().Where(x => x.StudentId.Equals(applicant.StudentId)
                                    //        && x.SessionId.Equals(sessionId)).FirstOrDefaultAsync();

                                    if (applicantfeepayment != null)
                                    {
                                        StudentAssignedRoom assignedRoom = await _db.StudentAssignedRooms.Include(i => i.Student).Include(i => i.Hostel).Include(i => i.Block)
                                        .Include(i => i.Room).Include(i => i.Session).AsNoTracking()
                                        .Where(x => x.StudentId.Trim().Equals(applicant.StudentId.Trim()) && x.SessionId.Equals(currentSessionId))
                                        .FirstOrDefaultAsync();

                                        var assinedRoom = _db.StudentAssignedRooms.Include(s => s.Session).Include(s => s.StudentAccommodationFeePayments)
                                                    .Where(x => x.StudentId.Equals(applicant.StudentId)
                                                    && x.SessionId.Equals(currentSessionId)).FirstOrDefault();

                                        applicantfeepayment.IsPayed = true;
                                        applicantfeepayment.TransactionMessage = result.Message;
                                        applicantfeepayment.ReferenceNo = result.Rrr;
                                        _db.Entry(applicantfeepayment).State = EntityState.Modified;

                                        assinedRoom.PaymentStatus = true;
                                        _db.Entry(assinedRoom).State = EntityState.Modified;

                                        await _db.SaveChangesAsync();
                                        message = $"Payment is validated successfully for this RRR {rrr} is {result.Message}";
                                    }
                                    else
                                    {
                                        message = $"Payment Record for this Applicant is not found RRR {rrr} is {result.Message}";
                                    }
                                }
                                else
                                {
                                    message = $"Payment status for this  is not found RRR {rrr} is {result.Message}";
                                }
                            }
                            catch (Exception ex)
                            {
                                message = $"Remita Network {ex.Message}";
                            }
                        }
                    }
                    else
                    {
                        message = "APplicant Record is not found";
                    }
                }

                if (FeeCategory.Equals(PaymentFeeCategory.Accommodation_application.ToString()))
                {
                    studentId = studentId.ToUpper().Trim();
                    var applicant = await _db.Students.Include(i => i.Session).AsNoTracking().Where(x => x.MatricNo.Trim().ToUpper().Equals(studentId) ||
                                    x.PrimaryEmail.Trim().ToUpper().Equals(studentId)).FirstOrDefaultAsync();

                    if (applicant != null)
                    {
                        var transactions = await _db.HostelApplications.Include(a => a.Student).AsNoTracking().Where(x => x.ReferenceNo.Trim().Equals(rrr.Trim())).FirstOrDefaultAsync();
                        if (transactions != null)
                        {
                            message = $"This RRR is already used by {transactions.Student.FullName} - {transactions.Student.PrimaryEmail}";
                        }
                        else
                        {
                            var hashrrr = _query.HashRrrQuery(rrr, RemitaConfigParams.APIKEY, RemitaConfigParams.MERCHANTID);
                            string posturl = RemitaConfigParams.CHECKSTATUSURL + "/" + RemitaConfigParams.MERCHANTID + "/" + rrr + "/" + hashrrr + "/" + "status.reg";
                            try
                            {
                                string jsondata = new WebClient().DownloadString(posturl);
                                var result = JsonConvert.DeserializeObject<RemitaResponse>(jsondata);

                                if (result.Status.Equals("00") || result.Status.Equals("01"))
                                {
                                    var sessionId = applicant.Session.SessionId;
                                    var applicantfeepayment = await _db.HostelApplications.Include(a => a.Student).AsNoTracking().Where(x => x.Student.PrimaryEmail.Equals(applicant.PrimaryEmail)
                                                            && x.SessionId.Equals(20)).FirstOrDefaultAsync();

                                    if (applicantfeepayment != null && result.PaymentDate.Year.Equals(applicantfeepayment.PaymentDateTime.Year)
                                                                   /*&& result.PaymentDate.Month.Equals(applicantfeepayment.PaymentDateTime.Month)*/
                                                                   && result.Amount.Equals(applicantfeepayment.AmountPayed)
                                                                   /* && compareValue((Double)applicantfeepayment.AmountPayed, (double)result.Amount)*/)
                                    {
                                        applicantfeepayment.IsPayed = true;
                                        applicantfeepayment.TransactionMessage = result.Message;
                                        applicantfeepayment.ReferenceNo = result.Rrr;
                                        _db.Entry(applicantfeepayment).State = EntityState.Modified;
                                        await _db.SaveChangesAsync();
                                        message = $"Payment is validated successfully for this RRR {rrr} is {result.Message}";
                                    }
                                    else
                                    {
                                        message = $"Payment Record for this Applicant is not found RRR {rrr} is {result.Message}";
                                    }
                                }
                                else
                                {
                                    message = $"Payment status for this  is not found RRR {rrr} is {result.Message}";
                                }
                            }
                            catch (Exception ex)
                            {
                                message = $"Remita Network {ex.Message}";
                            }
                        }
                    }
                    else
                    {
                        message = "APplicant Record is not found";
                    }
                }
                if (FeeCategory.Equals(PaymentFeeCategory.Change_of_course.ToString()) || FeeCategory.Equals(ChangeOfCourseType.Inter_Faculty_Transfer.ToString()))
                {
                    studentId = studentId.ToUpper().Trim();
                    var applicant = await _db.Students.Include(i => i.Session).AsNoTracking().Where(x => x.JambRegNo.Trim().ToUpper().Equals(studentId)
                    || x.PrimaryEmail.Trim().ToUpper().Equals(studentId) || x.MatricNo.Trim().ToUpper().Equals(studentId.Trim().ToUpper())).FirstOrDefaultAsync();

                    if (applicant != null)
                    {
                        var transactions = await _db.ChangeOfCoursePayments.Include(a => a.Student).AsNoTracking().Where(x => x.ReferenceNo.Trim().Equals(rrr.Trim())).FirstOrDefaultAsync();
                        if (transactions != null)
                        {
                            message = $"This RRR is already used by {transactions.Student.FullName} - {transactions.Student.PrimaryEmail}";
                        }
                        else
                        {
                            var hashrrr = _query.HashRrrQuery(rrr, RemitaConfigParams.APIKEY, RemitaConfigParams.MERCHANTID);
                            string posturl = RemitaConfigParams.CHECKSTATUSURL + "/" + RemitaConfigParams.MERCHANTID + "/" + rrr + "/" + hashrrr + "/" + "status.reg";
                            try
                            {
                                string jsondata = new WebClient().DownloadString(posturl);
                                var result = JsonConvert.DeserializeObject<RemitaResponse>(jsondata);

                                if (result.Status.Equals("00") || result.Status.Equals("01"))
                                {
                                    var sessionId = applicant.Session.SessionId;
                                    var applicantfeepayment = await _db.ChangeOfCoursePayments.Include(a => a.Student).Include(a => a.Session).Where(x => x.Student.StudentId.Trim().Equals(applicant.StudentId.Trim())
                                                            && x.SessionId.Equals(20)).FirstOrDefaultAsync();


                                    if (applicantfeepayment != null && result.PaymentDate.Year.Equals(applicantfeepayment.PaymentDateTime.Year)
                                                                    /*&& result.PaymentDate.Month.Equals(applicantfeepayment.PaymentDateTime.Month)*/
                                                                    && result.Amount.Equals(applicantfeepayment.TotalAmount)
                                                                    && compareValue((double)applicantfeepayment.TotalAmount, (double)result.Amount))
                                    {
                                        applicantfeepayment.IsPayed = true;
                                        applicantfeepayment.TransactionMessage = result.Message;
                                        applicantfeepayment.ReferenceNo = result.Rrr;
                                        _db.Entry(applicantfeepayment).State = EntityState.Modified;
                                        await _db.SaveChangesAsync();
                                        message = $"Payment is validated successfully for this RRR {rrr} is {result.Message}";
                                    }
                                    else
                                    {
                                        message = $"Payment Record for this Applicant is not found RRR {rrr} is {result.Message}";
                                    }
                                }
                                else
                                {
                                    message = $"Payment status for this  is not found RRR {rrr} is {result.Message}";
                                }
                            }
                            catch (Exception ex)
                            {
                                message = $"Remita Network {ex.Message}";
                            }
                        }
                    }
                    else
                    {
                        message = "APplicant Record is not found";
                    }
                }

                if (FeeCategory.Equals(ChangeOfCourseType.Waiver_Phd.ToString()))
                {
                    studentId = studentId.ToUpper().Trim();
                    var applicant = await _db.Applicants.Include(i => i.Session).AsNoTracking().Where(x => x.ApplicantId.Trim().ToUpper().Equals(studentId)
                    /*||x.Email.Trim().ToUpper().Equals(studentId)*/).FirstOrDefaultAsync();

                    if (applicant != null)
                    {
                        var transactions = await _db.ApplicantWaiverPayments.Include(a => a.Applicant).AsNoTracking().Where(x => x.ReferenceNo.Trim().Equals(rrr.Trim())).FirstOrDefaultAsync();
                        if (transactions != null)
                        {
                            message = $"This RRR is already used by {transactions.Applicant.FullName} - {transactions.Applicant.ApplicantEmail}";
                        }
                        else
                        {
                            var hashrrr = _query.HashRrrQuery(rrr, RemitaConfigParams.APIKEY, RemitaConfigParams.MERCHANTID);
                            string posturl = RemitaConfigParams.CHECKSTATUSURL + "/" + RemitaConfigParams.MERCHANTID + "/" + rrr + "/" + hashrrr + "/" + "status.reg";
                            try
                            {
                                string jsondata = new WebClient().DownloadString(posturl);
                                var result = JsonConvert.DeserializeObject<RemitaResponse>(jsondata);

                                if (result.Status.Equals("00") || result.Status.Equals("01"))
                                {
                                    var sessionId = applicant.Session.SessionId;
                                    var applicantfeepayment = await _db.ApplicantWaiverPayments.Include(a => a.Applicant).Include(a => a.Session).Where(x => x.ApplicantId.Equals(applicant.ApplicantId)
                                                            && x.SessionId.Equals(sessionId)).FirstOrDefaultAsync();

                                    if (applicantfeepayment != null && result.PaymentDate.Year.Equals(applicantfeepayment.PaymentDateTime.Year)
                                                                    /*&& result.PaymentDate.Month.Equals(applicantfeepayment.PaymentDateTime.Month)*/
                                                                    && result.Amount.Equals(applicantfeepayment.TotalAmount)
                                                                    && compareValue((double)applicantfeepayment.TotalAmount, (double)result.Amount))
                                    {
                                        applicantfeepayment.IsPayed = true;
                                        applicantfeepayment.ApplicantId = applicant.ApplicantEmail;
                                        applicantfeepayment.TransactionMessage = result.Message;
                                        applicantfeepayment.ReferenceNo = result.Rrr;
                                        _db.Entry(applicantfeepayment).State = EntityState.Modified;
                                        await _db.SaveChangesAsync();
                                        message = $"Payment is validated successfully for this RRR {rrr} is {result.Message}";
                                    }
                                    else
                                    {
                                        message = $"Payment Record for this Applicant is not found RRR {rrr} is {result.Message}";
                                    }
                                }
                                else
                                {
                                    message = $"Payment status for this  is not found RRR {rrr} is {result.Message}";
                                }
                            }
                            catch (Exception ex)
                            {
                                message = $"Remita Network {ex.Message}";
                            }
                        }
                    }
                    else
                    {
                        message = "APplicant Record is not found";
                    }
                }

                if (FeeCategory.Equals(PaymentFeeCategory.Utme_Screeing.ToString()))
                {
                    if (!string.IsNullOrEmpty(studentId) && !string.IsNullOrEmpty(rrr))
                    {
                        studentId = studentId.ToUpper().Trim();
                        var utmeApplicant = await _db.UtmeApplicants.Include(i => i.Session).AsNoTracking().Where(x => x.JambRegNo.Trim().ToUpper().Equals(studentId) ||
                                        x.Email.Trim().ToUpper().Equals(studentId)).FirstOrDefaultAsync();

                        if (utmeApplicant != null)
                        {
                            var transactions = await _db.ApplicantPayments.AsNoTracking().Where(x => x.ReferenceNo.Trim().Equals(rrr.Trim())).FirstOrDefaultAsync();
                            if (transactions != null)
                            {
                                message = $"This RRR is already used by {transactions.FullName} - {transactions.ApplicantEmail}";
                            }
                            else
                            {
                                var hashrrr = _query.HashRrrQuery(rrr, RemitaConfigParams.APIKEY, RemitaConfigParams.MERCHANTID);
                                string posturl = RemitaConfigParams.CHECKSTATUSURL + "/" + RemitaConfigParams.MERCHANTID + "/" + rrr + "/" + hashrrr + "/" + "status.reg";
                                try
                                {
                                    string jsondata = new WebClient().DownloadString(posturl);
                                    var result = JsonConvert.DeserializeObject<RemitaResponse>(jsondata);

                                    if (result.Status.Equals("00") || result.Status.Equals("01"))
                                    {
                                        var sessionId = utmeApplicant.Session.SessionId;

                                        var applicantfeepayment = await _db.ApplicantPayments.AsNoTracking().Where(x => x.JambRegNo.Equals(utmeApplicant.JambRegNo)
                                                                && x.SessionId.Equals(sessionId)).FirstOrDefaultAsync();

                                        if (applicantfeepayment != null && result.PaymentDate.Year.Equals(applicantfeepayment.PaymentDateTime.Year)
                                            /*&& result.PaymentDate.Month.Equals(applicantfeepayment.PaymentDateTime.Month)*/ && result.Amount.Equals(applicantfeepayment.AmountPayed))
                                        {
                                            applicantfeepayment.IsPayed = true;
                                            applicantfeepayment.TransactionMessage = result.Message;
                                            applicantfeepayment.ReferenceNo = result.Rrr;
                                            _db.Entry(applicantfeepayment).State = EntityState.Modified;
                                            await _db.SaveChangesAsync();
                                            message = $"Payment is validated successfully for this RRR {rrr} is {result.Message}";
                                        }
                                        else
                                        {
                                            message = $"Payment Record for this Applicant is not found RRR {rrr} is {result.Message}";
                                        }
                                    }
                                    else
                                    {
                                        message = $"Payment status for this  is not found RRR {rrr} is {result.Message}";
                                    }
                                }
                                catch (Exception ex)
                                {
                                    message = $"Remita Network {ex.Message}";
                                }
                            }
                        }
                        else
                        {
                            message = "Student Record is not found";
                        }
                    }
                    //message = "Feature not implemented yet";
                }

                if (FeeCategory.Equals(PaymentFeeCategory.Id_Card_Charges.ToString()))
                {
                    if (!string.IsNullOrEmpty(studentId) && !string.IsNullOrEmpty(rrr))
                    {
                        studentId = studentId.ToUpper().Trim();
                        var utmeApplicant = await _db.Students.Include(i => i.Session).AsNoTracking().Where(x => x.MatricNo.Trim().ToUpper().Equals(studentId) ||
                                        x.Email.Trim().ToUpper().Equals(studentId)).FirstOrDefaultAsync();

                        if (utmeApplicant != null)
                        {
                            var transactions = await _db.IdCardPayments.Include(a => a.Student).AsNoTracking().Where(x => x.ReferenceNo.Trim().Equals(rrr.Trim())).FirstOrDefaultAsync();
                            //_db.Entry(transactions).State = EntityState.Deleted;
                            //await _db.SaveChangesAsync();
                            if (transactions != null && transactions.IsPayed == true)
                            {
                                message = $"This RRR is already used by {transactions.Student.FullName} - {transactions.Student.JambRegNo}";
                            }
                            else
                            {
                                var hashrrr = _query.HashRrrQuery(rrr, RemitaConfigParams.APIKEY, RemitaConfigParams.MERCHANTID);
                                string posturl = RemitaConfigParams.CHECKSTATUSURL + "/" + RemitaConfigParams.MERCHANTID + "/" + rrr + "/" + hashrrr + "/" + "status.reg";
                                try
                                {
                                    string jsondata = new WebClient().DownloadString(posturl);
                                    var result = JsonConvert.DeserializeObject<RemitaResponse>(jsondata);

                                    if (result.Status.Equals("00") || result.Status.Equals("01"))
                                    {
                                        var sessionId = utmeApplicant.Session.SessionId;

                                        var applicantfeepayment = await _db.IdCardPayments.AsNoTracking().Where(x => x.StudentId.Equals(utmeApplicant.StudentId)
                                                                /*&& x.SessionId.Equals(sessionId)*/).FirstOrDefaultAsync();

                                        if (applicantfeepayment != null && result.Amount >= applicantfeepayment.TotalAmount)
                                        {
                                            applicantfeepayment.IsPayed = true;
                                            applicantfeepayment.TransactionMessage = result.Message;
                                            applicantfeepayment.ReferenceNo = result.Rrr;
                                            _db.Entry(applicantfeepayment).State = EntityState.Modified;
                                            await _db.SaveChangesAsync();
                                            message = $"Payment is validated successfully for this RRR {rrr} is {result.Message}";
                                        }
                                        else
                                        {
                                            message = $"Payment Record for this Applicant is not found RRR {rrr} is {result.Message}";
                                        }
                                    }
                                    else
                                    {
                                        message = $"Payment status for this  is not found RRR {rrr} is {result.Message}";
                                    }
                                }
                                catch (Exception ex)
                                {
                                    message = $"Remita Network {ex.Message}";
                                }
                            }
                        }
                        else
                        {
                            message = "Student Record is not found";
                        }
                    }
                    //message = "Feature not implemented yet";
                }
            }

            else
            {
                message = "Student Jamb No/Matric-No/PGA Form  or Remita Reference Number is empty";
            }
            ViewBag.Message = message;
            var feeCategory = from PaymentFeeCategory s in Enum.GetValues(typeof(PaymentFeeCategory))
                              select new { ID = s, Name = s.ToString() };
            ViewBag.FeeCategory = new SelectList(feeCategory, "Name", "Name");

            var sessions = await _db.Sessions.OrderByDescending(s => s.SessionName).ToListAsync();
            var sessionName = from s in sessions select new { Name = s.SessionName, Id = s.SessionId };
            ViewBag.Sessions = new SelectList(sessionName, "Id", "Name");

            return View();
        }

        public async Task<ActionResult> ValidateSessionalPayment(string studentId, int? Session, int? CorrectSession, string level)
        {
            var message = string.Empty;
            if (!string.IsNullOrEmpty(studentId))
            {
                studentId = studentId.ToUpper().Trim();
                var student = await _db.Students.Include(i => i.SchoolProgramme).AsNoTracking().Where(x => x.MatricNo.Trim().ToUpper().Equals(studentId) ||
                                x.JambRegNo.Trim().ToUpper().Equals(studentId)).FirstOrDefaultAsync();

                var applicant = await _db.Applicants.Where(i => i.ApplicantEmail.Trim().ToUpper().Equals(studentId.Trim().ToUpper())).FirstOrDefaultAsync();

                var paidSession = await _db.Sessions.Where(p => p.SessionId == Session).Select(p => p.SessionName).FirstOrDefaultAsync();
                var levelId = await _db.Levels.Where(l => l.LevelName == level).Select(l => l.LevelId).FirstOrDefaultAsync();

                if (student != null)
                {
                    var transactions = await _db.SchoolFeePayments.Include(i => i.Students).Include(i => i.Session).Include(i => i.Level)
                                .Where(x => x.StudentId.Trim().Equals(student.StudentId.Trim()) && x.Session.SessionName.Equals(paidSession) //set the session that was paid for
                                && (x.FeeCategory.Equals(SchoolFeeCategory.School_Charges.ToString()) || x.FeeCategory.Equals(SchoolFeeCategory.Acceptance.ToString())) && x.TotalAmount > 20000 && x.Status == true)
                                .FirstOrDefaultAsync();

                    //var transactions = _db.ApplicantPayments.Include(i => i.Session)
                    //               .Where(x => x.ApplicantEmail.Trim().ToUpper().Equals(applicant.ApplicantEmail.Trim().ToUpper()))
                    //               .FirstOrDefault();

                    if (transactions != null && CorrectSession != null) // for only change of session
                    {
                        transactions.SessionId = (int)CorrectSession;

                        _db.Entry(transactions).State = EntityState.Modified;
                        await _db.SaveChangesAsync();

                        message = $"Change of payment session for {student.FullName} is successful";
                    }

                    if (transactions != null && CorrectSession == null && !string.IsNullOrEmpty(level)) // for only change of level
                    {
                        transactions.LevelId = levelId;

                        _db.Entry(transactions).State = EntityState.Modified;
                        await _db.SaveChangesAsync();

                        message = $"Change of payment level for {student.FullName} is successful";
                    }

                    if (transactions != null && CorrectSession != null && !string.IsNullOrEmpty(level)) // for change of session and level
                    {
                        transactions.SessionId = (int)CorrectSession;
                        transactions.LevelId = levelId;

                        _db.Entry(transactions).State = EntityState.Modified;
                        await _db.SaveChangesAsync();

                        message = $"Change of payment session and level for {student.FullName} is successful";
                    }

                    else if (transactions == null)
                    {
                        message = $"No transaction for {paidSession} session found";
                    }
                }
                else
                {
                    message = "Student Record is not found";
                }
            }
            else
            {
                message = "Student Jamb No/Matric-No/PGA Form  is empty";
            }
            ViewBag.Message = message;
            var feeCategory = from PaymentFeeCategory s in Enum.GetValues(typeof(PaymentFeeCategory))
                              select new { ID = s, Name = s.ToString() };

            var sessions = await _db.Sessions.OrderByDescending(s => s.SessionName).ToListAsync();

            var session = from s in sessions select new { Name = s.SessionName, Id = s.SessionId };
            ViewBag.Session = new SelectList(session, "Id", "Name");
            ViewBag.CorrectSession = new SelectList(session, "Id", "Name");

            ViewBag.FeeCategory = new SelectList(feeCategory, "Name", "Name");
            return View();
        }

        [HttpGet]
        //[Authorize(Roles = RoleName.Admin + "," + RoleName.DatabaseAdmin + "," + RoleName.SuperAdmin + "," + RoleName.AdmissionUploadOfficer)]
        public ActionResult UpdateChangeOfCourseRRR(string message)
        {
            ViewBag.Message = message;
            return View();
        }

        //[Authorize(Roles = RoleName.Admin + "," + RoleName.DatabaseAdmin + "," + RoleName.SuperAdmin + "," + RoleName.AdmissionUploadOfficer)]
        public async Task<ActionResult> UpdateChangeOfCourseRRR(HttpPostedFileBase excelfile)
        {
            if (excelfile == null || excelfile.ContentLength == 0)
            {
                ViewBag.Error = "Please Select a excel file <br/>";
                ViewBag.Message = "You must select an excel file before you click Upload button";
                TempData["Title"] = "Error.";

                return View("Index");
            }
            HttpPostedFileBase file = Request.Files["excelfile"];
            if (excelfile.FileName.EndsWith("xls", StringComparison.CurrentCulture) || excelfile.FileName.EndsWith("xlsx", StringComparison.CurrentCulture))
            {
                string lastrecord = "";
                int recordCount = 0;
                string message = "";
                string fileContentType = file.ContentType;
                byte[] fileBytes = new byte[file.ContentLength];
                var data = file.InputStream.Read(fileBytes, 0, Convert.ToInt32(file.ContentLength));

                var utmeUploadError = new List<StudentUtmeUploadErrorVm>();
                bool hasUtmeUploadError = false;

                // Read data from excel file
                using (var package = new ExcelPackage(file.InputStream))
                {
                    ExcelValidation myExcel = new ExcelValidation();
                    var currentSheet = package.Workbook.Worksheets;
                    var workSheet = currentSheet.First();
                    var noOfCol = workSheet.Dimension.End.Column;
                    var noOfRow = workSheet.Dimension.End.Row;
                    int requiredField = 2;

                    string validCheck = myExcel.ValidateExcel(noOfRow, workSheet, requiredField);
                    if (!validCheck.Equals("Success"))
                    {
                        string[] ssizes = validCheck.Split(' ');
                        string[] myArray = new string[2];
                        for (int i = 0; i < ssizes.Length; i++)
                        {
                            myArray[i] = ssizes[i];
                        }
                        string lineError = $"Line/Row number {myArray[0]}  and column {myArray[1]} is not rightly formatted, Please Check for anomalies ";
                        ViewBag.Message = lineError;
                        return View("ErrorException");
                    }

                    for (int row = 2; row <= noOfRow; row++)
                    {
                        var matNo = workSheet.Cells[row, 1].Value.ToString().Trim();
                        var paymentRef = workSheet.Cells[row, 2].Value.ToString().Trim();

                        var student = await _db.Students.AsNoTracking()
                                            .Where(x => x.MatricNo.Trim().ToUpper().Equals(matNo.ToUpper()))
                                            .FirstOrDefaultAsync();


                        if (student != null)
                        {
                            //  var applicantfeepayment = await _db.AccommodationFeePayments.Where(x => x.StudentId.Equals(student.StudentId)
                            //&& x.SessionId.Equals(1)).FirstOrDefaultAsync();
                            var applicantfeepayment = await _db.SchoolFeePayments.Include(a => a.Students).AsNoTracking().Where(x => x.Students.Email.Equals(student.Email)
                                                            && x.SessionId.Equals(20)).FirstOrDefaultAsync();

                            if (applicantfeepayment != null)
                            {
                                applicantfeepayment.ReferenceNo = paymentRef;

                                _db.Entry(applicantfeepayment).State = EntityState.Modified;
                                await _db.SaveChangesAsync();
                                utmeUploadError.Add(new StudentUtmeUploadErrorVm { JambRegNo = student.JambRegNo, Row = row });
                                hasUtmeUploadError = true;
                            }
                        }
                        else
                        {
                            ViewBag.ErrorMessage = "Student not found";
                            return View("ErrorException");
                        }
                        recordCount++;
                        lastrecord = $"The last Updated record has the Surname  {student.LastName} and " +
                            $"First Name {student.FirstName} with  Reg No {student.MatricNo}";
                    }
                    try
                    {
                        await _db.SaveChangesAsync();

                    }
                    catch (Exception ex)
                    {
                        ViewBag.ErrorMessage = ex.Message;
                        return View("ErrorException");
                    }
                    if (hasUtmeUploadError)
                    {
                        ViewBag.ErrorInfo = $"Success!";
                        ViewBag.ErrorMessage = $"You have successfully Removed {recordCount} records...";
                        return View("ErrorException", utmeUploadError);
                    }
                    message = $"You have successfully Deleted {recordCount} records...  and {lastrecord}";
                    ViewBag.Message = message;
                    return RedirectToAction("Index", "Students", new { message });
                }
            }
            ViewBag.Error = $"File type is Incorrect <br/>";
            return View("Index");
        }

        [HttpPost]
        private async Task<ActionResult> GetSchoolFee()
        {
            #region Server Side filtering

            //Get parameter for sorting from grid table
            // get Start (paging start index) and length (page size for paging)
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            //Get Sort columns values when we click on Header Name of column
            //getting column name
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            //Soring direction(either desending or ascending)
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
            string search = Request.Form.GetValues("search[value]").FirstOrDefault();

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int totalRecords = 0;

            var schoolFee = await _feeQueryManager.GetSchoolFeePaymentList(SchoolFeeCategory.School_Charges.ToString(), sessionId);
            var v = schoolFee.Select(s => new
            {
                s.Students.MatricNo,
                s.Students.FullName,
                s.Students.Gender,
                s.ReferenceNo,
                Date = s.Date.ToString(),
                s.Students.Programme.ProgrammeName,
                s.Students.Level.LevelName,
                s.Students.PhoneNumber
            }).ToList();

            if (!string.IsNullOrEmpty(search))
            {
                v = v.Where(x => x.MatricNo.Equals(search) || x.FullName.Contains(search)
                            || x.ReferenceNo.Equals(search)).ToList();
            }

            totalRecords = v.Count();
            var data = v.Skip(skip).Take(pageSize).ToList();

            return Json(new { draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data },
                JsonRequestBehavior.AllowGet);

            #endregion Server Side filtering
        }

        public async Task<ActionResult> GetAcceptanceFee()
        {
            #region Server Side filtering

            //Get parameter for sorting from grid table
            // get Start (paging start index) and length (page size for paging)
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            //Get Sort columns values when we click on Header Name of column
            //getting column name
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            //Soring direction(either desending or ascending)
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
            string search = Request.Form.GetValues("search[value]").FirstOrDefault();

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int totalRecords = 0;
            var schoolFee = await _feeQueryManager.GetSchoolFeePaymentList(SchoolFeeCategory.Acceptance.ToString(), sessionId);

            var v = schoolFee.Select(s => new
            {
                s.Students.MatricNo,
                s.Students.FullName,
                s.Students.Gender,
                s.ReferenceNo,
                Date = s.Date.ToString(),
                s.Students.Programme.ProgrammeName,
                s.Students.Level.LevelName,
                s.Students.PhoneNumber
            }).ToList();

            if (!string.IsNullOrEmpty(search))
            {
                v = v.Where(x => x.MatricNo.Equals(search) || x.FullName.Contains(search)
                            || x.ReferenceNo.Equals(search)).ToList();
            }

            totalRecords = v.Count();
            var data = v.Skip(skip).Take(pageSize).ToList();

            return Json(new { draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data },
                JsonRequestBehavior.AllowGet);

            #endregion Server Side filtering
        }


        // GET: SchoolFeePayments/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var schoolFeePayment = await _db.SchoolFeePayments.FindAsync(id);
            if (schoolFeePayment == null)
            {
                return HttpNotFound();
            }
            return View(schoolFeePayment);
        }

        // GET: SchoolFeePayments/Create
        /// <summary>
        /// Fee selection page where we display current session,
        /// semester, fee category and and student Name
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> MakePayment(string message)
        {

            //var result = await ConfirmProbabtion();
            //if (result != null)
            //    return result;

            //var paymentSetting = await _feeQueryManager.GetPaymentSetting(sessionId);
            var student = await _db.Students.Include(i => i.SchoolProgramme).Include(i => i.Session).Include(i => i.Programme).Include(i => i.SchoolFeePayments).AsNoTracking()
                                .Where(x => x.Email.Trim().ToUpper().Equals(userId.Trim().ToUpper()))
                                .Select(s => new { s.SchoolProgrammeId, s.StudentStatus, s.Programme.ProgrammeId, s.Session.SessionId, s.Level.LevelName, s.SchoolFeePayments }).FirstOrDefaultAsync();

            string studentStatus = student.SessionId.Equals(sessionId) || student.StudentStatus.Equals(StudentStatus.New_Student.ToString())
                                         ? StudentStatus.New_Student.ToString()
                                         : StudentStatus.Returning.ToString();

            var feeSetting = await _feeQueryManager.GetPaymentSetting(sessionId, student.SchoolProgrammeId, studentStatus);

            var feeCategory = from SchoolFeeCategory s in Enum.GetValues(typeof(SchoolFeeCategory))
                              select new { ID = s, Name = s.ToString() };
            var feeCategory2 = from SchoolFeeCategory2 s in Enum.GetValues(typeof(SchoolFeeCategory2))
                               select new { ID = s, Name = s.ToString() };
            var schoolfeePaymentType = from SchoolFeePaymentType s in Enum.GetValues(typeof(SchoolFeePaymentType))
                                       select new { ID = s, Name = s.ToString() };
            if (feeSetting == null)
            {
                ViewBag.FeeCategory = new SelectList(feeCategory, "Name", "Name");
            }
            else if (feeSetting.AcceptPartPayment.Equals(true))
            {
                ViewBag.FeeCategory = new SelectList(feeCategory, "Name", "Name");
            }
            else
            {
                ViewBag.FeeCategory = new SelectList(feeCategory2, "Name", "Name");
            }
            if (student.StudentStatus.Equals(StudentStatus.New_Student.ToString()))
            {
                var undergraduateRule = await _query.GetUnderGraduateRule(student.ProgrammeId, student.SchoolProgrammeId);
                ViewBag.UndergraduateRule = undergraduateRule;
            }
            else
            {
                ViewBag.UndergraduateRule = null;
            }
            int currentSessionId = _query.GetCurrentSessionId(studentSchoolProgrammeId);
            ViewBag.SchoolFeePaymentType = new SelectList(schoolfeePaymentType, "Name", "Name");
            ViewBag.SemesterId = new SelectList(_query.GetCurrentSemesterList(studentSchoolProgrammeId), "SemesterId", "SemesterName");
            ViewBag.Sessions = new SelectList(_db.Sessions.AsNoTracking(), "SessionId", "SessionName", currentSessionId);
            ViewBag.hasPartPayment = student.SchoolFeePayments.Where(s => s.SessionId.Equals(sessionId) && s.Status == true && s.RemainingBalance > 0).Any();
            ViewBag.StudentId = await _query.GetUserFullName(userId);
            ViewBag.StudentStatus = studentStatus;
            ViewBag.Message = message;
            ViewBag.StudentLevel = student.LevelName;
            return View();
        }


        // GET: SchoolFeePayments/Create
        //public async Task<ActionResult> Create(SchoolFeePaymentVm model)
        //{
        //    if (model.FeeCategory.Equals(SchoolFeeCategory.Acceptance.ToString()) && _IsPayedAcceptance.Equals(true) ||
        //       model.FeeCategory.Equals(SchoolFeeCategory.School_Charges.ToString()) && _IsPayedSchoolFee.Equals(true))
        //    {
        //        return RedirectToAction("Index");
        //    }


        //    var student = await _db.Students.Include(i => i.SchoolProgramme).Include(i => i.Session).Include(i => i.Programme.Department.Faculty).Include(i => i.Level)
        //                        .AsNoTracking().FirstOrDefaultAsync(x => x.Email.Trim().ToUpper().Equals(userId.Trim().ToUpper())
        //                        && x.Active.Equals(true));

        //    /*---------------------------START: ADDED TO TEMPORARILY CLOSE UG PAYMENT ------------------------*/
        //    //if (student.SchoolProgramme.ProgrammeCategory.ToUpper() == ProgrammeCategory.UnderGraduate.ToString().ToUpper() && _query.GetCurrentSessionId(student.SchoolProgramme.SchoolProgrammeId) == model.SessionId)
        //    //{
        //    //    return RedirectToAction("MakePayment",
        //    //      new
        //    //      {
        //    //          message = $"Payments of school charges is closed till further notice."
        //    //      });
        //    //}
        //    /*---------------------------END: ADDED TO TEMPORARILY CLOSE UG PAYMENT ------------------------*/

        //    if ((student.Session.SessionId.Equals(model.SessionId) || student.StudentStatus.Equals(StudentStatus.New_Student.ToString()))
        //        && model.FeeCategory.Equals(SchoolFeeCategory.School_Charges.ToString()) && student.IsClearedFaculty.Equals(false)
        //        && student.SchoolProgramme.ProgrammeCategory.ToUpper() != ProgrammeCategory.Institute_Of_Education.ToString().ToUpper())
        //    {
        //        return RedirectToAction("MakePayment",
        //          new
        //          {
        //              message = $"Please complete your clearance at the Department and faculty before proceeding to paying school fee."
        //          });
        //    }

        //    var hasPayedList = await _db.SchoolFeePayments.AsNoTracking().Where(x => x.StudentId.Equals(student.StudentId) &&
        //                        x.SessionId.Equals(model.SessionId) && x.FeeCategory.Equals(model.FeeCategory))
        //                        .ToListAsync();

        //    var fullName = student.FullName;



        //    string studentStatus = student.Session.SessionId.Equals(model.SessionId) || student.StudentStatus.Equals(StudentStatus.New_Student.ToString())
        //                                ? StudentStatus.New_Student.ToString()
        //                                : StudentStatus.Returning.ToString();
        //    var paymentSetting = await _feeQueryManager.GetPaymentSetting(model.SessionId, student.SchoolProgramme.SchoolProgrammeId, studentStatus);
        //    if (paymentSetting == null)
        //    {
        //        return RedirectToAction("MakePayment",
        //            new
        //            {
        //                message = $"Payment Setting is not currently set in {model.FeeCategory} fee category for {student.SchoolProgramme.FancyName}. Please Report to the ICT Office for correction. Please try again..."
        //            });
        //    }

        //    var feeList = new List<FeeList>();

        //    //check spill-over student
        //    string fileName = "spillSTDS.txt";
        //    string path = Server.MapPath("~/" + fileName);
        //    Dictionary<string, StudentSpillVm> Spillresult = System.IO.File.ReadLines(path)
        //                                         .Select(line => line.Split(','))
        //                                         .ToDictionary(split => split[0],
        //                                                        split => new StudentSpillVm(split[0],
        //                                                                        decimal.Parse(split[1]),
        //                                                                        split[2]
        //                                                                        //split[3],
        //                                                                        //split[4]
        //                                                                        ));
        //    StudentSpillVm spillVm;

        //    if (student.StudentStatus.Equals(StudentStatus.Returning.ToString()) && Spillresult.TryGetValue(student.MatricNo, out spillVm))
        //    {
        //        feeList.Add(new FeeList { FeeTypeName = "10001", Amount = spillVm.Amount, Description = "School Charges" });
        //        feeList.Add(new FeeList { FeeTypeName = "0389", Amount = 10000, Description = "NUGA Charges (2 years)" });

        //        if ((await _feeQueryManager.GetDepartmentPaymentList(student.Programme.Department.DepartmentId, model.SessionId, (Int32)student.LevelId)).Count > 0)
        //        {
        //            feeList.Add(new FeeList { FeeTypeName = "0441", Amount = 12500, Description = "Lab,Studio and Workshop Charge" });
        //        }

        //    }
        //    else
        //    {

        //        feeList.AddRange(await _feeQueryManager.GetSchoolFeeList(model.FeeCategory, student, paymentSetting, model.SessionId));


        //        if (feeList.Count <= 0)
        //        {
        //            return RedirectToAction("MakePayment",
        //               new
        //               {
        //                   message = $"Payment is not currently set in {model.FeeCategory} fee category for {student.SchoolProgramme.FancyName} as {student.Indegine} at the moment, Please try again..."
        //               });
        //        }

        //        if (paymentSetting != null && paymentSetting.ConsiderDepartmentalFee && !model.IsPartPayment && model.FeeCategory.Equals(SchoolFeeCategory.School_Charges.ToString()))
        //        {
        //            //check for rogue programmes: int[] numbers = { 1, 2, 3, 4, 5 };
        //            int[] labProgrammes = { };
        //            if (labProgrammes.Contains((Int32)student.ProgrammeId))
        //            {
        //                feeList.AddRange(await _feeQueryManager.GetDepartmentPaymentList(80, model.SessionId, (Int32)student.LevelId));
        //            }
        //            else
        //            {
        //                feeList.AddRange(await _feeQueryManager.GetDepartmentPaymentList(student.Programme.Department.DepartmentId, model.SessionId, (Int32)student.LevelId));
        //            }
        //            //feeList.AddRange(await _feeQueryManager.GetDepartmentPaymentList(student.Programme.Department.DepartmentId, model.SessionId, (Int32)student.LevelId));

        //            //Add NUGA Cgarges
        //            feeList.Add(new FeeList { FeeTypeName = "0389", Amount = 10000, Description = "NUGA Charges (2 years)" });

        //        }

        //        //Add GST Charges to year 2 students
        //        if (student.Level.LevelName.Equals("200") && student.StudentStatus.Equals(StudentStatus.Returning.ToString()) && model.FeeCategory.Equals(SchoolFeeCategory.School_Charges.ToString()) && !student.Session.SessionId.Equals(model.SessionId))
        //        {
        //            feeList.Add(new FeeList { FeeTypeName = "1130", Amount = 5000, Description = "General Studies (GST) Charges" });
        //        }

        //        //---------- Add convocation charges to final year students ------------------------
        //        //var studentProgrammeDuration = _db.Programmes.Include(l => l.FinalLevel).Where(d => d.ProgrammeId.Equals((int)student.ProgrammeId)).FirstOrDefault();

        //        //var FinalLevelNumber = getLeveltNumber(studentProgrammeDuration.FinalLevel.LevelName);
        //        ////var nextLevelNumber = getLeveltNumber(nextLevel);

        //        //if (studentProgrammeDuration.FinalLevel.LevelId.Equals(student.LevelId))
        //        //{
        //        //    feeList.Add(new FeeList {
        //        //        FeeTypeName = "Convocation Charges for Grauating Students",
        //        //        Amount = 25000,
        //        //        Description = "Convocation Charges for Grauating Students" });
        //        //}


        //        feeList.AddRange(await _feeQueryManager.GetLatePaymentFeeList(model.SessionId, student.SchoolProgramme.SchoolProgrammeId, model.FeeCategory.ToUpper(), null));


        //    }


        //    //System.Threading.Thread.Sleep(1);
        //    long milliseconds = DateTime.Now.Ticks;
        //    var url = Url.Action("ConfrimPayment", "SchoolFeePayments", new { }, protocol: Request.Url.Scheme);
        //    string serviceTypeId = _feeQueryManager.GetServiceType(model.FeeCategory, student.SchoolProgramme.ProgrammeCategory);

        //    if (hasPayedList != null)
        //    {
        //        foreach (var hasPayed in hasPayedList)
        //        {
        //            if (hasPayed.Status.Equals(false))
        //            {
        //                serviceTypeId = _feeQueryManager.GetServiceType(hasPayed.FeeCategory, student.SchoolProgramme.ProgrammeCategory);

        //                var hashed = _query.HashRemitedValidate(hasPayed.OrderId, RemitaConfigParams.APIKEY, RemitaConfigParams.MERCHANTID);
        //                string checkurl = RemitaConfigParams.CHECKSTATUSURL + "/" + RemitaConfigParams.MERCHANTID + "/" + hasPayed.OrderId + "/" + hashed + "/" + "orderstatus.reg";
        //                string jsondata = new WebClient().DownloadString(checkurl);
        //                var result = JsonConvert.DeserializeObject<RemitaResponse>(jsondata);
        //                if (string.IsNullOrEmpty(result.Rrr))
        //                {
        //                    var entry = _db.Entry(hasPayed);
        //                    if (entry.State == EntityState.Detached)
        //                        _db.SchoolFeePayments.Attach(hasPayed);
        //                    _db.SchoolFeePayments.Remove(hasPayed);
        //                    await _db.SaveChangesAsync();
        //                }
        //                else
        //                {
        //                    return RedirectToAction("ConfrimPayment", new { orderID = hasPayed.OrderId });
        //                }
        //            }

        //        }

        //    }

        //    var session = _query.GetCurrentSession(student.SchoolProgrammeId);
        //    var semester = _query.GetCurrentSemester(student.SchoolProgrammeId);
        //    //var paymentSetting = await _feeQueryManager.GetPaymentSetting(sessionId);
        //    //var payingamount = _feeQueryManager.GetFeeAmount(model, feeList, paymentSetting);
        //    var payingamount = feeList.Sum(x => x.Amount);

        //    var confirmPaymentVm = new ConfirmPaymentVm
        //    {
        //        FeeLists = feeList,
        //        StudentName = fullName,
        //        StudentId = student.StudentId,
        //        FeeCategory = model.FeeCategory,
        //        TotalAmount = payingamount,
        //        SessionId = model.SessionId,
        //        SemesterId = model.SemesterId,
        //        SemesterName = await _db.Semesters.Where(x => x.SemesterId.Equals(model.SemesterId))
        //                        .Select(s => s.SemesterName).FirstOrDefaultAsync(),
        //        SessionName = await _db.Sessions.Where(x => x.SessionId.Equals(model.SessionId))
        //            .Select(s => s.SessionName).FirstOrDefaultAsync(),
        //        payerName = fullName,
        //        payerEmail = student.Email,
        //        payerPhone = student.PhoneNumber,
        //        amt = payingamount.ToString(),
        //        IsPartPayment = model.IsPartPayment
        //    };

        //    confirmPaymentVm.TotalAmount = payingamount;
        //    confirmPaymentVm.merchantId = RemitaConfigParams.MERCHANTID;
        //    confirmPaymentVm.orderId = $"{SchoolSetUp.CurrentSchoolName}SF{milliseconds}";
        //    confirmPaymentVm.responseurl = url;
        //    confirmPaymentVm.serviceTypeId = serviceTypeId;

        //    //if (model.SchoolFeePaymentType.ToUpper().Equals(SchoolFeePaymentType.Part_Payment.ToString().ToUpper()))
        //    //{
        //    //    confirmPaymentVm.IsPartPayment = true;
        //    //    confirmPaymentVm.PayingPercentage = paymentSetting?.FirstPaymentPercentage;
        //    //}
        //    //else
        //    //{
        //    //    confirmPaymentVm.IsPartPayment = false;
        //    //    confirmPaymentVm.PayingPercentage = paymentSetting?.FirstPaymentPercentage;
        //    //}


        //    if (confirmPaymentVm.TotalAmount < 10)
        //    {
        //        return RedirectToAction("MakePayment",
        //            new
        //            {
        //                message = $"Payment is not currently set in {model.FeeCategory} fee category for {student.SchoolProgramme.FancyName} as {student.Indegine} at the moment, Please try again..."
        //            });
        //    }

        //    return View(confirmPaymentVm);

        //}

        // GET: SchoolFeePayments/Create
        public async Task<ActionResult> Create(SchoolFeePaymentVm model)
        {
            if (!await IsPaymentAllowedAsync(model))
            {
                //return RedirectToAction("Index");
            }

            var student = await GetStudentAsync(userId);
            if (student == null)
            {
                // Handle case where student is not found
                return View("Error"); // Replace with appropriate error handling
            }

            //temporarilly block Nursing, Medicine and Law students
            //int[] labProgrammes = { 121, 123, 113 };
            //if (labProgrammes.Contains((Int32)student.ProgrammeId) && student.SessionId == 28)
            //{
            //    return RedirectToAction("MakePayment",
            //                      new
            //                      {
            //                          message = $"Please exercise patience as we look into issues regarding your school charges. visit ICT room 3 complexx II for clarification."
            //                      });
            //}

            //Added for UG students only
            //if (student.Session.SessionId.Equals(model.SessionId) && model.FeeCategory.Equals(SchoolFeeCategory.School_Charges.ToString()) && string.IsNullOrEmpty(student.ImeiNo)
            //    && student.SchoolProgramme.ProgrammeCategory.ToUpper() == ProgrammeCategory.UnderGraduate.ToString().ToUpper())
            //{
            //    return RedirectToAction("MakePayment",
            //      new
            //      {
            //          message = $"Please got to the VC's office to validate your admissoin letter before proceeding to paying school fee."
            //      });
            //}

            if ((student.Session.SessionId.Equals(model.SessionId) || student.StudentStatus.Equals(StudentStatus.New_Student.ToString()))
                && model.FeeCategory.Equals(SchoolFeeCategory.School_Charges.ToString()) && student.IsClearedFaculty.Equals(false)
                && student.SchoolProgramme.ProgrammeCategory.ToUpper() != ProgrammeCategory.Institute_Of_Education.ToString().ToUpper())
            {
                return RedirectToAction("MakePayment",
                  new
                  {
                      message = $"Please complete your clearance at the Department and faculty before proceeding to paying school fee."
                  });
            }

            var feeList = await GenerateFeeListAsync(student, model);
             if (feeList.Count == 0)
            {
                return RedirectToAction("MakePayment", new { message = "No fees found for the selected criteria." });
            }

            var hasUnfinishedPayments = await HasUnfinishedPaymentsAsync(student, model);
            if (hasUnfinishedPayments.Any())
            {
                //var hasPayedList = await _db.SchoolFeePayments.AsNoTracking().Where(x => x.StudentId.Equals(student.StudentId) &&
                //                x.SessionId.Equals(model.SessionId) && x.FeeCategory.Equals(model.FeeCategory))
                //                .ToListAsync();

                foreach (var hasPayed in hasUnfinishedPayments)
                {
                    if (hasPayed.Status.Equals(false))
                    {
                        string serviceTypeId = _feeQueryManager.GetServiceType(model.FeeCategory, student.SchoolProgramme.ProgrammeCategory);

                        var hashed = _query.HashRemitedValidate(hasPayed.OrderId, RemitaConfigParams.APIKEY, RemitaConfigParams.MERCHANTID);
                        string checkurl = RemitaConfigParams.CHECKSTATUSURL + "/" + RemitaConfigParams.MERCHANTID + "/" + hasPayed.OrderId + "/" + hashed + "/" + "orderstatus.reg";
                        string jsondata = new WebClient().DownloadString(checkurl);
                        var result = JsonConvert.DeserializeObject<RemitaResponse>(jsondata);
                        if (string.IsNullOrEmpty(result.Rrr))
                        {
                            var entry = _db.Entry(hasPayed);
                            if (entry.State == EntityState.Detached)
                                _db.SchoolFeePayments.Attach(hasPayed);
                            _db.SchoolFeePayments.Remove(hasPayed);
                            await _db.SaveChangesAsync();
                        }
                        else
                        {
                            return RedirectToAction("ConfrimPayment", new { orderID = hasPayed.OrderId });
                        }
                    }

                }
                //return RedirectToAction("ConfirmPayment", new { orderID = hasPayed.OrderId });
            }

            var existingPayment = await _db.SchoolFeePayments
                                   .Where(x => x.StudentId == student.StudentId
                                               && x.SessionId == model.SessionId
                                               //&& x.SemesterId == model.SemesterId
                                               && x.FeeCategory.Equals(model.FeeCategory)
                                               && x.Status) // Assuming 'Status' as true indicates a successful payment
                                   .OrderByDescending(x => x.Date)
                                   .FirstOrDefaultAsync();

            decimal balanceAmount = 0;
            bool isBalancePayment = false;

            if (existingPayment != null && existingPayment.RemainingBalance > 0)
            {
                // Successful part-payment exists, calculate the balance amount
                balanceAmount = existingPayment.RemainingBalance;
            }

            var confirmPaymentVm = PrepareConfirmPaymentVm(student, model, feeList, balanceAmount);
            return View(confirmPaymentVm);
        }

        private async Task<bool> IsPaymentAllowedAsync(SchoolFeePaymentVm model)
        {
            // Logic to check if payment is allowed
            // Return true if allowed, false otherwise
            // For example:
            return !(model.FeeCategory.Equals(SchoolFeeCategory.Acceptance.ToString()) && _IsPayedAcceptance) &&
                   !(model.FeeCategory.Equals(SchoolFeeCategory.School_Charges.ToString()) && _IsPayedSchoolFee);
        }

        private async Task<Student> GetStudentAsync(string userId)
        {
            // Fetch student information based on userId
            return await _db.Students.Include(s => s.SchoolProgramme)
                                     .Include(s => s.Session)
                                     .Include(s => s.Programme.Department.Faculty)
                                     .Include(s => s.Level)
                                     .AsNoTracking()
                                     .FirstOrDefaultAsync(x => x.Email.Trim().ToUpper().Equals(userId.Trim().ToUpper()) && x.Active);
        }

        private async Task<List<FeeList>> GenerateFeeListAsync(Student student, SchoolFeePaymentVm model)
        {
            var feeList = new List<FeeList>();
            string studentStatus = student.Session.SessionId.Equals(model.SessionId) || student.StudentStatus.Equals(StudentStatus.New_Student.ToString())
                                        ? StudentStatus.New_Student.ToString()
                                        : StudentStatus.Returning.ToString();
            var paymentSetting = await _feeQueryManager.GetPaymentSetting(model.SessionId, student.SchoolProgramme.SchoolProgrammeId, studentStatus);


            // Chech spill-over student logic and Create 50% of SchFee
            var (spillVm, isSpillOver) = await IsSpillOverStudent(student);
            if (isSpillOver)
            {
                // Add specific fees for spill-over student
                feeList.Add(new FeeList { FeeTypeName = "10001", Amount = spillVm.Amount, Description = "School Charges" });
                feeList.Add(new FeeList { FeeTypeName = "0389", Amount = 10000, Description = "NUGA Charges (2 years)" });

                if ((await _feeQueryManager.GetDepartmentPaymentList(student.Programme.Department.DepartmentId, model.SessionId, (Int32)student.LevelId)).Count > 0)
                {
                    feeList.Add(new FeeList { FeeTypeName = "0441", Amount = 12500, Description = "Lab,Studio and Workshop Charge" });
                }
            }
            else
            {
                // Standard fee list generation
                feeList.AddRange(await _feeQueryManager.GetSchoolFeeList(model.FeeCategory, student, paymentSetting, model.SessionId));
                if (feeList.Count <= 0)
                {
                    return feeList;
                }

                //Check all settings including partPayment
                if (paymentSetting != null && paymentSetting.ConsiderDepartmentalFee && !model.IsPartPayment && model.FeeCategory.Equals(SchoolFeeCategory.School_Charges.ToString()))
                {
                    //check for rogue programmes: int[] numbers = { 1, 2, 3, 4, 5 };
                    int[] labProgrammes = {28 };
                    if (labProgrammes.Contains((Int32)student.ProgrammeId))
                    {
                        //feeList.AddRange(await _feeQueryManager.GetDepartmentPaymentList(80, model.SessionId, (Int32)student.LevelId));
                    }
                    else
                    {
                        feeList.AddRange(await _feeQueryManager.GetDepartmentPaymentList(student.Programme.Department.DepartmentId, model.SessionId, (Int32)student.LevelId));
                    }

                    //Add NUGA Cgarges
                    feeList.Add(new FeeList { FeeTypeName = "0389", Amount = 10000, Description = "NUGA Charges (2 years)" });

                }
                //Add GST Charges to year 2 students
                if (student.Level.LevelName.Equals("200") && student.StudentStatus.Equals(StudentStatus.Returning.ToString())
                    && model.FeeCategory.Equals(SchoolFeeCategory.School_Charges.ToString()) && !student.Session.SessionId.Equals(model.SessionId))
                {
                    feeList.Add(new FeeList { FeeTypeName = "1130", Amount = 5000, Description = "General Studies (GST) Charges" });
                }

            }

            // Add logic for other charges like NUGA, GST, etc.
            return feeList;
        }

        //private bool IsSpillOverStudent(Student student)
        public async Task<(StudentSpillVm spillVm, bool isSpillOver)> IsSpillOverStudent(Student student)
        {
            // Implement logic to determine if the student is a spill-over student
            //check spill-over student
            string fileName = "spillSTDS.txt";
            string path = Server.MapPath("~/" + fileName);
            Dictionary<string, StudentSpillVm> Spillresult = System.IO.File.ReadLines(path)
                                                 .Select(line => line.Split(','))
                                                 .ToDictionary(split => split[0],
                                                                split => new StudentSpillVm(split[0],
                                                                                decimal.Parse(split[1]),
                                                                                split[2]
                                                                                //split[3],
                                                                                //split[4]
                                                                                ));

            StudentSpillVm spillVm;

            if (student.StudentStatus.Equals(StudentStatus.Returning.ToString()) && Spillresult.TryGetValue(student.MatricNo, out spillVm))
            {
                return (spillVm, true);
            }
            else
            {
                return (null, false);
            }

        }

        public async Task<List<SchoolFeePayment>> HasUnfinishedPaymentsAsync(Student student, SchoolFeePaymentVm model)
        {
            // Assuming SchoolFeePayments is a DbSet in your DbContext
            // and it contains a property `Status` to indicate if the payment is finished
            var hasUnfinishedPayments = await _db.SchoolFeePayments
                .Where(x => x.StudentId == student.StudentId
                            && x.SessionId == model.SessionId
                            && x.FeeCategory == model.FeeCategory
                            && x.Status == false).ToListAsync(); // Assuming false indicates an unfinished payment
                                                                 //.AnyAsync();

            return hasUnfinishedPayments;
        }

        private ConfirmPaymentVm PrepareConfirmPaymentVm(Student student, SchoolFeePaymentVm model, List<FeeList> feeList, decimal balanceAmont)
        {
            // Calculate the total amount from the fee list
            var totalAmount = feeList.Sum(f => f.Amount);
            long milliseconds = DateTime.Now.Ticks;
            var url = Url.Action("ConfrimPayment", "SchoolFeePayments", new { }, protocol: Request.Url.Scheme);
            string serviceTypeId = _feeQueryManager.GetServiceType(model.FeeCategory, student.SchoolProgramme.ProgrammeCategory);

            // Create a new instance of ConfirmPaymentVm and populate it
            var confirmPaymentVm = new ConfirmPaymentVm
            {
                FeeLists = feeList,
                StudentName = student.FullName, // Assuming FullName is a property of Student
                StudentId = student.StudentId,
                FeeCategory = model.FeeCategory,
                TotalAmount = balanceAmont > 0 ? balanceAmont : totalAmount,
                SessionId = model.SessionId,
                SemesterId = model.SemesterId,
                SemesterName = _query.GetCurrentSemesterName(semesterId), // Populate this if necessary
                SessionName = _query.GetSessionName(model.SessionId), // Populate this if necessary
                payerName = student.FullName,
                payerEmail = student.Email, // Assuming Email is a property of Student
                payerPhone = student.PhoneNumber, // Assuming PhoneNumber is a property of Student
                amt = balanceAmont > 0 ? balanceAmont.ToString() : totalAmount.ToString(),
                IsPartPayment = model.IsPartPayment,
                isBalancePayment = balanceAmont > 0 ? true : model.IsPartPayment
            };

            // Additional properties like MerchantId, OrderId, ResponseUrl, and ServiceTypeId
            // need to be set according to your application's logic and configuration.
            //confirmPaymentVm.TotalAmount = totalAmount;
            confirmPaymentVm.merchantId = RemitaConfigParams.MERCHANTID;
            confirmPaymentVm.orderId = $"{SchoolSetUp.CurrentSchoolName}SF{milliseconds}";
            confirmPaymentVm.responseurl = url;
            confirmPaymentVm.serviceTypeId = serviceTypeId;

            // Return the populated view model
            return confirmPaymentVm;
        }

        // Helper Function: Retrieve Payment Settings
        private async Task<PaymentSetting> GetPaymentSettingAsync(int sessionId, int schoolProgrammeId, string studentStatus)
        {
            return await _feeQueryManager.GetPaymentSetting(sessionId, schoolProgrammeId, studentStatus);
        }

        // Helper Function: Generate Fee List
        private async Task<List<FeeList>> GenerateFeeListAsync(Student student, ConfirmPaymentVm model, PaymentSetting paymentSetting)
        {
            // Implementation to generate the fee list based on student status, payment settings, etc.
            var feeList = new List<FeeList>();
            string studentStatus = student.Session.SessionId.Equals(model.SessionId) || student.StudentStatus.Equals(StudentStatus.New_Student.ToString())
                                        ? StudentStatus.New_Student.ToString()
                                        : StudentStatus.Returning.ToString();
            //var paymentSetting = await _feeQueryManager.GetPaymentSetting(model.SessionId, student.SchoolProgramme.SchoolProgrammeId, studentStatus);


            // Add spill-over student logic
            var (spillVm, isSpillOver) = await IsSpillOverStudent(student);
            if (isSpillOver)
            {
                // Add specific fees for spill-over student
                feeList.Add(new FeeList { FeeTypeName = "10001", Amount = spillVm.Amount, Description = "School Charges" });
                feeList.Add(new FeeList { FeeTypeName = "0389", Amount = 10000, Description = "NUGA Charges (2 years)" });

                if ((await _feeQueryManager.GetDepartmentPaymentList(student.Programme.Department.DepartmentId, model.SessionId, (Int32)student.LevelId)).Count > 0)
                {
                    feeList.Add(new FeeList { FeeTypeName = "0441", Amount = 12500, Description = "Lab,Studio and Workshop Charge" });
                }
            }
            else
            {
                // Standard fee list generation
                feeList.AddRange(await _feeQueryManager.GetSchoolFeeList(model.FeeCategory, student, paymentSetting, model.SessionId));
                if (feeList.Count <= 0)
                {
                    return feeList;
                }

                //Check all settings including partPayment
                if (paymentSetting != null && paymentSetting.ConsiderDepartmentalFee && !model.IsPartPayment && model.FeeCategory.Equals(SchoolFeeCategory.School_Charges.ToString()))
                {
                    //check for rogue programmes: int[] numbers = { 1, 2, 3, 4, 5 };
                    int[] labProgrammes = { 28 };
                    if (labProgrammes.Contains((Int32)student.ProgrammeId))
                    {
                        //feeList.AddRange(await _feeQueryManager.GetDepartmentPaymentList(80, model.SessionId, (Int32)student.LevelId));
                    }
                    else
                    {
                        feeList.AddRange(await _feeQueryManager.GetDepartmentPaymentList(student.Programme.Department.DepartmentId, model.SessionId, (Int32)student.LevelId));
                    }

                    //Add NUGA Cgarges
                    feeList.Add(new FeeList { FeeTypeName = "0389", Amount = 10000, Description = "NUGA Charges (2 years)" });

                }
                //Add GST Charges to year 2 students
                if (student.Level.LevelName.Equals("200") && student.StudentStatus.Equals(StudentStatus.Returning.ToString())
                    && model.FeeCategory.Equals(SchoolFeeCategory.School_Charges.ToString()) && !student.Session.SessionId.Equals(model.SessionId))
                {
                    feeList.Add(new FeeList { FeeTypeName = "1130", Amount = 5000, Description = "General Studies (GST) Charges" });
                }
                return feeList;
            }
            return feeList;
        }

        // Helper Function: Create Payment Log and Details in DB
        private async Task CreatePaymentLogAndDetailsAsync(Student student, ConfirmPaymentVm model, List<FeeList> feeList, PaymentSetting paymentSetting)
        {
            if (model.isBalancePayment)
            {
                // Implementation to create payment logs and details
                var schoolFeePayment = new SchoolFeePayment
                {
                    OrderId = model.orderId,
                    FeeCategory = model.FeeCategory,
                    Date = DateTime.Now,
                    SemesterId = semesterId,
                    SessionId = model.SessionId,
                    StudentId = model.StudentId,
                    PaidFee = model.TotalAmount,
                    TotalAmount = model.TotalAmount,
                    PaymentMode = model.PaymentMode,
                    IsPartPaymet = model.isBalancePayment,
                    LevelId = student.LevelId
                };
                _db.SchoolFeePayments.Add(schoolFeePayment);
                var log = new RemitaPaymentLog
                {
                    OrderId = model.orderId,
                    PaymentName = model.FeeCategory,
                    PaymentDate = DateTime.Now,
                    Amount = model.TotalAmount.ToString(),
                    PayerName = model.StudentName

                };
                _db.RemitaPaymentLogs.Add(log);

                foreach (var fee in feeList)
                {
                    var studentFeeDetails = new StudentPaymentDetail()
                    {
                        StudentId = student.StudentId,
                        SessionId = model.SessionId,
                        FeeTypeName = fee.FeeTypeName,
                        Amount = fee.Amount,
                        Description = fee.Description
                    };
                    _db.StudentPaymentDetails.Add(studentFeeDetails);

                    var stdPayDet = await _db.StudentPaymentDetails.Where(x => x.StudentId.Equals(student.StudentId)).FirstOrDefaultAsync();

                }
            }
            else
            {
                var schoolFeePayment = new SchoolFeePayment
                {
                    OrderId = model.orderId,
                    FeeCategory = model.FeeCategory,
                    Date = DateTime.Now,
                    SemesterId = semesterId,
                    SessionId = model.SessionId,
                    StudentId = model.StudentId,
                    PaidFee = feeList.Sum(s => s.Amount),
                    TotalAmount = model.IsPartPayment ? await getTotalAmount(paymentSetting, student, feeList, model.SessionId, model.FeeCategory.ToString()) : feeList.Sum(s => s.Amount),
                    PaymentMode = model.PaymentMode,
                    IsPartPaymet = model.IsPartPayment,
                    LevelId = student.LevelId
                };
                _db.SchoolFeePayments.Add(schoolFeePayment);
                var log = new RemitaPaymentLog
                {
                    OrderId = model.orderId,
                    PaymentName = model.FeeCategory,
                    PaymentDate = DateTime.Now,
                    Amount = feeList.Sum(s => s.Amount).ToString(),
                    PayerName = model.StudentName

                };
                _db.RemitaPaymentLogs.Add(log);

                foreach (var fee in feeList)
                {
                    var studentFeeDetails = new StudentPaymentDetail()
                    {
                        StudentId = student.StudentId,
                        SessionId = model.SessionId,
                        FeeTypeName = fee.FeeTypeName,
                        Amount = fee.Amount,
                        Description = fee.Description
                    };
                    _db.StudentPaymentDetails.Add(studentFeeDetails);

                    var stdPayDet = await _db.StudentPaymentDetails.Where(x => x.StudentId.Equals(student.StudentId)).FirstOrDefaultAsync();
                    //_db.Entry(stdPayDet).State = EntityState.Deleted;

                }
            }
            // Implementation to create payment logs and details

            await _db.SaveChangesAsync();
        }

        //public async Task<decimal> getTotalAmount(PaymentSetting paymentSetting, Student student, List<FeeList> feeList, int sessionId, String feeType)
        //{

        //    int[] labProgrammes = { };
        //    if (labProgrammes.Contains((Int32)student.ProgrammeId))
        //    {
        //        feeList.AddRange(await _feeQueryManager.GetDepartmentPaymentList(80, sessionId, (Int32)student.LevelId));
        //    }
        //    else
        //    {
        //        feeList.AddRange(await _feeQueryManager.GetDepartmentPaymentList(student.Programme.Department.DepartmentId, sessionId, (Int32)student.LevelId));
        //    }

        //    //Add NUGA Cgarges
        //    feeList.Add(new FeeList { FeeTypeName = "0389", Amount = 10000, Description = "NUGA Charges (2 years)" });


        //    //Add GST Charges to year 2 students
        //    if (student.Level.LevelName.Equals("200") && student.StudentStatus.Equals(StudentStatus.Returning.ToString())
        //        && feeType.Equals(SchoolFeeCategory.School_Charges.ToString()) && !student.Session.SessionId.Equals(sessionId))
        //    {
        //        feeList.Add(new FeeList { FeeTypeName = "1130", Amount = 5000, Description = "General Studies (GST) Charges" });
        //    }
        //    return feeList.Sum(s => s.Amount);
        //}

        //// POST: SchoolFeePayments/Create
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public async Task<ActionResult> Create(ConfirmPaymentVm model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var student = await GetStudentAsync(userId);
            if (student == null)
            {
                // Handle case where student is not found
                return View("Error"); // Replace with appropriate error handling
            }

            string studentStatus = student.Session.SessionId == model.SessionId || student.StudentStatus == StudentStatus.New_Student.ToString()
                                    ? StudentStatus.New_Student.ToString()
                                    : StudentStatus.Returning.ToString();

            var paymentSetting = await GetPaymentSettingAsync(model.SessionId, student.SchoolProgrammeId, studentStatus);
            if (paymentSetting == null)
            {
                // Handle payment setting not available
                return RedirectToAction("MakePayment",
                    new
                    {
                        message = $"Payment Setting is not currently set in {model.FeeCategory} fee category for {student.SchoolProgramme.FancyName}. Please Report to the ICT Office for correction. Please try again..."
                    });
            }

            var feeList = await GenerateFeeListAsync(student, model, paymentSetting);
            if (feeList.Count <= 0)
            {
                // Handle empty fee list
                return RedirectToAction("MakePayment",
                       new
                       {
                           message = $"Payment is not currently set in {model.FeeCategory} fee category for {student.SchoolProgramme.FancyName} as {student.Indegine} at the moment, Please try again..."
                       });
            }

            await CreatePaymentLogAndDetailsAsync(student, model, feeList, paymentSetting);

            //redirect to paystack for WASH programmes
            if (student.SchoolProgramme.SchoolProgrammeCode.Equals("WH"))
            {
                return RedirectToAction("SubmitPaystack", model);
            }

            // Continue with redirecting to Remita or other necessary steps
            model.hash = _query.HashRemitaRequest(model.merchantId, model.serviceTypeId, model.orderId, model.amt, model.responseurl, RemitaConfigParams.APIKEY);
            return RedirectToAction("SubmitRemita", model);
            //return RedirectToAction("SubmitPaystack", model);  //used to test remita api

        }


        public int getLeveltNumber(string level)
        {
            char result = level[0];
            int finalResult = (Int32)result;
            return finalResult;
        }

        //public ActionResult changeLevel()
        //{
        //    var payment = _db.SchoolFeePayments.Include(x => x.Students).Include(x => x.Level)
        //                                        .Where(x => x.ReferenceNo.Equals("140431542853") && x.Students.JambRegNo.Equals("36281250EH")).FirstOrDefault();
        //    if (payment != null)
        //    {
        //        payment.LevelId = 4;
        //    }

        //    return View("Sucess");
        //}



        // POST: SchoolFeePayments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //[ValidateInput(false)]
        //public async Task<ActionResult> Create(ConfirmPaymentVm model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        model.paymenttype = model.RemitaPaymentType.ToString().Replace("_", " ").ToLower();
        //        var student = await _db.Students.Include(i => i.SchoolProgramme).Include(i => i.Level)
        //            .Include(i => i.Session).Include(i => i.Programme.Department.Faculty)
        //                        .AsNoTracking().FirstOrDefaultAsync(x => x.Email.Trim().ToUpper().Equals(userId.Trim().ToUpper())
        //                        && x.Active.Equals(true));


        //        model.serviceTypeId = _feeQueryManager.GetServiceType(model.FeeCategory, student.SchoolProgramme.ProgrammeCategory);

        //        if (string.IsNullOrEmpty(model.payerEmail))
        //        {
        //            model.payerEmail = $"{model.payerName}@unijos.edu.ng";
        //        }
        //        if (string.IsNullOrEmpty(model.payerPhone))
        //        {
        //            model.payerPhone = "070300000000";
        //        }



        //        string indigenestatus = string.Empty;
        //        if (student.NationalityStatus)
        //        {
        //            indigenestatus = "Indigene student";
        //        }
        //        else
        //        {
        //            indigenestatus = "foreign student";
        //        }

        //        string studentStatus = student.Session.SessionId.Equals(model.SessionId) || student.StudentStatus.Equals(StudentStatus.New_Student.ToString())
        //                                 ? StudentStatus.New_Student.ToString()
        //                                 : StudentStatus.Returning.ToString();

        //        var feeList = new List<FeeList>();

        //        var paymentSetting = await _feeQueryManager.GetPaymentSetting(model.SessionId, student.SchoolProgramme.SchoolProgrammeId, studentStatus);

        //        //-------------------------BEGIN check spill-over student ----------
        //        string fileName = "spillSTDS.txt";
        //        string path = Server.MapPath("~/" + fileName);
        //        Dictionary<string, StudentSpillVm> result = System.IO.File.ReadLines(path)
        //                                             .Select(line => line.Split(','))
        //                                             .ToDictionary(split => split[0],
        //                                                            split => new StudentSpillVm(split[0],
        //                                                                            decimal.Parse(split[1]),
        //                                                                            split[2]
        //                                                                            //split[3],
        //                                                                            //split[4]
        //                                                                            ));
        //        StudentSpillVm spillVm;

        //        if (student.StudentStatus.Equals(StudentStatus.Returning.ToString()) && result.TryGetValue(student.MatricNo, out spillVm))
        //        {
        //            feeList.Add(new FeeList { FeeTypeName = "10001", Amount = spillVm.Amount, Description = "School Charges" });
        //            feeList.Add(new FeeList { FeeTypeName = "0389", Amount = 10000, Description = "NUGA Charges (2 years)" });
        //            feeList.Add(new FeeList { FeeTypeName = "0441", Amount = 12500, Description = "Lab,Studio and Workshop Charge" });

        //            var schoolFeePayment = new SchoolFeePayment
        //            {
        //                OrderId = model.orderId,
        //                FeeCategory = model.FeeCategory,
        //                Date = DateTime.Now,
        //                SemesterId = semesterId,
        //                SessionId = model.SessionId,
        //                StudentId = model.StudentId,
        //                PaidFee = feeList.Sum(s => s.Amount),
        //                TotalAmount = feeList.Sum(s => s.Amount),
        //                PaymentMode = model.PaymentMode,
        //                IsPartPaymet = model.IsPartPayment,
        //                LevelId = student.LevelId
        //            };
        //            _db.SchoolFeePayments.Add(schoolFeePayment);
        //            var log = new RemitaPaymentLog
        //            {
        //                OrderId = model.orderId,
        //                PaymentName = model.FeeCategory,
        //                PaymentDate = DateTime.Now,
        //                Amount = feeList.Sum(s => s.Amount).ToString(),
        //                PayerName = model.StudentName

        //            };
        //            _db.RemitaPaymentLogs.Add(log);

        //            foreach (var fee in feeList)
        //            {
        //                var studentFeeDetails = new StudentPaymentDetail()
        //                {
        //                    StudentId = student.StudentId,
        //                    SessionId = model.SessionId,
        //                    FeeTypeName = fee.FeeTypeName,
        //                    Amount = fee.Amount,
        //                    Description = fee.Description
        //                };
        //                _db.StudentPaymentDetails.Add(studentFeeDetails);

        //                var stdPayDet = await _db.StudentPaymentDetails.Where(x => x.StudentId.Equals(student.StudentId)).FirstOrDefaultAsync();


        //            }
        //        }
        //        else
        //        {
        //            feeList.AddRange(await _feeQueryManager.GetSchoolFeeList(model.FeeCategory, student, paymentSetting, model.SessionId));

        //            if (feeList.Count <= 0)
        //            {
        //                return RedirectToAction("MakePayment",
        //                   new
        //                   {
        //                       message = $"Payment is not currently set in {model.FeeCategory} fee category for {student.SchoolProgramme.FancyName} as {indigenestatus} at the moment, Please try again..."
        //                   });
        //            }

        //            if (paymentSetting != null && paymentSetting.ConsiderDepartmentalFee && !model.IsPartPayment && model.FeeCategory.Equals(SchoolFeeCategory.School_Charges.ToString()))
        //            {
        //                //check for roggue programmes: int[] numbers = { 1, 2, 3, 4, 5 };
        //                int[] labProgrammes = { };
        //                if (labProgrammes.Contains((Int32)student.ProgrammeId))
        //                {
        //                    feeList.AddRange(await _feeQueryManager.GetDepartmentPaymentList(80, model.SessionId, (Int32)student.LevelId));
        //                }
        //                else
        //                {
        //                    feeList.AddRange(await _feeQueryManager.GetDepartmentPaymentList(student.Programme.Department.DepartmentId, model.SessionId, (Int32)student.LevelId));
        //                }

        //                //Add NUGA Charges
        //                feeList.Add(new FeeList { FeeTypeName = "0389", Amount = 10000, Description = "NUGA Charges (2 years)" });
        //            }

        //            //Add GST Charges to year 2 students
        //            if (student.Level.LevelName.Equals("200") && student.StudentStatus.Equals(StudentStatus.Returning.ToString())
        //                && model.FeeCategory.Equals(SchoolFeeCategory.School_Charges.ToString()) && !student.Session.SessionId.Equals(model.SessionId))
        //            {
        //                feeList.Add(new FeeList { FeeTypeName = "1130", Amount = 5000, Description = "General Studies (GST) Charges" });
        //            }


        //            feeList.AddRange(await _feeQueryManager.GetLatePaymentFeeList(model.SessionId, student.SchoolProgramme.SchoolProgrammeId, model.FeeCategory.ToUpper(), null));

        //            var schoolFeePayment = new SchoolFeePayment
        //            {
        //                OrderId = model.orderId,
        //                FeeCategory = model.FeeCategory,
        //                Date = DateTime.Now,
        //                SemesterId = semesterId,
        //                SessionId = model.SessionId,
        //                StudentId = model.StudentId,
        //                PaidFee = feeList.Sum(s => s.Amount),
        //                TotalAmount = model.IsPartPayment ? await getTotalAmount(paymentSetting, student, feeList, model.SessionId, model.FeeCategory.ToString()) : feeList.Sum(s => s.Amount),
        //                PaymentMode = model.PaymentMode,
        //                IsPartPaymet = model.IsPartPayment,
        //                LevelId = student.LevelId
        //            };
        //            _db.SchoolFeePayments.Add(schoolFeePayment);
        //            var log = new RemitaPaymentLog
        //            {
        //                OrderId = model.orderId,
        //                PaymentName = model.FeeCategory,
        //                PaymentDate = DateTime.Now,
        //                Amount = feeList.Sum(s => s.Amount).ToString(),
        //                PayerName = model.StudentName

        //            };
        //            _db.RemitaPaymentLogs.Add(log);

        //            foreach (var fee in feeList)
        //            {
        //                var studentFeeDetails = new StudentPaymentDetail()
        //                {
        //                    StudentId = student.StudentId,
        //                    SessionId = model.SessionId,
        //                    FeeTypeName = fee.FeeTypeName,
        //                    Amount = fee.Amount,
        //                    Description = fee.Description
        //                };
        //                _db.StudentPaymentDetails.Add(studentFeeDetails);

        //                var stdPayDet = await _db.StudentPaymentDetails.Where(x => x.StudentId.Equals(student.StudentId)).FirstOrDefaultAsync();
        //                //_db.Entry(stdPayDet).State = EntityState.Deleted;

        //            }
        //        }

        //        await _db.SaveChangesAsync();
        //        model.hash = _query.HashRemitaRequest(model.merchantId, model.serviceTypeId, model.orderId, model.amt, model.responseurl, RemitaConfigParams.APIKEY);
        //        return RedirectToAction("SubmitRemita", model);
        //    }
        //    return View(model);
        //}

        public async Task<Decimal> getTotalAmount(PaymentSetting paymentSetting, Student student, List<FeeList> feeList, int sessionId, String feeType)
        {

            int[] labProgrammes = { 28 };
            if (labProgrammes.Contains((Int32)student.ProgrammeId))
            {
                //feeList.AddRange(await _feeQueryManager.GetDepartmentPaymentList(80, sessionId, (Int32)student.LevelId));
            }
            else
            {
                feeList.AddRange(await _feeQueryManager.GetDepartmentPaymentList(student.Programme.Department.DepartmentId, sessionId, (Int32)student.LevelId));
            }

            //Add NUGA Cgarges
            feeList.Add(new FeeList { FeeTypeName = "0389", Amount = 10000, Description = "NUGA Charges (2 years)" });


            //Add GST Charges to year 2 students
            if (student.Level.LevelName.Equals("200") && student.StudentStatus.Equals(StudentStatus.Returning.ToString())
                && feeType.Equals(SchoolFeeCategory.School_Charges.ToString()) && !student.Session.SessionId.Equals(sessionId))
            {
                feeList.Add(new FeeList { FeeTypeName = "1130", Amount = 5000, Description = "General Studies (GST) Charges" });
            }
            return feeList.Sum(s => s.Amount);
        }

        [AllowAnonymous]
        [ValidateInput(false)]
        public ActionResult SubmitRemita(ConfirmPaymentVm model)
        {
            return View(model);
        }


        [AllowAnonymous]
        [ValidateInput(false)]
        public ActionResult SubmitPaystack(ConfirmPaymentVm model)
        {
            return View(model);
        }


        [AllowAnonymous]
        public ActionResult RetrySchoolFeePayment(string rrr)
        {
            try
            {
                var hashrrr = _query.HashRrrQuery(rrr, RemitaConfigParams.APIKEY, RemitaConfigParams.MERCHANTID);
                string posturl = RemitaConfigParams.CHECKSTATUSURL + "/" + RemitaConfigParams.MERCHANTID + "/" + rrr + "/" + hashrrr + "/" + "status.reg";
                string jsondata = new WebClient().DownloadString(posturl);
                var result = JsonConvert.DeserializeObject<RemitaResponse>(jsondata);
                if (result.Status.Equals("00") || result.Status.Equals("01"))
                {
                    return RedirectToAction("ConfrimPayment", "SchoolFeePayments", new { RRR = result.Rrr, orderID = result.OrderId });
                }
                var url = Url.Action("ConfrimPayment", "SchoolFeePayments", new { }, protocol: Request.Url.Scheme);
                var hash = _query.HashRemitedRePost(RemitaConfigParams.MERCHANTID, rrr, RemitaConfigParams.APIKEY);

                var model = new RemitaRePostVm
                {
                    rrr = rrr,
                    merchantId = RemitaConfigParams.MERCHANTID,
                    hash = hash,
                    responseurl = url
                };
                return View(model);
            }
            catch (Exception)
            {
                return RedirectToAction("GetPaymentStatus", "RemitaServices", new { message = "Please check the RRR supplied and try again" });
                throw;
            }


        }


        [AllowAnonymous]
        public async Task<ActionResult> ConfrimPayment(string RRR, string orderID)
        {
            SchoolFeePayment schoofeepayment;
            RemitaResponse result = new RemitaResponse();
            if (string.IsNullOrEmpty(orderID))
            {
                schoofeepayment = await _db.SchoolFeePayments.Include(i => i.Students).AsNoTracking()
                    .Where(x => x.ReferenceNo.Equals(RRR))
                    .FirstOrDefaultAsync();
            }
            else
            {
                schoofeepayment = await _db.SchoolFeePayments.Include(i => i.Students).AsNoTracking()
                    .Where(x => x.OrderId.Equals(orderID.Trim()))
                    .FirstOrDefaultAsync();
            }
            if (schoofeepayment != null)
            {
                if (schoofeepayment.Status.Equals(true))
                {
                    result.Message = schoofeepayment.PaymentStatus;
                    result.OrderId = schoofeepayment.OrderId;
                    result.Rrr = schoofeepayment.ReferenceNo;
                    result.Status = schoofeepayment.Status.ToString();
                    return RedirectToAction("ConfrimRrrPayment", "RemitaPaymentLogs", result);
                }
                var log = await _db.RemitaPaymentLogs.AsNoTracking().Where(x => x.OrderId.Equals(schoofeepayment.OrderId))
                                            .FirstOrDefaultAsync();

                var hashed = _query.HashRemitedValidate(schoofeepayment.OrderId, RemitaConfigParams.APIKEY, RemitaConfigParams.MERCHANTID);
                string url = RemitaConfigParams.CHECKSTATUSURL + "/" + RemitaConfigParams.MERCHANTID + "/" + schoofeepayment.OrderId + "/" + hashed + "/" + "orderstatus.reg";
                string jsondata = new WebClient().DownloadString(url);
                result = JsonConvert.DeserializeObject<RemitaResponse>(jsondata);

                if (result.Status.Equals("00") || result.Status.Equals("01"))
                {
                    //check amount reurned  with Amount sent

                    if (schoofeepayment != null /*&& result.PaymentDate.Year.Equals(schoofeepayment.Date.Year)*/
                                        //&& result.PaymentDate.Month.Equals(schoofeepayment.Date.Month)
                                        && result.Amount >= schoofeepayment.PaidFee
                                        )
                    {
                        schoofeepayment.Status = true;
                        schoofeepayment.PaymentStatus = result.Message;
                        schoofeepayment.ReferenceNo = result.Rrr;
                        _db.Entry(schoofeepayment).State = EntityState.Modified;
                    }
                    else
                    {
                        return RedirectToAction("MakePayment",
                                new
                                {
                                    message = $"Payment Mismatch!! Please ensure to pay the  right amount or rsik puniitive actions!!!"
                                });
                    }


                    // Using SemaphoreSlim and Database Transaction to prevent matric number clashes
                    // Define a SemaphoreSlim with a maximum count of 1 to ensure exclusive access to the critical section.
                    var semaphore = new SemaphoreSlim(1, 1);

                    // Enter the critical section, ensuring exclusive access to this block of code.
                    await semaphore.WaitAsync();
                    using (var transaction = _db.Database.BeginTransaction()) //using database transactions to further prevent race conditions
                    {
                        try
                        {
                            if (schoofeepayment.FeeCategory.ToUpper().Equals(SchoolFeeCategory.School_Charges.ToString().ToUpper()) &&
                                schoofeepayment.Students.StudentStatus.ToUpper().Equals(StudentStatus.New_Student.ToString().ToUpper()))
                            {
                                var editStudent = await StudentQuery.SaveAndGenerateMatricNo(schoofeepayment.StudentId);

                                _query.UpdateTransactionLog(log, result);
                                await _db.SaveChangesAsync();
                                // Commit the transaction
                                transaction.Commit();

                                if (!string.IsNullOrEmpty(editStudent.MatricNo))
                                {
                                    var emailService = new EmailService();
                                    string body = $"{editStudent.FirstName} School Charges payment is confirmed. Your Mat-Num is {editStudent.MatricNo} and email is {editStudent.Email} Proceed with course registration";

                                    await emailService.SendAsync(new IdentityMessage
                                    {
                                        Destination = editStudent.PrimaryEmail,
                                        Body = $"Your Matric No {editStudent.MatricNo} has been generated successfully, a new University of Jos Email {editStudent.Email} has been generated." +
                                       $"You are now to login with this generated UNIJOS Email on the portal as your username with initial password to proceed. Visit www.unijos.edu.ng for detail",
                                        Subject = "MARIC NO GENERATED"
                                    });

                                    await SMSClass.SendSMS("UNIJOS SIS", body, editStudent.PhoneNumber); //EBULK SMS API
                                }

                                return RedirectToAction("LogOff", "Account", new
                                {
                                    url = "",
                                    message = $"Please go to your email({editStudent.PrimaryEmail}) to get your new login " +
                                    $"credentials to enable you continue your registration"
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            // Handle the exception here, log it, or take any necessary recovery action.
                            transaction.Rollback();
                            throw;
                        }
                        finally
                        {
                            // Release the semaphore to allow other threads to enter the critical section.
                            semaphore.Release();
                        }
                    }
                    //await _studentQuery.UpdateStudentRecord(schoofeepayment.FeeCategory, schoofeepayment.StudentId);

                    _query.UpdateTransactionLog(log, result);
                    await _db.SaveChangesAsync();
                }
                else
                {
                    schoofeepayment.Status = false;
                    schoofeepayment.PaymentStatus = result.Message;
                    schoofeepayment.ReferenceNo = result.Rrr;
                    _db.Entry(schoofeepayment).State = EntityState.Modified;

                    _query.UpdateTransactionLog(log, result);
                    await _db.SaveChangesAsync();

                    return RedirectToAction("RetrySchoolFeePayment", new { rrr = result.Rrr });

                }

                return RedirectToAction("ConfrimRrrPayment", "RemitaPaymentLogs", result);
            }
            var message = $"There is no payment that has either the RRR {RRR} or" +
                            $" Order Id {orderID} for School fee or Acceptance Fee";
            return RedirectToAction("GetPaymentStatus", "RemitaServices", new { message });

        }

        [AllowAnonymous]
        public async Task<ActionResult> ConfrimPaymentPaystack(string RRR, string orderID)
        {
            SchoolFeePayment schoofeepayment;
            RemitaResponse result = new RemitaResponse();
            if (string.IsNullOrEmpty(orderID))
            {
                schoofeepayment = await _db.SchoolFeePayments.Include(i => i.Students).AsNoTracking()
                    .Where(x => x.ReferenceNo.Equals(RRR))
                    .FirstOrDefaultAsync();
            }
            else
            {
                schoofeepayment = await _db.SchoolFeePayments.Include(i => i.Students).AsNoTracking()
                    .Where(x => x.OrderId.Equals(orderID.Trim()))
                    .FirstOrDefaultAsync();
            }
            if (schoofeepayment != null)
            {
                if (schoofeepayment.Status.Equals(true))
                {
                    result.Message = schoofeepayment.PaymentStatus;
                    result.OrderId = schoofeepayment.OrderId;
                    result.Rrr = schoofeepayment.ReferenceNo;
                    result.Status = schoofeepayment.Status.ToString();
                    return RedirectToAction("ConfrimRrrPayment", "RemitaPaymentLogs", result);
                }
                var log = await _db.RemitaPaymentLogs.AsNoTracking().Where(x => x.OrderId.Equals(schoofeepayment.OrderId))
                                            .FirstOrDefaultAsync();

                // Set your Paystack API reference here
                string reference = RRR;
                // Set your Paystack secret key here
                string secretKey = "sk_live_b7eeb010c3805dd446c779cc18f068667d6c8071";
                // Set the URL for Paystack API
                string apiUrl = $"https://api.paystack.co/transaction/verify/{reference}";

                // Create HttpClient instance
                HttpClient httpClient = new HttpClient();

                // Set Authorization header with Bearer token
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", secretKey);

                try
                {
                    // Send GET request to Paystack API
                    HttpResponseMessage response = await httpClient.GetAsync(apiUrl);

                    // Check if request was successful
                    if (response.IsSuccessStatusCode)
                    {
                        // Read response content
                        string jsonResponse = await response.Content.ReadAsStringAsync();

                        JObject jsonObject = JObject.Parse(jsonResponse);
                        if ((bool)jsonObject["status"] == true && (string)jsonObject["message"] == "Verification successful")
                        {
                            //check amount returned  with Amount sent

                            if (schoofeepayment != null /*&& result.PaymentDate.Year.Equals(schoofeepayment.Date.Year)*/
                                                //&& result.PaymentDate.Month.Equals(schoofeepayment.Date.Month)
                                                && (decimal)jsonObject["data"]["amount"] / 100 >= schoofeepayment.PaidFee
                                                )
                            {
                                schoofeepayment.Status = true;
                                schoofeepayment.PaymentStatus = (string)jsonObject["data"]["gateway_response"];
                                schoofeepayment.ReferenceNo = (string)jsonObject["data"]["reference"];
                                _db.Entry(schoofeepayment).State = EntityState.Modified;
                            }
                            else
                            {
                                return new JsonResult
                                {
                                    Data = new
                                    {
                                        status = false,
                                        message = $"Payment Mismatch!! Please ensure to pay the  right amount or risk puniitive actions!!!"
                                    }
                                };

                            }


                            // Using SemaphoreSlim and Database Transaction to prevent matric number clashes
                            // Define a SemaphoreSlim with a maximum count of 1 to ensure exclusive access to the critical section.
                            var semaphore = new SemaphoreSlim(1, 1);

                            // Enter the critical section, ensuring exclusive access to this block of code.
                            await semaphore.WaitAsync();
                            using (var transaction = _db.Database.BeginTransaction()) //using database transactions to further prevent race conditions
                            {
                                try
                                {
                                    if (schoofeepayment.FeeCategory.ToUpper().Equals(SchoolFeeCategory.School_Charges.ToString().ToUpper()) &&
                                        schoofeepayment.Students.StudentStatus.ToUpper().Equals(StudentStatus.New_Student.ToString().ToUpper()))
                                    {
                                        var editStudent = await StudentQuery.SaveAndGenerateMatricNo(schoofeepayment.StudentId);

                                        _query.UpdateTransactionLog(log, result);
                                        await _db.SaveChangesAsync();
                                        // Commit the transaction
                                        transaction.Commit();

                                        if (!string.IsNullOrEmpty(editStudent.MatricNo))
                                        {
                                            var emailService = new EmailService();
                                            string body = $"{editStudent.FirstName} School Charges payment is confirmed. Your Mat-Num is {editStudent.MatricNo} and email is {editStudent.Email} Proceed with course registration";

                                            await emailService.SendAsync(new IdentityMessage
                                            {
                                                Destination = editStudent.PrimaryEmail,
                                                Body = $"Your Matric No {editStudent.MatricNo} has been generated successfully, a new University of Jos Email {editStudent.Email} has been generated." +
                                               $"You are now to login with this generated UNIJOS Email on the portal as your username with initial password to proceed. Visit www.unijos.edu.ng for detail",
                                                Subject = "MARIC NO GENERATED"
                                            });

                                            await SMSClass.SendSMS("UNIJOS SIS", body, editStudent.PhoneNumber); //EBULK SMS API
                                        }

                                        return RedirectToAction("LogOff", "Account", new
                                        {
                                            url = "",
                                            message = $"Please go to your email({editStudent.PrimaryEmail}) to get your new login " +
                                            $"credentials to enable you continue your registration"
                                        });
                                    }
                                }
                                catch (Exception ex)
                                {
                                    // Handle the exception here, log it, or take any necessary recovery action.
                                    transaction.Rollback();
                                    throw;
                                }
                                finally
                                {
                                    // Release the semaphore to allow other threads to enter the critical section.
                                    semaphore.Release();
                                }
                            }
                            //await _studentQuery.UpdateStudentRecord(schoofeepayment.FeeCategory, schoofeepayment.StudentId);

                            _query.UpdateTransactionLog(log, result);
                            await _db.SaveChangesAsync();

                        }
                        else
                        {
                            schoofeepayment.Status = false;
                            schoofeepayment.PaymentStatus = result.Message;
                            schoofeepayment.ReferenceNo = result.Rrr;
                            _db.Entry(schoofeepayment).State = EntityState.Modified;

                            _query.UpdateTransactionLog(log, result);
                            await _db.SaveChangesAsync();

                            return RedirectToAction("RetrySchoolFeePayment", new { rrr = result.Rrr });
                            // Output error message
                            Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Output any exceptions
                    Console.WriteLine($"Exception: {ex.Message}");
                }
                finally
                {
                    // Dispose HttpClient
                    httpClient.Dispose();
                }

            }
            else
            {

                //return RedirectToAction("ConfrimRrrPayment", "RemitaPaymentLogs", result);
                return new JsonResult
                {
                    Data = new
                    {
                        status = false,
                        message = $"No such payment exist. visit the bursary department for further instructions!!!"
                    }
                };
            }
            return new JsonResult
            {
                Data = new
                {
                    status = true,
                    message = $"Payment Successful, Proceed to print receipt!!!"
                }
            };

        }


        public async Task<ActionResult> PrintReceipt(int id, int SessionId)
        {
            // Quering the SchoolFeePayment by Id to get payment Details
            var schoolFee = await _db.SchoolFeePayments.Include(i => i.Semester).Include(i => i.Level)
                                .Include(i => i.Session).AsNoTracking()
                                .Where(x => x.SchoolFeePaymentId.Equals(id)).FirstOrDefaultAsync();


            // From the result of SchoolfeePayment we get the studentId
            // Searching the Student table by the student Id
            var student = await _db.Students.AsNoTracking().Include(i => i.Level).Include(i => i.Programme).Include(i => i.Session)
                                        .Include(i => i.Programme.Department).Include(i => i.Programme.Department.Faculty).Include(i => i.SchoolProgramme)
                                        .AsNoTracking()
                                        .Where(x => x.StudentId.Equals(schoolFee.StudentId))
                                        .FirstOrDefaultAsync();

            string studentStatus = student.Session.SessionId.Equals(SessionId) || student.StudentStatus.Equals(StudentStatus.New_Student.ToString())
                                         ? StudentStatus.New_Student.ToString()
                                         : StudentStatus.Returning.ToString();

            var paymentSetting = await _feeQueryManager.GetPaymentSetting(SessionId, student.SchoolProgramme.SchoolProgrammeId, studentStatus);

            var feeList = new List<FeeList>();

            //check spill-over student
            string fileName = "spillSTDS.txt";
            string path = Server.MapPath("~/" + fileName);
            Dictionary<string, StudentSpillVm> Spillresult = System.IO.File.ReadLines(path)
                                                 .Select(line => line.Split(','))
                                                 .ToDictionary(split => split[0],
                                                                split => new StudentSpillVm(split[0],
                                                                                decimal.Parse(split[1]),
                                                                                split[2]
                                                                                //split[3],
                                                                                //split[4]
                                                                                ));
            StudentSpillVm spillVm;

            if (student.StudentStatus.Equals(StudentStatus.Returning.ToString()) && Spillresult.TryGetValue(student.MatricNo, out spillVm)) // check if person is on the spillOver List
            {
                feeList.Add(new FeeList { FeeTypeName = "10001", Amount = spillVm.Amount, Description = "School Charges" });
                feeList.Add(new FeeList { FeeTypeName = "0389", Amount = 10000, Description = "NUGA Charges (2 years)" });

                if ((await _feeQueryManager.GetDepartmentPaymentList(student.Programme.Department.DepartmentId, SessionId, (Int32)student.LevelId)).Count > 0)
                {
                    feeList.Add(new FeeList { FeeTypeName = "0441", Amount = 12500, Description = "Lab,Studio and Workshop Charge" });
                }


            }
            else
            {

                feeList.AddRange(await _feeQueryManager.GetSchoolFeeList(schoolFee.FeeCategory, student, paymentSetting, SessionId));

                if (schoolFee.TotalAmount > feeList.Sum(x => x.Amount))
                {
                    feeList.AddRange(await _feeQueryManager.GetLatePaymentFeeList(SessionId, student.SchoolProgrammeId, schoolFee.FeeCategory, schoolFee.Date));

                    //Add GST Charges to year 2 students
                    if (student.Level.LevelName.Equals("200") &&
                        student.StudentStatus.Equals(StudentStatus.Returning.ToString()) && !student.Session.SessionId.Equals(SessionId) &&
                        schoolFee.FeeCategory.Equals(SchoolFeeCategory.School_Charges.ToString()))
                    {
                        feeList.Add(new FeeList { FeeTypeName = "General Studies (GST) Charges", Amount = 5000, Description = "General Studies (GST) Charges" });
                    }

                    //if (paymentSetting != null && paymentSetting.ConsiderDepartmentalFee && !model.IsPartPayment)
                    //{
                    //check for roggue programmes: int[] numbers = { 1, 2, 3, 4, 5 };
                    int[] labProgrammes = { };
                    if (labProgrammes.Contains((Int32)student.ProgrammeId) && schoolFee.FeeCategory.Equals(SchoolFeeCategory.School_Charges.ToString()))
                    {
                        feeList.AddRange(await _feeQueryManager.GetDepartmentPaymentList(80, SessionId, (Int32)student.LevelId));
                    }
                    else
                    {
                        feeList.AddRange(await _feeQueryManager.GetDepartmentPaymentList(student.Programme.Department.DepartmentId, SessionId, (Int32)student.LevelId));
                    }

                    //Add NUGA Cgarges
                    feeList.Add(new FeeList { FeeTypeName = "0389", Amount = 10000, Description = "NUGA Charges (2 years)" });
                    //}
                }

            }

            var schoolFeePayment = new SchoolFeeReciept
            {
                Student = student,
                FeeCategory = schoolFee.FeeCategory,
                SchoolFeePayment = schoolFee,
                FeeLists = feeList
            };
            //return View(schoolFeePayment);
            return new ViewAsPdf(schoolFeePayment);
        }

        public async Task<ActionResult> SchoolFeePayment()
        {
            var mystates = from State s in Enum.GetValues(typeof(State))
                           select new { ID = s, Name = s.ToString() };

            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            ViewBag.DepartmentId = new SelectList(await _db.Departments.AsNoTracking().ToListAsync(), "DepartmentId", "DeptName");
            ViewBag.FacultyId = new SelectList(await _db.Faculties.AsNoTracking().ToListAsync(), "FacultyId", "FacultyName");
            ViewBag.LevelId = new SelectList(await _db.Levels.AsNoTracking().ToListAsync(), "LevelId", "LevelName");
            var feeCategory = from SchoolFeeCategory s in Enum.GetValues(typeof(SchoolFeeCategory))
                              select new { ID = s, Name = s.ToString() };

            ViewBag.FeeCategoryId = new MultiSelectList(feeCategory, "Name", "Name");
            ViewBag.SchoolProgrammeId = new SelectList(await _db.SchoolProgrammes.AsNoTracking().ToListAsync(), "SchoolProgrammeId", "FancyName");

            ViewBag.StateOfOrigin = new SelectList(mystates, "Name", "Name");


            return View();
        }


        public async Task<ActionResult> SchoolFeeDefaulters()
        {
            var mystates = from State s in Enum.GetValues(typeof(State))
                           select new { ID = s, Name = s.ToString() };

            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            ViewBag.DepartmentId = new SelectList(await _db.Departments.AsNoTracking().ToListAsync(), "DepartmentId", "DeptName");
            ViewBag.FacultyId = new SelectList(await _db.Faculties.AsNoTracking().ToListAsync(), "FacultyId", "FacultyName");
            ViewBag.LevelId = new SelectList(await _db.Levels.AsNoTracking().ToListAsync(), "LevelId", "LevelName");
            var feeCategory = from SchoolFeeCategory s in Enum.GetValues(typeof(SchoolFeeCategory))
                              select new { ID = s, Name = s.ToString() };

            ViewBag.FeeCategoryId = new SelectList(feeCategory, "Name", "Name");
            ViewBag.SchoolProgrammeId = new SelectList(await _db.SchoolProgrammes.AsNoTracking().ToListAsync(), "SchoolProgrammeId", "FancyName");
            ViewBag.StateOfOrigin = new SelectList(mystates, "Name", "Name");

            return View();
        }

        public async Task<ActionResult> GetStudentPayment(int SchoolProgrammeId, string FeeCategoryId, int? DepartmentId, int? FacultyId, int? LevelId,
                            int? SessionId, string HasPayed, DateTime? StartDate, DateTime? EndDate, string StateOfOrigin)
        {
            #region Server Side filtering

            //Get parameter for sorting from grid table
            // get Start (paging start index) and length (page size for paging)
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            //Get Sort columns values when we click on Header Name of column
            //getting column name
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            //Soring direction(either desending or ascending)
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
            string search = Request.Form.GetValues("search[value]").FirstOrDefault();

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int totalRecords = 0;
            bool hasPayed = false || (!string.IsNullOrEmpty(HasPayed) && HasPayed.Equals("True"));
            if (SessionId == null)
            {
                SessionId = _query.GetCurrentSessionId(SchoolProgrammeId);
            }
            var startDate = StartDate != null ? Convert.ToDateTime(StartDate) : new DateTime(2020, 12, 1);
            var endDate = EndDate != null ? Convert.ToDateTime(EndDate) : new DateTime(2030, 12, 1);

            var schoolFeeList2 = new List<SchoolFeePayment>();

            var schoolFee = await _db.SchoolFeePayments.Include(i => i.Students).Include(i => i.Session)
                                    .Include(i => i.Students.Programme.Department).Include(i => i.Students.Level)
                                    .AsNoTracking()
                                    .Where(x => x.FeeCategory.Equals(FeeCategoryId)
                                    && x.Status.Equals(hasPayed) && x.SessionId.Equals((int)SessionId)
                                    && x.Students.SchoolProgrammeId.Equals(SchoolProgrammeId)
                                    ).OrderBy(x => x.Students.Programme.Department.Faculty.FacultyName)
                                    .ThenBy(x => x.Students.Programme.Department.DeptName)
                                    .ThenBy(x => x.Students.Programme.ProgrammeName)
                                    .ThenBy(x => x.Students.Level.LevelName)
                                    .ToListAsync();
            schoolFee = schoolFee.Where(x => x.Date.Date >= startDate.Date && x.Date.Date <= endDate.Date).ToList();

            if (FacultyId != null)
            {
                schoolFee = schoolFee.Where(x => x.Students.Programme.Department.FacultyId.Equals((int)FacultyId)).ToList();
            }

            if (DepartmentId != null & LevelId != null)
            {
                schoolFee = schoolFee.Where(x => x.Students.Programme.Department.DepartmentId.Equals((int)DepartmentId)
                                                 && x.Students.Level.LevelId.Equals((int)LevelId)
                                                 && x.SessionId.Equals((int)SessionId)).ToList();
            }
            else if (DepartmentId != null)
            {
                schoolFee = schoolFee.Where(x => x.Students.Programme.Department.DepartmentId.Equals((int)DepartmentId)).ToList();
            }
            else if (LevelId != null)
            {
                schoolFee = schoolFee.Where(x => x.Students.Level.LevelId.Equals((int)LevelId)).ToList();
            }

            if (!string.IsNullOrEmpty(StateOfOrigin) && !StateOfOrigin.Equals("select_state"))
            {
                foreach (var item in schoolFee)
                {
                    if (item.Students.StateOfOrigin != null && item.Students.StateOfOrigin.Equals(StateOfOrigin))
                    {
                        //studentList2 = studentList.Where(x => x.StateOfOrigin.Equals(StateOfOrigin)).ToList();

                        schoolFeeList2.Add(item);
                    }
                }
            }
            else{
                schoolFeeList2 = schoolFee;
            }

            //    var feeRecord = schoolFeeList2.Select(s => new
            //{
            //    s.StudentId,
            //    MatricNo = !string.IsNullOrEmpty(s.Students.MatricNo) ? s.Students.MatricNo : s.Students.JambRegNo,
            //    s.Students.FullName,
            //    //GenderGender = !string.IsNullOrEmpty(s.Students.Gender) ? s.Students.Gender : "",
            //    Date = s.Date.ToString("dd/MM/yyyy"),
            //    s.Students.Programme.Department.DeptName,
            //    ProgrammeName = !string.IsNullOrEmpty(s.Students.Programme.ProgrammeName) ? s.Students.Programme.ProgrammeName : "",
            //    LevelName = !string.IsNullOrEmpty(s.Students.Level?.LevelName) ? s.Students.Level?.LevelName : "",
            //    PhoneNumber = !string.IsNullOrEmpty(s.Students.PhoneNumber) ? s.Students.PhoneNumber : "",
            //    s.PaidFee,
            //    s.Status,
            //    ReferenceNo = !string.IsNullOrEmpty(s.ReferenceNo) ? s.ReferenceNo : "",
            //    s.RemainingBalance,
            //    s.Students.StateOfOrigin,
            //    s.Students.Lga

            //}).ToList();
            var feeRecord = schoolFeeList2
                        .GroupBy(p => new
                        {
                            p.StudentId,
                            p.Students.MatricNo,
                            p.Students.FullName,
                            p.Students.StateOfOrigin,
                            p.Students.Lga,
                            p.Students.Programme.Department.DeptName,
                            p.Students.Programme.ProgrammeName,
                            p.Students.Level.LevelName,
                            p.Students.PhoneNumber
                        })
                        .Select(g => new
                        {
                            g.Key.StudentId,
                            MatricNo = !string.IsNullOrEmpty(g.Key.MatricNo) ? g.Key.MatricNo : g.First().Students.JambRegNo,
                            g.Key.FullName,
                            g.Key.StateOfOrigin,
                            g.Key.Lga,
                            g.Key.DeptName,
                            ProgrammeName = !string.IsNullOrEmpty(g.Key.ProgrammeName) ? g.Key.ProgrammeName : "",
                            LevelName = !string.IsNullOrEmpty(g.Key.LevelName) ? g.Key.LevelName : "",
                            PhoneNumber = !string.IsNullOrEmpty(g.Key.PhoneNumber) ? g.Key.PhoneNumber : "",
                            PaidFee = g.Sum(p => p.PaidFee),
                            Status = g.First().Status ? "Paid" : "Unpaid",
                            ReferenceNo = string.Join(", ", g.Select(p => p.ReferenceNo)),
                            RemainingBalance = g.Last().RemainingBalance,
                            Date = g.First().Date.ToString("dd/MM/yyyy"),
                            Action = "" // Placeholder for actions
                        })
                        .ToList();

            if (!string.IsNullOrEmpty(search))
            {
                feeRecord = feeRecord.Where(x => x.FullName.ToUpper().Contains(search.ToUpper().Trim())
                            || x.MatricNo.Trim().ToUpper().Equals(search.ToUpper().Trim())
                            || x.ReferenceNo.Contains(search)).ToList();
            }

            totalRecords = feeRecord.Count();
            var data = feeRecord.Skip(skip).Take(pageSize).ToList();

            return Json(new { draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data },
                JsonRequestBehavior.AllowGet);
            #endregion Server Side filtering
        }

        public async Task<ActionResult> GetSchoolFeeDefaulter(int SchoolProgrammeId, string FeeCategoryId, int? DepartmentId, int? FacultyId,
                                                                int? LevelId, int? SessionId, string StateOfOrigin)
        {

            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            //Get Sort columns values when we click on Header Name of column
            //getting column name
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            //Soring direction(either desending or ascending)
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
            string search = Request.Form.GetValues("search[value]").FirstOrDefault();

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int totalRecords = 0;

            if (SessionId == null)
            {
                SessionId = _query.GetCurrentSessionId(SchoolProgrammeId);
            }
            var defaulterList = new List<Student>();
            var defaulterList2 = new List<Student>();

            var studentsList = await _db.Students
                                        .Include(i => i.Level)
                                        .Include(i => i.Programme.Department)
                                        .Include(i => i.Programme.Department.Faculty)
                                        .AsNoTracking()
                                        .Where(x => x.Active && !x.IsGraduated && x.SchoolProgrammeId.Equals(SchoolProgrammeId))
                                    .OrderBy(s => s.StateOfOrigin)
                                    .ThenBy(s => s.Lga)
                                        .ToListAsync();

            var schoolFee = await _db.SchoolFeePayments
                .Include(i => i.Students)
                .Include(i => i.Session)
                .Include(i => i.Students.Programme.Department)
                .Include(i => i.Students.Programme.Department.Faculty)
                .Include(i => i.Students.Level)
                .AsNoTracking()
                .Where(x => x.FeeCategory.Equals(FeeCategoryId)
                            && x.Status
                            && x.SessionId.Equals((int)SessionId)
                            && x.Students.SchoolProgrammeId.Equals(SchoolProgrammeId))
                //.OrderBy(s => s.Students.StateOfOrigin)
                //.ThenBy(s => s.Students.Lga)
                .ToListAsync();

            if (FacultyId != null)
            {
                schoolFee = schoolFee.Where(x => x.Students.Programme.Department.Faculty.FacultyId.Equals((int)FacultyId)
                                                   && x.SessionId.Equals((int)SessionId)).ToList();

                studentsList = studentsList.Where(x => x.Programme.Department.Faculty.FacultyId.Equals((int)FacultyId)
                                                   ).ToList();
            }

            if (DepartmentId != null && LevelId != null)
            {
                schoolFee = schoolFee
                    .Where(x => x.Students.Programme.Department.DepartmentId.Equals((int)DepartmentId)
                                && x.Students.Level.LevelId.Equals((int)LevelId)
                                && x.SessionId.Equals((int)SessionId))
                    .ToList();
            }
            else if (DepartmentId != null)
            {
                schoolFee = schoolFee
                    .Where(x => x.Students.Programme.Department.DepartmentId.Equals((int)DepartmentId))
                    .ToList();
            }
            else if (LevelId != null)
            {
                schoolFee = schoolFee
                    .Where(x => x.Students.Level.LevelId.Equals((int)LevelId))
                    .ToList();
            }

            defaulterList = studentsList
                    .Where(student => !schoolFee.Any(x => x.StudentId.Equals(student.StudentId)))
                    .ToList();

            if (!string.IsNullOrEmpty(StateOfOrigin) && !StateOfOrigin.Equals("select_state"))
            {
                foreach (var item in defaulterList)
                {
                    if (item.StateOfOrigin != null && item.StateOfOrigin.Equals(StateOfOrigin))
                    {
                        //studentList2 = studentList.Where(x => x.StateOfOrigin.Equals(StateOfOrigin)).ToList();

                        defaulterList2.Add(item);
                    }
                }
            }

            var data = defaulterList2
                .Select(s => new
                {
                    s.MatricNo,
                    s.FullName,
                    s.Gender,
                    DeptName = s.Programme.Department.DeptName,
                    ProgrammeName = s.Programme.ProgrammeName,
                    LevelName = s.Level.LevelName,
                    s.PhoneNumber,
                    s.StateOfOrigin,
                    s.Lga
                })
                .ToList();

            totalRecords = data.Count();
            data = data.Skip(skip).Take(pageSize).ToList();

            return Json(new { draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data },
                JsonRequestBehavior.AllowGet);

            //return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        public async Task DownloadPendingPayment()
        {
            //var facilityList = Db.Communications.AsNoTracking().ToList();
            char c1 = 'A';
            ExcelPackage package = new ExcelPackage();
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Report");


            worksheet.Cells[$"{c1++}1"].Value = "Student Id";
            worksheet.Cells[$"{c1++}1"].Value = "Student Name";
            worksheet.Cells[$"{c1++}1"].Value = "OrderId";
            worksheet.Cells[$"{c1++}1"].Value = "Fee Type";
            worksheet.Cells[$"{c1++}1"].Value = "Amount";
            worksheet.Cells[$"{c1++}1"].Value = "Status";
            worksheet.Cells[$"{c1++}1"].Value = "Status Message";


            var feepayments = await _db.SchoolFeePayments.AsNoTracking().Where(x => x.Status.Equals(false)
                                    && x.SessionId.Equals(sessionId)).ToListAsync();

            int rowStart = 2;

            foreach (var feepayment in feepayments)
            {
                var student = await _db.Students.Where(x => x.StudentId.Equals(feepayment.StudentId))
                                    .Select(s => new { s.MatricNo, s.FirstName, s.LastName }).FirstOrDefaultAsync();
                worksheet.Cells[$"A{rowStart}"].Value = student.MatricNo;
                worksheet.Cells[$"B{rowStart}"].Value = student.LastName + student.FirstName;
                worksheet.Cells[$"C{rowStart}"].Value = feepayment.OrderId;
                worksheet.Cells[$"D{rowStart}"].Value = feepayment.FeeCategory;
                worksheet.Cells[$"E{rowStart}"].Value = feepayment.ReferenceNo;
                worksheet.Cells[$"F{rowStart}"].Value = feepayment.TotalAmount;
                worksheet.Cells[$"G{rowStart}"].Value = feepayment.Status;
                worksheet.Cells[$"H{rowStart}"].Value = feepayment.PaymentStatus;

                rowStart++;
            }
            // var info = results.FirstOrDefault();
            worksheet.Cells["A:AZ"].AutoFitColumns();
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment: filename=" + $"PreDegreeExamResult.xlsx");
            Response.BinaryWrite(package.GetAsByteArray());
            Response.End();

        }

        public async Task DownloadPendingSupplentary()
        {
            //var facilityList = Db.Communications.AsNoTracking().ToList();
            char c1 = 'A';
            ExcelPackage package = new ExcelPackage();
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Report");


            worksheet.Cells[$"{c1++}1"].Value = "Student Id";
            worksheet.Cells[$"{c1++}1"].Value = "Student Name";
            worksheet.Cells[$"{c1++}1"].Value = "OrderId";
            worksheet.Cells[$"{c1++}1"].Value = "RRR";
            worksheet.Cells[$"{c1++}1"].Value = "Status";
            worksheet.Cells[$"{c1++}1"].Value = "Status Message";


            var feepayments = await _db.SupplementaryList.AsNoTracking().Where(x => x.IsPayed.Equals(false)
                                    && x.ReferenceNo != null).ToListAsync();

            int rowStart = 2;
            //char c2 = 'A';

            foreach (var feepayment in feepayments)
            {
                worksheet.Cells[$"A{rowStart}"].Value = feepayment.JambRegNo;
                worksheet.Cells[$"B{rowStart}"].Value = $"{feepayment.LastName} {feepayment.FirstName}";
                worksheet.Cells[$"C{rowStart}"].Value = feepayment.OrderId;
                worksheet.Cells[$"D{rowStart}"].Value = feepayment.ReferenceNo;
                worksheet.Cells[$"E{rowStart}"].Value = feepayment.IsPayed;
                worksheet.Cells[$"F{rowStart}"].Value = feepayment.TransactionMessage;


                rowStart++;
            }
            // var info = results.FirstOrDefault();
            worksheet.Cells["A:AZ"].AutoFitColumns();
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment: filename=" + $"PreDegreeExamResult.xlsx");
            Response.BinaryWrite(package.GetAsByteArray());
            Response.End();

        }


        public ActionResult UploadStudentPayment()
        {
            return View();
        }


        [HttpPost]
        public async Task<ActionResult> UploadStudentPayment(HttpPostedFileBase excelfile)
        {
            if (excelfile == null || excelfile.ContentLength == 0)
            {
                ViewBag.Error = "Please Select a excel file <br/>";
                TempData["UserMessage"] = "Please Select a excel file.";
                TempData["Title"] = "Error.";

                return View("Index");
            }
            HttpPostedFileBase file = Request.Files["excelfile"];
            if (excelfile.FileName.EndsWith("xls", StringComparison.CurrentCulture) || excelfile.FileName.EndsWith("xlsx", StringComparison.CurrentCulture))
            {
                string lastrecord = "";
                int recordCount = 0;
                string message = "";
                string fileContentType = file.ContentType;
                byte[] fileBytes = new byte[file.ContentLength];
                var data = file.InputStream.Read(fileBytes, 0, Convert.ToInt32(file.ContentLength));

                // Read data from excel file
                using (var package = new ExcelPackage(file.InputStream))
                {
                    ExcelValidation myExcel = new ExcelValidation();
                    var currentSheet = package.Workbook.Worksheets;
                    var workSheet = currentSheet.First();
                    var noOfCol = workSheet.Dimension.End.Column;
                    var noOfRow = workSheet.Dimension.End.Row;
                    int requiredField = 10;


                    string validCheck = myExcel.ValidateExcel(noOfRow, workSheet, requiredField);
                    if (!validCheck.Equals("Success"))
                    {
                        string[] ssizes = validCheck.Split(' ');

                        string lineError = $"Line/Row number {ssizes[0]}  and column {ssizes[1]} is not rightly formatted, Please Check for anomalies ";
                        ViewBag.Message = lineError;
                        ViewBag.ErrorInfo = "Please Leave no column or row Empty/Blank";
                        ViewBag.ErrorMessage = lineError;
                        return View("ErrorException");
                    }
                    for (int row = 2; row <= noOfRow; row++)
                    {

                        var matricNumber = workSheet.Cells[row, 1].Value.ToString().Trim();
                        var rrr = workSheet.Cells[row, 2].Value.ToString().Trim().Replace("-", "");
                        var feeCategory = workSheet.Cells[row, 3].Value.ToString().Trim();
                        var semesterName = workSheet.Cells[row, 4].Value.ToString().Trim();
                        var sessionName = workSheet.Cells[row, 5].Value.ToString().Trim();
                        var status = workSheet.Cells[row, 6].Value.ToString().Trim();
                        var amount = Convert.ToDecimal(workSheet.Cells[row, 7].Value.ToString().Trim());
                        var amountPaid = Convert.ToDecimal(workSheet.Cells[row, 8].Value.ToString().Trim());
                        var orderId = workSheet.Cells[row, 9].Value.ToString().Trim();
                        var paymentDate = workSheet.Cells[row, 10].Value.ToString().Trim();
                        var paymentStatus = false;


                        var student = await _db.Students.AsNoTracking()
                                            .Where(x => x.MatricNo.ToUpper().Equals(matricNumber.ToUpper()))
                                            .FirstOrDefaultAsync();
                        var uploadedSessionId = GetSessionIdByYear(sessionName, allSessions);
                        var uploadedSemesterId = GetSemesterIdByName(semesterName, allSemesters);


                        if (student == null)
                        {
                            ViewBag.ErrorInfo = "Record Not Found";
                            ViewBag.ErrorMessage = $" The Student Matric \"{matricNumber}\"  specified in the excel at row {row} doesn't exist on the portal. " +
                                                   $"Please add the Department Option first before uploading or correct the excel sheet...";
                            return View("ErrorException");
                        }

                        if (feeCategory.ToUpper().Equals("SCHOOL CHARGES") || feeCategory.ToUpper().Equals("SCHOOL-CHARGES"))
                        {
                            feeCategory = SchoolFeeCategory.School_Charges.ToString();
                        }
                        else if (feeCategory.ToUpper().Equals("ACCEPTANCE"))
                        {
                            feeCategory = SchoolFeeCategory.Acceptance.ToString();
                        }
                        else
                        {
                            ViewBag.ErrorInfo = "School Fee Category type supported is \"School Fee\"  and \"Acceptance\" ";
                            ViewBag.ErrorMessage = "Please check the School Fee Category spelling very well ";
                            return View("ErrorException");
                        }
                        if (uploadedSessionId == 0)
                        {
                            ViewBag.ErrorInfo = "Record Not Found";
                            ViewBag.ErrorMessage = $" The Session Name \"{sessionName}\"  specified in the excel at row {row} doesn't exist on the portal. " +
                                                   $"Please add the Session Name first before uploading or correct the excel sheet...";
                            return View("ErrorException");
                        }
                        if (uploadedSemesterId == 0)
                        {
                            ViewBag.ErrorInfo = "Record Not Found";
                            ViewBag.ErrorMessage = $" The \"{semesterName}\" Semester specified in the excel doesn't exist on the portal. " +
                                                   $"Please add the level first before uploading or correct the excel sheet...";
                            return View("ErrorException");
                        }


                        if (status.ToUpper().Equals("TRUE"))
                        {
                            paymentStatus = true;
                        }
                        else if (status.ToUpper().Equals("FALSE"))
                        {
                            paymentStatus = false;
                        }
                        else
                        {
                            ViewBag.ErrorInfo = "Payment Status type supported is \"TRUE\",  and \"FALSE\"  ";
                            ViewBag.ErrorMessage = "Please check the Payment Status type spelling very well ";
                            return View("ErrorException");
                        }

                        try
                        {
                            var schoolFeePayment = new SchoolFeePayment()
                            {
                                StudentId = student.StudentId,
                                ReferenceNo = rrr,
                                OrderId = orderId,
                                Status = paymentStatus,
                                FeeCategory = feeCategory,
                                SemesterId = uploadedSemesterId,
                                SessionId = uploadedSessionId,
                                PaidFee = amountPaid,
                                TotalAmount = amount,
                                Date = ConvertToDateTime(paymentDate),
                            };
                            _db.SchoolFeePayments.Add(schoolFeePayment);

                            var studentFeeDetails = new StudentPaymentDetail()
                            {
                                StudentId = student.StudentId,
                                SessionId = uploadedSemesterId,
                                FeeTypeName = feeCategory,
                                Amount = amountPaid,
                                Description = feeCategory
                            };
                            _db.StudentPaymentDetails.Add(studentFeeDetails);

                            recordCount++;
                            lastrecord = $"The last Updated record has the Last Name {student.LastName} and First Name {student.FirstName} with Student Id {student.StudentId}";
                        }
                        catch (Exception ex)
                        {
                            ViewBag.ErrorInfo = "Please Leave no column or row Empty/Blank";
                            ViewBag.ErrorMessage = ex.Message;
                            return View("ErrorException");
                        }
                    }
                    try
                    {
                        await _db.SaveChangesAsync();
                        message = $"You have successfully Uploaded {recordCount} records...  and {lastrecord}";
                    }
                    catch (Exception ex)
                    {
                        ViewBag.ErrorInfo = "Record Not Saved";
                        ViewBag.ErrorMessage = $"Please check the System Generated number for duplicate {ex.InnerException.InnerException.Message} ";
                        return View("ErrorException");
                    }
                    return RedirectToAction("SchoolFeePayment");
                }
            }

            ViewBag.ErrorInfo = "File type is Incorrect < br />";
            ViewBag.ErrorMessage = $"Please check the and upload an excel file format";
            return View("ErrorException");
        }

        public async Task<ActionResult> UploadStudentToDelete(string formNo)
        {
            string lastrecord = "";

            try
            {
                var studentExit = await _db.Students.Where(x => x.JambRegNo.Trim().ToUpper().Equals(formNo.Trim().ToUpper())).ToListAsync();
                foreach (var student in studentExit)
                {

                    var payment = await _db.SchoolFeePayments.AnyAsync(x => x.StudentId.Equals(student.StudentId) && x.Status.Equals(true));
                    if (payment.Equals(false))
                    {
                        _db.Entry(student).State = EntityState.Deleted;
                        lastrecord = $"The last Updated record has the Last Name {student.LastName} and First Name {student.FirstName}";
                    }
                    else
                    {
                        student.Active = true;
                        _db.Entry(student).State = EntityState.Modified;
                    }
                }

            }
            catch (Exception ex)
            {
                ViewBag.ErrorInfo = "Please Leave no column or row Empty/Blank";
                ViewBag.ErrorMessage = ex.Message;
                return View("ErrorException");
            }

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                ViewBag.ErrorInfo = $"Delete Operation not successful";
                ViewBag.ErrorMessage = ex.Message;
                return View("ErrorException");
            }
            ViewBag.Message = lastrecord;
            return View();
        }


        private static int calLength(double num)
        {
            if (num < 0)
                throw new ArgumentOutOfRangeException();
            else if (num == 0)
                return 1;
            else
                return (int)Math.Floor(Math.Log10(num)) + 1;
        }

        private static int itterValue(int length)
        {
            if (length <= 3)
                return 0;

            int copyLength = length;

            while (length > 3)
            {
                length -= 3;
            }
            return copyLength - length;
        }

        private static bool compareValue(double x, double y)
        {
            int num_x = (int)Math.Floor(x);
            int num_y = (int)Math.Floor(y);
            int len_y = calLength(num_y);

            if (len_y == 4 && calLength(num_x) == 4)
            {
                if ((num_y > num_x) && (num_y - num_x) < 1000)
                    return true;
            }

            int num_itter = (int)Math.Pow(10, itterValue(len_y));

            int update_num_y = num_y / num_itter;

            if (num_x == (num_itter * update_num_y))
                return true;

            return false;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
