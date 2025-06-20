using System;

namespace DomainModels.ResultModule;

public class Programme
    {
        public int ProgrammeId { get; set; }
        public int? LevelId { get; set; }

        [Display(Name = "Department Name")]
        //[Required(ErrorMessage = "Your Department Name is required")]
        public int? DepartmentId { get; set; }

        [Index(IsUnique = true)]
        [MaxLength(50)]
        [Display(Name = "Programme Code")]
        [Required(ErrorMessage = "Your Programme Code is required")]
        public string ProgrammeCode { get; set; }

        [Display(Name = "Programme Name")]
        [Required(ErrorMessage = "Your Programme Name is required")]
        public string ProgrammeName { get; set; }

        [Display(Name = "Maximum Number of Semesters")]
        [Required(ErrorMessage = "Your Maximum Number of Semesters is required")]
        public int? NoOfSemesters { get; set; }

        [Display(Name = "Awarding Degree Name")]
        [Required(ErrorMessage = "Your Awarding Degree Name is required")]
        public string AwardingDegreeName { get; set; }

        public Department Department { get; set; }
        public Level FinalLevel { get; set; }

        public ICollection<Student> Students { get; set; }
        public ICollection<Course> Courses { get; set; }
        public ICollection<Result> Results { get; set; }
        public ICollection<CourseRegistration> CourseRegistrations { get; set; }
        public ICollection<StudentAssignment> StudentAssignments { get; set; }
        public ICollection<AvailableCourse> AvailableCourses { get; set; }
        public ICollection<UnderGraduateRule> UnderGraduateRules { get; set; }
        public ICollection<UtmeApplicant> UtmeApplicants { get; set; }
        public ICollection<ContinuousAssessment> ContinuousAssessments { get; set; }
        public ICollection<ContinuousAssessmentHistory> ContinuousAssessmentHistories { get; set; }
        public ICollection<Transfer> Transfers { get; set; }
        public ICollection<CourseLoadSetting> CourseLoadSettings { get; set; }
        public ICollection<DeCoreCourse> DeCoreCourses { get; set; }
        public ICollection<MedResultCategory> MedResultCategories { get; set; }
        public ICollection<ChangeOfCoursePayment> ChangeOfCoursePayment { get; set; }
        public ICollection<ApplicantWaiverPayment> ApplicantWaiverPayments { get; set; }
        public ICollection<ConvocationList> ConvocationLists { get; set; }
        public ICollection<UtmeScreeningCutOff> UtmeScreeningCutOffs { get; set; }


    }
