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
    public class MedResultCategoriesController : BaseController
    {
        public MedResultCategoriesController(SchoolDbContext _db) : base(_db)
        {

        }

        // GET: MedResultCategories
        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> GetIndex()
        {
            var data = await _db.MedResultCategories.Include(m => m.Programme).Include(i => i.Level)
                .AsNoTracking().Select(s => new
            {
                s.MedResultCategoryId,
                s.Programme.ProgrammeName,
                s.Level.LevelName,
                s.PassMark,
                s.CategoryName,
                s.TotalScore
            }).ToListAsync();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        // GET: MedResultCategories/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MedResultCategory medResultCategory = await _db.MedResultCategories.FindAsync(id);
            if (medResultCategory == null)
            {
                return HttpNotFound();
            }
            return View(medResultCategory);
        }


        // GET: MedResultCategories/Create
        public async Task<ActionResult> Save(int id)
        {
            var model = await _db.MedResultCategories.FindAsync(id);
            ViewBag.ProgrammeId = new SelectList(_db.Programmes.Include(i => i.Department.Faculty)
                                                .AsNoTracking().Where(x => x.Department.Faculty.FacultyCode.Equals("MD")), "ProgrammeId", "ProgrammeName", model?.ProgrammeId);
            ViewBag.LevelId = new SelectList(_db.Levels, "LevelId", "LevelName", model?.LevelId);
            return PartialView(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Save(MedResultCategory model)
        {
            var message = string.Empty;
            if (ModelState.IsValid)
            {
                if (model.MedResultCategoryId > 0)
                {
                    var medResultCategory = await _db.MedResultCategories.FindAsync(model.MedResultCategoryId);
                    if (medResultCategory != null)
                    {
                        medResultCategory.LevelId = model.LevelId;
                        medResultCategory.ProgrammeId = model.ProgrammeId;
                        medResultCategory.PassMark = model.PassMark;
                        medResultCategory.TotalScore = model.TotalScore;
                        medResultCategory.CategoryName = model.CategoryName.Trim().ToUpper();
                        _db.Entry(medResultCategory).State = EntityState.Modified;
                        await _db.SaveChangesAsync();
                        message = $"{model.CategoryName} Updated Successfully...";
                        return new JsonResult { Data = new { status = true, message } };
                    }
                }
                else
                {
                    model.CategoryName = model.CategoryName.Trim().ToUpper();
                    _db.MedResultCategories.Add(model);
                    await _db.SaveChangesAsync();
                    message = $"{model.CategoryName} Added Successfully.";
                    return new JsonResult { Data = new { status = true, message } };
                }
            }


            return new JsonResult { Data = new { status = false, message = "Data is not complete" } };
        }

       
        // POST: MedResultCategories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            MedResultCategory medResultCategory = await _db.MedResultCategories.FindAsync(id);
            _db.MedResultCategories.Remove(medResultCategory);
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
