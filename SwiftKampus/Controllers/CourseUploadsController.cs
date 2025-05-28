using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampusModel;
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
    public class CourseUploadsController : BaseController
    {

        public CourseUploadsController(SchoolDbContext db) : base(db)
        {

        }
        // GET: CourseUploads
        public async Task<ActionResult> Index()
        {
            var courseUploads = _db.CourseUploads.AsNoTracking().Include(c => c.Course);
            return View(await courseUploads.ToListAsync());
        }

        public ActionResult Glown()
        {
            return View();
        }
        public ActionResult ScienceDirect()
        {
            return View();
        }
        public ActionResult Ebsco()
        {
            return View();
        }

        public ActionResult Ebook77()
        {
            return View();
        }
        public ActionResult GenesisLib()
        {
            return View();
        }

        // GET: CourseUploads/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CourseUpload courseUpload = await _db.CourseUploads.FindAsync(id);
            if (courseUpload == null)
            {
                return HttpNotFound();
            }
            return View(courseUpload);
        }

        // GET: CourseUploads/Create
        public ActionResult Create()
        {
            ViewBag.CourseId = new SelectList(_db.Courses.AsNoTracking(), "CourseId", "CourseCode");
            return View();
        }

        // POST: CourseUploads/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CourseUpload courseUpload)
        {
            if (ModelState.IsValid)
            {
                string _FileName = String.Empty;
                try
                {
                    if (courseUpload.File.ContentLength > 0)
                    {
                        _FileName = Path.GetFileName(courseUpload.File.FileName);
                        string _path = HostingEnvironment.MapPath("~/UploadedFiles/") + _FileName;
                        courseUpload.FileLocation = _path;
                        var directory = new DirectoryInfo(HostingEnvironment.MapPath("~/UploadedFiles/"));
                        if (directory.Exists == false)
                        {
                            directory.Create();
                        }
                        courseUpload.File.SaveAs(_path);
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.Message = $"File upload failed!! {ex.Message}";
                    ViewBag.CourseId = new SelectList(_db.Courses, "CourseId", "CourseCode");
                    return View(courseUpload);
                }
                _db.CourseUploads.Add(courseUpload);
                TempData["UserMessage"] = "Course Uploaded Successfully.";
                TempData["Title"] = "Success.";

                await _db.SaveChangesAsync();
                return RedirectToAction("Create");
            }

            ViewBag.CourseId = new SelectList(_db.Courses.AsNoTracking(), "CourseId", "CourseCode", courseUpload.CourseId);
            return View(courseUpload);
        }

        //// GET: CourseUploads/Edit/5
        //public async Task<ActionResult> Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    CourseUpload courseUpload = await _db.CourseUploads.FindAsync(id);
        //    if (courseUpload == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    ViewBag.CourseId = new SelectList(_db.Courses, "CourseId", "CourseCode", courseUpload.CourseId);
        //    return View(courseUpload);
        //}

        //// POST: CourseUploads/Edit/5
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Edit([Bind(Include = "CourseUploadId,CourseId,FileLocation")] CourseUpload courseUpload)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _db.Entry(courseUpload).State = System.Data.Entity.EntityState.Modified;
        //        await _db.SaveChangesAsync();
        //        return RedirectToAction("Index");
        //    }
        //    ViewBag.CourseId = new SelectList(_db.Courses, "CourseId", "CourseCode", courseUpload.CourseId);
        //    return View(courseUpload);
        //}

        // GET: CourseUploads/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CourseUpload courseUpload = await _db.CourseUploads.FindAsync(id);
            if (courseUpload == null)
            {
                return HttpNotFound();
            }
            return View(courseUpload);
        }

        // POST: CourseUploads/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            CourseUpload courseUpload = await _db.CourseUploads.FindAsync(id);
            if (courseUpload != null) _db.CourseUploads.Remove(courseUpload);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<FileResult> Download(int id)
        {

            CourseUpload courseUpload = await _db.CourseUploads.FindAsync(id);

            string contentType = string.Empty;
            if (courseUpload != null)
            {
                if (courseUpload.FileLocation.Contains(".pdf"))
                {
                    contentType = "application/pdf";
                }

                else if (courseUpload.FileLocation.Contains(".docx"))
                {
                    contentType = "application/docx";
                }
            }

            return File(courseUpload.FileLocation, contentType, courseUpload.FileLocation);
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
