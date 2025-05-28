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
    public class FacultyFeeSettingsController : BaseController
    {

        public FacultyFeeSettingsController(SchoolDbContext db) : base(db)
        {

        }

        // GET: FacultyFeeSettings
        public async Task<ActionResult> Index()
        {
            var facultyFeeSettings = _db.FacultyFeeSettings.Include(f => f.FacultyFeeType).Include(f => f.Semester).Include(f => f.Session);
            return View(await facultyFeeSettings.ToListAsync());
        }
        public async Task<ActionResult> GetIndex()
        {
            var facultyFeesetting = await _db.FacultyFeeSettings.AsNoTracking().Include(c => c.Semester)
                .Include(c => c.Session).Include(c => c.FacultyFeeType).ToListAsync();

            var data = facultyFeesetting.Select(s => new
            {
                s.FacultyFeeType.FeeName,
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
            var facultyFeeSetting = await _db.FacultyFeeSettings.FindAsync(id);
            ViewBag.FacultyFeeTypeId = new SelectList(_db.FacultyFeeTypes, "FacultyFeeTypeId", "FeeName");
            ViewBag.SemesterId = new SelectList(_db.Semesters, "SemesterId", "SemesterName");
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            return PartialView(facultyFeeSetting);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Save(FacultyFeeSetting model)
        {
            bool status = false;
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                if (model.FacultyFeeSettingId > 0)
                {
                    var facultyFeesetting = await _db.FacultyFeeSettings.FindAsync(model.FacultyFeeSettingId);
                    if (facultyFeesetting != null)
                    {
                        try
                        {
                            facultyFeesetting.FacultyFeeTypeId = model.FacultyFeeTypeId;
                            facultyFeesetting.EndDate = model.EndDate;
                            facultyFeesetting.StartDate = model.StartDate;
                            facultyFeesetting.FinedAmount = model.FinedAmount;
                            facultyFeesetting.IsActive = model.IsActive;
                            facultyFeesetting.SemesterId = model.SemesterId;
                            facultyFeesetting.SessionId = model.SessionId;

                            _db.Entry(facultyFeesetting).State = EntityState.Modified;
                            await _db.SaveChangesAsync();
                            message = $"Faculty fee Setting Updated Successfully...";
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

                    _db.FacultyFeeSettings.Add(model);
                    await _db.SaveChangesAsync();
                    message = $"Faculty fee setting Added Successfully.";
                    return new JsonResult { Data = new { status = true, message } };
                }
            }
            return new JsonResult { Data = new { status, message = "Please complete all required field" } };
            //return View(subject);
        }


        #region MyRegion Redundant Code
        //// GET: FacultyFeeSettings/Details/5
        //public async Task<ActionResult> Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    FacultyFeeSetting facultyFeeSetting = await _db.FacultyFeeSettings.FindAsync(id);
        //    if (facultyFeeSetting == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(facultyFeeSetting);
        //}

        //// GET: FacultyFeeSettings/Create
        //public ActionResult Create()
        //{
        //    ViewBag.FacultyFeeTypeId = new SelectList(_db.FacultyFeeTypes, "FacultyFeeTypeId", "StudentType");
        //    ViewBag.SemesterId = new SelectList(_db.Semesters, "SemesterId", "SemesterName");
        //    ViewBag.SessionId = new SelectList(_db.Sessions, "SessionId", "SessionName");
        //    return View();
        //}

        //// POST: FacultyFeeSettings/Create
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Create(FacultyFeeSetting facultyFeeSetting)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _db.FacultyFeeSettings.Add(facultyFeeSetting);
        //        await _db.SaveChangesAsync();
        //        return RedirectToAction("Index");
        //    }

        //    ViewBag.FacultyFeeTypeId = new SelectList(_db.FacultyFeeTypes, "FacultyFeeTypeId", "StudentType", facultyFeeSetting.FacultyFeeTypeId);
        //    ViewBag.SemesterId = new SelectList(_db.Semesters, "SemesterId", "SemesterName", facultyFeeSetting.SemesterId);
        //    ViewBag.SessionId = new SelectList(_db.Sessions, "SessionId", "SessionName", facultyFeeSetting.SessionId);
        //    return View(facultyFeeSetting);
        //}

        //// GET: FacultyFeeSettings/Edit/5
        //public async Task<ActionResult> Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    FacultyFeeSetting facultyFeeSetting = await _db.FacultyFeeSettings.FindAsync(id);
        //    if (facultyFeeSetting == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    ViewBag.FacultyFeeTypeId = new SelectList(_db.FacultyFeeTypes, "FacultyFeeTypeId", "StudentType", facultyFeeSetting.FacultyFeeTypeId);
        //    ViewBag.SemesterId = new SelectList(_db.Semesters, "SemesterId", "SemesterName", facultyFeeSetting.SemesterId);
        //    ViewBag.SessionId = new SelectList(_db.Sessions, "SessionId", "SessionName", facultyFeeSetting.SessionId);
        //    return View(facultyFeeSetting);
        //}

        //// POST: FacultyFeeSettings/Edit/5
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Edit(FacultyFeeSetting facultyFeeSetting)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _db.Entry(facultyFeeSetting).State = EntityState.Modified;
        //        await _db.SaveChangesAsync();
        //        return RedirectToAction("Index");
        //    }
        //    ViewBag.FacultyFeeTypeId = new SelectList(_db.FacultyFeeTypes, "FacultyFeeTypeId", "StudentType", facultyFeeSetting.FacultyFeeTypeId);
        //    ViewBag.SemesterId = new SelectList(_db.Semesters, "SemesterId", "SemesterName", facultyFeeSetting.SemesterId);
        //    ViewBag.SessionId = new SelectList(_db.Sessions, "SessionId", "SessionName", facultyFeeSetting.SessionId);
        //    return View(facultyFeeSetting);
        //} 
        #endregion

        // GET: FacultyFeeSettings/Delete/5
        public async Task<PartialViewResult> Delete(int id)
        {
            var facultyFeeSetting = await _db.FacultyFeeSettings.FindAsync(id);
            return PartialView(facultyFeeSetting);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            bool status = false;
            string message = string.Empty;
            var facultyFeeSetting = await _db.FacultyFeeSettings.FindAsync(id);
            if (facultyFeeSetting != null)
            {
                _db.FacultyFeeSettings.Remove(facultyFeeSetting);
                await _db.SaveChangesAsync();
                status = true;
                message = "faculty fee settings Deleted Successfully...";
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
