using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampus.ViewModels.StudentStatusMgtVm;
using SwiftKampusModel.StudentStatusManagement;
using Unijos.Web.ViewModels.StudentStatus;
using Vereyon.Web;

namespace SwiftKampus.Controllers
{
    [Authorize]
    [Audit(AuditingLevel = 2)]
    public class ExtensionsController : BaseController
    {
        public ExtensionsController(SchoolDbContext db) : base(db)
        {

        }

        // GET: Extensions/ApplicationForm
        public async Task<ActionResult> ApplicationForm()
        {
            var StudentId = _studentQuery.GetStudentId(userId);
            var currentSession = _query.GetCurrentSessionName(studentSchoolProgrammeId);

            // ensure student does not have a pending application
            var ExtensionRequest = await _db.Extensions
                .AsNoTracking()
                .Where(dr =>
                    (dr.StudentId.Equals(StudentId) && dr.IsApplicationPending.Equals(true))
                ).FirstOrDefaultAsync();

            if (ExtensionRequest != null)
            {
                FlashMessage.Info("You cannot apply for  Extension of duration of studies because you have a pending application for extension of duration of studies.");
                return RedirectToAction("Index", "Home");
            }

            var student = _db.Students
                .Include(i => i.Programme)
                .Include(i => i.Programme.Department)
                .Include(i => i.Programme.Department.Faculty)
                .Where(u => u.StudentId.Equals(StudentId)).FirstOrDefault();

            var transfer = new ExtensionApplicationViewModel()
            {
                StudentId = student.StudentId,
                FacultyId = student.Programme.Department.Faculty.FacultyId,
                DepartmentId = student.Programme.Department.DepartmentId,
                ProgrammeId = student.ProgrammeId,
                FullName = $"{student.LastName} {student.MiddleName} {student.FirstName}",
               // Gender = student.Gender,
                MatricNumber = student.MatricNo,
                Faculty = student.Programme.Department.Faculty,
                Department = student.Programme.Department,
                Programme = student.Programme,
                PrimaryEmail = student.PrimaryEmail,
                SecondaryEmail = student.Email,
                PhoneNumber = student.PhoneNumber,
                SessionName = currentSession,
               // YearOfEntry = student.EnrollmentDate
            };

            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");

            IEnumerable<SelectListItem> values = from PeriodOfExtension pe in Enum.GetValues(typeof(PeriodOfExtension))
                                                 select new SelectListItem
                                                 {
                                                     Text = pe.ToString(),
                                                     Value = pe.ToString()
                                                 };
            ViewBag.PeriodOfExtension = new SelectList(values, "Value", "Text");

            IEnumerable<SelectListItem> extensionReasons = from ExtensionReason er in Enum.GetValues(typeof(ExtensionReason))
                                                           select new SelectListItem
                                                           {
                                                               Text = er.ToString(),
                                                               Value = er.ToString()
                                                           };
            ViewBag.ExtensionReason = new SelectList(extensionReasons, "Value", "Text");

            return View("ApplicationForm", transfer);
        }

        // POST: Extensions/JSONApplicationForm
        [HttpPost]
        public JsonResult JSONApplicationForm()
        {
            //var StudentId = await _db.Users.Where(st => st.Id.Equals(userId)).Select(st => st.StudentId).FirstOrDefaultAsync();
            var StudentId = _studentQuery.GetStudentId(userId);

            var SessionId = Convert.ToInt32(Request.Params["SessionId"].Trim());

            var now = DateTime.Now;
            var date = new DateTime(now.Year, now.Month, now.Day);
            var application = new Extension
            {
                StudentId = StudentId,
                PeriodOfExtensionSought = Request.Params["PeriodOfExtensionSought"],
                ReasonForExtension = Request.Params["ReasonForExtension"],
                ApplicationDate = date,
                SessionId = SessionId,
                IsApplicationPending = true
            };
            //{
            //    StudentId = StudentId,
            //    SessionId = Convert.ToInt32(Request.Params["SessionId"]),
            //    PeriodOfExtensionSought = Request.Params["PeriodOfExtensionSought"],
            //    ReasonForExtension = Request.Params["ReasonForExtension"],
            //    ApplicationDate = date,
            //    IsApplicationPending = true
            //};
            _db.Extensions.Add(application);
            _db.SaveChanges();

            // Checking no of files injected in Request object  

            if (Request.Files.Count > 0)
            {
                try
                {
                    //  Get all files from Request object  
                    HttpFileCollectionBase files = Request.Files;
                    for (int i = 0; i < files.Count; i++)
                    {
                        //string path = AppDomain.CurrentDomain.BaseDirectory + "/Content/Uploads/";  
                        //string filename = Path.GetFileName(Request.Files[i].FileName);  

                        HttpPostedFileBase file = files[i];
                        string originalFilename;

                        // Checking for Internet Explorer  
                        if (Request.Browser.Browser.ToUpper() == "IE" || Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER")
                        {
                            string[] testfiles = file.FileName.Split(new char[] { '\\' });
                            originalFilename = testfiles[testfiles.Length - 1];
                        }
                        else
                        {
                            originalFilename = Path.GetFileName(file.FileName);
                        }

                        string fileId = StudentId + "_" + Guid.NewGuid().ToString().Replace("-", "");
                        string fileName = fileId + ".pdf";

                        var path = Path.Combine(Server.MapPath("~/Content/ExtensionDocuments/"), fileName);
                        file.SaveAs(path);

                        var document = new ExtensionDocument
                        {
                            ExtensionId = application.ExtensionId,
                            DocumentName = fileId,
                            DocumentExtension = "pdf",
                            DocumentPath = path
                        };
                        //document.OriginalName = originalFilename;
                        _db.ExtensionDocuments.Add(document);
                        _db.SaveChanges();
                    }
                    // Returns message that successfully uploaded  
                    return Json("File Uploaded Successfully!");
                }
                catch (Exception ex)
                {
                    return Json("Error occurred. Error details: " + ex.Message);
                }
            }
            else
            {
                return Json("No files selected.");
            }
        }

        // GET: Extensions/ExtensionHODList
        public ActionResult ExtensionHODList()
        {
            return View();
        }

        // GET: Extensions/JSONStudentExtensionReqestsForHOD
        public async Task<JsonResult> JSONStudentExtensionReqestsForHOD()
        {
            var request = await _db.Extensions
                            .Include(dr => dr.Student)
                            .Include(dr => dr.Session)
                            .AsNoTracking()
                            .Where(dr => dr.HoDApproval.Equals(null))
                            .Select(dr => new ExtensionsViewModel
                            {
                                StudentId = dr.StudentId,
                                ExtensionId = dr.ExtensionId,
                                FullName = dr.Student.LastName + " " + dr.Student.FirstName + " " + dr.Student.MiddleName,
                                MatriculationNumber = dr.Student.MatricNo,
                                FacultyName = dr.Student.Programme.Department.Faculty.FacultyName,
                                DepartmentName = dr.Student.Programme.Department.DeptName,
                                ProgrammeName = dr.Student.Programme.ProgrammeName,
                                SessionId = dr.SessionId,
                                SessionName = dr.Session.SessionName
                            })
                            .ToListAsync();

            return Json(request, JsonRequestBehavior.AllowGet);
        }

        // GET: /Extensions/JSONReqestDetailsForHOD/
        public async Task<JsonResult> JSONReqestDetailsForHOD(int id)
        {
            var request = await _db.Extensions
                            .Include(dr => dr.Student)
                            .Include(dr => dr.Session)
                            .Include(dr => dr.ExtensionDocuments)
                            .AsNoTracking()
                            .Where(dr => dr.ExtensionId.Equals(id))
                            .Select(dr => new ExtensionRequestViewModel
                            {
                                StudentId = dr.StudentId,
                                ExtensionId = dr.ExtensionId,
                                FullName = dr.Student.LastName + " " + dr.Student.FirstName + " " + dr.Student.MiddleName,
                                MatriculationNumber = dr.Student.MatricNo,
                                FacultyName = dr.Student.Programme.Department.Faculty.FacultyName,
                                DepartmentName = dr.Student.Programme.Department.DeptName,
                                ProgrammeName = dr.Student.Programme.ProgrammeName,
                                SessionId = dr.SessionId,
                                SessionName = dr.Session.SessionName,
                                ReasonForExtension = dr.ReasonForExtension,
                                PeriodOfExtensionSought = dr.PeriodOfExtensionSought,
                                ExtensionDocuments = dr.ExtensionDocuments,
                                ApplicationDate = dr.ApplicationDate.ToShortDateString()
                            })
                            .FirstOrDefaultAsync();

            return Json(request, JsonRequestBehavior.AllowGet);
        }

        // POST: /Extensions/JSONExtensionReqestHODComment/
        [HttpPost]
        public async Task<JsonResult> JSONExtensionReqestHODComment(string ExtensionId, string HODComment, bool HoDApproval)
        {
            if (ModelState.IsValid)
            {
                var now = DateTime.Now;
                var date = new DateTime(now.Year, now.Month, now.Day);
                var id = Convert.ToInt32(ExtensionId);
                var extensionProcess = await _db.Extensions
                    .Where(dp => dp.ExtensionId.Equals(id)).FirstOrDefaultAsync();
                extensionProcess.HODComment = HODComment;
                extensionProcess.HoDApproval = HoDApproval;
                extensionProcess.IsApplicationPending &= HoDApproval != false;
                extensionProcess.HODCommentDate = date;
                _db.SaveChanges();

                return Json(new
                {
                    HODCommentForDeferment = extensionProcess.HODComment,
                    HODCommentDateForDeferment = extensionProcess.HODCommentDate,
                }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { HODComment = "" }, JsonRequestBehavior.AllowGet);
        }

        // GET: Extensions/ExtensionFacultyList
        public ActionResult ExtensionFacultyList()
        {
            return View();
        }

        // GET: Extensions/JSONStudentExtensionReqestsForFaculty
        public async Task<JsonResult> JSONStudentExtensionReqestsForFaculty()
        {
            var request = await _db.Extensions
                            .Include(dr => dr.Student)
                            .Include(dr => dr.Session)
                            .AsNoTracking()
                            .Where(dr => dr.HoDApproval != false && dr.FacultyApproval.Equals(null))
                            .Select(dr => new ExtensionsViewModel
                            {
                                StudentId = dr.StudentId,
                                ExtensionId = dr.ExtensionId,
                                FullName = dr.Student.LastName + " " + dr.Student.FirstName + " " + dr.Student.MiddleName,
                                MatriculationNumber = dr.Student.MatricNo,
                                FacultyName = dr.Student.Programme.Department.Faculty.FacultyName,
                                DepartmentName = dr.Student.Programme.Department.DeptName,
                                ProgrammeName = dr.Student.Programme.ProgrammeName,
                                SessionId = dr.SessionId,
                                SessionName = dr.Session.SessionName
                            })
                            .ToListAsync();

            return Json(request, JsonRequestBehavior.AllowGet);
        }

        // GET: /Extensions/JSONReqestDetailsForFaculty/
        public async Task<JsonResult> JSONReqestDetailsForFaculty(int id)
        {
            var request = await _db.Extensions
                            .Include(dr => dr.Student)
                            .Include(dr => dr.Session)
                            .AsNoTracking()
                            .Where(dr => dr.ExtensionId.Equals(id))
                            .Select(dr => new ExtensionRequestViewModel
                            {
                                StudentId = dr.StudentId,
                                ExtensionId = dr.ExtensionId,
                                FullName = dr.Student.LastName + " " + dr.Student.FirstName + " " + dr.Student.MiddleName,
                                MatriculationNumber = dr.Student.MatricNo,
                                FacultyName = dr.Student.Programme.Department.Faculty.FacultyName,
                                DepartmentName = dr.Student.Programme.Department.DeptName,
                                ProgrammeName = dr.Student.Programme.ProgrammeName,
                                SessionId = dr.SessionId,
                                SessionName = dr.Session.SessionName,
                                PeriodOfExtensionSought = dr.PeriodOfExtensionSought,
                                ReasonForExtension = dr.ReasonForExtension,
                                ExtensionDocuments = dr.ExtensionDocuments,
                                ApplicationDate = dr.ApplicationDate.ToShortDateString(),
                                HODComment = dr.HODComment,
                                HODCommentDate = dr.HODCommentDate.Value.ToString()
                            })
                            .FirstOrDefaultAsync();

            return Json(request, JsonRequestBehavior.AllowGet);
        }

        // POST: /Extensions/JSONExtensionReqestFacultyComment/
        [HttpPost]
        public async Task<JsonResult> JSONExtensionReqestFacultyComment(string ExtensionId, string FacultyComment, bool FacultyApproval)
        {
            if (ModelState.IsValid)
            {
                var now = DateTime.Now;
                var date = new DateTime(now.Year, now.Month, now.Day);
                var id = Convert.ToInt32(ExtensionId);
                var extensionProcess = await _db.Extensions
                    .Where(dp => dp.ExtensionId.Equals(id)).FirstOrDefaultAsync();
                extensionProcess.FacultyComment = FacultyComment;
                extensionProcess.FacultyApproval = FacultyApproval;
                extensionProcess.IsApplicationPending &= FacultyApproval != false;
                extensionProcess.FacultyCommentDate = date;
                _db.SaveChanges();

                return Json(new
                {
                    extensionProcess.FacultyComment,
                    extensionProcess.FacultyCommentDate,
                }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { FacultyComment = "" }, JsonRequestBehavior.AllowGet);
        }

        // GET: Extensions/ExtensionSenateList
        public ActionResult ExtensionSenateList()
        {
            return View();
        }

        // GET: Extensions/JSONStudentExtensionReqestsForSenate
        public async Task<JsonResult> JSONStudentExtensionReqestsForSenate()
        {
            var request = await _db.Extensions
                            .Include(dr => dr.Student)
                            .Include(dr => dr.Session)
                            .AsNoTracking()
                            .Where(dr => !dr.FacultyApproval.Equals(null) && dr.SenateApproval.Equals(null))
                            .Select(dr => new ExtensionsViewModel
                            {
                                StudentId = dr.StudentId,
                                ExtensionId = dr.ExtensionId,
                                FullName = dr.Student.LastName + " " + dr.Student.FirstName + " " + dr.Student.MiddleName,
                                MatriculationNumber = dr.Student.MatricNo,
                                FacultyName = dr.Student.Programme.Department.Faculty.FacultyName,
                                DepartmentName = dr.Student.Programme.Department.DeptName,
                                ProgrammeName = dr.Student.Programme.ProgrammeName,
                                SessionId = dr.SessionId,
                                SessionName = dr.Session.SessionName
                            })
                            .ToListAsync();

            return Json(request, JsonRequestBehavior.AllowGet);
        }

        // GET: /Extensions/JSONReqestDetailsForSenate/
        public async Task<JsonResult> JSONReqestDetailsForSenate(int id)
        {
            var request = await _db.Extensions
                            .Include(dr => dr.Student)
                            .Include(dr => dr.Session)
                            .AsNoTracking()
                            .Where(dr => dr.ExtensionId.Equals(id))
                            .Select(dr => new ExtensionRequestViewModel
                            {
                                StudentId = dr.StudentId,
                                ExtensionId = dr.ExtensionId,
                                FullName = dr.Student.LastName + " " + dr.Student.FirstName + " " + dr.Student.MiddleName,
                                MatriculationNumber = dr.Student.MatricNo,
                                FacultyName = dr.Student.Programme.Department.Faculty.FacultyName,
                                DepartmentName = dr.Student.Programme.Department.DeptName,
                                ProgrammeName = dr.Student.Programme.ProgrammeName,
                                SessionId = dr.SessionId,
                                SessionName = dr.Session.SessionName,
                                PeriodOfExtensionSought = dr.PeriodOfExtensionSought,
                                ReasonForExtension = dr.ReasonForExtension,
                                ExtensionDocuments = dr.ExtensionDocuments,
                                ApplicationDate = dr.ApplicationDate.ToShortDateString(),
                                HODComment = dr.HODComment,
                                HODCommentDate = dr.HODCommentDate.Value.ToString(),
                                FacultyComment = dr.FacultyComment,
                                FacultyCommentDate = dr.FacultyCommentDate.Value.ToString()
                            })
                            .FirstOrDefaultAsync();

            return Json(request, JsonRequestBehavior.AllowGet);
        }

        // POST: /Extensions/JSONExtensionReqestSenateApproval/
        [HttpPost]
        public async Task<JsonResult> JSONExtensionReqestSenateApproval(string ExtensionId, string SenateComment, bool SenateApproval)
        {
            if (ModelState.IsValid)
            {
                var now = DateTime.Now;
                var date = new DateTime(now.Year, now.Month, now.Day);
                var id = Convert.ToInt32(ExtensionId);
                var extensionProcess = await _db.Extensions
                    .Where(dp => dp.ExtensionId.Equals(id)).FirstOrDefaultAsync();
                extensionProcess.SenateComment = SenateComment;
                extensionProcess.SenateApproval = SenateApproval;
                extensionProcess.IsApplicationPending = false;
                extensionProcess.SenateApprovalDate = date;
                _db.SaveChanges();

                return Json(new
                {
                    extensionProcess.SenateComment,
                    extensionProcess.SenateApproval,
                    extensionProcess.SenateApprovalDate,
                }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { SenateApproval = false }, JsonRequestBehavior.AllowGet);
        }

        [Route("Extensions/DisplayDocument/{fileName}")]
        // GET: /Extensions/DisplayDocument/fileName
        public ActionResult DisplayDocument(string fileName)
        {
            string filePath = "~/Content/ExtensionDocuments/" + fileName + ".pdf";
            Response.AddHeader("Content-Disposition", "inline; filename=" + fileName);

            return File(filePath, "application/pdf");
        }
    }
}