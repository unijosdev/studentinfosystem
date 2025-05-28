using Microsoft.Ajax.Utilities;
using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampus.ViewModels;
using SwiftKampusModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SwiftKampus.Controllers
{
    [Authorize]
    [Audit(AuditingLevel = 2)]
    public class CoursePrerequisitesController : BaseController
    {

        public CoursePrerequisitesController(SchoolDbContext db) : base(db)
        {

        }

        // GET: CoursePrerequisites
        public async Task<ActionResult> Index()
        {
            if (!User.IsInRole("Admin"))
            {
                var staffId = userId;
                var staffDept = await _db.Staffs.AsNoTracking().Where(x => x.StaffId.Equals(staffId))
                                    .Select(s => s.DepartmentId).FirstOrDefaultAsync();
                ViewBag.ProgrammeId = new SelectList(_db.Programmes.AsNoTracking()
                    .Where(x => x.DepartmentId.Equals(staffDept)), "ProgrammeId", "ProgrammeName");
            }
            else
            {
                ViewBag.ProgrammeId = new SelectList(_db.Programmes.AsNoTracking(), "ProgrammeId", "ProgrammeName");
            }
            ViewBag.LevelId = new SelectList(_db.Levels.AsNoTracking(), "LevelId", "LevelName");
            ViewBag.SemesterId = new SelectList(_db.Semesters.AsNoTracking(), "SemesterId", "SemesterName");
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FancyName");

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

            var model = new List<PrerequisiteIndexVm>();
            var coursePrerequisites = _db.CoursePrerequisites.AsNoTracking()
                                        .DistinctBy(d => d.CourseId).ToList();
            foreach (var courseId in coursePrerequisites)
            {
                var builder = new StringBuilder();
                var mainCourse = await _db.Courses.Include(i => i.Level).Include(i => i.Semester)
                                    .Include(i => i.SchoolProgramme).Include(i => i.Programme).AsNoTracking()
                                    .Where(i => i.CourseId.Equals(courseId.CourseId)).FirstOrDefaultAsync();
                var searchCourse = await _db.CoursePrerequisites.AsNoTracking().Where(x => x.CourseId.Equals(courseId.CourseId))
                                    .ToListAsync();
                foreach (var item in searchCourse)
                {
                    var preCourse = await _db.Courses.FindAsync(item.PrerequisiteCourseId);
                    builder.Append(preCourse?.CourseCode);
                    builder.Append(", ");
                }
                var indexVm = new PrerequisiteIndexVm()
                {
                    MainCourse = mainCourse,
                    PrerequisiteCourse = builder.ToString(),
                    Level = mainCourse.Level,
                    Semester = mainCourse.Semester,
                    SchoolProgramme = mainCourse.SchoolProgramme,
                    Programme = mainCourse.Programme
                };
                model.Add(indexVm);
            }

            if (SchoolProgrammeId != null)
            {
                model = model.Where(x => x.SchoolProgramme.SchoolProgrammeId.Equals((int)SchoolProgrammeId)).ToList();
            }

            if (ProgrammeId != null)
            {
                model = model.Where(x => x.Programme.ProgrammeId.Equals((int)ProgrammeId)).ToList();

            }

            if (LevelId != null)
            {
                model = model.Where(x => x.Level.LevelId.Equals((int)LevelId)).ToList();

            }

            if (SemesterId != null)
            {
                model = model.Where(x => x.Semester.SemesterId.Equals((int)SemesterId)).ToList();
            }

            var data = model.Select(s => new
            {
                s.Level.LevelName,
                s.MainCourse.CourseName,
                s.MainCourse.CourseType,
                s.MainCourse.CourseCode,
                s.Programme.ProgrammeName,
                s.Semester.SemesterName,
                s.MainCourse.Credits,
                s.PrerequisiteCourse
            }).ToList();


            totalRecords = data.Count();
            var newData = data.Skip(skip).Take(pageSize).ToList();

            return Json(new { draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = newData },
                JsonRequestBehavior.AllowGet);

            #endregion Server Side filtering

        }


        // GET: CoursePrerequisites/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CoursePrerequisite coursePrerequisite = await _db.CoursePrerequisites.FindAsync(id);
            if (coursePrerequisite == null)
            {
                return HttpNotFound();
            }
            return View(coursePrerequisite);
        }

        // GET: CoursePrerequisites/Create
        public async Task<ActionResult> Create()
        {
            if (!User.IsInRole("Admin"))
            {
                var staffId = userId;
                var staffDept = await _db.Staffs.AsNoTracking().Where(x => x.StaffId.Equals(staffId))
                                    .Select(s => s.DepartmentId).FirstOrDefaultAsync();
                ViewBag.CourseId = new SelectList(_db.Courses.Include(i => i.Programme.Department).AsNoTracking()
                                                        .Where(x => x.Programme.Department.DepartmentId.Equals(staffDept)),
                                                        "CourseId", "CourseCode");
                ViewBag.PrerequisiteCourseId = new MultiSelectList(_db.Courses.Include(i => i.Programme.Department).AsNoTracking()
                                                    .Where(x => x.Programme.Department.DepartmentId.Equals(staffDept)),
                                                    "CourseId", "CourseCode");

            }
            else
            {
                ViewBag.CourseId = new SelectList(_db.Courses.AsNoTracking(), "CourseId", "CourseCode");
                ViewBag.PrerequisiteCourseId = new MultiSelectList(_db.Courses.AsNoTracking(), "CourseId", "CourseCode");

            }

            return View();
        }

        // POST: CoursePrerequisites/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CoursePrerequisiteVm model)
        {
            if (ModelState.IsValid)
            {
                var ifPreCourseExist = await _db.CoursePrerequisites.AsNoTracking()
                                .Where(x => x.CourseId.Equals(model.CourseId)).ToListAsync();
                if (ifPreCourseExist.Count > 0)
                {
                    _db.CoursePrerequisites.RemoveRange(ifPreCourseExist);
                    _db.SaveChanges();
                }
                foreach (var item in model.PrerequisiteCourseId)
                {
                    var coursePrerequisite = new CoursePrerequisite()
                    {
                        CourseId = model.CourseId,
                        PrerequisiteCourseId = item,
                        IsSiwes = model.IsSiwes
                    };
                    _db.CoursePrerequisites.Add(coursePrerequisite);
                }
                await _db.SaveChangesAsync();
                return new JsonResult { Data = new { status = true, message = $"{model.PrerequisiteCourseId.Count()} has been saved successfully" } };
            }

            return new JsonResult { Data = new { status = false, message = "Data not valid for processing" } };
        }

        // GET: CoursePrerequisites/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CoursePrerequisite coursePrerequisite = await _db.CoursePrerequisites.FindAsync(id);
            if (coursePrerequisite == null)
            {
                return HttpNotFound();
            }
            return View(coursePrerequisite);
        }

        // POST: CoursePrerequisites/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "CoursePrerequisiteId,CourseId,PrerequisiteCourseId")] CoursePrerequisite coursePrerequisite)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(coursePrerequisite).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(coursePrerequisite);
        }

        // GET: CoursePrerequisites/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CoursePrerequisite coursePrerequisite = await _db.CoursePrerequisites.FindAsync(id);
            if (coursePrerequisite == null)
            {
                return HttpNotFound();
            }
            return View(coursePrerequisite);
        }

        // POST: CoursePrerequisites/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            CoursePrerequisite coursePrerequisite = await _db.CoursePrerequisites.FindAsync(id);
            _db.CoursePrerequisites.Remove(coursePrerequisite);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
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
