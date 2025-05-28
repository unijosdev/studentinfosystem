using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampus.ViewModels;
using SwiftKampusModel;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SwiftKampus.Controllers
{
    [Authorize]
    [Audit(AuditingLevel = 2)]
    public class RemitaPaymentLogsController : BaseController
    {

        public RemitaPaymentLogsController(SchoolDbContext db) : base(db)
        {

        }

        // GET: RemitaPaymentLogs
        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> GetIndex()
        {
            // dc.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var data = await _db.RemitaPaymentLogs.AsNoTracking().Select(s => new
            {
                s.OrderId,
                s.Rrr,
                s.Amount,
                s.PayerName,
                s.PaymentName,
                s.TransactionMessage,
                s.RemitaPaymentLogId,
                s.PaymentDate,
            }).OrderByDescending(s => s.PaymentDate).ToListAsync();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult ConfirmationPage()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ConfirmationPage(ConfirmRrr model)
        {
            var hash = _query.HashRemitedRePost(RemitaConfigParams.MERCHANTID, model.rrr, RemitaConfigParams.APIKEY);
            var url = Url.Action("ConfrimRrrPayment", "RemitaPaymentLogs", new { },
                protocol: Request.Url.Scheme);
            var remitaRePostVm = new RemitaRePostVm
            {
                rrr = model.rrr,
                merchantId = RemitaConfigParams.MERCHANTID,
                hash = hash,
                responseurl = url
            };
            return RedirectToAction("SubmitConfirmationPage", remitaRePostVm);
        }

        public ActionResult SubmitConfirmationPage(RemitaRePostVm model)
        {
            return View(model);
        }

        public ActionResult ConfrimRrrPayment(RemitaResponse model)
        {
            return View(model);
        }

        //public ActionResult ConfrimRrrPayment(string RRR, string orderID)
        //{

        //    var hashed = _query.HashRemitedValidate(orderID, RemitaConfigParams.APIKEY, RemitaConfigParams.MERCHANTID);
        //    string url = RemitaConfigParams.CHECKSTATUSURL + "/" + RemitaConfigParams.MERCHANTID + "/" + orderID + "/" + hashed + "/" + "orderstatus.reg";
        //    string jsondata = new WebClient().DownloadString(url);
        //    RemitaResponse result = JsonConvert.DeserializeObject<RemitaResponse>(jsondata);
        //    return View(result);
        //}
        // GET: RemitaPaymentLogs/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RemitaPaymentLog remitaPaymentLog = await _db.RemitaPaymentLogs.FindAsync(id);
            if (remitaPaymentLog == null)
            {
                return HttpNotFound();
            }
            return View(remitaPaymentLog);
        }

        // GET: RemitaPaymentLogs/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: RemitaPaymentLogs/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "RemitaPaymentLogId,OrderId,Amount,Rrr,StatusCode,TransactionMessage,PaymentDate,PaymentName,PayerName")] RemitaPaymentLog remitaPaymentLog)
        {
            if (ModelState.IsValid)
            {
                _db.RemitaPaymentLogs.Add(remitaPaymentLog);
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(remitaPaymentLog);
        }

        // GET: RemitaPaymentLogs/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RemitaPaymentLog remitaPaymentLog = await _db.RemitaPaymentLogs.FindAsync(id);
            if (remitaPaymentLog == null)
            {
                return HttpNotFound();
            }
            return View(remitaPaymentLog);
        }

        // POST: RemitaPaymentLogs/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "RemitaPaymentLogId,OrderId,Amount,Rrr,StatusCode,TransactionMessage,PaymentDate,PaymentName,PayerName")] RemitaPaymentLog remitaPaymentLog)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(remitaPaymentLog).State = System.Data.Entity.EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(remitaPaymentLog);
        }

        // GET: RemitaPaymentLogs/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RemitaPaymentLog remitaPaymentLog = await _db.RemitaPaymentLogs.FindAsync(id);
            if (remitaPaymentLog == null)
            {
                return HttpNotFound();
            }
            return View(remitaPaymentLog);
        }

        // POST: RemitaPaymentLogs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            RemitaPaymentLog remitaPaymentLog = await _db.RemitaPaymentLogs.FindAsync(id);
            _db.RemitaPaymentLogs.Remove(remitaPaymentLog);
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
