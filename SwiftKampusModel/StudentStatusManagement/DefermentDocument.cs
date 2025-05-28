using System.ComponentModel.DataAnnotations;

namespace SwiftKampusModel.StudentStatusManagement
{
    public class DefermentDocument
    {
        [Key]
        public int DefermentDocumentId { get; set; }
        public int DefermentId { get; set; }
        public string DocumentName { get; set; }
        public string DocumentExtension { get; set; }
        public string DocumentPath { get; set; }
        public Deferment Deferment { get; set; }
    }
}
