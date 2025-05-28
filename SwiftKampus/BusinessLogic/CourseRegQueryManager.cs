using SwiftKampus.Abstractions;
using SwiftKampus.Models;
using SwiftKampus.ViewModels;
using SwiftKampusModel;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace SwiftKampus.BusinessLogic
{
    public class CourseRegQueryManager : ICourseRegQueryManager
    {
        private readonly SchoolDbContext _db;
        public CourseRegQueryManager(SchoolDbContext db)
        {
            _db = db;
        }

        public List<CourseRegVm> GetStudentCourseReg(List<CourseRegistration> courseList)
        {
            var courseVm = new List<CourseRegVm>();
            foreach (var mycourse in courseList)
            {
                var courseRegVm = new CourseRegVm
                {
                    CourseName = mycourse.Course.CourseName,
                    CourseCode = mycourse.Course.CourseCode
                };
                if (mycourse.IsStudentRegistered.Equals(true))
                {
                    courseRegVm.Staus = "Student Submitted";
                }
                if (mycourse.IsApproved.Equals(false) && mycourse.IsStudentRegistered.Equals(false))
                {
                    courseRegVm.Staus = mycourse.ReasonForReject;
                }
                else if(mycourse.IsApproved)
                {
                    courseRegVm.Staus = "Approved";
                }
                courseVm.Add(courseRegVm);
            }
            return courseVm;
        }

        //public async Task<Tuple<List<Semester>, List<Session>, List<Course>, List<Staff>>> GetViewDetails()
        //{
        //    var loginUser = await _db.Users.AsNoTracking().CountAsync(x => x.IsLogin.Equals(true));
        //    var allUsers = await _db.Users.AsNoTracking().CountAsync(x => x.EmailConfirmed.Equals(true));
        //    double val1 = loginUser * 100;
        //    var percentage = Math.Round(val1 / allUsers, 2);
        //    // Create a 3-tuple and return it  
        //    var activityStat = new Tuple<int, double, int>(
        //    loginUser, percentage, allUsers);
        //    return activityStat;
        //}

        public async Task<List<Course>> GetStaffDepartmentCourse(int staffDeptId)
        {
            return await _db.Courses.AsNoTracking().Include(i => i.Programme.Department)
                    .Where(x => x.Programme.Department.DepartmentId.Equals(staffDeptId))
                    .ToListAsync();
        }

        public CourseRegSetting CheckCourseRegSetting(int schoolProgrammeId, int semesterId, int sessionId)
        {
            return _db.CourseRegSettings.AsNoTracking().Where(x => x.SchoolProgrammeId.Equals(schoolProgrammeId) &&
                        x.SessionId.Equals(sessionId) && x.SemesterId.Equals(semesterId) && x.IsActive.Equals(true))
                        .FirstOrDefault();
        }

        public CourseRegSetting CheckPreviousCourseRegSetting(int schoolProgrammeId, int sessionId)
        {
            return _db.CourseRegSettings.AsNoTracking().Include(x => x.SchoolProgramme).Include(x => x.Session).Include(x => x.Session).Where(x => x.SchoolProgramme.SchoolProgrammeId.Equals(schoolProgrammeId) &&
                        x.Session.SessionId.Equals(sessionId) )
                        .FirstOrDefault();
        }
    }
}