using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;

namespace SwiftKampusModel
{
    public class ApplicationRole : IdentityRole
    {

        public ApplicationRole()
        {

        }

        public ApplicationRole(string name) : base(name)
        {

        }

        public ICollection<RolePermissions> Permissions { get; set; }
    }
}