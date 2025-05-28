using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampusModel.CBTE;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SwiftKampus.Controllers
{
    [Authorize]
    [Audit(AuditingLevel = 2)]
    public class ExamLogsController : BaseController
    {

        public ExamLogsController(SchoolDbContext db) : base(db)
        {

        }

        // GET: ExamLogs
        public async Task<ActionResult> Index(string studentId, int courseId, int levelId,
                        int semesterid, int sessionId)
        {
            var examLogs = _db.ExamLogs.AsNoTracking().Include(e => e.Course).Include(e => e.ExamType).Include(e => e.Level)
                            .Include(e => e.Semester).Include(e => e.Sessions)
                            .Where(x => x.StudentId.Equals(studentId) && x.CourseId.Equals(courseId) && x.LevelId.Equals(levelId)
                            && x.SemesterId.Equals(semesterid) && x.SessionId.Equals(sessionId));
            return View(await examLogs.ToListAsync());
        }

        // GET: ExamLogs/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ExamLog examLog = await _db.ExamLogs.FindAsync(id);
            if (examLog == null)
            {
                return HttpNotFound();
            }
            return View(examLog);
        }

        // GET: ExamLogs/Create
        public ActionResult Create()
        {
            ViewBag.CourseId = new SelectList(_db.Courses.AsNoTracking(), "CourseId", "CourseCode");
            ViewBag.ExamTypeId = new SelectList(_db.ExamTypes.AsNoTracking(), "ExamTypeId", "ExamTypeId");
            ViewBag.LevelId = new SelectList(_db.Levels.AsNoTracking(), "LevelId", "LevelName");
            ViewBag.SemesterId = new SelectList(_db.Semesters.AsNoTracking(), "SemesterId", "SemesterName");
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            return View();
        }

        // POST: ExamLogs/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "ExamLogId,StudentId,CourseId,LevelId,SemesterId,SessionId,ExamTypeId,Score")] ExamLog examLog)
        {
            if (ModelState.IsValid)
            {
                _db.ExamLogs.Add(examLog);
                await _db.SaveChangesAsync();
                return RedirectToAction("Create");
            }

            ViewBag.CourseId = new SelectList(_db.Courses.AsNoTracking(), "CourseId", "CourseCode", examLog.CourseId);
            ViewBag.ExamTypeId = new SelectList(_db.ExamTypes.AsNoTracking(), "ExamTypeId", "ExamTypeId", examLog.ExamTypeId);
            ViewBag.LevelId = new SelectList(_db.Levels.AsNoTracking(), "LevelId", "LevelName", examLog.LevelId);
            ViewBag.SemesterId = new SelectList(_db.Semesters.AsNoTracking(), "SemesterId", "SemesterName", examLog.SemesterId);
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName", examLog.SessionId);
            return View(examLog);
        }

        // GET: ExamLogs/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ExamLog examLog = await _db.ExamLogs.FindAsync(id);
            if (examLog == null)
            {
                return HttpNotFound();
            }
            ViewBag.CourseId = new SelectList(_db.Courses.AsNoTracking(), "CourseId", "CourseCode", examLog.CourseId);
            ViewBag.ExamTypeId = new SelectList(_db.ExamTypes.AsNoTracking(), "ExamTypeId", "ExamTypeId", examLog.ExamTypeId);
            ViewBag.LevelId = new SelectList(_db.Levels.AsNoTracking(), "LevelId", "LevelName", examLog.LevelId);
            ViewBag.SemesterId = new SelectList(_db.Semesters.AsNoTracking(), "SemesterId", "SemesterName", examLog.SemesterId);
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName", examLog.SessionId);
            return View(examLog);
        }

        // POST: ExamLogs/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "ExamLogId,StudentId,CourseId,LevelId,SemesterId,SessionId,ExamTypeId,Score")] ExamLog examLog)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(examLog).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.CourseId = new SelectList(_db.Courses.AsNoTracking(), "CourseId", "CourseCode", examLog.CourseId);
            ViewBag.ExamTypeId = new SelectList(_db.ExamTypes, "ExamTypeId", "ExamTypeId", examLog.ExamTypeId);
            ViewBag.LevelId = new SelectList(_db.Levels.AsNoTracking(), "LevelId", "LevelName", examLog.LevelId);
            ViewBag.SemesterId = new SelectList(_db.Semesters.AsNoTracking(), "SemesterId", "SemesterName", examLog.SemesterId);
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName", examLog.SessionId);
            return View(examLog);
        }

        // GET: ExamLogs/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ExamLog examLog = await _db.ExamLogs.FindAsync(id);
            if (examLog == null)
            {
                return HttpNotFound();
            }
            return View(examLog);
        }

        // POST: ExamLogs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            ExamLog examLog = await _db.ExamLogs.FindAsync(id);
            if (examLog != null) _db.ExamLogs.Remove(examLog);
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
