namespace SwiftKampusModel
{
    public class RejectedStudent
    {
        public int RejectedStudentId { get; set; }
        public string StudentId { get; set; }
        public int SessionId { get; set; }
        public string ReasonForRejection { get; set; }
        public string StaffId { get; set; }

        //public Student Student { get; set; }
    }
}
