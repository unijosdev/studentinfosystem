using System.ComponentModel.DataAnnotations;

namespace SwiftKampusModel
{
    public class DeptPosition
    {
        public int DeptPositionId { get; set; }

        [Display(Name = "Department Executive Position")]
        [Required(ErrorMessage = "Department Position is required")]
        public int StaffPositionId { get; set; }

        [Display(Name = "Department")]
        [Required(ErrorMessage = "Department is required")]
        public int DepartmentId { get; set; }

        [Display(Name = "Staff Name")]
        [Required(ErrorMessage = "Staff Name is required")]
        public string StaffId { get; set; }

        [Display(Name = "Session")]
        [Required(ErrorMessage = "Session is required")]
        public int SessionId { get; set; }

        [Display(Name = "Current Position Holder")]
        public bool IsActive { get; set; }

        public virtual Department Department { get; set; }
        public virtual StaffPosition StaffPosition { get; set; }
        public virtual Session Session { get; set; }
        public virtual Staff Staff { get; set; }

    }
}
