using Newtonsoft.Json;
using OfficeOpenXml;
using Rotativa;
using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampus.ViewModels;
using SwiftKampusModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SwiftKampus.Controllers
{
    [Authorize]
    [Audit(AuditingLevel = 2)]
    public class SupplementaryListsController : BaseController
    {

        public SupplementaryListsController(SchoolDbContext db) : base(db)
        {

        }
        // GET: SupplementaryLists
        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> GetIndex()
        {
            // dc.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var data = await _db.SupplementaryList.AsNoTracking().Where(x => x.IsAdmitted.Equals(false))
                .Select(s => new
                {
                    s.SupplementaryListId,
                    s.JambRegNo,
                    s.FullName,
                    s.Gender,
                    s.StateOfOrigin,
                    s.JambScore,
                    s.IsAdmitted
                }).ToListAsync();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        public async Task<PartialViewResult> Save(int id)
        {
            var supplementaryList = await _db.SupplementaryList.FindAsync(id);
            return PartialView(supplementaryList);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Save(SupplementaryList model)
        {
            bool status = false;
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                if (model.SupplementaryListId > 0)
                {
                    var supplementaryList = await _db.SupplementaryList.FindAsync(model.SupplementaryListId);
                    if (supplementaryList != null)
                    {
                        supplementaryList.JambRegNo = model.JambRegNo;
                        supplementaryList.FirstName = model.FirstName;
                        supplementaryList.LastName = model.LastName;
                        supplementaryList.OtherName = model.OtherName;
                        supplementaryList.Gender = model.Gender;
                        supplementaryList.StateOfOrigin = model.StateOfOrigin;
                        supplementaryList.JambScore = model.JambScore;
                        supplementaryList.Age = model.Age;
                        supplementaryList.CourseAbbreviation = model.CourseAbbreviation;
                        _db.Entry(supplementaryList).State = EntityState.Modified;
                        await _db.SaveChangesAsync();
                        message = $"{model.CourseAbbreviation} Admission List Updated Successfully...";
                        return new JsonResult { Data = new { status = true, message } };
                    }
                }
                else
                {
                    model.OrderId = DateTime.Now.Ticks.ToString();
                    _db.SupplementaryList.Add(model);
                    await _db.SaveChangesAsync();
                    message = $"{model.FullName} Added Successfully.";
                    return new JsonResult { Data = new { status = true, message } };
                }
            }
            return new JsonResult { Data = new { status, message } };
            //return View(subject);
        }

        //public async Task<ActionResult> AdmitStudent()
        //{
        //    var meritList = await _db.AdmissionLists.Select(s => s.JambRegNo).ToListAsync();
        //    int count = 0;
        //    foreach (var student in meritList)
        //    {
        //        var supplementList = await _db.SupplementaryList.Where(x => x.JambRegNo.Equals(student))
        //                                .FirstOrDefaultAsync();
        //        if (supplementList.IsAdmitted == false)
        //        {
        //            supplementList.IsAdmitted = true;
        //            _db.Entry(supplementList).State = EntityState.Modified;
        //            count += 1;
        //        }
        //        await _db.SaveChangesAsync();
        //        ViewBag.Message = $"{count} Number of Students was Added Successfully";
        //        return View();
        //    }

        //    return View();
        //}




        // GET: SupplementaryLists/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SupplementaryList supplementaryList = await _db.SupplementaryList.FindAsync(id);
            if (supplementaryList == null)
            {
                return HttpNotFound();
            }
            return View(supplementaryList);
        }

        // GET: SupplementaryLists/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: SupplementaryLists/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(SupplementaryList supplementaryList)
        {
            if (ModelState.IsValid)
            {
                _db.SupplementaryList.Add(supplementaryList);
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(supplementaryList);
        }

        // GET: SupplementaryLists/Edit/5
        public async Task<ActionResult> Edit(string jambregNo)
        {
            if (!string.IsNullOrEmpty(jambregNo))
            {
                var supplementaryList = await _db.SupplementaryList.AsNoTracking()
                                    .Where(x => x.JambRegNo.ToUpper().Equals(jambregNo.Trim().ToUpper()))
                                    .FirstOrDefaultAsync();
                if (supplementaryList != null)
                {
                    if (supplementaryList.IsAdmitted.Equals(true))
                    {
                        ViewBag.Message = "You have already been given admission";
                        return View();
                    }
                    return View(supplementaryList);
                }
                ViewBag.Message = "Please check your Jamb reg No and try again... Record Not Found";
                return View();
            }
            return View();
        }

        // POST: SupplementaryLists/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(SupplementaryList model)
        {
            if (ModelState.IsValid)
            {
                long milliseconds = DateTime.Now.Ticks;
                var url = Url.Action("ConfrimSupplementary", "SupplementaryLists", new { },
                                        protocol: Request.Url.Scheme);
                var hasTransaction = await _db.SupplementaryList.AsNoTracking()
                            .Where(x => x.JambRegNo.Equals(model.JambRegNo)).FirstOrDefaultAsync();
                model.OrderId = $"EBSSUP{milliseconds.ToString()}";

                //model.PA = model.RemitaPaymentType.ToString().Replace("_", " ").ToLower();
                if (hasTransaction.ReferenceNo != null)
                {
                    model.Hash = _query.HashRemitaRequest(RemitaConfigParams.MERCHANTID, RemitaConfigParams.SUPPLEMENTARYSERVICETYPE, hasTransaction.OrderId, "5000", url, RemitaConfigParams.APIKEY);
                    return RedirectToAction("ConfrimSupplementary", new { orderID = hasTransaction.OrderId });
                }
                hasTransaction.OrderId = model.OrderId;
                hasTransaction.PaymentDate = DateTime.Now;
                //ReferenceNo = reference
                _db.Entry(hasTransaction).State = EntityState.Modified;

                var log = new RemitaPaymentLog
                {
                    OrderId = model.OrderId,
                    PaymentName = "Supplementary List Fee ",
                    PaymentDate = DateTime.Now,
                    Amount = "5000.00",
                    PayerName = model.FullName

                };
                _db.RemitaPaymentLogs.Add(log);

                await _db.SaveChangesAsync();
                model.Hash = _query.HashRemitaRequest(RemitaConfigParams.MERCHANTID, RemitaConfigParams.SUPPLEMENTARYSERVICETYPE, hasTransaction.OrderId, "5000", url, RemitaConfigParams.APIKEY);
                model.ResponseUrl = url;
                return RedirectToAction("SubmitRemita", model);
            }
            return View(model);
        }

        public ActionResult SubmitRemita(SupplementaryList model)
        {
            var remitaPost = new RemitaPostVm()
            {
                payerName = model.FullName,
                payerEmail = $"{model.FullName}@ebusscreening.com",
                payerPhone = "08000000000",
                amt = "5000",
                hash = model.Hash,
                merchantId = RemitaConfigParams.MERCHANTID,
                orderId = model.OrderId,
                paymenttype = model.RemitaPaymentType.ToString(),
                responseurl = model.ResponseUrl,
                serviceTypeId = RemitaConfigParams.SUPPLEMENTARYSERVICETYPE,
            };
            return View(remitaPost);
        }


        public async Task<ActionResult> ConfrimSupplementary(string RRR, string orderID)
        {
            //var suplementaryList = await _db.SupplementaryList.AsNoTracking()
            //                   .Where(x => x.OrderId.Equals(orderID.Trim()))
            //                   .FirstOrDefaultAsync();
            //var log = await _db.RemitaPaymentLogs.AsNoTracking().Where(x => x.OrderId.Equals(orderID))
            //    .FirstOrDefaultAsync();
            SupplementaryList suplementaryList;
            RemitaResponse result = new RemitaResponse();
            if (string.IsNullOrEmpty(orderID))
            {
                suplementaryList = await _db.SupplementaryList.AsNoTracking()
                                  .Where(x => x.ReferenceNo.Equals(RRR.Trim()))
                                  .FirstOrDefaultAsync();
            }
            else
            {
                suplementaryList = await _db.SupplementaryList.AsNoTracking()
                    .Where(x => x.OrderId.Equals(orderID.Trim()))
                    .FirstOrDefaultAsync();
            }
            if (suplementaryList != null)
            {
                if (suplementaryList.IsPayed.Equals(true))
                {
                    //result.Message = suplementaryList.TransactionMessage;
                    //result.OrderId = suplementaryList.OrderId;
                    //result.Rrr = suplementaryList.ReferenceNo;
                    //result.Status = suplementaryList.IsPayed.ToString();
                    //return RedirectToAction("ConfrimRrrPayment", "RemitaPaymentLogs", result);
                    return RedirectToAction("SucessPage", new { orderId = suplementaryList.OrderId });
                }
                var log = await _db.RemitaPaymentLogs.AsNoTracking().Where(x => x.OrderId.Equals(suplementaryList.OrderId))
                    .FirstOrDefaultAsync();


                var hashed = _query.HashRemitedValidate(suplementaryList.OrderId, RemitaConfigParams.APIKEY, RemitaConfigParams.MERCHANTID);
                string url = RemitaConfigParams.CHECKSTATUSURL + "/" + RemitaConfigParams.MERCHANTID + "/" + suplementaryList.OrderId +
                             "/" + hashed + "/" + "orderstatus.reg";
                string jsondata = new WebClient().DownloadString(url);
                result = JsonConvert.DeserializeObject<RemitaResponse>(jsondata);

                if (result.Status.Equals("00") || result.Status.Equals("01"))
                {
                    suplementaryList.ReferenceNo = result.Rrr;
                    suplementaryList.IsPayed = true;
                    suplementaryList.TransactionMessage = result.Message;
                    _db.Entry(suplementaryList).State = EntityState.Modified;

                    _query.UpdateTransactionLog(log, result);

                    await _db.SaveChangesAsync();
                }
                else
                {
                    suplementaryList.ReferenceNo = result.Rrr;
                    suplementaryList.IsPayed = false;
                    suplementaryList.TransactionMessage = result.Message;
                    _db.Entry(suplementaryList).State = EntityState.Modified;

                    _query.UpdateTransactionLog(log, result);

                    await _db.SaveChangesAsync();
                    return RedirectToAction("RetrySupplementary", new { rrr = result.Rrr });
                }



                return RedirectToAction("SucessPage", new { orderId = orderID });
            }
            var message = $"There is no payment that has either the RRR {RRR} or" +
                          $" Order Id {orderID} for Supplementary List Payment";
            return RedirectToAction("GetPaymentStatus", "RemitaServices", new { message });
        }

        public async Task<ActionResult> SucessPage(string orderId)
        {
            var suplementaryList = await _db.SupplementaryList.AsNoTracking()
                               .Where(x => x.OrderId.Equals(orderId.Trim()))
                               .FirstOrDefaultAsync();
            return View(suplementaryList);
        }

#pragma warning disable CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
        public async Task<ActionResult> RetrySupplementary(string rrr)
#pragma warning restore CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
        {
            //var rrr = await _db.SupplementaryList.AsNoTracking()
            //                   .Where(x => x.OrderId.Equals(orderId.Trim()))
            //                   .Select(s => s.ReferenceNo).FirstOrDefaultAsync();
            var hash = _query.HashRemitedRePost(RemitaConfigParams.MERCHANTID, rrr, RemitaConfigParams.APIKEY);
            var url = Url.Action("ConfrimSupplementary", "SupplementaryLists", new { },
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

        public async Task<ActionResult> PrintReceipt(string id)
        {
            var schoolFee = await _db.SupplementaryList.AsNoTracking()
                .Where(x => x.JambRegNo.Equals(id)).FirstOrDefaultAsync();
            //return View(schoolFeePayment);
            return new ViewAsPdf(schoolFee);
        }



        // GET: SupplementaryLists/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SupplementaryList supplementaryList = await _db.SupplementaryList.FindAsync(id);
            if (supplementaryList == null)
            {
                return HttpNotFound();
            }
            return View(supplementaryList);
        }

        // POST: SupplementaryLists/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            SupplementaryList supplementaryList = await _db.SupplementaryList.FindAsync(id);
            _db.SupplementaryList.Remove(supplementaryList);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public PartialViewResult UploadSupplementaryList()
        {
            return PartialView();
        }


        [HttpPost]
        public async Task<ActionResult> UploadSupplementaryList(HttpPostedFileBase excelfile)
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
                    int requiredField = 8;

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

                        var jambregNo = workSheet.Cells[row, 1].Value.ToString().Trim();
                        var firstName = workSheet.Cells[row, 2].Value.ToString().Trim();
                        var middleName = workSheet.Cells[row, 3].Value.ToString().Trim();
                        var lastName = workSheet.Cells[row, 4].Value.ToString().Trim();
                        var state = workSheet.Cells[row, 5].Value.ToString().Trim();
                        var lga = workSheet.Cells[row, 6].Value.ToString().Trim();
                        var gender = workSheet.Cells[row, 7].Value.ToString().Trim();
                        var age = workSheet.Cells[row, 8].Value.ToString().Trim();
                        var score = workSheet.Cells[row, 9].Value.ToString().Trim();
                        var course = workSheet.Cells[row, 10].Value.ToString().Trim();

                        try
                        {
                            var model = new SupplementaryList()
                            {
                                JambRegNo = jambregNo,
                                FirstName = firstName,
                                LastName = lastName,
                                OtherName = middleName,
                                StateOfOrigin = state,
                                LocalGovtArea = lga,
                                Gender = gender,
                                Age = age,
                                JambScore = score,
                                CourseAbbreviation = course,
                                PaymentDate = DateTime.Now,
                                OrderId = DateTime.Now.Ticks.ToString(),

                            };
                            _db.SupplementaryList.Add(model);
                            recordCount++;
                            lastrecord = $"The last record uploaded has the Name {firstName} {lastName} and Dept Name {course}";
                        }
                        catch (Exception ex)
                        {
                            ViewBag.ErrorInfo = $"Please check row \"{row}\". There is possible Jamb RegNo duplicate, Please check and try again";
                            ViewBag.ErrorMessage = ex.Message;
                            return View("ErrorException");
                        }
                    }
                    await _db.SaveChangesAsync();
                    message = $"You have successfully Uploaded {recordCount} records...  and {lastrecord}";
                    ViewBag.Message = message;

                }
                return RedirectToAction("Index", "SupplementaryLists", new { message });
            }

            ViewBag.Error = $"File type is Incorrect <br/>";
            return View("Index");
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
