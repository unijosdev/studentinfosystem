namespace SwiftKampusModel.Accomodation
{
    public class SessionAccomodation
    {
        public int SessionAccomodationId { get; set; }
        public int SessionId { get; set; }
        public int RoomId { get; set; }
        public int? BlockId { get; set; }
        public int? HostelId { get; set; }
        public virtual Room Room { get; set; }
        public virtual Block Block { get; set; }
        public virtual Hostel Hostel { get; set; }

        public virtual Session Session { get; set; }
    }

    public class SessionAccomodationVm
    {
        public int SessionAccomodationId { get; set; }
        public int SessionId { get; set; }
        public int[] BlockId { get; set; }
        public bool IsAvailable { get; set; }

    }
}
