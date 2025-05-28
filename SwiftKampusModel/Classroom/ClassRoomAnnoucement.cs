namespace SwiftKampusModel.Classroom
{
    public class ClassRoomAnnoucement
    {
        public int ClassRoomAnnoucementId { get; set; }
        public int CourseId { get; set; }
        public string AssignmentBody { get; set; }
        public string AnnouncementType { get; set; }
        public virtual Course Course { get; set; }
    }
}