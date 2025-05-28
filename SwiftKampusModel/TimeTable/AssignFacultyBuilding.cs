namespace SwiftKampusModel.TimeTable
{
    public class AssignFacultyBuilding
    {
        public int AssignFacultyBuildingId { get; set; }
        public int BuildingId { get; set; }
        public int FacultyId { get; set; }
        public Faculty Faculty { get; set; }
        public Building Building { get; set; }
    }
}
