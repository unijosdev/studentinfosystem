using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampusModel;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SwiftKampus.Controllers
{
    [Authorize]
    [Audit(AuditingLevel = 2)]
    public class CourseLoadSettingsController : BaseController
    {

        public CourseLoadSettingsController(SchoolDbContext db) : base(db)
        {

        }
        // GET: CourseLoadSettings
        public ActionResult Index(string message)
        {
            ViewBag.Message = message;
            return View();
        }
        public async Task<ActionResult> GetIndex()
        {
            // dc.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key

            //var data = await _db.CourseLoadSettings.Include(i => i.Programme).Include(i => i.Level)
            //    .AsNoTracking().Select(s => new
            //    {
            //        s.Programme.ProgrammeName,
            //        s.Level.LevelName,
            //        s.Session.SessionName,
            //        s.MaximumCreditLoad,
            //        s.MinimumCreditLoad,
            //        s.CourseLoadSettingId
            //    }).ToListAsync();

            if (User.IsInRole(RoleName.LCordinator) || User.IsInRole(RoleName.Hod))
            {
                var userEmail = User.Identity.Name;
                var deptId = _db.Staffs.Where(x => x.Email == userEmail).Select(x => x.DepartmentId).FirstOrDefault();
                var data = await _db.CourseLoadSettings.Include(i => i.Programme).Include(i => i.Level).Where(x => x.Programme.DepartmentId == deptId)
                    .AsNoTracking().Select(s => new
                    {
                        s.Programme.ProgrammeName,
                        s.Level.LevelName,
                        s.Session.SessionName,
                        s.MaximumCreditLoad,
                        s.MinimumCreditLoad,
                        s.CourseLoadSettingId
                    }).ToListAsync();

                return Json(new { data }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var data = await _db.CourseLoadSettings.Include(i => i.Programme).Include(i => i.Level)
                    .AsNoTracking().Select(s => new
                    {
                        s.Programme.ProgrammeName,
                        s.Level.LevelName,
                        s.Session.SessionName,
                        s.MaximumCreditLoad,
                        s.MinimumCreditLoad,
                        s.CourseLoadSettingId
                    }).ToListAsync();

                return Json(new { data }, JsonRequestBehavior.AllowGet);
            }
        }

        public async Task<PartialViewResult> Save(int id)
        {
            var courseLoadSetting = await _db.CourseLoadSettings.FindAsync(id);

            // show only programmes in a level cordinator or HOD's department
            if (User.IsInRole(RoleName.LCordinator) || User.IsInRole(RoleName.Hod) )
            {
                var userEmail = User.Identity.Name;
                var deptId = _db.Staffs.Where(x => x.Email == userEmail).Select(x => x.DepartmentId).FirstOrDefault();
                ViewBag.ProgrammeId = new SelectList(_db.Programmes.AsNoTracking().Where(x => x.DepartmentId == deptId).ToList(), "ProgrammeId", "ProgrammeName");
            }
            else { ViewBag.ProgrammeId = new SelectList(_db.Programmes.AsNoTracking(), "ProgrammeId", "ProgrammeName"); }
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            ViewBag.LevelId = new SelectList(_db.Levels.AsNoTracking(), "LevelId", "LevelName");
            return PartialView(courseLoadSetting);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Save(CourseLoadSetting model)
        {
            bool status = false;
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                if (model.CourseLoadSettingId > 0)
                {
                    var courseLoadSetting = await _db.CourseLoadSettings.FindAsync(model.CourseLoadSettingId);
                    if (courseLoadSetting != null)
                    {
                        courseLoadSetting.SessionId = model.SessionId;
                        courseLoadSetting.ProgrammeId = model.ProgrammeId;
                        courseLoadSetting.LevelId = model.LevelId;
                        courseLoadSetting.MaximumCreditLoad = model.MaximumCreditLoad;
                        courseLoadSetting.MinimumCreditLoad = model.MinimumCreditLoad;
                        _db.Entry(courseLoadSetting).State = EntityState.Modified;
                        await _db.SaveChangesAsync();
                        message = "Course Load Updated Successfully...";
                        return new JsonResult { Data = new { status = true, message } };
                    }
                }
                else
                {
                    var checkCourseLoad = _db.CourseLoadSettings.Include(i => i.Session).AsNoTracking()
                                        .Any(x => x.ProgrammeId.Equals(model.ProgrammeId)
                                        && x.LevelId.Equals(model.LevelId)
                                        && x.Session.SessionId.Equals((int)model.SessionId));
                    if (checkCourseLoad)
                    {
                        message = "Sorry, you cant add thesame setting for a department option in thesame session.";
                        return new JsonResult { Data = new { status = false, message } };
                    }

                    _db.CourseLoadSettings.Add(model);
                    await _db.SaveChangesAsync();
                    message = "Course Load setting is Added Successfully.";
                    return new JsonResult { Data = new { status = true, message } };
                }
            }
            return new JsonResult { Data = new { status, message } };
            //return View(subject);
        }



        // GET: CourseLoadSettings/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CourseLoadSetting courseLoadSetting = await _db.CourseLoadSettings.FindAsync(id);
            if (courseLoadSetting == null)
            {
                return HttpNotFound();
            }
            return View(courseLoadSetting);
        }

        //// GET: CourseLoadSettings/Create
        //public ActionResult Create()
        //{
        //    ViewBag.DepartmentId = new SelectList(_db.Departments, "DepartmentId", "DeptCode");
        //    ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
        //    return View();
        //}

        //// POST: CourseLoadSettings/Create
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Create([Bind(Include = "CourseLoadSettingId,DepartmentId,SessionId,MaximumCreditLoad,MinimumCreditLoad")] CourseLoadSetting courseLoadSetting)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _db.CourseLoadSettings.Add(courseLoadSetting);
        //        await _db.SaveChangesAsync();
        //        return RedirectToAction("Index");
        //    }

        //    ViewBag.DepartmentId = new SelectList(_db.Departments, "DepartmentId", "DeptCode", courseLoadSetting.DepartmentId);
        //    ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName", courseLoadSetting.SessionId);
        //    return View(courseLoadSetting);
        //}

        //// GET: CourseLoadSettings/Edit/5
        //public async Task<ActionResult> Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    CourseLoadSetting courseLoadSetting = await _db.CourseLoadSettings.FindAsync(id);
        //    if (courseLoadSetting == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    ViewBag.DepartmentId = new SelectList(_db.Departments, "DepartmentId", "DeptCode", courseLoadSetting.DepartmentId);
        //    ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName", courseLoadSetting.SessionId);
        //    return View(courseLoadSetting);
        //}

        //// POST: CourseLoadSettings/Edit/5
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Edit([Bind(Include = "CourseLoadSettingId,DepartmentId,SessionId,MaximumCreditLoad,MinimumCreditLoad")] CourseLoadSetting courseLoadSetting)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _db.Entry(courseLoadSetting).State = EntityState.Modified;
        //        await _db.SaveChangesAsync();
        //        return RedirectToAction("Index");
        //    }
        //    ViewBag.DepartmentId = new SelectList(_db.Departments, "DepartmentId", "DeptCode", courseLoadSetting.DepartmentId);
        //    ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName", courseLoadSetting.SessionId);
        //    return View(courseLoadSetting);
        //}

        // GET: CourseLoadSettings/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CourseLoadSetting courseLoadSetting = await _db.CourseLoadSettings.FindAsync(id);
            if (courseLoadSetting == null)
            {
                return HttpNotFound();
            }
            return View(courseLoadSetting);
        }

        // POST: CourseLoadSettings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            CourseLoadSetting courseLoadSetting = await _db.CourseLoadSettings.FindAsync(id);
            _db.CourseLoadSettings.Remove(courseLoadSetting);
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
