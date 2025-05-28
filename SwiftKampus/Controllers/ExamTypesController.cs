using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampusModel.CBTE;
using System.Data.Entity;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SwiftKampus.Controllers
{
    [Authorize]
    [Audit(AuditingLevel = 2)]
    public class ExamTypesController : BaseController
    {

        public ExamTypesController(SchoolDbContext db) : base(db)
        {

        }
        // GET: ExamTypes
        public async Task<ActionResult> Index()
        {
            return View(await _db.ExamTypes.AsNoTracking().ToListAsync());
        }

        // GET: ExamTypes/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ExamType examType = await _db.ExamTypes.FindAsync(id);
            if (examType == null)
            {
                return HttpNotFound();
            }
            return View(examType);
        }

        // GET: ExamTypes/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ExamTypes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "ExamTypeId,ExamName")] ExamType examType)
        {
            if (ModelState.IsValid)
            {
                _db.ExamTypes.Add(examType);
                await _db.SaveChangesAsync();
                TempData["UserMessage"] = "Exam Type is Created Successfully.";
                TempData["Title"] = "Success.";

                return RedirectToAction("Create");
            }

            return View(examType);
        }

        // GET: ExamTypes/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ExamType examType = await _db.ExamTypes.FindAsync(id);
            if (examType == null)
            {
                return HttpNotFound();
            }
            return View(examType);
        }

        // POST: ExamTypes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "ExamTypeId,ExamName")] ExamType examType)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(examType).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                TempData["UserMessage"] = "Exam Type is Updated Successfully.";
                TempData["Title"] = "Success.";

                return RedirectToAction("Index");
            }
            return View(examType);
        }

        // GET: ExamTypes/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ExamType examType = await _db.ExamTypes.FindAsync(id);
            if (examType == null)
            {
                return HttpNotFound();
            }
            return View(examType);
        }

        // POST: ExamTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            ExamType examType = await _db.ExamTypes.FindAsync(id);
            if (examType != null) _db.ExamTypes.Remove(examType);
            await _db.SaveChangesAsync();
            TempData["UserMessage"] = "Exam Type is Deleted Successfully.";
            TempData["Title"] = "Error.";

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
