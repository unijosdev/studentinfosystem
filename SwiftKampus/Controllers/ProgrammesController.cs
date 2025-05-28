using OfficeOpenXml;
using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampusModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SwiftKampus.Controllers
{
    [Authorize]
    [Audit(AuditingLevel = 2)]
    public class ProgrammesController : BaseController
    {

        public ProgrammesController(SchoolDbContext db) : base(db)
        {

        }
        // GET: Programmes
        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> GetIndex()
        {
            // dc.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var data = await _db.Programmes.Include(i => i.Department).Include(i => i.FinalLevel).AsNoTracking()
                .Select(s => new
                {
                    s.ProgrammeId,
                    s.ProgrammeCode,
                    s.ProgrammeName,
                    s.Department.DeptName,
                    s.FinalLevel.LevelName,
                    s.AwardingDegreeName,
                    s.NoOfSemesters
                }).ToListAsync();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        public async Task<PartialViewResult> Save(int id)
        {
            var programme = await _db.Programmes.FindAsync(id);
            ViewBag.DepartmentId = new SelectList(_db.Departments.AsNoTracking(), "DepartmentId", "DeptName", programme?.DepartmentId);
            ViewBag.LevelId = new SelectList(_db.Levels.AsNoTracking(), "LevelId", "LevelName", programme?.LevelId);
            return PartialView(programme);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Save(Programme model)
        {
            bool status = false;
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                if (model.ProgrammeId > 0)
                {
                    var programme = await _db.Programmes.FindAsync(model.ProgrammeId);
                    if (programme != null)
                    {
                        programme.ProgrammeCode = model.ProgrammeCode;
                        programme.ProgrammeName = model.ProgrammeName;
                        programme.DepartmentId = model.DepartmentId;
                        programme.NoOfSemesters = model.NoOfSemesters;
                        programme.AwardingDegreeName = model.AwardingDegreeName;
                        programme.LevelId = model.LevelId;
                        _db.Entry(programme).State = EntityState.Modified;
                        await _db.SaveChangesAsync();
                        message = $"{model.ProgrammeName} Option Updated Successfully...";
                        return new JsonResult { Data = new { status = true, message } };
                    }
                }
                else
                {
                    _db.Programmes.Add(model);
                    await _db.SaveChangesAsync();
                    message = $"{model.ProgrammeName} Option Added Successfully.";
                    return new JsonResult { Data = new { status = true, message } };
                }
            }
            var builder = new StringBuilder();
            foreach (var item in ModelState)
            {
                builder.Append(item.Key);
                builder.Append(", ");
            }
            return new JsonResult { Data = new { status, message = $"Ooops... {builder.ToString()} are all Required" } };

        }

        // GET: Programmes/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Programme programme = await _db.Programmes.FindAsync(id);
            if (programme == null)
            {
                return HttpNotFound();
            }
            return View(programme);
        }

        public async Task<PartialViewResult> Delete(int id)
        {
            var programme = await _db.Programmes.Include(i => i.Department)
                        .Where(x => x.ProgrammeId.Equals(id)).FirstOrDefaultAsync();
            return PartialView(programme);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            bool status = false;
            string message = string.Empty;
            var programme = await _db.Programmes.FindAsync(id);
            if (programme != null)
            {
                _db.Programmes.Remove(programme);
                await _db.SaveChangesAsync();
                status = true;
                message = "Department Option Deleted Successfully...";
                return new JsonResult { Data = new { status, message } };
            }

            return new JsonResult { Data = new { status = false, message = $"{id } not deleted" } };
        }

        public PartialViewResult UploadProgramme()
        {
            return PartialView();
        }


        [HttpPost]
        public async Task<ActionResult> UploadProgramme(HttpPostedFileBase excelfile)
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
                    int requiredField = 5;

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
                        ViewBag.ErrorInfo = "Please Leave no column or row Empty/Blank";
                        ViewBag.ErrorMessage = $" The \"{lineError}\" is not formatted properly ";
                        return View("ErrorException");

                    }
                    for (int row = 2; row <= noOfRow; row++)
                    {
                        var deptCode = workSheet.Cells[row, 1].Value.ToString().Trim();
                        var programmeName = workSheet.Cells[row, 2].Value.ToString().Trim();
                        var programmeCode = workSheet.Cells[row, 3].Value.ToString().Trim();
                        var noofSemesters = workSheet.Cells[row, 4].Value.ToString().Trim();
                        var awardingDegreeName = workSheet.Cells[row, 5].Value.ToString().Trim();

                        var dept = await _db.Departments.AsNoTracking()
                            .Where(x => x.DeptCode.ToUpper().Equals(deptCode.ToUpper()))
                            .FirstOrDefaultAsync();

                        var programmes = await _db.Programmes.AsNoTracking()
                           .Where(x => x.ProgrammeCode.ToUpper().Equals(programmeCode.ToUpper()))
                           .FirstOrDefaultAsync();

                        if (dept == null)
                        {
                            ViewBag.ErrorInfo = "Please Leave no column or row Empty/Blank";
                            ViewBag.ErrorMessage = $" The \"{deptCode}\" Dept code specified in the excel doesn't exist on the portal. " +
                                                   $"Please add the faculty first before uploading or correct the excel sheet...";
                            return View("ErrorException");
                        }
                        if(programmeCode != null)
                        {
                            try
                            {
                                var model = new Programme()
                                {
                                    DepartmentId = dept.DepartmentId,
                                    ProgrammeCode = programmeCode,
                                    ProgrammeName = programmeName,
                                    NoOfSemesters = Convert.ToInt16(noofSemesters),
                                    AwardingDegreeName = awardingDegreeName
                                };
                                _db.Programmes.Add(model);
                                recordCount++;
                                lastrecord = $"The last record uploaded has the Certificate code {programmeCode} and Certificate Name {programmeName}";
                            }
                            catch (Exception ex)
                            {
                                ViewBag.ErrorInfo = $"There is possible Dept code duplicate at row ({row}), Please check and try again";
                                ViewBag.ErrorMessage = ex.Message;
                                return View("ErrorException");
                            }
                        }


                      
                    }
                    try
                    {
                        await _db.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        ViewBag.ErrorInfo = "There is possible Certificate code duplicate, " +
                                            "Please check for certificate code duplicates and try again";
                        ViewBag.ErrorMessage = ex.Message;
                        return View("ErrorException");
                    }

                    message = $"You have successfully Uploaded {recordCount} records...  and {lastrecord}";
                    ViewBag.Message = message;

                }
                return RedirectToAction("Index", "Programmes", new { message });
            }

            ViewBag.Error = $"File type is Incorrect <br/>";
            return View("Index");
        }

        public PartialViewResult UpdateProgrammeCode()
        {
            return PartialView();
        }

        [HttpPost]
        public async Task<ActionResult> UpdateProgrammeCode(HttpPostedFileBase excelfile)
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
                    int requiredField = 4;

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
                        ViewBag.Message = lineError;
                        return View("Index");
                    }
                    for (int row = 2; row <= noOfRow; row++)
                    {
                        var oldCode = workSheet.Cells[row, 1].Value.ToString().Trim();
                        var newCode = workSheet.Cells[row, 2].Value.ToString().Trim();
                        var noOfSemes = Convert.ToInt32(workSheet.Cells[row, 3].Value.ToString().Trim());
                        var awardingDegree = workSheet.Cells[row, 4].Value.ToString().Trim();


                        var programme = await _db.Programmes.AsNoTracking()
                            .Where(x => x.ProgrammeCode.ToUpper().Equals(oldCode.ToUpper()))
                            .FirstOrDefaultAsync();

                        if (programme == null)
                        {
                            ViewBag.ErrorInfo = "Please Leave no column or row Empty/Blank";
                            ViewBag.ErrorMessage = $" The \"{oldCode}\" dept code specified in the excel doesn't exist on the portal. " +
                                                   $"Please add the faculty first before uploading or correct the excel sheet...";
                            return View("ErrorException");
                        }                       

                        try
                        {
                            programme.ProgrammeCode = newCode;
                            programme.NoOfSemesters = noOfSemes;
                            programme.AwardingDegreeName = awardingDegree;
                            _db.Entry(programme).State = EntityState.Modified;
                            recordCount++;
                            lastrecord = $"The last record uploaded has the Dept code {programme.ProgrammeCode} and Dept Name {programme.ProgrammeName}";
                        }
                        catch (Exception ex)
                        {
                            ViewBag.ErrorInfo = "There is possible Dept code duplicate, Please check and try again";
                            ViewBag.ErrorMessage = ex.Message;
                            return View("ErrorException");
                        }
                    }
                    await _db.SaveChangesAsync();
                    message = $"You have successfully Uploaded {recordCount} records...  and {lastrecord}";
                    ViewBag.Message = message;

                }
                return RedirectToAction("Index", "Departments", new { message });
            }

            ViewBag.Error = $"File type is Incorrect <br/>";
            return View("Index");
        }


        public PartialViewResult UpdateProgrammeLevel()
        {
            return PartialView();
        }

        [HttpPost]
        public async Task<ActionResult> UpdateProgrammeLevel(HttpPostedFileBase excelfile)
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
                    int requiredField = 2;

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
                        ViewBag.Message = lineError;
                        return View("Index");
                    }
                    for (int row = 2; row <= noOfRow; row++)
                    {
                        var programmeCode = workSheet.Cells[row, 1].Value.ToString().Trim();
                        var levelName = workSheet.Cells[row, 2].Value.ToString().Trim();                  


                        var programme = await _db.Programmes.AsNoTracking()
                            .Where(x => x.ProgrammeCode.ToUpper().Equals(programmeCode.ToUpper()))
                            .FirstOrDefaultAsync();
                        var level = _db.Levels.AsNoTracking().Where(x => x.LevelName.Equals(levelName)).FirstOrDefault();

                        if (programme == null)
                        {
                            ViewBag.ErrorInfo = "Please Leave no column or row Empty/Blank";
                            ViewBag.ErrorMessage = $" The \"{programmeCode}\" dept code specified in the excel doesn't exist on the portal. " +
                                                   $"Please add the faculty first before uploading or correct the excel sheet...";
                            return View("ErrorException");
                        }

                        if (level == null)
                        {
                            ViewBag.ErrorInfo = "Please Leave no column or row Empty/Blank";
                            ViewBag.ErrorMessage = $" The \"{levelName}\" level code specified in the excel doesn't exist on the portal. " +
                                                   $"Please add the faculty first before uploading or correct the excel sheet...";
                            return View("ErrorException");
                        }

                        try
                        {
                            programme.LevelId = level.LevelId;                          
                            _db.Entry(programme).State = EntityState.Modified;
                            recordCount++;
                            lastrecord = $"The last record uploaded has the Dept code {programme.ProgrammeCode} and Dept Name {programme.ProgrammeName}";
                        }
                        catch (Exception ex)
                        {
                            ViewBag.ErrorInfo = "There is possible Dept code duplicate, Please check and try again";
                            ViewBag.ErrorMessage = ex.Message;
                            return View("ErrorException");
                        }
                    }
                    await _db.SaveChangesAsync();
                    message = $"You have successfully Uploaded {recordCount} records...  and {lastrecord}";
                    ViewBag.Message = message;

                }
                return RedirectToAction("Index", "Programmes", new { message });
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