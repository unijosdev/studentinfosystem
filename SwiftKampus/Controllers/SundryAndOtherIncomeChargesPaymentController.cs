using Newtonsoft.Json;
using Rotativa;
using SwiftKampus.Controllers;
using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampus.ViewModels;
using SwiftKampus.ViewModels.ProcessChangeOfCourse;
using SwiftKampusModel;
using SwiftKampusModel.AddmissionApplicant;
using SwiftKampusModel.ChangeOfCourse;
using SwiftKampusModel.Payment;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SwiftKampus.Abstractions.Controllers
{
    [Authorize]
    [Audit(AuditingLevel = 2)]
    public class SundryAndOtherIncomeChargesPaymentController : BaseController
    {
        public SundryAndOtherIncomeChargesPaymentController(SchoolDbContext db) : base(db)
        {

        }
        // GET: SundryAndOtherIncomeChargesPayment
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult MakePayment(int? sundryIncomeItemId)
        {
            ViewBag.SundryItems = new SelectList(_db.SundryAndOtherIncomeCharges.AsNoTracking(), "Id", "ChargeDescription", (int)sundryIncomeItemId);

            //ViewBag.ChangeOfCourseType = new SelectList(changeOfCoureType, "Name", "Name");
            return View();
        }

        public async Task<ActionResult> Create(int ChangeOfCourseType)
        {
            var student = _studentQuery.GetStudent(userId);
            var sundryPaymentVm = new SundryAndOtherIncomePaymentVm();
            var applicantsetting = new SundryAndOtherIncomeCharge();
            var id = "";
            var phoneNumber = "";

            if (ChangeOfCourseType > 0 )
            {
                applicantsetting = _db.SundryAndOtherIncomeCharges.AsNoTracking().Where(x => x.Id.Equals(ChangeOfCourseType)
                                        /*&& x..Equals(sessionId)*/).FirstOrDefault();
                if (applicantsetting == null)
                {
                    ViewBag.Message = $"Fee has not been set for this Programme ({student.SchoolProgramme.FancyName})";
                    return View();
                }
            }
            else
            {
                ViewBag.Message = $"Please select the change of course type";
                return View();
            }

            var hasTransactionProcessed = await _db.SundryAndOtherIncomeChargesPayments.AsNoTracking()
                                     .Where(x => x.ApplicationUserId.Equals(student.StudentId)
                                     /*&& x.SessionId.Equals(sessionId)*/ && x.IsPayed.Equals(true)
                                     && x.SundryAndOtherIncomeChargeId.Equals(ChangeOfCourseType)
                                     && x.IsProcessed.Equals(false)).FirstOrDefaultAsync();
            if (hasTransactionProcessed != null)
            {
                return RedirectToAction("Index");
            }

            var hasTransaction = await _db.SundryAndOtherIncomeChargesPayments.AsNoTracking()
                                     .Where(x => x.ApplicationUserId.Equals(student.StudentId)
                                     && /*x.SessionId.Equals(sessionId)*/  x.IsPayed.Equals(false)
                                     && x.SundryAndOtherIncomeChargeId.Equals(ChangeOfCourseType)).FirstOrDefaultAsync();
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
                            _db.SundryAndOtherIncomeChargesPayments.Attach(hasTransaction);
                        _db.SundryAndOtherIncomeChargesPayments.Remove(hasTransaction);
                        _db.SaveChanges();
                    }
                    else
                    {
                        return RedirectToAction("ConfrimApplicationPayment", new { orderID = hasTransaction.OrderId });
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
            var url = Url.Action("ConfrimApplicationPayment", "ChangeOfCoursePayments", new { },
                                   protocol: Request.Url.Scheme);
            if (applicantsetting != null)
            {
                sundryPaymentVm.FullName = student.FullName;
                sundryPaymentVm.TotalAmount = applicantsetting.Amount;
                sundryPaymentVm.payerName = await _query.GetUserFullName(userId);
                sundryPaymentVm.payerEmail = userId;
                sundryPaymentVm.payerPhone = phoneNumber ?? "07030000000";
                sundryPaymentVm.amt = applicantsetting.AmountInWords.ToString();
                sundryPaymentVm.merchantId = RemitaConfigParams.MERCHANTID;
                sundryPaymentVm.orderId = $"UJCOC{milliseconds.ToString()}";
                sundryPaymentVm.responseurl = url;
                sundryPaymentVm.ApplicationUserId = student.StudentId;
                sundryPaymentVm.serviceTypeId = RemitaConfigParams.SUPPLEMENTARYSERVICETYPE;
                sundryPaymentVm.ExpectedAmount = applicantsetting.Amount;
                sundryPaymentVm.PaymentDate = DateTime.Now.ToString();
                sundryPaymentVm.apiKey = RemitaConfigParams.APIKEY;
                sundryPaymentVm.SundryIncomeItemId = ChangeOfCourseType;
                //sundryPaymentVm.SessionName = _query.GetCurrentSessionName(applicantsetting.SchoolProgrammeId);

                var changeOfCoureType = from ChangeOfCourseType s in Enum.GetValues(typeof(ChangeOfCourseType))
                                        select new { ID = s, Name = s.ToString() };

                ViewBag.SundryItems = new SelectList(_db.SundryAndOtherIncomeCharges.AsNoTracking(), "Id", "ChargeDescription", (int)ChangeOfCourseType);

                return View(sundryPaymentVm);
            }
            //ViewBag.Message = $"{applicantsetting.SchoolProgramme.FancyName} Form is no longer Active for sales";
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
        public async Task<ActionResult> Create(SundryAndOtherIncomePaymentVm model)
        {
            if (ModelState.IsValid)
            {
                var StudentId = model.ApplicationUserId;
                var hasTransaction = await _db.SundryAndOtherIncomeChargesPayments.AsNoTracking()
                                        .Where(x => x.ApplicationUserId.Equals(StudentId)
                                        && x.SundryAndOtherIncomeChargeId.Equals(model.SundryIncomeItemId)).FirstOrDefaultAsync();

                model.paymenttype = model.RemitaPaymentType.ToString().Replace("_", " ").ToLower();
                if (hasTransaction != null)
                {
                    model.hash = _query.HashRemitaRequest(model.merchantId, model.serviceTypeId, hasTransaction.OrderId, model.amt, model.responseurl, RemitaConfigParams.APIKEY);
                    return RedirectToAction("ConfrimApplicationPayment", new { orderID = hasTransaction.OrderId });
                }
                var applicationPayment = new SundryAndOtherIncomeChargesPayment
                {
                    OrderId = model.orderId,
                    PaymentDate = DateTime.Now.ToString(),
                    //SessionId = model.SessionId,
                    ApplicationUserId = model.ApplicationUserId,
                    AmountPayment = Convert.ToDecimal(model.TotalAmount),
                    SundryAndOtherIncomeChargeId = model.SundryIncomeItemId
                    //ReferenceNo = reference
                };
                _db.SundryAndOtherIncomeChargesPayments.Add(applicationPayment);
                var log = new RemitaPaymentLog
                {
                    OrderId = model.orderId,
                    PaymentName = await _db.SundryAndOtherIncomeCharges.Where(s => s.Id.Equals(model.SundryIncomeItemId)).Select(x => x.ChargeName).FirstOrDefaultAsync(),
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

        public ActionResult SubmitRemita(ApplicantPaymentVm model)
        {
            return View(model);
        }
    }
}