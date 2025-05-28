using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwiftKampusModel.CourseForum
{
    public class ForumQuestion
    {
        public int ForumQuestionId { get; set; }
        public int CourseId { get; set; }
        public string Title { get; set; }
        public string Question { get; set; }
        public string StudentId { get; set; }
        public virtual Forum Forum { get; set; }
        public virtual Student Student { get; set; }
        public ICollection<ForumQuestionReply> ForumQuestionReplies { get; set; }
        public ForumQuestionView ForumQuestionView { get; set; }

    }


    public class ForumQuestionView
    {
        [Key, ForeignKey("ForumQuestion")]
        public int ForumQuestionId { get; set; }

        public int ViewCounter { get; set; }
        public virtual ForumQuestion ForumQuestion { get; set; }
    }

    public class ForumQuestionVm
    {
        public ForumQuestion ForumQuestion { get; set; }
        public Forum Forum { get; set; }
        public List<ForumQuestionReply> ForumQuestionReplies { get; set; }
        public List<ForumComment> ForumComment { get; set; }
    }


    public class ForumReplyCommentVm
    {
        public ForumQuestion ForumQuestion { get; set; }
        public List<ForumQuestionReply> ForumQuestionRepy { get; set; }
        public List<ForumComment> ForumComment { get; set; }
    }
}
