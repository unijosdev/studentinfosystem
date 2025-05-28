using Newtonsoft.Json;
using OfficeOpenXml;
using Rotativa;
using SwiftKampus.BusinessLogic;
using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampus.ViewModels;
using SwiftKampus.ViewModels.ProcessChangeOfCourse;
using SwiftKampusModel;
using SwiftKampusModel.AddmissionApplicant;
using SwiftKampusModel.ChangeOfCourse;
using SwiftKampusModel.Payment;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SwiftKampus.Controllers
{
    [Authorize]
    [Audit(AuditingLevel = 2)]
    public class ChangeOfCoursePaymentsController : BaseController
    {

        public ChangeOfCoursePaymentsController(SchoolDbContext db) : base(db)
        {

        }
        // GET: ChangeOfCoursePayments
        public async Task<ActionResult> Index()
        {
            var changeOfCoursePayments = await _db.ChangeOfCoursePayments.Include(c => c.Session).Include(i => i.Student)
                .Include(i => i.Programme).Where(x => x.Student.Email.Trim().ToUpper().Equals(userId.Trim().ToUpper()) && x.IsPayed.Equals(true)).ToListAsync();

            //var changeOfCoursePayments = await _db.ChangeOfCoursePayments.Include(c => c.Session).Include(a => a.Student).AsNoTracking()
            //                            .Where(x => x.Student.PrimaryEmail.Trim().ToUpper().Equals(userId.Trim().ToUpper())
            //                            && x.SessionId.Equals(sessionId)
            //                            //&& x.ChangeOfCourseType.Equals(ChangeOfCourseType)
            //                            && x.IsPayed.Equals(true)).ToListAsync();
            return View(changeOfCoursePayments);
        }

        public async Task<ActionResult> WaverIndex()
        {
            var changeOfCoursePayments = await _db.ApplicantWaiverPayments.Include(c => c.Session).Include(i => i.Applicant)
                .Include(i => i.Programme).Where(x => x.ApplicantId.Trim().ToUpper().Equals(userId.Trim().ToUpper()) && x.IsPayed.Equals(true)).ToListAsync();

            //var changeOfCoursePayments = await _db.ChangeOfCoursePayments.Include(c => c.Session).Include(a => a.Student).AsNoTracking()
            //                            .Where(x => x.Student.PrimaryEmail.Trim().ToUpper().Equals(userId.Trim().ToUpper())
            //                            && x.SessionId.Equals(sessionId)
            //                            //&& x.ChangeOfCourseType.Equals(ChangeOfCourseType)
            //                            && x.IsPayed.Equals(true)).ToListAsync();
            return View(changeOfCoursePayments);
        }

        public ActionResult PaymentHistory()
        {
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FancyName");
            ViewBag.FacultyId = new SelectList(_db.Faculties.AsNoTracking(), "FacultyId", "FacultyName");
            ViewBag.DepartmentId = new SelectList(_db.Departments.AsNoTracking(), "DepartmentId", "DeptName");
            ViewBag.ProgrammeId = new SelectList(_db.Programmes.AsNoTracking(), "ProgrammeId", "ProgrammeName");
            return View();
        }

        public async Task<ActionResult> PrintReceipt(int id)
        {
            var changeOfCourseReceipt = await _db.ChangeOfCoursePayments.Include(i => i.Student.Programme.Department.Faculty).Include(i => i.Session)
                                        .Where(x => x.ChangeOfCoursePaymentId.Equals(id) && x.IsPayed.Equals(true)).FirstOrDefaultAsync();

            return new ViewAsPdf(changeOfCourseReceipt);
        }
        public async Task<ActionResult> WaiverPrintReceipt(int id)
        {
            var changeOfCourseReceipt = await _db.ApplicantWaiverPayments.Include(i => i.Applicant).AsNoTracking()
                                                  .Include(i => i.Session)
                                        .Where(x => x.ApplicantWaiverPaymentId.Equals(id) && x.IsPayed.Equals(true)).FirstOrDefaultAsync();
            ViewBag.Applicant = _db.Applicants.Include(x => x.AvailableCourse).Include(x => x.AvailableCourse.Programme)
                                              .Include(x => x.AvailableCourse.Programme.Department.Faculty)
                                              .Include(x => x.AvailableCourse.Programme.Department)
                                              .Include(x => x.AvailableCourse)
                                               .Where(x => x.ApplicantEmail.Trim().ToUpper().Equals(changeOfCourseReceipt.ApplicantId.Trim().ToUpper())).FirstOrDefault();

            return new ViewAsPdf(changeOfCourseReceipt);
        }



        public ActionResult ChangeOfCourseReport()
        {
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FancyName");

            var changeOfCoureType = from ChangeOfCourseType s in Enum.GetValues(typeof(ChangeOfCourseType))
                                    select new { ID = s, Name = s.ToString() };
            var staffRecord = _db.Staffs.Include(i => i.Department.Faculty)
                                .FirstOrDefault(x => x.Email.ToUpper().Trim().Equals(userId.Trim().ToUpper()));

            if (User.IsInRole(RoleName.Dean))
            {
                ViewBag.FacultyId = new SelectList(_db.Faculties.AsNoTracking().Where(x => x.FacultyId.Equals(staffRecord.Department.FacultyId)), "FacultyId", "FacultyName");
                ViewBag.DepartmentId = new SelectList(_db.Departments.AsNoTracking().Where(x => x.FacultyId.Equals(staffRecord.Department.FacultyId)), "DepartmentId", "DeptName");
                ViewBag.ProgrammeId = new SelectList(_db.Programmes.Include(i => i.Department).AsNoTracking().Where(x => x.Department.FacultyId.Equals(staffRecord.Department.FacultyId)), "ProgrammeId", "ProgrammeName");
            }
            else if (User.IsInRole(RoleName.Hod))
            {
                ViewBag.FacultyId = new SelectList(_db.Faculties.AsNoTracking().Where(x => x.FacultyId.Equals(staffRecord.Department.FacultyId)), "FacultyId", "FacultyName");
                ViewBag.DepartmentId = new SelectList(_db.Departments.AsNoTracking().Where(x => x.DepartmentId.Equals(staffRecord.Department.DepartmentId)), "DepartmentId", "DeptName");
                ViewBag.ProgrammeId = new SelectList(_db.Programmes.Include(i => i.Department).AsNoTracking().Where(x => x.Department.FacultyId.Equals(staffRecord.Department.FacultyId)), "ProgrammeId", "ProgrammeName");
            }
            else
            {
                ViewBag.FacultyId = new SelectList(_db.Faculties.AsNoTracking(), "FacultyId", "FacultyName");
                ViewBag.DepartmentId = new SelectList(_db.Departments.AsNoTracking(), "DepartmentId", "DeptName");
                ViewBag.ProgrammeId = new SelectList(_db.Programmes.AsNoTracking(), "ProgrammeId", "ProgrammeName");
                ViewBag.ChangeOfCourseType = new SelectList(changeOfCoureType, "Name", "Name");
            }
            return View();
        }

        public ActionResult ChangeOfCourseHistory()
        {
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FancyName");
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");


            var staffRecord = _db.Staffs.Include(i => i.Department.Faculty)
                                .FirstOrDefault(x => x.Email.ToUpper().Trim().Equals(userId.Trim().ToUpper()));

            if (User.IsInRole(RoleName.Dean))
            {
                ViewBag.FacultyId = new SelectList(_db.Faculties.AsNoTracking().Where(x => x.FacultyId.Equals(staffRecord.Department.FacultyId)), "FacultyId", "FacultyName");
                ViewBag.DepartmentId = new SelectList(_db.Departments.AsNoTracking().Where(x => x.FacultyId.Equals(staffRecord.Department.FacultyId)), "DepartmentId", "DeptName");
                ViewBag.ProgrammeId = new SelectList(_db.Programmes.Include(i => i.Department).AsNoTracking().Where(x => x.Department.FacultyId.Equals(staffRecord.Department.FacultyId)), "ProgrammeId", "ProgrammeName");
            }
            else if (User.IsInRole(RoleName.Hod))
            {
                ViewBag.FacultyId = new SelectList(_db.Faculties.AsNoTracking().Where(x => x.FacultyId.Equals(staffRecord.Department.FacultyId)), "FacultyId", "FacultyName");
                ViewBag.DepartmentId = new SelectList(_db.Departments.AsNoTracking().Where(x => x.DepartmentId.Equals(staffRecord.Department.DepartmentId)), "DepartmentId", "DeptName");
                ViewBag.ProgrammeId = new SelectList(_db.Programmes.Include(i => i.Department).AsNoTracking().Where(x => x.Department.FacultyId.Equals(staffRecord.Department.FacultyId)), "ProgrammeId", "ProgrammeName");
            }
            else
            {
                ViewBag.FacultyId = new SelectList(_db.Faculties.AsNoTracking(), "FacultyId", "FacultyName");
                ViewBag.DepartmentId = new SelectList(_db.Departments.AsNoTracking(), "DepartmentId", "DeptName");
                ViewBag.ProgrammeId = new SelectList(_db.Programmes.AsNoTracking(), "ProgrammeId", "ProgrammeName");
            }
            return View();
        }


        public async Task<ActionResult> GetIndex(int? SchoolProgrammeId, int? FacultyId, int? DepartmentId, int? ProgrammeId, string ChangeOfCourseType)
        {         
            List<ChangeOfCoursePayment> changeOfCourseList = await QueryChangeOfCourse(SchoolProgrammeId, FacultyId, DepartmentId, ProgrammeId, ChangeOfCourseType);

            var changeOfCourseVm = MapToChangeOfCourseVm(changeOfCourseList.OrderBy(x => x.Student.FullName).ToList());

            var data = changeOfCourseVm.OrderBy(x => x.Id).ToList();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> GetHistoryIndex(int? SessionId, int? SchoolProgrammeId, int? FacultyId, int? DepartmentId, int? ProgrammeId)
        {
            List<ChangeOfCourseHistory> changeOfCourseList = await QueryChangeOfCourseHistory(SessionId, SchoolProgrammeId, FacultyId, DepartmentId, ProgrammeId);

            var changeOfCourseVm = MapToChangeOfCourseHistoryVm(changeOfCourseList.OrderBy(x => x.Student.FullName).ToList());

            var data = changeOfCourseVm.OrderBy(x => x.Id).ToList();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }


        public async Task DownloadChangeOfCourseReport(int? SchoolProgrammeId, int? FacultyId, int? DepartmentId, int? ProgrammeId, string ChangeOfCourseType)
        {
            List<ChangeOfCoursePayment> changeOfCourseList = await QueryChangeOfCourse(SchoolProgrammeId, FacultyId, DepartmentId, ProgrammeId, ChangeOfCourseType);

            var changeOfCourseVm = MapToChangeOfCourseVm(changeOfCourseList.OrderBy(x => x.Student.FullName).ToList());

            char c1 = 'A';
            ExcelPackage package = new ExcelPackage();
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Report");

            worksheet.Cells[$"{c1++}1"].Value = "S/N";
            worksheet.Cells[$"{c1++}1"].Value = "Form No";
            worksheet.Cells[$"{c1++}1"].Value = "FullName";
            worksheet.Cells[$"{c1++}1"].Value = "Mode Of Entry";
            worksheet.Cells[$"{c1++}1"].Value = "Email";
            worksheet.Cells[$"{c1++}1"].Value = "Phone Number";
            worksheet.Cells[$"{c1++}1"].Value = "Course Admitted";
            worksheet.Cells[$"{c1++}1"].Value = "Reson for Change";
            worksheet.Cells[$"{c1++}1"].Value = "New Course Applied";
            worksheet.Cells[$"{c1++}1"].Value = "Recommendation";

            int rowStart = 2;
            //char c2 = 'A';

            for (var i = 0; i < changeOfCourseVm.Count; i++)
            {
                worksheet.Cells[$"A{rowStart}"].Value = changeOfCourseVm[i].Id;
                worksheet.Cells[$"B{rowStart}"].Value = changeOfCourseVm[i].RegNo;
                worksheet.Cells[$"C{rowStart}"].Value = changeOfCourseVm[i].FullName;
                worksheet.Cells[$"D{rowStart}"].Value = changeOfCourseVm[i].ModeOfEntry;
                worksheet.Cells[$"E{rowStart}"].Value = changeOfCourseVm[i].Email;
                worksheet.Cells[$"F{rowStart}"].Value = changeOfCourseVm[i].PhoneNumber;
                worksheet.Cells[$"G{rowStart}"].Value = changeOfCourseVm[i].CourseAdmitted;
                worksheet.Cells[$"H{rowStart}"].Value = changeOfCourseVm[i].ReasonForRejection;
                worksheet.Cells[$"I{rowStart}"].Value = changeOfCourseVm[i].NewCourseApplied;
                worksheet.Cells[$"J{rowStart}"].Value = changeOfCourseVm[i].Rocommendation;
                rowStart++;
            }

            foreach (var applicant in changeOfCourseList)
            {
                applicant.IsDownloaded = true;
                _db.Entry(applicant).State = EntityState.Modified;
            }
            await _db.SaveChangesAsync();

            worksheet.Cells["A:AZ"].AutoFitColumns();
            worksheet.Column(1).Style.Locked = true;
            worksheet.Column(2).Style.Locked = true;
            worksheet.Column(3).Style.Locked = true;
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment: filename=" +
                                                      $"Dept Recommended List.xlsx");
            Response.BinaryWrite(package.GetAsByteArray());
            Response.End();
        }

        private async Task<List<ChangeOfCoursePayment>> QueryChangeOfCourse(int? SchoolProgrammeId, int? FacultyId, int? DepartmentId, int? ProgrammeId, string ChangeOfCourseType)
        {
            var sessionID = _query.GetCurrentSessionId((int)SchoolProgrammeId);
            var changeOfCourseList = await _db.ChangeOfCoursePayments.Include(c => c.Session).Include(i => i.Student.Programme.Department)
                                    .Include(i => i.Programme.Department).Where(x => x.SessionId.Equals(sessionID)
                                    && x.IsPayed.Equals(true) && x.ChangeOfCourseType.Equals(ChangeOfCourseType.ToString())
                                    && x.IsDownloaded != true).ToListAsync();

            if (ProgrammeId != null)
            {
                changeOfCourseList = changeOfCourseList.Where(x => x.Programme.ProgrammeId.Equals((int)ProgrammeId)).ToList();
            }
            else if (DepartmentId != null)
            {
                changeOfCourseList = changeOfCourseList.Where(x => x.Programme.Department.DepartmentId.Equals((int)DepartmentId)).ToList();
            }
            else if (FacultyId != null)
            {
                changeOfCourseList = changeOfCourseList.Where(x => x.Programme.Department.FacultyId.Equals((int)FacultyId)).ToList();
            }

            return changeOfCourseList;
        } 
        private async Task<List<ChangeOfCourseHistory>> QueryChangeOfCourseHistory(int? SessionId,int? SchoolProgrammeId, int? FacultyId, int? DepartmentId, int? ProgrammeId)
        {
            int sessionID = 0;
            if(SessionId == null)
            {
               sessionID = _query.GetCurrentSessionId((int)SchoolProgrammeId);
            }
            else
            {
                sessionID = (int)SessionId;
            }

            var changeOfCourseList = await _db.ChangeOfCourseHistory.Include(c => c.Student.Programme)
                                    .ToListAsync();

            if (ProgrammeId != null)
            {
                //changeOfCourseList = changeOfCourseList.Where(x => x.Programme.ProgrammeId.Equals((int)ProgrammeId)).ToList();
            }
            else if (DepartmentId != null)
            {
                //changeOfCourseList = changeOfCourseList.Where(x => x.Programme.Department.DepartmentId.Equals((int)DepartmentId)).ToList();
            }
            else if (FacultyId != null)
            {
                //changeOfCourseList = changeOfCourseList.Where(x => x.Programme.Department.FacultyId.Equals((int)FacultyId)).ToList();
            }

            return changeOfCourseList;
        }

        public async Task<ActionResult> GetPaymentHistory(int? SessionId, int? SchoolProgrammeId, int? FacultyId, int? DepartmentId, int? ProgrammeId,
            string hasRegistered)
        {           
            var changeOfCourseVm = new List<ChangeOfCoursePaymentHistoryVm>();
            bool status = false;
            if (!string.IsNullOrEmpty(hasRegistered) && hasRegistered.Equals("True"))
            {
                status = true;
            }
            int sessionID = 0;
            if (SchoolProgrammeId != null)
            {
                if(SessionId == null)
                {
                    sessionID = _query.GetCurrentSessionId((int)SchoolProgrammeId);
                }
                else
                {
                    sessionID = (int)SessionId;
                }

                var changeOfCourseList = await _db.ChangeOfCoursePayments.Include(c => c.Session).Include(i => i.Student.Programme)
                                        .Include(i => i.Programme.Department).Where(x => x.SessionId.Equals(sessionID)
                                        && x.IsPayed.Equals(status)).ToListAsync();

                if (ProgrammeId != null)
                {
                    changeOfCourseList = changeOfCourseList.Where(x => x.Programme.ProgrammeId.Equals((int)ProgrammeId)).ToList();
                }
                else if (DepartmentId != null)
                {
                    changeOfCourseList = changeOfCourseList.Where(x => x.Programme.Department.DepartmentId.Equals((int)DepartmentId)).ToList();
                }
                else if (FacultyId != null)
                {
                    changeOfCourseList = changeOfCourseList.Where(x => x.Programme.Department.FacultyId.Equals((int)FacultyId)).ToList();
                }
                changeOfCourseVm = MapToPaymentHistoryVm(changeOfCourseList.OrderBy(x => x.Student.FullName).ToList());
            }
            var data = changeOfCourseVm.OrderBy(x => x.Id).ToList();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        private List<ChangeOfCourseReportVm> MapToChangeOfCourseVm(List<ChangeOfCoursePayment> v)
        {
            var cOfCourse = new List<ChangeOfCourseReportVm>();
            int count = 1;
            foreach (var vm in v) // Remove the Take(10) Condition after the testing
            {
                var rejectStudent =  _db.RejectedStudents.AsNoTracking().Where(x => x.StudentId.Trim().Equals(vm.StudentId.Trim())
                                    /*&& x.SessionId.Equals(vm.SessionId)*/).FirstOrDefault();
                //var utmeScore = await _db.UtmeApplicants.Where(x => x.JambRegNo.ToUpper().Trim().Equals(vm.Student.JambRegNo.Trim().ToUpper()))
                //                    .Select(x => x.ResultGrade).FirstOrDefaultAsync();

                cOfCourse.Add(new ChangeOfCourseReportVm
                {
                    StudentId = vm.StudentId,
                    Id = count,
                    FullName = vm.Student?.FullName ?? "",
                    CourseAdmitted = vm.Student?.Programme?.ProgrammeName ?? "",
                    NewCourseApplied = vm.Programme?.ProgrammeName ?? "",
                    ReasonForRejection = rejectStudent?.ReasonForRejection ?? "",
                    //ReasonForRejection = "",
                    RegNo = vm.Student?.JambRegNo ?? "",
                    MatricNo = vm.Student?.MatricNo ?? "",
                    Rocommendation = "",
                    //Score = utmeScore
                    Score = "",
                    ModeOfEntry = vm.Student.ModeOfEntry ?? "",
                    PhoneNumber = vm.Student.PhoneNumber ?? "",
                    Email = vm.Student.Email ?? "",
                });
                count += 1;
            }
            return cOfCourse;
        }

        private List<ChangeOfCourseReportVm> MapToChangeOfCourseHistoryVm(List<ChangeOfCourseHistory> v)
        {
            var cOfCourse = new List<ChangeOfCourseReportVm>();
            int count = 1;
            var courseList = GetAllProgramme();
            foreach (var vm in v)
            {
                cOfCourse.Add(new ChangeOfCourseReportVm
                {
                    StudentId = vm.StudentId,
                    Id = count,
                    FullName = vm.Student?.FullName ?? "",
                    CourseAdmitted = vm.Student?.Programme?.ProgrammeName ?? "",
                    NewCourseApplied = courseList.Where(s => s.ProgrammeId.Equals(vm.NewProgrammeId)).Select(s => s.ProgrammeName).FirstOrDefault(),
                    ReasonForRejection = "",
                    RegNo = vm.Student?.JambRegNo ?? "",
                    Rocommendation = "",
                    Score = "",
                    ModeOfEntry = vm.Student.ModeOfEntry ?? "",
                    PhoneNumber = vm.Student.PhoneNumber ?? "",
                    Email = vm.Student.Email ?? "",
                });
                count += 1;
            }
            return cOfCourse;
        }

        private List<ChangeOfCoursePaymentHistoryVm> MapToPaymentHistoryVm(List<ChangeOfCoursePayment> v)
        {
            var cOfCourse = new List<ChangeOfCoursePaymentHistoryVm>();
            int count = 1;
            foreach (var vm in v)
            {
                cOfCourse.Add(new ChangeOfCoursePaymentHistoryVm
                {
                    Id = count,
                    FullName = vm.Student?.FullName ?? "",
                    CourseAdmitted = vm.Student?.Programme?.ProgrammeName ?? "",
                    Amount = vm.TotalAmount,
                    RegNo = vm.Student?.JambRegNo ?? "",
                    Status = vm.IsPayed.ToString(),
                    Message = vm.TransactionMessage,
                    ReferenceNumber = vm.ReferenceNo
                });
                count += 1;
            }
            return cOfCourse;
        }

        public async Task<PartialViewResult> Save(int id)
        {
            var grade = await _db.ChangeOfCoursePayments.FindAsync(id);
            ViewBag.ProgrammeId = new SelectList(_db.Programmes.AsNoTracking(), "ProgrammeId", "ProgrammeName");
            return PartialView(grade);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Save(ChangeOfCoursePayment model)
        {
            bool status = false;
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                if (model.ChangeOfCoursePaymentId > 0)
                {
                    var changeOfCourse = await _db.ChangeOfCoursePayments.FindAsync(model.ChangeOfCoursePaymentId);
                    if (changeOfCourse != null /*&& changeOfCourse.IsExpired.Equals(false)*/)
                    {
                        changeOfCourse.ProgrammeId = model.ProgrammeId;
                        changeOfCourse.IsExpired = true;
                        changeOfCourse.NoOfSubmit = changeOfCourse.NoOfSubmit + 1;
                       
                        _db.Entry(changeOfCourse).State = EntityState.Modified;
                        await _db.SaveChangesAsync();
                        message = $"Change of Course Saved Successfully...";
                        return new JsonResult { Data = new { status = true, message } };
                    }
                }                
            }
            return new JsonResult { Data = new { status, message = "Please select a programme and try saving again" } };
            //return View(subject);
        }

        // GET: ChangeOfCoursePayments/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ChangeOfCoursePayment changeOfCoursePayment = await _db.ChangeOfCoursePayments.FindAsync(id);
            if (changeOfCoursePayment == null)
            {
                return HttpNotFound();
            }
            return View(changeOfCoursePayment);
        }

        public ActionResult MakePayment()
        {
            var changeOfCoureType = from ChangeOfCourseType s in Enum.GetValues(typeof(ChangeOfCourseType))
                                    select new { ID = s, Name = s.ToString() };

            ViewBag.ChangeOfCourseType = new SelectList(changeOfCoureType, "Name", "Name");
            return View();
        }

        public ActionResult MakePaymentWaiver()
        {
            var changeOfCoureType = from ChangeOfCourseType s in Enum.GetValues(typeof(ChangeOfCourseType))
                                    select new { ID = s, Name = s.ToString() };

            ViewBag.ChangeOfCourseType = new SelectList(changeOfCoureType, "Name", "Name");
            return View();
        }

        public async Task<ActionResult> Create(string ChangeOfCourseType)
        {
            var student = _studentQuery.GetStudent(userId);

            var applicantPaymentVm = new ChangeOfCoursePaymentVm();
            var applicantsetting = new ChangeOfCourseFee();
            var id = "";
            var phoneNumber = "";

            if (!string.IsNullOrEmpty(ChangeOfCourseType))
            {
                applicantsetting = _db.ChangeOfCourseFees.AsNoTracking().Where(x => x.ChangeOfCourseType.Equals(ChangeOfCourseType)
                                        && x.SessionId.Equals(sessionId)).FirstOrDefault();
                if (applicantsetting == null)
                {
                    ViewBag.Message = $"Fee has not been set for this Programme ({student.SchoolProgramme.FancyName})";
                    return View();
                }
            }
            else
            {
                ViewBag.Message = $"Please select the change of course type";
                return View();
            }

            var hasTransactionProcessed = await _db.ChangeOfCoursePayments.AsNoTracking()
                                     .Where(x => x.StudentId.Trim().Equals(student.StudentId.Trim())
                                     && x.SessionId.Equals(sessionId) && x.IsPayed.Equals(true)
                                     && x.ChangeOfCourseType.Equals(ChangeOfCourseType)
                                     && x.IsProcessed.Equals(false)).FirstOrDefaultAsync();

            //var hasTransactionProcessed = await _db.ChangeOfCoursePayments.Include(a => a.Student).AsNoTracking()
            //                            .Where(x => x.Student.PrimaryEmail.Equals(student.PrimaryEmail)
            //                            && x.SessionId.Equals(sessionId)
            //                            && x.ChangeOfCourseType.Equals(ChangeOfCourseType)
            //                            && x.IsProcessed.Equals(false)
            //                            ).FirstOrDefaultAsync();
            if (hasTransactionProcessed != null && !hasTransactionProcessed.ChangeOfCourseType.Equals("Deferment"))
            {
                return RedirectToAction("Index");
            }
            else if (hasTransactionProcessed != null && hasTransactionProcessed.ChangeOfCourseType.Equals("Deferment")) //Redirects to Deferment Controller
            {
                return RedirectToAction("Index", "DefermentReabsorptions", new { area = "" });
            }

            var hasTransaction = await _db.ChangeOfCoursePayments.AsNoTracking()
                                     .Where(x => x.StudentId.Equals(student.StudentId)
                                     && x.SessionId.Equals(sessionId) && x.IsPayed.Equals(false)
                                     && x.ChangeOfCourseType.Equals(ChangeOfCourseType)).FirstOrDefaultAsync();
            if (hasTransaction != null)
            {
                var hashed = _query.HashRemitedValidate(hasTransaction.OrderId, RemitaConfigParams.APIKEY, RemitaConfigParams.MERCHANTID);
                string checkurl = RemitaConfigParams.CHECKSTATUSURL + "/" + RemitaConfigParams.MERCHANTID + "/" + hasTransaction.OrderId + "/" + hashed + "/" + "orderstatus.reg";
                var checkResult = CheckExistingTransaction(checkurl);

                if (checkResult.Item1.Equals(true))
                {
                    if (string.IsNullOrEmpty(checkResult.Item2.Rrr))
                    {
                        var entry = _db.Entry(hasTransaction);
                        if (entry.State == EntityState.Detached)
                            _db.ChangeOfCoursePayments.Attach(hasTransaction);
                        _db.ChangeOfCoursePayments.Remove(hasTransaction);
                        _db.SaveChanges();
                    }
                    else
                    {
                        return RedirectToAction("ConfrimApplicationPayment", new { orderID = hasTransaction.OrderId });
                    }
                }
                else
                {
                    ViewBag.ErrorInfo = $"Check your internet connection and try again";
                    ViewBag.ErrorMessage = "Remita is currently unreachable";
                    return View("RemitaErrorPage");
                }
            }
            if (student != null)
            {               
                id = student.JambRegNo;
                phoneNumber = student.PhoneNumber;             
            }
            else
            {
                ViewBag.Message = $"Student Record Not Found";
                return View();
            }

            long milliseconds = DateTime.Now.Ticks;
            var url = Url.Action("ConfrimApplicationPayment", "ChangeOfCoursePayments", new { },
                                   protocol: Request.Url.Scheme);
            if (applicantsetting != null)
            {
                applicantPaymentVm.FullName = student.FullName;
                applicantPaymentVm.TotalAmount = applicantsetting.ApplicationFee;
                applicantPaymentVm.payerName = await _query.GetUserFullName(userId);
                applicantPaymentVm.payerEmail = userId;
                applicantPaymentVm.payerPhone = phoneNumber ?? "07030000000";
                applicantPaymentVm.amt = applicantsetting.ApplicationFee.ToString();
                applicantPaymentVm.merchantId = RemitaConfigParams.MERCHANTID;
                applicantPaymentVm.orderId = $"UJCOC{milliseconds.ToString()}";
                applicantPaymentVm.responseurl = url;
                applicantPaymentVm.StudentId = student.StudentId;
                applicantPaymentVm.serviceTypeId = RemitaConfigParams.CHANGEOFCOURSE;
                applicantPaymentVm.ExpectedAmount = applicantsetting.ApplicationFee;
                applicantPaymentVm.SessionId = sessionId;
                applicantPaymentVm.apiKey = RemitaConfigParams.APIKEY;
                applicantPaymentVm.SessionName = _query.GetCurrentSessionName(sessionId);

                var changeOfCoureType = from ChangeOfCourseType s in Enum.GetValues(typeof(ChangeOfCourseType))
                                    select new { ID = s, Name = s.ToString() };

                ViewBag.ChangeOfCourseType = new SelectList(changeOfCoureType, "Name", "Name");

                return View(applicantPaymentVm);
            }
            //ViewBag.Message = $"{applicantsetting.SchoolProgramme.FancyName} Form is no longer Active for sales";
            return View();
        }

        // POST: ApplicantPayments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(ChangeOfCoursePaymentVm model)
        {
            if (ModelState.IsValid)
            {

                var hasTransaction = await _db.ChangeOfCoursePayments.AsNoTracking()
                                        .Where(x => x.StudentId.Equals(model.StudentId)
                                        && x.SessionId.Equals(model.SessionId)).FirstOrDefaultAsync();

                model.paymenttype = model.RemitaPaymentType.ToString().Replace("_", " ").ToLower();
                if (hasTransaction != null)
                {
                    model.hash = _query.HashRemitaRequest(model.merchantId, model.serviceTypeId, hasTransaction.OrderId, model.amt, model.responseurl, RemitaConfigParams.APIKEY);
                    return RedirectToAction("ConfrimApplicationPayment", new { orderID = hasTransaction.OrderId });
                }
                var applicationPayment = new ChangeOfCoursePayment
                {
                    OrderId = model.orderId,
                    PaymentDateTime = DateTime.Now,
                    SessionId = model.SessionId,
                    StudentId = model.StudentId,
                    TotalAmount = Convert.ToDecimal(model.amt),
                    ChangeOfCourseType = model.ChangeOfCourseType
                    //ReferenceNo = reference
                };
                _db.ChangeOfCoursePayments.Add(applicationPayment);
                var log = new RemitaPaymentLog
                {
                    OrderId = model.orderId,
                    PaymentName = "Change of Course Fee",
                    PaymentDate = DateTime.Now,
                    Amount = model.amt,
                    PayerName = model.payerName
                };
                _db.RemitaPaymentLogs.Add(log);
                await _db.SaveChangesAsync();
                model.hash = _query.HashRemitaRequest(model.merchantId, model.serviceTypeId, model.orderId, model.amt, model.responseurl, RemitaConfigParams.APIKEY);
                return RedirectToAction("SubmitRemita", model);
            }
            return View(model);
        }

        public async Task<ActionResult> CreateWaiverPayment(string ChangeOfCourseType)
        {
            var applicant = _studentQuery.GetApplicant(userId);

            var applicantPaymentVm = new ChangeOfCoursePaymentVm();
            var applicantsetting = new ChangeOfCourseFee();
            var id = "";
            var phoneNumber = "";
            var cSession = /*_query.GetCurrentSessionId((int)applicant.SchoolProgrammeId)*/ 20; //leave as 20 before publshing or find how session is saved for forms application

            if (!string.IsNullOrEmpty(ChangeOfCourseType))
            {
                applicantsetting = _db.ChangeOfCourseFees.AsNoTracking().Where(x => x.ChangeOfCourseType.Equals(ChangeOfCourseType)
                                        && x.SessionId.Equals(cSession)).FirstOrDefault();
                if (applicantsetting == null)
                {
                    ViewBag.Message = $"Fee has not been set for this Programme ({applicant.SchoolProgramme.FancyName})";
                    return View();
                }
            }
            else
            {
                ViewBag.Message = $"Please select the change of course type";
                return View();
            }

            //var hasTransactionProcessed = await _db.ChangeOfCoursePayments.AsNoTracking()
            //                         .Where(x => x.StudentId.Trim().Equals(student.StudentId.Trim())
            //                         && x.SessionId.Equals(sessionId) && x.IsPayed.Equals(true)
            //                         && x.ChangeOfCourseType.Equals(ChangeOfCourseType)
            //                         && x.IsProcessed.Equals(false)).FirstOrDefaultAsync();

            var hasTransactionProcessed = await _db.ApplicantWaiverPayments.Include(x => x.Applicant).AsNoTracking()
                                        .Where(x => x.ApplicantId.Trim().Equals(applicant.ApplicantEmail.Trim())
                                        && x.SessionId.Equals(cSession)
                                        && x.ChangeOfCourseType.Equals(ChangeOfCourseType)
                                        && x.IsProcessed.Equals(true)
                                        ).FirstOrDefaultAsync();
            if (hasTransactionProcessed != null)
            {
                return RedirectToAction("WaiverIndex");
            }

            var hasTransaction = await _db.ApplicantWaiverPayments.AsNoTracking()
                                     .Where(x => x.ApplicantId.Trim().ToUpper().Equals(applicant.ApplicantEmail.Trim().ToUpper())
                                     && x.SessionId.Equals(cSession) && x.IsPayed.Equals(true)
                                     && x.ChangeOfCourseType.Equals(ChangeOfCourseType)).FirstOrDefaultAsync();
            if (hasTransaction != null)
            {
                var hashed = _query.HashRemitedValidate(hasTransaction.OrderId, RemitaConfigParams.APIKEY, RemitaConfigParams.MERCHANTID);
                string checkurl = RemitaConfigParams.CHECKSTATUSURL + "/" + RemitaConfigParams.MERCHANTID + "/" + hasTransaction.OrderId + "/" + hashed + "/" + "orderstatus.reg";
                var checkResult = CheckExistingTransaction(checkurl);

                if (checkResult.Item1.Equals(true))
                {
                    if (string.IsNullOrEmpty(checkResult.Item2.Rrr))
                    {
                        var entry = _db.Entry(hasTransaction);
                        if (entry.State == EntityState.Detached)
                            _db.ApplicantWaiverPayments.Attach(hasTransaction);
                        _db.ApplicantWaiverPayments.Remove(hasTransaction);
                        _db.SaveChanges();
                    }
                    else
                    {
                        return RedirectToAction("ConfrimApplicationWaiverPayment", new { orderID = hasTransaction.OrderId });
                    }
                }
                else
                {
                    ViewBag.ErrorInfo = $"Check your internet connection and try again";
                    ViewBag.ErrorMessage = "Remita is currently unreachable";
                    return View("RemitaErrorPage");
                }
            }
            if (applicant != null)
            {
                id = applicant.ApplicantId;
                phoneNumber = applicant.PhoneNumber;
            }
            else
            {
                ViewBag.Message = $"Student Record Not Found";
                return View();
            }

            long milliseconds = DateTime.Now.Ticks;
            var url = Url.Action("ConfrimApplicationPayment", "ChangeOfCoursePayments", new { },
                                   protocol: Request.Url.Scheme);
            if (applicantsetting != null)
            {
                applicantPaymentVm.FullName = applicant.FullName;
                applicantPaymentVm.TotalAmount = applicantsetting.ApplicationFee;
                applicantPaymentVm.payerName = await _query.GetUserFullName(userId);
                applicantPaymentVm.payerEmail = userId;
                applicantPaymentVm.payerPhone = phoneNumber ?? "07030000000";
                applicantPaymentVm.amt = applicantsetting.ApplicationFee.ToString();
                applicantPaymentVm.merchantId = RemitaConfigParams.MERCHANTID;
                applicantPaymentVm.orderId = $"UJCOC{milliseconds.ToString()}";
                applicantPaymentVm.responseurl = url;
                applicantPaymentVm.StudentId =  applicant.ApplicantEmail;
                applicantPaymentVm.serviceTypeId = RemitaConfigParams.CHANGEOFCOURSE;
                applicantPaymentVm.ExpectedAmount = applicantsetting.ApplicationFee;
                applicantPaymentVm.SessionId = 20;
                applicantPaymentVm.apiKey = RemitaConfigParams.APIKEY;
                applicantPaymentVm.SessionName = /*_query.GetCurrentSessionName(applicantsetting.SchoolProgrammeId)*/ "2019/2020";

                var changeOfCoureType = from ChangeOfCourseType s in Enum.GetValues(typeof(ChangeOfCourseType))
                                        select new { ID = s, Name = s.ToString() };

                ViewBag.ChangeOfCourseType = new SelectList(changeOfCoureType, "Name", "Name");

                return View(applicantPaymentVm);
            }
            //ViewBag.Message = $"{applicantsetting.SchoolProgramme.FancyName} Form is no longer Active for sales";
            return View();
        }

        // POST: ApplicantPayments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateWaiverPayment(ChangeOfCoursePaymentVm model)
        {
            if (ModelState.IsValid)
            {

                var hasTransaction = await _db.ApplicantWaiverPayments.AsNoTracking()
                                        .Where(x => x.ApplicantId.Equals(model.StudentId) //StudentId = ApplicantId
                                        && x.SessionId.Equals(/*model.SessionId*/20)).FirstOrDefaultAsync();

                model.paymenttype = model.RemitaPaymentType.ToString().Replace("_", " ").ToLower();

                //_db.Entry(hasTransaction).State = EntityState.Deleted;
                //_db.SaveChangesAsync();
                if (hasTransaction != null)
                {
                    model.hash = _query.HashRemitaRequest(model.merchantId, model.serviceTypeId, hasTransaction.OrderId, model.amt, model.responseurl, RemitaConfigParams.APIKEY);
                    return RedirectToAction("ConfrimApplicationWaiverPayment", new { orderID = hasTransaction.OrderId });
                }
                var applicationPayment = new ApplicantWaiverPayment
                {
                    OrderId = model.orderId,
                    PaymentDateTime = DateTime.Now,
                    SessionId = /*model.SessionId*/20,
                    ApplicantId = model.StudentId,
                    TotalAmount = Convert.ToDecimal(model.amt),
                    ChangeOfCourseType = model.ChangeOfCourseType
                    //ReferenceNo = reference
                };
                _db.ApplicantWaiverPayments.Add(applicationPayment);
                var log = new RemitaPaymentLog
                {
                    OrderId = model.orderId,
                    PaymentName = "Change of Course Fee",
                    PaymentDate = DateTime.Now,
                    Amount = model.amt,
                    PayerName = model.payerName
                };
                _db.RemitaPaymentLogs.Add(log);
                await _db.SaveChangesAsync();
                model.hash = _query.HashRemitaRequest(model.merchantId, model.serviceTypeId, model.orderId, model.amt, model.responseurl, RemitaConfigParams.APIKEY);
                return RedirectToAction("SubmitRemita", model);
            }
            return View(model);
        }
        public ActionResult SubmitRemita(ApplicantPaymentVm model)
        {
            return View(model);
        }


        [AllowAnonymous]
        public async Task<ActionResult> ConfrimApplicationPayment(string RRR, string orderID)
        {
            ChangeOfCoursePayment applicationPayment;
            RemitaResponse result = new RemitaResponse();

            if (string.IsNullOrEmpty(orderID))
            {
                applicationPayment = await _db.ChangeOfCoursePayments.AsNoTracking()
                                            .Where(x => x.ReferenceNo.Equals(RRR.Trim()))
                                            .FirstOrDefaultAsync();
            }
            else
            {
                applicationPayment = await _db.ChangeOfCoursePayments.AsNoTracking()
                                            .Where(x => x.OrderId.Equals(orderID.Trim()))
                                            .FirstOrDefaultAsync();
            }
            if (applicationPayment != null)
            {
                if (applicationPayment.IsPayed.Equals(true))
                {
                    result.Message = applicationPayment.TransactionMessage;
                    result.OrderId = applicationPayment.OrderId;
                    result.Rrr = applicationPayment.ReferenceNo;
                    result.Status = applicationPayment.IsPayed.ToString();
                    return RedirectToAction("ConfrimRrrPayment", "RemitaPaymentLogs", result);
                }
                var log = await _db.RemitaPaymentLogs.AsNoTracking()
                                    .Where(x => x.OrderId.Equals(applicationPayment.OrderId))
                                    .FirstOrDefaultAsync();

                var hashed = _query.HashRemitedValidate(orderID, RemitaConfigParams.APIKEY, RemitaConfigParams.MERCHANTID);
                string url = RemitaConfigParams.CHECKSTATUSURL + "/" + RemitaConfigParams.MERCHANTID + "/" + orderID + "/" + hashed + "/" + "orderstatus.reg";
                string jsondata = new WebClient().DownloadString(url);
                result = JsonConvert.DeserializeObject<RemitaResponse>(jsondata);

                if (result.Status.Equals("00") || result.Status.Equals("01"))
                {
                    var appPayment =
                    applicationPayment.ReferenceNo = result.Rrr;
                    applicationPayment.IsPayed = true;
                    applicationPayment.TransactionMessage = result.Message;
                    _db.Entry(applicationPayment).State = EntityState.Modified;

                    _query.UpdateTransactionLog(log, result);
                    await _db.SaveChangesAsync();
                }
                else
                {
                    applicationPayment.ReferenceNo = result.Rrr;
                    applicationPayment.IsPayed = false;
                    applicationPayment.TransactionMessage = result.Message;
                    _db.Entry(applicationPayment).State = EntityState.Modified;

                    _query.UpdateTransactionLog(log, result);
                    await _db.SaveChangesAsync();
                    return RedirectToAction("RetryApplicationPayment", new { rrr = result.Rrr });
                }
                return RedirectToAction("Index");
            }
            var message = $"There is no payment that has either the RRR {RRR} or" +
                          $" Order Id {orderID} for Hostel Application Payment";
            return RedirectToAction("GetPaymentStatus", "RemitaServices", new { message });
        }

        public async Task<ActionResult> ConfrimApplicationWaiverPayment(string RRR, string orderID)
        {
            ApplicantWaiverPayment applicationPayment;
            RemitaResponse result = new RemitaResponse();

            if (string.IsNullOrEmpty(orderID))
            {
                applicationPayment = await _db.ApplicantWaiverPayments.AsNoTracking()
                                            .Where(x => x.ReferenceNo.Equals(RRR.Trim()))
                                            .FirstOrDefaultAsync();
            }
            else
            {
                applicationPayment = await _db.ApplicantWaiverPayments.AsNoTracking()
                                            .Where(x => x.OrderId.Equals(orderID.Trim()))
                                            .FirstOrDefaultAsync();
            }
            if (applicationPayment != null)
            {
                if (applicationPayment.IsPayed.Equals(true))
                {
                    result.Message = applicationPayment.TransactionMessage;
                    result.OrderId = applicationPayment.OrderId;
                    result.Rrr = applicationPayment.ReferenceNo;
                    result.Status = applicationPayment.IsPayed.ToString();
                    return RedirectToAction("ConfrimRrrPayment", "RemitaPaymentLogs", result);
                }
                var log = await _db.RemitaPaymentLogs.AsNoTracking()
                                    .Where(x => x.OrderId.Equals(applicationPayment.OrderId))
                                    .FirstOrDefaultAsync();

                var hashed = _query.HashRemitedValidate(orderID, RemitaConfigParams.APIKEY, RemitaConfigParams.MERCHANTID);
                string url = RemitaConfigParams.CHECKSTATUSURL + "/" + RemitaConfigParams.MERCHANTID + "/" + orderID + "/" + hashed + "/" + "orderstatus.reg";
                string jsondata = new WebClient().DownloadString(url);
                result = JsonConvert.DeserializeObject<RemitaResponse>(jsondata);

                if (result.Status.Equals("00") || result.Status.Equals("01"))
                {
                    var appPayment =
                    applicationPayment.ReferenceNo = result.Rrr;
                    applicationPayment.IsPayed = true;
                    applicationPayment.TransactionMessage = result.Message;
                    _db.Entry(applicationPayment).State = EntityState.Modified;

                    _query.UpdateTransactionLog(log, result);
                    await _db.SaveChangesAsync();
                }
                else
                {
                    applicationPayment.ReferenceNo = result.Rrr;
                    applicationPayment.IsPayed = false;
                    applicationPayment.TransactionMessage = result.Message;
                    _db.Entry(applicationPayment).State = EntityState.Modified;

                    _query.UpdateTransactionLog(log, result);
                    await _db.SaveChangesAsync();
                    return RedirectToAction("RetryApplicationPayment", new { rrr = result.Rrr });
                }
                return RedirectToAction("WaiverIndex");
            }
            var message = $"There is no payment that has either the RRR {RRR} or" +
                          $" Order Id {orderID} for Hostel Application Payment";
            return RedirectToAction("GetPaymentStatus", "RemitaServices", new { message });
        }

        [AllowAnonymous]
        public ActionResult RetryApplicationPayment(string rrr)
        {
            var hashrrr = _query.HashRrrQuery(rrr, RemitaConfigParams.APIKEY, RemitaConfigParams.MERCHANTID);
            string posturl = RemitaConfigParams.CHECKSTATUSURL + "/" + RemitaConfigParams.MERCHANTID + "/" + rrr + "/" + hashrrr + "/" + "status.reg";
            string jsondata = new WebClient().DownloadString(posturl);
            var result = JsonConvert.DeserializeObject<RemitaResponse>(jsondata);
            if (result.Status.Equals("00") || result.Status.Equals("01"))
            {
                return RedirectToAction("ConfrimApplicationPayment", "ChangeOfCoursePayments", new { RRR = result.Rrr, orderID = result.OrderId });
            }
            var hash = _query.HashRemitedRePost(RemitaConfigParams.MERCHANTID, rrr, RemitaConfigParams.APIKEY);
            var url = Url.Action("ConfrimApplicationPayment", "ChangeOfCoursePayments", new { },
                                   protocol: Request.Url.Scheme);
            var model = new RemitaRePostVm
            {
                rrr = rrr,
                merchantId = RemitaConfigParams.MERCHANTID,
                hash = hash,
                responseurl = url
            };
            return View(model);
        }



        // GET: ChangeOfCoursePayments/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ChangeOfCoursePayment changeOfCoursePayment = await _db.ChangeOfCoursePayments.FindAsync(id);
            if (changeOfCoursePayment == null)
            {
                return HttpNotFound();
            }
            return View(changeOfCoursePayment);
        }

        // POST: ChangeOfCoursePayments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            ChangeOfCoursePayment changeOfCoursePayment = await _db.ChangeOfCoursePayments.FindAsync(id);
            _db.ChangeOfCoursePayments.Remove(changeOfCoursePayment);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }



        //Logic for update of students' change of Course
        public ActionResult PrcocessChangeOfCourseList()
        {
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FancyName");
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");

            var changeOfCoureType = from ChangeOfCourseType s in Enum.GetValues(typeof(ChangeOfCourseType))
                                    select new { ID = s, Name = s.ToString() };

            var staffRecord = _db.Staffs.Include(i => i.Department.Faculty)
                                .FirstOrDefault(x => x.Email.ToUpper().Trim().Equals(userId.Trim().ToUpper()));


            if (User.IsInRole(RoleName.Dean))
            {
                ViewBag.FacultyId = new SelectList(_db.Faculties.AsNoTracking().Where(x => x.FacultyId.Equals(staffRecord.Department.FacultyId)), "FacultyId", "FacultyName");
                ViewBag.DepartmentId = new SelectList(_db.Departments.AsNoTracking().Where(x => x.FacultyId.Equals(staffRecord.Department.FacultyId)), "DepartmentId", "DeptName");
                ViewBag.ProgrammeId = new SelectList(_db.Programmes.Include(i => i.Department).AsNoTracking().Where(x => x.Department.FacultyId.Equals(staffRecord.Department.FacultyId)), "ProgrammeId", "ProgrammeName");
            }
            else if (User.IsInRole(RoleName.Hod))
            {
                ViewBag.FacultyId = new SelectList(_db.Faculties.AsNoTracking().Where(x => x.FacultyId.Equals(staffRecord.Department.FacultyId)), "FacultyId", "FacultyName");
                ViewBag.DepartmentId = new SelectList(_db.Departments.AsNoTracking().Where(x => x.DepartmentId.Equals(staffRecord.Department.DepartmentId)), "DepartmentId", "DeptName");
                ViewBag.ProgrammeId = new SelectList(_db.Programmes.Include(i => i.Department).AsNoTracking().Where(x => x.Department.FacultyId.Equals(staffRecord.Department.FacultyId)), "ProgrammeId", "ProgrammeName");
            }
            else
            {
                ViewBag.FacultyId = new SelectList(_db.Faculties.AsNoTracking(), "FacultyId", "FacultyName");
                ViewBag.DepartmentId = new SelectList(_db.Departments.AsNoTracking(), "DepartmentId", "DeptName");
                ViewBag.ProgrammeId = new SelectList(_db.Programmes.AsNoTracking(), "ProgrammeId", "ProgrammeName");
                ViewBag.ChangeOfCourseType = new SelectList(changeOfCoureType, "Name", "Name");
            }
            return View();
        }


        public async Task<ActionResult> ProcessChangeOfCourseGetIndex(int? SchoolProgrammeId, int? FacultyId, int? DepartmentId, int? ProgrammeId, int? SessionId, string ChangeOfCourseType)
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



            var changeOfCourseVm = new List<ChangeOfCourseReportVm>();
            if (SchoolProgrammeId != null)
            {
                //var sessionID = _query.GetCurrentSessionId((int)SchoolProgrammeId);
                var changeOfCourseList = await _db.ChangeOfCoursePayments.Include(c => c.Session)
                                        .Include(i => i.Student)
                                        .Include(i => i.Student.Programme)
                                        .Include(i => i.Programme.Department.Faculty)
                                        .Where(x => x.SessionId.Equals((Int32)SessionId)
                                        && x.IsProcessed.Equals(false) && x.ProgrammeId != null
                                        && x.Student.SchoolProgrammeId.Equals((Int32)SchoolProgrammeId)
                                        && x.ChangeOfCourseType.Equals(ChangeOfCourseType)).ToListAsync();


                if (ProgrammeId != null)
                {
                    changeOfCourseList = changeOfCourseList.Where(x => x.Programme.ProgrammeId.Equals((int)ProgrammeId) && x.IsProcessed.Equals(false)).ToList();
                }
                if (DepartmentId != null)
                {
                    changeOfCourseList = changeOfCourseList.Where(x => x.Programme.Department.DepartmentId.Equals((int)DepartmentId) && x.IsProcessed.Equals(false)).ToList();
                }
                if (ChangeOfCourseType != null)
                {
                    changeOfCourseList = changeOfCourseList.Where(x => x.ChangeOfCourseType.Equals(ChangeOfCourseType.ToString()) && x.IsProcessed.Equals(false)).ToList();
                }
                if (FacultyId != null)
                {
                    changeOfCourseList = changeOfCourseList.Where(x => x.Programme.Department.FacultyId.Equals((int)FacultyId) && x.IsProcessed.Equals(false)).ToList();
                }

                //if (!string.IsNullOrEmpty(search))
                //{
                //    changeOfCourseList = changeOfCourseList.Where(x => x.Student.JambRegNo.ToUpper().Contains(search.ToUpper().Trim())
                //                || x.Student.FullName.Trim().ToUpper().Equals(search.ToUpper().Trim())).ToList();
                //}

                if (!string.IsNullOrEmpty(search))
                {
                    changeOfCourseList = changeOfCourseList.Where(x => x.Student.JambRegNo.ToUpper().Contains(search.ToUpper().Trim())
                                || (x.Student.MatricNo != null && x.Student.MatricNo.ToUpper() == search.ToUpper().Trim())
                                || x.Student.FullName.Trim().ToUpper().Equals(search.ToUpper().Trim())).ToList();
                }

                changeOfCourseVm = MapToChangeOfCourseVm(changeOfCourseList);

            }

            totalRecords = changeOfCourseVm.Count();
            var data = changeOfCourseVm.Skip(skip).Take(pageSize).ToList();

            return Json(new { draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data },
                JsonRequestBehavior.AllowGet);

            #endregion Server Side filtering
        }


        public async Task<ActionResult> ProcessChangeOfCourseView(string studentId)
        {
            var student = await _db.Students.Include(s => s.Programme)
                                      .Include(s => s.Level).AsNoTracking()
                                      .Where(s => s.StudentId.Equals(studentId))
                                      .Select(s => new
                                      {
                                          s.Level.LevelName,
                                          s.Programme.ProgrammeName,
                                          s.LevelId,
                                          s.ProgrammeId,
                                          s.SchoolProgrammeId
                                      }).FirstOrDefaultAsync();

            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FancyName");
            ViewBag.ProgrammeId = new SelectList(_db.Programmes.AsNoTracking(), "ProgrammeId", "ProgrammeName");
            ViewBag.LevelId = new SelectList(_db.Levels.AsNoTracking(), "LevelId", "LevelName");
            ViewBag.FacultyId = new SelectList(_db.Faculties.AsNoTracking(), "FacultyId", "FacultyName");
            ViewBag.DepartmentId = new SelectList(_db.Departments.AsNoTracking(), "DepartmentId", "DeptName");
            var staff = _db.Staffs.FirstOrDefault(x => x.Email.Trim().ToUpper().Equals(userId.Trim().ToUpper()));
            if (!string.IsNullOrEmpty(studentId) && staff != null)
            {
                var model = new ProcessChangeOfCourseVM()
                {
                    StudentId = studentId,
                    OldprogrammeId = student.ProgrammeId,
                    OldLevelId = student.LevelId,
                    AuthorId = staff.StaffId,
                    OldprogrammeName = student.ProgrammeName,
                    OldLevelName = student.LevelName,
                    SessionAppliedForCOC = _query.GetCurrentSessionId((int)student.SchoolProgrammeId)
                };
                return PartialView(model);
            }
            ViewBag.Message = "Empty Student Id";
            return PartialView();
        }

        [HttpPost]
        public async Task<ActionResult> ProcessChangeOfCourse(ProcessChangeOfCourseVM model)
        {

            var student = await _db.Students.FindAsync(model.StudentId);
            var updateCOC = await _db.ChangeOfCoursePayments.Where(coc => coc.StudentId.Equals(model.StudentId) 
                                    && coc.SessionId.Equals(model.SessionAppliedForCOC) 
                                    && coc.IsProcessed.Equals(false))
                                    .FirstOrDefaultAsync();

            updateCOC.IsProcessed = true;                       //set COCPayment to processed

            student.ProgrammeId = model.ProgrammeId;         //Change stduent's programme
            student.LevelId = model.LevelId;         //Change stduent's programme
            student.IsClearedAcademics = false;     //set clearances to false to enable student edit biodata
            student.IsClearedDepartment = false;
            student.IsClearedFaculty = false;

            ChangeOfCourseHistory newCOCHostory = new ChangeOfCourseHistory
            {
                StaffId = model.AuthorId,
                StudentId = model.StudentId,
                OldLevelId = (int)model.OldLevelId,
                NewLevelId = (int)model.LevelId,
                ChangeType = "Change of Course",
                DateOfChange = DateTime.Now,
                OldProgrammeId = (int)model.OldprogrammeId,
                NewProgrammeId = model.ProgrammeId,
            };

            _db.ChangeOfCourseHistory.Add(newCOCHostory);
            _db.Entry(student).State = EntityState.Modified;
            _db.Entry(updateCOC).State = EntityState.Modified;
            try
            {
                await _db.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                Console.WriteLine(" " + ex.Message);
                return new JsonResult { Data = new { status = true, message = ex.Message } };
            }
            return new JsonResult { Data = new { status = true, message = "Student's Course Changed Successfully" } };

        }

        public ActionResult SuccessfulChangedCourses()
        {
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FancyName");


            var staffRecord = _db.Staffs.Include(i => i.Department.Faculty)
                                .FirstOrDefault(x => x.Email.ToUpper().Trim().Equals(userId.Trim().ToUpper()));


            if (User.IsInRole(RoleName.Dean))
            {
                ViewBag.FacultyId = new SelectList(_db.Faculties.AsNoTracking().Where(x => x.FacultyId.Equals(staffRecord.Department.FacultyId)), "FacultyId", "FacultyName");
                ViewBag.DepartmentId = new SelectList(_db.Departments.AsNoTracking().Where(x => x.FacultyId.Equals(staffRecord.Department.FacultyId)), "DepartmentId", "DeptName");
                ViewBag.ProgrammeId = new SelectList(_db.Programmes.Include(i => i.Department).AsNoTracking().Where(x => x.Department.FacultyId.Equals(staffRecord.Department.FacultyId)), "ProgrammeId", "ProgrammeName");
            }
            else if (User.IsInRole(RoleName.Hod))
            {
                ViewBag.FacultyId = new SelectList(_db.Faculties.AsNoTracking().Where(x => x.FacultyId.Equals(staffRecord.Department.FacultyId)), "FacultyId", "FacultyName");
                ViewBag.DepartmentId = new SelectList(_db.Departments.AsNoTracking().Where(x => x.DepartmentId.Equals(staffRecord.Department.DepartmentId)), "DepartmentId", "DeptName");
                ViewBag.ProgrammeId = new SelectList(_db.Programmes.Include(i => i.Department).AsNoTracking().Where(x => x.Department.FacultyId.Equals(staffRecord.Department.FacultyId)), "ProgrammeId", "ProgrammeName");
            }
            else
            {
                ViewBag.FacultyId = new SelectList(_db.Faculties.AsNoTracking(), "FacultyId", "FacultyName");
                ViewBag.DepartmentId = new SelectList(_db.Departments.AsNoTracking(), "DepartmentId", "DeptName");
                ViewBag.ProgrammeId = new SelectList(_db.Programmes.AsNoTracking(), "ProgrammeId", "ProgrammeName");
            }
            return View();
        }

        public async Task<ActionResult> ChangedCourseGetIndex(int? SchoolProgrammeId, int? FacultyId, int? DepartmentId, int? ProgrammeId)
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



            var changeOfCourseVm = new List<ChangeOfCourseReportVm>();
            if (SchoolProgrammeId != null)
            {
                var sessionID = _query.GetCurrentSessionId((int)SchoolProgrammeId);
                var changeOfCourseList = await _db.ChangeOfCoursePayments.Include(c => c.Session).Include(i => i.Student.Programme)
                                        .Include(i => i.Programme.Department.Faculty).Where(x => x.SessionId.Equals(sessionID) && x.IsProcessed.Equals(false)).ToListAsync();


                if (ProgrammeId != null)
                {
                    changeOfCourseList = changeOfCourseList.Where(x => x.Programme.ProgrammeId.Equals((int)ProgrammeId) && x.IsProcessed.Equals(false)).ToList();
                }
                else if (DepartmentId != null)
                {
                    changeOfCourseList = changeOfCourseList.Where(x => x.Programme.Department.DepartmentId.Equals((int)DepartmentId) && x.IsProcessed.Equals(false)).ToList();
                }
                else if (FacultyId != null)
                {
                    changeOfCourseList = changeOfCourseList.Where(x => x.Programme.Department.FacultyId.Equals((int)FacultyId) && x.IsProcessed.Equals(false)).ToList();
                }

                if (!string.IsNullOrEmpty(search))
                {
                    changeOfCourseList = changeOfCourseList.Where(x => x.Student.JambRegNo.ToUpper().Contains(search.ToUpper().Trim())
                                || x.Student.FullName.Trim().ToUpper().Equals(search.ToUpper().Trim())).ToList();
                }

                changeOfCourseVm = MapToChangeOfCourseVm(changeOfCourseList);

            }

            totalRecords = changeOfCourseVm.Count();
            var data = changeOfCourseVm.Skip(skip).Take(pageSize).ToList();

            return Json(new { draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data },
                JsonRequestBehavior.AllowGet);

            #endregion Server Side filtering
        }

        // GET: WaiverApplicants
        public ActionResult ApplicantWaiver(string message)
        {
            ViewBag.Message = message;
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            ViewData.Add("ActionMessage", "Utme Applicants list View");
            return View();
        }

        public async Task<ActionResult> GetApplicantWaiverIndex(string hasRegistered, string isDirectEntry, int? SessionId)
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

            var utmeIndex = new List<ApplicantWaiverPaymentVm>();
            var utmeApplicants = new List<ApplicantWaiverPayment>();
            var HasRegistered = hasRegistered.Equals("False") ? false : true;
            var IsDirectEntry = isDirectEntry.Equals("False") ? false : true;
            if (SessionId != null)
            {
                utmeApplicants = await _db.ApplicantWaiverPayments.Include(i => i.Programme).AsNoTracking()
                                            .Include(i => i.Applicant).Include(i=>i.Session)
                                   .Where(x => x.SessionId.Equals((int)SessionId) && x.IsPayed.Equals(HasRegistered) && x.IsExpired.Equals(IsDirectEntry)).ToListAsync();

            }
            else
            {
                utmeApplicants = await _db.ApplicantWaiverPayments.Include(i => i.Programme).AsNoTracking()
                                            .Include(i => i.Applicant).Include(i=>i.Session)
                                  .Where(x => x.SessionId.Equals(sessionId)&& x.IsPayed.Equals(HasRegistered) && x.IsExpired.Equals(IsDirectEntry)).ToListAsync();
            }

            if (!string.IsNullOrEmpty(search))
            {

                var v = utmeApplicants.Where(x => x.Applicant.ApplicantId.Trim().ToUpper().Contains(search.Trim().ToUpper()))
                                        .ToList();
                // Mapping the student to the correct ViewModel for json display
                utmeIndex = MapToWaiverIndex(v);
            }
            //else if (!string.IsNullOrEmpty(isDirectEntry) && !string.IsNullOrEmpty(hasRegistered))
            //{
            //    var v = utmeApplicants.Where(x => x.IsDirectEntry.ToString().ToUpper().Equals(isDirectEntry.ToUpper())
            //                            && x.HasRegistered.ToString().ToUpper().Equals(hasRegistered.ToUpper()))
            //                           .ToList();
            //    // Mapping the student to the correct ViewModel for json display
            //    utmeIndex = MapToUtmeIndex(v);
            //}
            else if (!string.IsNullOrEmpty(hasRegistered))
            {
                var v = utmeApplicants.Where(x => x.IsPayed.ToString().ToUpper().Equals(hasRegistered.ToUpper()))
                                       .ToList();
                // Mapping the student to the correct ViewModel for json display
                utmeIndex = MapToWaiverIndex(v);
            }
            //else if (!string.IsNullOrEmpty(isDirectEntry))
            //{
            //    var v = utmeApplicants.Where(x => x.IsDirectEntry.ToString().ToUpper().Equals(isDirectEntry.ToUpper()))
            //                           .ToList();
            //    // Mapping the student to the correct ViewModel for json display
            //    utmeIndex = MapToUtmeIndex(v);
            //}
            else
            {
                var v = utmeApplicants;
                // Mapping the student to the correct ViewModel for json display
                utmeIndex = MapToWaiverIndex(v);
            }

            totalRecords = utmeIndex.Count();
            var data = utmeIndex.Skip(skip).Take(pageSize).ToList();
            ViewData.Add("ActionMessage", $"{data.Count} List of Utme Applicants");

            return Json(new { draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data },
                JsonRequestBehavior.AllowGet);

            #endregion Server Side filtering
        }

        private List<ApplicantWaiverPaymentVm> MapToUtmeIndex(List<ApplicantWaiverPayment> v)
        {
            
            var utmeApplicant = v.Select(s => new ApplicantWaiverPaymentVm()
            {
                JambRegNo = s.Applicant.ApplicantId,
                MiddleName = "",
                StateOfOrigin = s.Applicant.StateOfOrigin,
                Lga = s.Programme.ProgrammeName,
                ResultGrade = "",   //Jamb Score
                FullName = $"{s.Applicant.LastName} {s.Applicant.FirstName}",
                Email = s.Applicant.ApplicantEmail,
                

            }).ToList();

            return utmeApplicant;
        }

        private List<ApplicantWaiverPaymentVm> MapToWaiverIndex(List<ApplicantWaiverPayment> v)
        {
            var utmeApplicant = new List<ApplicantWaiverPaymentVm>();
            foreach (var item in v)
            {
                
                var checkApplicant = _db.Applicants.Include(x => x.AvailableCourse).Where(x => x.ApplicantEmail.Trim().ToUpper().Equals(item.ApplicantId.Trim().ToUpper())).FirstOrDefault();
                var grade = _db.AttendedSchools .Where(x => x.ApplicantId.Trim().ToUpper().Equals(checkApplicant.ApplicantEmail.Trim().ToUpper()))/*.Select(x => x.ResultGrade)*/.ToList();
                var workPlace = _db.EmploymentDetails.Where(x => x.ApplicantId.Equals(checkApplicant.ApplicantEmail)).Select(x => x.EmployeeName).FirstOrDefault().ToString();
                utmeApplicant.Add(new ApplicantWaiverPaymentVm
                {
                    JambRegNo = checkApplicant.ApplicantId,
                    MiddleName = workPlace,
                    StateOfOrigin = checkApplicant.StateOfOrigin,
                    Lga = checkApplicant.AvailableCourse.ProgrammeName,
                    ResultGrade = GradeToString(grade),
                    FullName = $"{checkApplicant.LastName} {checkApplicant.FirstName}",
                    Email = workPlace,
                    PGRecommendation = ""

                });
            }
            return utmeApplicant;
        }

        private String GradeToString(List<AttendedSchool> grades)
        {
            string result = "";
            foreach (var item in grades)
            {
                result += item.SchoolName + " - "+ item.ResultGrade + ", ";
            }
         
            return result;
        }

        public async Task<ActionResult> ProcesWaiverApplication(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                var student = await _db.Applicants.Include(i => i.SchoolProgramme).AsNoTracking()
                                                  .Include(i => i.AvailableCourse.Programme)
                                .Where(x => x.ApplicantId.Equals(id)).FirstOrDefaultAsync();
                var fullName = student.FullName;

                var changeOfCoursePayments = _db.ApplicantWaiverPayments.Include(c => c.Session).Include(i => i.Applicant)
                                                        .Include(i => i.Programme)
                                                        .Where(x => x.ApplicantId.Trim().ToUpper().Equals(student.ApplicantEmail.Trim().ToUpper())).FirstOrDefault();

                string SMSbody = $"Dear {student.FirstName}, the VC UNIJOS has graciously approved your waiver application for {student.AvailableCourse.Programme.ProgrammeName}. Wish you luck with your admission";

                if (changeOfCoursePayments != null)
                {

                    changeOfCoursePayments.IsProcessed = true; //Approved
                    changeOfCoursePayments.IsExpired = true;
                    //changeOfCoursePayments. = true;

                    _db.Entry(changeOfCoursePayments).State = EntityState.Modified;
                    _db.SaveChanges();
                    await SMSClass.SendSMS("UNIJOS SIS", SMSbody, student.PhoneNumber); //EBULK SMS API

                    return new JsonResult { Data = new { status = true, message = $"{fullName} has been approved successfully." } };

                }
                else
                {
                }
                //student.IsClearedDepartment = true;
                _db.Entry(student).State = EntityState.Modified;
                await _db.SaveChangesAsync();

                return new JsonResult { Data = new { status = true, message = $"{student.FullName} Waiver application has been approved successfully" } };
            }
            return new JsonResult { Data = new { status = false, message = "Oops.. Something went wrong, Applicant NOT found" } };
            //return View(subject);
        }

        public async Task<ActionResult> DisaproveWaiverApplication(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                var student = await _db.Applicants.Include(i => i.SchoolProgramme).AsNoTracking()
                                                  .Include(i => i.AvailableCourse.Programme)
                                .Where(x => x.ApplicantId.Equals(id)).FirstOrDefaultAsync();
                var fullName = student.FullName;

                var changeOfCoursePayments = _db.ApplicantWaiverPayments.Include(c => c.Session).Include(i => i.Applicant)
                                                        .Include(i => i.Programme)
                                                        .Where(x => x.ApplicantId.Trim().ToUpper().Equals(student.ApplicantEmail.Trim().ToUpper())).FirstOrDefault();

                string SMSbody = $"Dear {student.FirstName}, sorry your waiver application for {student.AvailableCourse.Programme.ProgrammeName} is not approved. Wish you better luck next time.";

                if (changeOfCoursePayments != null)
                {

                    changeOfCoursePayments.IsProcessed = false; //Not Approved
                    changeOfCoursePayments.IsExpired = true;
                    //changeOfCoursePayments.is = true;

                    _db.Entry(changeOfCoursePayments).State = EntityState.Modified;
                    _db.SaveChanges();
                    await SMSClass.SendSMS("UNIJOS SIS", SMSbody, student.PhoneNumber); //EBULK SMS API

                    return new JsonResult { Data = new { status = true, message = $"{fullName} has been disapproved successfully." } };

                }
                else
                {
                }
                //student.IsClearedDepartment = true;
                _db.Entry(student).State = EntityState.Modified;
                await _db.SaveChangesAsync();

                return new JsonResult { Data = new { status = true, message = $"{student.FullName} Waiver application has been dis-approved successfully" } };
            }
            return new JsonResult { Data = new { status = false, message = "Oops.. Something went wrong, Applicant NOT found" } };
            //return View(subject);
        }
    }
}
