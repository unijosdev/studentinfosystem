using System;
using System.ComponentModel.DataAnnotations;

namespace SwiftKampusModel.ChangeOfCourse
{
    public class ChangeOfCourseHistory
    {
        public int ChangeOfCourseHistoryId { get; set; }

        public string StudentId { get; set; }

        [Display(Name = "Change Type")]
        [Required(ErrorMessage = "Change Type field is Required!")]
        public string ChangeType { get; set; }

        public int OldProgrammeId { get; set; }

        public int NewProgrammeId { get; set; }

        public DateTime DateOfChange { get; set; }

        public int OldLevelId { get; set; }

        public int NewLevelId { get; set; }
        //public int SessionId { get; set; }


        //[Display(Name = "Author")]
        [Required(ErrorMessage = "Author field is Required!")]
        public string StaffId { get; set; }

        public Student Student { get; set; }
        //public Student Session { get; set; }

        public Staff Staff { get; set; }
    }
}