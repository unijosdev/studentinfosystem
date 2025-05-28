using System;

namespace SwiftKampusModel.SchoolMail
{
    public class SchoolMailMessage
    {
        public int SchoolMailMessageId { get; set; }
        public string SenderId { get; set; }
        public string RecieverId { get; set; }
        public string RecieverFullName { get; set; }
        public string MessageSubject { get; set; }
        public string MessageBody { get; set; }
        public string UserCopied { get; set; }
        public DateTime MessageDataTime { get; set; }
        public string AttachmentLocation1 { get; set; }
        public string AttachmentLocation2 { get; set; }
        public string AttachmentLocation3 { get; set; }
        public string AttachmentLocation4 { get; set; }
        public string AttachmentLocation5 { get; set; }
        public bool IsImportant { get; set; }
        public bool IsDeleted { get; set; }
        public bool HasRead { get; set; }

    }

    public class SchoolDraftMessage
    {
        public int SchoolDraftMessageId { get; set; }
        public string SenderId { get; set; }
        public string RecieverId { get; set; }
        public string MessageSubject { get; set; }
        public string MessageBody { get; set; }
        public string UserCopied { get; set; }
        public DateTime MessageDataTime { get; set; }


    }

    public class SendMessageVm
    {
        public string MessageSubject { get; set; }
        public string MessageBody { get; set; }
        public string RecieverId { get; set; }
        public string AttachmentLocation1 { get; set; }
        public string AttachmentLocation2 { get; set; }
        public string AttachmentLocation3 { get; set; }
        public string AttachmentLocation4 { get; set; }
        public string AttachmentLocation5 { get; set; }
    }
}
