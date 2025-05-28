using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampusModel;
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
    public class SemestersController : BaseController
    {

        public SemestersController(SchoolDbContext db) : base(db)
        {

        }
        // GET: Semesters
        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> GetIndex()
        {
            // dc.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var semester = await _db.Semesters.AsNoTracking().ToListAsync();
            var data = semester.Select(s => new
            {
                s.SemesterId,
                s.SemesterName,           
            }).ToList();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        public async Task<PartialViewResult> Save(int id)
        {
            var semester = await _db.Semesters.FindAsync(id);
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FancyName");

            return PartialView(semester);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Save(Semester model)
        {
            bool status = false;
            string message = string.Empty;
            if (ModelState.IsValid)
            {               
                if (model.SemesterId > 0)
                {
                    var semester = await _db.Semesters.FindAsync(model.SemesterId);
                    if (semester != null)
                    {
                        try
                        {
                            semester.SemesterName = model.SemesterName.ToUpper().Trim();
                            _db.Entry(semester).State = EntityState.Modified;
                            await _db.SaveChangesAsync();
                            message = $"{model.SemesterName} Updated Successfully...";
                            ViewData.Add("ActionMessage", "user Edit semester");
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
                    try
                    {
                        model.SemesterName = model.SemesterName.ToUpper().Trim();
                        _db.Semesters.Add(model);
                        await _db.SaveChangesAsync();
                        message = $"{model.SemesterName} Semester Added Successfully.";
                        ViewData.Add("ActionMessage", "User add a new semester");
                        return new JsonResult { Data = new { status = true, message } };
                    }
                    catch (Exception ex)
                    {
                        return new JsonResult { Data = new { status = false, message = ex.Message } };
                    }
                }
            }
            return new JsonResult { Data = new { status, message } };
            //return View(subject);
        }

        // GET: Semesters/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Semester semester = await _db.Semesters.FindAsync(id);
            if (semester == null)
            {
                return HttpNotFound();
            }
            return View(semester);
        }

        // GET: Semesters/Delete/5
        public async Task<PartialViewResult> Delete(int id)
        {
            var semester = await _db.Semesters.FindAsync(id);
            return PartialView(semester);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            bool status = false;
            string message = string.Empty;
            var semester = await _db.Semesters.FindAsync(id);
            if (semester != null)
            {
                _db.Semesters.Remove(semester);
                await _db.SaveChangesAsync();
                status = true;
                message = "Semester Deleted Successfully...";
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