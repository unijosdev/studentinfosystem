using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;

using SendGrid;
using SendGrid.Helpers.Mail;

using SwiftKampus.BusinessLogic;
using SwiftKampus.Models;
using SwiftKampus.Services;

using SwiftKampusModel;

using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SwiftKampus
{
    public class EmailService : IIdentityMessageService
    {
        public async Task SendAsync(IdentityMessage message)
        {
            // Plug in your email service here to send an email.          
            using (var db = new SchoolDbContext())
            {
                var query = new QueryCommand(db);
                var username = await query.GetUserFullName(message.Destination);

                // **********Begin Sendgrid Implementation ****************//
                ////var apiKey = "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";
                //var apiKey = "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";
                //var client = new SendGridClient(apiKey);
                //var from = new EmailAddress($"noreply@unijos.com", "University of Jos");
                //var subject = !string.IsNullOrEmpty(message.Subject) ? message.Subject : "UNIJOS NOTIFICATION";
                //var to = new EmailAddress(message.Destination, username);
                //var plainTextContent = message.Body;
                //var htmlContent = message.Body;
                //var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
                //try
                //{
                //    var response = await client.SendEmailAsync(msg);
                //}
                //catch (Exception ex)
                //{
                //    Console.WriteLine(ex.Message);
                //}

                // **********End Sendgrid Implementation ****************//

                // **********Begin Office 365 Implementation ****************//

                var subject = !string.IsNullOrEmpty(message.Subject) ? message.Subject : "UNIJOS NOTIFICATION";

                MailMessage msg = new MailMessage();
                msg.To.Add(new MailAddress(message.Destination, username));
                msg.From = new MailAddress("notification@unijos.edu.ng", "UNIJOS");
                msg.Subject = subject;
                msg.Body = message.Body;
                msg.IsBodyHtml = true;


                //SmtpClient client = new SmtpClient("domain-com.mail.protection.outlook.com");
                SmtpClient client = new SmtpClient
                {
                    UseDefaultCredentials = false,
                    Credentials = new System.Net.NetworkCredential("notification@unijos.edu.ng", "UjPortal098"),
                    Host = "smtp.office365.com",
                    EnableSsl = true,
                    TargetName = "STARTTLS/smtp.office365.com",
                    Port = 25, // You can use Port 25 if 587 is blocked (mine is!)
                    DeliveryMethod = SmtpDeliveryMethod.Network
                };
                try
                {
                    client.Send(msg);

                }
                catch (Exception ex)
                {
                     Console.WriteLine(ex.Message);
                }

                // **********Begin Office 365 Implementation ****************//

            }

            //string schoolName = "UNIJOS";
            //string emailsetting = ConfigurationManager.AppSettings["GmailUserName"];
            //MailMessage email = new MailMessage(new MailAddress($"noreply{emailsetting}", "(UNIJOS NOTIFICATION, do not reply)"),
            //new MailAddress(message.Destination));

            //email.Subject = message.Subject;
            //email.Body = message.Body;
            //email.IsBodyHtml = true;

            //using (var mailClient = new EmailSetUpServices())
            //{
            //    //In order to use the original from email address, uncomment this line:
            //    email.From = new MailAddress(mailClient.UserName, $"(do not reply {schoolName})");

            //    await mailClient.SendMailAsync(email);
            //}
        }
    }

    public class SmsService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your SMS service here to send a text message.
            SmsServicesCustom _smsService = new SmsServicesCustom();

            string body = message.Body;
            string destination = message.Destination;
            SMS sms = new SMS()
            {
                SenderId = "UNIJOS",
                Message = body,
                Numbers = destination
            };
            string response = _smsService.Send(sms); //Send sms

            string code = _smsService.GetResponseMessage(response, out bool isSuccess, out string errMsg);

            if (!isSuccess)
            {
                isSuccess = false;
            }
            else
            {
                isSuccess = true;
            }

            return Task.FromResult(true);
        }
    }

    // Configure the application user manager used in this application. UserManager is defined in
    // ASP.NET Identity and is used by the application.
    public class ApplicationUserManager : UserManager<ApplicationUser>
    {
        public ApplicationUserManager(IUserStore<ApplicationUser> store)
            : base(store)
        {
        }

        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context)
        {
            var manager = new ApplicationUserManager(new UserStore<ApplicationUser>(context.Get<SchoolDbContext>()));
            // Configure validation logic for usernames
            manager.UserValidator = new UserValidator<ApplicationUser>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };

            // Configure validation logic for passwords
            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 6,
                RequireNonLetterOrDigit = false,
                RequireDigit = false,
                RequireLowercase = false,
                RequireUppercase = false,
            };

            // Configure user lockout defaults
            manager.UserLockoutEnabledByDefault = true;
            manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
            manager.MaxFailedAccessAttemptsBeforeLockout = 5;

            // Register two factor authentication providers. This application uses Phone and Emails
            // as a step of receiving a code for verifying the user You can write your own provider
            // and plug it in here.
            manager.RegisterTwoFactorProvider("Phone Code", new PhoneNumberTokenProvider<ApplicationUser>
            {
                MessageFormat = "Your security code is {0}"
            });
            manager.RegisterTwoFactorProvider("Email Code", new EmailTokenProvider<ApplicationUser>
            {
                Subject = "Security Code",
                BodyFormat = "Your security code is {0}"
            });
            manager.RegisterTwoFactorProvider("GoogleAuthenticator", new GoogleAuthenticatorTokenProvider());
            manager.EmailService = new EmailService();
            manager.SmsService = new SmsService();
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider =
                    new DataProtectorTokenProvider<ApplicationUser>(dataProtectionProvider.Create("ASP.NET Identity"));
            }
            return manager;
        }
    }

    // Configure the application sign-in manager which is used in this application.
    public class ApplicationSignInManager : SignInManager<ApplicationUser, string>
    {
        public ApplicationSignInManager(ApplicationUserManager userManager, IAuthenticationManager authenticationManager)
            : base(userManager, authenticationManager)
        {
        }

        public override Task<ClaimsIdentity> CreateUserIdentityAsync(ApplicationUser user)
        {
            return user.GenerateUserIdentityAsync((ApplicationUserManager)UserManager);
        }

        public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context)
        {
            return new ApplicationSignInManager(context.GetUserManager<ApplicationUserManager>(), context.Authentication);
        }
    }

    public class CustomSms
    {
        private readonly SchoolDbContext _db;
        private readonly SmsServicesCustom _smsService;

        public CustomSms()
        {
            _db = new SchoolDbContext();
            _smsService = new SmsServicesCustom();
        }

        public async Task<Task> SendStudentMsgAsync(SmsToStudent message)
        {
            // Plug in your SMS service here to send a text message.
            string body = message.Body;
            string destination = await _db.Users.AsNoTracking().Where(x => x.UserName.Equals(message.Destination))
                                    .Select(c => c.PhoneNumber).FirstOrDefaultAsync();
            SMS sms = new SMS()
            {
                SenderId = "UNIJOS",
                Message = body,
                Numbers = destination
            };
            string response = _smsService.Send(sms); //Send sms

            string code = _smsService.GetResponseMessage(response, out bool isSuccess, out string errMsg);

            if (!isSuccess)
            {
                isSuccess = false;
            }
            else
            {
                isSuccess = true;
            }

            return Task.FromResult(true);
        }

        public Task SendUnknowMsgAsync(SmsToStudent message)
        {
            // Plug in your SMS service here to send a text message.
            string body = message.Body;

            SMS sms = new SMS()
            {
                SenderId = "UNIJOS",
                Message = body,
                Numbers = message.Destination
            };
            string response = _smsService.Send(sms); //Send sms

            string code = _smsService.GetResponseMessage(response, out bool isSuccess, out string errMsg);

            if (!isSuccess)
            {
                isSuccess = false;
            }
            else
            {
                isSuccess = true;
            }

            return Task.FromResult(true);
        }
    }

    public class SmsServiceTemp
    {
        private readonly ConfigService _config;
        //private Cache _cache;

        //private string sessionId_cahe_key = "SmsSessionId_" + "GetSessionId";

        public SmsServiceTemp()
        {
            _config = new ConfigService();
            //_cache = HttpContext.Current.Cache;
        }

        //Default method for making request to the SMS gateway. This method is not likely to be changed no matter what
        //SMS gateway provider you want to use in the future.
        private string MakeHttpRequest(string url)
        {
            //Initialize the web request
            var webReq = (HttpWebRequest)WebRequest.Create(url);
            webReq.ContentLength = 0;

            webReq.Method = "POST";//We're making a post request. This is the recommended method by the gateway.
            webReq.Timeout = 600000;//Set the timeout for the request

            var webResp = (HttpWebResponse)webReq.GetResponse();

            //Read the response and output it.
            Stream answer = webResp.GetResponseStream();
            StreamReader _answer = new StreamReader(answer);

            string result = _answer.ReadToEnd();

            return result;
        }

        //Process the response from the sms gateway. By default, the gateway's response is in format below
        //OK: [RESPONSE-Message] -or- ERR: [ERROR NUMBER]: [ERROR DESCRIPTION]
        public string GetResponseMessage(string response, out bool success, out string errMsg)
        {
            //if the response contains 'OK', then the request was successful
            //bool isSuccess = response.Substring(0, response.IndexOf(":") + 1).Contains("OK");
            bool isSuccess = false;
            string errDesc = null;
            string code = null;
            string myresponse = response;

            if (myresponse.ToString().Contains("OK"))
            {
                isSuccess = true;
            }
            else if (myresponse.ToString().Contains("2907"))
            {
                errDesc = "Messaging Service not available";
                code = "FAIL";
            }
            else if (myresponse.ToString().Contains("2906"))
            {
                errDesc = "Insufficient Credit for messaging";
                code = "FAIL";
            }
            else
            {
                errDesc = "Error Sending Message";
                code = "FAIL";
            }

            success = isSuccess;
            errMsg = errDesc;
            return code;
        }

        public string Send(Sms sms)
        {
            //string sessionId = GetSessionId(); //Get the session id
            string smsUrl = _config.SmsUrl; //Get the sms gateway url from the config file

            //Form the command for sending message. You can download the API documentation for full list of commands
            //from http://kudisms.net
            string smsCmd = $"username={_config.SmsAccount}&password={_config.SubAccountPwd}&sender={sms.Sender}" +
                            $"&recipient={sms.Recipient}&message={sms.Message}";

            //Send sms message
            string response = MakeHttpRequest(smsUrl + smsCmd);

            //Process the response from the gateway
            string code = GetResponseMessage(response, out bool isSuccess, out string errMsg);

            ////401 error code indicate invalid Session ID. If the session id is not valid, then delete it from cache and make a
            ////request to get a new session id from the sms gateway
            //if (code == "401")
            //{
            //    _cache.Remove(sessionId_cahe_key);//delete the session id from the cache

            // sessionId = GetSessionId(); //Get the session id smsCmd =
            // String.Format("?cmd=sendmsg&sessionid={0}&message={1}&sender={2}" +
            // "&sendto={3}&msgtype=0", sessionId, sms.Message, sms.SenderId, sms.Numbers);

            //    return makeHttpRequest(smsUrl + smsCmd);//resend the sms to the gateway
            //}

            return response;
        }
    }

    public class Sms
    {
        public string Recipient { get; set; }

        public string Sender { get; set; }

        //[Display(Name = "Message")]
        //[DataType(DataType.MultilineText)]
        public string Message { get; set; }
    }
}