using Microsoft.AspNet.Identity;
using OfficeOpenXml;
using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampusModel;
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
    public class StaffsController : BaseController
    {

        public StaffsController(SchoolDbContext db) : base(db)
        {

        }
        // GET: Staffs
        public ActionResult Index()
        {
            var staffRole = from StaffRole s in Enum.GetValues(typeof(StaffRole))
                            select new { ID = s, Name = s.ToString() };
            ViewBag.StaffRole = new SelectList(staffRole, "Name", "Name");
            ViewBag.DepartmentId = new SelectList(_db.Departments.AsNoTracking(), "DepartmentId", "DeptName");
            ViewBag.FacultyId = new SelectList(_db.Faculties.AsNoTracking(), "FacultyId", "FacultyName");
            return View();
        }

        public ActionResult StaffList()
        {
            var staffRole = from StaffRole s in Enum.GetValues(typeof(StaffRole))
                            select new { ID = s, Name = s.ToString() };

            ViewBag.StaffRole = new SelectList(staffRole, "Name", "Name");

            var staffRecord = _db.Staffs.Include(i => i.Department.Faculty)
                    .FirstOrDefault(x => x.Email.ToUpper().Trim().Equals(userId.Trim().ToUpper()));

            if (User.IsInRole(RoleName.Dean))
            {
                ViewBag.FacultyId = new SelectList(_db.Faculties.AsNoTracking().Where(x => x.FacultyId.Equals(staffRecord.Department.FacultyId)), "FacultyId", "FacultyName");
                ViewBag.DepartmentId = new SelectList(_db.Departments.AsNoTracking().Where(x => x.FacultyId.Equals(staffRecord.Department.FacultyId)), "DepartmentId", "DeptName");
            }
            else if (User.IsInRole(RoleName.Hod))
            {
                ViewBag.FacultyId = new SelectList(_db.Faculties.AsNoTracking().Where(x => x.FacultyId.Equals(staffRecord.Department.FacultyId)), "FacultyId", "FacultyName");
                ViewBag.DepartmentId = new SelectList(_db.Departments.AsNoTracking().Where(x => x.DepartmentId.Equals(staffRecord.Department.DepartmentId)), "DepartmentId", "DeptName");
            }
            else
            {
                ViewBag.FacultyId = new SelectList(_db.Faculties.AsNoTracking(), "FacultyId", "FacultyName");
                ViewBag.DepartmentId = new SelectList(_db.Departments.AsNoTracking(), "DepartmentId", "DeptName");
            }
            return View();
        }       

        public async Task<ActionResult> GetIndex(string StaffRole, int? DepartmentId, int? FacultyId)
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
            var v = new List<StaffIndexVm>();

            var staffList = new List<Staff>();

            staffList = await _db.Staffs.Include(i => i.Department).AsNoTracking().ToListAsync();

            if (!string.IsNullOrEmpty(StaffRole))
            {
                staffList = staffList.Where(x => x.StaffRole.ToUpper().Equals(StaffRole.ToUpper())).ToList();
            }
            if(FacultyId != null)
            {
                staffList = staffList.Where(x => x.Department != null && x.Department.FacultyId.Equals((int)FacultyId)).ToList();
            }
            if(DepartmentId != null)
            {
                staffList = staffList.Where(x => x.Department != null && x.DepartmentId.Equals((int)DepartmentId)).ToList();
            }

            if (!string.IsNullOrEmpty(search))
            {
                search = search.Trim().ToUpper();
                staffList = staffList.Where(x => x.StaffId.Trim().ToUpper().Contains(search) || x.FullName.ToUpper().Contains(search)).ToList();               
            }

            v.AddRange(MapToStaffIndex(staffList.OrderBy(x => x.StaffId).ToList()));

            totalRecords = v.Count();
            var data = v.Skip(skip).Take(pageSize).ToList();

            return Json(new { draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data },
                JsonRequestBehavior.AllowGet);

            #endregion Server Side filtering
        }

        public async Task<ActionResult> EditStaff(string firstname, int deptId)
        {
            var fetchStaff = await _db.Staffs.Include(s => s.Department)
                                        .Where(s => s.LastName.Trim().ToUpper().Equals(firstname.Trim().ToUpper()) && s.Department.DepartmentId.Equals(deptId))
                                        .FirstOrDefaultAsync();

            if (fetchStaff != null)
            {
                //fetchStaff.StaffId = "00000";

                _db.Entry(fetchStaff).State = EntityState.Deleted;
                await _db.SaveChangesAsync();

            }
            return View("Success");
        }

            public async Task<ActionResult> GetNoneAcademic()
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

            var v = new List<StaffIndexVm>();

            var staffList = new List<Staff>();



            if (!string.IsNullOrEmpty(search))
            {
                staffList = await _db.Staffs.Include(i => i.Department).AsNoTracking().Where(x => x.StaffRole.Equals(StaffRole.None_Academic.ToString())
                            && (x.StaffId.Equals(search) || x.FullName.Equals(search)))
                            .ToListAsync();
                v.AddRange(MapToStaffIndex(staffList));
            }
            else
            {
                staffList = await _db.Staffs.Include(i => i.Department).AsNoTracking()
                            .Where(x => x.StaffRole.Equals(StaffRole.None_Academic.ToString())).ToListAsync();
                v.AddRange(MapToStaffIndex(staffList));
            }
            totalRecords = v.Count();
            var data = v.Skip(skip).Take(pageSize).ToList();

            return Json(new { draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data },
                JsonRequestBehavior.AllowGet);

            #endregion Server Side filtering
        }

        private List<StaffIndexVm> MapToStaffIndex(List<Staff> v)
        {
            //var staffIndex = new List<StaffIndexVm>();
            var staffIndex = v.Select(s => new StaffIndexVm()
            {
                StaffId = !string.IsNullOrEmpty(s.StaffId) ? s.StaffId : "",
                FullName = !string.IsNullOrEmpty(s.FullName) ? s.FullName : "",
                Gender = !string.IsNullOrEmpty(s.Gender) ? s.Gender : "",
                DeptName = !string.IsNullOrEmpty(s.Department?.DeptName) ? s.Department?.DeptName : ""
            }).ToList();
            #region for-each Re-factored
            //foreach (var staff in v)
            //{
            //    var index = new StaffIndexVm()
            //    {
            //        StaffId = staff.StaffId,
            //        FullName = staff.FullName,
            //        Gender = staff.Gender,
            //        DeptName = staff.Department.DeptName,

            //    };
            //    staffIndex.Add(index);
            //} 
            #endregion
            return staffIndex;
        }

        public PartialViewResult ExcelUpload()
        {
            return PartialView();
        }

        public async Task<PartialViewResult> PartialDetails(string id)
        {
            var username = User.Identity.GetUserId();
            //var user = await Db.Users.AsNoTracking().Where(c => c.Id.Equals(username)).Select(c => c.Email).FirstOrDefaultAsync();
            if (id == null)
            {
                id = username;
            }

            var student = await _db.Staffs.FindAsync(id);

            return PartialView(student);
        }

        //staffDashboard
        public ActionResult StaffDashboard()
        {
            //StudentDashboardVM model = new StudentDashboardVM();
            //var semester = await _db.Semesters.Where(x => x.ActiveSemester.Equals(true)).Select(x => x.SemesterId).FirstOrDefaultAsync();
            //var session = await _db.Sessions.Where(x => x.ActiveSession.Equals(true)).Select(x => x.SessionId).FirstOrDefaultAsync();
            ////var dept = _db.Departments.AsNoTracking().ToListAsync();
            //get the student department

            //var male = await _db.CourseRegistrations.AsNoTracking().Where(x => x.SemesterId.Equals(semester)
            //                                                                && x.SessionId.Equals(session)
            //                                                                 && x.ProgrammeId.Equals((int)student.ProgrammeId)).ToListAsync();

            //var female = await _db.CourseRegistrations.AsNoTracking().Where(x => x.SemesterId.Equals(semester)
            //                                                                  && x.SessionId.Equals(session)
            //                                                                  && x.ProgrammeId.Equals((int)student.ProgrammeId)).ToListAsync();
            return View();
        }

        // GET: Staffs/Details/5
        public async Task<ActionResult> Details(string id)
        {
            if (String.IsNullOrEmpty(id))
            {
                id = User.Identity.GetUserId();
            }
            Staff staff = await _db.Staffs.FindAsync(id);
            if (staff == null)
            {
                return HttpNotFound();
            }
            return View(staff);
        }

        // GET: Staffs/Create
        public ActionResult Create()
        {
            var mystates = from State s in Enum.GetValues(typeof(State))
                           select new { ID = s, Name = s.ToString() };
            var mygender = from Gender s in Enum.GetValues(typeof(Gender))
                           select new { ID = s, Name = s.ToString() };
            var marital = from Maritalstatus s in Enum.GetValues(typeof(Maritalstatus))
                          select new { ID = s, Name = s.ToString() };
            var staffRole = from StaffRole s in Enum.GetValues(typeof(StaffRole))
                            select new { ID = s, Name = s.ToString() };
            var religion = from Religion s in Enum.GetValues(typeof(Religion))
                           select new { ID = s, Name = s.ToString() };
            var lga = from LgaEnum s in Enum.GetValues(typeof(LgaEnum))
                      select new { ID = s, Name = s.ToString() };

            ViewBag.Lga = new SelectList(lga, "Name", "Name");
            ViewBag.StaffRole = new SelectList(staffRole, "Name", "Name");
            ViewBag.StateOfOrigin = new SelectList(mystates, "Name", "Name");
            ViewBag.Gender = new SelectList(mygender, "Name", "Name");
            ViewBag.MaritalStatus = new SelectList(marital, "Name", "Name");
            ViewBag.Religion = new SelectList(religion, "Name", "Name");

            ViewBag.DepartmentId = new SelectList(_db.Departments.AsNoTracking(), "DepartmentId", "DeptName");

            //ViewBag.StaffId = new SelectList(_db.OfficeAssignments.AsNoTracking(), "StaffId", "Location");
            return View();
        }

        // POST: Staffs/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Staff staff)
        {
            var mystates = from State s in Enum.GetValues(typeof(State))
                           select new { ID = s, Name = s.ToString() };
            var mygender = from Gender s in Enum.GetValues(typeof(Gender))
                           select new { ID = s, Name = s.ToString() };
            var marital = from Maritalstatus s in Enum.GetValues(typeof(Maritalstatus))
                          select new { ID = s, Name = s.ToString() };
            var staffRole = from StaffRole s in Enum.GetValues(typeof(StaffRole))
                            select new { ID = s, Name = s.ToString() };
            var religion = from Religion s in Enum.GetValues(typeof(Religion))
                           select new { ID = s, Name = s.ToString() };
            var lga = from LgaEnum s in Enum.GetValues(typeof(LgaEnum))
                      select new { ID = s, Name = s.ToString() };
            if (ModelState.IsValid)
            {
                _db.Staffs.Add(staff);
                try
                {
                    await _db.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ViewBag.Message = $"This staff Id {staff.StaffId} is already taken.{ex.InnerException}";
                    ViewBag.Lga = new SelectList(lga, "Name", "Name");
                    ViewBag.StaffRole = new SelectList(staffRole, "Name", "Name");
                    ViewBag.StateOfOrigin = new SelectList(mystates, "Name", "Name");
                    ViewBag.Gender = new SelectList(mygender, "Name", "Name");
                    ViewBag.MaritalStatus = new SelectList(marital, "Name", "Name");
                    ViewBag.Religion = new SelectList(religion, "Name", "Name");

                    ViewBag.DepartmentId = new SelectList(_db.Departments.AsNoTracking(), "DepartmentId", "DeptName");
                    return View(staff);
                }


            }


            ViewBag.Lga = new SelectList(lga, "Name", "Name");
            ViewBag.StaffRole = new SelectList(staffRole, "Name", "Name");
            ViewBag.StateOfOrigin = new SelectList(mystates, "Name", "Name");
            ViewBag.Gender = new SelectList(mygender, "Name", "Name");
            ViewBag.MaritalStatus = new SelectList(marital, "Name", "Name");
            ViewBag.Religion = new SelectList(religion, "Name", "Name");

            ViewBag.DepartmentId = new SelectList(_db.Departments.AsNoTracking(), "DepartmentId", "DeptName");
            //ViewBag.StaffId = new SelectList(_db.OfficeAssignments.AsNoTracking(), "StaffId", "Location", staff.StaffId);
            return View(staff);
        }

        // GET: Staffs/Edit/5
        public async Task<ActionResult> Edit(string id)
        {
            if (String.IsNullOrEmpty(id))
            {
                id = User.Identity.GetUserId();
            }
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Staff staff = await _db.Staffs.FindAsync(id);
            if (staff == null)
            {
                return HttpNotFound();
            }
            var mystates = from State s in Enum.GetValues(typeof(State))
                           select new { ID = s, Name = s.ToString() };
            var mygender = from Gender s in Enum.GetValues(typeof(Gender))
                           select new { ID = s, Name = s.ToString() };
            var marital = from Maritalstatus s in Enum.GetValues(typeof(Maritalstatus))
                          select new { ID = s, Name = s.ToString() };
            var staffRole = from StaffRole s in Enum.GetValues(typeof(StaffRole))
                            select new { ID = s, Name = s.ToString() };
            var religion = from Religion s in Enum.GetValues(typeof(Religion))
                           select new { ID = s, Name = s.ToString() };
            var lga = from LgaEnum s in Enum.GetValues(typeof(LgaEnum))
                      select new { ID = s, Name = s.ToString() };

            ViewBag.Lga = new SelectList(lga, "Name", "Name", staff?.Lga);
            ViewBag.StaffRole = new SelectList(staffRole, "Name", "Name", staff?.StaffRole);
            ViewBag.StateOfOrigin = new SelectList(mystates, "Name", "Name", staff?.StateOfOrigin);
            ViewBag.Gender = new SelectList(mygender, "Name", "Name", staff?.Gender);
            ViewBag.MaritalStatus = new SelectList(marital, "Name", "Name", staff?.MaritalStatus);
            ViewBag.Religion = new SelectList(religion, "Name", "Name", staff?.Religion);

            ViewBag.DepartmentId = new SelectList(_db.Departments.AsNoTracking(), "DepartmentId", "DeptName", staff?.DepartmentId);
            ViewBag.StaffId = new SelectList(_db.OfficeAssignments.AsNoTracking(), "StaffId", "Location", staff.StaffId);
            return View(staff);
        }

        // POST: Staffs/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Staff staff)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(staff).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                TempData["UserMessage"] = "Staff Profile is Updated Successfully.";
                TempData["Title"] = "Success.";

                if (User.IsInRole(RoleName.Admin))
                {
                    return RedirectToAction("Index");
                }
                return RedirectToAction("Details", new { id = staff.StaffId});
            }
            var mystates = from State s in Enum.GetValues(typeof(State))
                           select new { ID = s, Name = s.ToString() };
            var mygender = from Gender s in Enum.GetValues(typeof(Gender))
                           select new { ID = s, Name = s.ToString() };
            var marital = from Maritalstatus s in Enum.GetValues(typeof(Maritalstatus))
                          select new { ID = s, Name = s.ToString() };
            var staffRole = from StaffRole s in Enum.GetValues(typeof(StaffRole))
                            select new { ID = s, Name = s.ToString() };
            var religion = from Religion s in Enum.GetValues(typeof(Religion))
                           select new { ID = s, Name = s.ToString() };
            var lga = from LgaEnum s in Enum.GetValues(typeof(LgaEnum))
                      select new { ID = s, Name = s.ToString() };

            ViewBag.Lga = new SelectList(lga, "Name", "Name", staff?.Lga);
            ViewBag.StaffRole = new SelectList(staffRole, "Name", "Name", staff?.StaffRole);
            ViewBag.StateOfOrigin = new SelectList(mystates, "Name", "Name", staff?.StateOfOrigin);
            ViewBag.Gender = new SelectList(mygender, "Name", "Name", staff?.Gender);
            ViewBag.MaritalStatus = new SelectList(marital, "Name", "Name", staff?.MaritalStatus);
            ViewBag.Religion = new SelectList(religion, "Name", "Name", staff?.Religion);

            ViewBag.DepartmentId = new SelectList(_db.Departments.AsNoTracking(), "DepartmentId", "DeptName", staff.DepartmentId);
            ViewBag.StaffId = new SelectList(_db.OfficeAssignments.AsNoTracking(), "StaffId", "Location", staff.StaffId);
            return View(staff);
        }

        // GET: Staffs/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Staff staff = await _db.Staffs.FindAsync(id);
            if (staff == null)
            {
                return HttpNotFound();
            }
            return View(staff);
        }

        // POST: Staffs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            Staff staff = await _db.Staffs.FindAsync(id);
            if (staff != null) _db.Staffs.Remove(staff);

            // Remove the courses assigned to a lecturer
            var assignedCourses = await _db.AssignedCourses.Where(x => x.StaffId == id).ToListAsync();
            foreach (var item in assignedCourses)
            {
                _db.AssignedCourses.Remove(item);
            }

            await _db.SaveChangesAsync();
            TempData["UserMessage"] = "Staff Deleted Successfully.";
            TempData["Title"] = "Error.";

            return RedirectToAction("Index");
        }

        [AllowAnonymous]
        public async Task<ActionResult> RenderImage(string staffId)
        {
            Staff staff = await _db.Staffs.FindAsync(staffId);

            byte[] photoBack = staff.Passport;

            return File(photoBack, "image/png");
        }

        public ActionResult UploadStaff()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> UploadStaff(HttpPostedFileBase excelfile)
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
                string message = String.Empty;
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
                        //ViewBag.LineError = lineError;
                        TempData["UserMessage"] = lineError;
                        TempData["Title"] = "Error.";
                        return View("ErrorException");
                    }
                    try
                    {
                        for (int row = 2; row <= noOfRow; row++)
                        {
                            string name = workSheet.Cells[row, 10].Value.ToString().ToUpper().Trim();
                            int deptId = _db.Departments.AsNoTracking().Where(x => x.DeptCode.ToUpper().Trim().Equals(name))
                                            .Select(s => s.DepartmentId).FirstOrDefault();
                            var staffRole = workSheet.Cells[row, 12].Value.ToString().Trim();


                            if (staffRole.ToUpper().Equals("ACADEMIC"))
                            {
                                staffRole = StaffRole.Academic.ToString();
                            }
                            else if (staffRole.ToUpper().Equals("NONE ACADEMIC") || staffRole.ToUpper().Equals("NONE-ACADEMIC"))
                            {
                                staffRole = StaffRole.None_Academic.ToString();
                            }
                            else
                            {
                                ViewBag.ErrorInfo = "Staff role type supported is \"Academic\"  and \"None Academic\" ";
                                ViewBag.ErrorMessage = "Please check the staff role spelling very well ";
                                return View("ErrorException");
                            }
                            var staff = new Staff()
                            {
                                StaffId = workSheet.Cells[row, 1].Value.ToString().Trim(),
                                FirstName = workSheet.Cells[row, 2].Value.ToString().Trim(),
                                MiddleName = workSheet.Cells[row, 3].Value.ToString().Trim(),
                                LastName = workSheet.Cells[row, 4].Value.ToString().Trim(),
                                StateOfOrigin = workSheet.Cells[row, 5].Value.ToString().Trim(),
                                Nationality = workSheet.Cells[row, 6].Value.ToString().Trim(),
                                DateOfBirth = DateTime.Parse(workSheet.Cells[row, 7].Value.ToString().Trim()),
                                Gender = workSheet.Cells[row, 8].Value.ToString().Trim(),
                                ResumptionDate = DateTime.Parse(workSheet.Cells[row, 9].Value.ToString().Trim()),
                                DepartmentId = deptId,
                                MaritalStatus = workSheet.Cells[row, 11].Value.ToString().Trim(),
                                StaffRole = staffRole,
                                Email = workSheet.Cells[row, 13].Value.ToString().Trim(),
                            };

                            _db.Staffs.Add(staff);
                            recordCount++;
                            lastrecord = $"The last Updated record has the Last Name {staff.LastName} and First Name {staff.FirstName} with Student Id {staff.StaffId}";

                        }
                        await _db.SaveChangesAsync();
                        message = $"You have successfully Uploaded {recordCount} records...  and {lastrecord}";
                        TempData["UserMessage"] = message;
                        TempData["Title"] = "Success.";
                    }
                    catch (Exception e)
                    {
                        ViewBag.Message = e.Message;
                        return View("ErrorException");
                    }
                }
                return RedirectToAction("Index", "Staffs");
            }

            ViewBag.Error = $"File type is Incorrect <br/>";
            return View("Index");
        }

        //public async Task<ActionResult> StaffsInRole(string Rolrname)
        //{
        //    //var staffs = await _db.Staffs.Where(x => x.StaffRole.ToUpper().Trim().Equals(Rolrname.ToUpper().Trim())).ToListAsync();
        //    var staffs = await _db.Staffs.ToListAsync();
        //    return View();
        //}

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