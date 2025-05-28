using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SwiftKampusModel.Library
{
    public class Book
    {
        public int BookId { get; set; }

        [Key]
        [Display(Name = "Accession Number")]
        [Required(ErrorMessage = "Accession No is required")]
        public string AccessionNo { get; set; }

        [Display(Name = "Book Title")]
        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }

        public int TotalBook { get; set; }
        public int BorrowedBook { get; set; }
        public int AvailableBook
        {
            get
            {
                return TotalBook - BorrowedBook;
            }
        }

        [Display(Name = "Book Author")]
        [Required(ErrorMessage = "Author is required")]
        public string Author { get; set; }

        [Display(Name = "Joint Author")]
        public string JointAuthor { get; set; }

        [Display(Name = "Book Subject")]
        [Required(ErrorMessage = "Book Subject is required")]
        public string Subject { get; set; }



        [Display(Name = "Book's ISBN")]
        [Required(ErrorMessage = "ISBN is required")]
        public string ISBN { get; set; }

        [Display(Name = "Book Edition")]
        [Required(ErrorMessage = "Edition is required")]
        public string Edition { get; set; }

        [Display(Name = "Book Publisher")]
        [Required(ErrorMessage = "Publisher is required")]
        public string Publisher { get; set; }

        [Display(Name = "Place of Publish")]
        public string PlaceOfPublish { get; set; }
        public int? BookCategoryId { get; set; }
        public virtual ICollection<BookIssue> BookIssue { get; set; }
        public virtual BookCategory BookCategory { get; set; }

    }
}
