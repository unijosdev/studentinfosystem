namespace SwiftKampusModel.Siwes
{
    public class SiwesAttendance
    {
        public int SiwesAttendanceId { get; set; }
        public string StdentId { get; set; }
        public int SessionId { get; set; }
        public Student Student { get; set; }
        public Session Session { get; set; }
    }
}
