using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampusModel;
using SwiftKampusModel.Accomodation;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SwiftKampus.Controllers
{
    [Authorize]
    [Audit(AuditingLevel = 2)]
    public class HostelsController : BaseController
    {

        public HostelsController(SchoolDbContext db) : base(db)
        {

        }
        public ActionResult Index()
        {
            return View();
        }
        public async Task<ActionResult> GetIndex()
        {
            // dc.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var data = await _db.Hostels.Include(i => i.AssignedHostels).AsNoTracking().Select(s => new
            {
                s.HostelId,
                s.HostelName,
                s.HostelCode,
                s.Remark,   
                s.HostelPrice
            }).ToListAsync();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        public async Task<PartialViewResult> Save(int id)
        {
            var hostel = await _db.Hostels.FindAsync(id);
            ViewBag.FacultyId = new SelectList(_db.Faculties.AsNoTracking(), "FacultyId", "FacultyName");
            var gender = from Gender s in Enum.GetValues(typeof(Gender))
                         select new { ID = s, Name = s.ToString() };

            ViewBag.Gender = new MultiSelectList(gender, "Name", "Name");
            return PartialView(hostel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Save(Hostel model)
        {
            bool status = false;
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                if (model.HostelId > 0)
                {
                    var hostel = await _db.Hostels.FindAsync(model.HostelId);
                    if (hostel != null)
                    {
                        //hostel.FacultyId = model.FacultyId;
                        hostel.HostelCode = model.HostelCode;
                        hostel.HostelName = model.HostelName;
                        hostel.Remark = model.Remark;
                        hostel.HostelPrice = model.HostelPrice;
                        _db.Entry(hostel).State = EntityState.Modified;
                        await _db.SaveChangesAsync();
                        message = $"{model.HostelName} Updated Successfully...";
                        return new JsonResult { Data = new { status = true, message } };
                    }
                }
                else
                {
                    _db.Hostels.Add(model);
                    await _db.SaveChangesAsync();
                    message = $"{model.HostelName} Added Successfully.";
                    return new JsonResult { Data = new { status = true, message } };
                }
            }
            return new JsonResult { Data = new { status, message } };
            //return View(subject);
        }


        // GET: Hostels/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Hostel hostel = await _db.Hostels.FindAsync(id);
            if (hostel == null)
            {
                return HttpNotFound();
            }
            return View(hostel);
        }

        // GET: Hostels/Create
        public ActionResult Create()
        {
            ViewBag.FacultyId = new SelectList(_db.Faculties.AsNoTracking(), "FacultyId", "FacultyName");
            var gender = from Gender s in Enum.GetValues(typeof(Gender))
                         select new { ID = s, Name = s.ToString() };

            ViewBag.Gender = new MultiSelectList(gender, "Name", "Name");
            return View();
        }

        // POST: Hostels/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Hostel hostel)
        {
            if (ModelState.IsValid)
            {
                _db.Hostels.Add(hostel);
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            // ViewBag.FacultyId = new SelectList(_db.Faculties.AsNoTracking(), "FacultyId", "FacultyName", hostel.FacultyId);
            var gender = from Gender s in Enum.GetValues(typeof(Gender))
                         select new { ID = s, Name = s.ToString() };

            ViewBag.Gender = new MultiSelectList(gender, "Name", "Name");
            return View(hostel);
        }

        // GET: Hostels/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Hostel hostel = await _db.Hostels.FindAsync(id);
            if (hostel == null)
            {
                return HttpNotFound();
            }
            // ViewBag.FacultyId = new SelectList(_db.Faculties.AsNoTracking(), "FacultyId", "FacultyName", hostel.FacultyId);
            var gender = from Gender s in Enum.GetValues(typeof(Gender))
                         select new { ID = s, Name = s.ToString() };

            ViewBag.Gender = new MultiSelectList(gender, "Name", "Name"); return View(hostel);
        }

        // POST: Hostels/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Hostel hostel)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(hostel).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            // ViewBag.FacultyId = new SelectList(_db.Faculties.AsNoTracking(), "FacultyId", "FacultyName", hostel.FacultyId);
            var gender = from Gender s in Enum.GetValues(typeof(Gender))
                         select new { ID = s, Name = s.ToString() };

            ViewBag.Gender = new MultiSelectList(gender, "Name", "Name"); return View(hostel);
        }

        // GET: Hostels/Delete/5

        public async Task<PartialViewResult> Delete(int id)
        {
            var hostel = await _db.Hostels.FindAsync(id);
            return PartialView(hostel);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            bool status = false;
            string message = string.Empty;
            var hostel = await _db.Hostels.FindAsync(id);
            if (hostel != null)
            {
                _db.Hostels.Remove(hostel);
                await _db.SaveChangesAsync();
                status = true;
                message = "Hostel Deleted Successfully...";
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
