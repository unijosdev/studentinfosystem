using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampusModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SwiftKampus.Controllers
{
    [Authorize]
    [Audit(AuditingLevel = 2)]
    public class StaffPositionsController : BaseController
    {

        public StaffPositionsController(SchoolDbContext db) : base(db)
        {

        }
        // GET: StaffPositions
        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> GetIndex()
        {
            // dc.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var data = await _db.StaffPositions.AsNoTracking().Select(s => new { s.StaffPositionId, s.PositionName, s.PositionCode, s.PositionTypeName }).ToListAsync();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        //// GET: StaffPositions/Details/5
        //public async Task<ActionResult> Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    StaffPosition staffPosition = await _db.StaffPositions.FindAsync(id);
        //    if (staffPosition == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(staffPosition);
        //}
        public async Task<PartialViewResult> Save(int id)
        {
            var staffPosition = await _db.StaffPositions.FindAsync(id);
            return PartialView(staffPosition);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Save(StaffPosition model)
        {
            bool status = false;
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                if (model.StaffPositionId > 0)
                {
                    var staffPosition = await _db.StaffPositions.FindAsync(model.StaffPositionId);
                    if (staffPosition != null)
                    {
                        try
                        {
                            staffPosition.PositionCode = model.PositionCode.Trim();
                            staffPosition.PositionName = model.PositionName.Trim();
                            staffPosition.PositionType = model.PositionType;

                            _db.Entry(staffPosition).State = EntityState.Modified;
                            await _db.SaveChangesAsync();
                            message = $"{model.PositionName} Updated Successfully...";
                            return new JsonResult { Data = new { status = true, message } };
                        }
                        catch (Exception ex)
                        {
                            return new JsonResult { Data = new { status = false, message = ex.Message } };
                        }
                    }
                }
                else
                {
                    try
                    {
                        model.PositionCode = model.PositionCode.Trim();
                        model.PositionName = model.PositionName.Trim();
                        _db.StaffPositions.Add(model);
                        await _db.SaveChangesAsync();
                        message = $"{model.PositionName} Added Successfully.";
                        return new JsonResult { Data = new { status = true, message } };
                    }
                    catch (Exception ex)
                    {
                        return new JsonResult { Data = new { status = false, message = ex.Message } };
                    }

                }
            }
            return new JsonResult { Data = new { status, message } };
            //return View(subject);
        }



        // GET: StaffPositions/Delete/5
        public async Task<PartialViewResult> Delete(int id)
        {
            var staffPosition = await _db.StaffPositions.FindAsync(id);
            return PartialView(staffPosition);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            bool status = false;
            string message = string.Empty;
            var staffPosition = await _db.StaffPositions.FindAsync(id);
            if (staffPosition != null)
            {
                _db.StaffPositions.Remove(staffPosition);
                await _db.SaveChangesAsync();
                status = true;
                message = "Staff Position Deleted Successfully...";
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
