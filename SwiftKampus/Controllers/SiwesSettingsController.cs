using System.Data.Entity;
using System.Threading.Tasks;
using System.Net;
using System.Web.Mvc;
using SwiftKampus.Models;
using SwiftKampusModel.Siwes;
using SwiftKampus.Services;

namespace SwiftKampus.Controllers
{

    [Authorize]
    [Audit(AuditingLevel = 2)]
    public class SiwesSettingsController : BaseController
    {
        public SiwesSettingsController(SchoolDbContext db) : base(db)
        {

        }

        // GET: SiwesSettings
        public async Task<ActionResult> Index()
        {
            return View(await _db.SiwesSettings.ToListAsync());
        }

        // GET: SiwesSettings/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SiwesSetting siwesSetting = await _db.SiwesSettings.FindAsync(id);
            if (siwesSetting == null)
            {
                return HttpNotFound();
            }
            return View(siwesSetting);
        }

        // GET: SiwesSettings/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: SiwesSettings/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "SiwesSettingId,SessionId,StartDate,SiwesCode,SiwesFullCode")] SiwesSetting siwesSetting)
        {
            if (ModelState.IsValid)
            {
                _db.SiwesSettings.Add(siwesSetting);
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(siwesSetting);
        }

        // GET: SiwesSettings/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SiwesSetting siwesSetting = await _db.SiwesSettings.FindAsync(id);
            if (siwesSetting == null)
            {
                return HttpNotFound();
            }
            return View(siwesSetting);
        }

        // POST: SiwesSettings/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "SiwesSettingId,SessionId,StartDate,SiwesCode,SiwesFullCode")] SiwesSetting siwesSetting)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(siwesSetting).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(siwesSetting);
        }

        // GET: SiwesSettings/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SiwesSetting siwesSetting = await _db.SiwesSettings.FindAsync(id);
            if (siwesSetting == null)
            {
                return HttpNotFound();
            }
            return View(siwesSetting);
        }

        // POST: SiwesSettings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            SiwesSetting siwesSetting = await _db.SiwesSettings.FindAsync(id);
            _db.SiwesSettings.Remove(siwesSetting);
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
