using SwiftKampus.Controllers;
using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampus.ViewModels.MisconductVm;
using SwiftKampusModel.Misconduct;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;


namespace Unijos.Web.Controllers
{
    [Authorize]
    [Audit(AuditingLevel = 2)]
    public class StudentRecallRequestController : BaseController
    {
        public StudentRecallRequestController(SchoolDbContext db) : base(db)
        {
        }
        /*
         only students with one and two session rustication get the this link
         students on suspension can only be updated when called by the varsity
         */

        // GET: StudentRecallRequest
        public ActionResult Index()
        {
            //Replace hard-coded with auth-id
            var studentId = _studentQuery.GetStudentId(userId);
            //var studentId = _db.Users.Where(us => us.Id.Equals(userId)).Select(us => us.StudentId).First();
            var studentRequest = _db.Defaulters.Include(df => df.StudentDisciplinaryStatus)
                                               .Where(sr => sr.StudentId.Equals(studentId) && sr.StudentDisciplinaryStatus.StillValid == true)
                                               .FirstOrDefault();
            RecallRequestViewModel studentRequestVM = new RecallRequestViewModel
            {
                StudentId = studentRequest.StudentId,
                DefaulterId = studentRequest.DefaulterId
            };
            return View(studentRequestVM);
        }

        [HttpPost]
        // POST: StudentRecallRequest/Create
        public ActionResult JSONCreateRequest(string studentId, int defaulterId, int reasonId)
        {
            var checkApplication = _db.StudentRecallRequests.Where(sr => sr.StudentId.Equals(studentId) && sr.RequestTreated == false).FirstOrDefault();
            if (!(checkApplication == null))
            {
                var Errormessage = "You Already have a pending Application!";
                return Json(Errormessage, JsonRequestBehavior.AllowGet);
            }
            var now = DateTime.UtcNow;
            var date = new DateTime(now.Year, now.Month, now.Day, now.Hour + 1, now.Minute, now.Second); //Added +1 to hours to compensate for UTC
            var defaulterDetail = _db.Defaulters.Where(df => df.DefaulterId == defaulterId).FirstOrDefault();
            StudentRecallRequest newRequest = new StudentRecallRequest
            {
                StudentId = studentId,
                //newRequest.StudentDisciplinaryStatusId = (Int32)defaulterDetail.StudentDisciplinaryStatusId;
                ReasonForRequest = (ReasonForRequest)reasonId,
                DefualterId = defaulterId,
                DateCreated = date,
                RequestTreated = false
            };

            _db.StudentRecallRequests.Add(newRequest);
            _db.SaveChanges();
            var message = "Application Submitted successfully";
            return Json(message, JsonRequestBehavior.AllowGet);
        }
    }
}