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
    public class FacultyFeePaymentsController : BaseController
    {

        public FacultyFeePaymentsController(SchoolDbContext db) : base(db)
        {

        }


        // GET: FacultyFeePayments
        public async Task<ActionResult> Index()
        {
            var facultyFeePayments = _db.FacultyFeePayments.Include(f => f.Faculty)
                            .Include(f => f.Semester).Include(f => f.Session)
                            .Include(f => f.Students).Where(x => x.StudentId.Equals(userId));
            return View(await facultyFeePayments.ToListAsync());
        }

        // GET: FacultyFeePayments/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FacultyFeePayment facultyFeePayment = await _db.FacultyFeePayments.FindAsync(id);
            if (facultyFeePayment == null)
            {
                return HttpNotFound();
            }
            return View(facultyFeePayment);
        }

        // GET: FacultyFeePayments/Create
        [HttpGet]
        public async Task<ActionResult> MakePayment(string message)
        {
            var studentId = userId;
            var student = await _db.Students.Include(i => i.Programme.Department).AsNoTracking()
                                .Where(x => x.StudentId.Equals(studentId)).FirstOrDefaultAsync();
            var faculty = await _db.Faculties.AsNoTracking()
                            .Where(x => x.FacultyId.Equals(student.Programme.Department.FacultyId)).ToListAsync();
            ViewBag.FacultyId = new SelectList(faculty, "FacultyId", "FacultyName");
            ViewBag.SemesterId = new SelectList(_query.GetCurrentSemesterList(studentSchoolProgrammeId), "SemesterId", "SemesterName");
            ViewBag.SessionId = new SelectList(_query.GetCurrentSessionList(studentSchoolProgrammeId), "SessionId", "SessionName");
            ViewBag.StudentId = student.FullName;
            ViewBag.Message = message;
            return View();
        }


        // GET: FacultyFeePayments/Create
        public async Task<ActionResult> Create(FacultyFeePaymentVm model)
        {
            var studentId = userId;
            var facultyRemitaSetting = _db.FacultyRemitaSettings.AsNoTracking()
                                        .FirstOrDefault(x => x.FacultyId.Equals(model.FacultyId));
            if (facultyRemitaSetting == null)
            {
                return RedirectToAction("MakePayment",
                                new { message = "Payment for this Faculty has not been set on the portal" });
            }
            var serviceTypeId = facultyRemitaSetting.ServiceType;
            var hasPayedFacultyFee = await _db.FacultyFeePayments.AsNoTracking().Where(x => x.StudentId.Equals(studentId) &&
                                                    x.SemesterId.Equals(model.SemesterId) && x.SessionId.Equals(model.SessionId))
                                                    .FirstOrDefaultAsync();
            if (hasPayedFacultyFee != null && hasPayedFacultyFee.Status)
            {
                return View("Index");
            }
            if (hasPayedFacultyFee != null && hasPayedFacultyFee.Status.Equals(false))
            {
                var hashed = _query.HashRemitedValidate(hasPayedFacultyFee.OrderId, facultyRemitaSetting.ApiKey, facultyRemitaSetting.MerchantId);
                string checkurl = RemitaConfigParams.CHECKSTATUSURL + "/" + facultyRemitaSetting.MerchantId + "/" + hasPayedFacultyFee.OrderId + "/" + hashed + "/" + "orderstatus.reg";
                string jsondata = new WebClient().DownloadString(checkurl);
                var result = JsonConvert.DeserializeObject<RemitaResponse>(jsondata);
                if (string.IsNullOrEmpty(result.Rrr))
                {
                    var entry = _db.Entry(hasPayedFacultyFee);
                    if (entry.State == EntityState.Detached)
                        _db.FacultyFeePayments.Attach(hasPayedFacultyFee);
                    _db.FacultyFeePayments.Remove(hasPayedFacultyFee);
                    await _db.SaveChangesAsync();
                }
                else
                {
                    return RedirectToAction("ConfrimPayment", new { orderID = hasPayedFacultyFee.OrderId });
                }
            }
            var student = await _db.Students.AsNoTracking().Include(i => i.Level).Where(x => x.StudentId.Equals(studentId))
                                .FirstOrDefaultAsync();

            var paymentList = await _db.FacultyFeeTypes.AsNoTracking().Include(i => i.Level)
                                                    .Where(x => x.FacultyId.Equals(model.FacultyId)
                                                    && x.SchoolProgrammeId.Equals(student.SchoolProgrammeId)
                                                    && x.Level.LevelId.Equals(student.Level.LevelId)
                                                    && x.SemesterId.Equals(semesterId))
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
                StudentId = student.StudentId,
                TotalAmount = paymentList.Sum(s => s.Amount),
                SessionId = model.SessionId,
                SemesterId = model.SemesterId,
                PaymentId = model.FacultyId,
                SemesterName = await _db.Semesters.Where(x => x.SemesterId.Equals(model.SemesterId))
                    .Select(s => s.SemesterName).FirstOrDefaultAsync(),
                SessionName = await _db.Sessions.Where(x => x.SessionId.Equals(model.SessionId))
                    .Select(s => s.SessionName).FirstOrDefaultAsync(),
                payerName = $"{student.LastName} {student.FirstName} {student.MiddleName}",
                payerEmail = student?.Email ?? $"{student.LastName} {student.FirstName}@uniben.edu",
                payerPhone = student.PhoneNumber ?? "0700000000",
                amt = paymentList.Sum(s => s.Amount).ToString(CultureInfo.InvariantCulture)
            };

            confirmPaymentVm.apiKey = facultyRemitaSetting.ApiKey;
            confirmPaymentVm.TotalAmount = paymentList.Sum(s => s.Amount);
            confirmPaymentVm.merchantId = facultyRemitaSetting.MerchantId;
            confirmPaymentVm.orderId = $"UNIBENFF{DateTime.Now.Ticks}";
            confirmPaymentVm.responseurl = Url.Action("ConfrimPayment", "FacultyFeePayments", new { },
                                            protocol: Request.Url.Scheme);
            confirmPaymentVm.serviceTypeId = serviceTypeId;

            if (confirmPaymentVm.TotalAmount < 10)
            {
                return RedirectToAction("MakePayment",
                    new { message = "Payment is not currently set at the moment, Please try again..." });
            }
            ViewBag.Message = await _db.Faculties.AsNoTracking().Where(x => x.FacultyId.Equals(model.FacultyId))
                                    .Select(s => s.FacultyName).FirstOrDefaultAsync();
            return View(confirmPaymentVm);
        }

        // POST: FacultyFeePayments/Create
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

                var facultyFeepaymet = new FacultyFeePayment
                {
                    FacultyId = model.PaymentId,
                    OrderId = model.orderId,
                    Date = DateTime.Now,
                    SemesterId = model.SemesterId,
                    SessionId = model.SessionId,
                    StudentId = model.StudentId,
                    PaidFee = model.TotalAmount,
                    TotalAmount = model.TotalAmount,
                    PaymentMode = model.PaymentMode,

                };
                _db.FacultyFeePayments.Add(facultyFeepaymet);
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
                model.hash = _query.HashRemitaRequest(model.merchantId, model.serviceTypeId, model.orderId, model.amt, model.responseurl, model.apiKey);
                return RedirectToAction("SubmitRemita", model);
            }
            return View(model);
        }

        public async Task<ActionResult> ConfrimPayment(string RRR, string orderID)
        {
            FacultyFeePayment facultyFeePayment;
            RemitaResponse result = new RemitaResponse();
            if (string.IsNullOrEmpty(orderID))
            {
                facultyFeePayment = await _db.FacultyFeePayments.AsNoTracking()
                    .Where(x => x.ReferenceNo.Equals(RRR))
                    .FirstOrDefaultAsync();
            }
            else
            {
                facultyFeePayment = await _db.FacultyFeePayments.AsNoTracking()
                    .Where(x => x.OrderId.Equals(orderID.Trim()))
                    .FirstOrDefaultAsync();
            }
            if (facultyFeePayment != null)
            {
                var facultyRemitaSetting = await GetfacultyRemitaSetting(facultyFeePayment.FacultyId);
                if (facultyFeePayment.Status.Equals(true))
                {
                    result.Message = facultyFeePayment.PaymentStatus;
                    result.OrderId = facultyFeePayment.OrderId;
                    result.Rrr = facultyFeePayment.ReferenceNo;
                    result.Status = facultyFeePayment.Status.ToString();
                    return RedirectToAction("ConfrimRrrPayment", "RemitaPaymentLogs", result);
                }
                var log = await _db.RemitaPaymentLogs.AsNoTracking().Where(x => x.OrderId.Equals(facultyFeePayment.OrderId))
                                            .FirstOrDefaultAsync();

                var hashed = _query.HashRemitedValidate(facultyFeePayment.OrderId, facultyRemitaSetting?.ApiKey, facultyRemitaSetting?.MerchantId);
                string url = RemitaConfigParams.CHECKSTATUSURL + "/" + facultyRemitaSetting?.MerchantId + "/" + facultyFeePayment.OrderId + "/" + hashed + "/" + "orderstatus.reg";
                string jsondata = new WebClient().DownloadString(url);
                result = JsonConvert.DeserializeObject<RemitaResponse>(jsondata);

                if (result.Status.Equals("00") || result.Status.Equals("01"))
                {
                    facultyFeePayment.Status = true;
                    facultyFeePayment.PaymentStatus = result.Message;
                    facultyFeePayment.ReferenceNo = result.Rrr;
                    _db.Entry(facultyFeePayment).State = EntityState.Modified;

                    _query.UpdateTransactionLog(log, result);
                    await _db.SaveChangesAsync();
                }
                else
                {
                    facultyFeePayment.Status = false;
                    facultyFeePayment.PaymentStatus = result.Message;
                    facultyFeePayment.ReferenceNo = result.Rrr;
                    _db.Entry(facultyFeePayment).State = EntityState.Modified;

                    _query.UpdateTransactionLog(log, result);
                    await _db.SaveChangesAsync();
                    return RedirectToAction("RetryFacultyFeePayment", new { rrr = result.Rrr, facultyId = facultyFeePayment.FacultyId });

                }

                return RedirectToAction("ConfrimRrrPayment", "RemitaPaymentLogs", result);
            }
            var message = $"There is no payment that has either the RRR {RRR} or" +
                            $" Order Id {orderID} for School fee or Acceptance Fee";
            return RedirectToAction("GetPaymentStatus", "RemitaServices", new { message });
        }

        private async Task<FacultyRemitaSetting> GetfacultyRemitaSetting(int facultyFeePayment)
        {
            var facultyRemitaSetting = await _db.FacultyRemitaSettings.AsNoTracking()
                .Where(x => x.FacultyId.Equals(facultyFeePayment))
                .FirstOrDefaultAsync();
            return facultyRemitaSetting;
        }
        public ActionResult MyFile()
        {
            return View();
        }


        // GET: FacultyFeePayments/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FacultyFeePayment facultyFeePayment = await _db.FacultyFeePayments.FindAsync(id);
            if (facultyFeePayment == null)
            {
                return HttpNotFound();
            }
            ViewBag.FacultyId = new SelectList(_db.Faculties, "FacultyId", "FacultyCode", facultyFeePayment.FacultyId);
            ViewBag.SemesterId = new SelectList(_db.Semesters, "SemesterId", "SemesterName", facultyFeePayment.SemesterId);
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName", facultyFeePayment.SessionId);
            ViewBag.StudentId = new SelectList(_db.Students, "StudentId", "StudentType", facultyFeePayment.StudentId);
            return View(facultyFeePayment);
        }

        // POST: FacultyFeePayments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "FacultyFeePaymentId,StudentId,FacultyId,SemesterId,SessionId,PaidFee,TotalAmount,PaymentMode,Date,Remaining,PaymentStatus")] FacultyFeePayment facultyFeePayment)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(facultyFeePayment).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.FacultyId = new SelectList(_db.Faculties, "FacultyId", "FacultyCode", facultyFeePayment.FacultyId);
            ViewBag.SemesterId = new SelectList(_db.Semesters, "SemesterId", "SemesterName", facultyFeePayment.SemesterId);
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName", facultyFeePayment.SessionId);
            ViewBag.StudentId = new SelectList(_db.Students, "StudentId", "StudentType", facultyFeePayment.StudentId);
            return View(facultyFeePayment);
        }

        // GET: FacultyFeePayments/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FacultyFeePayment facultyFeePayment = await _db.FacultyFeePayments.FindAsync(id);
            if (facultyFeePayment == null)
            {
                return HttpNotFound();
            }
            return View(facultyFeePayment);
        }

        // POST: FacultyFeePayments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            FacultyFeePayment facultyFeePayment = await _db.FacultyFeePayments.FindAsync(id);
            if (facultyFeePayment != null) _db.FacultyFeePayments.Remove(facultyFeePayment);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> PrintReceipt(int id)
        {
            var facultyFee = await _db.FacultyFeePayments.Include(i => i.Semester).Include(i => i.Session).AsNoTracking()
                .Where(x => x.FacultyFeePaymentId.Equals(id)).FirstOrDefaultAsync();
            var facultyFeeReciept = new FacultyReciept
            {
                Student = await _db.Students.AsNoTracking().Include(i => i.Level).Include(i => i.Programme).Include(i => i.Programme.Department)
                .AsNoTracking().Where(x => x.StudentId.Equals(facultyFee.StudentId))
                .FirstOrDefaultAsync()
            };
            facultyFeeReciept.FacultyFeeTypes = await _db.FacultyFeeTypes.AsNoTracking().Where(x => x.FacultyId.Equals(facultyFee.FacultyId)
                                                        && x.SchoolProgrammeId.Equals(facultyFeeReciept.Student.SchoolProgrammeId)
                                                        && x.Level.LevelId.Equals(facultyFeeReciept.Student.Level.LevelId)).ToListAsync();

            facultyFeeReciept.FacultyFeePayment = facultyFee;

            //return View(schoolFeePayment);
            return new ViewAsPdf(facultyFeeReciept);
        }



        public async Task<ActionResult> FacultyFeeReport()
        {
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            ViewBag.SemesterId = new SelectList(await _db.Semesters.AsNoTracking().ToListAsync(), "SemesterId", "SemesterName");
            ViewBag.DepartmentId = new SelectList(await _db.Departments.AsNoTracking().ToListAsync(), "DepartmentId", "DeptName");
            ViewBag.LevelId = new SelectList(await _db.Levels.AsNoTracking().ToListAsync(), "LevelId", "LevelName");
            ViewBag.SchoolProgrammeId = new SelectList(await _db.SchoolProgrammes.AsNoTracking().ToListAsync(), "SchoolProgrammeId", "FancyName");

            return View();
        }

        public async Task<ActionResult> GetFacultyFeeReport(int SchoolProgrammeId, int? DepartmentId, int? LevelId, int? SessionId, int SemesterId)
        {
            if (SessionId == null)
            {
                SessionId = _query.GetCurrentSessionId(SchoolProgrammeId);
            }
            var facultyPayment = await _db.FacultyFeePayments.Include(i => i.Students).Include(i => i.Session)
                                .Include(i => i.Students.Programme.Department).Include(i => i.Students.Level)
                                .AsNoTracking().Where(x => x.Status.Equals(true)
                                && x.SessionId.Equals((int)SessionId)
                                && x.SemesterId.Equals(SemesterId)
                                && x.Students.SchoolProgrammeId.Equals(SchoolProgrammeId)).ToListAsync();
            if (DepartmentId != null & LevelId != null)
            {
                facultyPayment = facultyPayment.Where(x => x.Students.Programme.Department.DepartmentId.Equals((int)DepartmentId)
                                                                     && x.Students.Level.LevelId.Equals((int)LevelId)
                                                                     && x.SessionId.Equals((int)SessionId)).ToList();
            }
            else if (DepartmentId != null)
            {
                facultyPayment = facultyPayment.Where(x => x.Students.Programme.Department.DepartmentId.Equals((int)DepartmentId)
                ).ToList();
            }
            else if (LevelId != null)
            {
                facultyPayment = facultyPayment.Where(x => x.Students.Level.LevelId.Equals((int)LevelId)).ToList();
            }

            var data = facultyPayment.Select(s => new
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


        [AllowAnonymous]
        public ActionResult SubmitRemita(ConfirmPaymentVm model)
        {
            return View(model);
        }
        [AllowAnonymous]
        public async Task<ActionResult> RetryFacultyFeePayment(string rrr, int facultyId)
        {
            var facultyRemitaSetting = await GetfacultyRemitaSetting(facultyId);
            var hashrrr = _query.HashRrrQuery(rrr, facultyRemitaSetting?.ApiKey, facultyRemitaSetting?.MerchantId);
            string posturl = RemitaConfigParams.CHECKSTATUSURL + "/" + facultyRemitaSetting?.MerchantId + "/" + rrr + "/" + hashrrr + "/" + "status.reg";
            string jsondata = new WebClient().DownloadString(posturl);
            var result = JsonConvert.DeserializeObject<RemitaResponse>(jsondata);
            if (result.Status.Equals("00") || result.Status.Equals("01"))
            {
                return RedirectToAction("ConfrimPayment", "FacultyFeePayments", new { RRR = result.Rrr, orderID = result.OrderId });
            }
            var url = Url.Action("ConfrimPayment", "FacultyFeePayments", new { }, protocol: Request.Url.Scheme);
            var hash = _query.HashRemitedRePost(facultyRemitaSetting?.MerchantId, rrr, facultyRemitaSetting?.ApiKey);

            var model = new RemitaRePostVm
            {
                rrr = rrr,
                merchantId = facultyRemitaSetting?.MerchantId,
                hash = hash,
                responseurl = url
            };
            return View(model);
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
