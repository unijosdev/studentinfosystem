using SwiftKampusModel.AddmissionApplicant;
using System.Collections.Generic;

namespace SwiftKampusModel
{
    public class DeptResultType
    {
        public int DeptResultTypeId { get; set; }
        public int DepartmentId { get; set; }
        public int ResultTemplateId { get; set; }
        public int SchoolProgrammeId { get; set; }
        public int SessionId { get; set; }
        public int FailMark { get; set; }
        public double ProbationMark { get; set; }
        public bool HaveHundredLevelResult { get; set; } = false;
        public int? ResultTemplateForHundred { get; set; } 
        public Department Department { get; set; }
        public Session Session { get; set; }
        public ResultTemplate ResultTemplate { get; set; }
        public SchoolProgramme SchoolProgramme { get; set; }

    }

    public class ResultTemplate
    {
        public int ResultTemplateId { get; set; }
        public string ResultType { get; set; }
        public string FancyName { get; set; }
        public ICollection<DeptResultType> DeptResultTypes { get; set; }
        public ICollection<Grade> Grades { get; set; }
    }
}
