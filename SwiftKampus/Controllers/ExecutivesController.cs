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
    public class ExecutivesController : BaseController
    {

        public ExecutivesController(SchoolDbContext db) : base(db)
        {

        }

        // GET: Executives
        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> GetIndex()
        {
            // dc.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var executives = await _db.Executives.AsNoTracking().Include(e => e.StaffPosition).Include(e => e.Session).Include(e => e.Staff).ToListAsync();

            var data = executives.Select(s => new { s.ExecutiveId, s.Session.SessionName, s.Staff.UserName, s.StaffPosition.PositionName, s.IsActive }).ToList();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        public async Task<PartialViewResult> Save(int id)
        {
            var executive = await _db.Executives.FindAsync(id);
            ViewBag.StaffPositionId = new SelectList(_db.StaffPositions.AsNoTracking().Where(x => x.PositionTypeName.Equals(PositionType.Executive.ToString())), "StaffPositionId", "PositionName");
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            ViewBag.StaffId = new SelectList(_db.Staffs.AsNoTracking(), "StaffId", "UserName");
            return PartialView(executive);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Save(Executive model)
        {
            bool status = false;
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                if (model.IsActive.Equals(true))
                {
                    var current = _db.Executives.AsNoTracking().Where(s => s.IsActive.Equals(true)
                    && s.SessionId.Equals(model.SessionId));
                    if (current.Any())
                    {
                        message = "You cant assign two Staff to a Position in the same Session";
                        return new JsonResult { Data = new { status = false, message } };
                    }
                }
                if (model.ExecutiveId > 0)
                {
                    var executive = await _db.Executives.FindAsync(model.ExecutiveId);
                    if (executive != null)
                    {
                        try
                        {
                            executive.SessionId = model.SessionId;
                            executive.StaffId = model.StaffId;
                            executive.StaffPositionId = model.StaffPositionId;
                            executive.IsActive = model.IsActive;
                            _db.Entry(executive).State = EntityState.Modified;
                            await _db.SaveChangesAsync();
                            message = $"{executive.StaffPosition.PositionName} Updated Successfully...";
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

                    _db.Executives.Add(model);
                    await _db.SaveChangesAsync();
                    message = $"Executive Added Successfully.";
                    return new JsonResult { Data = new { status = true, message } };
                }
            }
            return new JsonResult { Data = new { status, message = "Invalid ModelS" } };
            //return View(subject);
        }


        // GET: Executives/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Executive executive = await _db.Executives.FindAsync(id);
            if (executive == null)
            {
                return HttpNotFound();
            }
            return View(executive);
        }


        // GET: Executives/Delete/5
        public async Task<PartialViewResult> Delete(int id)
        {
            var executive = await _db.Executives.Include(i => i.StaffPosition).FirstOrDefaultAsync(x => x.ExecutiveId.Equals(id));
            return PartialView(executive);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            bool status = false;
            string message = string.Empty;
            var executive = await _db.Executives.FindAsync(id);
            if (executive != null)
            {
                _db.Executives.Remove(executive);
                await _db.SaveChangesAsync();
                status = true;
                message = "Executive Deleted Successfully...";
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
