using SwiftKampusModel;
using SwiftKampusModel.AddmissionApplicant;

namespace SwiftKampus.ViewModels
{
    public class PrerequisiteIndexVm
    {
        public Course MainCourse { get; set; }
        public string PrerequisiteCourse { get; set; }
        public Level Level { get; set; }
        public Semester Semester { get; set; }
        public SchoolProgramme SchoolProgramme { get; set; }
        public Programme Programme { get; set; }
    }
}