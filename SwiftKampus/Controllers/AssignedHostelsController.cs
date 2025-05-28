using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampusModel.Accomodation;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SwiftKampus.Controllers
{
    [Authorize]
    [Audit(AuditingLevel = 2)]
    public class AssignedHostelsController : BaseController
    {

        public AssignedHostelsController(SchoolDbContext db) : base(db)
        {

        }

        // GET: AssignedHostels
        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> GetIndex()
        {
            // dc.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var data = await _db.AssignedHostels.Include(a => a.Faculty).Include(a => a.Hostel).AsNoTracking()
                .Select(s => new
            {
                s.Hostel.HostelName,
                s.Faculty.FacultyName,
                s.AssignedHostelId,               
            }).ToListAsync();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }


        public async Task<PartialViewResult> Save(int id)
        {
            var assignedHostel = await _db.AssignedHostels.FindAsync(id);
            ViewBag.FacultyId = new MultiSelectList(_db.Faculties.AsNoTracking(), "FacultyId", "FacultyName");
            ViewBag.HostelId = new SelectList(_db.Hostels.AsNoTracking(), "HostelId", "HostelCode", assignedHostel?.HostelId);
            if(assignedHostel != null)
            {
                var model = new AssignedHostelVm()
                {
                    HostelId = assignedHostel.HostelId,
                };
                return PartialView(model);
            }
            return PartialView();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Save(AssignedHostelVm model)
        {
            bool status = false;
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                if (model.AssignedHostelId > 0)
                {
                    var assignedHostel = await _db.AssignedHostels.FindAsync(model.AssignedHostelId);
                    if (assignedHostel != null)
                    {
                        assignedHostel.HostelId = model.HostelId;
                        assignedHostel.FacultyId = model.FacultyId[0];                      
                        _db.Entry(assignedHostel).State = EntityState.Modified;
                        await _db.SaveChangesAsync();
                        message = $"Assigned hostel Updated Successfully...";
                        return new JsonResult { Data = new { status = true, message } };

                    }
                }
                else
                {
                    foreach (var item in model.FacultyId)
                    {
                        var assignedHostel = new AssignedHostel()
                        {
                            HostelId = model.HostelId,
                            FacultyId = item,
                        };
                        _db.AssignedHostels.Add(assignedHostel);
                    }
                    await _db.SaveChangesAsync();
                    message = $"Hostel is assigned to faculty Successfully.";
                    return new JsonResult { Data = new { status = true, message } };
                }
            }
            return new JsonResult { Data = new { status, message } };
            //return View(subject);
        }

        // GET: AssignedHostels/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AssignedHostel assignedHostel = await _db.AssignedHostels.FindAsync(id);
            if (assignedHostel == null)
            {
                return HttpNotFound();
            }
            return View(assignedHostel);
        }

        // GET: AssignedHostels/Create
        public ActionResult Create()
        {
            ViewBag.FacultyId = new SelectList(_db.Faculties.AsNoTracking(), "FacultyId", "FacultyName");
            ViewBag.HostelId = new SelectList(_db.Hostels.AsNoTracking(), "HostelId", "HostelCode");
            return View();
        }

        // POST: AssignedHostels/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(AssignedHostelVm model)
        {
            if (ModelState.IsValid)
            {
                foreach (var faculty in model.FacultyId)
                {
                    var assignedHostel = new AssignedHostel
                    {
                        FacultyId = faculty,
                        HostelId = model.HostelId
                    };
                    _db.AssignedHostels.Add(assignedHostel);
                }

                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.FacultyId = new SelectList(_db.Faculties.AsNoTracking(), "FacultyId", "FacultyName", model.FacultyId);
            ViewBag.HostelId = new SelectList(_db.Hostels.AsNoTracking(), "HostelId", "HostelCode", model.HostelId);

            return View(model);
        }

        // GET: AssignedHostels/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AssignedHostel assignedHostel = await _db.AssignedHostels.FindAsync(id);
            if (assignedHostel == null)
            {
                return HttpNotFound();
            }
            ViewBag.FacultyId = new SelectList(_db.Faculties.AsNoTracking(), "FacultyId", "FacultyName", assignedHostel.FacultyId);
            ViewBag.HostelId = new SelectList(_db.Hostels.AsNoTracking(), "HostelId", "HostelCode", assignedHostel.HostelId);
            return View(assignedHostel);
        }

        // POST: AssignedHostels/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(AssignedHostel assignedHostel)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(assignedHostel).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.FacultyId = new SelectList(_db.Faculties.AsNoTracking(), "FacultyId", "FacultyName", assignedHostel.FacultyId);
            ViewBag.HostelId = new SelectList(_db.Hostels.AsNoTracking(), "HostelId", "HostelCode", assignedHostel.HostelId);
            return View(assignedHostel);
        }

        // GET: AssignedHostels/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AssignedHostel assignedHostel = await _db.AssignedHostels.FindAsync(id);
            if (assignedHostel == null)
            {
                return HttpNotFound();
            }
            return View(assignedHostel);
        }

        // POST: AssignedHostels/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            AssignedHostel assignedHostel = await _db.AssignedHostels.FindAsync(id);
            _db.AssignedHostels.Remove(assignedHostel);
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
