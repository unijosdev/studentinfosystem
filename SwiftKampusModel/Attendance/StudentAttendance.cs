using SwiftKampusModel.AddmissionApplicant;
using System;

namespace SwiftKampusModel.Attendance
{
    public class StudentAttendance
    {
        public int StudentAttendanceId { get; set; }
        public string StaffId { get; set; }
        public int SchoolProgrammeId { get; set; }
        public string StudentId { get; set; }
        public int CourseId { get; set; }
        public bool IsPresent { get; set; }
        public DateTime AttendanceDate { get; set; }
        public int SemesterId { get; set; }
        public int SessionId { get; set; }
        public Student Student { get; set; }
        public Course Course { get; set; }
        public SchoolProgramme SchoolProgramme { get; set; }

    }
    public class StaffAttendance
    {
        public int StaffAttendanceId { get; set; }
        public string StaffId { get; set; }
        public bool IsPresent { get; set; }
        public DateTime AttendanceDate { get; set; }
        public string ReasonForAbsence { get; set; }
        public virtual Staff Staff { get; set; }


    }
}
