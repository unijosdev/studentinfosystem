using System;
using System.Collections.Generic;

namespace SwiftKampusModel.CourseForum
{
    public class ForumQuestionReply
    {
        public int ForumQuestionReplyId { get; set; }
        public int ForumQuestionId { get; set; }
        public string Answer { get; set; }
        public DateTime ReplyDate { get; set; }
        public string UserId { get; set; }
        public virtual ForumQuestion FormQuestion { get; set; }
        public ICollection<VoteQuestionReply> VoteQuestionReplies { get; set; }

    }

    public class VoteQuestionReply
    {
        public int VoteQuestionReplyId { get; set; }
        public int ForumQuestionReplyId { get; set; }
        public Vote Vote { get; set; }
        public string UserId { get; set; }
        public virtual ForumQuestionReply ForumQuestionReply { get; set; }

    }
}