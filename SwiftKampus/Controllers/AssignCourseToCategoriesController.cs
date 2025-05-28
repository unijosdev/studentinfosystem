using SwiftKampus.Models;
using SwiftKampusModel;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SwiftKampus.Controllers
{
    public class AssignCourseToCategoriesController : BaseController
    {
        public AssignCourseToCategoriesController(SchoolDbContext db) : base(db)
        {

        }
        // GET: AssignCourseToCategories
        public ActionResult Index()
        {
            //var assignCourseToCategories = _db.AssignCourseToCategories.Include(a => a.Course).Include(a => a.CourseCategory);
            return View();
        }

        public async Task<ActionResult> GetIndex()
        {
            // dc.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var data = await _db.AssignCourseToCategories.Include(i => i.Course).Include(i => i.CourseCategory).AsNoTracking()
                .Select(s => new
                {
                    s.CourseCategory.CategoryName,
                    s.Course.CourseCode,
                    s.AssignCourseToCategoryId
                }).ToListAsync();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        public async Task<PartialViewResult> Save(int id)
        {
            var assignCourseToCategory = await _db.AssignCourseToCategories.FindAsync(id);
            ViewBag.CourseId = new SelectList(_db.Courses, "CourseId", "CourseCode", assignCourseToCategory.CourseId);
            ViewBag.CourseCategoryId = new SelectList(_db.CourseCategories, "CourseCategoryId", "CategoryCode", assignCourseToCategory.CourseCategoryId);
            return PartialView(assignCourseToCategory);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Save(AssignCourseToCategory model)
        {
            bool status = false;
            string message = string.Empty;
            if (ModelState.IsValid)
            {

                var assignCourseToCategory = await _db.AssignCourseToCategories.FindAsync(model.AssignCourseToCategoryId);
                if (assignCourseToCategory != null)
                {
                    assignCourseToCategory.CourseCategoryId = model.CourseCategoryId;
                    assignCourseToCategory.CourseId = model.CourseId;
                    _db.Entry(assignCourseToCategory).State = EntityState.Modified;
                    await _db.SaveChangesAsync();
                    message = "Assign Course to Category Updated Successfully...";
                    return new JsonResult { Data = new { status = true, message } };
                }

            }
            return new JsonResult { Data = new { status, message } };
            //return View(subject);
        }


        // GET: AssignCourseToCategories/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AssignCourseToCategory assignCourseToCategory = await _db.AssignCourseToCategories.FindAsync(id);
            if (assignCourseToCategory == null)
            {
                return HttpNotFound();
            }
            return View(assignCourseToCategory);
        }

        // GET: AssignCourseToCategories/Create
        public ActionResult Create()
        {
            ViewBag.CourseId = new MultiSelectList(_db.Courses, "CourseId", "CourseCode");
            ViewBag.CourseCategoryId = new SelectList(_db.CourseCategories, "CourseCategoryId", "CategoryCode");
            return View();
        }

        // POST: AssignCourseToCategories/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(AssignCourseToCategoryVm model)
        {
            if (ModelState.IsValid)
            {
                foreach (var courseId in model.CourseId)
                {
                    var assignCourseToCategory = new AssignCourseToCategory()
                    {
                        CourseCategoryId = model.CourseCategoryId,
                        CourseId = courseId
                    };
                    _db.AssignCourseToCategories.Add(assignCourseToCategory);
                }

                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.CourseId = new SelectList(_db.Courses, "CourseId", "CourseCode", model.CourseId);
            ViewBag.CourseCategoryId = new SelectList(_db.CourseCategories, "CourseCategoryId", "CategoryCode", model.CourseCategoryId);
            return View(model);
        }

        // GET: AssignCourseToCategories/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AssignCourseToCategory assignCourseToCategory = await _db.AssignCourseToCategories.FindAsync(id);
            if (assignCourseToCategory == null)
            {
                return HttpNotFound();
            }
            ViewBag.CourseId = new SelectList(_db.Courses, "CourseId", "CourseCode", assignCourseToCategory.CourseId);
            ViewBag.CourseCategoryId = new SelectList(_db.CourseCategories, "CourseCategoryId", "CategoryCode", assignCourseToCategory.CourseCategoryId);
            return View(assignCourseToCategory);
        }

        // POST: AssignCourseToCategories/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "AssignCourseToCategoryId,CourseId,CourseCategoryId")] AssignCourseToCategory assignCourseToCategory)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(assignCourseToCategory).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.CourseId = new SelectList(_db.Courses, "CourseId", "CourseCode", assignCourseToCategory.CourseId);
            ViewBag.CourseCategoryId = new SelectList(_db.CourseCategories, "CourseCategoryId", "CategoryCode", assignCourseToCategory.CourseCategoryId);
            return View(assignCourseToCategory);
        }

        // GET: AssignCourseToCategories/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AssignCourseToCategory assignCourseToCategory = await _db.AssignCourseToCategories.FindAsync(id);
            if (assignCourseToCategory == null)
            {
                return HttpNotFound();
            }
            return View(assignCourseToCategory);
        }

        // POST: AssignCourseToCategories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            AssignCourseToCategory assignCourseToCategory = await _db.AssignCourseToCategories.FindAsync(id);
            _db.AssignCourseToCategories.Remove(assignCourseToCategory);
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
