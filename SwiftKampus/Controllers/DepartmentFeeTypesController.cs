using Microsoft.AspNet.Identity;
using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampus.ViewModels;
using SwiftKampusModel;
using SwiftKampusModel.Payment;
using System;
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
    public class DepartmentFeeTypesController : BaseController
    {

        public DepartmentFeeTypesController(SchoolDbContext db) : base(db)
        {

        }

        public ActionResult Index()
        {
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            var studentType = from StudentType s in Enum.GetValues(typeof(StudentType))
                              select new { ID = s, Name = s.ToString() };

            ViewBag.StudentType = new SelectList(_db.SchoolProgrammes, "SchoolProgrammeId", "FancyName");
            ViewBag.LevelId = new SelectList(_db.Levels.AsNoTracking(), "LevelId", "LevelName");
            ViewBag.DepartmentId = new SelectList(_db.Departments.AsNoTracking().ToList(), "DepartmentId", "DeptName");
            return View();
        }
        public async Task<ActionResult> GetIndex(int? LevelId, int? DepartmentId, int? StudentType, int? SessionId)
        {
            // dc.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key

            var deptFeeType = new List<DepartmentFeeType>();
            if (DepartmentId != null)
            {
                deptFeeType = await _db.DepartmentFeeTypes.AsNoTracking().Include(c => c.Level)
                                .Include(c => c.Department).Include(i => i.Session).Include(i => i.SchoolProgramme)
                           .Where(x => x.Department.DepartmentId.Equals((int)DepartmentId)).ToListAsync();
            }

            if (LevelId != null)
            {
                deptFeeType = deptFeeType.Where(x => x.Level.LevelId.Equals((int)LevelId)).ToList();
            }

            if (SessionId != null)
            {
                deptFeeType = deptFeeType.Where(x => x.Session.SessionId.Equals(SessionId)).ToList();
            }
            if (StudentType != null)
            {
                deptFeeType = deptFeeType.Where(x => x.SchoolProgrammeId.Equals((int)StudentType)).ToList();
            }

            if (DepartmentId == null && LevelId == null && SessionId == null && StudentType == null)
            {
                deptFeeType = await _db.DepartmentFeeTypes.AsNoTracking().Include(c => c.Level).Include(c => c.Department).Include(c => c.SchoolProgramme)
                                        .Include(i => i.Session).ToListAsync();
            }
            var data = deptFeeType.Select(s => new
            {
                s.DepartmentFeeTypeId,
                s.Department.DeptName,
                s.Amount,
                s.AmountInWords,
                s.FeeName,
                s.Level.LevelName,
                s.Description,
                s.SchoolProgramme.FancyName,
                s.Session.SessionName

            }).ToList();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        public async Task<PartialViewResult> Save(int id)
        {
            var departmentFeeType = await _db.DepartmentFeeTypes.FindAsync(id);

            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            ViewBag.SchoolProgrammeId = new MultiSelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FancyName");

            ViewBag.LevelId = new MultiSelectList(_db.Levels.AsNoTracking(), "LevelId", "LevelName");
            if (Request.IsAuthenticated && User.IsInRole(RoleName.Hod))
            {
                var staffDept = await _db.Staffs.Include(i => i.Department).AsNoTracking()
                    .Where(x => x.Email.Equals(userId))
                    .Select(s => s.Department.DepartmentId).FirstOrDefaultAsync();
                ViewBag.DepartmentId = new SelectList(await _db.Departments.AsNoTracking()
                    .Where(x => x.DepartmentId.Equals(staffDept)).ToListAsync(), "DepartmentId", "DeptName");

            }
            else
            {
                ViewBag.DepartmentId = new SelectList(await _db.Departments.AsNoTracking().ToListAsync(), "DepartmentId", "DeptName");

            }
            var deptFeeTypeVm = new DepartmentFeeTypeVm();
            if (departmentFeeType != null)
            {
                deptFeeTypeVm.DepartmentFeeTypeId = departmentFeeType.DepartmentFeeTypeId;
                deptFeeTypeVm.FeeName = departmentFeeType.FeeName;
                deptFeeTypeVm.Amount = departmentFeeType.Amount;
                deptFeeTypeVm.AmountInWords = departmentFeeType.AmountInWords;
                deptFeeTypeVm.SchoolProgrammeId = departmentFeeType.SchoolProgrammeId;
                deptFeeTypeVm.Description = departmentFeeType.Description;
            }
            return PartialView(deptFeeTypeVm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Save(DepartmentFeeTypeVm model)
        {
            bool status = false;
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                if (model.DepartmentFeeTypeId > 0)
                {
                    var deptFeeType = await _db.DepartmentFeeTypes.FindAsync(model.DepartmentFeeTypeId);
                    if (deptFeeType != null)
                    {
                        deptFeeType.DepartmentFeeTypeId = model.DepartmentFeeTypeId;
                        deptFeeType.FeeName = model.FeeName;
                        deptFeeType.Amount = model.Amount;
                        deptFeeType.AmountInWords = model.AmountInWords;
                        deptFeeType.SchoolProgrammeId = model.SchoolProgrammeId;
                        deptFeeType.LevelId = model.LevelId[0];
                        deptFeeType.DepartmentId = model.DepartmentId;
                        deptFeeType.SessionId = model.SessionId;
                        deptFeeType.Description = model.Description;

                        _db.Entry(deptFeeType).State = System.Data.Entity.EntityState.Modified;
                        await _db.SaveChangesAsync();
                        message = $"{model.FeeName} Updated Successfully...";
                        return new JsonResult { Data = new { status = true, message } };
                    }
                }
                else
                {

                    foreach (var level in model.LevelId)
                    {
                        var deptFeeType = new DepartmentFeeType
                        {
                            DepartmentId = model.DepartmentId,
                            FeeName = model.FeeName,
                            Amount = model.Amount,
                            AmountInWords = model.AmountInWords,
                            SchoolProgrammeId = model.SchoolProgrammeId,
                            LevelId = level,
                            SessionId = model.SessionId,
                            Description = model.Description
                        };
                        _db.DepartmentFeeTypes.Add(deptFeeType);
                    }
                    await _db.SaveChangesAsync();
                    message = $"{model.FeeName} Added Successfully.";
                    return new JsonResult { Data = new { status = true, message } };
                }
            }
            return new JsonResult { Data = new { status, message = "Please enter the data required Correctly" } };
            //return View(subject);
        }

        // GET: DepartmentFeeTypes/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DepartmentFeeType departmentFeeType = await _db.DepartmentFeeTypes.FindAsync(id);
            if (departmentFeeType == null)
            {
                return HttpNotFound();
            }
            return View(departmentFeeType);
        }

        // GET: DepartmentFeeTypes/Create
        public async Task<ActionResult> Create()
        {
            if (User.IsInRole(RoleName.Admin))
            {
                ViewBag.DepartmentId = new SelectList(_db.Departments, "DepartmentId", "DeptName");

            }
            else
            {
                var staffId = User.Identity.GetUserName();
                var staff = await _db.Staffs.Where(x => x.StaffId.Equals(staffId))
                    .Select(s => s.DepartmentId).FirstOrDefaultAsync();
                ViewBag.DepartmentId = new SelectList(_db.Departments.AsNoTracking().Where(x => x.DepartmentId.Equals((int)staff)), "DepartmentId", "DeptName");
            }
            ViewBag.SchoolProgrammeId = new MultiSelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FancyName");

            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            ViewBag.LevelId = new SelectList(_db.Levels.AsNoTracking(), "LevelId", "LevelName");
            return View();
        }

        // POST: DepartmentFeeTypes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(DepartmentFeeTypeVm model)
        {
            if (ModelState.IsValid)
            {
                foreach (var item in model.LevelId)
                {
                    var departmentFeeType = new DepartmentFeeType
                    {
                        DepartmentId = model.DepartmentId,
                        FeeName = model.FeeName,
                        Amount = model.Amount,
                        AmountInWords = model.AmountInWords,
                        SessionId = model.SessionId,
                        SchoolProgrammeId = model.SchoolProgrammeId,
                        Description = model.Description,
                        LevelId = item
                    };
                    _db.DepartmentFeeTypes.Add(departmentFeeType);
                }

                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            if (User.IsInRole(RoleName.Admin))
            {
                ViewBag.DepartmentId = new SelectList(_db.Departments, "DepartmentId", "DeptName", model.DepartmentId);
            }
            else
            {
                var staffId = User.Identity.GetUserName();
                var staff = await _db.Staffs.Where(x => x.StaffId.Equals(staffId))
                    .Select(s => s.DepartmentId).FirstOrDefaultAsync();
                ViewBag.DepartmentId = new SelectList(_db.Departments.AsNoTracking().Where(x => x.DepartmentId.Equals((int)staff)), "DepartmentId", "DeptName", model.DepartmentId);
            }
            ViewBag.SchoolProgrammeId = new MultiSelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FancyName");

            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName", model.SessionId);
            ViewBag.LevelId = new SelectList(_db.Levels.AsNoTracking(), "LevelId", "LevelName");
            return View(model);
        }

        // GET: DepartmentFeeTypes/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DepartmentFeeType departmentFeeType = await _db.DepartmentFeeTypes.FindAsync(id);
            if (departmentFeeType == null)
            {
                return HttpNotFound();
            }
            if (User.IsInRole(RoleName.Admin))
            {
                ViewBag.DepartmentId = new SelectList(_db.Departments, "DepartmentId", "DeptName", departmentFeeType.DepartmentId);

            }
            else
            {
                var staffId = User.Identity.GetUserName();
                var staff = await _db.Staffs.Where(x => x.StaffId.Equals(staffId))
                    .Select(s => s.DepartmentId).FirstOrDefaultAsync();
                ViewBag.DepartmentId = new SelectList(_db.Departments.AsNoTracking().Where(x => x.DepartmentId.Equals((int)staff)), "DepartmentId", "DeptName", departmentFeeType.DepartmentId);
            }
            var studentType = from StudentType s in Enum.GetValues(typeof(StudentType))
                              select new { ID = s, Name = s.ToString() };

            ViewBag.StudentType = new MultiSelectList(studentType, "Name", "Name");
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName", departmentFeeType.SessionId);
            ViewBag.LevelId = new SelectList(_db.Levels.AsNoTracking(), "LevelId", "LevelName");
            return View(departmentFeeType);
        }

        // POST: DepartmentFeeTypes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(DepartmentFeeType departmentFeeType)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(departmentFeeType).State = System.Data.Entity.EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            if (User.IsInRole(RoleName.Admin))
            {
                ViewBag.DepartmentId = new SelectList(_db.Departments, "DepartmentId", "DeptName", departmentFeeType.DepartmentId);

            }
            else
            {
                var staffId = User.Identity.GetUserName();
                var staff = await _db.Staffs.Where(x => x.StaffId.Equals(staffId))
                    .Select(s => s.DepartmentId).FirstOrDefaultAsync();
                ViewBag.DepartmentId = new SelectList(_db.Departments.AsNoTracking().Where(x => x.DepartmentId.Equals((int)staff)), "DepartmentId", "DeptName", departmentFeeType.DepartmentId);
            }
            var studentType = from StudentType s in Enum.GetValues(typeof(StudentType))
                              select new { ID = s, Name = s.ToString() };

            ViewBag.StudentType = new MultiSelectList(studentType, "Name", "Name");
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName", departmentFeeType.SessionId);
            ViewBag.LevelId = new SelectList(_db.Levels.AsNoTracking(), "LevelId", "LevelName");
            return View(departmentFeeType);
        }

        // GET: DepartmentFeeTypes/Delete/5
        public async Task<PartialViewResult> Delete(int id)
        {
            var deptFee = await _db.DepartmentFeeTypes.FindAsync(id);
            return PartialView(deptFee);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            bool status = false;
            string message = string.Empty;
            var deptName = await _db.DepartmentFeeTypes.FindAsync(id);
            if (deptName != null)
            {
                _db.DepartmentFeeTypes.Remove(deptName);
                await _db.SaveChangesAsync();
                status = true;
                message = "Department Fee Type Deleted Successfully...";
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
