using Microsoft.AspNet.Identity;
using SwiftKampus.Models;
using SwiftKampus.Services;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SwiftKampus.Controllers
{
    [Authorize]
    [Audit(AuditingLevel = 2)]
    public class AssignStudentLevelController : BaseController
    {

        public AssignStudentLevelController(SchoolDbContext db):base(db)
        {

        }

        public ActionResult SelectLevel()
        {
            ViewBag.LevelId = new SelectList(_db.Levels, "LevelId", "LevelName");
            return View();
        }

        // GET: AssignStudentLevel
        public async Task<ActionResult> CheckStudent()
        {
            var studentName = User.Identity.GetUserName();
            var userDept = await _db.Staffs.AsNoTracking().Where(x => x.StaffId.Equals(studentName))
                                        .Select(s => s.DepartmentId).FirstOrDefaultAsync();

            var studentList = await _db.Students.Include(i => i.Programme).AsNoTracking().Where(x => x.Active.Equals(true)
                                    && x.Programme.Department.DepartmentId.Equals(userDept)).ToListAsync();

            int totalActiveSession = 0;

            foreach (var student in studentList)
            {
                var examRecords = await _db.Results.AsNoTracking().Where(x => x.StudentId.Equals(student.StudentId))
                                            .ToListAsync();
                foreach (var session in examRecords.Select(s => s.SessionId))
                {
                    var countExamInSession = examRecords.Count(x => x.SessionId.Equals(session));
                    if (countExamInSession == 2)
                    {
                        totalActiveSession += 1;
                    }
                }
                var count = DetermineLevel(totalActiveSession);
                var studentrecord = await _db.Students.FindAsync(student.StudentId);
                if (studentrecord != null)
                {
                    studentrecord.LevelId = await GetLevelId(count);
                    _db.Entry(studentrecord).State = EntityState.Modified;
                }
            }
            await _db.SaveChangesAsync();

            ViewBag.Message = $"Total Record Updates is {studentList.Count}";
            return View();
        }

        private string DetermineLevel(int validCount)
        {
            string level = string.Empty;
            switch (validCount)
            {
                case 1:
                    level = "100";
                    break;

                case 2:
                    level = "200";
                    break;

                case 3:
                    level = "300";
                    break;

                case 4:
                    level = "400";
                    break;

                case 5:
                    level = "400";
                    break;

                case 6:
                    level = "400";
                    break;

                default:
                    level = "Undefined";
                    break;
            }
            return level;
        }

        private async Task<int> GetLevelId(string levelName)
        {
            var levelId = await _db.Levels.AsNoTracking().Where(x => x.LevelName.ToUpper().Equals(levelName.ToUpper()))
                                .Select(s => s.LevelId).FirstOrDefaultAsync();
            return levelId;
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