using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Web;

namespace SwiftKampusModel.Siwes
{
    public class SiwesPlacement
    {
        public int SiwesPlacementId { get; set; }
        public string StudentId { get; set; }
        public int SessionId { get; set; }

        [DataType(DataType.Date)]
        public DateTime AcceptanceDate { get; set; }
        public string OrganizationName { get; set; }
        public string PersonInCharge { get; set; }
        public string OfficerRank { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
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
        public Student Student { get; set; }
        public Session Session { get; set; }
    }
}
