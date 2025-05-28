using Microsoft.Ajax.Utilities;
using OfficeOpenXml;
using SwiftKampus.BusinessLogic;
using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampus.ViewModels;
using SwiftKampus.ViewModels.Result;
using SwiftKampusModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SwiftKampus.Controllers
{
    [Authorize(Roles = RoleName.Academic + "," + RoleName.Admin + "," + RoleName.None_Academic)]
    [Audit(AuditingLevel = 2)]
    public class ContinuousAssessmentsController : BaseController
    {
        private ResultCommand _resultQuery;
        List<Semester> allSemesters;
        List<Session> allSessions;
        List<Level> allLevel;
        List<Programme> allProgramme;
        List<Course> allCourse;

        public ContinuousAssessmentsController(SchoolDbContext db) : base(db)
        {
            _resultQuery = new ResultCommand(_db);
            allSessions = GetAllSession();
            allSemesters = GetAllSemesters();
            allLevel = GetLevelList();
            allCourse = GetAllCourse();
            allProgramme = GetAllProgramme();
        }


        public async Task<ActionResult> Index()
        {
            var result = ConfirmSchoolFee();
            if (result != null)
                return result;

            var staff = await _db.Staffs.Include(x => x.Department.Faculty).Where(x => x.Email == userId).FirstOrDefaultAsync();

            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking().OrderBy(x => x.SchoolProgrammeId), "SchoolProgrammeId", "FancyName");
            ViewBag.FacultyId = new SelectList(_db.Faculties.AsNoTracking().Where(x => x.FacultyId == staff.Department.FacultyId), "FacultyId", "FacultyName");
            ViewBag.DepartmentId = new SelectList(_db.Departments.AsNoTracking().Where(x => x.DepartmentId == staff.DepartmentId), "DepartmentId", "DeptName");
            ViewBag.ProgrammeId = new SelectList(_db.Programmes.AsNoTracking().Where(x => x.DepartmentId == staff.DepartmentId), "ProgrammeId", "ProgrammeName");
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            if (User.IsInRole(RoleName.Academic))
            {
                var courseList = await _db.AssignedCourses.Include(i => i.Staff).AsNoTracking()
                                .Where(x => x.Staff.Email.Equals(userId))
                                        .Select(s => s.Course).ToListAsync();
                ViewBag.CourseId = new SelectList(courseList, "CourseId", "CourseCode");
            }
            else
            {
                ViewBag.CourseId = new SelectList(_db.Courses.AsNoTracking(), "CourseId", "CourseCode");
            }
            return View();
        }

        public async Task<ActionResult> LecturerIndex()
        {
            var result = ConfirmSchoolFee();
            if (result != null)
                return result;
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.OrderBy(x => x.SchoolProgrammeId).AsNoTracking(), "SchoolProgrammeId", "FancyName");
            if (User.IsInRole(RoleName.Academic))
            {
                var courseList = await _db.AssignedCourses.Include(i => i.Staff).AsNoTracking()
                                .Where(x => x.Staff.Email.Equals(userId))
                                        .Select(s => s.Course).ToListAsync();
                ViewBag.CourseId = new SelectList(courseList, "CourseId", "CourseCode");

            }
            return View();
        }


        public async Task<ActionResult> GetIndex(int? SchoolProgrammeId, int? CourseId, int? SessionId)
        {
            var staff = _db.Staffs.Include(x => x.Department.Faculty).Where(x => x.Email == userId).FirstOrDefault();
            var cA = await GetAssessmentList(SchoolProgrammeId, CourseId, SessionId);

            var cAs = cA.Where(x => x.Programme.DepartmentId == staff.DepartmentId).ToList();
            var data = cAs.Select(s => new
            {
                s.CourseId,
                s.Level.LevelName,
                s.Course.CourseCode,
                s.Programme.ProgrammeName,
                s.CaScore,
                ExamScore = (bool)s.IsAbsentForExam ? "ABS" : s.ExamScore.ToString(),
                s.GradePoint,
                s.Student.MatricNo,
                s.Grading,
                s.Total,
                s.Sessions.SessionName,
                s.Semester.SemesterName,
                s.Student.FullName,
                s.Remark,
                s.ContinuousAssessmentId

            }).OrderBy(s => s.MatricNo).ToList();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = RoleName.Admin)]
        [HttpPost]
        public async Task<ActionResult> RemoveCaResult(int? SchoolProgrammeId, int? CourseId, int? SessionId)
        {
            int caCount = 0;
            string message;
            if (CourseId != null && SessionId != null)
            {
                var cAs = await GetAssessmentList(SchoolProgrammeId, CourseId, SessionId);
                caCount = cAs.Count();
                foreach (var ca in cAs)
                {
                    _db.ContinuousAssessments.Remove(ca);
                }
                await _db.SaveChangesAsync();
                 message = $"{caCount} Assesment(s) has been removed successfully";
                return new JsonResult { Data = new { status = true, message } };
            }
            message = $"No record was removed from the Assessment, Make sure the course and session is seleted";
            return new JsonResult { Data = new { status = true, message } };
        }

        public async Task<ActionResult> DisplayResultDetails(int id)
        {
            var cA = await _db.ContinuousAssessments.Include(i => i.Programme).AsNoTracking()
                                .Where(x => x.ContinuousAssessmentId.Equals(id)).FirstOrDefaultAsync();
            var caList = await _db.ContinuousAssessments.Include(i => i.Programme).Include(i => i.Programme.Department.Faculty)
                            .Include(i => i.Semester).Include(i => i.Sessions)
                            .Include(i => i.Level).Include(i => i.Course).Include(i => i.Student).AsNoTracking()
                            .Where(x => x.Programme.ProgrammeId.Equals((int)cA.ProgrammeId) &&
                            x.SessionId.Equals(cA.SessionId) && x.SemesterId.Equals(cA.SemesterId) &&
                            x.CourseId.Equals(cA.CourseId)).ToListAsync();
            var deptResultTemplate = _resultQuery.GetDeptResultTemplate(cA.SessionId, (int)cA.Programme.DepartmentId);
            ViewBag.FailMark = deptResultTemplate?.FailMark;
            ViewBag.PrintId = id;
            return View(caList);
        }


        public async Task DownloadCaReport(int? SchoolProgrammeId, int? CourseId)
        {
            ExcelPackage package = new ExcelPackage();
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Report");

            var myCalist = await GetAssessmentList(SchoolProgrammeId, CourseId, null);
            var facultyName = myCalist.Select(s => s.Programme.Department.Faculty.FacultyName).FirstOrDefault();
            var deptName = myCalist.Select(s => s.Programme.Department.DeptName).FirstOrDefault();
            var semesterName = myCalist.Select(s => s.Semester.SemesterName).FirstOrDefault();
            var sessionName = myCalist.Select(s => s.Sessions.SessionName).FirstOrDefault();

            var Rng = worksheet.Cells[1, 2, 1, 2];
            Rng.Value = "UNIVERSITY OF JOS";
            Rng.Style.Font.Size = 11;
            Rng.Style.Font.Bold = true;
            var Rng2 = worksheet.Cells[2, 2, 2, 2];
            Rng2.Value = $"FACULTY OF {facultyName}";
            Rng2.Style.Font.Size = 11;
            Rng2.Style.Font.Bold = true;
            var Rng3 = worksheet.Cells[3, 2, 3, 2];
            Rng3.Value = $"DEPARTMENT OF {deptName}";
            Rng3.Style.Font.Size = 11;
            Rng3.Style.Font.Bold = true;
            var Rng4 = worksheet.Cells[4, 2, 4, 2];
            Rng4.Value = $"{semesterName} SEMESTER EXAMINATION RESULT {sessionName} SESSION";
            Rng4.Style.Font.Size = 11;
            Rng4.Style.Font.Bold = true;

            //Rng.Style.Font.Italic = true;

            char c1 = 'A';
            worksheet.Cells[$"{c1++}5"].Value = "S/N";
            worksheet.Cells[$"{c1++}5"].Value = "Student Name";
            worksheet.Cells[$"{c1++}5"].Value = "Matric No";
            worksheet.Cells[$"{c1++}5"].Value = "Ca Score";
            worksheet.Cells[$"{c1++}5"].Value = "Exam Score";
            worksheet.Cells[$"{c1++}5"].Value = "Total";
            worksheet.Cells[$"{c1++}5"].Value = "Grade";
            worksheet.Cells[$"{c1++}5"].Value = "Grade Point";
            worksheet.Cells[$"{c1++}5"].Value = "Remark";

            int rowStart = 2;
            int lastRow = 0;
            //char c2 = 'A';
            int sn = 1;
            for (var i = 0; i < myCalist.Count; i++)
            {

                worksheet.Cells[$"A{rowStart}"].Value = sn;
                worksheet.Cells[$"B{rowStart}"].Value = myCalist[i].Student.FullName;
                worksheet.Cells[$"C{rowStart}"].Value = myCalist[i].Student.MatricNo;
                worksheet.Cells[$"D{rowStart}"].Value = myCalist[i].CaScore;
                worksheet.Cells[$"E{rowStart}"].Value = myCalist[i].ExamScore;
                worksheet.Cells[$"F{rowStart}"].Value = myCalist[i].Total;
                worksheet.Cells[$"G{rowStart}"].Value = myCalist[i].Grading;
                worksheet.Cells[$"H{rowStart}"].Value = myCalist[i].GradePoint;
                worksheet.Cells[$"I{rowStart}"].Value = myCalist[i].Remark;
                worksheet.Cells[$"J{rowStart}"].Value = myCalist[i].StaffName;
                rowStart++;
                sn++;
                lastRow = rowStart;
            }

            int newStartRow = lastRow + 2;
            var summary = worksheet.Cells[$"C{newStartRow}"];
            Rng4.Value = $"SUMMARY";
            Rng4.Style.Font.Size = 11;
            Rng4.Style.Font.Bold = true;

            var gradeName = 'A';
            var gradeListVm = new List<GradeSummaryListVm>();
            foreach (var caList in myCalist)
            {
                gradeListVm.Add(new GradeSummaryListVm()
                {
                    GradeName = gradeName.ToString(),
                    GradeCount = myCalist.Count(x => x.Grading.Equals(gradeName.ToString()))
                });
            }

            char c2 = 'B';
            var gradeRow = newStartRow + 1;
            for (var i = 0; i < gradeListVm.Count; i++)
            {
                worksheet.Cells[$"{c2++}{gradeRow}"].Value = gradeListVm[i].GradeName;
                worksheet.Cells[$"{c2++}{gradeRow + 1}"].Value = gradeListVm[i].GradeCount;
                gradeRow = gradeRow + 1;
            }

            var info = myCalist.FirstOrDefault();
            worksheet.Cells["A:AZ"].AutoFitColumns();
            worksheet.Column(1).Style.Locked = true;
            worksheet.Column(2).Style.Locked = true;
            worksheet.Column(3).Style.Locked = true;
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment: filename=" + $"Result.xlsx");
            Response.BinaryWrite(package.GetAsByteArray());
            Response.End();

        }

        private async Task<List<ContinuousAssessment>> GetAssessmentList(int? SchoolProgrammeId, int? CourseId, int? pSessionId)
        {
            if (SchoolProgrammeId != null)
            {
                if (pSessionId == null)
                {
                    pSessionId = _query.GetCurrentSessionId((int)SchoolProgrammeId);
                }
                semesterId = _query.GetCurrentSemesterId((int)SchoolProgrammeId);
            }
            var cA = new List<ContinuousAssessment>();
            if (pSessionId != null)
            {
                if (CourseId != null)
                {
                    cA = await _db.ContinuousAssessments.Include(c => c.Course).Include(i => i.Level)
                                        .Include(c => c.Programme).Include(i => i.Semester)
                                        .Include(i => i.Sessions).Include(i => i.Student)
                                        .Include(i => i.Programme.Department.Faculty)
                                        .Include(i => i.Programme.Department)
                                        .Where(x => /*x.SemesterId.Equals(semesterId) &&*/
                                        x.SessionId.Equals((int)pSessionId) &&
                                        x.CourseId.Equals((int)CourseId))
                                        .ToListAsync();
                }
                else
                {
                    cA = await _db.ContinuousAssessments.Include(c => c.Course).Include(i => i.Level)
                                        .Include(c => c.Programme).Include(i => i.Semester)
                                        .Include(i => i.Sessions).Include(i => i.Student)
                                        .Include(i => i.Programme.Department.Faculty)
                                        .Include(i => i.Programme.Department)
                                        .Where(x => /*x.SemesterId.Equals(semesterId) &&*/ x.SessionId.Equals((int)pSessionId))
                                        .ToListAsync();
                }
            }

            return cA;
        }

        public async Task<ActionResult> DeptApprovalIndex()
        {
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.OrderBy(x => x.SchoolProgrammeId).AsNoTracking(), "SchoolProgrammeId", "FancyName");
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            ViewBag.LevelId = new SelectList(_db.Levels.AsNoTracking(), "LevelId", "LevelName");
            if (User.IsInRole(RoleName.Hod) && !User.IsInRole(RoleName.DirectorGST))
            {
                var staffDeptId = await _db.Staffs.AsNoTracking().Where(x => x.Email.Equals(userId))
                               .Select(s => s.Department.DepartmentId).FirstOrDefaultAsync();
                var programmes = await _db.Programmes.Include(i => i.Department).AsNoTracking()
                                .Where(x => x.Department.DepartmentId.Equals(staffDeptId)).ToListAsync();

                ViewBag.ProgrammeId = new SelectList(programmes, "ProgrammeId", "ProgrammeName");

            }
            else
            {
                ViewBag.ProgrammeId = new SelectList(_db.Programmes.AsNoTracking(), "ProgrammeId", "ProgrammeName");
            }

            return View();
        }

        public async Task<ActionResult> GetDeptApproval(int? SchoolProgrammeId, int? SessionId, int? ProgrammeId, int? LevelId)
        {
            if (SchoolProgrammeId != null)
            {
                if (SessionId != null)
                {
                    sessionId = (int)SessionId;
                }
                else
                {
                    sessionId = _query.GetCurrentSessionId((int)SchoolProgrammeId);
                }
                //semesterId = _query.GetCurrentSemesterId((int)SchoolProgrammeId);
            }
            var caList = new List<ContinuousAssessment>();

            var cA = _db.ContinuousAssessments.Include(c => c.Course).Include(i => i.Level)
                                    .Include(c => c.Programme).Include(i => i.Semester)
                                    .Include(i => i.Sessions).Include(i => i.Student)
                                    .Include(i => i.Course.Programme).AsNoTracking()
                                    .Where(x => x.SessionId.Equals(sessionId))
                                    .ToList().DistinctBy(x => x.CourseId).ToList();

            if (User.IsInRole(RoleName.Hod) && cA.Count > 0)
            {
                var staffDeptId = await _db.Staffs.AsNoTracking().Where(x => x.Email.Equals(userId))
                                 .Select(s => s.Department.DepartmentId).FirstOrDefaultAsync();
                var programmeIds = await _db.Programmes.Include(i => i.Department).AsNoTracking()
                                .Where(x => x.Department.DepartmentId.Equals(staffDeptId))
                                        .Select(s => s.ProgrammeId).ToListAsync();
                if (ProgrammeId != null)
                {
                    var myCaList = cA.Where(x => x.Course.Programme.ProgrammeId.Equals((int)ProgrammeId)).ToList();
                    caList.AddRange(myCaList);
                }
                else
                {
                    foreach (var programmeId in programmeIds)
                    {
                        var myCaList = cA.Where(x => x.Course.Programme.ProgrammeId.Equals(programmeId)).ToList();
                        caList.AddRange(myCaList);
                    }
                }
                if (LevelId != null)
                {
                    var myCaList = caList.Where(x => x.Level.LevelId.Equals((int)LevelId)).ToList();
                    caList = myCaList;
                }
            }
            else
            {
                if (ProgrammeId != null)
                {
                    cA = cA.Where(x => x.Programme.ProgrammeId.Equals((int)ProgrammeId)).ToList();
                }
                if (LevelId != null)
                {
                    cA = cA.Where(x => x.Level.LevelId.Equals((int)LevelId)).ToList();
                }
                caList.AddRange(cA);
            }

            var data = caList.Select(s => new DeptApprovalVm()
            {
                CourseId = s.CourseId,
                LevelName = s.Level.LevelName,
                CourseCode = s.Course.CourseCode,
                ProgrammeName = s.Programme.ProgrammeName,
                SessionName = s.Sessions.SessionName,
                SemesterName = s.Semester.SemesterName,
                ContinuousAssessmentId = s.ContinuousAssessmentId,
                IsDeptApproved = s.IsDeptApproved,
                IsFacultyApproved = s.IsFacultyApproved,
                IsSenateApproved = s.IsSenateApproved,
                ReasonForReject = s.ReasonForReject,
                Submitted = s.Submitted

            }).ToList();
            // return Json(new { data }, JsonRequestBehavior.AllowGet);
            return PartialView(data);
        }

        public List<ContinuousAssessment> GetAssessments(int id)
        {
            var cA = _db.ContinuousAssessments.AsNoTracking()
                           .Where(x => x.ContinuousAssessmentId.Equals(id))
                           .FirstOrDefault();
            if (cA != null)
            {
                return _db.ContinuousAssessments.AsNoTracking()
                            .Where(x => x.CourseId.Equals(cA.CourseId) &&
                            x.SemesterId.Equals(cA.SemesterId) &&
                            x.SessionId.Equals(cA.SessionId))
                            .ToList();
            }
            return null;
        }

        public async Task<ActionResult> DeptApproval(int id)
        {
            var cAList = GetAssessments(id);
            if (cAList != null)
            {
                foreach (var cAItem in cAList)
                {
                    cAItem.IsDeptApproved = true;
                    cAItem.ReasonForReject = "Dept Approved";
                    cAItem.Submitted = false;
                    _db.Entry(cAItem).State = EntityState.Modified;
                }
                await _db.SaveChangesAsync();

                return new JsonResult { Data = new { status = true, message = $"{cAList.Count} Student result has been approved successfully" } };

            }
            return new JsonResult { Data = new { status = true, message = "Error Approving results" } };
        }

        public void DeptReject(int id, string ReasonForReject)
        {

            var cAList = GetAssessments(id);
            if (cAList != null)
            {
                foreach (var cAItem in cAList)
                {
                    cAItem.IsDeptApproved = false;
                    cAItem.Submitted = false;
                    cAItem.ReasonForReject = $"Dept - {ReasonForReject}";
                    _db.Entry(cAItem).State = EntityState.Modified;
                }
                _db.SaveChanges();
            }
        }

        public void FacultyReject(int id, string ReasonForReject)
        {
            var cAList = GetAssessments(id);
            if (cAList != null)
            {
                foreach (var cAItem in cAList)
                {
                    cAItem.IsFacultyApproved = false;
                    cAItem.ReasonForReject = $"Faculty - {ReasonForReject}";
                    _db.Entry(cAItem).State = EntityState.Modified;
                }
                _db.SaveChanges();
            }
        }

        public void SenateReject(int id, string ReasonForReject)
        {
            var cAList = GetAssessments(id);
            if (cAList != null)
            {
                foreach (var cAItem in cAList)
                {
                    cAItem.IsSenateApproved = false;
                    cAItem.ReasonForReject = $"Senate - {ReasonForReject}";
                    _db.Entry(cAItem).State = EntityState.Modified;
                }
                _db.SaveChanges();
            }
        }

        public PartialViewResult DeptRejectSave(int id, string resultLevel)
        {
            var model = new DeptRejectVm()
            {
                ContinuousAssessmentId = id,
                ResultLevel = resultLevel,
            };
            return PartialView(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeptRejectSave(DeptRejectVm model)
        {
            if (model.ResultLevel.Equals("Dept"))
            {
                DeptReject(model.ContinuousAssessmentId, model.ReasonForReject);
            }
            else if (model.ResultLevel.Equals("Faculty"))
            {
                FacultyReject(model.ContinuousAssessmentId, model.ReasonForReject);
            }
            else if (model.ResultLevel.Equals("Senate"))
            {
                SenateReject(model.ContinuousAssessmentId, model.ReasonForReject);
            }

            var cA = await _db.ContinuousAssessments.AsNoTracking()
                           .Where(x => x.ContinuousAssessmentId.Equals(model.ContinuousAssessmentId))
                           .FirstOrDefaultAsync();

            if (cA != null)
            {
                var cAListHistory = await _db.ContinuousAssessmentHistories.AsNoTracking()
                            .Where(x => x.CourseId.Equals(cA.CourseId) &&
                            x.SemesterId.Equals(cA.SemesterId) &&
                            x.SessionId.Equals(cA.SessionId))
                            .ToListAsync();
                var cAList = await _db.ContinuousAssessments.AsNoTracking()
                            .Where(x => x.CourseId.Equals(cA.CourseId) &&
                            x.SemesterId.Equals(cA.SemesterId) &&
                            x.SessionId.Equals(cA.SessionId))
                            .ToListAsync();
                foreach (var cAItem in cAList)
                {
                    var cAHistory = new ContinuousAssessmentHistory()
                    {
                        CourseId = cAItem.CourseId,
                        LevelId = cAItem.LevelId,
                        CourseUnit = cAItem.CourseUnit,
                        SemesterId = cAItem.SemesterId,
                        SessionId = cAItem.SessionId,
                        CaScore = cAItem.CaScore,
                        ExamScore = cAItem.ExamScore,
                        ProgrammeId = cAItem.ProgrammeId,
                        QualityPoint = cAItem.QualityPoint,
                        StaffName = cAItem.StaffName,
                        Total = cAItem.Total,
                        ReasonForReject = model.ReasonForReject,
                        RejectionDate = DateTime.Now,
                        GradePoint = cAItem.GradePoint,
                        Remark = cAItem.Remark,
                        StudentId = cAItem.StudentId,
                        RejectedBy = userId,
                        NoOfReject = cAListHistory.Count + 1

                    };
                    _db.ContinuousAssessmentHistories.Add(cAHistory);
                }
                await _db.SaveChangesAsync();

                return new JsonResult { Data = new { status = true, message = $"{cAList.Count} Student result has been approved successfully" } };
            }

            return new JsonResult { Data = new { status = false, message = "Error Rejecting result" } };
            //return View(subject);
        }

        public async Task<ActionResult> FacultyApprovalIndex()
        {
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.OrderBy(x => x.SchoolProgrammeId).AsNoTracking(), "SchoolProgrammeId", "FancyName");
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            ViewBag.LevelId = new SelectList(_db.Levels.AsNoTracking(), "LevelId", "LevelName");
            if (User.IsInRole(RoleName.Dean) && !User.IsInRole(RoleName.DirectorGST))
            {
                var staffFacultyId = await _db.Staffs.AsNoTracking().Where(x => x.Email.Equals(userId))
                               .Select(s => s.Department.FacultyId).FirstOrDefaultAsync();
                var programmes = await _db.Programmes.Include(i => i.Department).AsNoTracking()
                                .Where(x => x.Department.FacultyId.Equals(staffFacultyId))
                                        .ToListAsync();
                ViewBag.ProgrammeId = new SelectList(programmes, "ProgrammeId", "ProgrammeName");

            } 
            else
            {
                ViewBag.ProgrammeId = new SelectList(_db.Programmes.AsNoTracking(), "ProgrammeId", "ProgrammeName");
            }
            return View();
        }

        public ActionResult GetFacultyApproval(int? SchoolProgrammeId, int? SessionId, int? ProgrammeId, int? LevelId)
        {
            if (SchoolProgrammeId != null)
            {
                if (SessionId != null)
                {
                    sessionId = (int)SessionId;
                }
                else
                {
                    sessionId = _query.GetCurrentSessionId((int)SchoolProgrammeId);
                }
                semesterId = _query.GetCurrentSemesterId((int)SchoolProgrammeId);
            }
            var caList = new List<ContinuousAssessment>();

            var cA = _db.ContinuousAssessments.Include(c => c.Course).Include(i => i.Level)
                                    .Include(c => c.Programme).Include(i => i.Semester)
                                    .Include(i => i.Sessions).Include(i => i.Student)
                                    .Include(i => i.Programme.Department).AsNoTracking()
                                    .Where(
                                    //x => x.SemesterId.Equals(semesterId) &&
                                    x => x.SessionId.Equals(sessionId) &&
                                    x.IsDeptApproved.Equals(true))
                                    .ToList().DistinctBy(x => x.CourseId).ToList();

            if (ProgrammeId != null)
            {
                cA = cA.Where(x => x.Programme.ProgrammeId.Equals((int)ProgrammeId)).ToList();
            }
            if (LevelId != null)
            {
                cA = cA.Where(x => x.Level.LevelId.Equals((int)LevelId)).ToList();
            }
            caList.AddRange(cA);


            var data = caList.Select(s => new DeptApprovalVm()
            {
                CourseId = s.CourseId,
                LevelName = s.Level.LevelName,
                CourseCode = s.Course.CourseCode,
                ProgrammeName = s.Programme.ProgrammeName,
                SessionName = s.Sessions.SessionName,
                SemesterName = s.Semester.SemesterName,
                ContinuousAssessmentId = s.ContinuousAssessmentId,
                IsDeptApproved = s.IsDeptApproved,
                IsFacultyApproved = s.IsFacultyApproved,
                IsSenateApproved = s.IsSenateApproved,
                ReasonForReject = s.ReasonForReject,
                Submitted = s.Submitted

            }).ToList();
            //return Json(new { data }, JsonRequestBehavior.AllowGet);
            return PartialView(data);

        }

        public async Task<ActionResult> FacultyApproval(int id)
        {
            var cAList = GetAssessments(id);
            if (cAList != null)
            {
                foreach (var cAItem in cAList)
                {
                    cAItem.IsFacultyApproved = true;
                    cAItem.ReasonForReject = "Faculty Approved";
                    _db.Entry(cAItem).State = EntityState.Modified;
                }
                await _db.SaveChangesAsync();

                return new JsonResult { Data = new { status = true, message = $"{cAList.Count} Student result has been approved successfully" } };

            }
            return new JsonResult { Data = new { status = true, message = "Error Approving results" } };
        }


        public ActionResult SenateApprovalIndex()
        {
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.OrderBy(x => x.SchoolProgrammeId).AsNoTracking(), "SchoolProgrammeId", "FancyName");
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            ViewBag.LevelId = new SelectList(_db.Levels.OrderBy(x => x.LevelId).AsNoTracking(), "LevelId", "LevelName");
            ViewBag.ProgrammeId = new SelectList(_db.Programmes.AsNoTracking(), "ProgrammeId", "ProgrammeName");
            return View();
        }

        public ActionResult GetSenateApproval(int? SchoolProgrammeId, int? SessionId, int? ProgrammeId, int? LevelId)
        {
            if (SchoolProgrammeId != null)
            {
                if (SessionId != null)
                {
                    sessionId = (int)SessionId;
                }
                else
                {
                    sessionId = _query.GetCurrentSessionId((int)SchoolProgrammeId);
                }
                //semesterId = _query.GetCurrentSemesterId((int)SchoolProgrammeId);
            }
            var caList = new List<ContinuousAssessment>();

            var cA = _db.ContinuousAssessments.Include(c => c.Course).Include(i => i.Level)
                                    .Include(c => c.Programme).Include(i => i.Semester)
                                    .Include(i => i.Sessions).Include(i => i.Student)
                                    .Include(i => i.Programme.Department).AsNoTracking()
                                    .Where(x => /*x.SemesterId.Equals(semesterId) &&*/
                                    x.SessionId.Equals(sessionId) &&
                                    x.IsFacultyApproved.Equals(true))
                                    .ToList().DistinctBy(x => x.CourseId).ToList();

            if (ProgrammeId != null)
            {
                cA = cA.Where(x => x.Programme.ProgrammeId.Equals((int)ProgrammeId)).ToList();
            }
            if (LevelId != null)
            {
                cA = cA.Where(x => x.Level.LevelId.Equals((int)LevelId)).ToList();
            }
            caList.AddRange(cA);

            var data = caList.Select(s => new DeptApprovalVm()
            {
                CourseId = s.CourseId,
                LevelName = s.Level.LevelName,
                CourseCode = s.Course.CourseCode,
                ProgrammeName = s.Programme.ProgrammeName,
                SessionName = s.Sessions.SessionName,
                SemesterName = s.Semester.SemesterName,
                ContinuousAssessmentId = s.ContinuousAssessmentId,
                IsDeptApproved = s.IsDeptApproved,
                IsFacultyApproved = s.IsFacultyApproved,
                IsSenateApproved = s.IsSenateApproved,
                ReasonForReject = s.ReasonForReject,
                Submitted = s.Submitted

            }).ToList();
            //return Json(new { data }, JsonRequestBehavior.AllowGet);
            return PartialView(data);
        }

        public async Task<ActionResult> SenateApproval(int id)
        {
            var cAList = GetAssessments(id);
            if (cAList != null)
            {
                foreach (var cAItem in cAList)
                {
                    cAItem.IsSenateApproved = true;
                    cAItem.ReasonForReject = "Senate Approved";
                    _db.Entry(cAItem).State = EntityState.Modified;
                }
                await _db.SaveChangesAsync();

                return new JsonResult { Data = new { status = true, message = $"{cAList.Count} Student result has been approved successfully" } };

            }
            return new JsonResult { Data = new { status = true, message = "Error Approving results" } };
        }


        //[RecurringAuthorize]
        public async Task<ActionResult> CreateCaView(string message)
        {
            ViewBag.FacultyId = new SelectList(_db.Faculties.AsNoTracking().OrderBy(x => x.FacultyId), "FacultyId", "FacultyName");
            ViewBag.DepartmentId = new SelectList(_db.Departments.AsNoTracking(), "DepartmentId", "DeptName");
            ViewBag.ProgrammeId = new SelectList(_db.Programmes.AsNoTracking(), "ProgrammeId", "ProgrammeName");
            ViewBag.SemesterId = new SelectList(_db.Semesters.AsNoTracking(), "SemesterId", "SemesterName");
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            ViewBag.Message = message;
            var lecturerCourse = new List<Course>();
            if (User.IsInRole(RoleName.Academic))
            {
                var courseList = await _db.AssignedCourses.Include(i => i.Staff).AsNoTracking()
                                .Where(x => x.Staff.Email.Equals(userId))
                                .Select(s => s.CourseId).ToListAsync();
                foreach (var course in courseList)
                {
                    var searchedCourse = await _db.Courses.FindAsync(course);
                    if (searchedCourse != null)
                    {
                        lecturerCourse.Add(searchedCourse);
                    }
                }
                ViewBag.CourseId = new SelectList(lecturerCourse, "CourseId", "CourseCode");
            }
            else
            {
                ViewBag.CourseId = new SelectList(_db.Courses.AsNoTracking(), "CourseId", "CourseCode");
            }

            ViewBag.SetUpCount = 0;
            return View();
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "CreateCa")]
        public async Task<ActionResult> CreateCa(SelectCaVm model)
        {
            //int checkResultTemplate = _resultQuery.CheckForResultTemplate(model.CourseId, model.SessionId);
            //if (checkResultTemplate <= 0)
            //{
            //    return RedirectToAction("CreateCaView", new { message = "Grading has not been set for this department please contact the HOD" });
            //}
            var course = _db.Courses.Include(i => i.Programme).Include(i => i.Level).Where(x => x.CourseId.Equals(model.CourseId)).FirstOrDefault();
            bool isDeptApproved = await _resultQuery.CheckDeptApproval(model.CourseId, model.SessionId, course.Level.LevelId, course.Programme.ProgrammeId);
            if (isDeptApproved)
            {
                return RedirectToAction("CreateCaView", new { message = "This result has already been approved by the Department. It can't be edited again until its rejected by the department" });
            }
            var result = await GenerateCaList(model);
            if (result.Item1 != null)
            {
                return View(result.Item1.ToList());
            }
            return RedirectToAction("CreateCaView", new { message = result.Item2 });
        }


        [HttpPost]
        [MultipleButton(Name = "action", Argument = "DownloadCa")]
        public async Task DownloadCa(SelectCaVm model)
        {
            var myCalist = await GenerateCaList(model);
            if (myCalist.Item1 == null)
            {
                Response.End();
            }

            char c1 = 'A';
            ExcelPackage package = new ExcelPackage();
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Report");
            var caProgrammeId = myCalist.Item1[0].Programme.ProgrammeId;

            var programme = _db.Programmes.Include(i => i.Department).Include(i => i.Department.Faculty)
                            .AsNoTracking().Where(x => x.ProgrammeId.Equals(caProgrammeId)).FirstOrDefault();

            var programmeId = worksheet.Cells["AA2"];
            var courseId = worksheet.Cells["AA3"];
            var levelId = worksheet.Cells["AA4"];
            var semester = worksheet.Cells["AA5"];
            var session = worksheet.Cells["AA6"];

            programmeId.Value = myCalist.Item1[0].Programme.ProgrammeId;
            courseId.Value = myCalist.Item1[0].Course.CourseId;
            levelId.Value = myCalist.Item1[0].Level.LevelId;
            semester.Value = myCalist.Item1[0].Semester.SemesterId;
            session.Value = myCalist.Item1[0].Session.SessionId;

            var Rng = worksheet.Cells["B1"];
            Rng.Value = "UNIVERSITY OF JOS";
            Rng.Style.Font.Size = 14;
            Rng.Style.Font.Bold = true;
            var Rng1 = worksheet.Cells["B2"];
            Rng1.Value = $"FACULTY OF {programme.Department.Faculty.FacultyName.ToUpper()}";
            Rng1.Style.Font.Bold = true;
            var Rng2 = worksheet.Cells["B3"];
            Rng2.Value = $"DEPARTMENT OF {programme.Department.DeptName.ToUpper()} ({myCalist.Item1[0].Programme.ProgrammeCode.ToUpper()})";
            Rng2.Style.Font.Bold = true;
            var Rng3 = worksheet.Cells["B4"];
            Rng3.Value = $"{myCalist.Item1[0].Course.CourseName.ToUpper()} ({myCalist.Item1[0].Course.CourseCode.ToUpper()})";
            Rng3.Style.Font.Bold = true;
            var Rng4 = worksheet.Cells["B5"];
            Rng4.Value = $"{myCalist.Item1[0].Level.LevelName.ToUpper()} LEVEL";
            Rng4.Style.Font.Bold = true;
            var Rng5 = worksheet.Cells["B6"];
            Rng5.Value = $"{myCalist.Item1[0].Semester.SemesterName.ToUpper()} SEMESTER";
            Rng5.Style.Font.Bold = true;
            var Rng6 = worksheet.Cells["B7"];
            Rng6.Value = $"{myCalist.Item1[0].Session.SessionName.ToUpper()} SESSION";
            Rng6.Style.Font.Bold = true;

            worksheet.Cells[$"{c1++}8"].Value = "Id";
            worksheet.Cells[$"{c1++}8"].Value = "Student Name";
            worksheet.Cells[$"{c1++}8"].Value = "Matric Number";
            //worksheet.Cells[$"{c1++}7"].Value = "Department Name";
            //worksheet.Cells[$"{c1++}7"].Value = "Course Name";
            //worksheet.Cells[$"{c1++}7"].Value = "Level Name";
            //worksheet.Cells[$"{c1++}7"].Value = "Semester Name";
            //worksheet.Cells[$"{c1++}7"].Value = "Session Name";       
            worksheet.Cells[$"{c1++}8"].Value = "Ca Score(40)";
            worksheet.Cells[$"{c1++}8"].Value = "Exam Score(60)";
            worksheet.Cells[$"{c1++}9"].Value = "Is Absent";
            //worksheet.Cells[$"{c1++}7"].Value = "Staff Name";

            int rowStart = 9;
            //char c2 = 'A';

            for (var i = 0; i < myCalist.Item1.Count; i++)
            {

                worksheet.Cells[$"A{rowStart}"].Value = myCalist.Item1[i].ContinuousAssessmentId;
                worksheet.Cells[$"B{rowStart}"].Value = myCalist.Item1[i].Student.FullName;
                worksheet.Cells[$"C{rowStart}"].Value = myCalist.Item1[i].Student.MatricNo;
                //worksheet.Cells[$"D{rowStart}"].Value = myCalist.Item1[i].Programme.ProgrammeCode;
                //worksheet.Cells[$"E{rowStart}"].Value = myCalist.Item1[i].Course.CourseCode;
                //worksheet.Cells[$"F{rowStart}"].Value = myCalist.Item1[i].Level.LevelName;
                //worksheet.Cells[$"G{rowStart}"].Value = myCalist.Item1[i].Semester.SemesterName;
                //worksheet.Cells[$"H{rowStart}"].Value = myCalist.Item1[i].Session.SessionName;              
                worksheet.Cells[$"D{rowStart}"].Value = myCalist.Item1[i].CaScore;
                worksheet.Cells[$"E{rowStart}"].Value = myCalist.Item1[i].ExamScore;
                worksheet.Cells[$"F{rowStart}"].Value = myCalist.Item1[i].IsAbsentForExam;
                //worksheet.Cells[$"K{rowStart}"].Value = myCalist.Item1[i].StaffName;
                rowStart++;
            }
            var info = myCalist.Item1.FirstOrDefault();
            //worksheet.Cells["A:AZ"].AutoFitColumns();
            worksheet.Column(1).Style.Locked = true;
            worksheet.Column(2).Style.Locked = true;
            worksheet.Column(3).Style.Locked = true;
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment: filename=" + $"{info.CourseId}{info.LevelId}Result.xlsx");
            Response.BinaryWrite(package.GetAsByteArray());
            Response.End();

        }

        private async Task<Tuple<List<ContinuousAssessmentVm>, string>> GenerateCaList(SelectCaVm model)
        {
            var courseDetails = await _db.Courses.Include(i => i.Programme).Include(i => i.Level).AsNoTracking()
                                            .Where(x => x.CourseId.Equals(model.CourseId))
                                            .Select(s => new
                                            {
                                                levelId = s.Level.LevelId,
                                                programmeId = s.Programme.ProgrammeId,
                                                s.CourseCode,
                                                s.SemesterId
                                            })
                                            .FirstOrDefaultAsync();

            var regStudent = _db.CourseRegistrations.Include(i => i.Students).Include(i => i.Programme.Department)
                                                    .Include(i => i.Level).Include(i => i.Students.Session).AsNoTracking()
                                                    .Where(x => x.CourseId.Equals(model.CourseId)
                                                    // && x.LevelId.Equals(courseDetails.levelId)
                                                    && x.ProgrammeId.Equals(courseDetails.programmeId)
                                                    && x.SessionId.Equals(model.SessionId)
                                                    //&& x.SemesterId.Equals((int)courseDetails.SemesterId)
                                                    && x.IsApproved.Equals(true))
                                                    .Select(x => new
                                                    {
                                                        x.Level.LevelOrder,
                                                        x.Students,
                                                        x.Students.Session.SessionName,
                                                        deptId = x.Programme.Department.DepartmentId
                                                    }).DistinctBy(x => x.Students.StudentId).ToList();
            if (!regStudent.Any())
            {
                return new Tuple<List<ContinuousAssessmentVm>, string>(null,
                                    $"Student is not registered for this course ({courseDetails.CourseCode})");

            }
            foreach (var student in regStudent)
            {
                int checkResultTemplate = _resultQuery.CheckForResultTemplate(model.CourseId, (int)student.Students.SessionId, student.LevelOrder);
                if (checkResultTemplate <= 0)
                {
                    return new Tuple<List<ContinuousAssessmentVm>, string>(null,
                                    $"Grading has not been set for {student.SessionName} session. Please contact the HOD");
                }
            }


            var calist = _db.ContinuousAssessments.AsNoTracking().Include(i => i.Student).Include(i => i.Programme.Department)
                                                    .Where(x => x.CourseId.Equals(model.CourseId) &&
                                                    //x.SemesterId.Equals(model.SemesterId) &&
                                                    x.SessionId.Equals(model.SessionId)).ToList();
            var myCalist = new List<ContinuousAssessmentVm>();
            if (calist.Any())
            {
                foreach (var list in calist)
                {
                    var ca = new ContinuousAssessmentVm()
                    {
                        ContinuousAssessmentId = list.ContinuousAssessmentId,
                        StudentId = list.StudentId,
                        CourseId = model.CourseId,
                        SemesterId = list.SemesterId,
                        LevelId = (int)list.LevelId,
                        SessionId = list.SessionId,
                        ProgrammeId = (int)list.ProgrammeId,
                        CaScore = list.CaScore,
                        ExamScore = list.ExamScore,
                        StaffName = userId,
                        IsAbsentForExam = (bool)list.IsAbsentForExam
                    };
                    myCalist.Add(ca);
                }
                if (regStudent.Count() > myCalist.Count())
                {

                    foreach (var student in regStudent)
                    {
                        if (!calist.Any(x => x.StudentId.Equals(student.Students.StudentId)))
                        {
                            var ca = new ContinuousAssessmentVm()
                            {
                                ContinuousAssessmentId = 0,
                                StudentId = student.Students.StudentId,
                                CourseId = model.CourseId,
                                SemesterId = model.SemesterId,
                                LevelId = (int)student.Students.LevelId,
                                SessionId = model.SessionId,
                                ProgrammeId = (int)student.Students.ProgrammeId,
                                CaScore = 0,
                                ExamScore = 0,
                                StaffName = userId,
                                IsAbsentForExam = false
                            };
                            myCalist.Add(ca);
                        }
                    }
                }
            }
            else
            {
                foreach (var student in regStudent)
                {
                    var ca = new ContinuousAssessmentVm()
                    {
                        ContinuousAssessmentId = 0,
                        StudentId = student.Students.StudentId,
                        CourseId = model.CourseId,
                        SemesterId = model.SemesterId,
                        //LevelId = (int)student.Students.LevelId, 
                        LevelId = courseDetails.levelId,// Replace with this code later
                        SessionId = model.SessionId,
                        ProgrammeId = (int)student.Students.ProgrammeId,
                        CaScore = 0,
                        ExamScore = 0,
                        StaffName = userId,
                        IsAbsentForExam = false
                    };
                    myCalist.Add(ca);
                }
            }
            // return myCalist;
            myCalist = myCalist.OrderBy(o => o.Student?.MatricNo).ToList();
            return new Tuple<List<ContinuousAssessmentVm>, string>(myCalist, "success");
        }

        // GET: ContinuousAssessments/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ContinuousAssessment continuousAssessment = await _db.ContinuousAssessments.FindAsync(id);
            if (continuousAssessment == null)
            {
                return HttpNotFound();
            }
            return View(continuousAssessment);
        }

        [HttpGet]
        public ActionResult Register(int? ProgrammeId, int? LevelId)
        {
            if (ProgrammeId != null && LevelId != null)
            {
                return RedirectToAction("Create", new { programeId = (int)ProgrammeId, levelId = (int)LevelId });
            }

            ViewBag.LevelId = new SelectList(_db.Levels.AsNoTracking(), "LevelId", "LevelName");
            ViewBag.ProgrammeId = new SelectList(_db.Programmes.AsNoTracking(), "ProgrammeId", "ProgrammeCode");
            return View();
        }

        // GET: ContinuousAssessments/Create
        public ActionResult Create(int schoolProgrammeId, int? programeId, int? levelId)
        {
            if (programeId != null && levelId != null)
            {
                var semes = _query.GetCurrentSemester(schoolProgrammeId);
                // ViewBag.StudentId = User.Identity.GetUserName();
                ViewBag.CourseId = new MultiSelectList(_db.CourseRegistrations.AsNoTracking().Where(x => x.Programme.ProgrammeId.Equals((int)programeId)
                                                        && x.Level.LevelId.Equals((int)levelId) && x.Semester.SemesterId.Equals(semes.SemesterId))
                                                        .DistinctBy(c => c.CourseId),
                                                         "CourseId", "CourseCode");
                ViewBag.LevelId = new SelectList(_db.Levels.AsNoTracking().Where(x => x.LevelId.Equals((int)levelId)), "LevelId", "LevelName");
                ViewBag.ProgrammeId = new SelectList(_db.Programmes.AsNoTracking().Where(x => x.ProgrammeId.Equals((int)programeId)), "ProgrammeId", "ProgrammeName");
                // ViewBag.DepartmentId = new SelectList(_db.Programmes.AsNoTracking().Where(x => x.ProgrammeId.Equals((int)programeId)), "DepartmentId", "DepartmentName");


                ViewBag.SemesterId = new SelectList(_query.GetCurrentSemesterList(schoolProgrammeId), "SemesterId", "SemesterName");
                ViewBag.SessionId = new SelectList(_query.GetCurrentSessionList(schoolProgrammeId), "SessionId", "SessionName");
                ViewBag.StudentId = new SelectList(_db.CourseRegistrations.Where(x => x.Programme.ProgrammeId.Equals((int)programeId)
                                                        && x.Level.LevelId.Equals((int)levelId) && x.Semester.SemesterId.Equals(semes.SemesterId))
                                                        .DistinctBy(c => c.StudentId), "StudentId", "StudentId");
            }
            ViewBag.StudentId = new SelectList(_db.Students.Where(x => x.Active.Equals(true)), "StudentId", "StudentId");
            ViewBag.LevelId = new SelectList(_db.Levels.AsNoTracking(), "LevelId", "LevelName");
            ViewBag.CourseId = new SelectList(_db.Courses.AsNoTracking(), "CourseId", "CourseCode");
            ViewBag.ProgrammeId = new SelectList(_db.Programmes.AsNoTracking(), "ProgrammeId", "ProgrammeCode");
            ViewBag.SemesterId = new SelectList(_db.Semesters.AsNoTracking(), "SemesterId", "SemesterName");
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            return View();
        }

        // POST: ContinuousAssessments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "ContinuousAssessmentId,StudentId,SemesterId,SessionId,CourseId,ProgrammeId,PracticalScore,Assignment,Test,ExamScore,StaffName,LevelId")] ContinuousAssessmentVm model)
        {
            if (ModelState.IsValid)
            {
                var continuousAssessment = new ContinuousAssessment()
                {
                    StudentId = model.StudentId,
                    SemesterId = model.SemesterId,
                    SessionId = model.SessionId,
                    CourseId = model.CourseId,
                    ProgrammeId = model.ProgrammeId,
                    LevelId = model.LevelId,
                    CaScore = model.CaScore,
                    ExamScore = model.ExamScore,
                    StaffName = model.StaffName,
                    Total = model.Total,
                    Grading = model.Grading,
                    Remark = model.Remark,
                    GradePoint = model.GradePoint,
                    QualityPoint = model.QualityPoint,
                    CourseUnit = model.CourseUnit
                };

                _db.ContinuousAssessments.Add(continuousAssessment);

                await _db.SaveChangesAsync();

                TempData["UserMessage"] = "Continuous Assessment Added Successfully.";
                TempData["Title"] = "Success.";
            }
            ViewBag.StudentId = new SelectList(_db.CourseRegistrations.Where(x => x.Programme.ProgrammeId.Equals(model.ProgrammeId)
                                                       && x.Level.LevelId.Equals(model.LevelId) && x.Semester.SemesterId.Equals(model.SemesterId))
                                                       .DistinctBy(c => c.StudentId), "StudentId", "StudentId");
            ViewBag.LevelId = new SelectList(_db.Levels.AsNoTracking(), "LevelId", "LevelName");
            ViewBag.CourseId = new SelectList(_db.Courses.AsNoTracking(), "CourseId", "CourseCode", model.CourseId);
            ViewBag.ProgrammeId = new SelectList(_db.Programmes.AsNoTracking(), "ProgrammeId", "ProgrammeCode", model.ProgrammeId);
            ViewBag.SemesterId = new SelectList(_db.Semesters.AsNoTracking(), "SemesterId", "SemesterName", model.SemesterId);
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName", model.SessionId);
            return View(model);
        }

        #region edit funtionality of CA
        // GET: ContinuousAssessments/Edit/5
        //public async Task<ActionResult> Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    ContinuousAssessment model = await _db.ContinuousAssessments.FindAsync(id);
        //    if (model == null)
        //    {
        //        return HttpNotFound();
        //    }

        //    var continuousAssessment = new ContinuousAssessmentVm()
        //    {
        //        StudentId = model.StudentId,
        //        SemesterId = model.SemesterId,
        //        SessionId = model.SessionId,
        //        CourseId = model.CourseId,
        //        ProgrammeId = (int)model.ProgrammeId,
        //        LevelId = (int)model.LevelId,
        //        //FirstTest = model.FirstTest,
        //        //SecondTest = model.SecondTest,
        //        CaScore = model.CaScore,
        //        ExamScore = model.ExamScore,
        //        StaffName = model.StaffName
        //        //Total = model.Total,
        //        //Grading = model.Grading,
        //        //Remark = model.Remark,
        //        //GradePoint = model.GradePoint,
        //        //QualityPoint = model.QualityPoint

        //    };

        //    ViewBag.LevelId = new SelectList(_db.Levels.AsNoTracking(), "LevelId", "LevelName");
        //    ViewBag.CourseId = new SelectList(_db.Courses.AsNoTracking(), "CourseId", "CourseCode", continuousAssessment.CourseId);
        //    ViewBag.ProgrammeId = new SelectList(_db.Programmes.AsNoTracking(), "ProgrammeId", "ProgrammeCode", continuousAssessment.ProgrammeId);
        //    ViewBag.SemesterId = new SelectList(_db.Semesters.AsNoTracking(), "SemesterId", "SemesterName", continuousAssessment.SemesterId);
        //    ViewBag.SessionId = new SelectList(_db.Sessions.AsNoTracking(), "SessionId", "SessionName", continuousAssessment.SessionId);
        //    return View(continuousAssessment);
        //}

        //// POST: ContinuousAssessments/Edit/5
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Edit(ContinuousAssessmentVm model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var continuousAssessment = new ContinuousAssessment()
        //        {
        //            StudentId = model.StudentId,
        //            SemesterId = model.SemesterId,
        //            SessionId = model.SessionId,
        //            CourseId = model.CourseId,
        //            ProgrammeId = model.ProgrammeId,
        //            LevelId = model.LevelId,
        //            //FirstTest = model.FirstTest,
        //            //SecondTest = model.SecondTest,
        //            CaScore = model.CaScore,
        //            ExamScore = model.ExamScore,
        //            StaffName = model.StaffName,
        //            Total = model.Total,
        //            Grading = model.Grading,
        //            Remark = model.Remark,
        //            GradePoint = model.GradePoint,
        //            QualityPoint = model.QualityPoint,
        //            CourseUnit = model.CourseUnit

        //        };
        //        _db.Entry(continuousAssessment).State = System.Data.Entity.EntityState.Modified;
        //        await _db.SaveChangesAsync();
        //        TempData["UserMessage"] = "Continuous Assessment Updated Successfully.";
        //        TempData["Title"] = "Success.";
        //        return RedirectToAction("Index");
        //    }
        //    ViewBag.LevelId = new SelectList(_db.Levels.AsNoTracking(), "LevelId", "LevelName");
        //    ViewBag.CourseId = new SelectList(_db.Courses.AsNoTracking(), "CourseId", "CourseCode", model.CourseId);
        //    ViewBag.ProgrammeId = new SelectList(_db.Programmes.AsNoTracking(), "ProgrammeId", "ProgrammeCode", model.ProgrammeId);
        //    ViewBag.SemesterId = new SelectList(_db.Semesters.AsNoTracking(), "SemesterId", "SemesterName", model.SemesterId);
        //    ViewBag.SessionId = new SelectList(_db.Sessions.AsNoTracking(), "SessionId", "SessionName", model.SessionId);
        //    return View(model);
        //} 
        #endregion

        // GET: ContinuousAssessments/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ContinuousAssessment continuousAssessment = await _db.ContinuousAssessments.FindAsync(id);
            if (continuousAssessment == null)
            {
                return HttpNotFound();
            }
            return View(continuousAssessment);
        }

        // POST: ContinuousAssessments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            ContinuousAssessment continuousAssessment = await _db.ContinuousAssessments.FindAsync(id);
            if (continuousAssessment != null) _db.ContinuousAssessments.Remove(continuousAssessment);
            await _db.SaveChangesAsync();
            TempData["UserMessage"] = "Continuous Assessment Deleted Successfully.";
            TempData["Title"] = "Error.";

            return RedirectToAction("Index");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SaveCa(List<ContinuousAssessmentVm> model)
        {
            if (ModelState.IsValid)
            {
                bool isAvailable = await _resultQuery.CheckForResult(model[0].CourseId, model[0].SessionId, model[0].LevelId, model[0].ProgrammeId);
                if (isAvailable && model[0].ContinuousAssessmentId == 0)
                {
                    return RedirectToAction("CreateCaView", new { message = "This result has already been uploaded. Download the recent version of the result to upload again" });
                }
                foreach (var item in model)
                {
                    var continiousAssesment = new ContinuousAssessment()
                    {
                        ContinuousAssessmentId = item.ContinuousAssessmentId,
                        StudentId = item.StudentId,
                        ProgrammeId = item.ProgrammeId,
                        CourseId = item.CourseId,
                        LevelId = item.LevelId,
                        SemesterId = item.SemesterId,
                        SessionId = item.SessionId,
                        CourseUnit = item.CourseUnit,
                        CaScore = item.CaScore,
                        ExamScore = item.ExamScore,
                        Total = item.Total,
                        Grading = item.Grading,
                        Remark = item.Remark,
                        GradePoint = item.GradePoint,
                        QualityPoint = item.QualityPoint,
                        StaffName = item.StaffName,
                        Submitted = true,
                        IsAbsentForExam = item.IsAbsentForExam,
                        ReasonForReject = "Lecturer Submit"
                    };
                    _db.ContinuousAssessments.AddOrUpdate(continiousAssesment);
                }
                await _db.SaveChangesAsync();
                return RedirectToAction("LecturerIndex");
            }

            return View("CreateCa");
        }

        [HttpGet]
        public PartialViewResult UploadResult()
        {
            return PartialView();
        }

        [HttpGet]
        public ActionResult UploadResultPage()
        {
            return View();
        }


        [HttpPost]
        public async Task<ActionResult> UploadResult(HttpPostedFileBase excelfile)
        {
            if (excelfile == null || excelfile.ContentLength == 0)
            {
                ViewBag.Error = "Please Select a excel file <br/>";
                TempData["UserMessage"] = "Please Select a excel file.";
                TempData["Title"] = "Error.";

                return RedirectToAction("Index");
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
                    var continiousAssesmentVmList = new List<ContinuousAssessmentVm>();
                    ExcelValidation myExcel = new ExcelValidation();
                    var currentSheet = package.Workbook.Worksheets;
                    var workSheet = currentSheet.First();
                    var noOfCol = workSheet.Dimension.End.Column;
                    var noOfRow = workSheet.Dimension.End.Row;

                    var programmeId = Convert.ToInt32(workSheet.Cells["AA2"].Value.ToString().Trim());
                    var courseId = Convert.ToInt32(workSheet.Cells["AA3"].Value.ToString().Trim());
                    var levelId = Convert.ToInt32(workSheet.Cells["AA4"].Value.ToString().Trim());
                    var semester = Convert.ToInt32(workSheet.Cells["AA5"].Value.ToString().Trim());
                    var session = Convert.ToInt32(workSheet.Cells["AA6"].Value.ToString().Trim());
                    string staffName = userId;
                    for (int row = 9; row <= noOfRow; row++)
                    {
                        int caId = Convert.ToInt32(workSheet.Cells[row, 1].Value.ToString().Trim());
                        string studentName = workSheet.Cells[row, 2].Value.ToString().Trim();
                        string studentId = workSheet.Cells[row, 3].Value.ToString().Trim();
                        double thirdTest = Convert.ToDouble(workSheet.Cells[row, 4].Value.ToString().Trim());
                        double examScore = Convert.ToDouble(workSheet.Cells[row, 5].Value.ToString().Trim());
                        string isAbsent = workSheet.Cells[row, 6].Value.ToString().Trim();

                        bool isAbsentExam = false || (isAbsent.ToUpper().Equals("TRUE") || isAbsent.ToUpper().Equals("YES"));

                        //get course code using courseId from template
                        var courseCode = await _resultQuery.getCourseCode(courseId);

                        if (courseCode == null)
                        {
                            continue;
                        }

                        //get specific student's courseID
                        var result = _studentQuery.GetStudentByMatNumber(studentId);
                        if (result == null)
                        {
                            return RedirectToAction("CreateCaView", new { message = $" {studentId} this matriculation number is not correct" });

                        }
                        programmeId = (Int32)_studentQuery.GetStudentByMatNumber(studentId).ProgrammeId;
                        courseId = await _resultQuery.getCourseIdForStudentProgramme(programmeId, courseCode.Replace(" ", ""));
                        if (!(courseId > 0))
                        {
                            ViewBag.ErrorMessage = "Applicant not found";
                            utmeUploadError.Add(new StudentUtmeUploadErrorVm { JambRegNo = studentId, Row = row });
                            hasUtmeUploadError = true;
                        }

                        var model = await GetModelDetails(studentId, levelId, courseId, semester, session, programmeId); 
                        var student =  _studentQuery.GetStudentByMatNumber(studentId);

                        if (student == null)
                        {
                            return RedirectToAction("CreateCaView", new { message = $" {studentId} this matriculation number is not correct" });
                        }
                        int checkResultTemplate = _resultQuery.CheckForResultTemplate(model.CourseId, model.ResultSessionId, model.LevelName);
                        if (checkResultTemplate <= 0)
                        {
                            return RedirectToAction("CreateCaView", new { message = $"Grading has not been set for this {studentId} please contact their HOD" });
                        }
                        bool isAvailable = await _resultQuery.CheckForResult(model.CourseId, model.SessionId, levelId, programmeId);

                        //check for courseReg and act befittingly

                        if (isAvailable && caId == 0)
                        {
                            continue;
                            //return RedirectToAction("CreateCaView", new { message = "This result has already been uploaded. Download the recent version of the result to upload again" });
                        }
                        bool isDeptApproved = await _resultQuery.CheckDeptApproval(model.CourseId, model.SessionId, levelId, programmeId);
                        if (isDeptApproved)
                        {
                            return RedirectToAction("CreateCaView", new { message = "This result has already been approved by the Department. It can't be edited again until its rejected by the department" });
                        }

                        try
                        {
                            var vm = new ContinuousAssessmentVm()
                            {
                                StudentId = model.StudentId,
                                CourseId = model.CourseId,
                                SemesterId = model.SemesterId,
                                SessionId = model.SessionId,
                                LevelId = model.LevelId,
                                ProgrammeId = model.ProgrammeId,
                                StaffName = staffName,
                                CaScore = thirdTest,
                                ExamScore = examScore,
                            };
                            var continiousAssesment = new ContinuousAssessment()
                            {
                                ContinuousAssessmentId = caId,
                                StudentId = vm.StudentId,
                                ProgrammeId = vm.ProgrammeId,
                                CourseId = vm.CourseId,
                                LevelId = vm.LevelId,
                                SemesterId = vm.SemesterId,
                                SessionId = vm.SessionId,
                                CourseUnit = vm.CourseUnit,
                                CaScore = thirdTest,
                                ExamScore = examScore,
                                Total = vm.Total,
                                Grading = vm.Grading,
                                Remark = vm.Remark,
                                GradePoint = vm.GradePoint,
                                QualityPoint = vm.QualityPoint,
                                StaffName = vm.StaffName,
                                IsAbsentForExam = isAbsentExam,
                                Submitted = true,
                                ReasonForReject = "Lecturer Submit"
                            };
                            _db.ContinuousAssessments.AddOrUpdate(continiousAssesment);
                        }
                        catch (Exception ex)
                        {
                            ViewBag.ErrorInfo = "The programme code in the excel doesn't exist";
                            ViewBag.ErrorMessage = ex.Message;
                            return View("ErrorException");
                        }
                    }
                    await _db.SaveChangesAsync();
                    message = $"You have successfully Uploaded {recordCount} records...  and {lastrecord}";
                }

                if (hasUtmeUploadError)
                {
                    ViewBag.ErrorInfo = $"Error!";
                    ViewBag.ErrorMessage = $"You have successfully Uploaded {recordCount} records...";
                    return View("ErrorException", utmeUploadError);
                }
                return RedirectToAction("Index", "ContinuousAssessments");
            }

            ViewBag.Error = $"File type is Incorrect <br/>";
            return RedirectToAction("Index");
        }


        [HttpGet]
        public ActionResult UploadLegacyResult()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> UploadLegacyResult(HttpPostedFileBase excelfile)
        {
            if (excelfile == null || excelfile.ContentLength == 0)
            {
                ViewBag.Error = "Please Select a excel file <br/>";
                TempData["UserMessage"] = "Please Select a excel file.";
                TempData["Title"] = "Error.";

                return RedirectToAction("Index");
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
                    var continiousAssesmentVmList = new List<ContinuousAssessmentVm>();
                    ExcelValidation myExcel = new ExcelValidation();
                    var currentSheet = package.Workbook.Worksheets;
                    var workSheet = currentSheet.First();
                    var noOfCol = workSheet.Dimension.End.Column;
                    var noOfRow = workSheet.Dimension.End.Row;
                    int requiredField = 9;

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

                    string staffName = userId;
                    int caId = 0;
                    for (int row = 2; row <= noOfRow; row++)
                    {
                        string matricNo = workSheet.Cells[row, 1].Value.ToString().Trim();
                        string courseCode = workSheet.Cells[row, 2].Value.ToString().Trim();
                        string programmeCode = workSheet.Cells[row, 3].Value.ToString().Trim();
                        string levelName = workSheet.Cells[row, 4].Value.ToString().Trim();
                        string semesterName = workSheet.Cells[row, 5].Value.ToString().Trim();
                        string sessionName = workSheet.Cells[row, 6].Value.ToString().Trim();
                        double caScore = Convert.ToDouble(workSheet.Cells[row, 7].Value.ToString().Trim());
                        double examScore = Convert.ToDouble(workSheet.Cells[row, 8].Value.ToString().Trim());
                        string isAbsent = workSheet.Cells[row, 9].Value.ToString().Trim();

                        var student = await _db.Students.AsNoTracking().Include(i => i.Programme)
                                           .Where(x => x.MatricNo.Trim().ToUpper().Equals(matricNo.Trim().ToUpper()))
                                           .FirstOrDefaultAsync();

                        var programme = GetProgrammeByCode(programmeCode, allProgramme);
                        var course = GetCourseByCode(courseCode, allCourse, programme);
                        var level = GetLevelIdByName(levelName, allLevel);
                        var semester = GetSemesterIdByName(semesterName, allSemesters);
                        var session = GetSessionIdByName(sessionName, allSessions);

                        if (student == null)
                        {
                            ViewBag.ErrorInfo = $"This {matricNo} Matric Number at row {row} doesn't exist in the student record on the system.";
                            ViewBag.ErrorMessage = "Please check the Matric Number type spelling very well ";
                            return View("ErrorException");
                        }
                        if (programme == 0)
                        {
                            ViewBag.ErrorInfo = $"This {programmeCode} Dept Option Code Code  at row {row} doesn't exist in the system.";
                            ViewBag.ErrorMessage = "Please check the Dept Option Code type spelling very well ";
                            return View("ErrorException");
                        }
                        if (course == 0)
                        {
                            ViewBag.ErrorInfo = $"This {courseCode} Course Code  at row {row} doesn't exist in the list of courses for this Dept Option on the system.";
                            ViewBag.ErrorMessage = "Please check the Course code type spelling very well ";
                            return View("ErrorException");
                        }
                        if (level == 0)
                        {
                            ViewBag.ErrorInfo = $"This {levelName} Level Name  at row {row} doesn't exist in the system.";
                            ViewBag.ErrorMessage = "Please check the Level Name spelling very well ";
                            return View("ErrorException");
                        }
                        if (semester == 0)
                        {
                            ViewBag.ErrorInfo = $"This {semester} Semester Code  at row {row} doesn't exist in the system.";
                            ViewBag.ErrorMessage = "Please check the Semester Name spelling very well ";
                            return View("ErrorException");
                        }

                        if (session == 0)
                        {
                            ViewBag.ErrorInfo = $"This {sessionName} Session Name at row {row} doesn't exist in the system.";
                            ViewBag.ErrorMessage = "Please check the Session Name spelling very well ";
                            return View("ErrorException");
                        }

                        bool isAbsentExam = false || (isAbsent.ToUpper().Equals("TRUE") || isAbsent.ToUpper().Equals("YES"));

                        var model = await GetModelDetails(student.MatricNo, level, course, semester,
                                            session, programme);
                        int checkResultTemplate = _resultQuery.CheckForResultTemplate(model.CourseId, model.ResultSessionId, model.LevelName);
                        if (checkResultTemplate <= 0)
                        {
                            return RedirectToAction("CreateCaView", new { message = "Grading has not been set for this department please contact the HOD" });
                        }
                        bool isAvailable = await _resultQuery.CheckForResult(model.CourseId, model.SessionId, level, programme);
                        if (isAvailable && caId == 0)
                        {
                            return RedirectToAction("CreateCaView", new { message = "This result has already been uploaded. Download the recent version of the result to upload again" });
                        }
                        bool isDeptApproved = await _resultQuery.CheckDeptApproval(model.CourseId, model.SessionId, level, programme);
                        if (isDeptApproved)
                        {
                            return RedirectToAction("CreateCaView", new { message = "This result has already been approved by the Department. It can't be edited again until its rejected by the department" });
                        }
                        try
                        {
                            var vm = new ContinuousAssessmentVm()
                            {
                                StudentId = model.StudentId,
                                CourseId = model.CourseId,
                                SemesterId = model.SemesterId,
                                SessionId = model.SessionId,
                                LevelId = model.LevelId,
                                ProgrammeId = model.ProgrammeId,
                                StaffName = staffName,
                                CaScore = caScore,
                                ExamScore = examScore,
                            };
                            var continiousAssesment = new ContinuousAssessment()
                            {
                                StudentId = vm.StudentId,
                                ProgrammeId = vm.ProgrammeId,
                                CourseId = vm.CourseId,
                                LevelId = vm.LevelId,
                                SemesterId = vm.SemesterId,
                                SessionId = vm.SessionId,
                                CourseUnit = vm.CourseUnit,
                                CaScore = caScore,
                                ExamScore = examScore,
                                Total = vm.Total,
                                Grading = vm.Grading,
                                Remark = vm.Remark,
                                GradePoint = vm.GradePoint,
                                QualityPoint = vm.QualityPoint,
                                StaffName = vm.StaffName,
                                IsAbsentForExam = isAbsentExam,
                                Submitted = true,
                                ReasonForReject = "Lecturer Submit",
                                IsDeptApproved = true,
                                IsFacultyApproved = true,
                                IsSenateApproved = false
                            };
                            _db.ContinuousAssessments.AddOrUpdate(continiousAssesment);
                        }
                        catch (Exception ex)
                        {
                            ViewBag.ErrorInfo = "The programme code in the excel doesn't exist";
                            ViewBag.ErrorMessage = ex.Message;
                            return View("ErrorException");
                        }
                    }
                    await _db.SaveChangesAsync();
                    message = $"You have successfully Uploaded {recordCount} records...  and {lastrecord}";
                }
                return RedirectToAction("Index", "ContinuousAssessments");
            }

            ViewBag.Error = $"File type is Incorrect <br/>";
            return RedirectToAction("Index");
        }



        [HttpGet]
        public ActionResult UploadMissingResult()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> UploadMissingResult(HttpPostedFileBase excelfile)
        {
            if (excelfile == null || excelfile.ContentLength == 0)
            {
                ViewBag.Error = "Please Select a excel file <br/>";
                TempData["UserMessage"] = "Please Select a excel file.";
                TempData["Title"] = "Error.";

                return RedirectToAction("Index");
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
                    var continiousAssesmentVmList = new List<ContinuousAssessmentVm>();
                    ExcelValidation myExcel = new ExcelValidation();
                    var currentSheet = package.Workbook.Worksheets;
                    var workSheet = currentSheet.First();
                    var noOfCol = workSheet.Dimension.End.Column;
                    var noOfRow = workSheet.Dimension.End.Row;
                    int requiredField = 9;

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

                    string staffName = userId;
                    int caId = 0;
                    for (int row = 2; row <= noOfRow; row++)
                    {
                        string matricNo = workSheet.Cells[row, 1].Value.ToString().Trim();
                        string courseCode = workSheet.Cells[row, 2].Value.ToString().Trim();
                        string programmeCode = workSheet.Cells[row, 3].Value.ToString().Trim();
                        string levelName = workSheet.Cells[row, 4].Value.ToString().Trim();
                        string semesterName = workSheet.Cells[row, 5].Value.ToString().Trim();
                        string sessionName = workSheet.Cells[row, 6].Value.ToString().Trim();
                        double caScore = Convert.ToDouble(workSheet.Cells[row, 7].Value.ToString().Trim());
                        double examScore = Convert.ToDouble(workSheet.Cells[row, 8].Value.ToString().Trim());
                        string isAbsent = workSheet.Cells[row, 9].Value.ToString().Trim();

                        var student = await _db.Students.AsNoTracking().Include(i => i.Programme)
                                           .Where(x => x.MatricNo.Trim().ToUpper().Equals(matricNo.Trim().ToUpper()))
                                           .FirstOrDefaultAsync();

                        var programme = GetProgrammeByCode(programmeCode, allProgramme);
                        var course = GetCourseByCode(courseCode, allCourse, programme);
                        var level = GetLevelIdByName(levelName, allLevel);
                        var semester = GetSemesterIdByName(semesterName, allSemesters);
                        var session = GetSessionIdByName(sessionName, allSessions);

                        if (student == null)
                        {
                            ViewBag.ErrorInfo = $"This {matricNo} Matric Number at row {row} doesn't exist in the student record on the system.";
                            ViewBag.ErrorMessage = "Please check the Matric Number type spelling very well ";
                            return View("ErrorException");
                        }
                        if (programme == 0)
                        {
                            ViewBag.ErrorInfo = $"This {programmeCode} Dept Option Code Code  at row {row} doesn't exist in the system.";
                            ViewBag.ErrorMessage = "Please check the Dept Option Code type spelling very well ";
                            return View("ErrorException");
                        }
                        if (course == 0)
                        {
                            ViewBag.ErrorInfo = $"This {courseCode} Course Code  at row {row} doesn't exist in the list of courses for this Dept Option on the system.";
                            ViewBag.ErrorMessage = "Please check the Course code type spelling very well ";
                            return View("ErrorException");
                        }
                        if (level == 0)
                        {
                            ViewBag.ErrorInfo = $"This {levelName} Level Name  at row {row} doesn't exist in the system.";
                            ViewBag.ErrorMessage = "Please check the Level Name spelling very well ";
                            return View("ErrorException");
                        }
                        if (semester == 0)
                        {
                            ViewBag.ErrorInfo = $"This {semester} Semester Code  at row {row} doesn't exist in the system.";
                            ViewBag.ErrorMessage = "Please check the Semester Name spelling very well ";
                            return View("ErrorException");
                        }

                        if (session == 0)
                        {
                            ViewBag.ErrorInfo = $"This {sessionName} Session Name at row {row} doesn't exist in the system.";
                            ViewBag.ErrorMessage = "Please check the Session Name spelling very well ";
                            return View("ErrorException");
                        }


                        bool isAbsentExam = false || (isAbsent.ToUpper().Equals("TRUE") || isAbsent.ToUpper().Equals("YES"));

                        var model = await GetModelDetails(student.MatricNo, level, course, semester,
                                            session, programme);
                        int checkResultTemplate = _resultQuery.CheckForResultTemplate(model.CourseId, model.ResultSessionId, model.LevelName);
                        if (checkResultTemplate <= 0)
                        {
                            return RedirectToAction("CreateCaView", new { message = "Grading has not been set for this department please contact the HOD" });
                        }

                        bool isAvailable = await _resultQuery.CheckForResult(student.StudentId, model.CourseId, model.SessionId, level, programme);
                        if (isAvailable && caId == 0)
                        {
                            return RedirectToAction("CreateCaView", new { message = "This result has already been uploaded. Download the recent version of the result to upload again" });
                        }

                        //bool isDeptApproved = await _resultQuery.CheckDeptApproval(model.CourseId, model.SessionId, level, programme);
                        //if (isDeptApproved)
                        //{
                        //    return RedirectToAction("CreateCaView", new { message = "This result has already been approved by the Department. It can't be edited again until its rejected by the department" });
                        //}

                        try
                        {
                            var vm = new ContinuousAssessmentVm()
                            {
                                StudentId = model.StudentId,
                                CourseId = model.CourseId,
                                SemesterId = model.SemesterId,
                                SessionId = model.SessionId,
                                LevelId = model.LevelId,
                                ProgrammeId = model.ProgrammeId,
                                StaffName = staffName,
                                CaScore = caScore,
                                ExamScore = examScore,
                            };
                            var continiousAssesment = new ContinuousAssessment()
                            {
                                StudentId = vm.StudentId,
                                ProgrammeId = vm.ProgrammeId,
                                CourseId = vm.CourseId,
                                LevelId = vm.LevelId,
                                SemesterId = vm.SemesterId,
                                SessionId = vm.SessionId,
                                CourseUnit = vm.CourseUnit,
                                CaScore = caScore,
                                ExamScore = examScore,
                                Total = vm.Total,
                                Grading = vm.Grading,
                                Remark = vm.Remark,
                                GradePoint = vm.GradePoint,
                                QualityPoint = vm.QualityPoint,
                                StaffName = vm.StaffName,
                                IsAbsentForExam = isAbsentExam,
                                Submitted = true,
                                ReasonForReject = "Lecturer Submit",
                                IsDeptApproved = true,
                                IsFacultyApproved = true,
                                IsSenateApproved = false
                            };
                            _db.ContinuousAssessments.AddOrUpdate(continiousAssesment);
                        }
                        catch (Exception ex)
                        {
                            ViewBag.ErrorInfo = "The programme code in the excel doesn't exist";
                            ViewBag.ErrorMessage = ex.Message;
                            return View("ErrorException");
                        }
                    }
                    await _db.SaveChangesAsync();
                    message = $"You have successfully Uploaded {recordCount} records...  and {lastrecord}";
                }
                return RedirectToAction("Index", "ContinuousAssessments");
            }

            ViewBag.Error = $"File type is Incorrect <br/>";
            return RedirectToAction("Index");
        }


        private async Task<GetCaRecordVm> GetModelDetails(string matricNo, int levelId, int courseId, int psemesterId, int psessionId,
                        int programmeId)
        {
            var studentId = await _db.Students.AsNoTracking().Where(x => x.MatricNo.Trim().ToUpper().Equals(matricNo.Trim().ToUpper()))
                                .Select(s => new { s.StudentId, s.SchoolProgrammeId }).FirstOrDefaultAsync();
            if (studentId != null )
            {
                var model = new GetCaRecordVm
                {
                    StudentId = studentId.StudentId,
                    LevelId = levelId,
                    CourseId = courseId,
                    SemesterId = psemesterId,
                    SessionId = psessionId,
                    ProgrammeId = programmeId,
                    DepartmentId = _db.Programmes.AsNoTracking().Include(i => i.Department).Where(x => x.ProgrammeId.Equals(programmeId))
                                .Select(s => s.Department.DepartmentId).FirstOrDefault(),
                    SchoolProgrammeId = studentId.SchoolProgrammeId,
                    LevelName = _query.GetLevelById(levelId).LevelOrder,
                    ResultSessionId = psessionId
                };
                return model;

            }

            return null;

        }

        public ActionResult GetDeptResultsUploaded()
        {
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FancyName");
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");

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

        public async Task<ActionResult> GetGetDeptResultsUploadedIndex(int? SchoolProgrammeId, int? FacultyId, int? DepartmentId, int? ProgrammeId,
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

           
                var query = _db.ContinuousAssessments
                           .Include(c => c.Programme)
                           .Include(c => c.Programme.Department)
                           .Include(c => c.Programme.Department.Faculty)
                           .Include(c => c.Programme)
                           .Where(ca => ca.SessionId.Equals((Int32)SessionId))
                           .OrderBy(ca => ca.Programme.Department.DeptName)
                           .ThenBy(ca => ca.Programme.ProgrammeName)
                           .ThenBy(ca => ca.Course.CourseName)
                           .Select(ca => new
                           {
                               CourseName = ca.Course.CourseName,
                               ProgrammeName = ca.Programme.ProgrammeName,
                               DepartmentName = ca.Programme.Department.DeptName,
                               FacultyName = ca.Programme.Department.Faculty.FacultyName,
                               Lecturer = ca.StaffName,
                               Semester = ca.Semester.SemesterName,
                               DepartmentApprove = ca.IsDeptApproved,
                               FacultyApprove = ca.IsFacultyApproved,
                               SenateApprove = ca.IsSenateApproved,
                               ProgrammeId = ca.ProgrammeId,
                               DepartmentId = ca.Programme.Department.DepartmentId,
                               FacultyId = ca.Programme.Department.Faculty.FacultyId,
                               LevelId = ca.LevelId
                               // Include other properties you may need
                           })
                           .Distinct()
                           .ToList();

            if (FacultyId != null)
            {
                query = query.Where(w => w.FacultyId.Equals(FacultyId)).ToList();
            }

            if (DepartmentId != null)
            {
                query = query.Where(w => w.DepartmentId.Equals(DepartmentId)).ToList();
            }

            if (ProgrammeId != null)
            {
                query = query.Where(w => w.ProgrammeId.Equals(ProgrammeId)).ToList();
            }

            if (LevelId != null)
            {
                query = query.Where(w => w.LevelId.Equals(LevelId)).ToList();
            }

            var viewModel = query.Select(item => new CourseViewModel
                {
                    CourseName = item.CourseName,
                    ProgrammeName = item.ProgrammeName,
                    DepartmentName = item.DepartmentName,
                    FacultyName = item.FacultyName,
                    FSemesterName = item.Semester,
                    IsDeptApprove = item.DepartmentApprove ? "Approved" : "Not Approved",
                    IsFacultyApprove = item.FacultyApprove ? "Approved" : "Not Approved",
                    IsSenateApprove = item.DepartmentApprove ? "Approved" : "Not Approved",
                    Lecturer = (_db.Staffs.Where(x => x.Email.Equals(item.Lecturer)).Select(u => new { Id = u.StaffId, FullName = u.FirstName + " " + u.LastName }).FirstOrDefault()).ToString()
                }).ToList();


            //if (!string.IsNullOrEmpty(search))
            //{
            //    studentIndex = query.Where(x => x.cardnumber.ToUpper().Contains(search.ToUpper().Trim())
            //                || x.firstname.Trim().ToUpper().Contains(search.ToUpper().Trim())).ToList();
            //    if (studentIndex.Count() == 0)
            //    {
            //        studentIndex = studentList.Where(x => !string.IsNullOrEmpty(x.email) &&
            //                x.email.ToUpper().Contains(search.ToUpper().Trim())).ToList();
            //    }
            //}

            //else
            //{
            //    studentIndex = studentList;
            //}

            totalRecords = viewModel.Count();
            var data = viewModel.Skip(skip).Take(pageSize).ToList();

            return Json(new { draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data },
                JsonRequestBehavior.AllowGet);

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
