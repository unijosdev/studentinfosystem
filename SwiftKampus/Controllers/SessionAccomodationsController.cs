using SwiftKampus.BusinessLogic;
using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampusModel.Accomodation;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace SwiftKampus.Controllers
{
    [Authorize]
    [Audit(AuditingLevel = 2)]
    public class SessionAccomodationsController : BaseController
    {

        public SessionAccomodationsController(SchoolDbContext db) : base(db)
        {

        }
        // GET: SessionAccomodations
        public ActionResult Index()
        {
            ViewBag.SessionId = new SelectList(_db.Sessions.AsNoTracking().OrderByDescending(x => x.SessionName).ToList(), "SessionId", "SessionName");
            ViewBag.HostelId = new SelectList(_db.Hostels.AsNoTracking().ToList(), "HostelId", "HostelName");
            //ViewBag.ProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FancyName");

            return View();
        }
        //public async task<actionresult> getindex(int? sessionid)
        //{
        //    // dc.configuration.lazyloadingenabled = false; // if your table is relational, contain foreign key
        //    //var data =;
        //    if (sessionid == null)
        //    {
        //        var data = await _db.sessionaccomodations
        //         //.where(s => s.sessionid.equals((int)sessionid))
        //         .include(s => s.room)
        //         .include(s => s.session)
        //         .asnotracking()
        //         .select(s => new
        //         {
        //             s.session.sessionname,
        //             s.hostel.hostelname,
        //             s.block.blockname,
        //             s.room.roomname,

        //         }).tolistasync();

        //        return json(new { data }, jsonrequestbehavior.allowget);
        //    }
        //    else
        //    {
        //        var data = await _db.sessionaccomodations
        //      .where(s => s.sessionid.equals((int)sessionid))
        //      .include(s => s.room)
        //      .include(s => s.session)
        //      .asnotracking()
        //      .select(s => new
        //      {
        //          s.session.sessionname,
        //          s.hostel.hostelname,
        //          s.block.blockname,
        //          s.room.roomname,

        //      }).tolistasync();
        //        return json(new { data }, jsonrequestbehavior.allowget);
        //    }


        //}

        public async Task<ActionResult> GetIndex(int? sessionId, int? hostelId, int? blockId)
        {
            // dc.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            //var data =;
            if (sessionId == null)
            {
                var data = await _db.SessionAccomodations
                 //.Where(s => s.SessionId.Equals((int)sessionId))
                 .Include(s => s.Room)
                 .Include(s => s.Session)
                 .AsNoTracking()
                 .Select(s => new
                 {
                     s.Session.SessionName,
                     s.Hostel.HostelName,
                     s.Block.BlockName,
                     s.Room.RoomName,
                 }).ToListAsync();

                return Json(new { data }, JsonRequestBehavior.AllowGet);
            }

            if (sessionId != null && hostelId != null && blockId != null)
            {
                var data = await _db.SessionAccomodations
              .Where(s => s.SessionId.Equals((int)sessionId) && s.HostelId == (int)hostelId && s.BlockId == (int)blockId)
              .Include(s => s.Room)
              .Include(s => s.Session)
              .AsNoTracking()
              .Select(s => new
              {
                  s.Session.SessionName,
                  s.Hostel.HostelName,
                  s.Block.BlockName,
                  s.Room.RoomName,
              }).ToListAsync();
                return Json(new { data }, JsonRequestBehavior.AllowGet);
            }

            if (sessionId != null && hostelId != null)
            {
                var data = await _db.SessionAccomodations
              .Where(s => s.SessionId.Equals((int)sessionId) && s.HostelId == (int)hostelId)
              .Include(s => s.Room)
              .Include(s => s.Session)
              .AsNoTracking()
              .Select(s => new
              {
                  s.Session.SessionName,
                  s.Hostel.HostelName,
                  s.Block.BlockName,
                  s.Room.RoomName,
              }).ToListAsync();
                return Json(new { data }, JsonRequestBehavior.AllowGet);
            }

            else
            {
                var data = await _db.SessionAccomodations
              .Where(s => s.SessionId.Equals((int)sessionId))
              .Include(s => s.Room)
              .Include(s => s.Session)
              .AsNoTracking()
              .Select(s => new
              {
                  s.Session.SessionName,
                  s.Hostel.HostelName,
                  s.Block.BlockName,
                  s.Room.RoomName,
              }).ToListAsync();
                return Json(new { data }, JsonRequestBehavior.AllowGet);
            }
        }

        public async Task<ActionResult> DeleteLastSessionMakeAvailable(int Id)
        {
            var availableHostel = await _db.SessionAccomodations.Where(s => s.SessionId.Equals(Id)).ToListAsync();

            foreach (var item in availableHostel)
            {
                if (item != null)
                {
                    item.SessionId = 20;
                    //_db.Entry(item).State = EntityState.Deleted;
                    _db.Entry(item).State = EntityState.Modified;

                }
            }
            try
            {
                _db.SaveChanges();
            }
            catch (System.Exception)
            {

                throw;
            }
            return View();
        }
        public async Task<PartialViewResult> Save(int id)
        {
            var seesionAccom = await _db.SessionAccomodations.FindAsync(id);
            ViewBag.BlockId = new SelectList(_db.Blocks.AsNoTracking(), "BlockId", "BlockName");
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            ViewBag.HostelId = new SelectList(_db.Hostels.AsNoTracking().ToList(), "HostelId", "HostelName");

            return PartialView(seesionAccom);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Save(SessionAccomodationVm model)
        {
            bool status = false;
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                foreach (var block in model.BlockId)
                {
                    var rooms = await _db.Rooms.AsNoTracking().Where(x => x.BlockId.Equals(block)).ToListAsync();
                    var hostel = await _db.Blocks.AsNoTracking().Where(x => x.BlockId.Equals(block)).Select(s => s.HostelId)
                        .FirstOrDefaultAsync();

                    foreach (var room in rooms)
                    {
                        SessionAccomodation sessionAccomodation = new SessionAccomodation
                        {
                            SessionId = model.SessionId,
                            RoomId = room.RoomId,
                            BlockId = block,
                            HostelId = hostel
                        };
                        _db.SessionAccomodations.Add(sessionAccomodation);
                    }
                }
                await _db.SaveChangesAsync();
                message = " Session Accomodation Added Successfully.";
                return new JsonResult { Data = new { status = true, message } };

            }
            return new JsonResult { Data = new { status, message } };
            //return View(subject);
        }



        // GET: SessionAccomodations/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SessionAccomodation sessionAccomodation = await _db.SessionAccomodations.FindAsync(id);
            if (sessionAccomodation == null)
            {
                return HttpNotFound();
            }
            return View(sessionAccomodation);
        }

        // GET: SessionAccomodations/Create
        public ActionResult Create()
        {
            ViewBag.BlockId = new SelectList(_db.Blocks.AsNoTracking(), "BlockId", "BlockName");
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            return View();
        }

        // POST: SessionAccomodations/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(SessionAccomodationVm model)
        {
            if (ModelState.IsValid)
            {
                foreach (var block in model.BlockId)
                {
                    var rooms = await _db.Rooms.AsNoTracking().Where(x => x.BlockId.Equals(block)).ToListAsync();
                    var hostel = await _db.Blocks.AsNoTracking().Where(x => x.BlockId.Equals(block)).Select(s => s.HostelId)
                                        .FirstOrDefaultAsync();

                    foreach (var room in rooms)
                    {
                        SessionAccomodation sessionAccomodation = new SessionAccomodation
                        {
                            SessionId = model.SessionId,
                            RoomId = room.RoomId,
                            BlockId = block,
                            HostelId = hostel
                        };
                        _db.SessionAccomodations.Add(sessionAccomodation);
                    }
                }
                // db.SessionAccomodations.Add(sessionAccomodation);
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.BlockId = new SelectList(_db.Blocks, "BlockId", "BlockName");
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");

            var body = "Hostel hostel jus went off";
            await SMSClass.SendSMS("UNIJOS SIS", body, "07035473090");
            return View(model);
        }

        // GET: SessionAccomodations/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SessionAccomodation sessionAccomodation = await _db.SessionAccomodations.FindAsync(id);
            if (sessionAccomodation == null)
            {
                return HttpNotFound();
            }
            ViewBag.RoomId = new SelectList(_db.Rooms.AsNoTracking(), "RoomId", "RoomName", sessionAccomodation.RoomId);
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName", sessionAccomodation.SessionId);
            return View(sessionAccomodation);
        }

        // POST: SessionAccomodations/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "SessionAccomodationId,SessionId,RoomId,BlockId,HostelId")] SessionAccomodation sessionAccomodation)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(sessionAccomodation).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.RoomId = new SelectList(_db.Rooms.AsNoTracking(), "RoomId", "RoomName", sessionAccomodation.RoomId);
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName", sessionAccomodation.SessionId);
            return View(sessionAccomodation);
        }

        // GET: SessionAccomodations/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SessionAccomodation sessionAccomodation = await _db.SessionAccomodations.FindAsync(id);
            if (sessionAccomodation == null)
            {
                return HttpNotFound();
            }
            return View(sessionAccomodation);
        }

        // POST: SessionAccomodations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            SessionAccomodation sessionAccomodation = await _db.SessionAccomodations.FindAsync(id);
            if (sessionAccomodation != null) _db.SessionAccomodations.Remove(sessionAccomodation);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // GET: Get the blocks in a hostel
        [Authorize]
        public ActionResult GetHostBlocks(int hostelId)
        {
            var blocks = _db.Blocks
                 .Where(s => s.HostelId.Equals((int)hostelId)).AsNoTracking()
                 .Select(s => new
                 {
                     s.BlockId,
                     s.BlockName,
                 }).OrderByDescending(s => s.BlockName);

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            var data = javaScriptSerializer.Serialize(blocks);

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        // GET: Get the hostel blocks that haven't been made available
        [Authorize]
        public ActionResult GetAvailableBlocks(int hostelId, int sessionId)
        {
            //List<string> availableBlocks = new List<string>();

            var blocks = _db.Blocks
                 .Where(s => s.HostelId.Equals((int)hostelId)).AsNoTracking()
                 .Select(s => new
                 {
                     s.BlockId,
                     s.BlockName
                 }).ToList();

            var sessionBlocks = _db.SessionAccomodations.Include(s => s.Block)
                 .Where(s => s.HostelId == (int)hostelId && s.SessionId == ((int)sessionId)).AsNoTracking()
                 .Select(s => new
                 {
                     s.BlockId
                 }).Distinct().ToList();

            if (sessionBlocks.Count == 0)
            {
                JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                var data = javaScriptSerializer.Serialize(blocks);

                return Json(data, JsonRequestBehavior.AllowGet);
            }
            else
            {
                foreach (var sessionBlock in sessionBlocks)
                {
                    foreach (var block in blocks.ToList())
                    {
                        if (block.BlockId == sessionBlock.BlockId)
                        {
                            blocks.Remove(block);
                        }
                    }
                }

                JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                var data = javaScriptSerializer.Serialize(blocks);

                return Json(data, JsonRequestBehavior.AllowGet);
            }

            
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
