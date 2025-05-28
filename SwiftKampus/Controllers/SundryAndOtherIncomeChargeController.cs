using SwiftKampus.Controllers;
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

namespace SwiftKampus.Abstractions.Controllers
{
    [Authorize]
    [Audit(AuditingLevel = 2)]
    public class SundryAndOtherIncomeChargeController : BaseController
    {

        public SundryAndOtherIncomeChargeController(SchoolDbContext db) : base(db)
        {

        }
        // GET: SundryAndOtherIncomeCharge
        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> GetIndex()
        {
            // dc.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var sundryCharge = await _db.SundryAndOtherIncomeCharges.AsNoTracking().ToListAsync();
            var data = sundryCharge.Select(s => new
            {
                s.Id,
                s.ChargeName,
                s.ChargeCode,
                s.ChargeDescription,
                s.Amount,
               
            }).ToList();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        public async Task<PartialViewResult> Save(int id)
        {
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FancyName");
            var sundryCharge = await _db.SundryAndOtherIncomeCharges.FindAsync(id);
            return PartialView(sundryCharge);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Save(SundryAndOtherIncomeCharge model)
        {
            bool status = false;
            string message = string.Empty;
            string[] ssizes = model.ChargeName.Trim().Split('-', '/');
           
            if (ModelState.IsValid)
            {
                if (model.Id > 0)
                {
                    var sundryCharge = await _db.SundryAndOtherIncomeCharges.FindAsync(model.Id);
                    if (sundryCharge != null)
                    {
                        try
                        {
                            sundryCharge.ChargeName = model.ChargeName.Trim();
                            sundryCharge.Amount = model.Amount;
                            sundryCharge.ChargeDescription = model.ChargeDescription;
                            sundryCharge.ChargeCode = model.ChargeCode;
                            sundryCharge.AmountInWords = model.AmountInWords;
                            sundryCharge.Status = model.Status;
                            _db.Entry(sundryCharge).State = System.Data.Entity.EntityState.Modified;
                            await _db.SaveChangesAsync();
                            message = $"{model.ChargeName} Updated Successfully...";
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
                    model.ChargeName = model.ChargeName;
                    model.Amount = model.Amount;
                    model.ChargeCode = model.ChargeCode;
                    model.ChargeDescription = model.ChargeDescription;
                    model.AmountInWords = model.AmountInWords;
                    model.Status = model.Status;
                    _db.SundryAndOtherIncomeCharges.Add(model);
                    await _db.SaveChangesAsync();
                    message = $"{model.ChargeName} Added Successfully.";
                    return new JsonResult { Data = new { status = true, message } };
                }
            }
            return new JsonResult { Data = new { status, message } };
            //return View(subject);
        }

        // GET: Sessions/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Session session = await _db.Sessions.FindAsync(id);
            if (session == null)
            {
                return HttpNotFound();
            }
            return View(session);
        }

        // GET: Sessions/Delete/5
        [HttpGet]
        public async Task<ActionResult> Delete(int id)
        {
            bool status = false;
            string message = string.Empty;
            var sundryCharge = await _db.SundryAndOtherIncomeCharges.FindAsync(id);
            if (sundryCharge != null)
            {
                _db.SundryAndOtherIncomeCharges.Remove(sundryCharge);
                await _db.SaveChangesAsync();
                status = true;
                message = "DataS Deleted Successfully...";
                //return new JsonResult { Data = new { status, message } };
                return this.Json(status, JsonRequestBehavior.AllowGet);
            }

            return new JsonResult { Data = new { status = false, message = $"{id } not deleted" } };
        }

        //[HttpGet, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> DeleteConfirmed(int id)
        //{
        //    bool status = false;
        //    string message = string.Empty;
        //    var sundryCharge = await _db.SundryAndOtherIncomeCharges.FindAsync(id);
        //    if (sundryCharge != null)
        //    {
        //        _db.SundryAndOtherIncomeCharges.Remove(sundryCharge);
        //        await _db.SaveChangesAsync();
        //        status = true;
        //        message = "DataS Deleted Successfully...";
        //        return new JsonResult { Data = new { status, message } };
        //    }

        //    return new JsonResult { Data = new { status = false, message = $"{id } not deleted" } };
        //}

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