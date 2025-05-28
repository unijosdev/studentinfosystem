using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampus.ViewModels.StudentBioData;
using SwiftKampusModel;
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
    public class QualificationsController : BaseController
    {

        public QualificationsController(SchoolDbContext db) : base(db)
        {

        }

        // GET: Qualifications
        public async Task<ActionResult> Index()
        {
            var myQualifications = await _db.Qualifications.AsNoTracking()
                        .Where(x => x.UserId.Equals(userId)).ToListAsync();
            return View(myQualifications);
        }

        // GET: Qualifications/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Qualification qualification = await _db.Qualifications.FindAsync(id);
            if (qualification == null)
            {
                return HttpNotFound();
            }
            return View(qualification);
        }

        // GET: Qualifications/Create
        public ActionResult Create()
        {
            var qualificationType = from Qualifications s in Enum.GetValues(typeof(Qualifications))
                                    select new { ID = s, Name = s.ToString() };
            var filetype = from FileTypes s in Enum.GetValues(typeof(FileTypes))
                           select new { ID = s, Name = s.ToString() };

            ViewBag.QualificationName = new SelectList(qualificationType, "Name", "Name");
            ViewBag.FileType = new SelectList(filetype, "Name", "Name");
            var model = _db.Qualifications.AsNoTracking().Where(x => x.UserId.Equals(userId)).FirstOrDefault();
            return View(model);
        }

        // POST: Qualifications/Create        
        [HttpPost]
        public async Task<ActionResult> Create(Qualification qualification)
            {
            if (ModelState.IsValid)
            {

                var oldQualification = _db.Qualifications.Where(x => x.UserId.Trim().ToUpper().Equals(userId.Trim().ToUpper())).FirstOrDefault();
                if (oldQualification != null)
                {
                    oldQualification.NameOfInstitution = qualification.NameOfInstitution;
                    oldQualification.QualificationName = qualification.QualificationName;
                    oldQualification.Discipline = qualification.Discipline;
                    oldQualification.Grade = qualification.Grade;
                    oldQualification.QualificationName = qualification.QualificationName;
                    _db.Entry(oldQualification).State = EntityState.Modified;
                }
                else
                {
                    qualification.UserId = userId;
                    _db.Qualifications.Add(qualification);
                }
                await _db.SaveChangesAsync();
                var message = $"{qualification.QualificationName} qualification is added successfully";
                return new JsonResult { Data = new { status = true, message } };
            }
            return new JsonResult { Data = new { status = true, message = "Please complete the form completely" } };
        }


        // GET: Qualifications/PCreate
        public ActionResult PCreate()
        {
            var qualificationType = from Qualifications s in Enum.GetValues(typeof(Qualifications))
                                    select new { ID = s, Name = s.ToString() };
            var filetype = from FileTypes s in Enum.GetValues(typeof(FileTypes))
                           select new { ID = s, Name = s.ToString() };

            ViewBag.QualificationName = new SelectList(qualificationType, "Name", "Name");
            ViewBag.FileType = new SelectList(filetype, "Name", "Name");
            var relevantQualifications = _db.Qualifications.AsNoTracking().Where(x => x.UserId.Equals(userId))
                                     .ToList();

            var model = new RelevantQualificationVm()
            {
                Qualification = new Qualification(),
                Qualifications = relevantQualifications
            };
            return View(model);
        }

        // POST: Qualifications/PCreate        
        [HttpPost]
        public async Task<ActionResult> PCreate(List<Qualification> qualifications)
        {
            string message = "";
            if (ModelState.IsValid)
            {
                foreach (var item in qualifications)
                {
                    item.UserId = userId;
                    _db.Qualifications.Add(item);
                }

                var relevantqualifications = _db.Qualifications.AsNoTracking()
                                  .Where(x => x.UserId.Equals(userId)).ToList();
                if (relevantqualifications != null && relevantqualifications.Any())
                {
                    foreach (var item in relevantqualifications)
                    {
                        _db.Entry(item).State = EntityState.Deleted;
                    }
                }
                await _db.SaveChangesAsync();
                message = $"{qualifications.Count()} Schools are added successfully";
                return new JsonResult { Data = new { status = true, message } };
            }

            message = $"Something is wrong with the data, Please check and try again";
            return new JsonResult { Data = new { status = false, message } };
        }

        // GET: Qualifications/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Qualification qualification = await _db.Qualifications.FindAsync(id);
            if (qualification == null)
            {
                return HttpNotFound();
            }
            var qualificationType = from Qualifications s in Enum.GetValues(typeof(Qualifications))
                                    select new { ID = s, Name = s.ToString() };

            var filetype = from FileTypes s in Enum.GetValues(typeof(FileTypes))
                           select new { ID = s, Name = s.ToString() };

            ViewBag.QualificationType = new SelectList(qualificationType, "Name", "Name");
            ViewBag.FileType = new SelectList(filetype, "Name", "Name");
            return View(qualification);
        }

        // POST: Qualifications/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Qualification qualification)
        {
            if (ModelState.IsValid)
            {
                string _FileName = String.Empty;
                //try
                //{
                //    if (qualification.File.ContentLength > 0)
                //    {
                //        _FileName = Path.GetFileName(qualification.File.FileName);
                //        string _path = HostingEnvironment.MapPath("~/QualificationDocument/") + _FileName;
                //        qualification.FileLocation = _path;
                //        var directory = new DirectoryInfo(HostingEnvironment.MapPath("~/QualificationDocument/"));
                //        if (directory.Exists == false)
                //        {
                //            directory.Create();
                //        }
                //        qualification.File.SaveAs(_path);
                //    }
                //}
                //catch (Exception ex)
                //{
                //    ViewBag.Message = $"File upload failed!! {ex.Message}";
                //    ViewBag.CourseId = new SelectList(_db.Courses, "CourseId", "CourseCode");
                //    return View(qualification);
                //}
                //qualification.FileLocation = _FileName;
                qualification.UserId = userId;
                _db.Entry(qualification).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            var qualificationType = from Qualifications s in Enum.GetValues(typeof(Qualifications))
                                    select new { ID = s, Name = s.ToString() };

            var filetype = from FileTypes s in Enum.GetValues(typeof(FileTypes))
                           select new { ID = s, Name = s.ToString() };

            ViewBag.QualificationType = new SelectList(qualificationType, "Name", "Name");
            ViewBag.FileType = new SelectList(filetype, "Name", "Name");
            return View(qualification);
        }

        // GET: Qualifications/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Qualification qualification = await _db.Qualifications.FindAsync(id);
            if (qualification == null)
            {
                return HttpNotFound();
            }

            return View(qualification);
        }

        // POST: Qualifications/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Qualification qualification = await _db.Qualifications.FindAsync(id);
            if (qualification != null) _db.Qualifications.Remove(qualification);
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
