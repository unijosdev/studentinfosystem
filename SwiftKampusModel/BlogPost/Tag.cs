using System.Collections.Generic;

namespace SwiftKampusModel.BlogPost
{
    public class Tag
    {

        public Tag()
        {
            Posts = new HashSet<Post>();
        }

        public int ID { get; set; }
        public string Name { get; set; }


        public ICollection<Post> Posts { get; set; }
    }
}
