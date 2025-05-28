using Newtonsoft.Json;
using OfficeOpenXml;
using Rotativa;
using SwiftKampus.BusinessLogic;
using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampus.ViewModels;
using SwiftKampus.ViewModels.Fee_Management;
using SwiftKampusModel;
using SwiftKampusModel.Accomodation;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace SwiftKampus.Controllers
{
    [Authorize]
    [Audit(AuditingLevel = 2)]
    public class AssignedRoomsController : BaseController
    {
        public AssignedRoomsController(SchoolDbContext db) : base(db)
        {
            _studentQuery = new StudentQueryManager(_db);
        }

        [Authorize(Roles = RoleName.Admin + "," + RoleName.DStudentAffairs + "," + RoleName.StudentAffairs)]
        public ActionResult Index(string message, List<ReserveAccomodationVm> model)
        {
            ViewBag.Message = message;
            ViewBag.Model = model;
            return View();
        }

        public async Task<ActionResult> GetIndex()
        {
            var assignedRoom = await _db.StudentAssignedRooms.Include(a => a.Block).Include(a => a.Hostel)
                                        .Include(a => a.Room).Include(a => a.Session).Include(a => a.Student)
                                        .Where(a => a.SessionId.Equals(29)) //Added for debugging
                                        .AsNoTracking().ToListAsync();

            var data = assignedRoom.Select(s => new
            {
                s.Student.MatricNo,
                s.Student.Email,
                s.StudentAssignedRoomId,
                s.Student.FullName,
                s.Hostel.HostelName,
                s.Room.RoomName,
                s.Block.BlockName,
                s.BedSpace,
                s.Session.SessionName,
                s.PaymentStatus
            }).ToList();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> HostelApplicationIndex()
        {
            var studentId = _studentQuery.GetStudentId(userId);
            var hostelApplicationFeeList = await _db.HostelApplications.Include(i => i.Student).Include(i => i.Session).AsNoTracking()
                                            .Where(x => x.StudentId.Equals(studentId)
                                            && x.IsPayed.Equals(true)).ToListAsync();
            return View(hostelApplicationFeeList);
        }

        public async Task<ActionResult> PrintHostelApplicationReceipt(int id)
        {
            var hostelApplicationFee = await _db.HostelApplications.Include(i => i.Student.Programme.Department.Faculty)
                                            .Include(i => i.Session).AsNoTracking()
                                            .FirstOrDefaultAsync(x => x.HostelApplicationId.Equals(id)
                                            && x.IsPayed.Equals(true));
            //return View();
            return new ViewAsPdf(hostelApplicationFee);
        }


        public async Task<ActionResult> HostelAccomodationFeeIndex()
        {
            var studentId = _studentQuery.GetStudentId(userId);
            var hostelAccomodationFeeList = await _db.StudentAccommodationFeePayments.Include(i => i.Student).Include(i => i.Session).AsNoTracking()
                                            .Where(x => x.StudentId.Equals(studentId)
                                            && x.IsPayed.Equals(true)).ToListAsync();
            return View(hostelAccomodationFeeList);
        }

        public async Task<ActionResult> PrintAccomodationFeeReceipt(int id)
        {
            var hostelApplicationFee = await _db.StudentAccommodationFeePayments.Include(i => i.Student.Programme.Department.Faculty)
                                            .Include(i => i.Session).AsNoTracking()
                                            .FirstOrDefaultAsync(x => x.StudentAssignedRoomId.Equals(id)
                                            && x.IsPayed.Equals(true));
            //return View();
            return new ViewAsPdf(hostelApplicationFee);
        }

        //[Authorize(Roles = RoleName.SuperAdmin + "," + RoleName.Admin)]
        public async Task<ActionResult> MyRoomMate(int? roomId)
        {
            var result = ConfirmSchoolFee();
            if (result != null)
                return result;
            if (roomId == null)
            {
                var id = _studentQuery.GetStudentId(userId);
                roomId = await _db.StudentAssignedRooms.AsNoTracking().Where(x => x.StudentId.Equals(id)
                                    && x.SessionId.Equals(sessionId)).Select(s => s.RoomId)
                                    .FirstOrDefaultAsync(); // where session is now
            }
            if (roomId != null)
            {
                var assignedRooms = _db.StudentAssignedRooms.AsNoTracking().Include(a => a.Block).Include(a => a.Hostel)
                                   .Include(a => a.Room).Include(a => a.Session)
                                   .Include(a => a.Student).Include(a => a.Student.Programme)
                                   .Where(x => x.RoomId.Equals((int)roomId) && x.SessionId.Equals(sessionId));
                if (!assignedRooms.Any())
                {
                    ViewBag.errorMessage = "Apply for Bed Space to Check Student Allocated to Room.";
                }

                return View(await assignedRooms.ToListAsync());
            }

            ViewBag.errorMessage = "Specify the Room you want to find its member";
            return View("ErrorException");
        }

        //[Authorize(Roles = RoleName.SuperAdmin + "," + RoleName.Admin)]
        public async Task<ActionResult> StudentDetails(string id)
        {
            var result = ConfirmSchoolFee();
            if (result != null)
                return result;

            if (id == null)
            {
                id = _studentQuery.GetStudentId(userId);
            }

            StudentAssignedRoom assignedRoom = await _db.StudentAssignedRooms.Include(i => i.Student).Include(i => i.Hostel).Include(i => i.Block)
                                        .Include(i => i.Room).Include(i => i.Session).AsNoTracking()
                                        .Where(x => x.StudentId.Trim().Equals(id.Trim()) && x.SessionId.Equals(sessionId))
                                        .FirstOrDefaultAsync();

            //assignedRoom.PaymentStatus = true;
            //_db.Entry(assignedRoom).State = EntityState.Modified;
            //_db.SaveChanges();

            StudentAccommodationFeePayment isPaidhostelApplicationFee = await _db.StudentAccommodationFeePayments.AsNoTracking()
                                                .Where(x => x.StudentId.Equals(id) && x.IsPayed.Equals(true))
                                                .FirstOrDefaultAsync();
            if (isPaidhostelApplicationFee != null)
            {
                if (isPaidhostelApplicationFee.IsPayed == true) ViewBag.rrr = isPaidhostelApplicationFee.ReferenceNo;
            }

            if (assignedRoom == null)
            {
                ViewBag.errorMessage = "Apply for Bed Space to Check Student Allocated to Room.";
                return RedirectToAction("HostelApplication");
            }
            return View(assignedRoom);
        }

        // GET: AssignedRooms/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            StudentAssignedRoom assignedRoom = await _db.StudentAssignedRooms.FindAsync(id);
            if (assignedRoom == null)
            {
                return HttpNotFound();
            }
            return View(assignedRoom);
        }

        //[Authorize(Roles = RoleName.SuperAdmin + "," + RoleName.Admin)]
        [HttpGet]
        public async Task<ActionResult> HostelApplicationPayment(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                id = _studentQuery.GetStudentId(userId);
            }
            var student = await _db.Students.FindAsync(id);

            //var student = await _db.Students.AsNoTracking().Where(x => x.StudentId.Equals(model.StudentId))
            //                      .FirstOrDefaultAsync();
            var hasTransaction = await _db.HostelApplications.AsNoTracking().Where(x => x.StudentId.Equals(id)
                               && x.SessionId.Equals(sessionId) && x.IsPayed.Equals(false)).FirstOrDefaultAsync();


            if (hasTransaction != null)
            {
                //model.hash = _query.HashRemitaRequest(model.merchantId, model.serviceTypeId, hasTransaction.OrderId, model.amt, model.responseurl, RemitaConfigParams.APIKEY);
                //return RedirectToAction("ConfrimHostelApplicationPayment", new { orderID = hasTransaction.OrderId });

                var hashed = _query.HashRemitedValidate(hasTransaction.OrderId, RemitaConfigParams.APIKEY, RemitaConfigParams.MERCHANTID);
                string checkurl = RemitaConfigParams.CHECKSTATUSURL + "/" + RemitaConfigParams.MERCHANTID + "/" + hasTransaction.OrderId + "/" + hashed + "/" + "orderstatus.reg";
                string jsondata = new WebClient().DownloadString(checkurl);
                var result = JsonConvert.DeserializeObject<RemitaResponse>(jsondata);
                if (string.IsNullOrEmpty(result.Rrr))
                {
                    var entry = _db.Entry(hasTransaction);
                    if (entry.State == EntityState.Detached)
                        _db.HostelApplications.Attach(hasTransaction);
                    _db.HostelApplications.Remove(hasTransaction);
                    await _db.SaveChangesAsync();
                }
                return RedirectToAction("ConfrimHostelApplicationPayment", new { orderID = hasTransaction.OrderId });
            }
            else
            {
                var session = _query.GetCurrentSessionList(studentSchoolProgrammeId);
                ViewBag.SessionId = new SelectList(session, "SessionId", "SessionName");
                long milliseconds = DateTime.Now.Ticks;
                var url = Url.Action("ConfrimHostelApplicationPayment", "AssignedRooms", new { },
                                        protocol: Request.Url.Scheme);

                ViewBag.ExpectedAmount = 1500;
                var hostelApplication = new HostelApplicationVm
                {
                    payerName = student?.FullName,
                    payerEmail = student?.Email ?? $"{student?.MatricNo}@unijos.edu.ng",
                    payerPhone = student?.PhoneNumber ?? "07030000000",
                    amt = "1500",
                    merchantId = RemitaConfigParams.MERCHANTID,
                    orderId = $"UJHA{milliseconds.ToString()}",
                    responseurl = url,
                    StudentId = id,
                    serviceTypeId = RemitaConfigParams.HOSTELAPPLICATIONSERVICETYPE,
                    ExpectedAmount = 1500,
                    SessionId = session.Select(s => s.SessionId).FirstOrDefault()
                };
                return View(hostelApplication);
            }

        }
        //[Authorize(Roles = RoleName.SuperAdmin + "," + RoleName.Admin)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> HostelApplicationPayment(HostelApplicationVm model)
        {
            if (ModelState.IsValid)
            {
                var hasTransaction = await _db.HostelApplications.AsNoTracking().Where(x => x.StudentId.Equals(model.StudentId)
                                && x.SessionId.Equals(model.SessionId) && x.IsPayed.Equals(false)).FirstOrDefaultAsync();
                var student = await _db.Students.AsNoTracking().Where(x => x.StudentId.Equals(model.StudentId))
                                        .FirstOrDefaultAsync();
                model.paymenttype = model.RemitaPaymentType.ToString().Replace("_", " ").ToLower();
                var hostelApplication = new HostelApplication
                {
                    OrderId = model.orderId,
                    PaymentDateTime = DateTime.Now,
                    SessionId = model.SessionId,
                    StudentId = model.StudentId,
                    ExpectedAmount = 1500,
                    AmountPayed = Convert.ToDouble(model.amt),
                    //ReferenceNo = reference
                };
                _db.HostelApplications.Add(hostelApplication);
                var log = new RemitaPaymentLog
                {
                    OrderId = model.orderId,
                    PaymentName = "Hostel Application Fee",
                    PaymentDate = DateTime.Now,
                    Amount = "1500.00",
                    PayerName = student.FullName
                };
                _db.RemitaPaymentLogs.Add(log);
                await _db.SaveChangesAsync();
                model.hash = _query.HashRemitaRequest(model.merchantId, model.serviceTypeId, model.orderId, model.amt, model.responseurl, RemitaConfigParams.APIKEY);
                return RedirectToAction("SubmitRemita", model);
            }
            return View(model);
        }

        public ActionResult SubmitRemita(HostelApplicationVm model)
        {
            return View(model);
        }

        #region Paystack

        //public ActionResult HostelApplicationPayment(HostelApplicationVm model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var url = Url.Action("ConfrimHostelApplicationPayment", "AssignedRooms", new { },
        //                            protocol: Request.Url.Scheme);
        //        var student = _db.Students.AsNoTracking().FirstOrDefault(x => x.StudentId.Equals(model.StudentId));
        //        var testOrLiveSecret = ConfigurationManager.AppSettings["PayStackSecret"];
        //        var api = new PayStackApi(testOrLiveSecret);
        //        // Initializing a transaction
        //        //var response = api.Transactions.Initialize("user@somewhere.net", 5000000);
        //        if (student != null)
        //        {
        //            var transactionInitializaRequest = new TransactionInitializeRequest
        //            {
        //                //Reference = "SwifKampus",
        //                AmountInKobo = ConvertToKobo((int)model.ExpectedAmount),
        //                CallbackUrl = url,
        //                Email = student.Email,
        //                Bearer = "Hostel Application",

        // CustomFields = new List<CustomField> { new CustomField("studentid","studentid",
        // model.StudentId), new CustomField("sessionid", "sessionid", model.SessionId.ToString()),
        // //new CustomField("paymentmode","paymentmode", schoolFeePayment.p) }

        // }; var response = api.Transactions.Initialize(transactionInitializaRequest);

        // if (response.Status) { //redirect to authorization url return
        // RedirectPermanent(response.Data.AuthorizationUrl); // return Content("Successful"); } }
        // return Content("An error occurred");

        //    }
        //    return View(model);
        //}

        #endregion Paystack

        [AllowAnonymous]
        public async Task<ActionResult> ConfrimHostelApplicationPayment(string RRR, string orderID)
        {
            HostelApplication hostelApplication;
            RemitaResponse result = new RemitaResponse();

            if (string.IsNullOrEmpty(orderID))
            {
                hostelApplication = await _db.HostelApplications.AsNoTracking()
                    .Where(x => x.ReferenceNo.Equals(RRR.Trim()))
                    .FirstOrDefaultAsync();
            }
            else
            {
                hostelApplication = await _db.HostelApplications.AsNoTracking()
                    .Where(x => x.OrderId.Equals(orderID.Trim()))
                    .FirstOrDefaultAsync();
            }
            if (hostelApplication != null)
            {
                if (hostelApplication.IsPayed.Equals(true))
                {
                    result.Message = hostelApplication.TransactionMessage;
                    result.OrderId = hostelApplication.OrderId;
                    result.Rrr = hostelApplication.ReferenceNo;
                    result.Status = hostelApplication.IsPayed.ToString();
                    return RedirectToAction("ConfrimRrrPayment", "RemitaPaymentLogs", result);
                }
                var log = await _db.RemitaPaymentLogs.AsNoTracking().Where(x => x.OrderId.Equals(hostelApplication.OrderId))
                    .FirstOrDefaultAsync();
                var hashed = _query.HashRemitedValidate(orderID, RemitaConfigParams.APIKEY, RemitaConfigParams.MERCHANTID);
                string url = RemitaConfigParams.CHECKSTATUSURL + "/" + RemitaConfigParams.MERCHANTID + "/" + orderID + "/" + hashed + "/" + "orderstatus.reg";
                string jsondata = new WebClient().DownloadString(url);
                result = JsonConvert.DeserializeObject<RemitaResponse>(jsondata);

                if (result.Status.Equals("00") || result.Status.Equals("01"))
                {
                    hostelApplication.ReferenceNo = result.Rrr;
                    hostelApplication.IsPayed = true;
                    hostelApplication.TransactionMessage = result.Message;
                    _db.Entry(hostelApplication).State = EntityState.Modified;

                    _query.UpdateTransactionLog(log, result);
                    await _db.SaveChangesAsync();
                }
                else
                {
                    hostelApplication.ReferenceNo = result.Rrr;
                    hostelApplication.IsPayed = false;
                    hostelApplication.TransactionMessage = result.Message;
                    _db.Entry(hostelApplication).State = EntityState.Modified;

                    _query.UpdateTransactionLog(log, result);
                    await _db.SaveChangesAsync();
                    return RedirectToAction("RetryHostelApplication", new { rrr = result.Rrr });
                }
                return RedirectToAction("ConfrimRrrPayment", "RemitaPaymentLogs", result);
            }
            var message = $"There is no payment that has either the RRR {RRR} or" +
                          $" Order Id {orderID} for Hostel Application Payment";
            return RedirectToAction("GetPaymentStatus", "RemitaServices", new { message });
        }

        [AllowAnonymous]
        public ActionResult RetryHostelApplication(string rrr)
        {
            try
            {
                var hashrrr = _query.HashRrrQuery(rrr, RemitaConfigParams.APIKEY, RemitaConfigParams.MERCHANTID);
                string posturl = RemitaConfigParams.CHECKSTATUSURL + "/" + RemitaConfigParams.MERCHANTID + "/" + rrr + "/" + hashrrr + "/" + "status.reg";
                string jsondata = new WebClient().DownloadString(posturl);
                var result = JsonConvert.DeserializeObject<RemitaResponse>(jsondata);
                if (result.Status.Equals("00") || result.Status.Equals("01"))
                {
                    return RedirectToAction("ConfrimHostelApplicationPayment", "AssignedRooms", new { RRR = result.Rrr, orderID = result.OrderId });
                }
                var hash = _query.HashRemitedRePost(RemitaConfigParams.MERCHANTID, rrr, RemitaConfigParams.APIKEY);
                var url = Url.Action("ConfrimHostelApplicationPayment", "AssignedRooms", new { },
                                       protocol: Request.Url.Scheme);
                var model = new RemitaRePostVm
                {
                    rrr = rrr,
                    merchantId = RemitaConfigParams.MERCHANTID,
                    hash = hash,
                    responseurl = url
                };
                return View(model);
            }
            catch (Exception)
            {
                return RedirectToAction("GetPaymentStatus", "RemitaServices", new { message = "Please check the RRR supplied and try again" });
            }

        }

        private int ConvertToKobo(int value)
        {
            return value * 100;
        }

        private int ConvertToNaira(int value)
        {
            return value / 100;
        }

        // This class creates a filter that sends out a message if the student tries accessing
        // the HostelApplication action when the set time hasn't reached
        public class HostelFilterAttribute : ActionFilterAttribute
        {
            public override void OnActionExecuting(ActionExecutingContext filterContext)
            {
                // Get the current time
                var currentTime = DateTime.Now;

                // Check if the current time is between 8 PM and 7 AM
                if (!(currentTime.Hour >= 20 || currentTime.Hour < 7))
                {
                    // JavaScript code to execute if the time is within the specified range
                    string javascriptCode = "<script>alert('Hostel Booking is only available between 8pm to 7am');</script>";
                    filterContext.Result = new ContentResult
                    {
                        Content = javascriptCode,
                        ContentType = "text/html" // Set the content type to HTML
                    };
                }
                //else
                //{
                //    // Code to execute if the time is outside the specified range
                //    string content = "Time is " + DateTime.Now;
                //    filterContext.Result = new ContentResult
                //    {
                //        Content = content,
                //        ContentType = "text/plain" // You can set the appropriate content type
                //    };
                //}
            }
        }

        [HostelFilter]
        //[Authorize(Roles = RoleName.SuperAdmin + "," + RoleName.Admin)]
        // GET: AssignedRooms/HostelApplication
        public async Task<ActionResult> HostelApplication(string id)
        {
            var result = ConfirmSchoolFee();
            if (result != null)
                return result;

            await DeAllocateRoom();
            if (string.IsNullOrEmpty(id))
            {
                id = _studentQuery.GetStudentId(userId);
            }
            string studentid = id;


            var alreadyBooked = await _db.StudentAssignedRooms.Where(x => x.StudentId.Equals(studentid)
                                        && x.SessionId.Equals(sessionId)).FirstOrDefaultAsync();

            var Booked = await _db.StudentAssignedRooms.Where(x => x.PaymentStatus.Equals(true)
                                        && x.SessionId.Equals(sessionId)).ToListAsync();

            if (alreadyBooked != null)
            {
                return RedirectToAction("StudentDetails");
            }



            var isHostelAvailableNow = await _db.SessionAccomodations.Include(i => i.Room).AsNoTracking().Where(i => i.SessionId.Equals(sessionId)).ToListAsync();
            var reservedRoom = await _db.ReservedRooms.Include(i => i.Room).AsNoTracking().ToListAsync();
            int isHostelAvailable = 0;
            if (isHostelAvailableNow != null)
            {
                isHostelAvailable = isHostelAvailableNow.Sum(x => x.Room.RoomCapacity);
            }
            var assignedRooms = _db.StudentAssignedRooms.Include(x => x.Session).Where(x => x.SessionId.Equals(sessionId)).AsNoTracking().Count();
            var totalSpace = reservedRoom.Sum(x => x.Room.RoomCapacity) + assignedRooms;
            //if (totalSpace < isHostelAvailable)
            if (true)
            {
                var isPayForApplication = await _db.HostelApplications.AsNoTracking()
                                    .Where(x => x.StudentId.Equals(studentid) && x.SessionId.Equals(sessionId))
                                    .FirstOrDefaultAsync();
                if (isPayForApplication == null || isPayForApplication.IsPayed)
                {
                    var student = await _db.Students.Include(i => i.Programme).AsNoTracking().Where(x => x.StudentId.Equals(studentid))
                        .Select(s => new
                        {
                            gender = s.Gender,
                            faculty = s.Programme.Department.FacultyId
                        }).FirstOrDefaultAsync();
                    var myHostelId = await _db.AssignedHostels.Where(x => x.FacultyId.Equals(student.faculty))
                                                .Select(x => x.HostelId).ToListAsync();

                    var hostelList = new List<Hostel>();
                    foreach (var hostel in myHostelId)
                    {
                        hostelList.Add(await _db.SessionAccomodations.Include(i => i.Hostel).AsNoTracking()
                                    .Where(x => x.Hostel.HostelId.Equals(hostel) && x.Session.SessionId.Equals(sessionId))
                                    .Select(s => s.Hostel).FirstOrDefaultAsync());
                    }

                    if (hostelList.Any())
                    {
                        ViewBag.AvailableHostel = new SelectList(hostelList, "HostelId", "HostelName");
                        return View();
                    }
                    return View("HostelNotFound");
                }
                return RedirectToAction("HostelApplicationPayment");
            }

            return View("HostelNotFound");
        }

        public async Task<ActionResult> HostelNotFound()
        {
            //await DeAllocateRoom();
            return View();
        }

        //[Authorize(Roles = RoleName.SuperAdmin + "," + RoleName.Admin)]
        [HostelFilter]
        [HttpPost]
        public async Task<ActionResult> HostelApplication(HostelChoiceVm model)
        {
            string studentId = _studentQuery.GetStudentId(userId);

            var student = await _db.Students.Include(i => i.Level).Include(i => i.Programme.Department)
                            .Include(i => i.Programme.FinalLevel)
                            .AsNoTracking().Where(x => x.StudentId.Equals(studentId))
                            .Select(s => new
                            {
                                gender = s.Gender,
                                faculty = s.Programme.Department.FacultyId,
                                levelName = s.Level.LevelName,
                                finalLevel = s.Programme.FinalLevel.LevelName,
                                sessionId = s.SessionId,
                                schoolProgrammeId = s.SchoolProgrammeId
                            }).FirstOrDefaultAsync();
            var sessionId = _query.GetCurrentSessionId(student.schoolProgrammeId);

            var myHostelId = await _db.AssignedHostels.Where(x => x.FacultyId.Equals(student.faculty))
                                    .Select(x => x.HostelId).ToListAsync();

            var hostelList = new List<Hostel>();
            foreach (var hostel in myHostelId)
            {
                hostelList.Add(await _db.SessionAccomodations.Include(i => i.Hostel).AsNoTracking()
                    .Where(x => x.Hostel.HostelId.Equals(hostel) && x.Session.SessionId.Equals(sessionId))
                    .Select(s => s.Hostel).FirstOrDefaultAsync());
            }

            // Query to check if student has been assigned a room
            var checkAcommodation = await _db.StudentAssignedRooms.AsNoTracking()
                                    .Where(x => x.StudentId.Equals(studentId)
                                    && x.SessionId.Equals(sessionId)).ToListAsync();
            // Checking if the query contains any result
            if (checkAcommodation.Any())
            {
                //await DeAllocateRoom();
                TempData["UserMessage"] = "You have been Allocated Bed Space Already.";
                TempData["Title"] = "Error.";

                ViewBag.AvailableHostel = new SelectList(hostelList, "HostelId", "HostelName");
                return RedirectToAction("StudentDetails");
            }

            int hostelId = Convert.ToInt32(model.AvailableHostel);

            //
            var hostelName = await _db.SessionAccomodations.AsNoTracking().Where(x => x.HostelId.ToString().Equals(hostelId.ToString()))
                                .Select(s => s.Hostel.HostelName).FirstOrDefaultAsync();

            // Including the block and Room in the search query for available accommodation
            var blocks = await _db.SessionAccomodations.Include(i => i.Room).Include(i => i.Block)
                                .Include(i => i.Hostel).AsNoTracking()
                                .Where(x => x.HostelId.ToString().Equals(hostelId.ToString()) && x.SessionId.Equals(sessionId))
                                .ToListAsync();

            // Iterating through the blocks in the hostel
            //foreach (var block in blocks.Where(x => x.Block.Gender.Equals(student.gender)).Select(s => s.Block).ToList())
            //{
            //var rooms = block.Rooms.Where(x => x.BlockId.Equals(block.BlockId)).ToList();

            // Iterating through each room in the block
            // Define a SemaphoreSlim with a maximum count of 1 to ensure exclusive access to the critical section.
            var semaphore = new SemaphoreSlim(1, 1);
            string errorMessage = ""; // Message to display to the user

            // Enter the critical section, ensuring exclusive access to this block of code.
            await semaphore.WaitAsync();
            using (var transaction = _db.Database.BeginTransaction()) //using database transactions to further prevent race conditions
            {
                try
                {
                    foreach (var room in blocks.Where(x => x.Block.Gender.Equals(student.gender)))
                    {
                        var reservedRoom = await _db.ReservedRooms.AsNoTracking().Where(x => x.RoomId.Equals(room.RoomId)).FirstOrDefaultAsync();
                        if (reservedRoom == null)
                        {
                            var checkRoom = await _db.StudentAssignedRooms.Include(s => s.Session).AsNoTracking().Where(x => x.RoomId.Equals(room.RoomId) && x.SessionId.Equals(sessionId)).CountAsync();

                            if (checkRoom < room.Room.RoomCapacity)
                            {
                                int spaceLeft = room.Room.RoomCapacity - checkRoom;
                                for (int i = 0; i < spaceLeft; i++)
                                {
                                    var bedSpace = ConvertToAlphabet(checkRoom);
                                    var isSpaceAvailable = await _db.StudentAssignedRooms.Include(x => x.Session).AsNoTracking().Where(x => x.RoomId.Equals(room.RoomId)
                                                                && x.BedSpace.Equals(bedSpace) && x.SessionId.Equals(sessionId)).FirstOrDefaultAsync();
                                    if (isSpaceAvailable == null)
                                    {
                                        if (student.sessionId == sessionId || student.levelName.Equals(student.finalLevel))
                                        {
                                            if (await TryAllocateRoom(studentId, room.Room, room.Block, hostelId, checkRoom))
                                            {
                                                transaction.Commit();
                                                return RedirectToAction("StudentDetails");
                                            }
                                        }
                                        else if (student.levelName.Equals("300") || student.levelName.Equals("400")
                                                 || student.levelName.Equals("500") || student.levelName.Equals("600"))
                                        {
                                            await SaveAssignedRoom(studentId, room.Room, room.Block, hostelId, checkRoom);
                                            transaction.Commit();
                                            return RedirectToAction("StudentDetails");
                                        }
                                        if (student.levelName.Equals("100") || student.levelName.Equals("200"))
                                        {
                                            await SaveAssignedRoom(studentId, room.Room, room.Block, hostelId, checkRoom);
                                            transaction.Commit();
                                            return RedirectToAction("StudentDetails");
                                        }
                                    }
                                    checkRoom += 1;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Handle the exception here, log it, or take any necessary recovery action.
                    errorMessage = $"An error occurred: {ex.Message}";
                    transaction.Rollback();
                }
                finally
                {
                    // Release the semaphore to allow other threads to enter the critical section.
                    semaphore.Release();
                }
            }

            ViewBag.Error = errorMessage;
            ViewBag.Message = "Please Try another Hostel because the selected hostel has no more space";
            ViewBag.AvailableHostel = new SelectList(hostelList, "HostelId", "HostelName");
            return View();
        }

        private async Task<bool> TryAllocateRoom(string studentId, Room room, Block block, int hostelId,
            int checkRoom)
        {
            var hostelName = await _db.Blocks.AsNoTracking().Include(i => i.Hostel)
                                .Where(x => x.HostelId.ToString().Equals(hostelId.ToString()))
                                    .Select(s => s.Hostel.HostelName).FirstOrDefaultAsync();
            var assignedRoom = new StudentAssignedRoom
            {
                StudentId = studentId,
                RoomId = room.RoomId,
                BlockId = block.BlockId,
                HostelId = hostelId,
                BedSpace = ConvertToAlphabet(checkRoom),
                SessionId = sessionId,
                AssignedDate = DateTime.Now,
                ReleaseDate = DateTime.Now.AddDays(1)
            };
            _db.StudentAssignedRooms.Add(assignedRoom);

            string body = $"Congratulations!!! You have been assigned {room.RoomId}" +
                          $" at block {block.BlockName} in {hostelName}. " +
                          $"Your room eligibility will be withdraw after {DateTime.Now.AddDays(2)} if you fail " +
                          $" to make payment.";
            //await SendSmsMessage(studentId, body);
            try
            {
                await _db.SaveChangesAsync();
                return true;

            }
            catch (Exception)
            {
                return false;
                throw;
            }
            //await DeAllocateRoom();
        }

        //[Authorize(Roles = RoleName.SuperAdmin + "," + RoleName.Admin)]
        private async Task SaveAssignedRoom(string studentId, Room room, Block block, int hostelId,
            int checkRoom)
        {
            var hostelName = await _db.Blocks.AsNoTracking().Include(i => i.Hostel)
                                .Where(x => x.HostelId.ToString().Equals(hostelId.ToString()))
                                    .Select(s => s.Hostel.HostelName).FirstOrDefaultAsync();
            var assignedRoom = new StudentAssignedRoom
            {
                StudentId = studentId,
                RoomId = room.RoomId,
                BlockId = block.BlockId,
                HostelId = hostelId,
                BedSpace = ConvertToAlphabet(checkRoom),
                SessionId = sessionId,
                AssignedDate = DateTime.Now,
                ReleaseDate = DateTime.Now.AddDays(1)
            };
            _db.StudentAssignedRooms.Add(assignedRoom);

            string body = $"Congratulations!!! You have been assigned {room.RoomId}" +
                          $" at block {block.BlockName} in {hostelName}. " +
                          $"Your room eligibility will be withdraw after {DateTime.Now.AddDays(2)} if you fail " +
                          $" to make payment.";
            //await SendSmsMessage(studentId, body);
            await _db.SaveChangesAsync();
            //await DeAllocateRoom();
        }

        //[Authorize(Roles = RoleName.SuperAdmin + "," + RoleName.Admin)]
        private async Task SaveAssignedRoom(Student student, Room room, Block block, int hostelId,
            char alphbet, int sSessionId)
        {
            var hostelName = await _db.Blocks.AsNoTracking().Include(i => i.Hostel)
                                .Where(x => x.HostelId.ToString().Equals(hostelId.ToString()))
                                    .Select(s => s.Hostel.HostelName).FirstOrDefaultAsync();

            var assignedRoom = new StudentAssignedRoom
            {
                StudentId = student.StudentId,
                RoomId = room.RoomId,
                BlockId = block.BlockId,
                HostelId = hostelId,
                BedSpace = alphbet.ToString(),
                SessionId = sSessionId,
                AssignedDate = DateTime.Now,
                ReleaseDate = DateTime.Now.AddDays(1)
            };
            _db.StudentAssignedRooms.Add(assignedRoom);

            string body = $"Congratulations, You have been assigned Room {room.RoomName}" +
                          $" at block {block.BlockName} in {hostelName} Hostel. " +
                          $"The room will be revoked on {DateTime.Now.AddDays(2)} if you fail " +
                          $" to make payment.";
            //await SendSmsMessage(studentId, body);

            if (await _db.SaveChangesAsync() > 0)
            {
                await SMSClass.SendSMS("UNIJOS", body, student.PhoneNumber);
            }
            //await DeAllocateRoom();
        }

        private async Task<StudentAssignedRoom> SaveReAssignedRoom(Student student, Room room, Block block, int hostelId,
            char alphbet, int sSessionId)
        {
            var hostelName = await _db.Blocks.AsNoTracking().Include(i => i.Hostel)
                                .Where(x => x.HostelId.ToString().Equals(hostelId.ToString()))
                                    .Select(s => s.Hostel.HostelName).FirstOrDefaultAsync();

            var assignedRoom = new StudentAssignedRoom
            {
                StudentId = student.StudentId,
                RoomId = room.RoomId,
                BlockId = block.BlockId,
                HostelId = hostelId,
                BedSpace = alphbet.ToString(),
                SessionId = sSessionId,
                AssignedDate = DateTime.Now,
                ReleaseDate = DateTime.Now.AddDays(1)
            };
            _db.StudentAssignedRooms.Add(assignedRoom);

            string body = $"Congratulations, You have been Reassigned Room {room.RoomName}" +
                          $" at block {block.BlockName} in {hostelName} Hostel. " +
                          $"The room will be revoked on {DateTime.Now.AddDays(2)} if you fail " +
                          $" to make payment.";
            //await SendSmsMessage(studentId, body);

            if (await _db.SaveChangesAsync() > 0)
            {
                await SMSClass.SendSMS("UNIJOS", body, student.PhoneNumber);
                return assignedRoom;
            }
            return assignedRoom;
            //await DeAllocateRoom();
        }

        //[Authorize(Roles = RoleName.SuperAdmin + "," + RoleName.Admin)]
        public async Task<ActionResult> Allocate()
        {

            var stduent = _studentQuery.GetStudent("2019ds0003@unijos.edu.ng");
            var assinedRoom = _db.StudentAssignedRooms.Where(x => x.StudentId.Equals(stduent.StudentId)).ToList();
            foreach (var aroom in assinedRoom)
            {
                _db.StudentAssignedRooms.Remove(aroom);
                _db.SaveChanges();
            }
            int cSessionId = _query.GetCurrentSessionId(stduent.SchoolProgrammeId);
            var room = _db.Rooms.Include(i => i.Buildings.Hostel).FirstOrDefault(x => x.RoomId.Equals(1632));
            await SaveAssignedRoom(stduent, room, room.Buildings, room.Buildings.HostelId, 'A', cSessionId);
            return View();
        }


        public async Task<ActionResult> ReAllocate(string Email, string BedspaceAllocate)
        {
            var stduent = _studentQuery.GetStudent(Email);
            //var getAssignedRoom = await _db.AssignedRooms.Where(x => x.StudentId.Equals(stduent.StudentId)).ToListAsync();
            //var getAssignedRoom = await _db.AssignedRooms.Where(x => x.BedSpace.Equals("No valid Room")).ToListAsync();

            var assinedRoom = await _db.StudentAssignedRooms.Where(x => x.StudentId.Equals(stduent.StudentId)).FirstOrDefaultAsync();
            var room = await _db.Rooms.Include(i => i.Buildings).FirstOrDefaultAsync(x => x.RoomId.Equals(assinedRoom.RoomId));
            if (assinedRoom != null)
            {
                assinedRoom.RoomId = room.RoomId;
                assinedRoom.BlockId = room.BlockId;
                assinedRoom.HostelId = room.Buildings.HostelId;
                assinedRoom.BedSpace = BedspaceAllocate;
                _db.Entry(assinedRoom).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                ViewBag.Message = "Changed Successfully";
            }
            return View();
        }

        public async Task<ActionResult> ReAllocateBedSpaceToStudent()
        {

            //var studentId = _studentQuery.GetStudentId();
            var getAssignedRoom = await _db.StudentAssignedRooms.Where(x => x.BedSpace.Equals("No valid Room")).ToListAsync();

            foreach (var item in getAssignedRoom)
            {
                var room = await _db.Rooms.Include(i => i.Buildings).FirstOrDefaultAsync(x => x.RoomId.Equals(item.RoomId));
                var assinedRoom = await _db.StudentAssignedRooms.Where(x => x.StudentId.Equals(item.StudentId)).FirstOrDefaultAsync();

                if (assinedRoom != null)
                {
                    assinedRoom.RoomId = room.RoomId;
                    assinedRoom.BlockId = room.BlockId;
                    assinedRoom.HostelId = room.Buildings.HostelId;
                    assinedRoom.BedSpace = "A";
                    _db.Entry(assinedRoom).State = EntityState.Modified;
                    await _db.SaveChangesAsync();
                    ViewBag.Message = "Changed Successfully";
                }
            }

            return View();
        }

        private string ConvertToAlphabet(int hostelValue)
        {
            string alphabet = string.Empty;
            switch (hostelValue)
            {
                case 0:
                    return "A";

                case 1:
                    return "B";

                case 2:
                    return "C";

                case 3:
                    return "D";

                case 4:
                    return "E";

                case 5:
                    return "F";

                case 6:
                    return "G";

                case 7:
                    return "H";

                case 8:
                    return "I";

                case 9:
                    return "J";

                default:
                    return "No valid Room";
            }
        }

        private async Task DeAllocateRoom()
        {
            var unPayedAllocation = await _db.StudentAssignedRooms.AsNoTracking().Where(x => x.PaymentStatus.Equals(false)
                                            && x.SessionId.Equals(sessionId)).ToListAsync();

            if (unPayedAllocation.Any())
            {
                foreach (var item in unPayedAllocation)
                {
                    var studentNo = item.StudentId;
                    //int dateCompare1 = DateTime.Compare(DateTime.Now.Date, item.ReleaseDate);

                    //int dateCompare1 = DateTime.Now.Hour - item.ReleaseDate.Hour;
                    //DateTime.Now.Hour >= item.ReleaseDate.Hour
                    //DateTime release = item.ReleaseDate.AddHours(-16); //deallocate room 8hrs after assigning if payment not made
                    if (DateTime.Now > item.ReleaseDate)
                    {
                        var asignHostel = _db.StudentAssignedRooms.FirstOrDefault(x => x.StudentAssignedRoomId.Equals(item.StudentAssignedRoomId));

                        //AssignedRoom assignedRoom = await _db.AssignedRooms.Include(i => i.Hostel).Include(i => i.Block)
                        //                            .Include(i => i.Room).AsNoTracking()
                        //                            .Where(x => x.AssignedRoomId.Equals(item.AssignedRoomId)).FirstOrDefaultAsync();

                        //var delHostel = await _db.Hostels.AsNoTracking().Where(x => x.HostelId.Equals(assignedRoom.Hostel.HostelId))
                        //                    .Select(s => s.HostelName).FirstOrDefaultAsync();
                        //var delBlock = await _db.Blocks.AsNoTracking().Where(x => x.BlockId.Equals(assignedRoom.Block.BlockId))
                        //    .Select(s => s.BlockName).FirstOrDefaultAsync();
                        //var delRoom = await _db.Rooms.AsNoTracking().Where(x => x.RoomId.Equals(assignedRoom.Room.RoomId))
                        //    .Select(s => s.RoomName).FirstOrDefaultAsync();

                        //string body = $"Please be informed that Room {delRoom}" +
                        //              $" at block {delBlock} in {delHostel} Hostel has been Deallocated from {item.StudentId}" +
                        //            $" for non-payment. Thanks";
                        //await SendSmsMessage(studentNo, body);

                        //AssignedRoom assign = await _db.AssignedRooms.FindAsync(item.AssignedRoomId);
                        StudentAccommodationFeePayment assignPayment = await _db.StudentAccommodationFeePayments.FindAsync(item.StudentAssignedRoomId);
                        if (assignPayment != null)
                        {
                            var log = await _db.RemitaPaymentLogs.AsNoTracking().Where(x => x.OrderId.Equals(assignPayment.OrderId))
                                                .FirstOrDefaultAsync();

                            var hashed = _query.HashRemitedValidate(assignPayment.OrderId, RemitaConfigParams.APIKEY, RemitaConfigParams.MERCHANTID);
                            string url = RemitaConfigParams.CHECKSTATUSURL + "/" + RemitaConfigParams.MERCHANTID + "/" + assignPayment.OrderId + "/" + hashed + "/" + "orderstatus.reg";
                            string jsondata = new WebClient().DownloadString(url);
                            var result = JsonConvert.DeserializeObject<RemitaResponse>(jsondata);

                            if (result.Status.Equals("00") || result.Status.Equals("01"))
                            {
                                assignPayment.IsPayed = true;
                                asignHostel.PaymentStatus = true;

                                assignPayment.TransactionMessage = result.Message;
                                assignPayment.ReferenceNo = result.Rrr;
                                assignPayment.PaymentDateTime = DateTime.Now;

                                _db.Entry(assignPayment).State = EntityState.Modified;
                                _db.Entry(asignHostel).State = EntityState.Modified;

                                _query.UpdateTransactionLog(log, result);
                                _db.SaveChanges();
                            }
                            //else
                            //{
                            //    _db.AccommodationFeePayments.Remove(assignPayment);
                            //    // _db.AssignedRooms.Remove(asignHostel);
                            //    _db.SaveChanges();
                            //}

                            else
                            {
                                // Not to delete special allocation
                                if (asignHostel != null && asignHostel.IsSpecialAllocation != true)
                                {
                                    _db.StudentAssignedRooms.Remove(asignHostel);
                                    _db.SaveChanges();
                                }
                            }
                        }
                        else
                        {
                            // Not to delete special allocation
                            if (asignHostel != null && asignHostel.IsSpecialAllocation != true)
                            {
                                _db.StudentAssignedRooms.Remove(asignHostel);
                                _db.SaveChanges();
                            }
                        }

                    }
                }
            }
        }

        private async Task SendSmsMessage(string studentId, string body)
        {
            var message = new SmsToStudent()
            {
                Destination = studentId,
                Body = body
            };
            CustomSms cs = new CustomSms();
            await cs.SendStudentMsgAsync(message);
        }

        #region Create and Edit of Assigned Room not used

        // GET: AssignedRooms/Create
        //public ActionResult Create()
        //{
        //    ViewBag.BlockId = new SelectList(_db.Blocks.AsNoTracking(), "BlockId", "BlockName");
        //    ViewBag.HostelId = new SelectList(_db.Hostels, "HostelId", "HostelCode");
        //    ViewBag.RoomId = new SelectList(_db.Rooms.AsNoTracking(), "RoomId", "RoomName");
        //    ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
        //    ViewBag.StudentId = new SelectList(_db.Students.AsNoTracking().AsNoTracking(), "StudentId", "FirstName");
        //    return View();
        //}

        //// POST: AssignedRooms/Create
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for
        //// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Create([Bind(Include = "AssignedRoomId,StudentId,SessionId,HostelId,BlockId,RoomId")] AssignedRoom assignedRoom)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _db.AssignedRooms.Add(assignedRoom);
        //        await _db.SaveChangesAsync();
        //        return RedirectToAction("Index");
        //    }

        //    ViewBag.BlockId = new SelectList(_db.Blocks.AsNoTracking(), "BlockId", "BlockName");
        //    ViewBag.HostelId = new SelectList(_db.Hostels, "HostelId", "HostelCode");
        //    ViewBag.RoomId = new SelectList(_db.Rooms.AsNoTracking(), "RoomId", "RoomName");
        //    ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
        //    ViewBag.StudentId = new SelectList(_db.Students.AsNoTracking().AsNoTracking(), "StudentId", "FirstName");
        //    return View(assignedRoom);
        //}

        // GET: AssignedRooms/Edit/5
        //public async Task<ActionResult> Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    AssignedRoom assignedRoom = await _db.AssignedRooms.FindAsync(id);
        //    if (assignedRoom == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    ViewBag.BlockId = new SelectList(_db.Blocks, "BlockId", "BlockName", assignedRoom.BlockId);
        //    ViewBag.HostelId = new SelectList(_db.Hostels, "HostelId", "HostelCode", assignedRoom.HostelId);
        //    ViewBag.RoomId = new SelectList(_db.Rooms, "RoomId", "RoomName", assignedRoom.RoomId);
        //    ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName", assignedRoom.SessionId);
        //    ViewBag.StudentId = new SelectList(_db.Students, "StudentId", "FirstName", assignedRoom.StudentId);
        //    return View(assignedRoom);
        //}

        //// POST: AssignedRooms/Edit/5
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for
        //// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Edit([Bind(Include = "AssignedRoomId,StudentId,SessionId,HostelId,BlockId,RoomId")] AssignedRoom assignedRoom)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _db.Entry(assignedRoom).State = EntityState.Modified;
        //        await _db.SaveChangesAsync();
        //        return RedirectToAction("Index");
        //    }
        //    ViewBag.BlockId = new SelectList(_db.Blocks, "BlockId", "BlockName", assignedRoom.BlockId);
        //    ViewBag.HostelId = new SelectList(_db.Hostels, "HostelId", "HostelCode", assignedRoom.HostelId);
        //    ViewBag.RoomId = new SelectList(_db.Rooms, "RoomId", "RoomName", assignedRoom.RoomId);
        //    ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName", assignedRoom.SessionId);
        //    ViewBag.StudentId = new SelectList(_db.Students, "StudentId", "FirstName", assignedRoom.StudentId);
        //    return View(assignedRoom);
        //}

        #endregion Create and Edit of Assigned Room not used

        // GET: AssignedRooms/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            StudentAssignedRoom assignedRoom = await _db.StudentAssignedRooms.FindAsync(id);
            if (assignedRoom == null)
            {
                return HttpNotFound();
            }
            return View(assignedRoom);
        }

        // POST: AssignedRooms/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            StudentAssignedRoom assignedRoom = await _db.StudentAssignedRooms.FindAsync(id);
            if (assignedRoom != null) _db.StudentAssignedRooms.Remove(assignedRoom);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [AllowAnonymous]
        public async Task<ActionResult> AccommodationFeePayment(int id)
        {
            if (ModelState.IsValid)
            {
                var model = await _db.StudentAssignedRooms.Include(i => i.Hostel).Include(i => i.Student).AsNoTracking()
                                .Where(x => x.StudentAssignedRoomId.Equals(id)).FirstOrDefaultAsync();
                decimal hostelPrice = model.Hostel.HostelPrice;
                var url = Url.Action("ConfrimAccomodationPayment", "AssignedRooms", new { },
                                    protocol: Request.Url.Scheme);
                var serviceTypeId = string.Empty;

                var hasTransaction = await _db.StudentAccommodationFeePayments.AsNoTracking().Where(x => x.StudentId.Equals(model.StudentId)
                                            && x.SessionId.Equals(model.SessionId)).FirstOrDefaultAsync();
                if (hasTransaction != null)
                {
                    if (hasTransaction.IsPayed.Equals(false))
                    {
                        serviceTypeId = RemitaConfigParams.ACCOMODATIONSERVICETYPE;

                        var hashed = _query.HashRemitedValidate(hasTransaction.OrderId, RemitaConfigParams.APIKEY, RemitaConfigParams.MERCHANTID);
                        string checkurl = RemitaConfigParams.CHECKSTATUSURL + "/" + RemitaConfigParams.MERCHANTID + "/" + hasTransaction.OrderId + "/" + hashed + "/" + "orderstatus.reg";
                        string jsondata = new WebClient().DownloadString(checkurl);
                        var result = JsonConvert.DeserializeObject<RemitaResponse>(jsondata);
                        if (string.IsNullOrEmpty(result.Rrr))
                        {
                            var entry = _db.Entry(hasTransaction);
                            if (entry.State == EntityState.Detached)
                                _db.StudentAccommodationFeePayments.Attach(hasTransaction);
                            _db.StudentAccommodationFeePayments.Remove(hasTransaction);
                            await _db.SaveChangesAsync();
                        }
                        else
                        {
                            return RedirectToAction("ConfrimAccomodationPayment", new { orderID = hasTransaction.OrderId });
                        }
                    }
                }

                var orderId = $"UJAP{DateTime.Now.Ticks.ToString()}";
                var accomodationPayment = new StudentAccommodationFeePayment
                {
                    StudentAssignedRoomId = model.StudentAssignedRoomId,
                    OrderId = orderId,
                    PaymentDateTime = DateTime.Now,
                    SessionId = model.SessionId,
                    StudentId = model.StudentId,
                    ExpectedAmount = Convert.ToDouble(hostelPrice),
                    AmountPayed = Convert.ToDouble(hostelPrice),
                    //ReferenceNo = reference
                };
                _db.StudentAccommodationFeePayments.Add(accomodationPayment);

                var log = new RemitaPaymentLog
                {
                    OrderId = orderId,
                    PaymentName = "Accommodation Fee",
                    PaymentDate = DateTime.Now,
                    Amount = hostelPrice.ToString(),
                    PayerName = model.Student.FullName
                };
                _db.RemitaPaymentLogs.Add(log);
                await _db.SaveChangesAsync();

                var hash = _query.HashRemitaRequest(RemitaConfigParams.MERCHANTID, RemitaConfigParams.ACCOMODATIONSERVICETYPE, orderId, hostelPrice.ToString(), url, RemitaConfigParams.APIKEY);
                var postModel = new RemitaPostVm
                {
                    payerName = model.Student.FullName,
                    payerEmail = model.Student.Email ?? $"{model.Student.FullName}@uniben.edu",
                    payerPhone = model.Student.PhoneNumber ?? "0703000000",
                    amt = hostelPrice.ToString(),
                    hash = hash,
                    orderId = orderId,
                    merchantId = RemitaConfigParams.MERCHANTID,
                    responseurl = url,
                    serviceTypeId = RemitaConfigParams.ACCOMODATIONSERVICETYPE,
                    paymenttype = RemitaPaymentType.MasterCard.ToString()
                };
                return RedirectToAction("SubmitAccomodationPayment", postModel);
            }
            return View("StudentDetails");
        }

        public ActionResult SubmitAccomodationPayment(RemitaPostVm model)
        {
            return View(model);
        }

        [HttpGet]
        //[Authorize(Roles = RoleName.Admin + "," + RoleName.DatabaseAdmin + "," + RoleName.SuperAdmin + "," + RoleName.AdmissionUploadOfficer)]
        public ActionResult UpdateAccommdationRRR(string message)
        {
            ViewBag.Message = message;
            return View();
        }

        //[Authorize(Roles = RoleName.Admin + "," + RoleName.DatabaseAdmin + "," + RoleName.SuperAdmin + "," + RoleName.AdmissionUploadOfficer)]
        public async Task<ActionResult> UpdateAccommdationRRR(HttpPostedFileBase excelfile)
        {
            if (excelfile == null || excelfile.ContentLength == 0)
            {
                ViewBag.Error = "Please Select a excel file <br/>";
                ViewBag.Message = "You must select an excel file before you click Upload button";
                TempData["Title"] = "Error.";

                return View("Index");
            }
            HttpPostedFileBase file = Request.Files["excelfile"];
            if (excelfile.FileName.EndsWith("xls", StringComparison.CurrentCulture) || excelfile.FileName.EndsWith("xlsx", StringComparison.CurrentCulture))
            {
                string lastrecord = "";
                int recordCount = 0;
                string message = "";
                string fileContentType = file.ContentType;
                byte[] fileBytes = new byte[file.ContentLength];
                var data = file.InputStream.Read(fileBytes, 0, Convert.ToInt32(file.ContentLength));

                var utmeUploadError = new List<StudentUtmeUploadErrorVm>();
                bool hasUtmeUploadError = false;

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
                        string[] ssizes = validCheck.Split(' ');
                        string[] myArray = new string[2];
                        for (int i = 0; i < ssizes.Length; i++)
                        {
                            myArray[i] = ssizes[i];
                        }
                        string lineError = $"Line/Row number {myArray[0]}  and column {myArray[1]} is not rightly formatted, Please Check for anomalies ";
                        ViewBag.Message = lineError;
                        return View("ErrorException");
                    }

                    for (int row = 2; row <= noOfRow; row++)
                    {
                        var matNo = workSheet.Cells[row, 1].Value.ToString().Trim();
                        var paymentRef = workSheet.Cells[row, 2].Value.ToString().Trim();

                        var student = await _db.Students.AsNoTracking()
                                            .Where(x => x.MatricNo.Trim().ToUpper().Equals(matNo.ToUpper()))
                                            .FirstOrDefaultAsync();


                        if (student != null)
                        {
                            var applicantfeepayment = await _db.StudentAccommodationFeePayments.Where(x => x.StudentId.Equals(student.StudentId)
                          && x.SessionId.Equals(1)).FirstOrDefaultAsync();

                            if (applicantfeepayment != null)
                            {
                                applicantfeepayment.ReferenceNo = paymentRef;

                                _db.Entry(applicantfeepayment).State = EntityState.Modified;
                                await _db.SaveChangesAsync();
                                utmeUploadError.Add(new StudentUtmeUploadErrorVm { JambRegNo = student.JambRegNo, Row = row });
                                hasUtmeUploadError = true;
                            }
                        }
                        else
                        {
                            ViewBag.ErrorMessage = "Student not found";
                            return View("ErrorException");
                        }
                        recordCount++;
                        lastrecord = $"The last Updated record has the Surname  {student.LastName} and " +
                            $"First Name {student.FirstName} with  Reg No {student.MatricNo}";
                    }
                    try
                    {
                        await _db.SaveChangesAsync();

                    }
                    catch (Exception ex)
                    {
                        ViewBag.ErrorMessage = ex.Message;
                        return View("ErrorException");
                    }
                    if (hasUtmeUploadError)
                    {
                        ViewBag.ErrorInfo = $"Success!";
                        ViewBag.ErrorMessage = $"You have successfully Removed {recordCount} records...";
                        return View("ErrorException", utmeUploadError);
                    }
                    message = $"You have successfully Deleted {recordCount} records...  and {lastrecord}";
                    ViewBag.Message = message;
                    return RedirectToAction("Index", "Students", new { message });
                }
            }
            ViewBag.Error = $"File type is Incorrect <br/>";
            return View("Index");
        }

        [AllowAnonymous]
        public async Task<ActionResult> ConfrimAccomodationPayment(string RRR, string orderID)
        {
            StudentAccommodationFeePayment accomodationPayment;
            RemitaResponse result = new RemitaResponse();
            if (string.IsNullOrEmpty(orderID))
            {
                accomodationPayment = await _db.StudentAccommodationFeePayments.AsNoTracking()
                    .Where(x => x.ReferenceNo.Equals(RRR.Trim()))
                    .FirstOrDefaultAsync();
            }
            else
            {
                accomodationPayment = await _db.StudentAccommodationFeePayments.AsNoTracking()
                    .Where(x => x.OrderId.Equals(orderID.Trim()))
                    .FirstOrDefaultAsync();
            }
            if (accomodationPayment != null)
            {
                if (accomodationPayment.IsPayed.Equals(true))
                {
                    result.Message = accomodationPayment.TransactionMessage;
                    result.OrderId = accomodationPayment.OrderId;
                    result.Rrr = accomodationPayment.ReferenceNo;
                    result.Status = accomodationPayment.IsPayed.ToString();
                    return RedirectToAction("ConfrimRrrPayment", "RemitaPaymentLogs", result);
                }
                var log = await _db.RemitaPaymentLogs.AsNoTracking().Where(x => x.OrderId.Equals(accomodationPayment.OrderId))
                                .FirstOrDefaultAsync();

                var hashed = _query.HashRemitedValidate(orderID, RemitaConfigParams.APIKEY, RemitaConfigParams.MERCHANTID);
                string url = RemitaConfigParams.CHECKSTATUSURL + "/" + RemitaConfigParams.MERCHANTID + "/" + accomodationPayment.OrderId + "/" + hashed + "/" + "orderstatus.reg";
                string jsondata = new WebClient().DownloadString(url);
                result = JsonConvert.DeserializeObject<RemitaResponse>(jsondata);

                if (result.Status.Equals("00") || result.Status.Equals("01"))
                {
                    accomodationPayment.IsPayed = true;
                    var assignedRoom = await _db.StudentAssignedRooms.FindAsync(accomodationPayment.StudentAssignedRoomId);
                    if (assignedRoom != null)
                    {
                        assignedRoom.PaymentStatus = true;
                    }
                    accomodationPayment.TransactionMessage = result.Message;
                    accomodationPayment.ReferenceNo = result.Rrr;
                    accomodationPayment.PaymentDateTime = DateTime.Now;
                    _db.Entry(accomodationPayment).State = EntityState.Modified;

                    _query.UpdateTransactionLog(log, result);

                    await _db.SaveChangesAsync();
                }
                else
                {
                    accomodationPayment.IsPayed = false;
                    accomodationPayment.TransactionMessage = result.Message;
                    accomodationPayment.ReferenceNo = result.Rrr;
                    accomodationPayment.PaymentDateTime = DateTime.Now;
                    _db.Entry(accomodationPayment).State = EntityState.Modified;

                    _query.UpdateTransactionLog(log, result);

                    await _db.SaveChangesAsync();

                    return RedirectToAction("RetryAccomodationPayment", new { rrr = result.Rrr });
                }

                return RedirectToAction("ConfrimRrrPayment", "RemitaPaymentLogs", result);
            }
            var message = $"There is no payment that has either the RRR {RRR} or" +
                          $" Order Id {orderID} for Accommodation Payment";
            return RedirectToAction("GetPaymentStatus", "RemitaServices", new { message });
        }

        [AllowAnonymous]
        public ActionResult RetryAccomodationPayment(string rrr)
        {
            try
            {
                var hashrrr = _query.HashRrrQuery(rrr, RemitaConfigParams.APIKEY, RemitaConfigParams.MERCHANTID);
                string posturl = RemitaConfigParams.CHECKSTATUSURL + "/" + RemitaConfigParams.MERCHANTID + "/" + rrr + "/" + hashrrr + "/" + "status.reg";
                string jsondata = new WebClient().DownloadString(posturl);
                var result = JsonConvert.DeserializeObject<RemitaResponse>(jsondata);
                if (result.Status.Equals("00") || result.Status.Equals("01"))
                {
                    return RedirectToAction("ConfrimAccomodationPayment", "AssignedRooms", new { RRR = result.Rrr, orderID = result.OrderId });
                }
                var hash = _query.HashRemitedRePost(RemitaConfigParams.MERCHANTID, rrr, RemitaConfigParams.APIKEY);
                var url = Url.Action("ConfrimAccomodationPayment", "AssignedRooms", new { },
                                       protocol: Request.Url.Scheme);
                var model = new RemitaRePostVm
                {
                    rrr = rrr,
                    merchantId = RemitaConfigParams.MERCHANTID,
                    hash = hash,
                    responseurl = url
                };
                return View(model);
            }
            catch (Exception)
            {
                return RedirectToAction("GetPaymentStatus", "RemitaServices", new { message = "Please check the RRR supplied and try again" });
            }

        }

        #region PayStack

        //public async Task<ActionResult> ConfrimAccommodationFeePayment(string reference)
        //{
        //    var testOrLiveSecret = ConfigurationManager.AppSettings["PayStackSecret"];
        //    var api = new PayStackApi(testOrLiveSecret);
        //    //Verifying a transaction
        //    var verifyResponse = api.Transactions.Verify(reference); // auto or supplied when initializing;
        //    if (verifyResponse.Status)
        //    {
        //        var convertedValues = new List<SelectableEnumItem>();
        //        var valuepair = verifyResponse.Data.Metadata.Where(x => x.Key.Contains("custom")).Select(s => s.Value);

        // foreach (var item in valuepair) { convertedValues = ((JArray)item).Select(x => new
        // SelectableEnumItem { key = (string)x["display_name"], value = (string)x["value"]
        // }).ToList(); } //var studentid = _db.Users.Find(id);assignedroomid var assignedRoomId =
        // Convert.ToInt32(convertedValues.Where(x => x.key.Equals("assignedroomid")) .Select(s => s.value).FirstOrDefault());

        // var assignedRoom = await _db.AssignedRooms.FindAsync(assignedRoomId); if (assignedRoom !=
        // null) { assignedRoom.PaymentStatus = true; _db.Entry(assignedRoom).State =
        // EntityState.Modified; }

        // var accomodationPayement = new AccommodationFeePayment() { PaymentDateTime = DateTime.Now,
        // SessionId = Convert.ToInt32(convertedValues.Where(x => x.key.Equals("sessionid"))
        // .Select(s => s.value).FirstOrDefault()), StudentId = convertedValues.Where(x =>
        // x.key.Equals("studentid")).Select(s => s.value) .FirstOrDefault(), AssignedRoomId =
        // assignedRoomId, ExpectedAmount = Convert.ToDouble(convertedValues.Where(x =>
        // x.key.Equals("expectedamount")).Select(s => s.value) .FirstOrDefault()), AmountPayed =
        // ConvertToNaira((int)verifyResponse.Data.Amount), ReferenceNo = reference };

        //        _db.AccommodationFeePayments.Add(accomodationPayement);
        //    }
        //    await _db.SaveChangesAsync();
        //    return RedirectToAction("StudentDetails");
        //}

        #endregion PayStack

        public PartialViewResult HostelHistory(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                id = _studentQuery.GetStudentId(userId);
            }
            var history = _db.StudentAccommodationFeePayments.Include(i => i.StudentAssignedRoom.Block).Include(i => i.StudentAssignedRoom)
                .Include(i => i.StudentAssignedRoom.Room).Include(i => i.Session).Include(i => i.Student).AsNoTracking()
                .Where(x => x.StudentId.Equals(id) && x.IsPayed.Equals(true)).ToList();
            return PartialView(history);
        }

        public async Task<ActionResult> HostelApplicationReport()
        {
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            ViewBag.DepartmentId = new SelectList(await _db.Departments.AsNoTracking().ToListAsync(), "DepartmentId", "DeptName");
            ViewBag.LevelId = new SelectList(await _db.Levels.AsNoTracking().ToListAsync(), "LevelId", "LevelName");
            ViewBag.SchoolProgrammeId = new SelectList(await _db.SchoolProgrammes.AsNoTracking().ToListAsync(), "SchoolProgrammeId", "FancyName");

            return View();
        }

        public async Task<ActionResult> GetHostelApplicationReport(int SchoolProgrammeId, int? DepartmentId, int? LevelId, int? SessionId)
        {
            if (SessionId == null)
            {
                SessionId = _query.GetCurrentSessionId(SchoolProgrammeId);
            }
            var aplicationFee = await _db.HostelApplications.Include(i => i.Student).Include(i => i.Session)
                                .Include(i => i.Student.Programme.Department).Include(i => i.Student.Level)
                                .Include(i => i.Student.SchoolProgramme).AsNoTracking()
                                .Where(x => x.IsPayed.Equals(true)
                                && x.SessionId.Equals((int)SessionId)
                                && x.Student.SchoolProgramme.SchoolProgrammeId.Equals(SchoolProgrammeId))
                                .ToListAsync();
            if (DepartmentId != null & LevelId != null)
            {
                aplicationFee = aplicationFee.Where(x => x.Student.Programme.Department.DepartmentId.Equals((int)DepartmentId)
                                                 && x.Student.Level.LevelId.Equals((int)LevelId)
                                                 && x.SessionId.Equals((int)SessionId)).ToList();
            }
            else if (DepartmentId != null)
            {
                aplicationFee = aplicationFee.Where(x => x.Student.Programme.Department.DepartmentId.Equals((int)DepartmentId)
                ).ToList();
            }
            else if (LevelId != null)
            {
                aplicationFee = aplicationFee.Where(x => x.Student.Level.LevelId.Equals((int)LevelId)).ToList();
            }

            var data = aplicationFee.Select(s => new
            {
                s.Student.MatricNo,
                s.Student.FullName,
                s.Student.Gender,
                s.Student.Programme.Department.DeptName,
                s.Student.Programme.ProgrammeName,
                s.Student.Level.LevelName,
                s.Student.PhoneNumber
            }).ToList();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> AccommodationFeeReport()
        {
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            ViewBag.HostelId = new SelectList(await _db.Hostels.AsNoTracking().ToListAsync(), "HostelId", "HostelName");
            ViewBag.BlockId = new SelectList(await _db.Blocks.AsNoTracking().ToListAsync(), "BlockId", "BlockName");

            return View();
        }

        public async Task<ActionResult> GetAccommodationFeeReport(int? HostelId, int? BlockId, int SessionId)
        {
            var accomodationPayment = await _db.StudentAccommodationFeePayments.Include(i => i.Student.SchoolProgramme)
                                        .Include(i => i.Session).Include(i => i.StudentAssignedRoom.Hostel).Include(i => i.Student.Programme)
                                        .Include(i => i.StudentAssignedRoom.Room).Include(i => i.StudentAssignedRoom.Block).AsNoTracking()
                                        .Where(x => x.IsPayed.Equals(true)
                                        && x.SessionId.Equals(SessionId))
                                        .ToListAsync();
            if (BlockId != null)
            {
                accomodationPayment = accomodationPayment.Where(x => x.StudentAssignedRoom.Block.BlockId.Equals((int)BlockId)).ToList();
            }
            else if (HostelId != null)
            {
                accomodationPayment = accomodationPayment.Where(x => x.StudentAssignedRoom.Hostel.HostelId.Equals((int)HostelId)).ToList();
            }


            var data = accomodationPayment.Select(s => new
            {
                MatricNo = s.Student.MatricNo ?? "",
                FullName = s.Student.FullName ?? "",
                Gender = s.Student.Gender ?? "",
                ReferenceNo = s.ReferenceNo ?? "",
                ProgrammeName = s.Student?.Programme?.ProgrammeName ?? "",
                HostelName = s.StudentAssignedRoom?.Hostel?.HostelName ?? "",
                BlockName = s.StudentAssignedRoom?.Block?.BlockName ?? "",
                RoomName = s.StudentAssignedRoom?.Room?.RoomName ?? "",
                s.AmountPayed,
                IsPayed = s.IsPayed.ToString() ?? ""
            }).ToList();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult UploadStudentAccomodation()
        {
            return PartialView();
        }


        [HttpPost]
        public async Task<ActionResult> UploadStudentAccomodation(HttpPostedFileBase excelfile)
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
                var utmeUploadError = new List<StudentUtmeUploadErrorVm>();

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
                        var studentEmail = workSheet.Cells[row, 1].Value.ToString().Trim();
                        var hostelName = workSheet.Cells[row, 2].Value.ToString().Trim();
                        var blockName = workSheet.Cells[row, 3].Value.ToString().Trim();
                        var roomName = workSheet.Cells[row, 4].Value.ToString().Trim();
                        var bedSpace = Convert.ToChar(workSheet.Cells[row, 5].Value.ToString().Trim());
                        var sessionName = workSheet.Cells[row, 6].Value.ToString().Trim();



                        var seession = _db.Sessions.FirstOrDefault(x => x.SessionName.ToUpper().Trim().Equals(sessionName));
                        int cSessionId = seession.SessionId;

                        var room = await _db.Rooms.Include(i => i.Buildings.Hostel).Include(i => i.Buildings).AsNoTracking()
                                    .Where(x => x.RoomName.Trim().ToUpper().Equals(roomName.ToUpper())
                                    && x.Buildings.BlockName.Trim().ToUpper().Equals(blockName.ToUpper())
                                    && x.Buildings.Hostel.HostelName.Trim().ToUpper().Equals(hostelName.ToUpper()))
                                    .FirstOrDefaultAsync();

                        //var stduent = _studentQuery.GetStudent(studentEmail);
                        var stduent = _studentQuery.GetStudentByMatNumber(studentEmail);


                        if (room == null || stduent == null)
                        {
                            utmeUploadError.Add(new StudentUtmeUploadErrorVm { JambRegNo = studentEmail, Row = row });
                        }
                        else
                        {
                            try
                            {
                                var assinedRoom = _db.StudentAssignedRooms.Include(s => s.Session).Include(s => s.StudentAccommodationFeePayments)
                                                    .Where(x => x.StudentId.Equals(stduent.StudentId)
                                                    && x.SessionId.Equals(cSessionId)).ToList();
                                if (assinedRoom.Count() == 0)
                                {
                                    await SaveAssignedRoom(stduent, room, room.Buildings, room.Buildings.HostelId, bedSpace, cSessionId);
                                    recordCount += 1;
                                }
                                else
                                {
                                    foreach (var aroom in assinedRoom)
                                    {
                                        var checkAccommodation = await _db.StudentAccommodationFeePayments.Include(a => a.Student)
                                                                    .Include(a => a.Session)
                                                                    .Where(a => a.Student.StudentId.Trim().Equals(stduent.StudentId.Trim())
                                                                    && a.Session.SessionId.Equals(cSessionId) && a.StudentAssignedRoomId.Equals(aroom.StudentAssignedRoomId)
                                                                    && a.IsPayed.Equals(true)).FirstOrDefaultAsync();

                                        if (checkAccommodation == null)
                                        {
                                            var checkAccommodationPay = await _db.StudentAccommodationFeePayments.Include(a => a.Student)
                                                                    .Include(a => a.Session)
                                                                    .Where(a => a.Student.StudentId.Trim().Equals(stduent.StudentId.Trim())
                                                                    && a.Session.SessionId.Equals(cSessionId) && a.StudentAssignedRoomId.Equals(aroom.StudentAssignedRoomId)
                                                                    && a.IsPayed.Equals(false)).FirstOrDefaultAsync();

                                            if (checkAccommodationPay != null)
                                            {
                                                _db.StudentAccommodationFeePayments.Remove(checkAccommodationPay);
                                                _db.StudentAssignedRooms.Remove(aroom);
                                                _db.SaveChanges();
                                            }
                                            else
                                            {
                                                _db.StudentAssignedRooms.Remove(aroom);
                                                _db.SaveChanges();
                                            }

                                            await SaveAssignedRoom(stduent, room, room.Buildings, room.Buildings.HostelId, bedSpace, cSessionId);
                                            recordCount += 1;

                                        }
                                        else //Used to changed Allocation already paid for whe there is over-allocation
                                        {
                                            //var reassigned = await SaveReAssignedRoom(stduent, room, room.Buildings, room.Buildings.HostelId, bedSpace, cSessionId);
                                            //recordCount += 1;

                                            aroom.RoomId = room.RoomId;
                                            aroom.BlockId = room.Buildings.BlockId;
                                            aroom.HostelId = room.Buildings.HostelId;
                                            aroom.BedSpace = bedSpace.ToString();

                                            string body = $"Dear {stduent.FirstName}, You have been Reassigned to {room.RoomName}" +
                                                          $" at block {room.Buildings.BlockName} in {hostelName}. " +
                                                          $"login and print your allocation form then proceed for clearance";
                                            _db.Entry(checkAccommodation).State = EntityState.Modified;

                                            if (await _db.SaveChangesAsync() > 0)
                                            {
                                                await SMSClass.SendSMS("UNIJOS", body, stduent.PhoneNumber);
                                            }
                                        }


                                        //var checkAccommodationPayTrue = await _db.AccommodationFeePayments.Include(a => a.Student)
                                        //                            .Include(a => a.Session)
                                        //                            .Where(a => a.Student.StudentId.Trim().Equals(stduent.StudentId.Trim())
                                        //                            && a.Session.SessionId.Equals(cSessionId) && a.IsPayed.Equals(true)).FirstOrDefaultAsync();
                                        //if (checkAccommodationPayTrue == null)
                                        //{
                                        //    _db.AssignedRooms.Remove(aroom);
                                        //    _db.SaveChanges();
                                        //}


                                    }
                                    //await SaveAssignedRoom(stduent, room, room.Buildings, room.Buildings.HostelId, bedSpace, cSessionId);
                                    //recordCount += 1;
                                }


                            }
                            catch (Exception)
                            {
                                utmeUploadError.Add(new StudentUtmeUploadErrorVm { JambRegNo = studentEmail, Row = row });
                            }
                        }


                    }
                    message = $"You have successfully Uploaded {recordCount} records...";
                    ViewBag.Message = message;

                }
                if (utmeUploadError.Count() > 0)
                {
                    ViewBag.ErrorInfo = $"These Student has not been assigned hostel yet";
                    ViewBag.ErrorMessage = $"You have successfully Uploaded {utmeUploadError.Count()} records...";
                    return View("ErrorException", utmeUploadError);
                }
                else
                {
                    return RedirectToAction("Index", "AssignedRooms", new { message });
                }
            }

            ViewBag.Error = $"File type is Incorrect <br/>";
            return View("Index");
        }



        public ActionResult UploadReallocatedAccomodation()
        {
            return PartialView();
        }


        [HttpPost]
        public async Task<ActionResult> UploadReallocatedAccomodation(HttpPostedFileBase excelfile)
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
                int recordCount = 0;
                string message = "";
                string fileContentType = file.ContentType;
                byte[] fileBytes = new byte[file.ContentLength];
                var data = file.InputStream.Read(fileBytes, 0, Convert.ToInt32(file.ContentLength));
                var utmeUploadError = new List<StudentUtmeUploadErrorVm>();

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

                    var seession = _db.Sessions.FirstOrDefault(x => x.SessionName.ToUpper().Trim().Equals("2019/2020"));
                    int cSessionId = seession.SessionId;

                    for (int row = 2; row <= noOfRow; row++)
                    {
                        var studentEmail = workSheet.Cells[row, 1].Value.ToString().Trim();
                        var hostelName = workSheet.Cells[row, 2].Value.ToString().Trim();
                        var blockName = workSheet.Cells[row, 3].Value.ToString().Trim();
                        var roomName = workSheet.Cells[row, 4].Value.ToString().Trim();
                        var bedSpace = Convert.ToChar(workSheet.Cells[row, 5].Value.ToString().Trim());


                        var room = await _db.Rooms.Include(i => i.Buildings.Hostel).AsNoTracking()
                                    .Where(x => x.RoomName.Trim().ToUpper().Equals(roomName.ToUpper())
                                    && x.Buildings.BlockName.Trim().ToUpper().Equals(blockName.ToUpper())
                                    && x.Buildings.Hostel.HostelName.Trim().ToUpper().Equals(hostelName.ToUpper()))
                                    .FirstOrDefaultAsync();
                        var stduent = _studentQuery.GetStudent(studentEmail);
                        //var activeSession = _query.GetCurrentProgrammeSessionId(stduent.SchoolProgrammeId);

                        if (room == null || stduent == null)
                        {
                            utmeUploadError.Add(new StudentUtmeUploadErrorVm { JambRegNo = studentEmail, Row = row });
                        }
                        else
                        {
                            try
                            {
                                var assinedRoom = await _db.StudentAssignedRooms.Where(x => x.StudentId.Equals(stduent.StudentId)).FirstOrDefaultAsync();
                                if (assinedRoom != null)
                                {
                                    assinedRoom.RoomId = room.RoomId;
                                    assinedRoom.BlockId = room.BlockId;
                                    assinedRoom.HostelId = room.Buildings.HostelId;
                                    assinedRoom.BedSpace = bedSpace.ToString();
                                    _db.Entry(assinedRoom).State = EntityState.Modified;
                                }
                                recordCount += 1;
                            }
                            catch (Exception)
                            {
                                utmeUploadError.Add(new StudentUtmeUploadErrorVm { JambRegNo = studentEmail, Row = row });
                            }
                        }
                    }
                    await _db.SaveChangesAsync();
                    message = $"You have successfully Uploaded {recordCount} records...";
                    ViewBag.Message = message;

                }
                if (utmeUploadError.Count() > 0)
                {
                    ViewBag.ErrorInfo = $"These Student has not been assigned hostel yet";
                    ViewBag.ErrorMessage = $"You have successfully Uploaded {utmeUploadError.Count()} records...";
                    return View("ErrorException", utmeUploadError);
                }
                else
                {
                    return RedirectToAction("Index", "AssignedRooms", new { message });
                }
            }

            ViewBag.Error = $"File type is Incorrect <br/>";
            return View("Index");
        }

        public async Task<ActionResult> PrintHostelAcceptanceLetter()
        {
            string email = userId;
            Student studentId = _studentQuery.GetStudent(email);
            var sessionId = _query.GetCurrentSessionId(studentId.SchoolProgrammeId);

            PrintHostelAcceptanceLetterVm isPaidhostelApplicationFee = await _db.StudentAccommodationFeePayments.AsNoTracking()
                                                .Include(i => i.Student.Programme.Department)
                                                .Include(i => i.Session)
                                                .Include(i => i.Student)
                                                .Include(i => i.StudentAssignedRoom.Hostel)
                                                .Include(i => i.StudentAssignedRoom)
                                                .Include(i => i.StudentAssignedRoom.Block)
                                                .Where(x => x.StudentId.Equals(studentId.StudentId) && x.IsPayed.Equals(true) && x.SessionId == sessionId)
                                                .Select(s => new PrintHostelAcceptanceLetterVm
                                                {
                                                    MatriculationNumber = s.Student.MatricNo,
                                                    StudentId = s.Student.StudentId,
                                                    Gender = s.Student.Gender,
                                                    BlockName = s.StudentAssignedRoom.Block.BlockName,
                                                    RoomName = s.StudentAssignedRoom.Room.RoomName,
                                                    Department = s.Student.Programme.Department.DeptName,
                                                    Hostel = s.StudentAssignedRoom.Hostel.HostelName,
                                                    DateAssigned = s.PaymentDateTime.ToString(),
                                                    firstName = s.Student.FirstName,
                                                    LastName = s.Student.LastName,
                                                    MiddleName = s.Student.MiddleName,
                                                    ProgrammeName = s.Student.Programme.ProgrammeName,
                                                    SessionName = s.Session.SessionName
                                                }).FirstOrDefaultAsync();
            if (isPaidhostelApplicationFee == null)
            {
                return RedirectToAction("StudentDashBoard", "Students");
            }

            return View(isPaidhostelApplicationFee);
        }


        //[Authorize(Roles = RoleName.SuperAdmin + "," + RoleName.Admin)]
        public async Task<ActionResult> GetHostelAllocationAndPaymentList()
        {
            var bookedAndPaid = await _db.StudentAssignedRooms.Where(x => x.PaymentStatus.Equals(true)
                                        && x.SessionId.Equals(sessionId)).ToListAsync();

            // Notify students of refund request
            //foreach (var item in bookedAndPaid)
            //{
            //    string body = $"{item.Student.FirstName} if you got multiple allocation and didn't get a room, go to bursary unit with evidence, to request refund";

            //    await SMSClass.SendSMS("UNIJOS SIS", body, "07035473090"); //EBULK SMS API
            //}

            var message = "Success";

            return new JsonResult { Data = new { status = true, message } };
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