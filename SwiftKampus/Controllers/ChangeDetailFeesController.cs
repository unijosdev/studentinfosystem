using System.Data.Entity;
using System.Threading.Tasks;
using System.Net;
using System.Web.Mvc;
using SwiftKampus.Models;
using SwiftKampusModel.Payment;
using SwiftKampus.Services;
using System.Linq;
using SwiftKampusModel;
using System;
using NumberToWordConverter;

namespace SwiftKampus.Controllers
{
    [Authorize]
    [Audit(AuditingLevel = 2)]
    public class ChangeDetailFeesController : BaseController
    {
        public ChangeDetailFeesController(SchoolDbContext db) : base(db)
        {

        }

        // GET: ChangeDetailFees
        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> GetIndex()
        {
            // dc.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var data = await _db.ChangeDetailFees.AsNoTracking().Select(s => new
            {
                s.FancyName,
                s.Amount,
                s.AmountInWords,
                s.ChangeDetailFeeId
            }).ToListAsync();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        public async Task<PartialViewResult> Save(int id)
        {
            var changeDetailFee = await _db.ChangeDetailFees.FindAsync(id);
            var gender = from DetailPayment s in Enum.GetValues(typeof(DetailPayment))
                         select new { ID = s, Name = s.ToString() };
            ViewBag.DetailCategory = new MultiSelectList(gender, "Name", "Name");
            return PartialView(changeDetailFee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Save(ChangeDetailFee model)
        {
            bool status = false;
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                if (model.ChangeDetailFeeId > 0)
                {
                    var changeDetailFee = await _db.ChangeDetailFees.FindAsync(model.ChangeDetailFeeId);
                    if (changeDetailFee != null)
                    {
                        changeDetailFee.AmountInWords = WordConverter.GetNumberConverter(Convert.ToInt32(model.Amount).ToString(), " Naira Only");
                        changeDetailFee.Amount = model.Amount;
                        changeDetailFee.DetailCategory = model.DetailCategory;
                        changeDetailFee.FancyName = model.FancyName;
                        _db.Entry(changeDetailFee).State = EntityState.Modified;
                        await _db.SaveChangesAsync();
                        message = $"{model.FancyName} Charges Updated Successfully...";
                        return new JsonResult { Data = new { status = true, message } };

                    }
                }
                else
                {
                    model.AmountInWords = WordConverter.GetNumberConverter(Convert.ToInt32(model.Amount).ToString(), " Naira Only");
                    _db.ChangeDetailFees.Add(model);
                    await _db.SaveChangesAsync();
                    message = $"{model.FancyName} Charges Added Successfully.";
                    return new JsonResult { Data = new { status = true, message } };
                }
            }
            return new JsonResult { Data = new { status, message } };
            //return View(subject);
        }



        // GET: ChangeDetailFees/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ChangeDetailFee changeDetailFee = await _db.ChangeDetailFees.FindAsync(id);
            if (changeDetailFee == null)
            {
                return HttpNotFound();
            }
            return View(changeDetailFee);
        }

        // POST: ChangeDetailFees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            ChangeDetailFee changeDetailFee = await _db.ChangeDetailFees.FindAsync(id);
            _db.ChangeDetailFees.Remove(changeDetailFee);
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
