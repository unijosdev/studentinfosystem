using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampusModel.AddmissionApplicant;
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
    public class UtmeScreningPoliciesController : BaseController
    {

        public UtmeScreningPoliciesController(SchoolDbContext db) : base(db)
        {

        }
        // GET: UtmeScreningPolicies
        public ActionResult Index()
        {
            ViewData.Add("ActionMessage", "List of Utme screening policy");
            return View();
        }

        public async Task<ActionResult> GetIndex()
        {
            // dc.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var data = await _db.UtmeScreningPolicies.Include(i => i.Session).AsNoTracking().Select(s => new
            {
                s.UtmeScreningPolicyId,
                s.Session.SessionName,
                s.JambPercentage,
                s.OLevelPercentage,
                s.JambMaximumScore,
                s.OLevelMaximumScore
            }).ToListAsync();
            ViewData.Add("ActionMessage", $"Getting ({data.Count}) list of policy");
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        public async Task<PartialViewResult> Save(int id)
        {
            var utmeScreningPolicy = await _db.UtmeScreningPolicies.FindAsync(id);
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            if (id == 0)
            {
                ViewData.Add("ActionMessage", "Create View for utme screnning policy");
            }
            else
            {
                ViewData.Add("ActionMessage", "Edit View for utme screnning policy");
            }
            return PartialView(utmeScreningPolicy);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Save(UtmeScreningPolicy model)
        {
            bool status = false;
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                if (model.UtmeScreningPolicyId > 0)
                {
                    var utmeScreningPolicy = await _db.UtmeScreningPolicies.FindAsync(model.UtmeScreningPolicyId);
                    if (utmeScreningPolicy != null)
                    {
                        utmeScreningPolicy.SessionId = model.SessionId;
                        utmeScreningPolicy.OLevelPercentage = model.OLevelPercentage;
                        utmeScreningPolicy.JambPercentage = model.JambPercentage;
                        utmeScreningPolicy.JambMaximumScore = model.JambMaximumScore;
                        utmeScreningPolicy.OLevelMaximumScore = model.OLevelMaximumScore;
                        _db.Entry(utmeScreningPolicy).State = EntityState.Modified;
                        await _db.SaveChangesAsync();
                        message = "Policy Updated Successfully...";
                        ViewData.Add("ActionMessage", $"Record {model.UtmeScreningPolicyId} {message}");
                        return new JsonResult { Data = new { status = true, message } };
                    }
                }
                else
                {
                    var checkExist = _db.UtmeScreningPolicies.AsNoTracking()
                                        .Any(x => x.SessionId.Equals(model.SessionId));
                    if (checkExist)
                    {
                        message = $"Policy has been set for selected session, Please Edit or choose another session";
                        return new JsonResult { Data = new { status = true, message } };
                    }
                    _db.UtmeScreningPolicies.Add(model);
                    await _db.SaveChangesAsync();
                    message = $"Policy is set Successfully.";
                    ViewData.Add("ActionMessage", $"{message}");
                    return new JsonResult { Data = new { status = true, message } };
                }
            }
            return new JsonResult { Data = new { status, message } };
            //return View(subject);
        }

        // GET: UtmeScreningPolicies/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UtmeScreningPolicy utmeScreningPolicy = await _db.UtmeScreningPolicies.FindAsync(id);
            if (utmeScreningPolicy == null)
            {
                return HttpNotFound();
            }
            return View(utmeScreningPolicy);
        }

        // GET: UtmeScreningPolicies/Create
        public ActionResult Create()
        {
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            return View();
        }

        // POST: UtmeScreningPolicies/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "UtmeScreningPolicyId,SessionId,JambPercentage,OLevelPercentage")] UtmeScreningPolicy utmeScreningPolicy)
        {
            if (ModelState.IsValid)
            {
                _db.UtmeScreningPolicies.Add(utmeScreningPolicy);
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName", utmeScreningPolicy.SessionId);
            return View(utmeScreningPolicy);
        }

        // GET: UtmeScreningPolicies/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UtmeScreningPolicy utmeScreningPolicy = await _db.UtmeScreningPolicies.FindAsync(id);
            if (utmeScreningPolicy == null)
            {
                return HttpNotFound();
            }
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName", utmeScreningPolicy.SessionId);
            return View(utmeScreningPolicy);
        }

        // POST: UtmeScreningPolicies/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "UtmeScreningPolicyId,SessionId,JambPercentage,OLevelPercentage")] UtmeScreningPolicy utmeScreningPolicy)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(utmeScreningPolicy).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName", utmeScreningPolicy.SessionId);
            return View(utmeScreningPolicy);
        }

        // GET: UtmeScreningPolicies/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UtmeScreningPolicy utmeScreningPolicy = await _db.UtmeScreningPolicies.FindAsync(id);
            if (utmeScreningPolicy == null)
            {
                return HttpNotFound();
            }
            ViewData.Add("ActionMessage", "Delete View for utme policy");
            return View(utmeScreningPolicy);
        }

        // POST: UtmeScreningPolicies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            UtmeScreningPolicy utmeScreningPolicy = await _db.UtmeScreningPolicies.FindAsync(id);
            _db.UtmeScreningPolicies.Remove(utmeScreningPolicy);
            await _db.SaveChangesAsync();
            ViewData.Add("ActionMessage", $"Delete for {id} record utme policy was deleted successfully");
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
