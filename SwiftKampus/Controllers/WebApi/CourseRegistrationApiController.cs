using SwiftKampus.BusinessLogic;
using SwiftKampus.Models;
using SwiftKampusModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace SwiftKampus.Controllers.WebApi
{
    [RoutePrefix("CourseRegistrationApi")]
    public class CourseRegistrationApiController : ApiController
    {
        private readonly SchoolDbContext _db;
        private readonly QueryCommand _query;

        public CourseRegistrationApiController(SchoolDbContext db)
        {
            _db = db;
            _query = new QueryCommand(db);
        }

        [HttpPost]
        [Route("GetCourses", Name = "GetCourses")]
        [ResponseType(typeof(CourseRegistrationVm))]
        public async Task<IHttpActionResult> GetCourses(string id)
        {
            var courseList = new List<Course>();
            var courseRegList = new List<CourseReg>();

            //Query db for student's departmentOption and current level
            var student = await _db.Students.Include(i => i.SchoolProgramme).AsNoTracking().Include(i => i.Programme).Include(i => i.Level)
                .Where(x => x.StudentId.Equals(id))
                .Select(s => new
                {
                    myProgrammeId = s.Programme.ProgrammeId,
                    LevelName = s.Level.LevelId,
                    s.MatricNo,
                    s.SchoolProgramme.SchoolProgrammeId,
                })
                .FirstOrDefaultAsync();
            if (student == null)
            {
                return BadRequest("level not assigned to student");
            }

            // querying db for current semester
            var semesterId = _query.GetCurrentSemesterId(student.SchoolProgrammeId);

            //Querying db for list of courses available for the student based on the current semester, level and departmental Option
            var courses = await _db.Courses.Include(i => i.Programme).Include(i => i.Semester)
                            .Include(i => i.Level).AsNoTracking()
                            .Where(x => x.Programme.ProgrammeId.Equals(student.myProgrammeId)
                            && x.Level.LevelId.Equals(student.LevelName)
                            && x.Semester.SemesterId.Equals(semesterId))
                .ToListAsync();
            var carryOverCourses = _db.CarryOverCourses.Include(i => i.Course.Level).AsNoTracking()
                                        .Where(x => x.StudentId.Equals(id)
                                    && x.SemesterId.Equals(semesterId)).Select(s => s.Course).ToList();
            foreach (var course in carryOverCourses)
            {
                courseList.Add(course);
            }
            foreach (var course in courses)
            {
                var coursereg = new CourseReg
                {
                    CourseId = course.CourseId,
                    CourseName = course.CourseName,
                    Credit = course.Credits
                };
                courseRegList.Add(coursereg);
            }

            var courseRegVm = new CourseRegistrationVm
            {
                StudentId = id,
                SemesterId = semesterId,
                ProgrammeId = student.myProgrammeId,
                LevelId = student.LevelName,
                CarryOverCoursesId = carryOverCourses.Select(s => s.CourseId).ToArray(),
                AvailableCredit = 24 - carryOverCourses.Sum(s => s.Credits),
                CarryOverCourses = carryOverCourses,
                Courses = courseRegList
            };
            return Ok(courseRegVm);
        }

        [HttpPost]
        [Route("RegisterCourse", Name = "RegisterCourse")]
        public async Task<IHttpActionResult> RegisterCourse(CourseRegApiVm model)
        {
            if (ModelState.IsValid)
            {
                var studentId = model.StudentId.Trim();
                var student = await _db.Students.Include(i => i.SchoolProgramme).Include(i => i.Programme).Include(i => i.Level)
                    .Include(i => i.Programme.Department)
                    .AsNoTracking().Where(x => x.StudentId.Equals(studentId)).FirstOrDefaultAsync();
                var sessionId = _query.GetCurrentSessionId(student.SchoolProgramme.SchoolProgrammeId);
                var semesterId = _query.GetCurrentSemesterId(student.SchoolProgramme.SchoolProgrammeId);
                //Determine total credit units of registered courses and make sure that the
                //total creditunits of courses registered doesn't exceed 24.
                int courseCreditTotal = model.Courses.Sum(s => s.Credit);



                var registeredCourseId = new List<CourseReg>();


                registeredCourseId.AddRange(model.Courses);

                if (courseCreditTotal <= model.AvailableCredit) // Checking if registered course is not greater than 24
                {
                    try
                    {
                        foreach (var courseId in registeredCourseId)
                        {

                            var courseReg = await _db.CourseRegistrations.Where(x => x.StudentId.Equals(studentId)
                                                    && x.SemesterId.Equals(semesterId) && x.SessionId.Equals(sessionId)
                                                    && x.CourseId.Equals(courseId.CourseId)).ToListAsync();

                            if (!courseReg.Any())
                            {

                                var courseRegistration = new CourseRegistration
                                {
                                    StudentId = studentId,
                                    LevelId = student.Level.LevelId,
                                    SemesterId = semesterId,
                                    SessionId = sessionId,
                                    ProgrammeId = student.Programme.ProgrammeId,
                                    CourseId = courseId.CourseId,
                                    DepartmentId = student.Programme.Department.DepartmentId
                                };
                                _db.CourseRegistrations.Add(courseRegistration);


                            }
                        }

                        await _db.SaveChangesAsync();
                        return Ok();
                    }
                    catch (Exception e)
                    {
                        return BadRequest($" Error message {e.Message}");
                    }
                }
                return BadRequest("Registration more than available credits");
            }
            return BadRequest("Bad Data");

        }
    }
}
