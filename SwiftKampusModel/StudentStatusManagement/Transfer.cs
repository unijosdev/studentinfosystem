using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SwiftKampusModel.StudentStatusManagement
{
    public class Transfer
    {
        [Key]
        public int TransferId { get; set; }
        public string StudentId { get; set; }
        public int ProgrammeId { get; set; }
        public int SessionId { get; set; }
        public Student Student { get; set; }
        public Programme CurrentProgramme { get; set; }
        public Session Session { get; set; }
        public TransferProcess TransferProcess { get; set; }
    }
}
