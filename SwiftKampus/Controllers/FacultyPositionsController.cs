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
    public class FacultyPositionsController : BaseController
    {

        public FacultyPositionsController(SchoolDbContext db) : base(db)
        {

        }

        // GET: FacultyPositions
        public ActionResult Index()
        {
            //var facultyPositions = _db.FacultyPositions.AsNoTracking().Include(f => f.Faculty).Include(f => f.StaffPosition).Include(f => f.Session).Include(f => f.Staff);
            //return View(await facultyPositions.ToListAsync());
            return View();

        }

        public async Task<ActionResult> GetIndex()
        {
            // dc.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var facultyPositions = await _db.FacultyPositions.AsNoTracking().Include(e => e.StaffPosition).Include(e => e.Session).Include(e => e.Staff).ToListAsync();

            var data = facultyPositions.Select(s => new { s.FacultyPositionId, s.Session.SessionName, s.Staff.UserName, s.StaffPosition.PositionName, s.IsActive }).ToList();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }


        public async Task<PartialViewResult> Save(int id)
        {
            var facultyPositions = await _db.FacultyPositions.FindAsync(id);
            ViewBag.FacultyId = new SelectList(_db.Faculties.AsNoTracking(), "FacultyId", "FacultyName");
            var positions = _db.StaffPositions.AsNoTracking().Where(x => x.PositionTypeName.Equals(PositionType.Faculty.ToString())
                                        && x.PositionTypeName.Equals(PositionType.Administrative.ToString()));
            ViewBag.StaffPositionId = new SelectList(positions, "StaffPositionId", "PositionName");
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            ViewBag.StaffId = new SelectList(_db.Staffs.AsNoTracking(), "StaffId", "UserName");
            return PartialView(facultyPositions);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Save(FacultyPosition model)
        {
            bool status = false;
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                if (model.IsActive.Equals(true))
                {
                    var current = _db.FacultyPositions.AsNoTracking().Where(s => s.IsActive.Equals(true)
                                                                           && s.SessionId.Equals(model.SessionId));
                    if (current.Any())
                    {
                        message = "You cant assign two Staff to a Position in the same Session";
                        return new JsonResult { Data = new { status = false, message } };
                    }
                }
                if (model.FacultyPositionId > 0)
                {
                    var facultyPositions = await _db.FacultyPositions.FindAsync(model.FacultyPositionId);
                    if (facultyPositions != null)
                    {
                        try
                        {
                            facultyPositions.SessionId = model.SessionId;
                            facultyPositions.StaffId = model.StaffId;
                            facultyPositions.StaffPositionId = model.StaffPositionId;
                            facultyPositions.IsActive = model.IsActive;
                            facultyPositions.FacultyId = model.FacultyId;
                            _db.Entry(facultyPositions).State = EntityState.Modified;
                            await _db.SaveChangesAsync();
                            message = $"{facultyPositions.StaffPosition.PositionName} Updated Successfully...";
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

                    _db.FacultyPositions.Add(model);
                    await _db.SaveChangesAsync();
                    message = $"Executive Added Successfully.";
                    return new JsonResult { Data = new { status = true, message } };
                }
            }
            return new JsonResult { Data = new { status, message } };
            //return View(subject);
        }



        // GET: FacultyPositions/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FacultyPosition facultyPosition = await _db.FacultyPositions.FindAsync(id);
            if (facultyPosition == null)
            {
                return HttpNotFound();
            }
            return View(facultyPosition);
        }


        // GET: FacultyPositions/Delete/5
        public async Task<PartialViewResult> Delete(int id)
        {
            var facultyPosition = await _db.FacultyPositions.Include(i => i.StaffPosition).FirstOrDefaultAsync(x => x.FacultyPositionId.Equals(id));
            return PartialView(facultyPosition);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            bool status = false;
            string message = string.Empty;
            var facultyPosition = await _db.FacultyPositions.FindAsync(id);
            if (facultyPosition != null)
            {
                _db.FacultyPositions.Remove(facultyPosition);
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
