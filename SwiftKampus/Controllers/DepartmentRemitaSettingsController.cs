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
    public class DepartmentRemitaSettingsController : BaseController
    {

        public DepartmentRemitaSettingsController(SchoolDbContext db) : base(db)
        {

        }
        // GET: DepartmentRemitaSettings
        public async Task<ActionResult> Index()
        {
            var departmentRemitaSettings = _db.DepartmentRemitaSettings.Include(d => d.Department);
            return View(await departmentRemitaSettings.ToListAsync());
        }
        public async Task<ActionResult> GetIndex()
        {
            // dc.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var data = await _db.DepartmentRemitaSettings.Include(i => i.Department).AsNoTracking()
                .Select(s => new
                {
                    s.Department.DeptName,
                    s.ApiKey,
                    s.ServiceType,
                    s.MerchantId,
                    s.DepartmentRemitaSettingId
                }).ToListAsync();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        public async Task<PartialViewResult> Save(int id)
        {
            var departmentRemitaSetting = await _db.DepartmentRemitaSettings.FindAsync(id);
            ViewBag.DepartmentId = new SelectList(await _db.Departments.AsNoTracking().ToListAsync(), "DepartmentId", "DeptName");

            return PartialView(departmentRemitaSetting);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Save(DepartmentRemitaSetting model)
        {
            bool status = false;
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                if (model.DepartmentRemitaSettingId > 0)
                {
                    var departmentRemitaSetting = await _db.DepartmentRemitaSettings.FindAsync(model.DepartmentRemitaSettingId);
                    if (departmentRemitaSetting != null)
                    {
                        departmentRemitaSetting.DepartmentId = model.DepartmentId;
                        departmentRemitaSetting.ApiKey = model.ApiKey;
                        departmentRemitaSetting.ServiceType = model.ServiceType;
                        departmentRemitaSetting.MerchantId = model.MerchantId;
                        _db.Entry(departmentRemitaSetting).State = EntityState.Modified;
                        await _db.SaveChangesAsync();
                        message = "Department Remita Setting Updated Successfully...";
                        return new JsonResult { Data = new { status = true, message } };
                    }
                }
                else
                {
                    _db.DepartmentRemitaSettings.Add(model);
                    await _db.SaveChangesAsync();
                    message = $"Department Remita Setting Added Successfully.";
                    return new JsonResult { Data = new { status = true, message } };
                }
            }
            return new JsonResult { Data = new { status, message = "Please enter the data required Correctly" } };
            //return View(subject);
        }


        #region Auto-generated code
        //// GET: DepartmentRemitaSettings/Details/5
        //public async Task<ActionResult> Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    DepartmentRemitaSetting departmentRemitaSetting = await _db.DepartmentRemitaSettings.FindAsync(id);
        //    if (departmentRemitaSetting == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(departmentRemitaSetting);
        //}

        //// GET: DepartmentRemitaSettings/Create
        //public ActionResult Create()
        //{
        //    ViewBag.DepartmentId = new SelectList(_db.Departments, "DepartmentId", "DeptCode");
        //    return View();
        //}

        //// POST: DepartmentRemitaSettings/Create
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Create([Bind(Include = "DepartmentRemitaSettingId,DepartmentId,ServiceType,MerchantId,ApiKey")] DepartmentRemitaSetting departmentRemitaSetting)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _db.DepartmentRemitaSettings.Add(departmentRemitaSetting);
        //        await _db.SaveChangesAsync();
        //        return RedirectToAction("Index");
        //    }

        //    ViewBag.DepartmentId = new SelectList(_db.Departments, "DepartmentId", "DeptCode", departmentRemitaSetting.DepartmentId);
        //    return View(departmentRemitaSetting);
        //}

        //// GET: DepartmentRemitaSettings/Edit/5
        //public async Task<ActionResult> Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    DepartmentRemitaSetting departmentRemitaSetting = await _db.DepartmentRemitaSettings.FindAsync(id);
        //    if (departmentRemitaSetting == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    ViewBag.DepartmentId = new SelectList(_db.Departments, "DepartmentId", "DeptCode", departmentRemitaSetting.DepartmentId);
        //    return View(departmentRemitaSetting);
        //}

        //// POST: DepartmentRemitaSettings/Edit/5
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Edit([Bind(Include = "DepartmentRemitaSettingId,DepartmentId,ServiceType,MerchantId,ApiKey")] DepartmentRemitaSetting departmentRemitaSetting)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _db.Entry(departmentRemitaSetting).State = EntityState.Modified;
        //        await _db.SaveChangesAsync();
        //        return RedirectToAction("Index");
        //    }
        //    ViewBag.DepartmentId = new SelectList(_db.Departments, "DepartmentId", "DeptCode", departmentRemitaSetting.DepartmentId);
        //    return View(departmentRemitaSetting);
        //}

        //// GET: DepartmentRemitaSettings/Delete/5
        //public async Task<ActionResult> Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    DepartmentRemitaSetting departmentRemitaSetting = await _db.DepartmentRemitaSettings.FindAsync(id);
        //    if (departmentRemitaSetting == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(departmentRemitaSetting);
        //}

        //// POST: DepartmentRemitaSettings/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> DeleteConfirmed(int id)
        //{
        //    DepartmentRemitaSetting departmentRemitaSetting = await _db.DepartmentRemitaSettings.FindAsync(id);
        //    _db.DepartmentRemitaSettings.Remove(departmentRemitaSetting);
        //    await _db.SaveChangesAsync();
        //    return RedirectToAction("Index");
        //} 
        #endregion

        public async Task<PartialViewResult> Delete(int id)
        {
            var departmentRemitaSetting = await _db.DepartmentRemitaSettings.FindAsync(id);
            return PartialView(departmentRemitaSetting);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            bool status = false;
            string message = string.Empty;
            var departmentRemitaSetting = await _db.DepartmentRemitaSettings.FindAsync(id);
            if (departmentRemitaSetting != null)
            {
                _db.DepartmentRemitaSettings.Remove(departmentRemitaSetting);
                await _db.SaveChangesAsync();
                status = true;
                message = "Department Remita Settings Deleted Successfully...";
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
