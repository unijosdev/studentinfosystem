//using System.Data.Entity;
//using System.Threading.Tasks;
//using System.Net;
//using System.Web.Mvc;
//using System;
//using SwiftKampus.Models;
//using SwiftKampusModel.Payment;
//using SwiftKampusModel;
//using SwiftKampus.ViewModels.Fee_Management;
//using System.Linq;
//using SwiftKampus.Services;

//namespace SwiftKampus.Controllers
//{
//    public class ChangeDetailPaymentsController : BaseController
//    {
//        public ChangeDetailPaymentsController(SchoolDbContext db) : base(db)
//        {

//        }

//        // GET: ChangeDetailPayments
//        public ActionResult Index()
//        {
//            return View();
//        }

//        public ActionResult MakePayment(string message)
//        {
//            var detailCategory = from DetailPayment s in Enum.GetValues(typeof(DetailPayment))
//                                 select new { ID = s, Name = s.ToString() };
//            ViewBag.ChangeDetailId = new SelectList(_db.ChangeDetailFees.ToList(), "ChangeDetailFeeId", "FancyName");
//            ViewBag.Message = message;
//            return View();
//        }

//        // GET: ChangeDetailPayments/Create
//        //public async Task<ActionResult> Create(int ChangeDetailId)
//        //{
//        //    var student = _studentQuery.GetStudent(userId);
//        //    var hasPayed = _db.ChangeDetailPayments.AsNoTracking().Where(x => x.st)
//        //    if (hasPayedList != null)
//        //    {
//        //        foreach (var hasPayed in hasPayedList)
//        //        {
//        //            if (hasPayed.Status.Equals(false))
//        //            {
//        //                serviceTypeId = _feeQueryManager.GetServiceType(hasPayed.FeeCategory);

//        //                var hashed = _query.HashRemitedValidate(hasPayed.OrderId, RemitaConfigParams.APIKEY, RemitaConfigParams.MERCHANTID);
//        //                string checkurl = RemitaConfigParams.CHECKSTATUSURL + "/" + RemitaConfigParams.MERCHANTID + "/" + hasPayed.OrderId + "/" + hashed + "/" + "orderstatus.reg";
//        //                string jsondata = new WebClient().DownloadString(checkurl);
//        //                var result = JsonConvert.DeserializeObject<RemitaResponse>(jsondata);
//        //                if (string.IsNullOrEmpty(result.Rrr))
//        //                {
//        //                    var entry = _db.Entry(hasPayed);
//        //                    if (entry.State == EntityState.Detached)
//        //                        _db.SchoolFeePayments.Attach(hasPayed);
//        //                    _db.SchoolFeePayments.Remove(hasPayed);
//        //                    await _db.SaveChangesAsync();
//        //                }
//        //                else
//        //                {
//        //                    return RedirectToAction("ConfrimPayment", new { orderID = hasPayed.OrderId });
//        //                }
//        //            }

//        //        }

//        //    }
//        //    var payment = _db.ChangeDetailFees.Find(ChangeDetailId);
//        //    if (payment != null)
//        //    {
//        //        long milliseconds = DateTime.Now.Ticks;
//        //        var url = Url.Action("ConfrimPayment", "ChangeDetailPayments", new { }, protocol: Request.Url.Scheme);

//        //        sessionId = _query.GetCurrentSessionId(student.Programme.ProgrammeId);
//        //        var confirmPaymentVm = new ChangeDetailPaymentVm
//        //        {
//        //            StudentName = student.FullName,
//        //            StudentId = student.StudentId,
//        //            FeeCategory = payment.DetailCategory,
//        //            TotalAmount = payment.Amount,
//        //            SessionId = sessionId,
//        //            SessionName = await _db.Sessions.Where(x => x.SessionId.Equals(sessionId))
//        //                                .Select(s => s.SessionName).FirstOrDefaultAsync(),
//        //            payerName = student.FullName,
//        //            payerEmail = student.Email,
//        //            payerPhone = student.PhoneNumber,
//        //            amt = payment.Amount.ToString()
//        //        };
//        //        confirmPaymentVm.TotalAmount = payment.Amount;
//        //        confirmPaymentVm.merchantId = RemitaConfigParams.MERCHANTID;
//        //        confirmPaymentVm.orderId = $"UTI{milliseconds}";
//        //        confirmPaymentVm.responseurl = url;
//        //        confirmPaymentVm.serviceTypeId = RemitaConfigParams.UTILITY;
//        //    }
//        //    return View();
//        //}

//        // POST: ChangeDetailPayments/Create
//        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
//        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        //public async Task<ActionResult> Create(ConfirmPaymentVm model)
//        //{
//        //    if (ModelState.IsValid)
//        //    {
//        //        _db.ChangeDetailPayments.Add(changeDetailPayment);
//        //        await _db.SaveChangesAsync();
//        //        return RedirectToAction("Index");
//        //    }

//        //    ViewBag.ChangeDetailFeeId = new SelectList(_db.ChangeDetailFees, "ChangeDetailFeeId", "FancyName", changeDetailPayment.ChangeDetailFeeId);
//        //    ViewBag.SessionId = new SelectList(_db.Sessions, "SessionId", "SessionName", changeDetailPayment.SessionId);
//        //    ViewBag.StudentId = new SelectList(_db.Students, "StudentId", "MatricNo", changeDetailPayment.StudentId);
//        //    return View(changeDetailPayment);
//        //}

//        // GET: ChangeDetailPayments/Edit/5
//        public async Task<ActionResult> Edit(int? id)
//        {
//            if (id == null)
//            {
//                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
//            }
//            ChangeDetailPayment changeDetailPayment = await _db.ChangeDetailPayments.FindAsync(id);
//            if (changeDetailPayment == null)
//            {
//                return HttpNotFound();
//            }
//            ViewBag.ChangeDetailFeeId = new SelectList(_db.ChangeDetailFees, "ChangeDetailFeeId", "FancyName", changeDetailPayment.ChangeDetailFeeId);
//            ViewBag.SessionId = new SelectList(_db.Sessions, "SessionId", "SessionName", changeDetailPayment.SessionId);
//            ViewBag.StudentId = new SelectList(_db.Students, "StudentId", "MatricNo", changeDetailPayment.StudentId);
//            return View(changeDetailPayment);
//        }

//        // POST: ChangeDetailPayments/Edit/5
//        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
//        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<ActionResult> Edit([Bind(Include = "ChangeDetailPaymentId,StudentId,ReferenceNo,OrderId,ChangeDetailFeeId,SessionId,TotalAmount,Date,Status,PaymentStatus,IsExpired")] ChangeDetailPayment changeDetailPayment)
//        {
//            if (ModelState.IsValid)
//            {
//                _db.Entry(changeDetailPayment).State = EntityState.Modified;
//                await _db.SaveChangesAsync();
//                return RedirectToAction("Index");
//            }
//            ViewBag.ChangeDetailFeeId = new SelectList(_db.ChangeDetailFees, "ChangeDetailFeeId", "FancyName", changeDetailPayment.ChangeDetailFeeId);
//            ViewBag.SessionId = new SelectList(_db.Sessions, "SessionId", "SessionName", changeDetailPayment.SessionId);
//            ViewBag.StudentId = new SelectList(_db.Students, "StudentId", "MatricNo", changeDetailPayment.StudentId);
//            return View(changeDetailPayment);
//        }

//        // GET: ChangeDetailPayments/Delete/5
//        public async Task<ActionResult> Delete(int? id)
//        {
//            if (id == null)
//            {
//                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
//            }
//            ChangeDetailPayment changeDetailPayment = await _db.ChangeDetailPayments.FindAsync(id);
//            if (changeDetailPayment == null)
//            {
//                return HttpNotFound();
//            }
//            return View(changeDetailPayment);
//        }

//        // POST: ChangeDetailPayments/Delete/5
//        [HttpPost, ActionName("Delete")]
//        [ValidateAntiForgeryToken]
//        public async Task<ActionResult> DeleteConfirmed(int id)
//        {
//            ChangeDetailPayment changeDetailPayment = await _db.ChangeDetailPayments.FindAsync(id);
//            _db.ChangeDetailPayments.Remove(changeDetailPayment);
//            await _db.SaveChangesAsync();
//            return RedirectToAction("Index");
//        }

//        protected override void Dispose(bool disposing)
//        {
//            if (disposing)
//            {
//                _db.Dispose();
//            }
//            base.Dispose(disposing);
//        }
//    }
//}
