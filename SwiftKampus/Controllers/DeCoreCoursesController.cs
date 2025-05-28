using System.Data.Entity;
using System.Threading.Tasks;
using System.Net;
using System.Web.Mvc;
using SwiftKampus.Models;
using SwiftKampusModel;
using SwiftKampus.Services;
using System.Linq;
using SwiftKampus.ViewModels;

namespace SwiftKampus.Controllers
{
    [Authorize]
    [Audit(AuditingLevel = 2)]
    public class DeCoreCoursesController : BaseController
    {

        public DeCoreCoursesController(SchoolDbContext db) : base(db)
        {

        }
        // GET: DeCoreCourses
        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> GetIndex()
        {
            var deCourses = await _db.DeCoreCourses.Include(i => i.Course).Include(i => i.Programme).Include(i => i.Level)
                                .AsNoTracking().ToListAsync();
            var data = deCourses.Select(s => new
            {
                s.Programme.ProgrammeName,
                s.Level.LevelName,
                s.Course.CourseCode,
                s.Course.CourseName,
                s.DeCoreCourseId
            }).ToList();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        // GET: DeCoreCourses/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DeCoreCourse deCoreCourse = await _db.DeCoreCourses.FindAsync(id);
            if (deCoreCourse == null)
            {
                return HttpNotFound();
            }
            return View(deCoreCourse);
        }

        // GET: DeCoreCourses/Create
        public async Task<ActionResult> Create(int? id)
        {
            var deCoreCourse = await _db.DeCoreCourses.FindAsync((int)id);
            ViewBag.CourseId = new MultiSelectList(_db.Courses, "CourseId", "CourseCode");
            ViewBag.LevelId = new SelectList(_db.Levels, "LevelId", "LevelName", deCoreCourse?.LevelId);
            ViewBag.ProgrammeId = new SelectList(_db.Programmes, "ProgrammeId", "ProgrammeName", deCoreCourse?.ProgrammeId);
            return View();
        }

        // POST: DeCoreCourses/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(DeCoreCourseCreateVm model)
        {
            if (ModelState.IsValid)
            {
                var ifDeCourseExist = await _db.DeCoreCourses
                                .Where(x => x.ProgrammeId.Equals(model.ProgrammeId) &&
                                x.LevelId.Equals(model.LevelId)).ToListAsync();
                if (ifDeCourseExist.Any())
                {
                    _db.DeCoreCourses.RemoveRange(ifDeCourseExist);
                    _db.SaveChanges();
                }
                foreach (var item in model.CourseId)
                {
                    var deCoreCourse = new DeCoreCourse()
                    {
                        CourseId = item,
                        LevelId = model.LevelId,
                        ProgrammeId = model.ProgrammeId
                    };
                    _db.DeCoreCourses.Add(deCoreCourse);
                }
                await _db.SaveChangesAsync();
                return new JsonResult { Data = new { status = true, message = $"{model.CourseId.Count()} has been saved successfully" } };
            }

            return new JsonResult { Data = new { status = false, message = "Data not valid for processing" } };
        }

        // GET: DeCoreCourses/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DeCoreCourse deCoreCourse = await _db.DeCoreCourses.FindAsync(id);
            if (deCoreCourse == null)
            {
                return HttpNotFound();
            }
            ViewBag.CourseId = new SelectList(_db.Courses, "CourseId", "CourseCode", deCoreCourse.CourseId);
            ViewBag.LevelId = new SelectList(_db.Levels, "LevelId", "LevelName", deCoreCourse.LevelId);
            ViewBag.ProgrammeId = new SelectList(_db.Programmes, "ProgrammeId", "ProgrammeCode", deCoreCourse.ProgrammeId);
            return View(deCoreCourse);
        }

        // POST: DeCoreCourses/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "DeCoreCourseId,ProgrammeId,CourseId,LevelId")] DeCoreCourse deCoreCourse)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(deCoreCourse).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.CourseId = new SelectList(_db.Courses, "CourseId", "CourseCode", deCoreCourse.CourseId);
            ViewBag.LevelId = new SelectList(_db.Levels, "LevelId", "LevelName", deCoreCourse.LevelId);
            ViewBag.ProgrammeId = new SelectList(_db.Programmes, "ProgrammeId", "ProgrammeCode", deCoreCourse.ProgrammeId);
            return View(deCoreCourse);
        }

        // GET: DeCoreCourses/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DeCoreCourse deCoreCourse = await _db.DeCoreCourses.FindAsync(id);
            if (deCoreCourse == null)
            {
                return HttpNotFound();
            }
            return View(deCoreCourse);
        }

        // POST: DeCoreCourses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            DeCoreCourse deCoreCourse = await _db.DeCoreCourses.FindAsync(id);
            _db.DeCoreCourses.Remove(deCoreCourse);
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
