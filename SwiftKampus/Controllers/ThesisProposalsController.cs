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
    public class ThesisProposalsController : BaseController
    {

        public ThesisProposalsController(SchoolDbContext db) : base(db)
        {

        }
        // GET: ThesisProposals
        public async Task<ActionResult> Index()
        {
            return View(await _db.ThesisProposals.ToListAsync());
        }

        // GET: ThesisProposals/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ThesisProposal thesisProposal = await _db.ThesisProposals.FindAsync(id);
            if (thesisProposal == null)
            {
                return HttpNotFound();
            }
            return View(thesisProposal);
        }

        // GET: ThesisProposals/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ThesisProposals/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "ThesisProposalId,SpecialInterest,ProposedTopic,Proposal,Attachment")] ThesisProposal thesisProposal)
        {
            if (ModelState.IsValid)
            {
                _db.ThesisProposals.Add(thesisProposal);
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(thesisProposal);
        }

        // GET: ThesisProposals/PCreate
        public ActionResult PCreate()
        {
            var user = User.IsInRole(RoleName.Student);
            if (user == true) { ViewBag.Role = "Student"; }
            else { ViewBag.Role = "Applicant"; }

            var thesisProposal = _db.ThesisProposals.AsNoTracking().Where(x => x.UserId.Equals(userId)).ToList();
            var model = new ThesisVm()
            {
                ThesisProposal = new ThesisProposal(),
                ThesisProposals = thesisProposal
            };
            return View(model);
        }

        // POST: ThesisProposals/PCreate
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> PCreate(List<ThesisProposal> thesisProposal)
        {
            if (ModelState.IsValid)
            {
                var oldThesis = _db.ThesisProposals.AsNoTracking()
                           .Where(x => x.UserId.Equals(userId)).ToList();
                if (oldThesis != null && oldThesis.Any())
                {
                    foreach (var item in oldThesis)
                    {
                        _db.Entry(item).State = EntityState.Deleted;
                    }
                }
                foreach (var item in thesisProposal)
                {
                    item.UserId = userId;
                    _db.ThesisProposals.Add(item);
                }
                await _db.SaveChangesAsync();
                var message = $"{thesisProposal.Count} Thesis are added successfully";
                return new JsonResult { Data = new { status = true, message } };
            }

            return new JsonResult { Data = new { status = false, message = "Please complete the form completely" } };
        }

        // GET: ThesisProposals/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ThesisProposal thesisProposal = await _db.ThesisProposals.FindAsync(id);
            if (thesisProposal == null)
            {
                return HttpNotFound();
            }
            return View(thesisProposal);
        }

        // POST: ThesisProposals/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "ThesisProposalId,SpecialInterest,ProposedTopic,Proposal,Attachment")] ThesisProposal thesisProposal)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(thesisProposal).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(thesisProposal);
        }

        // GET: ThesisProposals/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ThesisProposal thesisProposal = await _db.ThesisProposals.FindAsync(id);
            if (thesisProposal == null)
            {
                return HttpNotFound();
            }
            return View(thesisProposal);
        }

        // POST: ThesisProposals/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            ThesisProposal thesisProposal = await _db.ThesisProposals.FindAsync(id);
            _db.ThesisProposals.Remove(thesisProposal);
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
