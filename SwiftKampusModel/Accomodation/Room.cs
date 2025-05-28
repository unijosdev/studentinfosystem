using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SwiftKampusModel.Accomodation
{
    public class Room
    {
        [Key]
        public int RoomId { get; set; }

        [Display(Name = "Building Name")]
        [Required(ErrorMessage = "Your Building Name is required")]
        public int BlockId { get; set; }

        [Display(Name = "Room")]
        [Required(ErrorMessage = "Your Room is required")]
        public string RoomName { get; set; }

        [Display(Name = "Available Number of Space")]
        [Required(ErrorMessage = "Your Room Capacity is required")]
        public int RoomCapacity { get; set; }

        public virtual Block Buildings { get; set; }
        public virtual ICollection<SessionAccomodation> SessionAccomodations { get; set; }
        //public virtual ICollection<AssignedRoom> AssignedRooms { get; set; }
        public virtual ICollection<StudentAssignedRoom> StudentAssignedRooms { get; set; }
        public virtual ICollection<ReservedRoom> ReservedRooms { get; set; }
       // public virtual ICollection<AssignRoomToLevel> AssignRoomToLevels { get; set; }
    }
}