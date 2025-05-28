using SwiftKampusModel.AddmissionApplicant;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SwiftKampusModel.TimeTable
{
    public class TimeTablePeriod
    {
        public int TimeTablePeriodId { get; set; }
        public string Name { get; set; }
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        public int SemesterId { get; set; }
        public int SessionId { get; set; }
        public int SchoolProgrammeId { get; set; }
        public Semester Semester { get; set; }
        public Session Session { get; set; }
        public SchoolProgramme SchoolProgramme { get; set; }

        public ICollection<ClassRoomAllocation> ClassRoomAllocations { get; set; }
        public ICollection<ClassRoomAllocation> TimeTableAllocations { get; set; }
    }
}