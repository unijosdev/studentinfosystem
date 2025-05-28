using SwiftKampus.BusinessLogic;
using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampusModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace SwiftKampus.ViewModels
{
    public class ContinuousAssessmentVm
    {
        private readonly GradeRemark _myGradeRemark;
        private readonly ResultCommand _result;
        private readonly SchoolDbContext _db = new SchoolDbContext();

        public ContinuousAssessmentVm()
        {
            _result = new ResultCommand(_db);
            _myGradeRemark = new GradeRemark(_db);
        }
        public int ContinuousAssessmentId { get; set; }

        [Display(Name = "Student ID")]
        [Required(ErrorMessage = "Your Student ID Number is required")]
        [StringLength(55, ErrorMessage = "Your Student ID is too long")]
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

        public int ProgrammeId { get; set; }

        public int LevelId { get; set; }

        public int CourseUnit
        {
            get
            {
                return GetCourseCredit();
            }
        }

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
        public bool IsAbsentForExam { get; set; }
        public double Total
        {
            get
            {
                return CaScore + ExamScore;
            }
        }

        public string Grading
        {
            get
            {
                var _schoolProgrammeId = GetSchoolProgrammeId();
                var _resultTemplateId = GetResultTemplateId();
                return _myGradeRemark.Grading(Total, _schoolProgrammeId, _resultTemplateId);
            }

        }

        public string Remark
        {
            get
            {
                var _schoolProgrammeId = GetSchoolProgrammeId();
                var _resultTemplateId = GetResultTemplateId();
                return _myGradeRemark.Remark(Total, _schoolProgrammeId, _resultTemplateId);
            }
        }

        public int GradePoint
        {
            get
            {
                var _schoolProgrammeId = GetSchoolProgrammeId();
                var _resultTemplateId = GetResultTemplateId();
                return _myGradeRemark.GradingPoint(Total, _schoolProgrammeId, _resultTemplateId);
            }
        }

        public int QualityPoint
        {
            get
            {
                var courseCredit = GetCourseCredit();
                return courseCredit * GradePoint;
            }
        }
        public Student Student
        {
            get
            {
                var student = _db.Students.Find(StudentId);
                return student;
            }
        }
        public Course Course
        {
            get
            {
                var course = _db.Courses.Find(CourseId);
                return course;
            }
        }
        public Programme Programme
        {
            get
            {
                var programme = _db.Programmes.Find(ProgrammeId);
                return programme;
            }
        }
        public Level Level
        {
            get
            {
                var level = _db.Levels.Find(LevelId);
                return level;
            }
          
        }
        public Semester Semester
        {
            get
            {
                var semester = _db.Semesters.Find(SemesterId);
                return semester;
            }
           
        }
        public Session Session
        {
            get
            {
                var session = _db.Sessions.Find(SessionId);
                return session;
            }
          
        }
        //public Staff Staff
        //{
        //    get
        //    {
        //        var staff = _db.Staffs.Find(StaffName);
        //        return staff;
        //    }
        //    private set { }
        //}

        int GetCourseCredit()
        {
            int courseCredit = _db.Courses.AsNoTracking().Where(x => x.CourseId.Equals(CourseId))
                .Select(c => c.Credits)
                .FirstOrDefault();
            return courseCredit;
        }

        int GetSchoolProgrammeId()
        {
            return Student.SchoolProgrammeId;
        }

        int GetResultTemplateId()
        {
            return _result.GetResultTemplateId((int)Programme.DepartmentId, (int)Student.SessionId, Level.LevelOrder);
        }
    }

    public class SelectCaVm
    {
        public int CourseId { get; set; }
        public int SemesterId { get; set; }
        public int SessionId { get; set; }
    }

    public class GetCaRecordVm
    {
        public string StudentId { get; set; }
        public int SchoolProgrammeId { get; set; }
        public int ProgrammeId { get; set; }
        public int CourseId { get; set; }
        public int SemesterId { get; set; }
        public int SessionId { get; set; }
        public int DepartmentId { get; set; }
        public int LevelId { get; set; }
        public string LevelName { get; set; }
        public int ResultSessionId { get; set; }
    }
}
