using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampusModel.TimeTable;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SwiftKampus.Controllers
{
    [Authorize]
    [Audit(AuditingLevel = 2)]
    public class AssignFacultyBuildingsController : BaseController
    {
        public AssignFacultyBuildingsController(SchoolDbContext db) : base(db)
        {

        }
        // GET: AssignFacultyBuildings
        public ActionResult Index()
        {            
            return View();
        }

        public async Task<ActionResult> GetIndex()
        {
            // dc.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var data = await _db.AssignFacultyBuildings.Include(a => a.Building).Include(a => a.Faculty).AsNoTracking()
                        .Select(s => new
            {
                s.AssignFacultyBuildingId,
                s.Building.BuildingName,
                s.Faculty.FacultyName,                
            }).ToListAsync();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        public async Task<PartialViewResult> Save(int id)
        {
            var assignFacultyBuilding = await _db.AssignFacultyBuildings.FindAsync(id);
            ViewBag.BuildingId = new SelectList(_db.Buildings, "BuildingId", "BuildingName", assignFacultyBuilding?.BuildingId);
            ViewBag.FacultyId = new SelectList(_db.Faculties, "FacultyId", "FacultyCode", assignFacultyBuilding?.FacultyId);
            return PartialView(assignFacultyBuilding);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Save(AssignFacultyBuilding model)
        {
            bool status = false;
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                if (model.AssignFacultyBuildingId > 0)
                {
                    var assignFacultyBuilding = await _db.AssignFacultyBuildings.FindAsync(model.AssignFacultyBuildingId);
                    if (assignFacultyBuilding != null)
                    {
                        assignFacultyBuilding.BuildingId = model.BuildingId;
                        assignFacultyBuilding.FacultyId = model.FacultyId;
                        _db.Entry(assignFacultyBuilding).State = EntityState.Modified;
                        await _db.SaveChangesAsync();
                        message = "Assigned building Updated Successfully...";
                        return new JsonResult { Data = new { status = true, message } };
                    }
                }
                else
                {
                    _db.AssignFacultyBuildings.Add(model);
                    await _db.SaveChangesAsync();
                    message = "Building assigned to faculty Successfully.";
                    return new JsonResult { Data = new { status = true, message } };
                }
            }
            return new JsonResult { Data = new { status, message } };
            //return View(subject);
        }


        // GET: AssignFacultyBuildings/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AssignFacultyBuilding assignFacultyBuilding = await _db.AssignFacultyBuildings.FindAsync(id);
            if (assignFacultyBuilding == null)
            {
                return HttpNotFound();
            }
            return View(assignFacultyBuilding);
        }

        // GET: AssignFacultyBuildings/Create
        public ActionResult Create()
        {
            ViewBag.BuildingId = new SelectList(_db.Buildings, "BuildingId", "BuildingName");
            ViewBag.FacultyId = new SelectList(_db.Faculties, "FacultyId", "FacultyCode");
            return View();
        }

        // POST: AssignFacultyBuildings/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "AssignFacultyBuildingId,BuildingId,FacultyId")] AssignFacultyBuilding assignFacultyBuilding)
        {
            if (ModelState.IsValid)
            {
                _db.AssignFacultyBuildings.Add(assignFacultyBuilding);
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.BuildingId = new SelectList(_db.Buildings, "BuildingId", "BuildingName", assignFacultyBuilding.BuildingId);
            ViewBag.FacultyId = new SelectList(_db.Faculties, "FacultyId", "FacultyCode", assignFacultyBuilding.FacultyId);
            return View(assignFacultyBuilding);
        }

        // GET: AssignFacultyBuildings/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AssignFacultyBuilding assignFacultyBuilding = await _db.AssignFacultyBuildings.FindAsync(id);
            if (assignFacultyBuilding == null)
            {
                return HttpNotFound();
            }
            ViewBag.BuildingId = new SelectList(_db.Buildings, "BuildingId", "BuildingName", assignFacultyBuilding.BuildingId);
            ViewBag.FacultyId = new SelectList(_db.Faculties, "FacultyId", "FacultyCode", assignFacultyBuilding.FacultyId);
            return View(assignFacultyBuilding);
        }

        // POST: AssignFacultyBuildings/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "AssignFacultyBuildingId,BuildingId,FacultyId")] AssignFacultyBuilding assignFacultyBuilding)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(assignFacultyBuilding).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.BuildingId = new SelectList(_db.Buildings, "BuildingId", "BuildingName", assignFacultyBuilding.BuildingId);
            ViewBag.FacultyId = new SelectList(_db.Faculties, "FacultyId", "FacultyCode", assignFacultyBuilding.FacultyId);
            return View(assignFacultyBuilding);
        }

        // GET: AssignFacultyBuildings/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AssignFacultyBuilding assignFacultyBuilding = await _db.AssignFacultyBuildings.FindAsync(id);
            if (assignFacultyBuilding == null)
            {
                return HttpNotFound();
            }
            return View(assignFacultyBuilding);
        }

        // POST: AssignFacultyBuildings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            AssignFacultyBuilding assignFacultyBuilding = await _db.AssignFacultyBuildings.FindAsync(id);
            _db.AssignFacultyBuildings.Remove(assignFacultyBuilding);
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
