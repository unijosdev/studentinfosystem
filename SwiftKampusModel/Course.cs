using SwiftKampusModel.AddmissionApplicant;
using SwiftKampusModel.Attendance;
using SwiftKampusModel.CBTE;
using SwiftKampusModel.Classroom;
using SwiftKampusModel.Classroom.Assesment;
using SwiftKampusModel.CourseForum;
using SwiftKampusModel.TimeTable;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SwiftKampusModel
{
    public class Course
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

        [Range(1, 30)]
        [Required(ErrorMessage = "Your Course Credit is required")]
        public int Credits { get; set; }

        [Display(Name = "Semester Name")]
        public int? SemesterId { get; set; }

        [Display(Name = "School Level")]
        public int? LevelId { get; set; }

        public int? ProgrammeId { get; set; }

        public bool DeActivatedCourse { get; set; }

        public Semester Semester { get; set; }
        public SchoolProgramme SchoolProgramme { get; set; }
        public Level Level { get; set; }
        public Programme Programme { get; set; }
        public ICollection<Module> Modules { get; set; }
        public ICollection<ClassRoomAnnoucement> ClassRoomAnnoucements { get; set; }
        public ICollection<AssesmentQuestionAnswer> AssesmentQuestionAnswers { get; set; }
        public ICollection<StudentAssesment> StudentAssesments { get; set; }
        public ICollection<Enrollment> Enrollments { get; set; }
        public ICollection<Staff> Staffs { get; set; }
        public ICollection<QuestionAnswer> QuestionAnswers { get; set; }
        public ICollection<ExamRule> ExamRules { get; set; }
        public ICollection<CourseRegistration> CourseRegistrations { get; set; }
        public ICollection<ContinuousAssessment> ContinuousAssessments { get; set; }
        public ICollection<ExamSetting> ExamSettings { get; set; }
        public ICollection<ExamLog> ExamLogs { get; set; }
        public ICollection<CourseUpload> CourseUploads { get; set; }
        public ICollection<StudentAssignment> StudentAssignments { get; set; }
        public ICollection<AssignedCourse> AssignedCourses { get; set; }
        public Forum Forums { get; set; }
        public ICollection<ExamTimeTable> ExamTimeTables { get; set; }
        public ICollection<StudentAttendance> StudentAttendances { get; set; }
        public ICollection<ClassRoomAllocation> TimeTableAllocations { get; set; }
        public ICollection<ContinuousAssessmentHistory> ContinuousAssessmentHistories { get; set; }
        public ICollection<AssignCourseToCategory> AssignCourseToCategories { get; set; }
        public ICollection<CoursePrerequisite> CoursePrerequisites { get; set; }


    }


    public class CourseVm
    {
        public int CourseId { get; set; }

        [Required(ErrorMessage = "School Programme Name is required")]
        public int SchoolProgrammeId { get; set; }

        [Display(Name = "Course Code")]
        [Required(ErrorMessage = "Your Course Code is required")]
        public string CourseCode { get; set; }

        [Required(ErrorMessage = "Your Course Name is required")]
        [Display(Name = "Course Name")]
        public string CourseName { get; set; }


        [Display(Name = "Course Description")]
        public string CourseDescription { get; set; }

        public string CourseType { get; set; }

        [Range(1, 30)]
        [Required(ErrorMessage = "Your Course Credit is required")]
        public int Credits { get; set; }

        [Display(Name = "Semester Name")]
        [Required(ErrorMessage = "Semester Name is required")]
        public int? SemesterId { get; set; }      
        public bool IsSiwes { get; set; }

        [Required(ErrorMessage = "Programme Name is required")]
        public int ProgrammeId { get; set; }

        [Display(Name = "School Level")]
        [Required(ErrorMessage = "Level Name is required")]
        public int? LevelId { get; set; }

        public bool DeActivatedCourse { get; set; }
        public int[] PrerequisiteteCourseId { get; set; }
        public ICollection<DeCoreCourse> DeCoreCourses { get; set; }


    }
}