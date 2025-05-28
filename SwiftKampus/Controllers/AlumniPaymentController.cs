using Newtonsoft.Json;
using SwiftKampus.Abstractions;
using SwiftKampus.BusinessLogic;
using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampus.ViewModels.ShoppingCart;
using SwiftKampusModel.MarketPlace;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace SwiftKampus.Controllers
{
    public class AlumniPaymentController : BaseController
    {
        IFeeQueryManager _feeQueryManager;
        IStudentQueryManager StudentQuery { get; }

        public AlumniPaymentController(SchoolDbContext db) : base(db)
        {
            _db = db;
            StudentQuery = new StudentQueryManager(_db);
            _feeQueryManager = new FeeQueryManager(_db);
        }

        // GET: AlumniPayment
        public ActionResult Index(int id)
        {
            var model = new CheckoutVm();

            var transaction = _db.Transactions.Include(x => x.Customer).Include(x => x.Order).Where(x => x.Id == id).FirstOrDefault();
            var order = _db.Orders.Include(i => i.Customer).Include(i => i.OrderDetails).AsNoTracking()
                                .FirstOrDefault(x => x.OrderId.Equals(transaction.OrderId));

            var customer = _db.Customers.Where(x => x.Email == transaction.Customer.Email).FirstOrDefault();

            long milliseconds = DateTime.Now.Ticks;
            var url = Url.Action("ConfirmPayment", "AlumniPayment", new { }, protocol: Request.Url.Scheme);
            string serviceTypeId = RemitaConfigParams.SUPPLEMENTARYSERVICETYPE;

            model.payerName = order.Customer.Fullname;
            model.payerEmail = order.Customer.Email;
            model.payerPhone = order.Customer.PhoneNumber;
            model.amt = order.Total.ToString();
            model.merchantId = RemitaConfigParams.MERCHANTID;
            model.orderId = transaction.TransactionOrderId;
            model.responseurl = url;
            model.serviceTypeId = serviceTypeId;
            model.apiKey = RemitaConfigParams.APIKEY;

            model.hash = _query.HashRemitaRequest(model.merchantId, model.serviceTypeId, transaction.TransactionOrderId, model.amt.ToString(), model.responseurl, RemitaConfigParams.APIKEY);

            return View("~/Views/Checkout/SubmitRemita.cshtml", model);
        }


        // This function is hit after successful payment at remita
        public ActionResult ConfirmPayment(string RRR, string orderID)
        {
            Transaction transcriptPayment;
            RemitaResponse result = new RemitaResponse();

            if (string.IsNullOrEmpty(orderID))
            {
                transcriptPayment = _db.Transactions.AsNoTracking().Include(t => t.Customer)
                    .Where(x => x.TransactionReferenceNumber.Equals(RRR))
                    .FirstOrDefault();
            }
            else
            {
                transcriptPayment = _db.Transactions.AsNoTracking().Include( t => t.Customer)
                    .Where(x => x.TransactionOrderId.Equals(orderID.Trim()))
                    .FirstOrDefault();
            }

            if (transcriptPayment != null)
            {
                if (transcriptPayment.HasMadePayment.Equals(true))
                {
                    result.Message = transcriptPayment.TransactionMessage.ToString();
                    result.OrderId = transcriptPayment.TransactionOrderId;
                    result.Rrr = transcriptPayment.TransactionReferenceNumber;
                    result.Status = transcriptPayment.HasMadePayment.ToString();
                    return RedirectToAction("ConfrimRrrPayment", "RemitaPaymentLogs", result);
                }

                var log = _db.RemitaPaymentLogs.AsNoTracking().Where(x => x.OrderId.Equals(transcriptPayment.OrderId.ToString()))
                                            .FirstOrDefault();

                var hashed = _query.HashRemitedValidate(transcriptPayment.TransactionOrderId, RemitaConfigParams.APIKEY, RemitaConfigParams.MERCHANTID);
                string url = RemitaConfigParams.CHECKSTATUSURL + "/" + RemitaConfigParams.MERCHANTID + "/" + transcriptPayment.TransactionOrderId + "/" + hashed + "/" + "orderstatus.reg";
                string jsondata = new WebClient().DownloadString(url);
                result = JsonConvert.DeserializeObject<RemitaResponse>(jsondata);

                if (result.Status.Equals("00") || result.Status.Equals("01"))
                {
                    transcriptPayment.HasMadePayment = true;
                    transcriptPayment.TransactionReferenceNumber = result.Rrr;
                    transcriptPayment.TransactionMessage = result.Message;
                    _db.Entry(transcriptPayment).State = EntityState.Modified;

                    _query.UpdateTransactionLog(log, result);
                    _db.SaveChanges();

                    return RedirectToRoute("Default", new
                    {
                        controller = "Api/AlumniPayment",
                        action = "PaymentResponse",
                        paymentStatus = result.Status,
                        rrr = result.Rrr,
                        paymentDate = result.PaymentDate,
                        amount = result.Amount,
                        email = transcriptPayment.Customer.Email

                    });
                }
                else
                {
                    transcriptPayment.HasMadePayment = false;
                    transcriptPayment.TransactionMessage = result.Message;
                    transcriptPayment.TransactionReferenceNumber = result.Rrr;
                    _db.Entry(transcriptPayment).State = EntityState.Modified;

                    _query.UpdateTransactionLog(log, result);
                    _db.SaveChanges();

                    return RedirectToAction("RetrySchoolFeePayment", new { rrr = result.Rrr });

                }

                //return RedirectToAction("ConfrimRrrPayment", "RemitaPaymentLogs", result);
            }
            var message = $"There is no payment that has either the RRR {RRR} or" +
                            $" Order Id {orderID} for transcript payment";

            return RedirectToAction("NoTranscriptPayment", "Api/AlumniPayment", new { message });
        }

        public ActionResult RetrySchoolFeePayment(string rrr)
        {
            try
            {
                var hashrrr = _query.HashRrrQuery(rrr, RemitaConfigParams.APIKEY, RemitaConfigParams.MERCHANTID);
                string posturl = RemitaConfigParams.CHECKSTATUSURL + "/" + RemitaConfigParams.MERCHANTID + "/" + rrr + "/" + hashrrr + "/" + "status.reg";
                string jsondata = new WebClient().DownloadString(posturl);
                var result = JsonConvert.DeserializeObject<RemitaResponse>(jsondata);

                if (result.Status.Equals("00") || result.Status.Equals("01"))
                {
                    return RedirectToAction("ConfirmPayment", "AlumniPayment", new { RRR = result.Rrr, orderID = result.OrderId });
                }

                var url = Url.Action("ConfirmPayment", "AlumniPayment", new { }, protocol: Request.Url.Scheme);
                var hash = _query.HashRemitedRePost(RemitaConfigParams.MERCHANTID, rrr, RemitaConfigParams.APIKEY);

                var model = new RemitaRePostVm
                {
                    rrr = rrr,
                    merchantId = RemitaConfigParams.MERCHANTID,
                    hash = hash,
                    responseurl = url,

                };

                return View("~/Views/Checkout/RetryProductPayment.cshtml", model);
            }
            catch (Exception)
            {
                return RedirectToAction("GetPaymentStatus", "RemitaServices", new { message = "Please check the RRR supplied and try again" });
                throw;
            }
        }
    }
}