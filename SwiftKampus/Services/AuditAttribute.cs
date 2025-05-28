using SwiftKampus.Models;
using SwiftKampusModel;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Security;

namespace SwiftKampus.Services
{
    public class AuditAttribute : ActionFilterAttribute
    {
        public int AuditingLevel { get; set; }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var data = filterContext.Controller.ViewData.Where(x => x.Key.Equals("ActionMessage"))
                        .Select(s => s.Value).FirstOrDefault();
            //Stores the Request in an Accessible object
            var request = filterContext.HttpContext.Request;

            //Generate the appropriate key based on the user's Authentication Cookie
            //This is overkill as you should be able to use the Authorization Key from
            //Forms Authentication to handle this. 
            //var sessionIdentifier = string.Join("", MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(request.Cookies[FormsAuthentication.FormsCookieName].Value)).Select(s => s.ToString("x2")));
            var sessionIdentifier = string.Empty;
            var requestCookie = request.Cookies[FormsAuthentication.FormsCookieName];
            if (requestCookie != null)
            {
                sessionIdentifier = string.Join("",
                                    MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(requestCookie.Value))
                                    .Select(s => s.ToString("x2")));
            }
            //Generate an audit
            Audit audit = new Audit
            {
                SessionId = sessionIdentifier,
                AuditId = Guid.NewGuid(),
                IpAddress = request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? request.UserHostAddress,
                UrlAccessed = request.RawUrl,
                TimeAccessed = DateTime.UtcNow,
                ActionPerformed = data != null ? data.ToString() : "",
                UserName = (request.IsAuthenticated) ? filterContext.HttpContext.User.Identity.Name : "Anonymous",
                Data = SerializeRequest(request)
            };

            // Stores the Audit in the Database
            using (var db = new SchoolDbContext())
            {
                db.AuditRecords.Add(audit);
                db.SaveChanges();
            }

            base.OnActionExecuted(filterContext);
        }


        //public override void OnActionExecuting(ActionExecutingContext filterContext)
        //{



        //    // Finishes executing the Action as normal 
        //    base.OnActionExecuting(filterContext);
        //}

        // This will serialize the Request object based on the 
        // level that you determine
        private string SerializeRequest(HttpRequestBase request)
        {
            switch (AuditingLevel)
            {
                // No Request Data will be serialized
                case 0:
                default:
                    return "";
                // Basic Request Serialization - just stores Data
                case 1:
                    return Json.Encode(new { request.Cookies, request.Headers, request.Files });
                // Middle Level - Customize to your Preferences
                case 2:
                    return Json.Encode(new { request.Cookies, request.Headers, request.Files, request.Form,  });
                // Highest Level - Serialize the entire Request object (As mentioned earlier, this will blow up)
                case 3:
                    // We can't simply just Encode the entire 
                    // request string due to circular references 
                    // as well as objects that cannot "simply" 
                    // be serialized such as Streams, References etc.
                    return Json.Encode(request);
            }
        }
    }
}