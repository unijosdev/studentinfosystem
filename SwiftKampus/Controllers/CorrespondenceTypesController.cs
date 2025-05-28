using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampus.ViewModels;
using SwiftKampusModel;
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
    public class CorrespondenceTypesController : BaseController
    {

        public CorrespondenceTypesController(SchoolDbContext db) : base(db)
        {

        }

        // GET: CorrespondenceTypes
        public async Task<ActionResult> Index()
        {
            var correspondenceTypes = _db.CorrespondenceTypes.Include(c => c.Correspondence);
            return View(await correspondenceTypes.AsNoTracking().ToListAsync());
        }

        // GET: CorrespondenceTypes/Details/5
        /// <summary>
        /// List all the types of correspondence available to 
        /// members of the school community
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var correspondenceType = await _db.CorrespondenceTypes
                                             .AsNoTracking().Where(ct =>
                                             ct.CorrespondenceTypeId.Equals(id.Value))
                                             .SingleOrDefaultAsync();

            if (correspondenceType == null)
            {
                return HttpNotFound();
            }
            return View(correspondenceType);
        }

        // GET: CorrespondenceTypes/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: CorrespondenceTypes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateCorrespondenceTypeVM correspondenceType)
        {
            var ct = new CorrespondenceType();

            if (correspondenceType == null || ct.CorrespondenceTypeName.Equals(correspondenceType.Name))
            {
                TempData["UserMessage"] = "The correspondence is either empty or already exists!";
                TempData["Title"] = "Error.";
                return View(correspondenceType);
            }
            if (ModelState.IsValid)
            {
                ct.CorrespondenceTypeName = correspondenceType.Name;
                ct.Description = correspondenceType.Description;
                _db.CorrespondenceTypes.Add(ct);
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(correspondenceType);
        }

        // GET: CorrespondenceTypes/Edit/5
        public async Task<ActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var correspondenceType = await _db.CorrespondenceTypes.FindAsync(id);
            if (correspondenceType == null)
            {
                return HttpNotFound();
            }
            ViewBag.CorrespondenceTypeId = new SelectList(_db.Correspondences, "CorrespondenceId", "StudentId", correspondenceType.CorrespondenceTypeId);
            return View(correspondenceType);
        }

        // POST: CorrespondenceTypes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "CorrespondenceTypeId,CorrespondenceTypeName,Description")] CorrespondenceType correspondenceType)
        {
            if (ModelState.IsValid)
            {
                //_db.Entry(correspondenceType).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.CorrespondenceTypeId = new SelectList(_db.Correspondences, "CorrespondenceId", "StudentId", correspondenceType.CorrespondenceTypeId);
            return View(correspondenceType);
        }

        // GET: CorrespondenceTypes/Delete/5
        public async Task<ActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CorrespondenceType correspondenceType = await _db.CorrespondenceTypes.FindAsync(id);
            if (correspondenceType == null)
            {
                return HttpNotFound();
            }
            return View(correspondenceType);
        }

        // POST: CorrespondenceTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(Guid id)
        {
            CorrespondenceType correspondenceType = await _db.CorrespondenceTypes.FindAsync(id);
            _db.CorrespondenceTypes.Remove(correspondenceType);
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
