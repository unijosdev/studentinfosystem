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
    public class LectureRoomsController : BaseController
    {

        public LectureRoomsController(SchoolDbContext db) : base(db)
        {

        }
        // GET: LectureRooms
        public ActionResult Index()
        {
            // var lectureRooms = _db.LectureRooms.Include(l => l.Building);
            return View();
        }
        public async Task<ActionResult> GetIndex()
        {
            // dc.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var data = await _db.LectureRooms.Include(i => i.Building).AsNoTracking().Select(s => new
            {
                s.Building.BuildingName,
                s.LectureRoomId,
                s.LectureRoomName,
                s.LectureRoomCode,
                s.LectureRoomCapacity,
                s.SittingCapacity,
                s.IsGeneralLectureHall

            }).ToListAsync();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        public async Task<PartialViewResult> Save(int id)
        {
            var lectureRoom = await _db.LectureRooms.FindAsync(id);
            ViewBag.BuildingId = new SelectList(_db.Buildings, "BuildingId", "BuildingName");

            return PartialView(lectureRoom);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Save(LectureRoom model)
        {
            bool status = false;
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                if (model.LectureRoomId > 0)
                {
                    var lectureRoom = await _db.LectureRooms.FindAsync(model.LectureRoomId);
                    if (lectureRoom != null)
                    {
                        lectureRoom.BuildingId = model.BuildingId;
                        lectureRoom.LectureRoomCode = model.LectureRoomCode;
                        lectureRoom.LectureRoomName = model.LectureRoomName;
                        lectureRoom.LectureRoomCapacity = model.LectureRoomCapacity;
                        _db.Entry(lectureRoom).State = EntityState.Modified;
                        await _db.SaveChangesAsync();
                        message = $"{model.LectureRoomName} Updated Successfully...";
                        return new JsonResult { Data = new { status = true, message } };
                    }
                }
                else
                {
                    _db.LectureRooms.Add(model);
                    await _db.SaveChangesAsync();
                    message = $"{model.LectureRoomName} Added Successfully.";
                    return new JsonResult { Data = new { status = true, message } };
                }
            }
            return new JsonResult { Data = new { status, message } };
            //return View(subject);
        }

        #region MyRegion


        //// GET: LectureRooms/Details/5
        //public async Task<ActionResult> Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    LectureRoom lectureRoom = await _db.LectureRooms.FindAsync(id);
        //    if (lectureRoom == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(lectureRoom);
        //}

        //// GET: LectureRooms/Create
        //public ActionResult Create()
        //{
        //    ViewBag.BuildingId = new SelectList(_db.Buildings, "BuildingId", "BuildingName");
        //    return View();
        //}

        //// POST: LectureRooms/Create
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Create([Bind(Include = "LectureRoomId,BuildingId,LectureRoomName,LectureRoomCode,LectureRoomCapacity")] LectureRoom lectureRoom)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _db.LectureRooms.Add(lectureRoom);
        //        await _db.SaveChangesAsync();
        //        return RedirectToAction("Index");
        //    }

        //    ViewBag.BuildingId = new SelectList(_db.Buildings, "BuildingId", "BuildingName", lectureRoom.BuildingId);
        //    return View(lectureRoom);
        //}

        //// GET: LectureRooms/Edit/5
        //public async Task<ActionResult> Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    LectureRoom lectureRoom = await _db.LectureRooms.FindAsync(id);
        //    if (lectureRoom == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    ViewBag.BuildingId = new SelectList(_db.Buildings, "BuildingId", "BuildingName", lectureRoom.BuildingId);
        //    return View(lectureRoom);
        //}

        //// POST: LectureRooms/Edit/5
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Edit([Bind(Include = "LectureRoomId,BuildingId,LectureRoomName,LectureRoomCode,LectureRoomCapacity")] LectureRoom lectureRoom)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _db.Entry(lectureRoom).State = EntityState.Modified;
        //        await _db.SaveChangesAsync();
        //        return RedirectToAction("Index");
        //    }
        //    ViewBag.BuildingId = new SelectList(_db.Buildings, "BuildingId", "BuildingName", lectureRoom.BuildingId);
        //    return View(lectureRoom);
        //} 
        #endregion

        // GET: LectureRooms/Delete/5
        public async Task<PartialViewResult> Delete(int id)
        {
            var lectureRoom = await _db.LectureRooms.FindAsync(id);
            return PartialView(lectureRoom);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            bool status = false;
            string message = string.Empty;
            var lectureRoom = await _db.LectureRooms.FindAsync(id);
            if (lectureRoom != null)
            {
                _db.LectureRooms.Remove(lectureRoom);
                await _db.SaveChangesAsync();
                status = true;
                message = "Lecture Room Deleted Successfully...";
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
