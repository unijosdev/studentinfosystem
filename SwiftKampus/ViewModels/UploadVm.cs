using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web;

namespace SwiftKampus.ViewModels
{
    public class UploadVm
    {
        public string CourseName { get; set; }

        public string FileLocation { get; set; }

        [Display(Name = "Upload File")]
        //[ValidateFile(ErrorMessage = "Please select a PNG/JPEG image smaller than 1MB")]
        [NotMapped]
        public HttpPostedFileBase File { get; set; }
    }
}