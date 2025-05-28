using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SwiftKampusModel.Accomodation
{
    public class Hostel
    {
        [Key]
        public int HostelId { get; set; }

        [Display(Name = "Hostel Code")]
        [Required(ErrorMessage = "Your Hostel Code is required")]
        public string HostelCode { get; set; }

        [Display(Name = "Hostel Name")]
        [Required(ErrorMessage = "Your Hostel Name is required")]
        public string HostelName { get; set; }  

        [Required(ErrorMessage = "Your Hostel Price is required")]
        public decimal HostelPrice { get; set; }

        [Display(Name = "Remark")]
        [Required(ErrorMessage = "Your Hostel Name is required")]
        public string Remark { get; set; }

        public virtual ICollection<Block> Blocks { get; set; }
        public virtual ICollection<SessionAccomodation> SessionAccomodations { get; set; }
        //public virtual ICollection<AssignedRoom> AssignedRooms { get; set; }
        public virtual ICollection<StudentAssignedRoom> StudentAssignedRooms { get; set; }
        public virtual ICollection<AssignedHostel> AssignedHostels { get; set; }
    }
}