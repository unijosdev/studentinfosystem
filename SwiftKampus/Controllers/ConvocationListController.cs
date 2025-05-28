using SwiftKampus.Models;
using System.Linq;
using System.Data.Entity;
using System.Web.Mvc;
using System.Threading.Tasks;
using SwiftKampusModel;
using System;
using System.Collections.Generic;
using SwiftKampus.ViewModels;
using SwiftKampus.Services;
using SwiftKampus.ViewModels.ConvocationVm;
using System.Web;
using OfficeOpenXml;
using SwiftKampusModel.Convocation;

namespace SwiftKampus.Controllers
{
    [RoutePrefix("Convocation")]
    public class ConvocationListController : BaseController
    {
        private List<Session> _sessions;
        public ConvocationListController(SchoolDbContext db) : base(db)
        {
            _sessions = GetAllSession();
        }
        // GET: ConvocationList
        public ActionResult ConvocationList()
        {
            {
                ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FancyName");
                ViewBag.FacultyId = new SelectList(_db.Faculties.AsNoTracking(), "FacultyId", "FacultyName");
                ViewBag.DepartmentId = new SelectList(_db.Departments.AsNoTracking(), "DepartmentId", "DeptName");
                ViewBag.ProgrammeId = new SelectList(_db.Programmes.AsNoTracking(), "ProgrammeId", "ProgrammeName");
                ViewBag.SessionId = new SelectList(_db.Sessions.AsNoTracking(), "SessionId", "SessionName");
                return View();
            }
        }

        public async Task<ActionResult> getConvocationList(/*int? SchoolProgrammeId,*/ string hasRegistered, int? FacultyId, int? DepartmentId, int? ProgrammeId, int?  SessionId)
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

            var staffRecord = _db.Staffs.Include(i => i.Department.Faculty)
                              .FirstOrDefault(x => x.Email.ToUpper().Trim().Equals(userId.Trim().ToUpper()));
            if (User.IsInRole(RoleName.Dean))
            {
                FacultyId = staffRecord?.Department.FacultyId;
            }
            if (User.IsInRole(RoleName.Hod))
            {
                DepartmentId = staffRecord?.Department.DepartmentId;
            }

            var studentIndex = new List<ConvocationListVm>();
            bool activeStudent = false;
            if (!string.IsNullOrEmpty(hasRegistered) && hasRegistered.Equals("True"))
            {
                activeStudent = true;
            }

            var studentList = await GetAllConvocationList(/*SchoolProgrammeId,*/ FacultyId, DepartmentId, ProgrammeId, SessionId);

            if (!string.IsNullOrEmpty(search))
            {
                studentIndex = studentList.Where(x => x.Fullname.ToUpper().Contains(search.ToUpper().Trim())).ToList();
                //if (studentIndex.Count() == 0)
                //{
                //    studentIndex = studentList.Where(x => !string.IsNullOrEmpty(x.Email) &&
                //            x.Email.ToUpper().Contains(search.ToUpper().Trim())).ToList();
                //}
            }

            else
            {
                studentIndex = studentList;
            }

            totalRecords = studentIndex.Count();
            var data = studentIndex.Skip(skip).Take(pageSize).ToList();

            return Json(new { draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data },
                JsonRequestBehavior.AllowGet);

            #endregion Server Side filtering
        }

        public async Task<List<ConvocationListVm>> GetAllConvocationList(/*int? schoolProgrammeId, */ int? facultyId, int? departmentId, int? programmeId, int? SessionId)
        {
            //var model;
            var studentList = new List<ConvocationListVm>();
            if (programmeId != null)
            {
                studentList = await _db.ConvocationLists.Include(i => i.Student)
                                                        .Include(i => i.Student.Programme.Department.Faculty)
                                                        .Include(i => i.Student.Programme.Department)
                                                        .Include(i => i.Student.Programme)
                                                        .Include(i => i.Session).AsNoTracking()
                                                        .Where(x => x.Student.ProgrammeId == (int)programmeId && x.SessionId == (int)SessionId)
                                                        .Select(x => new ConvocationListVm()
                                                        {
                                                            Matric = x.Student.MatricNo,
                                                            Fullname = x.Corrected_LastName + " " + x.Corrected_FirstName + " " + x.Corrected_MiddleName,
                                                            ClassOfDegree = x.ClassOfDegree,
                                                            PhoneNumber = x.Student.PhoneNumber,
                                                            ProgrammeOfStudy = x.Student.Programme.ProgrammeName,
                                                            WillAttend = x.WillAttend,
                                                            LeaseGown = x.WillLeaseGown,
                                                        }).ToListAsync();
                return studentList;

            }
            else
            {
                studentList = await _db.ConvocationLists.Include(i => i.Student)
                                                       .Include(i => i.Student.Programme.Department.Faculty)
                                                       .Include(i => i.Student.Programme.Department)
                                                       .Include(i => i.Student.Programme)
                                                       .Include(i => i.Session).AsNoTracking()
                                                       //.Where(x => x.Student.ProgrammeId.Equals((int)programmeId))
                                                       .Select(x => new ConvocationListVm()
                                                       {
                                                           Matric = x.Student.MatricNo,
                                                           Fullname = x.Corrected_LastName + " " + x.Corrected_FirstName + " " + x.Corrected_MiddleName,
                                                           ClassOfDegree = x.ClassOfDegree,
                                                           PhoneNumber = x.Student.PhoneNumber,
                                                           ProgrammeOfStudy = x.Student.Programme.ProgrammeName,
                                                           WillAttend = x.WillAttend,
                                                           LeaseGown = x.WillLeaseGown,
                                                       }).ToListAsync();
                return studentList;
            }

        }

        public PartialViewResult ExcelUpload()
        {
            return PartialView();
        }

        [Authorize(Roles = RoleName.Admin + "," + RoleName.DatabaseAdmin + "," + RoleName.SuperAdmin)]
        [HttpPost]
        public async Task<ActionResult> UploadConvocationList(HttpPostedFileBase excelfile)
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

                    List<string> nonExisting = new List<string>();


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

                    int count = 0;
                    var ugSchoolProgrammeId = await GetUndergraduateSchoolProgrammeId();
                    var currentSessionId = _query.GetCurrentSessionId(ugSchoolProgrammeId);
                    var utmeUploadError = new List<StudentUtmeUploadErrorVm>();
                    bool hasUtmeUploadError = false;

                    var programmes = GetAllProgramme();
                    var levels = GetLevelList();

                    //var emailList = new List<string>();
                    for (int row = 2; row <= noOfRow; row++)
                    {
                        var matricNo = workSheet.Cells[row, 2].Value.ToString().Trim();
                        var firstname = workSheet.Cells[row, 3].Value.ToString().Trim();
                        var lastname = workSheet.Cells[row, 4 ].Value.ToString().Trim();
                        var middlename = workSheet.Cells[row, 5].Value.ToString().Trim();
                        var classOfDegree = workSheet.Cells[row, 6].Value.ToString().Trim();
                        string code = workSheet.Cells[row, 7].Value.ToString().Trim().ToUpper();
                        var sessionName = workSheet.Cells[row, 8].Value.ToString().Trim();
                        int sessionId = _db.Sessions.Where(x => x.SessionName.Equals(sessionName)).Select(i => i.SessionId).First();


                        var programmeCode = programmes.FirstOrDefault(x => x.ProgrammeCode.Trim().ToUpper().Equals(code));

                        if (programmeCode == null)
                        {
                            ViewBag.ErrorInfo = "Please Leave no column or row Empty/Blank";
                            ViewBag.ErrorMessage = $" The department Option \"{code}\"  specified in the excel at row {row} doesn't exist on the portal. " +
                                                   $"Please add the Department Option first before uploading or correct the excel sheet...";
                            return View("ErrorException");
                        }

                        try
                        {
                            var studentExit = _db.Students.Any(x => x.MatricNo.Trim().ToUpper().Equals(matricNo.ToUpper()));
                            if (studentExit)
                            {
                                var graduant = new ConvocationList();

                                var studentId = _db.Students.Where(x => x.MatricNo.Equals(matricNo)).Select(i => i.StudentId).First();

                                graduant.StudentId = studentId;
                                graduant.Corrected_FirstName = firstname;
                                graduant.Corrected_LastName = lastname;
                                graduant.Corrected_MiddleName = middlename != null ? middlename : null ;
                                graduant.ClassOfDegree = classOfDegree;
                                graduant.ProgrammeId = _db.Programmes.Where(x => x.ProgrammeCode.Equals(code)).Select(i => i.ProgrammeId).First();
                                graduant.SessionId = sessionId;

                                _db.ConvocationLists.Add(graduant);
                                count += 1;
                                lastrecord = $"The last Updated record has the Last Name {graduant.Corrected_FirstName} and First Name {graduant.Corrected_LastName} with Student Id {graduant.Corrected_MiddleName}";


                            }
                            else
                            {
                                nonExisting.Add(matricNo);
                            }

                        }
                        catch (Exception ex)
                        {
                            ViewBag.ErrorInfo = "Please Leave no column or row Empty/Blank";
                            ViewBag.ErrorMessage = ex.Message;
                            return View("ErrorException");
                        }
                    }
                    try
                    {
                        await _db.SaveChangesAsync();
                    }
                    catch (Exception)
                    {

                        throw;
                    }

                    if (hasUtmeUploadError)
                    {
                        ViewBag.ErrorInfo = $"These applicants wasn't found on the system";
                        ViewBag.ErrorMessage = $"You have successfully Uploaded {count} records...";
                        return View("ErrorException", utmeUploadError);
                    }
                    message = $"You have successfully Uploaded {count} records...  and {nonExisting} are NOT on the Student records";
                }
                return RedirectToAction("ConvocationList", "ConvocationList", new { message });
            }

            ViewBag.Error = $"File type is Incorrect <br/>";
            return View("Index");
        }


        public async Task<ActionResult> GraduantView()
        {
            var studentRecord = _db.Students.FirstOrDefault(x => x.Email.ToUpper().Trim().Equals(userId.Trim().ToUpper()));

            var graduateRecord = await _db.ConvocationLists.Include(i => i.Student)
                                                     .Include(i => i.Student.Programme.Department.Faculty)
                                                     .Include(i => i.Student.Programme.Department)
                                                     .Include(i => i.Student.Programme)
                                                     .Include(i => i.Session).AsNoTracking()
                                                       .Where(x => x.StudentId.Equals(studentRecord.StudentId))
                                                       .Select(x => new ConvocationListVm()
                                                       {
                                                           studentId = x.StudentId,
                                                           Matric = x.Student.MatricNo,
                                                           Fullname = x.Corrected_LastName + " " + x.Corrected_FirstName + " " + x.Corrected_MiddleName,
                                                           Department = x.Student.Programme.Department.DeptName,
                                                           Faculty = x.Student.Programme.Department.Faculty.FacultyName,
                                                           PhoneNumber = x.Student.PhoneNumber,
                                                           ProgrammeOfStudy = x.Student.Programme.ProgrammeName,
                                                           WillAttend = x.WillAttend,
                                                           LeaseGown = x.WillLeaseGown,
                                                           Email  = x.Student.PrimaryEmail
                                                       }).FirstOrDefaultAsync();
            return View(graduateRecord);
        }


        public async Task<PartialViewResult> ShowGrauandName(string studentId)
        {
            var graduandDeatil = await _db.ConvocationLists.Where(g => g.StudentId.Trim().ToUpper().Equals(studentId)).FirstOrDefaultAsync();
            return PartialView(graduandDeatil);
        }

        public async Task<ActionResult> JASONUpdateGraduandNames(ConvocationList formRecord)
        {
            string message = "";
            var graduant = await _db.ConvocationLists.Where(g => g.StudentId.ToUpper().Trim().Equals(formRecord.StudentId)).FirstOrDefaultAsync();
            graduant.Corrected_FirstName = formRecord.Corrected_FirstName;
            graduant.Corrected_LastName = formRecord.Corrected_LastName;
            graduant.Corrected_MiddleName = formRecord.Corrected_MiddleName != null ? formRecord.Corrected_MiddleName : null;

            _db.Entry(graduant).State = EntityState.Modified;
            if (_db.SaveChanges() > 0)
            {
                var fullname = $"{graduant.Corrected_LastName.ToUpper()}  {graduant.Corrected_FirstName} { formRecord.Corrected_MiddleName}";
                message = fullname;
            }
            else
            {
                message = "Eroor, Please try again!";
            }
           
            return Json(message);
        }

        [HttpGet]
        public async Task<ActionResult> JASONOptAttendance(string studentId)
        {
            string message = "";
            bool status = false;
            var graduant = await _db.ConvocationLists.Where(g => g.StudentId.ToUpper().Trim().Equals(studentId)).FirstOrDefaultAsync();
            graduant.WillAttend = true;
            
            _db.Entry(graduant).State = EntityState.Modified;
            if (_db.SaveChanges() > 0)
            {
                status = true;
            }
            else
            {
                status = false;
            }
            return this.Json(status, JsonRequestBehavior.AllowGet);
        }

        public async Task<PartialViewResult> ShowLeaseGown(string studentId)
        {
            var graduandDetail = await _db.ConvocationLists.Include(i => i.Student.SchoolProgramme)
                                                           .Where(g => g.StudentId.Trim().ToUpper().Equals(studentId))
                                                           .FirstOrDefaultAsync();
            ViewBag.gownChargeId = new SelectList(_db.SundryAndOtherIncomeCharges.AsNoTracking(), "Id", "ChargeDescription");

            var model = new AcademicGownLeaseVM()
            {
                StudentId = graduandDetail.StudentId,
                SessionId = graduandDetail.SessionId,
                SchoolProgrammeId = graduandDetail.ProgrammeId,
                SchoolProhrammeName = graduandDetail.Student.SchoolProgramme.SchoolProgrammeCode
            };

            return PartialView(model);
        }

        public async Task<ActionResult> JASONCreateLeaseGown(AcademicGownLeaseVM formData)
        {
            var leaseGown = new AcademicGownLease();
            string message = "";
            var checkLeaseGown = await _db.AcademicGownLeases.Where(g => g.StudentId.ToUpper().Trim().Equals(formData.StudentId)).FirstOrDefaultAsync();
            var leaseGownStatus = await _db.ConvocationLists.Where(l => l.StudentId.ToUpper().Trim().Equals(formData.StudentId)).FirstOrDefaultAsync();
            if (checkLeaseGown != null)
            {
                checkLeaseGown.StudentId = formData.StudentId;
                checkLeaseGown.GownHeight = formData.GownHeight;
                checkLeaseGown.LeaseType = formData.LeaseType;
                checkLeaseGown.SessionId = formData.SessionId;

                leaseGownStatus.WillLeaseGown = true;
                _db.Entry(leaseGownStatus).State = EntityState.Modified;
                _db.Entry(checkLeaseGown).State = EntityState.Modified;
            }
            else
            {
                leaseGown.StudentId = formData.StudentId;
                leaseGown.GownHeight = formData.GownHeight;
                leaseGown.LeaseType = formData.LeaseType;
                leaseGown.SessionId = formData.SessionId;

                leaseGownStatus.WillLeaseGown = true;
                _db.Entry(leaseGownStatus).State = EntityState.Modified;
                _db.AcademicGownLeases.Add(leaseGown);
            }
            if (_db.SaveChanges() > 0)
            {
                message = $"success";
            }
            else
            {
                message = "Eroor, Please try again!";
            }

            return Json(message);
        }

        public ActionResult SuperAdminDashBoard()
        {
            return View();
        }
    }
}