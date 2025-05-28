using System.Data.Entity;
using System.Threading.Tasks;
using System.Net;
using System.Web.Mvc;
using SwiftKampus.Models;
using SwiftKampusModel;
using System.Linq;
using System;
using SwiftKampus.Services;

namespace SwiftKampus.Controllers
{
    [Authorize]
    [Audit(AuditingLevel = 2)]
    public class AssignSemesterToSchoolsController : BaseController
    {
        public AssignSemesterToSchoolsController(SchoolDbContext _db) : base(_db)
        {
        }

        // GET: AssignSemesterToSchools
        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> GetIndex()
        {
            // dc.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var assignSemester = await _db.AssignSemesterToSchools.Include(a => a.SchoolProgramme).Include(a => a.Semester).AsNoTracking().ToListAsync();
            var data = assignSemester.Select(s => new
            {
                s.Semester.SemesterName,
                s.SchoolProgramme.FancyName,
                s.ActiveSemester,
                s.AssignSemesterToSchoolId
            }).ToList();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        // GET: AssignSemesterToSchools/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AssignSemesterToSchool assignSemesterToSchool = await _db.AssignSemesterToSchools.FindAsync(id);
            if (assignSemesterToSchool == null)
            {
                return HttpNotFound();
            }
            return View(assignSemesterToSchool);
        }

        // GET: AssignSemesterToSchools/Create
        public async Task<PartialViewResult> Save(int id)
        {
            var semester = await _db.AssignSemesterToSchools.FindAsync(id);
            ViewBag.SemesterId = new SelectList(_db.Semesters, "SemesterId", "SemesterName", semester?.SemesterId);
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FancyName", semester?.SchoolProgrammeId);
         
            return PartialView(semester);
         
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Save(AssignSemesterToSchool model)
        {
            bool status = false;
            string message = string.Empty;
            if (model.ActiveSemester.Equals(true))
            {
                var checkForActiveSemester = await _db.AssignSemesterToSchools.AsNoTracking()
                                       .Where(x => x.SchoolProgrammeId.Equals(model.SchoolProgrammeId)
                                       && x.ActiveSemester.Equals(true)).FirstOrDefaultAsync();
                if (checkForActiveSemester != null)
                {
                    message = "There is an active semester for the school programme";
                    return new JsonResult { Data = new { status = false, message } };
                }
            }
           
            if (ModelState.IsValid)
            {
                if (model.AssignSemesterToSchoolId > 0)
                {
                    var assignSemesterToSchool = await _db.AssignSemesterToSchools.FindAsync(model.AssignSemesterToSchoolId);
                    if (assignSemesterToSchool != null)
                    {
                        try
                        {
                            assignSemesterToSchool.SemesterId = model.SemesterId;
                            assignSemesterToSchool.SchoolProgrammeId = model.SchoolProgrammeId;
                            assignSemesterToSchool.ActiveSemester = model.ActiveSemester;
                            _db.Entry(assignSemesterToSchool).State = EntityState.Modified;
                            await _db.SaveChangesAsync();
                            message = "Semester Updated Successfully...";
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
                    var checkExist = _db.AssignSemesterToSchools.AsNoTracking()
                                      .Where(x => x.SchoolProgrammeId.Equals(model.SchoolProgrammeId) &&
                                      x.SemesterId.Equals(model.SemesterId)).FirstOrDefault();
                    if (checkExist != null)
                    {
                        message = "Semester is already assigned to this school Programme";
                        return new JsonResult { Data = new { status = false, message } };
                    }


                    _db.AssignSemesterToSchools.Add(model);
                    await _db.SaveChangesAsync();
                    message = $"semester is assigned to school Successfully.";
                    return new JsonResult { Data = new { status = true, message } };
                }
            }
            return new JsonResult { Data = new { status, message } };
            //return View(subject);
        }

        // GET: AssignSemesterToSchools/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AssignSemesterToSchool assignSemesterToSchool = await _db.AssignSemesterToSchools.FindAsync(id);
            if (assignSemesterToSchool == null)
            {
                return HttpNotFound();
            }
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes, "SchoolProgrammeId", "ProgrammeType", assignSemesterToSchool.SchoolProgrammeId);
            ViewBag.SemesterId = new SelectList(_db.Semesters, "SemesterId", "SemesterName", assignSemesterToSchool.SemesterId);
            return View(assignSemesterToSchool);
        }

        // POST: AssignSemesterToSchools/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "AssignSemesterToSchoolId,SemesterId,SchoolProgrammeId,ActiveSemester")] AssignSemesterToSchool assignSemesterToSchool)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(assignSemesterToSchool).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes, "SchoolProgrammeId", "ProgrammeType", assignSemesterToSchool.SchoolProgrammeId);
            ViewBag.SemesterId = new SelectList(_db.Semesters, "SemesterId", "SemesterName", assignSemesterToSchool.SemesterId);
            return View(assignSemesterToSchool);
        }

        // GET: AssignSemesterToSchools/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AssignSemesterToSchool assignSemesterToSchool = await _db.AssignSemesterToSchools.FindAsync(id);
            if (assignSemesterToSchool == null)
            {
                return HttpNotFound();
            }
            return View(assignSemesterToSchool);
        }

        // POST: AssignSemesterToSchools/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            AssignSemesterToSchool assignSemesterToSchool = await _db.AssignSemesterToSchools.FindAsync(id);
            _db.AssignSemesterToSchools.Remove(assignSemesterToSchool);
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
