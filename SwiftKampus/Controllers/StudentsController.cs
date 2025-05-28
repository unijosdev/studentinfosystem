using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Microsoft.Ajax.Utilities;
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
using SwiftKampus.ViewModels.StudentBioData;
using SwiftKampus.ViewModels.StudentStatusMgtVm;
using SwiftKampusModel;
using static SwiftKampus.Controllers.AssignedRoomsController;

namespace SwiftKampus.Controllers
{
    [Authorize]
    [Audit(AuditingLevel = 2)]
    public class StudentsController : BaseController
    {
        IFeeQueryManager _feeQueryManager;
        ICourseRegQueryManager _courseRegQuery;
        IQueryCommand _currentSession;
        private List<Session> allSessions;

        public StudentsController(SchoolDbContext db) : base(db)
        {
            _courseRegQuery = new CourseRegQueryManager(_db);
            _feeQueryManager = new FeeQueryManager(_db);
            _currentSession = new QueryCommand(_db);
            allSessions = GetAllSession();
        }

        public async Task<ActionResult> DisplayEmailSearch()
        {
            var matricNo = "PGA1813166405";
            try
            {
                var students = await _db.Students.AsNoTracking()
                                        .Where(x => x.JambRegNo.Trim().ToUpper().Equals(matricNo.ToUpper()))
                                        .ToListAsync();
                foreach (var student in students)
                {
                    var checkStudent = await _db.SchoolFeePayments.FirstOrDefaultAsync(x => x.StudentId.Equals(student.StudentId)
                                            && x.Status.Equals(true));
                    if (checkStudent != null)
                    {
                        student.Active = true;
                        _db.Entry(student).State = EntityState.Modified;
                    }
                    else
                    {
                        _db.Entry(student).State = EntityState.Deleted;
                    }
                }
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                ViewBag.ErrorInfo = "Please Leave no column or row Empty/Blank";
                ViewBag.ErrorMessage = ex.Message;
                return View("ErrorException");
            }
            return View();
        }

        public ActionResult ProcessBioData()
        {
            var student = _studentQuery.GetStudent(userId);


            if (student.SchoolProgramme.ProgrammeCategory.Trim().ToUpper().Equals(ProgrammeCategory.Institute_Of_Education.ToString().ToUpper())
                && student.IsClearedDepartment)
            {
                return RedirectToAction("Details");
            }
            else if (student.StudentStatus.Equals(StudentStatus.Returning.ToString()) && student.IsClearedAll.Equals(true))
            {
                return RedirectToAction("Details");
            }
            else
            {
                var result = ConfirmSchoolFeeAndAcceptance();
                if (result != null)
                {
                    return result;
                }
                else
                {
                    if (student.IsRemedialStudent != null && student.IsRemedialStudent.Equals(true))
                    {
                        var result2 = ConfirmApplicationFee();
                        if (result2 != null)
                            return result2;
                    }
                }
            }
            ViewBag.ModeOfEntry = student.ModeOfEntry;
            var model = _applicantType;
            return View(model);
        }

        public ActionResult Index()
        {
            var mystates = from State s in Enum.GetValues(typeof(State))
                           select new { ID = s, Name = s.ToString() };

            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FancyName");
            ViewBag.FacultyId = new SelectList(_db.Faculties.AsNoTracking(), "FacultyId", "FacultyName");
            ViewBag.DepartmentId = new SelectList(_db.Departments.AsNoTracking(), "DepartmentId", "DeptName");
            ViewBag.ProgrammeId = new SelectList(_db.Programmes.AsNoTracking(), "ProgrammeId", "ProgrammeName");
            ViewBag.LevelId = new SelectList(_db.Levels.AsNoTracking(), "LevelId", "LevelName");
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            ViewBag.StateOfOrigin = new SelectList(mystates, "Name", "Name");
            if (User.IsInRole("Student"))
            {
                return RedirectToAction("Login", "Account");
            }
            return View();
        }

        public ActionResult ScholarshipIndex()
        {
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FancyName");
            ViewBag.FacultyId = new SelectList(_db.Faculties.AsNoTracking(), "FacultyId", "FacultyName");
            ViewBag.DepartmentId = new SelectList(_db.Departments.AsNoTracking(), "DepartmentId", "DeptName");
            ViewBag.ProgrammeId = new SelectList(_db.Programmes.AsNoTracking(), "ProgrammeId", "ProgrammeName");
            ViewBag.LevelId = new SelectList(_db.Levels.AsNoTracking(), "LevelId", "LevelName");
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            if (User.IsInRole("Student"))
            {
                return RedirectToAction("Login", "Account");
            }
            return View();
        }

        public async Task<PartialViewResult> MakeSchoolarshipStudent(string id)
        {
            var student = await _db.Students.FindAsync(id);
            return PartialView(student);
        }

        [HttpPost]
        public async Task<ActionResult> MakeSchoolarshipStudent(string StudentId, string Schoolarship)
        {
            bool status = false;
            string message = string.Empty;
            if (!StudentId.IsNullOrWhiteSpace() && !Schoolarship.IsNullOrWhiteSpace())
            {
                bool IsSchoolarshipStudent = Schoolarship.ToLower().Equals("true") ? true : false;
                string scholarshipText = IsSchoolarshipStudent ? "Activated" : "De-Activated";
                var student = await _db.Students.FindAsync(StudentId);
                if (student != null)
                {
                    student.IsSchoolarshipStudent = IsSchoolarshipStudent;

                    _db.Entry(student).State = EntityState.Modified;
                    await _db.SaveChangesAsync();
                    message = $"{student.FullName} Scholarship has been {scholarshipText} Successfully...";
                    return new JsonResult { Data = new { status = true, message } };
                }
            }
            return new JsonResult { Data = new { status, message = $"Ooops... are all Required" } };
        }

        //[AllowAnonymous]
        //public async Task<ActionResult> MigrateRemedial2018()
        //{

        //    var students = await _db.Students.Include(i => i.Session).Include(i => i.SchoolProgramme).Include(i => i.Programme)
        //                    .Where(x => x.SchoolProgramme.ProgrammeCategory.Equals(ProgrammeCategory.Remedial_Science.ToString())
        //                    && x.Session.SessionName.Equals("2018/2019")).ToListAsync();

        //    foreach (var student in students)
        //    {
        //        //await NotifyByEmail(student.Email, student.LastName, student.FirstName, student.Programme.ProgrammeName, student.Email,
        //        //         student.MatricNo, student.Session.SessionName, student.SchoolProgramme.FullName);
        //        var sessionId = _db.Sessions.AsNoTracking().Where(x => x.SessionName.Equals("2019/2020"))
        //                    .Select(s => s.SessionId).FirstOrDefault();

        //        student.SessionId = sessionId;
        //        _db.Entry(student).State = EntityState.Modified;

        //    }
        //    await _db.SaveChangesAsync();

        //    ViewBag.Message = students.Count();
        //    return View();
        //}

        public ActionResult GeneratedMatricNoIndex()
        {
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FancyName");
            ViewBag.FacultyId = new SelectList(_db.Faculties.AsNoTracking(), "FacultyId", "FacultyName");
            ViewBag.DepartmentId = new SelectList(_db.Departments.AsNoTracking(), "DepartmentId", "DeptName");
            ViewBag.ProgrammeId = new SelectList(_db.Programmes.AsNoTracking(), "ProgrammeId", "ProgrammeName");
            ViewBag.LevelId = new SelectList(_db.Levels.AsNoTracking(), "LevelId", "LevelName");
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            if (User.IsInRole("Student"))
            {
                return RedirectToAction("Login", "Account");
            }
            return View();
        }

        public ActionResult StudntIndexForLibraryUpload()
        {
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FancyName");
            ViewBag.FacultyId = new SelectList(_db.Faculties.AsNoTracking(), "FacultyId", "FacultyName");
            ViewBag.DepartmentId = new SelectList(_db.Departments.AsNoTracking(), "DepartmentId", "DeptName");
            ViewBag.ProgrammeId = new SelectList(_db.Programmes.AsNoTracking(), "ProgrammeId", "ProgrammeName");
            ViewBag.LevelId = new SelectList(_db.Levels.AsNoTracking(), "LevelId", "LevelName");
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            if (User.IsInRole("Student"))
            {
                return RedirectToAction("Login", "Account");
            }
            return View();
        }


        public async Task<ActionResult> DuplicateIndex()
        {
            //var model = new List<DuplicateStudentVm>();

            //var students = _db.Students.Include(i => i.SchoolProgramme).AsNoTracking()
            //                .Where(x => x.StudentStatus.Trim().ToUpper()
            //                 .Equals(StudentStatus.New_Student.ToString().Trim().ToUpper())
            //                 && x.SchoolProgramme.SchoolProgrammeCode != "UG"
            //                 && x.Active.Equals(true)).ToList();

            //foreach (var student in students)
            //{
            //    var studentCount = _db.Students.Count(x => x.Email.Trim().ToUpper().Equals(student.Email.Trim().ToUpper()));
            //    if (studentCount > 1 && model.Count(x => x.MatricNo.Equals(student.JambRegNo)) <= 0)
            //    {
            //        model.Add(new DuplicateStudentVm
            //        {
            //            FullName = student.FullName,
            //            MatricNo = student.JambRegNo,
            //            Count = studentCount
            //        });
            //        //students.RemoveAll(x => x.Email.Trim().ToUpper().Equals(student.Email.Trim().ToUpper()));
            //    }
            //}
            //return View(model);
            var lastrecord = "";
            var matricNo = "PGA185306493";
            try
            {
                var students = await _db.Students.AsNoTracking()
                                        .Where(x => x.JambRegNo.Trim().ToUpper().Equals(matricNo.ToUpper()))
                                        .ToListAsync();
                foreach (var student in students)
                {
                    var checkStudent = await _db.SchoolFeePayments.FirstOrDefaultAsync(x => x.StudentId.Equals(student.StudentId));
                    if (checkStudent != null)
                    {
                        student.Active = true;
                        _db.Entry(student).State = EntityState.Modified;
                        lastrecord = $"The last Updated record has the Last Name {student.LastName} and First Name {student.FirstName} with Student Id {student.StudentId}";
                    }
                    else
                    {
                        // _db.Students.Remove(student);
                        _db.Entry(student).State = EntityState.Deleted;
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorInfo = "Please Leave no column or row Empty/Blank";
                ViewBag.ErrorMessage = ex.Message;
                return View("ErrorException");
            }
            await _db.SaveChangesAsync();
            lastrecord = $"You have successfully Uploaded records...  and {lastrecord}";
            ViewBag.Message = lastrecord;
            return View();
        }



        public ActionResult StudentList()
        {
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FancyName");

            var staffRecord = _db.Staffs.Include(i => i.Department)
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
            ViewBag.LevelId = new SelectList(_db.Levels.AsNoTracking(), "LevelId", "LevelName");
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            return View();
        }

        public ActionResult StudentListBloodGroup()
        {
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FancyName");

            var staffRecord = _db.Staffs.Include(i => i.Department)
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
            ViewBag.LevelId = new SelectList(_db.Levels.AsNoTracking(), "LevelId", "LevelName");
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            return View();
        }

        public async Task<ActionResult> GetAllStudentsForBloodGroupUpload(int? SchoolProgrammeId, string hasRegistered, int? FacultyId,
                                     int? DepartmentId, int? ProgrammeId, int? SessionId, int? LevelId)
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

            var staffRecord = _db.Staffs.Include(i => i.Department.Faculty)
                              .FirstOrDefault(x => x.Email.ToUpper().Trim().Equals(userId.Trim().ToUpper()));
            if (User.IsInRole(RoleName.Dean))
            {
                FacultyId = staffRecord?.Department.FacultyId;
            }
            if (User.IsInRole(RoleName.Hod))
            {
                DepartmentId = staffRecord?.Department.DepartmentId;
            }

            var studentIndex = new List<StudentIndexVM>();

            string ClearanceStage = "";
            var studentList = await _studentQuery.GetStudentList(SchoolProgrammeId, true, FacultyId, DepartmentId, ProgrammeId, ClearanceStage, SessionId, LevelId);

            studentList = studentList.Where(s => string.IsNullOrEmpty(s.BloodGroup)).ToList();

            if (!string.IsNullOrEmpty(search))
            {
                studentIndex = studentList.Where(x => x.FullName.ToUpper().Contains(search.ToUpper().Trim())
                            || x.MatricNo.Trim().ToUpper().Contains(search.ToUpper().Trim())).ToList();
                if (studentIndex.Count() == 0)
                {
                    studentIndex = studentList.Where(x => !string.IsNullOrEmpty(x.Email) &&
                            x.Email.ToUpper().Contains(search.ToUpper().Trim())).ToList();
                }
            }
            else
            {
                studentIndex = studentList;
            }

            totalRecords = studentIndex.Count();
            var data = studentIndex.Skip(skip).Take(pageSize).ToList();

            return Json(new { draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data },
                JsonRequestBehavior.AllowGet);

            #endregion Server Side filtering
        }
        public ActionResult DepartmentClearance()
        {
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FancyName");
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            ViewBag.CurrentUGSession = _currentSession.GetCurrentSessionName(1);
            ViewBag.CurrentPGSession = _currentSession.GetCurrentSessionName(2);
            return View();
        }

        public ActionResult FacultyClearance()
        {
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FancyName");
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            ViewBag.CurrentUGSession = _currentSession.GetCurrentSessionName(1);
            ViewBag.CurrentPGSession = _currentSession.GetCurrentSessionName(2);
            return View();
        }
        public ActionResult AcademicClearance()
        {
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FancyName");
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            ViewBag.CurrentUGSession = _currentSession.GetCurrentSessionName(1);
            ViewBag.CurrentPGSession = _currentSession.GetCurrentSessionName(2);
            return View();
        }

        public ActionResult AllClearanceReport()
        {
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FancyName");
            ViewBag.FacultyId = new SelectList(_db.Faculties.AsNoTracking(), "FacultyId", "FacultyName");
            ViewBag.DepartmentId = new SelectList(_db.Departments.AsNoTracking(), "DepartmentId", "DeptName");
            ViewBag.ProgrammeId = new SelectList(_db.Programmes.AsNoTracking(), "ProgrammeId", "ProgrammeName");
            return View();
        }
        public ActionResult StudentInfo()
        {
            return View();
        }


        public async Task<ActionResult> GetIndex(int? SchoolProgrammeId, string hasRegistered, int? FacultyId, int? DepartmentId, int? ProgrammeId,
            string ClearanceStage, int? SessionId, int? LevelId, string gender, string StateOfOrigin)
        {

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

            var staffRecord = _db.Staffs.Include(i => i.Department.Faculty)
                              .FirstOrDefault(x => x.Email.ToUpper().Trim().Equals(userId.Trim().ToUpper()));
            if (User.IsInRole(RoleName.Dean))
            {
                FacultyId = staffRecord?.Department.FacultyId;
            }
            if (User.IsInRole(RoleName.Hod))
            {
                DepartmentId = staffRecord?.Department.DepartmentId;
            }
            var studentIndex = new List<StudentIndexVM>();
            bool activeStudent = false;
            if (!string.IsNullOrEmpty(hasRegistered) && hasRegistered.Equals("True"))
            {
                activeStudent = true;
            }

            var studentList = await _studentQuery.GetStudentList(SchoolProgrammeId, activeStudent, FacultyId, DepartmentId, ProgrammeId, ClearanceStage, SessionId, LevelId, gender, StateOfOrigin);

            if (!string.IsNullOrEmpty(search))
            {
                studentIndex = studentList.Where(x => x.FullName.ToUpper().Contains(search.ToUpper().Trim())
                            || x.MatricNo.Trim().ToUpper().Contains(search.ToUpper().Trim())).ToList();
                if (studentIndex.Count() == 0)
                {
                    studentIndex = studentList.Where(x => !string.IsNullOrEmpty(x.Email) &&
                            x.Email.ToUpper().Contains(search.ToUpper().Trim())).ToList();
                }
            }
            else
            {
                studentIndex = studentList;
            }

            totalRecords = studentIndex.Count();
            var data = studentIndex.Skip(skip).Take(pageSize).ToList();

            return Json(new { draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data },
                JsonRequestBehavior.AllowGet);

        }
        public async Task<ActionResult> GetMatIndex(int? SchoolProgrammeId, int? FacultyId, int? DepartmentId, int? ProgrammeId, int? SessionId, int? LevelId, string isDownloded)
        {

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

            var staffRecord = _db.Staffs.Include(i => i.Department.Faculty)
                              .FirstOrDefault(x => x.Email.ToUpper().Trim().Equals(userId.Trim().ToUpper()));
            if (User.IsInRole(RoleName.Dean))
            {
                FacultyId = staffRecord?.Department.FacultyId;
            }
            if (User.IsInRole(RoleName.Hod))
            {
                DepartmentId = staffRecord?.Department.DepartmentId;
            }
            var studentIndex = new List<StudentIndexVM>();

            var studentList = await _studentQuery.GetStudentListFor365EmailUpload(SchoolProgrammeId, FacultyId, DepartmentId, ProgrammeId, SessionId, LevelId, isDownloded);

            if (!string.IsNullOrEmpty(search))
            {
                studentIndex = studentList.Where(x => x.FullName.ToUpper().Contains(search.ToUpper().Trim())
                            || x.MatricNo.Trim().ToUpper().Contains(search.ToUpper().Trim())).ToList();
                if (studentIndex.Count() == 0)
                {
                    studentIndex = studentList.Where(x => !string.IsNullOrEmpty(x.Email) &&
                            x.Email.ToUpper().Contains(search.ToUpper().Trim())).ToList();
                }
            }

            else
            {
                studentIndex = studentList.Where(x => x.MatricNo.StartsWith("UJ/")).ToList();
            }

            totalRecords = studentIndex.Count();
            var data = studentIndex.Skip(skip).Take(pageSize).ToList();

            return Json(new { draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data },
                JsonRequestBehavior.AllowGet);

        }

        public async Task<ActionResult> GetLibraryIndex(int? SchoolProgrammeId, int? FacultyId, int? DepartmentId, int? ProgrammeId,
                                     int? SessionId, int? LevelId)
        {

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

            var staffRecord = _db.Staffs.Include(i => i.Department.Faculty)
                              .FirstOrDefault(x => x.Email.ToUpper().Trim().Equals(userId.Trim().ToUpper()));
            if (User.IsInRole(RoleName.Dean))
            {
                FacultyId = staffRecord?.Department.FacultyId;
            }
            if (User.IsInRole(RoleName.Hod))
            {
                DepartmentId = staffRecord?.Department.DepartmentId;
            }
            var studentIndex = new List<LibraryPatronUploadVm>();

            var studentList = await _studentQuery.GetStudentListForLibraryUpload(SchoolProgrammeId, FacultyId, DepartmentId, ProgrammeId, SessionId, LevelId);

            if (!string.IsNullOrEmpty(search))
            {
                studentIndex = studentList.Where(x => x.cardnumber.ToUpper().Contains(search.ToUpper().Trim())
                            || x.firstname.Trim().ToUpper().Contains(search.ToUpper().Trim())).ToList();
                if (studentIndex.Count() == 0)
                {
                    studentIndex = studentList.Where(x => !string.IsNullOrEmpty(x.email) &&
                            x.email.ToUpper().Contains(search.ToUpper().Trim())).ToList();
                }
            }

            else
            {
                studentIndex = studentList;
            }

            totalRecords = studentIndex.Count();
            var data = studentIndex.Skip(skip).Take(pageSize).ToList();

            return Json(new { draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data },
                JsonRequestBehavior.AllowGet);

        }

        public async Task DownloadMatGenReport(int? SchoolProgrammeId, int? FacultyId, int? DepartmentId, int? ProgrammeId,
                                     int? SessionId, int? LevelId, string isDownloded)
        {
            var model = await _studentQuery.GetStudentListFor365EmailUpload(SchoolProgrammeId, FacultyId, DepartmentId, ProgrammeId, SessionId, LevelId, isDownloded);
            foreach (var item in model)
            {
                var student = await _db.Students.FindAsync(item.StudentId);
                student.IsDownloaded = true;
                _db.Entry(student).State = EntityState.Modified;
            }
            await _db.SaveChangesAsync();

            char c1 = 'A';
            ExcelPackage package = new ExcelPackage();
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Report");

            worksheet.Cells[$"{c1++}1"].Value = "No";
            worksheet.Cells[$"{c1++}1"].Value = "Matric";
            worksheet.Cells[$"{c1++}1"].Value = "Email";
            worksheet.Cells[$"{c1++}1"].Value = "Full Name";
            worksheet.Cells[$"{c1++}1"].Value = "Phone Number";
            worksheet.Cells[$"{c1++}1"].Value = "Gender";
            worksheet.Cells[$"{c1++}1"].Value = "Level";
            worksheet.Cells[$"{c1++}1"].Value = "Department";

            int rowStart = 2;

            for (var i = 0; i < model.Count; i++)
            {
                worksheet.Cells[$"A{rowStart}"].Value = i;
                worksheet.Cells[$"B{rowStart}"].Value = model[i].MatricNo;
                worksheet.Cells[$"C{rowStart}"].Value = model[i].Email;
                worksheet.Cells[$"D{rowStart}"].Value = model[i].FullName;
                worksheet.Cells[$"E{rowStart}"].Value = model[i].PhoneNumber;
                worksheet.Cells[$"F{rowStart}"].Value = model[i].Gender;
                worksheet.Cells[$"G{rowStart}"].Value = model[i].LevelName;
                worksheet.Cells[$"H{rowStart}"].Value = model[i].DeptName;
                rowStart++;
            }

            worksheet.Cells["A:AZ"].AutoFitColumns();
            worksheet.Column(1).Style.Locked = true;
            worksheet.Column(2).Style.Locked = true;
            worksheet.Column(3).Style.Locked = true;
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment: filename=" +
                                                      $"ApplicantPayment.xlsx");
            Response.BinaryWrite(package.GetAsByteArray());
            Response.End();
        }

        public async Task<ActionResult> GetDepartmentClearance(int? SchoolProgrammeId, string hasRegistered, int? SessionId)
        {
            //#region Server Side filtering

            ////Get parameter for sorting from grid table
            //// get Start (paging start index) and length (page size for paging)
            //var draw = Request.Form.GetValues("draw").FirstOrDefault();
            //var start = Request.Form.GetValues("start").FirstOrDefault();
            //var length = Request.Form.GetValues("length").FirstOrDefault();
            ////Get Sort columns values when we click on Header Name of column
            ////getting column name
            //var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            ////Soring direction(either desending or ascending)
            //var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
            //string search = Request.Form.GetValues("search[value]").FirstOrDefault();

            //int pageSize = length != null ? Convert.ToInt32(length) : 0;
            //int skip = start != null ? Convert.ToInt32(start) : 0;
            //int totalRecords = 0;

            //var studentIndex = new List<StudentIndexVM>();

            //var staffDept = await _db.Staffs.AsNoTracking().Where(x => x.Email.Equals(userId))
            //                        .Select(s => s.DepartmentId).FirstOrDefaultAsync();

            //var studentList = await _studentQuery.GetStudentDeptList(SchoolProgrammeId, staffDept);

            //if (!string.IsNullOrEmpty(search))
            //{

            //    studentIndex = studentList.Where(x => x.MatricNo.ToUpper().Equals(search.ToUpper().Trim())
            //                            || x.FullName.ToUpper().Contains(search.ToUpper().Trim()))
            //                            .ToList();
            //}
            //else
            //{
            //    studentIndex = studentList;
            //}

            //studentIndex = studentIndex.OrderBy(x => x.FullName).ToList();
            //totalRecords = studentIndex.Count();
            //var data = studentIndex.Skip(skip).Take(pageSize).ToList();

            //return Json(new { draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data },
            //    JsonRequestBehavior.AllowGet);

            //#endregion Server Side filtering
            //SchoolProgrammeId = await GetUndergraduateSchoolProgrammeId();
            bool isCleared = false;
            if (hasRegistered.Equals("Cleared"))
            {
                isCleared = true;
            }
            var studentIndex = new List<StudentIndexVM>();

            var staffDept = await _db.Staffs.AsNoTracking().Where(x => x.Email.Equals(userId))
                                    .Select(s => s.DepartmentId).FirstOrDefaultAsync();

            var studentList = await _studentQuery.GetStudentDeptList(SchoolProgrammeId, staffDept, isCleared, SessionId);
            studentIndex = studentList;

            var data = studentIndex.OrderBy(x => x.JambRegNo).ToList();

            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        // This fetches a single student for department clearance
        public async Task<ActionResult> GetDClearance(int? SchoolProgrammeId, string hasRegistered, int? SessionId, string studentId)
        {
            bool isCleared = false;
            if (hasRegistered.Equals("Cleared"))
            {
                isCleared = true;
            }
            var studentIndex = new List<StudentIndexVM>();

            var staffDept = await _db.Staffs.AsNoTracking().Where(x => x.Email.Equals(userId))
                                    .Select(s => s.DepartmentId).FirstOrDefaultAsync();

            //var studentList = await _studentQuery.GetStudentDeptList(SchoolProgrammeId, staffDept, isCleared, SessionId);
            var schoolProgrammeName = await _db.SchoolProgrammes.Where(x => x.SchoolProgrammeId.Equals((int)SchoolProgrammeId)).Select(x => x.SchoolProgrammeCode).FirstOrDefaultAsync();

            if (schoolProgrammeName.Equals("UG"))
            {
                var studentList = await _db.Students.Include(i => i.SchoolProgramme).Include(i => i.Programme).Include(i => i.Session)
                                    .Include(i => i.Programme.Department).Include(i => i.Level).AsNoTracking()
                                    .Where(x => x.SchoolProgrammeId.Equals((int)SchoolProgrammeId)
                                    && x.Programme.Department.DepartmentId.Equals((int)staffDept)
                                    && x.IsClearedDepartment.Equals(isCleared)
                                    && x.Session.SessionId.Equals((int)SessionId)
                                    && (x.MatricNo.ToUpper().Trim().Equals(studentId.ToUpper().Trim()) || x.JambRegNo.Equals(studentId.ToUpper().Trim()))
                                    && x.IsClearedFaculty.Equals(true)   //Added to change order of clearance
                                    && x.Active.Equals(true)
                                    && x.IsDelete.Equals(false) && x.IsGraduated.Equals(false))
                                    .Select(s => new StudentIndexVM()
                                    {
                                        StudentId = s.StudentId,
                                        FirstName = s.FirstName,
                                        LastName = s.LastName,
                                        MiddleName = s.MiddleName,
                                        Gender = s.Gender,
                                        ProgrammeName = s.Programme.ProgrammeName,
                                        MatricNo = !string.IsNullOrEmpty(s.MatricNo) ? s.MatricNo : s.JambRegNo,
                                        PhoneNumber = s.PhoneNumber,
                                        JambRegNo = s.JambRegNo,
                                        LevelName = s.Level.LevelName ?? "",
                                        SchoolProgrammeCode = s.SchoolProgramme.ProgrammeCategory,
                                        Email = s.Email,
                                        ModeOfEntry = s.ModeOfEntry
                                    }).ToListAsync();

                studentIndex = studentList;
            }
            else if (schoolProgrammeName.Equals("RM_SC")) // for Remedial students clearance
            {
                var studentList = await _db.Students.Include(i => i.SchoolProgramme).Include(i => i.Programme).Include(i => i.Session)
                                    .Include(i => i.Programme.Department).Include(i => i.Level).AsNoTracking()
                                    .Where(x => x.SchoolProgrammeId.Equals((int)SchoolProgrammeId)
                                    && x.Programme.Department.DepartmentId.Equals((int)staffDept)
                                    && x.IsClearedDepartment.Equals(isCleared)
                                    && x.Session.SessionId.Equals((int)SessionId)
                                    && (x.MatricNo.ToUpper().Trim().Equals(studentId.ToUpper().Trim()) || x.JambRegNo.Equals(studentId.ToUpper().Trim()))
                                    && x.Active.Equals(true)
                                    && x.IsDelete.Equals(false) && x.IsGraduated.Equals(false))
                                    .Select(s => new StudentIndexVM()
                                    {
                                        StudentId = s.StudentId,
                                        FirstName = s.FirstName,
                                        LastName = s.LastName,
                                        MiddleName = s.MiddleName,
                                        Gender = s.Gender,
                                        ProgrammeName = s.Programme.ProgrammeName,
                                        MatricNo = !string.IsNullOrEmpty(s.MatricNo) ? s.MatricNo : s.JambRegNo,
                                        PhoneNumber = s.PhoneNumber,
                                        JambRegNo = s.JambRegNo,
                                        LevelName = s.Level.LevelName ?? "",
                                        SchoolProgrammeCode = s.SchoolProgramme.ProgrammeCategory,
                                        Email = s.Email,
                                        ModeOfEntry = s.ModeOfEntry
                                    }).ToListAsync();

                studentIndex = studentList;
            }
            else
            {
                var studentList = await _db.Students.Include(i => i.SchoolProgramme).Include(i => i.Programme).Include(i => i.Session)
                                    .Include(i => i.Programme.Department).Include(i => i.Level).AsNoTracking()
                                    .Where(x => x.SchoolProgrammeId.Equals((int)SchoolProgrammeId)
                                    && x.Programme.Department.DepartmentId.Equals((int)staffDept)
                                    && x.IsClearedDepartment.Equals(isCleared)
                                    && x.Session.SessionId.Equals((int)SessionId)
                                    && (x.MatricNo.ToUpper().Trim().Equals(studentId.ToUpper().Trim()) || x.JambRegNo.Equals(studentId.ToUpper().Trim()))
                                    && x.IsClearedAcademics.Equals(true)   //Added to change order of clearance
                                    && x.Active.Equals(true)
                                    && x.IsDelete.Equals(false) && x.IsGraduated.Equals(false))
                                    .Select(s => new StudentIndexVM()
                                    {
                                        StudentId = s.StudentId,
                                        FirstName = s.FirstName,
                                        LastName = s.LastName,
                                        MiddleName = s.MiddleName,
                                        Gender = s.Gender,
                                        ProgrammeName = s.Programme.ProgrammeName,
                                        MatricNo = !string.IsNullOrEmpty(s.MatricNo) ? s.MatricNo : s.JambRegNo,
                                        PhoneNumber = s.PhoneNumber,
                                        JambRegNo = s.JambRegNo,
                                        LevelName = s.Level.LevelName ?? "",
                                        SchoolProgrammeCode = s.SchoolProgramme.ProgrammeCategory,
                                        Email = s.Email,
                                        ModeOfEntry = s.ModeOfEntry
                                    }).ToListAsync();

                studentIndex = studentList;
            }

            //studentIndex = studentList;

            var data = studentIndex.OrderBy(x => x.JambRegNo).ToList();

            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> GetClearanceReport(int SchoolProgrammeId, string hasRegistered, int? FacultyId, int? DepartmentId,
            int? ProgrammeId, string clearanceStage)
        {
            bool isCleared = false;
            if (hasRegistered.Equals("Cleared"))
            {
                isCleared = true;
            }
            var studentIndex = new List<StudentIndexVM>();
            var SessionId = _query.GetCurrentSessionId(SchoolProgrammeId);

            var studentList = await _studentQuery.GetStudentDeptList(SchoolProgrammeId, ProgrammeId, DepartmentId, FacultyId, isCleared, SessionId, clearanceStage);
            studentIndex = studentList;

            var data = studentIndex.OrderBy(x => x.FullName).ToList();

            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }


        public async Task<ActionResult> GetFacultyClearance(int? SchoolProgrammeId, string hasRegistered, int? SessionId)
        {
            //#region Server Side filtering

            ////Get parameter for sorting from grid table
            //// get Start (paging start index) and length (page size for paging)
            //var draw = Request.Form.GetValues("draw").FirstOrDefault();
            //var start = Request.Form.GetValues("start").FirstOrDefault();
            //var length = Request.Form.GetValues("length").FirstOrDefault();
            ////Get Sort columns values when we click on Header Name of column
            ////getting column name
            //var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            ////Soring direction(either descending or ascending)
            //var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
            //string search = Request.Form.GetValues("search[value]").FirstOrDefault();

            //int pageSize = length != null ? Convert.ToInt32(length) : 0;
            //int skip = start != null ? Convert.ToInt32(start) : 0;
            //int totalRecords = 0;

            //var studentIndex = new List<StudentIndexVM>();

            //int? staffFacultyId = await _db.Staffs.Include(i => i.Department.Faculty).AsNoTracking()
            //                    .Where(x => x.Email.Equals(userId))
            //                    .Select(s => s.Department.FacultyId).FirstOrDefaultAsync();
            //if (staffFacultyId == 0)
            //{
            //    staffFacultyId = null;
            //}

            //var studentList = await _studentQuery.GetStudentfacultyList(SchoolProgrammeId, staffFacultyId);

            //if (!string.IsNullOrEmpty(search))
            //{

            //    studentIndex = studentList.Where(x => x.MatricNo.ToUpper().Equals(search.ToUpper().Trim())
            //                            || x.FullName.ToUpper().Contains(search.ToUpper().Trim()))
            //                            .ToList();
            //}
            //else
            //{
            //    studentIndex = studentList;
            //}

            //studentIndex = studentIndex.OrderBy(x => x.FullName).ToList();
            //totalRecords = studentIndex.Count();
            //var data = studentIndex.Skip(skip).Take(pageSize).ToList();

            //return Json(new { draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data },
            //    JsonRequestBehavior.AllowGet);

            //#endregion Server Side filtering
            bool isCleared = false;
            if (hasRegistered.Equals("Cleared"))
            {
                isCleared = true;
            }
            var studentIndex = new List<StudentIndexVM>();

            int? staffFacultyId = await _db.Staffs.Include(i => i.Department.Faculty).AsNoTracking()
                                .Where(x => x.Email.Equals(userId))
                                .Select(s => s.Department.FacultyId).FirstOrDefaultAsync();
            if (staffFacultyId == 0)
            {
                staffFacultyId = null;
            }

            var studentList = await _studentQuery.GetStudentfacultyList(SchoolProgrammeId, staffFacultyId, isCleared, SessionId);
            studentIndex = studentList;

            var data = studentIndex.OrderBy(x => x.FullName).ToList();

            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        // This fetches a single student for faculty clearance
        public async Task<ActionResult> GetFClearance(int? SchoolProgrammeId, string hasRegistered, int? SessionId, string studentId)
        {
            bool isCleared = false;
            if (hasRegistered.Equals("Cleared"))
            {
                isCleared = true;
            }
            var studentIndex = new List<StudentIndexVM>();

            int? staffFacultyId = await _db.Staffs.Include(i => i.Department.Faculty).AsNoTracking()
                                .Where(x => x.Email.Equals(userId))
                                .Select(s => s.Department.FacultyId).FirstOrDefaultAsync();
            if (staffFacultyId == 0)
            {
                staffFacultyId = null;
            }

            //var studentList = await _studentQuery.GetStudentfacultyList(SchoolProgrammeId, staffFacultyId, isCleared, SessionId);
            var schoolProgrammeName = await _db.SchoolProgrammes.Where(x => x.SchoolProgrammeId.Equals((int)SchoolProgrammeId)).Select(x => x.SchoolProgrammeCode).FirstOrDefaultAsync();

            //Check for Undergraduate Students
            if (schoolProgrammeName.Equals("UG"))
            {
                var studentList = await _db.Students.Include(i => i.Programme).Include(i => i.Session)
                                    .Include(i => i.Programme.Department).AsNoTracking()
                                    .Where(x => x.IsDelete.Equals(false)
                                    && x.SchoolProgrammeId.Equals((int)SchoolProgrammeId)
                                    && x.Programme.Department.FacultyId.Equals((int)staffFacultyId)
                                    && x.IsClearedFaculty.Equals(isCleared)
                                    && x.Session.SessionId.Equals((int)SessionId)
                                    && (x.MatricNo.ToUpper().Trim().Equals(studentId.ToUpper().Trim()) || x.JambRegNo.Equals(studentId.ToUpper().Trim()))
                                    && x.IsClearedAcademics.Equals(true) //Added to change order of clearance
                                                                         //&& x.IsClearedAcademics.Equals(true) //Added to change order of clearance
                                                                         //&& x.StudentStatus.ToUpper().Equals(StudentStatus.New_Student.ToString().ToUpper())
                                                                         //&& x.IsClearedAll.Equals(false)
                                    && x.IsGraduated.Equals(false) && x.IsDelete.Equals(false))
                                    .Select(s => new StudentIndexVM()
                                    {
                                        StudentId = s.StudentId,
                                        FirstName = s.FirstName,
                                        LastName = s.LastName,
                                        MiddleName = s.MiddleName,
                                        Gender = s.Gender,
                                        ProgrammeName = s.Programme.ProgrammeName,
                                        MatricNo = !string.IsNullOrEmpty(s.MatricNo) ? s.MatricNo : s.JambRegNo,
                                        PhoneNumber = s.PhoneNumber,
                                        JambRegNo = s.JambRegNo,
                                        ModeOfEntry = s.ModeOfEntry,
                                        LevelName = s.Level.LevelName,
                                        SchoolProgrammeCode = s.SchoolProgramme.ProgrammeCategory
                                    }).ToListAsync();

                studentIndex = studentList;
            }
            else //For all Other Non-Undergraduate students
            {
                var studentList = await _db.Students.Include(i => i.Programme).Include(i => i.Session)
                                    .Include(i => i.Programme.Department).AsNoTracking()
                                    .Where(x => x.IsDelete.Equals(false)
                                    && x.SchoolProgrammeId.Equals((int)SchoolProgrammeId)
                                    && x.Programme.Department.FacultyId.Equals((int)staffFacultyId)
                                    && x.IsClearedFaculty.Equals(isCleared)
                                    && x.IsClearedDepartment.Equals(true)
                                    && x.Session.SessionId.Equals((int)SessionId)
                                    && (x.MatricNo.ToUpper().Trim().Equals(studentId.ToUpper().Trim()) || x.JambRegNo.Equals(studentId.ToUpper().Trim()))
                                    && x.IsClearedAcademics.Equals(true) //Added to change order of clearance
                                    //&& x.StudentStatus.ToUpper().Equals(StudentStatus.New_Student.ToString().ToUpper())
                                    //&& x.IsClearedAll.Equals(false)
                                    && x.IsGraduated.Equals(false) && x.IsDelete.Equals(false))
                                    .Select(s => new StudentIndexVM()
                                    {
                                        StudentId = s.StudentId,
                                        FirstName = s.FirstName,
                                        LastName = s.LastName,
                                        MiddleName = s.MiddleName,
                                        Gender = s.Gender,
                                        ProgrammeName = s.Programme.ProgrammeName,
                                        MatricNo = !string.IsNullOrEmpty(s.MatricNo) ? s.MatricNo : s.JambRegNo,
                                        PhoneNumber = s.PhoneNumber,
                                        JambRegNo = s.JambRegNo,
                                        ModeOfEntry = s.ModeOfEntry,
                                        LevelName = s.Level.LevelName,
                                        SchoolProgrammeCode = s.SchoolProgramme.ProgrammeCategory
                                    }).ToListAsync();

                studentIndex = studentList;
            }

            //studentIndex = studentList;

            var data = studentIndex.OrderBy(x => x.FullName).ToList();

            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> GetAcademicClearance(int? SchoolProgrammeId, string hasRegistered, int? SessionId)
        {
            bool isCleared = false;
            if (hasRegistered.Equals("Cleared"))
            {
                isCleared = true;
            }
            var studentIndex = new List<StudentIndexVM>();
            int? staffFacultyId = await _db.Staffs.Include(i => i.Department.Faculty).AsNoTracking()
                                .Where(x => x.Email.Equals(userId))
                                .Select(s => s.Department.FacultyId).FirstOrDefaultAsync();
            if (staffFacultyId == 0)
            {
                staffFacultyId = null;
            }
            var studentList = await _studentQuery.GetStudentAcademicList(SchoolProgrammeId, staffFacultyId, isCleared, SessionId);
            studentIndex = studentList;

            var data = studentIndex.OrderBy(x => x.FullName).ToList();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
            //var jsonResult = Json(data, JsonRequestBehavior.AllowGet);
            //jsonResult.MaxJsonLength = int.MaxValue;
            //return jsonResult;

        }

        // This fetches a single student for faculty clearance
        public async Task<ActionResult> GetAClearance(int? SchoolProgrammeId, string hasRegistered, int? SessionId, string studentId)
        {
            bool isCleared = false;
            if (hasRegistered.Equals("Cleared"))
            {
                isCleared = true;
            }
            var studentIndex = new List<StudentIndexVM>();
            int? staffFacultyId = await _db.Staffs.Include(i => i.Department.Faculty).AsNoTracking()
                                .Where(x => x.Email.Equals(userId))
                                .Select(s => s.Department.FacultyId).FirstOrDefaultAsync();
            if (staffFacultyId == 0)
            {
                staffFacultyId = null;
            }

            //var studentList = await _studentQuery.GetStudentAcademicList(SchoolProgrammeId, staffFacultyId, isCleared, SessionId);
            //studentIndex = studentList;

            var schoolProgrammeName = await _db.SchoolProgrammes.Where(x => x.SchoolProgrammeId.Equals((int)SchoolProgrammeId)).Select(x => x.SchoolProgrammeCode).FirstOrDefaultAsync();

            if (schoolProgrammeName.Equals("UG"))
            {
                var studentList = await _db.Students.Include(i => i.SchoolProgramme).Include(i => i.Programme)
                                 .Include(i => i.Programme.Department).AsNoTracking()
                                 .Where(x => x.SchoolProgrammeId.Equals((int)SchoolProgrammeId)
                                 && x.Programme.Department.FacultyId.Equals((int)staffFacultyId)
                                 //&& x.StudentStatus.ToUpper().Equals(StudentStatus.New_Student.ToString().ToUpper())
                                 //&& x.IsClearedAll.Equals(false)
                                 //&& x.IsClearedFaculty.Equals(true) //Commented to change clearance order
                                 && (x.MatricNo.ToUpper().Trim().Equals(studentId.ToUpper().Trim()) || x.JambRegNo.Equals(studentId.ToUpper().Trim()))
                                 && x.IsClearedAcademics.Equals(isCleared)
                                 && x.Session.SessionId.Equals((int)SessionId)
                                 && x.IsDelete.Equals(false) && x.IsGraduated.Equals(false))
                                 .Select(s => new StudentIndexVM()
                                 {
                                     StudentId = s.StudentId,
                                     FirstName = s.FirstName,
                                     LastName = s.LastName,
                                     MiddleName = s.MiddleName,
                                     Gender = s.Gender,
                                     ProgrammeName = s.Programme.ProgrammeName,
                                     MatricNo = !string.IsNullOrEmpty(s.MatricNo) ? s.MatricNo : s.JambRegNo,
                                     PhoneNumber = s.PhoneNumber,
                                     JambRegNo = s.JambRegNo,
                                     ModeOfEntry = s.ModeOfEntry,
                                     LevelName = s.Level.LevelName,
                                     SchoolProgrammeCode = s.SchoolProgramme.ProgrammeCategory,
                                 }).ToListAsync();

                studentIndex = studentList;
            }
            else //For all Other Non-Undergraduate studentsGetAClearance
            {
                var studentList = await _db.Students.Include(i => i.SchoolProgramme).Include(i => i.Programme)
                                 .Include(i => i.Programme.Department).AsNoTracking()
                                 .Where(x => x.SchoolProgrammeId.Equals((int)SchoolProgrammeId)
                                 && x.Programme.Department.FacultyId.Equals((int)staffFacultyId)
                                 //&& x.StudentStatus.ToUpper().Equals(StudentStatus.New_Student.ToString().ToUpper())
                                 //&& x.IsClearedAll.Equals(false)
                                 && (x.MatricNo.ToUpper().Trim().Equals(studentId.ToUpper().Trim()) || x.JambRegNo.Equals(studentId.ToUpper().Trim()))
                                 //&& x.IsClearedFaculty.Equals(true) //Commented to change clearance order
                                 && x.IsClearedAcademics.Equals(isCleared)
                                 && x.Session.SessionId.Equals((int)SessionId)
                                 && x.IsDelete.Equals(false) && x.IsGraduated.Equals(false))
                                 .Select(s => new StudentIndexVM()
                                 {
                                     StudentId = s.StudentId,
                                     FirstName = s.FirstName,
                                     LastName = s.LastName,
                                     MiddleName = s.MiddleName,
                                     Gender = s.Gender,
                                     ProgrammeName = s.Programme.ProgrammeName,
                                     MatricNo = !string.IsNullOrEmpty(s.MatricNo) ? s.MatricNo : s.JambRegNo,
                                     PhoneNumber = s.PhoneNumber,
                                     JambRegNo = s.JambRegNo,
                                     ModeOfEntry = s.ModeOfEntry,
                                     LevelName = s.Level.LevelName,
                                     SchoolProgrammeCode = s.SchoolProgramme.ProgrammeCategory,
                                 }).ToListAsync();

                studentIndex = studentList;
            }

            var data = studentIndex.OrderBy(x => x.FullName).ToList();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }


        public async Task<ActionResult> DeptClearStudent(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                var student = await _db.Students.Include(i => i.SchoolProgramme).AsNoTracking()
                                .Where(x => x.StudentId.Equals(id)).FirstOrDefaultAsync();
                var fullName = student.FullName;
                var session = _query.GetCurrentSession(student.SchoolProgrammeId);

                string SMSbody = $"{student.FirstName} Congratulations! you have successfully done your registration. Please do use your dashboard and email regularly.";

                if (student.SchoolProgramme.ProgrammeCategory.Equals(ProgrammeCategory.Preliminary_French.ToString())
                    || student.SchoolProgramme.ProgrammeCategory.Equals(ProgrammeCategory.Remedial_Science.ToString()))
                {

                    //if (!string.IsNullOrEmpty(student.MatricNo))
                    //{
                    //    return new JsonResult { Data = new { status = false, message = $"Oops.. This student already has a Matric No of ({student.MatricNo})" } };
                    //}
                    //var matricNo = await SaveAndGenerateMatricNo(id, session);

                    //student.IsClearedFaculty = true;
                    //student.MatricNo = matricNo;
                    //student.PrimaryEmail = $"{matricNo}@unijos.edu.ng";

                    //_db.Entry(student).State = EntityState.Modified;
                    student.IsClearedFaculty = true;
                    student.IsClearedDepartment = true;
                    student.IsClearedAcademics = true;

                    _db.Entry(student).State = EntityState.Modified;
                    _db.SaveChanges();
                    await SMSClass.SendSMS("UNIJOS SIS", SMSbody, student.PhoneNumber); //EBULK SMS API

                    return new JsonResult { Data = new { status = true, message = $"{fullName} has been cleared successfully." } };

                }
                else
                {
                }
                student.IsClearedDepartment = true;
                //var student = new Student { StudentId = "S12345" };
                student.ClearAtDepartment(userId);
                _db.Entry(student).State = EntityState.Modified;
                await _db.SaveChangesAsync();

                return new JsonResult { Data = new { status = true, message = $"{student.FullName} has been cleared successfully" } };
            }
            return new JsonResult { Data = new { status = false, message = "Oops.. Something went wrong, student cant be found" } };
            //return View(subject);
        }


        public async Task<ActionResult> AcademicClearStudent(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                var student = await _db.Students.AsNoTracking().Where(x => x.StudentId.Equals(id)).FirstOrDefaultAsync();

                student.IsClearedAcademics = true;
                student.ClearAtAcademic(userId);
                string body = $"{student.FirstName} Congratulations you have been cleared. Proceed to your Faculty Officer" +
                               $" for the second clearance";

                if (!string.IsNullOrEmpty(student.MatricNo))
                {
                    student.StudentStatus = StudentStatus.Returning.ToString();
                }
                _db.Entry(student).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                await SMSClass.SendSMS("UNIJOS SIS", body, student.PhoneNumber); //EBULK SMS API

                return new JsonResult { Data = new { status = true, message = $"{student.FullName} has been cleared successfully" } };
            }
            return new JsonResult { Data = new { status = false, message = "Oops.. Something went wrong, student cant be found" } };
        }


        public async Task<ActionResult> FacultyClearStudent(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                var student = await _db.Students.Include(s => s.Session).Include(s => s.SchoolProgramme).Include(s => s.Programme.Department).Where(x => x.StudentId.Equals(id)).FirstOrDefaultAsync();
                var sessionId = _query.GetCurrentSessionId(student.SchoolProgrammeId);

                //string studentStatus = student.Session.SessionId.Equals(sessionId) || student.StudentStatus.Equals(StudentStatus.New_Student.ToString())
                //                       ? StudentStatus.New_Student.ToString()
                //                       : StudentStatus.Returning.ToString();

                //var paymentSetting = await _feeQueryManager.GetPaymentSetting(sessionId, student.SchoolProgramme.SchoolProgrammeId, studentStatus);

                //var feeType = new List<ReportFeeListVm>();
                //decimal TaltoalFeeAmonut = 0;
                //var feeList = await _feeQueryManager.GetSchoolFeeList(SchoolFeeCategory.School_Charges.ToString(), student, paymentSetting, sessionId);
                //feeList.AddRange(await _feeQueryManager.GetDepartmentPaymentList(student.Programme.Department.DepartmentId, sessionId, (Int32)student.LevelId));

                //TaltoalFeeAmonut = feeList.Sum(s => s.Amount);
                
                string SMSbody = $"{student.FirstName} Congratulations! you have been cleared. Proceed to pay your school charges from your dashboard.";

                if (!string.IsNullOrEmpty(student.MatricNo))
                {
                    return new JsonResult { Data = new { status = false, message = $"Oops.. This student already has a Matric No of ({student.MatricNo})" } };
                }
                var fullName = student.FullName;
                var session = _query.GetCurrentSession(student.SchoolProgrammeId);
                student.IsClearedFaculty = true;
                student.ClearAtFaculty(userId);
                _db.Entry(student).State = EntityState.Modified;
                _db.SaveChanges();

                //if (student.SchoolProgramme.SchoolProgrammeCode.Equals("UG"))
                //{
                //     SMSbody = $"Congratulations! you have been cleared. Your Admission letter is being re-validated. You will receive a text message in a moment to proceed to pay school charges.";

                //}
                await SMSClass.SendSMS("UNIJOS SIS", SMSbody, student.PhoneNumber); //EBULK SMS API

                return new JsonResult { Data = new { status = true, message = $"{fullName} has been cleared successfully." } };
            }
            return new JsonResult { Data = new { status = false, message = "Oops.. Something went wrong, student cant be found" } };
        }

        public async Task<ActionResult> AdmissionClearStudent(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                var student = await _db.Students.Include(s => s.Session).Include(s => s.SchoolProgramme).Include(s => s.Programme.Department).Where(x => x.StudentId.Equals(id)).FirstOrDefaultAsync();
                var sessionId = _query.GetCurrentSessionId(student.SchoolProgrammeId);

                string studentStatus = student.Session.SessionId.Equals(sessionId) || student.StudentStatus.Equals(StudentStatus.New_Student.ToString())
                                       ? StudentStatus.New_Student.ToString()
                                       : StudentStatus.Returning.ToString();

                var paymentSetting = await _feeQueryManager.GetPaymentSetting(sessionId, student.SchoolProgramme.SchoolProgrammeId, studentStatus);

                var feeType = new List<ReportFeeListVm>();
                decimal TaltoalFeeAmonut = 0;
                var feeList = await _feeQueryManager.GetSchoolFeeList(SchoolFeeCategory.School_Charges.ToString(), student, paymentSetting, sessionId);
                feeList.AddRange(await _feeQueryManager.GetDepartmentPaymentList(student.Programme.Department.DepartmentId, sessionId, (Int32)student.LevelId));

                TaltoalFeeAmonut = feeList.Sum(s => s.Amount);
                //foreach (var item in feeList)
                //{
                //    TaltoalFeeAmonut += item.Amount;
                //}
                string SMSbody = $"{student.FirstName} Congratulations! you have been cleared. Proceed to pay {TaltoalFeeAmonut} school charges from your dashboard.";

                if (!string.IsNullOrEmpty(student.MatricNo))
                {
                    return new JsonResult { Data = new { status = false, message = $"Oops.. This student already has a Matric No of ({student.MatricNo})" } };
                }
                var fullName = student.FullName;
                var session = _query.GetCurrentSession(student.SchoolProgrammeId);
                student.ImeiNo = Guid.NewGuid().ToString();
                //student.ClearAtFaculty(userId);
                _db.Entry(student).State = EntityState.Modified;
                _db.SaveChanges();

            
                await SMSClass.SendSMS("UNIJOS SIS", SMSbody, student.PhoneNumber); //EBULK SMS API

                return new JsonResult { Data = new { status = true, message = $"{fullName} has been cleared successfully." } };
            }
            return new JsonResult { Data = new { status = false, message = "Oops.. Something went wrong, student cant be found" } };
        }


        public PartialViewResult RejectStudent(string studentId, string level)
        {
            if (!string.IsNullOrEmpty(studentId) && !string.IsNullOrEmpty(level))
            {
                var model = new RejectStudentVm()
                {
                    StudentId = studentId,
                    LevelOfReject = level
                };
                return PartialView(model);
            }
            ViewBag.Message = "Empty Student Id";
            return PartialView();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RejectStudent(RejectStudentVm model)
        {

            if (ModelState.IsValid)
            {
                var rejectedStudent = new RejectedStudent()
                {
                    StudentId = model.StudentId,
                    SessionId = sessionId,
                    ReasonForRejection = model.ReasonForReject,
                    StaffId = userId
                };
                _db.RejectedStudents.Add(rejectedStudent);

                var student = await _db.Students.FindAsync(model.StudentId);
                string body = $"{student.FirstName} Sorry your clearance was not successful; you may wish to apply for " +
                               $"change of course from your dashboard on payment of 30k";

                student.IsClearedAcademics = false;
                student.IsClearedFaculty = false;
                student.IsClearedDepartment = false;

                _db.Entry(student).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                await SMSClass.SendSMS("UNIJOS SIS", body, student.PhoneNumber); //EBULK SMS API

                return new JsonResult { Data = new { status = true, message = "Student has been Rejected Successfully" } };
            }
            return new JsonResult { Data = new { status = false, message = "Oops.. Something went Wrong" } };
            //return View(subject);
        }

        public PartialViewResult UploadBloodGroupView(string studentId)
        {
            if (!string.IsNullOrEmpty(studentId) /*&& !string.IsNullOrEmpty(level)*/)
            {
                var model = new UploadBloodGroupVm()
                {
                    StudentId = studentId,
                };
                return PartialView(model);
            }
            ViewBag.Message = "Empty Student Id";
            return PartialView();
        }
        public async Task<ActionResult> UploadBloodGroup(UploadBloodGroupVm model)
        {

            var student = await _db.Students.FindAsync(model.StudentId);
            student.BloodGroup = model.BloodGroup;

            _db.Entry(student).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return new JsonResult { Data = new { status = true, message = "Student Blood Group has been Upload Successfully" } };

        }

        public async Task<ActionResult> UploadStudentBloodGroup(HttpPostedFileBase excelfile)
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
                        var bloodGroup = workSheet.Cells[row, 2].Value.ToString().Trim();

                        //var student = await _db.Students
                        //                    .Where(x => x.MatricNo.Trim().ToUpper().Equals(matNo.ToUpper()))
                        //                    .FirstOrDefaultAsync();
                        var student = await _db.Students.FirstOrDefaultAsync(x => x.MatricNo.Trim().ToUpper().Equals(matNo.ToUpper()));


                        if (student != null)
                        {
                            student.BloodGroup = bloodGroup;
                            _db.Entry(student).State = EntityState.Modified;


                        }
                        else
                        {
                            utmeUploadError.Add(new StudentUtmeUploadErrorVm { JambRegNo = matNo, Row = row });
                            hasUtmeUploadError = true;
                            //ViewBag.ErrorMessage = $"Student not found {matNo}";
                            //return View("ErrorException");
                        }
                        recordCount++;
                        //lastrecord = $"The last Updated record has the Surname  {student.LastName} and " +
                        //    $"First Name {student.FirstName} with  Reg No {student.MatricNo}";
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
                        ViewBag.ErrorMessage = $"You have successfully Uploaded {recordCount} records...";
                        return View("ErrorException", utmeUploadError);
                    }
                    message = $"You have successfully Uploaded {recordCount} records...  and {lastrecord}";
                    ViewBag.Message = message;
                    return RedirectToAction("Index", "Students", new { message });
                }
            }
            ViewBag.Error = $"File type is Incorrect <br/>";
            return View("Index");
        }


        public PartialViewResult ExcelUpload()
        {
            return PartialView();
        }

        public PartialViewResult BloodGroupExcelUpload()
        {
            return PartialView();
        }

        public async Task<PartialViewResult> PartialDetails(string id)
        {
            if (id == null)
            {
                id = userId;
            }

            var student = await _db.Students.FindAsync(id);

            return PartialView(student);
        }

        public async Task<PartialViewResult> PartialDetail(string id)
        {
            if (id == null)
            {
                id = userId;
            }
            var student = await _db.Students.FindAsync(id);
            var model = new StudentPartialVm()
            {
                Student = student,
                ApplicantOLevelResults = await _db.ApplicantOLevelResults.Include(i => i.Subject).AsNoTracking()
                                            .Where(x => x.ApplicantId.Equals(student.Email)).ToListAsync()
            };
            return PartialView(model);
        }

        //student dashboard
        public async Task<ActionResult> StudentDashboard()
        {
            var userIdUpper = userId.Trim().ToUpper();
            // Use projection to only load necessary fields to reduce payload
            var studentProjection = await _db.Students
                .Include(s => s.Session)
                .Where(s => s.Email.Trim().ToUpper() == userIdUpper)
                .Select(s => new
                {
                    s.StudentId,
                    s.MatricNo,
                    s.JambRegNo,
                    s.Email,
                    DepartmentName = s.Programme.Department.DeptName,
                    FacultyName = s.Programme.Department.Faculty.FacultyName,
                    CurrentLevel = s.Level.LevelName,
                    s.ProgrammeId,
                    s.SessionId,
                    s.Session.SessionName,
                    s.StudentStatus,
                    s.SchoolProgrammeId,
                    s.IsClearedAcademics,
                    s.IsClearedFaculty,
                    s.IsClearedDepartment,
                    ProgrammeName = s.Programme.ProgrammeName,
                    s.BloodGroup
                    // ... other properties needed
                }).FirstOrDefaultAsync();

            if (studentProjection == null)
            {
                return HttpNotFound("Student not found.");
            }

            // Create view model and set the properties from the projection
            var model = new StudentDashboardVM
            {
                StudentId = studentProjection.StudentId,
                MatricNo = studentProjection.MatricNo,
                JambReg = studentProjection.JambRegNo,
                PrimaryEmail = studentProjection.Email,
                DepartmentName = studentProjection.DepartmentName,
                FacultyName = studentProjection.FacultyName,
                CurrentLevel = studentProjection.CurrentLevel,
                SemesterName = _query.GetCurrentSemesterName(studentSchoolProgrammeId),
                SessionName = _query.GetCurrentSessionName(studentSchoolProgrammeId)
            };

            // Check and generate matric no if needed
            if (_IsPayedSchoolFee && studentProjection.StudentStatus.ToUpper() == StudentStatus.New_Student.ToString().ToUpper()
                && string.IsNullOrEmpty(studentProjection.MatricNo))
            {
                var editStudent = await _studentQuery.SaveAndGenerateMatricNo(studentProjection.StudentId);

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

                return RedirectToAction("LogOff", "Account", new
                {
                    url = "",
                    message = $"Your Matric No is {editStudent.MatricNo} and email is {editStudent.Email} " +
                    $"Login with this new UNIJOS Email as your username with initial password to proceed."
                });
            }

            // Use the previous studentProjection.StudentId to fetch other data like course registration, fees, etc.
            // This is where you would call methods like SetCourseRegistrationsAsync and SetFeeInformationAsync if they were available

            // Assuming _query.UserActivityStatistic() is an async method
            var userActivity = await _query.UserActivityStatistic();
            ViewBag.OnlineUser = userActivity.Item1;
            ViewBag.OnlineUserPercentage = userActivity.Item2;
            ViewBag.AllUsers = userActivity.Item3;
            ViewBag.ProgrammeName = studentProjection.ProgrammeName; // This assumes ProgrammeName is included in the projection
            ViewBag.DClearance = studentProjection.IsClearedDepartment;
            ViewBag.FClearance = studentProjection.IsClearedFaculty;
            ViewBag.AClearance = studentProjection.IsClearedAcademics;
            ViewBag.BloodGroup = studentProjection.BloodGroup;

            // Save changes if any were made
            await _db.SaveChangesAsync();

            return View(model);
        }

        // GET: Students/Details/5
        public async Task<ActionResult> Details(string id)
        {

            if (string.IsNullOrEmpty(id))
            {
                id = userId;
                var result = ConfirmSchoolFeeAndAcceptance();
                if (result != null)
                    return result;

                ViewBag.ApplicantType = _applicantType;
            }

            var student = await _db.Students.Include(i => i.Results.Select(x => x.Sessions)).Include(i => i.Level)
                                .Include(i => i.Programme.Department.Faculty).Include(i => i.SchoolProgramme).AsNoTracking()
                                .Where(x => x.StudentId.Trim().ToUpper().Equals(id.Trim().ToUpper()) ||
                                x.Email.Trim().ToUpper().Equals(id.Trim().ToUpper()))
                                .FirstOrDefaultAsync();
            var model = new StudentFullBioDataVm(_db, student);
            ViewBag.ResultName = model.OLevelResult.DistinctBy(x => x.ResultName).Select(s => s.ResultName);
            return View(model);
        }
        public async Task<ActionResult> StudentFullDetail(string id)
        {
            //var result = ConfirmSchoolFeeAndAcceptance();
            //if (result != null)
            //    return result;


            if (string.IsNullOrEmpty(id))
            {
                id = userId;
            }

            var student = await _db.Students.Include(i => i.Results.Select(x => x.Sessions)).Include(i => i.Level)
                                .Include(i => i.Programme.Department.Faculty).Include(i => i.SchoolProgramme)
                                .Include(i => i.Session).AsNoTracking()
                                .Where(x => x.StudentId.Trim().ToUpper().Equals(id.Trim().ToUpper()) ||
                                x.Email.Trim().ToUpper().Equals(id.Trim().ToUpper()))
                                .FirstOrDefaultAsync();

            string body = $"{student.FirstName} Print, Read n Edit your biodata then print 5 copies; Go to the " +
                               $" {student.Programme.Department.Faculty.FacultyName} boardroom for 1st clearance";

            var model = new StudentFullBioDataVm(_db, student);
            ViewBag.ResultName = model.OLevelResult.DistinctBy(x => x.ResultName).Select(s => s.ResultName);

            ViewBag.ApplicantType = _applicantType = new ApplicantTypeVm()
            {
                ApplicantType = student.SchoolProgramme.ProgrammeCategory,
                TimeType = student.SchoolProgramme.ProgrammeType
            };
            //await SMSClass.SendSMS("UNIJOS SIS", body, student.PhoneNumber); //EBULK SMS API
            return new ViewAsPdf(model);
            //return View(model);
        }

        //
        //[AllowAnonymous]
        //public async Task<ActionResult> ExternalStudentFullDetail(string id)
        //{
        //    //var result = ConfirmSchoolFeeAndAcceptance();
        //    //if (result != null)
        //    //    return result;


        //    if (string.IsNullOrEmpty(id))
        //    {
        //        id = userId;
        //        var result = ConfirmSchoolFeeAndAcceptance();
        //        if (result != null)
        //            return result;

        //        ViewBag.ApplicantType = _applicantType;
        //    }

        //    var student = await _db.Students.Include(i => i.Results.Select(x => x.Sessions)).Include(i => i.Level)
        //                        .Include(i => i.Programme.Department.Faculty).Include(i => i.SchoolProgramme).AsNoTracking()
        //                        .Where(x => x.StudentId.Trim().ToUpper().Equals(id.Trim().ToUpper()) ||
        //                        x.Email.Trim().ToUpper().Equals(id.Trim().ToUpper()))
        //                        .FirstOrDefaultAsync();
        //    var model = new StudentFullBioDataVm(_db, student);
        //    ViewBag.ResultName = model.OLevelResult.DistinctBy(x => x.ResultName).Select(s => s.ResultName);

        //    //await SMSClass.SendSMS("UNIJOS SIS", body, student.PhoneNumber); //EBULK SMS API
        //    return View(model);
        //    //return View(model);
        //}
        // GET: Students/Create
        [Authorize(Roles = RoleName.Admin)]
        public ActionResult Create()
        {

            var mystates = from State s in Enum.GetValues(typeof(State))
                           select new { ID = s, Name = s.ToString() };
            var religion = from Religion s in Enum.GetValues(typeof(Religion))
                           select new { ID = s, Name = s.ToString() };
            var mygender = from Gender s in Enum.GetValues(typeof(Gender))
                           select new { ID = s, Name = s.ToString() };
            var studentStaus = from StudentStatus s in Enum.GetValues(typeof(StudentStatus))
                               select new { ID = s, Name = s.ToString() };

            var lga = from LgaEnum s in Enum.GetValues(typeof(LgaEnum))
                      select new { ID = s, Name = s.ToString() };
            var maritalStatus = from Maritalstatus s in Enum.GetValues(typeof(Maritalstatus))
                                select new { ID = s, Name = s.ToString() };

            ViewBag.MaritalStatus = new SelectList(maritalStatus, "Name", "Name");

            ViewBag.StudentStatus = new SelectList(studentStaus, "Name", "Name");
            ViewBag.Lga = new SelectList(lga, "Name", "Name");
            ViewBag.Religion = new SelectList(religion, "Name", "Name");
            ViewBag.StateOfOrigin = new SelectList(mystates, "Name", "Name");
            ViewBag.Gender = new SelectList(mygender, "Name", "Name");
            ViewBag.LevelId = new SelectList(_db.Levels.AsNoTracking(), "LevelId", "LevelName");
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            ViewBag.ProgrammeId = new SelectList(_db.Programmes.AsNoTracking(), "ProgrammeId", "ProgrammeName");
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FancyName");

            return View();
        }

        // POST: Students/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = RoleName.Admin)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Student student)
        {
            if (ModelState.IsValid)
            {
                var studentId = DateTime.Now.Ticks;
                student.StudentId = $"{SchoolSetUp.CurrentSchoolName}{studentId}";
                _db.Students.Add(student);
                await _db.SaveChangesAsync();

                return RedirectToAction("Index");
            }
            var mystates = from State s in Enum.GetValues(typeof(State))
                           select new { ID = s, Name = s.ToString() };
            var physicallyChallenge = from PhysicallyChallenge s in Enum.GetValues(typeof(PhysicallyChallenge))
                                      select new { ID = s, Name = s.ToString() };
            ViewBag.PChallengedDetail = new SelectList(physicallyChallenge, "Name", "Name");

            var religion = from Religion s in Enum.GetValues(typeof(Religion))
                           select new { ID = s, Name = s.ToString() };
            var mygender = from Gender s in Enum.GetValues(typeof(Gender))
                           select new { ID = s, Name = s.ToString() };

            var studentStaus = from StudentStatus s in Enum.GetValues(typeof(StudentStatus))
                               select new { ID = s, Name = s.ToString() };
            var lga = from LgaEnum s in Enum.GetValues(typeof(LgaEnum))
                      select new { ID = s, Name = s.ToString() };
            var maritalStatus = from Maritalstatus s in Enum.GetValues(typeof(Maritalstatus))
                                select new { ID = s, Name = s.ToString() };

            ViewBag.MaritalStatus = new SelectList(maritalStatus, "Name", "Name");
            ViewBag.Lga = new SelectList(lga, "Name", "Name");
            ViewBag.StudentStatus = new SelectList(studentStaus, "Name", "Name");

            ViewBag.Religion = new SelectList(religion, "Name", "Name");

            ViewBag.StateOfOrigin = new SelectList(mystates, "Name", "Name");
            ViewBag.Gender = new SelectList(mygender, "Name", "Name");
            ViewBag.LevelId = new SelectList(_db.Levels.AsNoTracking(), "LevelId", "LevelName");
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            ViewBag.ProgrammeId = new SelectList(_db.Programmes.AsNoTracking(), "ProgrammeId", "ProgrammeName");
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FancyName");

            return View(student);
        }

        public ActionResult SavePassport(string id)

        {
            //checks if student has payed acceptance and redirect as appopriate...
            var result = ConfirmAcceptanceFee();
            if (result != null)
                return result;

            if (string.IsNullOrEmpty(id))
            {
                id = userId;
            }

            Student student = _db.Students.AsNoTracking().Where(x => x.Email.Equals(id)).FirstOrDefault();
            if (student == null)
            {
                return HttpNotFound();
            }

            ViewBag.applicantType = _applicantType.ApplicantType;
            ViewBag.studentStatus = student.StudentStatus;

            return PartialView(student);
        }

        [HttpPost]
        public async Task<ActionResult> SavePassport(Student model)
        {
            if (ModelState.IsValid)
            {
                var student = await _db.Students.FindAsync(model.StudentId);
                var UGStudentPassport = await _db.UtmeApplicants.Where(x => x.JambRegNo == student.JambRegNo).Select(x => x.Passport).FirstOrDefaultAsync();
                var PGStudentPassport = await _db.Applicants.Where(x => x.ApplicantId == student.JambRegNo).Select(x => x.ApplicatPassport).FirstOrDefaultAsync();

                if (student != null)
                {
                    if (model.Signature == null)
                    {
                        if (student.Signature == null) return new JsonResult { Data = new { status = false, message = "Please upload Signature again." } };
                    }
                    if (model.Passport == null)
                    {
                        // If the student is UG, get the passport from the utme applicant db. If PG and others, get the passport from the Applicants db
                        // If students happens to be Remedial that got admission into UG, get the passport from the Applicants db
                        if (student.SchoolProgrammeId == 1)
                        {
                            if (UGStudentPassport == null) { student.Passport = PGStudentPassport; }
                            else { student.Passport = UGStudentPassport; }

                        }
                        //else { student.Passport = PGStudentPassport; }
                    }
                    else if (model.Passport != null)
                    {
                        student.Passport = model.Passport;
                    }
                    if (model.Signature != null)
                    {
                        student.Signature = model.Signature;
                    }

                    _db.Entry(student).State = EntityState.Modified;
                    await _db.SaveChangesAsync();
                }
                if (User.IsInRole(RoleName.Student))
                {
                    return new JsonResult { Data = new { status = true, message = "Profile updated successfully" } };
                }
                return RedirectToAction("Index");
            }
            return new JsonResult { Data = new { status = false, message = "Fill all required fields" } };
            // return View(model);
        }

        public ActionResult StudentEdit(string id)
        {
            //checks if student has payed acceptance and redirect as appopriate...
            var result = ConfirmAcceptanceFee();
            if (result != null)
                return result;

            if (String.IsNullOrEmpty(id))
            {
                id = userId;
            }
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Student student = _db.Students.Include(x => x.SchoolProgramme).AsNoTracking().Where(x => x.Email.Equals(id)).FirstOrDefault();
            if (student == null)
            {
                return HttpNotFound();
            }
            var nationalities = from IndegineStatus s in Enum.GetValues(typeof(IndegineStatus))
                                select new { ID = s, Name = s.ToString() };
            var mystates = from State s in Enum.GetValues(typeof(State))
                           select new { ID = s, Name = s.ToString() };
            var religion = from Religion s in Enum.GetValues(typeof(Religion))
                           select new { ID = s, Name = s.ToString() };
            var mygender = from Gender s in Enum.GetValues(typeof(Gender))
                           select new { ID = s, Name = s.ToString() };

            var studentStaus = from StudentStatus s in Enum.GetValues(typeof(StudentStatus))
                               select new { ID = s, Name = s.ToString() };
            ViewBag.StudentStatus = new SelectList(studentStaus, "Name", "Name");
            var lga = from LgaEnum s in Enum.GetValues(typeof(LgaEnum))
                      select new { ID = s, Name = s.ToString() };
            var maritalStatus = from Maritalstatus s in Enum.GetValues(typeof(Maritalstatus))
                                select new { ID = s, Name = s.ToString() };
            var physicallyChallenge = from PhysicallyChallenge s in Enum.GetValues(typeof(PhysicallyChallenge))
                                      select new { ID = s, Name = s.ToString() };
            ViewBag.PChallengedDetail = new SelectList(physicallyChallenge, "Name", "Name");

            ViewBag.MaritalStatus = new SelectList(maritalStatus, "Name", "Name", student?.MaritalStatus);
            ViewBag.Lga = new SelectList(lga, "Name", "Name", student?.Lga);
            ViewBag.Religion = new SelectList(religion, "Name", "Name", student?.Religion);
            ViewBag.StateOfOrigin = new SelectList(mystates, "Name", "Name", student?.StateOfOrigin);
            ViewBag.Gender = new SelectList(mygender, "Name", "Name", student?.Gender);
            ViewBag.LevelId = new SelectList(_db.Levels.AsNoTracking(), "LevelId", "LevelName", student?.LevelId);
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName", student?.SessionId);
            ViewBag.ProgrammeId = new SelectList(_db.Programmes.AsNoTracking(), "ProgrammeId", "ProgrammeName", student?.ProgrammeId);
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FancyName", student?.SchoolProgrammeId);
            ViewBag.SchoolProgrammeCategory = student.SchoolProgramme.ProgrammeCategory;
            ViewBag.Nationality = new SelectList(nationalities, "Name", "Name", student?.Nationality);

            var model = new StudentEditViewModel
            {
                DateOfBirth = student.DateOfBirth,
                EnrollmentDate = student.EnrollmentDate,
                FirstName = student.FirstName,
                Gender = student.Gender,
                JambRegNo = student.JambRegNo,
                LastName = student.LastName,
                LevelId = student.LevelId,
                MatricNo = student.MatricNo,
                MiddleName = student.MiddleName,
                Nationality = student.Nationality,
                PhoneNumber = student.PhoneNumber,
                TownOfBirth = student.TownOfBirth,
                ProgrammeId = student.ProgrammeId,
                Religion = student.Religion,
                StateOfOrigin = student.StateOfOrigin,
                StudentId = student.StudentId,
                Hobby = student.Hobby,
                Lga = student.Lga,
                MaritalStatus = student.MaritalStatus,
                IsPhysicallyChallenged = Convert.ToBoolean(student.IsPhysicallyChallenged),
                PChallengedDetail = student.PChallengedDetail
            };
            return View(model);
        }


        public async Task<ActionResult> Edit(string id)
        {
            var result = ConfirmSchoolFee();
            if (result != null)
                return result;

            if (string.IsNullOrEmpty(id))
            {
                id = userId;
            }
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Student student = await _db.Students.AsNoTracking()
                                .Where(x => x.StudentId.Equals(id) || x.Email.Equals(id))
                                .FirstOrDefaultAsync();
            if (student == null)
            {
                return HttpNotFound();
            }
            var mystates = from State s in Enum.GetValues(typeof(State))
                           select new { ID = s, Name = s.ToString() };
            var religion = from Religion s in Enum.GetValues(typeof(Religion))
                           select new { ID = s, Name = s.ToString() };
            var mygender = from Gender s in Enum.GetValues(typeof(Gender))
                           select new { ID = s, Name = s.ToString() };

            var studentStaus = from StudentStatus s in Enum.GetValues(typeof(StudentStatus))
                               select new { ID = s, Name = s.ToString() };
            ViewBag.StudentStatus = new SelectList(studentStaus, "Name", "Name", student?.StudentStatus);
            var lga = from LgaEnum s in Enum.GetValues(typeof(LgaEnum))
                      select new { ID = s, Name = s.ToString() };
            var maritalStatus = from Maritalstatus s in Enum.GetValues(typeof(Maritalstatus))
                                select new { ID = s, Name = s.ToString() };

            ViewBag.MaritalStatus = new SelectList(maritalStatus, "Name", "Name", student?.MaritalStatus);
            ViewBag.Lga = new SelectList(lga, "Name", "Name", student?.Lga);
            ViewBag.Religion = new SelectList(religion, "Name", "Name", student?.Religion);
            ViewBag.StateOfOrigin = new SelectList(mystates, "Name", "Name", student?.StateOfOrigin);
            ViewBag.Gender = new SelectList(mygender, "Name", "Name", student?.Gender);
            ViewBag.LevelId = new SelectList(_db.Levels.AsNoTracking(), "LevelId", "LevelName", student?.LevelId);
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName", student?.SessionId);
            ViewBag.ProgrammeId = new SelectList(_db.Programmes.AsNoTracking(), "ProgrammeId", "ProgrammeName", student?.ProgrammeId);
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FancyName", student.SchoolProgrammeId);

            return View(student);
        }

        [Authorize(Roles = RoleName.Admin)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Student model)
        {
            if (ModelState.IsValid)
            {
                var student = await _db.Students.FindAsync(model.StudentId);
                model.IsPhysicallyChallenged = model.IsPhysicallyChallenged ?? false;

                if (student != null)
                {
                    if (User.IsInRole(RoleName.Student))
                    {
                        //if (model.Passport == null || model.Signature == null)
                        //{
                        //    if (student.Passport == null || student.Signature == null)
                        //    {
                        //        return new JsonResult { Data = new { status = false, message = "Please upload both Passport and Signature again." } };
                        //    }
                        //}

                        if (model.Passport != null)
                        {
                            student.Passport = model.Passport;
                        }
                        if (model.Signature != null)
                        {
                            student.Signature = model.Signature;
                        }
                        student.TownOfBirth = model.TownOfBirth;
                        student.StateOfOrigin = model.StateOfOrigin;
                        student.Nationality = model.Nationality;
                        student.Lga = model.Lga;
                        student.DateOfBirth = model.DateOfBirth;
                        student.PhoneNumber = model.PhoneNumber;
                        student.Gender = model.Gender;
                        student.MaritalStatus = model.MaritalStatus;
                        student.IsPhysicallyChallenged = model.IsPhysicallyChallenged;
                        if ((bool)model.IsPhysicallyChallenged)
                        {
                            student.PChallengedDetail = model.PChallengedDetail;
                        }
                        student.Hobby = model.Hobby;
                        student.Religion = model.Religion;
                    }
                    else
                    {
                        student.LastName = model.LastName;
                        student.FirstName = model.FirstName;
                        student.MiddleName = model.MiddleName;
                        student.PhoneNumber = model.PhoneNumber;
                        student.DateOfBirth = model.DateOfBirth;
                        student.SchoolProgrammeId = model.SchoolProgrammeId;
                        student.TownOfBirth = model.TownOfBirth;
                        student.StateOfOrigin = model.StateOfOrigin;
                        student.Nationality = model.Nationality;
                        student.CountryOfBirth = model.CountryOfBirth;
                        student.Gender = model.Gender;
                        student.ProgrammeId = model.ProgrammeId;
                        student.Passport = model.Passport;
                        student.StudentStatus = model.StudentStatus;
                        student.Lga = model.Lga;
                        student.MaritalStatus = model.MaritalStatus;
                        student.LevelId = model.LevelId;
                    }
                    _db.Entry(student).State = EntityState.Modified;
                    await _db.SaveChangesAsync();
                }
                if (User.IsInRole(RoleName.Student))
                {
                    return new JsonResult { Data = new { status = true, message = "Profile updated successfully" } };
                }
                return RedirectToAction("Index");
            }
            return new JsonResult { Data = new { status = false, message = "Fill all required fields" } };
            // return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> StudentFormEdit(StudentEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                var student = await _db.Students.FindAsync(model.StudentId);

                if (student != null)
                {
                    student.TownOfBirth = model.TownOfBirth;
                    student.StateOfOrigin = model.StateOfOrigin;
                    student.Nationality = model.Nationality;
                    student.Lga = model.Lga;
                    student.DateOfBirth = model.DateOfBirth;
                    student.PhoneNumber = model.PhoneNumber;
                    student.Gender = model.Gender;
                    student.MaritalStatus = model.MaritalStatus;
                    student.IsPhysicallyChallenged = model.IsPhysicallyChallenged;
                    if ((bool)model.IsPhysicallyChallenged)
                    {
                        student.PChallengedDetail = model.PChallengedDetail;
                    }
                    student.Hobby = model.Hobby;
                    student.Religion = model.Religion;

                    _db.Entry(student).State = EntityState.Modified;
                    await _db.SaveChangesAsync();
                }
                return new JsonResult { Data = new { status = true, message = "Profile updated successfully" } };
            }
            return new JsonResult { Data = new { status = false, message = "Fill all required fields" } };
            // return View(model);
        }

        [Authorize(Roles = RoleName.Admin + "," + RoleName.DatabaseAdmin + "," + RoleName.SuperAdmin)]
        public ActionResult UploadStudentLevel(string message)
        {
            ViewBag.Message = message;
            return View();
        }

        [Authorize(Roles = RoleName.Admin + "," + RoleName.DatabaseAdmin + "," + RoleName.SuperAdmin)]
        [HttpPost]
        public async Task<ActionResult> UploadStudentLevel(HttpPostedFileBase excelfile)
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
                //int recordCount = 0;
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
                    int requiredField = 1;


                    string validCheck = myExcel.ValidateExcel(noOfRow, workSheet, requiredField);
                    if (!validCheck.Equals("Success"))
                    {
                        //string row = "";
                        //string column = "";
                        string[] ssizes = validCheck.Split(' ');

                        string lineError = $"Line/Row number {ssizes[0]}  and column {ssizes[1]} is not rightly formatted, Please Check for anomalies ";
                        //ViewBag.LineError = lineError;
                        ViewBag.Message = lineError;
                        RedirectToAction("Index", "Guardians");
                    }

                    int count = 0;
                    var levelId = _db.Levels.AsNoTracking().Where(s => s.LevelName.Equals("100"))
                                    .Select(s => s.LevelId).FirstOrDefault();
                    for (int row = 2; row <= noOfRow; row++)
                    {
                        var matricNo = workSheet.Cells[row, 1].Value.ToString().Trim();
                        try
                        {
                            var student = await _db.Students.AsNoTracking().Where(x => x.MatricNo.Equals(matricNo))
                                                    .FirstOrDefaultAsync();
                            if (student != null)
                            {
                                student.LevelId = levelId;
                                _db.Entry(student).State = EntityState.Modified;
                                count += 1;

                                lastrecord = $"The last Updated record has the Last Name {student.LastName} and First Name {student.FirstName} with Student Id {student.StudentId}";
                            }


                        }
                        catch (Exception ex)
                        {
                            ViewBag.ErrorInfo = "Please Leave no column or row Empty/Blank";
                            ViewBag.ErrorMessage = ex.Message;
                            return View("ErrorException");
                        }
                    }
                    await _db.SaveChangesAsync();
                    message = $"You have successfully Uploaded {count} records...  and {lastrecord}";

                }
                return RedirectToAction("UploadChanges", "Students", new { message });
            }

            ViewBag.Error = $"File type is Incorrect <br/>";
            return View("Index");
        }

        //[Authorize(Roles = RoleName.Admin + "," + RoleName.DatabaseAdmin + "," + RoleName.SuperAdmin)]
        public ActionResult UploadStudentTransfer(string message)
        {
            ViewBag.Message = message;
            return View();
        }

        //[Authorize(Roles = RoleName.Admin + "," + RoleName.DatabaseAdmin + "," + RoleName.SuperAdmin)]
        [HttpPost]
        public async Task<ActionResult> UploadStudentTransfer(HttpPostedFileBase excelfile)
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
                //int recordCount = 0;
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
                    int requiredField = 1;


                    string validCheck = myExcel.ValidateExcel(noOfRow, workSheet, requiredField);
                    if (!validCheck.Equals("Success"))
                    {
                        //string row = "";
                        //string column = "";
                        string[] ssizes = validCheck.Split(' ');

                        string lineError = $"Line/Row number {ssizes[0]}  and column {ssizes[1]} is not rightly formatted, Please Check for anomalies ";
                        //ViewBag.LineError = lineError;
                        ViewBag.Message = lineError;
                        RedirectToAction("Index", "Guardians");
                    }

                    int count = 0;
                    var levelId = _db.Levels.AsNoTracking().Where(s => s.LevelName.Equals("100"))
                                    .Select(s => s.LevelId).FirstOrDefault();
                    for (int row = 2; row <= noOfRow; row++)
                    {
                        var matricNo = workSheet.Cells[row, 1].Value.ToString().Trim().ToUpper();
                        try
                        {
                            var student = await _db.Students.AsNoTracking().Where(x => x.MatricNo.Equals(matricNo))
                                                    .FirstOrDefaultAsync();
                            if (student != null)
                            {
                                student.IsClearedAcademics = false;
                                student.IsClearedFaculty = false;
                                student.IsClearedDepartment = false;
                                _db.Entry(student).State = EntityState.Modified;
                                count += 1;

                                lastrecord = $"The last Updated record has the Last Name {student.LastName} and First Name {student.FirstName} with Student Id {student.StudentId}";
                            }


                        }
                        catch (Exception ex)
                        {
                            ViewBag.ErrorInfo = "Please Leave no column or row Empty/Blank";
                            ViewBag.ErrorMessage = ex.Message;
                            return View("ErrorException");
                        }
                    }
                    await _db.SaveChangesAsync();
                    message = $"You have successfully Uploaded {count} records...  and {lastrecord}";

                }
                return RedirectToAction("UploadStudentTransfer", "Students", new { message });
            }

            ViewBag.Error = $"File type is Incorrect <br/>";
            return View("Index");
        }

        [Authorize(Roles = RoleName.Admin + "," + RoleName.DatabaseAdmin + "," + RoleName.SuperAdmin)]
        public ActionResult DeleteStudentDuplicate(string message)
        {
            ViewBag.Message = message;
            return View();
        }

        [Authorize(Roles = RoleName.Admin + "," + RoleName.DatabaseAdmin + "," + RoleName.SuperAdmin)]
        [HttpPost]
        public async Task<ActionResult> DeleteStudentDuplicate(HttpPostedFileBase excelfile)
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
                //int recordCount = 0;
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
                    int requiredField = 1;


                    string validCheck = myExcel.ValidateExcel(noOfRow, workSheet, requiredField);
                    if (!validCheck.Equals("Success"))
                    {
                        string[] ssizes = validCheck.Split(' ');
                        string lineError = $"Line/Row number {ssizes[0]}  and column {ssizes[1]} is not rightly formatted, Please Check for anomalies ";
                        ViewBag.Message = lineError;
                        RedirectToAction("Index", "Guardians");
                    }

                    int count = 0;
                    int deleteCount = 0;

                    for (int row = 2; row <= noOfRow; row++)
                    {
                        var matricNo = workSheet.Cells[row, 1].Value.ToString().Trim();
                        var programeId = Int32.Parse(workSheet.Cells[row, 2].Value.ToString().Trim());
                        try
                        {
                            var students = await _db.Students.Include(x => x.Programme).AsNoTracking()
                                                    .Where(x => x.JambRegNo.Trim().ToUpper().Equals(matricNo.ToUpper()) && x.Programme.ProgrammeId.Equals(programeId)).ToListAsync();

                            if (students.Count() >= 1)
                            {
                                bool checkIfRegistered = students.Any(x => x.Active.Equals(true));
                                foreach (var student in students)
                                {
                                    if (checkIfRegistered || count != 0)
                                    {
                                        if (student.Active)
                                        {
                                            var checkStudent = await _db.SchoolFeePayments.FirstOrDefaultAsync(x => x.StudentId.Equals(student.StudentId));
                                            if (checkStudent != null)
                                            {
                                                student.Active = true;
                                                _db.Entry(student).State = EntityState.Modified;
                                                lastrecord = $"The last Updated record has the Last Name {student.LastName} and First Name {student.FirstName} with Student Id {student.StudentId}";
                                            }
                                            else
                                            {
                                                _db.Entry(student).State = EntityState.Deleted;
                                                deleteCount += 1;
                                            }
                                        }
                                        if (student.Active && count > 0)
                                        {
                                            var checkStudent = await _db.SchoolFeePayments.FirstOrDefaultAsync(x => x.StudentId.Equals(student.StudentId));
                                            if (checkStudent != null)
                                            {
                                                _db.Entry(checkStudent).State = EntityState.Deleted;
                                                _db.SaveChanges();
                                            }
                                            var studentDetails = await _db.StudentPaymentDetails.Where(x => x.StudentId.Equals(student.StudentId)).ToListAsync();
                                            foreach (var details in studentDetails)
                                            {
                                                _db.Entry(details).State = EntityState.Deleted;
                                                _db.SaveChanges();
                                            }
                                            _db.Entry(student).State = EntityState.Deleted;
                                            deleteCount += 1;
                                        }
                                    }
                                    count += 1;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            ViewBag.ErrorInfo = "Please Leave no column or row Empty/Blank";
                            ViewBag.ErrorMessage = ex.Message;
                            return View("ErrorException");
                        }
                    }
                    await _db.SaveChangesAsync();
                    message = $"You have successfully deleted {deleteCount} records...  and {lastrecord}";
                }
                return RedirectToAction("DeleteStudentDuplicate", "Students", new { message });
            }

            ViewBag.Error = $"File type is Incorrect <br/>";
            return View("Index");
        }


        [Authorize(Roles = RoleName.Admin + "," + RoleName.DatabaseAdmin + "," + RoleName.SuperAdmin)]
        public ActionResult DeleteUploadStudents(string message)
        {
            ViewBag.Message = message;
            return View();
        }

        [Authorize(Roles = RoleName.Admin + "," + RoleName.DatabaseAdmin + "," + RoleName.SuperAdmin)]
        [HttpPost]
        public async Task<ActionResult> DeleteUploadStudents(HttpPostedFileBase excelfile)
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
                //int recordCount = 0;
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
                    int requiredField = 1;


                    string validCheck = myExcel.ValidateExcel(noOfRow, workSheet, requiredField);
                    if (!validCheck.Equals("Success"))
                    {
                        //string row = "";
                        //string column = "";
                        string[] ssizes = validCheck.Split(' ');

                        string lineError = $"Line/Row number {ssizes[0]}  and column {ssizes[1]} is not rightly formatted, Please Check for anomalies ";
                        //ViewBag.LineError = lineError;
                        ViewBag.Message = lineError;
                        RedirectToAction("DeleteUploadStudents");
                    }

                    int count = 0;
                    for (int row = 2; row <= noOfRow; row++)
                    {
                        var matricNo = workSheet.Cells[row, 1].Value.ToString().Trim();
                        try
                        {
                            var student = await _db.Students.Where(x => x.JambRegNo.Equals(matricNo))
                                                    .ToListAsync();
                            if (student.Count != null)
                            {
                                //var oldUser = _db.Users.AsNoTracking().FirstOrDefault(x => x.Id.Equals(student.StudentId) || x.Id.Equals(matricNo)
                                //                                                           || x.Email.Equals(student.Email));
                                //if (oldUser != null)
                                //{
                                //    _db.Entry(oldUser).State = EntityState.Deleted;
                                //}
                                var removeStudent = await _db.Students.Where(x => x.JambRegNo.Equals(matricNo)).FirstOrDefaultAsync();
                                removeStudent.SessionId = 25;
                                //_db.Entry(removeStudent).State = EntityState.Deleted;
                                _db.Entry(removeStudent).State = EntityState.Modified;
                                count += 1;

                                //var lastStudent = await _db.Students.Where(x => x.JambRegNo.Equals(matricNo)).FirstOrDefaultAsync();

                                //if (lastStudent != null)
                                //{
                                //    lastStudent.SessionId = 24;
                                //    lastStudent.ProgrammeId = 337;
                                //    _db.Entry(lastStudent).State = EntityState.Modified;
                                //}
                            }
                            //else
                            //{
                            //    var lastStudent = await _db.Students.Where(x => x.JambRegNo.Equals(matricNo)).FirstOrDefaultAsync();

                            //    if (lastStudent != null)
                            //    {
                            //        lastStudent.SessionId = 24;
                            //        lastStudent.ProgrammeId = 337;
                            //        _db.Entry(lastStudent).State = EntityState.Modified;
                            //    }

                            //}
                            lastrecord = $"The last Updated record has the Last Name {count}";



                        }
                        catch (Exception ex)
                        {
                            ViewBag.ErrorInfo = "Please Leave no column or row Empty/Blank";
                            ViewBag.ErrorMessage = ex.Message;
                            return View("ErrorException");
                        }
                    }
                    await _db.SaveChangesAsync();
                    message = $"You have successfully Uploaded {count} records...  and {lastrecord}";

                }
                return RedirectToAction("DeleteUploadStudents", "Students", new { message });
            }

            ViewBag.Error = $"File type is Incorrect <br/>";
            return View("Index");
        }


        public ActionResult GetLgaList(string codeId)
        {
            string result = string.Empty;
            var item = new List<string>();
            var getLgaList = PopulateLga();
            codeId = codeId.ToUpper();

            if (!string.IsNullOrEmpty(codeId))
            {
                if (getLgaList.ContainsKey(codeId))
                {
                    item = getLgaList[codeId];
                }

                JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                result = javaScriptSerializer.Serialize(item);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            result = "Pick a State";
            return Json(result, JsonRequestBehavior.AllowGet);


        }

        public ActionResult GetDeptList(int codeId)
        {
            var item = new List<DeptDropVm>
            {
                new DeptDropVm
                {
                    DepartmentId = null,
                    DeptName = ""
                }
            };
            item.AddRange(_db.Departments.AsNoTracking().Where(x => x.FacultyId.Equals(codeId))
                .Select(s => new DeptDropVm
                {
                    DepartmentId = s.DepartmentId,
                    DeptName = s.DeptName
                }).OrderBy(x => x.DeptName));

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            var result = javaScriptSerializer.Serialize(item);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetProgrammeList(int codeId)
        {
            var item = _db.Programmes.Include(i => i.Department).AsNoTracking()
                .Where(x => x.Department.DepartmentId.Equals(codeId))
                .Select(s => new { s.ProgrammeId, s.ProgrammeName }).OrderBy(x => x.ProgrammeName);

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            var result = javaScriptSerializer.Serialize(item);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetProgrammeList2(int codeId)
        {
            var item = new List<ProgrammeDropVm>
            {
                new ProgrammeDropVm
                {
                    ProgrammeId = null,
                    ProgrammeName = ""
                }
            };

            item.AddRange(_db.Programmes.Include(i => i.Department).AsNoTracking().Where(x => x.Department.DepartmentId.Equals(codeId))
                .Select(s => new ProgrammeDropVm
                {
                    ProgrammeId = s.ProgrammeId,
                    ProgrammeName = s.ProgrammeName
                }).OrderBy(x => x.ProgrammeName));

            //var item = _db.Programmes.Include(i => i.Department).AsNoTracking()
            //    .Where(x => x.Department.DepartmentId.Equals(codeId))
            //    .Select(s => new { s.ProgrammeId, s.ProgrammeName }).OrderBy(x => x.ProgrammeName);

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            var result = javaScriptSerializer.Serialize(item);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetCourseList(int codeId)
        {
            var item = _db.Courses.Include(i => i.Programme).AsNoTracking()
                .Where(x => x.Programme.ProgrammeId.Equals(codeId))
                .Select(s => new { s.CourseId, s.CourseName }).OrderBy(x => x.CourseName);

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            var result = javaScriptSerializer.Serialize(item);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetCourseCodeList(int codeId)
        {
            var item = _db.Courses.Include(i => i.Programme).AsNoTracking()
                .Where(x => x.Programme.ProgrammeId.Equals(codeId))
                .Select(s => new { s.CourseId, s.CourseCode });

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            var result = javaScriptSerializer.Serialize(item);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        // GET: Students/Delete/5
        [Authorize(Roles = RoleName.Admin + "," + RoleName.SuperAdmin)]
        public async Task<PartialViewResult> Delete(string id)
        {
            var student = await _db.Students.FindAsync(id);
            return PartialView(student);
        }

        // Edit this part based on the constraints tied to the student to be deleted
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = RoleName.Admin + "," + RoleName.SuperAdmin)]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            //bool status = false;
            //string message = string.Empty;
            //var student = await _db.Students.FindAsync(id);
            //if (student != null)
            //{
            //    _db.Students.Remove(student);
            //    await _db.SaveChangesAsync();
            //    status = true;
            //    message = "Student Deleted Successfully...";
            //    return new JsonResult { Data = new { status, message } };
            //}

            //return new JsonResult { Data = new { status = false, message = $"{id } not deleted" } };

            bool status = false;
            string message = string.Empty;
            var student = await _db.Students.FindAsync(id);
            // var SpD = _db.SchoolFeePayments.Where(x => x.StudentId.Equals(id)).ToList();
            var IdCardPayments = _db.IdCardPayments.Where(x => x.StudentId.Equals(id)).ToList();
            foreach (var item in IdCardPayments)
            {
                _db.Entry(item).State = EntityState.Deleted;
            }
            var ChangeDetailPayments = _db.ChangeDetailPayments.Where(x => x.StudentId.Equals(id)).ToList();
            foreach (var item in ChangeDetailPayments)
            {
                _db.Entry(item).State = EntityState.Deleted;
            }
            var StudentPaymentDetails = _db.StudentPaymentDetails.Where(x => x.StudentId.Equals(id)).ToList();
            foreach (var item in StudentPaymentDetails)
            {
                //item.StudentId = "UNIJOS637666998826199989//
                _db.Entry(item).State = EntityState.Deleted;
            }
            await _db.SaveChangesAsync();

            if (student != null)
            {
                _db.Students.Remove(student);
                await _db.SaveChangesAsync();
                status = true;
                message = "Student Deleted Successfully...";
                return new JsonResult { Data = new { status, message } };
            }

            return new JsonResult { Data = new { status = false, message = $"{id } not deleted" } };
        }

        [AllowAnonymous]
        public async Task<ActionResult> RenderImage(string studentId)



        {
            //Student student = await _db.Students.FindAsync(studentId);
            var student = await _db.Students.Include(x => x.SchoolProgramme).AsNoTracking().Where(x => x.StudentId == studentId || x.JambRegNo == studentId)
                                    .Select(x => new { x.JambRegNo, x.SchoolProgramme.ProgrammeCategory, x.Passport }).FirstOrDefaultAsync();

            // Get the passport the student used during forms application
            var UGStudentPassport = await _db.UtmeApplicants.AsNoTracking().Where(x => x.JambRegNo == student.JambRegNo).Select(x => x.Passport).FirstOrDefaultAsync();

            // For undergraduate students that have passport in UtmeApplicants table
            if (student.ProgrammeCategory == "UnderGraduate" && UGStudentPassport != null)
            {
                byte[] photo = UGStudentPassport;
                return File(photo, "image/png");
            }

            // For UG students that have no passport in UtmeApplicants db
            if (UGStudentPassport == null)
            {
                byte[] photo = student.Passport;
                return File(photo, "image/png");
            }

            byte[] photoBack = student.Passport;
            return File(photoBack, "image/png");

            //byte[] photoBack = student.Passport;
        }


        [AllowAnonymous]
        public async Task<ActionResult> RenderSignature(string studentId)
        {
            Student student = await _db.Students.FindAsync(studentId);

            byte[] photoBack = student.Signature;

            return File(photoBack, "image/png");
        }
        [AllowAnonymous]
        public async Task<ActionResult> RenderAnyImage(string userId)
        {
            Student student = await _db.Students.FindAsync(userId);
            byte[] photoBack = null;
            if (student != null)
            {
                photoBack = student.Passport;
            }
            var staff = await _db.Staffs.FindAsync(userId);
            if (staff != null)
            {
                photoBack = staff.Passport;
            }

            return File(photoBack, "image/png");
        }

        [AllowAnonymous]
        public async Task<string> DisplayName(string studentId)
        {
            Student student = await _db.Students.FindAsync(studentId);

            return $"{student?.LastName} {student?.FirstName}";
        }

        [AllowAnonymous]
        public async Task<string> DisplayAnyName(string userId)
        {
            Student student = await _db.Students.FindAsync(userId);
            if (student != null)
            {
                return $"{student?.LastName} {student?.FirstName}";
            }
            var staff = await _db.Staffs.FindAsync(userId);
            return $"{staff?.LastName} {staff?.FirstName}";


        }

        public ActionResult UploadStudent()
        {
            return View();
        }


        [HttpPost]
        public async Task<ActionResult> UploadStudent(HttpPostedFileBase excelfile)
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
                    int requiredField = 16;
                    var utmeUploadError = new List<StudentUtmeUploadErrorVm>();
                    bool hasUtmeUploadError = false;


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
                        var studentId = DateTime.Now.Ticks;
                        string matricNo = workSheet.Cells[row, 1].Value.ToString().Trim();
                        string code = workSheet.Cells[row, 12].Value.ToString().Trim();
                        string level = workSheet.Cells[row, 13].Value.ToString().Trim();
                        string studentType = workSheet.Cells[row, 14].Value.ToString().Trim();
                        string sessionName = workSheet.Cells[row, 15].Value.ToString().Trim();
                        string modeOfEntry = workSheet.Cells[row, 16].Value.ToString().Trim();

                        var lastName = workSheet.Cells[row, 3].Value.ToString().Trim();
                        var firstName = workSheet.Cells[row, 4].Value.ToString().Trim();
                        var middleName = workSheet.Cells[row, 5].Value.ToString().Trim();
                        var email = workSheet.Cells[row, 6].Value.ToString().Trim();


                        if (lastName.Trim().Equals("."))
                        {
                            lastName = "";
                        }
                        if (middleName.Trim().Equals("."))
                        {
                            middleName = "";
                        }
                        if (firstName.Trim().Equals("."))
                        {
                            middleName = "";
                        }


                        var programmeCode = await _db.Programmes.AsNoTracking()
                                            .Where(x => x.ProgrammeCode.ToUpper().Equals(code.ToUpper()))
                                            .FirstOrDefaultAsync();
                        var schoolProgramme = await _db.SchoolProgrammes.AsNoTracking()
                                            .Where(x => x.SchoolProgrammeCode.ToUpper().Equals(studentType.ToUpper()))
                                            .FirstOrDefaultAsync();
                        var uploadedSessionId = await _db.Sessions.AsNoTracking().Where(x => x.SessionName.ToUpper().Equals(sessionName.ToUpper()))
                                                   .FirstOrDefaultAsync();
                        var levelId = _db.Levels.AsNoTracking().Where(s => s.LevelName.Equals(level))
                                            .Select(s => s.LevelId.ToString()).FirstOrDefault();


                        if (levelId == null)
                        {
                            ViewBag.ErrorInfo = "Please Leave no column or row Empty/Blank";
                            ViewBag.ErrorMessage = $" The \"{level}\" level specified in the excel doesn't exist on the portal. " +
                                                   $"Please add the level first before uploading or correct the excel sheet...";
                            return View("ErrorException");
                        }
                        if (programmeCode == null)
                        {
                            ViewBag.ErrorInfo = "Please Leave no column or row Empty/Blank";
                            ViewBag.ErrorMessage = $" The department Option \"{code}\"  specified in the excel at row {row} doesn't exist on the portal. " +
                                                   $"Please add the Department Option first before uploading or correct the excel sheet...";
                            return View("ErrorException");
                        }
                        if (uploadedSessionId == null)
                        {
                            ViewBag.ErrorInfo = "Please Leave no column or row Empty/Blank";
                            ViewBag.ErrorMessage = $" The Session Name \"{sessionName}\"  specified in the excel at row {row} doesn't exist on the portal. " +
                                                   $"Please add the Session Name first before uploading or correct the excel sheet...";
                            return View("ErrorException");
                        }

                        if (schoolProgramme == null)
                        {
                            ViewBag.ErrorInfo = $"This {studentType} school programme Code  at row {row} doesn't exist in the system.";
                            ViewBag.ErrorMessage = "Please check the Student Programme type spelling very well ";
                            return View("ErrorException");
                        }
                        var checkStudent = _db.Students.FirstOrDefault(x => x.MatricNo.Trim().ToUpper().Equals(matricNo.ToUpper()));
                        if (checkStudent == null)
                        {
                            try
                            {
                                var student = new Student()
                                {
                                    StudentId = $"{SchoolSetUp.CurrentSchoolName}{studentId}",
                                    //MatricNo = workSheet.Cells[row, 1].Value.ToString().Trim(),
                                    JambRegNo = workSheet.Cells[row, 2].Value.ToString().Trim(),
                                    FirstName = firstName,
                                    MiddleName = middleName,
                                    LastName = lastName,
                                    Email = email,
                                    StateOfOrigin = workSheet.Cells[row, 7].Value.ToString().Trim(),
                                    Nationality = workSheet.Cells[row, 8].Value.ToString().Trim(),
                                    DateOfBirth = DateTime.Parse(workSheet.Cells[row, 9].Value.ToString().Trim()),
                                    Gender = workSheet.Cells[row, 10].Value.ToString().Trim(),
                                    EnrollmentDate = DateTime.Parse(workSheet.Cells[row, 11].Value.ToString().Trim()),
                                    ProgrammeId = programmeCode.ProgrammeId,
                                    LevelId = int.Parse(levelId),
                                    StudentStatus = StudentStatus.Returning.ToString(),
                                    SchoolProgrammeId = schoolProgramme.SchoolProgrammeId,
                                    SessionId = uploadedSessionId.SessionId,
                                    ModeOfEntry = modeOfEntry,
                                    IsClearedAcademics = false,
                                    IsClearedDepartment = false,
                                    IsClearedFaculty = false,
                                };

                                _db.Students.Add(student);
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
                        else
                        {
                            utmeUploadError.Add(new StudentUtmeUploadErrorVm { JambRegNo = matricNo, Row = row });
                            hasUtmeUploadError = true;
                        }

                    }
                    await _db.SaveChangesAsync();
                    if (hasUtmeUploadError)
                    {
                        ViewBag.ErrorInfo = $"These applicants wasn't found on the system";
                        ViewBag.ErrorMessage = $"You have successfully Uploaded {recordCount} records...";
                        return View("ErrorException", utmeUploadError);
                    }
                    message = $"You have successfully Uploaded {recordCount} records...  and {lastrecord}";
                }
                return RedirectToAction("Index", "Students", new { message });
            }

            ViewBag.Error = $"File type is Incorrect <br/>";
            return View("Index");
        }


        public ActionResult UploadRemedialAdmission()
        {
            return View();
        }


        [HttpPost]
        public async Task<ActionResult> UploadRemedialAdmission(HttpPostedFileBase excelfile)
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
                    int requiredField = 15;


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
                        var studentId = DateTime.Now.Ticks;
                        string code = workSheet.Cells[row, 11].Value.ToString().Trim();
                        string level = workSheet.Cells[row, 12].Value.ToString().Trim();
                        string studentType = workSheet.Cells[row, 13].Value.ToString().Trim();
                        string sessionName = workSheet.Cells[row, 14].Value.ToString().Trim();
                        string modeOfEntry = workSheet.Cells[row, 15].Value.ToString().Trim();

                        var lastName = workSheet.Cells[row, 2].Value.ToString().Trim();
                        var firstName = workSheet.Cells[row, 3].Value.ToString().Trim();
                        var middleName = workSheet.Cells[row, 4].Value.ToString().Trim();
                        var email = workSheet.Cells[row, 5].Value.ToString().Trim();
                        var jambRegNo = workSheet.Cells[row, 1].Value.ToString();


                        if (lastName.Trim().Equals("."))
                        {
                            lastName = "";
                        }
                        if (middleName.Trim().Equals("."))
                        {
                            middleName = "";
                        }
                        if (firstName.Trim().Equals("."))
                        {
                            middleName = "";
                        }


                        var programmeCode = await _db.Programmes.AsNoTracking()
                                            .Where(x => x.ProgrammeCode.ToUpper().Equals(code.ToUpper()))
                                            .FirstOrDefaultAsync();
                        var schoolProgramme = await _db.SchoolProgrammes.AsNoTracking()
                                            .Where(x => x.SchoolProgrammeCode.ToUpper().Equals(studentType.ToUpper()))
                                            .FirstOrDefaultAsync();
                        var uploadedSessionId = await _db.Sessions.AsNoTracking().Where(x => x.SessionName.ToUpper().Equals(sessionName.ToUpper()))
                                                   .FirstOrDefaultAsync();
                        var levelId = _db.Levels.AsNoTracking().Where(s => s.LevelName.Equals(level))
                                            .Select(s => s.LevelId.ToString()).FirstOrDefault();
                        if (levelId == null)
                        {
                            ViewBag.ErrorInfo = "Please Leave no column or row Empty/Blank";
                            ViewBag.ErrorMessage = $" The \"{level}\" level specified in the excel doesn't exist on the portal. " +
                                                   $"Please add the level first before uploading or correct the excel sheet...";
                            return View("ErrorException");
                        }
                        if (programmeCode == null)
                        {
                            ViewBag.ErrorInfo = "Please Leave no column or row Empty/Blank";
                            ViewBag.ErrorMessage = $" The department Option \"{code}\"  specified in the excel at row {row} doesn't exist on the portal. " +
                                                   $"Please add the Department Option first before uploading or correct the excel sheet...";
                            return View("ErrorException");
                        }
                        if (uploadedSessionId == null)
                        {
                            ViewBag.ErrorInfo = "Please Leave no column or row Empty/Blank";
                            ViewBag.ErrorMessage = $" The Session Name \"{sessionName}\"  specified in the excel at row {row} doesn't exist on the portal. " +
                                                   $"Please add the Session Name first before uploading or correct the excel sheet...";
                            return View("ErrorException");
                        }

                        if (schoolProgramme == null)
                        {
                            ViewBag.ErrorInfo = $"This {studentType} school programme Code  at row {row} doesn't exist in the system.";
                            ViewBag.ErrorMessage = "Please check the Student Programme type spelling very well ";
                            return View("ErrorException");
                        }
                        var studentExit = _db.Students.Any(x => x.JambRegNo.Trim().ToUpper().Equals(jambRegNo.Trim().ToUpper()));
                        if (!studentExit)
                        {

                            try
                            {
                                var student = new Student()
                                {
                                    StudentId = $"{SchoolSetUp.CurrentSchoolName}{studentId}",
                                    JambRegNo = workSheet.Cells[row, 1].Value.ToString().Trim(),
                                    FirstName = firstName,
                                    MiddleName = middleName,
                                    LastName = lastName,
                                    Email = email,
                                    StateOfOrigin = workSheet.Cells[row, 6].Value.ToString().Trim(),
                                    Nationality = workSheet.Cells[row, 7].Value.ToString().Trim(),
                                    DateOfBirth = DateTime.Parse(workSheet.Cells[row, 8].Value.ToString().Trim()),
                                    Gender = workSheet.Cells[row, 9].Value.ToString().Trim(),
                                    EnrollmentDate = DateTime.Parse(workSheet.Cells[row, 10].Value.ToString().Trim()),
                                    ProgrammeId = programmeCode.ProgrammeId,
                                    LevelId = int.Parse(levelId),
                                    StudentStatus = StudentStatus.New_Student.ToString(),
                                    SchoolProgrammeId = schoolProgramme.SchoolProgrammeId,
                                    SessionId = uploadedSessionId.SessionId,
                                    ModeOfEntry = modeOfEntry,
                                    IsRemedialStudent = false,
                                };

                                _db.Students.Add(student);
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
                    }
                    await _db.SaveChangesAsync();
                    message = $"You have successfully Uploaded {recordCount} records...  and {lastrecord}";
                    TempData["UserMessage"] = message;
                    TempData["Title"] = "Success.";
                }
                return RedirectToAction("Index", "Students");
            }

            ViewBag.Error = $"File type is Incorrect <br/>";
            return View("Index");
        }


        [Authorize(Roles = RoleName.Admin + "," + RoleName.DatabaseAdmin + "," + RoleName.SuperAdmin + "," + RoleName.AdmissionUploadOfficer)]
        public ActionResult UploadUtmeAdmission(string message)
        {
            ViewBag.Message = message;
            return View();
        }

        [Authorize(Roles = RoleName.Admin + "," + RoleName.DatabaseAdmin + "," + RoleName.SuperAdmin + "," + RoleName.AdmissionUploadOfficer)]
        [HttpPost]
        public async Task<ActionResult> UploadUtmeAdmission(HttpPostedFileBase excelfile)
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
                    int requiredField = 4;


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

                    int count = 0;
                    var ugSchoolProgrammeId = await GetUndergraduateSchoolProgrammeId();
                    var currentSessionId = _query.GetCurrentSessionId(ugSchoolProgrammeId);
                    var utmeUploadError = new List<StudentUtmeUploadErrorVm>();
                    bool hasUtmeUploadError = false;

                    var programmes = GetAllProgramme();
                    var levels = GetLevelList();

                    //var emailList = new List<string>();
                    for (int row = 2; row <= noOfRow; row++)
                    {
                        var studentId = DateTime.Now.Ticks;
                        var jambNo = workSheet.Cells[row, 1].Value.ToString().Trim();
                        string code = workSheet.Cells[row, 2].Value.ToString().Trim().ToUpper();
                        string level = workSheet.Cells[row, 3].Value.ToString().Trim().ToUpper();
                        string sessionName = workSheet.Cells[row, 4].Value.ToString().Trim();

                        //var checkstudent = _db.Students.AsNoTracking().Any(x => x.JambRegNo.ToUpper().Equals(jambNo.ToUpper()));
                        //if (checkstudent)
                        //{
                        //    ViewBag.ErrorInfo = "Multiple Jamb No Detected";
                        //    ViewBag.ErrorMessage = $" The \"{jambNo}\" Jamb No specified in the excel at row {row} already exist for a student on the portal. " +
                        //                           $"Please check carefully before uploading and correct the excel sheet...";
                        //    return View("ErrorException");
                        //}

                        var programmeCode = programmes.FirstOrDefault(x => x.ProgrammeCode.Trim().ToUpper().Equals(code));
                        var levelId = levels.FirstOrDefault(x => x.LevelName.Trim().ToUpper().Equals(level));

                        if (levelId == null)
                        {
                            ViewBag.ErrorInfo = "Please Leave no column or row Empty/Blank";
                            ViewBag.ErrorMessage = $" The \"{level}\" level specified in the excel doesn't exist on the portal. " +
                                                   $"Please add the level first before uploading or correct the excel sheet...";
                            return View("ErrorException");
                        }
                        if (programmeCode == null)
                        {
                            ViewBag.ErrorInfo = "Please Leave no column or row Empty/Blank";
                            ViewBag.ErrorMessage = $" The department Option \"{code}\"  specified in the excel at row {row} doesn't exist on the portal. " +
                                                   $"Please add the Department Option first before uploading or correct the excel sheet...";
                            return View("ErrorException");
                        }

                        try
                        {
                            var studentExit = _db.Students.Any(x => x.JambRegNo.Trim().ToUpper().Equals(jambNo.ToUpper()));
                            if (!studentExit)
                            {
                                var student = new Student();

                                var applicant = await _db.UtmeApplicants.AsNoTracking().Where(x => x.JambRegNo.Trim().ToUpper().Equals(jambNo.Trim().ToUpper())
                                                && x.HasRegistered.Equals(true)).FirstOrDefaultAsync();
                                //var applicantPayment = await _db.ApplicantPayments.AsNoTracking().Where(x => x.JambRegNo.Trim().ToUpper().Equals(jambNo.ToUpper())
                                //                        || x.ApplicantEmail.Trim().ToUpper().Equals(applicant.Email.Trim().ToUpper()) 
                                //                        && x.IsPayed.Equals(true)).FirstOrDefaultAsync();

                                if (applicant != null)
                                {
                                    student.StudentId = $"{SchoolSetUp.CurrentSchoolName}{studentId}";
                                    student.Email = applicant.Email;
                                    student.Active = true;
                                    student.StudentStatus = StudentStatus.New_Student.ToString();
                                    student.PhoneNumber = applicant.PhoneNumber;
                                    student.JambRegNo = applicant.JambRegNo;
                                    student.FirstName = applicant.FirstName;
                                    student.MiddleName = applicant.MiddleName;
                                    student.LastName = applicant.Surname;
                                    student.StateOfOrigin = applicant.StateOfOrigin;
                                    student.DateOfBirth = (DateTime)applicant.DateOfBirth;
                                    student.Gender = applicant.Gender;
                                    student.EnrollmentDate = DateTime.Now;
                                    student.ProgrammeId = programmeCode.ProgrammeId;
                                    student.LevelId = levelId.LevelId;
                                    student.SchoolProgrammeId = ugSchoolProgrammeId;
                                    student.SessionId = currentSessionId;
                                    student.Passport = applicant.Passport;
                                    student.Nationality = "Nigeria";
                                    student.ModeOfEntry = applicant.IsDirectEntry.Equals(true) ? "DE" : "UTME"; // Checking for Mode of Entry

                                    _db.Students.Add(student);
                                    count += 1;
                                    lastrecord = $"The last Updated record has the Last Name {student.LastName} and First Name {student.FirstName} with Student Id {student.StudentId}";

                                }
                                else
                                {
                                    utmeUploadError.Add(new StudentUtmeUploadErrorVm { JambRegNo = jambNo, Row = row });
                                    hasUtmeUploadError = true;
                                }
                            }

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
                    }
                    catch (Exception)
                    {

                        throw;
                    }

                    if (hasUtmeUploadError)
                    {
                        ViewBag.ErrorInfo = $"These applicants wasn't found on the system";
                        ViewBag.ErrorMessage = $"You have successfully Uploaded {count} records...";
                        return View("ErrorException", utmeUploadError);
                    }
                    message = $"You have successfully Uploaded {count} records...  and {lastrecord}";
                }
                return RedirectToAction("Index", "Students", new { message });
            }

            ViewBag.Error = $"File type is Incorrect <br/>";
            return View("Index");
        }


        [HttpGet]
        [Authorize(Roles = RoleName.Admin + "," + RoleName.DatabaseAdmin + "," + RoleName.SuperAdmin + "," + RoleName.AdmissionUploadOfficer)]
        public ActionResult DeleteUtmeAdmission(string message)
        {
            ViewBag.Message = message;
            return View();
        }

        [Authorize(Roles = RoleName.Admin + "," + RoleName.DatabaseAdmin + "," + RoleName.SuperAdmin + "," + RoleName.AdmissionUploadOfficer)]
        public async Task<ActionResult> DeleteUtmeAdmission(HttpPostedFileBase excelfile)
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
                        var jambNo = workSheet.Cells[row, 1].Value.ToString().Trim();
                        var progId = Convert.ToInt32(workSheet.Cells[row, 2].Value);



                        //if (!ModeOfEntry.RS.ToString().Equals(changeModeOfEntry.Trim().ToUpper().ToString()))
                        //{
                        //    ViewBag.ErrorInfo = "Whoops! Please check and correct Mode of Entry";
                        //    return View("ErrorException");
                        //}

                        //if (levelId < 1)
                        //{
                        //    ViewBag.ErrorInfo = "Please Leave no column or row Empty/Blank";
                        //    ViewBag.ErrorMessage = $" The Level Name  \"{changeModeOfEntry}\" at row {row}  specified in the excel doesn't exist on the portal. " +
                        //                           $"Please add the Department Option first before uploading or correct the excel sheet...";
                        //    return View("ErrorException");
                        //}
                        var student = await _db.Students.Include(x => x.Programme).AsNoTracking()
                                            .Where(x => x.StudentId.Trim().ToUpper().Equals(jambNo.Trim().ToUpper()) /*&& x.Programme.ProgrammeId.Equals(progId)*/)
                                            .FirstOrDefaultAsync();
                        //var checkAvailPayments = _db.StudentPaymentDetails.Where(x => x.StudentId.Equals(student.StudentId)).ToList();
                        //if (checkAvailPayments.Count > 0)
                        //{
                        //    ViewBag.ErrorMessage = "Student has payement and can't be Removed!";
                        //    return View("ErrorException");
                        //}

                        if (student != null)
                        {
                            student.IsDelete = true;
                            student.Active = false;
                            _db.Entry(student).State = EntityState.Modified;
                            //await _db.SaveChangesAsync();
                            utmeUploadError.Add(new StudentUtmeUploadErrorVm { JambRegNo = student.JambRegNo, Row = row });
                            hasUtmeUploadError = true;
                        }
                        else
                        {
                            ViewBag.ErrorMessage = "Student not found";
                            return View("ErrorException");
                        }
                        recordCount++;
                        lastrecord = $"The last Updated record has the Surname  {student.FirstName} and " +
                            $"First Name {student.FirstName} with  Reg No {student.JambRegNo}";
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

        //public async Task<string> setClearance(string matNo)
        //{
        //    var student = await  _db.Students.Where(x => x.MatricNo.Trim().ToUpper().Equals(matNo.ToUpper()))
        //                                   .FirstOrDefaultAsync();
        //    if (student != null)
        //    {
        //        student.IsGraduated = false;
        //        student.IsClearedAcademics = true;
        //        student.IsClearedDepartment = true;
        //        student.IsClearedFaculty = true;
        //        _db.Entry(student).State = EntityState.Modified;
        //        _db.SaveChangesAsync();
        //    }
        //    return "success";

        //}


        // Enable just one spillover student from their dashboard
        [HttpGet]
        public async Task<ActionResult> EnableSpillOver(string studentId)
        {
            studentId = studentId.ToUpper().Trim();
            var student = await _db.Students.Include(i => i.SchoolProgramme).AsNoTracking().Where(x => x.StudentId.Trim().ToUpper().Equals(studentId) ||
                            x.JambRegNo.Trim().ToUpper().Equals(studentId)).FirstOrDefaultAsync();

            student.IsGraduated = false;
            student.IsClearedDepartment = false;
            student.IsClearedFaculty = false;
            student.IsClearedAcademics = false;

            _db.Entry(student).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            return RedirectToAction("CustomDashborad", "Account");
        }

        // For excel upload for spillover students
        [HttpGet]
        //[Authorize(Roles = RoleName.Admin + "," + RoleName.DatabaseAdmin + "," + RoleName.SuperAdmin + "," + RoleName.AdmissionUploadOfficer)]
        public ActionResult EnableSpillOverForStudent(string message)
        {
            ViewBag.Message = message;
            return View();
        }

        // For excel upload for spillover students
        [HttpGet]
        //[Authorize(Roles = RoleName.Admin + "," + RoleName.DatabaseAdmin + "," + RoleName.SuperAdmin + "," + RoleName.AdmissionUploadOfficer)]
        public ActionResult ValidateAdmissionLetter(string message)
        {
            ViewBag.Message = message;
            return View();
        }

        //[Authorize(Roles = RoleName.Admin + "," + RoleName.DatabaseAdmin + "," + RoleName.SuperAdmin + "," + RoleName.AdmissionUploadOfficer)]
        public async Task<ActionResult>
            EnableSpillOverForStudent(HttpPostedFileBase excelfile)
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

                        var student = await _db.Students.AsNoTracking()
                                            .Where(x => x.MatricNo.Trim().ToUpper().Equals(matNo.ToUpper()))
                                            .ToListAsync();

                        foreach (var item in student)
                        {
                            if (student != null)
                            {
                                item.IsGraduated = false;
                                item.IsClearedDepartment = false;
                                item.IsClearedFaculty = false;
                                item.IsClearedAcademics = false;
                                _db.Entry(item).State = EntityState.Modified;

                                utmeUploadError.Add(new StudentUtmeUploadErrorVm { JambRegNo = item.JambRegNo, Row = row });
                                hasUtmeUploadError = true;
                            }
                            else
                            {
                                ViewBag.ErrorMessage = "Student not found";
                                return View("ErrorException");
                            }
                            recordCount++;
                            lastrecord = $"The last Updated record has the Surname  {item.LastName} and " +
                                $"First Name {item.FirstName} with  Reg No {item.MatricNo}";
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

        //[Authorize(Roles = RoleName.Admin + "," + RoleName.DatabaseAdmin + "," + RoleName.SuperAdmin + "," + RoleName.AdmissionUploadOfficer)]
        public async Task<ActionResult> ValidateAdmissionLetter(HttpPostedFileBase excelfile)
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

                    string SMSbody = $"Congratulations! Your admission letter has been re-validated. You can proceed to pay your school charges.";


                    for (int row = 2; row <= noOfRow; row++)
                    {
                        var matNo = workSheet.Cells[row, 1].Value.ToString().Trim();

                        var student = await _db.Students.AsNoTracking()
                                            .Where(x => x.JambRegNo.Trim().ToUpper().Equals(matNo.ToUpper()))
                                            .FirstOrDefaultAsync();

                        //foreach (var item in student)
                        //{
                            if (student != null)
                            {
                            student.ImeiNo = Guid.NewGuid().ToString();
                                _db.Entry(student).State = EntityState.Modified;

                            await SMSClass.SendSMS("UNIJOS SIS", SMSbody, student.PhoneNumber); //EBULK SMS API

                            utmeUploadError.Add(new StudentUtmeUploadErrorVm { JambRegNo = student.JambRegNo, Row = row });
                                hasUtmeUploadError = true;
                            }
                            else
                            {
                                ViewBag.ErrorMessage = "Student not found";
                                return View("ErrorException");
                            }
                            recordCount++;
                            lastrecord = $"The last Updated record has the Surname  {student.LastName} and " +
                                $"First Name {student.FirstName} with  Reg No {student.MatricNo}";
                        //}


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
                        //ViewBag.ErrorMessage = $"You have successfully Removed {recordCount} records...";
                        return View("ErrorException", utmeUploadError);
                    }
                    message = $"You have successfully Validated {recordCount} records...  and {lastrecord}";
                    ViewBag.Message = message;
                    return RedirectToAction("Index", "Students", new { message });
                }
            }
            ViewBag.Error = $"File type is Incorrect <br/>";
            return View("Index");
        }
        [HttpGet]
        [Authorize(Roles = RoleName.Admin + "," + RoleName.DatabaseAdmin + "," + RoleName.SuperAdmin + "," + RoleName.AdmissionUploadOfficer)]
        public ActionResult UpdateClearancesFromItalyDb(string message)
        {
            ViewBag.Message = message;
            return View();
        }
        [Authorize(Roles = RoleName.Admin + "," + RoleName.DatabaseAdmin + "," + RoleName.SuperAdmin + "," + RoleName.AdmissionUploadOfficer)]
        public async Task<ActionResult>
            UpdateClearancesFromItalyDb(HttpPostedFileBase excelfile)
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
                        var deptClearace = workSheet.Cells[row, 4].Value.ToString().Trim().ToUpper();
                        var facClearance = workSheet.Cells[row, 6].Value.ToString().Trim().ToUpper();
                        var acadClearance = workSheet.Cells[row, 5].Value.ToString().Trim().ToUpper();
                        var LevelId = Int32.Parse(workSheet.Cells[row, 3].Value.ToString().Trim());
                        var progId = Int32.Parse(workSheet.Cells[row, 2].Value.ToString().Trim());

                        var student = await _db.Students
                                            .Where(x => x.JambRegNo.Trim().ToUpper().Equals(matNo.ToUpper()))
                                            .FirstOrDefaultAsync();

                        if (student != null)
                        {
                            student.IsClearedAcademics = acadClearance.Equals("TRUE") ? true : false;
                            student.IsClearedDepartment = deptClearace.Equals("TRUE") ? true : false;
                            student.IsClearedFaculty = facClearance.Equals("TRUE") ? true : false;
                            //student.LevelId = LevelId;
                            //student.ProgrammeId = progId;
                            _db.Entry(student).State = EntityState.Modified;

                            utmeUploadError.Add(new StudentUtmeUploadErrorVm { JambRegNo = student.JambRegNo, Row = row });
                            hasUtmeUploadError = true;
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

        [Authorize(Roles = RoleName.Admin + "," + RoleName.DatabaseAdmin + "," + RoleName.SuperAdmin + "," + RoleName.AdmissionUploadOfficer)]
        public ActionResult UploadAdmission(string message)
        {
            ViewBag.Message = message;
            return View();
        }

        [Authorize(Roles = RoleName.Admin + "," + RoleName.DatabaseAdmin + "," + RoleName.SuperAdmin + "," + RoleName.AdmissionUploadOfficer)]
        [HttpPost]
        public async Task<ActionResult> UploadAdmission(HttpPostedFileBase excelfile)
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
                string message = "";
                string fileContentType = file.ContentType;
                byte[] fileBytes = new byte[file.ContentLength];
                var data = file.InputStream.Read(fileBytes, 0, Convert.ToInt32(file.ContentLength));
                var phoneNumbers = new List<String>();

                // Read data from excel file
                using (var package = new ExcelPackage(file.InputStream))
                {
                    ExcelValidation myExcel = new ExcelValidation();
                    var currentSheet = package.Workbook.Worksheets;
                    var workSheet = currentSheet.First();
                    var noOfCol = workSheet.Dimension.End.Column;
                    var noOfRow = workSheet.Dimension.End.Row;
                    int requiredField = 5;


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

                    int count = 0;

                    for (int row = 2; row <= noOfRow; row++)
                    {
                        var studentId = DateTime.Now.Ticks;
                        var formNo = workSheet.Cells[row, 1].Value.ToString().Trim();
                        string code = workSheet.Cells[row, 2].Value.ToString().Trim();
                        string level = workSheet.Cells[row, 3].Value.ToString().Trim();
                        string schoolCode = workSheet.Cells[row, 4].Value.ToString().Trim();
                        string sessionName = workSheet.Cells[row, 5].Value.ToString().Trim();
                        var checkstudent = _db.Students.AsNoTracking()
                                   .Any(x => x.JambRegNo.ToUpper().Equals(formNo.ToUpper()));
                        if (checkstudent)
                        {
                            ViewBag.ErrorInfo = "Multiple Jamb No Detected";
                            ViewBag.ErrorMessage = $" The \"{formNo}\" Form No specified in the excel at row {row} already exist on the portal. " +
                                                   $"Please check carefully before uploading and correct the excel sheet...";
                            return View("ErrorException");
                        }


                        var programmeCode = await _db.Programmes.AsNoTracking()
                                            .Where(x => x.ProgrammeCode.ToUpper().Equals(code.ToUpper()))
                                            .FirstOrDefaultAsync();
                        var uploadedSessionId = await _db.Sessions.AsNoTracking().Where(x => x.SessionName.ToUpper().Equals(sessionName.ToUpper()))
                                                   .FirstOrDefaultAsync();
                        var levelId = _db.Levels.AsNoTracking().Where(s => s.LevelName.Equals(level))
                                           .FirstOrDefault();
                        if (levelId == null)
                        {
                            ViewBag.ErrorInfo = "Please Leave no column or row Empty/Blank";
                            ViewBag.ErrorMessage = $" The \"{level}\" level specified in the excel doesn't exist on the portal. " +
                                                   $"Please add the level first before uploading or correct the excel sheet...";
                            return View("ErrorException");
                        }
                        var schoolProgramme = await _db.SchoolProgrammes.AsNoTracking()
                                          .Where(x => x.SchoolProgrammeCode.ToUpper().Equals(schoolCode.ToUpper()))
                                          .FirstOrDefaultAsync();
                        if (programmeCode == null)
                        {
                            ViewBag.ErrorInfo = "Please Leave no column or row Empty/Blank";
                            ViewBag.ErrorMessage = $" The department Option \"{code}\"  specified in the excel at row {row} doesn't exist on the portal. " +
                                                   $"Please add the Department Option first before uploading or correct the excel sheet...";
                            return View("ErrorException");
                        }
                        if (uploadedSessionId == null)
                        {
                            ViewBag.ErrorInfo = "Please Leave no column or row Empty/Blank";
                            ViewBag.ErrorMessage = $" The Session Name \"{sessionName}\"  specified in the excel at row {row} doesn't exist on the portal. " +
                                                   $"Please add the Session Name first before uploading or correct the excel sheet...";
                            return View("ErrorException");
                        }

                        if (schoolProgramme == null)
                        {
                            ViewBag.ErrorInfo = $"The Undergraduate school programme doesn't exist in the system.";
                            ViewBag.ErrorMessage = "Please check the Student Programme type spelling very well ";
                            return View("ErrorException");
                        }
                        try
                        {
                            var student = new Student();

                            var applicant = await _db.Applicants.AsNoTracking().Where(x => x.ApplicantId.ToUpper().Equals(formNo.ToUpper()))
                                            .FirstOrDefaultAsync();
                            if (applicant != null)
                            {
                                student.StudentId = $"{SchoolSetUp.CurrentSchoolName}{studentId}";
                                student.JambRegNo = formNo;
                                student.Email = applicant.ApplicantEmail;
                                student.Active = true;
                                student.StudentStatus = StudentStatus.New_Student.ToString();
                                student.PhoneNumber = applicant.PhoneNumber;
                                student.FirstName = applicant.FirstName;
                                student.MiddleName = applicant.MiddleName;
                                student.LastName = applicant.LastName;
                                student.StateOfOrigin = applicant.StateOfOrigin;
                                student.DateOfBirth = applicant.DateOfBirth;
                                student.Gender = applicant.Gender;
                                student.EnrollmentDate = DateTime.Now;
                                student.ProgrammeId = programmeCode.ProgrammeId;
                                student.LevelId = levelId.LevelId;
                                student.SchoolProgrammeId = schoolProgramme.SchoolProgrammeId;
                                student.SessionId = uploadedSessionId.SessionId;
                                student.Passport = applicant.ApplicatPassport;

                                _db.Students.Add(student);
                                count += 1;

                                lastrecord = $"The last Updated record has the Last Name {student.LastName} and First Name {student.FirstName} with Student Id {student.StudentId}";
                                phoneNumbers.Add(student.PhoneNumber);
                            }
                            else
                            {
                                ViewBag.ErrorInfo = $"This applicant with Form No {formNo} at row {row} wasn't found on the system";
                                ViewBag.ErrorMessage = $"This applicant with Form No {formNo} at row {row} wasn't found on the system";
                                return View("ErrorException");
                            }
                        }
                        catch (Exception ex)
                        {
                            ViewBag.ErrorInfo = "Please Leave no column or row Empty/Blank";
                            ViewBag.ErrorMessage = ex.Message;
                            return View("ErrorException");
                        }
                    }
                    await _db.SaveChangesAsync();
                    message = $"You have successfully Uploaded {count} records...  and {lastrecord}";

                    //var smsMessage = $"You hsve been offerred admission...go to your email for registration procedure";
                    //var stringPhoneNumbers = ProcessNumbers(phoneNumbers);
                    //await SMSClass.SendSMS("UNIJOS", smsMessage, stringPhoneNumbers);
                }
                return RedirectToAction("Index", "Students", new { message });
            }

            ViewBag.Error = $"File type is Incorrect <br/>";
            return View("Index");
        }

        private string ProcessNumbers(List<String> PhoneNums)
        {
            //string[] Result = new string[PhoneNums.Length];
            string Result = "";
            foreach (var item in PhoneNums)
            {
                Result += "0" + item + ",";
            }
            return Result;
        }


        [Authorize(Roles = RoleName.Admin + "," + RoleName.DatabaseAdmin + "," + RoleName.SuperAdmin + "," + RoleName.AdmissionUploadOfficer)]
        public ActionResult UploadStudentToDelete(string message)
        {
            ViewBag.Message = message;
            return View();
        }

        [Authorize(Roles = RoleName.Admin + "," + RoleName.DatabaseAdmin + "," + RoleName.SuperAdmin + "," + RoleName.AdmissionUploadOfficer)]
        [HttpPost]
        public async Task<ActionResult> UploadStudentToDelete(HttpPostedFileBase excelfile)
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
                    int requiredField = 1;


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

                    int count = 0;
                    var utmeUploadError = new List<StudentUtmeUploadErrorVm>();
                    bool hasUtmeUploadError = false;

                    for (int row = 2; row <= noOfRow; row++)
                    {
                        var formNo = workSheet.Cells[row, 1].Value.ToString().Trim();

                        try
                        {
                            var studentExit = await _db.Students.FirstOrDefaultAsync(x => x.JambRegNo.Trim().ToUpper().Equals(formNo.Trim().ToUpper()));
                            if (studentExit != null)
                            {
                                var payment = await _db.SchoolFeePayments.AnyAsync(x => x.StudentId.Equals(studentExit.StudentId));
                                if (payment.Equals(false))
                                {
                                    var user = await _db.Users.Where(x => x.StudentId.Trim().ToUpper().Equals(studentExit.StudentId.Trim().ToUpper())
                                                || x.Email.Trim().ToUpper().Equals(studentExit.Email.Trim().ToUpper()))
                                                .FirstOrDefaultAsync();
                                    if (user != null)
                                    {
                                        _db.Entry(user).State = EntityState.Deleted;
                                    }
                                    _db.Entry(studentExit).State = EntityState.Deleted;
                                    count += 1;
                                    lastrecord = $"The last Updated record has the Last Name {studentExit.LastName} and First Name {studentExit.FirstName}";
                                }
                                else
                                {
                                    utmeUploadError.Add(new StudentUtmeUploadErrorVm { JambRegNo = formNo, Row = row, Message = "Payment has already been Initiated for this candidate" });
                                    hasUtmeUploadError = true;
                                }
                            }
                            else
                            {
                                utmeUploadError.Add(new StudentUtmeUploadErrorVm { JambRegNo = formNo, Row = row, Message = $"Record for student cant be found" });
                                hasUtmeUploadError = true;
                            }

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
                    }
                    catch (Exception ex)
                    {
                        ViewBag.ErrorInfo = $"Delete Operation not successful";
                        ViewBag.ErrorMessage = ex.Message;
                        return View("ErrorException");
                    }

                    if (hasUtmeUploadError)
                    {
                        ViewBag.ErrorInfo = $"These applicants wasn't found on the system";
                        ViewBag.ErrorMessage = $"You have successfully Uploaded {count} records...";
                        return View("ErrorException", utmeUploadError);
                    }
                    message = $"You have successfully Uploaded {count} records...  and {lastrecord}";
                }
                return RedirectToAction("Index", "Students", new { message });
            }

            ViewBag.Error = $"File type is Incorrect <br/>";
            return View("Index");
        }

        public ActionResult UploadChangeOfAdmittedSession()
        {
            return View();
        }


        [HttpPost]
        public async Task<ActionResult> UploadChangeOfAdmittedSession(HttpPostedFileBase excelfile)
        {
            if (excelfile == null || excelfile.ContentLength == 0)
            {
                ViewBag.Error = "Please Select a excel file <br/>";
                ViewBag.Message = "You must select an excel file before you click Upload button";
                TempData["Title"] = "Error.";
                return View("Index");
            }
            HttpPostedFileBase file = Request.Files["excelfile"];
            if (excelfile.FileName.EndsWith("xls", StringComparison.CurrentCulture) ||
                excelfile.FileName.EndsWith("xlsx", StringComparison.CurrentCulture))
            {
                int recordCount = 0;
                string message = "";
                string fileContentType = file.ContentType;
                byte[] fileBytes = new byte[file.ContentLength];
                var data = file.InputStream.Read(fileBytes, 0, Convert.ToInt32(file.ContentLength));
                var utmeUploadError = new List<StudentUtmeUploadErrorVm>();

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
                        return View("Index");
                    }

                    for (int row = 2; row <= noOfRow; row++)
                    {
                        var jambRegNo = workSheet.Cells[row, 1].Value.ToString().Trim();
                        var sessionName = workSheet.Cells[row, 2].Value.ToString().Trim();

                        var student = await _db.Students.FirstOrDefaultAsync(x => x.JambRegNo.Trim().ToUpper().Equals(jambRegNo.ToUpper()));

                        var session = _db.Sessions.AsNoTracking().Where(s => s.SessionName.Trim().ToUpper().Equals(sessionName.ToUpper()))
                                           .FirstOrDefault();
                        if (session == null)
                        {
                            ViewBag.ErrorInfo = "Please Leave no column or row Empty/Blank";
                            ViewBag.ErrorMessage = $" The \"{sessionName}\" Session specified in the excel doesn't exist on the portal. " +
                                                   $"Please add the session first before uploading or correct the excel sheet...";
                            return View("ErrorException");
                        }

                        if (student == null)
                        {
                            utmeUploadError.Add(new StudentUtmeUploadErrorVm { JambRegNo = jambRegNo, Row = row });
                        }
                        else
                        {
                            try
                            {
                                student.SessionId = session.SessionId;
                                _db.Entry(student).State = EntityState.Modified;
                                recordCount += 1;
                            }
                            catch (Exception)
                            {
                                utmeUploadError.Add(new StudentUtmeUploadErrorVm { JambRegNo = jambRegNo, Row = row });
                            }
                        }
                    }
                    try
                    {
                        await _db.SaveChangesAsync();
                        if (utmeUploadError.Count() > 0)
                        {
                            ViewBag.ErrorInfo = $"These Student has not been assigned new Session yet";
                            ViewBag.ErrorMessage = $"You have successfully Uploaded {utmeUploadError.Count()} records...";
                            return View("ErrorException", utmeUploadError);
                        }
                        else
                        {
                            message = $"You have successfully Uploaded {recordCount} records...";
                            ViewBag.Message = message;
                            return RedirectToAction("Index", new { message });
                        }
                    }
                    catch (Exception ex)
                    {
                        ViewBag.ErrorInfo = $"Change of session is not completed successfully";
                        ViewBag.ErrorMessage = ex.Message;
                        return View("ErrorException");
                    }
                }
            }
            ViewBag.Error = $"File type is Incorrect <br/>";
            return View("Index");
        }


        public ActionResult UploadChangeOfCourse()
        {
            return View();
        }


        [HttpPost]
        public async Task<ActionResult> UploadChangeOfCourse(HttpPostedFileBase excelfile)
        {
            if (excelfile == null || excelfile.ContentLength == 0)
            {
                ViewBag.Error = "Please Select a excel file <br/>";
                ViewBag.Message = "You must select an excel file before you click Upload button";
                TempData["Title"] = "Error.";
                return View("Index");
            }
            HttpPostedFileBase file = Request.Files["excelfile"];
            if (excelfile.FileName.EndsWith("xls", StringComparison.CurrentCulture) ||
                excelfile.FileName.EndsWith("xlsx", StringComparison.CurrentCulture))
            {
                int recordCount = 0;
                string message = "";
                string fileContentType = file.ContentType;
                byte[] fileBytes = new byte[file.ContentLength];
                var data = file.InputStream.Read(fileBytes, 0, Convert.ToInt32(file.ContentLength));
                var utmeUploadError = new List<StudentUtmeUploadErrorVm>();

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
                        return View("Index");
                    }

                    for (int row = 2; row <= noOfRow; row++)
                    {
                        var jambRegNo = workSheet.Cells[row, 1].Value.ToString().Trim();
                        var courseCode = workSheet.Cells[row, 2].Value.ToString().Trim();
                        var levelName = workSheet.Cells[row, 3].Value.ToString().Trim();

                        var student = await _db.Students.FirstOrDefaultAsync(x => x.MatricNo.Trim().ToUpper().Equals(jambRegNo.ToUpper()) || x.JambRegNo.Trim().ToUpper().Equals(jambRegNo.ToUpper()));

                        var programmeCode = await _db.Programmes.AsNoTracking()
                                            .Where(x => x.ProgrammeCode.Trim().ToUpper().Equals(courseCode.ToUpper()))
                                            .FirstOrDefaultAsync();

                        var levelId = _db.Levels.AsNoTracking().Where(s => s.LevelName.Trim().ToUpper().Equals(levelName.ToUpper()))
                                           .FirstOrDefault();
                        if (levelId == null)
                        {
                            ViewBag.ErrorInfo = "Please Leave no column or row Empty/Blank";
                            ViewBag.ErrorMessage = $" The \"{levelName}\" level specified in the excel doesn't exist on the portal. " +
                                                   $"Please add the level first before uploading or correct the excel sheet...";
                            return View("ErrorException");
                        }
                        if (programmeCode == null)
                        {
                            ViewBag.ErrorInfo = "Please Leave no column or row Empty/Blank";
                            ViewBag.ErrorMessage = $" The department Option \"{courseCode}\"  specified in the excel at row {row} doesn't exist on the portal. " +
                                                   $"Please add the Department Option first before uploading or correct the excel sheet...";
                            return View("ErrorException");
                        }

                        if (student == null)
                        {
                            utmeUploadError.Add(new StudentUtmeUploadErrorVm { JambRegNo = jambRegNo, Row = row });
                        }
                        else
                        {
                            try
                            {
                                student.StudentStatus = StudentStatus.New_Student.ToString();
                                student.ProgrammeId = programmeCode.ProgrammeId;
                                student.LevelId = levelId.LevelId;

                                //student.IsClearedDepartment = false;
                                //student.IsClearedFaculty = false;
                                //student.IsClearedAcademics = false;

                                _db.Entry(student).State = EntityState.Modified;
                                recordCount += 1;

                                //var schoolFeePayment = await _db.SchoolFeePayments.AsNoTracking().Where(x => x.StudentId.Equals(student.StudentId)
                                //                        && x.FeeCategory.ToUpper().Trim().Equals(SchoolFeeCategory.School_Charges.ToString().Trim().ToUpper())
                                //                        && x.Status.Equals(false)).ToListAsync();

                                //foreach (var payment in schoolFeePayment)
                                //{
                                //    _db.Entry(payment).State = EntityState.Deleted;
                                //}
                            }
                            catch (Exception)
                            {
                                utmeUploadError.Add(new StudentUtmeUploadErrorVm { JambRegNo = jambRegNo, Row = row });
                            }
                        }
                    }
                    try
                    {
                        await _db.SaveChangesAsync();
                        if (utmeUploadError.Count() > 0)
                        {
                            ViewBag.ErrorInfo = $"These Student has not been assigned new Programme yet";
                            ViewBag.ErrorMessage = $"You have successfully Uploaded {utmeUploadError.Count()} records...";
                            return View("ErrorException", utmeUploadError);
                        }
                        else
                        {
                            message = $"You have successfully Uploaded {recordCount} records...";
                            ViewBag.Message = message;
                            return RedirectToAction("Index", new { message });
                        }
                    }
                    catch (Exception ex)
                    {
                        ViewBag.ErrorInfo = $"Delete Operation not successful";
                        ViewBag.ErrorMessage = ex.Message;
                        return View("ErrorException");
                    }
                }
            }
            ViewBag.Error = $"File type is Incorrect <br/>";
            return View("Index");
        }


        private async Task<int> GetStudentNumber()
        {
            var studentCount = await _db.Students.AsNoTracking().CountAsync();
            return studentCount;
        }

        public async Task<ActionResult> InActiveStudent()
        {
            ViewBag.DepartmentId = new SelectList(await _db.Departments.AsNoTracking().ToListAsync(), "DepartmentId", "DeptName");
            ViewBag.LevelId = new SelectList(await _db.Levels.AsNoTracking().ToListAsync(), "LevelId", "LevelName");
            return View();
        }

        public async Task<ActionResult> InactiveReport(int? DepartmentId, int? LevelId, string Active, string Indegeneous)
        {
            var inactiveStudent = await _db.Students.Include(i => i.Programme.Department).Include(i => i.Level)
                        .AsNoTracking().ToListAsync();
            if (!String.IsNullOrEmpty(Indegeneous))
            {
                inactiveStudent = inactiveStudent.Where(x => x.Indegine.Equals(true)).ToList();
            }
            if (!String.IsNullOrEmpty(Active))
            {
                inactiveStudent = inactiveStudent.Where(x => x.Active.Equals(true)).ToList();
            }
            else
            {
                inactiveStudent = inactiveStudent.Where(x => x.Active.Equals(false)).ToList();
            }


            if (DepartmentId != null & LevelId != null)
            {
                inactiveStudent = inactiveStudent.Where(
                    x => x.Programme.Department.DepartmentId.Equals((int)DepartmentId)
                         && x.Level.LevelId.Equals((int)LevelId)).ToList();
            }
            else if (DepartmentId == null & LevelId != null)
            {
                inactiveStudent = inactiveStudent.Where(
                    x => x.Level.LevelId.Equals((int)LevelId)).ToList();
            }
            else if (DepartmentId != null & LevelId == null)
            {
                inactiveStudent = inactiveStudent.Where(
                    x => x.Programme.Department.DepartmentId.Equals((int)DepartmentId)).ToList();
            }
            var data = inactiveStudent.Select(s => new
            {
                s.FullName,
                s.Gender,
                s.Programme.Department.DeptName,
                s.Programme.ProgrammeName,
                s.Level.LevelName,
                s.Active,
            }).ToList();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> DownloadPdfReport(int? DepartmentId, int? LevelId, string Active, string Indegeneous)
        {
            var inactiveStudent = await _db.Students.Include(i => i.Programme.Department).Include(i => i.Level)
                .AsNoTracking().ToListAsync();
            if (!String.IsNullOrEmpty(Indegeneous))
            {
                inactiveStudent = inactiveStudent.Where(x => x.Indegine.Equals(true)).ToList();
            }
            if (!String.IsNullOrEmpty(Active))
            {
                inactiveStudent = inactiveStudent.Where(x => x.Active.Equals(true)).ToList();
            }
            else
            {
                inactiveStudent = inactiveStudent.Where(x => x.Active.Equals(false)).ToList();
            }

            if (DepartmentId != null & LevelId != null)
            {
                inactiveStudent = inactiveStudent.Where(
                    x => x.Programme.Department.DepartmentId.Equals((int)DepartmentId)
                         && x.Level.LevelId.Equals((int)LevelId)).ToList();
            }
            else if (DepartmentId == null & LevelId != null)
            {
                inactiveStudent = inactiveStudent.Where(
                    x => x.Level.LevelId.Equals((int)LevelId)).ToList();
            }
            else if (DepartmentId != null & LevelId == null)
            {
                inactiveStudent = inactiveStudent.Where(
                    x => x.Programme.Department.DepartmentId.Equals((int)DepartmentId)).ToList();
            }

            return new ViewAsPdf("DownloadPdfReport", inactiveStudent);
        }

        public async Task DownloadExcelReport(int? DepartmentId, int? LevelId, string Active, string Indegeneous)
        {
            var inactiveStudent = await _db.Students.Include(i => i.Programme.Department).Include(i => i.Level)
                .AsNoTracking().ToListAsync();
            if (!String.IsNullOrEmpty(Indegeneous))
            {
                inactiveStudent = inactiveStudent.Where(x => x.Indegine.Equals(true)).ToList();
            }
            if (!String.IsNullOrEmpty(Active))
            {
                inactiveStudent = inactiveStudent.Where(x => x.Active.Equals(true)).ToList();
            }
            else
            {
                inactiveStudent = inactiveStudent.Where(x => x.Active.Equals(false)).ToList();
            }

            if (DepartmentId != null & LevelId != null)
            {
                inactiveStudent = inactiveStudent.Where(
                    x => x.Programme.Department.DepartmentId.Equals((int)DepartmentId)
                         && x.Level.LevelId.Equals((int)LevelId)).ToList();
            }
            else if (DepartmentId == null & LevelId != null)
            {
                inactiveStudent = inactiveStudent.Where(
                    x => x.Level.LevelId.Equals((int)LevelId)).ToList();
            }
            else if (DepartmentId != null & LevelId == null)
            {
                inactiveStudent = inactiveStudent.Where(
                    x => x.Programme.Department.DepartmentId.Equals((int)DepartmentId)).ToList();
            }

            char c1 = 'A';
            ExcelPackage package = new ExcelPackage();
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Report");

            worksheet.Cells[$"{c1++}1"].Value = "Student Id";
            worksheet.Cells[$"{c1++}1"].Value = "Student FullName";
            worksheet.Cells[$"{c1++}1"].Value = "Department Name";
            worksheet.Cells[$"{c1++}1"].Value = "Programme Name";
            worksheet.Cells[$"{c1++}1"].Value = "Level Name";
            worksheet.Cells[$"{c1++}1"].Value = "Status";

            int rowStart = 2;
            //char c2 = 'A';

            for (var i = 0; i < inactiveStudent.Count; i++)
            {

                worksheet.Cells[$"A{rowStart}"].Value = inactiveStudent[i].StudentId;
                worksheet.Cells[$"B{rowStart}"].Value = inactiveStudent[i].FullName;
                worksheet.Cells[$"C{rowStart}"].Value = inactiveStudent[i].Programme.Department.DeptName;
                worksheet.Cells[$"D{rowStart}"].Value = inactiveStudent[i].Programme.ProgrammeName;
                worksheet.Cells[$"E{rowStart}"].Value = inactiveStudent[i].Active;

                rowStart++;
            }

            worksheet.Cells["A:AZ"].AutoFitColumns();
            worksheet.Column(1).Style.Locked = true;
            worksheet.Column(2).Style.Locked = true;
            worksheet.Column(3).Style.Locked = true;
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment: filename=" +
                        $"{inactiveStudent.Select(s => s.Programme.Department.DeptName).FirstOrDefault()}InactiveStudent.xlsx");
            Response.BinaryWrite(package.GetAsByteArray());
            Response.End();

        }


        public async Task<ActionResult> GraduatedStudent()
        {
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            ViewBag.DepartmentId = new SelectList(await _db.Departments.AsNoTracking().ToListAsync(), "DepartmentId", "DeptName");

            return View();
        }

        public async Task<ActionResult> GraduatedStudentReport(int? SessionId, int? DepartmentId, string Indegeneous)
        {
            var inactiveStudent = await _db.Students.Include(i => i.Programme.Department).Include(i => i.Level)
                                    .Include(i => i.Session).AsNoTracking().Where(x => x.IsGraduated.Equals(true))
                                    .ToListAsync();
            if (!String.IsNullOrEmpty(Indegeneous))
            {
                inactiveStudent = inactiveStudent.Where(x => x.Indegine.Equals(true)).ToList();
            }
            if (SessionId != null && DepartmentId != null)
            {
                inactiveStudent = inactiveStudent.Where(
                    x => x.Session.SessionId.Equals(SessionId)
                    && x.Programme.Department.DepartmentId.Equals(DepartmentId))
                    .ToList();
            }
            else if (SessionId != null && DepartmentId == null)
            {
                inactiveStudent = inactiveStudent.Where(
                        x => x.Session.SessionId.Equals(SessionId))
                    .ToList();
            }
            else if (SessionId != null && DepartmentId != null)
            {
                inactiveStudent = inactiveStudent.Where(
                        x => x.Programme.Department.DepartmentId.Equals(DepartmentId))
                    .ToList();
            }
            var data = inactiveStudent.Select(s => new
            {
                s.FullName,
                s.Gender,
                s.Programme.Department.DeptName,
                s.Programme.ProgrammeName,
                s.Level.LevelName,
                s.PhoneNumber
            }).ToList();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> DownloadGraduatePdfReport(int? SessionId, int? DepartmentId, string Indegeneous)
        {
            var inactiveStudent = await _db.Students.Include(i => i.Programme.Department).Include(i => i.Level)
                                    .Include(i => i.Session).AsNoTracking().Where(x => x.IsGraduated.Equals(true))
                                    .ToListAsync();
            if (!String.IsNullOrEmpty(Indegeneous))
            {
                inactiveStudent = inactiveStudent.Where(x => x.Indegine.Equals(true)).ToList();
            }
            if (SessionId != null && DepartmentId != null)
            {
                inactiveStudent = inactiveStudent.Where(
                        x => x.Session.SessionId.Equals(SessionId)
                             && x.Programme.Department.DepartmentId.Equals(DepartmentId)).ToList();
            }
            else if (SessionId != null && DepartmentId == null)
            {
                inactiveStudent = inactiveStudent.Where(
                        x => x.Session.SessionId.Equals(SessionId)).ToList();
            }
            else if (SessionId != null && DepartmentId != null)
            {
                inactiveStudent = inactiveStudent.Where(
                        x => x.Programme.Department.DepartmentId.Equals(DepartmentId))
                    .ToList();
            }
            var SessionName = "";
            ViewBag.SessionName = SessionName;

            return new ViewAsPdf("DownloadGraduatePdfReport", inactiveStudent);
        }

        public async Task DownloadGraduateExcelReport(int? SessionId, int? DepartmentId, string Indegeneous)
        {
            var inactiveStudent = await _db.Students.Include(i => i.Programme.Department).Include(i => i.Level)
                                .Include(i => i.Session).AsNoTracking().Where(x => x.IsGraduated.Equals(true)).ToListAsync();
            if (!String.IsNullOrEmpty(Indegeneous))
            {
                inactiveStudent = inactiveStudent.Where(x => x.Indegine.Equals(true)).ToList();
            }
            if (SessionId != null && DepartmentId != null)
            {
                inactiveStudent = inactiveStudent.Where(
                        x => x.Session.SessionId.Equals(SessionId)
                             && x.Programme.Department.DepartmentId.Equals(DepartmentId))
                    .ToList();
            }
            else if (SessionId != null && DepartmentId == null)
            {
                inactiveStudent = inactiveStudent.Where(
                        x => x.Session.SessionId.Equals(SessionId))
                    .ToList();
            }
            else if (SessionId != null && DepartmentId != null)
            {
                inactiveStudent = inactiveStudent.Where(
                        x => x.Programme.Department.DepartmentId.Equals(DepartmentId))
                    .ToList();
            }

            char c1 = 'A';
            ExcelPackage package = new ExcelPackage();
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Report");

            worksheet.Cells[$"{c1++}1"].Value = "Student Id";
            worksheet.Cells[$"{c1++}1"].Value = "Student FullName";
            worksheet.Cells[$"{c1++}1"].Value = "Department Name";
            worksheet.Cells[$"{c1++}1"].Value = "Programme Name";
            worksheet.Cells[$"{c1++}1"].Value = "Level Name";
            worksheet.Cells[$"{c1++}1"].Value = "Status";

            int rowStart = 2;
            //char c2 = 'A';

            for (var i = 0; i < inactiveStudent.Count; i++)
            {

                worksheet.Cells[$"A{rowStart}"].Value = inactiveStudent[i].StudentId;
                worksheet.Cells[$"B{rowStart}"].Value = inactiveStudent[i].FullName;
                worksheet.Cells[$"C{rowStart}"].Value = inactiveStudent[i].Programme.Department.DeptName;
                worksheet.Cells[$"D{rowStart}"].Value = inactiveStudent[i].Programme.ProgrammeName;
                worksheet.Cells[$"E{rowStart}"].Value = inactiveStudent[i].Active;

                rowStart++;
            }

            worksheet.Cells["A:AZ"].AutoFitColumns();
            worksheet.Column(1).Style.Locked = true;
            worksheet.Column(2).Style.Locked = true;
            worksheet.Column(3).Style.Locked = true;
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment: filename=" +
                                                      $"{inactiveStudent.Select(s => s.Programme.Department.DeptName).FirstOrDefault()}InactiveStudent.xlsx");
            Response.BinaryWrite(package.GetAsByteArray());
            Response.End();

        }


        public async Task<ActionResult> DownloadStudentApi()
        {
            var downloadJsonData = new List<StudentUploadApiVm>();
            string path = Server.MapPath("~/student2017Session.json");
            using (StreamReader r = new StreamReader(path))
            {
                var json = r.ReadToEnd();
                downloadJsonData = JsonConvert.DeserializeObject<List<StudentUploadApiVm>>(json);
            }
            int recordCount = 0;
            string lastRecord = "";
            var schoolProgrammeId = await GetUndergraduateSchoolProgrammeId();
            var level = new Level();
            var levelList = GetLevelList();

            foreach (var item in downloadJsonData.Where(x => x.SessionName.Equals("2008") && !string.IsNullOrEmpty(x.ProgrammeCode) &&
                            x.ProgrammeCode != "0"))
            {
                var sessinIdd = GetSessionIdByYear(item.SessionName, allSessions);
                var checkStudent = await _db.Students.AnyAsync(x => x.MatricNo.Trim().ToUpper().Equals(item.MatricNo.Trim().ToUpper()));
                if (!checkStudent)
                {
                    var studentId = DateTime.Now.Ticks;
                    var programme = await _db.Programmes.AsNoTracking()
                                       .Where(x => x.ProgrammeCode.ToUpper().Equals(item.ProgrammeCode.ToUpper()))
                                       .FirstOrDefaultAsync();
                    if (!string.IsNullOrEmpty(item.LevelName))
                    {
                        level = levelList.Where(s => s.LevelName.Equals(item.LevelName)).FirstOrDefault();
                    }
                    else
                    {
                        level = null;
                    }

                    if (programme == null)
                    {
                        ViewBag.ErrorInfo = "Please Leave no column or row Empty/Blank";
                        ViewBag.ErrorMessage = $" The department Option \"{item.ProgrammeCode}\"  specified  doesn't exist on the portal. " +
                                               $"Please add the Department Option first before uploading or correct the excel sheet...";
                        return View("ErrorException");
                    }

                    try
                    {
                        var student = new Student()
                        {
                            StudentId = $"{SchoolSetUp.CurrentSchoolName}{studentId}",
                            MatricNo = item.MatricNo,
                            JambRegNo = item.JambRegNo,
                            FirstName = item.FirstName,
                            MiddleName = item.MiddleName,
                            LastName = item.LastName,
                            Email = item.Email.Replace(" ", ""),
                            PhoneNumber = item.PhoneNumber,
                            StateOfOrigin = item.StateOfOrigin,
                            Nationality = item.Nationality,
                            DateOfBirth = item.DateOfBirthOrg,
                            Gender = item.Gender,
                            EnrollmentDate = item.EnrollmentDateOrg,
                            ProgrammeId = programme.ProgrammeId,
                            LevelId = level?.LevelId,
                            StudentStatus = StudentStatus.Returning.ToString(),
                            SchoolProgrammeId = schoolProgrammeId,
                            SessionId = GetSessionIdByYear(item.SessionName.Trim(), allSessions),
                            ModeOfEntry = string.IsNullOrEmpty(item.ModeOfEntry) ? "UTME" : item.ModeOfEntry.Trim().ToUpper(),
                            MaritalStatus = item.MaritalStatus,
                            //Passport = ConvertImageFromUrl(item.ImageUrl),
                            //Signature = ConvertImageFromUrl(item.PassportUrl),
                            IsClearedAcademics = true,
                            IsClearedDepartment = true,
                            IsClearedFaculty = true,
                        };

                        _db.Students.Add(student);
                        recordCount++;
                        lastRecord = $"The last Updated record has the Last Name {student.LastName} and First Name {student.FirstName} with Student Id {student.StudentId}";
                    }
                    catch (Exception ex)
                    {
                        ViewBag.ErrorInfo = $"Please Leave no column or row Empty/Blank {item.MatricNo}";
                        ViewBag.ErrorMessage = ex.Message;
                        return View("ErrorException");
                    }
                }

            }
            try
            {
                await _db.SaveChangesAsync();
                ViewBag.ErrorInfo = $"{recordCount} record(s) has been downloaded and uploaded to the database completely";
                ViewBag.ErrorMessage = $"{recordCount} record(s) has been downloaded and uploaded to the database completely";
                return View("ErrorException");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorInfo = $"Saving Error";
                ViewBag.ErrorMessage = ex.Message;
                return View("ErrorException");
            }
        }




        public byte[] ConvertImageFromUrl(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    using (var webClient = new WebClient())
                    {
                        return webClient.DownloadData(url);
                    }
                }
                catch (Exception)
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }


        public ActionResult UploadPostGraduateStudent()
        {
            return View();
        }



        [HttpPost]
        public async Task<ActionResult> UploadPostGraduateStudent(HttpPostedFileBase excelfile)
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
                    int requiredField = 15;

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
                        var studentId = DateTime.Now.Ticks;
                        string formNo = workSheet.Cells[row, 1].Value.ToString().Trim();
                        string lastName = workSheet.Cells[row, 2].Value.ToString().Trim();
                        string firstName = workSheet.Cells[row, 3].Value.ToString().Trim();
                        string middleName = workSheet.Cells[row, 4].Value.ToString().Trim();
                        string email = workSheet.Cells[row, 5].Value.ToString().Trim();
                        string phoneNumber = workSheet.Cells[row, 6].Value.ToString().Trim();
                        DateTime dateOfBirth = Convert.ToDateTime(workSheet.Cells[row, 7].Value.ToString().Trim());
                        string gender = workSheet.Cells[row, 8].Value.ToString().Trim();
                        string state = workSheet.Cells[row, 9].Value.ToString().Trim();
                        string nationality = workSheet.Cells[row, 10].Value.ToString().Trim();
                        string levelName = workSheet.Cells[row, 11].Value.ToString().Trim();
                        string schoolProgrammeCode = workSheet.Cells[row, 12].Value.ToString().Trim();
                        string deptOptionCode = workSheet.Cells[row, 13].Value.ToString().Trim();
                        string sessionName = workSheet.Cells[row, 14].Value.ToString().Trim();
                        string modeofEntry = workSheet.Cells[row, 15].Value.ToString().Trim();


                        if (lastName.Trim().Equals("."))
                        {
                            lastName = "";
                        }
                        if (middleName.Trim().Equals("."))
                        {
                            middleName = "";
                        }
                        if (firstName.Trim().Equals("."))
                        {
                            middleName = "";
                        }

                        var programmeCode = await _db.Programmes.AsNoTracking()
                                            .Where(x => x.ProgrammeCode.ToUpper().Equals(deptOptionCode.ToUpper())).FirstOrDefaultAsync();

                        var schoolProgramme = await _db.SchoolProgrammes.AsNoTracking()
                                            .Where(x => x.SchoolProgrammeCode.ToUpper().Equals(schoolProgrammeCode.ToUpper())).FirstOrDefaultAsync();

                        var uploadedSessionId = await _db.Sessions.AsNoTracking().Where(x => x.SessionName.ToUpper().Equals(sessionName.ToUpper())).FirstOrDefaultAsync();

                        var level = _db.Levels.AsNoTracking().Where(s => s.LevelName.Equals(levelName)).FirstOrDefault();

                        if (!modeofEntry.ToUpper().Equals("PG") && !modeofEntry.ToUpper().Equals("RS") && !modeofEntry.ToUpper().Equals("PF") &&
                            !modeofEntry.ToUpper().Equals("PT") && !modeofEntry.ToUpper().Equals("IJMB") && !modeofEntry.ToUpper().Equals("ICT") &&
                            !modeofEntry.ToUpper().Equals("IDP") && !modeofEntry.ToUpper().Equals("IOE"))
                        {
                            ViewBag.ErrorInfo = "Please Leave no column or row Empty/Blank";
                            ViewBag.ErrorMessage = $" The \"{modeofEntry}\" mode of entry specified in the excel doesn't exist on the portal. " +
                                                   $"Please add the mode of entry first before uploading or correct the excel sheet...";
                            return View("ErrorException");
                        }

                        if (level == null)
                        {
                            ViewBag.ErrorInfo = "Please Leave no column or row Empty/Blank";
                            ViewBag.ErrorMessage = $" The \"{levelName}\" level specified in the excel doesn't exist on the portal. " +
                                                   $"Please add the level first before uploading or correct the excel sheet...";
                            return View("ErrorException");
                        }
                        if (programmeCode == null)
                        {
                            ViewBag.ErrorInfo = "Please Leave no column or row Empty/Blank";
                            ViewBag.ErrorMessage = $" The department Option \"{deptOptionCode}\"  specified in the excel at row {row} doesn't exist on the portal. " +
                                                   $"Please add the Department Option first before uploading or correct the excel sheet...";
                            return View("ErrorException");
                        }
                        if (uploadedSessionId == null)
                        {
                            ViewBag.ErrorInfo = "Please Leave no column or row Empty/Blank";
                            ViewBag.ErrorMessage = $" The Session Name \"{sessionName}\"  specified in the excel at row {row} doesn't exist on the portal. " +
                                                   $"Please add the Session Name first before uploading or correct the excel sheet...";
                            return View("ErrorException");
                        }

                        if (schoolProgramme == null)
                        {
                            ViewBag.ErrorInfo = $"This {schoolProgrammeCode} School programme Code  at row {row} doesn't exist in the system.";
                            ViewBag.ErrorMessage = "Please check the Student Programme type spelling very well ";
                            return View("ErrorException");
                        }

                        var checkStudent = _db.Students.FirstOrDefault(x => x.JambRegNo.Trim().ToUpper().Equals(formNo.ToUpper()));
                        if (checkStudent == null)
                        {
                            try
                            {
                                var student = new Student()
                                {
                                    StudentId = $"{SchoolSetUp.CurrentSchoolName}{studentId}",
                                    JambRegNo = formNo,
                                    FirstName = firstName,
                                    MiddleName = middleName,
                                    LastName = lastName,
                                    Email = email,
                                    StateOfOrigin = state,
                                    Nationality = nationality,
                                    DateOfBirth = dateOfBirth,
                                    Gender = gender,
                                    EnrollmentDate = DateTime.Now,
                                    ProgrammeId = programmeCode.ProgrammeId,
                                    LevelId = level?.LevelId,
                                    StudentStatus = StudentStatus.New_Student.ToString(),
                                    SchoolProgrammeId = schoolProgramme.SchoolProgrammeId,
                                    SessionId = uploadedSessionId.SessionId,
                                    PhoneNumber = phoneNumber,
                                    ModeOfEntry = modeofEntry
                                };
                                _db.Students.Add(student);
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

                    }
                    await _db.SaveChangesAsync();
                    message = $"You have successfully Uploaded {recordCount} records...  and {lastrecord}";
                }
                return RedirectToAction("Index", "Students", new { message });
            }
            ViewBag.Error = $"File type is Incorrect <br/>";
            return View("Index");
        }


        public async Task<ActionResult> GetAllActiveStudents(string programme)
        {
            var students = new List<AllActiveStudentsVM>();
            SchoolFeePaymentVm model = new SchoolFeePaymentVm
            {
                FeeCategory = SchoolFeeCategory.School_Charges.ToString(),
                SessionId = 28
            };

            if (programme == null)
            {
                students = await _db.Students.Include(s => s.Programme.Department)
                                      .Include(s => s.Programme.Department.Faculty)
                                      .Include(s => s.Programme)
                                      .Include(s => s.Level)
                                      .Include(s => s.SchoolProgramme).AsNoTracking()
                                      .Where(s => s.Level.LevelName != null && s.IsGraduated.Equals(false) && s.Active == true && s.IsPhysicallyChallenged == true/*s.Session.SessionName.Equals("2018/2019") s.LevelId == 1 || s.LevelId == 2 && s.SessionId == 20*/)

                                      .Select(s => new AllActiveStudentsVM
                                      {
                                          MatricNO = s.MatricNo,
                                          JambNo = s.JambRegNo,
                                          LevelName = s.Level.LevelName,
                                          Firstname = s.FirstName,
                                          Lastname = s.LastName,
                                          Middlename = s.MiddleName,
                                          FacultyName = s.Programme.Department.Faculty.FacultyName,
                                          Gender = s.Gender,
                                          DeptName = s.Programme.Department.DeptName,
                                          DeptOptionNAme = s.Programme.ProgrammeName,
                                          PhoneNumber = s.PhoneNumber,
                                          ModeOfEntry = s.ModeOfEntry,
                                          State = s.StateOfOrigin,
                                          LocalGovernment = s.Lga,
                                          Nationality = s.Nationality,
                                          MaritalStatus = s.MaritalStatus,
                                          SecondaryEmail = s.Email,
                                          StudentStatus = s.StudentStatus,
                                          PhysicalStatus = s.Programme.FinalLevel.LevelName.ToString(),
                                          SchoolCode = s.SchoolProgramme.FancyName,
                                          DateUploaded = s.EnrollmentDate
                                      }).OrderBy(s => s.FacultyName)
                                      //.OrderBy(s => s.FacultyName)
                                      .ThenBy(s => s.DeptName)
                                      .ThenBy(s => s.DeptOptionNAme)
                                      .ThenBy(s => s.LevelName).ToListAsync();
            }
            else

                students = await _db.Students
                                      .Include(s => s.Programme.Department)
                                      .Include(s => s.Programme.Department.Faculty)
                                      .Include(s => s.Programme)
                                      .Include(s => s.Level)
                                      .Include(s => s.Session)
                                      .Include(s => s.SchoolProgramme).AsNoTracking()
                                      //.Where(s => s.Active.Equals(true) && s.Programme.ProgrammeId == 138 && s.Session.SessionName.Equals("2016/2017") || s.Session.SessionName.Equals("2017/2018") && s.SchoolProgramme.SchoolProgrammeCode.Equals(programme.Trim().ToUpper())/* && s.Level.LevelName != null*/ /*&& s.StateOfOrigin.ToUpper().ToString().Equals("OYO")&& s.Session.SessionName.Equals("2019/2020") s.LevelId == 1 || s.LevelId == 2 && s.SessionId == 20*/)
                                      //.Where(x => x.Nationality.ToUpper().Equals("NIGERIA") || x.Nationality.ToUpper().Equals("NIGERIAN"))
                                      //.Where(x => x.IsGraduated.Equals(true))
                                      //.Where(x => x.Students.MatricNo != null)
                                      //  .Where(x => x.FeeCategory.Equals("School_Charges") && x.Status.Equals(true) && x.SessionId == 24
                                      //&& x.Students.SchoolProgramme.SchoolProgrammeCode.Equals(programme.Trim().ToUpper()) && x.Students.Programme.Department.Faculty.FacultyId == 8)
                                      //.Where(x => x.Session.SessionId == 24)
                                      //.Where(x => x.Status == true""
                                      .Where(x => x.Active.Equals(true) && x.IsDelete.Equals(false) && x.IsGraduated.Equals(false) && x.SchoolProgramme.SchoolProgrammeCode.Equals(programme.Trim().ToUpper()) /*&& x.StateOfOrigin.Trim().ToUpper().Equals("KOGI")*/
                                      && x.StudentId != null)
                                      //.Where(x => x.Students.Programme.Department.Faculty.FacultyId == 7)
                                      //.Where(x => x.Students.SchoolProgramme.SchoolProgrammeCode.Equals(programme.Trim().ToUpper()))
                                      .Select(s => new AllActiveStudentsVM
                                      {
                                          student = _db.Students.Where(x => x.StudentId.Equals(s.StudentId)).FirstOrDefault(),
                                          DepartmentId = s.Programme.Department.DepartmentId,
                                          MatricNO = s.MatricNo,
                                          JambNo = s.JambRegNo,
                                          LevelName = s.Level.LevelName,
                                          Firstname = s.FirstName,
                                          Lastname = s.LastName,
                                          Middlename = s.MiddleName,
                                          FacultyName = s.Programme.Department.Faculty.FacultyName,
                                          Gender = s.Gender,
                                          DeptName = s.Programme.Department.DeptName,
                                          DeptOptionNAme = s.Programme.ProgrammeName,
                                          PhoneNumber = s.PhoneNumber,
                                          ModeOfEntry = s.ModeOfEntry,
                                          State = s.StateOfOrigin,
                                          LocalGovernment = s.Lga,
                                          Nationality = s.Nationality,
                                          MaritalStatus = s.MaritalStatus,
                                          SecondaryEmail = s.Email,
                                          StudentStatus = s.StudentStatus,
                                          Session = s.Session.SessionName,
                                          PhysicalStatus = s.Programme.FinalLevel.LevelName,
                                          DateOfBirth = s.DateOfBirth,
                                          //SchoolFeeCharge =  GenerateFeeListAsync(s, model),
                                          //directEntryExam = _db.DirectEntryExams.FirstOrDefault(x => x.UserId.Equals(s.Email) || x.UserId.Equals(s.PrimaryEmail))
                                      }).OrderBy(s => s.FacultyName)
                                      //.OrderBy(s => s.FacultyName)
                                      .ThenBy(s => s.DeptName)
                                      .ThenBy(s => s.DeptOptionNAme)
                                      .ThenBy(s => s.LevelName).ToListAsync();

            //var assignedRoom = await _db.StudentAssignedRooms.Include(a => a.Block).Include(a => a.Hostel)
            //                            .Include(a => a.Room).Include(a => a.Session).Include(a => a.Student)
            //                            .Where(a => a.Session.SessionId == 28)
            //                            .AsNoTracking().ToListAsync();

            //students = students 
            //       .Where(student => !assignedRoom.Any(x => x.StudentId.Equals(student.StudentId)))
            //       .ToList();
            //var schoolFee = await _db.SchoolFeePayments.Include(i => i.Students).Include(i => i.Session)
            //                        .Include(i => i.Students.Programme.Department).Include(i => i.Students.Level)
            //                        .AsNoTracking().Where(x => x.FeeCategory.Equals(SchoolFeeCategory2.School_Charges.ToString())
            //                        && x.Status.Equals(true) && x.SessionId.Equals(28)
            //                        && x.Students.SchoolProgrammeId.Equals(1) && x.StudentId != null && x.Students.StateOfOrigin.Trim().ToUpper().Equals("KOGI"))
            //                        .OrderBy(s => s.Students.StateOfOrigin)
            //                          //.OrderBy(s => s.FacultyName)
            //                          .ThenBy(s => s.Students.Lga)
            //                          //.ThenBy(s => s.stDeptOptionNAme)
            //                          //.ThenBy(s => s.LevelName)
            //                          .ToListAsync();

            //students = students
            //        .Where(student => !schoolFee.Any(x => x.StudentId.Trim().ToUpper().Equals(student.student.StudentId.Trim().ToUpper())))
            //        .ToList();

            //int count = 0;

            //foreach (var item in students)
            //{
            //    try
            //    {
            //        var x = await GenerateFeeListAsync(item.student, model);
            //        item.SchoolFeeCharge = x;
            //        count++;
            //    }
            //    catch (Exception e)
            //    {

            //        throw;
            //    }

            //}

            return View(students);
        }
        //temporary added 
        private async Task<string> GenerateFeeListAsync(Student student, SchoolFeePaymentVm model)
        {
            var feeList = new List<FeeList>();
            var departmentId = _db.Programmes
                      .Where(x => x.ProgrammeId == (int)student.ProgrammeId)
                      .Select(x => x.DepartmentId)
                      .FirstOrDefault();
            string studentStatus = student.SessionId.Equals(model.SessionId)
                                        ? StudentStatus.New_Student.ToString()
                                        : StudentStatus.Returning.ToString();
            var paymentSetting = await _feeQueryManager.GetPaymentSetting(model.SessionId, student.SchoolProgrammeId, studentStatus);


            // Chech spill-over student logic and Create 50% of SchFee
            var (spillVm, isSpillOver) = await IsSpillOverStudent(student);
            if (isSpillOver)
            {
                // Add specific fees for spill-over student
                feeList.Add(new FeeList { FeeTypeName = "10001", Amount = spillVm.Amount, Description = "School Charges" });
                feeList.Add(new FeeList { FeeTypeName = "0389", Amount = 10000, Description = "NUGA Charges (2 years)" });

                if ((await _feeQueryManager.GetDepartmentPaymentList((short)departmentId, model.SessionId, (Int32)student.LevelId)).Count > 0)
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
                    return feeList.Sum(s => s.Amount).ToString();
                }

                //Check all settings including partPayment
                if (paymentSetting != null && paymentSetting.ConsiderDepartmentalFee && !model.IsPartPayment && model.FeeCategory.Equals(SchoolFeeCategory.School_Charges.ToString()))
                {
                    //check for rogue programmes: int[] numbers = { 1, 2, 3, 4, 5 };
                    int[] labProgrammes = { };
                    if (labProgrammes.Contains((Int32)student.ProgrammeId))
                    {
                        feeList.AddRange(await _feeQueryManager.GetDepartmentPaymentList(80, model.SessionId, (Int32)student.LevelId));
                    }
                    else
                    {
                        feeList.AddRange(await _feeQueryManager.GetDepartmentPaymentList((short)departmentId, model.SessionId, (Int32)student.LevelId));
                    }

                    //Add NUGA Cgarges
                    feeList.Add(new FeeList { FeeTypeName = "0389", Amount = 10000, Description = "NUGA Charges (2 years)" });

                }
                //Add GST Charges to year 2 students
                if (student.LevelId.Equals(2) && student.StudentStatus.Equals(StudentStatus.Returning.ToString())
                    && model.FeeCategory.Equals(SchoolFeeCategory.School_Charges.ToString()) && !student.SessionId.Equals(model.SessionId))
                {
                    feeList.Add(new FeeList { FeeTypeName = "1130", Amount = 5000, Description = "General Studies (GST) Charges" });
                }

            }

            // Add logic for other charges like NUGA, GST, etc.

            return feeList.Sum(s => s.Amount).ToString();
        }
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

        [Authorize]
        [HttpGet]
        public ActionResult NotifyStudents()
        {
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FancyName");
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            return View();
        }

        [Authorize]
        [HttpGet]
        public ActionResult NotifyStudentsSMS()
        {
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FancyName");
            ViewBag.FacultyId = new SelectList(_db.Faculties.AsNoTracking().ToList(), "FacultyId", "FacultyName");
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateInput(false)]
        public async Task<ActionResult> NotifyStudents(ApplicantNotificationVm model)
        {
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FancyName");

            if (ModelState.IsValid)
            {
                var applicants = await _db.Students.AsNoTracking()
                                .Where(x => x.SchoolProgramme.SchoolProgrammeId.Equals(model.SchoolProgrammeId)
                                && x.Session.SessionId.Equals(model.SessionId) && x.Active.Equals(true)).ToListAsync();
                foreach (var applicant in applicants)
                {
                    if (!string.IsNullOrEmpty(applicant.Email))
                    {
                        await NotifyApplicantByEmail(applicant.Email, applicant.FullName, model.Subject,
                                model.Body, applicant.StudentId);
                    }
                }

                //await NotifyApplicantByEmail("kunlesymls@gmail.com", "Joseph Ajileye", model.Subject,
                //                model.Body, "AbC123");
                //return new JsonResult { Data = new { status = true, message = $" Email Message has been sent successfully" } };
                return new JsonResult { Data = new { status = true, message = $"{applicants.Count} Email Message has been sent successfully" } };
            }

            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FancyName");
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateInput(false)]
        public async Task<ActionResult> NotifyStudentsSMS(ApplicantNotificationVm model)
        {
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FancyName");

            //if (ModelState.IsValid)
            //{
            var students = await _db.Students.AsNoTracking()
                            .Include(x => x.Programme.Department.Faculty)
                            .Include(x => x.Session)
                            .Where(x => x.SchoolProgramme.SchoolProgrammeId.Equals(model.SchoolProgrammeId)
                            //&& x.Session.SessionId.Equals(model.SessionId)
                            && x.Active.Equals(true)
                            //& x.StudentStatus.Equals(StudentStatus.New_Student.ToString())
                            && x.Programme.Department.Faculty.FacultyId.Equals(model.FacultyId)
                            //&& x.IsGraduated.Equals(false)
                            //&& x.IsDelete.Equals(false)
                            && x.PhoneNumber != null).ToListAsync();
            if (model.SessionId != null)
            {
                students = students.Where(x => x.Session.SessionId.Equals(model.SessionId)).ToList();
            }

            foreach (var applicant in students)
            {
                if (applicant.PhoneNumber != null)
                {
                    await SMSClass.SendSMS("UNIJOS", $"Dear {applicant.FirstName}{model.Body}", applicant.PhoneNumber); //EBULK SMS API
                }

            }

            //await NotifyApplicantByEmail("kunlesymls@gmail.com", "Joseph Ajileye", model.Subject,
            //                model.Body, "AbC123");
            //return new JsonResult { Data = new { status = true, message = $" Email Message has been sent successfully" } };
            return new JsonResult { Data = new { status = true, message = $"{students.Count} SMS has been sent successfully" } };

        }

        [Authorize]
        [ValidateInput(false)]
        public async Task DownloadNotifyStudents(int SchoolProgrammeId, int SessionId)
        {
            var applicants = await _db.Students.Include(i => i.SchoolProgrammeId).AsNoTracking()
                                .Where(x => x.SchoolProgramme.SchoolProgrammeId.Equals(SchoolProgrammeId)
                                && x.Session.SessionId.Equals(SessionId)).ToListAsync();

            char c1 = 'A';
            ExcelPackage package = new ExcelPackage();
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Report");

            worksheet.Cells[$"{c1++}1"].Value = "S/N";
            worksheet.Cells[$"{c1++}1"].Value = "Form No";
            worksheet.Cells[$"{c1++}1"].Value = "Programme Name";
            worksheet.Cells[$"{c1++}1"].Value = "FullName";
            worksheet.Cells[$"{c1++}1"].Value = "Email";
            worksheet.Cells[$"{c1++}1"].Value = "Phone Number";

            int rowStart = 2;
            //char c2 = 'A';

            for (var i = 0; i < applicants.Count; i++)
            {
                worksheet.Cells[$"A{rowStart}"].Value = i + 1;
                worksheet.Cells[$"B{rowStart}"].Value = applicants[i].StudentId;
                worksheet.Cells[$"C{rowStart}"].Value = applicants[i].SchoolProgramme.FullName;
                worksheet.Cells[$"D{rowStart}"].Value = applicants[i].FullName;
                worksheet.Cells[$"E{rowStart}"].Value = applicants[i].PrimaryEmail;
                worksheet.Cells[$"F{rowStart}"].Value = applicants[i].PhoneNumber;
                rowStart++;
            }

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
        public ActionResult SuperAdminDashBoard()
        {
            return View();
        }

        [HttpGet]
        public ActionResult BatchStudentsLevelMigration()
        {
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FancyName");
            ViewBag.FacultyId = new SelectList(_db.Faculties.AsNoTracking(), "FacultyId", "FacultyName");
            ViewBag.LevelId = new SelectList(_db.Levels.AsNoTracking(), "LevelId", "LevelName");
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> BatchStudentsLevelMigration(int SchoolProgrammeId, int FacultyId, int? LevelId)
        {

            var message = string.Empty;
            var status = false;
            var studentsToMigrateLevel = new List<Student>();
            if (LevelId != null)
            {
                studentsToMigrateLevel = await _db.Students.Include(s => s.SchoolProgramme)
                                                           .Include(s => s.Programme.Department.Faculty)
                                                           .Include(s => s.Level)
                                                           .Where(s => s.SchoolProgrammeId.Equals(SchoolProgrammeId)
                                                           && s.Programme.Department.Faculty.FacultyId.Equals(FacultyId)
                                                           && s.LevelId != null
                                                           && s.Level.LevelId.Equals((int)LevelId)
                                                           && s.Active.Equals(true) && s.IsGraduated.Equals(false)
                                                           && s.ProgrammeId != null /*&& s.SessionId != 24*/).ToListAsync();
            }
            else
            {
                studentsToMigrateLevel = await _db.Students.Include(s => s.SchoolProgramme)
                                                          .Include(s => s.Programme.Department.Faculty)
                                                          .Include(s => s.Level)
                                                          .Where(s => s.SchoolProgrammeId.Equals(SchoolProgrammeId)
                                                          && s.Programme.Department.Faculty.FacultyId.Equals(FacultyId)
                                                          && s.Active.Equals(true) && s.IsGraduated.Equals(false) && s.LevelId != null
                                                          && s.ProgrammeId != null /*&& s.SessionId != 24*/).ToListAsync();

            }

            foreach (var item in studentsToMigrateLevel)
            {
                var studentCurrentLevel = item.Level.LevelName;
                var studentProgrammeDuration = _db.Programmes.Include(l => l.FinalLevel).Where(d => d.ProgrammeId.Equals((int)item.ProgrammeId)).FirstOrDefault();
                var nextLevel = "";
                switch (studentCurrentLevel)
                {
                    case "100":
                        nextLevel = "200";
                        break;
                    case "200":
                        nextLevel = "300";
                        break;
                    case "300":
                        nextLevel = "400";
                        break;
                    case "400":
                        nextLevel = "500";
                        break;
                    default:
                        nextLevel = "600";
                        break;
                }

                var FinalLevelNumber = getLeveltNumber(studentProgrammeDuration.FinalLevel.LevelName);
                var nextLevelNumber = getLeveltNumber(nextLevel);

                if (nextLevelNumber > FinalLevelNumber)
                {
                    item.IsGraduated = true;
                    _db.Entry(item).State = EntityState.Modified;
                    message = $"Students for faculty of {item.Programme.Department.Faculty.FacultyName} Level Migrated Successful!";
                }
                else
                {
                    var nextLevelId = _query.GetLevelByName(nextLevel);
                    item.LevelId = nextLevelId;
                    _db.Entry(item).State = EntityState.Modified;
                    message = $"Students for faculty of {item.Programme.Department.Faculty.FacultyName} Level Migrated Successful!";
                }

            }
            try
            {
                await _db.SaveChangesAsync();
                status = true;
            }
            catch (Exception ex)
            {
                ViewBag.ErrorInfo = $"Noxamudiana";
                return new JsonResult { Data = new { status, message = $"Level Not Changed" } };
            }
            return new JsonResult { Data = new { status, message } };
            //return View();
        }

        [HttpGet]
        [Authorize(Roles = RoleName.Admin + "," + RoleName.DatabaseAdmin + "," + RoleName.SuperAdmin + "," + RoleName.AdmissionUploadOfficer)]
        public ActionResult FlagRemsStudent(string message)
        {
            ViewBag.Message = message;
            return View();
        }

        [Authorize(Roles = RoleName.Admin + "," + RoleName.DatabaseAdmin + "," + RoleName.SuperAdmin + "," + RoleName.AdmissionUploadOfficer)]
        public async Task<ActionResult> FlagRemsStudent(HttpPostedFileBase excelfile)
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

                        var student = await _db.Students.AsNoTracking()
                                            .Where(x => x.JambRegNo.Trim().ToUpper().Equals(matNo.ToUpper()))
                                            .SingleOrDefaultAsync();

                        if (student != null)
                        {
                            student.IsDelete = false;
                            _db.Entry(student).State = EntityState.Modified;

                            utmeUploadError.Add(new StudentUtmeUploadErrorVm { JambRegNo = student.JambRegNo, Row = row });
                            hasUtmeUploadError = true;
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
                        ViewBag.ErrorMessage = $"You have successfully Flagged {recordCount} records...";
                        return View("ErrorException", utmeUploadError);
                    }
                    message = $"You have successfully Flagged {recordCount} records...  and {lastrecord}";
                    ViewBag.Message = message;
                    return RedirectToAction("Index", "Students", new { message });
                }
            }
            ViewBag.Error = $"File type is Incorrect <br/>";
            return View("Index");
        }

        public int getLeveltNumber(string level)
        {
            char result = level[0];
            int finalResult = (Int32)result;
            return finalResult;
        }

        public ActionResult IndexSearch()
        {
            return View();
        }

        public async Task<ActionResult> ShowNewStudentsRegReort()
        {
            var model = new List<RegistrationReportVm>();
            //var students = await _db.Students.Include(a => a.Programme).Include(a => a.Programme.Department).Include(a => a.Session).AsNoTracking()
            //                                 .Where(s => s.Session.SessionId.Equals(20) && !string.IsNullOrEmpty(s.MatricNo)).ToListAsync();
            var deptStudents = await _db.Departments.Where(x => x.DeptCode.Length == 6).AsNoTracking().ToListAsync();
            var progStudents = await _db.Programmes.Where(x => x.ProgrammeCode.Length == 10).AsNoTracking().ToListAsync();
            int count = 0;

            foreach (var dept in deptStudents)
            {
                //var studentDept = _db.Students.Include(s => s.Programme.Department).Include(s => s.Level).Include(s => s.Session)
                //                     .Where(s => s.Programme.Department.DepartmentId.Equals(dept.DepartmentId)
                //                     && s.SchoolProgramme.SchoolProgrammeCode.Equals("UG") && s.Active.Equals(true)
                //                     && s.IsDelete.Equals(false)
                //                     && s.Session.SessionId.Equals(24) && !string.IsNullOrEmpty(s.MatricNo)).ToList();

                var studentProg = _db.Students.Include(s => s.Programme).Include(s => s.Level).Include(s => s.Session).Include(s => s.SchoolFeePayments)
                                     .Where(s => s.Programme.Department.DepartmentId.Equals(dept.DepartmentId)
                                     && s.SchoolProgramme.SchoolProgrammeCode.Equals("PGD")
                                     && s.IsDelete.Equals(false)
                                     && s.Active.Equals(true)
                                     /*&& s.Session.SessionId.Equals(24)*/ && !string.IsNullOrEmpty(s.MatricNo)).ToList();

                var studentNotReg = _db.Students.Include(s => s.Programme).Include(s => s.Level).Include(s => s.Session)
                                     .Where(s => s.Programme.ProgrammeId.Equals(dept.DepartmentId)
                                     && s.SchoolProgramme.SchoolProgrammeCode.Equals("UG")
                                     && s.IsDelete.Equals(false)
                                     && s.Session.SessionId.Equals(24)).ToList();

                model.Add(new RegistrationReportVm
                {
                    Id = ++count,
                    DepartmentName = dept.DeptName,
                    //DERegCount = studentProg.Where(x => x.Level.LevelId == 2).Count(),
                    //UTMERegCount = studentProg.Where(x => x.Level.LevelId == 1).Count(),
                    //TotalRegistered = studentProg.Where(x => x.Level.LevelId == 2).Count() + studentProg.Where(x => x.Level.LevelId == 1).Count(),
                    TotalRegistered = studentProg.Count(),
                    //TotalAdmitted = studentNotReg.Count()
                });
            }

            return View(model);
        }
        public async Task<ActionResult> NotifyUnRegisteredNewStudents()

        {
            var model = new List<RegistrationReportVm>();
            //var students = await _db.Students.Include(a => a.Programme).Include(a => a.Programme.Department).Include(a => a.Session).AsNoTracking()
            //                                 .Where(s => s.Session.SessionId.Equals(20) && !string.IsNullOrEmpty(s.MatricNo)).ToListAsync();
            var deptStudents = await _db.Departments.Where(x => x.DeptCode.Length == 6).AsNoTracking().ToListAsync();
            var progStudents = await _db.Programmes.Where(x => x.ProgrammeCode.Length == 10).AsNoTracking().ToListAsync();
            int count = 0;
            foreach (var dept in progStudents)
            {
                var studentDept = _db.Students.Include(s => s.Programme.Department).Include(s => s.Level).Include(s => s.Session)
                                     .Where(s => s.Programme.ProgrammeId.Equals(dept.ProgrammeId)
                                     && s.SchoolProgramme.SchoolProgrammeCode.Equals("UG") /*&& s.Active.Equals(true)*/
                                     && s.IsDelete.Equals(false)
                                     && s.Session.SessionId.Equals(20) /*&& !string.IsNullOrEmpty(s.MatricNo)*/).ToList();

                var studentProg = _db.Students.Include(s => s.Programme).Include(s => s.Level).Include(s => s.Session)
                                     .Where(s => s.Programme.ProgrammeId.Equals(dept.ProgrammeId)
                                     && s.SchoolProgramme.SchoolProgrammeCode.Equals("UG") && s.Active.Equals(true)
                                     && s.IsDelete.Equals(false) /*&& s.IsClearedAcademics.Equals(false)*/
                                     && s.Session.SessionId.Equals(20) && !string.IsNullOrEmpty(s.MatricNo)).ToList();
                model.Add(new RegistrationReportVm
                {
                    Id = ++count,
                    DepartmentName = dept.ProgrammeName,
                    DERegCount = studentProg.Where(x => x.Level.LevelId == 2).Count(),
                    UTMERegCount = studentProg.Where(x => x.Level.LevelId == 1).Count(),
                    TotalRegistered = studentProg.Where(x => x.Level.LevelId == 2).Count() + studentProg.Where(x => x.Level.LevelId == 1).Count(),
                    NotRegistered = studentDept.Where(x => x.Signature == null).Count(),
                    TotalAdmitted = studentDept.Count()
                });
            }

            return View(model);
        }

        public async Task<ActionResult> getDisabledStudents()
        {
            var disabledCount = await _db.SchoolFeePayments.Include(s => s.Students)
                                            .Include(s => s.Session)
                                            .Include(s => s.Students.SchoolProgramme)
                                            .Where(d => d.Students.IsPhysicallyChallenged == true && d.Students.SchoolProgramme.SchoolProgrammeId == 1 /*&& d.Session.SessionName.Equals("2018/2019")*/).CountAsync();
            return View(disabledCount);
        }
        public ActionResult NotifyPGSchoolFee()
        {
            return View();
        }

        [Authorize(Roles = RoleName.Admin + "," + RoleName.DatabaseAdmin + "," + RoleName.SuperAdmin + "," + RoleName.AdmissionUploadOfficer)]
        public async Task<ActionResult> NotifyPGSchoolFeePaymentDeadline(HttpPostedFileBase excelfile)
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
                        var fee = workSheet.Cells[row, 2].Value.ToString().Trim();
                        var StudentType = workSheet.Cells[row, 3].Value.ToString().Trim();

                        var student = await _db.Students.Include(x => x.SchoolProgramme).AsNoTracking()
                                            .Where(x => x.MatricNo.Trim().ToUpper().Equals(matNo.ToUpper()) || x.JambRegNo.Trim().ToUpper().Equals(matNo.ToUpper()))
                                            .FirstOrDefaultAsync();
                        //var schoolFee = student.SchoolProgramme.SchoolFeeTypes.Where(x => x.)


                        if (student != null)
                        {
                            var feeType = new List<ReportFeeListVm>();
                            decimal TaltoalFeeAmonut = 0;
                            var feeList = await _feeQueryManager.GetSchoolFeeListReport(fee, student.SchoolProgrammeId, 1, StudentType);
                            foreach (var item in feeList)
                            {
                                TaltoalFeeAmonut += item.Amount;
                            }

                            var matOrFormNo = student.MatricNo != null ? student.MatricNo : student.JambRegNo;
                            if (student.PhoneNumber != null)
                            {
                                await SMSClass.SendSMS("UNIJOS", $"Dear {student.FirstName} ({matOrFormNo}) you have till 31 August 2020 to pay your {fee} of 234,000 for UNIJOS {student.SchoolProgramme.FancyName}. Failure to pay on or before 31st August, you are at the risk of forfeiting your admission/studentship. Call the Financial Officer on 08037001597 UNIJOS PG Sch for more info.", student.PhoneNumber); //EBULK SMS API
                            }
                            else
                            {
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
                        //await _db.SaveChangesAsync();

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
        public async Task<ActionResult> getAllRejected()
        {
            var students = new List<Student>();
            var list = await _db.RejectedStudents.ToListAsync();
            foreach (var item in list)
            {
                Student getStudent = _db.Students.Include(s => s.SchoolProgramme).Where(s => s.StudentId.Equals(item.StudentId)).FirstOrDefault();
                if (getStudent.SchoolProgramme.SchoolProgrammeCode == "MP" || getStudent.SchoolProgramme.SchoolProgrammeCode == "DP" || getStudent.SchoolProgramme.SchoolProgrammeCode == "PP")
                {
                    students.Add(getStudent);
                }
            }
            return View(students);
        }

        public string setProp(string id)
        {
            var checkStudent = _db.Students.Include(i => i.SchoolProgramme)
                                .FirstOrDefault(x => x.Email.Trim().ToUpper().Equals(id.ToUpper().Trim())
                                && x.Active.Equals(true) && x.IsGraduated.Equals(false));

            //checkStudent.IsRemedialStudent = null;
            checkStudent.IsClearedAcademics = true;
            checkStudent.IsClearedDepartment = true;
            checkStudent.IsClearedFaculty = true;
            _db.Entry(checkStudent).State = EntityState.Modified;
            _db.SaveChanges();
            return "success";
        }

        public ActionResult BloodGroupList()
        {
            return View();
        }

        // Save blood group that student entered manually
        [HttpPost]
        public async Task<ActionResult> BloodGroupList(string bloodGroup)
        {
            var message = "";
            Student student = await _db.Students.Include(i => i.Programme).Include(i => i.Level).Include(i => i.Session)
                                        .Include(i => i.Programme.Department.Faculty)
                                        .Include(i => i.Level).Include(i => i.SchoolProgramme)/*.AsNoTracking()*/
                                        .FirstOrDefaultAsync(s => s.Email.Trim().ToUpper().Equals(userId.Trim().ToUpper()));

            student.BloodGroup = bloodGroup;
            _db.Entry(student).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            message = $"{student.FullName} blood group saved successfully";
            return new JsonResult { Data = new { status = true, message } };
        }

        // Update student phone number
        [Authorize(Roles = RoleName.Admin + "," + RoleName.TSupport)]
        public async Task<ActionResult> UpdatePhoneNumber(string id, string phoneNumber)
        {
            if (id == null || phoneNumber == null)
                ViewBag.message = "Matric No and Phone Number cannot be empty";

            else
            {
                var student = await _db.Students.Where(s => s.MatricNo.ToUpper().Equals(id.Trim().ToUpper()) ||
                                            s.JambRegNo.ToUpper().Equals(id.Trim().ToUpper())).FirstOrDefaultAsync();

                if (student == null)
                    ViewBag.message = "No student found with such details";

                else
                {
                    student.PhoneNumber = phoneNumber;
                    _db.Entry(student).State = EntityState.Modified;
                    await _db.SaveChangesAsync();

                    ViewBag.message = $"{student.FullName} phone number updated successfully";

                }
            }
            return View("UpdatePhoneNumber");
        }

        // Get a student matric number
        [Authorize(Roles = RoleName.Admin + "," + RoleName.TSupport)]
        public async Task<ActionResult> GetMatricNo(string id)
        {
            if (id == null)
            {
                ViewBag.message = "Jamb Number cannot be null";
            }
            else
            {
                var student = await _db.Students.Where(s => s.JambRegNo.Trim().ToUpper().Equals(id.Trim().ToUpper())).FirstOrDefaultAsync();

                if (student == null)
                    ViewBag.message = "No student found with such details";
                else
                {
                    if (student.MatricNo == null)
                        ViewBag.message = $"{student.FullName} has no matric number";
                    else
                    {
                        ViewBag.message = $"{student.FullName} matric number is {student.MatricNo}";
                    }
                }
            }
            return View();
        }

        // Get duplicate matric numbers
        [Authorize(Roles = RoleName.Admin + "," + RoleName.TSupport)]
        public async Task<ActionResult> GetDuplicateMatricNo(string id)
        {
            if (id == null)
            {
                ViewBag.message = "Matric Number cannot be null";
            }
            else
            {
                var students = await _db.Students.Where(s => s.MatricNo.Trim().ToUpper().Equals(id.Trim().ToUpper())
                                                  || s.StudentId.Equals(id.Trim().ToUpper())).ToListAsync();

                if (students.Count() == 0)
                    ViewBag.message = "No students found with such details";
                else
                {
                    return View(students);
                }
            }
            return View();
        }

        // Fix Matric number clash
        [Authorize(Roles = RoleName.Admin + "," + RoleName.TSupport)]
        public async Task<ActionResult> FixDuplicateMatricNo(string id)
        {
            var student = await _db.Students.Where(s => s.StudentId.Trim().ToUpper().Equals(id.Trim().ToUpper())).FirstOrDefaultAsync();
            string primaryEmail = student.PrimaryEmail;

            student.MatricNo = null;
            student.StudentStatus = "New_Student";

            _db.Entry(student).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            ViewBag.message = $"{student.FullName} matric number removed successfully";

            var editStudent = await _studentQuery.SaveAndGenerateMatricNo(id);

            // Notify student of new matric Number via email and text
            var emailService = new EmailService();
            string body = $"{student.FirstName} Your Mat-Num is {editStudent.MatricNo} and email is {editStudent.Email} Login with the new email and your previous password";

            await emailService.SendAsync(new IdentityMessage
            {
                Destination = primaryEmail,
                Body = $"Your Matric No {editStudent.MatricNo} has been generated successfully, a new University of Jos Email {editStudent.Email} has been generated." +
               $"You are now to login with this generated UNIJOS Email on the portal as your username with initial password to proceed. Visit portal.unijos.edu.ng for details",
                Subject = "MARIC NO GENERATED"
            });

            await SMSClass.SendSMS("UNIJOS SIS", body, editStudent.PhoneNumber); //EBULK SMS API

            return RedirectToAction("GetDuplicateMatricNo");
        }


        // This guy is responsible for fixing issue that occurs when students see "New_Student" status, despite having a matric number
        public async Task<ActionResult> WrongStudentStatus()
        {

            var students = await _db.Students.Where(x => x.Active == true && x.IsClearedFaculty == true && x.Session.SessionName == "2020/2021" && x.StudentStatus == "New_Student" && x.SchoolProgrammeId == 1).ToListAsync();

            foreach (var student in students)
            {
                student.StudentStatus = "Returning";

                _db.Entry(student).State = EntityState.Modified;
                await _db.SaveChangesAsync();
            }

            return View();
        }


        // Generate report for all active students for different programmes and export as excel file
        [Authorize(Roles = RoleName.Admin)]
        public string AllStudentsReport()
        {
            var student = _db.Students.Include(i => i.Programme).Include(i => i.Level).Include(i => i.Session)
                                        .Include(i => i.Programme.Department.Faculty)
                                        .Include(i => i.Level).Include(i => i.SchoolProgramme)
                                        .Where(x => x.SchoolProgramme.FancyName == "Undergraduate Full-Time" && x.Active == true).ToList();

            char c1 = 'A';
            ExcelPackage package = new ExcelPackage();
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("StudentReport");

            //worksheet.Cells[$"{c1++}1"].Value = "No";
            worksheet.Cells[$"{c1++}1"].Value = "Matric No";
            worksheet.Cells[$"{c1++}1"].Value = "First Name";
            worksheet.Cells[$"{c1++}1"].Value = "Last Name";
            worksheet.Cells[$"{c1++}1"].Value = "Faculty";
            worksheet.Cells[$"{c1++}1"].Value = "Department";
            worksheet.Cells[$"{c1++}1"].Value = "Course";
            worksheet.Cells[$"{c1++}1"].Value = "Current Level";
            worksheet.Cells[$"{c1++}1"].Value = "Phone Number";
            worksheet.Cells[$"{c1++}1"].Value = "Email";
            worksheet.Cells[$"{c1++}1"].Value = "Gender";

            int rowStart = 2;

            for (var i = 0; i < student.Count - 1; i++)
            {
                //worksheet.Cells[$"A{rowStart}"].Value = i++;
                worksheet.Cells[$"A{rowStart}"].Value = student[i].MatricNo;
                worksheet.Cells[$"B{rowStart}"].Value = student[i].FirstName;
                worksheet.Cells[$"C{rowStart}"].Value = student[i].LastName;
                worksheet.Cells[$"D{rowStart}"].Value = student[i].Programme.Department.Faculty.FacultyName;
                worksheet.Cells[$"E{rowStart}"].Value = student[i].Programme.Department.DeptName;
                worksheet.Cells[$"F{rowStart}"].Value = student[i].Programme.ProgrammeName;
                worksheet.Cells[$"G{rowStart}"].Value = student[i].Level.LevelName;
                worksheet.Cells[$"H{rowStart}"].Value = student[i].PhoneNumber;
                worksheet.Cells[$"I{rowStart}"].Value = student[i].Email;
                worksheet.Cells[$"J{rowStart}"].Value = student[i].Gender;
                rowStart++;
            }

            worksheet.Cells["A:AZ"].AutoFitColumns();
            worksheet.Column(1).Style.Locked = true;
            worksheet.Column(2).Style.Locked = true;
            worksheet.Column(3).Style.Locked = true;
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment: filename=" +
                                                      $"StudentReport.xlsx");
            Response.BinaryWrite(package.GetAsByteArray());
            Response.End();

            return "Success";
        }


        // Generate report for all staff and export as excel file
        [Authorize(Roles = RoleName.Admin)]
        public string AllStaffReport()
        {
            var staff = _db.Staffs.Include(s => s.Department).Include(s => s.Department.Faculty).ToList();

            char c1 = 'A';
            ExcelPackage package = new ExcelPackage();
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("StaffReport");

            //worksheet.Cells[$"{c1++}1"].Value = "No";
            worksheet.Cells[$"{c1++}1"].Value = "File No";
            worksheet.Cells[$"{c1++}1"].Value = "First Name";
            worksheet.Cells[$"{c1++}1"].Value = "Last Name";
            worksheet.Cells[$"{c1++}1"].Value = "Faculty";
            worksheet.Cells[$"{c1++}1"].Value = "Department";
            worksheet.Cells[$"{c1++}1"].Value = "Phone Number";
            worksheet.Cells[$"{c1++}1"].Value = "Email";
            worksheet.Cells[$"{c1++}1"].Value = "Gender";

            int rowStart = 2;

            for (var i = 0; i < staff.Count - 1; i++)
            {
                //worksheet.Cells[$"A{rowStart}"].Value = i++;
                worksheet.Cells[$"A{rowStart}"].Value = staff[i].StaffId;
                worksheet.Cells[$"B{rowStart}"].Value = staff[i].FirstName;
                worksheet.Cells[$"C{rowStart}"].Value = staff[i].LastName;
                if (staff[i].Department == null)
                {
                    worksheet.Cells[$"D{rowStart}"].Value = "";
                    worksheet.Cells[$"E{rowStart}"].Value = "";
                }
                else
                {
                    worksheet.Cells[$"D{rowStart}"].Value = staff[i].Department.Faculty.FacultyName;
                    worksheet.Cells[$"E{rowStart}"].Value = staff[i].Department.DeptName;
                }
                worksheet.Cells[$"F{rowStart}"].Value = staff[i].PhoneNumber;
                worksheet.Cells[$"G{rowStart}"].Value = staff[i].Email;
                worksheet.Cells[$"H{rowStart}"].Value = staff[i].Gender;
                rowStart++;
            }

            worksheet.Cells["A:AZ"].AutoFitColumns();
            worksheet.Column(1).Style.Locked = true;
            worksheet.Column(2).Style.Locked = true;
            worksheet.Column(3).Style.Locked = true;
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment: filename=" +
                                                      $"StaffReport.xlsx");
            Response.BinaryWrite(package.GetAsByteArray());
            Response.End();

            return "Success";
        }


        public ActionResult GetCurrentSessionId(int programmeId)
        {
            var currentSessionId = _currentSession.GetCurrentSessionId(programmeId);
            //var sessions = _db.Sessions.ToList();
            //return new JsonResult { Data = new { status = true, sessions, JsonRequestBehavior.AllowGet } };
            return Json(new { Data = currentSessionId }, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> StudentFeeAndCourseRegReport()
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

        [HttpPost]
        public ActionResult GetStudentCourseAndFeeReport(int SchoolProgrammeId, int? DepartmentId, int? LevelId, int? SessionId, int? FacultyId)
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

            var courseReg = _db.Students.Include(x => x.Programme.Department.Faculty)
                                    .Include(c => c.Programme.Department).Include(c => c.Level)
                                    .Include(i => i.Programme).Include(i => i.SchoolProgramme).AsNoTracking()
                                    .Where(x => x.SchoolProgramme.SchoolProgrammeId.Equals(SchoolProgrammeId) && x.Active.Equals(true) && x.IsGraduated.Equals(false))
                                    .OrderBy(o => o.Programme.Department.DeptName).ThenBy(o => o.Level.LevelName).ToList();

            if (FacultyId != null)
            {
                courseReg = courseReg.Where(x => x.Programme.Department.Faculty.FacultyId.Equals((int)FacultyId)
                                                 /*&& x.Students.LevelId.Equals((int)LevelId)*/).ToList();
            }

            //if (DepartmentId != null & LevelId != null)
            //{
            //    courseReg = courseReg.Where(x => x.Students.Programme.Department.DepartmentId.Equals((int)DepartmentId)
            //                                     && x.Students.LevelId.Equals((int)LevelId)).ToList();
            //}
            if (DepartmentId != null)
            {
                courseReg = courseReg.Where(x => x.Programme.Department.DepartmentId.Equals((int)DepartmentId)).ToList();
            }

            if (LevelId != null)
            {
                courseReg = courseReg.Where(x => x.Level.LevelId.Equals((int)LevelId)).ToList();
            }

            var records = new List<StudentSchChargesAndCourseRegVm>();

            foreach (var item in courseReg)
            {
                records.Add(
                    new StudentSchChargesAndCourseRegVm
                    {
                        MatricNum = item.MatricNo,
                        fullname = item.FullName,
                        DeptName = item.Programme.Department.DeptName,
                        Programme = item.Programme.ProgrammeName,
                        Level = item.Level.LevelName,
                        PaymentSatus = _db.SchoolFeePayments.Where(x => x.StudentId.Equals(item.StudentId) && x.SessionId.Equals((int)SessionId)).FirstOrDefault() != null ? "Paid" : "Not Paid",
                        CourseRegStatus = _db.CourseRegistrations.Where(x => x.StudentId.Equals(item.StudentId) && x.SessionId.Equals((int)SessionId)).FirstOrDefault() != null ? "True" : "False"
                    });
            }

            //var data = courseReg.Select(s => new StudentSchChargesAndCourseRegVm
            //{
            //    MatricNum = s.MatricNo,
            //    fullname = s.FullName,
            //    DeptName = s.Programme.Department.DeptName,
            //    Programme = s.Programme.ProgrammeName,
            //    Level = s.Level.LevelName,
            //    PaymentSatus = s.SchoolFeePayments == null ? "Paid" : "Not Paid",
            //    CourseRegStatus = s.CourseRegistrations == null ? "True" : "False"

            //}).ToList();
            totalRecords = records.Count();
            var data = records.Skip(skip).Take(pageSize).ToList();

            return Json(new { draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data },
                JsonRequestBehavior.AllowGet);
        }

        public async Task<String> TestSMSApi()
        {
            await SMSClass.SendSMS("UNIJOS SIS", "Api SMS Check!!!", "07035473090"); //EBULK SMS API
            return "success";
        }

        // Get the details of a single student
        [Authorize(Roles = RoleName.Admin + "," + RoleName.SuperAdmin + "," + RoleName.TSupport)]
        public ActionResult FindStudent()
        {
            return View();
        }

        // Get the details of a single student using matric/jamb/form number
        [Authorize(Roles = RoleName.Admin + "," + RoleName.SuperAdmin + "," + RoleName.TSupport)]
        [HttpPost]
        public async Task<ActionResult> FindStudent(string id)
        {
            var studentId = id.Trim().ToUpper();
            var student = await _db.Students.Include(x => x.SchoolProgramme)
                                            .Include(x => x.Programme)
                                            .Include(x => x.Programme.Department)
                                            .Include(x => x.Programme.Department.Faculty)
                                            .Include(x => x.Session)
                                            .Include(x => x.Level)
                                            .Where(x => x.JambRegNo.ToUpper() == studentId || x.MatricNo.ToUpper() == studentId)
                                            .Select(x => new
                                            {
                                                StudentId = x.StudentId,
                                                MatricNo = x.MatricNo,
                                                JambRegNo = x.JambRegNo,
                                                Email = x.Email,
                                                PrimaryEmail = x.PrimaryEmail,
                                                LastName = x.LastName,
                                                FirstName = x.FirstName,
                                                MiddleName = x.MiddleName,
                                                DateOfBirth = x.DateOfBirth.ToString(),
                                                SchoolProgramme = x.SchoolProgramme.FancyName,
                                                Faculty = x.Programme.Department.Faculty.FacultyName,
                                                Department = x.Programme.Department.DeptName,
                                                Programme = x.Programme.ProgrammeName,
                                                Level = x.Level.LevelName,
                                                Session = x.Session.SessionName,
                                                ModeOfEntry = x.ModeOfEntry,
                                                StudentStatus = x.StudentStatus,
                                                Active = x.Active,
                                                IsGraduated = x.IsGraduated,
                                                IsClearedDepartment = x.IsClearedDepartment,
                                                IsClearedFaculty = x.IsClearedFaculty,
                                                IsClearedAcademics = x.IsClearedAcademics,
                                                Gender = x.Gender,
                                                Passport = x.Passport,
                                            })
                                            .FirstOrDefaultAsync();

            //var studentDetails = JsonConvert.SerializeObject(student, Formatting.None, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

            return new JsonResult { Data = new { student } };
        }

        // Soft delete UG students without MatNo
        [Authorize(Roles = RoleName.Admin)]
        public async Task<string> SoftDeleteStudents()
        {
            var students = await _db.Students
                                .Where(x => x.Active == true && x.SchoolProgrammeId == 1 &&
                                        x.IsDelete == false && x.IsGraduated == false
                                        && x.MatricNo == null && x.LevelId > 2)
                                .ToListAsync();

            //var count = 0;

            foreach (var item in students)
            {
                item.IsDelete = true;
                _db.Entry(item).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                //count++;
            }

            return "Success";
        }

        // Change email of 2018 students still using personal email to login
        [Authorize(Roles = RoleName.Admin)]
        public async Task<string> ChangeStudentsEmail()
        {
            var students = await _db.Students.Where(x => x.SessionId == 1 && x.SchoolProgrammeId == 1 && x.IsGraduated == false
                                                && x.IsDelete == false && x.PrimaryEmail != null && x.Email != x.PrimaryEmail && !(x.Email.Contains("@unijos.edu.ng")))
                                                .ToListAsync();

            foreach (var item in students)
            {
                var primaryEmail = item.PrimaryEmail;
                var emailSignUp = item.Email;
                var user = await _db.Users.AsNoTracking()
                                                        .Where(x => x.Email.ToUpper().Equals(item.Email.ToUpper()) ||
                                                        x.StudentId.Equals(item.StudentId))
                                                        .FirstOrDefaultAsync();

                if (user?.Email != null)
                {
                    user.Email = primaryEmail;
                    user.UserName = primaryEmail;
                    _db.Entry(user).State = EntityState.Modified;

                    item.Email = primaryEmail;
                    item.PrimaryEmail = emailSignUp;
                    _db.Entry(item).State = EntityState.Modified;
                    await _db.SaveChangesAsync();

                    string body = $"{item.FirstName} your login email to the portal is now {primaryEmail} Use your current password";
                    await SMSClass.SendSMS("UNIJOS SIS", body, item.PhoneNumber); //EBULK SMS API
                }
            }

            return "Success";
        }

        [Authorize(Roles = RoleName.Admin + "," + RoleName.TSupport)]
        public async Task<ActionResult> ChangeStudentLevel(string studentId, int? Level)
        {
            var message = string.Empty;

            if (!string.IsNullOrEmpty(studentId))
            {
                studentId = studentId.ToUpper().Trim();
                var student = await _db.Students.Where(x => x.MatricNo.Trim().ToUpper().Equals(studentId) ||
                                x.JambRegNo.Trim().ToUpper().Equals(studentId)).FirstOrDefaultAsync();

                if (student != null)
                {
                    student.LevelId = Level;
                    _db.Entry(student).State = EntityState.Modified;
                    await _db.SaveChangesAsync();

                    message = $"Change of level for {student.FullName} is successful";
                }
                else
                {
                    message = "Student Record is not found";
                }
            }
            else
            {
                message = "Student Jamb-No/Matric-No/Form-No is empty";
            }

            ViewBag.Message = message;
            var levels = await _db.Levels.OrderByDescending(s => s.LevelName).ToListAsync();

            var level = from s in levels select new { Name = s.LevelName, Id = s.LevelId };
            ViewBag.Level = new SelectList(level, "Id", "Name");

            return View();
        }

        // Print exam card for undergrad students that have paid fees for the current session
        public async Task<ActionResult> PrintExamCard()
        {
            //var studentId = User.Identity.GetUserId();
            var student = await _db.Students.AsNoTracking().Include(i => i.Level).Include(i => i.Programme).Include(i => i.Session)
                                       .Include(i => i.Programme.Department).Include(i => i.Programme.Department.Faculty).Include(i => i.SchoolProgramme)
                                       .AsNoTracking()
                                       .Where(x => x.Email.Trim().ToUpper().Equals(userId.Trim().ToUpper()))
                                       .FirstOrDefaultAsync();

            var deptCode = student.Programme.Department.DeptCode.ToUpper();
            var matricNo = student.MatricNo.Trim().ToUpper();

            // Using regex to get only the last numbers of the matric number
            string pattern = @"\d+$";
            Match match = Regex.Match(matricNo, pattern);
            string result = match.Value;

            var securityCode = deptCode + "_" + result;

            var currentSessionId = _currentSession.GetCurrentSessionId(1);
            //var currentSemesterId = _currentSession.GetCurrentSemesterId(1);
            var currentSemesterName = _currentSession.GetCurrentSemesterName(1);
            var feePayment = await _db.SchoolFeePayments.AsNoTracking().Where(x => x.StudentId.Equals(student.StudentId) && x.SessionId == currentSessionId)
                        .Select(x => new { x.ReferenceNo, x.Status, x.PaidFee, x.TotalAmount }).FirstOrDefaultAsync();
            var currentSemesterCourses = await _db.CourseRegistrations.AsNoTracking().Include(x => x.Course).Include(x => x.Course.Semester)
                                        .Where(x => x.StudentId.Equals(student.StudentId) && x.SessionId.Equals(currentSessionId) && x.Course.Semester.SemesterName == currentSemesterName)
                                        .ToListAsync();

            var examCardVm = new PrintExamCardVm
            {
                Student = student,
                RRR = feePayment.ReferenceNo,
                CourseRegistration = currentSemesterCourses
            };

            ViewBag.SecurityCode = securityCode;
            ViewBag.CurrentSession = _currentSession.GetCurrentSessionName(1);
            ViewBag.CurrentSemester = currentSemesterName;

            // for first semester course reg
            if (!(currentSemesterName.Trim().ToUpper() == "FIRST"))
            {
                if (feePayment.Status.Equals(true) && currentSemesterCourses.Count() > 0)
                {
                    return new ViewAsPdf(examCardVm);
                }
                else
                {
                    string errorMessage = "<div style='color: red; font-family: Arial, sans-serif; font-size: 24px; padding: 100px; background-color: #ffecee; text-align: center;'>You are yet to do your course registration for this session. <br/> Do that and try again</div>";
                    return Content(errorMessage, "text/html");
                }
            }

            // for second semester course reg
            if (currentSemesterName.Trim().ToUpper() == "SECOND")
            {
                if ((feePayment.TotalAmount - feePayment.PaidFee) == 0 && currentSemesterCourses.Count() > 0)
                {
                    return new ViewAsPdf(examCardVm);
                }
                else
                {
                    string errorMessage = "<div style='color: red; font-family: Arial, sans-serif; font-size: 24px; padding: 100px; background-color: #ffecee; text-align: center;'>You are yet to pay your fee balance or do your course registration for this session. <br/> Do that and try again</div>";
                    return Content(errorMessage, "text/html");
                }
            }

            return new ViewAsPdf(examCardVm);
        }

        [Authorize(Roles = RoleName.Admin + "," + RoleName.AcademicOffice + "," + RoleName.DAPM_Sup)]
        public async Task<ActionResult> DownloadStudentReport()
        {
            ViewBag.SchoolProgrammeId = new SelectList(await _db.SchoolProgrammes.AsNoTracking().ToListAsync(), "SchoolProgrammeId", "FancyName");
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");

            return View();
        }

        [Authorize(Roles = RoleName.Admin + "," + RoleName.AcademicOffice + "," + RoleName.DAPM_Sup)]
        [HttpPost]
        public async Task DownloadStudentReport(Student model)
        {
            var students = await _db.Students.AsNoTracking().Include(x => x.Programme.Department.Faculty).Include(x => x.Programme.Department)
                            .Include(x => x.Programme).Include(x => x.Level).Where(x => x.SchoolProgrammeId == model.SchoolProgrammeId && x.SessionId == model.SessionId
                            && x.IsGraduated.Equals(false) && x.Active.Equals(true)).ToListAsync();

            char c1 = 'A';
            ExcelPackage package = new ExcelPackage();
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Student Report");

            worksheet.Cells[$"{c1++}1"].Value = "Matric No";
            worksheet.Cells[$"{c1++}1"].Value = "Full Name";
            worksheet.Cells[$"{c1++}1"].Value = "Faculty";
            worksheet.Cells[$"{c1++}1"].Value = "Department";
            worksheet.Cells[$"{c1++}1"].Value = "Programme";
            worksheet.Cells[$"{c1++}1"].Value = "Level";
            worksheet.Cells[$"{c1++}1"].Value = "State of Origin";
            worksheet.Cells[$"{c1++}1"].Value = "LGA";
            worksheet.Cells[$"{c1++}1"].Value = "Email";
            worksheet.Cells[$"{c1++}1"].Value = "Gender";
            worksheet.Cells[$"{c1++}1"].Value = "Phone";

            int rowStart = 2;

            for (var i = 0; i < students.Count; i++)
            {
                worksheet.Cells[$"A{rowStart}"].Value = students[i].MatricNo;
                worksheet.Cells[$"B{rowStart}"].Value = students[i].FullName;
                worksheet.Cells[$"C{rowStart}"].Value = students[i].Programme.Department.Faculty.FacultyName;
                worksheet.Cells[$"D{rowStart}"].Value = students[i].Programme.Department.DeptName;
                worksheet.Cells[$"E{rowStart}"].Value = students[i].Programme.ProgrammeName;
                worksheet.Cells[$"F{rowStart}"].Value = students[i].Level.LevelName;
                worksheet.Cells[$"G{rowStart}"].Value = students[i].StateOfOrigin;
                worksheet.Cells[$"H{rowStart}"].Value = students[i].Lga;
                worksheet.Cells[$"I{rowStart}"].Value = students[i].Email != null ? students[i].Email : students[i].PrimaryEmail;
                worksheet.Cells[$"J{rowStart}"].Value = students[i].Gender;
                worksheet.Cells[$"K{rowStart}"].Value = students[i].PhoneNumber;

                rowStart++;
            }

            worksheet.Cells["A:AZ"].AutoFitColumns();
            worksheet.Column(1).Style.Locked = true;
            worksheet.Column(2).Style.Locked = true;
            worksheet.Column(3).Style.Locked = true;
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment: filename=" +
                        $"{students.Select(s => s.Programme.Department.DeptName).FirstOrDefault()}StudentsReport.xlsx");
            Response.BinaryWrite(package.GetAsByteArray());
            Response.End();
        }



        public ActionResult ClearanceLog()
        {
            string LogFilePath = "ClearanceLog.txt"; // Path to your log file
            string path = Server.MapPath("~/" + LogFilePath);
            try
            {
                // Read all lines from the log file
                var logEntries = System.IO.File.ReadAllLines(path);

                // Process entries into a collection of objects for the v
                var logs = logEntries.Select(entry =>
                {
                    var fields = entry.Split(',');
                    return new ClearanceLog
                    {
                        JambRegNo = fields[0],
                        ClearanceType = fields[1],
                        Officer = fields[2],
                        DateTime = DateTime.Parse(fields[3]) // Ensure correct parsing for DateTime
                    };
                }).ToList();

                return View(logs);
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error reading log file: {ex.Message}";
                return View(Enumerable.Empty<object>()); // Return an empty list on error
            }
        }

        public ActionResult getStudentsEnrollmentByDept()
        {
            var genderCountByDepartment = _db.Students
                .GroupBy(s => new
                {
                    s.Programme.DepartmentId,
                    s.Programme.Department.DeptName,
                    s.LevelId
                })
                .Select(g => new DepartmentLevelGenderCountViewModel
                {
                    DepartmentId = (Int32)g.Key.DepartmentId,
                    DepartmentName = g.Key.DeptName,
                    LevelId = g.Key.LevelId ?? 0, // Handle null LevelId
            MaleCount = g.Count(s => s.Gender == "Male"),
                    FemaleCount = g.Count(s => s.Gender == "Female")
                })
                .ToList();

            return View(genderCountByDepartment);
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