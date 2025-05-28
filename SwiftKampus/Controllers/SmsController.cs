using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampus.ViewModels.Sms;
using SwiftKampusModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SwiftKampus.Controllers
{
    [Authorize]
    [Audit(AuditingLevel = 2)]
    public class SmsController : BaseController
    {
        readonly CustomSms cs;
        private readonly SmsServiceTemp _smsService;

        public SmsController(SchoolDbContext db) : base(db)
        {
            cs = new CustomSms();
            _smsService = new SmsServiceTemp();
        }


        [HttpGet]
        public ActionResult SendSMS()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> SendSMS(SmsViewModel model)
        {

            if (ModelState.IsValid)
            {
                await SendUnkownSmsMessage(model.Destination, model.Message);
            }

            return View(model);
        }

        [HttpGet]
        public ActionResult SendToStaff()
        {
            //SMS sms = new SMS();
            // ViewBag.ContactGroup = new SelectList(db.Classes, "FullClassName", "FullClassName");
            return View();
        }

        [HttpPost]
        public ActionResult SendToStaff(SendToStaffViewModel model)
        {

            if (ModelState.IsValid)
            {
                //var staffList = _db.Staffs.Select(x => x.PhoneNumber);
                //foreach (var staffNumber in staffList)
                //{
                //    SMS sms = new SMS()
                //    {
                //        SenderId = model.SenderId,
                //        Message = model.Message,
                //        Numbers = staffNumber
                //    };
                //    try
                //    {
                //        bool isSuccess = false;
                //        string errMsg = null;
                //        string response = _smsService.Send(sms); //Send sms

                //        string code = _smsService.GetResponseMessage(response, out isSuccess, out errMsg);

                //        if (!isSuccess)
                //        {
                //            ModelState.AddModelError("", errMsg);
                //        }
                //        else
                //        {
                //            ViewBag.Message = "Message was successfully sent.";
                //        }
                //    }
                //    catch (Exception ex)
                //    {
                //        ModelState.AddModelError("", ex.Message);
                //    }


                // }
            }

            return View(model);
        }


        [HttpGet]
        public ActionResult SendtoAllStudent()
        {
            var type = from StudentSms s in Enum.GetValues(typeof(StudentSms))
                       select new { ID = s, Name = s.ToString() };


            ViewBag.StudentCategory = new SelectList(type, "Name", "Name");
            ViewBag.Department = new MultiSelectList(_db.Departments, "DepartmentId", "DeptName");
            return View();
        }

        [HttpPost]
        //public async Task<ActionResult> SendtoAllStudent(SendToAllStudentVm model)
        //{

        //    if (ModelState.IsValid)
        //    {
        //        if (model.Department == null || model.Department.Length == 0)
        //        {
        //            if (model.StudentCategory.Equals(StudentSms.All_Student.ToString()))
        //            {
        //                var studentList = await _db.Students.AsNoTracking().Where(x => x.Active.Equals(true)).ToListAsync();
        //                foreach (var student in studentList)
        //                {
        //                    await SendSmsMessage(student.StudentId, model.Message);
        //                }
        //            }
        //            if (model.StudentCategory.Equals(StudentSms.Registered_Student.ToString()))
        //            {
        //                var semesterId = await _db.Semesters.AsNoTracking().Where(x => x.ActiveSemester.Equals(true))
        //                    .Select(s => s.SemesterId).FirstOrDefaultAsync();
        //                var sessionId = await _db.Sessions.AsNoTracking().Where(x => x.ActiveSession.Equals(true))
        //                    .Select(s => s.SessionId).FirstOrDefaultAsync();

        //                var registeredStudent = _db.CourseRegistrations.Where(x => x.SemesterId.Equals(semesterId)
        //                                                                           && x.SessionId.Equals(sessionId))
        //                    .Select(s => s.StudentId).AsParallel().Distinct();
        //                foreach (var student in registeredStudent)
        //                {
        //                    await SendSmsMessage(student, model.Message);
        //                }
        //            }
        //            if (model.StudentCategory.Equals(StudentSms.All_Staff.ToString()))
        //            {
        //                var staffList = await _db.Staffs.AsNoTracking().Where(x => x.IsActiveStaff.Equals(true)).ToListAsync();
        //                foreach (var staff in staffList)
        //                {
        //                    await SendSmsMessage(staff.StaffId, model.Message);
        //                }
        //            }
        //        }
        //        if (model.Department != null)
        //        {
        //            foreach (var item in model.Department)
        //            {
        //                if (model.StudentCategory.Equals(StudentSms.All_Student.ToString()))
        //                {
        //                    var studentList = await _db.Students.AsNoTracking().Where(x => x.Active.Equals(true)
        //                                    && x.Programme.DepartmentId.Equals(item)).ToListAsync();
        //                    foreach (var student in studentList)
        //                    {
        //                        await SendSmsMessage(student.StudentId, model.Message);
        //                    }
        //                }
        //                if (model.StudentCategory.Equals(StudentSms.Registered_Student.ToString()))
        //                {
        //                    var semesterId = await _db.Semesters.AsNoTracking().Where(x => x.ActiveSemester.Equals(true))
        //                        .Select(s => s.SemesterId).FirstOrDefaultAsync();
        //                    var sessionId = await _db.Sessions.AsNoTracking().Where(x => x.ActiveSession.Equals(true))
        //                        .Select(s => s.SessionId).FirstOrDefaultAsync();

        //                    var registeredStudent = _db.CourseRegistrations.Where(x => x.SemesterId.Equals(semesterId)
        //                                                && x.SessionId.Equals(sessionId) && x.Department.DepartmentId.Equals(item))
        //                                                .Select(s => s.StudentId).AsParallel().Distinct();
        //                    foreach (var student in registeredStudent)
        //                    {
        //                        await SendSmsMessage(student, model.Message);
        //                    }
        //                }
        //                if (model.StudentCategory.Equals(StudentSms.All_Staff.ToString()))
        //                {
        //                    var staffList = await _db.Staffs.AsNoTracking().Where(x => x.IsActiveStaff.Equals(true)
        //                                        && x.DepartmentId.Equals(item)).ToListAsync();
        //                    foreach (var staff in staffList)
        //                    {
        //                        await SendSmsMessage(staff.StaffId, model.Message);
        //                    }
        //                }
        //            }
        //        }


        //    }
        //    return View(model);
        //}

        private async Task SendSmsMessage(string studentId, string body)
        {
            var message = new SmsToStudent()
            {
                Destination = studentId,
                Body = body
            };

            await cs.SendStudentMsgAsync(message);
        }

        private async Task SendUnkownSmsMessage(string studentId, string body)
        {
            var message = new SmsToStudent()
            {
                Destination = studentId,
                Body = body
            };

            await cs.SendUnknowMsgAsync(message);
        }

        public async Task<ActionResult> SendtoAllPreDegreeStudent()
        {
            var studentList = await _db.PreDegreeStudents.AsNoTracking().ToListAsync();
            foreach (var student in studentList)
            {
                var message = new SmsToStudent
                {
                    Destination = student.PhoneNumber,
                    Body = $"Hello, Your username for the upcoming Predregree Exam is {student.RegNo}" +
                           $" and password is \" {student.Password} \""
                };

                await cs.SendUnknowMsgAsync(message);
            }

            return RedirectToAction("DashBoard", "Home");
        }
    }
}