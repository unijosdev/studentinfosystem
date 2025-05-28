using Newtonsoft.Json;
using SwiftKampus.BusinessLogic;
using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampus.ViewModels;
using SwiftKampusModel;
using SwiftKampusModel.Payment;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SwiftKampus.Controllers
{
    public class IdCardPaymentsController : BaseController
    {
        public IdCardPaymentsController(SchoolDbContext _db) : base(_db)
        {
        }

        // GET: IdCardPayments
        public async Task<ActionResult> Index()
        {
            var student = await _db.Students.Include(i => i.SchoolProgramme).Include(i => i.Session).Include(i => i.Programme.Department.Faculty)
                               .AsNoTracking().FirstOrDefaultAsync(x => x.Email.Trim().ToUpper().Equals(userId.Trim().ToUpper())
                               && x.Active.Equals(true));
            if (User.IsInRole(RoleName.Student))
            {
                var idCardPayments = _db.IdCardPayments.Include(i => i.Session).Include(i => i.Student)
                    .Where(s => s.StudentId.Equals(student.StudentId));
                return View(await idCardPayments.ToListAsync());
            }
            else
            {
                var idCardPayments = _db.IdCardPayments.Include(i => i.Session).Include(i => i.Student);
                return View(await idCardPayments.ToListAsync());
            }
           
        }

        public ActionResult PaymentHistory()
        {
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FancyName");
            ViewBag.FacultyId = new SelectList(_db.Faculties.AsNoTracking(), "FacultyId", "FacultyName");
            ViewBag.DepartmentId = new SelectList(_db.Departments.AsNoTracking(), "DepartmentId", "DeptName");
            ViewBag.ProgrammeId = new SelectList(_db.Programmes.AsNoTracking(), "ProgrammeId", "ProgrammeName");
            return View();
        }

        public async Task<ActionResult> GetPaymentHistory(int? SessionId, int? SchoolProgrammeId, int? FacultyId, int? DepartmentId, int? ProgrammeId,
           string hasRegistered)
        {
            var idCardPaymentVm = new List<ChangeOfCoursePaymentHistoryVm>();
            bool status = false;
            if (!string.IsNullOrEmpty(hasRegistered) && hasRegistered.Equals("True"))
            {
                status = true;
            }
            int sessionID = 0;
            if (SchoolProgrammeId != null)
            {
                if (SessionId == null)
                {
                    sessionID = _query.GetCurrentSessionId((int)SchoolProgrammeId);
                }
                else
                {
                    sessionID = (int)SessionId;
                }

                var idCardPaymentList = await _db.IdCardPayments.Include(c => c.Session).Include(i => i.Student.Programme)
                                        .Include(i => i.Student.Programme.Department).Where(x => x.SessionId.Equals(sessionID)
                                        && x.IsPayed.Equals(status)).ToListAsync();

                if (ProgrammeId != null)
                {
                    idCardPaymentList = idCardPaymentList.Where(x => x.Student.Programme.ProgrammeId.Equals((int)ProgrammeId)).ToList();
                }
                else if (DepartmentId != null)
                {
                    idCardPaymentList = idCardPaymentList.Where(x => x.Student.Programme.Department.DepartmentId.Equals((int)DepartmentId)).ToList();
                }
                else if (FacultyId != null)
                {
                    idCardPaymentList = idCardPaymentList.Where(x => x.Student.Programme.Department.FacultyId.Equals((int)FacultyId)).ToList();
                }
                idCardPaymentVm = MapToIdCardPaymentHistoryVm(idCardPaymentList.OrderBy(x => x.Student.FullName).ToList());
            }
            var data = idCardPaymentVm.OrderBy(x => x.Id).ToList();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }


        private List<ChangeOfCoursePaymentHistoryVm> MapToIdCardPaymentHistoryVm(List<IdCardPayment> v)
        {
            var cOfCourse = new List<ChangeOfCoursePaymentHistoryVm>();
            int count = 1;
            foreach (var vm in v)
            {
                cOfCourse.Add(new ChangeOfCoursePaymentHistoryVm
                {
                    Id = count,
                    FullName = vm.Student?.FullName ?? "",
                    CourseAdmitted = vm.Student?.Programme?.ProgrammeName ?? "",
                    Amount = vm.TotalAmount,
                    RegNo = vm.Student?.JambRegNo ?? "",
                    Status = vm.IsPayed.ToString(),
                    Message = vm.TransactionMessage,
                    ReferenceNumber = vm.ReferenceNo
                });
                count += 1;
            }
            return cOfCourse;
        }
        // GET: IdCardPayments/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            IdCardPayment idCardPayment = await _db.IdCardPayments.FindAsync(id);
            if (idCardPayment == null)
            {
                return HttpNotFound();
            }
            return View(idCardPayment);
        }


        //public ActionResult MakePayment()
        //{
        //    var changeOfCoureType = from ChangeOfCourseType s in Enum.GetValues(typeof(ChangeOfCourseType))
        //                            select new { ID = s, Name = s.ToString() };

        //    ViewBag.ChangeOfCourseType = new SelectList(changeOfCoureType, "Name", "Name");
        //    return View();
        //}

        public async Task<ActionResult> Create()
        {
            var student = _studentQuery.GetStudent(userId);
            var applicantPaymentVm = new IdCardPaymentVm();
            var idCardsetting = new IdCardPaymentSetting();
            var id = "";
            var phoneNumber = "";


            idCardsetting = _db.IdCardPaymentSettings.AsNoTracking().Where(x => x.StudentType.Equals(student.StudentStatus)
                                    && x.SessionId.Equals(sessionId)).FirstOrDefault();

            if (idCardsetting == null)
            {
                ViewBag.Message = $"Fee has not been set for this Student Type ({student.StudentStatus})";
                return View();
            }

            var hasTransactionProcessed = await _db.IdCardPayments.AsNoTracking()
                                     .Where(x => x.StudentId.Equals(student.StudentId)
                                     && x.SessionId.Equals(sessionId) && x.IsPayed.Equals(true)
                                     && x.Student.StudentStatus.Equals(student.StudentStatus)
                                     && x.IsProcessed.Equals(false)).FirstOrDefaultAsync();

            if (hasTransactionProcessed != null)
            {
                return RedirectToAction("Index");
            }

            var hasTransaction = await _db.IdCardPayments.AsNoTracking()
                                     .Where(x => x.StudentId.Equals(student.StudentId)
                                     && x.SessionId.Equals(sessionId) && x.IsPayed.Equals(false)
                                     && x.Student.StudentStatus.Equals(student.StudentStatus))
                                     .FirstOrDefaultAsync();

            if (hasTransaction != null)
            {
                var hashed = _query.HashRemitedValidate(hasTransaction.OrderId, RemitaConfigParams.APIKEY, RemitaConfigParams.MERCHANTID);
                string checkurl = RemitaConfigParams.CHECKSTATUSURL + "/" + RemitaConfigParams.MERCHANTID + "/" + hasTransaction.OrderId + "/" + hashed + "/" + "orderstatus.reg";
                var checkResult = CheckExistingTransaction(checkurl);

                if (checkResult.Item1.Equals(true))
                {
                    if (string.IsNullOrEmpty(checkResult.Item2.Rrr))
                    {
                        var entry = _db.Entry(hasTransaction);
                        if (entry.State == EntityState.Detached)
                            _db.IdCardPayments.Attach(hasTransaction);
                        _db.IdCardPayments.Remove(hasTransaction);
                        _db.SaveChanges();
                    }
                    else
                    {
                        return RedirectToAction("ConfrimIdCardPayment", new { orderID = hasTransaction.OrderId });
                    }
                }
                else
                {
                    ViewBag.ErrorInfo = $"Check your internet connection and try again";
                    ViewBag.ErrorMessage = "Remita is currently unreachable";
                    return View("RemitaErrorPage");
                }
            }
            if (student != null)
            {
                id = student.JambRegNo;
                phoneNumber = student.PhoneNumber;
            }
            else
            {
                ViewBag.Message = $"Student Record Not Found";
                return View();
            }

            long milliseconds = DateTime.Now.Ticks;
            var url = Url.Action("ConfrimIdCardPayment", "IdCardPayments", new { },
                                   protocol: Request.Url.Scheme);
            if (idCardsetting != null)
            {
                applicantPaymentVm.FullName = student.FullName;
                applicantPaymentVm.TotalAmount = idCardsetting.Amount;
                applicantPaymentVm.payerName = await _query.GetUserFullName(userId);
                applicantPaymentVm.payerEmail = userId;
                applicantPaymentVm.payerPhone = phoneNumber ?? "07030000000";
                applicantPaymentVm.amt = idCardsetting.Amount.ToString();
                applicantPaymentVm.merchantId = RemitaConfigParams.MERCHANTID;
                applicantPaymentVm.orderId = $"UJID{milliseconds.ToString()}";
                applicantPaymentVm.responseurl = url;
                applicantPaymentVm.StudentId = student.StudentId;
                applicantPaymentVm.serviceTypeId = RemitaConfigParams.CHANGEOFCOURSE;
                applicantPaymentVm.ExpectedAmount = idCardsetting.Amount;
                applicantPaymentVm.SessionId = sessionId;
                applicantPaymentVm.StudentType = student.StudentStatus;
                applicantPaymentVm.apiKey = RemitaConfigParams.APIKEY;
                applicantPaymentVm.SessionName = _query.GetCurrentSessionName(student.SchoolProgrammeId);

                return View(applicantPaymentVm);
            }
            //ViewBag.Message = $"{applicantsetting.SchoolProgramme.FancyName} Form is no longer Active for sales";
            return View();
        }

        // POST: ApplicantPayments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(IdCardPaymentVm model)
        {
            if (ModelState.IsValid)
            {

                var hasTransaction = await _db.IdCardPayments.AsNoTracking()
                                        .Where(x => x.StudentId.Equals(model.StudentId)
                                        && x.SessionId.Equals(model.SessionId)).FirstOrDefaultAsync();

                model.paymenttype = model.RemitaPaymentType.ToString().Replace("_", " ").ToLower();
                if (hasTransaction != null)
                {
                    model.hash = _query.HashRemitaRequest(model.merchantId, model.serviceTypeId, hasTransaction.OrderId, model.amt, model.responseurl, RemitaConfigParams.APIKEY);
                    return RedirectToAction("ConfrimApplicationPayment", new { orderID = hasTransaction.OrderId });
                }
                var idCardPayment = new IdCardPayment
                {
                    OrderId = model.orderId,
                    PaymentDateTime = DateTime.Now,
                    SessionId = model.SessionId,
                    StudentId = model.StudentId,
                    TotalAmount = Convert.ToDecimal(model.amt),
                    CardType = model.StudentType
                    //ReferenceNo = reference
                };
                _db.IdCardPayments.Add(idCardPayment);
                var log = new RemitaPaymentLog
                {
                    OrderId = model.orderId,
                    PaymentName = "ID Card Payment",
                    PaymentDate = DateTime.Now,
                    Amount = model.amt,
                    PayerName = model.payerName
                };
                _db.RemitaPaymentLogs.Add(log);
                await _db.SaveChangesAsync();
                model.hash = _query.HashRemitaRequest(model.merchantId, model.serviceTypeId, model.orderId, model.amt, model.responseurl, RemitaConfigParams.APIKEY);
                return RedirectToAction("SubmitRemita", model);
            }
            return View(model);
        }

        public ActionResult SubmitRemita(IdCardPaymentVm model)
        {
            return View(model);
        }


        [AllowAnonymous]
        public async Task<ActionResult> ConfrimIdCardPayment(string RRR, string orderID)
        {
            IdCardPayment idCardPayment;
            RemitaResponse result = new RemitaResponse();

            if (string.IsNullOrEmpty(orderID))
            {
                idCardPayment = await _db.IdCardPayments.AsNoTracking()
                                            .Where(x => x.ReferenceNo.Equals(RRR.Trim()))
                                            .FirstOrDefaultAsync();
            }
            else
            {
                idCardPayment = await _db.IdCardPayments.AsNoTracking()
                                            .Where(x => x.OrderId.Equals(orderID.Trim()))
                                            .FirstOrDefaultAsync();
            }
            if (idCardPayment != null)
            {
                if (idCardPayment.IsPayed.Equals(true))
                {
                    result.Message = idCardPayment.TransactionMessage;
                    result.OrderId = idCardPayment.OrderId;
                    result.Rrr = idCardPayment.ReferenceNo;
                    result.Status = idCardPayment.IsPayed.ToString();
                    return RedirectToAction("ConfrimRrrPayment", "RemitaPaymentLogs", result);
                }
                var log = await _db.RemitaPaymentLogs.AsNoTracking()
                                    .Where(x => x.OrderId.Equals(idCardPayment.OrderId))
                                    .FirstOrDefaultAsync();

                var hashed = _query.HashRemitedValidate(orderID, RemitaConfigParams.APIKEY, RemitaConfigParams.MERCHANTID);
                string url = RemitaConfigParams.CHECKSTATUSURL + "/" + RemitaConfigParams.MERCHANTID + "/" + orderID + "/" + hashed + "/" + "orderstatus.reg";
                string jsondata = new WebClient().DownloadString(url);
                result = JsonConvert.DeserializeObject<RemitaResponse>(jsondata);
                var student = _studentQuery.GetStudent(userId);

                string SMSbody = $"{student.FirstName} your ID Card payment is confirmed. You are to make payment for your medical examination from your dashboard";

                if (result.Status.Equals("00") || result.Status.Equals("01"))
                {
                    var appPayment =
                    idCardPayment.ReferenceNo = result.Rrr;
                    idCardPayment.IsPayed = true;
                    idCardPayment.TransactionMessage = result.Message;
                    _db.Entry(idCardPayment).State = EntityState.Modified;

                    _query.UpdateTransactionLog(log, result);
                    await _db.SaveChangesAsync();
                    await SMSClass.SendSMS("UNIJOS SIS", SMSbody, student.PhoneNumber); //EBULK SMS API

                }
                else
                {
                    idCardPayment.ReferenceNo = result.Rrr;
                    idCardPayment.IsPayed = false;
                    idCardPayment.TransactionMessage = result.Message;
                    _db.Entry(idCardPayment).State = EntityState.Modified;

                    _query.UpdateTransactionLog(log, result);
                    await _db.SaveChangesAsync();
                    return RedirectToAction("RetryIdCardPayment", new { rrr = result.Rrr });
                }
                return RedirectToAction("Index");
            }
            var message = $"There is no payment that has either the RRR {RRR} or" +
                          $" Order Id {orderID} for Id Application Payment";
            return RedirectToAction("GetPaymentStatus", "RemitaServices", new { message });
        }

        [AllowAnonymous]
        public ActionResult RetryIdCardPayment(string rrr)
        {
            var hashrrr = _query.HashRrrQuery(rrr, RemitaConfigParams.APIKEY, RemitaConfigParams.MERCHANTID);
            string posturl = RemitaConfigParams.CHECKSTATUSURL + "/" + RemitaConfigParams.MERCHANTID + "/" + rrr + "/" + hashrrr + "/" + "status.reg";
            string jsondata = new WebClient().DownloadString(posturl);
            var result = JsonConvert.DeserializeObject<RemitaResponse>(jsondata);

            if (result.Status.Equals("00") || result.Status.Equals("01"))
            {
                return RedirectToAction("ConfrimIdCardPayment", "IdCardPayments", new { RRR = result.Rrr, orderID = result.OrderId });
            }
            var hash = _query.HashRemitedRePost(RemitaConfigParams.MERCHANTID, rrr, RemitaConfigParams.APIKEY);
            var url = Url.Action("ConfrimIdCardPayment", "IdCardPayments", new { },
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

        public ActionResult ProcessPaymentsForFaculty(int? facultyId, int? sessionId)
        {
           
            ViewBag.SchoolProgrammeId = new SelectList(_db.Faculties.AsNoTracking(), "FacultyId", "FacultyName");
            ViewBag.Session = new SelectList(_db.Sessions.OrderByDescending(s => s.SessionName).AsNoTracking(), "SessionId", "SessionName");

            HttpRequest request = System.Web.HttpContext.Current.Request;

            if (request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {

                long milliseconds = DateTime.Now.Ticks;
                var count = 0;
                string message;

                // Retrieve all students in the given faculty
                var studentsInFaculty = _db.Students.Include(s => s.Programme).Include(s => s.Programme.Department).Include(s => s.Programme.Department.Faculty)
                    .Where(s => s.Programme.Department.Faculty.FacultyId == (Int32)facultyId && s.SessionId == (Int32)sessionId && s.MatricNo != null)
                    .ToList();

                // Iterate through each student
                foreach (var student in studentsInFaculty)
                {
                    // Check if a record already exists for the student and session in IdCardPayment table
                    var existingPayment = _db.IdCardPayments
                        .Any(p => p.StudentId == student.StudentId && p.SessionId == (Int32)sessionId);

                    // If a record already exists, skip processing for this student
                    if (existingPayment)
                        continue;

                    // Check if there's a successful payment for the student and session
                    var successfulPayment = _db.SchoolFeePayments
                        .Include(p => p.Session)
                        .FirstOrDefault(p => p.StudentId == student.StudentId && p.SessionId == sessionId && p.Status);

                    if (successfulPayment != null)
                    {
                        // If a successful payment exists, create a record in the IdCardPayment table
                        var idCardPayment = new IdCardPayment
                        {
                            StudentId = student.StudentId,
                            ReferenceNo = GenerateReferenceNumber(),
                            OrderId = GenerateOrderId(),
                            SessionId = (Int32)sessionId,
                            TotalAmount = 2000, // Provide the appropriate total amount
                            PaymentDateTime = DateTime.Now, // Use appropriate date and time
                            IsPayed = true, // Assuming initially not paid
                            TransactionMessage = "Successful"       // Populate other properties as needed
                        };

                        _db.IdCardPayments.Add(idCardPayment);
                        count++;
                    }
                }

                // Save changes to the database after processing all students
                if (_db.SaveChanges() > 0)
                {
                     message = count + $" records created successfully";
                    return new JsonResult { Data = new { status = true, message } };
                }
                message = count + $" records created successfully";
                return new JsonResult { Data = new { status = true, message } };
            }
            return View();
        }

        private string GenerateReferenceNumber()
        {
            // Your logic to generate a reference number
            return Guid.NewGuid().ToString(); // Example: Generate a GUID
        }

        private string GenerateOrderId()
        {
            // Your logic to generate an order ID
            return Guid.NewGuid().ToString(); // Example: Generate a GUID
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
