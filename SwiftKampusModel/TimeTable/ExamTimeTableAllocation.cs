namespace SwiftKampusModel.TimeTable
{
    public class ExamTimeTableAllocation
    {
        public int ExamTimeTableAllocationId { get; set; }
        public int LectureRoomId { get; set; }
        public int ExamTimeTableId { get; set; }
        public virtual ExamTimeTable ExamTimeTable { get; set; }
        public virtual LectureRoom LectureRoom { get; set; }
    }
}