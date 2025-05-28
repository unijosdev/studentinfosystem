using System;

namespace SwiftKampusModel
{
    public class IdCardRequest
    {
        public int IdCardRequestId { get; set; }
        public int SessionId { get; set; }
        public string StudentId { get; set; }
        public string RequestReason { get; set; }
        public DateTime DateOfRequest { get; set; }
        public Session Session { get; set; }
        public Student Student { get; set; }
    }
}
