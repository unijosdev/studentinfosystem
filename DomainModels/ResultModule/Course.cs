using System;

namespace DomainModels.ResultModule;

public class Course
    {
        public int CourseId { get; set; }
        public int? SchoolProgrammeId { get; set; }

        public string CourseCode { get; set; }

        public string CourseName { get; set; }

        public string CourseDescription { get; set; }

        public string CourseType { get; set; }

        public int Credits { get; set; }

        public int? SemesterId { get; set; }

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

