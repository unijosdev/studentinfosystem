using System.ComponentModel.DataAnnotations;

namespace SwiftKampus.ViewModels
{
    public class CoursePrerequisiteVm
    {
        public int CourseId { get; set; }
        public int[] PrerequisiteCourseId { get; set; }
        public bool IsSiwes { get; set; }
    }

    public class DeCoreCourseCreateVm
    {
        public int LevelId { get; set; }
        public int ProgrammeId { get; set; }
        public int[] CourseId { get; set; }
    
    }

    public class PreviousCourseRegistrationVm
    {
        public string MatNum { get; set; }
        public int SessionId { get; set; }
        public int LevelId { get; set; }

    }

    public class CourseVm
    {
        public int CourseId { get; set; }
        public int? SchoolProgrammeId { get; set; }


        [Display(Name = "Course Code")]
        [Required(ErrorMessage = "Your Course Code is required")]
        public string CourseCode { get; set; }

        [Required(ErrorMessage = "Your Course Name is required")]
        [Display(Name = "Course Name")]
        public string CourseName { get; set; }


        [Display(Name = "Course Description")]
        public string CourseDescription { get; set; }

        public string CourseType { get; set; }

        [Range(1, 20)]
        [Required(ErrorMessage = "Your Course Credit is required")]
        public int Credits { get; set; }

        [Display(Name = "Semester Name")]
        public int? SemesterId { get; set; }

        [Display(Name = "School Level")]
        public int? LevelId { get; set; }

        public int? ProgrammeId { get; set; }

        public bool DeActivatedCourse { get; set; }
        public int[] PrerequisiteteCourseId { get; set; }
    }
}