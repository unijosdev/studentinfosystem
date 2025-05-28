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
    public class DeptPositionsController : BaseController
    {

        public DeptPositionsController(SchoolDbContext db) : base(db)
        {

        }

        // GET: DeptPositions
        public ActionResult Index()
        {
            //var deptPositions = _db.DeptPositions.AsNoTracking().Include(d => d.Department).Include(d => d.StaffPosition).Include(d => d.Session).Include(d => d.Staff);
            //return View(await deptPositions.ToListAsync());
            return View();
        }


        public async Task<ActionResult> GetIndex()
        {
            // dc.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var deptPosition = await _db.DeptPositions.AsNoTracking().Include(e => e.StaffPosition).Include(e => e.Session).Include(e => e.Staff).ToListAsync();

            var data = deptPosition.Select(s => new { s.DeptPositionId, s.Session.SessionName, s.Staff.UserName, s.StaffPosition.PositionName, s.IsActive }).ToList();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }



        public async Task<PartialViewResult> Save(int id)
        {
            var deptPosition = await _db.DeptPositions.FindAsync(id);
            var positions = _db.StaffPositions.AsNoTracking().Where(x => x.PositionTypeName.Equals(PositionType.Department.ToString())
                                                                    && x.PositionTypeName.Equals(PositionType.Administrative.ToString()));
            ViewBag.StaffPositionId = new SelectList(positions, "StaffPositionId", "PositionName");
            ViewBag.DepartmentId = new SelectList(_db.Departments.AsNoTracking(), "DepartmentId", "DeptName");
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            ViewBag.StaffId = new SelectList(_db.Staffs.AsNoTracking(), "StaffId", "UserName");
            return PartialView(deptPosition);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Save(DeptPosition model)
        {
            bool status = false;
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                if (model.IsActive.Equals(true))
                {
                    var current = _db.DeptPositions.AsNoTracking().Where(s => s.IsActive.Equals(true)
                                                                                 && s.SessionId.Equals(model.SessionId));
                    if (current.Any())
                    {
                        message = "You cant assign two Staff to a Position in the same Session";
                        return new JsonResult { Data = new { status = false, message } };
                    }
                }
                if (model.DeptPositionId > 0)
                {
                    var deptPosition = await _db.DeptPositions.FindAsync(model.DeptPositionId);
                    if (deptPosition != null)
                    {
                        try
                        {
                            deptPosition.SessionId = model.SessionId;
                            deptPosition.StaffId = model.StaffId;
                            deptPosition.StaffPositionId = model.StaffPositionId;
                            deptPosition.IsActive = model.IsActive;
                            deptPosition.DepartmentId = model.DepartmentId;
                            _db.Entry(deptPosition).State = EntityState.Modified;
                            await _db.SaveChangesAsync();
                            message = $"{deptPosition.StaffPosition.PositionName} Updated Successfully...";
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

                    _db.DeptPositions.Add(model);
                    await _db.SaveChangesAsync();
                    message = $"Executive Added Successfully.";
                    return new JsonResult { Data = new { status = true, message } };
                }
            }
            return new JsonResult { Data = new { status, message } };
            //return View(subject);
        }



        // GET: DeptPositions/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DeptPosition deptPosition = await _db.DeptPositions.FindAsync(id);
            if (deptPosition == null)
            {
                return HttpNotFound();
            }
            return View(deptPosition);
        }



        // GET: DeptPositions/Delete/5
        public async Task<PartialViewResult> Delete(int id)
        {
            var deptPosition = await _db.DeptPositions.Include(i => i.StaffPosition).FirstOrDefaultAsync(x => x.DeptPositionId.Equals(id));
            return PartialView(deptPosition);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            bool status = false;
            string message = string.Empty;
            var deptPosition = await _db.DeptPositions.FindAsync(id);
            if (deptPosition != null)
            {
                _db.DeptPositions.Remove(deptPosition);
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
