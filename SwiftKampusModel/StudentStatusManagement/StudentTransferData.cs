using System;

namespace SwiftKampusModel.StudentStatusManagement
{
    public class StudentTransferData
    {
        public int StudentTransferDataId { get; set; }
        public string StudentId { get; set; }
        public string ProgrammeId { get; set; }
        public string LevelId { get; set; }
        public string SessionId { get; set; }
        public string ModeOfEntry { get; set; }
        public DateTime DateTransfered { get; set; }
        public Student Student { get; set; }
    }
}
