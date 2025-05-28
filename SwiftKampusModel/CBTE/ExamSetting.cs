using System;
using System.ComponentModel.DataAnnotations;

namespace SwiftKampusModel.CBTE
{
    public class ExamSetting
    {
        public int ExamSettingId { get; set; }

        [Display(Name = "Course Name")]
        [Required(ErrorMessage = "Course Name is required")]
        public int CourseId { get; set; }

        [Display(Name = "Level Name")]
        [Required(ErrorMessage = "Level Name is required")]
        public int LevelId { get; set; }

        [Display(Name = "Semester")]
        [Required(ErrorMessage = "Semester is required")]
        public int SemesterId { get; set; }

        [Display(Name = "Session")]
        [Required(ErrorMessage = "Session is required")]
        public int SessionId { get; set; }

        [Display(Name = "Start Date")]
        [Required(ErrorMessage = "Start Date is required")]
        [DataType(DataType.Date)]
        public DateTime ExamDate { get; set; }

        public int ExamTypeId { get; set; }

        public virtual Course Course { get; set; }
        public virtual Level Level { get; set; }
        public virtual Semester Semester { get; set; }
        public virtual Session Sessions { get; set; }
        public virtual ExamType ExamType { get; set; }
    }
}
