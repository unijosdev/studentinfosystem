using System;
using System.Web.Mvc;

namespace SwiftKampus.CustomFilters
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class AuthLogAttribute : AuthorizeAttribute
    {
        public AuthLogAttribute()
        {
            View = "Authorization Failed";
        }

        public string View { get; set; }

        /// <summary>
        /// Check for Authorization
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);
            IsUserAuthorized(filterContext);
        }

        /// <summary>
        /// Method to check if the user is Authorized or not
        /// if yes continue to perform the action else redirect to error page
        /// </summary>
        /// <param name="filterContext"></param>
        private void IsUserAuthorized(AuthorizationContext filterContext)
        {
            // If the Result returns null then the user is Authorized 
            if (filterContext.Result == null)
                return;

            //If the user is Un-Authorized then Navigate to Auth Failed View 
            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
            {

                // var result = new ViewResult { ViewName = View };
                var vr = new ViewResult
                {
                    ViewName = View
                };

                ViewDataDictionary dict = new ViewDataDictionary
                {
                    { "Message", "Sorry you are not Authorized to Perform this Action, Please contact System Administrator" }
                };

                vr.ViewData = dict;

                var result = vr;

                filterContext.Result = result;
            }
        }
    }
}

