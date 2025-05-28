using System.ComponentModel.DataAnnotations;

namespace SwiftKampusModel
{
    public class Executive
    {
        public int ExecutiveId { get; set; }

        [Display(Name = "Executive Position")]
        [Required(ErrorMessage = "Executive Position is required")]
        public int StaffPositionId { get; set; }

        [Display(Name = "Staff Name")]
        [Required(ErrorMessage = "Staff Name is required")]
        public string StaffId { get; set; }

        [Display(Name = "Session Name")]
        [Required(ErrorMessage = "Session Name is required")]
        public int SessionId { get; set; }

        [Display(Name = "Current Position Holder")]
        public bool IsActive { get; set; }

        public virtual StaffPosition StaffPosition { get; set; }
        public virtual Session Session { get; set; }
        public virtual Staff Staff { get; set; }
    }
}
