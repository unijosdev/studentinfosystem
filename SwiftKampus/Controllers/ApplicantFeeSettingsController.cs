using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampusModel.AddmissionApplicant;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SwiftKampus.Controllers
{
    [Authorize]
    [Audit(AuditingLevel = 2)]
    public class ApplicantFeeSettingsController : BaseController
    {
        public ApplicantFeeSettingsController(SchoolDbContext db) : base(db)
        {

        }


        // GET: ApplicantFeeSettings
        public ActionResult Index()
        {
            return View();
        }
        public async Task<ActionResult> GetIndex()
        {
            // dc.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var applicationfee = await _db.ApplicantFeeSettings.Include(i => i.SchoolProgramme).Include(a => a.Session)
                                                .AsNoTracking().ToListAsync();
            var data = applicationfee.Select(s => new
            {
                s.ApplicantFeeSettingId,
                s.AmountInWords,
                s.SchoolProgramme.FancyName,
                s.Session.SessionName,
                s.ApplicationFee,
                s.IsEmailEnabled,
                s.IsSmsEnabled,
                s.AllocatedSms,
                s.AllocatedEmail
            }).ToList();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }


        // GET: ApplicantFeeSettings/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicantFeeSetting applicantFeeSetting = await _db.ApplicantFeeSettings.FindAsync(id);
            if (applicantFeeSetting == null)
            {
                return HttpNotFound();
            }
            return View(applicantFeeSetting);
        }

        public async Task<PartialViewResult> Save(int id)
        {
            var block = await _db.ApplicantFeeSettings.FindAsync(id);
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes, "SchoolProgrammeId", "FancyName");
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            return PartialView(block);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Save(ApplicantFeeSetting model)
        {
            bool status = false;
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                if (model.ApplicantFeeSettingId > 0)
                {
                    var applicantFeeSettings = await _db.ApplicantFeeSettings.FindAsync(model.ApplicantFeeSettingId);
                    if (applicantFeeSettings != null)
                    {
                        applicantFeeSettings.AmountInWords = model.AmountInWords;
                        applicantFeeSettings.SchoolProgrammeId = model.SchoolProgrammeId;
                        applicantFeeSettings.ApplicationFee = model.ApplicationFee;
                        applicantFeeSettings.SessionId = model.SessionId;
                        applicantFeeSettings.MakeActive = model.MakeActive;
                        applicantFeeSettings.IsSmsEnabled = model.IsSmsEnabled;
                        applicantFeeSettings.IsEmailEnabled = model.IsEmailEnabled;
                        applicantFeeSettings.AllocatedEmail = model.AllocatedEmail;
                        applicantFeeSettings.AllocatedSms = model.AllocatedSms;
                        _db.Entry(applicantFeeSettings).State = EntityState.Modified;
                        await _db.SaveChangesAsync();
                        message = "Application Setting Updated Successfully...";
                        return new JsonResult { Data = new { status = true, message } };
                    }
                }
                else
                {
                    _db.ApplicantFeeSettings.Add(model);
                    await _db.SaveChangesAsync();
                    message = "Applicant fee Added Successfully.";
                    return new JsonResult { Data = new { status = true, message } };
                }
            }
            return new JsonResult { Data = new { status, message = "Invalid Post" } };
            //return View(subject);
        }



        #region Create/Edit Code
        //// GET: ApplicantFeeSettings/Create
        //public ActionResult Create()
        //{
        //    ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes, "SchoolProgrammeId", "ProgrammeName");
        //    ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
        //    return View();
        //}

        //// POST: ApplicantFeeSettings/Create
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Create(ApplicantFeeSetting applicantFeeSetting)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _db.ApplicantFeeSettings.Add(applicantFeeSetting);
        //        await _db.SaveChangesAsync();
        //        return RedirectToAction("Index");
        //    }

        //    ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes, "SchoolProgrammeId", "ProgrammeName", applicantFeeSetting.SchoolProgrammeId);
        //    ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName", applicantFeeSetting.SessionId);
        //    return View(applicantFeeSetting);
        //}

        //// GET: ApplicantFeeSettings/Edit/5
        //public async Task<ActionResult> Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    ApplicantFeeSetting applicantFeeSetting = await _db.ApplicantFeeSettings.FindAsync(id);
        //    if (applicantFeeSetting == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes, "SchoolProgrammeId", "ProgrammeName", applicantFeeSetting.SchoolProgrammeId);
        //    ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName", applicantFeeSetting.SessionId);
        //    return View(applicantFeeSetting);
        //}

        //// POST: ApplicantFeeSettings/Edit/5
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Edit(ApplicantFeeSetting applicantFeeSetting)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _db.Entry(applicantFeeSetting).State = System.Data.Entity.EntityState.Modified;
        //        await _db.SaveChangesAsync();
        //        return RedirectToAction("Index");
        //    }
        //    ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes, "SchoolProgrammeId", "ProgrammeName", applicantFeeSetting.SchoolProgrammeId);
        //    ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName", applicantFeeSetting.SessionId);
        //    return View(applicantFeeSetting);
        //} 
        #endregion

        // GET: ApplicantFeeSettings/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicantFeeSetting applicantFeeSetting = await _db.ApplicantFeeSettings.FindAsync(id);
            if (applicantFeeSetting == null)
            {
                return HttpNotFound();
            }
            return View(applicantFeeSetting);
        }

        // POST: ApplicantFeeSettings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            ApplicantFeeSetting applicantFeeSetting = await _db.ApplicantFeeSettings.FindAsync(id);
            _db.ApplicantFeeSettings.Remove(applicantFeeSetting);
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
