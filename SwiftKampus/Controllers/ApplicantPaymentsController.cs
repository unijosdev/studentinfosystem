using Newtonsoft.Json;
using OfficeOpenXml;
using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampus.ViewModels;
using SwiftKampusModel;
using SwiftKampusModel.AddmissionApplicant;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SwiftKampus.Controllers
{
    [Authorize]
    [Audit(AuditingLevel = 2)]
    public class ApplicantPaymentsController : BaseController
    {

        public ApplicantPaymentsController(SchoolDbContext db) : base(db)
        {

        }
        // GET: ApplicantPayments
        public ActionResult Index()
        {
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FancyName");
            ViewBag.SessionId = new SelectList(_db.Sessions.AsNoTracking(), "SessionId", "SessionName");
            return View();
        }

        public async Task<ActionResult> GetIndex(int? SchoolProgrammeId, int? SessionId, string PaymentStatus)
        {
            #region Server Side filtering

            //Get parameter for sorting from grid table
            // get Start (paging start index) and length (page size for paging)
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            //Get Sort columns values when we click on Header Name of column
            //getting column name
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            //Soring direction(either desending or ascending)
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
            string search = Request.Form.GetValues("search[value]").FirstOrDefault();

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int totalRecords = 0;

            sessionId = _query.GetCurrentSessionId((int)SchoolProgrammeId);

            var applicantPayment = await _db.ApplicantPayments.AsNoTracking()
                                    .Where(x => x.SchoolProgrammeId.Equals((int)SchoolProgrammeId) &&
                                    x.SessionId.Equals((int)SessionId) /*&& x.IsPayed.Equals(true)*/)
                                    .ToListAsync();
            if (!string.IsNullOrEmpty(PaymentStatus))
            {
                bool hasPayed = Convert.ToBoolean(PaymentStatus);
                applicantPayment = applicantPayment.Where(x => x.IsPayed.Equals(hasPayed)).ToList();
            }
 

            if (!string.IsNullOrEmpty(search))
            {
                applicantPayment = applicantPayment.Where(x => x.ReferenceNo.Trim().ToUpper().Equals(search.ToUpper().Trim()) ||
                                    x.FullName.ToUpper().Contains(search.ToUpper().Trim())
                            || x.ApplicantEmail.Trim().ToUpper().Contains(search.ToUpper().Trim())).ToList();
                
            }
            var model = new List<ApplicantPaymentIndexVm>();

            model = applicantPayment.Select(s => new ApplicantPaymentIndexVm()
            {
                ApplicantEmail = s.ApplicantEmail,
                FullName = s.FullName,
                JambRegNo = s.JambRegNo,
                Amount = s.AmountPayed.ToString(),
                Message = s.TransactionMessage,
                PaymentDate = s.PaymentDateTime.ToString("dd MMM yyyy hh:mm"),
                RRR = s.ReferenceNo
            }).ToList();

            totalRecords = model.Count();
            var data = model.Skip(skip).Take(pageSize).ToList();

            return Json(new { draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data },
                JsonRequestBehavior.AllowGet);

            #endregion Server Side filtering
        }

        public async Task<ActionResult> UpdateApplicantRecord()
        {
            int count = 0;
            var utmeApplicants = await _db.UtmeApplicants.AsNoTracking().ToListAsync();
            var newUtmeApplicants = utmeApplicants.Select(s => new { s.JambRegNo, s.FullName, s.Email }).ToList();
            foreach (var applicant in newUtmeApplicants)
            {
                var applicantPayment = _db.ApplicantPayments.AsNoTracking()
                                    .Where(x => x.ApplicantEmail.Trim().ToUpper().Equals(applicant.Email.Trim().ToUpper()))
                                    .FirstOrDefault();
                if (applicantPayment != null)
                {

                    applicantPayment.FullName = applicant.FullName;
                    applicantPayment.JambRegNo = applicant.JambRegNo;
                    _db.Entry(applicantPayment).State = EntityState.Modified;
                    count += 1;
                }

            }
            await _db.SaveChangesAsync();
            ViewBag.Message = count;
            return View();
        }

        public async Task<ActionResult> Details()
        {
            var model = new ApplicantReceiptVm();
            var utmeApplicant = _db.UtmeApplicants.Include(i => i.SchoolProgramme).Include(i => i.Programme)
                              .AsNoTracking().Where(x => x.Email.Equals(userId))
                              .FirstOrDefault();
            if (utmeApplicant != null)
            {
                var payment = await _db.ApplicantPayments.AsNoTracking().Where(x => x.ApplicantEmail.Equals(userId)
                && x.PaymentDateTime.Year.Equals(DateTime.Now.Year))
                                .FirstOrDefaultAsync();
                model.FullName = utmeApplicant.FullName;
                model.FormName = utmeApplicant.SchoolProgramme.FancyName;
                model.AmountPayed = payment?.AmountPayed.ToString();
                model.Rrr = payment?.ReferenceNo;
                model.PaymentDate = payment.PaymentDateTime.ToString();
                model.Email = utmeApplicant.Email;

            }
            var applicant = await _db.Applicants.Include(i => i.SchoolProgramme).Include(i => i.AvailableCourse)
                                    .AsNoTracking()
                                    .Where(s => s.ApplicantEmail.Equals(userId))
                                    .FirstOrDefaultAsync();
            if (applicant != null)
            {
                var payment = await _db.ApplicantPayments.AsNoTracking().Where(x => x.ApplicantEmail.Equals(userId))
                               .FirstOrDefaultAsync();
                model.FullName = $"{applicant.LastName} {applicant.FirstName}";
                model.FormName = applicant.SchoolProgramme.FancyName;
                model.AmountPayed = payment?.AmountPayed.ToString();
                model.Rrr = payment?.ReferenceNo;
                model.PaymentDate = payment.PaymentDateTime.ToString();
                model.Email = applicant.ApplicantEmail;
            }
            return View(model);
        }

        // GET: ApplicantPayments/Create
        public async Task<ActionResult> Create()
        {
            var applicantPaymentVm = new ApplicantPaymentVm();
            var applicantsetting = new ApplicantFeeSetting();
            var id = "";
            var phoneNumber = "";
            int schoolProgrammeId = 0;


            var utmeApplicant = await _db.UtmeApplicants.Include(i => i.SchoolProgramme).AsNoTracking().Where(x => x.Email.Equals(userId))
                              .Select(s => new { s.SchoolProgrammeId, s.PhoneNumber, s.JambRegNo, s.Email, s.SchoolProgramme, s.SessionId })
                              .FirstOrDefaultAsync();

            if (utmeApplicant != null)
            {
                sessionId = utmeApplicant.SessionId;
                applicantsetting = await _db.ApplicantFeeSettings.Include(i => i.SchoolProgramme).AsNoTracking()
                                            .Where(x => x.SchoolProgrammeId.Equals((int)utmeApplicant.SchoolProgrammeId)
                                            && x.SessionId.Equals(utmeApplicant.SessionId)).FirstOrDefaultAsync();

                id = utmeApplicant.JambRegNo;
                schoolProgrammeId = (int)utmeApplicant.SchoolProgrammeId;
                phoneNumber = utmeApplicant.PhoneNumber;
                if (applicantsetting == null)
                {
                    ViewBag.Message = $"Fee has not been set for this Programme ({utmeApplicant.SchoolProgramme.FancyName})";

                    return View();
                }

            }
            else
            {
                var applicant = await _db.Applicants.Include(i => i.SchoolProgramme).AsNoTracking()
                                   .Where(x => x.ApplicantEmail.Equals(userId))
                                   .Select(s => new { s.SchoolProgrammeId, s.PhoneNumber, s.ApplicantEmail, s.ApplicantId, s.SchoolProgramme, s.SessionId })
                                   .FirstOrDefaultAsync();
               
                if (applicant != null)
                {
                    sessionId = (int)applicant.SessionId;
                    id = applicant.ApplicantEmail;
                    schoolProgrammeId = (int)applicant.SchoolProgrammeId;
                    phoneNumber = applicant.PhoneNumber;

                    applicantsetting = await _db.ApplicantFeeSettings.Include(i => i.SchoolProgramme).AsNoTracking()
                                        .Where(x => x.SchoolProgrammeId.Equals(schoolProgrammeId)
                                        && x.SessionId.Equals(sessionId)).FirstOrDefaultAsync();

                }
                if (applicantsetting == null)
                {
                    ViewBag.Message = $"Fee has not been set for this Programme ({applicant.SchoolProgramme.FancyName})";

                    return View();
                }
            }

            var session = _db.Sessions.FirstOrDefault(x => x.SessionId.Equals(sessionId));


            var hasTransaction = await _db.ApplicantPayments.AsNoTracking().Where(x => x.ApplicantEmail.Equals(userId)
                                       && x.SessionId.Equals(sessionId)).FirstOrDefaultAsync();
            if (hasTransaction != null && hasTransaction.PaymentDateTime.Year.Equals(DateTime.Now.Year))
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
                            _db.ApplicantPayments.Attach(hasTransaction);
                        _db.ApplicantPayments.Remove(hasTransaction);
                        _db.SaveChanges();
                    }
                    else
                    {
                        return RedirectToAction("ConfrimApplicationPayment", new { orderID = hasTransaction.OrderId });
                    }
                }
                else
                {
                    ViewBag.ErrorInfo = $"Check your Internet connection and try again";
                    ViewBag.ErrorMessage = "Remita is currently unreachable";
                    return View("RemitaErrorPage");
                }
            }

            long milliseconds = DateTime.Now.Ticks;
            var url = Url.Action("ConfrimApplicationPayment", "ApplicantPayments", new { },
                                   protocol: Request.Url.Scheme);
            if (applicantsetting != null && applicantsetting.SchoolProgramme.ActiveSale.Equals(true))
            {
                applicantPaymentVm.SchoolProgrammeId = schoolProgrammeId;
                applicantPaymentVm.ApplicantEmail = userId;
                applicantPaymentVm.TotalAmount = applicantsetting.ApplicationFee;
                applicantPaymentVm.payerName = await _query.GetUserFullName(userId);
                applicantPaymentVm.payerEmail = userId;
                applicantPaymentVm.payerPhone = phoneNumber ?? "07030000000";
                applicantPaymentVm.amt = applicantsetting.ApplicationFee.ToString();
                applicantPaymentVm.merchantId = RemitaConfigParams.MERCHANTID;
                applicantPaymentVm.orderId = $"UNIJOSAP{milliseconds.ToString()}";
                applicantPaymentVm.responseurl = url;
                applicantPaymentVm.ApplicantId = id;
                applicantPaymentVm.serviceTypeId = RemitaConfigParams.UTMEAPPLICANTION;
                applicantPaymentVm.ExpectedAmount = applicantsetting.ApplicationFee;
                applicantPaymentVm.SessionId = sessionId;
                applicantPaymentVm.apiKey = RemitaConfigParams.APIKEY;
                applicantPaymentVm.SessionName = session.SessionName;

                ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName", sessionId);
                return View(applicantPaymentVm);
            }
            ViewBag.Message = $"{applicantsetting.SchoolProgramme.FancyName} Form is no longer Active for sales";
            return View();
        }

        (bool, RemitaResponse) CheckExistingTransaction(string checkUrl)
        {
            try
            {
                string jsondata = new WebClient().DownloadString(checkUrl);
                var result = JsonConvert.DeserializeObject<RemitaResponse>(jsondata);
                return (true, result);
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                return (false, null);
            }
        }

        // POST: ApplicantPayments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public async Task<ActionResult> Create(ApplicantPaymentVm model)
        {
            if (ModelState.IsValid)
            {
                var hasTransaction = await _db.ApplicantPayments.AsNoTracking()
                                        .Where(x => x.ApplicantEmail.ToUpper().Trim().Equals(model.ApplicantEmail.ToUpper().Trim())
                                        && x.SessionId.Equals(model.SessionId)).FirstOrDefaultAsync();

                model.paymenttype = model.RemitaPaymentType.ToString().Replace("_", " ").ToLower();
                if (hasTransaction != null)
                {
                    var hashed = _query.HashRemitedValidate(hasTransaction.OrderId, RemitaConfigParams.APIKEY, RemitaConfigParams.MERCHANTID);
                    string checkurl = RemitaConfigParams.CHECKSTATUSURL + "/" + RemitaConfigParams.MERCHANTID + "/" + hasTransaction.OrderId + "/" + hashed + "/" + "orderstatus.reg";
                    string jsondata = new WebClient().DownloadString(checkurl);
                    var result = JsonConvert.DeserializeObject<RemitaResponse>(jsondata);
                    if (string.IsNullOrEmpty(result.Rrr))
                    {
                        var entry = _db.Entry(hasTransaction);
                        if (entry.State == EntityState.Detached)
                            _db.ApplicantPayments.Attach(hasTransaction);
                        _db.ApplicantPayments.Remove(hasTransaction);
                        await _db.SaveChangesAsync();
                    }
                    else
                    {
                        return RedirectToAction("ConfrimApplicationPayment", new { orderID = hasTransaction.OrderId });
                    }
                }

                var applicationPayment = new ApplicantPayment
                {
                    OrderId = model.orderId,
                    PaymentDateTime = DateTime.Now,
                    SessionId = model.SessionId,
                    SchoolProgrammeId = model.SchoolProgrammeId,
                    ApplicantEmail = model.ApplicantEmail,
                    ExpectedAmount = model.ExpectedAmount,
                    AmountPayed = Convert.ToDecimal(model.amt),
                    JambRegNo = model.ApplicantId,
                    FullName = model.payerName,
                };
                _db.ApplicantPayments.Add(applicationPayment);
                var log = new RemitaPaymentLog
                {
                    OrderId = model.orderId,
                    PaymentName = "Applicant Application Fee",
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

        [ValidateInput(false)]
        public ActionResult SubmitRemita(ApplicantPaymentVm model)
        {
            return View(model);
        }


        [AllowAnonymous]
        public async Task<ActionResult> ConfrimApplicationPayment(string RRR, string orderID)
        {
            ApplicantPayment applicationPayment;
            RemitaResponse result = new RemitaResponse();

            if (string.IsNullOrEmpty(orderID))
            {
                applicationPayment = await _db.ApplicantPayments.AsNoTracking()
                                            .Where(x => x.ReferenceNo.Equals(RRR.Trim()))
                                            .FirstOrDefaultAsync();
            }
            else
            {
                applicationPayment = await _db.ApplicantPayments.AsNoTracking()
                                            .Where(x => x.OrderId.Equals(orderID.Trim()))
                                            .FirstOrDefaultAsync();
            }
            if (applicationPayment != null)
            {
                var log = await _db.RemitaPaymentLogs.AsNoTracking()
                                    .Where(x => x.OrderId.Equals(applicationPayment.OrderId))
                                    .FirstOrDefaultAsync();

                var hashed = _query.HashRemitedValidate(orderID, RemitaConfigParams.APIKEY, RemitaConfigParams.MERCHANTID);
                string url = RemitaConfigParams.CHECKSTATUSURL + "/" + RemitaConfigParams.MERCHANTID + "/" + orderID + "/" + hashed + "/" + "orderstatus.reg";
                //string jsondata = new WebClient().DownloadString(url);
                var checkResult = CheckExistingTransaction(url);

                //result = JsonConvert.DeserializeObject<RemitaResponse>(jsondata);
                if (checkResult.Item1.Equals(true))
                {
                    result = checkResult.Item2;

                    if (result.Status.Equals("00") || result.Status.Equals("01"))
                    {
                        applicationPayment.ReferenceNo = result.Rrr;
                        applicationPayment.IsPayed = true;
                        applicationPayment.TransactionMessage = result.Message;
                        _db.Entry(applicationPayment).State = EntityState.Modified;

                        _query.UpdateTransactionLog(log, result);
                        var myEmail = applicationPayment.ApplicantEmail;
                        UpdateApplicantRecord(myEmail);
                        await _db.SaveChangesAsync();
                    }
                    else
                    {
                        applicationPayment.ReferenceNo = result.Rrr;
                        applicationPayment.IsPayed = false;
                        applicationPayment.TransactionMessage = result.Message;
                        _db.Entry(applicationPayment).State = EntityState.Modified;

                        _query.UpdateTransactionLog(log, result);
                        await _db.SaveChangesAsync();
                        return RedirectToAction("RetryApplicationPayment", new { rrr = result.Rrr });
                    }
                }
                else
                {
                    ViewBag.ErrorInfo = $"Check your internet connection and try again";
                    ViewBag.ErrorMessage = "Remita is currently unreachable";
                    return View("RemitaErrorPage");
                }
                return RedirectToAction("ConfrimRrrPayment", "RemitaPaymentLogs", result);
            }
            var message = $"There is no payment that has either the RRR {RRR} or" +
                          $" Order Id {orderID} for Application Payment";
            return RedirectToAction("GetPaymentStatus", "RemitaServices", new { message });
        }

        private void UpdateApplicantRecord(string email)
        {
            var utmeApllicant = _db.UtmeApplicants.AsNoTracking()
                .Where(x => x.Email.Trim().ToUpper().Equals(email.Trim().ToUpper()))
                .FirstOrDefault();
            if (utmeApllicant != null)
            {
                utmeApllicant.HasPayed = true;
                _db.Entry(utmeApllicant).State = EntityState.Modified;

            }
            var applicant = _db.Applicants.AsNoTracking()
                        .Where(x => x.ApplicantEmail.Trim().ToUpper().Equals(email.Trim().ToUpper()))
                        .FirstOrDefault();
            if (applicant != null)
            {
                applicant.HasPayed = true;
                _db.Entry(applicant).State = EntityState.Modified;
            }


        }

        [AllowAnonymous]
        public ActionResult RetryApplicationPayment(string rrr)
        {
            var hashrrr = _query.HashRrrQuery(rrr, RemitaConfigParams.APIKEY, RemitaConfigParams.MERCHANTID);
            string posturl = RemitaConfigParams.CHECKSTATUSURL + "/" + RemitaConfigParams.MERCHANTID + "/" + rrr + "/" + hashrrr + "/" + "status.reg";
            try
            {
                string jsondata = new WebClient().DownloadString(posturl);
                var result = JsonConvert.DeserializeObject<RemitaResponse>(jsondata);
                if (result.Status.Equals("00") || result.Status.Equals("01"))
                {
                    return RedirectToAction("ConfrimApplicationPayment", "ApplicantPayments", new { RRR = result.Rrr, orderID = result.OrderId });
                }
                var hash = _query.HashRemitedRePost(RemitaConfigParams.MERCHANTID, rrr, RemitaConfigParams.APIKEY);
                var url = Url.Action("ConfrimApplicationPayment", "ApplicantPayments", new { },
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

                ViewBag.ErrorInfo = $"Check your internet connection and try again";
                ViewBag.ErrorMessage = "Remita is currently unreachable";
                return View("RemitaErrorPage");
            }

        }

        #region PayStack
        //public async Task<ActionResult> ConfrimPayment(string reference)
        //{
        //    var testOrLiveSecret = ConfigurationManager.AppSettings["PayStackSecret"];
        //    var api = new PayStackApi(testOrLiveSecret);
        //    //Verifying a transaction
        //    var verifyResponse = api.Transactions.Verify(reference); // auto or supplied when initializing;
        //    if (verifyResponse.Status)
        //    {
        //        var convertedValues = new List<SelectableEnumItem>();
        //        var valuepair = verifyResponse.Data.Metadata.Where(x => x.Key.Contains("custom")).Select(s => s.Value);

        //        foreach (var item in valuepair)
        //        {
        //            convertedValues = ((JArray)item).Select(x => new SelectableEnumItem
        //            {
        //                key = (string)x["display_name"],
        //                value = (string)x["value"]
        //            }).ToList();
        //        }
        //        //var studentid = _db.Users.Find(id);
        //        var applicantEmail = convertedValues.Where(x => x.key.Equals("applicantemail")).Select(s => s.value)
        //            .FirstOrDefault();
        //        var applicantPayment = new ApplicantPayment
        //        {
        //            ApplicantEmail = applicantEmail,
        //            Date = DateTime.Now,
        //            SessionId = Convert.ToInt32(convertedValues.Where(x => x.key.Equals("sessionid")).Select(s => s.value).FirstOrDefault()),
        //            AmountPayed = _query.ConvertToNaira(verifyResponse.Data.Amount),
        //            TotalAmount = Convert.ToDecimal(convertedValues.Where(x => x.key.Equals("totalamount")).Select(s => s.value).FirstOrDefault()),
        //            ReferenceKey = reference

        //        };
        //        _db.ApplicantPayments.Add(applicantPayment);
        //        _db.SaveChanges();
        //        var applicant = _db.Applicants.AsNoTracking()
        //                            .FirstOrDefault(x => x.ApplicantEmail.Equals(applicantEmail));
        //        applicant.HasPayed = true;

        //        _db.Entry(applicant).State = EntityState.Modified;
        //        await _db.SaveChangesAsync();
        //    }

        //    return RedirectToAction("Index");
        //} 
        #endregion


        // GET: ApplicantPayments/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicantPayment applicantPayment = await _db.ApplicantPayments.FindAsync(id);
            if (applicantPayment == null)
            {
                return HttpNotFound();
            }
            ViewBag.ApplicantEmail = new SelectList(_db.Applicants, "ApplicantEmail", "ApplicantId", applicantPayment.ApplicantEmail);
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName", applicantPayment.SessionId);
            return View(applicantPayment);
        }

        // POST: ApplicantPayments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "ApplicantEmail,ReferenceKey,AmountPayed,SessionId,TotalAmount,Date,Remaining,Status,PaymentStatus")] ApplicantPayment applicantPayment)
        {
            if (ModelState.IsValid)
            {
                ApplicantPayment applicantPaymentCheck = await _db.ApplicantPayments.FindAsync(9405);
                applicantPaymentCheck.ApplicantEmail = applicantPayment.ApplicantEmail + "2018/2019";
                _db.Entry(applicantPaymentCheck).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.ApplicantEmail = new SelectList(_db.Applicants, "ApplicantEmail", "ApplicantId", applicantPayment.ApplicantEmail);
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName", applicantPayment.SessionId);
            return View(applicantPayment);
        }

        // GET: ApplicantPayments/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //ApplicantPayment applicantPayment = await _db.ApplicantPayments.FindAsync(id);
            var applicantPayment = await _db.ApplicantPayments.Where(x => x.ApplicantEmail.ToUpper().Equals(id.ToUpper())).ToListAsync();
            if (applicantPayment == null)
            {
                return HttpNotFound();
            }
            return View(applicantPayment);
        }

        public async Task DownloadReport(int? SchoolProgrammeId, string PaymentStatus)
        {
            sessionId = _query.GetCurrentSessionId((int)SchoolProgrammeId);

            var applicantPayment = await _db.ApplicantPayments.AsNoTracking()
                                    .Where(x => x.SchoolProgrammeId.Equals((int)SchoolProgrammeId) &&
                                    x.SessionId.Equals(sessionId) && x.IsPayed.Equals(true))
                                    .OrderByDescending(x => x.PaymentDateTime).ToListAsync();
            if (!string.IsNullOrEmpty(PaymentStatus))
            {
                bool hasPayed = Convert.ToBoolean(PaymentStatus);
                applicantPayment = applicantPayment.Where(x => x.IsPayed.Equals(hasPayed)).ToList();
            }
            var model = new List<ApplicantPaymentIndexVm>();
            model = applicantPayment.Select(s => new ApplicantPaymentIndexVm()
            {
                ApplicantEmail = s.ApplicantEmail,
                FullName = s.FullName,
                JambRegNo = s.JambRegNo,
                Amount = s.AmountPayed.ToString(),
                Message = s.TransactionMessage,
                PaymentDate = s.PaymentDateTime.ToString("dd MMM yyyy hh:mm"),
                RRR = s.ReferenceNo
            }).ToList();

            char c1 = 'A';
            ExcelPackage package = new ExcelPackage();
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Report");

            worksheet.Cells[$"{c1++}1"].Value = "Jamb Reg No";
            worksheet.Cells[$"{c1++}1"].Value = "FullName";
            worksheet.Cells[$"{c1++}1"].Value = "Email";
            worksheet.Cells[$"{c1++}1"].Value = "RRR";
            worksheet.Cells[$"{c1++}1"].Value = "Amount";
            worksheet.Cells[$"{c1++}1"].Value = "Payment Date";

            int rowStart = 2;
            //char c2 = 'A';

            for (var i = 0; i < model.Count; i++)
            {

                worksheet.Cells[$"A{rowStart}"].Value = model[i].JambRegNo;
                worksheet.Cells[$"B{rowStart}"].Value = model[i].FullName;
                worksheet.Cells[$"C{rowStart}"].Value = model[i].ApplicantEmail;
                worksheet.Cells[$"D{rowStart}"].Value = model[i].RRR;
                worksheet.Cells[$"E{rowStart}"].Value = model[i].Amount;
                worksheet.Cells[$"F{rowStart}"].Value = model[i].PaymentDate;

                rowStart++;
            }

            worksheet.Cells["A:AZ"].AutoFitColumns();
            worksheet.Column(1).Style.Locked = true;
            worksheet.Column(2).Style.Locked = true;
            worksheet.Column(3).Style.Locked = true;
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment: filename=" +
                                                      $"ApplicantPayment.xlsx");
            Response.BinaryWrite(package.GetAsByteArray());
            Response.End();
        }

        // POST: ApplicantPayments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            ApplicantPayment applicantPayment = await _db.ApplicantPayments.FindAsync(id);
            _db.ApplicantPayments.Remove(applicantPayment);
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
