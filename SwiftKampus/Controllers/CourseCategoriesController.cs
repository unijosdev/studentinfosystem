using SwiftKampus.Models;
using SwiftKampusModel;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SwiftKampus.Controllers
{
    public class CourseCategoriesController : BaseController
    {
        public CourseCategoriesController(SchoolDbContext db) : base(db)
        {

        }
        // GET: CourseCategories
        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> GetIndex()
        {
            // dc.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var data = await _db.CourseCategories.Include(i => i.Department).AsNoTracking().Select(s => new
            {
                s.CourseCategoryId,
                s.CategoryCode,
                s.CategoryName,
                s.Department.DeptName,
                NoOfCourse = s.AssignCourseToCategories.Count(x => x.CourseCategoryId.Equals(s.CourseCategoryId))

            }).ToListAsync();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }


        public async Task<PartialViewResult> Save(int id)
        {
            var courseCategory = await _db.CourseCategories.FindAsync(id);
            ViewBag.DepartmentId = new SelectList(_db.Departments.AsNoTracking(), "DepartmentId", "DeptName");

            return PartialView(courseCategory);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Save(CourseCategory model)
        {
            bool status = false;
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                if (model.CourseCategoryId > 0)
                {
                    var courseCategory = await _db.CourseCategories.FindAsync(model.CourseCategoryId);
                    if (courseCategory != null)
                    {
                        courseCategory.CategoryCode = model.CategoryCode;
                        courseCategory.CategoryName = model.CategoryName;
                        _db.Entry(courseCategory).State = EntityState.Modified;
                        await _db.SaveChangesAsync();
                        message = $"{model.CategoryName} Updated Successfully...";
                        return new JsonResult { Data = new { status = true, message } };

                    }
                }
                else
                {
                    _db.CourseCategories.Add(model);
                    await _db.SaveChangesAsync();
                    message = $"{model.CategoryName} Added Successfully.";
                    return new JsonResult { Data = new { status = true, message } };
                }
            }
            return new JsonResult { Data = new { status, message } };
            //return View(subject);
        }



        #region Create and edit scaffolded codes
        //// GET: CourseCategories/Create
        //public ActionResult Create()
        //{
        //    return View();
        //}

        //// POST: CourseCategories/Create
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Create([Bind(Include = "CourseCategoryId,CategoryCode,CategoryName")] CourseCategory courseCategory)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _db.CourseCategories.Add(courseCategory);
        //        await _db.SaveChangesAsync();
        //        return RedirectToAction("Index");
        //    }

        //    return View(courseCategory);
        //}

        //// GET: CourseCategories/Edit/5
        //public async Task<ActionResult> Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    CourseCategory courseCategory = await _db.CourseCategories.FindAsync(id);
        //    if (courseCategory == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(courseCategory);
        //}

        //// POST: CourseCategories/Edit/5
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Edit([Bind(Include = "CourseCategoryId,CategoryCode,CategoryName")] CourseCategory courseCategory)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _db.Entry(courseCategory).State = EntityState.Modified;
        //        await _db.SaveChangesAsync();
        //        return RedirectToAction("Index");
        //    }
        //    return View(courseCategory);
        //} 
        #endregion

        // GET: CourseCategories/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CourseCategory courseCategory = await _db.CourseCategories.FindAsync(id);
            if (courseCategory == null)
            {
                return HttpNotFound();
            }
            return View(courseCategory);
        }

        // POST: CourseCategories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            CourseCategory courseCategory = await _db.CourseCategories.FindAsync(id);
            _db.CourseCategories.Remove(courseCategory);
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
