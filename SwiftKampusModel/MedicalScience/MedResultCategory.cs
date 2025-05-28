namespace SwiftKampusModel.MedicalScience
{
    public class MedResultCategory
    {
        public int MedResultCategoryId { get; set; }
        public int LevelId { get; set; }
        public int ProgrammeId { get; set; }
        public string CategoryName { get; set; }   
        public int TotalScore { get; set; }
        public int PassMark { get; set; }
        public Level Level { get; set; }
        public Programme Programme { get; set; }

    }
}
