namespace SwiftKampusModel.Accomodation
{
    public class AssignedHostel
    {
        public int AssignedHostelId { get; set; }
        public int HostelId { get; set; }
        public int FacultyId { get; set; }
        public virtual Faculty Faculty { get; set; }
        public virtual Hostel Hostel { get; set; }

    }
    public class AssignedHostelVm
    {
        public int AssignedHostelId { get; set; }
        public int HostelId { get; set; }
        public int[] FacultyId { get; set; }

    }

    //public class AssignRoomToLevel
    //{
    //    public int AssignRoomToLevelId { get; set; }
    //    public int RoomId { get; set; }
    //    public int LevelId { get; set; }
    //    public virtual Room Room { get; set; }
    //    public virtual Level Level { get; set; }
    //}

    public class ReservedRoom
    {
        public int ReservedRoomId { get; set; }
        public int RoomId { get; set; }
        public string ReasonForReserve { get; set; }
        public virtual Room Room { get; set; }

    }
}
