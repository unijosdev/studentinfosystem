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
    public class FacultiesController : BaseController
    {

        public FacultiesController(SchoolDbContext db) : base(db)
        {

        }

        // GET: Faculties
        public ActionResult Index()
        {
            //ViewBag.Message = message;
            //return View(await _db.Faculties.AsNoTracking().ToListAsync());
            return View();
        }


        public async Task<ActionResult> GetIndex()
        {
            // dc.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var data = await _db.Faculties.AsNoTracking().Select(s => new { s.FacultyId, s.FacultyCode, s.FacultyName }).ToListAsync();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        public async Task<PartialViewResult> Save(int id)
        {
            var assignSubject = await _db.Faculties.FindAsync(id);
            return PartialView(assignSubject);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Save(Faculty model)
        {
            bool status = false;
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                if (model.FacultyId > 0)
                {
                    var faculty = await _db.Faculties.FindAsync(model.FacultyId);
                    if (faculty != null)
                    {
                        faculty.FacultyCode = model.FacultyCode;
                        faculty.FacultyName = model.FacultyName;
                        _db.Entry(faculty).State = EntityState.Modified;
                        await _db.SaveChangesAsync();
                        message = "Faculty Updated Successfully...";
                        return new JsonResult { Data = new { status = true, message } };
                    }
                }
                else
                {
                    _db.Faculties.Add(model);
                    await _db.SaveChangesAsync();
                    message = $"{model.FacultyName} Faculty Added Successfully.";
                    return new JsonResult { Data = new { status = true, message } };
                }
            }
            return new JsonResult { Data = new { status, message = "Please enter the data required Correctly" } };
            //return View(subject);
        }

        // GET: Faculties/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Faculty faculty = await _db.Faculties.FindAsync(id);
            if (faculty == null)
            {
                return HttpNotFound();
            }
            return View(faculty);
        }

        public async Task<PartialViewResult> Delete(int id)
        {
            var assignSubject = await _db.Faculties.FindAsync(id);
            return PartialView(assignSubject);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            bool status = false;
            string message = string.Empty;
            var faculty = await _db.Faculties.FindAsync(id);
            if (faculty != null)
            {
                _db.Faculties.Remove(faculty);
                await _db.SaveChangesAsync();
                status = true;
                message = "Faculty Deleted Successfully...";
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