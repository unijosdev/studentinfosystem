using OfficeOpenXml;
using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampus.ViewModels;
using SwiftKampusModel.AddmissionApplicant;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SwiftKampus.Controllers
{
    [Authorize]
    [Audit(AuditingLevel = 2)]
    public class UtmeScreeningCutOffsController : BaseController
    {
        public UtmeScreeningCutOffsController(SchoolDbContext _db) : base(_db)
        {
        }

        // GET: UtmeScreeningCutOffs
        public ActionResult Index()
        {
            return View();
        }

        // Action Method to return json data to the data-table
        public async Task<ActionResult> GetIndex()
        {
            // dc.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var staffRecord = _db.Staffs.Include(i => i.Department)
                               .FirstOrDefault(x => x.Email.ToUpper().Trim().Equals(userId.Trim().ToUpper()));
            var model = new List<UtmeScreeningCutOff>();
            if (User.IsInRole(RoleName.Hod))
            {
                model = await _db.UtmeScreeningCutOffs.Include(i => i.Session)
                    .Include(i => i.Programme.Department).AsNoTracking()
                    .Where(x => x.Programme.Department.DepartmentId.Equals(staffRecord.Department.DepartmentId)).ToListAsync();
            }
            else
            {
                model = await _db.UtmeScreeningCutOffs.Include(i => i.Session)
                    .Include(i => i.Programme).AsNoTracking().ToListAsync();
            }
            var data = model.Select(s => new
            {
                s.UtmeScreeningCutOffId,
                s.CutOffMark,
                s.Session?.SessionName,
                s?.Programme?.ProgrammeName,
            }).ToList();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }


        // Get method for partial view save/add and update of record
        public async Task<PartialViewResult> Save(int id)
        {
            var staffRecord = _db.Staffs.Include(i => i.Department)
                                .FirstOrDefault(x => x.Email.ToUpper().Trim().Equals(userId.Trim().ToUpper()));
            var utmeScreeningCutOff = await _db.UtmeScreeningCutOffs.Include(i => i.Programme.Department).AsNoTracking()
                                .FirstOrDefaultAsync(i =>i.UtmeScreeningCutOffId.Equals(id));
            if (User.IsInRole(RoleName.Hod))
            {
                ViewBag.ProgrammeId = new SelectList(_db.Programmes.Include(i => i.Department).AsNoTracking()
                                .Where(x => x.Department.DepartmentId.Equals(staffRecord.Department.DepartmentId)), 
                                "ProgrammeId", "ProgrammeName", utmeScreeningCutOff?.ProgrammeId);
            }
            else
            {
                ViewBag.ProgrammeId = new SelectList(_db.Programmes, "ProgrammeId", "ProgrammeName", utmeScreeningCutOff?.ProgrammeId);
            }
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName", utmeScreeningCutOff?.SessionId);
            return PartialView(utmeScreeningCutOff);
        }


        // Post method that return json for saving and updating record
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Save(UtmeScreeningCutOff model)
        {
            if (ModelState.IsValid)
            {
                string message;
                if (model.UtmeScreeningCutOffId > 0)
                {                    
                    _db.Entry(model).State = EntityState.Modified;
                    await _db.SaveChangesAsync();
                    message = $"UTME Screening CutOff Mark Updated Successfully...";
                    return new JsonResult { Data = new { status = true, message } };
                }
                var exist = _db.UtmeScreeningCutOffs.Include(i => i.Programme)
                                .Include(i =>i.Session).AsNoTracking()
                          .Any(x => x.Session.SessionId.Equals((int)model.SessionId)
                          && x.Programme.ProgrammeId.Equals((int)model.ProgrammeId));

                if (exist)
                {
                    return new JsonResult
                    {
                        Data = new
                        {
                            status = false,
                            message = $"Utme Screening CutOff Mark has been set for the selected Programme in the selected Sessiom"
                        }
                    };
                }
                _db.UtmeScreeningCutOffs.Add(model);
                await _db.SaveChangesAsync();
                message = $"Utme Screening CutOff Mark has been set Successfully.";
                return new JsonResult { Data = new { status = true, message } };

            }
            return new JsonResult { Data = new { status = false, message = "Invalid Model" } };
        }

       

        // GET: UtmeScreeningCutOffs/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UtmeScreeningCutOff utmeScreeningCutOff = await _db.UtmeScreeningCutOffs.Include(i => i.Programme)
                                    .Include(i => i.Session).AsNoTracking()
                                    .FirstOrDefaultAsync(i => i.UtmeScreeningCutOffId.Equals((int)id));
            if (utmeScreeningCutOff == null)
            {
                return HttpNotFound();
            }
            return View(utmeScreeningCutOff);
        }

        // POST: UtmeScreeningCutOffs/Delete/5
        [HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            UtmeScreeningCutOff utmeScreeningCutOff = await _db.UtmeScreeningCutOffs.FindAsync(id);
            _db.UtmeScreeningCutOffs.Remove(utmeScreeningCutOff);
            await _db.SaveChangesAsync();
            return new JsonResult { Data = new { status = true, message = "Utme CutOff has been deleted successfully" } };
        }

        [HttpGet]
        [Authorize(Roles = RoleName.Admin + "," + RoleName.DatabaseAdmin + "," + RoleName.SuperAdmin + "," + RoleName.AdmissionUploadOfficer)]
        public ActionResult UploadDeptUtmeCutOff(string message)
        {
            ViewBag.Message = message;
            return View();
        }

        [Authorize(Roles = RoleName.Admin + "," + RoleName.DatabaseAdmin + "," + RoleName.SuperAdmin + "," + RoleName.AdmissionUploadOfficer)]
        public async Task<ActionResult> UploadDeptUtmeCutOff(HttpPostedFileBase excelfile)
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
                        var sessionName = workSheet.Cells[row, 2].Value.ToString().Trim();
                        var ProgrammeCode = workSheet.Cells[row, 1].Value.ToString().Trim();
                        var CutOffMark = workSheet.Cells[row, 3].Value.ToString().Trim();

                        //var student = await _db.Students.AsNoTracking()
                        //                    .Where(x => x.MatricNo.Trim().ToUpper().Equals(matNo.ToUpper()))
                        //                    .SingleOrDefaultAsync();
                        var sessionId = _query.GetCurrentSessionId(1);
                        var session = await _db.Sessions.Where(x => x.SessionName.Trim().Equals(sessionName.Trim())).FirstOrDefaultAsync();
                        var programmeId = await _db.Programmes.Where(x => x.ProgrammeCode.Trim().ToUpper().Equals(ProgrammeCode.Trim().ToUpper())).FirstOrDefaultAsync();

                        

                        if (programmeId != null)
                        {
                            var deptCutOff = new UtmeScreeningCutOff
                            {
                                SessionId = session.SessionId,
                                ProgrammeId = programmeId.ProgrammeId,
                                CutOffMark = Convert.ToInt32(CutOffMark)
                            };
                            _db.UtmeScreeningCutOffs.Add(deptCutOff);
                            utmeUploadError.Add(new StudentUtmeUploadErrorVm { JambRegNo = ProgrammeCode, Row = row });
                            hasUtmeUploadError = true;
                        }
                        else
                        {
                            ViewBag.ErrorMessage = "Student not found";
                            return View("ErrorException");
                        }
                        recordCount++;
                        lastrecord = $"The last Updated record has the DeptCoode  {ProgrammeCode} ";
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
