using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampusModel;
using SwiftKampusModel.StudentStatusManagement;
using Unijos.Web.ViewModels.StudentStatus;
using Vereyon.Web;

namespace SwiftKampus.Controllers
{
    [Authorize]
    [Audit(AuditingLevel = 2)]
    public class DefermentReabsorptionsController : BaseController
    {
        public DefermentReabsorptionsController(SchoolDbContext db) : base(db)
        {

        }

        // GET: ChangeOfCoursePayments
        public async Task<ActionResult> Index()
        {
            var StudentId = _studentQuery.GetStudentId(userId);
            var DefermentReabsorptions = await _db.ChangeOfCoursePayments.Include(c => c.Session).Include(i => i.Student).AsNoTracking()
                .Include(i => i.Student.Programme).Where(x => x.Student.Email.Trim().ToUpper().Equals(userId.Trim().ToUpper()) && x.ChangeOfCourseType.Equals(ChangeOfCourseType.Deferment.ToString())).ToListAsync();


            //var changeOfCoursePayments = await _db.ChangeOfCoursePayments.Include(c => c.Session).Include(a => a.Student).AsNoTracking()
            //                            .Where(x => x.Student.PrimaryEmail.Trim().ToUpper().Equals(userId.Trim().ToUpper())
            //                            && x.SessionId.Equals(sessionId)
            //                            //&& x.ChangeOfCourseType.Equals(ChangeOfCourseType)
            //                            && x.IsPayed.Equals(true)).ToListAsync();
            return View(DefermentReabsorptions);
        }

        // GET:  
        public async Task<ActionResult> ApplicationForm(int ChangeOfCoursePaymentId)
        {
            var paymentCheck = _db.ChangeOfCoursePayments.Where(x => x.ChangeOfCoursePaymentId.Equals(ChangeOfCoursePaymentId) && x.IsPayed.Equals(true));
            if (paymentCheck == null)
            {
                return RedirectToAction("Index");
            }
            var StudentId = _studentQuery.GetStudentId(userId);
            var sessions = _db.Sessions.ToList();

            // ensure student does not have a pending application
            var defermentRequest = await _db.Deferments.Include(d => d.Session)
                .AsNoTracking()
                .Where(dr =>
                    (dr.StudentId.Equals(StudentId) && dr.IsApplicationPending.Equals(true))
                ).FirstOrDefaultAsync();

            // return Content("" + defermentRequest.Count());
            DefermentReabsorptionApplicationViewModel transfer = new DefermentReabsorptionApplicationViewModel();
            if (defermentRequest != null)
            {
                FlashMessage.Info("You cannot apply for deferment because you have a pending deferment application.");
                //return RedirectToAction("Index", "DefermentReabsorptions");
                 transfer = new DefermentReabsorptionApplicationViewModel()
                {
                    Student = _db.Students.Include(i => i.Programme).Include(i => i.Programme.Department)
                            .Include(i => i.Programme.Department.Faculty)
                            .Where(s => s.StudentId.Equals(StudentId)).FirstOrDefault(),
                    SessionName = sessions.Where(s=>s.SessionId.Equals(defermentRequest.StartSessionId)).Select(s=>s.SessionName).FirstOrDefault(),
                    ChangeOfCoursePaymentId = ChangeOfCoursePaymentId,
                    ReasonForDeferment = defermentRequest?.ReasonForDeferment ?? "",
                    
                    
                 };
                ViewBag.StartSession = new SelectList(_db.Sessions.AsNoTracking(), "SessionId", "SessionName", defermentRequest.StartSessionId);
                ViewBag.Sessions = new SelectList(_db.Sessions.AsNoTracking(), "SessionId", "SessionName");
                ViewBag.EndSession = ViewBag.Sessions;
            }
            else
            {
                 transfer = new DefermentReabsorptionApplicationViewModel()
                {
                    Student = _db.Students.Include(i => i.Programme).Include(i => i.Programme.Department)
                            .Include(i => i.Programme.Department.Faculty)
                            .Where(s => s.StudentId.Equals(StudentId)).FirstOrDefault(),
                    SessionName = _query.GetCurrentSessionName(studentSchoolProgrammeId),
                    ChangeOfCoursePaymentId = ChangeOfCoursePaymentId
                };

                ViewBag.StartSessionId = new SelectList(_db.Sessions, "SessionId", "SessionName");
                ViewBag.EndSessionId = ViewBag.StartSessionId;
            }


            return View("ApplicationForm", transfer);
        }

        // POST: DefermentReabsorption/JSONDefermentApplicationForm
        [HttpPost]
        public JsonResult JSONDefermentApplicationForm()
        {
            var now = DateTime.Now;
            var date = new DateTime(now.Year, now.Month, now.Day);

            var StudentId =  _studentQuery.GetStudentId(userId);
            int paymentId = Convert.ToInt32(Request.Params["ChangeOfCoursePaymentId"]);

            // ensure student does not have a pending application
            var defermentRequest =  _db.Deferments.Include(d => d.Session).Include(d=>d.Student)
                .Where(dr =>
                    dr.ChangeOfCoursePaymentId.Equals(paymentId) && dr.IsApplicationPending.Equals(true)
                ).FirstOrDefault();

            if (defermentRequest != null)
            {
                defermentRequest.StudentId = StudentId;
                defermentRequest.StartSessionId = Request.Params["StartSessionId"];
                defermentRequest.EndSessionId = Request.Params["EndSessionId"];
                defermentRequest.ReasonForDeferment = Request.Params["ReasonForDeferment"];
                defermentRequest.ApplicationDateForDerferment = date;
                defermentRequest.IsApplicationPending = true;
                defermentRequest.ChangeOfCoursePaymentId = Convert.ToInt32(Request.Params["ChangeOfCoursePaymentId"]);

                _db.Entry(defermentRequest).State = EntityState.Modified;

                if (Request.Files.Count > 0)
                {
                    try
                    {
                        //  Get all files from Request object  
                        HttpFileCollectionBase files = Request.Files;
                        for (int i = 0; i < files.Count; i++)
                        {
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

                            var path = Path.Combine(Server.MapPath("~/Content/DefermentDocuments/"), fileName);
                            file.SaveAs(path);

                            var document = new DefermentDocument
                            {
                                DefermentId = defermentRequest.DefermentId,
                                DocumentName = fileId,
                                DocumentPath = path,
                                DocumentExtension = "pdf"
                            };
                            //document.OriginalName = originalFilename;
                            _db.DefermentDocuments.Add(document);
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
            else
            {
                var application = new Deferment
                {
                    StudentId = StudentId,
                    StartSessionId = Request.Params["StartSessionId"],
                    EndSessionId = Request.Params["EndSessionId"],
                    ReasonForDeferment = Request.Params["ReasonForDeferment"],
                    ApplicationDateForDerferment = date,
                    IsApplicationPending = true,
                    ChangeOfCoursePaymentId = Convert.ToInt32(Request.Params["ChangeOfCoursePaymentId"])
                };
                _db.Deferments.Add(application);

                if (Request.Files.Count > 0)
                {
                    try
                    {
                        //  Get all files from Request object  
                        HttpFileCollectionBase files = Request.Files;
                        for (int i = 0; i < files.Count; i++)
                        {
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

                            var path = Path.Combine(Server.MapPath("~/Content/DefermentDocuments/"), fileName);
                            file.SaveAs(path);

                            var document = new DefermentDocument
                            {
                                DefermentId = application.DefermentId,
                                DocumentName = fileId,
                                DocumentPath = path,
                                DocumentExtension = "pdf"
                            };
                            //document.OriginalName = originalFilename;
                            _db.DefermentDocuments.Add(document);
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
            
            
            _db.SaveChanges();

            // Checking no of files injected in Request object  

            //if (Request.Files.Count > 0)
            //{
            //    try
            //    {
            //        //  Get all files from Request object  
            //        HttpFileCollectionBase files = Request.Files;
            //        for (int i = 0; i < files.Count; i++)
            //        {
            //            HttpPostedFileBase file = files[i];
            //            string originalFilename;

            //            // Checking for Internet Explorer  
            //            if (Request.Browser.Browser.ToUpper() == "IE" || Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER")
            //            {
            //                string[] testfiles = file.FileName.Split(new char[] { '\\' });
            //                originalFilename = testfiles[testfiles.Length - 1];
            //            }
            //            else
            //            {
            //                originalFilename = Path.GetFileName(file.FileName);
            //            }

            //            string fileId = StudentId + "_" + Guid.NewGuid().ToString().Replace("-", "");
            //            string fileName = fileId + ".pdf";

            //            var path = Path.Combine(Server.MapPath("~/Content/DefermentDocuments/"), fileName);
            //            file.SaveAs(path);

            //            var document = new DefermentDocument
            //            {
            //                DefermentId = application.DefermentId,
            //                DocumentName = fileId,
            //                DocumentPath = path,
            //                DocumentExtension = "pdf"
            //            };
            //            //document.OriginalName = originalFilename;
            //            _db.DefermentDocuments.Add(document);
            //            _db.SaveChanges();
            //        }
            //        // Returns message that successfully uploaded  
            //        return Json("File Uploaded Successfully!");
            //    }
            //    catch (Exception ex)
            //    {
            //        return Json("Error occurred. Error details: " + ex.Message);
            //    }
            //}
            //else
            //{
            //    return Json("No files selected.");
            //}
        }

        // GET: DefermentReabsorption/DefermentHODList
        public ActionResult DefermentHODList()
        {
            return View();
        }

        // GET: DefermentReabsorption/JSONStudentDefermentReqestsForHOD
        public async Task<JsonResult> JSONStudentDefermentReqestsForHOD()
        {
            var request = await _db.Deferments
                            .Include(dr => dr.Student)
                            .AsNoTracking()
                            .Where(dr => dr.HODApprovalForDeferment.Equals(null))
                            .Select(dr => new ListDefermentRequestsViewModel
                            {
                                StudentId = dr.StudentId,
                                DefermentId = dr.DefermentId,
                                FullName = dr.Student.LastName + " " + dr.Student.FirstName + " " + dr.Student.MiddleName,
                                MatriculationNumber = dr.Student.MatricNo,
                                FacultyName = dr.Student.Programme.Department.Faculty.FacultyName,
                                DepartmentName = dr.Student.Programme.Department.DeptName,
                                DepartmentOptionName = dr.Student.Programme.ProgrammeName,
                                StartSessionId = dr.StartSessionId,
                                EndSessionId = dr.EndSessionId
                            })
                            .ToListAsync();

            return Json(request, JsonRequestBehavior.AllowGet);
        }

        // GET: /DefermentReabsorption/JSONReqestDetailsForHOD/
        public async Task<JsonResult> JSONReqestDetailsForHOD(int id)
        {
            var request = await _db.Deferments
                            .Include(dr => dr.Student)
                            .AsNoTracking()
                            .Where(dr => dr.DefermentId.Equals(id))
                            .Select(dr => new DefermentRequestViewModel
                            {
                                StudentId = dr.StudentId,
                                DefermentReabsorptionId = dr.DefermentId,
                                FullName = dr.Student.LastName + " " + dr.Student.FirstName + " " + dr.Student.MiddleName,
                                MatriculationNumber = dr.Student.MatricNo,
                                FacultyName = dr.Student.Programme.Department.Faculty.FacultyName,
                                DepartmentName = dr.Student.Programme.Department.DeptName,
                                DepartmentOptionName = dr.Student.Programme.ProgrammeName,
                                StartSessionId = dr.StartSessionId,
                                EndSessionId = dr.EndSessionId,
                                ReasonForDeferment = dr.ReasonForDeferment,
                                DefermentDocuments = dr.DefermentDocuments,
                                ApplicationDateForDerferment = dr.ApplicationDateForDerferment.Value.ToString()
                            })
                            .FirstOrDefaultAsync();

            return Json(request, JsonRequestBehavior.AllowGet);
        }

        // POST: /DefermentReabsorption/JSONDefermentReqestHODComment/
        [HttpPost]
        public async Task<JsonResult> JSONDefermentReqestHODComment(string DefermentReabsorptionId, string HODComment, bool HODApprovalForDeferment)
        {
            if (ModelState.IsValid)
            {
                var now = DateTime.Now;
                var date = new DateTime(now.Year, now.Month, now.Day);
                var id = Convert.ToInt32(DefermentReabsorptionId);
                var defermentProcess = await _db.Deferments
                    .Where(dp => dp.DefermentId.Equals(id)).FirstOrDefaultAsync();
                defermentProcess.HODCommentForDeferment = HODComment;
                defermentProcess.HODApprovalForDeferment = HODApprovalForDeferment;
                defermentProcess.IsApplicationPending &= HODApprovalForDeferment != false;
                defermentProcess.HODCommentDateForDeferment = date;
                _db.SaveChanges();

                return Json(new
                {
                    defermentProcess.HODCommentForDeferment,
                    defermentProcess.HODCommentDateForDeferment,
                }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { HODCommentForDeferment = "" }, JsonRequestBehavior.AllowGet);
        }

        // GET: DefermentReabsorption/DefermentFacultyList
        public ActionResult DefermentFacultyList()
        {
            return View();
        }

        // GET: DefermentReabsorption/JSONStudentDefermentReqestsForFaculty
        public async Task<JsonResult> JSONStudentDefermentReqestsForFaculty()
        {
            var request = await _db.Deferments
                            .Include(dr => dr.Student)
                            .AsNoTracking()
                            .Where(dr => (dr.HODApprovalForDeferment == true) && dr.FacultyApprovalForDerferment.Equals(null))
                            .Select(dr => new ListDefermentRequestsViewModel
                            {
                                StudentId = dr.StudentId,
                                DefermentId = dr.DefermentId,
                                FullName = dr.Student.LastName + " " + dr.Student.FirstName + " " + dr.Student.MiddleName,
                                MatriculationNumber = dr.Student.MatricNo,
                                FacultyName = dr.Student.Programme.Department.Faculty.FacultyName,
                                DepartmentName = dr.Student.Programme.Department.DeptName,
                                DepartmentOptionName = dr.Student.Programme.ProgrammeName,
                                StartSessionId = dr.StartSessionId,
                                EndSessionId = dr.EndSessionId,
                            })
                            .ToListAsync();

            return Json(request, JsonRequestBehavior.AllowGet);
        }

        // GET: /DefermentReabsorption/JSONReqestDetailsForFaculty/
        public async Task<JsonResult> JSONReqestDetailsForFaculty(int id)
        {
            var request = await _db.Deferments
                            .Include(dr => dr.Student)
                            .Include(dr => dr.DefermentDocuments)
                            .AsNoTracking()
                            .Where(dr => dr.DefermentId.Equals(id))
                            .Select(dr => new DefermentRequestViewModel
                            {
                                StudentId = dr.StudentId,
                                DefermentReabsorptionId = dr.DefermentId,
                                FullName = dr.Student.LastName + " " + dr.Student.FirstName + " " + dr.Student.MiddleName,
                                MatriculationNumber = dr.Student.MatricNo,
                                FacultyName = dr.Student.Programme.Department.Faculty.FacultyName,
                                DepartmentName = dr.Student.Programme.Department.DeptName,
                                DepartmentOptionName = dr.Student.Programme.ProgrammeName,
                                StartSessionId = dr.StartSessionId,
                                EndSessionId = dr.EndSessionId,
                                ReasonForDeferment = dr.ReasonForDeferment,
                                DefermentDocuments = dr.DefermentDocuments,
                                ApplicationDateForDerferment = dr.ApplicationDateForDerferment.Value.ToString(),
                                HODCommentForDeferment = dr.HODCommentForDeferment,
                                HODCommentDateForDeferment = dr.HODCommentDateForDeferment.Value.ToString()
                            })
                            .FirstOrDefaultAsync();

            return Json(request, JsonRequestBehavior.AllowGet);
        }

        // POST: /DefermentReabsorption/JSONDefermentReqestFacultyComment/
        [HttpPost]
        public async Task<JsonResult> JSONDefermentReqestFacultyComment(string DefermentReabsorptionId, string FacultyComment, bool FacultyApprovalForDerferment)
        {
            if (ModelState.IsValid)
            {
                var now = DateTime.Now;
                var date = new DateTime(now.Year, now.Month, now.Day);
                var id = Convert.ToInt32(DefermentReabsorptionId);
                var defermentProcess = await _db.Deferments
                    .Where(dp => dp.DefermentId.Equals(id)).FirstOrDefaultAsync();
                defermentProcess.FacultyCommentForDeferment = FacultyComment;
                defermentProcess.FacultyApprovalForDerferment = FacultyApprovalForDerferment;
                defermentProcess.IsApplicationPending &= FacultyApprovalForDerferment != false;
                defermentProcess.FacultyCommentDateForDeferment = date;
                _db.SaveChanges();

                return Json(new
                {
                    defermentProcess.FacultyCommentForDeferment,
                    defermentProcess.FacultyCommentDateForDeferment,
                }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { FacultyCommentForDeferment = "" }, JsonRequestBehavior.AllowGet);
        }

        // GET: DefermentReabsorption/DefermentSenateList
        public ActionResult DefermentSenateList()
        {
            return View();
        }

        // GET: DefermentReabsorption/JSONStudentDefermentReqestsForSenate
        public async Task<JsonResult> JSONStudentDefermentReqestsForSenate()
        {
            var request = await _db.Deferments
                            .Include(dr => dr.Student)
                            .AsNoTracking()
                            .Where(dr => dr.FacultyApprovalForDerferment == true && dr.SenateApprovalForDeferment.Equals(null))
                            .Select(dr => new ListDefermentRequestsViewModel
                            {
                                StudentId = dr.StudentId,
                                DefermentId = dr.DefermentId,
                                FullName = dr.Student.LastName + " " + dr.Student.FirstName + " " + dr.Student.MiddleName,
                                MatriculationNumber = dr.Student.MatricNo,
                                FacultyName = dr.Student.Programme.Department.Faculty.FacultyName,
                                DepartmentName = dr.Student.Programme.Department.DeptName,
                                DepartmentOptionName = dr.Student.Programme.ProgrammeName,
                                StartSessionId = dr.StartSessionId,
                                EndSessionId = dr.EndSessionId
                            })
                            .ToListAsync();

            return Json(request, JsonRequestBehavior.AllowGet);
        }

        // GET: /DefermentReabsorption/JSONReqestDetailsForSenate/
        public async Task<JsonResult> JSONReqestDetailsForSenate(int id)
        {
            var request = await _db.Deferments
                            .Include(dr => dr.Student)
                            .Include(dr => dr.DefermentDocuments)
                            .AsNoTracking()
                            .Where(dr => dr.DefermentId.Equals(id))
                            .Select(dr => new DefermentRequestViewModel
                            {
                                StudentId = dr.StudentId,
                                DefermentReabsorptionId = dr.DefermentId,
                                FullName = dr.Student.LastName + " " + dr.Student.FirstName + " " + dr.Student.MiddleName,
                                MatriculationNumber = dr.Student.MatricNo,
                                FacultyName = dr.Student.Programme.Department.Faculty.FacultyName,
                                DepartmentName = dr.Student.Programme.Department.DeptName,
                                DepartmentOptionName = dr.Student.Programme.ProgrammeName,
                                StartSessionId = dr.StartSessionId,
                                EndSessionId = dr.EndSessionId,
                                ReasonForDeferment = dr.ReasonForDeferment,
                                DefermentDocuments = dr.DefermentDocuments,
                                ApplicationDateForDerferment = dr.ApplicationDateForDerferment.Value.ToString(),
                                HODCommentForDeferment = dr.HODCommentForDeferment,
                                HODCommentDateForDeferment = dr.HODCommentDateForDeferment.Value.ToString(),
                                FacultyCommentForDeferment = dr.FacultyCommentForDeferment,
                                FacultyCommentDateForDeferment = dr.FacultyCommentDateForDeferment.Value.ToString()
                            })
                            .FirstOrDefaultAsync();

            return Json(request, JsonRequestBehavior.AllowGet);
        }

        // POST: /DefermentReabsorption/JSONDefermentReqestSenateApproval/
        [HttpPost]
        public async Task<JsonResult> JSONDefermentReqestSenateApproval(string DefermentReabsorptionId, string SenateComment, bool SenateApprovalForDeferment)
        {
            if (ModelState.IsValid)
            {
                var now = DateTime.Now;
                var date = new DateTime(now.Year, now.Month, now.Day);
                var id = Convert.ToInt32(DefermentReabsorptionId);
                var defermentProcess = await _db.Deferments
                    .Where(dp => dp.DefermentId.Equals(id)).FirstOrDefaultAsync();
                defermentProcess.SenateCommentForDeferment = SenateComment;
                defermentProcess.SenateApprovalForDeferment = SenateApprovalForDeferment;
                defermentProcess.IsApplicationPending &= SenateApprovalForDeferment != false;
                defermentProcess.SenateApprovalDateForDeferment = date;
                _db.SaveChanges();

                return Json(new
                {
                    defermentProcess.SenateCommentForDeferment,
                    defermentProcess.SenateApprovalForDeferment,
                    defermentProcess.SenateApprovalDateForDeferment,
                }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { SenateApprovalForDeferment = false }, JsonRequestBehavior.AllowGet);
        }

        // GET: DefermentReabsorption/ReabsorptionApplicationForm
        public async Task<ActionResult> ReabsorptionApplicationForm()
        {
            var StudentId = _studentQuery.GetStudentId(userId);

            var defermentRequest = await _db.Reabsorptions
                  .AsNoTracking()
                  .Include(d => d.Deferment)
                  .Where(dr => dr.Deferment.StudentId.Equals(StudentId) && dr.Deferment.IsApplicationPending.Equals(false) && dr.IsApplicationPending.Equals(true))
                  .FirstOrDefaultAsync();

            if (defermentRequest == null)
            {
                FlashMessage.Info("You cannot apply for  Re-Absorption because you either have a pending  Re-Absorption application or you have not deferred any session.");
                return RedirectToAction("Index", "Home");
            }

            var transfer = new DefermentReabsorptionApplicationViewModel()
            {

                DefermentReabsorptionId = defermentRequest.DefermentId,
                Student = _db.Students.Include(i => i.Programme).Include(i => i.Programme.Department)
                            .Include(i => i.Programme.Department.Faculty)
                            .Where(s => s.StudentId.Equals(defermentRequest.Deferment.StudentId)).FirstOrDefault(),
                ReasonForDeferment = defermentRequest.Deferment.ReasonForDeferment,
                SessionName = _query.GetCurrentSessionName(studentSchoolProgrammeId)

            };

            ViewBag.EndSessionId = ViewBag.StartSessionId;

            return View("ReabsorptionApplicationForm", transfer);
        }

        // POST: DefermentReabsorption/JSONReabsorptionApplicationForm
        [HttpPost]
        public async Task<JsonResult> JSONReabsorptionApplicationForm()
        {
            var now = DateTime.Now;
            var date = new DateTime(now.Year, now.Month, now.Day);
            var id = Convert.ToInt32(Request.Params["DefermentReabsorptionId"]);
            var application = await _db.Reabsorptions.Where(dr => dr.DefermentId.Equals(id)).FirstOrDefaultAsync();
            //application.rea = Request.Params["ReasonForExtensionDuration"];
            application.ApplicationDateForReabsorption = date;
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

                        string fileId = Guid.NewGuid().ToString().Replace("-", "");
                        string fileName = fileId + ".pdf";

                        var path = Path.Combine(Server.MapPath("~/Content/DefermentDocuments/"), fileName);
                        file.SaveAs(path);

                        var document = new DefermentDocument
                        {
                            DefermentId = application.DefermentId,
                            DocumentName = fileId
                        };
                        _db.DefermentDocuments.Add(document);
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


        // GET: DefermentReabsorption/ReabsorptionHODList
        public ActionResult ReabsorptionHODList()
        {
            return View();
        }

        // GET: DefermentReabsorption/JSONStudentReabsorptionReqestsForHOD
        public async Task<JsonResult> JSONStudentReabsorptionReqestsForHOD()
        {
            var request = await _db.Reabsorptions
                            .Include(dr => dr.Deferment.Student)
                            .AsNoTracking()
                            .Where(dr => dr.HODApprovalForReabsorption.Equals(null)
                            && !dr.ApplicationDateForReabsorption.Equals(null))
                            .Select(dr => new ListDefermentRequestsViewModel
                            {
                                StudentId = dr.Deferment.StudentId,
                                DefermentId = dr.DefermentId,
                                FullName = dr.Deferment.Student.FullName,
                                MatriculationNumber = dr.Deferment.Student.MatricNo,
                                FacultyName = dr.Deferment.Student.Programme.Department.Faculty.FacultyName,
                                DepartmentName = dr.Deferment.Student.Programme.Department.DeptName,
                                DepartmentOptionName = dr.Deferment.Student.Programme.ProgrammeName,
                                StartSessionId = dr.Deferment.StartSessionId,
                                EndSessionId = dr.Deferment.EndSessionId,
                            })
                            .ToListAsync();

            return Json(request, JsonRequestBehavior.AllowGet);
        }

        // GET: /DefermentReabsorption/JSONReabsorptionReqestDetailsForHOD/
        public async Task<JsonResult> JSONReabsorptionReqestDetailsForHOD(int id)
        {
            var request = await _db.Reabsorptions
                            .Include(dr => dr.Deferment.Student)
                            .Include(dr => dr.Deferment.DefermentDocuments)
                            .AsNoTracking()
                            .Where(dr => dr.DefermentId.Equals(id))
                            .Select(dr => new DefermentRequestViewModel
                            {
                                StudentId = dr.Deferment.StudentId,
                                DefermentReabsorptionId = dr.DefermentId,
                                FullName = dr.Deferment.Student.LastName + " " + dr.Deferment.Student.FirstName + " " + dr.Deferment.Student.MiddleName,
                                MatriculationNumber = dr.Deferment.Student.MatricNo,
                                FacultyName = dr.Deferment.Student.Programme.Department.Faculty.FacultyName,
                                DepartmentName = dr.Deferment.Student.Programme.Department.DeptName,
                                DepartmentOptionName = dr.Deferment.Student.Programme.ProgrammeName,
                                StartSessionId = dr.Deferment.StartSessionId,
                                EndSessionId = dr.Deferment.EndSessionId,
                                ReasonForDeferment = dr.Deferment.ReasonForDeferment,
                                DefermentDocuments = dr.Deferment.DefermentDocuments,
                                ApplicationDateForDerferment = dr.Deferment.ApplicationDateForDerferment.Value.ToString(),
                                HODCommentForDeferment = dr.Deferment.HODCommentForDeferment,
                                HODCommentDateForDeferment = dr.Deferment.HODCommentDateForDeferment.Value.ToString(),
                                FacultyCommentForDeferment = dr.Deferment.FacultyCommentForDeferment,
                                FacultyCommentDateForDeferment = dr.Deferment.FacultyCommentDateForDeferment.Value.ToString(),
                                SenateCommentForDeferment = dr.Deferment.SenateCommentForDeferment,
                                SenateApprovalForDeferment = dr.Deferment.SenateApprovalForDeferment,
                                SenateApprovalDateForDeferment = dr.Deferment.SenateApprovalDateForDeferment.Value.ToString(),
                            })
                            .FirstOrDefaultAsync();

            return Json(request, JsonRequestBehavior.AllowGet);
        }

        // POST: /DefermentReabsorption/JSONReabsorptionReqestHODComment/
        [HttpPost]
        public async Task<JsonResult> JSONReabsorptionReqestHODComment(string DefermentReabsorptionId, string HODComment, bool HODApprovalForReabsorption)
        {
            if (ModelState.IsValid)
            {
                var now = DateTime.Now;
                var date = new DateTime(now.Year, now.Month, now.Day);
                var id = Convert.ToInt32(DefermentReabsorptionId);
                var reabsorptionProcess = await _db.Reabsorptions
                    .Where(dp => dp.DefermentId.Equals(id)).FirstOrDefaultAsync();
                reabsorptionProcess.HODCommentForReabsorption = HODComment;
                reabsorptionProcess.HODApprovalForReabsorption = HODApprovalForReabsorption;
                reabsorptionProcess.HODCommentDateForReabsorption = date;
                _db.SaveChanges();

                return Json(new
                {
                    reabsorptionProcess.HODCommentForReabsorption,
                    reabsorptionProcess.HODCommentDateForReabsorption,
                }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { HODCommentForReabsorption = "" }, JsonRequestBehavior.AllowGet);
        }

        // GET: DefermentReabsorption/ReabsorptionFacultyList
        public ActionResult ReabsorptionFacultyList()
        {
            return View();
        }

        // GET: DefermentReabsorption/JSONStudentReabsorptionReqestsForFaculty
        public async Task<JsonResult> JSONStudentReabsorptionReqestsForFaculty()
        {
            var request = await _db.Reabsorptions
                            .Include(dr => dr.Deferment.Student)
                            .AsNoTracking()
                            .Where(dr => dr.FacultyApprovalForReabsorption.Equals(null)
                            && !dr.HODApprovalForReabsorption.Equals(null))
                            .Select(dr => new ListDefermentRequestsViewModel
                            {
                                StudentId = dr.Deferment.StudentId,
                                DefermentId = dr.DefermentId,
                                FullName = dr.Deferment.Student.LastName + " " + dr.Deferment.Student.FirstName + " " + dr.Deferment.Student.MiddleName,
                                MatriculationNumber = dr.Deferment.Student.MatricNo,
                                FacultyName = dr.Deferment.Student.Programme.Department.Faculty.FacultyName,
                                DepartmentName = dr.Deferment.Student.Programme.Department.DeptName,
                                DepartmentOptionName = dr.Deferment.Student.Programme.ProgrammeName,
                                StartSessionId = dr.Deferment.StartSessionId,
                                EndSessionId = dr.Deferment.EndSessionId,
                            })
                            .ToListAsync();

            return Json(request, JsonRequestBehavior.AllowGet);
        }

        // GET: /DefermentReabsorption/JSONReabsorptionReqestDetailsForFaculty/
        public async Task<JsonResult> JSONReabsorptionReqestDetailsForFaculty(int id)
        {
            var request = await _db.Reabsorptions
                            .Include(dr => dr.Deferment.Student)
                            .Include(dr => dr.Deferment.DefermentDocuments)
                            .AsNoTracking()
                            .Where(dr => dr.DefermentId.Equals(id))
                            .Select(dr => new DefermentRequestViewModel
                            {
                                StudentId = dr.Deferment.StudentId,
                                DefermentReabsorptionId = dr.DefermentId,
                                FullName = dr.Deferment.Student.LastName + " " + dr.Deferment.Student.FirstName + " " + dr.Deferment.Student.MiddleName,
                                MatriculationNumber = dr.Deferment.Student.MatricNo,
                                FacultyName = dr.Deferment.Student.Programme.Department.Faculty.FacultyName,
                                DepartmentName = dr.Deferment.Student.Programme.Department.DeptName,
                                DepartmentOptionName = dr.Deferment.Student.Programme.ProgrammeName,
                                StartSessionId = dr.Deferment.StartSessionId,
                                EndSessionId = dr.Deferment.EndSessionId,
                                ReasonForDeferment = dr.Deferment.ReasonForDeferment,
                                DefermentDocuments = dr.Deferment.DefermentDocuments,
                                ApplicationDateForDerferment = dr.Deferment.ApplicationDateForDerferment.Value.ToString(),
                                HODCommentForDeferment = dr.Deferment.HODCommentForDeferment,
                                HODCommentDateForDeferment = dr.Deferment.HODCommentDateForDeferment.Value.ToString(),
                                FacultyCommentForDeferment = dr.Deferment.FacultyCommentForDeferment,
                                FacultyCommentDateForDeferment = dr.Deferment.FacultyCommentDateForDeferment.Value.ToString(),
                                SenateCommentForDeferment = dr.Deferment.SenateCommentForDeferment,
                                SenateApprovalForDeferment = dr.Deferment.SenateApprovalForDeferment,
                                SenateApprovalDateForDeferment = dr.Deferment.SenateApprovalDateForDeferment.Value.ToString(),
                                HODCommentForReabsorption = dr.HODCommentForReabsorption,
                                HODCommentDateForReabsorption = dr.HODCommentDateForReabsorption.Value.ToString()
                            })
                            .FirstOrDefaultAsync();

            return Json(request, JsonRequestBehavior.AllowGet);
        }

        // POST: /DefermentReabsorption/JSONReabsorptionReqestFacultyComment/
        [HttpPost]
        public async Task<JsonResult> JSONReabsorptionReqestFacultyComment(string DefermentReabsorptionId, string FacultyComment, bool FacultyApprovalForReabsorption)
        {
            if (ModelState.IsValid)
            {
                var now = DateTime.Now;
                var date = new DateTime(now.Year, now.Month, now.Day);
                var id = Convert.ToInt32(DefermentReabsorptionId);
                var reabsorptionProcess = await _db.Reabsorptions
                    .Where(dp => dp.DefermentId.Equals(id)).FirstOrDefaultAsync();
                reabsorptionProcess.FacultyCommentForReabsorption = FacultyComment;
                reabsorptionProcess.FacultyApprovalForReabsorption = FacultyApprovalForReabsorption;
                reabsorptionProcess.FacultyCommentDateForReabsorption = date;
                _db.SaveChanges();

                return Json(new
                {
                    reabsorptionProcess.FacultyCommentForReabsorption,
                    reabsorptionProcess.FacultyCommentDateForReabsorption,
                }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { FacultyCommentForReabsorption = "" }, JsonRequestBehavior.AllowGet);
        }

        // GET: DefermentReabsorption/ReabsorptionSenateList
        public ActionResult ReabsorptionSenateList()
        {
            return View();
        }

        // GET: DefermentReabsorption/JSONStudentReabsorptionReqestsForSenate
        public async Task<JsonResult> JSONStudentReabsorptionReqestsForSenate()
        {
            var request = await _db.Reabsorptions
                            .Include(dr => dr.Deferment.Student)
                            .AsNoTracking()
                            .Where(dr => !dr.FacultyApprovalForReabsorption.Equals(null) && dr.SenateApprovalForReabsorption.Equals(null))
                            .Select(dr => new ListDefermentRequestsViewModel
                            {
                                StudentId = dr.Deferment.StudentId,
                                DefermentId = dr.Deferment.DefermentId,
                                FullName = dr.Deferment.Student.LastName + " " + dr.Deferment.Student.FirstName + " " + dr.Deferment.Student.MiddleName,
                                MatriculationNumber = dr.Deferment.Student.MatricNo,
                                FacultyName = dr.Deferment.Student.Programme.Department.Faculty.FacultyName,
                                DepartmentName = dr.Deferment.Student.Programme.Department.DeptName,
                                DepartmentOptionName = dr.Deferment.Student.Programme.ProgrammeName,
                            })
                            .ToListAsync();

            return Json(request, JsonRequestBehavior.AllowGet);
        }

        // GET: /DefermentReabsorption/JSONReabsorptionReqestDetailsForSenate/
        public async Task<JsonResult> JSONReabsorptionReqestDetailsForSenate(int id)
        {
            var request = await _db.Reabsorptions
                            .Include(dr => dr.Deferment.Student)
                            .Include(dr => dr.Deferment.DefermentDocuments)
                            .AsNoTracking()
                            .Where(dr => dr.DefermentId.Equals(id))
                            .Select(dr => new DefermentRequestViewModel
                            {
                                StudentId = dr.Deferment.StudentId,
                                DefermentReabsorptionId = dr.Deferment.DefermentId,
                                FullName = dr.Deferment.Student.LastName + " " + dr.Deferment.Student.FirstName + " " + dr.Deferment.Student.MiddleName,
                                MatriculationNumber = dr.Deferment.Student.MatricNo,
                                FacultyName = dr.Deferment.Student.Programme.Department.Faculty.FacultyName,
                                DepartmentName = dr.Deferment.Student.Programme.Department.DeptName,
                                DepartmentOptionName = dr.Deferment.Student.Programme.ProgrammeName,
                                StartSessionId = dr.Deferment.StartSessionId,
                                EndSessionId = dr.Deferment.EndSessionId,
                                ReasonForDeferment = dr.Deferment.ReasonForDeferment,
                                DefermentDocuments = dr.Deferment.DefermentDocuments,
                                ApplicationDateForDerferment = dr.Deferment.ApplicationDateForDerferment.Value.ToString(),
                                HODCommentForDeferment = dr.Deferment.HODCommentForDeferment,
                                HODCommentDateForDeferment = dr.Deferment.HODCommentDateForDeferment.Value.ToString(),
                                FacultyCommentForDeferment = dr.Deferment.FacultyCommentForDeferment,
                                FacultyCommentDateForDeferment = dr.Deferment.FacultyCommentDateForDeferment.Value.ToString(),
                                SenateCommentForDeferment = dr.Deferment.SenateCommentForDeferment,
                                SenateApprovalForDeferment = dr.Deferment.SenateApprovalForDeferment,
                                SenateApprovalDateForDeferment = dr.Deferment.SenateApprovalDateForDeferment.Value.ToString(),
                                HODCommentForReabsorption = dr.HODCommentForReabsorption,
                                HODCommentDateForReabsorption = dr.HODCommentDateForReabsorption.Value.ToString(),
                                FacultyCommentForReabsorption = dr.FacultyCommentForReabsorption,
                                FacultyCommentDateForReabsorption = dr.FacultyCommentDateForReabsorption.Value.ToString()
                            })
                            .FirstOrDefaultAsync();

            return Json(request, JsonRequestBehavior.AllowGet);
        }

        // POST: /DefermentReabsorption/JSONReabsorptionReqestSenateApproval/
        [HttpPost]
        public async Task<JsonResult> JSONReabsorptionReqestSenateApproval(string DefermentReabsorptionId, string SenateComment, bool SenateApprovalForReabsorption)
        {
            if (ModelState.IsValid)
            {
                var now = DateTime.Now;
                var date = new DateTime(now.Year, now.Month, now.Day);
                var id = Convert.ToInt32(DefermentReabsorptionId);
                var reabsorptionProcess = await _db.Reabsorptions
                    .Where(dp => dp.DefermentId.Equals(id)).FirstOrDefaultAsync();
                reabsorptionProcess.SenateCommentForReabsorption = SenateComment;
                reabsorptionProcess.SenateApprovalForReabsorption = SenateApprovalForReabsorption;
                reabsorptionProcess.SenateApprovalDateForReabsorption = date;
                _db.SaveChanges();

                return Json(new
                {
                    reabsorptionProcess.SenateCommentForReabsorption,
                    reabsorptionProcess.SenateApprovalForReabsorption,
                    reabsorptionProcess.SenateApprovalDateForReabsorption,
                }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { SenateCommentForReabsorption = false }, JsonRequestBehavior.AllowGet);
        }

        [Route("DefermentReabsorption/DisplayDocument/{fileName}")]
        // GET: /DefermentReabsorption/DisplayDocument/fileName
        public ActionResult DisplayDocument(string fileName)
        {
            string filePath = "~/Content/DefermentDocuments/" + fileName + ".pdf";
            Response.AddHeader("Content-Disposition", "inline; filename=" + fileName);

            return File(filePath, "application/pdf");
        }
    }
}