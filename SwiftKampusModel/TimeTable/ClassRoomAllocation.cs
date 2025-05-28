using System;
using System.ComponentModel.DataAnnotations;

namespace SwiftKampusModel.TimeTable
{
    public class ClassRoomAllocation
    {
        public int ClassRoomAllocationId { get; set; }
        public int TimeTablePeriodId { get; set; }
        public int CourseId { get; set; }
        public int LectureRoomId { get; set; }

        [DataType(DataType.Time)]
        public String StartTime { get; set; }

        [DataType(DataType.Time)]
        public String EndTime { get; set; }

        public bool IsApproved { get; set; }

        public TimeTablePeriod TimeTablePeriod { get; set; }
        public Course Course { get; set; }
        public LectureRoom LectureRoom { get; set; }
    }
}
