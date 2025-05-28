using SwiftKampusModel;
using SwiftKampusModel.TimeTable;
using System.Collections.Generic;

namespace SwiftKampus.ViewModels
{
    public class SetTimeTableVm
    {
        public int TimeTableId { get; set; }
        public int TimeTableAllocationId { get; set; }
        public int SelectableLectureRoomId { get; set; }
        public int SelectedCourseId { get; set; }
        public List<Course> SelectableCourses { get; set; }
        public List<LectureRoom> SelectableLectureRooms { get; set; }
    }
}