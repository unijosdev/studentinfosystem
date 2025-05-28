using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Drawing.Imaging;
using System.Web;

namespace SwiftKampus.ViewModels
{
    public class ValidateFileAttribute : RequiredAttribute
    {
        public override bool IsValid(object value)
        {
            if (!(value is HttpPostedFileBase file))
            {
                return true;
            }

            if (file.ContentLength > 1 * 1024 * 1024)
            {
                return false;
            }


            using (var img = Image.FromStream(file.InputStream))
            {
                return img.RawFormat.Equals(img.RawFormat.Equals(ImageFormat.Png) ? ImageFormat.Png : ImageFormat.Jpeg);
            }          
        }
    }
}