using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampusModel.AddmissionApplicant;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SwiftKampus.Controllers
{
    [Authorize]
    [Audit(AuditingLevel = 2)]
    public class SubjectsController : BaseController
    {

        public SubjectsController(SchoolDbContext db) : base(db)
        {

        }
        // GET: Subjects
        public async Task<ActionResult> Index()
        {
            return View(await _db.Subjects.AsNoTracking().ToListAsync());
        }
        public async Task<ActionResult> GetIndex()
        {
            // dc.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var subject = await _db.Subjects.AsNoTracking().ToListAsync();
            var data = subject.Select(s => new
            {
                s.SubjectId,
                s.CourseName,
                s.CourseCode
            }).ToList();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        public async Task<PartialViewResult> Save(int id)
        {
            var subject = await _db.Subjects.FindAsync(id);
            return PartialView(subject);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Save(Subject model)
        {
            bool status = false;
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                if (model.SubjectId > 0)
                {
                    var subject = await _db.Subjects.FindAsync(model.SubjectId);
                    if (subject != null)
                    {
                        subject.CourseName = model.CourseName;
                        subject.CourseCode = model.CourseCode;
                        _db.Entry(subject).State = EntityState.Modified;
                        await _db.SaveChangesAsync();
                        message = $"{model.CourseName} Updated Successfully...";
                        return new JsonResult { Data = new { status = true, message } };
                    }
                }
                else
                {
                    _db.Subjects.Add(model);
                    await _db.SaveChangesAsync();
                    message = $"{model.CourseName} Added Successfully.";
                    return new JsonResult { Data = new { status = true, message } };
                }
            }
            return new JsonResult { Data = new { status, message } };
            //return View(subject);
        }



        // GET: Subjects/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Subject subject = await _db.Subjects.FindAsync(id);
            if (subject == null)
            {
                return HttpNotFound();
            }
            return View(subject);
        }

        // GET: Subjects/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Subjects/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "SubjectId,CourseCode,CourseName")] Subject subject)
        {
            if (ModelState.IsValid)
            {
                _db.Subjects.Add(subject);
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(subject);
        }

        // GET: Subjects/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Subject subject = await _db.Subjects.FindAsync(id);
            if (subject == null)
            {
                return HttpNotFound();
            }
            return View(subject);
        }

        // POST: Subjects/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "SubjectId,CourseCode,CourseName")] Subject subject)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(subject).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(subject);
        }

        // GET: Subjects/Delete/5
        public async Task<PartialViewResult> Delete(int id)
        {
            var subject = await _db.Subjects.FindAsync(id);
            return PartialView(subject);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            bool status = false;
            string message = string.Empty;
            var subject = await _db.Subjects.FindAsync(id);
            if (subject != null)
            {
                _db.Subjects.Remove(subject);
                await _db.SaveChangesAsync();
                status = true;
                message = "Subject Deleted Successfully...";
                return new JsonResult { Data = new { status, message } };
            }

            return new JsonResult { Data = new { status = false, message = $"{id } not deleted" } };
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
