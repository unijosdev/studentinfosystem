using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using OfficeOpenXml;

using Rotativa;

using SwiftKampus.Abstractions;
using SwiftKampus.BusinessLogic;
using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampus.ViewModels;
using SwiftKampus.ViewModels.Fee_Management;
using SwiftKampus.ViewModels.StudentBioData;
using SwiftKampus.ViewModels.StudentStatusMgtVm;
using SwiftKampusModel;
using SwiftKampusModel.AddmissionApplicant;

namespace SwiftKampus.Controllers
{
    [Authorize]
    [Audit(AuditingLevel = 2)]
    public class AffiliateController : BaseController
    {
        IFeeQueryManager _feeQueryManager;
        ICourseRegQueryManager _courseRegQuery;
        IStaffQueryManager _staffQuery;
        private List<Session> allSessions;

        public AffiliateController(SchoolDbContext db) : base(db)
        {
            _courseRegQuery = new CourseRegQueryManager(_db);
            _feeQueryManager = new FeeQueryManager(_db);
            _staffQuery = new StaffQueryManager(_db);
            allSessions = GetAllSession();
        }
        // GET: Affiliate
        public async Task<ActionResult> Index()

        {
            var mystates = from State s in Enum.GetValues(typeof(State))
                           select new { ID = s, Name = s.ToString() };
            var staffDeptId = await _staffQuery.GetStaffDepartmentId(userId);
            var staffDept = await _db.Departments.Include(x => x.Faculty).Where(x => x.DepartmentId == staffDeptId).FirstOrDefaultAsync();

            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking().Where(x => x.SchoolProgrammeId == 53), "SchoolProgrammeId", "FancyName");
            ViewBag.FacultyId = new SelectList(_db.Faculties.AsNoTracking().Where(x => x.FacultyId == staffDept.FacultyId), "FacultyId", "FacultyName");
            ViewBag.DepartmentId = new SelectList(_db.Departments.AsNoTracking().Where(x => x.DepartmentId == staffDeptId), "DepartmentId", "DeptName");
            ViewBag.ProgrammeId = new SelectList(_db.Programmes.AsNoTracking(), "ProgrammeId", "ProgrammeName");
            ViewBag.LevelId = new SelectList(_db.Levels.AsNoTracking(), "LevelId", "LevelName");
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            ViewBag.StateOfOrigin = new SelectList(mystates, "Name", "Name");
            if (User.IsInRole("Student"))
            {
                return RedirectToAction("Login", "Account");
            }
            return View();
        }

        public ActionResult ProgrammeCodes()
        {
            return View();
        }

        public async Task<ActionResult> GetProgrammeCodes()
        {

            var staffDeptId = await _staffQuery.GetStaffDepartmentId(userId);
            var staffDept = await _db.Departments.Include(x => x.Faculty).Where(x => x.DepartmentId == staffDeptId).FirstOrDefaultAsync();
            var data = await _db.Programmes.Include(i => i.Department.Faculty).AsNoTracking().Where(i => i.Department.FacultyId == staffDept.FacultyId)
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

        public ActionResult UploadStudent()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> UploadStudent(HttpPostedFileBase excelfile)
        {
            if (excelfile == null || excelfile.ContentLength == 0)
            {
                ViewBag.Error = "Please Select a excel file <br/>";
                TempData["UserMessage"] = "Please Select a excel file.";
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
                    int requiredField = 13;
                    var utmeUploadError = new List<StudentUtmeUploadErrorVm>();
                    bool hasUtmeUploadError = false;
                    var matNumCountFromDb = 0;
                    var matNumCount = 1;

                    string validCheck = myExcel.ValidateExcel(noOfRow, workSheet, requiredField);
                    if (!validCheck.Equals("Success"))
                    {
                        string[] ssizes = validCheck.Split(' ');

                        string lineError = $"Line/Row number {ssizes[0]}  and column {ssizes[1]} is not rightly formatted, Please Check for anomalies ";
                        ViewBag.Message = lineError;
                        ViewBag.ErrorInfo = "Please Leave no column or row Empty/Blank";
                        ViewBag.ErrorMessage = lineError;
                        return View("ErrorException");
                    }

                    for (int row = 2; row <= noOfRow; row++)
                    {
                        var studentId = DateTime.Now.Ticks;
                        string JambRegNo = workSheet.Cells[row, 1].Value.ToString().Trim();
                        string code = workSheet.Cells[row, 11].Value.ToString().Trim();
                        string level = workSheet.Cells[row, 12].Value.ToString().Trim();
                        string studentType = workSheet.Cells[row, 13].Value.ToString().Trim();
                        string sessionName = workSheet.Cells[row, 14].Value.ToString().Trim();
                        string modeOfEntry = workSheet.Cells[row, 15].Value.ToString().Trim();

                        var lastName = workSheet.Cells[row, 2].Value.ToString().Trim();
                        var firstName = workSheet.Cells[row, 3].Value.ToString().Trim();
                        var middleName = workSheet.Cells[row, 4].Value.ToString().Trim();
                        var email = workSheet.Cells[row, 5].Value.ToString().Trim();


                        if (lastName.Trim().Equals("."))
                        {
                            lastName = "";
                        }
                        if (middleName.Trim().Equals("."))
                        {
                            middleName = "";
                        }
                        if (firstName.Trim().Equals("."))
                        {
                            middleName = "";
                        }


                        var programmeCode = await _db.Programmes.Include(x => x.Department.Faculty).AsNoTracking()
                                            .Where(x => x.ProgrammeCode.ToUpper().Equals(code.ToUpper()))
                                            .FirstOrDefaultAsync();
                        var schoolProgramme = await _db.SchoolProgrammes.AsNoTracking()
                                            .Where(x => x.SchoolProgrammeCode.ToUpper().Equals(studentType.ToUpper()))
                                            .FirstOrDefaultAsync();
                        var uploadedSessionId = await _db.Sessions.AsNoTracking().Where(x => x.SessionName.ToUpper().Equals(sessionName.ToUpper()))
                                                   .FirstOrDefaultAsync();
                        var levelId = _db.Levels.AsNoTracking().Where(s => s.LevelName.Equals(level))
                                            .Select(s => s.LevelId.ToString()).FirstOrDefault();


                        if (levelId == null)
                        {
                            ViewBag.ErrorInfo = "Please Leave no column or row Empty/Blank";
                            ViewBag.ErrorMessage = $" The \"{level}\" level specified in the excel doesn't exist on the portal. " +
                                                   $"Please add the level first before uploading or correct the excel sheet...";
                            return View("ErrorException");
                        }
                        if (programmeCode == null)
                        {
                            ViewBag.ErrorInfo = "Please Leave no column or row Empty/Blank";
                            ViewBag.ErrorMessage = $" The department Option \"{code}\"  specified in the excel at row {row} doesn't exist on the portal. " +
                                                   $"Please add the Department Option first before uploading or correct the excel sheet...";
                            return View("ErrorException");
                        }
                        if (uploadedSessionId == null)
                        {
                            ViewBag.ErrorInfo = "Please Leave no column or row Empty/Blank";
                            ViewBag.ErrorMessage = $" The Session Name \"{sessionName}\"  specified in the excel at row {row} doesn't exist on the portal. " +
                                                   $"Please add the Session Name first before uploading or correct the excel sheet...";
                            return View("ErrorException");
                        }

                        if (schoolProgramme == null)
                        {
                            ViewBag.ErrorInfo = $"This {studentType} school programme Code  at row {row} doesn't exist in the system.";
                            ViewBag.ErrorMessage = "Please check the Student Programme type spelling very well ";
                            return View("ErrorException");
                        }
                        var checkStudent = _db.Students.FirstOrDefault(x => x.MatricNo.Trim().ToUpper().Equals(JambRegNo.ToUpper()));
                        matNumCountFromDb = await _db.Students.Include(i => i.Programme.Department.Faculty).AsNoTracking()
                                   .CountAsync(x => x.Programme.Department.FacultyId.Equals(programmeCode.Department.Faculty.FacultyId)
                                   && x.Session.SessionId.Equals(uploadedSessionId.SessionId)
                                   && x.SchoolProgramme.SchoolProgrammeId.Equals(schoolProgramme.SchoolProgrammeId)
                                   && !string.IsNullOrEmpty(x.MatricNo));

                        if (checkStudent == null)
                        {
                            try
                            {
                                var student = new Student()
                                {
                                    StudentId = $"{SchoolSetUp.CurrentSchoolName}{studentId}",
                                    //MatricNo = workSheet.Cells[row, 1].Value.ToString().Trim(),
                                    JambRegNo = workSheet.Cells[row, 2].Value.ToString().Trim(),
                                    FirstName = firstName,
                                    MiddleName = middleName,
                                    LastName = lastName,
                                    Email = email,
                                    MatricNo = GenerateMatricNo(schoolProgramme, uploadedSessionId.StartDate.Year, programmeCode.Department.Faculty.FacultyCode, matNumCountFromDb != 0 ? matNumCountFromDb + matNumCount++ : ++matNumCount),
                                    StateOfOrigin = workSheet.Cells[row, 6].Value.ToString().Trim(),
                                    Nationality = workSheet.Cells[row, 7].Value.ToString().Trim(),
                                    DateOfBirth = DateTime.Parse(workSheet.Cells[row, 8].Value.ToString().Trim()),
                                    Gender = workSheet.Cells[row, 9].Value.ToString().Trim(),
                                    EnrollmentDate = DateTime.Parse(workSheet.Cells[row, 10].Value.ToString().Trim()),
                                    ProgrammeId = programmeCode.ProgrammeId,
                                    LevelId = int.Parse(levelId),
                                    StudentStatus = StudentStatus.Returning.ToString(),
                                    SchoolProgrammeId = schoolProgramme.SchoolProgrammeId,
                                    SessionId = uploadedSessionId.SessionId,
                                    ModeOfEntry = modeOfEntry,
                                    IsClearedAcademics = false,
                                    IsClearedDepartment = false,
                                    IsClearedFaculty = false,
                                };

                                _db.Students.Add(student);
                                recordCount++;
                                lastrecord = $"The last Updated record has the Last Name {student.LastName} and First Name {student.FirstName} with Student Id {student.StudentId}";
                            }
                            catch (Exception ex)
                            {
                                ViewBag.ErrorInfo = "Please Leave no column or row Empty/Blank";
                                ViewBag.ErrorMessage = ex.Message;
                                return View("ErrorException");
                            }
                        }
                        else
                        {
                            utmeUploadError.Add(new StudentUtmeUploadErrorVm { JambRegNo = JambRegNo, Row = row });
                            hasUtmeUploadError = true;
                        }

                    }
                    await _db.SaveChangesAsync();
                    if (hasUtmeUploadError)
                    {
                        ViewBag.ErrorInfo = $"These applicants wasn't found on the system";
                        ViewBag.ErrorMessage = $"You have successfully Uploaded {recordCount} records...";
                        return View("ErrorException", utmeUploadError);
                    }
                    message = $"You have successfully Uploaded {recordCount} records...  and {lastrecord}";
                }
                return RedirectToAction("Index", "Students", new { message });
            }

            ViewBag.Error = $"File type is Incorrect <br/>";
            return View("Index");
        }

        public PartialViewResult ExcelUpload()
        {
            return PartialView();
        }

        public string GenerateMatricNo(SchoolProgramme model, int year, string facultyCode, int newNumber)
        {
            string myNo = ConvertToProperMatric(newNumber);
            if (model.ProgrammeCategory.Equals(ProgrammeCategory.UnderGraduate.ToString())
                    && model.ProgrammeType.Equals(ProgrammeType.Full_Time.ToString()))
            {
                return $"UJ/{year}/{facultyCode}/{myNo}";
            }
            if (model.ProgrammeCategory.Equals(ProgrammeCategory.UnderGraduate.ToString())
                    && model.ProgrammeType.Equals(ProgrammeType.Part_Time.ToString()))
            {
                return $"UJ/{year}/PT{facultyCode}/{myNo}";
            }
            if (model.ProgrammeCategory.Equals(ProgrammeCategory.Phd.ToString())
                   || model.ProgrammeCategory.Equals(ProgrammeCategory.Masters.ToString())
                   || model.ProgrammeCategory.Equals(ProgrammeCategory.Post_Graduate.ToString()))
            {
                return $"UJ/{year}/PG{facultyCode}/{myNo}";
            }
            if (model.ProgrammeCategory.Equals(ProgrammeCategory.Remedial_Science.ToString())
                  || model.ProgrammeCategory.Equals(ProgrammeCategory.IJMB.ToString())
                  || model.ProgrammeCategory.Equals(ProgrammeCategory.Preliminary_French.ToString()))
            {
                return $"UJ/{year}/{model.SchoolProgrammeCode}/{myNo}";
            }
            if (model.ProgrammeCategory.Equals(ProgrammeCategory.Institute_Of_Education.ToString()))
            {
                return $"UJ/{year}/{facultyCode}/PT/{myNo}";
            }
            return "";
        }

        private string ConvertToProperMatric(int number)
        {
            string no = number.ToString();
            if (no.Count() == 1)
            {
                return $"000{number}";
            }
            if (no.Count() == 2)
            {
                return $"00{number}";
            }
            if (no.Count() == 3)
            {
                return $"0{number}";
            }
            return number.ToString();
        }
    }
}