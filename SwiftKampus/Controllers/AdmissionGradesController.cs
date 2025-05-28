using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampusModel;
using SwiftKampusModel.AddmissionApplicant;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SwiftKampus.Controllers
{
    [Authorize]
    [Audit(AuditingLevel = 2)]
    public class AdmissionGradesController : BaseController
    {

        public AdmissionGradesController(SchoolDbContext db) : base(db)
        {

        }
        // GET: AdmissionGrades
        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> GetIndex()
        {
            // dc.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var data = await _db.AdmissionGrades.AsNoTracking().Select(s => new
            {
                s.GradeName,
                s.GradePoint,
                s.AdmissionGradeId
            }).ToListAsync();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        public async Task<PartialViewResult> Save(int id)
        {
            var admissionGrade = await _db.AdmissionGrades.FindAsync(id);
            var myGrade = from ResultTypeGrade s in Enum.GetValues(typeof(ResultTypeGrade))
                          select new { ID = s, Name = s.ToString() };

            ViewBag.GradeName = new MultiSelectList(myGrade, "Name", "Name");
            return PartialView(admissionGrade);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Save(AdmissionGrade model)
        {
            bool status = false;
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                if (model.AdmissionGradeId.Equals(true))
                {
                    var current = await _db.AdmissionGrades.AsNoTracking()
                                    .Where(s => s.GradeName.Equals(model.GradeName.ToUpper().Trim()))
                                    .CountAsync();
                    if (current >= 1)
                    {
                        message = "You cant have more than ONE Grade Name saved";
                        return new JsonResult { Data = new { status = false, message } };
                    }
                }
                if (model.AdmissionGradeId > 0)
                {
                    var admissionGrade = await _db.AdmissionGrades.FindAsync(model.AdmissionGradeId);
                    if (admissionGrade != null)
                    {
                        try
                        {
                            admissionGrade.GradeName = model.GradeName.ToUpper().Trim();
                            admissionGrade.GradePoint = model.GradePoint;
                            _db.Entry(admissionGrade).State = EntityState.Modified;
                            await _db.SaveChangesAsync();
                            message = $"{model.GradeName} Updated Successfully...";
                            return new JsonResult { Data = new { status = true, message } };
                        }
                        catch (Exception ex)
                        {
                            return new JsonResult { Data = new { status = false, message = ex.Message } };
                        }
                    }
                }
                else
                {
                    model.GradeName = model.GradeName.ToUpper().Trim();
                    _db.AdmissionGrades.Add(model);
                    await _db.SaveChangesAsync();
                    message = $"{model.GradeName} Added Successfully.";
                    return new JsonResult { Data = new { status = true, message } };
                }
            }
            return new JsonResult { Data = new { status, message } };
            //return View(subject);
        }


        // GET: AdmissionGrades/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AdmissionGrade admissionGrade = await _db.AdmissionGrades.FindAsync(id);
            if (admissionGrade == null)
            {
                return HttpNotFound();
            }
            return View(admissionGrade);
        }

        // GET: AdmissionGrades/Create
        public ActionResult Create()
        {
            var myGrade = from ResultTypeGrade s in Enum.GetValues(typeof(ResultTypeGrade))
                          select new { ID = s, Name = s.ToString() };

            ViewBag.GradeName = new MultiSelectList(myGrade, "Name", "Name");
            return View();
        }

        // POST: AdmissionGrades/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(AdmissionGrade admissionGrade)
        {
            if (ModelState.IsValid)
            {
                _db.AdmissionGrades.Add(admissionGrade);
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(admissionGrade);
        }

        // GET: AdmissionGrades/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AdmissionGrade admissionGrade = await _db.AdmissionGrades.FindAsync(id);
            if (admissionGrade == null)
            {
                return HttpNotFound();
            }
            return View(admissionGrade);
        }

        // POST: AdmissionGrades/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "AdmissionGradeId,GradeName,GradePoint")] AdmissionGrade admissionGrade)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(admissionGrade).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(admissionGrade);
        }

        // GET: AdmissionGrades/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AdmissionGrade admissionGrade = await _db.AdmissionGrades.FindAsync(id);
            if (admissionGrade == null)
            {
                return HttpNotFound();
            }
            return View(admissionGrade);
        }

        // POST: AdmissionGrades/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            AdmissionGrade admissionGrade = await _db.AdmissionGrades.FindAsync(id);
            _db.AdmissionGrades.Remove(admissionGrade);
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
