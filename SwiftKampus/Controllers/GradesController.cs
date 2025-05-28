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
    public class GradesController : BaseController
    {

        public GradesController(SchoolDbContext db) : base(db)
        {

        }

        // GET: Grades
        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> GetIndex()
        {
            // dc.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var data = await _db.Grades.Include(i => i.SchoolProgramme).Include(i => i.ResultTemplate)
                        .AsNoTracking().Select(s => new
                        {
                            s.SchoolProgramme.FancyName,
                            s.GradeId,
                            s.GradeName,
                            s.GradePoint,
                            s.MaximumValue,
                            s.MinimumValue,
                            ResultTemplateName = s.ResultTemplate.FancyName,
                            s.Remark
                        }).ToListAsync();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        public async Task<PartialViewResult> Save(int id)
        {
            var grade = await _db.Grades.FindAsync(id);
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FancyName");
            ViewBag.ResultTemplateId = new SelectList(_db.ResultTemplates.AsNoTracking(), "ResultTemplateId", "FancyName");

            return PartialView(grade);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Save(Grade model)
        {
            bool status = false;
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                if (model.GradeId > 0)
                {
                    var grade = await _db.Grades.FindAsync(model.GradeId);
                    if (grade != null)
                    {
                        grade.SchoolProgrammeId = model.SchoolProgrammeId;
                        grade.ResultTemplateId = model.ResultTemplateId;
                        grade.GradeName = model.GradeName;
                        grade.GradePoint = model.GradePoint;
                        grade.MaximumValue = model.MaximumValue;
                        grade.MinimumValue = model.MinimumValue;
                        grade.Remark = model.Remark;
                        _db.Entry(grade).State = EntityState.Modified;
                        await _db.SaveChangesAsync();
                        message = $"{model.GradeName} Updated Successfully...";
                        return new JsonResult { Data = new { status = true, message } };
                    }
                }
                else
                {
                    _db.Grades.Add(model);
                    await _db.SaveChangesAsync();
                    message = $"{model.GradeName} Added Successfully.";
                    return new JsonResult { Data = new { status = true, message } };
                }
            }
            return new JsonResult { Data = new { status, message } };
            //return View(subject);
        }
        // GET: Grades/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Grade grade = await _db.Grades.FindAsync(id);
            if (grade == null)
            {
                return HttpNotFound();
            }
            return View(grade);
        }

        // GET: Grades/Create
        public ActionResult Create()
        {

            return View();
        }

        // POST: Grades/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "GradeId,GradeName,MinimumValue,MaximumValue,GradePoint,Remark")] Grade grade)
        {
            if (ModelState.IsValid)
            {
                _db.Grades.Add(grade);
                await _db.SaveChangesAsync();
                TempData["UserMessage"] = "Grade is Added Successfully.";
                TempData["Title"] = "Success.";

                return RedirectToAction("Create");
            }


            return View(grade);
        }

        // GET: Grades/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Grade grade = await _db.Grades.FindAsync(id);
            if (grade == null)
            {
                return HttpNotFound();
            }

            return View(grade);
        }

        // POST: Grades/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "GradeId,GradeName,MinimumValue,MaximumValue,GradePoint,Remark,FacultyId")] Grade grade)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(grade).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                TempData["UserMessage"] = "Grade is Updated Successfully.";
                TempData["Title"] = "Success.";
                return RedirectToAction("Index");
            }

            return View(grade);
        }

        // GET: Grades/Delete/5
        public async Task<PartialViewResult> Delete(int id)
        {
            var grade = await _db.Grades.FindAsync(id);
            return PartialView(grade);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            bool status = false;
            string message = string.Empty;
            var grade = await _db.Grades.FindAsync(id);
            if (grade != null)
            {
                _db.Grades.Remove(grade);
                await _db.SaveChangesAsync();
                status = true;
                message = "Grade Deleted Successfully...";
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
