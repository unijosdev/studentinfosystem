using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampusModel;
using SwiftKampusModel.AddmissionApplicant;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SwiftKampus.Controllers
{
    [Authorize]
    [Audit(AuditingLevel = 2)]
    public class SchoolProgrammesController : BaseController
    {

        public SchoolProgrammesController(SchoolDbContext db) : base(db)
        {

        }
        // GET: SchoolProgrammes
        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> GetIndex()
        {
            // dc.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var schoolProgramme = await _db.SchoolProgrammes.Include(i => i.Session).AsNoTracking().ToListAsync();
            var data = schoolProgramme.Select(s => new
            {
                s.SchoolProgrammeId,
                s.ProgrammeType,
                s.ActiveSale,
                s.FancyName,
                s.SchoolProgrammeCode,
                s.FullName,
                s.ProgrammeCategory,
                s.Session?.SessionName
            }).ToList();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }


        public async Task<PartialViewResult> Save(int id)
        {
            var schoolProgramme = await _db.SchoolProgrammes.FindAsync(id);
            var programmeCategory = from ProgrammeCategory s in Enum.GetValues(typeof(ProgrammeCategory))
                                    select new { ID = s, Name = s.ToString() };
            var programmetype = from ProgrammeType s in Enum.GetValues(typeof(ProgrammeType))
                                select new { ID = s, Name = s.ToString() };
            ViewBag.ProgrammeCategory = new SelectList(programmeCategory, "Name", "Name", schoolProgramme?.ProgrammeCategory);
            ViewBag.ProgrammeType = new SelectList(programmetype, "Name", "Name", schoolProgramme?.ProgrammeType);
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName", schoolProgramme?.SessionId);
            return PartialView(schoolProgramme);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Save(SchoolProgramme model)
        {
            bool status = false;
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                if (model.SchoolProgrammeId > 0)
                {
                    var schoolProgramme = await _db.SchoolProgrammes.FindAsync(model.SchoolProgrammeId);
                    if (schoolProgramme != null)
                    {
                        schoolProgramme.ProgrammeCategory = model.ProgrammeCategory;
                        schoolProgramme.ProgrammeType = model.ProgrammeType;
                        schoolProgramme.ActiveSale = model.ActiveSale;
                        schoolProgramme.FancyName = model.FancyName;
                        schoolProgramme.SchoolProgrammeCode = model.SchoolProgrammeCode;
                        schoolProgramme.Duration = model.Duration;
                        schoolProgramme.Description = model.Description;
                        schoolProgramme.ClosingDate = model.ClosingDate;
                        schoolProgramme.ImageName = model.ImageName;
                        schoolProgramme.SessionId = model.SessionId;
                        _db.Entry(schoolProgramme).State = EntityState.Modified;
                        await _db.SaveChangesAsync();
                        message = $"{model.FancyName} Updated Successfully...";
                        return new JsonResult { Data = new { status = true, message } };
                    }
                }
                else
                {
                    _db.SchoolProgrammes.Add(model);
                    await _db.SaveChangesAsync();
                    message = $"{model.FancyName} Added Successfully.";
                    return new JsonResult { Data = new { status = true, message } };
                }
            }
            var builder = new StringBuilder();
            foreach (var item in ModelState)
            {
                builder.Append(item.Key);
                builder.Append(", ");
            }
            return new JsonResult { Data = new { status, message = $"Ooops... {builder.ToString()} are all Required" } };
            //return View(subject);
        }

        // GET: SchoolProgrammes/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SchoolProgramme schoolProgramme = await _db.SchoolProgrammes.FindAsync(id);
            if (schoolProgramme == null)
            {
                return HttpNotFound();
            }
            return View(schoolProgramme);
        }

        // GET: SchoolProgrammes/Create
        public ActionResult Create()
        {
            var programmeCategory = from ProgrammeCategory s in Enum.GetValues(typeof(ProgrammeCategory))
                                    select new { ID = s, Name = s.ToString() };
            var programmetype = from ProgrammeType s in Enum.GetValues(typeof(ProgrammeType))
                                select new { ID = s, Name = s.ToString() };
            ViewBag.ProgrammeCategory = new SelectList(programmeCategory, "Name", "Name");
            ViewBag.ProgrammeType = new SelectList(programmetype, "Name", "Name");
            return View();
        }

        // POST: SchoolProgrammes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "SchoolProgrammeId,ProgrammeName,ProgrammeType,ProgrammeCategory,ActiveSale")] SchoolProgramme schoolProgramme)
        {
            if (ModelState.IsValid)
            {
                _db.SchoolProgrammes.Add(schoolProgramme);
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            var programmeCategory = from ProgrammeCategory s in Enum.GetValues(typeof(ProgrammeCategory))
                                    select new { ID = s, Name = s.ToString() };
            var programmetype = from ProgrammeType s in Enum.GetValues(typeof(ProgrammeType))
                                select new { ID = s, Name = s.ToString() };
            ViewBag.ProgrammeCategory = new SelectList(programmeCategory, "Name", "Name");
            ViewBag.ProgrammeType = new SelectList(programmetype, "Name", "Name");
            return View(schoolProgramme);
        }

        // GET: SchoolProgrammes/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SchoolProgramme schoolProgramme = await _db.SchoolProgrammes.FindAsync(id);
            if (schoolProgramme == null)
            {
                return HttpNotFound();
            }
            var programmeCategory = from ProgrammeCategory s in Enum.GetValues(typeof(ProgrammeCategory))
                                    select new { ID = s, Name = s.ToString() };
            var programmetype = from ProgrammeType s in Enum.GetValues(typeof(ProgrammeType))
                                select new { ID = s, Name = s.ToString() };
            ViewBag.ProgrammeCategory = new SelectList(programmeCategory, "Name", "Name");
            ViewBag.ProgrammeType = new SelectList(programmetype, "Name", "Name");
            return View(schoolProgramme);
        }

        // POST: SchoolProgrammes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "SchoolProgrammeId,ProgrammeName,ProgrammeType,ProgrammeCategory,ActiveSale")] SchoolProgramme schoolProgramme)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(schoolProgramme).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            var programmeCategory = from ProgrammeCategory s in Enum.GetValues(typeof(ProgrammeCategory))
                                    select new { ID = s, Name = s.ToString() };
            var programmetype = from ProgrammeType s in Enum.GetValues(typeof(ProgrammeType))
                                select new { ID = s, Name = s.ToString() };
            ViewBag.ProgrammeCategory = new SelectList(programmeCategory, "Name", "Name");
            ViewBag.ProgrammeType = new SelectList(programmetype, "Name", "Name");
            return View(schoolProgramme);
        }

        // GET: SchoolProgrammes/Delete/5
        public async Task<PartialViewResult> Delete(int id)
        {
            var schoolProgramme = await _db.SchoolProgrammes.FindAsync(id);
            return PartialView(schoolProgramme);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            bool status = false;
            string message = string.Empty;
            var schoolProgramme = await _db.SchoolProgrammes.FindAsync(id);
            if (schoolProgramme != null)
            {
                _db.SchoolProgrammes.Remove(schoolProgramme);
                await _db.SaveChangesAsync();
                status = true;
                message = "School Programme Deleted Successfully...";
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
