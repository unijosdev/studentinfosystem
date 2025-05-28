using OfficeOpenXml;
using SwiftKampus.BusinessLogic;
using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampusModel;
using System;
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
    public class LevelsController : BaseController
    {

        public LevelsController(SchoolDbContext db) : base(db)
        {

        }
        // GET: Levels
        public ActionResult Index()
        {
            return View();
        }

        // Action Method to return json data to the data-table
        public async Task<ActionResult> GetIndex()
        {
            // dc.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var model = await _db.Levels.Include(i => i.SchoolProgramme).AsNoTracking()
                .ToListAsync();
            var data = model.Select(s => new
            {
                s.LevelId,
                s.LevelName,
                s.LevelOrder,
                s?.SchoolProgramme?.FancyName,
            }).ToList();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        // Get method for partial view save/add and update of record
        public async Task<PartialViewResult> Save(int id)
        {
            var level = await _db.Levels.FindAsync(id);
            var levelOrders = LevelOrder();
            var levelOrder = from s in levelOrders
                             select new { ID = s, Name = s.ToString() };
            ViewBag.LevelOrder = new SelectList(levelOrder, "Name", "Name");
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FancyName", level?.SchoolProgrammeId);

            return PartialView(level);
        }


        // Post method that return json for saving and updating record
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Save(Level model)
        {
            if (ModelState.IsValid)
            {


                string message;
                if (model.LevelId > 0)
                {
                    model.LevelName = model.LevelName.ToUpper().Trim();
                    model.LevelOrder = model.LevelOrder;
                    _db.Entry(model).State = EntityState.Modified;
                    await _db.SaveChangesAsync();
                    message = $"{model.LevelName} Level Updated Successfully...";
                    return new JsonResult { Data = new { status = true, message } };
                }
                var exist = _db.Levels.AsNoTracking()
                          .Any(x => x.LevelName.ToUpper().Equals(model.LevelName.Trim().ToUpper()));

                if (exist)
                {
                    return new JsonResult
                    {
                        Data = new
                        {
                            status = false,
                            message = $"This Level Name supplied ({model.LevelName}) is already existing" +
                                            $" on the portal, Please check and try again"
                        }
                    };
                }
                model.LevelName = model.LevelName.ToUpper().Trim();
                _db.Levels.Add(model);
                await _db.SaveChangesAsync();
                message = $"{model.LevelName} Level Added Successfully.";
                return new JsonResult { Data = new { status = true, message } };

            }
            return new JsonResult { Data = new { status = false, message = "Invalid Model" } };
        }

        // GET: Levels/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Level level = await _db.Levels.FindAsync(id);
            if (level == null)
            {
                return HttpNotFound();
            }
            return View(level);
        }

        // Get Partial view for deleting record
        public async Task<PartialViewResult> Delete(int id)
        {
            var level = await _db.Levels.FindAsync(id);
            return PartialView(level);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            bool status = false;
            string message = string.Empty;
            var level = await _db.Levels.FindAsync(id);
            if (level != null)
            {
                _db.Levels.Remove(level);
                await _db.SaveChangesAsync();
                status = true;
                message = "Level Deleted Successfully...";
                return new JsonResult { Data = new { status, message } };
            }

            return new JsonResult { Data = new { status = false, message = $"{id } not deleted" } };
        }


        public ActionResult UpdateStudentLevel()
        {
            return View();
        }


        [HttpPost]
        public async Task<ActionResult> UpdateStudentLevel(HttpPostedFileBase excelfile)
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
                        var matricNo = workSheet.Cells[row, 1].Value.ToString().Trim();
                        var levelName = workSheet.Cells[row, 2].Value.ToString().Trim();

                        int levelId = await _db.Levels.AsNoTracking()
                                            .Where(x => x.LevelName.Trim().ToUpper().Equals(levelName.ToUpper()))
                                            .Select(x => x.LevelId).FirstOrDefaultAsync();

                        if (levelId < 1)
                        {
                            ViewBag.ErrorInfo = "Please Leave no column or row Empty/Blank";
                            ViewBag.ErrorMessage = $" The Level Name  \"{levelName}\" at row {row}  specified in the excel doesn't exist on the portal. " +
                                                   $"Please add the Department Option first before uploading or correct the excel sheet...";
                            return View("ErrorException");
                        }
                        var student = await _db.Students.AsNoTracking()
                                            .Where(x => x.MatricNo.Trim().ToUpper().Equals(matricNo.ToUpper()))
                                            .FirstOrDefaultAsync();

                        if (student != null)
                        {
                            student.LevelId = levelId;
                            _db.Entry(student).State = EntityState.Modified;
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
                    message = $"You have successfully Uploaded {recordCount} records...  and {lastrecord}";
                    ViewBag.Message = message;
                    return RedirectToAction("Index", "Students", new { message });
                }
            }
            ViewBag.Error = $"File type is Incorrect <br/>";
            return View("Index");
        }

        public ActionResult UpdateStudentModeOfEntry()
        {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> UpdateStudentModeOfEntry(HttpPostedFileBase excelfile)
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
                        var changeModeOfEntry = workSheet.Cells[row, 2].Value.ToString().Trim();

                        int levelId = await _db.Levels.AsNoTracking()
                                            .Where(x => x.LevelName.Trim().ToUpper().Equals(changeModeOfEntry.ToUpper()))
                                            .Select(x => x.LevelId).FirstOrDefaultAsync();

                        if (!ModeOfEntry.RS.ToString().Equals(changeModeOfEntry.Trim().ToUpper().ToString()))
                        {
                            ViewBag.ErrorInfo = "Whoops! Please check and correct Mode of Entry";
                            return View("ErrorException");
                        }

                        //if (levelId < 1)
                        //{
                        //    ViewBag.ErrorInfo = "Please Leave no column or row Empty/Blank";
                        //    ViewBag.ErrorMessage = $" The Level Name  \"{changeModeOfEntry}\" at row {row}  specified in the excel doesn't exist on the portal. " +
                        //                           $"Please add the Department Option first before uploading or correct the excel sheet...";
                        //    return View("ErrorException");
                        //}
                        var student = await _db.Students.AsNoTracking()
                                            .Where(x => x.JambRegNo.Trim().ToUpper().Equals(jambNo.ToUpper()))
                                            .FirstOrDefaultAsync();

                        if (student != null)
                        {
                            student.ModeOfEntry = changeModeOfEntry.Trim().ToUpper().ToString();
                            _db.Entry(student).State = EntityState.Modified;
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
                    message = $"You have successfully Updated {recordCount} records...  and {lastrecord}";
                    ViewBag.Message = message;
                    return RedirectToAction("Index", "Students", new { message });
                }
            }
            ViewBag.Error = $"File type is Incorrect <br/>";
            return View("Index");
        }

        public ActionResult MigrateStudentToNextLevel()
        {
            ViewBag.FacultyId = new SelectList(_db.Faculties.AsNoTracking(), "FacultyId", "FacultyName");
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            return View();
        }

        public async Task<ActionResult> MigrateStudentToNextLevel(int facultyId, int sessionId)
        {
            var _resultCommand = new ResultCommand(_db);
            int count = 0;
            var allStudentInFaculty = _db.Students.Include(i => i.Programme.Department)
                                        .Include(i => i.Level).AsNoTracking()
                                        .Where(x => x.Programme.Department.FacultyId.Equals(facultyId) &&
                                        x.Active.Equals(true));
            foreach (var student in allStudentInFaculty)
            {
                var deptTemplate = _db.DeptResultTypes.AsNoTracking().Where(x => x.DepartmentId.Equals(student.Programme.Department.DepartmentId) &&
                                        x.SessionId.Equals((int)student.SessionId)).FirstOrDefault();
                var nextLevel = _query.GetNextLevel(student.Level.LevelOrder);
                var newLevelId = _resultCommand.GetLevelByName(nextLevel);
                if (deptTemplate != null)
                {
                    var allCa = await _resultCommand.GetCurrentCa(student.StudentId, student.Level.LevelId, sessionId);
                    if (deptTemplate.ResultTemplate.ResultType.Equals(ResultNameType.Pharmacy.ToString()))
                    {
                        if (allCa.All(x => x.Total > deptTemplate.FailMark))
                        {
                            student.LevelId = newLevelId;
                            _db.Entry(student).State = EntityState.Modified;
                            count += 1;
                        }
                    }
                    else
                    {
                        var cgpa = Convert.ToDouble(await _resultCommand.CalculateCgpa(student.StudentId));
                        if (student.Level.LevelOrder.Equals("200") && cgpa >= 1.00)
                        {
                            student.LevelId = newLevelId;
                            _db.Entry(student).State = EntityState.Modified;
                            count += 1;
                        }
                    }
                }
            }
            return View();
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