using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Web;

namespace SwiftKampusModel
{
    public class PreDegreeStudent
    {
        [Key]
        public string RegNo { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Department { get; set; }
        public string Gender { get; set; }
        public string JambRegNo { get; set; }
        public string LGA { get; set; }
        public string State { get; set; }
        public string CourseInView { get; set; }
        public string JambScore { get; set; }
        public string PhoneNumber { get; set; }
        public string PdScore { get; set; }
        public string Password { get; set; }
        public bool IsRegistered { get; set; }
        public byte[] Passport { get; set; }

        [Display(Name = "Upload A Passport/Picture")]
        [ValidateFile(ErrorMessage = "Please select a PNG/JPEG image smaller than 20kb")]
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
                    Passport = target.ToArray();
                }
                catch (Exception ex)
                {
                    var message = ex.Message;
                }
            }
        }

        //public virtual ICollection<PreDegreeExam> PreDegreeExams { get; set; }
    }
}
