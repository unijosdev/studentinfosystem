using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web;

namespace SwiftKampusModel.AddmissionApplicant
{
    public class RelevantDocument
    {
        public int RelevantDocumentId { get; set; }
        public string UserId { get; set; }
        public string FileName { get; set; }
        public string FileAddress { get; set; }
        [NotMapped]
        public HttpPostedFileBase File { get; set; }
    }
}
