using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using OfficeOpenXml;
using Rotativa;
using SwiftKampus.Abstractions;
using SwiftKampus.BusinessLogic;
using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampus.ViewModels;
using SwiftKampusModel;
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
    public class CourseRegistrationsController : BaseController
    {
        private ICourseRegQueryManager _courseReg;
        private ResultCommand _resultQuery;
        private string studentId;
        public CourseRegistrationsController(SchoolDbContext db) : base(db)
        {
            studentId = _studentQuery.GetStudentId(userId);
            _courseReg = new CourseRegQueryManager(db);
            _resultQuery = new ResultCommand(db);
        }

        //Get RegisteredCourse://  the single instance of all course registration
        public async Task<ActionResult> RegisteredCourse(string message)
        {
            var result = ConfirmSchoolFee();
            //if (result != null)
            //    return result;
            var isRegistered = await _db.CourseRegistrations.Where(x => x.StudentId.Equals(studentId)
                                && x.SemesterId.Equals(semesterId) &&
                                x.SessionId.Equals(sessionId)).ToListAsync();
            //foreach (var item in isRegistered)
            //{
            //    _db.Entry(item).State = EntityState.Deleted;
            //}

            //await _db.SaveChangesAsync();
            if (isRegistered.Any())
            {
                ViewBag.Registered = true;
                ViewBag.Message = $"You have already registered for courses in this Semester {message}";
            }
            else
            {
                ViewBag.Registered = false;
                ViewBag.Message = $"You haven't registered any courses for this Semester {message}";
            }
            ViewBag.Message = message;

            return View();
        }

        //Get RegisteredCourse://  the single instance of all course registration
        public async Task<ActionResult> RegisteredPreviousCourse(string message, string studentId)
        {
            //var result = ConfirmSchoolFee();
            //if (result != null)
            //    return result;
            var isRegistered = await _db.CourseRegistrations.Where(x => x.StudentId.Equals(studentId)
                                ).ToListAsync();
            //foreach (var item in isRegistered)
            //{
            //    _db.Entry(item).State = EntityState.Deleted;
            //}

            //await _db.SaveChangesAsync();
            if (isRegistered.Any())
            {
                ViewBag.Registered = true;
                ViewBag.studentId = studentId;
                ViewBag.Message = $"You have successfully registered! {message}";
            }
            else
            {
                ViewBag.Registered = false;
                ViewBag.Message = $"You haven't registered any courses for this Semester {message}";
            }
            ViewBag.Message = message;

            return View();
        }

        public async Task<ActionResult> RegisteredStudents(string message)
        {
            ViewBag.SchoolProgrammeId = new SelectList(await _db.SchoolProgrammes.AsNoTracking().ToListAsync(), "SchoolProgrammeId", "FancyName");
            ViewBag.SemesterId = new SelectList(await _db.Semesters.AsNoTracking().ToListAsync(), "SemesterId", "SemesterName");
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");

            ViewBag.Message = message;
            return View();
        }

        public async Task<ActionResult> ConfirmApproval(int id)
        {
            var regHistory = _db.CourseRegistrations.AsNoTracking().Where(x => x.CourseRegistrationId.Equals(id))
                                    .FirstOrDefault();

            var courseReg = await _db.CourseRegistrations.Include(c => c.Course).Include(s => s.Students)
                                    .Include(c => c.Level).Include(c => c.Programme).Include(c => c.Semester)
                                    .Include(c => c.Session).AsNoTracking()
                                    .Where(x => x.StudentId.Equals(regHistory.StudentId) &&
                                    x.SessionId.Equals(regHistory.SessionId) && x.SemesterId.Equals(regHistory.SemesterId))
                                    .ToListAsync();
            if (courseReg.Count == 0)
            {
                var msg1 = $"Course registration history not found";
                return Json(new { Data = msg1 }, JsonRequestBehavior.AllowGet);
            }

            foreach (var course in courseReg)
            {
                var approveCourse = await _db.CourseRegistrations.FindAsync(course.CourseRegistrationId);
                approveCourse.IsApproved = true;
                approveCourse.IsStudentRegistered = true;
                approveCourse.ApprovedBy = userId;
                _db.Entry(approveCourse).State = EntityState.Modified;
            }
            await _db.SaveChangesAsync();
            var msg = $"{courseReg.Count} Course(s) has been approved successfully";
            //ViewBag.Message = message;
            return Json(new { Data = msg }, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> ConfirmDisApproval(int id)
        {
            var regHistory = _db.CourseRegistrations.AsNoTracking().Where(x => x.CourseRegistrationId.Equals(id))
                                    .FirstOrDefault();
            var courseReg = await _db.CourseRegistrations.Include(c => c.Course).Include(s => s.Students)
                                    .Include(c => c.Level).Include(c => c.Programme).Include(c => c.Semester)
                                    .Include(c => c.Session).AsNoTracking()
                                    .Where(x => x.StudentId.Equals(regHistory.StudentId) &&
                                    x.SessionId.Equals(regHistory.SessionId) && x.SemesterId.Equals(regHistory.SemesterId))
                                    .ToListAsync();
            if (courseReg.Count == 0)
            {
                var msg1 = $"Course registration history not found";
                return Json(new { Data = msg1 }, JsonRequestBehavior.AllowGet);
            }

            foreach (var course in courseReg)
            {
                var approveCourse = await _db.CourseRegistrations.FindAsync(course.CourseRegistrationId);
                approveCourse.IsApproved = false;
                approveCourse.IsStudentRegistered = false;
                _db.Entry(approveCourse).State = EntityState.Modified;
            }
            await _db.SaveChangesAsync();
            var msg = $"{courseReg.Count} Course(s) has been approved successfully";
            return Json(new { Data = msg }, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> GetIndex(int? SchoolProgrammeId, int? SessionId, int? SemesterId, string status)
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
            bool isApproved = false || (!string.IsNullOrEmpty(status) && status.ToUpper().Equals("TRUE"));

            var regCourses = new List<CourseRegistration>();
            if (Request.IsAuthenticated && User.IsInRole(RoleName.Student) || User.IsInRole(RoleName.TSupport))
            {
                // dc.Configuration.LazyLoadingEnabled = false; // if your table is relational,
                // contain foreign key
                regCourses = _db.CourseRegistrations.Include(c => c.Course).Include(s => s.Students)
                                    .Include(c => c.Level).Include(c => c.Programme).Include(c => c.Semester)
                                    .Include(c => c.Session).AsNoTracking().Where(x => x.StudentId.Equals(studentId))
                                    .OrderByDescending(o => o.SessionId).DistinctBy(d => new { d.SessionId }).ToList();
            }
            else if (Request.IsAuthenticated && User.IsInRole(RoleName.Hod))
            {
                var staffDept = await _db.Staffs.Include(i => i.Department).AsNoTracking()
                                        .Where(x => x.Email.Equals(userId))
                                        .Select(s => s.Department.DepartmentId).FirstOrDefaultAsync();
                regCourses = _db.CourseRegistrations.Include(c => c.Course).Include(s => s.Students)
                                .Include(c => c.Level).Include(c => c.Programme).Include(c => c.Semester)
                                .Include(c => c.Session).Include(i => i.Department).AsNoTracking()
                                .Where(x => x.SemesterId.Equals((int)SemesterId)
                                && x.SessionId.Equals((int)SessionId) && x.Department.DepartmentId.Equals(staffDept)
                                && x.Students.SchoolProgrammeId.Equals((int)SchoolProgrammeId)
                                && x.IsApproved.Equals(isApproved))
                                .OrderByDescending(o => o.SessionId).DistinctBy(d => d.StudentId).ToList();
            }
            else if (Request.IsAuthenticated && User.IsInRole(RoleName.Admin))
            {
                regCourses = _db.CourseRegistrations.Include(c => c.Course).Include(s => s.Students)
                            .Include(c => c.Level).Include(c => c.Programme).Include(c => c.Semester)
                            .Include(c => c.Session).AsNoTracking()
                            .Where(x => x.SemesterId.Equals((int)SemesterId)
                            && x.SessionId.Equals((int)SessionId)
                            && x.Students.SchoolProgrammeId.Equals((int)SchoolProgrammeId)
                            && x.IsApproved.Equals(isApproved))
                            .OrderBy(o => o.SessionId).DistinctBy(d => d.StudentId).ToList();
            }

            var data = regCourses.Select(s => new
            {
                s.CourseRegistrationId,
                s.Students.FullName,
                s.Level.LevelName,
                s.Session.SessionName,
                s.Semester.SemesterName,
                s.Programme.ProgrammeName,
                s.StudentId,
                s.IsApproved
            });

            totalRecords = data.Count();
            var newData = data.Skip(skip).Take(pageSize).ToList();

            return Json(new { draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = newData },
                JsonRequestBehavior.AllowGet);

            #endregion Server Side filtering

        }

        public async Task<ActionResult> GetStudentPreviousCourseIndex(string StudentId, string status)
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
            bool isApproved = false || (!string.IsNullOrEmpty(status) && status.ToUpper().Equals("TRUE"));

            var regCourses = new List<CourseRegistration>();
            
                // dc.Configuration.LazyLoadingEnabled = false; // if your table is relational,
                // contain foreign key
                regCourses = _db.CourseRegistrations.Include(c => c.Course).Include(s => s.Students)
                                    .Include(c => c.Level).Include(c => c.Programme).Include(c => c.Semester)
                                    .Include(c => c.Session).AsNoTracking().Where(x => x.StudentId.Equals(StudentId))
                                    .OrderByDescending(o => o.SessionId).DistinctBy(d => new { d.SessionId }).ToList();
            

            var data = regCourses.Select(s => new
            {
                s.CourseRegistrationId,
                s.Students.FullName,
                s.Level.LevelName,
                s.Session.SessionName,
                s.Semester.SemesterName,
                s.Programme.ProgrammeName,
                s.StudentId,
                s.IsApproved
            });

            totalRecords = data.Count();
            var newData = data.Skip(skip).Take(pageSize).ToList();

            return Json(new { draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = newData },
                JsonRequestBehavior.AllowGet);

            #endregion Server Side filtering

        }

        public async Task<ActionResult> GetRegisteredStudentSearch(int? SchoolProgrammeId, int? SessionId, int? SemesterId, string status)
        {

            bool isApproved = false || (!string.IsNullOrEmpty(status) && status.ToUpper().Equals("TRUE"));

            var regCourses = new List<CourseRegistration>();
            if (User.IsInRole(RoleName.DClearanceOfficer) || User.IsInRole(RoleName.Hod)
                || User.IsInRole(RoleName.LCordinator))
            {
                var staffDept = await _db.Staffs.Include(i => i.Department).AsNoTracking()
                                        .Where(x => x.Email.Equals(userId))
                                        .Select(s => s.Department.DepartmentId).FirstOrDefaultAsync();
                regCourses = _db.CourseRegistrations.Include(c => c.Course).Include(s => s.Students)
                                .Include(c => c.Level).Include(c => c.Programme).Include(c => c.Semester)
                                .Include(c => c.Session).Include(i => i.Department).AsNoTracking()
                                .Where(x => x.SemesterId.Equals((int)SemesterId)
                                && x.SessionId.Equals((int)SessionId) && x.Department.DepartmentId.Equals(staffDept)
                                && x.Students.SchoolProgrammeId.Equals((int)SchoolProgrammeId)
                                && x.IsApproved.Equals(isApproved) && x.IsStudentRegistered.Equals(true))
                                .OrderByDescending(o => o.SessionId).DistinctBy(d => d.StudentId).ToList();
            }
            else if (Request.IsAuthenticated && User.IsInRole(RoleName.Admin) )
            {
                regCourses = _db.CourseRegistrations.Include(c => c.Course).Include(s => s.Students)
                            .Include(c => c.Level).Include(c => c.Programme).Include(c => c.Semester)
                            .Include(c => c.Session).AsNoTracking()
                            .Where(x => x.SemesterId.Equals((int)SemesterId)
                            && x.SessionId.Equals((int)SessionId)
                            && x.Students.SchoolProgrammeId.Equals((int)SchoolProgrammeId)
                            && x.IsApproved.Equals(isApproved))
                            .OrderBy(o => o.SessionId).DistinctBy(d => d.StudentId).ToList();
            }

            var data = regCourses.Select(s => new CoureRegApprovalVm()
            {
                CourseRegistrationId = s.CourseRegistrationId,
                FullName = s.Students.FullName,
                MatricNo = s.Students.MatricNo,
                LevelName = s.Level.LevelName,
                SessionName = s.Session.SessionName,
                SemesterName = s.Semester.SemesterName,
                ProgrammeName = s.Programme.ProgrammeName,
                StudentId = s.StudentId,
                IsApproved = s.IsApproved ? "Course Approved" : "Not Approved Yet",
                ApprovalStatus = s.IsApproved,
                IsStudentSubmitted = s.IsStudentRegistered
            });

            //return PartialView(data);
            return Json(new { data }, JsonRequestBehavior.AllowGet);

        }

        public async Task<PartialViewResult> PartialDetails(int id)
        {
            var cr = await _db.CourseRegistrations.AsNoTracking().Where(x => x.CourseRegistrationId.Equals(id))
                            .Select(s => new { s.StudentId, s.SessionId }).FirstOrDefaultAsync();

            var courseList = await _db.CourseRegistrations.Include(c => c.Course).Include(s => s.Students)
                                 .Include(c => c.Level).Include(c => c.Programme).Include(c => c.Course.Semester)
                                 .Include(c => c.Session).AsNoTracking().Where(x => x.StudentId.Equals(cr.StudentId)
                                 && x.SessionId.Equals(cr.SessionId)).OrderBy(o => o.Course.Semester.SemesterName)
                                 .ToListAsync();

            return PartialView(courseList);
        }

        public async Task<ActionResult> PrintDetails(int id)
        {
            var cr = await _db.CourseRegistrations.AsNoTracking().Where(x => x.CourseRegistrationId.Equals(id))
                          .Select(s => new { s.StudentId, s.SessionId }).FirstOrDefaultAsync();

            var courseList = await _db.CourseRegistrations.Include(c => c.Course).Include(s => s.Students)
                                      .Include(c => c.Level).Include(c => c.Programme).Include(c => c.Course.Semester)
                                      .Include(c => c.Session).Include(i => i.Department).AsNoTracking()
                                      .Where(x => x.StudentId.Equals(cr.StudentId) && x.SessionId.Equals(cr.SessionId))
                                      .OrderBy(o => o.Course.Semester.SemesterName).ToListAsync();
            return new ViewAsPdf(courseList);
        }

        // GET: CourseRegistrations
        public async Task<ActionResult> Index()
        {
            var studentName = User.Identity.GetUserId();

            List<CourseRegistrationsIndexVM> courseRegs;

            var courseRegistrations = _db.CourseRegistrations.AsNoTracking().Include(c => c.Course)
                        .Include(c => c.Level).Include(c => c.Programme).Include(c => c.Semester)
                        .Include(c => c.Session);

            if (Request.IsAuthenticated && User.IsInRole("Student"))
            {
                courseRegs = await courseRegistrations.Where(cr => cr.StudentId.Equals(studentName))
                                .Select(cr => new CourseRegistrationsIndexVM
                                {
                                    StudentName = cr.Students.FullName,
                                    Semester = cr.Semester.SemesterName,
                                    Session = cr.Session.SessionName,
                                    ProgrammeName = cr.Programme.ProgrammeName,
                                    LevelName = cr.Level.LevelName,
                                    CourseName = cr.Course.CourseName,
                                    CreditLoad = cr.Course.Credits,
                                    RegistrationId = cr.CourseRegistrationId
                                }).ToListAsync();
            }
            else
            {
                courseRegs = courseRegistrations.AsEnumerable()
                    .DistinctBy(c => c.StudentId).Select(cr => new CourseRegistrationsIndexVM
                    {
                        StudentName = cr.Students.FullName,
                        Semester = cr.Semester.SemesterName,
                        Session = cr.Session.SessionName,
                        ProgrammeName = cr.Programme.ProgrammeName,
                        LevelName = cr.Level.LevelName,
                        CourseName = cr.Course.CourseName,
                        CreditLoad = cr.Course.Credits,
                        RegistrationId = cr.CourseRegistrationId
                    }).ToList();
            }

            return View(courseRegs);
        }

        // GET: CourseRegistrations/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CourseRegistration courseRegistration = await _db.CourseRegistrations.AsNoTracking()
                                .Where(cr => cr.CourseId == id).SingleOrDefaultAsync();

            if (courseRegistration == null)
            {
                return HttpNotFound();
            }

            ViewBag.SemesterId = semesterId;
            ViewBag.SessionId = sessionId;
            return View(courseRegistration);
        }

        [HttpGet]
        public ActionResult Register()
        {
            ViewBag.LevelId = new SelectList(_db.Levels, "LevelId", "LevelName");
            ViewBag.ProgrammeId = new SelectList(_db.Programmes, "ProgrammeId", "ProgrammeCode");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(SearchCourseVm model)
        {
            return RedirectToAction("Create", new { programeId = model.ProgrammeId, levelId = model.LevelId });
        }

        public async Task<ActionResult> StudentPreviousCourseReg()
        {
            ViewBag.SessionId = new SelectList(_db.Sessions.AsNoTracking(), "SessionId", "SessionName");
            ViewBag.LevelId = new SelectList(_db.Levels.AsNoTracking(), "LevelId", "LevelName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ConfirmAllCourses(string id)
        {
            int count = 0;
            var allRegCourse = await _db.CourseRegistrations.AsNoTracking().Where(x => x.StudentId.Equals(id) &&
                                                         x.SemesterId.Equals(semesterId)
                                                         && x.SessionId.Equals(sessionId)
                                                         && x.IsApproved.Equals(false))
                                                         .ToListAsync();

            if (allRegCourse.Any())
            {
                foreach (var course in allRegCourse)
                {
                    var editCourseReg = await _db.CourseRegistrations.FindAsync(course.CourseRegistrationId);
                    if (editCourseReg != null)
                    {
                        editCourseReg.IsApproved = true;
                        count += 1;
                    }
                    _db.Entry(editCourseReg).State = EntityState.Modified;
                }
                await _db.SaveChangesAsync();
                var message = $"{count} Course(s) Approved Successfully...";
                return new JsonResult { Data = new { status = true, message } };
            }
            return new JsonResult
            {
                Data = new
                {
                    status = false,
                    message = "There is no pending course Registration Found. " +
                                "Please Register a course and try again"
                }
            };
            // return RedirectToAction("Create");
        }


        // GET: CourseRegistrations/Create
        public async Task<ActionResult> Create(string message)
        {
            var result = ConfirmSchoolFee();
            if (result != null)
                return result;

            var checckStudent = await _db.Students.Where(x => x.StudentId.Equals(studentId)).FirstOrDefaultAsync();
            //if (checckStudent.SessionId == 1 || checckStudent.SessionId <= 24)
            //{
            //    var idCardCheck = await _db.IdCardPayments.Include(x => x.Student)
            //                                    //.Include(x => x.Student.Session)
            //                                    .Where(x => x.StudentId.Equals(studentId) && x.IsPayed.Equals(true)).FirstOrDefaultAsync(); //1=2018/2019 & 20=2019/2020
            //    if (idCardCheck == null)
            //    {
            //        return RedirectToAction("RegisteredCourse", "CourseRegistrations",
            //        new
            //        {
            //            message = "Please apply for your ID card before you can do the course registration!"
            //        });
            //    }
            //}

            

            var courseList = new List<Course>();
            var carryOverCourses = new List<Course>();
            var deCourseList = new List<Course>();
            var unRegisteredcourseList = new List<Course>();
            var prerequisteCourseList = new List<CoursePrerequisite>();
            var preCourseList = new List<Course>();
            var student = _studentQuery.GetStudent(userId);

            var checkCourseRegSetting = _courseReg.CheckCourseRegSetting(student.SchoolProgrammeId, semesterId, sessionId);
            if (checkCourseRegSetting != null)
            {
                int dateCompare1 = DateTime.Compare(DateTime.Now.Date, checkCourseRegSetting.ClosingDate);
                if (dateCompare1 > 0)
                {
                    return RedirectToAction("RegisteredCourse", "CourseRegistrations",
                    new
                    {
                        message = $"The registration closing date is {checkCourseRegSetting.ClosingDate.ToString("dd MMM yyyy")}"
                    });
                }
            }
            else
            {
                return RedirectToAction("RegisteredCourse", "CourseRegistrations",
                new
                {
                    message = $"Contact the Hod to Setup opening registration Date and closing date"
                });
            }

            var courseReg = _db.CourseRegistrations.Any(x => x.StudentId.Equals(student.StudentId)
                                           && x.SemesterId.Equals(semesterId) && x.SessionId.Equals(sessionId)
                                           && x.IsApproved.Equals(true));

            if (courseReg)
            {
                return RedirectToAction("RegisteredCourse", "CourseRegistrations",
                 new
                 {
                     message = "You cant make any more changes because you have successfully submitted your course Registration and the Level Coordinator has approve the course registration."
                 });
            }

            var courseLoad = await _db.CourseLoadSettings.Include(i => i.Session).AsNoTracking()
                               .Where(x => x.ProgrammeId.Equals(student.Programme.ProgrammeId)
                               && x.LevelId.Equals(student.Level.LevelId)
                               && x.Session.SessionId.Equals(sessionId)).FirstOrDefaultAsync();
            if (courseLoad == null)
            {
                return RedirectToAction("RegisteredCourse", "CourseRegistrations",
                    new
                    {
                        message = "Maximum and Minimum Course Load has not been set by the HOD for this department option. Please try again later"
                    });
            }

            if (student == null)
            {
                return RedirectToAction("RegisteredCourse", "CourseRegistrations",
                    new
                    {
                        message = "Please, Check your profile to be sure you are assigned Level and Departmental Option"
                    });
            }
            //Querying db for list of courses available for the student based on the current semester, level and departmental Option
            var courses = await _db.Courses.Include(i => i.Programme).Include(i => i.Semester).Include(i => i.Level)
                                    .AsNoTracking()
                                    .Where(x => x.Programme.ProgrammeId.Equals(student.Programme.ProgrammeId)
                                     && x.Level.LevelId.Equals(student.Level.LevelId)
                                     //&& x.Semester.SemesterId.Equals(semesterId)  // Course Reg is sessional
                                     && x.DeActivatedCourse.Equals(false))
                                    .ToListAsync();



            if (!string.IsNullOrEmpty(student.ModeOfEntry) && student.ModeOfEntry.ToUpper().Trim().Equals("DE"))
            {
                var deCourses = await _db.DeCoreCourses.Include(i => i.Course).AsNoTracking()
                                .Where(x => x.ProgrammeId.Equals((int)student.ProgrammeId) &&
                                x.LevelId.Equals((int)student.LevelId))
                                .Select(s => s.Course).ToListAsync();
                deCourseList.AddRange(deCourses);
            }

            if (checkCourseRegSetting.ActivatePrequisite.Equals(true))
            {
                var caList = await _db.ContinuousAssessments.Include(i => i.Course.Level).AsNoTracking()
                                       .Where(x => x.StudentId.Equals(student.StudentId)).ToListAsync();

                carryOverCourses = _resultQuery.CheckForCarryOver(caList, student);
                foreach (var course in carryOverCourses)
                {
                    var checkPrerequisite = await _db.CoursePrerequisites.AsNoTracking()
                                            .Where(x => x.PrerequisiteCourseId.Equals(course.CourseId))
                                            .ToListAsync();
                    if (checkPrerequisite.Any())
                    {
                        //preCourseList.Add(course); //To display the list of carryover prerequisite course
                        prerequisteCourseList.AddRange(checkPrerequisite);
                    }
                }
                foreach (var course in prerequisteCourseList)
                {
                    var removeCourseId = courses.Where(x => x.CourseId.Equals(course.CourseId)).ToList();
                    if (removeCourseId.Any())
                    {
                        courseList.AddRange(removeCourseId);
                        preCourseList.AddRange(removeCourseId);
                    }
                }
                foreach (var item in courseList)
                {
                    courses.Remove(courses.FirstOrDefault(x => x.CourseId.Equals(item.CourseId)));
                }
            }
            else
            {
                carryOverCourses = await _resultQuery.CheckForPreviousLevelCourse(student);
            }

            ViewBag.Message = message;

            var registeredCarryOverCourse = carryOverCourses;

            var courseRegVm = new CourseRegistrationVm()
            {
                CarryOverCoursesId = registeredCarryOverCourse.Select(s => s.CourseId).ToArray(),
                PrerequisiteCoursesId = preCourseList.Select(s => s.CourseId).ToArray(),
                AvailableCredit = courseLoad.MaximumCreditLoad - registeredCarryOverCourse.Sum(s => s.Credits),
                MaximumCreditLoad = courseLoad.MaximumCreditLoad,
                MinimumCreditLoad = courseLoad.MinimumCreditLoad,
                CoreCourses = courses.Where(x => x.CourseType.ToUpper().Equals(CourseType.Core.ToString().ToUpper())).Select(x => x.CourseId).ToArray(),
                StudentId = student.StudentId,
                LevelId = student.Level.LevelId,
                SemesterId = semesterId,
                SessionId = sessionId,
                DepartmentId = student.Programme.Department.DepartmentId,
                ProgrammeId = student.Programme.ProgrammeId

            };
            ViewBag.CarryOverCourses = carryOverCourses;
            ViewBag.Courses = courses;
            ViewBag.DeCourses = deCourseList;
            ViewBag.UnRegisteredCourse = preCourseList;
            return View(courseRegVm);
        }


        public async Task<ActionResult> CreatePreviousCourseReg( PreviousCourseRegistrationVm model)
        {
            var result = ConfirmSchoolFee();
            //if (result != null)
            //    return result;

            int SemesterId = 1;
            var StudentId = _studentQuery.GetStudentId(model.MatNum);

            var checckStudent = await _db.Students.Where(x => x.StudentId.Equals(StudentId)).FirstOrDefaultAsync();
            if (checckStudent.SessionId == 1 || checckStudent.SessionId >= 20)
            {
                var idCardCheck = await _db.IdCardPayments.Include(x => x.Student)
                                                //.Include(x => x.Student.Session)
                                                .Where(x => x.StudentId.Equals(StudentId) && x.IsPayed.Equals(true)).FirstOrDefaultAsync(); //1=2018/2019 & 20=2019/2020
                if (idCardCheck == null)
                {
                    return RedirectToAction("RegisteredCourse", "CourseRegistrations",
                    new
                    {
                        message = "Please apply for your ID card before you can do the course registration!"
                    });
                }
            }

            

            var courseList = new List<Course>();
            var carryOverCourses = new List<Course>();
            var deCourseList = new List<Course>();
            var unRegisteredcourseList = new List<Course>();
            var prerequisteCourseList = new List<CoursePrerequisite>();
            var preCourseList = new List<Course>();
            var student = _studentQuery.GetStudent(checckStudent.Email);

            var checkCourseRegSetting = _courseReg.CheckPreviousCourseRegSetting(student.SchoolProgrammeId, model.SessionId);
            

            var courseReg = _db.CourseRegistrations.Any(x => x.StudentId.Equals(student.StudentId)
                                           && x.SemesterId.Equals(SemesterId) && x.SessionId.Equals(model.SessionId)
                                           && x.IsApproved.Equals(true));

            if (courseReg)
            {
                return RedirectToAction("RegisteredCourse", "CourseRegistrations",
                 new
                 {
                     message = "You cant make any more changes because you have successfully submitted your course Registration and the Level Coordinator has approve the course registration."
                 });
            }

            var courseLoad = await _db.CourseLoadSettings.Include(i => i.Session).AsNoTracking()
                               .Where(x => x.ProgrammeId.Equals(student.Programme.ProgrammeId)
                               && x.LevelId.Equals(model.LevelId)
                               && x.Session.SessionId.Equals(model.SessionId)).FirstOrDefaultAsync();
            if (courseLoad == null)
            {
                return RedirectToAction("RegisteredCourse", "CourseRegistrations",
                    new
                    {
                        message = "Maximum and Minimum Course Load has not been set by the HOD for this department option. Please try again later"
                    });
            }

            if (student == null)
            {
                return RedirectToAction("RegisteredCourse", "CourseRegistrations",
                    new
                    {
                        message = "Please, Check your profile to be sure you are assigned Level and Departmental Option"
                    });
            }
            //Querying db for list of courses available for the student based on the current semester, level and departmental Option
            var courses = await _db.Courses.Include(i => i.Programme).Include(i => i.Semester).Include(i => i.Level)
                                    .AsNoTracking()
                                    .Where(x => x.Programme.ProgrammeId.Equals(student.Programme.ProgrammeId)
                                     && x.Level.LevelId.Equals(model.LevelId)
                                     //&& x.Semester.SemesterId.Equals(semesterId)  // Course Reg is sessional
                                     && x.DeActivatedCourse.Equals(false))
                                    .ToListAsync();



            if (!string.IsNullOrEmpty(student.ModeOfEntry) && student.ModeOfEntry.ToUpper().Trim().Equals("DE"))
            {
                var deCourses = await _db.DeCoreCourses.Include(i => i.Course).AsNoTracking()
                                .Where(x => x.ProgrammeId.Equals((int)student.ProgrammeId) &&
                                x.LevelId.Equals((int)model.LevelId))
                                .Select(s => s.Course).ToListAsync();
                deCourseList.AddRange(deCourses);
            }

            if (checkCourseRegSetting.ActivatePrequisite.Equals(true))
            {
                var caList = await _db.ContinuousAssessments.Include(i => i.Course.Level).AsNoTracking()
                                       .Where(x => x.StudentId.Equals(student.StudentId)).ToListAsync();

                carryOverCourses = _resultQuery.CheckForCarryOver(caList, student);
                foreach (var course in carryOverCourses)
                {
                    var checkPrerequisite = await _db.CoursePrerequisites.AsNoTracking()
                                            .Where(x => x.PrerequisiteCourseId.Equals(course.CourseId))
                                            .ToListAsync();
                    if (checkPrerequisite.Any())
                    {
                        //preCourseList.Add(course); //To display the list of carryover prerequisite course
                        prerequisteCourseList.AddRange(checkPrerequisite);
                    }
                }
                foreach (var course in prerequisteCourseList)
                {
                    var removeCourseId = courses.Where(x => x.CourseId.Equals(course.CourseId)).ToList();
                    if (removeCourseId.Any())
                    {
                        courseList.AddRange(removeCourseId);
                        preCourseList.AddRange(removeCourseId);
                    }
                }
                foreach (var item in courseList)
                {
                    courses.Remove(courses.FirstOrDefault(x => x.CourseId.Equals(item.CourseId)));
                }
            }
            else
            {
                carryOverCourses = await _resultQuery.CheckForPreviousLevelCourse(student);
            }

            //ViewBag.Message = message;

            var registeredCarryOverCourse = carryOverCourses;

            var courseRegVm = new CourseRegistrationVm()
            {
                CarryOverCoursesId = registeredCarryOverCourse.Select(s => s.CourseId).ToArray(),
                PrerequisiteCoursesId = preCourseList.Select(s => s.CourseId).ToArray(),
                AvailableCredit = courseLoad.MaximumCreditLoad - registeredCarryOverCourse.Sum(s => s.Credits),
                MaximumCreditLoad = courseLoad.MaximumCreditLoad,
                MinimumCreditLoad = courseLoad.MinimumCreditLoad,
                CoreCourses = courses.Where(x => x.CourseType.ToUpper().Equals(CourseType.Core.ToString().ToUpper())).Select(x => x.CourseId).ToArray(),
                StudentId = student.StudentId,
                LevelId = model.LevelId,
                SemesterId = SemesterId,
                SessionId = model.SessionId,
                DepartmentId = student.Programme.Department.DepartmentId,
                ProgrammeId = student.Programme.ProgrammeId

            };
            ViewBag.CarryOverCourses = carryOverCourses;
            ViewBag.Courses = courses;
            ViewBag.DeCourses = deCourseList;
            ViewBag.UnRegisteredCourse = preCourseList;
            return View(courseRegVm);
        }

        // POST: CourseRegistrations/Create To protect from overposting attacks, please enable the
        // specific properties you want to bind to, for more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CourseRegistrationVm model)
        {
            if (ModelState.IsValid)
            {
                var student = await _db.Students.AsNoTracking().Include(i => i.Programme)
                                   .Include(i => i.Programme.Department).Include(i => i.Level)
                           .Where(x => x.Email.Trim().ToUpper().Equals(userId.Trim().ToUpper()))
                           .Select(s => new
                           {
                               s.StudentId,
                               s.FirstName, //Added to be used for SMS 
                               s.PhoneNumber, //Added to be used for enable SMS
                               myProgrammeId = s.Programme.ProgrammeId,
                               s.Level.LevelId,
                               s.MatricNo,
                               s.Programme.Department.DepartmentId
                           })
                           .FirstOrDefaultAsync();
                string SMSbody = $"{student.FirstName} your course registration is complete, kindly see your Dept/Level Cordinator for final clearance and documentation";

                var courseLoad = await _db.CourseLoadSettings.Include(i => i.Session).AsNoTracking()
                              .Where(x => x.ProgrammeId.Equals(student.myProgrammeId)
                               && x.LevelId.Equals(student.LevelId)
                              && x.Session.SessionId.Equals(sessionId)).FirstOrDefaultAsync();

                var registeredCourseId = new List<int>();
                if (model.CarryOverCoursesId != null)
                {
                    registeredCourseId.AddRange(model.CarryOverCoursesId);
                }
                if (model.CoreCourses != null)
                {
                    registeredCourseId.AddRange(model.CoreCourses);
                }
                if (model.CourseId != null && model.CourseId.Count() > 0)
                {
                    registeredCourseId.AddRange(model.CourseId);
                }

                int courseCreditTotal = 0;
                //Determine total credit units of registered courses and make sure that the
                //total credit units of courses registered doesn't exceed 24.
                foreach (var courseId in registeredCourseId)
                {
                    var course = await _db.Courses.FindAsync(courseId);
                    if (course != null) courseCreditTotal += course.Credits;
                }


                if (courseCreditTotal < courseLoad.MinimumCreditLoad) // Checking if registered course is not less than 24
                {
                    return RedirectToAction("Create", new { message = "You are trying to register LESS than the required Course Load" });
                }
                if (courseCreditTotal > courseLoad.MaximumCreditLoad) // Checking if registered course is not greater than 24
                {
                    return RedirectToAction("Create", new { message = "You are trying to register MORE than the required Course Load" });
                }

                var oldCourseRegs = await _db.CourseRegistrations.AsNoTracking().Where(x => x.StudentId.Equals(student.StudentId) &&
                                        x.SessionId.Equals(model.SessionId)).ToListAsync();
                if (oldCourseRegs.Any())
                {
                    foreach (var oldRegistration in oldCourseRegs)
                    {
                        _db.Entry(oldRegistration).State = EntityState.Deleted;
                    }
                    _db.SaveChanges();
                }
                foreach (var courseId in registeredCourseId)
                {
                    var deptId = await _db.Programmes.AsNoTracking()
                                        .Where(x => x.ProgrammeId.Equals(model.ProgrammeId))
                                        .Select(s => s.Department.DepartmentId).FirstOrDefaultAsync();

                    var courseRegistration = new CourseRegistration
                    {
                        StudentId = student.StudentId,
                        LevelId = model.LevelId,
                        SemesterId = model.SemesterId,
                        SessionId = model.SessionId,
                        ProgrammeId = model.ProgrammeId,
                        CourseId = courseId,
                        DepartmentId = deptId,
                        IsStudentRegistered = true
                    };
                    _db.CourseRegistrations.Add(courseRegistration);
                }

                await _db.SaveChangesAsync();
                await SMSClass.SendSMS("UNIJOS SIS", SMSbody, student.PhoneNumber); //EBULK SMS API

                return RedirectToAction("RegisteredCourse");
            }

            return RedirectToAction("Create");
        }

        
        // POST: CourseRegistrations/Create To protect from overposting attacks, please enable the
        // specific properties you want to bind to, for more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreatePreviousCourseReg(CourseRegistrationVm model)
        {
            if (ModelState.IsValid)
            {
                var student = await _db.Students.AsNoTracking().Include(i => i.Programme)
                                   .Include(i => i.Programme.Department).Include(i => i.Level)
                           .Where(x => x.StudentId.Equals(model.StudentId))
                           .Select(s => new
                           {
                               s.StudentId,
                               s.FirstName, //Added to be used for SMS 
                               s.PhoneNumber, //Added to be used for enable SMS
                               myProgrammeId = s.Programme.ProgrammeId,
                               s.Level.LevelId,
                               s.MatricNo,
                               s.Programme.Department.DepartmentId
                           })
                           .FirstOrDefaultAsync();
                string SMSbody = $"{student.FirstName} your course registration is complete, kindly see your Dept/Level Cordinator for final clearance and documentation";

                var courseLoad = await _db.CourseLoadSettings.Include(i => i.Session).AsNoTracking()
                              .Where(x => x.ProgrammeId.Equals(model.ProgrammeId)
                               && x.LevelId.Equals((Int32)model.LevelId)
                              && x.Session.SessionId.Equals(model.SessionId)).FirstOrDefaultAsync();

                var registeredCourseId = new List<int>();
                if (model.CarryOverCoursesId != null)
                {
                    registeredCourseId.AddRange(model.CarryOverCoursesId);
                }
                if (model.CoreCourses != null)
                {
                    registeredCourseId.AddRange(model.CoreCourses);
                }
                if (model.CourseId != null && model.CourseId.Count() > 0)
                {
                    registeredCourseId.AddRange(model.CourseId);
                }

                int courseCreditTotal = 0;
                //Determine total credit units of registered courses and make sure that the
                //total credit units of courses registered doesn't exceed 24.
                foreach (var courseId in registeredCourseId)
                {
                    var course = await _db.Courses.FindAsync(courseId);
                    if (course != null) courseCreditTotal += course.Credits;
                }


                if (courseCreditTotal < courseLoad.MinimumCreditLoad) // Checking if registered course is not less than 24
                {
                    return RedirectToAction("CourseRegError", new { message = "You are trying to register LESS than the required Course Load" });
                }
                if (courseCreditTotal > courseLoad.MaximumCreditLoad) // Checking if registered course is not greater than 24
                {
                    return RedirectToAction("CourseRegError", new { message = "You are trying to register MORE than the required Course Load" });
                }

                var oldCourseRegs = await _db.CourseRegistrations.AsNoTracking().Where(x => x.StudentId.Equals(student.StudentId) &&
                                        x.SessionId.Equals(model.SessionId)).ToListAsync();
                if (oldCourseRegs.Any())
                {
                    foreach (var oldRegistration in oldCourseRegs)
                    {
                        _db.Entry(oldRegistration).State = EntityState.Deleted;
                    }
                    _db.SaveChanges();
                }
                foreach (var courseId in registeredCourseId)
                {
                    var deptId = await _db.Programmes.AsNoTracking()
                                        .Where(x => x.ProgrammeId.Equals(model.ProgrammeId))
                                        .Select(s => s.Department.DepartmentId).FirstOrDefaultAsync();

                    var courseRegistration = new CourseRegistration
                    {
                        StudentId = student.StudentId,
                        LevelId = model.LevelId,
                        SemesterId = model.SemesterId,
                        SessionId = model.SessionId,
                        ProgrammeId = model.ProgrammeId,
                        CourseId = courseId,
                        DepartmentId = deptId,
                        IsStudentRegistered = true
                    };
                    _db.CourseRegistrations.Add(courseRegistration);
                }

                await _db.SaveChangesAsync();
                await SMSClass.SendSMS("UNIJOS SIS", SMSbody, student.PhoneNumber); //EBULK SMS API

                return RedirectToAction("RegisteredPreviousCourse", new { message = "You are trying to register MORE than the required Course Load", studentId=student.StudentId });
                //return View("SUCCESS!");
            }

            return RedirectToAction("Create");
        }


        // This function gets hit when a student is trying to register more or less than the required course loads, for previous course reg
        public string CourseRegError(string message)
        {
            return $"<h2 style='color:red; display:flex; justify-content:center; margin-top:50px;'>{message}</h2>";
        }


        #region Old Create/Registration Code
        //// GET: CourseRegistrations/Create
        //public async Task<ActionResult> Create(string message)
        //{
        //    var result = ConfirmSchoolFee();
        //    if (result != null)
        //        return result;



        //    var courseList = new List<Course>();
        //    var unRegisteredcourseList = new List<Course>();
        //    var prerequisteCourseList = new List<CoursePrerequisite>();
        //    var studentSearch = await _db.Students.AsNoTracking().Include(i => i.Programme)
        //                            .Include(i => i.Programme.Department).Include(i => i.Level)
        //                            .Where(x => x.Email.Equals(userId)).ToListAsync();    //.FirstOrDefaultAsync();

        //    var student = studentSearch.Select(s => new
        //    {
        //        s.StudentId,
        //        s.SchoolProgrammeId,
        //        myProgrammeId = s.Programme.ProgrammeId,
        //        s.Level.LevelId,
        //        s.MatricNo,
        //        s.Programme.Department.DepartmentId,
        //        s.IsClearedAll
        //    }).FirstOrDefault();

        //    var checkCourseRegSetting = _db.CourseRegSettings.AsNoTracking().Where(x => x.SchoolProgrammeId.Equals(student.SchoolProgrammeId) &&
        //                                x.SessionId.Equals(sessionId) && x.SemesterId.Equals(semesterId) && x.IsActive.Equals(true))
        //                                .FirstOrDefault();

        //    int dateCompare1 = DateTime.Compare(DateTime.Now.Date, checkCourseRegSetting.ClosingDate);
        //    if (dateCompare1 > 0)
        //    {
        //        return RedirectToAction("RegisteredCourse", "CourseRegistrations",
        //        new
        //        {
        //            message = $"The registration closing date is {checkCourseRegSetting.ClosingDate.ToString("dd MMM yyyy")}"
        //        });
        //    }

        //    var courseReg = _db.CourseRegistrations.Any(x => x.StudentId.Equals(student.StudentId)
        //                                   && x.SemesterId.Equals(semesterId) && x.SessionId.Equals(sessionId)
        //                                   && x.IsStudentRegistered.Equals(true));

        //    if (courseReg)
        //    {
        //        return RedirectToAction("RegisteredCourse", "CourseRegistrations",
        //         new
        //         {
        //             message = "You have successfully submitted your course Registration. Kindly wait for the Level Coordinator approval or disapproval"
        //         });
        //    }

        //    var courseLoad = await _db.CourseLoadSettings.Include(i => i.Session).AsNoTracking()
        //                       .Where(x => x.ProgrammeId.Equals(student.myProgrammeId)
        //                       && x.LevelId.Equals(student.LevelId)
        //                       && x.Session.SessionId.Equals(sessionId)).FirstOrDefaultAsync();
        //    if (courseLoad == null)
        //    {

        //        return RedirectToAction("RegisteredCourse", "CourseRegistrations",
        //            new
        //            {
        //                message = "Maximum and Minimum Course Load has not been set by the HOD for this department option. Please try again later"
        //            });
        //    }

        //    if (student == null)
        //    {
        //        return RedirectToAction("RegisteredCourse", "CourseRegistrations",
        //            new
        //            {
        //                message = "Please, Check your profile to be sure you are assigned Level and Departmental Option"
        //            });
        //    }


        //    //Querying db for list of courses available for the student based on the current semester, level and departmental Option
        //    var courses = await _db.Courses.Include(i => i.Programme).Include(i => i.Semester).Include(i => i.Level)
        //                            .AsNoTracking()
        //                            .Where(x => x.Programme.ProgrammeId.Equals(student.myProgrammeId)
        //                             && x.Level.LevelId.Equals(student.LevelId)
        //                             //&& x.Semester.SemesterId.Equals(semesterId)  // Course Reg is sessional
        //                             && x.DeActivatedCourse.Equals(false))
        //                            .ToListAsync();
        //    //var carryOverCourses = _db.CarryOverCourses.Include(i => i.Course.Level).AsNoTracking()
        //    //                            .Where(x => x.StudentId.Equals(student.StudentId))
        //    //                            .Select(s => s.Course).ToList();

        //    var carryOverCourses = _db.ContinuousAssessments.Include(i => i.Course.Level).AsNoTracking()
        //                                .Where(x => x.StudentId.Equals(student.StudentId) &&
        //                                x.Total <= 39).Select(s => s.Course).ToList();
        //    foreach (var course in carryOverCourses)
        //    {
        //        var checkPrerequisite = await _db.CoursePrerequisites.AsNoTracking()
        //                                .Where(x => x.CourseId.Equals(course.CourseId))
        //                                .ToListAsync();
        //        prerequisteCourseList.AddRange(checkPrerequisite);
        //        courseList.Add(course);
        //        courses.Add(course);
        //    }
        //    foreach (var course in courses)
        //    {
        //        foreach (var item in prerequisteCourseList)
        //        {
        //            if (course.CourseId.Equals(item))
        //            {
        //                unRegisteredcourseList.Add(course);
        //            }
        //            else
        //            {
        //                courseList.Add(course);
        //            }
        //        }
        //    }

        //    ViewBag.StudentId = student.MatricNo;
        //    ViewBag.Message = message;

        //    ViewBag.CourseId = new MultiSelectList(courses.Where(x => x.CourseType.ToUpper() != CourseType.Core.ToString().ToUpper()), "CourseId", "CourseName");
        //    ViewBag.LevelId = new SelectList(_db.Levels.AsNoTracking().Where(x => x.LevelId.Equals(student.LevelId)), "LevelId", "LevelName");

        //    ViewBag.ProgrammeId = new SelectList(_db.Programmes.AsNoTracking()
        //        .Where(x => x.ProgrammeId.Equals(student.myProgrammeId)),
        //        "ProgrammeId", "ProgrammeName");

        //    ViewBag.DepartmentId = new SelectList(_db.Programmes.AsNoTracking()
        //        .Where(x => x.ProgrammeId.Equals(student.myProgrammeId)),
        //        "DepartmentId", "DepartmentName");

        //    ViewBag.SemesterId = new SelectList(_query.GetCurrentSemesterList(studentSchoolProgrammeId), "SemesterId", "SemesterName");
        //    ViewBag.SessionId = new SelectList(_query.GetCurrentSessionList(studentSchoolProgrammeId), "SessionId", "SessionName");

        //    //var registeredCarryOverCourse = carryOverCourses.Where(x => x.CourseType.ToUpper().Equals(CourseType.Core.ToString().ToUpper()));
        //    var registeredCarryOverCourse = carryOverCourses;

        //    var courseRegVm = new CourseRegistrationVm()
        //    {
        //        ProgrammeId = Convert.ToInt16(carryOverCourses.Select(x => x.ProgrammeId).FirstOrDefault()),
        //        CarryOverCoursesId = registeredCarryOverCourse.Select(s => s.CourseId).ToArray(),
        //        PrerequisiteCoursesId = unRegisteredcourseList.Select(s => s.CourseId).ToArray(),
        //        AvailableCredit = courseLoad.MaximumCreditLoad - registeredCarryOverCourse.Sum(s => s.Credits),
        //        MaximumCreditLoad = courseLoad.MaximumCreditLoad,
        //        MinimumCreditLoad = courseLoad.MinimumCreditLoad,
        //        CoreCourses = courses.Where(x => x.CourseType.ToUpper().Equals(CourseType.Core.ToString().ToUpper())).Select(x => x.CourseId).ToArray()
        //    };
        //    ViewBag.CarryOverCourses = carryOverCourses;
        //    ViewBag.Courses = courses;
        //    ViewBag.UnRegisteredCourse = unRegisteredcourseList;
        //    return View(courseRegVm);
        //}

        //// POST: CourseRegistrations/Create To protect from overposting attacks, please enable the
        //// specific properties you want to bind to, for more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Create(CourseRegistrationVm model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var student = await _db.Students.AsNoTracking().Include(i => i.Programme)
        //                           .Include(i => i.Programme.Department).Include(i => i.Level)
        //                   .Where(x => x.Email.Equals(userId))
        //                   .Select(s => new
        //                   {
        //                       s.StudentId,
        //                       myProgrammeId = s.Programme.ProgrammeId,
        //                       s.Level.LevelId,
        //                       s.MatricNo,
        //                       s.Programme.Department.DepartmentId
        //                   })
        //                   .FirstOrDefaultAsync();
        //        var courseLoad = await _db.CourseLoadSettings.Include(i => i.Session).AsNoTracking()
        //                      .Where(x => x.ProgrammeId.Equals(student.myProgrammeId)
        //                       && x.LevelId.Equals(student.LevelId)
        //                      && x.Session.SessionId.Equals(sessionId)).FirstOrDefaultAsync();

        //        var registeredCourseId = new List<int>();
        //        if (model.CarryOverCoursesId != null)
        //        {
        //            registeredCourseId.AddRange(model.CarryOverCoursesId);
        //        }
        //        if (model.CoreCourses != null)
        //        {
        //            registeredCourseId.AddRange(model.CoreCourses);
        //        }

        //        registeredCourseId.AddRange(model.CourseId);

        //        int courseCreditTotal = 0;
        //        //Determine total credit units of registered courses and make sure that the
        //        //total credit units of courses registered doesn't exceed 24.
        //        foreach (var courseId in registeredCourseId)
        //        {
        //            var course = await _db.Courses.FindAsync(courseId);
        //            if (course != null) courseCreditTotal += course.Credits;
        //        }


        //        if (courseCreditTotal < courseLoad.MinimumCreditLoad) // Checking if registered course is not less than 24
        //        {
        //            return RedirectToAction("Create", new { message = "You are trying to register less than the required Course Load" });
        //        }
        //        if (courseCreditTotal > courseLoad.MaximumCreditLoad) // Checking if registered course is not greater than 24
        //        {
        //            return RedirectToAction("Create", new { message = "You are trying to register more than the required Course Load" });
        //        }

        //        var oldCourseRegs = await _db.CourseRegistrations.AsNoTracking().Where(x => x.StudentId.Equals(student.StudentId) &&
        //                                x.SessionId.Equals(model.SessionId)).ToListAsync();
        //        if (oldCourseRegs.Any())
        //        {
        //            foreach (var oldRegistration in oldCourseRegs)
        //            {
        //                _db.Entry(oldRegistration).State = EntityState.Deleted;
        //            }
        //            _db.SaveChanges();
        //        }
        //        foreach (var courseId in registeredCourseId)
        //        {
        //            var deptId = await _db.Programmes.AsNoTracking()
        //                                .Where(x => x.ProgrammeId.Equals(model.ProgrammeId))
        //                                .Select(s => s.Department.DepartmentId).FirstOrDefaultAsync();

        //            var courseRegistration = new CourseRegistration
        //            {
        //                StudentId = student.StudentId,
        //                LevelId = model.LevelId,
        //                SemesterId = model.SemesterId,
        //                SessionId = model.SessionId,
        //                ProgrammeId = model.ProgrammeId,
        //                CourseId = courseId,
        //                DepartmentId = deptId,
        //                IsStudentRegistered = true
        //            };
        //            _db.CourseRegistrations.Add(courseRegistration);
        //        }

        //        await _db.SaveChangesAsync();
        //        return RedirectToAction("RegisteredCourse");
        //    }

        //    return RedirectToAction("Create");
        //} 
        #endregion

        #region Unused code
        //// GET: CourseRegistrations/Edit/5
        //public async Task<ActionResult> Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    CourseRegistration courseRegistration = await _db.CourseRegistrations.FindAsync(id);
        //    if (courseRegistration == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    ViewBag.CourseId = new SelectList(_db.Courses, "CourseId", "CourseCode", courseRegistration.CourseId);
        //    ViewBag.LevelId = new SelectList(_db.Levels, "LevelId", "LevelName", courseRegistration.LevelId);
        //    ViewBag.ProgrammeId = new SelectList(_db.Programmes, "ProgrammeId", "ProgrammeCode", courseRegistration.ProgrammeId);
        //    ViewBag.SemesterId = new SelectList(_db.Semesters.AsNoTracking().Where(x => x.ActiveSemester.Equals(true)), "SemesterId", "SemesterName");
        //    ViewBag.SessionId = new SelectList(_db.Sessions.AsNoTracking().Where(x => x.ActiveSession.Equals(true)), "SessionId", "SessionName");
        //    return View(courseRegistration);
        //}

        //// POST: CourseRegistrations/Edit/5 To protect from overposting attacks, please enable the
        //// specific properties you want to bind to, for more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Edit([Bind(Include = "CourseRegistrationId,StudentId,LevelId,ProgrammeId,DepartmentId,SemesterId,SessionId,CourseId,IsApproved")] CourseRegistration courseRegistration)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var departmentId = await _db.Programmes.AsNoTracking()
        //                           .Where(c => c.ProgrammeId.Equals(courseRegistration.ProgrammeId))
        //                           .Select(x => x.DepartmentId).FirstOrDefaultAsync();
        //        courseRegistration.DepartmentId = departmentId;
        //        _db.Entry(courseRegistration).State = EntityState.Modified;
        //        await _db.SaveChangesAsync();
        //        return RedirectToAction("Index");
        //    }
        //    ViewBag.CourseId = new SelectList(_db.Courses, "CourseId", "CourseName", courseRegistration.CourseId);
        //    ViewBag.LevelId = new SelectList(_db.Levels, "LevelId", "LevelName", courseRegistration.LevelId);
        //    ViewBag.ProgrammeId = new SelectList(_db.Programmes, "ProgrammeId", "ProgrammeCode", courseRegistration.ProgrammeId);
        //    ViewBag.SemesterId = new SelectList(_db.Semesters.AsNoTracking().Where(x => x.ActiveSemester.Equals(true)), "SemesterId", "SemesterName");
        //    ViewBag.SessionId = new SelectList(_db.Sessions.AsNoTracking().Where(x => x.ActiveSession.Equals(true)), "SessionId", "SessionName");
        //    return View(courseRegistration);
        //}

        //public async Task<ActionResult> AdminEdit(string id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    var courseRegistration = await _db.CourseRegistrations.Where(x => x.StudentId.Equals(id)).ToListAsync();
        //    if (courseRegistration == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    ViewBag.CourseId = new SelectList(courseRegistration, "CourseId", "CourseCode", courseRegistration.CourseId);
        //    ViewBag.LevelId = new SelectList(_db.Levels.AsNoTracking().Where(x => x.LevelId.Equals((int)levelId)), "LevelId", "LevelName");
        //    ViewBag.ProgrammeId = new SelectList(_db.Programmes.AsNoTracking().Where(x => x.ProgrammeId.Equals((int)programeId)), "ProgrammeId", "ProgrammeName");
        //    ViewBag.DepartmentId = new SelectList(_db.Programmes.AsNoTracking().Where(x => x.ProgrammeId.Equals((int)programeId)), "DepartmentId", "DepartmentName");
        //    ViewBag.SemesterId = new SelectList(_db.Semesters.AsNoTracking().Where(x => x.ActiveSemester.Equals(true)), "SemesterId", "SemesterName");
        //    ViewBag.SessionId = new SelectList(_db.Sessions.AsNoTracking().Where(x => x.ActiveSession.Equals(true)), "SessionId", "SessionName");
        //    return View(courseRegistration);
        //}

        //// POST: CourseRegistrations/Edit/5
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for
        //// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> AdminEdit([Bind(Include = "CourseRegistrationId,StudentId,LevelId,ProgrammeId,DepartmentId,SemesterId,SessionId,CourseId,IsApproved")] CourseRegistration courseRegistration)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var departmentId = await _db.Programmes.AsNoTracking()
        //                           .Where(c => c.ProgrammeId.Equals(courseRegistration.ProgrammeId))
        //                           .Select(x => x.DepartmentId).FirstOrDefaultAsync();
        //        courseRegistration.DepartmentId = departmentId;
        //        _db.Entry(courseRegistration).State = System.Data.Entity.EntityState.Modified;
        //        await _db.SaveChangesAsync();
        //        return RedirectToAction("Index");
        //    }
        //    ViewBag.CourseId = new SelectList(_db.Courses, "CourseId", "CourseCode", courseRegistration.CourseId);
        //    ViewBag.LevelId = new SelectList(_db.Levels, "LevelId", "LevelName", courseRegistration.LevelId);
        //    ViewBag.ProgrammeId = new SelectList(_db.Programmes, "ProgrammeId", "ProgrammeCode", courseRegistration.ProgrammeId);
        //    ViewBag.SemesterId = new SelectList(_db.Semesters, "SemesterId", "SemesterName", courseRegistration.SemesterId);
        //    ViewBag.SessionId = new SelectList(_db.Sessions, "SessionId", "SessionName", courseRegistration.SessionId);
        //    return View(courseRegistration);
        //} 
        #endregion

        // GET: CourseRegistrations/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CourseRegistration courseRegistration = await _db.CourseRegistrations.FindAsync(id);
            if (courseRegistration == null)
            {
                return HttpNotFound();
            }
            return View(courseRegistration);
        }

        // POST: CourseRegistrations/Delete/5
        [HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            bool status = false;
            string message = string.Empty;
            var courseRegistration = await _db.CourseRegistrations.FindAsync(id);
            if (courseRegistration != null)
            {
                studentId = courseRegistration.StudentId;
                _db.CourseRegistrations.Remove(courseRegistration);
                await _db.SaveChangesAsync();
                status = true;
                message = "Course removed Successfully...";
                return new JsonResult { Data = new { status, message, studentId } };
            }

            return new JsonResult { Data = new { status = false, message = $"{id } not deleted" } };
        }

        public async Task<ActionResult> DisplayRegisteredStudent(int id)
        {
            var data = new List<CourseRegDisplayVm>();
            var assignedCourse = await _db.AssignedCourses.Include(a => a.Course.SchoolProgramme).Include(a => a.Staff)
                                    .Include(i => i.Session).AsNoTracking()
                                    .Where(x => x.AssignedCourseId.Equals(id)).ToListAsync();
            var firstAssigned = assignedCourse.FirstOrDefault();

            var courseReg = _db.CourseRegistrations.Include(c => c.Course).Include(s => s.Students)
                                       .Include(c => c.Students.Programme.Department).Include(c => c.Students.Level)
                                       .Include(i => i.Students.Programme).Include(i => i.Students.SchoolProgramme).AsNoTracking()
                                       .Where(x => x.SessionId.Equals(firstAssigned.Session.SessionId)
                                        && x.CourseId.Equals(firstAssigned.CourseId)
                                        && x.Students.SchoolProgramme.SchoolProgrammeId.Equals(firstAssigned.Course.SchoolProgramme.SchoolProgrammeId))
                                       .OrderBy(o => o.StudentId).DistinctBy(d => d.SemesterId).ToList();
            int count = 1;
            foreach (var item in courseReg)
            {
                data.Add(new CourseRegDisplayVm
                {
                    No = count,
                    MatricNo = item.Students.MatricNo,
                    FullName = item.Students.FullName,
                    Gender = item.Students.Gender,
                    Email = item.Students.Email.Contains("unijos.edu.ng") ? item.Students.Email : item.Students.PrimaryEmail,
                    Dept = item.Students.Programme.Department.DeptName,
                    ProgrammeName = item.Students.Programme.ProgrammeName,
                    LevelName = item.Students.Level.LevelName,
                    PhoneNumber = item.Students.PhoneNumber
                });
                count++;

            }
            ViewBag.AssignedCourseId = id;
            return View(data);
        }

        public async Task<ActionResult> RegisteredStudent()
        {
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            ViewBag.SemesterId = new SelectList(await _db.Semesters.AsNoTracking().ToListAsync(), "SemesterId", "SemesterName");
            if (User.IsInRole(RoleName.Hod) || User.IsInRole(RoleName.LCordinator))
            {
                var staffDept = await _db.Staffs.Include(i => i.Department).AsNoTracking()
                    .Where(x => x.Email.Equals(userId))
                    .Select(s => s.Department.DepartmentId).FirstOrDefaultAsync();
                ViewBag.DepartmentId = new SelectList(await _db.Departments.AsNoTracking()
                    .Where(x => x.DepartmentId.Equals(staffDept)).ToListAsync(), "DepartmentId", "DeptName");
            }
            else
            {
                ViewBag.DepartmentId = new SelectList(await _db.Departments.AsNoTracking().ToListAsync(), "DepartmentId", "DeptName");
            }
            ViewBag.SchoolProgrammeId = new SelectList(await _db.SchoolProgrammes.AsNoTracking().ToListAsync(), "SchoolProgrammeId", "FancyName");

            ViewBag.LevelId = new SelectList(await _db.Levels.AsNoTracking().ToListAsync(), "LevelId", "LevelName");
            ViewBag.FacultyId = new SelectList(await _db.Faculties.AsNoTracking().ToListAsync(), "FacultyId", "FacultyName");
            return View();
        }

        public async Task<ActionResult> UnRegisteredStudent()
        {
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            ViewBag.SemesterId = new SelectList(await _db.Semesters.AsNoTracking().ToListAsync(), "SemesterId", "SemesterName");
            if (Request.IsAuthenticated && User.IsInRole(RoleName.Hod) || User.IsInRole(RoleName.LCordinator))
            {
                var staffDept = await _db.Staffs.Include(i => i.Department).AsNoTracking()
                                    .Where(x => x.Email.Equals(userId)).Select(s => s.Department.DepartmentId)
                                    .FirstOrDefaultAsync();
                ViewBag.DepartmentId = new SelectList(await _db.Departments.AsNoTracking()
                    .Where(x => x.DepartmentId.Equals(staffDept)).ToListAsync(), "DepartmentId", "DeptName");
            }
            else
            {
                ViewBag.DepartmentId = new SelectList(await _db.Departments.AsNoTracking().ToListAsync(), "DepartmentId", "DeptName");
            }
            ViewBag.SchoolProgrammeId = new SelectList(await _db.SchoolProgrammes.AsNoTracking().ToListAsync(), "SchoolProgrammeId", "FancyName");

            ViewBag.LevelId = new SelectList(await _db.Levels.AsNoTracking().ToListAsync(), "LevelId", "LevelName");
            return View();
        }

        [HttpPost]
        public ActionResult GetRegisteredStudent(int SchoolProgrammeId, int? DepartmentId, int? LevelId, int? SessionId, int? FacultyId)
        {
            if (SessionId == null)
            {
                SessionId = _query.GetCurrentSessionId(SchoolProgrammeId);
            }
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

            var courseReg = _db.CourseRegistrations.Include(c => c.Course).Include(s => s.Students)
                                    .Include(c => c.Students.Programme.Department)
                                    .Include(c => c.Students.Level)
                                    .Include(c => c.Students.Programme.Department.Faculty)
                                    .Include(i => i.Programme).Include(i => i.Students.SchoolProgramme).AsNoTracking()
                                    .Where(x =>  x.SessionId.Equals((int)SessionId)
                                        && x.Students.SchoolProgramme.SchoolProgrammeId.Equals(SchoolProgrammeId))
                                    .OrderBy(o => o.Students.Programme.Department.DeptName)
                                    .DistinctBy(d => d.Students.MatricNo)
                                    .ToList();

            if (FacultyId != null)
            {
                courseReg = courseReg.Where(x => x.Students.Programme.Department.Faculty.FacultyId.Equals((int)FacultyId)
                                                 /*&& x.Students.LevelId.Equals((int)LevelId)*/).ToList();
            }

            //if (DepartmentId != null & LevelId != null)
            //{
            //    courseReg = courseReg.Where(x => x.Students.Programme.Department.DepartmentId.Equals((int)DepartmentId)
            //                                     && x.Students.LevelId.Equals((int)LevelId)).ToList();
            //}
            if (DepartmentId != null)
            {
                courseReg = courseReg.Where(x => x.Students.Programme.Department.DepartmentId.Equals((int)DepartmentId)).ToList();
            }

            if (LevelId != null)
            {
                courseReg = courseReg.Where(x => x.Students.Level.LevelId.Equals((int)LevelId)).ToList();
            }

            var record = courseReg.Select(s => new
            {
                s.Students.MatricNo,
                s.Students.FullName,
                s.Students.Gender,
                s.Students.Programme.Department.DeptName,
                s.Students.Programme.ProgrammeName,
                s.Students.Level.LevelName,
                s.Students.PhoneNumber
            }).ToList();

            totalRecords = record.Count();
            var data = record.Skip(skip).Take(pageSize).ToList();

            return Json(new { draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data },
                JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> GetUnRegisteredStudent(int SchoolProgrammeId,  int? DepartmentId, int? LevelId, int? SessionId)
        {
            if (SessionId == null)
            {
                SessionId = _query.GetCurrentSessionId(SchoolProgrammeId);
            }
            if (DepartmentId == null)
            {
                var deptId = await _db.Staffs.AsNoTracking().Where(x => x.Email.Equals(userId))
                                    .Select(s => s.DepartmentId).FirstOrDefaultAsync();
                DepartmentId = deptId;
            }

            var defaulterList = new List<Student>();
            var studentsList = await _db.Students.Include(i => i.Level).Include(i => i.Programme.Department)
                                                    .AsNoTracking().Where(x => x.Active.Equals(true)
                                                    && x.IsGraduated.Equals(false)).ToListAsync();

            var courseReg = _db.CourseRegistrations.Include(c => c.Course).Include(s => s.Students)
                            .Include(c => c.Students.Programme.Department).Include(c => c.Students.Level)
                            .Include(i => i.Students.Programme)
                            .Include(i => i.Students.SchoolProgrammeId.Equals(SchoolProgrammeId)).AsNoTracking()
                            .Where(x => x.SessionId.Equals((int)SessionId) 
                            && x.IsApproved.Equals(true)).AsEnumerable()
                            .DistinctBy(x => x.StudentId).ToList();

            if (DepartmentId != null & LevelId != null)
            {
                courseReg = courseReg.Where(x => x.Students.Programme.Department.DepartmentId.Equals((int)DepartmentId)
                                                 && x.Students.Level.LevelId.Equals((int)LevelId)
                                                 && x.SessionId.Equals((int)SessionId)).ToList();
            }
            else if (DepartmentId != null)
            {
                courseReg = courseReg.Where(x => x.Students.Programme.Department.DepartmentId.Equals((int)DepartmentId)).ToList();
            }
            else if (LevelId != null)
            {
                courseReg = courseReg.Where(x => x.Students.Level.LevelId.Equals((int)LevelId)).ToList();
            }
            foreach (var student in studentsList)
            {
                var defaulter = courseReg.FirstOrDefault(x => x.StudentId.Equals(student.StudentId));
                if (defaulter == null)
                {
                    defaulterList.Add(student);
                }
            }

            var data = defaulterList.Select(s => new
            {
                s.MatricNo,
                s.FullName,
                s.Gender,
                s.Programme.Department.DeptName,
                s.Programme.ProgrammeName,
                s.Level.LevelName,
                s.PhoneNumber
            }).ToList();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        public PartialViewResult RejectCourseReg(int courseRegId)
        {
            var model = new RejectCourseRegVm()
            {
                CourseRegistrationId = courseRegId
            };
            return PartialView(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RejectCourseReg(RejectCourseRegVm model)
        {
            if (ModelState.IsValid)
            {
                var regHistory = _db.CourseRegistrations.AsNoTracking().Where(x => x.CourseRegistrationId.Equals(model.CourseRegistrationId))
                                    .FirstOrDefault();
                var courseReg = await _db.CourseRegistrations.Include(c => c.Course).Include(s => s.Students)
                                        .Include(c => c.Level).Include(c => c.Programme).Include(c => c.Semester)
                                        .Include(c => c.Session).AsNoTracking()
                                        .Where(x => x.StudentId.Equals(regHistory.StudentId) &&
                                        x.SessionId.Equals(regHistory.SessionId) && x.SemesterId.Equals(regHistory.SemesterId))
                                        .ToListAsync();
                if (courseReg.Count == 0)
                {
                    var msg1 = $"Course registration history not found";
                    return Json(new { status = false, message = msg1 }, JsonRequestBehavior.AllowGet);
                }

                foreach (var course in courseReg)
                {
                    var approveCourse = await _db.CourseRegistrations.FindAsync(course.CourseRegistrationId);
                    approveCourse.IsApproved = false;
                    approveCourse.IsStudentRegistered = false;
                    approveCourse.ReasonForReject = model.ReasonForReject;
                    _db.Entry(approveCourse).State = EntityState.Modified;
                }
                await _db.SaveChangesAsync();
                return new JsonResult { Data = new { status = true, message = "Course Registration has been rejected successfully" } };

            }
            return new JsonResult { Data = new { status = false, message = "Oops.. Something went Wrong" } };
        }

        public async Task DownloadStudentCourseReg(int id)
        {
            var data = new List<CourseRegDisplayVm>();
            var assignedCourse = await _db.AssignedCourses.Include(a => a.Course.SchoolProgramme).Include(a => a.Staff)
                                    .Include(i => i.Session).AsNoTracking()
                                    .Where(x => x.AssignedCourseId.Equals((int)id)).ToListAsync();
            var firstAssigned = assignedCourse.FirstOrDefault();

            var courseReg = _db.CourseRegistrations.Include(c => c.Course).Include(s => s.Students)
                                       .Include(c => c.Students.Programme.Department).Include(c => c.Students.Level)
                                       .Include(i => i.Students.Programme).Include(i => i.Students.SchoolProgramme).AsNoTracking()
                                       .Where(x => x.SessionId.Equals(firstAssigned.Session.SessionId)
                                        && x.CourseId.Equals(firstAssigned.CourseId)
                                        && x.Students.SchoolProgramme.SchoolProgrammeId.Equals(firstAssigned.Course.SchoolProgramme.SchoolProgrammeId))
                                       .OrderBy(o => o.StudentId).DistinctBy(d => d.SemesterId).ToList();
            int count = 1;
            foreach (var item in courseReg)
            {
                data.Add(new CourseRegDisplayVm
                {
                    No = count,
                    MatricNo = item.Students.MatricNo,
                    FullName = item.Students.FullName,
                    Gender = item.Students.Gender,
                    Email = item.Students.Email.Contains("unijos.edu.ng") ? item.Students.Email : item.Students.PrimaryEmail,
                    Dept = item.Students.Programme.Department.DeptName,
                    ProgrammeName = item.Students.Programme.ProgrammeName,
                    LevelName = item.Students.Level.LevelName,
                    PhoneNumber = item.Students.PhoneNumber
                });
                count++;

            }

            char c1 = 'A';
            ExcelPackage package = new ExcelPackage();
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Report");

            worksheet.Cells[$"{c1++}1"].Value = "No";
            worksheet.Cells[$"{c1++}1"].Value = "Matric No";
            worksheet.Cells[$"{c1++}1"].Value = "FullName";
            worksheet.Cells[$"{c1++}1"].Value = "Email";
            worksheet.Cells[$"{c1++}1"].Value = "Level";
            worksheet.Cells[$"{c1++}1"].Value = "Gender";
            worksheet.Cells[$"{c1++}1"].Value = "Programme";
            worksheet.Cells[$"{c1++}1"].Value = "Department";

            int rowStart = 2;
            for (var i = 0; i < data.Count; i++)
            {
                worksheet.Cells[$"A{rowStart}"].Value = data[i].No;
                worksheet.Cells[$"B{rowStart}"].Value = data[i].MatricNo;
                worksheet.Cells[$"C{rowStart}"].Value = data[i].FullName;
                worksheet.Cells[$"D{rowStart}"].Value = data[i].Email;
                worksheet.Cells[$"E{rowStart}"].Value = data[i].LevelName;
                worksheet.Cells[$"F{rowStart}"].Value = data[i].Gender;
                worksheet.Cells[$"G{rowStart}"].Value = data[i].ProgrammeName;
                worksheet.Cells[$"G{rowStart}"].Value = data[i].Dept;
                rowStart++;
            }

            worksheet.Cells["A:AZ"].AutoFitColumns();
            worksheet.Column(1).Style.Locked = true;
            worksheet.Column(2).Style.Locked = true;
            worksheet.Column(3).Style.Locked = true;
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment: filename=" +
                                                      $"CourseRegistration.xlsx");
            Response.BinaryWrite(package.GetAsByteArray());
            Response.End();
            //return View();
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