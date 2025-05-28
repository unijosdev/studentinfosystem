using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Google;
using Microsoft.Owin.Security.MicrosoftAccount;
using Owin;
using SwiftKampus.Models;
using SwiftKampusModel;
using System;

namespace SwiftKampus
{
    public partial class Startup
    {
        // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            // Configure the db context, user manager and signin manager to use a single instance per request
            app.CreatePerOwinContext(SchoolDbContext.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);

            // Enable the application to use a cookie to store information for the signed in user
            // and to use a cookie to temporarily store information about a user logging in with a third party login provider
            // Configure the sign in cookie
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                // add these lines
                SlidingExpiration = true,
                ExpireTimeSpan = TimeSpan.FromMinutes(20),
                // rest of your code
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login"),
                Provider = new CookieAuthenticationProvider
                {
                    // Enables the application to validate the security stamp when the user logs in.
                    // This is a security feature which is used when you change a password or add an external login to your account.  
                    OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, ApplicationUser>(
                        validateInterval: TimeSpan.FromMinutes(30),
                        regenerateIdentity: (manager, user) => user.GenerateUserIdentityAsync(manager))
                }
            });
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            // Enables the application to temporarily store user information when they are verifying the second factor in the two-factor authentication process.
            app.UseTwoFactorSignInCookie(DefaultAuthenticationTypes.TwoFactorCookie, TimeSpan.FromMinutes(5));

            // Enables the application to remember the second login verification factor such as phone or email.
            // Once you check this option, your second step of verification during the login process will be remembered on the device where you logged in from.
            // This is similar to the RememberMe option when you log in.
            app.UseTwoFactorRememberBrowserCookie(DefaultAuthenticationTypes.TwoFactorRememberBrowserCookie);

            // Uncomment the following lines to enable logging in with third party login providers
            var microsftaccountOption = new MicrosoftAccountAuthenticationOptions()
            {

                ClientId = "971589bb-0530-4c4d-89ba-ccdde0b5579b",
                ClientSecret = "9DFF4CCA96473ECD7E152157AF70FBCF26CAF5DE",
                //Provider = new MicrosoftAccountAuthenticationProvider()
                //{
                //    OnAuthenticated = async context =>
                //    {
                //        context.Identity.AddClaim(new System.Security.Claims.Claim("MicrosoftAccessToken", context.AccessToken));
                //        foreach (var claim in context.User)
                //        {
                //            var claimType = String.Format("urn:microsoft:{0}", claim.Key);
                //            string claimValue = claim.Value.ToString();
                //            if (!context.Identity.HasClaim(claimType, claimValue))
                //            {
                //                context.Identity.AddClaim(new System.Security.Claims.Claim(claimType, claimValue,
                //                                            "XmlSchemasString", "Microsoft"));
                //            }
                //        }
                //    }
                //}
            };
            //microsftaccountOption.Scope.Add("wl.basic");
            //microsftaccountOption.Scope.Add("wl.emails");
            app.UseMicrosoftAccountAuthentication(microsftaccountOption);
            //app.UseMicrosoftAccountAuthentication(
            //    clientId: "971589bb-0530-4c4d-89ba-ccdde0b5579b",
            //    clientSecret: "9DFF4CCA96473ECD7E152157AF70FBCF26CAF5DE");


            //app.UseTwitterAuthentication(
            //   consumerKey: "",
            //   consumerSecret: "");

            //app.UseFacebookAuthentication(
            //   appId: "",
            //   appSecret: "");

            app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions()
            {
                ClientId = "779181765136 - qka9kon6klese1e0ktha5egct2mpt0aj.apps.googleusercontent.com",
                ClientSecret = "GpTnmQ3C8tgtrii2UPRSr0HH"
            });
        }
    }
}