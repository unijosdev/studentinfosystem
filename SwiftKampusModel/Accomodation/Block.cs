using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SwiftKampusModel.Accomodation
{
    public class Block
    {
        [Key]
        public int BlockId { get; set; }

        [Display(Name = "Hostel Name")]
        [Required(ErrorMessage = "Your Hostel Name is required")]
        public int HostelId { get; set; }

        [Display(Name = "Block Name")]
        [Required(ErrorMessage = "Your Block Name is required")]
        public string BlockName { get; set; }

        [Display(Name = "Building Capacity(Room)")]
        //[Required(ErrorMessage = "Your Building Capacity is required")]
        public int Capacity { get; set; }

        [Display(Name = "Select Gender of Occupants")]
        [Required(ErrorMessage = "Your Hostel Name is required")]
        public string Gender { get; set; }

        public virtual Hostel Hostel { get; set; }
        public virtual ICollection<Room> Rooms { get; set; }
        public virtual ICollection<SessionAccomodation> SessionAccomodations { get; set; }
        //public virtual ICollection<AssignedRoom> AssignedRooms { get; set; }
        public virtual ICollection<StudentAssignedRoom> StudentAssignedRooms { get; set; }


    }
}