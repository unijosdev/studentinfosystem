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
    public class LibraryRegistrationsController : BaseController
    {

        public LibraryRegistrationsController(SchoolDbContext db) : base(db)
        {

        }

        // GET: LibraryRegistrations
        public async Task<ActionResult> Index()
        {
            var libraryRegistrations = _db.LibraryRegistrations.Include(l => l.MembershipType);
            return View(await libraryRegistrations.ToListAsync());
        }

        // GET: LibraryRegistrations/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LibraryRegistration libraryRegistration = await _db.LibraryRegistrations.FindAsync(id);
            if (libraryRegistration == null)
            {
                return HttpNotFound();
            }
            return View(libraryRegistration);
        }

        // GET: LibraryRegistrations/Create
        public ActionResult Create()
        {
            ViewBag.MembershipTypeId = new SelectList(_db.MembershipTypes, "MembershipTypeId", "TypeOfMembership");
            return View();
        }

        // POST: LibraryRegistrations/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "LibraryRegistrationId,MembershipTypeId")] LibraryRegistration libraryRegistration)
        {
            if (ModelState.IsValid)
            {
                _db.LibraryRegistrations.Add(libraryRegistration);
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.MembershipTypeId = new SelectList(_db.MembershipTypes, "MembershipTypeId", "TypeOfMembership", libraryRegistration.MembershipTypeId);
            return View(libraryRegistration);
        }

        // GET: LibraryRegistrations/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LibraryRegistration libraryRegistration = await _db.LibraryRegistrations.FindAsync(id);
            if (libraryRegistration == null)
            {
                return HttpNotFound();
            }
            ViewBag.MembershipTypeId = new SelectList(_db.MembershipTypes, "MembershipTypeId", "TypeOfMembership", libraryRegistration.MembershipTypeId);
            return View(libraryRegistration);
        }

        // POST: LibraryRegistrations/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "LibraryRegistrationId,MembershipTypeId")] LibraryRegistration libraryRegistration)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(libraryRegistration).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.MembershipTypeId = new SelectList(_db.MembershipTypes, "MembershipTypeId", "TypeOfMembership", libraryRegistration.MembershipTypeId);
            return View(libraryRegistration);
        }

        // GET: LibraryRegistrations/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LibraryRegistration libraryRegistration = await _db.LibraryRegistrations.FindAsync(id);
            if (libraryRegistration == null)
            {
                return HttpNotFound();
            }
            return View(libraryRegistration);
        }

        // POST: LibraryRegistrations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            LibraryRegistration libraryRegistration = await _db.LibraryRegistrations.FindAsync(id);
            _db.LibraryRegistrations.Remove(libraryRegistration);
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
