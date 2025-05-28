using Newtonsoft.Json;
using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampus.ViewModels;
using SwiftKampusModel;
using SwiftKampusModel.Accomodation;
using SwiftKampusModel.AddmissionApplicant;
using SwiftKampusModel.Payment;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SwiftKampus.Controllers
{
    public class RemitaServicesController : BaseController
    {

        public RemitaServicesController(SchoolDbContext db) : base(db)
        {

        }

        public ActionResult GetPaymentStatus(string message)
        {
            ViewBag.Message = message;
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> GetPaymentStatus(ConfirmRrr model)
        {
            if (!string.IsNullOrEmpty(model.rrr))
            {

                if (model.ServiceType.Equals(ServiceType.School_Charges) || model.ServiceType.Equals(ServiceType.Accepatance_Fee))
                {
                    return RedirectToAction("RetrySchoolFeePayment", "SchoolFeePayments", new { rrr = model.rrr.Trim() });
                }
                if (model.ServiceType.Equals(ServiceType.Hostel_Application_Fee))
                {
                    return RedirectToAction("RetryHostelApplication", "AssignedRooms", new { rrr = model.rrr.Trim() });
                }

                if (model.ServiceType.Equals(ServiceType.Acommodation_Payment_Fee))
                {
                    return RedirectToAction("RetryAccomodationPayment", "AssignedRooms", new { rrr = model.rrr.Trim() });
                }
                if (model.ServiceType.Equals(ServiceType.Supplementary_List_Payment))
                {
                    return RedirectToAction("RetrySupplementary", "SupplementaryLists", new { rrr = model.rrr.Trim() });
                }
                if (model.ServiceType.Equals(ServiceType.Admission_Application))
                {
                    return RedirectToAction("RetryApplicationPayment", "ApplicantPayments", new { rrr = model.rrr.Trim() });
                }
                var checkDeptPayment = await _db.DepartmentFeePayments.Where(x => x.ReferenceNo.Equals(model.rrr))
                    .FirstOrDefaultAsync();
                var checkfacultyPayment = await _db.FacultyFeePayments.Where(x => x.ReferenceNo.Equals(model.rrr))
                    .FirstOrDefaultAsync();
                if (checkfacultyPayment != null)
                {
                    return RedirectToAction("RetryFacultyFeePayment", "FacultyFeePayments", new { rrr = model.rrr.Trim(), facultyId = checkfacultyPayment.FacultyId });
                }
                if (checkDeptPayment != null)
                {
                    return RedirectToAction("RetryDeptFeePayment", "DepartmentFeePayments", new { rrr = model.rrr.Trim(), deptId = checkDeptPayment.DepartmentId });

                }
            }
            ViewBag.Message = "RRR cannot be empty or the selected Category is not applicable yet ";
            return View();
        }


        // GET: RemitaServices
        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> PaymentNotification(List<RemitaNotificationVm> item)
        {
            if (ModelState.IsValid)
            {
                foreach (var model in item)
                {
                    if (!string.IsNullOrEmpty(model.orderRef) && !string.IsNullOrEmpty(model.rrr)
                        && !string.IsNullOrEmpty(model.serviceTypeId))
                    {
                        if (model.serviceTypeId.Equals(RemitaConfigParams.SCHOOLFEESERVICETYPE)
                            || model.serviceTypeId.Equals(RemitaConfigParams.ACCEPTANCESERVICETYPE))
                        {
                            await ProcessSchoolFee(model.rrr, model.orderRef);
                        }
                        else if (model.serviceTypeId.Equals(RemitaConfigParams.HOSTELAPPLICATIONSERVICETYPE))
                        {
                            await ProcessHostelApplicationFee(model.rrr, model.orderRef);
                        }
                        else if (model.serviceTypeId.Equals(RemitaConfigParams.ACCOMODATIONSERVICETYPE))
                        {
                            await ProcessAccomodationFee(model.rrr, model.orderRef);
                        }
                        else if (model.serviceTypeId.Equals(RemitaConfigParams.UTMEAPPLICANTION))
                        {
                            await ProcessUtmeApplication(model.rrr, model.orderRef);
                        }
                        var checkDeptPayment = await _db.DepartmentFeePayments.Where(x => x.ReferenceNo.Equals(model.rrr))
                                                .FirstOrDefaultAsync();
                        var checkfacultyPayment = await _db.FacultyFeePayments.Where(x => x.ReferenceNo.Equals(model.rrr))
                            .FirstOrDefaultAsync();



                        if (checkfacultyPayment != null)
                        {
                            var facultyServiceTypeId = await _db.FacultyRemitaSettings.AsNoTracking()
                                .Where(x => x.FacultyId.Equals(checkfacultyPayment.FacultyId)).FirstOrDefaultAsync();
                            if (model.serviceTypeId.Equals(facultyServiceTypeId?.ServiceType))
                            {
                                await ProcessFacultyFee(model.rrr, model.orderRef, facultyServiceTypeId);
                            }
                        }
                        if (checkDeptPayment != null)
                        {
                            var deptServiceTypeId = await _db.DepartmentRemitaSettings.AsNoTracking()
                                .Where(x => x.DepartmentId.Equals(checkDeptPayment.DepartmentId))
                                .FirstOrDefaultAsync();
                            if (model.serviceTypeId.Equals(deptServiceTypeId?.ServiceType))
                            {
                                await ProcessDeptFee(model.rrr, model.orderRef, deptServiceTypeId);
                            }

                        }
                        else
                        {
                            return Content("Service Type is not registered on the Portal yet");
                        }

                    }
                }

            }
            return Content("Ok");
        }

        private async Task ProcessDeptFee(string rrr, string orderId, DepartmentRemitaSetting departmentRemita)
        {
            DepartmentFeePayment deptFeePayment;
            if (string.IsNullOrEmpty(orderId))
            {
                deptFeePayment = await _db.DepartmentFeePayments.AsNoTracking()
                    .Where(x => x.ReferenceNo.Equals(rrr))
                    .FirstOrDefaultAsync();
            }
            else
            {
                deptFeePayment = await _db.DepartmentFeePayments.AsNoTracking()
                    .Where(x => x.OrderId.Equals(orderId.Trim()))
                    .FirstOrDefaultAsync();
            }
            if (deptFeePayment != null)
            {
                if (deptFeePayment.Status.Equals(true))
                {

                }
                else
                {
                    var log = await _db.RemitaPaymentLogs.AsNoTracking()
                        .Where(x => x.OrderId.Equals(deptFeePayment.OrderId))
                        .FirstOrDefaultAsync();

                    var hashed = _query.HashRemitedValidate(deptFeePayment.OrderId, departmentRemita?.ApiKey,
                                        departmentRemita?.MerchantId);
                    string url = RemitaConfigParams.CHECKSTATUSURL + "/" + departmentRemita?.MerchantId + "/" +
                                 deptFeePayment.OrderId + "/" + hashed + "/" + "orderstatus.reg";
                    string jsondata = new WebClient().DownloadString(url);
                    RemitaResponse result = JsonConvert.DeserializeObject<RemitaResponse>(jsondata);

                    if (result.Status.Equals("00") || result.Status.Equals("01"))
                    {
                        deptFeePayment.Status = true;
                        deptFeePayment.TransactionMessage = result.Message;
                        deptFeePayment.ReferenceNo = result.Rrr;
                        _db.Entry(deptFeePayment).State = EntityState.Modified;

                        log.Rrr = result.Rrr;
                        log.StatusCode = result.Status;
                        log.TransactionMessage = result.Message;
                        _db.Entry(log).State = EntityState.Modified;
                        await _db.SaveChangesAsync();
                    }
                    else
                    {
                        deptFeePayment.Status = false;
                        deptFeePayment.TransactionMessage = result.Message;
                        deptFeePayment.ReferenceNo = result.Rrr;
                        _db.Entry(deptFeePayment).State = EntityState.Modified;

                        log.Rrr = result.Rrr;
                        log.StatusCode = result.Status;
                        log.TransactionMessage = result.Message;
                        _db.Entry(log).State = EntityState.Modified;
                        await _db.SaveChangesAsync();
                    }
                }


            }
        }

        private async Task ProcessFacultyFee(string rrr, string orderId, FacultyRemitaSetting facultyRemita)
        {
            FacultyFeePayment facultyFeePayment;
            if (string.IsNullOrEmpty(orderId))
            {
                facultyFeePayment = await _db.FacultyFeePayments.AsNoTracking()
                    .Where(x => x.ReferenceNo.Equals(rrr))
                    .FirstOrDefaultAsync();
            }
            else
            {
                facultyFeePayment = await _db.FacultyFeePayments.AsNoTracking()
                    .Where(x => x.OrderId.Equals(orderId.Trim()))
                    .FirstOrDefaultAsync();
            }
            if (facultyFeePayment != null)
            {
                if (facultyFeePayment.Status.Equals(true))
                {

                }
                else
                {
                    var log = await _db.RemitaPaymentLogs.AsNoTracking()
                        .Where(x => x.OrderId.Equals(facultyFeePayment.OrderId))
                        .FirstOrDefaultAsync();

                    var hashed = _query.HashRemitedValidate(facultyFeePayment.OrderId, facultyRemita?.ApiKey,
                                        facultyRemita?.MerchantId);
                    string url = RemitaConfigParams.CHECKSTATUSURL + "/" + facultyRemita?.MerchantId + "/" +
                                 facultyFeePayment.OrderId + "/" + hashed + "/" + "orderstatus.reg";
                    string jsondata = new WebClient().DownloadString(url);
                    RemitaResponse result = JsonConvert.DeserializeObject<RemitaResponse>(jsondata);

                    if (result.Status.Equals("00") || result.Status.Equals("01"))
                    {
                        facultyFeePayment.Status = true;
                        facultyFeePayment.TransactionMessage = result.Message;
                        facultyFeePayment.ReferenceNo = result.Rrr;
                        _db.Entry(facultyFeePayment).State = EntityState.Modified;

                        log.Rrr = result.Rrr;
                        log.StatusCode = result.Status;
                        log.TransactionMessage = result.Message;
                        _db.Entry(log).State = EntityState.Modified;
                        await _db.SaveChangesAsync();
                    }
                    else
                    {
                        facultyFeePayment.Status = false;
                        facultyFeePayment.TransactionMessage = result.Message;
                        facultyFeePayment.ReferenceNo = result.Rrr;
                        _db.Entry(facultyFeePayment).State = EntityState.Modified;

                        log.Rrr = result.Rrr;
                        log.StatusCode = result.Status;
                        log.TransactionMessage = result.Message;
                        _db.Entry(log).State = EntityState.Modified;
                        await _db.SaveChangesAsync();
                    }
                }


            }
        }

        private async Task ProcessSchoolFee(string RRR, string orderID)
        {
            SchoolFeePayment schoofeepayment;
            if (string.IsNullOrEmpty(orderID))
            {
                schoofeepayment = await _db.SchoolFeePayments.AsNoTracking()
                    .Where(x => x.ReferenceNo.Equals(RRR))
                    .FirstOrDefaultAsync();
            }
            else
            {
                schoofeepayment = await _db.SchoolFeePayments.AsNoTracking()
                    .Where(x => x.OrderId.Equals(orderID.Trim()))
                    .FirstOrDefaultAsync();
            }
            if (schoofeepayment != null)
            {
                if (schoofeepayment.Status.Equals(true))
                {

                }
                else
                {
                    var log = await _db.RemitaPaymentLogs.AsNoTracking()
                        .Where(x => x.OrderId.Equals(schoofeepayment.OrderId))
                        .FirstOrDefaultAsync();

                    var hashed = _query.HashRemitedValidate(schoofeepayment.OrderId, RemitaConfigParams.APIKEY,
                        RemitaConfigParams.MERCHANTID);
                    string url = RemitaConfigParams.CHECKSTATUSURL + "/" + RemitaConfigParams.MERCHANTID + "/" +
                                 orderID + "/" + hashed + "/" + "orderstatus.reg";
                    string jsondata = new WebClient().DownloadString(url);
                    RemitaResponse result = JsonConvert.DeserializeObject<RemitaResponse>(jsondata);

                    if (result.Status.Equals("00") || result.Status.Equals("01"))
                    {
                        schoofeepayment.Status = true;
                        schoofeepayment.PaymentStatus = result.Message;
                        schoofeepayment.ReferenceNo = result.Rrr;
                        _db.Entry(schoofeepayment).State = EntityState.Modified;

                        log.Rrr = result.Rrr;
                        log.StatusCode = result.Status;
                        log.TransactionMessage = result.Message;
                        _db.Entry(log).State = EntityState.Modified;
                        await _db.SaveChangesAsync();
                    }
                    else
                    {
                        schoofeepayment.Status = false;
                        schoofeepayment.PaymentStatus = result.Message;
                        schoofeepayment.ReferenceNo = result.Rrr;
                        _db.Entry(schoofeepayment).State = EntityState.Modified;

                        log.Rrr = result.Rrr;
                        log.StatusCode = result.Status;
                        log.TransactionMessage = result.Message;
                        _db.Entry(log).State = EntityState.Modified;
                        await _db.SaveChangesAsync();
                    }
                }


            }
        }
        private async Task ProcessHostelApplicationFee(string RRR, string orderID)
        {
            HostelApplication hostelApplication;
            if (string.IsNullOrEmpty(orderID))
            {
                hostelApplication = await _db.HostelApplications.AsNoTracking()
                    .Where(x => x.ReferenceNo.Equals(RRR))
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

                }
                else
                {
                    var log = await _db.RemitaPaymentLogs.AsNoTracking()
                        .Where(x => x.OrderId.Equals(hostelApplication.OrderId))
                        .FirstOrDefaultAsync();

                    var hashed = _query.HashRemitedValidate(hostelApplication.OrderId, RemitaConfigParams.APIKEY,
                        RemitaConfigParams.MERCHANTID);
                    string url = RemitaConfigParams.CHECKSTATUSURL + "/" + RemitaConfigParams.MERCHANTID + "/" +
                                 orderID + "/" + hashed + "/" + "orderstatus.reg";
                    string jsondata = new WebClient().DownloadString(url);
                    RemitaResponse result = JsonConvert.DeserializeObject<RemitaResponse>(jsondata);

                    if (result.Status.Equals("00") || result.Status.Equals("01"))
                    {
                        hostelApplication.IsPayed = true;
                        hostelApplication.TransactionMessage = result.Message;
                        hostelApplication.ReferenceNo = result.Rrr;
                        _db.Entry(hostelApplication).State = EntityState.Modified;

                        log.Rrr = result.Rrr;
                        log.StatusCode = result.Status;
                        log.TransactionMessage = result.Message;
                        _db.Entry(log).State = EntityState.Modified;
                        await _db.SaveChangesAsync();
                    }
                    else
                    {
                        hostelApplication.IsPayed = false;
                        hostelApplication.TransactionMessage = result.Message;
                        hostelApplication.ReferenceNo = result.Rrr;
                        _db.Entry(hostelApplication).State = EntityState.Modified;

                        log.Rrr = result.Rrr;
                        log.StatusCode = result.Status;
                        log.TransactionMessage = result.Message;
                        _db.Entry(log).State = EntityState.Modified;
                        await _db.SaveChangesAsync();
                    }
                }


            }
        }

        private async Task ProcessAccomodationFee(string RRR, string orderID)
        {
            StudentAccommodationFeePayment accomodationFeePayment;
            if (string.IsNullOrEmpty(orderID))
            {
                accomodationFeePayment = await _db.StudentAccommodationFeePayments.AsNoTracking()
                    .Where(x => x.ReferenceNo.Equals(RRR))
                    .FirstOrDefaultAsync();
            }
            else
            {
                accomodationFeePayment = await _db.StudentAccommodationFeePayments.AsNoTracking()
                    .Where(x => x.OrderId.Equals(orderID.Trim()))
                    .FirstOrDefaultAsync();
            }
            if (accomodationFeePayment != null)
            {
                if (accomodationFeePayment.IsPayed.Equals(true))
                {

                }
                else
                {
                    var log = await _db.RemitaPaymentLogs.AsNoTracking()
                        .Where(x => x.OrderId.Equals(accomodationFeePayment.OrderId))
                        .FirstOrDefaultAsync();

                    var hashed = _query.HashRemitedValidate(accomodationFeePayment.OrderId, RemitaConfigParams.APIKEY,
                        RemitaConfigParams.MERCHANTID);
                    string url = RemitaConfigParams.CHECKSTATUSURL + "/" + RemitaConfigParams.MERCHANTID + "/" +
                                 orderID + "/" + hashed + "/" + "orderstatus.reg";
                    string jsondata = new WebClient().DownloadString(url);
                    RemitaResponse result = JsonConvert.DeserializeObject<RemitaResponse>(jsondata);

                    if (result.Status.Equals("00") || result.Status.Equals("01"))
                    {
                        accomodationFeePayment.IsPayed = true;
                        accomodationFeePayment.TransactionMessage = result.Message;
                        accomodationFeePayment.ReferenceNo = result.Rrr;
                        _db.Entry(accomodationFeePayment).State = EntityState.Modified;

                        log.Rrr = result.Rrr;
                        log.StatusCode = result.Status;
                        log.TransactionMessage = result.Message;
                        _db.Entry(log).State = EntityState.Modified;
                        await _db.SaveChangesAsync();
                    }
                    else
                    {
                        accomodationFeePayment.IsPayed = false;
                        accomodationFeePayment.TransactionMessage = result.Message;
                        accomodationFeePayment.ReferenceNo = result.Rrr;
                        _db.Entry(accomodationFeePayment).State = EntityState.Modified;

                        log.Rrr = result.Rrr;
                        log.StatusCode = result.Status;
                        log.TransactionMessage = result.Message;
                        _db.Entry(log).State = EntityState.Modified;
                        await _db.SaveChangesAsync();
                    }
                }


            }
        }

        private async Task ProcessUtmeApplication(string RRR, string orderID)
        {
            ApplicantPayment applicantPayment;
            if (string.IsNullOrEmpty(orderID))
            {
                applicantPayment = await _db.ApplicantPayments.AsNoTracking()
                    .Where(x => x.ReferenceNo.Equals(RRR))
                    .FirstOrDefaultAsync();
            }
            else
            {
                applicantPayment = await _db.ApplicantPayments.AsNoTracking()
                    .Where(x => x.OrderId.Equals(orderID.Trim()))
                    .FirstOrDefaultAsync();
            }
            if (applicantPayment != null)
            {
                if (applicantPayment.IsPayed.Equals(true))
                {

                }
                else
                {
                    var log = await _db.RemitaPaymentLogs.AsNoTracking()
                        .Where(x => x.OrderId.Equals(applicantPayment.OrderId))
                        .FirstOrDefaultAsync();

                    var hashed = _query.HashRemitedValidate(applicantPayment.OrderId, RemitaConfigParams.APIKEY,
                        RemitaConfigParams.MERCHANTID);
                    string url = RemitaConfigParams.CHECKSTATUSURL + "/" + RemitaConfigParams.MERCHANTID + "/" +
                                 orderID + "/" + hashed + "/" + "orderstatus.reg";
                    string jsondata = new WebClient().DownloadString(url);
                    RemitaResponse result = JsonConvert.DeserializeObject<RemitaResponse>(jsondata);

                    if (result.Status.Equals("00") || result.Status.Equals("01"))
                    {
                        applicantPayment.IsPayed = true;
                        applicantPayment.TransactionMessage = result.Message;
                        applicantPayment.ReferenceNo = result.Rrr;
                        _db.Entry(applicantPayment).State = EntityState.Modified;

                        log.Rrr = result.Rrr;
                        log.StatusCode = result.Status;
                        log.TransactionMessage = result.Message;
                        _db.Entry(log).State = EntityState.Modified;
                        await _db.SaveChangesAsync();
                    }
                    else
                    {
                        applicantPayment.IsPayed = false;
                        applicantPayment.TransactionMessage = result.Message;
                        applicantPayment.ReferenceNo = result.Rrr;
                        _db.Entry(applicantPayment).State = EntityState.Modified;

                        log.Rrr = result.Rrr;
                        log.StatusCode = result.Status;
                        log.TransactionMessage = result.Message;
                        _db.Entry(log).State = EntityState.Modified;
                        await _db.SaveChangesAsync();
                    }
                }


            }
        }

    }
}