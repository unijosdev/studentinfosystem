using System;
using System.ComponentModel.DataAnnotations;

namespace SwiftKampusModel
{
    public class AcademicSession
    {
        public int AcademicSessionId { get; set; }

        public int SessionId { get; set; }

        [Display(Name = "Semester Name")]
        [Required(ErrorMessage = "Session Start is required")]
        public int SemesterId { get; set; }

        [Display(Name = "Session Start")]
        [Required(ErrorMessage = "Session Start is required")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Display(Name = "Session End")]
        [Required(ErrorMessage = "Session End is required")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        public virtual Session Session { get; set; }
        public virtual Semester Semester { get; set; }
    }
}
