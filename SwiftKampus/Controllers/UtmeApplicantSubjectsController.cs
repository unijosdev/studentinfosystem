using OfficeOpenXml;
using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampus.ViewModels;
using SwiftKampusModel;
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
    public class UtmeApplicantSubjectsController : BaseController
    {
        public UtmeApplicantSubjectsController(SchoolDbContext db) : base(db)
        {
        }
        // GET: UtmeApplicantSubjects
        public ActionResult Index()
        {
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            return View();
        }


        public async Task<ActionResult> GetIndex(int SessionId)
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

            var schoolProgrammeId = _db.SchoolProgrammes.AsNoTracking()
                                .Where(x => x.ProgrammeCategory.Equals(ProgrammeCategory.UnderGraduate.ToString()) &&
                                x.ProgrammeType.Equals(ProgrammeType.Full_Time.ToString()))
                                .Select(s => s.SchoolProgrammeId).FirstOrDefault();
            sessionId = SessionId;

            var model = new List<UtmeApplicantSubject>();
            var utmeApplicants = await _db.UtmeApplicants.AsNoTracking().Where(x => x.SessionId.Equals(sessionId))
                                .Select(s => s.JambRegNo).ToListAsync();
            foreach (var utmeApplicant in utmeApplicants)
            {
                model.AddRange(await _db.UtmeApplicantSubjects.AsNoTracking()
                                        .Where(x => x.JambRegNo.Equals(utmeApplicant)).ToListAsync());

            }
            if (!string.IsNullOrEmpty(search))
            {
                model = model.Where(x => x.JambRegNo.ToUpper().Equals(search.ToUpper().Trim())).ToList();
            }

            totalRecords = model.Count();
            var data = model.Skip(skip).Take(pageSize).ToList();

            return Json(new { draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data },
                JsonRequestBehavior.AllowGet);

            #endregion Server Side filtering
        }



        // GET: UtmeApplicantSubjects/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UtmeApplicantSubject utmeApplicantSubject = await _db.UtmeApplicantSubjects.FindAsync(id);
            if (utmeApplicantSubject == null)
            {
                return HttpNotFound();
            }
            return View(utmeApplicantSubject);
        }


        public async Task<PartialViewResult> Save(int id)
        {
            var utmeApplicantSubject = await _db.UtmeApplicantSubjects.FindAsync(id);

            return PartialView(utmeApplicantSubject);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Save(UtmeApplicantSubject model)
        {
            bool status = false;
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                if (model.UtmeApplicantSubjectId > 0)
                {
                    var utmeApplicantSubject = await _db.UtmeApplicantSubjects.FindAsync(model.UtmeApplicantSubjectId);
                    if (utmeApplicantSubject != null)
                    {
                        utmeApplicantSubject.JambRegNo = model.JambRegNo;
                        utmeApplicantSubject.SubjectName = model.SubjectName;
                        utmeApplicantSubject.Score = model.Score;
                        _db.Entry(utmeApplicantSubject).State = EntityState.Modified;
                        await _db.SaveChangesAsync();
                        message = $"{model.JambRegNo} Updated Successfully...";
                        return new JsonResult { Data = new { status = true, message } };

                    }
                }
                else
                {
                    _db.UtmeApplicantSubjects.Add(model);
                    await _db.SaveChangesAsync();
                    message = $"{model.JambRegNo} Added Successfully.";
                    return new JsonResult { Data = new { status = true, message } };
                }
            }
            return new JsonResult { Data = new { status, message } };
            //return View(subject);
        }

        // GET: UtmeApplicantSubjects/Create
        public ActionResult Create()
        {
            ViewBag.JambRegNo = new SelectList(_db.UtmeApplicants, "JambRegNo", "Surname");
            return View();
        }

        // POST: UtmeApplicantSubjects/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "UtmeApplicantSubjectId,JambRegNo,SubjectName,Score")] UtmeApplicantSubject utmeApplicantSubject)
        {
            if (ModelState.IsValid)
            {
                _db.UtmeApplicantSubjects.Add(utmeApplicantSubject);
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.JambRegNo = new SelectList(_db.UtmeApplicants, "JambRegNo", "Surname", utmeApplicantSubject.JambRegNo);
            return View(utmeApplicantSubject);
        }

        // GET: UtmeApplicantSubjects/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UtmeApplicantSubject utmeApplicantSubject = await _db.UtmeApplicantSubjects.FindAsync(id);
            if (utmeApplicantSubject == null)
            {
                return HttpNotFound();
            }
            ViewBag.JambRegNo = new SelectList(_db.UtmeApplicants, "JambRegNo", "Surname", utmeApplicantSubject.JambRegNo);
            return View(utmeApplicantSubject);
        }

        // POST: UtmeApplicantSubjects/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "UtmeApplicantSubjectId,JambRegNo,SubjectName,Score")] UtmeApplicantSubject utmeApplicantSubject)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(utmeApplicantSubject).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.JambRegNo = new SelectList(_db.UtmeApplicants, "JambRegNo", "Surname", utmeApplicantSubject.JambRegNo);
            return View(utmeApplicantSubject);
        }

        // GET: UtmeApplicantSubjects/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UtmeApplicantSubject utmeApplicantSubject = await _db.UtmeApplicantSubjects.FindAsync(id);
            if (utmeApplicantSubject == null)
            {
                return HttpNotFound();
            }
            return View(utmeApplicantSubject);
        }

        // POST: UtmeApplicantSubjects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            UtmeApplicantSubject utmeApplicantSubject = await _db.UtmeApplicantSubjects.FindAsync(id);
            _db.UtmeApplicantSubjects.Remove(utmeApplicantSubject);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }


        public PartialViewResult UploadUtmeSubjects()
        {
            return PartialView();
        }


        [HttpPost]
        public async Task<ActionResult> UploadUtmeSubjects(HttpPostedFileBase excelfile)
        {
            string message = "";
            if (excelfile == null || excelfile.ContentLength == 0)
            {
                ViewBag.ErrorInfo = "Please Select a excel file <br/>";
                ViewBag.ErrorMessage += "You must select an excel file before you click Upload button";
                return View("ErrorException");
            }
            HttpPostedFileBase file = Request.Files["excelfile"];
            if (excelfile.FileName.EndsWith("xls", StringComparison.CurrentCulture) || excelfile.FileName.EndsWith("xlsx", StringComparison.CurrentCulture))
            {
                int recordCount = 0;
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
                    int requiredField = 9;

                    string validCheck = myExcel.ValidateExcel(noOfRow, workSheet, requiredField);
                    if (!validCheck.Equals("Success"))
                    {
                        //string row = "";
                        //string column = "";
                        string[] ssizes = validCheck.Split(' ');
                        string[] myArray = new string[2];
                        for (int i = 0; i < ssizes.Length; i++)
                        {
                            myArray[i] = ssizes[i];
                            // myArray[i] = ssizes[];
                        }
                        string lineError = $"Line/Row number {myArray[0]}  and column {myArray[1]} is not rightly formatted, Please Check for anomalies ";
                        ViewBag.ErrorInfo = lineError;
                        ViewBag.ErrorMessage = lineError;
                        return View("ErrorException");
                    }
                    for (int row = 2; row <= noOfRow; row++)
                    {
                        var jambRegNo = workSheet.Cells[row, 1].Value.ToString().Trim();
                        var utmeApplicant = await _db.UtmeApplicants.FindAsync(jambRegNo);

                        if (utmeApplicant == null)
                        {
                            utmeUploadError.Add(new StudentUtmeUploadErrorVm { JambRegNo = jambRegNo, Row = row });
                        }
                        else
                        {
                            try
                            {
                                var utmeSubjects = await _db.UtmeApplicantSubjects.Where(x => x.JambRegNo.ToUpper().Trim().Equals(jambRegNo.ToUpper())).ToListAsync();
                                if (utmeSubjects.Count() > 0)
                                {
                                    _db.UtmeApplicantSubjects.RemoveRange(utmeSubjects);
                                }
                                var utmeApplicantSubjectList = new List<UtmeApplicantSubject>()
                            {
                                new UtmeApplicantSubject()
                                {
                                    JambRegNo = jambRegNo,
                                    SubjectName = workSheet.Cells[row, 2].Value.ToString().Trim(),
                                    Score = workSheet.Cells[row, 3].Value.ToString().Trim(),
                                },
                                  new UtmeApplicantSubject()
                                {
                                    JambRegNo = jambRegNo,
                                    SubjectName = workSheet.Cells[row, 4].Value.ToString().Trim(),
                                    Score = workSheet.Cells[row, 5].Value.ToString().Trim(),
                                },
                                    new UtmeApplicantSubject()
                                {
                                    JambRegNo = jambRegNo,
                                    SubjectName = workSheet.Cells[row, 6].Value.ToString().Trim(),
                                    Score = workSheet.Cells[row, 7].Value.ToString().Trim(),
                                },
                                      new UtmeApplicantSubject()
                                {
                                    JambRegNo = jambRegNo,
                                    SubjectName = workSheet.Cells[row, 8].Value.ToString().Trim(),
                                    Score = workSheet.Cells[row, 9].Value.ToString().Trim(),
                                },
                            };
                                _db.UtmeApplicantSubjects.AddRange(utmeApplicantSubjectList);
                                recordCount += 1;
                            }
                            catch (Exception ex)
                            {
                                ViewBag.ErrorInfo = $"Error Saving the Utme Subjects in row {row} of the excel";
                                ViewBag.ErrorMessage = ex.Message;
                                return View("ErrorException");
                            }
                        }
                    }
                    try
                    {
                        await _db.SaveChangesAsync();
                        if (utmeUploadError.Count() > 0)
                        {
                            ViewBag.ErrorInfo = $"These Utme Applicants Subject has not been uploaded successfully";
                            ViewBag.ErrorMessage = $"You have successfully Uploaded {utmeUploadError.Count()} records...";
                            return View("ErrorException", utmeUploadError);
                        }
                        else
                        {
                            message = $"You have successfully Uploaded {recordCount} records...";
                            ViewBag.Message = message;
                            return RedirectToAction("Index", "UtmeApplicantSubjects", new { message });
                        }
                    }
                    catch (Exception ex)
                    {
                        ViewBag.ErrorInfo = $"Upload of Utme Subject is not completed successfully";
                        ViewBag.ErrorMessage = ex.Message;
                        return View("ErrorException");
                    }
                }
            }
            message = $"File type is Incorrect <br/>";
            return RedirectToAction("Index", "UtmeApplicantSubjects", new { message });
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
