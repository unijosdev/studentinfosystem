using SwiftKampus.Models;
using SwiftKampus.Services;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SwiftKampus.Controllers
{
    [Authorize]
    [Audit(AuditingLevel = 2)]
    public class EventsController : BaseController
    {

        public EventsController(SchoolDbContext db) : base(db)
        {

        }
        // GET: Events
        public async Task<ActionResult> Birthdays()
        {
            var staffs = await _db.Staffs.Include(i => i.Department).AsNoTracking()
                                .Where(x => x.DateOfBirth.Month == DateTime.Now.Month &&
                                x.DateOfBirth.Day == DateTime.Now.Day).ToListAsync();
            return View(staffs);
        }

        public async Task<ActionResult> StudentBirthdays()
        {
            //var mydept = await _db.Students.Include(i => i.Programme.Department).AsNoTracking()
            //                        .Where(x => x.StudentId.Equals(userId))
            //                        .Select(s => s.Programme.Department.DepartmentId)
            //                        .FirstOrDefaultAsync();
            var students = await _db.Students.Include(i => i.Programme).Include(i => i.Programme.Department)
                            .AsNoTracking().Where(x => x.DateOfBirth.Month == DateTime.Now.Month &&
                            x.DateOfBirth.Day == DateTime.Now.Day).ToListAsync();
            return View(students);
        }

        public ActionResult StudentProfile()
        {
            return View();
        }
    }
}