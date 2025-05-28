using System;

namespace SwiftKampusModel
{
    public class Audit
    {
        // Audit Properties
        public string SessionId { get; set; }

        public Guid AuditId { get; set; }
        public string UserName { get; set; }
        public string IpAddress { get; set; }
        public string UrlAccessed { get; set; }
        public DateTime TimeAccessed { get; set; }
        public string Data { get; set; }
        public string ActionPerformed { get; set; }

    }

    public class AuditVm
    {
        public Guid AuditId { get; set; }
        public string UserName { get; set; }
        public string IpAddress { get; set; }
        public string UrlAccessed { get; set; }
        public string TimeAccessed { get; set; }
        public string Data { get; set; }
        public string ActionPerformed { get; set; }
    }

    public class LoginDetailVm
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string FullName { get; set; }
        public string UserType { get; set; }
        public int SchoolProgrammeId { get; set; }
    }
}