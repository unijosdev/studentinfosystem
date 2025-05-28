using System.Data.Entity;
using System.Threading.Tasks;
using System.Net;
using System.Web.Mvc;
using SwiftKampus.Models;
using SwiftKampusModel.MedicalScience;
using System.Linq;
using SwiftKampus.Services;

namespace SwiftKampus.Controllers
{
    [Authorize]
    [Audit(AuditingLevel = 2)]
    public class MedResultCasController : BaseController
    {
        public MedResultCasController(SchoolDbContext _db) : base(_db)
        {

        }

        // GET: MedResultCas
        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> GetIndex()
        {
            var data = await _db.MedResultCas.Include(m => m.MedResultCategory).Include(i => i.MedResultCategory.Level)
                .Include(i => i.MedResultCategory.Programme).AsNoTracking().Select(s => new
                {
                    s.MedResultCategory.CategoryName,
                    s.ResultCaName,
                    s.MaximumScore,
                    s.PassMark,
                    s.MedResultCaId,
                    s.MedResultCategory.Programme.ProgrammeName,
                    s.MedResultCategory.Level.LevelName
                }).ToListAsync();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        // GET: MedResultCas/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MedResultCa medResultCa = await _db.MedResultCas.FindAsync(id);
            if (medResultCa == null)
            {
                return HttpNotFound();
            }
            return View(medResultCa);
        }

        public async Task<PartialViewResult> Save(int id)
        {
            var medResultCa = await _db.MedResultCas.FindAsync(id);
            ViewBag.MedResultCategoryId = new SelectList(_db.MedResultCategories, "MedResultCategoryId", "CategoryName", medResultCa?.MedResultCategoryId);
            return PartialView(medResultCa);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Save(MedResultCa model)
        {
            bool status = false;
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                if (model.MedResultCaId > 0)
                {
                    var medResultCa = await _db.MedResultCas.FindAsync(model.MedResultCaId);
                    if (medResultCa != null)
                    {
                        medResultCa.ResultCaName = model.ResultCaName.Trim().ToUpper();
                        medResultCa.PassMark = model.PassMark;
                        medResultCa.MaximumScore = model.MaximumScore;
                        medResultCa.MedResultCategoryId = model.MedResultCategoryId;
                        _db.Entry(medResultCa).State = EntityState.Modified;
                        await _db.SaveChangesAsync();
                        message = $"{model.ResultCaName} Updated Successfully...";
                        return new JsonResult { Data = new { status = true, message } };
                    }
                }
                else
                {
                    _db.MedResultCas.Add(model);
                    await _db.SaveChangesAsync();
                    message = $"{model.ResultCaName} Added Successfully.";
                    return new JsonResult { Data = new { status = true, message } };
                }
            }
            return new JsonResult { Data = new { status, message } };
            //return View(subject);
        }



        // GET: MedResultCas/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MedResultCa medResultCa = await _db.MedResultCas.FindAsync(id);
            if (medResultCa == null)
            {
                return HttpNotFound();
            }
            return View(medResultCa);
        }

        // POST: MedResultCas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            MedResultCa medResultCa = await _db.MedResultCas.FindAsync(id);
            _db.MedResultCas.Remove(medResultCa);
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
