using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampusModel.Classroom;
using System;
using System.Data.Entity;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Web.Mvc;

namespace SwiftKampus.Controllers
{
    [Authorize]
    [Audit(AuditingLevel = 2)]
    public class TopicMaterialsController : BaseController
    {

        public TopicMaterialsController(SchoolDbContext db) : base(db)
        {

        }
        // GET: TopicMaterials
        public async Task<ActionResult> Index()
        {
            var topicMaterials = _db.TopicMaterials.Include(t => t.Topic);
            return View(await topicMaterials.ToListAsync());
        }

        // GET: TopicMaterials/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TopicMaterial topicMaterial = await _db.TopicMaterials.FindAsync(id);
            if (topicMaterial == null)
            {
                return HttpNotFound();
            }
            return View(topicMaterial);
        }

        // GET: TopicMaterials/Create
        public ActionResult Create()
        {
            ViewBag.TopicId = new SelectList(_db.Topics, "TopicId", "TopicName");
            return View();
        }

        // POST: TopicMaterials/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(TopicMaterial topicMaterial)
        {
            if (ModelState.IsValid)
            {
                string _FileName = String.Empty;
                if (topicMaterial.File.ContentLength > 0)
                {
                    _FileName = Path.GetFileName(topicMaterial.File.FileName);
                    string _path = HostingEnvironment.MapPath("~/MaterialUpload/") + _FileName;
                    topicMaterial.FileLocation = _path;
                    var directory = new DirectoryInfo(HostingEnvironment.MapPath("~/MaterialUpload/"));
                    if (directory.Exists == false)
                    {
                        directory.Create();
                    }
                    topicMaterial.File.SaveAs(_path);
                }
                topicMaterial.FileLocation = _FileName;
                _db.TopicMaterials.Add(topicMaterial);
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.TopicId = new SelectList(_db.Topics, "TopicId", "TopicName", topicMaterial.TopicId);
            return View(topicMaterial);
        }

        // GET: TopicMaterials/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TopicMaterial topicMaterial = await _db.TopicMaterials.FindAsync(id);
            if (topicMaterial == null)
            {
                return HttpNotFound();
            }
            ViewBag.TopicId = new SelectList(_db.Topics, "TopicId", "TopicName", topicMaterial.TopicId);
            return View(topicMaterial);
        }

        // POST: TopicMaterials/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(TopicMaterial topicMaterial)
        {
            if (ModelState.IsValid)
            {
                string _FileName = String.Empty;
                if (topicMaterial.File.ContentLength > 0)
                {
                    _FileName = Path.GetFileName(topicMaterial.File.FileName);
                    string _path = HostingEnvironment.MapPath("~/MaterialUpload/") + _FileName;
                    topicMaterial.FileLocation = _path;
                    var directory = new DirectoryInfo(HostingEnvironment.MapPath("~/MaterialUpload/"));
                    if (directory.Exists == false)
                    {
                        directory.Create();
                    }
                    topicMaterial.File.SaveAs(_path);
                }
                topicMaterial.FileLocation = _FileName;
                _db.Entry(topicMaterial).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.TopicId = new SelectList(_db.Topics, "TopicId", "TopicName", topicMaterial.TopicId);
            return View(topicMaterial);
        }

        // GET: TopicMaterials/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TopicMaterial topicMaterial = await _db.TopicMaterials.FindAsync(id);
            if (topicMaterial == null)
            {
                return HttpNotFound();
            }
            return View(topicMaterial);
        }

        // POST: TopicMaterials/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            TopicMaterial topicMaterial = await _db.TopicMaterials.FindAsync(id);
            if (topicMaterial != null) _db.TopicMaterials.Remove(topicMaterial);
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
