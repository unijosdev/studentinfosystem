using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampus.ViewModels;
using SwiftKampusModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SwiftKampus.Controllers
{
    [Authorize]
    [Audit(AuditingLevel = 2)]
    public class RolesAdminController : BaseController
    {

        public RolesAdminController(SchoolDbContext db) : base(db)
        {

        }
        private ApplicationUserManager _userManager;

        public ApplicationUserManager UserManager
        {
            get { return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>(); }
            private set { _userManager = value; }
        }

        // GET: RolesAdmin
        public ActionResult Index()
        {
            var roles = _db.Roles.AsNoTracking().ToList();
            return View(roles);
        }

        //GET: RolesAdmin/AddStaff/5
        [HttpGet]
        public PartialViewResult AddStaff(string id)
        {
            ViewBag.StaffId = id;
            ViewBag.StaffRoleName = new MultiSelectList(_db.Roles.AsNoTracking().ToList(), "Name", "Name");
            var staffRoleVm = new StaffRoleVm()
            {
                StaffId = id
            };
            return PartialView(staffRoleVm);
        }

        [HttpPost]
        public ActionResult AddStaff(StaffRoleVm model)
        {
            string message = string.Empty;
            foreach (var name in model.StaffRoleName)
            {
                UserManager.AddToRole(model.StaffId, name);
            }
            message = $"{model.StaffId} is assigned to Role Successfully.";
            return new JsonResult { Data = new { status = true, message } };
        }

        //GET: RolesAdmin/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: RolesAdmin/Create
        [HttpPost]
        public async Task<ActionResult> Create(FormCollection collection)
        {
            try
            {
                _db.Roles.Add(new IdentityRole()
                {
                    Name = collection["RoleName"]
                });
                await _db.SaveChangesAsync();

                ViewBag.Message = "Roles Added Successfully.";
                TempData["Title"] = "Success.";

                return RedirectToAction("Index");
            }
            catch
            {
                ViewBag.Message = "Role is not Added, Please try again later.";
                TempData["Title"] = "Error.";
                return View();
            }
        }

        // GET: RolesAdmin/Edit/5
        public ActionResult Edit(string roleName)
        {
            var thisRole = _db.Roles.FirstOrDefault(r => r.Name.Equals(roleName, StringComparison.CurrentCultureIgnoreCase));
            return View(thisRole);
        }

        // POST: /Roles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Microsoft.AspNet.Identity.EntityFramework.IdentityRole role)
        {
            try
            {
                _db.Entry(role).State = EntityState.Modified;
                _db.SaveChanges();
                ViewBag.ResultMessage = "Role Updated Successfully.";
                return RedirectToAction("Index");
            }
            catch
            {
                ViewBag.ResultMessage = "Update is Unsuccessful, Please try again later.";
                return View();
            }
        }

        public ActionResult ManageUserRoles()
        {
            // pre-populate roles for the view dropdown
            ViewBag.Roles = new SelectList(_db.Roles.AsNoTracking(), "Name", "Name");
            ViewBag.Username = new SelectList(_db.Staffs.AsNoTracking().OrderBy(x => x.LastName), "StaffId", "Username");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RoleAddToUser(string UserName, string RoleName)
        {
            ApplicationUser user = _db.Users.FirstOrDefault(u => u.Id.Equals(UserName, StringComparison.CurrentCultureIgnoreCase));
            UserManager.AddToRole(user?.Id, RoleName);

            ViewBag.ResultMessage = "Role created successfully !";

            // pre-populate roles for the view dropdown
            ViewBag.Username = new SelectList(_db.Staffs.AsNoTracking().OrderBy(x => x.LastName), "StaffId", "Username");
            ViewBag.Roles = new SelectList(_db.Roles.AsNoTracking(), "Name", "Name");
            return View("ManageUserRoles");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GetRoles(string Username)
        {
            if (!string.IsNullOrWhiteSpace(Username))
            {
                var user = _db.Users.AsNoTracking().FirstOrDefault(c => c.Id.Equals(Username));
                if (user == null)
                {
                    // var mlist = _db.Roles.OrderBy(r => r.Name).ToList().Select(rr => new
                    // SelectListItem { Value = rr.Name.ToString(), Text = rr.Name }).ToList();
                    ViewBag.Roles = new SelectList(_db.Roles.AsNoTracking(), "Name", "Name");
                    ViewBag.Username = new SelectList(_db.Staffs.AsNoTracking().OrderBy(x => x.LastName), "StaffId", "Username");
                    ViewBag.ResultMessage = "Couldn't Find User.";
                    return View("ManageUserRoles");
                }
                //var account = new AccountController();

                ViewBag.RolesForThisUser = UserManager.GetRoles(user.Id);
                ViewBag.Username = new SelectList(_db.Staffs.AsNoTracking(), "StaffId", "Username");
                ViewBag.Roles = new SelectList(_db.Roles.AsNoTracking(), "Name", "Name");
            }

            return View("ManageUserRoles");
        }

        // GET: RolesAdmin/Delete/5
        public ActionResult Delete(string RoleName)
        {
            var thisRole = _db.Roles.AsNoTracking().FirstOrDefault(r => r.Name.Equals(RoleName, StringComparison.CurrentCultureIgnoreCase));
            _db.Roles.Remove(thisRole);
            _db.SaveChanges();
            ViewBag.ResultMessage = "Role deleted Successfully.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteRoleForUser(string UserName, string RoleName)
        {
            //var account = new AccountController();
            ApplicationUser user = _db.Users.AsNoTracking().FirstOrDefault(u => u.Id.Equals(UserName, StringComparison.CurrentCultureIgnoreCase));

            if (user != null && UserManager.IsInRole(user.Id, RoleName))
            {
                UserManager.RemoveFromRole(user.Id, RoleName);
                ViewBag.ResultMessage = "Role removed from this user successfully !";
            }
            else
            {
                ViewBag.ResultMessage = "This user doesn't belong to selected role.";
            }

            ViewBag.Roles = new SelectList(_db.Roles.AsNoTracking(), "Name", "Name");
            ViewBag.Username = new SelectList(_db.Staffs.AsNoTracking().OrderBy(x => x.LastName), "StaffId", "Username");
            return View("ManageUserRoles");
        }
    }
}