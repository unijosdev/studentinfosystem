using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampusModel;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SwiftKampus.Controllers
{
    [Authorize]
    [Audit(AuditingLevel = 2)]
    public class StudentAssignmentsController : BaseController
    {

        public StudentAssignmentsController(SchoolDbContext db) : base(db)
        {

        }
        // GET: StudentAssignments
        public async Task<ActionResult> Index()
        {
            if (User.IsInRole(RoleName.Academic))
            {

            }
            else if (User.IsInRole(RoleName.Student))
            {

            }

            var studentAssignments = _db.StudentAssignments.AsNoTracking().Include(s => s.Course)
                .Include(s => s.Level).Include(s => s.Programme).Include(s => s.Semester)
                .Include(s => s.Sessions).Include(s => s.Student);
            return View(await studentAssignments.ToListAsync());
        }

        // GET: StudentAssignments/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            StudentAssignment studentAssignment = await _db.StudentAssignments.FindAsync(id);
            if (studentAssignment == null)
            {
                return HttpNotFound();
            }
            return View(studentAssignment);
        }

        // GET: StudentAssignments/Create
        public ActionResult Create()
        {
            ViewBag.CourseId = new SelectList(_db.Courses.AsNoTracking(), "CourseId", "CourseCode");
            ViewBag.LevelId = new SelectList(_db.Levels.AsNoTracking(), "LevelId", "LevelName");
            ViewBag.ProgrammeId = new SelectList(_db.Programmes.AsNoTracking(), "ProgrammeId", "ProgrammeCode");
            ViewBag.StudentId = new SelectList(_db.Students.AsNoTracking(), "StudentId", "FirstName");
            return View();
        }

        // POST: StudentAssignments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "StudentAssignmentId,StudentId,SemesterId,SessionId,LevelId,CourseId,ProgrammeId,AssignmentQuestion,AssignmentAnswer")] StudentAssignment studentAssignment)
        {
            if (ModelState.IsValid)
            {
                studentAssignment.SemesterId = semesterId;
                studentAssignment.SessionId = sessionId;
                _db.StudentAssignments.Add(studentAssignment);
                await _db.SaveChangesAsync();
                TempData["UserMessage"] = "Assignment Submitted Successfully.";
                TempData["Title"] = "Success.";

                return RedirectToAction("Create");
            }

            ViewBag.CourseId = new SelectList(_db.Courses.AsNoTracking(), "CourseId", "CourseCode", studentAssignment.CourseId);
            ViewBag.LevelId = new SelectList(_db.Levels.AsNoTracking(), "LevelId", "LevelName", studentAssignment.LevelId);
            ViewBag.ProgrammeId = new SelectList(_db.Programmes.AsNoTracking(), "ProgrammeId", "ProgrammeCode", studentAssignment.ProgrammeId);
            ViewBag.StudentId = new SelectList(_db.Students.AsNoTracking(), "StudentId", "FirstName", studentAssignment.StudentId);
            return View(studentAssignment);
        }

        // GET: StudentAssignments/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            StudentAssignment studentAssignment = await _db.StudentAssignments.FindAsync(id);
            if (studentAssignment == null)
            {
                return HttpNotFound();
            }
            ViewBag.CourseId = new SelectList(_db.Courses.AsNoTracking(), "CourseId", "CourseCode", studentAssignment.CourseId);
            ViewBag.LevelId = new SelectList(_db.Levels.AsNoTracking(), "LevelId", "LevelName", studentAssignment.LevelId);
            ViewBag.ProgrammeId = new SelectList(_db.Programmes.AsNoTracking(), "ProgrammeId", "ProgrammeCode", studentAssignment.ProgrammeId);
            ViewBag.StudentId = new SelectList(_db.Students.AsNoTracking(), "StudentId", "FirstName", studentAssignment.StudentId);
            return View(studentAssignment);
        }

        // POST: StudentAssignments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public async Task<ActionResult> Edit([Bind(Include = "StudentAssignmentId,StudentId,SemesterId,SessionId,LevelId,CourseId,ProgrammeId,AssignmentQuestion,AssignmentAnswer")] StudentAssignment studentAssignment)
        {
            if (ModelState.IsValid)
            {
                studentAssignment.SemesterId = semesterId;
                studentAssignment.SessionId = sessionId;
                _db.Entry(studentAssignment).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                TempData["UserMessage"] = "Assignment is Updated Successfully.";
                TempData["Title"] = "Success.";
                return RedirectToAction("Index");
            }
            ViewBag.CourseId = new SelectList(_db.Courses.AsNoTracking(), "CourseId", "CourseCode", studentAssignment.CourseId);
            ViewBag.LevelId = new SelectList(_db.Levels.AsNoTracking(), "LevelId", "LevelName", studentAssignment.LevelId);
            ViewBag.ProgrammeId = new SelectList(_db.Programmes.AsNoTracking(), "ProgrammeId", "ProgrammeCode", studentAssignment.ProgrammeId);
            ViewBag.StudentId = new SelectList(_db.Students.AsNoTracking(), "StudentId", "FirstName", studentAssignment.StudentId);
            return View(studentAssignment);
        }

        // GET: StudentAssignments/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            StudentAssignment studentAssignment = await _db.StudentAssignments.FindAsync(id);
            if (studentAssignment == null)
            {
                return HttpNotFound();
            }
            return View(studentAssignment);
        }

        // POST: StudentAssignments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            StudentAssignment studentAssignment = await _db.StudentAssignments.FindAsync(id);
            if (studentAssignment != null) _db.StudentAssignments.Remove(studentAssignment);
            await _db.SaveChangesAsync();
            TempData["UserMessage"] = "Assignment is Deleted Successfully.";
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
