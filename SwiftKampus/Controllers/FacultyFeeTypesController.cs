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
    public class FacultyFeeTypesController : BaseController
    {

        public FacultyFeeTypesController(SchoolDbContext db) : base(db)
        {

        }

        // GET: FacultyFeeTypes
        //public async Task<ActionResult> Index()
        //{
        //    var facultyFeeTypes = _db.FacultyFeeTypes.Include(f => f.Faculty).Include(f => f.Semester);
        //    return View(await facultyFeeTypes.ToListAsync());
        //}
        public ActionResult Index()
        {
            ViewBag.SemesterId = new SelectList(_db.Semesters, "SemesterId", "SemesterName");

            ViewBag.StudentType = new SelectList(_db.SchoolProgrammes, "SchoolProgrammeId", "FancyName");
            ViewBag.LevelId = new SelectList(_db.Levels.AsNoTracking(), "LevelId", "LevelName");
            ViewBag.FacultyId = new SelectList(_db.Faculties.AsNoTracking(), "FacultyId", "FacultyName");
            return View();
        }

        public async Task<ActionResult> GetIndex(int? LevelId, int? FacultyId, int? StudentType, int? SemesterId)
        {

            var facultyFeeType = new List<FacultyFeeType>();
            if (FacultyId != null)
            {
                facultyFeeType = await _db.FacultyFeeTypes.AsNoTracking().Include(c => c.Level)
                                .Include(c => c.Faculty).Include(i => i.Semester).Include(i => i.SchoolProgrammeId)
                           .Where(x => x.Faculty.FacultyId.Equals((int)FacultyId)).ToListAsync();
            }

            if (LevelId != null)
            {
                facultyFeeType = facultyFeeType.Where(x => x.Level.LevelId.Equals((int)LevelId)).ToList();
            }

            if (SemesterId != null)
            {
                facultyFeeType = facultyFeeType.Where(x => x.Semester.SemesterId.Equals(SemesterId)).ToList();
            }
            if (StudentType != null)
            {
                facultyFeeType = facultyFeeType.Where(x => x.SchoolProgrammeId.Equals((int)StudentType)).ToList();
            }

            if (FacultyId == null && LevelId == null && SemesterId == null && StudentType == null)
            {
                facultyFeeType = await _db.FacultyFeeTypes.AsNoTracking().Include(c => c.Level).Include(c => c.Faculty)
                                        .Include(i => i.Semester).ToListAsync();
            }
            // dc.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var data = facultyFeeType.Select(s => new
            {
                s.FacultyFeeTypeId,
                s.Faculty.FacultyName,
                s.Amount,
                s.AmountInWords,
                s.FeeName,
                s.Level.LevelName,
                s.Description,
                s.SchoolProgramme.FancyName,
                s.Semester.SemesterName

            }).ToList();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }
        public async Task<PartialViewResult> Save(int id)
        {
            var schoolFeeType = await _db.FacultyFeeTypes.FindAsync(id);

            ViewBag.SemesterId = new SelectList(_db.Semesters, "SemesterId", "SemesterName");


            ViewBag.SchoolProgrammeId = new MultiSelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FancyName");
            ViewBag.LevelId = new MultiSelectList(_db.Levels.AsNoTracking(), "LevelId", "LevelName");
            ViewBag.FacultyId = new MultiSelectList(_db.Faculties.AsNoTracking(), "FacultyId", "FacultyName");
            var facultyFeeType = new FacultyFeeTypeVm();
            if (schoolFeeType != null)
            {
                facultyFeeType.FacultyFeeTypeId = schoolFeeType.FacultyFeeTypeId;
                facultyFeeType.FeeName = schoolFeeType.FeeName;
                facultyFeeType.Amount = schoolFeeType.Amount;
                facultyFeeType.AmountInWords = schoolFeeType.AmountInWords;
                facultyFeeType.SchoolProgrammeId = schoolFeeType.SchoolProgrammeId;
                facultyFeeType.Description = schoolFeeType.Description;
            }
            return PartialView(facultyFeeType);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Save(FacultyFeeTypeVm model)
        {
            bool status = false;
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                if (model.FacultyFeeTypeId > 0)
                {
                    var facultyFeeType = await _db.FacultyFeeTypes.FindAsync(model.FacultyFeeTypeId);
                    if (facultyFeeType != null)
                    {
                        facultyFeeType.FacultyFeeTypeId = model.FacultyFeeTypeId;
                        facultyFeeType.FeeName = model.FeeName;
                        facultyFeeType.Amount = model.Amount;
                        facultyFeeType.AmountInWords = model.AmountInWords;
                        facultyFeeType.SchoolProgrammeId = model.SchoolProgrammeId;
                        facultyFeeType.LevelId = model.LevelId[0];
                        facultyFeeType.FacultyId = model.FacultyId;
                        facultyFeeType.SemesterId = model.SemesterId;
                        facultyFeeType.Description = model.Description;

                        _db.Entry(facultyFeeType).State = EntityState.Modified;
                        await _db.SaveChangesAsync();
                        message = $"{model.FeeName} Updated Successfully...";
                        return new JsonResult { Data = new { status = true, message } };
                    }
                }
                else
                {

                    foreach (var level in model.LevelId)
                    {
                        var facultyFeeType = new FacultyFeeType
                        {
                            FacultyFeeTypeId = model.FacultyFeeTypeId,
                            FeeName = model.FeeName,
                            Amount = model.Amount,
                            AmountInWords = model.AmountInWords,
                            SchoolProgrammeId = model.SchoolProgrammeId,
                            LevelId = level,
                            SemesterId = model.SemesterId,
                            FacultyId = model.FacultyId,
                            Description = model.Description
                        };
                        _db.FacultyFeeTypes.Add(facultyFeeType);
                    }
                    await _db.SaveChangesAsync();
                    message = $"{model.FeeName} Added Successfully.";
                    return new JsonResult { Data = new { status = true, message } };
                }
            }
            return new JsonResult { Data = new { status, message = "Please enter the data required Correctly" } };
            //return View(subject);
        }

        // GET: FacultyFeeTypes/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FacultyFeeType facultyFeeType = await _db.FacultyFeeTypes.FindAsync(id);
            if (facultyFeeType == null)
            {
                return HttpNotFound();
            }
            return View(facultyFeeType);
        }

        // GET: FacultyFeeTypes/Create
        public ActionResult Create()
        {
            ViewBag.FacultyId = new SelectList(_db.Faculties, "FacultyId", "FacultyName");
            ViewBag.SemesterId = new SelectList(_db.Semesters, "SemesterId", "SemesterName");
            ViewBag.SchoolProgrammeId = new MultiSelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FancyName");

            ViewBag.LevelId = new MultiSelectList(_db.Levels.AsNoTracking(), "LevelId", "LevelName");
            return View();
        }

        // POST: FacultyFeeTypes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(FacultyFeeTypeVm model)
        {
            if (ModelState.IsValid)
            {
                foreach (var item in model.LevelId)
                {
                    var facultyFeeType = new FacultyFeeType
                    {

                        FacultyId = model.FacultyId,
                        FeeName = model.FeeName,
                        SemesterId = model.SemesterId,
                        Amount = model.Amount,
                        AmountInWords = model.AmountInWords,
                        SchoolProgrammeId = model.SchoolProgrammeId,
                        Description = model.Description,
                        LevelId = item
                    };
                    _db.FacultyFeeTypes.Add(facultyFeeType);
                }


                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.FacultyId = new SelectList(_db.Faculties, "FacultyId", "FacultyName", model.FacultyId);
            ViewBag.SemesterId = new SelectList(_db.Semesters, "SemesterId", "SemesterName", model.SemesterId);
            ViewBag.SchoolProgrammeId = new MultiSelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FancyName");

            ViewBag.LevelId = new MultiSelectList(_db.Levels.AsNoTracking(), "LevelId", "LevelName");
            return View(model);
        }

        // GET: FacultyFeeTypes/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FacultyFeeType facultyFeeType = await _db.FacultyFeeTypes.FindAsync(id);
            if (facultyFeeType == null)
            {
                return HttpNotFound();
            }
            ViewBag.FacultyId = new SelectList(_db.Faculties, "FacultyId", "FacultyName", facultyFeeType.FacultyId);
            ViewBag.SemesterId = new SelectList(_db.Semesters, "SemesterId", "SemesterName", facultyFeeType.SemesterId);
            var studentType = from StudentType s in Enum.GetValues(typeof(StudentType))
                              select new { ID = s, Name = s.ToString() };

            ViewBag.StudentType = new SelectList(studentType, "Name", "Name");
            ViewBag.LevelId = new SelectList(_db.Levels.AsNoTracking(), "LevelId", "LevelName");

            return View(facultyFeeType);
        }

        // POST: FacultyFeeTypes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "FacultyFeeTypeId,FacultyId,SemesterId,StudentType,FeeName,Amount,AmountInWords,Description")] FacultyFeeType facultyFeeType)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(facultyFeeType).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.FacultyId = new SelectList(_db.Faculties, "FacultyId", "FacultyName", facultyFeeType.FacultyId);
            ViewBag.SemesterId = new SelectList(_db.Semesters, "SemesterId", "SemesterName", facultyFeeType.SemesterId);
            var studentType = from StudentType s in Enum.GetValues(typeof(StudentType))
                              select new { ID = s, Name = s.ToString() };

            ViewBag.StudentType = new SelectList(studentType, "Name", "Name");
            ViewBag.LevelId = new SelectList(_db.Levels.AsNoTracking(), "LevelId", "LevelName");
            return View(facultyFeeType);
        }

        // GET: FacultyFeeTypes/Delete/5
        public async Task<PartialViewResult> Delete(int id)
        {
            var facultyFeeType = await _db.FacultyFeeTypes.FindAsync(id);
            return PartialView(facultyFeeType);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            bool status = false;
            string message = string.Empty;
            var facultyFeeType = await _db.FacultyFeeTypes.FindAsync(id);
            if (facultyFeeType != null)
            {
                _db.FacultyFeeTypes.Remove(facultyFeeType);
                await _db.SaveChangesAsync();
                status = true;
                message = "Faculty  Fee Type Deleted Successfully...";
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
