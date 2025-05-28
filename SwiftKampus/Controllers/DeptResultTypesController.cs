using Microsoft.Ajax.Utilities;
using SwiftKampus.Models;
using SwiftKampusModel;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SwiftKampus.Controllers
{
    public class DeptResultTypesController : BaseController
    {
        public DeptResultTypesController(SchoolDbContext db) : base(db)
        {

        }
        // GET: DeptResultTypes
        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> GetIndex()
        {
            if (User.IsInRole("SuperAdmin") || User.IsInRole("Admin"))
            {
                var data = await _db.DeptResultTypes.Include(i => i.Session).Include(i => i.Department).AsNoTracking()
                                .Select(s => new
                                {
                                    s.Department.DeptName,
                                    ResultTypeName = s.ResultTemplate.FancyName,
                                    s.SchoolProgramme.FancyName,
                                    s.FailMark,
                                    s.ProbationMark,
                                    s.DepartmentId,
                                    s.Session.SessionName,
                                    s.DeptResultTypeId
                                }).ToListAsync();
                return Json(new { data }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var staffDeptId = await _db.Staffs.Include(x => x.Department).Where(x => x.Email == userId).Select(x => x.DepartmentId).FirstOrDefaultAsync();

                var data = await _db.DeptResultTypes.Include(i => i.Session).Include(i => i.Department).Where(i => i.DepartmentId == staffDeptId).AsNoTracking()
                                    .Select(s => new
                                    {
                                        s.Department.DeptName,
                                        ResultTypeName = s.ResultTemplate.FancyName,
                                        s.SchoolProgramme.FancyName,
                                        s.FailMark,
                                        s.ProbationMark,
                                        s.DepartmentId,
                                        s.Session.SessionName,
                                        s.DeptResultTypeId
                                    }).ToListAsync();
                return Json(new { data }, JsonRequestBehavior.AllowGet);
            }
        }


        public async Task<PartialViewResult> Save(int id)
        {
            var deptResultType = await _db.DeptResultTypes.FindAsync(id);

            if (User.IsInRole("SuperAdmin") || User.IsInRole("Admin"))
            {
                ViewBag.DepartmentId = new SelectList(_db.Departments.OrderBy(x => x.DeptName), "DepartmentId", "DeptName", deptResultType?.DepartmentId);
            }
            else
            {
                var staffDeptId = await _db.Staffs.Include(x => x.Department).Where(x => x.Email == userId).Select(x => x.DepartmentId).FirstOrDefaultAsync();

                ViewBag.DepartmentId = new SelectList(_db.Departments.Where(x => x.DepartmentId == staffDeptId), "DepartmentId", "DeptName", deptResultType?.DepartmentId);
            }
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName", deptResultType?.SessionId);
            ViewBag.ResultTemplateId = new SelectList(_db.ResultTemplates, "ResultTemplateId", "FancyName", deptResultType?.ResultTemplateId);
            ViewBag.ResultTemplateForHundred = new SelectList(_db.ResultTemplates, "ResultTemplateId", "FancyName", deptResultType?.ResultTemplateId);
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.OrderBy(x => x.SchoolProgrammeId), "SchoolProgrammeId", "FancyName", deptResultType?.SchoolProgrammeId);
            return PartialView(deptResultType);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Save(DeptResultType model)
        {
            bool status = false;
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                var checkForTemplateGrade = _db.Grades.AsNoTracking()
                                            .Count(x => x.ResultTemplateId.Equals(model.ResultTemplateId));
                if (checkForTemplateGrade > 2)
                {
                    if (model.DeptResultTypeId > 0)
                    {

                        var deptResultType = await _db.DeptResultTypes.FindAsync(model.DeptResultTypeId);
                        if (deptResultType != null)
                        {
                            deptResultType.DepartmentId = model.DepartmentId;
                            deptResultType.SchoolProgrammeId = model.SchoolProgrammeId;
                            deptResultType.ResultTemplateId = model.ResultTemplateId;
                            deptResultType.FailMark = model.FailMark;
                            deptResultType.ProbationMark = model.ProbationMark;
                            _db.Entry(deptResultType).State = EntityState.Modified;
                            await _db.SaveChangesAsync();
                            message = $"Dept Result Type Updated Successfully...";
                            return new JsonResult { Data = new { status = true, message } };
                        }
                    }
                    else
                    {
                        _db.DeptResultTypes.Add(model);
                        await _db.SaveChangesAsync();
                        message = "Dept Result Type Added Successfully.";
                        return new JsonResult { Data = new { status = true, message } };
                    }
                }
                message = "Grade has not been set by the appropriate authorities";
                return new JsonResult { Data = new { status, message } };
            }
            return new JsonResult { Data = new { status, message } };
            //return View(subject);
        }

        // GET: DeptResultTypes/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DeptResultType deptResultType = await _db.DeptResultTypes.FindAsync(id);
            if (deptResultType == null)
            {
                return HttpNotFound();
            }
            return View(deptResultType);
        }

        // GET: DeptResultTypes/Create
        public ActionResult Create()
        {
            ViewBag.DepartmentId = new SelectList(_db.Departments, "DepartmentId", "DeptCode");
            ViewBag.ResultTemplateId = new SelectList(_db.ResultTemplates, "ResultTemplateId", "ResultType");
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes, "SchoolProgrammeId", "ProgrammeType");
            return View();
        }

        // POST: DeptResultTypes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "DepartmentId,ResultTemplateId,SchoolProgrammeId")] DeptResultType deptResultType)
        {
            if (ModelState.IsValid)
            {
                _db.DeptResultTypes.Add(deptResultType);
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.DepartmentId = new SelectList(_db.Departments, "DepartmentId", "DeptCode", deptResultType.DepartmentId);
            ViewBag.ResultTemplateId = new SelectList(_db.ResultTemplates, "ResultTemplateId", "ResultType", deptResultType.ResultTemplateId);
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes, "SchoolProgrammeId", "ProgrammeType", deptResultType.SchoolProgrammeId);
            return View(deptResultType);
        }

        // GET: DeptResultTypes/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DeptResultType deptResultType = await _db.DeptResultTypes.FindAsync(id);
            if (deptResultType == null)
            {
                return HttpNotFound();
            }
            ViewBag.DepartmentId = new SelectList(_db.Departments, "DepartmentId", "DeptCode", deptResultType.DepartmentId);
            ViewBag.ResultTemplateId = new SelectList(_db.ResultTemplates, "ResultTemplateId", "ResultType", deptResultType.ResultTemplateId);
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes, "SchoolProgrammeId", "ProgrammeType", deptResultType.SchoolProgrammeId);
            return View(deptResultType);
        }

        // POST: DeptResultTypes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "DepartmentId,ResultTemplateId,SchoolProgrammeId")] DeptResultType deptResultType)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(deptResultType).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.DepartmentId = new SelectList(_db.Departments, "DepartmentId", "DeptCode", deptResultType.DepartmentId);
            ViewBag.ResultTemplateId = new SelectList(_db.ResultTemplates, "ResultTemplateId", "ResultType", deptResultType.ResultTemplateId);
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes, "SchoolProgrammeId", "ProgrammeType", deptResultType.SchoolProgrammeId);
            return View(deptResultType);
        }

        // GET: DeptResultTypes/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DeptResultType deptResultType = await _db.DeptResultTypes.FindAsync(id);
            if (deptResultType == null)
            {
                return HttpNotFound();
            }
            return View(deptResultType);
        }

        // POST: DeptResultTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            DeptResultType deptResultType = await _db.DeptResultTypes.FindAsync(id);
            _db.DeptResultTypes.Remove(deptResultType);
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
