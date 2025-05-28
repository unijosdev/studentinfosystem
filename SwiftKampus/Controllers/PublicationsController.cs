using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampus.ViewModels.StudentBioData;
using SwiftKampusModel.AddmissionApplicant;
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
    public class PublicationsController : BaseController
    {

        public PublicationsController(SchoolDbContext db) : base(db)
        {

        }

        // GET: Publications
        public async Task<ActionResult> Index()
        {
            return View(await _db.Publications.AsNoTracking().Where(x => x.UserId.Equals(userId)).ToListAsync());
        }

        // GET: Publications/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Publication publication = await _db.Publications.FindAsync(id);
            if (publication == null)
            {
                return HttpNotFound();
            }
            return View(publication);
        }

        // GET: Publications/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Publications/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(List<Publication> publication)
        {
            if (ModelState.IsValid)
            {
                foreach (var item in publication)
                {
                    item.UserId = userId;
                    _db.Publications.Add(item);
                }
                await _db.SaveChangesAsync();
                var message = $"{publication.Count} Publicantion(s) are added successfully";
                return new JsonResult { Data = new { status = true, message } };
            }

            return new JsonResult { Data = new { status = false, message = "Please complete the form completely" } };
        }


        // GET: Publications/PCreate
        public ActionResult PCreate()
        {
            var user = User.IsInRole(RoleName.Student);
            if (user) ViewBag.isStudent = "Student";

            var publications = _db.Publications.AsNoTracking().Where(x => x.UserId.Equals(userId)).ToList();
            var model = new PublicationVm()
            {
                Publication = new Publication(),
                Publications = publications
            };
            return View(model);
        }

        // POST: Publications/PCreate
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> PCreate(List<Publication> publications)
        {
            if (ModelState.IsValid)
            {
                foreach (var item in publications)
                {
                    item.UserId = userId;
                    _db.Publications.Add(item);
                }

                var oldPublications = _db.Publications.AsNoTracking()
                                .Where(x => x.UserId.Equals(userId)).ToList();
                if (oldPublications != null && oldPublications.Any())
                {
                    foreach (var item in oldPublications)
                    {
                        _db.Entry(item).State = EntityState.Deleted;
                    }
                }
                await _db.SaveChangesAsync();
                var message = $"{publications.Count} Publicantion(s) are added successfully";
                return new JsonResult { Data = new { status = true, message } };
            }

            return new JsonResult { Data = new { status = false, message = $"{publications.Count()} Please complete the form completely" } };
        }

        // GET: Publications/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Publication publication = await _db.Publications.FindAsync(id);
            if (publication == null)
            {
                return HttpNotFound();
            }
            return View(publication);
        }

        // POST: Publications/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "PublicationId,Title,Institution,PublicationType,Qualification,Publisher,PublishedDate")] Publication publication)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(publication).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(publication);
        }

        // GET: Publications/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Publication publication = await _db.Publications.FindAsync(id);
            if (publication == null)
            {
                return HttpNotFound();
            }
            return View(publication);
        }

        // POST: Publications/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Publication publication = await _db.Publications.FindAsync(id);
            _db.Publications.Remove(publication);
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
