using SwiftKampus.Models;
using System.Web.Http;

namespace SwiftKampus.Controllers.WebApi
{
    [System.Web.Http.RoutePrefix("AccomodationApi")]
    public class AccomodationApiController : ApiController
    {
        private readonly SchoolDbContext _db;

        public AccomodationApiController(SchoolDbContext db)
        {
            _db = db;
        }

        //[System.Web.Http.HttpPost]
        //[System.Web.Http.Route("CheckHostel", Name = "CheckHostel")]
        //public async Task<IHttpActionResult> CheckHostel(string studentId)
        //{
        //    if (string.IsNullOrEmpty(studentId))
        //    {
        //        return BadRequest("Student cannot be empty or null");
        //    }
        //    var student = await _db.Students.Include(i => i.Programme).AsNoTracking().Where(x => x.StudentId.Equals(studentId))
        //                                    .Select(s => new
        //                                    {
        //                                        gender = s.Gender,
        //                                        faculty = s.Programme.Department.FacultyId
        //                                    }).FirstOrDefaultAsync();
        //    if (student == null)
        //    {
        //        return BadRequest("Student doesn't exist");
        //    }
        //    var sessionId = await _db.Sessions.AsNoTracking().Where(x => x.ActiveSession.Equals(true))
        //                    .Select(s => s.SessionId).FirstOrDefaultAsync();

        //    var alreadyBooked = _db.AssignedRooms.AsNoTracking().Where(x => x.StudentId.Equals(studentId)
        //                                                                    && x.SessionId.Equals(sessionId));
        //    if (alreadyBooked.Any())
        //    {
        //        return Ok("Already Booked");
        //    }

        //    var isHostelAvailableNow = await _db.SessionAccomodations.Include(i => i.Room).AsNoTracking().ToListAsync();
        //    int isHostelAvailable = 0;
        //    if (isHostelAvailableNow != null)
        //    {
        //        isHostelAvailable = isHostelAvailableNow.Sum(x => x.Room.RoomCapacity);
        //    }
        //    var assignedRooms = _db.AssignedRooms.AsNoTracking().Count();
        //    if (assignedRooms < isHostelAvailable)
        //    {
        //        var isPayForApplication = await _db.HostelApplications.AsNoTracking()
        //            .Where(x => x.StudentId.Equals(studentId) && x.SessionId.Equals(sessionId))
        //            .FirstOrDefaultAsync();
        //        if (isPayForApplication != null && isPayForApplication.IsPayed)
        //        {

        //            var myHostelId = await _db.AssignedHostels.Where(x => x.FacultyId.Equals(student.faculty))
        //                .Select(x => x.HostelId).ToListAsync();
        //            var hostelList = new List<Hostel>();
        //            foreach (var hostel in myHostelId)
        //            {
        //                hostelList.Add(await _db.SessionAccomodations.Include(i => i.Hostel).AsNoTracking()
        //                    .Where(x => x.Hostel.HostelId.Equals(hostel))
        //                    .Select(s => s.Hostel).FirstOrDefaultAsync());
        //            }


        //            if (hostelList.Any())
        //            {
        //                var availableHostel = hostelList.Select(x => new { x.HostelId, x.HostelName });
        //                return Ok(availableHostel);
        //            }
        //            return BadRequest("HostelNotFound");

        //        }
        //        return BadRequest("Make Payment");

        //    }

        //    return BadRequest("HostelNotFound");
        //}
    }
}
