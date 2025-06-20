using System;

namespace DomainModels.ResultModule;

public class CourseLoadSetting
    {
        public int CourseLoadSettingId { get; set; }
        public int ProgrammeId { get; set; }
        public int LevelId { get; set; }
        public int? SessionId { get; set; }
        public int MaximumCreditLoad { get; set; }
        public int MinimumCreditLoad { get; set; }
        public Programme Programme { get; set; }
        public Level Level { get; set; }
        public Session Session { get; set; }
    }
