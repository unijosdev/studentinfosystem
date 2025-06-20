using System;

namespace DomainModels.ResultModule;

public class Result
    {
        public int ResultId { get; set; }
        public string StudentId { get; set; } = string.Empty;

        public int TotalCourseUnit { get; set; }

        public double TotalGradePoint { get; set; }

        public double TotalQualityPoint { get; set; }

        public string LevelName { get; set; } = string.Empty;

        public double Gpa
        {
            get
            {
                return TotalQualityPoint / TotalCourseUnit;
            }
        }

        public double Cgpa { get; set; }

        public Guid ProgrammeId { get; set; }

        public Guid SemesterId { get; set; }

        public Guid SessionId { get; set; }
        public Student Student { get; set; }
        public Semester Semester { get; set; }
        public Session Session { get; set; }
        public Programme Programme { get; set; }
    }