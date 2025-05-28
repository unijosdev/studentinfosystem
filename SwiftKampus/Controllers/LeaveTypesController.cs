using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampusModel.Employee.Leave;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SwiftKampus.Controllers
{
    [Authorize]
    [Audit(AuditingLevel = 2)]
    public class LeaveTypesController : BaseController
    {

        public LeaveTypesController(SchoolDbContext db) : base(db)
        {

        }
        // GET: LeaveTypes
        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> GetIndex()
        {
            // dc.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var data = await _db.LeaveTypes.AsNoTracking().Select(s => new
            {
                s.LeaveTypeId,
                s.LeaveTypeName,
                s.MaximumLeaveCount,
                s.IsActive,
                s.LeaveTime
            }).ToListAsync();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        public async Task<PartialViewResult> Save(int id)
        {
            var leaveType = await _db.LeaveTypes.FindAsync(id);
            return PartialView(leaveType);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Save(LeaveType model)
        {
            bool status = false;
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                if (model.LeaveTypeId > 0)
                {
                    var leaveType = await _db.LeaveTypes.FindAsync(model.LeaveTypeId);
                    if (leaveType != null)
                    {
                        leaveType.LeaveTypeId = model.LeaveTypeId;
                        leaveType.LeaveTypeName = model.LeaveTypeName;
                        leaveType.MaximumLeaveCount = model.MaximumLeaveCount;
                        leaveType.EbanbleCarryForward = model.EbanbleCarryForward;
                        leaveType.IsActive = model.IsActive;
                        leaveType.LeaveTime = model.LeaveTime;
                        _db.Entry(leaveType).State = EntityState.Modified;
                        await _db.SaveChangesAsync();
                        message = $"{model.LeaveTypeName} Updated Successfully...";
                        return new JsonResult { Data = new { status = true, message } };
                    }
                }
                else
                {
                    _db.LeaveTypes.Add(model);
                    await _db.SaveChangesAsync();
                    message = $"{model.LeaveTypeName} Added Successfully.";
                    return new JsonResult { Data = new { status = true, message } };
                }
            }
            return new JsonResult { Data = new { status, message } };
            //return View(subject);
        }


        // GET: LeaveTypes/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LeaveType leaveType = await _db.LeaveTypes.FindAsync(id);
            if (leaveType == null)
            {
                return HttpNotFound();
            }
            return View(leaveType);
        }

        // GET: LeaveTypes/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: LeaveTypes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(LeaveType leaveType)
        {
            if (ModelState.IsValid)
            {
                _db.LeaveTypes.Add(leaveType);
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(leaveType);
        }

        // GET: LeaveTypes/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LeaveType leaveType = await _db.LeaveTypes.FindAsync(id);
            if (leaveType == null)
            {
                return HttpNotFound();
            }
            return View(leaveType);
        }

        // POST: LeaveTypes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(LeaveType leaveType)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(leaveType).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(leaveType);
        }

        // GET: LeaveTypes/Delete/5
        public async Task<PartialViewResult> Delete(int id)
        {
            LeaveType leaveType = await _db.LeaveTypes.FindAsync(id);
            return PartialView(leaveType);
        }

        // POST: LeaveTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            bool status = false;
            string message = string.Empty;
            var leaveType = await _db.LeaveTypes.FindAsync(id);
            if (leaveType != null)
            {
                _db.LeaveTypes.Remove(leaveType);
                await _db.SaveChangesAsync();
                status = true;
                message = "Leave Type Deleted Successfully...";
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
