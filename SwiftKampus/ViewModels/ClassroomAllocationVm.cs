namespace SwiftKampus.ViewModels
{
    public class ClassroomAllocationVm
    {
        public int TimeTableAllocationId { get; set; }
        public int TimeTableId { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string CourseCode { get; set; }
        public string LectureRoomCode { get; set; }
    }
}