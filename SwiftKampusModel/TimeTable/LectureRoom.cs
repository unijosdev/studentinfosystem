using System.Collections.Generic;

namespace SwiftKampusModel.TimeTable
{
    public class LectureRoom
    {
        public int LectureRoomId { get; set; }
        public int BuildingId { get; set; }
        public string LectureRoomName { get; set; }
        public string LectureRoomCode { get; set; }
        public int LectureRoomCapacity { get; set; }
        public int SittingCapacity { get; set; }
        public bool IsGeneralLectureHall { get; set; }
        public Building Building { get; set; }
        public ICollection<ExamTimeTableAllocation> ExamTimeTableAllocations { get; set; }
        public ICollection<ClassRoomAllocation> TimeTableAllocations { get; set; }

    }
}