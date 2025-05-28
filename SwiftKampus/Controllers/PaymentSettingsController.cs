using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampusModel;
using SwiftKampusModel.Payment;
using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SwiftKampus.Controllers
{
    [Authorize]
    [Audit(AuditingLevel = 2)]
    public class PaymentSettingsController : BaseController
    {

        public PaymentSettingsController(SchoolDbContext db) : base(db)
        {

        }
        // GET: PaymentSettings
        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> GetIndex()
        {
            // dc.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var data = await _db.PaymentSettings.Include(i => i.Session).Include(i => i.SchoolProgramme).AsNoTracking()
                .Select(s => new
                {
                    s.SchoolProgramme.FancyName,
                    s.Session.SessionName,
                    s.SchoolFeeType,
                    s.StudentType,
                    s.AcceptPartPayment,
                    s.ConsiderIndigine,
                    s.ConsiderDepartmentalFee,
                    s.FirstPaymentPercentage,
                    s.ConsiderNationality,
                    s.PaymentSettingId
                }).ToListAsync();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        public async Task<PartialViewResult> Save(int id)
        {
            var paymentSetting = await _db.PaymentSettings.FindAsync(id);
            var feeCategory = from SchoolFeeCategory s in Enum.GetValues(typeof(SchoolFeeCategory))
                              select new { ID = s, Name = s.ToString() };
            var studentType = from StudentStatus s in Enum.GetValues(typeof(StudentStatus))
                              select new { ID = s, Name = s.ToString() };

            ViewBag.SchoolFeeType = new SelectList(feeCategory, "Name", "Name");
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName", paymentSetting?.SessionId);
            ViewBag.StudentType = new SelectList(studentType, "Name", "Name");
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes, "SchoolProgrammeId", "FancyName", paymentSetting?.SchoolProgrammeId);

            return PartialView(paymentSetting);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Save(PaymentSetting model)
        {
            bool status = false;
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                if (model.PaymentSettingId > 0)
                {
                    var paymentSetting = await _db.PaymentSettings.FindAsync(model.PaymentSettingId);
                    if (paymentSetting != null)
                    {
                        paymentSetting.SessionId = model.SessionId;
                        paymentSetting.SchoolFeeType = model.SchoolFeeType;
                        paymentSetting.AcceptPartPayment = model.AcceptPartPayment;
                        paymentSetting.FirstPaymentPercentage = model.FirstPaymentPercentage;
                        paymentSetting.ConsiderIndigine = model.ConsiderIndigine;
                        paymentSetting.ConsiderNationality = model.ConsiderNationality;
                        paymentSetting.ConsiderDepartmentalFee = model.ConsiderDepartmentalFee;
                        paymentSetting.StudentType = model.StudentType;

                        _db.Entry(paymentSetting).State = EntityState.Modified;
                        await _db.SaveChangesAsync();
                        message = $"{model.SchoolFeeType} Updated Successfully...";
                        return new JsonResult { Data = new { status = true, message } };
                    }
                }
                else
                {
                    _db.PaymentSettings.Add(model);
                    await _db.SaveChangesAsync();
                    message = $"{model.SchoolFeeType} Added Successfully.";
                    return new JsonResult { Data = new { status = true, message } };
                }
            }
            return new JsonResult { Data = new { status, message } };
            //return View(subject);
        }

        // GET: PaymentSettings/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PaymentSetting paymentSetting = await _db.PaymentSettings.FindAsync(id);
            if (paymentSetting == null)
            {
                return HttpNotFound();
            }
            return View(paymentSetting);
        }



        public async Task<PartialViewResult> Delete(int id)
        {
            var paymentSetting = await _db.PaymentSettings.FindAsync(id);
            return PartialView(paymentSetting);
        }


        // POST: PaymentSettings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            bool status = false;
            string message = string.Empty;
            var paymentSetting = await _db.PaymentSettings.FindAsync(id);
            if (paymentSetting != null)
            {
                _db.PaymentSettings.Remove(paymentSetting);
                await _db.SaveChangesAsync();
                status = true;
                message = "Payment Setting has been deleted Successfully...";
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
