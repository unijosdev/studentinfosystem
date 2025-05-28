using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampusModel.TimeTable;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SwiftKampus.Controllers
{
    [Authorize]
    [Audit(AuditingLevel = 2)]
    public class TimeTablePeriodsController : BaseController
    {

        public TimeTablePeriodsController(SchoolDbContext db) : base(db)
        {

        }
        // GET: TimeTablePeriods
        public ActionResult Index()
        {
            //var timeTablePeriods = _db.TimeTablePeriods.Include(t => t.Semester).Include(t => t.Session);
            return View();
        }
        public async Task<ActionResult> GetIndex()
        {
            // dc.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var timeTablePeriods = _db.TimeTablePeriods.Include(t => t.Semester).Include(t => t.Session);
            var data = await timeTablePeriods.Select(s => new
            {
                s.Session.SessionName,
                s.Semester.SemesterName,
                StartDate = s.StartDate.ToString(),
                EndDate = s.EndDate.ToString(),
                s.Name,
                s.TimeTablePeriodId
            }).ToListAsync();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        public async Task<PartialViewResult> Save(int id)
        {
            var timeTablePeriod = await _db.TimeTablePeriods.FindAsync(id);
            ViewBag.SemesterId = new SelectList(_db.Semesters, "SemesterId", "SemesterName");
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes, "SchoolProgrammeId", "FancyName");
            return PartialView(timeTablePeriod);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Save(TimeTablePeriod model)
        {
            bool status = false;
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                if (model.TimeTablePeriodId > 0)
                {
                    var timeTablePeriod = await _db.TimeTablePeriods.FindAsync(model.TimeTablePeriodId);
                    if (timeTablePeriod != null)
                    {
                        try
                        {
                            timeTablePeriod.SemesterId = model.SemesterId;
                            timeTablePeriod.SessionId = model.SessionId;
                            timeTablePeriod.EndDate = model.EndDate.Date;
                            timeTablePeriod.StartDate = model.StartDate;
                            timeTablePeriod.Name = model.Name;
                            timeTablePeriod.SchoolProgrammeId = model.SchoolProgrammeId;

                            _db.Entry(timeTablePeriod).State = System.Data.Entity.EntityState.Modified;
                            await _db.SaveChangesAsync();
                            message = $"{model.Name} Updated Successfully...";
                            return new JsonResult { Data = new { status = true, message } };
                        }
                        catch (Exception ex)
                        {
                            return new JsonResult { Data = new { status = false, message = ex.Message } };
                        }
                    }
                }
                else
                {
                    model.StartDate = model.StartDate.Date;
                    model.EndDate = model.EndDate.Date;
                    _db.TimeTablePeriods.Add(model);
                    await _db.SaveChangesAsync();
                    message = $"{model.Name} Added Successfully.";
                    return new JsonResult { Data = new { status = true, message } };
                }
            }
            return new JsonResult { Data = new { status, message } };
            //return View(subject);
        }

        #region MyRegion
        //// GET: TimeTablePeriods/Details/5
        //public async Task<ActionResult> Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    TimeTablePeriod timeTablePeriod = await _db.TimeTablePeriods.FindAsync(id);
        //    if (timeTablePeriod == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(timeTablePeriod);
        //}

        //// GET: TimeTablePeriods/Create
        //public ActionResult Create()
        //{
        //    ViewBag.SemesterId = new SelectList(_db.Semesters, "SemesterId", "SemesterName");
        //    ViewBag.SessionId = new SelectList(_db.Sessions, "SessionId", "SessionName");
        //    return View();
        //}

        //// POST: TimeTablePeriods/Create
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Create(TimeTablePeriod timeTablePeriod)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _db.TimeTablePeriods.Add(timeTablePeriod);
        //        await _db.SaveChangesAsync();
        //        return RedirectToAction("Index");
        //    }

        //    ViewBag.SemesterId = new SelectList(_db.Semesters, "SemesterId", "SemesterName", timeTablePeriod.SemesterId);
        //    ViewBag.SessionId = new SelectList(_db.Sessions, "SessionId", "SessionName", timeTablePeriod.SessionId);
        //    return View(timeTablePeriod);
        //}

        //// GET: TimeTablePeriods/Edit/5
        //public async Task<ActionResult> Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    TimeTablePeriod timeTablePeriod = await _db.TimeTablePeriods.FindAsync(id);
        //    if (timeTablePeriod == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    ViewBag.SemesterId = new SelectList(_db.Semesters, "SemesterId", "SemesterName", timeTablePeriod.SemesterId);
        //    ViewBag.SessionId = new SelectList(_db.Sessions, "SessionId", "SessionName", timeTablePeriod.SessionId);
        //    return View(timeTablePeriod);
        //}

        //// POST: TimeTablePeriods/Edit/5
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Edit(TimeTablePeriod timeTablePeriod)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _db.Entry(timeTablePeriod).State = System.Data.Entity.EntityState.Modified;
        //        await _db.SaveChangesAsync();
        //        return RedirectToAction("Index");
        //    }
        //    ViewBag.SemesterId = new SelectList(_db.Semesters, "SemesterId", "SemesterName", timeTablePeriod.SemesterId);
        //    ViewBag.SessionId = new SelectList(_db.Sessions, "SessionId", "SessionName", timeTablePeriod.SessionId);
        //    return View(timeTablePeriod);
        //} 
        #endregion

        // GET: TimeTablePeriods/Delete/5
        public async Task<PartialViewResult> Delete(int id)
        {
            var timeTablePeriod = await _db.TimeTablePeriods.FindAsync(id);
            return PartialView(timeTablePeriod);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            bool status = false;
            string message = string.Empty;
            var timeTablePeriod = await _db.TimeTablePeriods.FindAsync(id);
            if (timeTablePeriod != null)
            {
                _db.TimeTablePeriods.Remove(timeTablePeriod);
                await _db.SaveChangesAsync();
                status = true;
                message = "Time Table Schedule Deleted Successfully...";
                return new JsonResult { Data = new { status, message } };
            }

            return new JsonResult { Data = new { status = false, message = $"{id } not deleted" } };
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
