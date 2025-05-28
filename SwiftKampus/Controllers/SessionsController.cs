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
    public class SessionsController : BaseController
    {

        public SessionsController(SchoolDbContext db) : base(db)
        {

        }
        //// GET: Sessions
        //public async Task<ActionResult> Index()
        //{
        //    return View(await _db.Sessions.AsNoTracking().OrderBy(s => s.SessionName).ToListAsync());
        //    //var orderedList = cnt.OrderBy(x => x.GroupTypeId).ThenBy(x => x.ContentId);
        //}
        // GET: Sessions
        public ActionResult Index()
        {
            return View();
            //var orderedList = cnt.OrderBy(x => x.GroupTypeId).ThenBy(x => x.ContentId);
        }

        public async Task<ActionResult> GetIndex()
        {
            // dc.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var session = await _db.Sessions.AsNoTracking().ToListAsync();
            var data = session.Select(s => new
            {
                s.SessionId,
                s.SessionName,
                StartDate = s.StartDate.ToString("dd MMM yyyy"),
                EndDate = s.EndDate.ToString("dd MMM yyyy"),
            }).ToList();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        public async Task<PartialViewResult> Save(int id)
        {
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FancyName");
            var session = await _db.Sessions.FindAsync(id);
            return PartialView(session);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Save(Session model)
        {
            bool status = false;
            string message = string.Empty;
            string[] ssizes = model.SessionName.Trim().Split('-', '/');
            int yearOne = Convert.ToInt16(ssizes[0]);
            int yearTwo = Convert.ToInt16(ssizes[1]);
            if (yearTwo - yearOne > 1)
            {
                return new JsonResult { Data = new { status = false, message = "Interval between session can only be one year" } };
            }
            var startYear = model.StartDate.Year;
            var endYear = model.EndDate.Year;
            if (endYear - startYear > 1)
            {
                return new JsonResult { Data = new { status = false, message = "Interval between Start Date and End Date cannot be greater than one year" } };
            }
            if (endYear - startYear < 1)
            {
                return new JsonResult { Data = new { status = false, message = "Interval between Start Date and End Date cannot be Less than one year" } };
            }
            if (ModelState.IsValid)
            {                
                if (model.SessionId > 0)
                {
                    var session = await _db.Sessions.FindAsync(model.SessionId);
                    if (session != null)
                    {
                        try
                        {
                            session.SessionName = model.SessionName.Trim();
                            session.StartDate = model.StartDate.Date;
                            session.EndDate = model.EndDate.Date;
                            _db.Entry(session).State = System.Data.Entity.EntityState.Modified;
                            await _db.SaveChangesAsync();
                            message = $"{model.SessionName} Updated Successfully...";
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
                    model.StartDate = model.StartDate.Date;
                    model.EndDate = model.EndDate.Date;
                    _db.Sessions.Add(model);
                    await _db.SaveChangesAsync();
                    message = $"{model.SessionName} Added Successfully.";
                    return new JsonResult { Data = new { status = true, message } };
                }
            }
            return new JsonResult { Data = new { status, message } };
            //return View(subject);
        }

        // GET: Sessions/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Session session = await _db.Sessions.FindAsync(id);
            if (session == null)
            {
                return HttpNotFound();
            }
            return View(session);
        }

        // GET: Sessions/Delete/5
        public async Task<PartialViewResult> Delete(int id)
        {
            var session = await _db.Sessions.FindAsync(id);
            return PartialView(session);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            bool status = false;
            string message = string.Empty;
            var session = await _db.Sessions.FindAsync(id);
            if (session != null)
            {
                _db.Sessions.Remove(session);
                await _db.SaveChangesAsync();
                status = true;
                message = "Session Deleted Successfully...";
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