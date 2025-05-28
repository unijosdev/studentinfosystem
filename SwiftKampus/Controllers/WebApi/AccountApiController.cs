using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;

using SwiftKampus.Models;
using SwiftKampusModel;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;



namespace SwiftKampus.Controllers.WebApi
{
    [RoutePrefix("AccountApi")]
    public class AccountApiController : ApiController
    {
        private ApplicationUserManager _userManager;
        private ApplicationSignInManager _signInManager;
        private readonly SchoolDbContext _db;

        public AccountApiController(SchoolDbContext db)
        {
            _db = db;
        }

        public AccountApiController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            _db = new SchoolDbContext();
            SignInManager = signInManager;
        }
        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? Request.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ISecureDataFormat<AuthenticationTicket> AccessTokenFormat { get; set; }

        public AccountApiController(ApplicationUserManager userManager,
            ISecureDataFormat<AuthenticationTicket> accessTokenFormat)
        {
            AccessTokenFormat = accessTokenFormat;
            UserManager = userManager;
        }

        public ApplicationUserManager UserManager
        {
            get { return _userManager ?? Request.GetOwinContext().GetUserManager<ApplicationUserManager>(); }
            private set { _userManager = value; }
        }



        [HttpPost]
        [Route("SignUp", Name = "SignUp")]
        [ResponseType(typeof(SignUpViewModel))]
        public async Task<IHttpActionResult> SignUp(string id)
        {
            var model = new SignUpViewModel();

            if (id != null)
            {

                var checkStudent = await _db.Students.AsNoTracking().Include(i => i.Programme)
                            .Where(x => x.MatricNo.ToUpper().Equals(id.ToUpper().Trim()) ||
                                    x.JambRegNo.ToUpper().Equals(id.ToUpper())
                                    || x.StudentId.Equals(id)).FirstOrDefaultAsync();

                if (checkStudent != null)
                {
                    if (checkStudent.Active.Equals(false))
                    {
                        if (checkStudent.MatricNo != null)
                        {
                            model.UserId = checkStudent.MatricNo;
                        }
                        else
                        {
                            model.UserId = checkStudent.JambRegNo;
                        }
                        model.FirstName = checkStudent.FirstName;
                        model.LastName = checkStudent.LastName;
                        model.Department = checkStudent.Programme.ProgrammeName;
                        return Ok(model);
                    }
                    return BadRequest("Account Already Activated");

                }

                return BadRequest("Student Record  Doesn't Exist");

            }
            return BadRequest("Student Id cannot be null");
        }



        [HttpPost]
        [Route("RegisterStudent", Name = "RegisterStudent")]
        public async Task<IHttpActionResult> RegisterStudent(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var student = await _db.Students.AsNoTracking().Where(x => (x.MatricNo.Equals(model.StudentId.Trim())
                                    || x.JambRegNo.Equals(model.StudentId)) && x.IsGraduated.Equals(false))
                                    .FirstOrDefaultAsync();
                if (student != null)
                {
                    //var checkImel = await  _db.Users.AsNoTracking().Where(x => x.ImeiNo.Equals(model.Imei)).ToListAsync();
                    //if (checkImel != null)
                    //{
                    //    return BadRequest("Imel is already used");
                    //}

                    var user = new ApplicationUser
                    {
                        Id = student.StudentId,
                        UserName = student.FullName,
                        Email = model.Email,
                        StudentId = student.MatricNo,
                        ImeiNo = model.Imei
                    };
                    var result = await UserManager.CreateAsync(user, model.Password);
                    if (result.Succeeded)
                    {
                        student.Email = model.Email;
                        student.ImeiNo = model.Imei;
                        student.Active = true;
                        _db.Entry(student).State = EntityState.Modified;

                        await _db.SaveChangesAsync();

                        await this.UserManager.AddToRoleAsync(user.Id, "Student");

                        var code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                        var callbackUrl = Get(user.Id, code);
                        await UserManager.SendEmailAsync(user.Id, "Confirm your account",
                            "Please confirm your student account by clicking this link: <a href=\"" + callbackUrl +
                            "\">link</a> <p>Should in case you cant open the link <br/> Please copy the below link and paste if into a browser" +
                            " <br/>" + callbackUrl + "</p>");

                        return Ok("Registration Successful");
                    }
                    return BadRequest(result.ToString());
                }
                return BadRequest("Student Not Found");
            }

            // If we got this far, something failed, redisplay form
            return BadRequest("Model Not Valid");
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("Login", Name = "Login")]
        public async Task<IHttpActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Model Not Correct");
            }

            var studentApi = new GetStudent();

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true

            var student = await _db.Students.AsNoTracking().Where(x => (x.MatricNo.Equals(model.Email.Trim())
                                        || x.JambRegNo.Equals(model.Email)
                                        || x.Email.ToUpper().Equals(model.Email.ToUpper())
                                        || x.StudentId.Equals(model.Email.Trim()))
                                        && x.IsGraduated.Equals(false))
                                        .FirstOrDefaultAsync();

            if (student != null)
            {
                studentApi.StudentId = student.StudentId;
                var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(c =>
                    c.Id.ToUpper().Equals(student.StudentId.ToUpper().Trim()));
                if (user != null)
                {
                    if (user.ImeiNo.Equals(model.Imei))
                    {

                        if (!await UserManager.IsEmailConfirmedAsync(user.Id))
                        {
                            return BadRequest("User needs to confirm registered email");

                            //return View("Error");
                        }
                        var result = await SignInManager.PasswordSignInAsync(user.UserName, model.Password,
                            model.RememberMe, shouldLockout: false);
                        //if (result != 0)
                        //{
                        //    result = await SignInManager.PasswordSignInAsync(user.Email, model.Password, model.RememberMe, shouldLockout: false);
                        //}
                        switch (result)
                        {
                            case SignInStatus.Success:
                                return Ok(studentApi);
                            default:
                                return BadRequest(
                                    "Error, Please login from your device and check your Student Id and password carefully");
                        }
                    }
                    return BadRequest("Imei Not correct");
                }

            }
            return BadRequest("No Student record found");
        }

        public string Get(string userId, string code)
        {
            var url = this.Url.Link("Default", new { Controller = "Account", Action = "ConfirmEmail", userId, code });
            return url;
        }
    }


    public class MySignUp
    {
        public string Id { get; set; }
    }
}

public class GetStudent
{
    public string StudentId { get; set; }
}
