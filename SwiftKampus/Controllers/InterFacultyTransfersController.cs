using SwiftKampus.Controllers;
using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampusModel.StudentStatusManagement;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Unijos.Web.ViewModels.StudentStatus;

namespace Unijos.Web.Controllers.StudentStatus
{
    [Authorize]
    [Audit(AuditingLevel = 2)]
    public class InterFacultyTransfersController : BaseController
    {
        public InterFacultyTransfersController(SchoolDbContext db) : base(db)
        {
        }


        // GET: InterFacultyTransfers/ApplicationForm
        public async Task<ActionResult> ApplicationForm()
        {
            var studentId = _studentQuery.GetStudentId(userId);
            var currentSessionId = sessionId;
            var student = await _db.Students
                .Include(st => st.Programme.Department.Faculty)
                .Include(st => st.Programme.Department)
                .Include(st => st.Programme)
                .AsNoTracking()
                .Where(u => u.StudentId.Equals(studentId)).FirstOrDefaultAsync();

            var transfer = new InterFacultyTransferApplicationViewModel()
            {
                Student = student,
                SessionId = currentSessionId
            };

            var departmentOptions = await _db.Programmes.Include(i => i.Department.Faculty).AsNoTracking()
                                        .Where(dp => dp.Department.Faculty.FacultyId.Equals(student.Programme.Department.Faculty.FacultyId))
                                        .ToListAsync();

            ViewBag.NewDepartmentOptionId = new SelectList(departmentOptions, "DepartmentOptionId", "DeptOptionName");

            return View("ApplicationForm", transfer);
        }

        // POST: /InterFacultyTransfers/ApplicationForm/
        [HttpPost]
        public async Task<JsonResult> ApplicationForm(string StudentId, int NewDepartmentOptionId)
        {
            if (ModelState.IsValid)
            {
                var student = await _db.Students.Where(st => st.StudentId.Equals(StudentId)).FirstOrDefaultAsync();
                var now = DateTime.Now;
                var date = new DateTime(now.Year, now.Month, now.Day);

                Transfer transfer = new Transfer()
                {
                    StudentId = StudentId,
                    ProgrammeId = NewDepartmentOptionId,
                    SessionId = sessionId
                };
                _db.Transfers.Add(transfer);
                _db.SaveChanges();
                TransferProcess transferProcess = new TransferProcess()
                {
                    TransferId = transfer.TransferId
                };
                _db.TransferProcesses.Add(transferProcess);
                _db.SaveChanges();

                return Json(new
                {
                    transferProcess.TransferId,
                    DepartmentOptionId = transfer.ProgrammeId,
                }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { DepartmentOptionId = "" }, JsonRequestBehavior.AllowGet);
        }

        // GET: InterFacultyTransfers/Students
        public ActionResult Students()
        {
            return View();
        }

        // GET: InterFacultyTransfers/GandCStudents
        public async Task<JsonResult> GandCStudents()
        {
            var data = await _db.TransferProcesses
                .Include(t => t.Transfer)
                .Include(t => t.Transfer.Student.Programme)
                .Where(t => t.GandCActed.Equals(null))
                .AsNoTracking()
                .Select(t => new ListOfStudentsApplyingForInterFacultyViewModel
                {
                    MatriculationNumber = t.Transfer.Student.MatricNo,
                    FullName = t.Transfer.Student.LastName + " " + t.Transfer.Student.FirstName + " " + t.Transfer.Student.MiddleName,
                    FacultyName = t.Transfer.Student.Programme.Department.Faculty.FacultyName,
                    DepartmentName = t.Transfer.Student.Programme.Department.DeptName,
                    DepartmentOptionName = t.Transfer.Student.Programme.ProgrammeName,
                    TransferProcessId = t.TransferId
                })
                .ToListAsync();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        // GET: /Extensions/JSONReqestDetailsForGandC/
        public async Task<JsonResult> JSONReqestDetailsForGandC(int id)
        {

            var student = _db.TransferProcesses.Include(i => i.Transfer).Include(i => i.Transfer.Student)
                                .AsNoTracking().Where(x => x.TransferId.Equals(id))
                                .Select(s => s.Transfer.Student).FirstOrDefault();
            var currentSessionName = _query.GetCurrentSemesterName(student.SchoolProgrammeId);

            var request = await _db.TransferProcesses
                .Include(t => t.Transfer)
                .Include(t => t.Transfer.CurrentProgramme)
                .AsNoTracking()
                .Select(t => new InterFacultyTransferApplicationViewModel
                {
                    Student = student,
                    Programme = t.Transfer.CurrentProgramme,
                    SessionName = currentSessionName,
                    TransferId = t.TransferId
                })
                .Where(t => t.TransferId.Equals(id)).FirstOrDefaultAsync();

            return Json(request, JsonRequestBehavior.AllowGet);
        }

        // POST: /InterFacultyTransfers/JSONGandCCommentForm/
        [HttpPost]
        public async Task<JsonResult> JSONGandCCommentForm(string TransferProcessId, string GandCRecommendation, bool GandCActed)
        {
            if (ModelState.IsValid)
            {
                var now = DateTime.Now;
                var date = new DateTime(now.Year, now.Month, now.Day);
                var id = Convert.ToInt32(TransferProcessId);
                var transferProcess = await _db.TransferProcesses
                    .Where(tp => tp.TransferId.Equals(id)).FirstOrDefaultAsync();
                transferProcess.GandCRecommendation = GandCRecommendation;
                transferProcess.GandCActed = GandCActed;
                transferProcess.GandCRecommendationDate = date;
                _db.SaveChanges();

                return Json(new
                {
                    transferProcess.GandCRecommendation,
                    transferProcess.GandCActed,
                    transferProcess.GandCRecommendationDate
                }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { GandCRecommendation = "" }, JsonRequestBehavior.AllowGet);
        }

        // List students inter faculty requests that current HOD has not commented on
        // GET: InterFacultyTransfers/StudentReqestsForHOD
        public ActionResult StudentReqestsForHOD()
        {
            return View();
        }

        // GET: InterFacultyTransfers/JSONStudentReqestsForHOD
        public async Task<JsonResult> JSONStudentLeaveReqestsForHOD()
        {
            var students = await _db.TransferProcesses
                .Include(t => t.Transfer)
                .Include(t => t.Transfer.CurrentProgramme)
                .AsNoTracking()
                .Where(t => t.HODActted.Equals(null) && !t.GandCActed.Equals(null))
                .Select(t => new ListOfStudentsApplyingForInterFacultyViewModel
                {
                    MatriculationNumber = t.Transfer.Student.MatricNo,
                    FullName = t.Transfer.Student.LastName + " " + t.Transfer.Student.FirstName + " " + t.Transfer.Student.MiddleName,
                    FacultyName = t.Transfer.Student.Programme.Department.Faculty.FacultyName,
                    DepartmentName = t.Transfer.Student.Programme.Department.Faculty.FacultyName,
                    DepartmentOptionName = t.Transfer.Student.Programme.ProgrammeName,
                    TransferProcessId = t.TransferId
                })
                .ToListAsync();

            return Json(students, JsonRequestBehavior.AllowGet);
        }

        // GET: /InterFacultyTransfers/JSONReqestDetailsForHOD/
        public async Task<JsonResult> JSONReqestDetailsForHOD(int id)
        {

            var student = _db.TransferProcesses.Include(i => i.Transfer).Include(i => i.Transfer.Student)
                               .AsNoTracking().Where(x => x.TransferId.Equals(id))
                               .Select(s => s.Transfer.Student).FirstOrDefault();
            var currentSessionName = _query.GetCurrentSemesterName(student.SchoolProgrammeId);

            var request = await _db.TransferProcesses
                .Include(t => t.Transfer)
                .Include(t => t.Transfer.CurrentProgramme)
                .AsNoTracking()
                .Select(t => new InterFacultyTransferApplicationViewModel
                {
                    Student = student,
                    Programme = t.Transfer.CurrentProgramme,
                    SessionName = currentSessionName,
                    TransferId = t.TransferId,
                    //ApplicationDate = t.a.Value.ToString(),
                    GandCRecommendation = t.GandCRecommendation,
                    GandCRecommendationDate = t.GandCRecommendationDate.Value.ToString()
                })
                .Where(t => t.TransferId.Equals(id)).FirstOrDefaultAsync();

            return Json(request, JsonRequestBehavior.AllowGet);
        }

        // POST: /InterFacultyTransfers/JSONHODCommentForm/
        [HttpPost]
        public async Task<JsonResult> JSONHODCommentForm(string TransferProcessId, string HODComment, bool HODActted)
        {
            if (ModelState.IsValid)
            {
                var now = DateTime.Now;
                var date = new DateTime(now.Year, now.Month, now.Day);
                var id = Convert.ToInt32(TransferProcessId);
                var transferProcess = await _db.TransferProcesses
                    .Where(tp => tp.TransferId.Equals(id)).FirstOrDefaultAsync();
                transferProcess.HODComment = HODComment;
                transferProcess.HODActted = HODActted;
                transferProcess.HODCommentDate = date;
                _db.SaveChanges();

                return Json(new
                {
                    transferProcess.HODComment,
                    transferProcess.HODActted,
                    transferProcess.HODCommentDate
                }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { HODComment = "" }, JsonRequestBehavior.AllowGet);
        }

        // List students inter faculty requests that new HOD has not commented on
        // GET: InterFacultyTransfers/StudentReqestsForNewHOD
        public async Task<ActionResult> StudentReqestsForNewHOD()
        {
            var values = await _db.Levels
                        .AsNoTracking()
                        .Where(lv => lv.LevelName.Equals("100") || lv.LevelName.Equals("200"))
                        .ToListAsync();

            ViewBag.AcceptableLevelId = new SelectList(values, "LevelId", "LevelName");

            return View();
        }

        // GET: InterFacultyTransfers/JSONStudentReqestsForHOD
        public async Task<JsonResult> JSONStudentLeaveReqestsForNewHOD()
        {
            var students = await _db.TransferProcesses
                .Include(t => t.Transfer)
                .Include(t => t.Transfer.CurrentProgramme)
                .AsNoTracking()
                .Where(t => t.NewHODApproval.Equals(null) && !t.HODActted.Equals(null))
                .Select(t => new ListOfStudentsApplyingForInterFacultyViewModel
                {
                    MatriculationNumber = t.Transfer.Student.MatricNo,
                    FullName = t.Transfer.Student.LastName + " " + t.Transfer.Student.FirstName + " " + t.Transfer.Student.MiddleName,
                    FacultyName = t.Transfer.Student.Programme.Department.Faculty.FacultyName,
                    DepartmentName = t.Transfer.Student.Programme.Department.Faculty.FacultyName,
                    DepartmentOptionName = t.Transfer.Student.Programme.ProgrammeName,
                    TransferProcessId = t.TransferId
                })
                .ToListAsync();

            return Json(students, JsonRequestBehavior.AllowGet);
        }

        // GET: /InterFacultyTransfers/JSONReqestDetailsForNewHOD/
        public async Task<JsonResult> JSONReqestDetailsForNewHOD(int id)
        {
            var student = _db.TransferProcesses.Include(i => i.Transfer).Include(i => i.Transfer.Student)
                            .AsNoTracking().Where(x => x.TransferId.Equals(id))
                            .Select(s => s.Transfer.Student).FirstOrDefault();
            var currentSessionName = _query.GetCurrentSemesterName(student.SchoolProgrammeId);

            var request = await _db.TransferProcesses
                .Include(t => t.Transfer)
                .Include(t => t.Transfer.CurrentProgramme)
                .AsNoTracking()
                .Select(t => new InterFacultyTransferApplicationViewModel
                {
                    Student = student,
                    Programme = t.Transfer.CurrentProgramme,
                    SessionName = currentSessionName,
                    TransferId = t.TransferId,
                    //ApplicationDate = t.a.Value.ToString(),
                    GandCRecommendation = t.GandCRecommendation,
                    GandCRecommendationDate = t.GandCRecommendationDate.Value.ToString(),
                    HODComment = t.HODComment,
                    HODCommentDate = t.HODCommentDate.Value.ToString()
                })
                .Where(t => t.TransferId.Equals(id)).FirstOrDefaultAsync();

            return Json(request, JsonRequestBehavior.AllowGet);
        }

        // POST: InterFacultyTransfers/JSONNewHODStudentLeaveCommentForm
        [HttpPost]
        public async Task<JsonResult> JSONNewHODStudentLeaveCommentForm(string TransferProcessId, string EntryRequirementsMet, string AcceptableLevelId, string HODComment, bool NewHODApproval)
        {
            if (ModelState.IsValid)
            {
                var now = DateTime.Now;
                var date = new DateTime(now.Year, now.Month, now.Day);
                var id = Convert.ToInt32(TransferProcessId);
                var transferProcess = await _db.TransferProcesses
                    .Where(tp => tp.TransferId.Equals(id)).FirstOrDefaultAsync();

                if (string.IsNullOrEmpty(EntryRequirementsMet))
                    transferProcess.EntryRequirementsMet = false;
                else
                    transferProcess.EntryRequirementsMet = true;

                transferProcess.AcceptableLevelId = Convert.ToInt32(AcceptableLevelId);
                transferProcess.NewHODComment = HODComment;
                transferProcess.NewHODApproval = NewHODApproval;
                transferProcess.NewHODCommentDate = date;
                _db.SaveChanges();

                return Json(new
                {
                    transferProcess.NewHODComment,
                    transferProcess.NewHODCommentDate
                }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { NewHODComment = "" }, JsonRequestBehavior.AllowGet);
        }

        // List students inter faculty requests that new faculty has not commented on
        // GET: InterFacultyTransfers/StudentReqestsForFaculty
        public ActionResult StudentReqestsForFaculty()
        {
            return View();
        }

        // GET: InterFacultyTransfers/JSONStudentReqestsForHOD
        public async Task<JsonResult> JSONStudentLeaveReqestsForFacultyAsync()
        {
            var students = await _db.TransferProcesses
                .Include(t => t.Transfer)
                .Include(t => t.Transfer.CurrentProgramme)
                .AsNoTracking()
                .Where(t => t.NewFacultyApproval.Equals(null) && !t.NewHODApproval.Equals(null))
                .Select(t => new ListOfStudentsApplyingForInterFacultyViewModel
                {
                    MatriculationNumber = t.Transfer.Student.MatricNo,
                    FullName = t.Transfer.Student.LastName + " " + t.Transfer.Student.FirstName + " " + t.Transfer.Student.MiddleName,
                    FacultyName = t.Transfer.Student.Programme.Department.Faculty.FacultyName,
                    DepartmentName = t.Transfer.Student.Programme.Department.Faculty.FacultyName,
                    DepartmentOptionName = t.Transfer.Student.Programme.ProgrammeName,
                    NewFacultyName = t.Transfer.CurrentProgramme.Department.Faculty.FacultyName,
                    NewDepartmentName = t.Transfer.CurrentProgramme.Department.DeptName,
                    NewDepartmentOptionName = t.Transfer.CurrentProgramme.ProgrammeName,
                    TransferProcessId = t.TransferId
                })
                .ToListAsync();

            return Json(students, JsonRequestBehavior.AllowGet);
        }

        // GET: /InterFacultyTransfers/JSONReqestDetailsForNewFaculty/
        public async Task<JsonResult> JSONReqestDetailsForNewFacultyAsync(int id)
        {
            //var currentSessionName = "2017/2018";

            //var request = await _db.TransferProcesses
            //    .Include(t => t.Transfer)
            //    .Include(t => t.Transfer.DepartmentOption)
            //    .AsNoTracking()
            //    .Select(t => new InterFacultyTransferApplicationViewModel
            //    {
            //        StudentId = t.Transfer.StudentId,
            //        PresentFacultyName = t.Transfer.Student.DepartmentOption.Department.Faculty.FacultyName,
            //        PresentDepartmentName = t.Transfer.Student.DepartmentOption.Department.DeptName,
            //        PresentDepartmentOptionName = t.Transfer.Student.DepartmentOption.DeptOptionName,
            //        FullName = t.Transfer.Student.LastName + " " + t.Transfer.Student.FirstName + " " + t.Transfer.Student.MiddleName,
            //        Gender = t.Transfer.Student.Gender,
            //        MatricNumber = t.Transfer.Student.MatricNumber,
            //        SelectedFacultyName = t.DepartmentOption.Department.Faculty.FacultyName,
            //        SelectedDepartmentName = t.DepartmentOption.Department.DeptName,
            //        SelectedDepartmentOptionName = t.DepartmentOption.DeptOptionName,
            //        SessionName = currentSessionName,
            //        TransferProcessId = t.TransferProcessId,
            //        ApplicationDate = t.ApplicationDate.Value.ToString(),
            //        GandCRecommendation = t.GandCRecommendation,
            //        GandCRecommendationDate = t.GandCRecommendationDate.Value.ToString(),
            //        HODComment = t.HODComment,
            //        HODCommentDate = t.HODCommentDate.Value.ToString(),
            //        EntryRequirementsMet = t.EntryRequirementsMet,
            //        AcceptableLevelId = t.AcceptableLevelId,
            //        NewHODComment = t.NewHODComment,
            //        NewHODCommentDate = t.NewHODCommentDate.Value.ToString()
            //    })
            //    .Where(t => t.TransferProcessId.Equals(id)).FirstOrDefaultAsync();
            var student = _db.TransferProcesses.Include(i => i.Transfer).Include(i => i.Transfer.Student)
                           .AsNoTracking().Where(x => x.TransferId.Equals(id))
                           .Select(s => s.Transfer.Student).FirstOrDefault();
            var currentSessionName = _query.GetCurrentSemesterName(student.SchoolProgrammeId);

            var request = await _db.TransferProcesses
                .Include(t => t.Transfer)
                .Include(t => t.Transfer.CurrentProgramme)
                .AsNoTracking()
                .Select(t => new InterFacultyTransferApplicationViewModel
                {
                    Student = student,
                    Programme = t.Transfer.CurrentProgramme,
                    SessionName = currentSessionName,
                    TransferId = t.TransferId,
                    //ApplicationDate = t.a.Value.ToString(),
                    GandCRecommendation = t.GandCRecommendation,
                    GandCRecommendationDate = t.GandCRecommendationDate.Value.ToString(),
                    HODComment = t.HODComment,
                    HODCommentDate = t.HODCommentDate.Value.ToString(),
                    EntryRequirementsMet = t.EntryRequirementsMet,
                    AcceptableLevelId = t.AcceptableLevelId,
                    NewHODComment = t.NewHODComment,
                    NewHODCommentDate = t.NewHODCommentDate.Value.ToString()
                })
                .Where(t => t.TransferId.Equals(id)).FirstOrDefaultAsync();


            return Json(request, JsonRequestBehavior.AllowGet);
        }

        // POST: InterFacultyTransfers/JSONNewFacultyCommentForm
        [HttpPost]
        public async Task<JsonResult> JSONNewFacultyCommentForm(string TransferProcessId, string NewFacultyComment, bool NewFacultyApproval)
        {
            if (ModelState.IsValid)
            {
                var now = DateTime.Now;
                var date = new DateTime(now.Year, now.Month, now.Day);
                var id = Convert.ToInt32(TransferProcessId);
                var transferProcess = await _db.TransferProcesses
                    .Where(tp => tp.TransferId.Equals(id)).FirstOrDefaultAsync();
                transferProcess.NewFacultyComment = NewFacultyComment;
                transferProcess.NewFacultyApproval = NewFacultyApproval;
                transferProcess.NewFacultyCommentDate = date;
                _db.SaveChanges();

                return Json(new
                {
                    transferProcess.NewFacultyComment,
                    transferProcess.NewFacultyCommentDate
                }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { NewFacultyComment = "" }, JsonRequestBehavior.AllowGet);
        }

        // List students inter faculty requests that senate can approve
        // GET: InterFacultyTransfers/StudentReqestsForSenate
        public ActionResult StudentReqestsForSenate()
        {
            return View();
        }

        // GET: InterFacultyTransfers/JSONStudentReqestsForSenate
        public async Task<JsonResult> JSONStudentReqestsForSenate()
        {
            var students = await _db.TransferProcesses
                .Include(t => t.Transfer)
                .Include(t => t.Transfer.CurrentProgramme)
                .AsNoTracking()
                .Where(t => t.SenateApproval.Equals(null) && !t.NewFacultyApproval.Equals(null))
                .Select(t => new ListOfStudentsApplyingForInterFacultyViewModel
                {
                    MatriculationNumber = t.Transfer.Student.MatricNo,
                    FullName = t.Transfer.Student.LastName + " " + t.Transfer.Student.FirstName + " " + t.Transfer.Student.MiddleName,
                    FacultyName = t.Transfer.Student.Programme.Department.Faculty.FacultyName,
                    DepartmentName = t.Transfer.Student.Programme.Department.DeptName,
                    DepartmentOptionName = t.Transfer.Student.Programme.ProgrammeName,
                    NewFacultyName = t.Transfer.CurrentProgramme.Department.Faculty.FacultyName,
                    NewDepartmentName = t.Transfer.CurrentProgramme.Department.DeptName,
                    NewDepartmentOptionName = t.Transfer.CurrentProgramme.ProgrammeName,
                    TransferProcessId = t.TransferId
                })
                .ToListAsync();

            return Json(students, JsonRequestBehavior.AllowGet);
        }


        // GET: InterFacultyTransfers/JSONSingleStudentReqestsForSenate
        public async Task<JsonResult> JSONSingleStudentReqestsForSenate(int id)
        {
            var studentSchoolProgrammeId = _db.TransferProcesses.Include(i => i.Transfer).Include(i => i.Transfer.Student)
                                .AsNoTracking().Where(x => x.TransferId.Equals(id))
                                .Select(s => s.Transfer.Student.SchoolProgrammeId).FirstOrDefault();
            var currentSessionName = _query.GetCurrentSemesterName(studentSchoolProgrammeId);

            var transferRequest = await _db.TransferProcesses
                .Include(t => t.Transfer)
                .Include(t => t.Transfer.CurrentProgramme)
                .Include(t => t.Transfer.Student)
                .AsNoTracking()
                .Select(t => new InterFacultyTransferApplicationViewModel
                {
                    Student = t.Transfer.Student,
                    SessionName = currentSessionName,
                    TransferId = t.TransferId,
                    Programme = t.Transfer.CurrentProgramme,
                    // ApplicationDate = t.ApplicationDate.Value.ToString(),
                    GandCRecommendation = t.GandCRecommendation,
                    GandCRecommendationDate = t.GandCRecommendationDate.Value.ToString(),
                    HODComment = t.HODComment,
                    HODCommentDate = t.HODCommentDate.Value.ToString(),
                    EntryRequirementsMet = t.EntryRequirementsMet,
                    AcceptableLevelId = t.AcceptableLevelId,
                    NewHODComment = t.NewHODComment,
                    NewHODCommentDate = t.NewHODCommentDate.Value.ToString(),
                    NewFacultyComment = t.NewFacultyComment,
                    NewFacultyCommentDate = t.NewFacultyCommentDate.Value.ToString()
                })
                .Where(t => t.TransferId.Equals(id)).FirstOrDefaultAsync();

            return Json(transferRequest, JsonRequestBehavior.AllowGet);
        }

        // POST: InterFacultyTransfers/JSONApproveSingleStudentReqests
        [HttpPost]
        public async Task<JsonResult> JSONApproveSingleStudentReqests(string TransferProcessId, string SenateComment, bool SenateApproval)
        {
            if (ModelState.IsValid)
            {
                var now = DateTime.Now;
                var date = new DateTime(now.Year, now.Month, now.Day);
                var id = Convert.ToInt32(TransferProcessId);
                var transferProcess = await _db.TransferProcesses
                    .Where(tp => tp.TransferId.Equals(id)).FirstOrDefaultAsync();
                transferProcess.SenateComment = SenateComment;
                transferProcess.SenateApproval = SenateApproval;
                transferProcess.SenateApprovalDates = date;
                _db.SaveChanges();

                // change departmentOption of the student here
                // send notification to all officers involved

                return Json(new
                {
                    transferProcess.SenateApproval,
                }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { SenateApproval = false, }, JsonRequestBehavior.AllowGet);
        }
    }
}