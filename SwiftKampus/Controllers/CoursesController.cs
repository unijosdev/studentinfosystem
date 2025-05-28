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
    public class CoursesController : BaseController
    {

        public CoursesController(SchoolDbContext db) : base(db)
        {

        }

        public async Task<ActionResult> Index(string message)
        {
            ViewBag.Success = message;
            ViewBag.LevelId = new SelectList(_db.Levels.AsNoTracking(), "LevelId", "LevelName");
            ViewBag.SemesterId = new SelectList(_db.Semesters.AsNoTracking(), "SemesterId", "SemesterName");
            var studentType = from StudentType s in Enum.GetValues(typeof(StudentType))
                              select new { ID = s, Name = s.ToString() };
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FancyName");
            //if (!User.IsInRole(RoleName.DAPM_Officer))
            //{
            //    var staffId = userId;
            //    var staffDept = await _db.Staffs.AsNoTracking().Where(x => x.StaffId.Equals(staffId))
            //                        .Select(s => s.DepartmentId).FirstOrDefaultAsync();
            //    ViewBag.ProgrammeId = new SelectList(_db.Programmes.AsNoTracking().Where(x => x.DepartmentId.Equals(staffDept)), "ProgrammeId", "ProgrammeName");

            //}
            //else
            //{
                ViewBag.ProgrammeId = new SelectList(_db.Programmes.AsNoTracking(), "ProgrammeId", "ProgrammeName");
            //}
            return View();
        }

        public async Task<ActionResult> HodActivation(string message)
        {
            ViewBag.Success = message;
            ViewBag.LevelId = new SelectList(_db.Levels.AsNoTracking(), "LevelId", "LevelName");
            ViewBag.SemesterId = new SelectList(_db.Semesters.AsNoTracking(), "SemesterId", "SemesterName");

            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FancyName");
            if (!User.IsInRole("Admin"))
            {
                var staffId = userId;
                var staffDept = await _db.Staffs.Include(i => i.Department).AsNoTracking()
                                        .Where(x => x.Email.Equals(staffId))
                                        .Select(s => s.Department.DepartmentId)
                                        .FirstOrDefaultAsync();
                ViewBag.ProgrammeId = new SelectList(_db.Programmes.Include(i => i.Department).AsNoTracking()
                                            .Where(x => x.Department.DepartmentId.Equals(staffDept)), "ProgrammeId", "ProgrammeName");

            }
            else
            {
                ViewBag.ProgrammeId = new SelectList(_db.Programmes.AsNoTracking(), "ProgrammeId", "ProgrammeName");
            }
            return View();
        }

        public async Task<ActionResult> GetIndex(int? LevelId, int? SemesterId, int? ProgrammeId, int? SchoolProgrammeId)
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

            var courses = new List<Course>();
            if (SchoolProgrammeId != null)
            {
                courses = await _db.Courses.Include(i => i.SchoolProgramme).Include(c => c.Level)
                            .Include(c => c.Programme).Include(c => c.Semester).Include(i => i.CoursePrerequisites).AsNoTracking()
                           .Where(x => x.SchoolProgramme.SchoolProgrammeId.Equals((int)SchoolProgrammeId)).ToListAsync();
            }
            else
            {
                courses = await _db.Courses.Include(i => i.SchoolProgramme).Include(c => c.Level).Include(c => c.Programme)
                                .Include(c => c.Semester).Include(i => i.CoursePrerequisites).AsNoTracking().ToListAsync();
            }

            if (!string.IsNullOrEmpty(search))
            {
                var v = courses.Where(x => x.CourseCode.ToUpper().Equals(search.ToUpper().Trim())
                                    || x.CourseName.ToUpper().Equals(search.ToUpper()))
                                        .ToList();
            }
            if (ProgrammeId != null)
            {
                courses = await _db.Courses.AsNoTracking()
                           .Include(c => c.Level).Include(c => c.Programme).Include(c => c.Semester)
                           .Include(i => i.CoursePrerequisites)
                           .Where(x => x.Programme.ProgrammeId.Equals((int)ProgrammeId)).ToListAsync();
            }
            if (LevelId != null)
            {
                courses = courses.Where(x => x.Level.LevelId.Equals((int)LevelId)).ToList();
            }

            if (SemesterId != null)
            {
                courses = courses.Where(x => x.Semester.SemesterId.Equals((int)SemesterId)).ToList();
            }
            //if (SchoolProgrammeId != null)
            //{
            //    courses = courses.Where(x => x.SchoolProgrammeId.Equals((int)SchoolProgrammeId)).ToList();
            //}
            var data = courses.Select(s => new
            {
                s.CourseId,
                s.Level.LevelName,
                s.CourseName,
                s.CourseType,
                s.CourseCode,
                s.Programme.ProgrammeName,
                s.Semester.SemesterName,
                DeActivatedCourse = s.DeActivatedCourse ? "Is Deactivated": "Is Activated",
                s.Credits,
                Prerequisite = s.CoursePrerequisites.Count(),
            }).ToList();


            totalRecords = data.Count();
            var newData = data.Skip(skip).Take(pageSize).ToList();

            return Json(new { draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = newData },
                JsonRequestBehavior.AllowGet);

            #endregion Server Side filtering

        }

        public async Task<ActionResult> ReActivateCourses(int id)
        {
            string message = string.Empty;

            var course = await _db.Courses.FindAsync(id);

            if (course != null)
            {
                if (course.DeActivatedCourse.Equals(false))
                {
                    course.DeActivatedCourse = true;
                    message = $"{course.CourseName} has been Re-Activated successfully";
                }
                else
                {
                    message = $"{course.CourseName} has already been Activated";
                }
                _db.Entry(course).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return new JsonResult { Data = new { status = true, message } };
            }
            return new JsonResult { Data = new { status = false, message = "Sorry Course was not found" } };

        }
        public async Task<ActionResult> DeActivateCourses(int id)
        {
            string message = string.Empty;

            var course = await _db.Courses.FindAsync(id);

            if (course != null)
            {
                if (course.DeActivatedCourse.Equals(true))
                {
                    course.DeActivatedCourse = false;
                    message = $"{course.CourseName} has been De-Activated successfully";
                }
                else
                {
                    message = $"{course.CourseName} has already been De-Activated";
                }

                _db.Entry(course).State = EntityState.Modified;
                await _db.SaveChangesAsync();

                return new JsonResult { Data = new { status = true, message } };

            }
            return new JsonResult { Data = new { status = false, message = "Sorry Course was not found" } };

        }


        public async Task<ActionResult> Save(int id)
        {
            var course = await _db.Courses.Include(i => i.CoursePrerequisites)
                                //.Include(i => i.SchoolProgramme)
                                //.Include(i => i.Level)
                                //.Include(i => i.Semester)
                                //.Include(i => i.Programme)
                                .Include(i => i.Programme.Department)
                                .AsNoTracking().Where(x => x.CourseId.Equals(id)).FirstOrDefaultAsync();

            //if (!User.IsInRole(RoleName.DAPM_Officer))
            //{
            //    var staffId = userId;
            //    var staffDept = await _db.Staffs.AsNoTracking().Where(x => x.StaffId.Equals(staffId))
            //                        .Select(s => s.DepartmentId).FirstOrDefaultAsync();
            //    var programmes = _db.Programmes.AsNoTracking().Where(x => x.DepartmentId.Equals(staffDept));
            //    var precourse = new List<Course>();
            //    foreach (var item in programmes)
            //    {
            //        precourse.AddRange(_db.Courses.Include(i => i.Programme).AsNoTracking()
            //                            .Where(x => x.Programme.ProgrammeId.Equals(item.ProgrammeId)));
            //    }
            //    ViewBag.DepartmentId = new SelectList(_db.Departments.AsNoTracking().Where(x => x.DepartmentId.Equals(staffDept)), "DepartmentId", "DeptName", course?.Programme.DepartmentId);
            //    ViewBag.ProgrammeId = new SelectList(programmes, "ProgrammeId", "ProgrammeName");
            //    ViewBag.PrerequisiteteCourseId = new MultiSelectList(precourse, "CourseId", "CourseCode", course?.CoursePrerequisites);

            //}
            //else
            //{
            ViewBag.DepartmentId = new SelectList(_db.Departments.AsNoTracking(), "DepartmentId", "DeptName", course?.Programme.DepartmentId);
            ViewBag.ProgrammeId = new SelectList(_db.Programmes.AsNoTracking(), "ProgrammeId", "ProgrammeName");
            ViewBag.PrerequisiteteCourseId = new MultiSelectList(_db.Courses.AsNoTracking(), "CourseId", "CourseCode", course?.CoursePrerequisites);

            //}
            var courseType = from CourseType s in Enum.GetValues(typeof(CourseType))
                             select new { ID = s, Name = s.ToString() };
            var studentType = from StudentType s in Enum.GetValues(typeof(StudentType))
                              select new { ID = s, Name = s.ToString() };
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FancyName", course?.SchoolProgrammeId);
            ViewBag.CourseType = new SelectList(courseType, "Name", "Name", course?.CourseType);
            ViewBag.LevelId = new SelectList(_db.Levels.AsNoTracking(), "LevelId", "LevelName", course?.LevelId);
            ViewBag.SemesterId = new SelectList(_db.Semesters.AsNoTracking(), "SemesterId", "SemesterName", course?.SemesterId);
            if (course != null)
            {
                var model = new CourseVm()
                {
                    CourseCode = course.CourseCode,
                    CourseName = course.CourseName,
                    CourseDescription = course.CourseDescription,
                    CourseType = course.CourseType,
                    Credits = course.Credits,
                    CourseId = course.CourseId,
                    ProgrammeId = (int)course.ProgrammeId,
                    LevelId = (int)course.LevelId,
                };
                return PartialView(model);
            }

            return PartialView();
        }

        //public async Task<ActionResult> Save(int id)
        //{
        //    var course = await _db.Courses.Include(i => i.CoursePrerequisites)
        //                        .Include(i => i.SchoolProgramme)
        //                        .Include(i => i.Level)
        //                        .Include(i => i.Semester)
        //                        .Include(i => i.Programme)
        //                        .Include(i => i.Programme.Department)
        //                        .AsNoTracking()
        //                        .Where(x => x.CourseId.Equals(id))
        //                        .FirstOrDefaultAsync();

        //    if (!User.IsInRole("Admin"))
        //    {
        //        var staffId = userId;
        //        var staffDept = await _db.Staffs.AsNoTracking()
        //                            .Where(x => x.StaffId.Equals(staffId))
        //                            .Select(s => s.DepartmentId)
        //                            .FirstOrDefaultAsync();

        //        var programmes = await _db.Programmes.AsNoTracking()
        //                            .Where(x => x.DepartmentId.Equals(staffDept))
        //                            .ToListAsync(); // Convert to list to modify options

        //        var precourse = new List<Course>();
        //        foreach (var item in programmes)
        //        {
        //            precourse.AddRange(await _db.Courses.Include(i => i.Programme)
        //                                    .AsNoTracking()
        //                                    .Where(x => x.Programme.ProgrammeId.Equals(item.ProgrammeId))
        //                                    .ToListAsync());
        //        }

        //        ViewBag.DepartmentId = new SelectList(_db.Departments.AsNoTracking()
        //                                        .Where(x => x.DepartmentId.Equals(staffDept)),
        //                                        "DepartmentId", "DeptName",
        //                                        course?.Programme.DepartmentId);

        //        Convert programmes to a list and insert a default option
        //       var programmeList = programmes
        //           .Select(p => new SelectListItem { Value = p.ProgrammeId.ToString(), Text = p.ProgrammeName })
        //           .ToList();
        //        programmeList.Insert(0, new SelectListItem { Value = "", Text = "-- Select an option --" });

        //        ViewBag.ProgrammeId = new SelectList(programmeList, "Value", "Text");

        //        ViewBag.PrerequisiteteCourseId = new MultiSelectList(precourse, "CourseId", "CourseCode", course?.CoursePrerequisites);
        //    }
        //    else
        //    {
        //        ViewBag.DepartmentId = new SelectList(_db.Departments.AsNoTracking(), "DepartmentId", "DeptName", course?.Programme.DepartmentId);
        //        ViewBag.ProgrammeId = new SelectList(_db.Programmes.AsNoTracking(), "ProgrammeId", "ProgrammeName");

        //        ViewBag.PrerequisiteteCourseId = new MultiSelectList(_db.Courses.AsNoTracking(), "CourseId", "CourseCode", course?.CoursePrerequisites);
        //    }

        //    var courseType = from CourseType s in Enum.GetValues(typeof(CourseType))
        //                     select new { ID = s, Name = s.ToString() };

        //    var studentType = from StudentType s in Enum.GetValues(typeof(StudentType))
        //                      select new { ID = s, Name = s.ToString() };

        //    ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FancyName", course?.SchoolProgrammeId);
        //    ViewBag.CourseType = new SelectList(courseType, "Name", "Name", course?.CourseType);
        //    ViewBag.LevelId = new SelectList(_db.Levels.AsNoTracking(), "LevelId", "LevelName", course?.LevelId);
        //    ViewBag.SemesterId = new SelectList(_db.Semesters.AsNoTracking(), "SemesterId", "SemesterName", course?.SemesterId);

        //    if (course != null)
        //    {
        //        var model = new CourseVm()
        //        {
        //            CourseCode = course.CourseCode,
        //            CourseName = course.CourseName,
        //            CourseDescription = course.CourseDescription,
        //            CourseType = course.CourseType,
        //            Credits = course.Credits,
        //            CourseId = course.CourseId,
        //            ProgrammeId = (int)course.ProgrammeId,
        //            LevelId = (int)course.LevelId,
        //        };
        //        return PartialView(model);
        //    }

        //    return PartialView();
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Save(CourseVm model)
        {
            bool status = false;
            string message = string.Empty;
            if (ModelState.IsValid)
            {               
                var course = new Course();
                if (model.CourseId > 0)
                {                   
                    course = await _db.Courses.FindAsync(model.CourseId);
                    if (course != null)
                    {
                        course.CourseCode = model.CourseCode;
                        course.CourseName = model.CourseName;
                        course.Credits = model.Credits;
                        course.CourseDescription = model.CourseDescription;
                        course.CourseType = model.CourseType;
                        course.SemesterId = model.SemesterId;
                        course.LevelId = model.LevelId;
                        course.ProgrammeId = model.ProgrammeId;
                        course.SchoolProgrammeId = model.SchoolProgrammeId;

                        _db.Entry(course).State = EntityState.Modified;
                        await _db.SaveChangesAsync();
                        message = $"{course.CourseName} Updated Successfully...";
                    }
                }
                else
                {
                    var checkCourse = _db.Courses.Include(i => i.Programme).AsNoTracking()
                                 .Any(x => x.Programme.ProgrammeId.Equals((int)model.ProgrammeId)
                                 && x.CourseCode.ToUpper().Equals(model.CourseCode.ToUpper()));
                    if (checkCourse)
                    {
                        message = "Sorry, you cant add the same Course Code twice for a department.";
                        return new JsonResult { Data = new { status= false, message } };
                    }
                    course.SchoolProgrammeId = model.SchoolProgrammeId;
                    course.CourseCode = model.CourseCode;
                    course.CourseName = model.CourseName;
                    course.Credits = model.Credits;
                    course.CourseDescription = model.CourseDescription;
                    course.CourseType = model.CourseType;
                    course.SemesterId = model.SemesterId;
                    course.LevelId = model.LevelId;
                    course.ProgrammeId = model.ProgrammeId;
                    _db.Courses.Add(course);
                    _db.SaveChanges();
                    message = $"{model.CourseName} Added Successfully.";
                }

                if(model.PrerequisiteteCourseId != null && model.PrerequisiteteCourseId.Any())
                {
                    var coursePrerequisites = await _db.CoursePrerequisites.AsNoTracking()
                                            .Where(x => x.CourseId.Equals(course.CourseId)).ToListAsync();
                    foreach (var item in coursePrerequisites)
                    {
                        _db.Entry(item).State = EntityState.Deleted;
                    }
                    _db.SaveChanges();

                    foreach (var prerequisite in model.PrerequisiteteCourseId)
                    {
                        var coursePrerequisite = new CoursePrerequisite()
                        {
                            CourseId = course.CourseId,
                            PrerequisiteCourseId = prerequisite,
                            IsSiwes = model.IsSiwes
                        };
                        _db.CoursePrerequisites.Add(coursePrerequisite);
                    }
                    message += $" {model.PrerequisiteteCourseId.Count()} Prerequisite courses added";
                    _db.SaveChanges();
                }               
                return new JsonResult { Data = new { status = true, message } };
            }
            return new JsonResult { Data = new { status, message = "Please complete all required field" } };
            //return View(subject);
        }

        // GET: Courses/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Course course = await _db.Courses.FindAsync(id);
            if (course == null)
            {
                return HttpNotFound();
            }
            return View(course);
        }

        // GET: Courses/Delete/5
        public async Task<PartialViewResult> Delete(int id)
        {
            var course = await _db.Courses.FindAsync(id);
            return PartialView(course);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            bool status = false;
            string message = string.Empty;
            var course = await _db.Courses.FindAsync(id);
            if (course != null)
            {
                _db.Courses.Remove(course);
                await _db.SaveChangesAsync();
                status = true;
                message = $"{course.CourseName} Deleted Successfully...";
                return new JsonResult { Data = new { status, message } };
            }

            return new JsonResult { Data = new { status = false, message = $"{id } not deleted" } };
        }


        public async Task<FileResult> Download(int id)
        {

            CourseUpload courseUpload = await _db.CourseUploads.FindAsync(id);

            string contentType = string.Empty;

            if (courseUpload.FileLocation.Contains(".pdf"))
            {
                contentType = "application/pdf";
            }

            else if (courseUpload.FileLocation.Contains(".docx"))
            {
                contentType = "application/docx";
            }
            return File(courseUpload.FileLocation, contentType, courseUpload.FileLocation);
        }

        public PartialViewResult UploadCourse()
        {
            return PartialView();
        }


        [HttpPost]
        public async Task<ActionResult> UploadCourse(HttpPostedFileBase excelfile)
        {
            if (excelfile == null || excelfile.ContentLength == 0)
            {
                ViewBag.ErrorInfo = "Please Select a excel file <br/>";
                ViewBag.ErrorMessage = "You must select an excel file before you click Upload button";
                return View("ErrorException");
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
                        var courseCode = workSheet.Cells[row, 1].Value.ToString().Trim();
                        var courseName = workSheet.Cells[row, 2].Value.ToString().Trim();
                        var courseDescription = workSheet.Cells[row, 3].Value.ToString().Trim();
                        var courseType = workSheet.Cells[row, 4].Value.ToString().Trim();
                        var credit = Convert.ToInt16(workSheet.Cells[row, 5].Value.ToString().Trim());
                        var semseterName = workSheet.Cells[row, 6].Value.ToString().Trim();
                        var levelName = workSheet.Cells[row, 7].Value.ToString().Trim();
                        var deptOption = workSheet.Cells[row, 8].Value.ToString().Trim();
                        var studentType = workSheet.Cells[row, 9].Value.ToString().Trim();

                        var levelId = await _db.Levels.AsNoTracking()
                                                .Where(x => x.LevelName.ToUpper().Equals(levelName.ToUpper()))
                                                .Select(s => s.LevelId).FirstOrDefaultAsync();
                        var UploadsemesterId = await _db.Semesters.AsNoTracking()
                                                .Where(x => x.SemesterName.ToUpper().Equals(semseterName.ToUpper()))
                                                .Select(s => s.SemesterId).FirstOrDefaultAsync();
                        var programme = await _db.Programmes.Include(i => i.Department).AsNoTracking()
                                                .Where(x => x.ProgrammeCode.ToUpper().Equals(deptOption.ToUpper()))
                                                .FirstOrDefaultAsync();

                        var schoolProgramme = await _db.SchoolProgrammes.AsNoTracking()
                                                .Where(x => x.SchoolProgrammeCode.ToUpper().Equals(studentType.ToUpper()))
                                                .FirstOrDefaultAsync();
                        if (courseType.ToUpper().Equals("CORE"))
                        {
                            courseType = CourseType.Core.ToString();
                        }
                        else if (courseType.ToUpper().Equals("MANDATORY"))
                        {
                            courseType = CourseType.Madatory.ToString();
                        }
                        else if (courseType.ToUpper().Equals("ELECTIVE"))
                        {
                            courseType = CourseType.Elective.ToString();
                        }
                        else
                        {
                            ViewBag.ErrorInfo = "Course type supported is \"Core\", \"Mandatory\" and \"Elective\" ";
                            ViewBag.ErrorMessage = "Please check the course type spelling very well ";
                            return View("ErrorException");
                        }


                        if (schoolProgramme == null)
                        {
                            ViewBag.ErrorInfo = $"The School Programme code uploaded  {studentType} at row {row} is not supported on the portal.";
                            ViewBag.ErrorMessage = "Please check the Student type spelling very well ";
                            return View("ErrorException");
                        }

                        if (programme == null)
                        {
                            ViewBag.ErrorInfo = $"The Department Option code uploaded  {deptOption} at row {row} is not supported on the portal.";
                            ViewBag.ErrorMessage = "Please check the Student type spelling very well ";
                            return View("ErrorException");
                        }

                        var checkCourse = _db.Courses.Include(i => i.Programme).AsNoTracking()
                                            .Any(x => x.Programme.ProgrammeId.Equals((int)programme.ProgrammeId)
                                            && x.CourseCode.ToUpper().Equals(courseCode.ToUpper()));
                        if (checkCourse.Equals(false))
                        {                           
                            try
                            {
                                var course = new Course()
                                {
                                    CourseCode = courseCode,
                                    CourseName = courseName,
                                    CourseDescription = courseDescription,
                                    CourseType = courseType,
                                    Credits = credit,
                                    SemesterId = UploadsemesterId,
                                    LevelId = levelId,
                                    ProgrammeId = programme.ProgrammeId,
                                    SchoolProgrammeId = schoolProgramme.SchoolProgrammeId
                                };

                                _db.Courses.Add(course);
                                recordCount++;
                                lastrecord = $"The last Updated record has the Course code {courseCode} and Course Name {courseName} in {deptOption}";
                            }
                            catch (Exception ex)
                            {
                                ViewBag.ErrorInfo = "The Dept code or Semester Name in the excel doesn't exist";
                                ViewBag.ErrorMessage = ex.Message;
                                return View("ErrorException");
                            }
                        }
                        //else
                        //{
                        //    message = "Sorry, you cant add the same Course Code twice for a department.";
                        //    ViewBag.ErrorInfo = $"Sorry, you cant add the same Course Code twice for a department. " +
                        //                        $"The course code ({courseCode}) for  Department Option code uploaded  {deptOption} at row {row} already exist on the portal.";
                        //    ViewBag.ErrorMessage = "Duplicate Course code for thesame department option ";
                        //    return View("ErrorException");
                        //}                       
                    }
                    await _db.SaveChangesAsync();
                    message = $"You have successfully Uploaded {recordCount} records...  and {lastrecord}";
                    ViewBag.Message = message;

                }
                return RedirectToAction("Index", "Courses", new { message });
            }

            ViewBag.Error = $"File type is Incorrect <br/>";
            return View("Index");
        }



        [HttpGet]
        public ActionResult DeleteDuplicateCourse()
        {
            ViewBag.FacultyId = new SelectList(_db.Faculties.AsNoTracking(), "FacultyId", "FacultyName");
            ViewBag.DepartmentId = new SelectList(_db.Departments.AsNoTracking(), "DepartmentId", "DeptName");
            ViewBag.ProgrammeId = new SelectList(_db.Programmes.AsNoTracking(), "ProgrammeId", "ProgrammeName");
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> DeleteDuplicateCourse(int ProgrammeId)
        {
            var courses = new List<Course>();
            var deptOptionCourses = await _db.Courses.Include(i => i.Programme).AsNoTracking()
                            .Where(x => x.Programme.ProgrammeId.Equals(ProgrammeId)).ToListAsync();

            foreach (var course in deptOptionCourses)
            {
                var courseCount = deptOptionCourses.Where(x => x.CourseCode.Equals(course.CourseCode)).ToList();
                if ( courseCount.Count > 1)
                {
                    courses.Add(courseCount[0]);
                    for (int i = 1; i < courseCount.Count; i++)
                    {
                        _db.Entry(courseCount[i]).State = EntityState.Deleted;
                    }
                }
            }

            await _db.SaveChangesAsync();
            return View(courses);
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
