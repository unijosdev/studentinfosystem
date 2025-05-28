using System.ComponentModel.DataAnnotations;

namespace SwiftKampusModel
{
    public class AssignedCourse
    {
        public int AssignedCourseId { get; set; }
        public string StaffId { get; set; }
        public int CourseId { get; set; }
        public int? SessionId { get; set; }
        public int? SemesterId { get; set; }
        public bool IsMainLecture { get; set; }
        public virtual Staff Staff { get; set; }
        public virtual Course Course { get; set; }
        public virtual Semester Semester { get; set; }
        public virtual Session Session { get; set; }
    }

    public class AssignedCourseVm
    {
        public int AssignedCourseId { get; set; }

        [Display(Name = "Lecturers Name")]
        public string StaffId { get; set; }

        [Display(Name = "Courses")]
        public int[] CourseId { get; set; }
        [Display(Name = "Session Name")]
        public int SessionId { get; set; }

        //[Display(Name = "Semester Name")]
        //public int SemesterId { get; set; }

        [Display(Name = "Is Main Lecturer")]
        public bool IsMainLecture { get; set; }

    }
}
