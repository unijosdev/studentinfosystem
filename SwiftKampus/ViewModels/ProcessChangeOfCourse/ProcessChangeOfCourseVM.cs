using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SwiftKampus.ViewModels.ProcessChangeOfCourse
{
    public class ProcessChangeOfCourseVM
    {
        public string StudentId { get; set; }

        public int ProgrammeId { get; set; }

        public int? LevelId { get; set; }

        public int? OldLevelId { get; set; }

        public int? OldprogrammeId { get; set; }

        public string OldprogrammeName { get; set; }

        public string OldLevelName { get; set; }

        public string NewLevelName { get; set; }

        public string AuthorId { get; set; } //Alias staffId

        public int SessionAppliedForCOC { get; set; } //COC: Change Of Course
    }
}