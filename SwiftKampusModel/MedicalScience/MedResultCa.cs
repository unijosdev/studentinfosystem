using System.Collections.Generic;

namespace SwiftKampusModel.MedicalScience
{
    public class MedResultCa
    {
        public int MedResultCaId { get; set; }
        public int MedResultCategoryId { get; set; }
        public string ResultCaName { get; set; }
        public int MaximumScore { get; set; }
        public int PassMark { get; set; }
        public MedResultCategory MedResultCategory { get; set; }
        public ICollection<MedContiniousAssesment> MedContiniousAssesments { get; set; }

    }
}
