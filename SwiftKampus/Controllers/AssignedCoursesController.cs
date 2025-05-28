using SwiftKampus.BusinessLogic;
using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampus.ViewModels;
using SwiftKampusModel;
using SwiftKampusModel.Attendance;
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
    public class AssignedCoursesController : BaseController
    {
        private CourseRegQueryManager _courseQuery;
        private StaffQueryManager _staffQuery;
        public AssignedCoursesController(SchoolDbContext db) : base(db)
        {
            _courseQuery = new CourseRegQueryManager(_db);
            _staffQuery = new StaffQueryManager(_db);

        }

        // GET: AssignedCourses
        public ActionResult Index()
        {
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FancyName");
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");

            ViewBag.ProgrammeId = new SelectList(_db.Programmes.AsNoTracking(), "ProgrammeId", "ProgrammeName");
            ViewBag.CourseId = new SelectList(_db.Courses.AsNoTracking(), "CourseId", "CourseCode");
            return View();
        }
        public ActionResult LecturerIndex()
        {
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FancyName");
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            return View();
        }
        public async Task<ActionResult> GetIndex(int? SchoolProgrammeId, int? SessionId)
        {
            
            if (SchoolProgrammeId != null)
            {
               sessionId = _query.GetCurrentSessionId((int)SchoolProgrammeId);
                //semesterId = _query.GetCurrentSemesterId((int)SchoolProgrammeId);
            }
            if (SessionId != null)
            {
                sessionId = (int)SessionId;
            }

            // dc.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key

            var assignedCourse = await _db.AssignedCourses.Include(a => a.Course).Include(a => a.Staff)
                                .Include(i => i.Session).Include(i => i.Semester).AsNoTracking()
                                .Where(x => x.Session.SessionId.Equals(sessionId)).ToListAsync();

            if (User.IsInRole(RoleName.Academic) && User.IsInRole(RoleName.Hod))
            {
                var HODDeptId = await _db.Staffs.Where(x => x.Email.Equals(userId)).FirstOrDefaultAsync();
                assignedCourse = await _db.AssignedCourses.Include(a => a.Course).Include(a => a.Staff)
                                .Include(i => i.Session).Include(i => i.Semester)
                                .Include(i => i.Staff.Department).AsNoTracking()
                                .Where(x => x.Session.SessionId.Equals(sessionId)
                                && x.Staff.Department.DepartmentId.Equals((int)HODDeptId.DepartmentId)).ToListAsync();
                //assignedCourse = assignedCourse.Where(x => x.Staff.Email.Equals(userId)).ToList();
            }
            if (User.IsInRole(RoleName.Academic) && !User.IsInRole(RoleName.Hod))
            {
                assignedCourse = assignedCourse.Where(x => x.Staff.Email.Equals(userId)).ToList();
            }


            var data = assignedCourse.Select(s => new
            {
                s.Course.CourseName,
                s.Staff.FullName,
                s.Session.SessionName,
                s.Semester.SemesterName,
                s.AssignedCourseId
            }).ToList();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        public async Task<PartialViewResult> Save(int id)
        {
            if (User.IsInRole(RoleName.Academic))
            {
                var staffDeptId = await _staffQuery.GetStaffDepartmentId(userId);
                ViewBag.CourseId = new MultiSelectList(await _courseQuery.GetStaffDepartmentCourse(staffDeptId), "CourseId", "CourseName");
                ViewBag.StaffId = new SelectList(await _db.Staffs.AsNoTracking().Include(i => i.Department)
                                                .Where(x => x.Department.DepartmentId.Equals(staffDeptId))
                                                .ToListAsync(), "StaffId", "FullName");

            }
            else
            {
                ViewBag.CourseId = new MultiSelectList(_db.Courses.AsNoTracking(), "CourseId", "CourseName");
                ViewBag.StaffId = new SelectList(_db.Staffs.AsNoTracking(), "StaffId", "FullName");
            }
            ViewBag.SemesterId = new SelectList(_db.Semesters.AsNoTracking(), "SemesterId", "SemesterName");
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            var assignedCourse = await _db.AssignedCourses.FindAsync(id);
            if (assignedCourse != null)
            {
                var model = new AssignedCourseVm
                {
                    AssignedCourseId = assignedCourse.AssignedCourseId
                };
                return PartialView(model);
            }

            return PartialView();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Save(AssignedCourseVm model)
        {

            if (ModelState.IsValid)
            {
                if (model.AssignedCourseId > 0)
                {
                    var assignedCourse = await _db.AssignedCourses.FindAsync(model.AssignedCourseId);
                    var courseId = model.CourseId[0];
                    var courseDetail = await _db.Courses.FindAsync(courseId);

                    if (assignedCourse != null)
                    {
                        assignedCourse.CourseId = model.CourseId[0];
                        assignedCourse.StaffId = model.StaffId;
                        assignedCourse.SemesterId = courseDetail.SemesterId;
                        assignedCourse.SessionId = model.SessionId;

                        _db.Entry(assignedCourse).State = EntityState.Modified;
                        await _db.SaveChangesAsync();
                        var message = "Course Assigned Updated Successfully...";
                        return new JsonResult { Data = new { status = true, message } };
                    }
                }
                else
                {
                    foreach (var course in model.CourseId)
                    {
                        var courseDetail = await _db.Courses.FindAsync(course);
                        var assignedCourse = new AssignedCourse
                        {
                            CourseId = course,
                            StaffId = model.StaffId,
                            SemesterId = courseDetail.SemesterId,
                            SessionId = model.SessionId
                        };
                        _db.AssignedCourses.Add(assignedCourse);
                    }
                    await _db.SaveChangesAsync();
                    var message = $"Courses Added Successfully.";
                    return new JsonResult { Data = new { status = true, message } };
                }
            }
            return new JsonResult { Data = new { status = false, message = "Please fill all required field" } };
            //return View(subject);
        }
        // GET: Grades/Details/5

        public async Task<ActionResult> ElectiveLecturerGet(int codeId)
        {
            var utmeUploadError = new List<StaffAssigendCourseVm>();
            sessionId = _query.GetCurrentSessionId(1);
            var staffDeptId = await _staffQuery.GetStaffDepartmentId(userId);

            var assignedCourse = await _db.AssignedCourses.Include(a => a.Course).Include(a => a.Staff)
                                .Include(i => i.Session).Include(i => i.Semester).AsNoTracking()
                                .Where(x => x.Session.SessionId.Equals(sessionId) && x.CourseId.Equals(codeId)).FirstOrDefaultAsync();

            ViewBag.ElectiveCourseId = new SelectList(await _courseQuery.GetStaffDepartmentCourse(staffDeptId), "CourseId", "CourseName");

            if (assignedCourse == null)
            {
                var message = $"Course has NOT been assisned at parent Dept!.";
                return new JsonResult { Data = new { status = true, message } };
            }

            utmeUploadError.Add(new StaffAssigendCourseVm { StaffName = assignedCourse.Staff.FullName, StaffId = assignedCourse.StaffId, Row = 1 });

            return View("ElectiveLecturerGet", utmeUploadError);
        }

        public async Task<ActionResult> AssignElectiveCourse(int courseId, string staffId)
        {

            var courseDetail = await _db.Courses.FindAsync(courseId);
            var assignedCourse = new AssignedCourse
            {
                CourseId = courseId,
                StaffId = staffId,
                SemesterId = courseDetail.SemesterId,
                SessionId = _query.GetCurrentSessionId(1)
            };
            _db.AssignedCourses.Add(assignedCourse);

            await _db.SaveChangesAsync();
            var message = $"Courses Added Successfully.";
            return new JsonResult { Data = new { status = true, message } };
        }

        // GET: AssignedCourses/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AssignedCourse assignedCourse = await _db.AssignedCourses.FindAsync(id);
            if (assignedCourse == null)
            {
                return HttpNotFound();
            }
            return View(assignedCourse);
        }

        // GET: AssignedCourses/Create
        public async Task<ActionResult> Create()
        {
            if (User.IsInRole(RoleName.Academic))
            {
                var staffDeptId = await _staffQuery.GetStaffDepartmentId(userId);
                ViewBag.CourseId = new MultiSelectList(await _courseQuery.GetStaffDepartmentCourse(staffDeptId), "CourseId", "CourseName");
                ViewBag.StaffId = new SelectList(await _db.Staffs.AsNoTracking().Include(i => i.Department)
                                                .Where(x => x.Department.DepartmentId.Equals(staffDeptId))
                                                .ToListAsync(), "StaffId", "FullName");

            }
            else
            {
                ViewBag.CourseId = new MultiSelectList(_db.Courses.AsNoTracking(), "CourseId", "CourseName");
                ViewBag.StaffId = new SelectList(_db.Staffs.AsNoTracking(), "StaffId", "FullName");
            }
            ViewBag.SemesterId = new SelectList(_db.Semesters.AsNoTracking(), "SemesterId", "SemesterName");
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");

            return View();
        }

        // POST: AssignedCourses/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(AssignedCourseVm model)
        {
            if (ModelState.IsValid)
            {
                if (model.AssignedCourseId > 0)
                {
                    int courseId = model.CourseId[0];
                    var course = await _db.Courses.FindAsync(courseId);
                    var assignedCourse = await _db.AssignedCourses.FindAsync(model.AssignedCourseId);
                    if (assignedCourse != null)
                    {
                        assignedCourse.CourseId = model.CourseId[0];
                        assignedCourse.StaffId = model.StaffId;
                        assignedCourse.SemesterId = course.SemesterId;
                        assignedCourse.SessionId = model.SessionId;

                        _db.Entry(assignedCourse).State = EntityState.Modified;
                        await _db.SaveChangesAsync();
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    foreach (var courseId in model.CourseId)
                    {
                        var check = _db.AssignedCourses.Include(i => i.Session).AsNoTracking().Any(x => x.CourseId.Equals(courseId) &&
                                        x.Session.SessionId.Equals(model.SessionId) && x.StaffId.Equals(model.StaffId));
                                        
                        if (!check)
                        {                            
                            var course = await _db.Courses.FindAsync(courseId);
                            var assignedCourse = new AssignedCourse
                            {
                                CourseId = courseId,
                                StaffId = model.StaffId,
                                SemesterId = course.SemesterId,
                                SessionId = model.SessionId
                            };
                            _db.AssignedCourses.Add(assignedCourse);
                        }                     
                       
                    }
                    await _db.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
            }
            if (User.IsInRole(RoleName.Academic))
            {
                var staffDeptId = await _staffQuery.GetStaffDepartmentId(userId);
                ViewBag.CourseId = new MultiSelectList(await _courseQuery.GetStaffDepartmentCourse(staffDeptId), "CourseId", "CourseName");
                ViewBag.StaffId = new SelectList(await _db.Staffs.AsNoTracking().Include(i => i.Department)
                                                .Where(x => x.Department.DepartmentId.Equals(staffDeptId))
                                                .ToListAsync(), "StaffId", "FullName");

            }
            else
            {
                ViewBag.CourseId = new MultiSelectList(_db.Courses.AsNoTracking(), "CourseId", "CourseName");
                ViewBag.StaffId = new SelectList(_db.Staffs.AsNoTracking(), "StaffId", "FullName");
            }
            ViewBag.SemesterId = new SelectList(_db.Semesters.AsNoTracking(), "SemesterId", "SemesterName");
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            return View(model);
        }

        // GET: AssignedCourses/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AssignedCourse assignedCourse = await _db.AssignedCourses.FindAsync(id);
            if (assignedCourse == null)
            {
                return HttpNotFound();
            }
            if (User.IsInRole(RoleName.Academic))
            {
                var staffDeptId = await _staffQuery.GetStaffDepartmentId(userId);
                ViewBag.CourseId = new MultiSelectList(await _courseQuery.GetStaffDepartmentCourse(staffDeptId), "CourseId", "CourseName");
                ViewBag.StaffId = new SelectList(await _db.Staffs.AsNoTracking().Include(i => i.Department)
                                                .Where(x => x.Department.DepartmentId.Equals(staffDeptId))
                                                .ToListAsync(), "StaffId", "FullName");
            }
            else
            {
                ViewBag.CourseId = new MultiSelectList(_db.Courses.AsNoTracking(), "CourseId", "CourseName");
                ViewBag.StaffId = new SelectList(_db.Staffs.AsNoTracking(), "StaffId", "FullName");
            }
            ViewBag.SemesterId = new SelectList(_db.Semesters.AsNoTracking(), "SemesterId", "SemesterName");
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            return View(assignedCourse);
        }

        // POST: AssignedCourses/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(AssignedCourse assignedCourse)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(assignedCourse).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            if (User.IsInRole(RoleName.Academic))
            {
                var staffDeptId = await _staffQuery.GetStaffDepartmentId(userId);
                ViewBag.CourseId = new MultiSelectList(await _courseQuery.GetStaffDepartmentCourse(staffDeptId), "CourseId", "CourseName");
                ViewBag.StaffId = new SelectList(await _db.Staffs.AsNoTracking().Include(i => i.Department)
                                                .Where(x => x.Department.DepartmentId.Equals(staffDeptId))
                                                .ToListAsync(), "StaffId", "FullName");
            }
            else
            {
                ViewBag.CourseId = new MultiSelectList(_db.Courses.AsNoTracking(), "CourseId", "CourseName");
                ViewBag.StaffId = new SelectList(_db.Staffs.AsNoTracking(), "StaffId", "FullName");
            }
            ViewBag.SemesterId = new SelectList(_db.Semesters.AsNoTracking(), "SemesterId", "SemesterName");
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            return View(assignedCourse);
        }

        // GET: AssignedCourses/Delete/5
        public async Task<PartialViewResult> Delete(int id)
        {
            var assignedCourse = await _db.AssignedCourses.FindAsync(id);
            return PartialView(assignedCourse);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            bool status = false;
            string message = string.Empty;
            var assignedCourse = await _db.AssignedCourses.FindAsync(id);
            if (assignedCourse != null)
            {
                _db.AssignedCourses.Remove(assignedCourse);
                await _db.SaveChangesAsync();
                status = true;
                message = "Assigned Course Deleted Successfully...";
                return new JsonResult { Data = new { status, message } };
            }

            return new JsonResult { Data = new { status = false, message = $"{id } not deleted" } };
        }


        public PartialViewResult PickAttendance(int id, int SchoolProgrammeId)
        {
            var model = new StudentAttendanceVm()
            {
                CourseId = id,
                SchoolProgrammeId = SchoolProgrammeId
            };
            return PartialView(model);
        }

        [HttpPost]
        public async Task<ActionResult> PickAttendance(StudentAttendanceVm model)
        {
            sessionId = _query.GetCurrentSessionId(model.SchoolProgrammeId);
            semesterId = _query.GetCurrentSemesterId(model.SchoolProgrammeId);
            var assignedCourse = await _db.AssignedCourses.FindAsync(model.CourseId);
            var checkAttendace = await _db.StudentAttendances.AsNoTracking()
                                .Where(x => x.CourseId.Equals(assignedCourse.CourseId)
                                && x.AttendanceDate.Year.Equals(model.AttendanceDate.Year)
                                && x.AttendanceDate.Month.Equals(model.AttendanceDate.Month)
                                && x.AttendanceDate.Day.Equals(model.AttendanceDate.Day)).FirstOrDefaultAsync();
            if (checkAttendace != null)
            {
                return new JsonResult { Data = new { status = true, id = assignedCourse.CourseId, date = model.AttendanceDate.ToString() } };

            }
            var regStudent = await _db.CourseRegistrations.Include(i => i.Course).Include(i => i.Students).Include(i => i.Level)
                .Include(i => i.Programme).AsNoTracking()
                .Where(x => x.SemesterId.Equals(semesterId) && x.SessionId.Equals(sessionId)
                            && x.CourseId.Equals(assignedCourse.CourseId)).ToListAsync();
            foreach (var student in regStudent)
            {
                var studentAttendance = new StudentAttendance()
                {
                    StaffId = assignedCourse.StaffId,
                    StudentId = student.StudentId,
                    CourseId = student.CourseId,
                    IsPresent = model.MarkAttendanceForAll,
                    SemesterId = semesterId,
                    SessionId = sessionId,
                    AttendanceDate = model.AttendanceDate,
                    SchoolProgrammeId = model.SchoolProgrammeId
                };
                _db.StudentAttendances.Add(studentAttendance);
            }
            await _db.SaveChangesAsync();

            //ViewBag.Id = model.CourseId;
            //ViewBag.SchoolProgrammeId = model.SchoolProgrammeId;
            //return View(model);
            return new JsonResult { Data = new { status = true, id = assignedCourse.CourseId, date = model.AttendanceDate.ToString() } };
        }


        public ActionResult RegisteredStudent(int id, string attendanceDate)
        {
            ViewBag.Id = id;
            ViewBag.AttendanceDate = attendanceDate;
            return View();
        }

        public async Task<ActionResult> GetRegisteredStudent(int id, string attendanceDate)
        {
            var myDate = Convert.ToDateTime(attendanceDate);
            var regStudent = await _db.StudentAttendances.Include(i => i.Student.Programme)
                            .Include(i => i.Student.Level).Include(i => i.Student)
                            .Include(i => i.Course).AsNoTracking()
                            .Where(x => x.CourseId.Equals(id) &&
                             x.AttendanceDate.Year.Equals(myDate.Year) &&
                             x.AttendanceDate.Month.Equals(myDate.Month) &&
                             x.AttendanceDate.Day.Equals(myDate.Day)
                            /*&& x.AttendanceDate.Equals(attendanceDate)*/)
                            .ToListAsync();

            ViewBag.StudentCount = regStudent.Count;
            var data = regStudent.Select(s => new
            {
                s.StudentAttendanceId,
                s.Course.CourseName,
                s.Student.Programme.ProgrammeName,
                s.Student.Level.LevelName,
                s.Student.StudentId,
                s.Student.MatricNo,
                s.Student.FullName,
                AttendanceDate = s.AttendanceDate.ToString(),
                s.IsPresent
            }).ToList();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<ActionResult> ChangeAttendance(int id)
        {
            var studentAttendance = await _db.StudentAttendances.FindAsync(id);
            if (studentAttendance.IsPresent.Equals(true))
            {
                studentAttendance.IsPresent = false;
            }
            else if (studentAttendance.IsPresent.Equals(false))
            {
                studentAttendance.IsPresent = true;
            }
            _db.Entry(studentAttendance).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return new JsonResult { Data = new { status = true, message = $"This student has been marked as {studentAttendance.IsPresent}" } };

        }

        public PartialViewResult AssociateLecturer(int id)
        {
            ViewBag.Id = id;
            return PartialView();
        }

        public async Task<ActionResult> GetAssociateLecturer(int id)
        {
            var assignedCourse = await _db.AssignedCourses.FindAsync(id);

            var associateStaff = await _db.AssignedCourses.Include(i => i.Course).Include(i => i.Staff)
                            .Include(i => i.Staff.Department).AsNoTracking()
                            .Where(x => x.Semester.SemesterId.Equals(semesterId) && x.Session.SessionId.Equals(sessionId)
                            && x.CourseId.Equals(assignedCourse.CourseId)).ToListAsync();
            ViewBag.StaffCount = associateStaff.Count;
            var data = associateStaff.Select(s => new
            {
                s.StaffId,
                s.Course.CourseName,
                s.Staff.Department.DeptName,
                s.Staff.FullName,
                s.Staff.PhoneNumber

            }).ToList();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        public PartialViewResult TimeTable()
        {
            return PartialView();
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
