using OfficeOpenXml;
using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampusModel.Accomodation;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace SwiftKampus.Controllers
{
    [Authorize]
    [Audit(AuditingLevel = 2)]
    public class RoomsController : BaseController
    {

        public RoomsController(SchoolDbContext db) : base(db)
        {

        }

        // GET: Rooms
        public ActionResult Index(string message)
        {
            ViewBag.Message = message;
            return View();
        }

        public async Task<ActionResult> GetIndex()
        {
            // dc.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var data = await _db.Rooms.Include(i => i.Buildings).AsNoTracking().Select(s => new { s.RoomName, s.RoomId, s.Buildings.BlockName, s.RoomCapacity }).ToListAsync();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        // GET: Rooms
        public ActionResult ReservedIndex(string message)
        {
            ViewBag.Message = message;
            return View();
        }

        public ActionResult ReservedSupervision(string message)
        {
            ViewBag.Message = message;
            return View();
        }

        public async Task<ActionResult> GetReservedIndex()
        {
            // dc.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var data = await _db.ReservedRooms.Include(i => i.Room.Buildings)
                .AsNoTracking().Select(s => new { s.Room.RoomName, s.ReservedRoomId, s.ReasonForReserve, s.Room.Buildings.BlockName }).ToListAsync();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult RemoveUnAssignedReservedSppace(string hostelName)
        {
            var hostel = _db.Hostels.Where(x => x.HostelName.Trim().ToUpper().Equals(hostelName.Trim().ToUpper())).FirstOrDefault();
            var unAssignedReservations =  _db.ReservedRooms.Include(r => r.Room.StudentAssignedRooms)
                                            .Where(x => x.Room.Buildings.Hostel.HostelId.Equals(hostel.HostelId)).ToList();
            foreach (var room in unAssignedReservations)
            {
                //check if room is assigned for the current session
                var checkRoomAssign = _db.StudentAssignedRooms.Include(a => a.Session).Where(a => a.RoomId.Equals(room.RoomId)
                && a.Session.SessionId.Equals(24)).ToList();
                if (checkRoomAssign.Count() == 0)
                {
                    _db.ReservedRooms.Remove(room);
                    _db.SaveChanges();
                }
            }

            return View();
        }

        public async Task<PartialViewResult> Reserve(int id)
        {
            List<Room> room;
            ViewBag.BlockId = new SelectList(_db.Blocks.AsNoTracking(), "BlockId", "BlockName");
            if (id > 0)
            {
                room = await _db.Rooms.AsNoTracking().Where(x => x.RoomId.Equals(id)).ToListAsync();
                ViewBag.RoomId = new SelectList(room, "RoomId", "RoomName");
                var reserveRoom = await _db.ReservedRooms.AsNoTracking().Where(x => x.RoomId.Equals(id))
                    .FirstOrDefaultAsync();
                return PartialView(reserveRoom);
            }
            room = await _db.Rooms.AsNoTracking().ToListAsync();
            ViewBag.RoomId = new SelectList(room, "RoomId", "RoomName");
            return PartialView();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Reserve(ReservedRoom model)
        {
            bool status = false;
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                if (model.ReservedRoomId > 0)
                {
                    var reservedRoom = await _db.ReservedRooms.FindAsync(model.RoomId);
                    if (reservedRoom != null)
                    {
                        _db.ReservedRooms.Remove(reservedRoom);
                        await _db.SaveChangesAsync();


                        message = $"{model.Room.RoomName} Removed Successfully...";
                        return new JsonResult { Data = new { status = true, message } };
                    }
                }
                else
                {
                    _db.ReservedRooms.Add(model);
                    await _db.SaveChangesAsync();
                    message = $"{model.RoomId} reserved Successfully.";
                    return new JsonResult { Data = new { status = true, message } };
                }
            }
            return new JsonResult { Data = new { status, message } };
            //return View(subject);
        }

        public async Task<PartialViewResult> Save(int id)
        {
            var room = await _db.Rooms.FindAsync(id);
            ViewBag.BlockId = new SelectList(_db.Blocks.AsNoTracking(), "BlockId", "BlockName");

            return PartialView(room);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Save(Room model)
        {
            bool status = false;
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                if (model.RoomId > 0)
                {
                    var room = await _db.Rooms.FindAsync(model.RoomId);
                    if (room != null)
                    {
                        room.RoomName = model.RoomName;
                        room.RoomCapacity = model.RoomCapacity;
                        room.BlockId = model.BlockId;

                        _db.Entry(room).State = EntityState.Modified;
                        await _db.SaveChangesAsync();
                        message = $"{model.RoomName} Updated Successfully...";
                        return new JsonResult { Data = new { status = true, message } };
                    }
                }
                else
                {
                    _db.Rooms.Add(model);
                    await _db.SaveChangesAsync();
                    message = $"{model.RoomName} Added Successfully.";
                    return new JsonResult { Data = new { status = true, message } };
                }
            }
            return new JsonResult { Data = new { status, message } };
            //return View(subject);
        }


        // GET: Rooms/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Room room = await _db.Rooms.FindAsync(id);
            if (room == null)
            {
                return HttpNotFound();
            }
            return View(room);
        }

        // GET: Rooms/Create
        public ActionResult Create()
        {
            ViewBag.BlockId = new SelectList(_db.Blocks.AsNoTracking(), "BlockId", "BlockName");
            return View();
        }

        // POST: Rooms/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "RoomId,BlockId,RoomName,RoomCapacity")] Room room)
        {
            if (ModelState.IsValid)
            {
                _db.Rooms.Add(room);
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.BlockId = new SelectList(_db.Blocks.AsNoTracking(), "BlockId", "BlockName", room.BlockId);
            return View(room);
        }

        // GET: Rooms/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Room room = await _db.Rooms.FindAsync(id);
            if (room == null)
            {
                return HttpNotFound();
            }
            ViewBag.BlockId = new SelectList(_db.Blocks.AsNoTracking(), "BlockId", "BlockName", room.BlockId);
            return View(room);
        }

        // POST: Rooms/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "RoomId,BlockId,RoomName,RoomCapacity")] Room room)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(room).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.BlockId = new SelectList(_db.Blocks.AsNoTracking(), "BlockId", "BlockName", room.BlockId);
            return View(room);
        }

        // GET: Rooms/Delete/5
        public async Task<PartialViewResult> Delete(int id)
        {
            var room = await _db.Rooms.FindAsync(id);
            return PartialView(room);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            bool status = false;
            string message = string.Empty;
            var room = await _db.Rooms.FindAsync(id);
            if (room != null)
            {
                _db.Rooms.Remove(room);
                await _db.SaveChangesAsync();
                status = true;
                message = "Rooms Deleted Successfully...";
                return new JsonResult { Data = new { status, message } };
            }

            return new JsonResult { Data = new { status = false, message = $"{id } not deleted" } };
        }

        // GET: Rooms/Delete/5
        public async Task<PartialViewResult> DeleteReserved(int id)
        {
            var room = await _db.ReservedRooms.Include(i => i.Room).AsNoTracking()
                                .Where(x => x.ReservedRoomId.Equals(id)).FirstOrDefaultAsync();
            return PartialView(room);
        }

        [HttpPost, ActionName("DeleteReserved")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteReservedConfirmed(int id)
        {
            bool status = false;
            string message = string.Empty;
            var room = await _db.ReservedRooms.FindAsync(id);
            if (room != null)
            {
                _db.ReservedRooms.Remove(room);
                await _db.SaveChangesAsync();
                status = true;
                message = "Reserved Room Deleted Successfully...";
                return new JsonResult { Data = new { status, message } };
            }

            return new JsonResult { Data = new { status = false, message = $"{id } not deleted" } };
        }

        public PartialViewResult UploadRooms()
        {
            return PartialView();
        }


        [HttpPost]
        public async Task<ActionResult> UploadRooms(HttpPostedFileBase excelfile)
        {
            if (excelfile == null || excelfile.ContentLength == 0)
            {
                ViewBag.Error = "Please Select a excel file <br/>";
                ViewBag.Message = "You must select an excel file before you click Upload button";
                TempData["Title"] = "Error.";

                return View("Index");
            }
            HttpPostedFileBase file = Request.Files["excelfile"];
            if (excelfile.FileName.EndsWith("xls", StringComparison.CurrentCulture) ||
                excelfile.FileName.EndsWith("xlsx", StringComparison.CurrentCulture))
            {
                string lastrecord = "";
                int recordCount = 0;
                string message = "";
                string fileContentType = file.ContentType;
                byte[] fileBytes = new byte[file.ContentLength];
                var data = file.InputStream.Read(fileBytes, 0, Convert.ToInt32(file.ContentLength));

                // Read data from excel file
                using (var package = new ExcelPackage(file.InputStream))
                {
                    ExcelValidation myExcel = new ExcelValidation();
                    var currentSheet = package.Workbook.Worksheets;
                    var workSheet = currentSheet.First();
                    var noOfCol = workSheet.Dimension.End.Column;
                    var noOfRow = workSheet.Dimension.End.Row;
                    int requiredField = 3;

                    string validCheck = myExcel.ValidateExcel(noOfRow, workSheet, requiredField);
                    if (!validCheck.Equals("Success"))
                    {
                        //string row = "";
                        //string column = "";
                        string[] ssizes = validCheck.Split(' ');
                        string[] myArray = new string[2];
                        for (int i = 0; i < ssizes.Length; i++)
                        {
                            myArray[i] = ssizes[i];
                            // myArray[i] = ssizes[];
                        }
                        string lineError = $"Line/Row number {myArray[0]}  and column {myArray[1]} is not rightly formatted, Please Check for anomalies ";
                        //ViewBag.LineError = lineError;
                        ViewBag.Message = lineError;
                        return View("Index");
                    }
                    for (int row = 2; row <= noOfRow; row++)
                    {
                        var blockName = workSheet.Cells[row, 1].Value.ToString().Trim();


                        var block = await _db.Blocks.AsNoTracking()
                            .Where(x => x.BlockName.Trim().ToUpper().Equals(blockName.ToUpper()))
                            .FirstOrDefaultAsync();

                        if (block == null)
                        {

                            ViewBag.ErrorInfo = "Please Leave no column or row Empty/Blank";
                            ViewBag.ErrorMessage = $" The \"{blockName}\" Block Name specified in the excel doesn't exist on the portal. " +
                                                   $"Please add/update the Hostel first before uploading or correct the excel sheet...";
                            return View("ErrorException");
                        }

                        try
                        {
                            var room = new Room()
                            {
                                BlockId = block.BlockId,
                                RoomName = workSheet.Cells[row, 2].Value.ToString().Trim(),
                                RoomCapacity = Convert.ToUInt16(workSheet.Cells[row, 3].Value.ToString().Trim())

                            };

                            _db.Rooms.Add(room);
                            recordCount++;
                            lastrecord = $"The last Updated record has the Room Name {room.RoomName}";
                        }
                        catch (Exception ex)
                        {
                            ViewBag.ErrorInfo = $"Error Saving the Rooms in row {row} of the excel";
                            ViewBag.ErrorMessage = ex.Message;
                            return View("ErrorException");
                        }
                    }
                    await _db.SaveChangesAsync();
                    message = $"You have successfully Uploaded {recordCount} records...  and {lastrecord}";
                    ViewBag.Message = message;

                }
                return RedirectToAction("Index", "Rooms", new { message });
            }

            ViewBag.Error = $"File type is Incorrect <br/>";
            return View("Index");
        }

        public ActionResult UploadReserverdRooms()
        {
            return PartialView();
        }


        [HttpPost]
        public async Task<ActionResult> UploadReserverdRooms(HttpPostedFileBase excelfile)
        {
            if (excelfile == null || excelfile.ContentLength == 0)
            {
                ViewBag.Error = "Please Select a excel file <br/>";
                ViewBag.Message = "You must select an excel file before you click Upload button";
                TempData["Title"] = "Error.";

                return View("Index");
            }
            HttpPostedFileBase file = Request.Files["excelfile"];
            if (excelfile.FileName.EndsWith("xls", StringComparison.CurrentCulture) ||
                excelfile.FileName.EndsWith("xlsx", StringComparison.CurrentCulture))
            {
                string lastrecord = "";
                int recordCount = 0;
                string message = "";
                string fileContentType = file.ContentType;
                byte[] fileBytes = new byte[file.ContentLength];
                var data = file.InputStream.Read(fileBytes, 0, Convert.ToInt32(file.ContentLength));

                // Read data from excel file
                using (var package = new ExcelPackage(file.InputStream))
                {
                    ExcelValidation myExcel = new ExcelValidation();
                    var currentSheet = package.Workbook.Worksheets;
                    var workSheet = currentSheet.First();
                    var noOfCol = workSheet.Dimension.End.Column;
                    var noOfRow = workSheet.Dimension.End.Row;
                    int requiredField = 2;

                    string validCheck = myExcel.ValidateExcel(noOfRow, workSheet, requiredField);
                    if (!validCheck.Equals("Success"))
                    {
                        //string row = "";
                        //string column = "";
                        string[] ssizes = validCheck.Split(' ');
                        string[] myArray = new string[2];
                        for (int i = 0; i < ssizes.Length; i++)
                        {
                            myArray[i] = ssizes[i];
                            // myArray[i] = ssizes[];
                        }
                        string lineError = $"Line/Row number {myArray[0]}  and column {myArray[1]} is not rightly formatted, Please Check for anomalies ";
                        //ViewBag.LineError = lineError;
                        ViewBag.Message = lineError;
                        return View("Index");
                    }
                    for (int row = 2; row <= noOfRow; row++)
                    {
                        var roomName = workSheet.Cells[row, 2].Value.ToString().Trim();
                        var hostelName = workSheet.Cells[row, 1].Value.ToString().Trim();

                        var block = await _db.Blocks.AsNoTracking()
                            .Where(x => x.BlockName.Trim().ToUpper().Equals(hostelName.ToUpper()))
                            .FirstOrDefaultAsync();

                        var room = await _db.Rooms.AsNoTracking()
                            .Where(x => x.RoomName.Trim().ToUpper().Equals(roomName.ToUpper()) && x.BlockId.Equals(block.BlockId))
                            .FirstOrDefaultAsync();

                        if (room == null)
                        {

                            ViewBag.ErrorInfo = "Please Leave no column or row Empty/Blank";
                            ViewBag.ErrorMessage = $" The \"{roomName}\" Room Name specified in the excel doesn't exist on the portal. " +
                                                   $"Please add/update the Hostel first before uploading or correct the excel sheet...";
                            return View("ErrorException");
                        }

                        try
                        {
                            var reservedRoom = new ReservedRoom()
                            {
                                RoomId = room.RoomId,
                                ReasonForReserve = workSheet.Cells[row, 3].Value.ToString().Trim()
                            };

                            _db.ReservedRooms.Add(reservedRoom);
                            recordCount++;
                            lastrecord = $"The last Updated record has the Room Name {room.RoomName}";
                        }
                        catch (Exception ex)
                        {
                            ViewBag.ErrorInfo = $"Error Saving the Reserved Room in row {row} of the excel";
                            ViewBag.ErrorMessage = ex.Message;
                            return View("ErrorException");
                        }
                    }
                    await _db.SaveChangesAsync();
                    message = $"You have successfully Uploaded {recordCount} records...  and {lastrecord}";
                    ViewBag.Message = message;

                }
                return RedirectToAction("Index", "Rooms", new { message });
            }

            ViewBag.Error = $"File type is Incorrect <br/>";
            return View("Index");
        }

        public ActionResult GetRoomList(int codeId)
        {
            var item = _db.Rooms.AsNoTracking()
                .Where(x => x.BlockId.Equals(codeId))
                .Select(s => new { s.RoomId, s.RoomName });

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            var result = javaScriptSerializer.Serialize(item);
            return Json(result, JsonRequestBehavior.AllowGet);
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
