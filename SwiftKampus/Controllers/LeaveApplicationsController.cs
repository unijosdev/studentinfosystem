using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampusModel.Employee.Leave;
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
    public class LeaveApplicationsController : BaseController
    {

        public LeaveApplicationsController(SchoolDbContext db) : base(db)
        {

        }

        // GET: LeaveApplications
        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> GetIndex()
        {
            var myData = await _db.LeaveApplications.Include(l => l.LeaveType).Include(l => l.Staff).AsNoTracking()
                                .ToListAsync();
            if (User.IsInRole(RoleName.Admin))
            {
                var data = myData.Select(s => new
                {
                    s.LeaveApplicationId,
                    s.Staff.FullName,
                    s.StartDate,
                    s.EndDate,
                    s.ReasonForLeave,
                    s.IsHalfDay,
                    LeaveStatus = s.LeaveStatus.ToString()
                }).ToList();
                return Json(new { data }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var data = myData.Where(x => x.StaffId.Equals(userId)).Select(s => new
                {
                    s.LeaveApplicationId,
                    s.LeaveType.LeaveTypeName,
                    s.Staff.FullName,
                    StartDate = s.StartDate.ToString(),
                    EndDate = s.EndDate.ToString(),
                    s.ReasonForLeave,
                    s.IsHalfDay,
                    s.Staus
                }).ToList();
                return Json(new { data }, JsonRequestBehavior.AllowGet);
            }
            // dc.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key

        }

        public ActionResult UnApproved()
        {
            return View();
        }

        public async Task<ActionResult> GetUnApproved()
        {
            var myData = await _db.LeaveApplications.Include(l => l.LeaveType).Include(l => l.Staff)
                                    .Include(i => i.Staff.Department).AsNoTracking().ToListAsync();

            var staffDept = await _db.Staffs.Include(i => i.Department).AsNoTracking().Where(x => x.StaffId.Equals(userId))
                                .Select(x => x.Department.DepartmentId).FirstOrDefaultAsync();

            if (User.IsInRole(RoleName.Hod))
            {
                var data = myData.Where(x => x.IsHodApproved.Equals(false) && x.Staff.Department.DepartmentId.Equals(staffDept))
                    .Select(s => new
                    {
                        s.LeaveType.LeaveTypeName,
                        s.LeaveApplicationId,
                        s.Staff.FullName,
                        StartDate = s.StartDate.ToString(),
                        EndDate = s.EndDate.ToString(),
                        s.ReasonForLeave,
                        s.IsHalfDay,
                        s.Staus,
                        LeaveStatus = s.LeaveStatus.ToString()
                    }).ToList();
                return Json(new { data }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var data = myData.Where(x => x.IsHrApproved.Equals(false) && x.IsHodApproved.Equals(true)).Select(s => new
                {
                    s.LeaveApplicationId,
                    s.LeaveType.LeaveTypeName,
                    s.Staff.FullName,
                    StartDate = s.StartDate.ToString(),
                    EndDate = s.EndDate.ToString(),
                    s.ReasonForLeave,
                    s.IsHalfDay,
                    s.Staus
                }).ToList();
                return Json(new { data }, JsonRequestBehavior.AllowGet);
            }
            // dc.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key

        }
        // GET: LeaveApplications/Details/5
        public async Task<PartialViewResult> Details(int id)
        {
            var leaveApplication = await _db.LeaveApplications.Include(l => l.LeaveType)
                                        .Include(l => l.Staff).Include(i => i.Staff.Department).AsNoTracking()
                                        .Where(x => x.LeaveApplicationId.Equals(id)).FirstOrDefaultAsync();

            return PartialView(leaveApplication);
        }

        [HttpPost]
        public async Task<ActionResult> Approve(int id)
        {
            string message = string.Empty;
            LeaveApplication leaveApplication = await _db.LeaveApplications.FindAsync(id);
            if (User.IsInRole(RoleName.Hod))
            {
                if (leaveApplication != null) leaveApplication.IsHodApproved = true;
                message = $"{leaveApplication.StaffId} Leave Approved Successfully...";

            }
            else if (User.IsInRole(RoleName.Registrar))
            {
                if (leaveApplication != null) leaveApplication.IsHrApproved = true;
                message = $"{leaveApplication.StaffId} Leave Approved Successfully...";

            }
            if (leaveApplication != null)
            {
                message = $"Only HOD and Registrar are allowed to Approve staff Leave";
                _db.Entry(leaveApplication).State = EntityState.Modified;
            }
            await _db.SaveChangesAsync();

            return new JsonResult { Data = new { status = true, message } };
        }
        // GET: LeaveApplications/Create
        public ActionResult Create()
        {
            ViewBag.LeaveTypeId = new SelectList(_db.LeaveTypes, "LeaveTypeId", "LeaveTypeName");
            //ViewBag.StaffId = new SelectList(_db.Staffs, "StaffId", "FullName");
            return View();
        }

        // POST: LeaveApplications/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(LeaveApplication leaveApplication)
        {
            if (ModelState.IsValid)
            {
                leaveApplication.StaffId = userId;
                leaveApplication.ApplicationDate = DateTime.Now;
                _db.LeaveApplications.Add(leaveApplication);
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.LeaveTypeId = new SelectList(_db.LeaveTypes, "LeaveTypeId", "LeaveTypeName", leaveApplication.LeaveTypeId);
            //ViewBag.StaffId = new SelectList(_db.Staffs, "StaffId", "Designation", leaveApplication.StaffId);
            return View(leaveApplication);
        }

        // GET: LeaveApplications/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LeaveApplication leaveApplication = await _db.LeaveApplications.FindAsync(id);
            if (leaveApplication == null)
            {
                return HttpNotFound();
            }
            ViewBag.LeaveTypeId = new SelectList(_db.LeaveTypes, "LeaveTypeId", "LeaveTypeName", leaveApplication.LeaveTypeId);
            ViewBag.StaffId = new SelectList(_db.Staffs, "StaffId", "Designation", leaveApplication.StaffId);
            return View(leaveApplication);
        }

        // POST: LeaveApplications/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(LeaveApplication leaveApplication)
        {
            if (ModelState.IsValid)
            {
                leaveApplication.StaffId = userId;
                leaveApplication.ApplicationDate = DateTime.Now;
                _db.Entry(leaveApplication).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.LeaveTypeId = new SelectList(_db.LeaveTypes, "LeaveTypeId", "LeaveTypeName", leaveApplication.LeaveTypeId);
            ViewBag.StaffId = new SelectList(_db.Staffs, "StaffId", "Designation", leaveApplication.StaffId);
            return View(leaveApplication);
        }

        // GET: LeaveApplications/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LeaveApplication leaveApplication = await _db.LeaveApplications.FindAsync(id);
            if (leaveApplication == null)
            {
                return HttpNotFound();
            }
            return View(leaveApplication);
        }

        // POST: LeaveApplications/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            LeaveApplication leaveApplication = await _db.LeaveApplications.FindAsync(id);
            if (leaveApplication != null) _db.LeaveApplications.Remove(leaveApplication);
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
