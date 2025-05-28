using Rotativa;
using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampus.ViewModels;
using SwiftKampus.ViewModels.CBTE;
using SwiftKampusModel.CBTE;
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
    public class QuestionAnswersController : BaseController
    {

        public QuestionAnswersController(SchoolDbContext db) : base(db)
        {

        }

        [Authorize(Roles = RoleName.SuperAdmin + "," + RoleName.ComputerBaseTest)]
        // GET: QuestionAnswers
        public async Task<ActionResult> Index()
        {
            var questionAnswers = _db.QuestionAnswers.AsNoTracking().Include(q => q.Course).Include(q => q.Level);
            return View(await questionAnswers.ToListAsync());
        }

        // GET: QuestionAnswers/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            QuestionAnswer questionAnswer = await _db.QuestionAnswers.FindAsync(id);
            if (questionAnswer == null)
            {
                return HttpNotFound();
            }
            return View(questionAnswer);
        }

        // GET: QuestionAnswers/Create
        public ActionResult Create()
        {
            ViewBag.CourseId = new SelectList(_db.Courses.AsNoTracking(), "CourseId", "CourseCode");
            ViewBag.LevelId = new SelectList(_db.Levels.AsNoTracking(), "LevelId", "LevelName");
            ViewBag.ExamTypeId = new SelectList(_db.ExamTypes.AsNoTracking(), "ExamTypeId", "ExamName");
            return View();
        }

        // POST: QuestionAnswers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(QuestionAnswerVm model)
        {
            if (ModelState.IsValid)
            {
                var questionAnswer = new QuestionAnswer
                {
                    CourseId = model.CourseId,
                    LevelId = model.LevelId,
                    ExamTypeId = model.ExamTypeId,
                    Question = model.Question,
                    Option1 = model.Option1,
                    Option2 = model.Option2,
                    Option3 = model.Option3,
                    Option4 = model.Option4,
                    Answer = model.Answer,
                    QuestionHint = model.QuestionHint,
                    QuestionType = model.QuestionType,

                };
                _db.QuestionAnswers.Add(questionAnswer);
                await _db.SaveChangesAsync();
                TempData["UserMessage"] = "Question is Added Successfully.";
                TempData["Title"] = "Success.";

                return RedirectToAction("Create");

            }

            ViewBag.CourseId = new SelectList(_db.Courses.AsNoTracking(), "CourseId", "CourseCode", model.CourseId);
            ViewBag.LevelId = new SelectList(_db.Levels.AsNoTracking(), "LevelId", "LevelName", model.LevelId);
            ViewBag.ExamTypeId = new SelectList(_db.ExamTypes.AsNoTracking(), "ExamTypeId", "ExamName");
            return View(model);
        }

        // GET: QuestionAnswers/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            QuestionAnswer questionAnswer = await _db.QuestionAnswers.FindAsync(id);
            if (questionAnswer == null)
            {
                return HttpNotFound();
            }
            ViewBag.CourseId = new SelectList(_db.Courses.AsNoTracking(), "CourseId", "CourseCode", questionAnswer.CourseId);
            ViewBag.LevelId = new SelectList(_db.Levels.AsNoTracking(), "LevelId", "LevelName", questionAnswer.LevelId);
            ViewBag.ExamTypeId = new SelectList(_db.ExamTypes.AsNoTracking(), "ExamTypeId", "ExamName");
            var model = new QuestionAnswerVm()
            {
                QuestionAnswerId = questionAnswer.QuestionAnswerId,
                Question = questionAnswer.Question,
                Option1 = questionAnswer.Option1,
                Option2 = questionAnswer.Option2,
                Option3 = questionAnswer.Option3,
                Option4 = questionAnswer.Option4,
                Answer = questionAnswer.Answer,
                QuestionHint = questionAnswer.QuestionHint
            };
            return View(model);
        }

        // POST: QuestionAnswers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(QuestionAnswerVm model)
        {
            if (ModelState.IsValid)
            {
                var questionAnswer = new QuestionAnswer
                {
                    QuestionAnswerId = model.QuestionAnswerId,
                    CourseId = model.CourseId,
                    ExamTypeId = model.ExamTypeId,
                    LevelId = model.LevelId,
                    Question = model.Question,
                    Option1 = model.Option1,
                    Option2 = model.Option2,
                    Option3 = model.Option3,
                    Option4 = model.Option4,
                    Answer = model.Answer,
                    QuestionHint = model.QuestionHint,
                    QuestionType = model.QuestionType,

                };
                _db.Entry(questionAnswer).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                TempData["UserMessage"] = "Question is Updated Successfully.";
                TempData["Title"] = "Success.";

                return RedirectToAction("Index");
            }
            ViewBag.CourseId = new SelectList(_db.Courses.AsNoTracking(), "CourseId", "CourseCode", model.CourseId);
            ViewBag.LevelId = new SelectList(_db.Levels.AsNoTracking(), "LevelId", "LevelName", model.LevelId);
            ViewBag.ExamTypeId = new SelectList(_db.ExamTypes.AsNoTracking(), "ExamTypeId", "ExamName");
            return View(model);
        }

        // GET: QuestionAnswers/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            QuestionAnswer questionAnswer = await _db.QuestionAnswers.FindAsync(id);
            if (questionAnswer == null)
            {
                return HttpNotFound();
            }
            return View(questionAnswer);
        }

        // POST: QuestionAnswers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            QuestionAnswer questionAnswer = await _db.QuestionAnswers.FindAsync(id);
            if (questionAnswer != null) _db.QuestionAnswers.Remove(questionAnswer);
            await _db.SaveChangesAsync();
            TempData["UserMessage"] = "Question is Deleted Successfully.";
            TempData["Title"] = "Error.";

            return RedirectToAction("Index");
        }

        public ActionResult ExamReport()
        {
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FancyName");
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            ViewBag.Courses = new SelectList(_db.Courses.AsNoTracking(), "CourseId", "CourseName");
            ViewBag.LevelId = new SelectList(_db.Levels.AsNoTracking(), "LevelId", "LevelName");
            ViewBag.ExamTypeId = new SelectList(_db.ExamTypes.AsNoTracking(), "ExamTypeId", "ExamName");
            return View();
        }

        public async Task<ActionResult> GetStudentsQuestionAnswer(int? CoursesId, string hasRegistered, int? SessionId)
        {
            

            //#endregion Server Side filtering
            //SchoolProgrammeId = await GetUndergraduateSchoolProgrammeId();
            bool isCleared = false;
            if (hasRegistered.Equals("Cleared"))
            {
                isCleared = true;
            }
            var studentIndex = new List<ExamsLogVm>();

            var staffDept = await _db.Staffs.AsNoTracking().Where(x => x.Email.Equals(userId))
                                    .Select(s => s.DepartmentId).FirstOrDefaultAsync();

            //var studentList = await _studentQuery.GetStudentDeptList(SchoolProgrammeId, staffDept, isCleared, SessionId);
            var studentList = await _db.ExamLogs.Include(x => x.Student)
                                                .Include(x => x.Course)
                                                .Where(x => x.SessionId == (Int32)SessionId && x.CourseId == (Int32)CoursesId)
                                                .Select(s => new ExamsLogVm()
                                                {
                                                    ExamLogId = s.ExamLogId,
                                                    StudentId = s.StudentId,
                                                    FirstName = s.Student.FirstName,
                                                    MiddleName = s.Student.MiddleName,
                                                    LastName = s.Student.LastName,
                                                    TotalScore = s.TotalScore,
                                                    CourseName = s.Course.CourseName,
                                                    Score = s.Score,
                                                    MatricNumber = !string.IsNullOrEmpty(s.Student.MatricNo) ? s.Student.MatricNo : s.Student.JambRegNo,
                                                    Status = s.ExamTaken,
                                                    
                                                }).ToListAsync();
            studentIndex = studentList;

            var data = studentIndex.OrderBy(x => x.MatricNumber).ToList();

            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }


        public async Task<ActionResult> flagExam(int id)
        {
            if (!string.IsNullOrEmpty(id.ToString()))
            {
                var ExamFlag = await _db.ExamLogs
                                .Where(x => x.ExamLogId.Equals(id)).FirstOrDefaultAsync();
                ExamFlag.ExamTaken = false;
                _db.Entry(ExamFlag).State = EntityState.Modified;
                string SMSbody = $" Exam expunged!!";
                await _db.SaveChangesAsync();

                return new JsonResult { Data = new { status = true, message = $" Exam expunged!!" } };
            }
            return new JsonResult { Data = new { status = false, message = "Oops.. Something went wrong, try again" } };
        }

        public async Task<ActionResult> ExamFullDetail(int id)
        {

            var examLog = await _db.ExamLogs.Where(x => x.ExamLogId.Equals(id)).FirstOrDefaultAsync();

            var student = await _db.StudentQuestions.AsNoTracking()
                                .Where(x => x.SessionId.Equals(examLog.SessionId) && x.CourseId.Equals(examLog.CourseId) && x.StudentId.Equals(examLog.StudentId))
                                .ToListAsync();


            //return new ViewAsPdf(student);
            return View(student);
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
