using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwiftKampusModel
{
    public class ContinuousAssessment
    {
        public int ContinuousAssessmentId { get; set; }

        [Display(Name = "Student ID")]
        [Required(ErrorMessage = "Your Student ID Number is required")]
        [StringLength(25, ErrorMessage = "Your Student ID is too long")]
        public string StudentId { get; set; }

        [Display(Name = "Semester")]
        [Required(ErrorMessage = "Semester is required")]
        public int SemesterId { get; set; }

        [Display(Name = "Session")]
        [Required(ErrorMessage = "Session is required")]
        public int SessionId { get; set; }

        [Display(Name = "Course Name")]
        [Required(ErrorMessage = "Course Name is required")]
        public int CourseId { get; set; }

        public int? ProgrammeId { get; set; }

        public int? LevelId { get; set; }

        public int CourseUnit { get; set; }

        [Display(Name = "CA")]
        [Required(ErrorMessage = "CA is required")]
        [Range(0, 40, ErrorMessage = "Enter number between 0 to 40")]
        public double CaScore { get; set; }

        [Display(Name = "Exam Score")]
        [Required(ErrorMessage = "Exam Score is required")]
        [Range(0, 60, ErrorMessage = "Enter number between 0 to 60")]
        public double ExamScore { get; set; }

        [Display(Name = "Staff Name")]
        [Required(ErrorMessage = "Staff name is required")]
        public string StaffName { get; set; }

        public double Total {get; set; }

        public string Grading { get; set; }

        public string Remark { get; set; }

        public int GradePoint { get; set; }
        public int QualityPoint { get; set; }
        public bool Submitted { get; set; }
        public bool IsDeptApproved { get; set; }
        public bool IsFacultyApproved { get; set; }
        public bool IsSenateApproved { get; set; }
        public bool? IsAbsentForExam { get; set; }
        public int NoOfSubmission { get; set; }
        public string ReasonForReject { get; set; }

        [NotMapped]
        public int IsClearedAll
        {
            get
            {
                if (Submitted.Equals(true))                   
                {
                    NoOfSubmission = NoOfSubmission + 1;
                }
                return IsClearedAll;
            }
        }

        public Student Student { get; set; }
        public Semester Semester { get; set; }
        public Session Sessions { get; set; }
        public Course Course { get; set; }
        public Programme Programme { get; set; }
        public Level Level { get; set; }
    }

    public class CourseViewModel
    {
        public string CourseName { get; set; }
        public string ProgrammeName { get; set; }
        public string DepartmentName { get; set; }
        public string FacultyName { get; set; }
        public string FSemesterName { get; set; }
        public string Lecturer { get; set; }
        public string IsDeptApprove { get; set; }
        public string IsFacultyApprove { get; set; }
        public string IsSenateApprove { get; set; }
    }
}
