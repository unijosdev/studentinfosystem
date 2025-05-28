using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampusModel;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SwiftKampus.Controllers
{
    [Authorize]
    [Audit(AuditingLevel = 2)]
    public class ClassDegreesController : BaseController
    {
        public ClassDegreesController(SchoolDbContext db) : base(db)
        {

        }
        // GET: ClassDegrees
        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> GetIndex()
        {
            // dc.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var data = await _db.ClassDegrees.Include(i => i.SchoolProgramme).AsNoTracking().Select(s => new
            {
                s.SchoolProgramme.FancyName,
                s.ClassDegreeId,
                s.DegreeName,
                s.MaximumValue,
                s.MinimumValue,
                s.Remark
            }).ToListAsync();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        public async Task<PartialViewResult> Save(int id)
        {
            var classDegree = await _db.ClassDegrees.FindAsync(id);
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FancyName");

            return PartialView(classDegree);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Save(ClassDegree model)
        {
            bool status = false;
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                if (model.ClassDegreeId > 0)
                {
                    var classDegree = await _db.ClassDegrees.FindAsync(model.ClassDegreeId);
                    if (classDegree != null)
                    {
                        classDegree.SchoolProgrammeId = model.SchoolProgrammeId;
                        classDegree.DegreeName = model.DegreeName;
                        classDegree.MaximumValue = model.MaximumValue;
                        classDegree.MinimumValue = model.MinimumValue;
                        classDegree.Remark = model.Remark;
                        _db.Entry(classDegree).State = EntityState.Modified;
                        await _db.SaveChangesAsync();
                        message = $"{model.DegreeName} Updated Successfully...";
                        return new JsonResult { Data = new { status = true, message } };
                    }
                }
                else
                {
                    _db.ClassDegrees.Add(model);
                    await _db.SaveChangesAsync();
                    message = $"{model.DegreeName} Added Successfully.";
                    return new JsonResult { Data = new { status = true, message } };
                }
            }
            return new JsonResult { Data = new { status, message } };
            //return View(subject);
        }

        // GET: ClassDegrees/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ClassDegree classDegree = await _db.ClassDegrees.FindAsync(id);
            if (classDegree == null)
            {
                return HttpNotFound();
            }
            return View(classDegree);
        }

        // GET: ClassDegrees/Create
        public ActionResult Create()
        {
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes, "SchoolProgrammeId", "ProgrammeType");
            return View();
        }

        // POST: ClassDegrees/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "ClassDegreeId,SchoolProgrammeId,DegreeName,MinimumValue,MaximumValue,Remark")] ClassDegree classDegree)
        {
            if (ModelState.IsValid)
            {
                _db.ClassDegrees.Add(classDegree);
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes, "SchoolProgrammeId", "ProgrammeType", classDegree.SchoolProgrammeId);
            return View(classDegree);
        }

        // GET: ClassDegrees/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ClassDegree classDegree = await _db.ClassDegrees.FindAsync(id);
            if (classDegree == null)
            {
                return HttpNotFound();
            }
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes, "SchoolProgrammeId", "ProgrammeType", classDegree.SchoolProgrammeId);
            return View(classDegree);
        }

        // POST: ClassDegrees/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "ClassDegreeId,SchoolProgrammeId,DegreeName,MinimumValue,MaximumValue,Remark")] ClassDegree classDegree)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(classDegree).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes, "SchoolProgrammeId", "ProgrammeType", classDegree.SchoolProgrammeId);
            return View(classDegree);
        }

        // GET: ClassDegrees/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ClassDegree classDegree = await _db.ClassDegrees.FindAsync(id);
            if (classDegree == null)
            {
                return HttpNotFound();
            }
            return View(classDegree);
        }

        // POST: ClassDegrees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            ClassDegree classDegree = await _db.ClassDegrees.FindAsync(id);
            _db.ClassDegrees.Remove(classDegree);
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
