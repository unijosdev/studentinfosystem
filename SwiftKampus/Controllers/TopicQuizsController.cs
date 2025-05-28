using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampusModel;
using SwiftKampusModel.Classroom.Quiz;
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
    public class TopicQuizsController : BaseController
    {

        public TopicQuizsController(SchoolDbContext db) : base(db)
        {

        }
        // GET: TopicQuizs
        public async Task<ActionResult> Index()
        {
            var topicQuizs = _db.TopicQuizs.Include(t => t.Topic);
            return View(await topicQuizs.ToListAsync());
        }

        // GET: TopicQuizs/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TopicQuiz topicQuiz = await _db.TopicQuizs.FindAsync(id);
            if (topicQuiz == null)
            {
                return HttpNotFound();
            }
            return View(topicQuiz);
        }

        // GET: TopicQuizs/Create
        public ActionResult Create()
        {
            ViewBag.TopicId = new SelectList(_db.Topics, "TopicId", "TopicName");

            var questionType = from QuestionType s in Enum.GetValues(typeof(QuestionType))
                               select new { ID = s, Name = s.ToString() };

            ViewBag.QuestionType = new SelectList(questionType, "Name", "Name");

            return View();
        }

        // POST: TopicQuizs/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(TopicQuiz topicQuiz)
        {
            if (ModelState.IsValid)
            {
                _db.TopicQuizs.Add(topicQuiz);
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.TopicId = new SelectList(_db.Topics, "TopicId", "TopicName", topicQuiz.TopicId);

            return View(topicQuiz);
        }

        // GET: TopicQuizs/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TopicQuiz topicQuiz = await _db.TopicQuizs.FindAsync(id);
            if (topicQuiz == null)
            {
                return HttpNotFound();
            }
            ViewBag.TopicId = new SelectList(_db.Topics, "TopicId", "TopicName", topicQuiz.TopicId);
            var questionType = from QuestionType s in Enum.GetValues(typeof(QuestionType))
                               select new { ID = s, Name = s.ToString() };

            ViewBag.QuestionType = new SelectList(questionType, "Name", "Name");

            return View(topicQuiz);
        }

        // POST: TopicQuizs/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "TopicQuizId,TopicId,Question,Option1,Option2,Option3,Option4,Answer,QuestionHint,QuestionType,IsFillInTheGag,IsMultiChoiceAnswer,IsSingleChoiceAnswer")] TopicQuiz topicQuiz)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(topicQuiz).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.TopicId = new SelectList(_db.Topics, "TopicId", "TopicName", topicQuiz.TopicId);
            var questionType = from QuestionType s in Enum.GetValues(typeof(QuestionType))
                               select new { ID = s, Name = s.ToString() };

            ViewBag.QuestionType = new SelectList(questionType, "Name", "Name");


            return View(topicQuiz);
        }

        // GET: TopicQuizs/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TopicQuiz topicQuiz = await _db.TopicQuizs.FindAsync(id);
            if (topicQuiz == null)
            {
                return HttpNotFound();
            }
            return View(topicQuiz);
        }

        // POST: TopicQuizs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            TopicQuiz topicQuiz = await _db.TopicQuizs.FindAsync(id);
            _db.TopicQuizs.Remove(topicQuiz);
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
