using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using SwiftKampusModel;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace SwiftKampus.Models
{
    public class ApplicationRoleManager : RoleManager<ApplicationRole>
    {
        public ApplicationRoleManager(IRoleStore<ApplicationRole, string> store) : base(store)
        {
        }

        public static ApplicationRoleManager Create(IdentityFactoryOptions<ApplicationRoleManager> opts,
            IOwinContext context)
        {
            return new ApplicationRoleManager(new RoleStore<ApplicationRole>(
                context.Get<SchoolDbContext>()));
        }

        public Task<ApplicationRole> FindByIdNonTrackingAsync(string roleId)
        {
            return this.Roles.AsNoTracking().Where(r => r.Id == roleId).SingleOrDefaultAsync();
        }
    }
}