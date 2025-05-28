using System.Web;
using System.Web.Mvc;

namespace SwiftKampus.Services
{
    public class RecurringAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var reauth = (bool?)httpContext.Session["ReAuthenticated"];
            var result = base.AuthorizeCore(httpContext) && (reauth ?? false);
            httpContext.Session["ReAuthenticated"] = !result;
            return result;
        }
    }
}