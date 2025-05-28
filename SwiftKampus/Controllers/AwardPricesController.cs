using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampus.ViewModels.StudentBioData;
using SwiftKampusModel.AddmissionApplicant;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SwiftKampus.Controllers
{
    [Authorize]
    [Audit(AuditingLevel = 2)]
    public class AwardPricesController : BaseController
    {

        public AwardPricesController(SchoolDbContext db) : base(db)
        {

        }
        // GET: AwardPrices
        public async Task<ActionResult> Index()
        {
            var result = ConfirmApplicationFee();
            if (result != null)
                return result;

            var awardPrices = await _db.AwardPrices.AsNoTracking()
                       .Where(x => x.ApplicantId.Equals(userId)).ToListAsync();
            return View(awardPrices);
        }

        // GET: AwardPrices/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AwardPrice awardPrice = await _db.AwardPrices.FindAsync(id);
            if (awardPrice == null)
            {
                return HttpNotFound();
            }
            return View(awardPrice);
        }

        // GET: AwardPrices/Create
        public ActionResult Create()
        {
            var result = ConfirmApplicationFee();
            if (result != null)
                return result;

            return View();
        }

        // POST: AwardPrices/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(List<AwardPrice> awardPrice)
        {
            var result = ConfirmApplicationFee();
            if (result != null)
                return result;

            if (ModelState.IsValid)
            {
                foreach (var item in awardPrice)
                {
                    item.ApplicantId = userId;
                    _db.AwardPrices.Add(item);
                }
                await _db.SaveChangesAsync();
                var message = $"{awardPrice.Count} awards are added successfully";
                return new JsonResult { Data = new { status = true, message } };
            }
            return new JsonResult { Data = new { status = true, message = "Please complete the form completely" } };
        }


        // GET: AwardPrices/Create
        public ActionResult PCreate()
        {          
            var award = _db.AwardPrices.AsNoTracking().Where(x => x.ApplicantId.Equals(userId))
                                                  .ToList();
            var model = new AwardVm()
            {
                AwardPrice = new AwardPrice(),
                AwardPrices = award
            };

            return View(model);

        }



        [HttpPost]
        public async Task<ActionResult> PCreate(List<AwardPrice> awardPrice)
        {
            var result = ConfirmAcceptanceFee();
            if (result != null)
                return result;

            if (ModelState.IsValid)
            {
                var award = _db.AwardPrices.AsNoTracking()
                                 .Where(x => x.ApplicantId.Equals(userId)).ToList();
                if (award != null && award.Any())
                {
                    foreach (var item in award)
                    {
                        _db.Entry(item).State = EntityState.Deleted;
                    }
                }
                _db.SaveChanges();

                foreach (var item in awardPrice)
                {
                    item.ApplicantId = userId;
                    _db.AwardPrices.Add(item);
                }
               
                await _db.SaveChangesAsync();
                var message = $"{awardPrice.Count} awards are added successfully";
                return new JsonResult { Data = new { status = true, message } };
            }
            return new JsonResult { Data = new { status = false, message = "Please complete the form completely" } };
        }


        // GET: AwardPrices/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            var result = ConfirmApplicationFee();
            if (result != null)
                return result;

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AwardPrice awardPrice = await _db.AwardPrices.FindAsync(id);
            if (awardPrice == null)
            {
                return HttpNotFound();
            }
            return View(awardPrice);
        }

        // POST: AwardPrices/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "AwardPriceId,ApplicantId,AwardingBody,AcademicPrice,Year")] AwardPrice awardPrice)
        {

            var result = ConfirmApplicationFee();
            if (result != null)
                return result;

            if (ModelState.IsValid)
            {
                _db.Entry(awardPrice).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(awardPrice);
        }

        // GET: AwardPrices/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AwardPrice awardPrice = await _db.AwardPrices.FindAsync(id);
            if (awardPrice == null)
            {
                return HttpNotFound();
            }
            return View(awardPrice);
        }

        // POST: AwardPrices/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            AwardPrice awardPrice = await _db.AwardPrices.FindAsync(id);
            _db.AwardPrices.Remove(awardPrice);
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
