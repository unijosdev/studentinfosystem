using System;
using System.Collections.Generic;

namespace SwiftKampusModel.CourseForum
{
    public class ForumComment
    {
        public int ForumCommentId { get; set; }
        public int ContentId { get; set; }
        public string Body { get; set; }
        public string UserId { get; set; }
        public DateTime CommentDateTime { get; set; }
        public virtual ICollection<CommentReply> CommentReplies { get; set; }
    }
}