using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwiftKampusModel.Accomodation
{
    public class AccommodationFeePayment
    {
        [Key, ForeignKey("AssignedRoom")]
        public int AssignedRoomId { get; set; }
        public string StudentId { get; set; }
        public string ReferenceNo { get; set; }

        [Index(IsUnique = true)]
        [MaxLength(50)]
        [Required]
        public string OrderId { get; set; }
        public int SessionId { get; set; }
        public double ExpectedAmount { get; set; }
        public double AmountPayed { get; set; }
        public bool IsPayed { get; set; }
        public string TransactionMessage { get; set; }
        public DateTime PaymentDateTime { get; set; }
        public virtual Student Student { get; set; }
        public virtual Session Session { get; set; }
        public virtual AssignedRoom AssignedRoom { get; set; }
    }
}
