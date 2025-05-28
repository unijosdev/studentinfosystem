using SwiftKampusModel;
using System.Collections.Generic;

namespace SwiftKampus.ViewModels
{
    public class RolesAdminVM
    {

    }

    public class CreateRoleVM
    {
        public string RoleName { get; set; }
    }

    public class EditRoleVM
    {
        public string RoleName { get; set; }
        
        public string RoleId { get; set; }
    }

    public class ManageUserRolesVM
    {

        public string RoleID { get; set; }

        public IEnumerable<ApplicationRole> UserRoles { get; set; }

        public string StaffID { get; set; }

        public IEnumerable<Staff> Staff { get; set; }
    }
}