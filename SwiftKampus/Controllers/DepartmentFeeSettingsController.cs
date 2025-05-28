using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampusModel.Payment;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SwiftKampus.Controllers
{
    [Authorize]
    [Audit(AuditingLevel = 2)]
    public class DepartmentFeeSettingsController : BaseController
    {

        public DepartmentFeeSettingsController(SchoolDbContext db) : base(db)
        {

        }

        // GET: DepartmentFeeSettings
        public async Task<ActionResult> Index()
        {
            var departmentFeeSettings = _db.DepartmentFeeSettings.Include(d => d.DepartmentFeeType).Include(d => d.Semester).Include(d => d.Session);
            return View(await departmentFeeSettings.ToListAsync());
        }

        public async Task<ActionResult> GetIndex()
        {
            var deptFeesetting = await _db.DepartmentFeeSettings.AsNoTracking().Include(c => c.Semester)
                .Include(c => c.Session).Include(c => c.DepartmentFeeType).ToListAsync();

            var data = deptFeesetting.Select(s => new
            {
                s.DepartmentFeeType.FeeName,
                s.Semester.SemesterName,
                s.Session.SessionName,
                s.EndDate,
                s.StartDate,
                s.FinedAmount,
                s.IsActive

            }).ToList();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        public async Task<PartialViewResult> Save(int id)
        {
            var deptFeeSetting = await _db.DepartmentFeeSettings.FindAsync(id);
            ViewBag.DepartmentFeeTypeId = new SelectList(_db.DepartmentFeeTypes, "DepartmentFeeTypeId", "FeeName");
            ViewBag.SemesterId = new SelectList(_db.Semesters, "SemesterId", "SemesterName");
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            return PartialView(deptFeeSetting);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Save(DepartmentFeeSetting model)
        {
            bool status = false;
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                if (model.DepartmentFeeSettingId > 0)
                {
                    var deptFeesetting = await _db.DepartmentFeeSettings.FindAsync(model.DepartmentFeeSettingId);
                    if (deptFeesetting != null)
                    {
                        try
                        {
                            deptFeesetting.DepartmentFeeTypeId = model.DepartmentFeeTypeId;
                            deptFeesetting.EndDate = model.EndDate;
                            deptFeesetting.StartDate = model.StartDate;
                            deptFeesetting.FinedAmount = model.FinedAmount;
                            deptFeesetting.IsActive = model.IsActive;
                            deptFeesetting.SemesterId = model.SemesterId;
                            deptFeesetting.SessionId = model.SessionId;

                            _db.Entry(deptFeesetting).State = EntityState.Modified;
                            await _db.SaveChangesAsync();
                            message = $"Department fee Setting Updated Successfully...";
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

                    _db.DepartmentFeeSettings.Add(model);
                    await _db.SaveChangesAsync();
                    message = $"Department fee setting Added Successfully.";
                    return new JsonResult { Data = new { status = true, message } };
                }
            }
            return new JsonResult { Data = new { status, message = "Please complete all required field" } };
            //return View(subject);
        }


        #region MyRegion redundant Code
        //// GET: DepartmentFeeSettings/Details/5
        //public async Task<ActionResult> Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    DepartmentFeeSetting departmentFeeSetting = await _db.DepartmentFeeSettings.FindAsync(id);
        //    if (departmentFeeSetting == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(departmentFeeSetting);
        //}

        //// GET: DepartmentFeeSettings/Create
        //public ActionResult Create()
        //{
        //    ViewBag.DepartmentFeeTypeId = new SelectList(_db.DepartmentFeeTypes, "DepartmentFeeTypeId", "StudentType");
        //    ViewBag.SemesterId = new SelectList(_db.Semesters, "SemesterId", "SemesterName");
        //    ViewBag.SessionId = new SelectList(_db.Sessions, "SessionId", "SessionName");
        //    return View();
        //}

        //// POST: DepartmentFeeSettings/Create
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Create(DepartmentFeeSetting departmentFeeSetting)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _db.DepartmentFeeSettings.Add(departmentFeeSetting);
        //        await _db.SaveChangesAsync();
        //        return RedirectToAction("Index");
        //    }

        //    ViewBag.DepartmentFeeTypeId = new SelectList(_db.DepartmentFeeTypes, "DepartmentFeeTypeId", "StudentType", departmentFeeSetting.DepartmentFeeTypeId);
        //    ViewBag.SemesterId = new SelectList(_db.Semesters, "SemesterId", "SemesterName", departmentFeeSetting.SemesterId);
        //    ViewBag.SessionId = new SelectList(_db.Sessions, "SessionId", "SessionName", departmentFeeSetting.SessionId);
        //    return View(departmentFeeSetting);
        //}

        //// GET: DepartmentFeeSettings/Edit/5
        //public async Task<ActionResult> Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    DepartmentFeeSetting departmentFeeSetting = await _db.DepartmentFeeSettings.FindAsync(id);
        //    if (departmentFeeSetting == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    ViewBag.DepartmentFeeTypeId = new SelectList(_db.DepartmentFeeTypes, "DepartmentFeeTypeId", "StudentType", departmentFeeSetting.DepartmentFeeTypeId);
        //    ViewBag.SemesterId = new SelectList(_db.Semesters, "SemesterId", "SemesterName", departmentFeeSetting.SemesterId);
        //    ViewBag.SessionId = new SelectList(_db.Sessions, "SessionId", "SessionName", departmentFeeSetting.SessionId);
        //    return View(departmentFeeSetting);
        //}

        //// POST: DepartmentFeeSettings/Edit/5
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Edit(DepartmentFeeSetting departmentFeeSetting)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _db.Entry(departmentFeeSetting).State = EntityState.Modified;
        //        await _db.SaveChangesAsync();
        //        return RedirectToAction("Index");
        //    }
        //    ViewBag.DepartmentFeeTypeId = new SelectList(_db.DepartmentFeeTypes, "DepartmentFeeTypeId", "StudentType", departmentFeeSetting.DepartmentFeeTypeId);
        //    ViewBag.SemesterId = new SelectList(_db.Semesters, "SemesterId", "SemesterName", departmentFeeSetting.SemesterId);
        //    ViewBag.SessionId = new SelectList(_db.Sessions, "SessionId", "SessionName", departmentFeeSetting.SessionId);
        //    return View(departmentFeeSetting);
        //} 
        #endregion

        // GET: DepartmentFeeSettings/Delete/5
        public async Task<PartialViewResult> Delete(int id)
        {
            var deptFeeSetting = await _db.DepartmentFeeSettings.FindAsync(id);
            return PartialView(deptFeeSetting);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            bool status = false;
            string message = string.Empty;
            var deptFeeSetting = await _db.DepartmentFeeSettings.FindAsync(id);
            if (deptFeeSetting != null)
            {
                _db.DepartmentFeeSettings.Remove(deptFeeSetting);
                await _db.SaveChangesAsync();
                status = true;
                message = "Department fee settings Deleted Successfully...";
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
