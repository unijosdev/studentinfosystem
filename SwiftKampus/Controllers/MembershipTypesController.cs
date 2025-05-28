using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampusModel.Library;
using System.Data.Entity;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SwiftKampus.Controllers
{
    [Authorize]
    [Audit(AuditingLevel = 2)]
    public class MembershipTypesController : BaseController
    {

        public MembershipTypesController(SchoolDbContext db) : base(db)
        {

        }
        // GET: MembershipTypes
        public async Task<ActionResult> Index()
        {
            return View(await _db.MembershipTypes.ToListAsync());
        }

        // GET: MembershipTypes/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MembershipType membershipType = await _db.MembershipTypes.FindAsync(id);
            if (membershipType == null)
            {
                return HttpNotFound();
            }
            return View(membershipType);
        }

        // GET: MembershipTypes/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: MembershipTypes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "MembershipTypeId,TypeOfMembership,NumberOfBookToBorrow")] MembershipType membershipType)
        {
            if (ModelState.IsValid)
            {
                _db.MembershipTypes.Add(membershipType);
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(membershipType);
        }

        // GET: MembershipTypes/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MembershipType membershipType = await _db.MembershipTypes.FindAsync(id);
            if (membershipType == null)
            {
                return HttpNotFound();
            }
            return View(membershipType);
        }

        // POST: MembershipTypes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "MembershipTypeId,TypeOfMembership,NumberOfBookToBorrow")] MembershipType membershipType)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(membershipType).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(membershipType);
        }

        // GET: MembershipTypes/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MembershipType membershipType = await _db.MembershipTypes.FindAsync(id);
            if (membershipType == null)
            {
                return HttpNotFound();
            }
            return View(membershipType);
        }

        // POST: MembershipTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            MembershipType membershipType = await _db.MembershipTypes.FindAsync(id);
            _db.MembershipTypes.Remove(membershipType);
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
