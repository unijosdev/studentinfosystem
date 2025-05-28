using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwiftKampusModel
{
    public class OfficeAssignment
    {
        [Key]
        [ForeignKey("Staff")]
        public string StaffId { get; set; }

        [StringLength(50)]
        [Display(Name = "Office Location")]
        public string Location { get; set; }

        public virtual Staff Staff { get; set; }
    }
}