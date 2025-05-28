using SwiftKampus.BusinessLogic;
using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampus.ViewModels;
using SwiftKampusModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SwiftKampus.Controllers
{
    [Authorize]
    [Audit(AuditingLevel = 2)]
    public class SiwesController : BaseController
    {
        readonly ResultCommand _resultCommand;
        public SiwesController(SchoolDbContext db) : base(db)
        {
            _resultCommand = new ResultCommand(db);
        }

        // GET: Siwes
        public ActionResult Index()
        {
            ViewBag.FacultyId = new SelectList(_db.Faculties.AsNoTracking(), "FacultyId", "FacultyName");
            ViewBag.DepartmentId = new SelectList(_db.Departments.AsNoTracking(), "DepartmentId", "DeptName");
            ViewBag.ProgrammeId = new SelectList(_db.Programmes.AsNoTracking(), "ProgrammeId", "ProgrammeName");
            return View();
        }

        public async Task<ActionResult> GetIndex(int? ProgrammeId, int? levelId)
        {
            var data = new List<StudentIndexVM>();
            if (ProgrammeId != null)
            {
                var schoolProgrammeId = await GetUndergraduateSchoolProgrammeId();
                sessionId = _query.GetCurrentSessionId(schoolProgrammeId);
                //var students = await _resultCommand.StudentRegForCourse((int)ProgrammeId, sessionId);
                var qualifiedStudent = new List<Student>();
                qualifiedStudent.AddRange(_resultCommand.GetSiwesList((int)ProgrammeId, (int)levelId));

                data = qualifiedStudent.Select(s => new StudentIndexVM()
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
                    LevelName = s.Level.LevelName
                }).ToList();
            }
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult IntroductionLetter(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                studentId = userId;
            }
            var student = _studentQuery.GetStudent(userId);

            if (_resultCommand.GetStudentSiwesStatus(student.Programme.ProgrammeId, studentId))
            {
                return View(student);
            }
            ViewBag.Message = "You are not qualified for SIWES";
            return View();
        }

        public ActionResult AcceptanceLetter(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                studentId = userId;
            }
            var student = _studentQuery.GetStudent(userId);

            if (_resultCommand.GetStudentSiwesStatus(student.Programme.ProgrammeId, studentId))
            {
                return View(student);
            }
            ViewBag.Message = "You are not qualified for SIWES";
            return View();
        }
    }
}