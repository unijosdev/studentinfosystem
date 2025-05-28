using Microsoft.AspNet.Identity;
using OfficeOpenXml;
using OfficeOpenXml.Drawing;
using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampusModel;
using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SwiftKampus.Controllers
{
    [Authorize]
    [Audit(AuditingLevel = 2)]
    public class PreDegreeStudentsController : BaseController
    {

        public PreDegreeStudentsController(SchoolDbContext db) : base(db)
        {

        }
        // GET: PreDegreeStudents
        public async Task<ActionResult> Index()
        {
            return View(await _db.PreDegreeStudents.ToListAsync());
        }

        public ActionResult GetData()
        {
            // dc.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var data = _db.PreDegreeStudents.Select(s => new { s.RegNo, s.FullName, s.Department, s.Gender }).ToList();
            return Json(new { data }, JsonRequestBehavior.AllowGet);

        }

        // GET: PreDegreeStudents/Details/5
        public async Task<ActionResult> Details(string id)
        {
            if (id == null)
            {
                id = User.Identity.GetUserName();
            }
            var preDegreeStudent = await _db.PreDegreeStudents.AsNoTracking()
                .Where(x => x.RegNo.Equals(id)).FirstOrDefaultAsync();
            if (preDegreeStudent == null)
            {
                return HttpNotFound();
            }
            return View(preDegreeStudent);
        }

        // GET: PreDegreeStudents/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: PreDegreeStudents/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(PreDegreeStudent preDegreeStudent)
        {
            if (ModelState.IsValid)
            {
                _db.PreDegreeStudents.Add(preDegreeStudent);
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(preDegreeStudent);
        }

        // GET: PreDegreeStudents/Edit/5
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)
            {
                id = User.Identity.GetUserName();

            }
            var preDegreeStudent = await _db.PreDegreeStudents.AsNoTracking()
                                .Where(x => x.RegNo.Equals(id)).FirstOrDefaultAsync();
            if (preDegreeStudent == null)
            {
                return HttpNotFound();
            }
            return View(preDegreeStudent);
        }

        // POST: PreDegreeStudents/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(PreDegreeStudent preDegreeStudent)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(preDegreeStudent).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(preDegreeStudent);
        }

        // GET: PreDegreeStudents/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var preDegreeStudent = await _db.PreDegreeStudents.FindAsync(id);
            if (preDegreeStudent == null)
            {
                return HttpNotFound();
            }
            return View(preDegreeStudent);
        }

        // POST: PreDegreeStudents/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var preDegreeStudent = await _db.PreDegreeStudents.FindAsync(id);
            if (preDegreeStudent != null) _db.PreDegreeStudents.Remove(preDegreeStudent);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> RenderImage(string studentId)
        {
            var preDegree = await _db.PreDegreeStudents.Where(x => x.RegNo.Equals(studentId))
                                        .FirstOrDefaultAsync();

            byte[] photoBack = preDegree.Passport;

            return File(photoBack, "image/png");
        }

        [AllowAnonymous]
        public ActionResult UploadStudent()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> UploadStudent(HttpPostedFileBase excelfile)
        {
            if (excelfile == null || excelfile.ContentLength == 0)
            {
                ViewBag.Error = "Please Select a excel file <br/>";
                TempData["UserMessage"] = "Please Select a excel file.";
                TempData["Title"] = "Error.";

                return View("UploadStudent");
            }
            HttpPostedFileBase file = Request.Files["excelfile"];
            if (excelfile.FileName.EndsWith("xls", StringComparison.CurrentCulture) ||
                excelfile.FileName.EndsWith("xlsx", StringComparison.CurrentCulture))
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
                    int requiredField = 10;

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
                        //ViewBag.LineError = lineError;
                        TempData["UserMessage"] = lineError;
                        TempData["Title"] = "Error.";
                        return View();
                    }
                    for (int row = 2; row <= noOfRow; row++)
                    {
                        var regNo = workSheet.Cells[row, 3].Value.ToString().Trim();
                        var pic = workSheet.Drawings[regNo] as ExcelPicture;
                        try
                        {
                            //ExcelPicture picture = workSheet.Drawings;
                            var preDegree = new PreDegreeStudent
                            {
                                FullName = workSheet.Cells[row, 1].Value.ToString().Trim(),
                                Gender = workSheet.Cells[row, 2].Value.ToString().Trim(),
                                RegNo = regNo,
                                JambRegNo = workSheet.Cells[row, 4].Value.ToString().Trim(),
                                LGA = workSheet.Cells[row, 5].Value.ToString().Trim(),
                                State = workSheet.Cells[row, 6].Value.ToString().Trim(),
                                CourseInView = workSheet.Cells[row, 7].Value.ToString().Trim(),
                                JambScore = workSheet.Cells[row, 8].Value.ToString().Trim(),
                                PhoneNumber = workSheet.Cells[row, 9].Value.ToString().Trim(),
                                Department = workSheet.Cells[row, 10].Value.ToString().Trim(),
                                Password = workSheet.Cells[row, 11].Value.ToString().Trim(),
                                //Passport = ImageToByteArray(pic.Image),
                            };

                            _db.PreDegreeStudents.Add(preDegree);
                            recordCount++;
                            lastrecord = $"The last Updated record has the  Name {preDegree.FullName} with Reg No Id {preDegree.RegNo}";

                        }
                        catch (Exception ex)
                        {
                            ViewBag.ErrorInfo = "The image not found";
                            ViewBag.ErrorMessage = ex.Message;
                            return View("ErrorException");
                        }
                    }
                    await _db.SaveChangesAsync();

                    message = $"You have successfully Uploaded {recordCount} records...  and {lastrecord}";
                    TempData["UserMessage"] = message;
                    TempData["Title"] = "Success.";

                }
                return RedirectToAction("Index", "PreDegreeStudents");
            }

            ViewBag.Error = $"File type is Incorrect <br/>";
            return View();
        }

        [AllowAnonymous]
        public ActionResult UploadExam()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> UploadExam(HttpPostedFileBase excelfile)
        {
            if (excelfile == null || excelfile.ContentLength == 0)
            {
                ViewBag.Error = "Please Select a excel file <br/>";
                TempData["UserMessage"] = "Please Select a excel file.";
                TempData["Title"] = "Error.";

                return View("UploadStudent");
            }
            HttpPostedFileBase file = Request.Files["excelfile"];
            if (excelfile.FileName.EndsWith("xls", StringComparison.CurrentCulture) ||
                excelfile.FileName.EndsWith("xlsx", StringComparison.CurrentCulture))
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
                    //int requiredField = 10;


                    for (int row = 2; row <= noOfRow; row++)
                    {
                        var preDegreeExam = new PreDegreeExam
                        {
                            PreDegreeExamId = Convert.ToInt32(workSheet.Cells[row, 1].Value.ToString().Trim()),
                            RegNo = workSheet.Cells[row, 2].Value.ToString().Trim(),
                            SubjectName = workSheet.Cells[row, 3].Value.ToString().Trim(),
                            Score = Convert.ToDouble(workSheet.Cells[row, 4].Value.ToString().Trim())
                        };

                        _db.PreDegreeExams.Add(preDegreeExam);
                        recordCount++;

                    }
                    await _db.SaveChangesAsync();
                    message = $"You have successfully Uploaded {recordCount} records...  and {lastrecord}";
                    TempData["UserMessage"] = message;
                    TempData["Title"] = "Success.";

                }
                return RedirectToAction("Index", "PreDegreeStudents");
            }

            ViewBag.Error = $"File type is Incorrect <br/>";
            return View();
        }

        public byte[] ImageToByteArray(System.Drawing.Image imageIn)
        {
            using (var ms = new MemoryStream())
            {
                imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
                return ms.ToArray();
            }
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
