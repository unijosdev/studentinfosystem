using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampusModel.Payment;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SwiftKampus.Controllers
{
    [Authorize]
    [Audit(AuditingLevel = 2)]
    public class FacultyRemitaSettingsController : BaseController
    {

        public FacultyRemitaSettingsController(SchoolDbContext db) : base(db)
        {

        }
        // GET: FacultyRemitaSettings
        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> GetIndex()
        {
            // dc.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var data = await _db.FacultyRemitaSettings.Include(i => i.Faculty).AsNoTracking()
                .Select(s => new
                {
                    s.Faculty.FacultyName,
                    s.ApiKey,
                    s.MerchantId,
                    s.ServiceType,
                    s.FacultyRemitaSettingId
                }).ToListAsync();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        public async Task<PartialViewResult> Save(int id)
        {
            var facultyRemitaSetting = await _db.FacultyRemitaSettings.FindAsync(id);
            ViewBag.FacultyId = new SelectList(_db.Faculties.AsNoTracking(), "FacultyId", "FacultyName");
            return PartialView(facultyRemitaSetting);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Save(FacultyRemitaSetting model)
        {
            bool status = false;
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                if (model.FacultyRemitaSettingId > 0)
                {
                    var facultyRemitaSetting = await _db.FacultyRemitaSettings.FindAsync(model.FacultyRemitaSettingId);
                    if (facultyRemitaSetting != null)
                    {
                        facultyRemitaSetting.FacultyId = model.FacultyId;
                        facultyRemitaSetting.ApiKey = model.ApiKey;
                        facultyRemitaSetting.MerchantId = model.MerchantId;
                        facultyRemitaSetting.ServiceType = model.ServiceType;
                        _db.Entry(facultyRemitaSetting).State = EntityState.Modified;
                        await _db.SaveChangesAsync();
                        message = $"{model.ServiceType} Remita Settings Updated Successfully...";
                        return new JsonResult { Data = new { status = true, message } };
                    }
                }
                else
                {
                    _db.FacultyRemitaSettings.Add(model);
                    await _db.SaveChangesAsync();
                    message = $"{model.ApiKey} Faculty Remita Settings Added Successfully.";
                    return new JsonResult { Data = new { status = true, message } };
                }
            }
            return new JsonResult { Data = new { status, message } };
            //return View(subject);
        }


        #region Auto-generated Code
        //// GET: FacultyRemitaSettings/Details/5
        //public async Task<ActionResult> Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    FacultyRemitaSetting facultyRemitaSetting = await _db.FacultyRemitaSettings.FindAsync(id);
        //    if (facultyRemitaSetting == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(facultyRemitaSetting);
        //}

        //// GET: FacultyRemitaSettings/Create
        //public ActionResult Create()
        //{
        //    ViewBag.FacultyId = new SelectList(_db.Faculties, "FacultyId", "FacultyCode");
        //    return View();
        //}

        //// POST: FacultyRemitaSettings/Create
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Create(FacultyRemitaSetting facultyRemitaSetting)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _db.FacultyRemitaSettings.Add(facultyRemitaSetting);
        //        await _db.SaveChangesAsync();
        //        return RedirectToAction("Index");
        //    }

        //    ViewBag.FacultyId = new SelectList(_db.Faculties, "FacultyId", "FacultyCode", facultyRemitaSetting.FacultyId);
        //    return View(facultyRemitaSetting);
        //}

        //// GET: FacultyRemitaSettings/Edit/5
        //public async Task<ActionResult> Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    FacultyRemitaSetting facultyRemitaSetting = await _db.FacultyRemitaSettings.FindAsync(id);
        //    if (facultyRemitaSetting == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    ViewBag.FacultyId = new SelectList(_db.Faculties, "FacultyId", "FacultyCode", facultyRemitaSetting.FacultyId);
        //    return View(facultyRemitaSetting);
        //}

        //// POST: FacultyRemitaSettings/Edit/5
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Edit(FacultyRemitaSetting facultyRemitaSetting)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _db.Entry(facultyRemitaSetting).State = EntityState.Modified;
        //        await _db.SaveChangesAsync();
        //        return RedirectToAction("Index");
        //    }
        //    ViewBag.FacultyId = new SelectList(_db.Faculties, "FacultyId", "FacultyCode", facultyRemitaSetting.FacultyId);
        //    return View(facultyRemitaSetting);
        //} 
        #endregion

        public async Task<PartialViewResult> Delete(int id)
        {
            var facultyRemitaSetting = await _db.FacultyRemitaSettings.FindAsync(id);
            return PartialView(facultyRemitaSetting);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            bool status = false;
            string message = string.Empty;
            var facultyRemitaSetting = await _db.FacultyRemitaSettings.FindAsync(id);
            if (facultyRemitaSetting != null)
            {
                _db.FacultyRemitaSettings.Remove(facultyRemitaSetting);
                await _db.SaveChangesAsync();
                status = true;
                message = "Faculty Remita Setting Deleted Successfully...";
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
