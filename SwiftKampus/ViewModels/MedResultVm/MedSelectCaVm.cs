using System.ComponentModel.DataAnnotations;

namespace SwiftKampus.ViewModels.MedResultVm
{
    public class MedSelectCaVm
    {
        public int SessionId { get; set; }
        [Required]
        public int MedResultCaId { get; set; }
        public int MedResultCategoryId { get; set; }  
        public int LevelId { get; set; }  
    }
}