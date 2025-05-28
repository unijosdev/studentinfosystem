namespace SwiftKampusModel
{
    public class StaffPosition
    {
        public int StaffPositionId { get; set; }
        public string PositionCode { get; set; }
        public string PositionName { get; set; }
        public PositionType PositionType { get; set; }

        public string PositionTypeName
        {
            get { return PositionType.ToString(); }
        }

    }
}