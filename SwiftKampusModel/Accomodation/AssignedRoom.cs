using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwiftKampusModel.Accomodation
{
    public class AssignedRoom
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AssignedRoomId { get; set; }

        [Display(Name = "Matric No")]
        public string StudentId { get; set; }

        public int SessionId { get; set; }

        public int? HostelId { get; set; }

        public int? BlockId { get; set; }

        public int RoomId { get; set; }
        public string BedSpace { get; set; }
        public bool PaymentStatus { get; set; }
        public bool? IsSpecialAllocation { get; set; }
        public DateTime? AssignedDate { get; set; }
        public DateTime ReleaseDate { get; set; }
        public Room Room { get; set; }
        public Block Block { get; set; }
        public Hostel Hostel { get; set; }

        public Student Student { get; set; }

        public Session Session { get; set; }
        public AccommodationFeePayment AccommodationFeePayments { get; set; }
    }
}