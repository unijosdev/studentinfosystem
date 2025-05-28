using NumberToWordConverter;
using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampusModel;
using SwiftKampusModel.Payment;
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
    public class IdCardPaymentSettingsController : BaseController
    {
        public IdCardPaymentSettingsController(SchoolDbContext _db) : base(_db)
        {
        }

        // GET: IdCardPaymentSettings
        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> GetIndex()
        {
            // dc.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var data = await _db.IdCardPaymentSettings.Include(i => i.Session).AsNoTracking()
                .Select(s => new
            {
                s.Session.SessionName,
                s.StudentType,
                s.Amount,
                s.AmountInWords,
                s.IdCardPaymentSettingId
            }).ToListAsync();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        public async Task<PartialViewResult> Save(int id)
        {
            var idCardSetting = await _db.IdCardPaymentSettings.FindAsync(id);
            ViewBag.SessionId = new SelectList(_db.Sessions.OrderByDescending(x => x.SessionName).AsNoTracking(), 
                                "SessionId", "SessionName", idCardSetting?.SessionId);
            var studentStatus = from StudentStatus s in Enum.GetValues(typeof(StudentStatus))
                         select new { ID = s, Name = s.ToString() };

            ViewBag.StudentType = new SelectList(studentStatus, "Name", "Name", idCardSetting?.StudentType);
            return PartialView(idCardSetting);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Save(IdCardPaymentSetting model)
        {
            bool status = false;
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                if (model.IdCardPaymentSettingId > 0)
                {
                    var idCardPayment = await _db.IdCardPaymentSettings.FindAsync(model.IdCardPaymentSettingId);
                    if (idCardPayment != null)
                    {
                        idCardPayment.SessionId = model.SessionId;
                        idCardPayment.StudentType = model.StudentType;
                        idCardPayment.Amount = model.Amount;
                        idCardPayment.AmountInWords = $"{WordConverter.GetNumberConverter(Convert.ToInt32(model.Amount).ToString()).Replace(".", "")} Naira Only.";
                        _db.Entry(idCardPayment).State = EntityState.Modified;
                        await _db.SaveChangesAsync();
                        message = $"{model.StudentType} Updated Successfully...";
                        return new JsonResult { Data = new { status = true, message } };

                    }
                }
                else
                {
                    model.AmountInWords = $"{WordConverter.GetNumberConverter(Convert.ToInt32(model.Amount).ToString()).Replace(".", "")} Naira Only.";
                    _db.IdCardPaymentSettings.Add(model);
                    await _db.SaveChangesAsync();
                    message = $"{model.StudentType} Added Successfully.";
                    return new JsonResult { Data = new { status = true, message } };
                }
            }
            return new JsonResult { Data = new { status, message } };
            //return View(subject);
        }


        // GET: IdCardPaymentSettings/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            IdCardPaymentSetting idCardPaymentSetting = await _db.IdCardPaymentSettings.FindAsync(id);
            if (idCardPaymentSetting == null)
            {
                return HttpNotFound();
            }
            return View(idCardPaymentSetting);
        }

        // POST: IdCardPaymentSettings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            IdCardPaymentSetting idCardPaymentSetting = await _db.IdCardPaymentSettings.FindAsync(id);
            _db.IdCardPaymentSettings.Remove(idCardPaymentSetting);
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
