using System.Data.Entity;
using System.Threading.Tasks;
using System.Net;
using System.Web.Mvc;
using SwiftKampus.Models;
using SwiftKampusModel;
using SwiftKampus.Services;
using System.Linq;

namespace SwiftKampus.Controllers
{
    [Authorize]
    [Audit(AuditingLevel = 2)]
    public class CourseRegSettingsController : BaseController
    {
        public CourseRegSettingsController(SchoolDbContext db) : base(db)
        {
        }

        // GET: CourseRegSettings
        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> GetIndex()
        {
            var setting = await _db.CourseRegSettings.Include(c => c.SchoolProgramme).Include(c => c.Semester).Include(c => c.Session)
                            .AsNoTracking().ToListAsync();
            var data = setting.Select(s => new
                        {
                            s.SchoolProgramme.FancyName,
                            s.Session.SessionName,
                            s.Semester.SemesterName,
                            s.IsActive,
                            s.ActivatePrequisite,
                            OpeningDate = s.OpeningDate.ToString("dd MMM yyyy"),
                            ClosingDate = s.ClosingDate.ToString("dd MMM yyyy"),
                            s.CourseRegSettingId
                        }).ToList();

            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        // GET: CourseRegSettings/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CourseRegSetting courseRegSetting = await _db.CourseRegSettings.FindAsync(id);
            if (courseRegSetting == null)
            {
                return HttpNotFound();
            }
            return View(courseRegSetting);
        }


        public async Task<PartialViewResult> Save(int id)
        {
            var courseRegSetting = await _db.CourseRegSettings.FindAsync(id);          

            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes, "SchoolProgrammeId", "FancyName", courseRegSetting?.SchoolProgrammeId);
            ViewBag.SemesterId = new SelectList(_db.Semesters, "SemesterId", "SemesterName", courseRegSetting?.SemesterId);
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName", courseRegSetting?.SessionId);

            return PartialView(courseRegSetting);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Save(CourseRegSetting model)
        {
            bool status = false;
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                if (model.CourseRegSettingId > 0)
                {
                    var courseRegSetting = await _db.CourseRegSettings.FindAsync(model.CourseRegSettingId);
                    if (courseRegSetting != null)
                    {
                        courseRegSetting.SchoolProgrammeId = model.SchoolProgrammeId;
                        courseRegSetting.SessionId = model.SessionId;
                        courseRegSetting.SemesterId = model.SemesterId;
                        courseRegSetting.OpeningDate = model.OpeningDate;
                        courseRegSetting.ClosingDate = model.ClosingDate;
                        courseRegSetting.ActivatePrequisite = model.ActivatePrequisite;
                        courseRegSetting.IsActive = model.IsActive;
                        _db.Entry(courseRegSetting).State = EntityState.Modified;
                        await _db.SaveChangesAsync();
                        message = $"Course Reg Setting Updated Successfully...";
                        return new JsonResult { Data = new { status = true, message } };

                    }
                }
                else
                {
                    var checkCourseRegSetting = _db.CourseRegSettings.AsNoTracking().Any(x => x.SchoolProgrammeId.Equals(model.SchoolProgrammeId) &&
                                                    x.SemesterId.Equals(model.SemesterId) && x.SessionId.Equals(model.SessionId));
                    if (!checkCourseRegSetting)
                    {
                        _db.CourseRegSettings.Add(model);
                        await _db.SaveChangesAsync();
                    }                   
                    message = $"Course Reg Setting Added Successfully.";
                    return new JsonResult { Data = new { status = true, message } };
                }
            }
            return new JsonResult { Data = new { status, message } };
            //return View(subject);
        }



        // GET: CourseRegSettings/Create
        public ActionResult Create()
        {
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes, "SchoolProgrammeId", "ProgrammeType");
            ViewBag.SemesterId = new SelectList(_db.Semesters, "SemesterId", "SemesterName");
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            return View();
        }

        // POST: CourseRegSettings/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "CourseRegSettingId,SchoolProgrammeId,SessionId,SemesterId,ClosingDate,OpeningDate,IsActive")] CourseRegSetting courseRegSetting)
        {
            if (ModelState.IsValid)
            {
                _db.CourseRegSettings.Add(courseRegSetting);
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes, "SchoolProgrammeId", "ProgrammeType", courseRegSetting.SchoolProgrammeId);
            ViewBag.SemesterId = new SelectList(_db.Semesters, "SemesterId", "SemesterName", courseRegSetting.SemesterId);
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName", courseRegSetting.SessionId);
            return View(courseRegSetting);
        }

        // GET: CourseRegSettings/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CourseRegSetting courseRegSetting = await _db.CourseRegSettings.FindAsync(id);
            if (courseRegSetting == null)
            {
                return HttpNotFound();
            }
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes, "SchoolProgrammeId", "ProgrammeType", courseRegSetting.SchoolProgrammeId);
            ViewBag.SemesterId = new SelectList(_db.Semesters, "SemesterId", "SemesterName", courseRegSetting.SemesterId);
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName", courseRegSetting.SessionId);
            return View(courseRegSetting);
        }

        // POST: CourseRegSettings/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "CourseRegSettingId,SchoolProgrammeId,SessionId,SemesterId,ClosingDate,OpeningDate,IsActive")] CourseRegSetting courseRegSetting)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(courseRegSetting).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes, "SchoolProgrammeId", "ProgrammeType", courseRegSetting.SchoolProgrammeId);
            ViewBag.SemesterId = new SelectList(_db.Semesters, "SemesterId", "SemesterName", courseRegSetting.SemesterId);
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName", courseRegSetting.SessionId);
            return View(courseRegSetting);
        }

        // GET: CourseRegSettings/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CourseRegSetting courseRegSetting = await _db.CourseRegSettings.FindAsync(id);
            if (courseRegSetting == null)
            {
                return HttpNotFound();
            }
            return View(courseRegSetting);
        }

        // POST: CourseRegSettings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            CourseRegSetting courseRegSetting = await _db.CourseRegSettings.FindAsync(id);
            _db.CourseRegSettings.Remove(courseRegSetting);
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
