using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampusModel.Classroom.Quiz;
using System.Data.Entity;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SwiftKampus.Controllers
{
    [Authorize]
    [Audit(AuditingLevel = 2)]
    public class QuizRulesController : BaseController
    {

        public QuizRulesController(SchoolDbContext db) : base(db)
        {

        }

        // GET: QuizRules
        public async Task<ActionResult> Index()
        {
            return View(await _db.QuizRules.ToListAsync());
        }

        // GET: QuizRules/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            QuizRule quizRule = await _db.QuizRules.FindAsync(id);
            if (quizRule == null)
            {
                return HttpNotFound();
            }
            return View(quizRule);
        }

        // GET: QuizRules/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: QuizRules/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "QuizRuleId,ScorePerQuestion,TotalQuestion,MaximumTime")] QuizRule quizRule)
        {
            if (ModelState.IsValid)
            {
                _db.QuizRules.Add(quizRule);
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(quizRule);
        }

        // GET: QuizRules/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            QuizRule quizRule = await _db.QuizRules.FindAsync(id);
            if (quizRule == null)
            {
                return HttpNotFound();
            }
            return View(quizRule);
        }

        // POST: QuizRules/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "QuizRuleId,ScorePerQuestion,TotalQuestion,MaximumTime")] QuizRule quizRule)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(quizRule).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(quizRule);
        }

        // GET: QuizRules/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            QuizRule quizRule = await _db.QuizRules.FindAsync(id);
            if (quizRule == null)
            {
                return HttpNotFound();
            }
            return View(quizRule);
        }

        // POST: QuizRules/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            QuizRule quizRule = await _db.QuizRules.FindAsync(id);
            _db.QuizRules.Remove(quizRule);
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
