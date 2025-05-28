using System;
using System.ComponentModel.DataAnnotations;

namespace SwiftKampus.ViewModels
{
    public class StudentAttendanceVm
    {
        public int CourseId { get; set; }
        public int SchoolProgrammeId { get; set; }

        [DataType(DataType.Date)]
        public DateTime AttendanceDate { get; set; }

        public bool MarkAttendanceForAll { get; set; }
    }
}