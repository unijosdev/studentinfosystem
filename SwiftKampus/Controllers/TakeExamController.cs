using Microsoft.AspNet.Identity;
using StAugustine.ViewModel.CBTE;
using SwiftKampus.BusinessLogic;
using SwiftKampus.CustomFilters;
using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampus.ViewModels.CBTE;
using SwiftKampusModel.CBTE;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SwiftKampus.Controllers
{
    //[Authorize]
    //[Audit(AuditingLevel = 2)]
    [AuthorizeIPAddress]
    public class TakeExamController : BaseController
    {

        public TakeExamController(SchoolDbContext db) : base(db)
        {

        }
        //// GET: TakeExam
        //public ActionResult Index()
        //{
        //    return View();
        //}

        private static Random rng = new Random();

        public ActionResult SelectSubject()
        {
            string studentName = _studentQuery.GetStudentId(User.Identity.GetUserName());
            var student = _db.Students.Include(s => s.Programme).Where(s => s.StudentId.Equals(studentName)).FirstOrDefault();

            ViewBag.SubjectName = new SelectList(_db.Courses.Where(c => c.ProgrammeId == (Int32)student.Programme.ProgrammeId).ToList(), "CourseId", "CourseName");
            ViewBag.LevelId = new SelectList(_db.Levels.AsNoTracking(), "LevelId", "LevelName");
            ViewBag.ExamTypeId = new SelectList(_db.ExamTypes.AsNoTracking(), "ExamTypeId", "ExamName");
            Session["Rem_Time"] = null;
            ViewBag.Time = Session["Rem_Time"];
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> SelectSubject(SelectSubjectViewModel model)
        {
            var result = ConfirmSchoolFee();
            if (result != null)
                return result;

            if (ModelState.IsValid)
            {
                //string studentName = User.Identity.GetUserName();
                string studentName = _studentQuery.GetStudentId(User.Identity.GetUserName());

                //var studentId = db.Students.Where(x => x.UserName.Equals(studentName)).Select(c => c.StudentId).FirstOrDefault();
                //bool myDate = false;

                var rules = await _db.ExamRules.AsNoTracking().Where(x => x.CourseId.Equals(model.SubjectName) && x.LevelId.Equals(model.LevelId) && x.ResultDivision.Equals(model.ExamTypeId))
                                 .Select(s => new { totoalquestion = s.TotalQuestion, examTime = s.MaximumTime }).FirstOrDefaultAsync();
                if (rules == null)
                {

                }
                var examDate = await _db.ExamSettings.AsNoTracking().Where(x => x.CourseId.Equals(model.SubjectName)
                                                           && x.LevelId.Equals(model.LevelId)
                                                           && x.SemesterId.Equals(semesterId)
                                                           && x.SessionId.Equals(sessionId))
                                                            .Select(s => s.ExamDate)
                                                        .FirstOrDefaultAsync();
                int dateCompare1 = DateTime.Compare(examDate.Date, DateTime.Now.Date);
                if (dateCompare1 == 0)
                {
                    var examLog = await _db.ExamLogs.AsNoTracking().Where(x => x.StudentId.Equals(studentName)
                                                                        && x.ExamTypeId.Equals(model.ExamTypeId) &&
                                                                        x.SemesterId.Equals(semesterId)
                                                                        && x.SessionId.Equals(sessionId) &&
                                                                        x.LevelId.Equals(model.LevelId)
                                                                        && x.CourseId.Equals(model.SubjectName))
                                                                        .Select(s => new { examTaken = s.ExamTaken })
                                                                        .SingleOrDefaultAsync();
                    if (examLog != null)
                    {
                        if (examLog.examTaken.Equals(true))
                        {
                            ViewBag.SubjectName = new SelectList(_db.Courses.AsNoTracking(), "CourseId", "CourseName");
                            ViewBag.LevelId = new SelectList(_db.Levels.AsNoTracking(), "LevelId", "LevelName");
                            ViewBag.ExamTypeId = new SelectList(_db.ExamTypes.AsNoTracking(), "ExamTypeId", "ExamName");
                            ViewBag.ErrorMessage = $"You have already Taken the exam, If you try thrice, the will carry" +
                                                   $" the course over ";
                            return View(model);
                            // return warning message that exam has been taken already
                        }
                    }
                    var questionExist = await _db.StudentQuestions.Where(x => x.StudentId.Equals(studentName)
                                                                              && x.CourseId.Equals(model.SubjectName)
                                                                              && x.LevelId.Equals(model.LevelId)
                                                                              && x.SemesterId.Equals(semesterId)
                                                                              && x.SessionId.Equals(sessionId))
                                                                                .CountAsync();
                    if (questionExist > 1)
                    {
                        return RedirectToAction("Exam", new
                        {
                            questionNo = 1,
                            courseId = model.SubjectName,
                            studentid = studentName,
                            level = model.LevelId,
                            examtype = model.ExamTypeId
                        });
                    }

                    //var r = new Random();
                    if (rules != null)
                    {
                        var myquestion = await _db.QuestionAnswers.Where(x => x.LevelId.Equals(model.LevelId)
                                                                              && x.CourseId.Equals(model.SubjectName)
                                                                              && x.ExamTypeId.Equals(model.ExamTypeId))
                            .Take(rules.totoalquestion).ToListAsync();
                        // var myquestion = bquestion.OrderBy(x => Guid.NewGuid()).Take(totalQuestion).ToList();
                        //var tenRandomUser = listUsr.OrderBy(u => r.Next()).Take(10);

                        var shuffledcards = myquestion.OrderBy(a => rng.Next()).ToList();

                        int count = 1;
                        foreach (var question in shuffledcards)
                        {
                            var studentQuestion = new StudentQuestion()
                            {
                                StudentId = studentName,
                                CourseId = question.CourseId,
                                LevelId = model.LevelId,
                                SemesterId = semesterId,
                                SessionId = sessionId,
                                ExamTypeId = question.ExamTypeId,
                                Question = question.Question,
                                Option1 = question.Option1,
                                Option2 = question.Option2,
                                Option3 = question.Option3,
                                Option4 = question.Option4,
                                FilledAnswer = String.Empty,
                                Answer = question.Answer,
                                QuestionHint = question.QuestionHint,
                                IsFillInTheGag = question.IsFillInTheGag,
                                IsMultiChoiceAnswer = question.IsMultiChoiceAnswer,
                                QuestionNumber = count,
                                TotalQuestion = rules.totoalquestion,
                                ExamTime = rules.examTime

                            };
                            _db.StudentQuestions.Add(studentQuestion);
                            count++;

                        }
                    }

                    await _db.SaveChangesAsync();
                    return RedirectToAction("Exam", new
                    {
                        questionNo = 1,
                        courseId = model.SubjectName,
                        studentid = studentName,
                        level = model.LevelId,
                        examtype = model.ExamTypeId
                    });
                }
                ViewBag.SubjectName = new SelectList(_db.Courses.AsNoTracking(), "CourseId", "CourseName");
                ViewBag.LevelId = new SelectList(_db.Levels.AsNoTracking(), "LevelId", "LevelName");
                ViewBag.ExamTypeId = new SelectList(_db.ExamTypes.AsNoTracking(), "ExamTypeId", "ExamName");
                ViewBag.ErrorMessage = $"The Exam is schedule for {examDate:d} ";
                return View(model);
                //return caution message of exam date time not correct
            }
            ViewBag.SubjectName = new SelectList(_db.Courses, "CourseId", "CourseName");
            return View(model);
        }

        [HttpGet]
        public async Task<ActionResult> Exam(int questionNo, int courseId, string studentid, int level) /*string examtype*/
            {
            int myno = questionNo;
            var question = _db.StudentQuestions.FirstOrDefault(s => s.StudentId.Equals(studentid)
                                                                   && s.QuestionNumber.Equals(myno));
            if (question != null)
            {
                if (Session["Rem_Time"] == null)
                {
                    int time = question.ExamTime;
                    Session["Rem_Time"] = DateTime.Now.AddMinutes(time).ToString("MM-dd-yyyy h:mm:ss tt");
                }
                //Session["Rem_Time"] = DateTime.Now.AddMinutes(2).ToString("dd-MM-yyyy h:mm:ss tt");
                // Session["Rem_Time"] = DateTime.Now.AddMinutes(2).ToString("MM-dd-yyyy h:mm:ss tt");
                ViewBag.Message = "Modify this template to jump-start your ASP.NET MVC application.";
                ViewBag.Rem_Time = Session["Rem_Time"];
                ViewBag.Course = await _db.Courses.AsNoTracking().Where(x => x.CourseId.Equals(courseId))
                                        .Select(c => c.CourseName).FirstOrDefaultAsync();
                ViewBag.Level = await _db.Levels.AsNoTracking().Where(x => x.LevelId.Equals(level))
                                        .Select(c => c.LevelName).FirstOrDefaultAsync();

                ViewBag.Questions = await _db.StudentQuestions.Where(x => x.LevelId.Equals(level)
                                                                              && x.CourseId.Equals(courseId)
                                                                              && x.SemesterId.Equals(semesterId)
                                                                              && x.SessionId.Equals(sessionId)
                                                                              && x.StudentId.Equals(studentid))
                                                                             .OrderBy(x => x.QuestionNumber).ToListAsync();
            }
            return View(question);
        }


        [HttpPost]
        [ValidateInput(false)]
        [MultipleButton(Name = "action", Argument = "Next")]
        //public async Task<ActionResult> Next(DisplayQuestionViewModel model, string fiiledAnswer,
        //            string Check1, string Check2, string Check3, string Check4)
        public async Task<ActionResult> Next(DisplayQuestionViewModel model, string fiiledAnswer)
        {
            var studentId = model.StudentId;
            int questionId = model.QuestionNo;
            var questionType = CheckQuestionType(model);
            if (questionType != null)
            {

                if (questionType.IsFillInTheGag)
                {
                    if (!String.IsNullOrEmpty(fiiledAnswer))
                    {
                        await SaveAnswer(model, studentId, questionId, fiiledAnswer);
                        return RedirectToAction("Exam", "TakeExam",
                            new
                            {
                                questionNo = ++questionId,
                                courseId = model.CourseId,
                                studentid = model.StudentId,
                                level = model.LevelId
                            });
                    }
                }
                else if (questionType.IsMultiChoiceAnswer)
                {
                    StringBuilder builder = new StringBuilder();

                    if (model.Check1.Equals(true))
                    {
                        builder.Append("A");
                    }
                    if (model.Check2.Equals(true))
                    {
                        builder.Append("B");
                    }
                    if (model.Check3.Equals(true))
                    {
                        builder.Append("C");
                    }
                    if (model.Check4.Equals(true))
                    {
                        builder.Append("D");
                    }
                    string value = builder.ToString();

                    string checkedAnswer = SortStringAlphabetically(builder.ToString());

                    await SaveMultiChoiceAnswer(model, checkedAnswer);
                }
                else
                {
                    if (!String.IsNullOrEmpty(model.SelectedAnswer))
                    {
                        string answer = CheckAnswerForSingleChoice(model);
                        await SaveAnswer(model, studentId, questionId, answer);
                        return RedirectToAction("Exam", "TakeExam",
                            new
                            {
                                questionNo = ++questionId,
                                courseId = model.CourseId,
                                studentid = model.StudentId,
                                level = model.LevelId
                            });
                    }
                }
            }

            ViewBag.SubjectName = new SelectList(_db.Courses, "CourseId", "CourseCode");
            return RedirectToAction("Exam", new { questionNo = ++questionId, courseId = model.CourseId, studentid = model.StudentId, level = model.LevelId });
        }




        [HttpPost]
        [ValidateInput(false)]
        [MultipleButton(Name = "action", Argument = "Previous")]
        //public async Task<ActionResult> Previous(DisplayQuestionViewModel model, string fiiledAnswer,
        //            string Check1, string Check2, string Check3, string Check4)
        public async Task<ActionResult> Previous(DisplayQuestionViewModel model, string fiiledAnswer)
        {
            var studentId = model.StudentId;
            int questionId = model.QuestionNo;
            var questionType = CheckQuestionType(model);
            if (questionType != null)
            {
                if (questionType.IsFillInTheGag)
                {
                    if (!String.IsNullOrEmpty(fiiledAnswer))
                    {
                        await SaveAnswer(model, studentId, questionId, fiiledAnswer);
                        return RedirectToAction("Exam", "TakeExam", new
                        {
                            questionNo = ++questionId,
                            courseId = model.CourseId,
                            studentid = model.StudentId,
                            level = model.LevelId
                        });
                    }
                }
                else if (questionType.IsMultiChoiceAnswer)
                {
                    StringBuilder builder = new StringBuilder();

                    if (model.Check1.Equals(true))
                    {
                        builder.Append("A");
                    }
                    if (model.Check2.Equals(true))
                    {
                        builder.Append("B");
                    }
                    if (model.Check3.Equals(true))
                    {
                        builder.Append("C");
                    }
                    if (model.Check4.Equals(true))
                    {
                        builder.Append("D");
                    }
                    string value = builder.ToString();

                    string checkedAnswer = SortStringAlphabetically(builder.ToString());

                    await SaveMultiChoiceAnswer(model, checkedAnswer);
                }
                else
                {
                    if (!String.IsNullOrEmpty(model.SelectedAnswer))
                    {
                        string answer = CheckAnswerForSingleChoice(model);
                        await SaveAnswer(model, studentId, questionId, answer);
                        return RedirectToAction("Exam", "TakeExam",
                            new
                            {
                                questionNo = --questionId,
                                courseId = model.CourseId,
                                studentid = model.StudentId,
                                level = model.LevelId
                            });
                    }
                }
            }

            ViewBag.SubjectName = new SelectList(_db.Courses, "CourseId", "CourseCode");
            return RedirectToAction("Exam", new { questionNo = --questionId, courseId = model.CourseId, studentid = model.StudentId, level = model.LevelId });
        }


        [HttpPost]
        [ValidateInput(false)]
        [MultipleButton(Name = "action", Argument = "SubmitExam")]
        //public async Task<ActionResult> SubmitExam(DisplayQuestionViewModel model, string fiiledAnswer,
        //            string Check1, string Check2, string Check3, string Check4)
        public async Task<ActionResult> SubmitExam(DisplayQuestionViewModel model, string fiiledAnswer)
        {
            double scoreCount = 0;

            var studentId = model.StudentId;
            int questionId = model.QuestionNo;

            var questionType = CheckQuestionType(model);
            #region Submit Answer
            if (questionType != null)
            {
                if (questionType.IsFillInTheGag)
                {
                    if (!String.IsNullOrEmpty(fiiledAnswer))
                    {
                        await SaveAnswer(model, studentId, questionId, fiiledAnswer);
                        scoreCount = _db.StudentQuestions.Count(x => x.IsCorrect.Equals(true));
                        //return RedirectToAction("ExamIndex", "TakeExam",
                        //    new { score = scoreCount, courseId = model.CourseId, studentid = model.StudentId });
                    }
                }
                else if (questionType.IsMultiChoiceAnswer)
                {
                    StringBuilder builder = new StringBuilder();

                    if (model.Check1.Equals(true))
                    {
                        builder.Append("A");
                    }
                    if (model.Check2.Equals(true))
                    {
                        builder.Append("B");
                    }
                    if (model.Check3.Equals(true))
                    {
                        builder.Append("C");
                    }
                    if (model.Check4.Equals(true))
                    {
                        builder.Append("D");
                    }
                    string value = builder.ToString();

                    string checkedAnswer = SortStringAlphabetically(builder.ToString());

                    await SaveMultiChoiceAnswer(model, checkedAnswer);
                }
                else
                {
                    if (!String.IsNullOrEmpty(model.SelectedAnswer))
                    {
                        string answer = CheckAnswerForSingleChoice(model);
                        await SaveAnswer(model, studentId, questionId, answer);
                        scoreCount = _db.StudentQuestions.Count(x => x.IsCorrect.Equals(true));
                        //return RedirectToAction("ExamIndex", "TakeExam",
                        //    new { score = scoreCount, courseId = model.CourseId, studentid = model.StudentId });
                    }
                }

            }
            #endregion

            scoreCount = await _db.StudentQuestions.Where(x => x.StudentId.Equals(model.StudentId) && x.CourseId.Equals(model.CourseId)
                                               && x.LevelId.Equals(model.LevelId) && x.SessionId.Equals(model.SessionId)
                                               && x.SemesterId.Equals(model.SemesterId)).CountAsync(c => c.IsCorrect.Equals(true));
            var studentdetails = _db.StudentQuestions.FirstOrDefault(x => x.StudentId.Equals(model.StudentId) && x.CourseId.Equals(model.CourseId)
                                               && x.LevelId.Equals(model.LevelId) && x.SessionId.Equals(model.SessionId)
                                               && x.SemesterId.Equals(model.SemesterId));

            if (studentdetails != null)
            {
                await ProcessResult(model, studentdetails, scoreCount);

            }

            return RedirectToAction("Index", "ExamLogs", new
            {
                studentId = model.StudentId,
                courseId = model.CourseId,
                levelId = model.LevelId,
                semesterId = model.SemesterId,
                sessionId = model.SessionId
            });
        }

        private async Task ProcessResult(DisplayQuestionViewModel model, StudentQuestion studentdetails, double scoreCount)
        {
            var examRule = await _db.ExamRules.AsNoTracking().Where(x => x.CourseId.Equals(model.CourseId) && x.LevelId.Equals(model.LevelId))
                                    .Select(s => new { scorePerQuestion = s.ScorePerQuestion, totalQuestion = s.TotalQuestion })
                                    .FirstOrDefaultAsync();
            double sum = examRule.scorePerQuestion * examRule.totalQuestion;
            //var studentId = _studentQuery.GetStudentId(studentdetails.StudentId);
            double total = scoreCount * examRule.scorePerQuestion;
            var examLog = new ExamLog()
            {
                //StudentId = _studentQuery.GetStudentId(studentdetails.StudentId),
                StudentId = studentdetails.StudentId,
                CourseId = studentdetails.CourseId,
                LevelId = studentdetails.LevelId,
                SemesterId = studentdetails.SemesterId,
                SessionId = studentdetails.SessionId,
                ExamTypeId = studentdetails.ExamTypeId,
                Score = total,
                TotalScore = sum,
                ExamTaken = true
            };
            // _db.ExamLogs.AddOrUpdate(examLog);
            _db.Set<ExamLog>().AddOrUpdate(examLog);
            await _db.SaveChangesAsync();

            Session["Rem_Time"] = null;

            string courseName = await _db.Courses.AsNoTracking().Where(x => x.CourseId.Equals(model.CourseId))
                .Select(c => c.CourseName).FirstOrDefaultAsync();
            string examName = await _db.ExamTypes.AsNoTracking().Where(x => x.ExamTypeId.Equals(model.ExamTypeId))
                .Select(c => c.ExamName).FirstOrDefaultAsync();

            var semesterName = _query.GetCurrentSemesterName(studentSchoolProgrammeId);
            var sessioName = _query.GetCurrentSessionName(studentSchoolProgrammeId);

            //var message = new SmsToStudent()
            //{
            //    Destination = model.StudentId,
            //    Body =
            //        $"Your have successfully submitted {courseName} in {examName} for {semesterName} semester in {sessioName}"
            //};
            //CustomSms cs = new CustomSms();
            //await cs.SendStudentMsgAsync(message);
            var body = $"Your have successfully submitted {courseName} in {examName} for {semesterName} semester in {sessioName}";
            var student = _db.Students.Find(studentdetails);
            await SMSClass.SendSMS("UNIJOS SIS", body, student.PhoneNumber); //EBULK SMS API
        }

        private async Task ProcessResultTimeOut(ExamLogVm model, StudentQuestion studentdetails, double scoreCount)
        {
            var examRule = await _db.ExamRules.AsNoTracking().Where(x => x.ResultDivision.Equals(model.ExamTypeId))
                                    .Select(s => new { scorePerQuestion = s.ScorePerQuestion, totalQuestion = s.TotalQuestion })
                                    .FirstOrDefaultAsync();
            double sum = examRule.scorePerQuestion * examRule.totalQuestion;
            double total = scoreCount * examRule.scorePerQuestion;
            var examLog = new ExamLog()
            {
                StudentId = studentdetails.StudentId,
                CourseId = studentdetails.CourseId,
                LevelId = studentdetails.LevelId,
                SemesterId = studentdetails.SemesterId,
                SessionId = studentdetails.SessionId,
                ExamTypeId = studentdetails.ExamTypeId,
                Score = total,
                TotalScore = sum,
                ExamTaken = true
            };

            //_db.ExamLogs.AddOrUpdate(examLog);
            _db.Set<ExamLog>().AddOrUpdate(examLog);
            await _db.SaveChangesAsync();

            Session["Rem_Time"] = null;

            string courseName = await _db.Courses.AsNoTracking().Where(x => x.CourseId.Equals(model.CourseId))
                .Select(c => c.CourseName).FirstOrDefaultAsync();
            string examName = await _db.ExamTypes.AsNoTracking().Where(x => x.ExamTypeId.Equals(model.ExamTypeId))
                .Select(c => c.ExamName).FirstOrDefaultAsync();
            var semesterName = _query.GetCurrentSemesterName(studentSchoolProgrammeId);
            var sessioName = _query.GetCurrentSessionName(studentSchoolProgrammeId);

            var body = $"Your have successfully submitted {courseName} in {examName} for {semesterName} semester in {sessioName}";
            var student = _db.Students.Find(studentdetails);
            await SMSClass.SendSMS("UNIJOS SIS", body, student.PhoneNumber); //EBULK SMS API
        }


        public async Task<ActionResult> SubmitExam(string studentId, int courseId, int levelId, int examType)
        {
            string myStudentId = studentId.Trim();
            double scoreCount = await _db.StudentQuestions.Where(x => x.StudentId.Equals(myStudentId) && x.SemesterId.Equals(semesterId)
                        && x.CourseId.Equals(courseId) && x.LevelId.Equals(levelId) && x.SessionId.Equals(sessionId))
                        .CountAsync(c => c.IsCorrect.Equals(true));

            var studentdetails =
                await _db.StudentQuestions.Where(x => x.StudentId.Equals(myStudentId) && x.SemesterId.Equals(semesterId)
                        && x.CourseId.Equals(courseId) && x.LevelId.Equals(levelId) && x.SessionId.Equals(sessionId)).FirstOrDefaultAsync();

            if (studentdetails != null || studentdetails == null)
            {
                var model = new ExamLogVm()
                {
                    StudentId = studentId,
                    CourseId = courseId,
                    LevelId = levelId,
                    ExamTypeId = examType

                };
                await ProcessResultTimeOut(model, studentdetails, scoreCount);

            }

            return RedirectToAction("Index", "ExamLogs", new
            {
                studentId,
                courseId,
                levelId,
                semesterId,
                sessionId
            });
        }

        public ActionResult ExamIndex(string studentId, int? courseId, string score)
        {

            ViewBag.StudentId = studentId;
            ViewBag.CourseId = courseId;
            ViewBag.Score = score;

            Session["Rem_Time"] = null;
            return View();
            //return View(studentList.ToList());
        }

        [ValidateInput(false)]
        private async Task SaveAnswer(DisplayQuestionViewModel model, string studentId, int questionId, string answer)
        {
            var question = _db.StudentQuestions.FirstOrDefault(s => s.StudentId.Equals(studentId)
                                                                   && s.QuestionNumber.Equals(questionId)
                                                                   && s.CourseId.Equals(model.CourseId));
            if (question.IsFillInTheGag.Equals(true))
            {
                if (question.Answer.ToUpper().Equals(answer.ToUpper()))
                {
                    question.IsCorrect = true;
                    question.SelectedAnswer = answer;
                    question.Check1 = model.Check1;
                    question.Check2 = model.Check2;
                    question.Check3 = model.Check3;
                    question.Check4 = model.Check4;
                    _db.Entry(question).State = EntityState.Modified;
                    await _db.SaveChangesAsync();
                }
                else
                {
                    question.IsCorrect = false;
                    question.SelectedAnswer = answer;
                    question.Check1 = model.Check1;
                    question.Check2 = model.Check2;
                    question.Check3 = model.Check3;
                    question.Check4 = model.Check4;
                    _db.Entry(question).State = System.Data.Entity.EntityState.Modified;
                    await _db.SaveChangesAsync();
                }
            }
            else
            {
                if (ConvertAnsewrToAlphabet(question).ToUpper().Equals(answer.ToUpper()))
                {
                    question.IsCorrect = true;
                    question.SelectedAnswer = model.SelectedAnswer;
                    question.Check1 = model.Check1;
                    question.Check2 = model.Check2;
                    question.Check3 = model.Check3;
                    question.Check4 = model.Check4;
                    _db.Entry(question).State = EntityState.Modified;
                    await _db.SaveChangesAsync();
                }
                else
                {
                    question.IsCorrect = false;
                    question.SelectedAnswer = model.SelectedAnswer;
                    question.Check1 = model.Check1;
                    question.Check2 = model.Check2;
                    question.Check3 = model.Check3;
                    question.Check4 = model.Check4;
                    _db.Entry(question).State = System.Data.Entity.EntityState.Modified;
                    await _db.SaveChangesAsync();
                }
            }
            
        }

        private async Task SaveMultiChoiceAnswer(DisplayQuestionViewModel model, string checkedAnswer)
        {
            var question = _db.StudentQuestions.FirstOrDefault(s => s.StudentId.Equals(model.StudentId)
                                                                   && s.QuestionNumber.Equals(model.QuestionNo)
                                                                   && s.CourseId.Equals(model.CourseId));
            string[] myAnswer = question.Answer.Split(',');
            StringBuilder answerbuilder = new StringBuilder();

            foreach (var item in myAnswer)
            {
                answerbuilder.Append(item);
            }

            string value = answerbuilder.ToString();
            string answer = SortStringAlphabetically(answerbuilder.ToString());


            if (answer.ToUpper().Equals(checkedAnswer.ToUpper()))
            {
                question.IsCorrect = true;
                question.SelectedAnswer = model.SelectedAnswer;
                question.Check1 = model.Check1;
                question.Check2 = model.Check2;
                question.Check3 = model.Check3;
                question.Check4 = model.Check4;
                _db.Entry(question).State = EntityState.Modified;
                await _db.SaveChangesAsync();
            }
            else
            {
                question.IsCorrect = false;
                question.SelectedAnswer = model.SelectedAnswer;
                question.Check1 = model.Check1;
                question.Check2 = model.Check2;
                question.Check3 = model.Check3;
                question.Check4 = model.Check4;
                _db.Entry(question).State = System.Data.Entity.EntityState.Modified;
                await _db.SaveChangesAsync();
            }
        }

        static string SortStringAlphabetically(string str)
        {
            char[] foo = str.ToArray();
            Array.Sort(foo);
            return new string(foo);
        }

        private string CheckAnswerForSingleChoice(DisplayQuestionViewModel model)
        {
            if (model.SelectedAnswer.Equals(model.Option1))
            {
                return "A";
            }
            if (model.SelectedAnswer.Equals(model.Option2))
            {
                return "B";
            }
            if (model.SelectedAnswer.Equals(model.Option3))
            {
                return "C";
            }
            if (model.SelectedAnswer.Equals(model.Option4))
            {
                return "D";
            }
            return "";
        }

        private string ConvertAnsewrToAlphabet(StudentQuestion model)
        {
            if (model.Answer.Equals(model.Option1))
            {
                return "A";
            }
            if (model.Answer.Equals(model.Option2))
            {
                return "B";
            }
            if (model.Answer.Equals(model.Option3))
            {
                return "C";
            }
            if (model.Answer.Equals(model.Option4))
            {
                return "D";
            }
            return "";
        }

        private StudentQuestion CheckQuestionType(DisplayQuestionViewModel model)
        {
            var questionType = _db.StudentQuestions.FirstOrDefault(x => x.QuestionNumber.Equals(model.QuestionNo)
                                                                       && x.StudentId.Equals(model.StudentId) &&
                                                                       x.CourseId.Equals(model.CourseId));
            return questionType;
        }

        //public void Shuffle<T>(this IList<T> list)
        //{
        //    int n = list.Count;
        //    while (n > 1)
        //    {
        //        n--;
        //        int k = rng.Next(n + 1);
        //        T value = list[k];
        //        list[k] = list[n];
        //        list[n] = value;
        //    }
        //}

    }

}