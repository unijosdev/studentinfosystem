using SwiftKampusModel.AddmissionApplicant;
using SwiftKampusModel.CBTE;
using SwiftKampusModel.MedicalScience;
using SwiftKampusModel.Payment;
using SwiftKampusModel.StudentStatusManagement;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwiftKampusModel
{
    public class Level
    {
        public int LevelId { get; set; }
        public int? SchoolProgrammeId { get; set; }

        [Index(IsUnique = true)]
        [MaxLength(20)]
        [Display(Name = "Level Name")]
        public string LevelName { get; set; }

        public string LevelOrder { get; set; }
        public SchoolProgramme SchoolProgramme { get; set; }

        public ICollection<Student> Students { get; set; }
        public ICollection<Programme> ProgrammeFinalLevel { get; set; }
        public ICollection<CourseRegistration> CourseRegistrations { get; set; }
        public ICollection<QuestionAnswer> QuestionAnswers { get; set; }
        public ICollection<ExamSetting> ExamSettings { get; set; }
        public ICollection<ExamLog> ExamLogs { get; set; }
        public ICollection<ContinuousAssessment> ContinuousAssessments { get; set; }
        public ICollection<StudentAssignment> StudentAssignments { get; set; }
        public ICollection<FacultyFeeType> FacultyFeeTypes { get; set; }
        public ICollection<DepartmentFeeType> DepartmentFeeTypes { get; set; }
        public ICollection<SchoolFeeType> SchoolFeeTypes { get; set; }
        public ICollection<ContinuousAssessmentHistory> ContinuousAssessmentHistories { get; set; }
        public ICollection<CourseLoadSetting> CourseLoadSettings { get; set; }
        public ICollection<DeCoreCourse> DeCoreCourses { get; set; }
        public ICollection<MedResultCategory> MedResultCategories { get; set; }
        public ICollection<SchoolFeePayment> SchoolFeePayments { get; set; }

    }
}