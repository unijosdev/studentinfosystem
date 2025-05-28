using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using SwiftKampus.Models;
using SwiftKampusModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Http;
using Newtonsoft.Json;

namespace SwiftKampus.Controllers.Api
{
    public class AccountController : ApiController
    {

        private ApplicationUserManager _userManager;
        private ApplicationSignInManager _signInManager;
        private readonly SchoolDbContext _db;

        public AccountController(SchoolDbContext db)
        {
            _db = db;
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            _db = new SchoolDbContext();
            SignInManager = signInManager;
        }

        // POST api/<controller>
        public async Task<IHttpActionResult> SignUp(string id)
        {
            var model = new SignUpViewModel();
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
                        return Ok($"This Student Account has been registered with {checkStudent.Email}. " +
                            $"Please check your Email Inbox and Activate your account");
                        //return View(model);
                    }
                    model.UserId = checkStudent.MatricNo;
                    model.FirstName = checkStudent.FirstName;
                    model.LastName = checkStudent.LastName;
                    model.Department = checkStudent.Programme.ProgrammeName;
                    model.Email = checkStudent.Email;
                    model.LevelId = checkStudent.LevelId;

                    //return RedirectToRoute("RegisterStudent", model);
                    return Ok(model);
                }
                else
                {
                    return BadRequest("Your User Id cannot be found, Please Check the User Id and try again.." +
                                  " Please Contact the ICT office for more inquiry");
                }
            }

            // If we got this far, something failed, redisplay form
            return BadRequest("Your User Id is required, Please Check the User Id and try again");
            //return View(model);

            //return Ok();
        }

        public ApplicationSignInManager SignInManager
        {
            get => _signInManager ?? Request.GetOwinContext().Get<ApplicationSignInManager>();
            private set => _signInManager = value;
        }

        //private readonly IAuthenticationManager _authmgr;

        public ApplicationUserManager UserManager
        {
            get => _userManager ?? Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            private set => _userManager = value;
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

        // POST api/<controller>/login
        public async Task<IHttpActionResult> Login(string Email, string Imei, string Password, bool RememberMe)
        {
            //if (!ModelState.IsValid)
            //{
            //    ViewData.Add("ActionMessage", "Error: invalid data supplied");
            //    return View(model);
            //}

            var model = new SignUpViewModel();

            if (Email.ToLower().Trim().Contains("unijos.edu.ng"))
            {
                var student = await _db.Students.AsNoTracking().Where(x => x.Email.Trim().ToUpper().Equals(Email.Trim().ToUpper())
                                    || x.PrimaryEmail.Trim().ToUpper().Equals(Email.Trim().ToUpper()))
                                 .FirstOrDefaultAsync();
                if (student != null)
                {
                    var userRecord = await _db.Users.Where(x => x.Email.Trim().ToUpper().Equals(student.PrimaryEmail.Trim().ToUpper())
                    || x.Id.Equals(student.StudentId)).FirstOrDefaultAsync();

                    //model.Email = student.Email;
                    //model.FirstName = student.FirstName;
                    //model.LastName = student.LastName;
                    //model.LevelId = student.LevelId;

                    if (userRecord != null)
                    {
                        userRecord.Email = student.Email;
                        userRecord.UserName = student.Email;
                        _db.Entry(userRecord).State = EntityState.Modified;
                        _db.SaveChanges();
                    }
                }
            }

            var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(c =>
                                c.Email.Trim().ToUpper().Equals(Email.ToUpper().Trim()));
            if (user == null)
            {
                var student = await _db.Students/*.AsNoTracking()*/
                                .Where(x => x.PrimaryEmail.Trim().ToUpper().Equals(Email.Trim().ToUpper()))
                                .FirstOrDefaultAsync();
                if (student != null)
                {
                    var message = "Please go to your email" + student.PrimaryEmail + "to get your new login " +
                            "credentials to enable you continue your registration";
                    return Ok(message);
                }
                else
                {
                    return BadRequest("Invalid Login Credential");
                    //return Ok(model);
                }
            }

            //if (!await UserManager.IsEmailConfirmedAsync(user.Id))
            //{

            //    var code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
            //    var callbackUrl = Url.Route("ConfirmEmail", "Account", new { userId = user.Id, code }, protocol: Request.Url.Scheme);
            //    string body = $"Dear {user.Email}. Please confirm your applicant account by clicking this link: <a href=\"{callbackUrl}" +
            //       "\">link</a> <p>Should in case you cant open the link <br/> Please copy the below link and paste if into a browser" +
            //       " <br/>" + callbackUrl + "</p>";
            //    await UserManager.SendEmailAsync(user.Id, "Confirm your account", body);
            //    return View("RedirectRegistration");
            //}

            var result = await SignInManager.PasswordSignInAsync(user.UserName, Password, RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    await ActivateUserLogin(user);

                    // Get payments for the student
                    var student = await _db.Students.AsNoTracking().Where(x => x.Email.Trim().ToUpper().Equals(Email.Trim().ToUpper())
                                    || x.PrimaryEmail.Trim().ToUpper().Equals(Email.Trim().ToUpper())).FirstOrDefaultAsync();

                    var schoolFeesPayment = await _db.SchoolFeePayments.Include(s => s.Students).Where(s => s.Students.StudentId.Equals(student.StudentId)).FirstOrDefaultAsync();
                    var idCardPayment = await _db.IdCardPayments.Where(s => s.Student.StudentId.Equals(student.StudentId)).FirstOrDefaultAsync();
                    
                    var details = new
                    {
                        Email = schoolFeesPayment.Students.Email,
                        FirstName = schoolFeesPayment.Students.FirstName,
                        LastName = schoolFeesPayment.Students.LastName,
                        LevelId = schoolFeesPayment.LevelId,
                        matricNo = schoolFeesPayment.Students.MatricNo,
                        schoolCharges = schoolFeesPayment.Status,
                        acceptanceFeePayment = schoolFeesPayment.IsPartPaymet,
                        idCardPay = idCardPayment.IsPayed
                    };

                    var detail = JsonConvert.SerializeObject(details);
                    //var jsonR =  new JsonResult()
                    //{
                    //    Data = detail,
                    //    MaxJsonLength = 86753090,
                    //    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    //};

                    return Ok(detail);

                //return RedirectToAction("CustomDashborad", new { username = user.UserName, returnUrl });

                case SignInStatus.LockedOut:
                    return Ok("Lockout");
                    //return View("Lockout");

                case SignInStatus.RequiresVerification:
                    return Ok("Login required verification");
                    //return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, model.RememberMe });

                case SignInStatus.Failure:
                default:
                    //ModelState.AddModelError("", "Invalid Login Attempt.");
                    return BadRequest("Login attempt failed");

                    //return View(model);
            }

            //return Ok(model);
        }

        // POST api/account/stafflogin
        // Generates bearer tokens for registered staff
        public async Task<IHttpActionResult> StaffLogin(string Email, string Password)
        {
            var rememberMe = false;
            var staff = await _db.Staffs.AsNoTracking().Where(x => x.Email.Trim().ToUpper().Equals(Email.Trim().ToUpper())).FirstOrDefaultAsync();

            //var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(c =>
            //                    c.Email.Trim().ToUpper().Equals(Email.ToUpper().Trim()));
            //if (user == null)
            //{
            //        return BadRequest("Invalid Login Credential");
            //}

            if (staff == null)
            {
                return BadRequest("No user with such details found");
            }

            var result = await SignInManager.PasswordSignInAsync(staff.Email, Password, rememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    //await ActivateUserLogin(user);

                    // Generate token
                    var roles = new string[] { "Admin" };
                    var jwtSecurityToken = Authentication.GenerateJwtToken(staff.Email, roles.ToList());
                    //var validUserName = Authentication.ValidateToken(jwtSecurityToken);

                    return Ok(jwtSecurityToken);

                case SignInStatus.LockedOut:
                    return Ok("Lockout");

                case SignInStatus.RequiresVerification:
                    return Ok("Login required verification");

                case SignInStatus.Failure:
                default:
                    return BadRequest("Login attempt failed");
            }
        }

        // POST api/<controller>
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<controller>/5
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
        }
    }
}