using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwiftKampusModel.CourseForum
{
    public class Forum
    {
        [Key, ForeignKey("Course")]
        public int CourseId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsForumAvailable { get; set; }
        public virtual Course Course { get; set; }
        public virtual ICollection<ForumQuestion> ForumQuestions { get; set; }
        public virtual ForumView ForumView { get; set; }

    }

    public class ForumView
    {
        [Key, ForeignKey("Forum")]
        public int CourseId { get; set; }
        public int ViewCounter { get; set; }
        public virtual Forum Forum { get; set; }
    }

    public class ForumIndexVm
    {
        public Forum Forum { get; set; }
        public List<ForumQuestion> ForumQuestion { get; set; }

    }
}
