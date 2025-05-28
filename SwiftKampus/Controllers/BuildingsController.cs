using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampusModel.TimeTable;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SwiftKampus.Controllers
{
    [Authorize]
    [Audit(AuditingLevel = 2)]
    public class BuildingsController : BaseController
    {

        public BuildingsController(SchoolDbContext db) : base(db)
        {

        }

        // GET: Buildings
        public ActionResult Index()
        {
            return View();
        }
        public async Task<ActionResult> GetIndex()
        {
            // dc.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var data = await _db.Buildings.AsNoTracking().Select(s => new
            {
                s.BuildingId,
                s.BuildingName,
                s.BuildingCode,
                s.BuildingLocation,
                s.IsMultiPurpose
            }).ToListAsync();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        public async Task<PartialViewResult> Save(int id)
        {
            var building = await _db.Buildings.FindAsync(id);
            return PartialView(building);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Save(Building model)
        {
            bool status = false;
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                if (model.BuildingId > 0)
                {
                    var building = await _db.Buildings.FindAsync(model.BuildingId);
                    if (building != null)
                    {
                        building.BuildingCode = model.BuildingCode;
                        building.BuildingName = model.BuildingName;
                        building.BuildingLocation = model.BuildingLocation;
                        _db.Entry(building).State = EntityState.Modified;
                        await _db.SaveChangesAsync();
                        message = $"{model.BuildingName} Updated Successfully...";
                        return new JsonResult { Data = new { status = true, message } };
                    }
                }
                else
                {
                    _db.Buildings.Add(model);
                    await _db.SaveChangesAsync();
                    message = $"{model.BuildingName} Added Successfully.";
                    return new JsonResult { Data = new { status = true, message } };
                }
            }
            return new JsonResult { Data = new { status, message } };
            //return View(subject);
        }



        #region MyRegion
        //// GET: Buildings/Details/5
        //public async Task<ActionResult> Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Building building = await _db.Buildings.FindAsync(id);
        //    if (building == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(building);
        //}

        //// GET: Buildings/Create
        //public ActionResult Create()
        //{
        //    return View();
        //}

        //// POST: Buildings/Create
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Create([Bind(Include = "BuildingId,BuildingName,BuildingCode,BuildingLocation")] Building building)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _db.Buildings.Add(building);
        //        await _db.SaveChangesAsync();
        //        return RedirectToAction("Index");
        //    }

        //    return View(building);
        //}

        //// GET: Buildings/Edit/5
        //public async Task<ActionResult> Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Building building = await _db.Buildings.FindAsync(id);
        //    if (building == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(building);
        //}

        //// POST: Buildings/Edit/5
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Edit(Building building)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _db.Entry(building).State = EntityState.Modified;
        //        await _db.SaveChangesAsync();
        //        return RedirectToAction("Index");
        //    }
        //    return View(building);
        //} 
        #endregion

        // GET: Buildings/Delete/5
        public async Task<PartialViewResult> Delete(int id)
        {
            var building = await _db.Buildings.FindAsync(id);
            return PartialView(building);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            bool status = false;
            string message = string.Empty;
            var building = await _db.Buildings.FindAsync(id);
            if (building != null)
            {
                _db.Buildings.Remove(building);
                await _db.SaveChangesAsync();
                status = true;
                message = "Building Deleted Successfully...";
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
