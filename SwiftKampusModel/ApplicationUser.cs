using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SwiftKampusModel
{
    public class ApplicationUser : IdentityUser
    {
        public bool IsGoogleAuthenticatorEnabled { get; set; }
        public string GoogleAuthenticatorSecretKey { get; set; }
        public string StudentId { get; set; }
        public string StaffId { get; set; }
        public string JambRegNo { get; set; }
        public string ImeiNo { get; set; }
        public bool IsLogin { get; set; }
        public string PrimaryMail { get; set; }
        public DateTime? LastLogin { get; set; }
        public DateTime? LastLogOut { get; set; }
        public int NumberOfLogins { get; set; }
        public ICollection<ChatConnection> ChatConnection { get; set; }


        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }
}
