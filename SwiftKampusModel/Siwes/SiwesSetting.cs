using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Web;

namespace SwiftKampusModel.Siwes
{
    public class SiwesSetting
    {
        public int SiwesSettingId { get; set; }
        public int SessionId { get; set; }

        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }
        public string SiwesCode { get; set; }
        public string DirectorName { get; set; }
        public string SiwesFullCode
        {
            get
            {
                return $"UJ/SIWES/LISS/150/Vol.1/{SiwesCode}";
            }
            set { }
        }

        public byte[] Signature { get; set; }

        [Display(Name = "Upload Signature")]
        [ValidateFile(ErrorMessage = "Please select a PNG/JPEG image smaller than 25kb")]
        [NotMapped]
        public HttpPostedFileBase File
        {
            get
            {
                return null;
            }

            set
            {
                try
                {
                    var target = new MemoryStream();

                    if (value.InputStream == null)
                        return;

                    value.InputStream.CopyTo(target);
                    Signature = target.ToArray();
                }
                catch (Exception ex)
                {
                    var message = ex.Message;
                }
            }
        }

        public Session Session { get; set; }
    }
}
