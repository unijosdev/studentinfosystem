using System;

namespace SwiftKampusModel.CourseForum
{
    public class CommentReply
    {
        public int CommentReplyId { get; set; }
        public int ForumCommentId { get; set; }
        public string Reply { get; set; }
        public string UserId { get; set; }
        public DateTime ReplyDateTime { get; set; }
        public virtual ForumComment ForumComment { get; set; }

    }
}