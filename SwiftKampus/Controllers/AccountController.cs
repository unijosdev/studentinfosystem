using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Newtonsoft.Json;
using Rotativa;
using SendGrid;
using SendGrid.Helpers.Mail;
using SwiftKampus.BusinessLogic;
using SwiftKampus.Models;
using SwiftKampus.Services;

using SwiftKampusModel;
using SwiftKampusModel.AddmissionApplicant;
using SwiftKampusModel.Payment;

namespace SwiftKampus.Controllers
{
    [Authorize]
    [Audit(AuditingLevel = 2)]
    public class AccountController : BaseController
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public AccountController(SchoolDbContext db) : base(db)
        {

        }

        public AccountController(ApplicationUserManager userManager,
            ApplicationSignInManager signInManager,
            SchoolDbContext db, IAuthenticationManager authenticationManager) : base(db)
        {
            UserManager = userManager;
            SignInManager = signInManager;
            _authmgr = authenticationManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get => _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            private set => _signInManager = value;
        }

        private readonly IAuthenticationManager _authmgr;

        public ApplicationUserManager UserManager
        {
            get => _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            private set => _userManager = value;
        }

        private static Random random = new Random();

        //public async Task<ActionResult> RegisterSupport()
        //{
        //    try
        //    {
        //        var user = new ApplicationUser
        //        {
        //            Id = "support@unijos.com",
        //            UserName = "support@unijos.com",
        //            Email = "support@unijos.com",
        //        };
        //        var result = await UserManager.CreateAsync(user, "unijossupport@12345");
        //        if (result.Succeeded)
        //        {
        //            await this.UserManager.AddToRoleAsync(user.Id, RoleName.TSupport);

        //            ViewBag.Message = "Created Successfully";
        //            return View();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ViewBag.Message = ex.Message;
        //        return View();
        //    }
        //    return View();
        //}

        // List of failed email verification users
        public ActionResult UnverifiedEmail(string message)
        {
            var userList = _db.Users.AsNoTracking().ToList();
            ViewBag.Message = message;
            ViewData.Add("ActionMessage", "View list of unverified email");
            return View(userList);
        }

        public async Task<ActionResult> RemoveUnVerifiedEmail()
        {
            var userList = _db.Users.AsNoTracking().Where(x => x.EmailConfirmed.Equals(false)).ToList();
            foreach (var user in userList)
            {
                var myUser = _db.Users.Find(user.Id);
                _db.Users.Remove(myUser);
            }
            await _db.SaveChangesAsync();
            ViewData.Add("ActionMessage", $"{userList.Count} users is deleted");
            return RedirectToAction("UnverifiedEmail", new { message = $"{userList.Count} users have been deleted" });
        }

        //public async Task<ActionResult> DeleteUserAccount(string Id)
        //{
        //    if (string.IsNullOrEmpty(Id))
        //    {
        //        ViewBag.Message = "User Id cannot be null";
        //        ViewData.Add("ActionMessage", "Error because user id is null");
        //        return View();
        //    }
        //    var user = await _db.Users.AsNoTracking().Where(x => x.Id.ToUpper().Equals(Id.Trim().ToUpper())
        //                            || x.Email.Trim().ToUpper().Equals(Id.Trim().ToUpper()))
        //                            .FirstOrDefaultAsync();
        //    if (user != null)
        //    {
        //        var student = _db.Students.FirstOrDefault(x => x.Email.Equals(user.Email));
        //        student.Active = false;
        //        _db.Entry(student).State = EntityState.Modified;
        //        _db.Entry(user).State = EntityState.Deleted;
        //        await _db.SaveChangesAsync();
        //        ViewBag.Message = $"Account with {userId} has been deleted Successfully";
        //        ViewData.Add("ActionMessage", $"Account with {userId} is deleted Successfully");
        //        return View();
        //    }
        //    ViewBag.Message = "User Not Found";
        //    ViewData.Add("ActionMessage", "Error, User Account not found");
        //    return View();
        //}
        [Authorize(Roles = RoleName.SuperAdmin + "," + RoleName.Admin + "," + RoleName.TSupport)]
        public async Task<ActionResult> SearchEmail(string Id)
        {
            if (string.IsNullOrEmpty(Id))
            {
                ViewBag.Message = "Jamb Reg No is Required";
                ViewData.Add("ActionMessage", "Error, Jamb Reg No is Required");
                return View();
            }
            var user = await _db.Users.AsNoTracking().Where(x => x.Id.Trim().ToUpper().Equals(Id.Trim().ToUpper())
                            || x.Email.Trim().ToUpper().Equals(Id.Trim().ToUpper()))
                            .FirstOrDefaultAsync();

            if (user != null)
            {
                var student = await _db.Students.FindAsync(user.Id);
                ViewBag.Message = $"This Registered Email is ( {user.Email} )  ({student?.FullName})  ({student?.JambRegNo}) and email confirmation is ({user.EmailConfirmed})";
                return View();
            }

            ViewBag.Message = "User Not Found";
            ViewData.Add("ActionMessage", "Error: User not Found");
            return View();
        }

        //[Authorize(Roles = RoleName.SuperAdmin)]
        //[Authorize(Roles = RoleName.Admin)]
        //[Authorize(Roles = RoleName.TSupport)]
        [Authorize(Roles = RoleName.SuperAdmin + "," + RoleName.Admin + "," + RoleName.TSupport)]
        //public async Task<ActionResult> ChangeEmail(string Id, string Email, string rrr, int? ChangeDetailFeeId, string StudentType)

        
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public async Task<ActionResult> ChangeEmail(string Id, string Email, int? ChangeDetailFeeId, string StudentType)
        {
            
            var studentStaus = from StudentCategory s in Enum.GetValues(typeof(StudentCategory))
                               select new { ID = s, Name = s.ToString() };
            ViewBag.StudentType = new SelectList(studentStaus, "Name", "Name");
            ViewBag.ChangeDetailFeeId = new SelectList(_db.ChangeDetailFees.ToList(), "ChangeDetailFeeId", "FancyName");

            if (string.IsNullOrEmpty(Id) && string.IsNullOrEmpty(Email)/* && string.IsNullOrEmpty(rrr)*/
                 && ChangeDetailFeeId == null)
            {
                ViewBag.Message = "User Id Email and RRR is Required";
                ViewData.Add("ActionMessage", "Error, User Id and email is Required");
                ViewBag.ChangeDetailFeeId = new SelectList(_db.ChangeDetailFees.ToList(), "ChangeDetailFeeId", "FancyName");
                return View();
            }

            var detailFee = _db.ChangeDetailFees.Find(ChangeDetailFeeId);

            //var hashrrr = _query.HashRrrQuery(rrr, RemitaConfigParams.APIKEY, RemitaConfigParams.MERCHANTID);
            //string posturl = RemitaConfigParams.CHECKSTATUSURL + "/" + RemitaConfigParams.MERCHANTID + "/" + rrr + "/" + hashrrr + "/" + "status.reg";
            //try
            //{
            //    string jsondata = new WebClient().DownloadString(posturl);
            //    var result = JsonConvert.DeserializeObject<RemitaResponse>(jsondata);

                //if (result.Status.Equals("00") || result.Status.Equals("01") || true)
                //{
                    //int amount = (int)detailFee.Amount;
                    //int ramount = (int)result.Amount;
                    //if (detailFee != null && amount == ramount)
                    //{
                        //if (!_db.ChangeDetailPayments.Any(x => x.ReferenceNo.Trim().Equals(rrr.Trim()) && x.IsExpired.Equals(true)))
                        //{
                            if (detailFee.DetailCategory.Equals(DetailPayment.Change_Email.ToString()))
                            {
                                if (StudentType.Equals(StudentCategory.Student.ToString()))
                                {
                                    var student = await _db.Students.AsNoTracking()
                                           .Where(x => x.JambRegNo.ToUpper().Equals(Id.Trim().ToUpper()) || x.Email.Trim().ToUpper().Equals(Id.Trim().ToUpper()))
                                           .FirstOrDefaultAsync();

                                    if (student != null)
                                    {
                                        var detailPayment = new ChangeDetailPayment()
                                        {
                                            StudentId = student.StudentId,
                                            StudentCategory = StudentType,
                                            Date = DateTime.Now,
                                            //OrderId = string.IsNullOrEmpty(result.OrderId) ? result.Rrr : result.OrderId,
                                            OrderId = RandomString(10),
                                            //ReferenceNo = result.Rrr,
                                            ReferenceNo = RandomString(10),
                                            //PaymentStatus = result.Message,
                                            PaymentStatus = "Success",
                                            SessionId = _query.GetCurrentSessionId(student.SchoolProgrammeId),
                                            ChangeDetailFeeId = (int)ChangeDetailFeeId,
                                            TotalAmount = detailFee.Amount
                                        };
                                        var user = await _db.Users.AsNoTracking()
                                                        .Where(x => x.Email.ToUpper().Equals(student.Email.ToUpper()) ||
                                                        x.StudentId.Equals(student.StudentId))
                                                        .FirstOrDefaultAsync();

                                        if (user == null)
                                        {
                                            ViewBag.Message = $"This student {Id} has not been registered before";
                                            ViewBag.ChangeDetailFeeId = new SelectList(_db.ChangeDetailFees.ToList(), "ChangeDetailFeeId", "FancyName");
                                            return View();
                                        }
                                        if (user?.Email != null)
                                        {
                                            user.Email = Email.Trim();
                                            user.UserName = Email.Trim();
                                            _db.Entry(user).State = EntityState.Modified;

                                            student.Email = Email.Trim();
                                            _db.Entry(student).State = EntityState.Modified;
                                        }
                                        detailPayment.IsExpired = false;
                                        _db.ChangeDetailPayments.Add(detailPayment);

                                        await _db.SaveChangesAsync();
                                        ViewBag.Message = "Changes Applied Successfully";
                                        ViewData.Add("ActionMessage", $"Changes Applied Successfully for {student.FullName}");
                                        ViewBag.ChangeDetailFeeId = new SelectList(_db.ChangeDetailFees.ToList(), "ChangeDetailFeeId", "FancyName");
                                        return View();
                                    }
                                }
                                else if (StudentType.Equals(StudentCategory.Utme_Applicant.ToString()))
                                {
                                    var utmeApplicant = await _db.UtmeApplicants.AsNoTracking()
                                                       .Where(x => x.JambRegNo.ToUpper().Equals(Id.Trim().ToUpper())
                                                       || x.Email.ToUpper().Trim().Equals(Id.Trim().ToUpper()))
                                                       .FirstOrDefaultAsync();

                                    if (utmeApplicant != null)
                                    {
                                        var detailPayment = new ChangeDetailPayment()
                                        {
                                            ApplicantId = utmeApplicant.JambRegNo,
                                            StudentCategory = StudentType,
                                            Date = DateTime.Now,
                                            //OrderId = string.IsNullOrEmpty(result.OrderId) ? result.Rrr : result.OrderId,
                                            OrderId = RandomString(10),
                                            //ReferenceNo = result.Rrr,
                                            ReferenceNo = RandomString(10),
                                            PaymentStatus = "Success",
                                            SessionId = utmeApplicant.SessionId,
                                            ChangeDetailFeeId = (int)ChangeDetailFeeId,
                                            TotalAmount = detailFee.Amount
                                        };
                                        var user = await _db.Users.Where(x => x.Email.ToUpper().Equals(utmeApplicant.Email.ToUpper()) ||
                                                        x.Id.Equals(utmeApplicant.JambRegNo)).FirstOrDefaultAsync();

                                        if (user == null)
                                        {
                                            ViewBag.Message = $"This Utme Applicant with {Id} has not been registered before";
                                            ViewBag.ChangeDetailFeeId = new SelectList(_db.ChangeDetailFees.ToList(), "ChangeDetailFeeId", "FancyName");
                                            return View();
                                        }
                                        if (user?.Email != null)
                                        {
                                            //await this.UserManager.AddToRoleAsync(user.Id, "Applicant"); //use when applicant role aint set
                                            user.Email = Email.Trim();
                                            user.UserName = Email.Trim();
                                            _db.Entry(user).State = EntityState.Modified;

                                            utmeApplicant.Email = Email.Trim();
                                            _db.Entry(utmeApplicant).State = EntityState.Modified;
                                        }
                                        detailPayment.IsExpired = false;
                                        _db.ChangeDetailPayments.Add(detailPayment);

                                        var paymentRecord = _db.ApplicantPayments.FirstOrDefault(x => x.JambRegNo.ToUpper().Trim().Equals(utmeApplicant.JambRegNo.ToUpper().Trim())
                                                                    || x.ApplicantEmail.ToUpper().Trim().Equals(utmeApplicant.Email.ToUpper().Trim()) 
                                                                    && x.TransactionMessage.Equals("Successful") && x.IsPayed == true);
                                        if (paymentRecord != null)
                                        {
                                            paymentRecord.ApplicantEmail = Email.Trim();
                                            _db.Entry(paymentRecord).State = EntityState.Modified;
                                        }

                                        await _db.SaveChangesAsync();
                                        ViewBag.Message = "Changes Applied Successfully";
                                        ViewData.Add("ActionMessage", $"Changes Applied Successfully for {utmeApplicant.FullName}");
                                        return View();
                                    }
                                }
                                else if (StudentType.Equals(StudentCategory.Sales_Of_Form.ToString()))
                                {
                                    var salesOfForm = await _db.Applicants.AsNoTracking()
                                                       .FirstOrDefaultAsync(x => x.ApplicantEmail.ToUpper().Equals(Id.Trim().ToUpper()));
                                    if (salesOfForm != null)
                                    {
                                        var detailPayment = new ChangeDetailPayment()
                                        {
                                            ApplicantId = salesOfForm.ApplicantId,
                                            StudentCategory = StudentType,
                                            Date = DateTime.Now,
                                            //OrderId = string.IsNullOrEmpty(result.OrderId) ? result.Rrr : result.OrderId,
                                            OrderId = RandomString(10),
                                            //ReferenceNo = result.Rrr,
                                            ReferenceNo = RandomString(10),
                                            //PaymentStatus = result.Message,
                                            PaymentStatus = "Success",
                                            SessionId = (int)salesOfForm.SessionId,
                                            ChangeDetailFeeId = (int)ChangeDetailFeeId,
                                            TotalAmount = detailFee.Amount
                                        };
                                        var user = await _db.Users.AsNoTracking()
                                                        .FirstOrDefaultAsync(x => x.Email.ToUpper().Equals(salesOfForm.ApplicantId.ToUpper()) ||
                                                        x.Id.Equals(salesOfForm.ApplicantId));
                                        if (user == null)
                                        {
                                            ViewBag.Message = $"This Applicant {Id} has not been registered before";
                                            ViewBag.ChangeDetailFeeId = new SelectList(_db.ChangeDetailFees.ToList(), "ChangeDetailFeeId", "FancyName");
                                            return View();
                                        }
                                        if (user?.Email != null) 
                                        {
                                            user.Email = Email.Trim();
                                            user.UserName = Email.Trim();
                                            _db.Entry(user).State = EntityState.Modified;

                                            salesOfForm.ApplicantEmail = Email.Trim();
                                            _db.Entry(salesOfForm).State = EntityState.Modified;
                                        }
                                        detailPayment.IsExpired = false;
                                        _db.ChangeDetailPayments.Add(detailPayment);

                                        var paymentRecord = _db.ApplicantPayments.FirstOrDefault(x => x.JambRegNo.ToUpper().Trim().Equals(salesOfForm.ApplicantId.ToUpper().Trim())
                                                                   || x.ApplicantEmail.ToUpper().Trim().Equals(salesOfForm.ApplicantEmail.ToUpper().Trim()));
                                        if (paymentRecord != null)
                                        {
                                            paymentRecord.ApplicantEmail = Email.Trim();
                                            _db.Entry(paymentRecord).State = EntityState.Modified;
                                        }

                                        await _db.SaveChangesAsync();
                                        ViewBag.Message = "Changes Applied Successfully";
                                        ViewData.Add("ActionMessage", $"Changes Applied Successfully for {salesOfForm.FullName}");
                                        ViewBag.ChangeDetailFeeId = new SelectList(_db.ChangeDetailFees.ToList(), "ChangeDetailFeeId", "FancyName");
                                        return View();
                                    }
                                }
                            }
                            else if (detailFee.DetailCategory.Equals(DetailPayment.Remove_Account.ToString()))
                            {
                                if (StudentType.Equals(StudentCategory.Student.ToString()))
                                {
                                    var student = await _db.Students.AsNoTracking()
                                           .Where(x => x.JambRegNo.ToUpper().Equals(Id.Trim().ToUpper()))
                                           .FirstOrDefaultAsync();

                                    if (student != null)
                                    {
                                        var detailPayment = new ChangeDetailPayment()
                                        {
                                            StudentId = student.StudentId,
                                            StudentCategory = StudentType,
                                            Date = DateTime.Now,
                                            //OrderId = string.IsNullOrEmpty(result.OrderId) ? result.Rrr : result.OrderId,
                                            OrderId = RandomString(10),
                                            //ReferenceNo = result.Rrr,
                                            ReferenceNo = RandomString(10),
                                            //PaymentStatus = result.Message,
                                            PaymentStatus = "Success",
                                            SessionId = _query.GetCurrentSessionId(student.SchoolProgrammeId),
                                            ChangeDetailFeeId = (int)ChangeDetailFeeId,
                                            TotalAmount = detailFee.Amount
                                        };
                                        var user = await _db.Users.AsNoTracking()
                                                        .Where(x => x.Email.ToUpper().Equals(student.Email.ToUpper()) ||
                                                        x.StudentId.Equals(student.StudentId))
                                                        .FirstOrDefaultAsync();

                                        if (user == null)
                                        {
                                            ViewBag.Message = $"This student {Id} has not been registered before";
                                            return View();
                                        }
                                        else
                                        {
                                            _db.Entry(user).State = EntityState.Deleted;

                                            student.Active = false;
                                            _db.Entry(student).State = EntityState.Modified;
                                        }

                                        detailPayment.IsExpired = false;
                                        _db.ChangeDetailPayments.Add(detailPayment);

                                        await _db.SaveChangesAsync();
                                        ViewBag.Message = "Changes Applied Successfully";
                                        ViewData.Add("ActionMessage", $"Changes Applied Successfully for {student.FullName}");
                                        ViewBag.ChangeDetailFeeId = new SelectList(_db.ChangeDetailFees.ToList(), "ChangeDetailFeeId", "FancyName");
                                        return View();
                                    }
                                }
                                else if (StudentType.Equals(StudentCategory.Utme_Applicant.ToString()))
                                {
                                    var utmeApplicant = await _db.UtmeApplicants.AsNoTracking()
                                                       .Where(x => x.JambRegNo.ToUpper().Equals(Id.Trim().ToUpper())
                                                       || x.Email.ToUpper().Trim().Equals(Id.Trim().ToUpper()))
                                                       .FirstOrDefaultAsync();

                                    if (utmeApplicant != null)
                                    {
                                        var detailPayment = new ChangeDetailPayment()
                                        {
                                            ApplicantId = utmeApplicant.JambRegNo,
                                            StudentCategory = StudentType,
                                            Date = DateTime.Now,
                                            //OrderId = string.IsNullOrEmpty(result.OrderId) ? result.Rrr : result.OrderId,
                                            OrderId = RandomString(10),
                                            ReferenceNo = RandomString(10),
                                            PaymentStatus = "Success",
                                            SessionId = utmeApplicant.SessionId,
                                            ChangeDetailFeeId = (int)ChangeDetailFeeId,
                                            TotalAmount = detailFee.Amount
                                        };
                                        var user = await _db.Users.AsNoTracking().Where(x => x.Email.ToUpper().Equals(utmeApplicant.Email.ToUpper())
                                                        || x.Id.Equals(utmeApplicant.JambRegNo)).FirstOrDefaultAsync();
                                        if (user == null)
                                        {
                                            ViewBag.Message = $"This Utme Applicant with {Id} has not been registered before";
                                            return View();
                                        }

                                        var paymentRecord = _db.ApplicantPayments.FirstOrDefault(x => x.JambRegNo.ToUpper().Trim().Equals(utmeApplicant.JambRegNo.ToUpper().Trim())
                                                                    || x.ApplicantEmail.ToUpper().Trim().Equals(utmeApplicant.Email.ToUpper().Trim()));
                                        if (paymentRecord == null)
                                        {
                                            _db.Entry(user).State = EntityState.Deleted;

                                            utmeApplicant.HasRegistered = false;
                                            _db.Entry(utmeApplicant).State = EntityState.Modified;
                                        }
                                        else
                                        {
                                            ViewBag.Message = $"This Utme Applicant has made payment with this email";
                                            return View();
                                        }
                                        detailPayment.IsExpired = true;
                                        _db.ChangeDetailPayments.Add(detailPayment);

                                        await _db.SaveChangesAsync();
                                        ViewBag.Message = "Changes Applied Successfully";
                                        ViewData.Add("ActionMessage", $"Changes Applied Successfully for {utmeApplicant.FullName}");
                                        return View();
                                    }
                                }
                                else if (StudentType.Equals(StudentCategory.Sales_Of_Form.ToString()))
                                {
                                    var salesOfForm = await _db.Applicants.AsNoTracking()
                                                       .FirstOrDefaultAsync(x => x.ApplicantEmail.ToUpper().Equals(Id.Trim().ToUpper()));
                                    if (salesOfForm != null)
                                    {
                                        var detailPayment = new ChangeDetailPayment()
                                        {
                                            ApplicantId = salesOfForm.ApplicantId,
                                            StudentCategory = StudentType,
                                            Date = DateTime.Now,
                                            //OrderId = string.IsNullOrEmpty(result.OrderId) ? result.Rrr : result.OrderId,
                                            OrderId = RandomString(10),
                                            //ReferenceNo = result.Rrr,
                                            ReferenceNo = RandomString(10),
                                            //PaymentStatus = result.Message,
                                            PaymentStatus = "Success",
                                            SessionId = (int)salesOfForm.SessionId,
                                            ChangeDetailFeeId = (int)ChangeDetailFeeId,
                                            TotalAmount = detailFee.Amount
                                        };
                                        var user = await _db.Users.AsNoTracking()
                                                        .FirstOrDefaultAsync(x => x.Email.ToUpper().Equals(salesOfForm.ApplicantId.ToUpper()) ||
                                                        x.Id.Equals(salesOfForm.ApplicantId));
                                        if (user == null)
                                        {
                                            ViewBag.Message = $"This Utme Applicant with {Id} has not been registered before";
                                            return View();
                                        }

                                        var paymentRecord = _db.ApplicantPayments.FirstOrDefault(x => x.JambRegNo.ToUpper().Trim().Equals(salesOfForm.ApplicantId.ToUpper().Trim())
                                                                    || x.ApplicantEmail.ToUpper().Trim().Equals(salesOfForm.ApplicantEmail.ToUpper().Trim()));
                                        if (paymentRecord == null)
                                        {
                                            _db.Entry(user).State = EntityState.Deleted;
                                            //salesOfForm. = false;
                                            //_db.Entry(salesOfForm).State = EntityState.Modified;
                                        }
                                        else
                                        {
                                            ViewBag.Message = $"This Applicant has made payment with this email";
                                            return View();
                                        }
                                        detailPayment.IsExpired = true;
                                        _db.ChangeDetailPayments.Add(detailPayment);

                                        await _db.SaveChangesAsync();
                                        ViewBag.Message = "Changes Applied Successfully";
                                        ViewData.Add("ActionMessage", $"Changes Applied Successfully for {salesOfForm.FullName}");
                                        ViewBag.ChangeDetailFeeId = new SelectList(_db.ChangeDetailFees.ToList(), "ChangeDetailFeeId", "FancyName");
                                        return View();
                                    }
                                }
                            }
                            else
                            {
                                ViewBag.Message = $"This Feature has not been implemented at the moment";
                                ViewData.Add("ActionMessage", $"This Feature has not been implemented at the moment");
                                ViewBag.ChangeDetailFeeId = new SelectList(_db.ChangeDetailFees.ToList(), "ChangeDetailFeeId", "FancyName");
                                return View();
                            }
                        //}
                        //else
                        //{
                        //    ViewBag.Message = $"This RRR {rrr} has been used for a payment before";
                        //    ViewData.Add("ActionMessage", $"This RRR {rrr} has been used for a payment before");
                        //    ViewBag.ChangeDetailFeeId = new SelectList(_db.ChangeDetailFees.ToList(), "ChangeDetailFeeId", "FancyName");
                        //    return View();
                        //}
                    //}
                    //else
                    //{
                    //    ViewBag.Message = $"This amount paid with this RRR {rrr} is different from the required amount";
                    //    ViewData.Add("ActionMessage", $"This RRR {rrr} has been used for a payment before");
                    //    ViewBag.ChangeDetailFeeId = new SelectList(_db.ChangeDetailFees.ToList(), "ChangeDetailFeeId", "FancyName");
                    //    return View();
                    //}
                //}
                //else
                //{
                //    ViewBag.Message = $"Payment for this {rrr} can not be verified at the moment";
                //    ViewData.Add("ActionMessage", $"Payment for this {rrr} cant be verified at the moment");
                //    ViewBag.ChangeDetailFeeId = new SelectList(_db.ChangeDetailFees.ToList(), "ChangeDetailFeeId", "FancyName");
                //    return View();
                //}
            //}
            //catch (Exception ex)
            //{
            //    ViewBag.Message = $"Remita Network {ex.Message}";
            //    ViewData.Add("ActionMessage", "Error: Network not responding");
            //    ViewBag.ChangeDetailFeeId = new SelectList(_db.ChangeDetailFees.ToList(), "ChangeDetailFeeId", "FancyName");
            //    return View();
            //}

            ViewBag.Message = "User Not Found";
            ViewData.Add("ActionMessage", "Error: User not Found");
            ViewBag.ChangeDetailFeeId = new SelectList(_db.ChangeDetailFees.ToList(), "ChangeDetailFeeId", "FancyName");
            return View();
        }

        [Authorize(Roles = RoleName.SuperAdmin + "," + RoleName.Admin + "," + RoleName.TSupport)]
        public async Task<ActionResult> UpdateApplicantRecord(string Id, string Email)
        {
            if (string.IsNullOrEmpty(Id) && string.IsNullOrEmpty(Email))
            {
                ViewBag.Message = "User Id and email is Required";
                ViewData.Add("ActionMessage", "Error, User Id and email is Required");
                return View();
            }
            var userRecord = await _db.Users.AsNoTracking()
                                .Where(x => x.Email.Trim().ToUpper().Equals(Email.Trim().ToUpper())).FirstOrDefaultAsync();
            if (userRecord != null)
            {
                var utmeStudent = _db.UtmeApplicants.AsNoTracking().Where(x => x.JambRegNo.Trim().ToUpper().Equals(Id.Trim().ToUpper()))
                                    .FirstOrDefault();
                utmeStudent.HasRegistered = true;
                utmeStudent.Email = userRecord.Email;
                utmeStudent.PhoneNumber = userRecord.PhoneNumber;

                _db.Entry(utmeStudent).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                ViewBag.Message = "Changes Applied Successfully";
                ViewData.Add("ActionMessage", $"Changes Applied Successfully for {utmeStudent.FullName}");
                return View();
            }
            ViewBag.Message = "User Not Found";
            ViewData.Add("ActionMessage", "Error: User not Found");
            return View();
        }

        //[Authorize(Roles = RoleName.Admin + "," + RoleName.DatabaseAdmin + "," + RoleName.SuperAdmin)]
        public async Task<ActionResult> RemoveUserAccount(string Id)
        {
            if (string.IsNullOrEmpty(Id))
            {
                ViewBag.Message = "User Id or Email is Required";
                ViewData.Add("ActionMessage", "Error, User Id is Required");
                return View();
            }

            var utmeApplicant = _db.UtmeApplicants.AsNoTracking().FirstOrDefault(x => x.JambRegNo.Trim().ToUpper().Equals(Id.Trim().ToUpper())
                                    || x.Email.Trim().ToUpper().Equals(Id.Trim().ToUpper()));

            if (utmeApplicant != null)
            {
                utmeApplicant.HasRegistered = false;
                _db.Entry(utmeApplicant).State = EntityState.Modified;

                var user = await _db.Users.Where(x => x.Id.Trim().ToUpper().Equals(utmeApplicant.JambRegNo.Trim().ToUpper())
                                    || x.Email.Trim().ToUpper().Equals(utmeApplicant.Email.Trim().ToUpper()))
                                    .FirstOrDefaultAsync();
                if (user != null)
                {
                    _db.Entry(user).State = EntityState.Deleted;
                    ViewBag.Message = "Changes Applied Successfully";
                }
                await _db.SaveChangesAsync();
                ViewBag.Message = "Changes Applied Successfully";

                return View();
            }

            var sofApplicant = _db.Applicants.AsNoTracking().FirstOrDefault(x => x.ApplicantId.Trim().ToUpper().Equals(Id.Trim().ToUpper())
                                   || x.ApplicantEmail.Trim().ToUpper().Equals(Id.Trim().ToUpper()));

            if (sofApplicant != null)
            {
                var email = sofApplicant.ApplicantEmail.Trim().ToUpper();
                var applicantId = sofApplicant.ApplicantId.Trim().ToUpper();
                //_db.Entry(sofApplicant).State = EntityState.Deleted;
                var user = await _db.Users.Where(x => x.Id.Trim().ToUpper().Equals(applicantId)
                                    || x.Email.Trim().ToUpper().Equals(email))
                                    .FirstOrDefaultAsync();
                if (user != null)
                {
                    _db.Entry(user).State = EntityState.Deleted;
                    ViewBag.Message = "Changes Applied Successfully";
                }
                await _db.SaveChangesAsync();
                ViewBag.Message = "Changes Applied Successfully";


                return View();
            }

            var student = await _db.Students.AsNoTracking().Where(x => x.JambRegNo.ToUpper().Equals(Id.Trim().ToUpper())
                                    || x.Email.Trim().ToUpper().Equals(Id.Trim().ToUpper())).FirstOrDefaultAsync();
            if (student != null)
            {
                student.Active = false;
                _db.Entry(student).State = EntityState.Modified;

                var user = await _db.Users.Where(x => x.StudentId.Trim().ToUpper().Equals(student.StudentId.Trim().ToUpper())
                                    || x.Email.Trim().ToUpper().Equals(student.Email.Trim().ToUpper()))
                                    .FirstOrDefaultAsync();
                if (user != null)
                {
                    _db.Entry(user).State = EntityState.Deleted;
                    ViewBag.Message = "Changes Applied Successfully";
                }
                await _db.SaveChangesAsync();
                ViewBag.Message = "Changes Applied Successfully";

                return View();
            }

            var staff = await _db.Staffs.AsNoTracking().Where(x => x.StaffId.ToUpper().Equals(Id.Trim().ToUpper())
                                  || x.Email.Trim().ToUpper().Equals(Id.Trim().ToUpper())).FirstOrDefaultAsync();
            if (staff != null)
            {
                staff.IsActiveStaff = false;
                _db.Entry(staff).State = EntityState.Modified;

                var user = await _db.Users.Where(x => x.Id.Equals(staff.StaffId.Trim())
                                    || x.Email.Trim().ToUpper().Equals(staff.Email.Trim().ToUpper()))
                                    .FirstOrDefaultAsync();
                if (user != null)
                {
                    _db.Entry(user).State = EntityState.Deleted;
                }
                await _db.SaveChangesAsync();
                ViewBag.Message = "Changes Applied Successfully";
                ViewData.Add("ActionMessage", $"Staff Account is deleted successfully {user.Email}");
                return View();
            }
            ViewBag.Message = "User Not Found";
            ViewData.Add("ActionMessage", "Error: User not found");
            return View();
        }


        public async Task<ActionResult> MakeStudentAccountActive(string Id)
        {
            if (string.IsNullOrEmpty(Id))
            {
                ViewBag.Message = "User Id or Email is Required";
                ViewData.Add("ActionMessage", "Error, User Id is Required");
                return View();
            }

            var student = await _db.Students.AsNoTracking().Where(x => x.JambRegNo.ToUpper().Equals(Id.Trim().ToUpper())
                                    || x.Email.Trim().ToUpper().Equals(Id.Trim().ToUpper())).FirstOrDefaultAsync();
            if (student != null)
            {
                var user = await _db.Users.AsNoTracking().Where(x => x.Id.Equals(student.StudentId)
                                || x.Email.Trim().ToUpper().Equals(student.Email.Trim().ToUpper())).FirstOrDefaultAsync();

                if (user != null)
                {
                    var roles = await UserManager.GetRolesAsync(user.Id);
                    if (!roles.Any(x => x.Equals(RoleName.Student)))
                    {
                        await UserManager.RemoveFromRolesAsync(user.Id, roles.ToArray());
                        await UserManager.AddToRoleAsync(user.Id, RoleName.Student);
                    }

                    student.Active = true;
                    _db.Entry(student).State = EntityState.Modified;
                    ViewBag.Message = "Changes Applied Successfully";
                    await _db.SaveChangesAsync();
                    return View();
                }
            }
            ViewBag.Message = "User Not Found";
            ViewData.Add("ActionMessage", "Error: User not found");
            return View();
        }

        [HttpGet]
        public ActionResult MigrateFromApplicantToStudent()
        {
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FancyName");
            ViewBag.FacultyId = new SelectList(_db.Faculties.AsNoTracking(), "FacultyId", "FacultyName");
            ViewBag.DepartmentId = new SelectList(_db.Departments.AsNoTracking(), "DepartmentId", "DeptName");
            ViewBag.ProgrammeId = new SelectList(_db.Programmes.AsNoTracking(), "ProgrammeId", "ProgrammeName");
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> MigrateFromApplicantToStudent(int SchoolProgrammeId, int FacultyId, int? DepartmentId, int? ProgrammeId, int SessionId)
        {
            int count = 0;
            //var session = _query.GetCurrentSession(SchoolProgrammeId);
            var session = _db.Sessions.Find(SessionId);
            var students = await _db.Students.Include(i => i.Programme.Department.Faculty).Include(i => i.SchoolProgramme)
                                .Include(i => i.Programme.Department)
                                .Include(i => i.Programme).AsNoTracking()
                                .Where(x => x.Session.SessionId.Equals(session.SessionId) && x.SchoolProgramme.SchoolProgrammeId.Equals(SchoolProgrammeId)
                                && x.StudentStatus.ToUpper().Equals(StudentStatus.New_Student.ToString().ToUpper())
                                && x.Programme.Department.FacultyId.Equals(FacultyId))
                                .ToListAsync();

            if(DepartmentId != null)
            {
                students = students.Where(x => x.Programme.Department.DepartmentId.Equals((int)DepartmentId)).ToList();
            }
                
             if (ProgrammeId != null)
            {
                students = students.Where(x => x.Programme.ProgrammeId.Equals((int)ProgrammeId)).ToList();
            }
           

            foreach (var student in students)
            {
                var user = await _db.Users.AsNoTracking().Where(x => x.Email.Trim().ToUpper().Equals(student.Email.Trim().ToUpper())).FirstOrDefaultAsync();
                string body = "Dear Applicant, you have been offered admission" +
                               $" to study {student.Programme.ProgrammeName} by UNIJOS. visit https://portal.unijos.edu.ng/Account/Login to login and pay pre-registration charge.";
                if (user != null)
                {
                    var roles = await UserManager.GetRolesAsync(user.Id);
                    if (!roles.Any(x => x.Equals(RoleName.Student)))
                    {
                        await UserManager.RemoveFromRolesAsync(user.Id, roles.ToArray());
                        await UserManager.AddToRoleAsync(user.Id, RoleName.Student);
                        //await NotifyByEmail(user.Id, student.LastName, student.FirstName, student.Programme.ProgrammeName,
                        //                    student.Email, student.JambRegNo, session.SessionName);

                        var customSms = new CustomSms();
                        if (!string.IsNullOrEmpty(student.PhoneNumber))
                        {
                            //await customSms.SendUnknowMsgAsync(new SmsToStudent() { Body = body, Destination = student.PhoneNumber }); //24LiveSMS API
                            await SMSClass.SendSMS("UNIJOS SIS", body, student.PhoneNumber); //EBULK SMS API
                        }

                        count += 1;
                        //student.IsMigrated = true;
                        //_db.Entry(student).State = EntityState.Modified;
                        //await _db.SaveChangesAsync();
                    }
                }
            }

            var message = $"{count} Student Account is migrated successfully";
            ViewData.Add("ActionMessage", message);
            if (count > 0)
            {
                return new JsonResult { Data = new { status = true, message } };
            }
            return new JsonResult { Data = new { status = false, message } };
        }

        async Task NotifyByEmail(string userName, string lastName, string firstName, string programmeName, string email, string matricNo, string sessionName)
        {
            string msgBody = GetEmailTemplate();
            msgBody = msgBody.Replace("{STUDENTNAME}", $"{lastName} {firstName}");
            msgBody = msgBody.Replace("{PROGRAMMENAME}", programmeName);
            msgBody = msgBody.Replace("{SESSIONAME}", sessionName);
            msgBody = msgBody.Replace("{EMAILNAME}", email);
            msgBody = msgBody.Replace("{JAMBREGNO}", matricNo);
            //string msgBody = $"<strong> Hello {lastName} {firstName}, </strong><br />" +
            //                                    $"<strong>Admission Notification Notice</ strong>" +
            //                                    "<p>" +
            //                                    "<br> You have been offered a provisional admission to University of Jos" +
            //                                    $"<br> to study {programmeName} for {sessionName} Academic Session" +
            //                                    "</p>" +
            //                                    "<p>" + "<br>Login to portal.unijos.edu.ng, click on login button enter your reg no or email address in the username " +
            //                                    "box and type in the password your used during POST UTME screening to pay your acceptance fee" +
            //                                    "</p>" +
            //                                    "<p>" +
            //                                    $"<br>Your Email is {email} and JambReg/Form No No is {matricNo}" +
            //                                    "</p>" +
            //                                    "<a href=\"https://portal.unijos.edu.ng" + "/Account/Login" + "\">Login now</a>";

            await UserManager.SendEmailAsync(userName, "Notification of Offer of Provisional Admission", msgBody);
        }

        //[AllowAnonymous]
        public async Task<ActionResult> SendAdmissionMail()
        {
            ////await NotifyByEmail("CM123456", "Ajileye", "Joseph", "Computer Science", "Kunlesymls@gmail.com", "JS42552525", "2018/2019");
            //string body = "You've been offered admission" +
            //                   $" to study Computer Science by Unijos. Visit www.unijos.edu.ng for detail";
            //var customSms = new CustomSms();
            //await customSms.SendUnknowMsgAsync(new SmsToStudent() { Body = body, Destination = "08024413200,08036323803" });
            //await customSms.SendUnknowMsgAsync(new SmsToStudent() { Body = body, Destination = "08030417117,07036927669" });
            //return View();
            int SchoolProgrammeId = await GetUndergraduateSchoolProgrammeId();
            int count = 0;
            var session = _query.GetCurrentSession(SchoolProgrammeId);
            var students = await _db.Students.Include(i => i.Programme).Include(i => i.SchoolProgramme).AsNoTracking()
                                .Where(x => x.Session.SessionId.Equals(session.SessionId)
                                && x.StudentStatus.ToUpper().Equals(StudentStatus.New_Student.ToString().ToUpper()))
                                .ToListAsync();
            count = students.Count();
            //foreach (var student in students)
            //{
            //    var user = await _db.Users.AsNoTracking().Where(x => x.Email.Trim().ToUpper().Equals(student.Email.Trim().ToUpper())).FirstOrDefaultAsync();                
            //    if (user != null)
            //    {
            //        var roles = await UserManager.GetRolesAsync(user.Id);
            //        if (roles.Any(x => x.Equals(RoleName.Student)))
            //        {
            //            //await UserManager.RemoveFromRolesAsync(user.Id, roles.ToArray());
            //            //await UserManager.AddToRoleAsync(user.Id, RoleName.Student);
            //            //await NotifyByEmail(user.Id, student.LastName, student.FirstName, student.Programme.ProgrammeName,
            //            //                    student.Email, student.JambRegNo, session.SessionName);

            //            //var customSms = new CustomSms();
            //            //if (!string.IsNullOrEmpty(student.PhoneNumber))
            //            //{
            //            //    await customSms.SendUnknowMsgAsync(new SmsToStudent() { Body = body, Destination = student.PhoneNumber });
            //            //}

            //            count += 1;
            //            //student.IsMigrated = true;
            //            //_db.Entry(student).State = EntityState.Modified;
            //            //await _db.SaveChangesAsync();
            //        }
            //    }
            //}

            var message = $"{count} Student Account is migrated successfully";
            ViewData.Add("ActionMessage", message);
            if (count > 0)
            {
                return new JsonResult { Data = new { status = true, message } };
            }
            return new JsonResult { Data = new { status = false, message } };
        }

        public async Task<ActionResult> TestEmail()
        {
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress($"noreply@unijos.com", "University of Jos");
            var subject = $"UNIJOS NOTIFICATION";
            var to = new EmailAddress("kunlesymls@gmail.com", "joeseph");
            var plainTextContent = "testing Message";
            var htmlContent = "Test Message";
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            try
            {
                var response = await client.SendEmailAsync(msg);
                ViewBag.Message = "Sent Successfuly";
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
            }
            return View();
            //await UserManager.SendEmailAsync("85002070JH", "Confirm your account",
            //                  "Please confirm your staff account by clicking this link: <a href=\"" +
            //                  "\">link</a>");
            //ViewBag.Message = "Sent Successfully";
            //return View();
        }
        //student dashboard      

        [AllowAnonymous]
        public async Task<ActionResult> CheckAdmissionStatus(string id)
        {
            var student = await _db.Students.AsNoTracking().Include(i => i.Programme)
                                    .Include(i => i.Programme.Department)
                                    .Include(i => i.Programme.Department.Faculty)
                                    .Where(x => x.JambRegNo.ToUpper().Equals(id.Trim().ToUpper())
                                    && x.IsDelete.Equals(false)).FirstOrDefaultAsync();
            ViewData.Add("ActionMessage", "View Check Admission Status");
            return View(student);
        }


        [AllowAnonymous]
        public ActionResult CheckAdmission()
        {
            ViewData.Add("ActionMessage", "View Check Admission");

            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CheckAdmission(string FormNo)
        {
            if (!string.IsNullOrEmpty(FormNo))
            {
                FormNo = FormNo.Trim().ToUpper();
                var student = await _db.Students.AsNoTracking().Include(i => i.Programme)
                                    .Include(i => i.Programme.Department)
                                    .Include(i => i.Programme.Department.Faculty)
                                    .Include(i => Session)
                                    .Where(x => (x.JambRegNo.ToUpper().Equals(FormNo) ||
                                    x.Email.ToUpper().Equals(FormNo) || x.MatricNo.Trim().ToUpper().Equals(FormNo))
                                    && x.StudentStatus.ToUpper().Equals(StudentStatus.New_Student.ToString().ToUpper())
                                    && x.IsDelete.Equals(false)).FirstOrDefaultAsync();



                if (student != null)
                {
                    var ugSchoolProgrammeId = await GetUndergraduateSchoolProgrammeId();
                    if (student.SchoolProgrammeId.Equals(ugSchoolProgrammeId) && (student.ModeOfEntry != "RS" && student.IsRemedialStudent.Equals(false)) && !student.Nationality.Equals("African"))
                    {
                        return View("AdmissionConfirmation", student);
                    }
                    if (student.Active.Equals(true) && student.ModeOfEntry.Equals(ModeOfEntry.UTME.ToString()))
                    {
                        //ViewBag.Message = $"This Student Account has been registered with {student.Email}. " +
                        //    $"Please check your Email Inbox and Activate your account";
                        ViewBag.message = $"Congratulations! You have been offered admission to study {student.Programme.ProgrammeName}." +
                            " Login and proceed with registration using your email and password used during UTME/DE screenning.";
                        return View();
                    }
                    var register = new RegisterViewModel
                    {
                        StudentId = student.StudentId,
                        LastName = student.LastName,
                        FirstName = student.FirstName,
                        Department = student.Programme.ProgrammeName,
                        IsNewStudent = student.StudentStatus == StudentStatus.New_Student.ToString() ? true : false,
                        Email = student.Email,
                        Password = "",
                        IsRemedial = student.IsRemedialStudent ?? false,
                    };
                    ViewBag.LevelId = new SelectList(_db.Levels.AsNoTracking().ToList(), "LevelId", "LevelName", student.LevelId);
                    return View("RegisterStudent", register);
                }
                else
                {
                    ViewBag.Message = "Ooops.. This Jamb No/Form No has not been offered Admission yet...";
                    return View();
                }
            }
            ViewBag.Message = "Please type in a valid Form No";
            return View();
        }

        // GET: /Account/AdmissionConfirmation
        [AllowAnonymous]
        public ActionResult AdmissionConfirmation(Student model)
        {
            return View(model);
        }

        [AllowAnonymous]
        public async Task<ActionResult> PrintAdmission(string id)
        {
            var supplementaryList = await _db.Students.AsNoTracking().Include(i => i.Programme)
                .Include(i => i.Programme.Department).Include(i => i.Programme.Department.Faculty)
                .Where(x => x.JambRegNo.ToUpper().Equals(id.ToUpper()))
                .FirstOrDefaultAsync();
            ViewData.Add("ActionMessage", "Print Admission Letter");

            return new ViewAsPdf(supplementaryList);
        }

        [AllowAnonymous]
        public ActionResult VerifyAdmission(AdmissionViewModel model)
        {
            var staff = new AdmissionViewModel()
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                RegNo = model.RegNo,
                Department = model.Department,
            };
            ViewData.Add("ActionMessage", "Verify Admission details");

            return View(staff);
        }

        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl, string message)
        {
            ViewBag.ReturnUrl = returnUrl;
            ViewData.Add("ActionMessage", "Login View");
            ViewBag.Message = message;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                ViewData.Add("ActionMessage", "Error: invalid data supplied");
                return View(model);
            }

            //for temporarily blocking oout some students
            string[] emailList = {
                                    "2022MS0474@unijos.edu.ng", "2022AG0138@unijos.edu.ng", "2022AG0147@unijos.edu.ng", "2022AR0819@unijos.edu.ng",
                                    "2022AR0740@unijos.edu.ng", "2022AR0762@unijos.edu.ng", "2022AR0837@unijos.edu.ng", "2022BS0117@unijos.edu.ng",
                                    "2022ED0933@unijos.edu.ng", "2022EV0262@unijos.edu.ng", "2022EV0298@unijos.edu.ng", "2022NS0969@unijos.edu.ng",
                                    "2022ED0709@unijos.edu.ng", "2022ED0932@unijos.edu.ng", "2022SS0644@unijos.edu.ng", "2022DS0055@unijos.edu.ng",
                                    "2022ED0864@unijos.edu.ng", "2022EV0253@unijos.edu.ng", "2022EV0294@unijos.edu.ng", "2022EV0220@unijos.edu.ng",
                                    "2022ED0894@unijos.edu.ng", "2022NS1001@unijos.edu.ng", "2022AR0755@unijos.edu.ng", "2022AR0764@unijos.edu.ng",
                                    "2022ED0710@unijos.edu.ng", "2022ED0816@unijos.edu.ng", "2022NS0881@unijos.edu.ng", "2022NS0947@unijos.edu.ng",
                                    "2022MS0440@unijos.edu.ng", "2022ED0679@unijos.edu.ng", "2022ED0717@unijos.edu.ng", "2022ED0998@unijos.edu.ng",
                                    "2022ED0639@unijos.edu.ng", "2022ED0663@unijos.edu.ng", "2022AR0664@unijos.edu.ng", "2022AR0571@unijos.edu.ng",
                                    "2022ED0702@unijos.edu.ng", "2022CS0248@unijos.edu.ng", "2022EN0211@unijos.edu.ng", "2022HS0371@unijos.edu.ng",
                                    "2022ED0771@unijos.edu.ng", "2022ED0906@unijos.edu.ng", "2022SS0640@unijos.edu.ng", "2022SS0667@unijos.edu.ng",
                                    "2022ED0953@unijos.edu.ng", "2022NS0899@unijos.edu.ng", "2022ED0696@unijos.edu.ng", "2022ED0811@unijos.edu.ng",
                                    "2022ED0824@unijos.edu.ng", "2022ED0831@unijos.edu.ng", "2022ED0983@unijos.edu.ng", "2022ED0806@unijos.edu.ng",
                                    "2022ED0955@unijos.edu.ng", "2022ED0734@unijos.edu.ng", "2022ED0980@unijos.edu.ng", "2022ED0821@unijos.edu.ng",
                                    "2022NS0986@unijos.edu.ng", "2022AR0754@unijos.edu.ng", "2022AR0840@unijos.edu.ng"
                                };
            if (emailList.Contains((model.Email)))
            {

                ModelState.AddModelError("", "Sorry, you are temporarily suspended from accessing your dashboard until you provide your admission letter.!");
                ViewData.Add("ActionMessage", "Sorry, you are temporarily suspended from accessing your dashboard until you provide your admission letter.");
                return View(model);
            }

            ApplicationUser user = null;
            Student student = null;

            // Combined query to get user and student (if exists)
            user = await GetUserByEmail(model.Email);
            student = user == null ? await GetStudentByPrimaryEmail(model.Email) : null;

            // Handling special email domain
            if (user != null && model.Email.ToLower().Trim().Contains("unijos.edu.ng"))
            {
                UpdateUserEmail(user, student);
            }

            if (user == null)
            {
                if (student != null)
                {
                    // Handle case where only student record exists
                    return HandleStudentWithoutUser(student, model);
                }
                else
                {
                    // Invalid credentials case
                    return HandleInvalidCredentials(model);
                }
            }

            if (!await UserManager.IsEmailConfirmedAsync(user.Id))
            {
                return await SendConfirmationEmail(user);
            }

            // Proceed with login
            return await ProcessLogin(user, model, returnUrl);
        }

        // Auxiliary methods for clarity and reusability
        private async Task<ApplicationUser> GetUserByEmail(string email)
        {
            return await _db.Users.AsNoTracking().FirstOrDefaultAsync(c =>
                                        c.Email.Trim().ToUpper().Equals(email.ToUpper().Trim()));
        }

        private async Task<Student> GetStudentByPrimaryEmail(string email)
        {
            return await _db.Students.AsNoTracking()
                                     .FirstOrDefaultAsync(x => x.PrimaryEmail.Trim().ToUpper().Equals(email.Trim().ToUpper()));
        }

        private void UpdateUserEmail(ApplicationUser user, Student student)
        {
            if (student != null)
            {
                user.Email = student.Email;
                user.UserName = student.Email;
                _db.Entry(user).State = EntityState.Modified;
                _db.SaveChanges();
            }
        }

        private ActionResult HandleStudentWithoutUser(Student student, LoginViewModel model)
        {
            ModelState.AddModelError("", $"Please go to your email({student.PrimaryEmail}) to get your new login credentials");
            ViewData.Add("ActionMessage", "Please check your email for login credentials");
            return View(model);
        }

        private ActionResult HandleInvalidCredentials(LoginViewModel model)
        {
            ModelState.AddModelError("", "Invalid Login Credential");
            ViewData.Add("ActionMessage", "Invalid Login Credential");
            return View(model);
        }

        private async Task<ActionResult> SendConfirmationEmail(ApplicationUser user)
        {
            var code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
            var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code }, protocol: Request.Url.Scheme);
            string body = $"Dear {user.Email}. Please confirm your account by clicking this link: <a href=\"{callbackUrl}\">link</a>";
            await UserManager.SendEmailAsync(user.Id, "Confirm your account", body);
            return View("RedirectRegistration");
        }

        private async Task<ActionResult> ProcessLogin(ApplicationUser user, LoginViewModel model, string returnUrl)
        {
            var result = await SignInManager.PasswordSignInAsync(user.UserName, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    await ActivateUserLogin(user);
                    ViewData.Add("ActionMessage", "Login Successful");
                    return RedirectToAction("CustomDashborad", new { username = user.UserName, returnUrl });
                case SignInStatus.LockedOut:
                    ViewData.Add("ActionMessage", "Lockout");
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    ViewData.Add("ActionMessage", "Login required verification");
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid Login Attempt.");
                    ViewData.Add("ActionMessage", "Login attempt failed");
                    return View(model);
            }
        }

        // This method will Activate IsLogin for all the users that successfully Login
        private async Task ActivateUserLogin(ApplicationUser user)
        {
            var logginUser = _db.Users.Find(user.Id);
            logginUser.IsLogin = true;
            logginUser.LastLogin = DateTime.Now;
            logginUser.NumberOfLogins = logginUser.NumberOfLogins + 1;
            _db.Entry(logginUser).State = EntityState.Modified;
            await _db.SaveChangesAsync();
        }

        /// <summary>
        /// Redirect to custom landing page based on User Role
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        public ActionResult CustomDashborad(string returnUrl)
       {
            ViewData.Add("ActionMessage", "Redirect to dashboard");
            if (returnUrl != null)
            {
                //return RedirectToLocal(returnUrl);
                return Redirect(returnUrl);
            }
            if (User.IsInRole(RoleName.Admin) || User.IsInRole(RoleName.Bursar) || User.IsInRole(RoleName.VC)
                || User.IsInRole(RoleName.DVC) || User.IsInRole(RoleName.Registrar))
            {
                return RedirectToAction("DashBoard", "Home");
            }
            if (User.IsInRole(RoleName.Student))
            {
                return RedirectToAction("StudentDashBoard", "Students");
            }
            if (User.IsInRole(RoleName.Academic))
            {
                return RedirectToAction("LecturerDashboard", "Home");
            }
            if (User.IsInRole(RoleName.None_Academic))
            {
                return RedirectToAction("StaffDashBoard", "Staffs");
            }
            if (User.IsInRole(RoleName.Hod))
            {
                return RedirectToAction("HodDashboard", "Home");
            }
            if (User.IsInRole(RoleName.Dean))
            {
                return RedirectToAction("DeanDashBoard", "Home");
            }
            if (User.IsInRole(RoleName.SuperAdmin))
            {
                return RedirectToAction("SuperAdminDashBoard", "Students");
            }
            if (User.IsInRole(RoleName.PreDegree))
            {
                return RedirectToAction("Details", "PreDegreeStudents");
            }
            if (User.IsInRole(RoleName.Applicant))
            {
                return RedirectToAction("ApplicantDashBoard", "Home");
            }
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. If a
            // user enters incorrect codes for a specified amount of time then the user account will
            // be locked out for a specified amount of time. You can configure the account lockout
            // settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToAction("CustomDashborad", new { username = User.Identity.GetUserName() });

                case SignInStatus.LockedOut:
                    return View("Lockout");

                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        // GET: /Account/AddmissionRegistration
        [AllowAnonymous]
        public async Task<ActionResult> AddmissionRegistration(string programme)
        {
            var fullName = $"{ProgrammeCategory.UnderGraduate.ToString().ToUpper()} {ProgrammeType.Full_Time.ToString().ToUpper()}";
            var schoolProgramme = await _db.SchoolProgrammes.AsNoTracking().Where(x => x.ActiveSale.Equals(true)).ToListAsync();
            var programmeList = new List<string>();
            schoolProgramme = schoolProgramme.Where(x => x.FullName.ToUpper() != fullName && x.ProgrammeCategory.Equals(programme)).ToList();
            foreach (var programmes in schoolProgramme)
            {
                programmeList.Add(programmes.FancyName);
            }
            ViewBag.Programme = programmeList;
            ViewBag.SchoolProgrammeId = new SelectList(schoolProgramme, "SchoolProgrammeId", "FancyName");

            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddmissionRegistration(AddmisionRegistrationVm model)
        {
            var schoolProgramme = _db.SchoolProgrammes.Include(i => i.Session).AsNoTracking()
                                    .Where(x => x.ActiveSale.Equals(true)
                                       && x.SchoolProgrammeId.Equals(model.SchoolProgrammeId)).ToList();
            var programmeList = new List<string>();
            foreach (var programme in schoolProgramme)
            {
                programmeList.Add(programme.FullName);
            }

            ViewBag.Programme = programmeList;

            if (model.Email.ToLower().Trim().Contains("unijos.edu.ng"))
            {
                ViewBag.Message = $"This official email ({model.Email}) cannot be used for UTME Screening";
                ViewBag.SchoolProgrammeId = new SelectList(schoolProgramme, "SchoolProgrammeId", "FullName");

                return View(model);
            }
            if (ModelState.IsValid)
            {
                var checkStudentEmail = await _db.Students.Where(x => x.Email.Trim().ToUpper().Equals(model.Email.Trim().ToUpper())
                                          || x.PrimaryEmail.Trim().ToUpper().Equals(model.Email.Trim().ToUpper()))
                                            . Select(x => new { x.Email, x.PrimaryEmail}).FirstOrDefaultAsync();
                var checkApplicantEmail = await _db.Applicants.Where(x => x.ApplicantEmail.Trim().ToUpper().Equals(model.Email.Trim().ToUpper()))
                                            .Select(x => x.ApplicantEmail).FirstOrDefaultAsync();
                var firstSchoolProgramme = schoolProgramme.FirstOrDefault();

                // run this code if the model.email hasn't been used by a student or previous applicant
                if (string.IsNullOrEmpty(checkStudentEmail?.Email) && string.IsNullOrEmpty(checkStudentEmail?.PrimaryEmail) && string.IsNullOrEmpty(checkApplicantEmail))
                {
                    if (firstSchoolProgramme.Session != null)
                    {
                        var splitSession = firstSchoolProgramme.Session.SessionName.Split('/', '-');
                        var checkUser = new ApplicationUser();
                        string formId = "";
                        do
                        {
                            var no = DateTime.Now.Ticks;
                            string value = no.ToString();
                            var number = value.Substring(value.Length - 5);
                            formId = $"{firstSchoolProgramme.SchoolProgrammeCode}{splitSession[1].Substring(2, 2)}{number}";
                            checkUser = _db.Users.Find(formId);

                        } while (checkUser != null);

                        var user = new ApplicationUser { Id = formId, UserName = model.Email, Email = model.Email };
                        var result = await UserManager.CreateAsync(user, model.Password);
                        if (result.Succeeded)
                        {
                            //await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                            var applicant = new Applicant()
                            {
                                ApplicantId = formId,
                                ApplicantEmail = model.Email,
                                PhoneNumber = model.PhoneNumber,
                                DateOfBirth = DateTime.Now,
                                FirstName = model.FirstName,
                                LastName = model.LastName,
                                SchoolProgrammeId = model.SchoolProgrammeId,
                                SessionId = firstSchoolProgramme.SessionId,
                                ReasonDeptForRejection = "",
                                ReasonForFacultyRejection = "",
                                ReasonForPGRejection = "",
                            };
                            var applicantType = await _db.SchoolProgrammes.Where(x => x.SchoolProgrammeId.Equals(model.SchoolProgrammeId))
                                                    .Select(s => s.ProgrammeCategory).FirstOrDefaultAsync();
                            _db.Applicants.Add(applicant);
                            await _db.SaveChangesAsync();

                            await this.UserManager.AddToRoleAsync(user.Id, "Applicant");

                            string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                            var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code }, protocol: Request.Url.Scheme);
                            await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                            return View("RedirectRegistration");
                        }
                        AddErrors(result);
                        //return View(model);
                    }
                    else
                    {
                        ViewBag.Message = "Current Session is not set at the moment";
                    }
                }
                else
                {
                    ViewBag.Message = $"This email ({model.Email}) has already been used by a student or an applicant. Please use another email";
                    ViewBag.SchoolProgrammeId = new SelectList(schoolProgramme, "SchoolProgrammeId", "FullName");
                    return View(model);
                }
            }
           
            ViewBag.SchoolProgrammeId = new SelectList(schoolProgramme, "SchoolProgrammeId", "FullName");
            // If we got this far, something failed, redisplay form
            return View(model);
        }

        private string ConvertToProperNumber(int number)
        {
            string no = number.ToString();
            if (no.Count() == 1)
            {
                return $"000{number}";
            }
            if (no.Count() == 2)
            {
                return $"00{number}";
            }
            if (no.Count() == 3)
            {
                return $"0{number}";
            }
            return number.ToString();
        }

        [AllowAnonymous]
        public ActionResult RedirectRegistration()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult SignUp()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SignUp(string id)
        {
            var model = new SignUpViewModel();
            var checkStaff = new Staff();
            var checkPreDegree = new PreDegreeStudent();

            if (id != null)
            {
                var checkStudent = await _db.Students.AsNoTracking().Include(i => i.Programme)
                                    .Where(x => (x.MatricNo.Trim().ToUpper().Equals(id.Trim().ToUpper().Trim())
                                    || x.Email.Trim().ToUpper().Equals(id.Trim().ToUpper().Trim())))
                                    .FirstOrDefaultAsync();

                if (checkStudent != null)
                {
                    if (checkStudent.Active.Equals(true) && string.IsNullOrEmpty(checkStudent.PrimaryEmail))
                    {
                        ViewBag.Message = $"This Student Account has been registered with {checkStudent.Email}. " +
                            $"Please check your Email Inbox and Activate your account";
                        return View(model);
                    }
                    //model.UserId = checkStudent.MatricNo;
                    model.UserId = checkStudent.MatricNo ?? checkStudent.JambRegNo; // return the JambRegNo if the MatricNo is null. Useful after removing a student's account without a matric number
                    model.FirstName = checkStudent.FirstName;
                    model.LastName = checkStudent.LastName;
                    model.Department = checkStudent.Programme.ProgrammeName;
                    model.Email = checkStudent.Email;
                    model.LevelId = checkStudent.LevelId;
                    return RedirectToAction("RegisterStudent", model);
                }
                else
                {
                    checkStaff = await _db.Staffs.Include(i => i.Department).AsNoTracking()
                                  .Where(x => x.Email.ToUpper().Equals(id.ToUpper().Trim())
                                  || x.StaffId.Trim().ToUpper().Equals(id.Trim().ToUpper()))
                                  .FirstOrDefaultAsync();
                }
                if (checkStaff != null)
                {
                    if (checkStaff.IsActiveStaff.Equals(true))
                    {
                        ViewBag.Message = $"This Staff Account has been registered with {checkStaff.Email}. " +
                            $"Please check your Email Inbox and Activate your account";
                        return View(model);
                    }
                    model.FirstName = checkStaff.FirstName;
                    model.LastName = checkStaff.LastName;
                    model.UserId = checkStaff.StaffId;
                    model.Department = checkStaff.Department.DeptName;
                    model.Email = checkStaff.Email;
                    return RedirectToAction("RegisterStaff", model);
                }
                else
                {
                    checkPreDegree = await _db.PreDegreeStudents.AsNoTracking()
                                      .Where(x => x.RegNo.ToUpper().Equals(id.ToUpper().Trim()))
                                      .FirstOrDefaultAsync();
                }
                if (checkPreDegree != null)
                {
                    model.FirstName = checkPreDegree.FullName;
                    model.UserId = checkPreDegree.RegNo;
                    model.Department = checkPreDegree.Department;
                    return RedirectToAction("RegisterPreDregree", model);
                }
                ViewBag.Message = "Your User Id cannot be found, Please Check the User Id and try again.." +
                                  " Please Contact the ICT office for more inquiry";
                return View(model);
            }

            // If we got this far, something failed, redisplay form
            ViewBag.Message = "Your User Id is required, Please Check the User Id and try again";
            return View(model);
        }

        //// GET: /Account/Register
        //[AllowAnonymous]
        //public ActionResult Register()
        //{
        //    return View();
        //}

        //// POST: /Account/Register
        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Register(RegisterSuperAdminModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
        //        var result = await UserManager.CreateAsync(user, model.Password);
        //        if (result.Succeeded)
        //        {
        //            //await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
        //            await this.UserManager.AddToRoleAsync(user.Id, "SuperAdmin");

        //            // For more information on how to enable account confirmation and password reset
        //            // please visit http://go.microsoft.com/fwlink/?LinkID=320771 Send an email with
        //            // this link string code = await
        //            // UserManager.GenerateEmailConfirmationTokenAsync(user.Id); var callbackUrl =
        //            // Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code },
        //            // protocol: Request.Url.Scheme); await UserManager.SendEmailAsync(user.Id,
        //            // "Confirm your account", "Please confirm your account by clicking <a href=\"" +
        //            // callbackUrl + "\">here</a>");

        //            return RedirectToAction("Index", "Home");
        //        }
        //        AddErrors(result);
        //    }

        //    // If we got this far, something failed, redisplay form
        //    return View(model);
        //}

        // GET: /Account/RegisterStudent
        [AllowAnonymous]
        public ActionResult RegisterPreDregree(SignUpViewModel model)
        {
            var register = new RegisterViewModel
            {
                StudentId = model.UserId,
                LastName = model.LastName,
                FirstName = model.FirstName,
                //Department = model.Department,
                Email = "",
                Password = ""
            };
            return View(register);
        }

        // POST: /Account/RegisterStudent
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RegisterPreDregree(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var preDegree = await _db.PreDegreeStudents.AsNoTracking().Where(x => x.RegNo.Equals(model.StudentId))
                                    .FirstOrDefaultAsync();
                if (preDegree != null)
                {
                    var user = new ApplicationUser { UserName = model.StudentId, Email = model.Email, StudentId = model.StudentId };
                    var result = await UserManager.CreateAsync(user, model.Password);
                    try
                    {
                        var id = model.StudentId;
                        var mypreDegree = await _db.PreDegreeStudents.Where(x => x.RegNo.Equals(model.StudentId))
                                                        .FirstOrDefaultAsync();
                        if (mypreDegree != null)
                        {
                            mypreDegree.Email = model.Email;
                            mypreDegree.IsRegistered = true;
                            _db.Entry(mypreDegree).State = EntityState.Modified;
                        }
                        await _db.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        TempData["UserMessage"] = $"An Error Occurred {ex.Message}.";
                    }
                    if (result.Succeeded)
                    {
                        await this.UserManager.AddToRoleAsync(user.Id, "PreDegree");

                        var code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                        var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code }, protocol: Request.Url.Scheme);
                        await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your student account by clicking this link: <a href=\"" + callbackUrl + "\">link</a>");
                        ViewBag.Link = callbackUrl;
                        await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your student account by clicking this link: <a href=\"" + callbackUrl + "\">link</a>");
                        ViewBag.Link = callbackUrl;
                        TempData["UserMessage"] = $"Registration is Successful for {user.UserName}, Please Confirm Your Email to Login.";
                        return View("RedirectRegistration");
                    }
                    AddErrors(result);
                }
                else
                {
                    return View("ErrorNotFound");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        // GET: /Account/RegisterStudent
        [AllowAnonymous]
        [ValidateInput(false)]
        public ActionResult RegisterStudent(SignUpViewModel model)
        {
            var register = new RegisterViewModel
            {
                StudentId = model.UserId,
                LastName = model.LastName,
                FirstName = model.FirstName,
                Department = model.Department,
                IsRemedial = model.IsRemedial,
                IsNewStudent = model.StudentStatus == StudentStatus.New_Student.ToString() ? true : false,
                Email = model.Email,
                Password = "",
                LevelId = 0

            };
            ViewBag.LevelId = new SelectList(_db.Levels.AsNoTracking().ToList(), "LevelId", "LevelName");
            return View(register);
        }

        // POST: /Account/RegisterStudent
        [HttpPost]
        [AllowAnonymous]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RegisterStudent(RegisterViewModel model)
        {

            if (ModelState.IsValid)
            {
                var student = await _db.Students.AsNoTracking().Where(x => (x.StudentId.Equals(model.StudentId.Trim()) ||
                                        x.MatricNo.ToUpper().Equals(model.StudentId) || x.JambRegNo.ToUpper().Equals(model.StudentId))
                                         && x.IsGraduated.Equals(false)).FirstOrDefaultAsync();
                if (student != null)
                {

                    if (student.Active.Equals(true) && string.IsNullOrEmpty(student.PrimaryEmail))
                    {
                        ViewBag.Message = $"This Student has been registered with this email ({student.Email})" +
                            $" Contact the online help for support";
                        ViewBag.LevelId = new SelectList(_db.Levels.AsNoTracking().ToList(), "LevelId", "LevelName", model.LevelId);
                        return View(model);
                    }

                    try
                    {
                        var user = new ApplicationUser { Id = student.StudentId, UserName = model.Email, Email = model.Email, StudentId = student.StudentId };
                        var result = await UserManager.CreateAsync(user, model.Password);

                        var id = model.StudentId;

                        if (result.Succeeded)
                        {
                            await this.UserManager.AddToRoleAsync(user.Id, RoleName.Student);
                            var code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                            var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code }, protocol: Request.Url.Scheme);
                            await UserManager.SendEmailAsync(user.Id, "Confirm your account",
                                "Please confirm your student account by clicking this link: <a href=\"" + callbackUrl +
                                "\">link</a> <p>Should in case you cant open the link <br/> Please copy the below link and paste if into a browser" +
                                " <br/>" + callbackUrl + "</p>");
                            ViewBag.Link = callbackUrl;

                            Student myStudent = await _db.Students
                                               .Where(x => x.StudentId.Equals(id) || x.JambRegNo.Equals(id)
                                               || x.MatricNo.Equals(id))
                                               .FirstOrDefaultAsync();
                            if (myStudent != null)
                            {
                                if (myStudent.StudentStatus.ToUpper().Equals(StudentStatus.Returning.ToString().ToUpper()))
                                {
                                    myStudent.LevelId = model.LevelId;
                                }
                                myStudent.Email = model.Email;
                                myStudent.Active = true;
                                _db.Entry(myStudent).State = EntityState.Modified;
                            }
                            await _db.SaveChangesAsync();

                            ViewBag.Message = $"Registration is Successful for {user.UserName}, Please Confirm Your Email to Login.";
                            return View("RedirectRegistration");
                        }
                        AddErrors(result);
                        ViewBag.LevelId = new SelectList(_db.Levels.AsNoTracking().ToList(), "LevelId", "LevelName", model.LevelId);
                        return View(model);
                    }
                    catch (Exception ex)
                    {
                        var userDetails = _db.Users.Find(student.StudentId);
                        var studentError = _db.Students.Where(x => x.StudentId.Equals(userDetails.Id)).FirstOrDefault();
                        ViewBag.Message = $"An Error Occurred, This user with Matric No ({studentError.JambRegNo}) and Full Name ({studentError.FullName})" +
                            $"has an account already with this email ({userDetails?.Email}), " + ex.Message +
                            $"Please contact online support for help.";
                        ViewBag.LevelId = new SelectList(_db.Levels.AsNoTracking().ToList(), "LevelId", "LevelName", model.LevelId);

                        return View(model);
                    }
                }
                else
                {
                    ViewBag.Message = $"An Error Occurred.";
                    ViewBag.LevelId = new SelectList(_db.Levels.AsNoTracking().ToList(), "LevelId", "LevelName", model.LevelId);
                    return View(model);
                }
            }

            // If we got this far, something failed, redisplay form
            ViewBag.LevelId = new SelectList(_db.Levels.AsNoTracking().ToList(), "LevelId", "LevelName", model.LevelId);
            return View(model);
        }

        // GET: /Account/RegisterUTME
        [AllowAnonymous]
        public ActionResult RegisterUtmeApplicant(SignUpUtmeViewModel model)
        {
            var register = new RegisterUtmeViewModel
            {
                SessionId = model.SessionId,
                JambRegNo = model.JambRegNo,
                LastName = model.LastName,
                FirstName = model.FirstName,
                FirstChoice = model.FirstChoice,
                PhoneNumber = "",
                Email = "",
                Password = ""
            };
            return View(register);
        }

        //POST: /Account/RegisterStudent
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RegisterUtmeApplicant(RegisterUtmeViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Email.ToLower().Trim().Contains("unijos.edu.ng"))
                {
                    ViewBag.Message = $"This official email ({model.Email}) cannot be used for UTME Screening";
                    return View(model);
                }
                var checkExistingUtmeStudent = await _db.UtmeApplicants.Include(a => a.Session).AsNoTracking()
                                 .Where(x => x.Email.ToUpper().Trim().Equals(model.Email.Trim().ToUpper())
                                 && x.SessionId != model.SessionId).FirstOrDefaultAsync();
                if (checkExistingUtmeStudent != null)
                {
                    var oldEmail = checkExistingUtmeStudent.Email;

                    checkExistingUtmeStudent.HasRegistered = true;
                    checkExistingUtmeStudent.Email = $"{checkExistingUtmeStudent.Session.SessionName}_{checkExistingUtmeStudent.Email}";
                    _db.Entry(checkExistingUtmeStudent).State = EntityState.Modified;



                    //update oldEmail on paymentTbl
                    //var controller = DependencyResolver.Current.GetService<ApplicantsController>();
                    //controller.ControllerContext = new ControllerContext(this.Request.RequestContext, controller);
                    //var updateLastApplicationEmail = controller.updateLastYearAppPayment(checkExistingUtmeStudent.Email);
                    var changeLastYearEmail = await updateLastYearAppPayment(oldEmail);


                    // Untested Live Code
                    var user = await _db.Users.AsNoTracking().Where(x => x.Id.Equals(checkExistingUtmeStudent.JambRegNo)
                                        || x.Email.Trim().ToUpper().Equals(oldEmail.Trim().ToUpper())).FirstOrDefaultAsync();

                    var roles = await UserManager.GetRolesAsync(user?.Id);
                    if (user != null)
                    {
                        if (!roles.Any(x => x.Equals(RoleName.Student)))
                        {
                            _db.Entry(user).State = EntityState.Deleted;
                        }
                        else
                        {
                            ViewBag.Message = $"This email ({model.Email}) has already been used by a student";
                            return View(model);
                        }
                    }
                    await _db.SaveChangesAsync();
                    // Untested Live Code
                }

                var utmeStudent = await _db.UtmeApplicants.AsNoTracking()
                                   .Where(x => x.JambRegNo.ToUpper().Trim().Equals(model.JambRegNo.Trim().ToUpper()))
                                   .FirstOrDefaultAsync();

                // Check if the email applicant supplied has been used before
                var checkUtmeApplicantEmail = await _db.UtmeApplicants.Where(x => x.Email.Trim().ToUpper().Equals(model.Email.Trim().ToUpper()))
                                            .Select(x => x.Email).FirstOrDefaultAsync();

                var checkStudentEmail = await _db.Students.Where(x => x.Email.Trim().ToUpper().Equals(model.Email.Trim().ToUpper())
                                          || x.PrimaryEmail.Trim().ToUpper().Equals(model.Email.Trim().ToUpper()))
                                            .Select(x => new { x.Email, x.PrimaryEmail }).FirstOrDefaultAsync();

                if (checkUtmeApplicantEmail != null || !string.IsNullOrEmpty(checkStudentEmail?.Email) || !string.IsNullOrEmpty(checkStudentEmail?.PrimaryEmail))
                {
                    ViewBag.Message = $"This email ({model.Email}) has already been used by an applicant or student. Please use another email";
                    return View(model);
                }

                if (utmeStudent != null && utmeStudent.HasRegistered.Equals(true))
                {
                    ViewBag.Message = $"This Applicant has been registered with this email ({utmeStudent.Email})" +
                        $"Please confirm your email or reset your password, " +
                        $"Otherwise contact the Online help for support";
                    return View(model);
                }

                try
                {
                    var user = new ApplicationUser { Id = model.JambRegNo, UserName = model.Email, Email = model.Email };
                    var result = await UserManager.CreateAsync(user, model.Password);

                    if (result.Succeeded)
                    {
                        await this.UserManager.AddToRoleAsync(user.Id, "Applicant");

                        utmeStudent.HasRegistered = true;
                        utmeStudent.Email = model.Email;
                        utmeStudent.PhoneNumber = model.PhoneNumber;

                        _db.Entry(utmeStudent).State = EntityState.Modified;
                        await _db.SaveChangesAsync();

                        var code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                        var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code }, protocol: Request.Url.Scheme);
                        await UserManager.SendEmailAsync(user.Id, "Confirm your account",
                            $"Dear {utmeStudent.FullName}. " +
                            "Please confirm your applicant account by clicking this link: <a href=\"" + callbackUrl +
                            "\">link</a> <p>Should in case you cant open the link <br/> Please copy the below link and paste if into a browser" +
                            " <br/>" + callbackUrl + "</p>");
                        ViewBag.Link = callbackUrl;

                        ViewBag.Message = $"Registration is Successful for {user.UserName}, Please Confirm Your Email to Login.";
                        return View("RedirectRegistration");
                    }
                    AddErrors(result);
                    return View(model);
                }
                catch (Exception ex)
                {
                    var userDetails = _db.Users.Find(utmeStudent.JambRegNo);
                    ViewBag.Message = $"An Error Occurred, This user with Jamb Reg No ({utmeStudent.JambRegNo}) has an account already with this email ({userDetails?.Email}), " +
                        $"Please contact online support for help.  {ex.Message}";
                    return View(model);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        // GET: /Account/RegisterStaff
        [AllowAnonymous]
        public ActionResult RegisterStaff(SignUpViewModel model)
        {
            var staff = new RegisterStaffVm()
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                StaffId = model.UserId,
                Department = model.Department,
                Email = model.Email,
                Password = ""
            };
            return View(staff);
        }

        // POST: /Account/RegisterStafff
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RegisterStaff(RegisterStaffVm model)
        {
            if (ModelState.IsValid)
            {
                var staff = await _db.Staffs.AsNoTracking().Where(x => x.StaffId.Equals(model.StaffId))
                    .FirstOrDefaultAsync();
                if (staff != null)
                {
                    if (staff.IsActiveStaff.Equals(true))
                    {
                        ViewBag.Message = $"This Staff has been registered with this email ({staff.Email})" +
                            $" Contact the online help for support";
                        return View(model);
                    }
                    try
                    {
                        var user = new ApplicationUser
                        {
                            Id = staff.StaffId,
                            UserName = model.Email,
                            Email = model.Email,
                            StaffId = model.StaffId
                        };
                        var result = await UserManager.CreateAsync(user, model.Password);
                        if (result.Succeeded)
                        {
                            await this.UserManager.AddToRoleAsync(user.Id, staff.StaffRole);

                            var code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                            var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code },
                                protocol: Request.Url.Scheme);
                            await UserManager.SendEmailAsync(user.Id, "Confirm your account",
                                "Please confirm your staff account by clicking this link: <a href=\"" + callbackUrl +
                                "\">link</a>");
                            ViewBag.Link = callbackUrl;
                            try
                            {
                                staff.Email = model.Email;
                                staff.IsActiveStaff = true;
                                _db.Entry(staff).State = EntityState.Modified;
                                await _db.SaveChangesAsync();
                            }
                            catch (Exception ex)
                            {
                                ViewBag.Message = $"An error occurred {ex.Message}";
                            }
                            ViewBag.Message = $"Registration is Successful for {user.UserName}, Please Confirm Your Email to Login.";
                            return View("RedirectRegistration");
                        }
                        AddErrors(result);
                        return View(model);
                    }
                    catch (Exception)
                    {
                        var userDetails = _db.Users.Find(staff.StaffId);
                        ViewBag.Message = $"An Error Occurred, This user with Staff Id No ({staff.StaffId}) has an account already with this email ({userDetails?.Email}), " +
                            $"Please contact online support for help.";
                        return View(model);
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            var student = _studentQuery.GetStudent(userId);
            
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            if (student != null)
            {
                string body = $"Congratulations! {student.FirstName} you have successfully sign-up to your dashboard." +
                               $" Pls use it always for all your registration and payments";
                await SMSClass.SendSMS("UNIJOS SIS", body, student.PhoneNumber); //EBULK SMS API

            }
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Query for both user existence and email confirmation in one go to optimize database access
            var isEmailConfirmed = await _db.Users
                .AsNoTracking()
                .Where(u => u.Email.Trim().ToUpper() == model.Email.Trim().ToUpper())
                .Select(u => u.EmailConfirmed)
                .FirstOrDefaultAsync();

            if (!isEmailConfirmed)
            {
                // Redirect to confirmation without revealing the user's existence or email confirmation status
                return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // Retrieve user ID for the email
            var userId = await _db.Users
                .AsNoTracking()
                .Where(u => u.Email.Trim().ToUpper() == model.Email.Trim().ToUpper())
                .Select(u => u.Id)
                .FirstOrDefaultAsync();

            // Generate password reset token and construct callback URL
            string code = await UserManager.GeneratePasswordResetTokenAsync(userId);
            var callbackUrl = Url.Action("ResetPassword", "Account", new { userId, code }, protocol: Request.Url.Scheme);

            // Send email asynchronously
            await UserManager.SendEmailAsync(userId, "Reset Password", $"Please reset your password by clicking <a href=\"{callbackUrl}\">here</a>");

            // Redirect to confirmation view
            return RedirectToAction("ForgotPasswordConfirmation", "Account");
        }

        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(c => c.Email.Trim().ToUpper().Equals(model.Email.Trim().ToUpper()));
            //var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, model.ReturnUrl, model.RememberMe });
        }

        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);

                case SignInStatus.LockedOut:
                    return View("Lockout");

                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });

                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        // POST: /Account/LogOff
        [HttpGet]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> LogOff(string url, string message)
        {
            var logginUser = await _db.Users.AsNoTracking().Where(x => x.Email.Equals(userId))
                .FirstOrDefaultAsync();
            if (logginUser != null)
            {
                logginUser.IsLogin = false;
                logginUser.LastLogOut = DateTime.Now;
                _db.Entry(logginUser).State = EntityState.Modified;
                await _db.SaveChangesAsync();
            }
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Login", "Account", new { returnUrl = url, message });
        }

        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        public async Task<ActionResult> ReNotifyApplicant(string Jamb, string email)
        {
            //int count = 0;
            var session = _query.GetCurrentSession(1);
            var student = await _db.Students.Include(i => i.Programme.Department.Faculty).Include(i => i.SchoolProgramme).AsNoTracking()
                                .Where(x => x.Session.SessionId.Equals(session.SessionId) && x.SchoolProgramme.SchoolProgrammeId.Equals(1)
                                && x.StudentStatus.ToUpper().Equals(StudentStatus.New_Student.ToString().ToUpper())
                                && x.JambRegNo.Equals(Jamb))
                                .FirstOrDefaultAsync();
            if (student != null)
            {
                var user = await _db.Users.AsNoTracking().Where(x => x.Email.Trim().ToUpper().Equals(student.Email.Trim().ToUpper())).FirstOrDefaultAsync();
                string body = "Dear Applicant, You have been offered admission" +
                               $" to study {student.Programme.ProgrammeName} by UNIJOS. Check your email for detail";
                if (user != null)
                {
                    //var roles = await UserManager.GetRolesAsync(user.Id);
                    //if (!roles.Any(x => x.Equals(RoleName.Student)))
                    //{
                    //    await UserManager.RemoveFromRolesAsync(user.Id, roles.ToArray());
                    //    await UserManager.AddToRoleAsync(user.Id, RoleName.Student);
                        await NotifyByEmail(user.Id, student.LastName, student.FirstName, student.Programme.ProgrammeName,
                                            email, student.JambRegNo, session.SessionName);

                        //var customSms = new CustomSms();
                        //if (!string.IsNullOrEmpty(student.PhoneNumber))
                        //{
                        //    //await customSms.SendUnknowMsgAsync(new SmsToStudent() { Body = body, Destination = student.PhoneNumber }); //24LiveSMS API
                        //    await SMSClass.SendSMS("UNIJOS SIS", body, student.PhoneNumber); //EBULK SMS API
                        //}

                        //count += 1;
                        //student.IsMigrated = true;
                        //_db.Entry(student).State = EntityState.Modified;
                        //await _db.SaveChangesAsync();
                    //}
                }
            }
            return View("Succesful");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        public async Task<ActionResult> ActivateStudent(string studentEmail)
        {
            var student = await _db.Students.AsNoTracking().Where(x => x.Email.Trim().ToUpper().Equals(studentEmail.Trim().ToUpper())
                                    || x.PrimaryEmail.Trim().ToUpper().Equals(studentEmail.Trim().ToUpper()))
                                 .FirstOrDefaultAsync();
            var applicant = await _db.UtmeApplicants.Where(a => a.Email.Trim().ToUpper().Equals(studentEmail.Trim().ToUpper())).FirstOrDefaultAsync();
            if (applicant != null)
            {
                var userRecord = await _db.Users.Where(x => x.Email.Trim().ToUpper().Equals(applicant.Email.Trim().ToUpper())
               /* || x.Id.Equals(student.StudentId)*/).FirstOrDefaultAsync();

                if (userRecord != null)
                {
                    userRecord.EmailConfirmed = true;
                    //userRecord.UserName = student.Email;
                    _db.Entry(userRecord).State = EntityState.Modified;
                    _db.SaveChanges();
                }
            }
            return Json("Success!", JsonRequestBehavior.AllowGet);
        }

        #region Helpers

        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager => HttpContext.GetOwinContext().Authentication;

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private string DisplayErrors(IdentityResult result)
        {
            string message = " ";
            foreach (var error in result.Errors)
            {
                message = message + error;
            }
            return message;
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }

        public async Task<ActionResult> updateLastYearAppPayment(string email)
        {
            var applicatPayments = await _db.ApplicantPayments.Include(x => x.Session)
                                        .Where(x => x.ApplicantEmail.Trim().ToUpper().Equals(email.Trim().ToUpper()))
                                        .ToListAsync();
            if (applicatPayments.Count > 0)
            {
                foreach (var item in applicatPayments)
                {
                    item.ApplicantEmail = item.Session.SessionName + item.ApplicantEmail;
                    _db.Entry(item).State = EntityState.Modified;
                }

                await _db.SaveChangesAsync();
            }

            return Json("succes", JsonRequestBehavior.AllowGet);
        }

        #endregion Helpers
    }
}