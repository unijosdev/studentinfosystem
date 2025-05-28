using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampusModel;
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
    public class SchoolFeeSettingsController : BaseController
    {

        public SchoolFeeSettingsController(SchoolDbContext db) : base(db)
        {

        }
        // GET: SchoolFeeSettings
        //public async Task<ActionResult> Index()
        //{
        //    var schoolFeeSettings = _db.SchoolFeeSettings.Include(s => s.FeeCategory).Include(s => s.Semester).Include(s => s.Session);
        //    return View(await schoolFeeSettings.ToListAsync());
        //}
        public ActionResult Index()
        {
            return View();
        }
        public async Task<ActionResult> GetIndex()
        {
            var schoolFeesetting = await _db.SchoolFeeSettings.AsNoTracking()
                .Include(i => i.SchoolProgramme).Include(c => c.Session).ToListAsync();

            var data = schoolFeesetting.Select(s => new
            {
                s.FeeCategory,
                s.SchoolProgramme.FancyName,
                s.Session.SessionName,
                EndDate = s.EndDate.ToString(),
                StartDate = s.StartDate.ToString(),
                s.FinedAmount,
                s.IsActive,
                s.SchoolFeeSettingId

            }).ToList();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        public async Task<PartialViewResult> Save(int id)
        {
            var schoolFee = await _db.SchoolFeeSettings.FindAsync(id);
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            var feeCategory = from SchoolFeeCategory s in Enum.GetValues(typeof(SchoolFeeCategory))
                              select new { ID = s, Name = s.ToString() };
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FancyName");
            ViewBag.FeeCategory = new SelectList(feeCategory, "Name", "Name");
            return PartialView(schoolFee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Save(SchoolFeeSetting model)
        {
            bool status = false;
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                if (model.SchoolFeeSettingId > 0)
                {
                    var schoolFeesetting = await _db.SchoolFeeSettings.FindAsync(model.SchoolFeeSettingId);
                    if (schoolFeesetting != null)
                    {
                        try
                        {
                            schoolFeesetting.FeeCategory = model.FeeCategory;
                            schoolFeesetting.EndDate = model.EndDate;
                            schoolFeesetting.StartDate = model.StartDate;
                            schoolFeesetting.FinedAmount = model.FinedAmount;
                            schoolFeesetting.IsActive = model.IsActive;
                            schoolFeesetting.SessionId = model.SessionId;

                            _db.Entry(schoolFeesetting).State = EntityState.Modified;
                            await _db.SaveChangesAsync();
                            message = $" School fee Setting Updated Successfully...";
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

                    _db.SchoolFeeSettings.Add(model);
                    await _db.SaveChangesAsync();
                    message = $"School fee setting Added Successfully.";
                    return new JsonResult { Data = new { status = true, message } };
                }
            }
            return new JsonResult { Data = new { status, message = "Please complete all required field" } };
            //return View(subject);
        }

        #region Redundant Code
        //// GET: SchoolFeeSettings/Details/5
        //public async Task<ActionResult> Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    SchoolFeeSetting schoolFeeSetting = await _db.SchoolFeeSettings.FindAsync(id);
        //    if (schoolFeeSetting == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(schoolFeeSetting);
        //}

        //// GET: SchoolFeeSettings/Create
        //public ActionResult Create()
        //{
        //       var feeCategory = from SchoolFeeCategory s in Enum.GetValues(typeof(SchoolFeeCategory))
        //select new { ID = s, Name = s.ToString()
        //};

        //ViewBag.FeeCategory = new MultiSelectList(feeCategory, "Name", "Name");
        //    ViewBag.SemesterId = new SelectList(_db.Semesters, "SemesterId", "SemesterName");
        //    ViewBag.SessionId = new SelectList(_db.Sessions, "SessionId", "SessionName");
        //    return View();
        //}

        //// POST: SchoolFeeSettings/Create
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Create([Bind(Include = "SchoolFeeSettingId,FeeCategoryId,FinedAmount,StartDate,EndDate,SemesterId,SessionId,IsActive")] SchoolFeeSetting schoolFeeSetting)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _db.SchoolFeeSettings.Add(schoolFeeSetting);
        //        await _db.SaveChangesAsync();
        //        return RedirectToAction("Index");
        //    }

        //       var feeCategory = from SchoolFeeCategory s in Enum.GetValues(typeof(SchoolFeeCategory))
        //select new { ID = s, Name = s.ToString()
        //};

        //ViewBag.FeeCategory = new MultiSelectList(feeCategory, "Name", "Name");
        //    ViewBag.SemesterId = new SelectList(_db.Semesters, "SemesterId", "SemesterName", schoolFeeSetting.SemesterId);
        //    ViewBag.SessionId = new SelectList(_db.Sessions, "SessionId", "SessionName", schoolFeeSetting.SessionId);
        //    return View(schoolFeeSetting);
        //}

        //// GET: SchoolFeeSettings/Edit/5
        //public async Task<ActionResult> Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    SchoolFeeSetting schoolFeeSetting = await _db.SchoolFeeSettings.FindAsync(id);
        //    if (schoolFeeSetting == null)
        //    {
        //        return HttpNotFound();
        //    }
        //       var feeCategory = from SchoolFeeCategory s in Enum.GetValues(typeof(SchoolFeeCategory))
        //select new { ID = s, Name = s.ToString()
        //};

        //ViewBag.FeeCategory = new MultiSelectList(feeCategory, "Name", "Name");
        //    ViewBag.SemesterId = new SelectList(_db.Semesters, "SemesterId", "SemesterName", schoolFeeSetting.SemesterId);
        //    ViewBag.SessionId = new SelectList(_db.Sessions, "SessionId", "SessionName", schoolFeeSetting.SessionId);
        //    return View(schoolFeeSetting);
        //}

        //// POST: SchoolFeeSettings/Edit/5
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Edit([Bind(Include = "SchoolFeeSettingId,FeeCategoryId,FinedAmount,StartDate,EndDate,SemesterId,SessionId,IsActive")] SchoolFeeSetting schoolFeeSetting)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _db.Entry(schoolFeeSetting).State = EntityState.Modified;
        //        await _db.SaveChangesAsync();
        //        return RedirectToAction("Index");
        //    }
        //       var feeCategory = from SchoolFeeCategory s in Enum.GetValues(typeof(SchoolFeeCategory))
        //select new { ID = s, Name = s.ToString()
        //};

        //ViewBag.FeeCategory = new MultiSelectList(feeCategory, "Name", "Name");
        //    ViewBag.SemesterId = new SelectList(_db.Semesters, "SemesterId", "SemesterName", schoolFeeSetting.SemesterId);
        //    ViewBag.SessionId = new SelectList(_db.Sessions, "SessionId", "SessionName", schoolFeeSetting.SessionId);
        //    return View(schoolFeeSetting);
        //} 
        #endregion

        // GET: SchoolFeeSettings/Delete/5
        public async Task<PartialViewResult> Delete(int id)
        {
            var schoolFeeSetting = await _db.SchoolFeeSettings.FindAsync(id);
            return PartialView(schoolFeeSetting);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            bool status = false;
            string message = string.Empty;
            var schoolFeeSetting = await _db.SchoolFeeSettings.FindAsync(id);
            if (schoolFeeSetting != null)
            {
                _db.SchoolFeeSettings.Remove(schoolFeeSetting);
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
