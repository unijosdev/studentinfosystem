using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web;

namespace SwiftKampusModel
{
    public class CourseUpload
    {
        public int CourseUploadId { get; set; }

        public int CourseId { get; set; }

        [Display(Name = "Book Name")]
        [Required(ErrorMessage = "Book Name is required")]
        public string Name { get; set; }


        [Display(Name = "Book Author")]
        [Required(ErrorMessage = "Book Author is required")]
        public string Author { get; set; }

        [Display(Name = "Description")]
        //[Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        public string FileLocation { get; set; }

        [NotMapped]
        public HttpPostedFileBase File { get; set; }

        public virtual Course Course { get; set; }

    }
}
