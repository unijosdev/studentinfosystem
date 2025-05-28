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
    public class ExamRulesController : BaseController
    {

        public ExamRulesController(SchoolDbContext db) : base(db)
        {

        }

        // GET: ExamRules
        public async Task<ActionResult> Index()
        {
            var examRules = _db.ExamRules.AsNoTracking().Include(e => e.Course).Include(e => e.Level);
            return View(await examRules.ToListAsync());
        }

        // GET: ExamRules/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ExamRule examRule = await _db.ExamRules.FindAsync(id);
            if (examRule == null)
            {
                return HttpNotFound();
            }
            return View(examRule);
        }

        // GET: ExamRules/Create
        public ActionResult Create()
        {
            ViewBag.CourseId = new SelectList(_db.Courses.AsNoTracking(), "CourseId", "CourseCode");
            ViewBag.LevelId = new SelectList(_db.Levels.AsNoTracking(), "LevelId", "LevelName");
            ViewBag.ResultDivision = new SelectList(_db.ExamTypes.AsNoTracking(), "ExamTypeId", "ExamName");
            return View();
        }

        // POST: ExamRules/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "ExamRuleId,CourseId,LevelId,ResultDivision,ScorePerQuestion,TotalQuestion,MaximumTime")] ExamRule model)
        {
            if (ModelState.IsValid)
            {
                var examRule = new ExamRule()
                {
                    CourseId = model.CourseId,
                    ResultDivision = model.ResultDivision,
                    ScorePerQuestion = model.ScorePerQuestion,
                    TotalQuestion = model.TotalQuestion,
                    MaximumTime = model.MaximumTime,
                    LevelId = model.LevelId
                };
                _db.ExamRules.Add(examRule);
                await _db.SaveChangesAsync();
                TempData["UserMessage"] = "Exam Rule is Added Successfully.";
                TempData["Title"] = "Success.";

                return RedirectToAction("Create");
            }

            ViewBag.CourseId = new SelectList(_db.Courses.AsNoTracking(), "CourseId", "CourseCode", model.CourseId);
            ViewBag.LevelId = new SelectList(_db.Levels.AsNoTracking(), "LevelId", "LevelName", model.LevelId);
            ViewBag.ResultDivision = new SelectList(_db.ExamTypes.AsNoTracking(), "ExamTypeId", "ExamName");
            return View(model);
        }

        // GET: ExamRules/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ExamRule examRule = await _db.ExamRules.FindAsync(id);
            if (examRule == null)
            {
                return HttpNotFound();
            }
            ViewBag.CourseId = new SelectList(_db.Courses.AsNoTracking(), "CourseId", "CourseCode", examRule.CourseId);
            ViewBag.LevelId = new SelectList(_db.Levels.AsNoTracking(), "LevelId", "LevelName", examRule.LevelId);
            ViewBag.ResultDivision = new SelectList(_db.ExamTypes.AsNoTracking(), "ExamTypeId", "ExamName");
            return View(examRule);
        }

        // POST: ExamRules/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "ExamRuleId,CourseId,LevelId,ResultDivision,ScorePerQuestion,TotalQuestion,MaximumTime")] ExamRule examRule)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(examRule).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                TempData["UserMessage"] = "Exam Rule is Updated Successfully.";
                TempData["Title"] = "Success.";
                return RedirectToAction("Index");
            }
            ViewBag.CourseId = new SelectList(_db.Courses.AsNoTracking(), "CourseId", "CourseCode", examRule.CourseId);
            ViewBag.LevelId = new SelectList(_db.Levels.AsNoTracking(), "LevelId", "LevelName", examRule.LevelId);
            ViewBag.ResultDivision = new SelectList(_db.ExamTypes.AsNoTracking(), "ExamTypeId", "ExamName");
            return View(examRule);
        }

        // GET: ExamRules/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ExamRule examRule = await _db.ExamRules.FindAsync(id);
            if (examRule == null)
            {
                return HttpNotFound();
            }
            return View(examRule);
        }

        // POST: ExamRules/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            ExamRule examRule = await _db.ExamRules.FindAsync(id);
            if (examRule != null) _db.ExamRules.Remove(examRule);
            await _db.SaveChangesAsync();
            TempData["UserMessage"] = "Exam Rule is Deleted Successfully.";
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
