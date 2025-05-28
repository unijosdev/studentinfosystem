using System;
using System.Collections.Generic;

namespace SwiftKampusModel.BlogPost
{
    public class Post
    {
        public Post()
        {
            Comments = new HashSet<Comment>();
            Tags = new HashSet<Tag>();
        }

        public int ID { get; set; }
        public string Title { get; set; }
        public DateTime DateTime { get; set; }
        public string Body { get; set; }

        public bool MakePrivate { get; set; }

        public string StaffId { get; set; }

        public Staff Staff { get; set; }
        public ICollection<Comment> Comments { get; set; }

        public ICollection<Tag> Tags { get; set; }
    }
}
