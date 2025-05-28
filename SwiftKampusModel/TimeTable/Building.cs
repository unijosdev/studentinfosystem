using System.Collections.Generic;

namespace SwiftKampusModel.TimeTable
{
    public class Building
    {
        public int BuildingId { get; set; }
        public string BuildingName { get; set; }
        public string BuildingCode { get; set; }
        public bool IsMultiPurpose { get; set; }
        public string BuildingLocation { get; set; }
        public ICollection<LectureRoom> LectureRooms { get; set; }
        public ICollection<AssignFacultyBuilding> AssignFacultyBuilding { get; set; }

    }
}
