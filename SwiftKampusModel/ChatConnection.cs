using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwiftKampusModel
{
    public class ChatConnection
    {
        public int ChatConnectionId { get; set; }
        [ForeignKey("ApplicationUser")]
        public string UserId { get; set; }
        public string ConnectionId { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }
    }

    public class PrivateMessage
    {
        public int PrivateMessageId { get; set; }
        public string Message { get; set; }
        public string FromUser { get; set; }
        public string ToUser { get; set; }
        public DateTime MessageDate { get; set; }
    }

    public class GroupMessage
    {
        public int GroupMessageId { get; set; }
        public string Message { get; set; }
        public string FromUser { get; set; }
        public string GroupName { get; set; }
        public DateTime MessageDate { get; set; }
    }
}