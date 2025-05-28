using System.Collections.Generic;

namespace SwiftKampusModel.Library
{
    public class BookCategory
    {
        public int BookCategoryId { get; set; }
        public string BookCategoryName { get; set; }
        public ICollection<Book> Books { get; set; }
    }
}
