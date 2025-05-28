using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SwiftKampusModel
{
    public class CourseRegistration
    {
        public int CourseRegistrationId { get; set; }

        [Display(Name = "Student Number")]
        [Required(ErrorMessage = "Student Number cannot be empty")]
        public string StudentId { get; set; }

        [Display(Name = "Level Name")]
        [Required(ErrorMessage = "Class Name cannot be empty")]
        public int LevelId { get; set; }

        [Display(Name = "Programme Name")]
        [Required(ErrorMessage = "Programme Name is required")]
        public int ProgrammeId { get; set; }


        [Display(Name = "Department Name")]
        //[Required]
        public int? DepartmentId { get; set; }

        [Display(Name = "Semester Name")]
        [Required(ErrorMessage = "Semester Name cannot be empty")]
        public int SemesterId { get; set; }

        [Display(Name = "Session Name")]
        [Required(ErrorMessage = "Session Name cannot be empty")]
        public int SessionId { get; set; }

        [Display(Name = "Course Name")]
        [Required(ErrorMessage = "Course Name cannot be empty")]
        public int CourseId { get; set; }
        public bool IsStudentRegistered { get; set; }
        public bool IsApproved { get; set; }
        public string ReasonForReject { get; set; }
        public string ApprovedBy { get; set; }
        public Course Course { get; set; }
        public Semester Semester { get; set; }
        public Student Students { get; set; }
        public Level Level { get; set; }
        public Session Session { get; set; }
        public Programme Programme { get; set; }
        public Department Department { get; set; }
    }


    public class CourseRegistrationVm
    {
        [Display(Name = "Student Number")]
        [Required(ErrorMessage = "Student Number cannot be empty")]
        public string StudentId { get; set; }

        [Display(Name = "Level Name")]
        [Required(ErrorMessage = "Class Name cannot be empty")]
        public int LevelId { get; set; }

        [Display(Name = "Programme Name")]
        [Required(ErrorMessage = "Programme Name is required")]
        public int ProgrammeId { get; set; }

        [Display(Name = "Department Name")]
        public int? DepartmentId { get; set; }

        [Display(Name = "Semester Name")]
        [Required(ErrorMessage = "Semester Name cannot be empty")]
        public int SemesterId { get; set; }

        [Display(Name = "Session Name")]
        [Required(ErrorMessage = "Session Name cannot be empty")]
        public int SessionId { get; set; }

        [Display(Name = "Subject Name")]
        //[Required(ErrorMessage = "Subject Name cannot be empty")]
        public int[] CourseId { get; set; }
        //public List<Course> CarryOverCourses { get; set; }
        public int[] CarryOverCoursesId { get; set; }
        public int[] CoreCourses { get; set; }
        public int[] PrerequisiteCoursesId { get; set; }
        public int AvailableCredit { get; set; }
        public int MaximumCreditLoad { get; set; }
        public int MinimumCreditLoad { get; set; }
        public Student Student { get; set; }
        public List<CourseReg> Courses { get; set; }
        public List<Course> CarryOverCourses { get; set; }

    }

    public class CourseReg
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; }
        public int Credit { get; set; }
    }

    public class CourseRegApiVm
    {
        public string StudentId { get; set; }
        public int AvailableCredit { get; set; }
        public List<CourseReg> Courses { get; set; }
    }
}
