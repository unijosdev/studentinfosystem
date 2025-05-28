using System.ComponentModel.DataAnnotations;

namespace SwiftKampusModel.Accomodation
{
    public class PrefferedChoice
    {

        public int PrefferedChoiceId { get; set; }

        [Display(Name = "Matric No")]
        public string StudentId { get; set; }

        public int HostelId { get; set; }

        public int BuildingId { get; set; }

        public virtual Student Students { get; set; }
    }
}