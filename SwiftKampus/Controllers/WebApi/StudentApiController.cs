//namespace SwiftKampus.Controllers.WebApi
//{
    //[RoutePrefix("StudentApi")]
    //public class StudentApiController : ApiController
    //{

    //    private readonly SchoolDbContext _db;
    //    private readonly QueryCommand _query;

    //    public StudentApiController(SchoolDbContext db)
    //    {
    //        _db = db;
    //        _query = new QueryCommand(_db);
    //    }

    //    [HttpPost]
    //    [Route("Dashboard", Name = "Dashboard")]
    //    [ResponseType(typeof(StudentDashboardApiVm))]
    //    public async Task<IHttpActionResult> Dashboard(string id)
    //    {
    //        var model = new StudentDashboardApiVm();

    //        if (id != null)
    //        {
    //            var semesterId = _query.GetCurrentSemesterId();
    //            var sessionId = _query.GetCurrentSessionId();

    //            var student = await _db.Students.Include(i => i.Programme).Include(i => i.Level)
    //                                .Include(i => i.Programme.Department)
    //                                .Include(i => i.Programme.Department.Faculty).AsNoTracking()
    //                                .Where(x => (x.MatricNo.ToUpper().Equals(id.ToUpper().Trim()) ||
    //                                 x.StudentId.ToUpper().Equals(id.ToUpper()) || x.Email.Equals(id))
    //                                && x.Active.Equals(true)).FirstOrDefaultAsync();



    //            if (student != null)
    //            {
    //                var courseReg = await _db.CourseRegistrations.AsNoTracking().Where(x => x.SemesterId.Equals(semesterId)
    //                                                && x.SessionId.Equals(sessionId)
    //                                                && x.StudentId.Equals(student.StudentId)
    //                                                && x.IsApproved.Equals(true)).CountAsync();
    //                model.StudentId = student.StudentId;
    //                model.FullName = student.FullName;
    //                //model.LevelName = student.Level.LevelName;
    //                model.ProgrammeName = student.Programme.ProgrammeName;
    //                model.DepartmentName = student.Programme.Department.DeptName;
    //                model.FacultyName = student.Programme.Department.Faculty.FacultyName;
    //                model.Passport = student.Passport;
    //                model.SemesterName = await _db.Semesters.AsNoTracking().Where(x => x.ActiveSemester.Equals(true))
    //                                        .Select(s => s.SemesterName).FirstOrDefaultAsync();
    //                model.SessionName = await _db.Sessions.AsNoTracking().Where(x => x.ActiveSession.Equals(true))
    //                                        .Select(s => s.SessionName).FirstOrDefaultAsync();
    //                model.LevelName = student.Level.LevelName;

    //                if (courseReg > 1)
    //                {
    //                    model.NoOfRegCourses = courseReg;
    //                }
    //                else
    //                {
    //                    model.NoOfRegCourses = 0;
    //                }
    //                model.SchoolFees = 15000.00m;

    //                return Ok(model);
    //            }

    //        }
    //        return BadRequest("Student Not Found");
    //    }

    //    [HttpPost]
    //    [Route("CheckPayment", Name = "CheckPayment")]
    //    public IHttpActionResult CheckPayment(string studentId)
    //    {
    //        bool hasPayed = false;
    //        var sessionId = GetCurrentSessionId();
    //        var semesterId = GetCurrentSemesterId();
    //        var checkStudent = _db.Students.AsNoTracking().FirstOrDefault(x => (x.StudentId.ToUpper().Equals(studentId.ToUpper().Trim())
    //                                  || x.MatricNo.ToUpper().Equals(studentId.ToUpper().Trim())
    //                                  || x.JambRegNo.ToUpper().Equals(studentId.ToUpper().Trim()))
    //                                 && x.Active.Equals(true) && x.IsGraduated.Equals(false));
    //        if (checkStudent != null)
    //        {
    //            var levelName = _db.Levels.Where(x => x.LevelId.Equals((int)checkStudent.LevelId))
    //                .Select(s => s.LevelName).FirstOrDefault();
    //            int schCat = (int)SchoolFeeCategory.Acceptance;
    //            int schfee = (int)SchoolFeeCategory.School_Fee;
    //            if (levelName != null && levelName.Equals("100"))
    //            {
    //                var acceptancefee = _db.SchoolFeePayments.FirstOrDefault(
    //                    x => x.StudentId.Equals(checkStudent.StudentId)
    //                         && x.SessionId.Equals(sessionId) && x.Status.Equals(true)
    //                         && (x.FeeCategory.Equals(SchoolFeeCategory.Acceptance.ToString()) ||
    //                             x.FeeCategory.Equals(schCat.ToString())));
    //                if (acceptancefee != null) hasPayed = acceptancefee.Status;
    //            }

    //            var schoolfee = _db.SchoolFeePayments.FirstOrDefault(x => x.StudentId.Equals(checkStudent.StudentId)
    //                                                                      && x.SessionId.Equals(sessionId) && x.Status.Equals(true)
    //                                                                      && (x.FeeCategory.Equals(SchoolFeeCategory.School_Fee.ToString()) ||
    //                                                                          x.FeeCategory.Equals(schfee.ToString())));
    //            if (schoolfee != null)
    //            {
    //                hasPayed = schoolfee.Status;
    //            }
    //            else
    //            {
    //                hasPayed = false;
    //            }

    //        }
    //        return Ok(hasPayed);

    //    }
    //    private int GetCurrentSessionId()
    //    {
    //        var sessionId = _db.Sessions.AsNoTracking().Where(x => x.ActiveSession.Equals(true))
    //            .Select(s => s.SessionId).FirstOrDefault();
    //        return sessionId;
    //    }

    //    private int GetCurrentSemesterId()
    //    {
    //        var semesterId = _db.Semesters.AsNoTracking().Where(x => x.ActiveSemester.Equals(true))
    //            .Select(s => s.SemesterId).FirstOrDefault();
    //        return semesterId;
    //    }
    //}


//}


