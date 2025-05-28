using System.Collections.Generic;

namespace SwiftKampusModel
{
    public class Permission
    {
        public int PermissionId { get; set; }

        public string PermissionName { get; set; }

        public string Description { get; set; }

        public ICollection<RolePermissions> Roles { get; set; }
    }
}
