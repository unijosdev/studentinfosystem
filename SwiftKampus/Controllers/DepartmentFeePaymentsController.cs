using Newtonsoft.Json;
using Rotativa;
using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampus.ViewModels.Fee_Management;
using SwiftKampusModel;
using SwiftKampusModel.Payment;
using System;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SwiftKampus.Controllers
{
    [Authorize]
    [Audit(AuditingLevel = 2)]
    public class DepartmentFeePaymentsController : BaseController
    {

        public DepartmentFeePaymentsController(SchoolDbContext db) : base(db)
        {

        }


        // GET: DepartmentFeePayments
        public async Task<ActionResult> Index()
        {
            var departmentFeePayments = _db.DepartmentFeePayments.Include(d => d.Department).Include(d => d.Semester).Include(d => d.Session).Include(d => d.Students);
            return View(await departmentFeePayments.ToListAsync());
        }

        // GET: SchoolFeePayments/Create
        [HttpGet]
        public async Task<ActionResult> MakeDepartmentPayment(string message)
        {
            var studentId = userId;
            var student = await _db.Students.Include(i => i.Programme.Department).AsNoTracking()
                .Where(x => x.StudentId.Equals(studentId)).FirstOrDefaultAsync();
            var departments = await _db.Departments.AsNoTracking()
                .Where(x => x.DepartmentId.Equals(student.Programme.Department.DepartmentId)).ToListAsync();
            ViewBag.DepartmentId = new SelectList(departments, "DepartmentId", "DeptName");
            ViewBag.SemesterId = new SelectList(_query.GetCurrentSemesterList(studentSchoolProgrammeId), "SemesterId", "SemesterName");
            ViewBag.SessionId = new SelectList(_query.GetCurrentSessionList(studentSchoolProgrammeId), "SessionId", "SessionName");
            ViewBag.StudentId = student.FullName;
            ViewBag.Message = message;
            return View();
        }

        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DepartmentFeePayment departmentFeePayment = await _db.DepartmentFeePayments.FindAsync(id);
            if (departmentFeePayment == null)
            {
                return HttpNotFound();
            }
            return View(departmentFeePayment);
        }

        // GET: DepartmentFeePayments/Create
        public async Task<ActionResult> Create(DepartmentPaymentVm model)
        {

            var studentId = userId;
            var deptRemitaService = await GetDeptRemitaService(model.DepartmentId);
            var serviceTypeId = deptRemitaService?.ServiceType;
            var haspayedDepartmentFee = await _db.DepartmentFeePayments.AsNoTracking().Where(x => x.StudentId.Equals(studentId) &&
                                            x.SemesterId.Equals(model.SemesterId) && x.SessionId.Equals(model.SessionId))
                                            .FirstOrDefaultAsync();
            if (haspayedDepartmentFee != null && haspayedDepartmentFee.Status)
            {
                return View("Index");
            }
            if (haspayedDepartmentFee != null && haspayedDepartmentFee.Status.Equals(false))
            {
                var hashed = _query.HashRemitedValidate(haspayedDepartmentFee.OrderId, deptRemitaService?.ApiKey, deptRemitaService?.MerchantId);
                string checkurl = RemitaConfigParams.CHECKSTATUSURL + "/" + deptRemitaService?.MerchantId + "/" + haspayedDepartmentFee.OrderId + "/" + hashed + "/" + "orderstatus.reg";
                string jsondata = new WebClient().DownloadString(checkurl);
                var result = JsonConvert.DeserializeObject<RemitaResponse>(jsondata);
                if (string.IsNullOrEmpty(result.Rrr))
                {
                    var entry = _db.Entry(haspayedDepartmentFee);
                    if (entry.State == EntityState.Detached)
                        _db.DepartmentFeePayments.Attach(haspayedDepartmentFee);
                    _db.DepartmentFeePayments.Remove(haspayedDepartmentFee);
                    await _db.SaveChangesAsync();
                }
                else
                {
                    return RedirectToAction("ConfrimPayment", new { orderID = haspayedDepartmentFee.OrderId });
                }
            }
            var student = await _db.Students.AsNoTracking().Include(i => i.Level).Include(i => i.Programme.Department).Where(x => x.StudentId.Equals(studentId))
                                .FirstOrDefaultAsync();

            var paymentList = await _db.DepartmentFeeTypes.AsNoTracking().Include(i => i.Level)
                                            .Where(x => x.DepartmentId.Equals(model.DepartmentId)
                                            //&& x.StudentType.Equals(student.StudentType)
                                            && x.Level.LevelId.Equals(student.Level.LevelId)
                                            && x.SessionId.Equals(model.SessionId))
                                            .ToListAsync();

            var feeList = paymentList.Select(fee => new FeeList
            {
                FeeTypeName = fee.FeeName,
                Amount = fee.Amount,
                Description = fee.Description
            })
                .ToList();
            var confirmPaymentVm = new ConfirmPaymentVm
            {
                FeeLists = feeList,
                StudentName = $"{student.LastName} {student.FirstName} {student.MiddleName}",
                StudentId = studentId,
                TotalAmount = paymentList.Sum(s => s.Amount),
                SessionId = model.SessionId,
                SemesterId = model.SemesterId,
                PaymentId = model.DepartmentId,
                SemesterName = await _db.Semesters.Where(x => x.SemesterId.Equals(model.SemesterId))
                    .Select(s => s.SemesterName).FirstOrDefaultAsync(),
                SessionName = await _db.Sessions.Where(x => x.SessionId.Equals(model.SessionId))
                    .Select(s => s.SessionName).FirstOrDefaultAsync(),
                payerName = $"{student.LastName} {student.FirstName} {student.MiddleName}",
                payerEmail = student?.Email ?? $"{student.LastName} {student.FirstName}@uniben.edu",
                payerPhone = student.PhoneNumber ?? "0700000000",
                amt = paymentList.Sum(s => s.Amount).ToString(CultureInfo.InvariantCulture)
            };

            confirmPaymentVm.TotalAmount = paymentList.Sum(s => s.Amount);
            confirmPaymentVm.merchantId = deptRemitaService?.MerchantId;
            confirmPaymentVm.apiKey = deptRemitaService?.ApiKey;
            confirmPaymentVm.orderId = $"UNIBENDF{DateTime.Now.Ticks}";
            confirmPaymentVm.responseurl = Url.Action("ConfrimPayment", "DepartmentFeePayments", new { },
                                            protocol: Request.Url.Scheme);
            confirmPaymentVm.serviceTypeId = serviceTypeId;

            if (confirmPaymentVm.TotalAmount < 10)
            {
                return RedirectToAction("MakeDepartmentPayment",
                    new { message = "Payment is not currently set at the moment, Please try again..." });
            }

            return View(confirmPaymentVm);
        }

        private async Task<DepartmentRemitaSetting> GetDeptRemitaService(int model)
        {
            return await _db.DepartmentRemitaSettings.AsNoTracking()
                .Where(x => x.DepartmentId.Equals(model))
                .FirstOrDefaultAsync();
        }

        // POST: DepartmentFeePayments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(ConfirmPaymentVm model)
        {
            if (ModelState.IsValid)
            {
                model.paymenttype = model.RemitaPaymentType.ToString().Replace("_", " ").ToLower();


                if (string.IsNullOrEmpty(model.payerEmail))
                {
                    model.payerEmail = $"{model.payerName}@uniben.edu";
                }
                if (string.IsNullOrEmpty(model.payerPhone))
                {
                    model.payerEmail = "0703000000";
                }

                var deptFeepayment = new DepartmentFeePayment
                {
                    DepartmentId = model.PaymentId,
                    OrderId = model.orderId,
                    Date = DateTime.Now,
                    SemesterId = model.SemesterId,
                    SessionId = model.SessionId,
                    StudentId = model.StudentId,
                    PaidFee = model.TotalAmount,
                    TotalAmount = model.TotalAmount,
                    PaymentMode = model.PaymentMode,

                };
                _db.DepartmentFeePayments.Add(deptFeepayment);
                var log = new RemitaPaymentLog
                {
                    OrderId = model.orderId,
                    PaymentName = model.FeeCategory,
                    PaymentDate = DateTime.Now,
                    Amount = model.TotalAmount.ToString(CultureInfo.InvariantCulture),
                    PayerName = model.StudentName

                };
                _db.RemitaPaymentLogs.Add(log);
                await _db.SaveChangesAsync();
                model.hash = _query.HashRemitaRequest(model.merchantId, model.serviceTypeId, model.orderId, model.amt, model.responseurl, RemitaConfigParams.APIKEY);
                return RedirectToAction("SubmitRemita", model);
            }
            return View(model);
        }


        [AllowAnonymous]
        public ActionResult SubmitRemita(ConfirmPaymentVm model)
        {
            return View(model);
        }
        [AllowAnonymous]
        public async Task<ActionResult> RetryDeptFeePayment(string rrr, int deptId)
        {
            var deptRemitaService = await GetDeptRemitaService(deptId);

            var hashrrr = _query.HashRrrQuery(rrr, deptRemitaService?.ApiKey, deptRemitaService?.MerchantId);
            string posturl = RemitaConfigParams.CHECKSTATUSURL + "/" + deptRemitaService?.MerchantId + "/" + rrr + "/" + hashrrr + "/" + "status.reg";
            string jsondata = new WebClient().DownloadString(posturl);
            var result = JsonConvert.DeserializeObject<RemitaResponse>(jsondata);
            if (result.Status.Equals("00") || result.Status.Equals("01"))
            {
                return RedirectToAction("ConfrimPayment", "DepartmentFeePayments", new { RRR = result.Rrr, orderID = result.OrderId });
            }
            var url = Url.Action("ConfrimPayment", "DepartmentFeePayments", new { }, protocol: Request.Url.Scheme);
            var hash = _query.HashRemitedRePost(RemitaConfigParams.MERCHANTID, rrr, RemitaConfigParams.APIKEY);

            var model = new RemitaRePostVm
            {
                rrr = rrr,
                merchantId = deptRemitaService?.MerchantId,
                hash = hash,
                responseurl = url
            };
            return View(model);
        }


        public async Task<ActionResult> ConfrimPayment(string RRR, string orderID)
        {
            DepartmentFeePayment deptFeePayment;
            RemitaResponse result = new RemitaResponse();
            if (string.IsNullOrEmpty(orderID))
            {
                deptFeePayment = await _db.DepartmentFeePayments.AsNoTracking()
                    .Where(x => x.ReferenceNo.Equals(RRR))
                    .FirstOrDefaultAsync();
            }
            else
            {
                deptFeePayment = await _db.DepartmentFeePayments.AsNoTracking()
                    .Where(x => x.OrderId.Equals(orderID.Trim()))
                    .FirstOrDefaultAsync();
            }
            if (deptFeePayment != null)
            {
                var deptRemitaService = await GetDeptRemitaService(deptFeePayment.DepartmentId);
                if (deptFeePayment.Status.Equals(true))
                {
                    result.Message = deptFeePayment.PaymentStatus;
                    result.OrderId = deptFeePayment.OrderId;
                    result.Rrr = deptFeePayment.ReferenceNo;
                    result.Status = deptFeePayment.Status.ToString();
                    return RedirectToAction("ConfrimRrrPayment", "RemitaPaymentLogs", result);
                }
                var log = await _db.RemitaPaymentLogs.AsNoTracking().Where(x => x.OrderId.Equals(deptFeePayment.OrderId))
                                            .FirstOrDefaultAsync();

                var hashed = _query.HashRemitedValidate(deptFeePayment.OrderId, deptRemitaService?.ApiKey, deptRemitaService?.MerchantId);
                string url = RemitaConfigParams.CHECKSTATUSURL + "/" + deptRemitaService?.MerchantId + "/" + deptFeePayment.OrderId + "/" + hashed + "/" + "orderstatus.reg";
                string jsondata = new WebClient().DownloadString(url);
                result = JsonConvert.DeserializeObject<RemitaResponse>(jsondata);

                if (result.Status.Equals("00") || result.Status.Equals("01"))
                {
                    deptFeePayment.Status = true;
                    deptFeePayment.PaymentStatus = result.Message;
                    deptFeePayment.ReferenceNo = result.Rrr;
                    _db.Entry(deptFeePayment).State = EntityState.Modified;

                    _query.UpdateTransactionLog(log, result);
                    await _db.SaveChangesAsync();
                }
                else
                {
                    deptFeePayment.Status = false;
                    deptFeePayment.PaymentStatus = result.Message;
                    deptFeePayment.ReferenceNo = result.Rrr;
                    _db.Entry(deptFeePayment).State = EntityState.Modified;

                    _query.UpdateTransactionLog(log, result);
                    await _db.SaveChangesAsync();
                    return RedirectToAction("RetryDeptFeePayment", new { rrr = result.Rrr, deptId = deptFeePayment.DepartmentId });

                }

                return RedirectToAction("ConfrimRrrPayment", "RemitaPaymentLogs", result);
            }
            var message = $"There is no payment that has either the RRR {RRR} or" +
                            $" Order Id {orderID} for School fee or Acceptance Fee";
            return RedirectToAction("GetPaymentStatus", "RemitaServices", new { message });
        }

        // GET: DepartmentFeePayments/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DepartmentFeePayment departmentFeePayment = await _db.DepartmentFeePayments.FindAsync(id);
            if (departmentFeePayment == null)
            {
                return HttpNotFound();
            }
            ViewBag.DepartmentId = new SelectList(_db.Departments, "DepartmentId", "DeptCode", departmentFeePayment.DepartmentId);
            ViewBag.SemesterId = new SelectList(_db.Semesters, "SemesterId", "SemesterName", departmentFeePayment.SemesterId);
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName", departmentFeePayment.SessionId);
            ViewBag.StudentId = new SelectList(_db.Students, "StudentId", "StudentType", departmentFeePayment.StudentId);
            return View(departmentFeePayment);
        }

        // POST: DepartmentFeePayments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "DepartmentFeePaymentId,StudentId,DepartmentId,SemesterId,SessionId,PaidFee,TotalAmount,PaymentMode,Date,Remaining,PaymentStatus")] DepartmentFeePayment departmentFeePayment)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(departmentFeePayment).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.DepartmentId = new SelectList(_db.Departments, "DepartmentId", "DeptCode", departmentFeePayment.DepartmentId);
            ViewBag.SemesterId = new SelectList(_db.Semesters, "SemesterId", "SemesterName", departmentFeePayment.SemesterId);
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName", departmentFeePayment.SessionId);
            ViewBag.StudentId = new SelectList(_db.Students, "StudentId", "StudentType", departmentFeePayment.StudentId);
            return View(departmentFeePayment);
        }

        // GET: DepartmentFeePayments/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var departmentFeePayment = await _db.DepartmentFeePayments.FindAsync(id);
            if (departmentFeePayment == null)
            {
                return HttpNotFound();
            }
            return View(departmentFeePayment);
        }

        // POST: DepartmentFeePayments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            DepartmentFeePayment departmentFeePayment = await _db.DepartmentFeePayments.FindAsync(id);
            if (departmentFeePayment != null) _db.DepartmentFeePayments.Remove(departmentFeePayment);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }


        public async Task<ActionResult> PrintReceipt(int id)
        {
            var deptFee = await _db.DepartmentFeePayments.Include(i => i.Semester).Include(i => i.Session).AsNoTracking()
                                        .Where(x => x.DepartmentFeePaymentId.Equals(id)).FirstOrDefaultAsync();
            var deptFeeReciept = new DepartmentReciept
            {
                Student = await _db.Students.AsNoTracking().Include(i => i.Level).Include(i => i.Programme).Include(i => i.Programme.Department)
                                            .AsNoTracking().Where(x => x.StudentId.Equals(deptFee.StudentId))
                                            .FirstOrDefaultAsync()
            };
            deptFeeReciept.DepartmentFeeTypes = await _db.DepartmentFeeTypes.AsNoTracking()
                                                    .Where(x => x.DepartmentId.Equals(deptFee.DepartmentId)
                                                    && x.SchoolProgrammeId.Equals(deptFeeReciept.Student.SchoolProgrammeId)
                                                    && x.Level.LevelId.Equals(deptFeeReciept.Student.Level.LevelId)).ToListAsync();

            deptFeeReciept.DepartmentFeePayment = deptFee;

            //return View(schoolFeePayment);
            return new ViewAsPdf(deptFeeReciept);
        }


        public async Task<ActionResult> DeptFeeReport()
        {
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            ViewBag.SemesterId = new SelectList(await _db.Semesters.AsNoTracking().ToListAsync(), "SemesterId", "SemesterName");
            ViewBag.DepartmentId = new SelectList(await _db.Departments.AsNoTracking().ToListAsync(), "DepartmentId", "DeptName");
            ViewBag.LevelId = new SelectList(await _db.Levels.AsNoTracking().ToListAsync(), "LevelId", "LevelName");
            ViewBag.SchoolProgrammeId = new SelectList(await _db.SchoolProgrammes.AsNoTracking().ToListAsync(), "SchoolProgrammeId", "FancyName");

            return View();
        }

        public async Task<ActionResult> GetDeptFeeReport(int SchoolProgrammeId, int? DepartmentId, int? LevelId, int? SessionId, int SemesterId)
        {
            if (SessionId == null)
            {
                SessionId = _query.GetCurrentSessionId(SchoolProgrammeId);
            }
            var deptPayment = await _db.DepartmentFeePayments.Include(i => i.Students).Include(i => i.Session)
                                .Include(i => i.Students.Programme.Department).Include(i => i.Students.Level)
                                .AsNoTracking()
                                .Where(x => x.Status.Equals(true) 
                                && x.SessionId.Equals((int)SessionId)
                                && x.SemesterId.Equals(SemesterId)
                                && x.Students.SchoolProgrammeId.Equals(SchoolProgrammeId)).ToListAsync();
            if (DepartmentId != null & LevelId != null)
            {
                deptPayment = deptPayment.Where(x => x.Students.Programme.Department.DepartmentId.Equals((int)DepartmentId)
                                                                     && x.Students.Level.LevelId.Equals((int)LevelId)
                                                                     && x.SessionId.Equals((int)SessionId)).ToList();
            }
            else if (DepartmentId != null)
            {
                deptPayment = deptPayment.Where(x => x.Students.Programme.Department.DepartmentId.Equals((int)DepartmentId)
                ).ToList();
            }
            else if (LevelId != null)
            {
                deptPayment = deptPayment.Where(x => x.Students.Level.LevelId.Equals((int)LevelId)).ToList();
            }

            var data = deptPayment.Select(s => new
            {
                s.Students.MatricNo,
                s.Students.FullName,
                s.Students.Gender,
                s.Students.Programme.Department.DeptName,
                s.Students.Programme.ProgrammeName,
                s.Students.Level.LevelName,
                s.Students.PhoneNumber
            }).ToList();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
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
