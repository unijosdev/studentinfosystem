using SwiftKampus.Models;
using System.Linq;
using System.Data.Entity;
using System.Web.Mvc;
using System.Threading.Tasks;
using SwiftKampusModel;
using System;
using System.Collections.Generic;
using SwiftKampus.ViewModels;
using SwiftKampus.Services;
using Microsoft.AspNet.Identity;
using SwiftKampusModel.Payment;

namespace SwiftKampus.Controllers
{
    [Authorize]
    [Audit(AuditingLevel = 2)]
    public class IdCardsController : BaseController
    {
        private List<Session> _sessions;
        private List<Level> _levels;
        public IdCardsController(SchoolDbContext db) : base(db)
        {
            _sessions = GetAllSession();
            _levels = GetLevelList();
        }
        // GET: IdCards
        public ActionResult NewStudentCard()
        {
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FancyName");
            ViewBag.FacultyId = new SelectList(_db.Faculties.AsNoTracking(), "FacultyId", "FacultyName");
            ViewBag.DepartmentId = new SelectList(_db.Departments.AsNoTracking(), "DepartmentId", "DeptName");
            ViewBag.ProgrammeId = new SelectList(_db.Programmes.AsNoTracking(), "ProgrammeId", "ProgrammeName");
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            return View();
        }

        public async Task<ActionResult> NewStudentIdCard(int SchoolProgrammeId, int? ProgrammeId, string IsPrinted, string IsDispatch, int? SessionId, int? FacultyId)
        {
            //if(ProgrammeId == null)
            //{
            //    ViewBag.Message = "Please select a programme";
            //    return PartialView();
            //}

            bool isPrinted = !string.IsNullOrEmpty(IsPrinted) && IsPrinted.ToLower().Equals("true") ? true : false;
            bool isDispatch = !string.IsNullOrEmpty(IsDispatch) && IsDispatch.ToLower().Equals("true") ? true : false;

            var students = await _db.IdCardPayments.Include(c => c.Student.Session).Include(i => i.Student)
                                        .Include(i => i.Student.Programme).Include(i => i.Student.Programme.Department.Faculty)
                                        .Where(x => x.Student.SchoolProgrammeId.Equals((int)SchoolProgrammeId)
                                        && x.IsPayed.Equals(true)
                                        && !string.IsNullOrEmpty(x.Student.BloodGroup))
                                        .ToListAsync();
            if (FacultyId != null)
            {
                students = students.Where(x => x.Student.Programme.Department.Faculty.FacultyId.Equals(FacultyId)).ToList();
            }
            if (ProgrammeId != null)
            {
                students = students.Where(x => x.Student.Programme.ProgrammeId.Equals((int)ProgrammeId)).ToList();
            }

            if (SessionId != null)
            {
                students = students.Where(x => x.Student.SessionId.Equals((int)SessionId)).ToList();
            }
            if (isPrinted)
            {
                students = students.Where(s => s.IsPrinted.Equals(isPrinted)).ToList();
            }
            else
            {
                students = students.Where(s => s.IsPrinted != true).ToList();
            }
            if (!string.IsNullOrEmpty(IsDispatch))
            {
                if (isDispatch)
                {
                    students = students.Where(s => s.IsDispatched.Equals(isDispatch)).ToList();
                }
                else
                {
                    students = students.Where(s => s.IsDispatched != true).ToList();
                }
            }

            var model = GetIdCardPrintVm(students);
            return PartialView(model);
        }

        // handles ID Card request for a single student
        public async Task<ActionResult> NewSingleStudentIdCard(string id)
        {

            //bool isPrinted = !string.IsNullOrEmpty(IsPrinted) && IsPrinted.ToLower().Equals("true") ? true : false;
            //bool isDispatch = !string.IsNullOrEmpty(IsDispatch) && IsDispatch.ToLower().Equals("true") ? true : false;

            var students = await _db.IdCardPayments.Include(c => c.Student.Session).Include(i => i.Student)
                                        .Include(i => i.Student.Programme).Include(i => i.Student.Programme.Department.Faculty)
                                        .Where(x => x.Student.MatricNo.ToUpper().Equals(id.Trim().ToUpper())
                                        && x.IsPayed.Equals(true)
                                        && !string.IsNullOrEmpty(x.Student.BloodGroup))
                                        .ToListAsync();

            var model = GetIdCardPrintVm(students);
            return PartialView("NewStudentIdCard", model);
        }

        private List<IdCardPrintVm> GetIdCardPrintVm(List<IdCardPayment> students)
        {
            var model = new List<IdCardPrintVm>();
            foreach (var payment in students)
            {
                var student = payment.Student;
                var splitSessionName = student.Session.SessionName.Split('/', '-');
                int year = Convert.ToInt16(splitSessionName[0]) + Convert.ToInt16(student.Programme.NoOfSemesters) / 2;

                model.Add(new IdCardPrintVm()
                {
                    StudentId = student.StudentId,
                    FullName = student.FullName,
                    MatricNumber = student.MatricNo,
                    ExpiringYear = year.ToString(),
                    ProgrammeName = student.Programme.ProgrammeName,
                    BloodGroup = string.IsNullOrEmpty(student.BloodGroup) ? " " : student.BloodGroup,
                    PrintCount = Convert.ToInt16(payment.NoOfCount)
                });
            }
            return model;
        }

        public async Task<ActionResult> ViewIdCard(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                var student = await _db.Students.Include(i => i.Programme.FinalLevel).Include(i => i.SchoolProgramme)
                             .Include(i => i.Session).Include(i => i.Programme).AsNoTracking()
                             .Where(x => x.StudentId.Equals(id)
                             && x.IsDelete.Equals(false))
                             .FirstOrDefaultAsync();
                var splitSessionName = student.Session.SessionName.Split('/', '-');
                ViewBag.ExpiryDate = Convert.ToInt16(splitSessionName[0]) + student.Programme.NoOfSemesters / 2;
                return View(student);
            }
            return View();
        }

        public async Task<ActionResult> MarkAsPrint(List<string> StudentId)
        {
            if (StudentId != null && StudentId.Count() > 0)
            {
                foreach (var item in StudentId)
                {
                    //var student = await _db.Students.FindAsync(item);
                    var student = await _db.IdCardPayments.Include(c => c.Student.Session).Include(i => i.Student)
                                      .Include(i => i.Student.Programme)
                                      .Where(x => x.StudentId.Equals(item)
                                      && x.IsPayed.Equals(true))
                                      .FirstOrDefaultAsync();
                    student.IsPrinted = true;
                    student.DatePrinted = DateTime.Now;
                    student.NoOfCount = student.NoOfCount != null ? student.NoOfCount + 1 : 1;
                    _db.Entry(student).State = EntityState.Modified;
                }
                await _db.SaveChangesAsync();
                return new JsonResult { Data = new { status = true, message = $"{StudentId.Count()} ID cards has been marked as print successfully" } };
            }

            return new JsonResult { Data = new { status = false, message = $"{StudentId.Count()} card sent. Please check student and try again..." } };

        }

        public async Task<ActionResult> DispatchAsPrint(List<string> StudentId)
        {
            if (StudentId != null && StudentId.Count() > 0)
            {
                foreach (var item in StudentId)
                {
                    //var student = await _db.Students.FindAsync(item);
                    var student = await _db.IdCardPayments.Include(c => c.Student.Session).Include(i => i.Student)
                                      .Include(i => i.Student.Programme)
                                      .Where(x => x.StudentId.Equals(item)
                                      && x.IsPayed.Equals(true) && x.IsProcessed.Equals(false))
                                      .FirstOrDefaultAsync();
                    student.IsDispatched = true;
                    student.DateDispatched = DateTime.Now;
                    _db.Entry(student).State = EntityState.Modified;

                    var emailService = new EmailService();
                    await emailService.SendAsync(new IdentityMessage
                    {
                        Destination = student.Student.Email,
                        Body = $"Hello {student.Student.FullName}, your Id Card is ready for collection.",
                        Subject = "ID CARD"
                    });
                }
                await _db.SaveChangesAsync();
                return new JsonResult { Data = new { status = true, message = $"{StudentId.Count()} ID cards has been dispatched successfully" } };
            }

            return new JsonResult { Data = new { status = false, message = $"{StudentId.Count()} card sent. Please check student and try again..." } };

        }
    }
}