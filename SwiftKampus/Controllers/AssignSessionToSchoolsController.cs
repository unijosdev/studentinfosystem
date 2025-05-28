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
    public class AssignSessionToSchoolsController : BaseController
    {       
        public AssignSessionToSchoolsController(SchoolDbContext _db) : base(_db)
        {
        }

        // GET: AssignSessionToSchools
        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> GetIndex()
        {
            // dc.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var assignSession = await _db.AssignSessionToSchools.Include(a => a.SchoolProgramme).Include(a => a.Session)
                                .AsNoTracking().ToListAsync();
            var data = assignSession.Select(s => new
            {
                s.Session.SessionName,
                s.SchoolProgramme.FancyName,
                s.ActiveSession,
                s.AssignSessionToSchoolId
            }).ToList();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }


        // GET: AssignSessionToSchools/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AssignSessionToSchool assignSessionToSchool = await _db.AssignSessionToSchools.FindAsync(id);
            if (assignSessionToSchool == null)
            {
                return HttpNotFound();
            }
            return View(assignSessionToSchool);
        }

        // GET: AssignSessionToSchools/Create
        public async Task<PartialViewResult> Save(int id)
        {
            var session = await _db.AssignSessionToSchools.FindAsync(id);
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName", session?.SessionId);
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FancyName", session?.SchoolProgrammeId);
    
            return PartialView(session);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Save(AssignSessionToSchool model)
        {
            bool status = false;
            string message = string.Empty;
            if (model.ActiveSession.Equals(true))
            {
                var checkForActiveSession = await _db.AssignSessionToSchools.AsNoTracking()
                                       .Where(x => x.SchoolProgrammeId.Equals(model.SchoolProgrammeId)
                                       && x.ActiveSession.Equals(true)).FirstOrDefaultAsync();
                if (checkForActiveSession != null)
                {
                    message = "There is an active session for the school programme";
                    return new JsonResult { Data = new { status = false, message } };
                }
            }
           
            
            if (ModelState.IsValid)
            {
                if (model.AssignSessionToSchoolId > 0)
                {
                    var assignSessionToSchool = await _db.AssignSessionToSchools.FindAsync(model.AssignSessionToSchoolId);
                    if (assignSessionToSchool != null)
                    {
                        try
                        {
                            assignSessionToSchool.SessionId = model.SessionId;
                            assignSessionToSchool.SchoolProgrammeId = model.SchoolProgrammeId;
                            assignSessionToSchool.ActiveSession = model.ActiveSession;
                            _db.Entry(assignSessionToSchool).State = EntityState.Modified;
                            await _db.SaveChangesAsync();
                            message = "Session Updated Successfully...";
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
                    var checkExist = _db.AssignSessionToSchools.AsNoTracking()
                                        .Where(x => x.SchoolProgrammeId.Equals(model.SchoolProgrammeId) &&
                                        x.SessionId.Equals(model.SessionId)).FirstOrDefault();
                    if(checkExist != null)
                    {
                        message = "Session is already assigned to this school Programme";
                        return new JsonResult { Data = new { status = false, message } };
                    }

                    _db.AssignSessionToSchools.Add(model);
                    await _db.SaveChangesAsync();
                    message = $"Session is assigned to school Successfully.";
                    return new JsonResult { Data = new { status = true, message } };
                }
            }
            return new JsonResult { Data = new { status, message } };
            //return View(subject);
        }

        // GET: AssignSessionToSchools/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AssignSessionToSchool assignSessionToSchool = await _db.AssignSessionToSchools.FindAsync(id);
            if (assignSessionToSchool == null)
            {
                return HttpNotFound();
            }
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes, "SchoolProgrammeId", "ProgrammeType", assignSessionToSchool.SchoolProgrammeId);
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName", assignSessionToSchool.SessionId);
            return View(assignSessionToSchool);
        }

        // POST: AssignSessionToSchools/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "AssignSessionToSchoolId,SessionId,SchoolProgrammeId,ActiveSession")] AssignSessionToSchool assignSessionToSchool)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(assignSessionToSchool).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes, "SchoolProgrammeId", "ProgrammeType", assignSessionToSchool.SchoolProgrammeId);
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName", assignSessionToSchool.SessionId);
            return View(assignSessionToSchool);
        }

        // GET: AssignSessionToSchools/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AssignSessionToSchool assignSessionToSchool = await _db.AssignSessionToSchools.FindAsync(id);
            if (assignSessionToSchool == null)
            {
                return HttpNotFound();
            }
            return View(assignSessionToSchool);
        }

        // POST: AssignSessionToSchools/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            AssignSessionToSchool assignSessionToSchool = await _db.AssignSessionToSchools.FindAsync(id);
            _db.AssignSessionToSchools.Remove(assignSessionToSchool);
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
