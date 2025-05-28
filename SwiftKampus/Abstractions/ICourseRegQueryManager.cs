using System.Collections.Generic;
using System.Threading.Tasks;
using SwiftKampus.ViewModels;
using SwiftKampusModel;

namespace SwiftKampus.Abstractions
{
    public interface ICourseRegQueryManager
    {
        Task<List<Course>> GetStaffDepartmentCourse(int staffDeptId);
        List<CourseRegVm> GetStudentCourseReg(List<CourseRegistration> courseList);
        CourseRegSetting CheckCourseRegSetting(int schoolProgramme, int semester, int session);
        CourseRegSetting CheckPreviousCourseRegSetting(int schoolProgramme, int session);
    }
}