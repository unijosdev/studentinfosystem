using System.ComponentModel.DataAnnotations;


namespace SwiftKampusModel.StudentStatusManagement
{
    public class ExtensionDocument
    {
        [Key]
        public int ExtensionDocumentId { get; set; }
        public int ExtensionId { get; set; }
        public string DocumentName { get; set; }
        public string DocumentExtension { get; set; }
        public string DocumentPath { get; set; }
        public Extension Extension { get; set; }
    }
}
