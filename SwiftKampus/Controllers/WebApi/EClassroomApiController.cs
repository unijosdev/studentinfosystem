//namespace SwiftKampus.Controllers.WebApi
//{
    //[System.Web.Http.RoutePrefix("EClassroomApi")]
    //public class EClassroomApiController : ApiController
    //{
    //    private readonly SchoolDbContext _db;
    //    public readonly QueryCommand _query;

    //    public EClassroomApiController(SchoolDbContext db)
    //    {
    //        _db = db;
    //        _query = new QueryCommand(_db);
    //    }


    //    [HttpPost]
    //    [Route("MyCourses", Name = "MyCourses")]
    //    public async Task<IHttpActionResult> MyCourses(string studentId)
    //    {
    //        var courseList = new List<Course>();
    //        //var semesterId = _query.GetCurrentSemesterId();
    //        //var sessionId = _query.GetCurrentSessionId();

    //        var id = studentId;
    //        var courseReg = await _db.CourseRegistrations.AsNoTracking().Where(x => x.StudentId.Equals(id)
    //                                && x.Semester.SemesterId.Equals(semesterId) && x.Session.SessionId.Equals(sessionId))
    //                                   .Select(s => s.CourseId).ToListAsync();
    //        foreach (var courseId in courseReg)
    //        {
    //            var course = await _db.Courses.Include(c => c.Level).Include(c => c.Programme).AsNoTracking()
    //                .Where(x => x.CourseId.Equals(courseId)).FirstOrDefaultAsync();
    //            courseList.Add(course);
    //        }

    //        var data = courseList.Select(s => new
    //        {
    //            s.CourseId,
    //            s.Level.LevelName,
    //            s.CourseName,
    //            s.CourseCode,
    //            s.Programme.ProgrammeName,
    //            s.CourseDescription,
    //            s.Credits
    //        }).ToList();
    //        return Ok(data);
    //    }

    //    [HttpPost]
    //    [Route("GetModules", Name = "GetModules")]
    //    public async Task<IHttpActionResult> GetModules(int courseId)
    //    {
    //        var modules = await _db.Modules.AsNoTracking().Where(x => x.CourseId.Equals(courseId))
    //                        .ToListAsync();
    //        return Ok(modules);
    //    }

    //    [HttpPost]
    //    [Route("GetTopics", Name = "GetTopics")]
    //    public async Task<IHttpActionResult> GetTopics(int moduleId)
    //    {
    //        var topics = await _db.Topics.AsNoTracking().Where(x => x.ModuleId.Equals(moduleId))
    //                .ToListAsync();
    //        return Ok(topics);
    //    }

    //    [HttpPost]
    //    [Route("GetTopicContent", Name = "GetTopicContent")]
    //    public async Task<IHttpActionResult> GetTopicContent(int topicId)
    //    {
    //        var topics = await _db.TopicMaterials.AsNoTracking().Where(x => x.TopicId.Equals(topicId))
    //            .ToListAsync();
    //        return Ok(topics);
    //    }


    //}
//}
