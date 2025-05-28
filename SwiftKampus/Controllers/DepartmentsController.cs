using OfficeOpenXml;
using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampus.ViewModels;
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
    public class DepartmentsController : BaseController
    {

        public DepartmentsController(SchoolDbContext db) : base(db)
        {

        }

        // GET: Departments
        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> GetIndex()
        {
            // dc.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var data = await _db.Departments.Include(i => i.Faculty).AsNoTracking().Select(s => new
            {
                s.DepartmentId,
                s.Faculty.FacultyName,
                s.DeptCode,
                s.DeptName,
                s.DeptLocation
            }).ToListAsync();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        public async Task<PartialViewResult> Save(int id)
        {
            var dept = await _db.Departments.FindAsync(id);
            ViewBag.FacultyId = new SelectList(_db.Faculties.AsNoTracking(), "FacultyId", "FacultyName");
            return PartialView(dept);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Save(Department model)
        {
            bool status = false;
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                if (model.DepartmentId > 0)
                {
                    var dept = await _db.Departments.FindAsync(model.DepartmentId);
                    if (dept != null)
                    {
                        dept.FacultyId = model.FacultyId;
                        dept.DeptCode = model.DeptCode;
                        dept.DeptName = model.DeptName;
                        dept.DeptLocation = model.DeptLocation;
                        _db.Entry(dept).State = EntityState.Modified;
                        await _db.SaveChangesAsync();
                        message = $"{model.DeptName} Department Updated Successfully...";
                        return new JsonResult { Data = new { status = true, message } };
                    }
                }
                else
                {
                    _db.Departments.Add(model);
                    await _db.SaveChangesAsync();
                    message = $"{model.DeptName} Department Added Successfully.";
                    return new JsonResult { Data = new { status = true, message } };
                }
            }

            var builder = new StringBuilder();
            foreach (var item in ModelState)
            {
                builder.Append(item.Key);
                builder.Append(", ");
            }
            return new JsonResult { Data = new { status, message = $"Ooops... {builder.ToString()} are all Required" } };            //return View(subject);
        }

        [Authorize(Roles = RoleName.Admin)]
        public async Task<PartialViewResult> UpgradeLevel(string studentId)
        {
            var student = await _db.Students.FindAsync(studentId);
            ViewBag.LevelId = new SelectList(_db.Levels.AsNoTracking(), "LevelId", "LevelName", student?.LevelId);
            var studentStaus = from StudentStatus s in Enum.GetValues(typeof(StudentStatus))
                               select new { ID = s, Name = s.ToString() };
            var mygender = from Gender s in Enum.GetValues(typeof(Gender))
                           select new { ID = s, Name = s.ToString() };
            var modeOfEntry = from ModeOfEntry s in Enum.GetValues(typeof(ModeOfEntry))
                              select new { ID = s, Name = s.ToString() };
            ViewBag.StudentStatus = new SelectList(studentStaus, "Name", "Name", student?.StudentStatus);
            ViewBag.Gender = new SelectList(mygender, "Name", "Name", student?.Gender);
            ViewBag.ModeOfEntry = new SelectList(modeOfEntry, "Name", "Name", student?.ModeOfEntry);
            ViewBag.ModeOfEntry = new SelectList(modeOfEntry, "Name", "Name", student?.ModeOfEntry);
            ViewBag.SessionId = new SelectList(_db.Sessions.AsNoTracking(), "SessionId", "SessionName", student?.SessionId);
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FancyName", student?.SchoolProgrammeId);

            return PartialView(student);
        }

        [HttpPost]
        [Authorize(Roles = RoleName.Admin)]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> SaveUpgradeLevel(UpgradeDeptLevelVm model)
        {
            bool status = false;
            string message = string.Empty;
            //if (ModelState.IsValid)
            //{
            var student = await _db.Students.FindAsync(model.StudentId);
            if (student != null)
            {
                student.LevelId = model.LevelId;
                student.SchoolProgrammeId = model.SchoolProgrammeId;
                student.LastName = model.LastName;
                student.FirstName = model.FirstName;
                student.MiddleName = model.MiddleName;
                student.MatricNo = model.MatricNo;
                student.PrimaryEmail = model.PrimaryEmail;
                student.StudentStatus = model.StudentStatus;
                student.Email = model.Email;
                student.Gender = model.Gender;
                student.SessionId = model.SessionId;
                student.DateOfBirth = model.DateOfBirth;
                student.ModeOfEntry = model.ModeOfEntry;
                student.IsPhysicallyChallenged = model.IsPhysicallyChallenged;
                _db.Entry(student).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                message = $"{student.FullName} Level Updated Successfully... Please Refresh your dashboard";
                return new JsonResult { Data = new { status = true, message } };
            }
            // }
            return new JsonResult { Data = new { status, message = $"Ooops... please select a level" } };
            //return View(subject);
        }



        // GET: Departments/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Department department = await _db.Departments.FindAsync(id);
            if (department == null)
            {
                return HttpNotFound();
            }
            return View(department);
        }

        public async Task<PartialViewResult> Delete(int id)
        {
            var dept = await _db.Departments.FindAsync(id);
            return PartialView(dept);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            bool status = false;
            string message = string.Empty;
            var dept = await _db.Departments.FindAsync(id);
            if (dept != null)
            {
                _db.Departments.Remove(dept);
                await _db.SaveChangesAsync();
                status = true;
                message = "Department Deleted Successfully...";
                return new JsonResult { Data = new { status, message } };
            }

            return new JsonResult { Data = new { status = false, message = $"{id } not deleted" } };
        }

        public PartialViewResult UploadDepartment()
        {
            return PartialView();
        }


        [HttpPost]
        public async Task<ActionResult> UploadDepartment(HttpPostedFileBase excelfile)
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
                        var facultyCode = workSheet.Cells[row, 1].Value.ToString().Trim();
                        var deptName = workSheet.Cells[row, 2].Value.ToString().Trim();
                        var deptCode = workSheet.Cells[row, 3].Value.ToString().Trim();
                        var deptLocation = workSheet.Cells[row, 4].Value.ToString().Trim();

                        var faculty = await _db.Faculties.AsNoTracking()
                            .Where(x => x.FacultyCode.ToUpper().Equals(facultyCode.ToUpper()))
                            .FirstOrDefaultAsync();

                        if (faculty == null)
                        {
                            ViewBag.ErrorInfo = "Please Leave no column or row Empty/Blank";
                            ViewBag.ErrorMessage = $" The \"{facultyCode}\" faculty code specified in the excel doesn't exist on the portal. " +
                                                   $"Please add the faculty first before uploading or correct the excel sheet...";
                            return View("ErrorException");
                        }

                        try
                        {
                            var model = new Department()
                            {
                                FacultyId = faculty.FacultyId,
                                DeptCode = deptCode,
                                DeptName = deptName,
                                DeptLocation = deptLocation
                            };
                            _db.Departments.Add(model);
                            recordCount++;
                            lastrecord = $"The last record uploaded has the Dept code {deptCode} and Dept Name {deptName}";
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

        public PartialViewResult UpdateDepartmentCode()
        {
            return PartialView();
        }

        [HttpPost]
        public async Task<ActionResult> UpdateDepartmentCode(HttpPostedFileBase excelfile)
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
                    int requiredField = 3;

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
                        var location = workSheet.Cells[row, 3].Value.ToString().Trim();


                        var dept = await _db.Departments.AsNoTracking()
                            .Where(x => x.DeptCode.ToUpper().Equals(oldCode.ToUpper()))
                            .FirstOrDefaultAsync();

                        if (dept == null)
                        {
                            ViewBag.ErrorInfo = "Please Leave no column or row Empty/Blank";
                            ViewBag.ErrorMessage = $" The \"{oldCode}\" dept code specified in the excel doesn't exist on the portal. " +
                                                   $"Please add the faculty first before uploading or correct the excel sheet...";
                            return View("ErrorException");
                        }

                        try
                        {
                            dept.DeptCode = newCode;
                            dept.DeptLocation = location;
                            _db.Entry(dept).State = EntityState.Modified;
                            recordCount++;
                            lastrecord = $"The last record uploaded has the Dept code {dept.DeptCode} and Dept Name {dept.DeptName}";
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