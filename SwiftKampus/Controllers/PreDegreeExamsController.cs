using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampusModel;
using System.Data.Entity;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SwiftKampus.Controllers
{
    [Authorize]
    [Audit(AuditingLevel = 2)]
    public class PreDegreeExamsController : BaseController
    {

        public PreDegreeExamsController(SchoolDbContext db) : base(db)
        {

        }
        // GET: PreDegreeExams
        public async Task<ActionResult> Index()
        {
            var preDegreeExams = _db.PreDegreeExams;
            return View(await preDegreeExams.ToListAsync());
        }

        // GET: PreDegreeExams/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PreDegreeExam preDegreeExam = await _db.PreDegreeExams.FindAsync(id);
            if (preDegreeExam == null)
            {
                return HttpNotFound();
            }
            return View(preDegreeExam);
        }

        // GET: PreDegreeExams/Create
        public ActionResult Create()
        {
            ViewBag.RegNo = new SelectList(_db.PreDegreeStudents, "RegNo", "FullName");
            return View();
        }

        // POST: PreDegreeExams/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "PreDegreeExamId,RegNo,SubjectName,Score")] PreDegreeExam preDegreeExam)
        {
            if (ModelState.IsValid)
            {
                _db.PreDegreeExams.Add(preDegreeExam);
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.RegNo = new SelectList(_db.PreDegreeStudents, "RegNo", "FullName", preDegreeExam.RegNo);
            return View(preDegreeExam);
        }

        // GET: PreDegreeExams/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PreDegreeExam preDegreeExam = await _db.PreDegreeExams.FindAsync(id);
            if (preDegreeExam == null)
            {
                return HttpNotFound();
            }
            ViewBag.RegNo = new SelectList(_db.PreDegreeStudents, "RegNo", "FullName", preDegreeExam.RegNo);
            return View(preDegreeExam);
        }

        // POST: PreDegreeExams/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "PreDegreeExamId,RegNo,SubjectName,Score")] PreDegreeExam preDegreeExam)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(preDegreeExam).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.RegNo = new SelectList(_db.PreDegreeStudents, "RegNo", "FullName", preDegreeExam.RegNo);
            return View(preDegreeExam);
        }

        // GET: PreDegreeExams/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PreDegreeExam preDegreeExam = await _db.PreDegreeExams.FindAsync(id);
            if (preDegreeExam == null)
            {
                return HttpNotFound();
            }
            return View(preDegreeExam);
        }

        // POST: PreDegreeExams/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            PreDegreeExam preDegreeExam = await _db.PreDegreeExams.FindAsync(id);
            if (preDegreeExam != null) _db.PreDegreeExams.Remove(preDegreeExam);
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
