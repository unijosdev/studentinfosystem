using SwiftKampus.Models;
using SwiftKampusModel;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SwiftKampus.Controllers
{
    public class ConvertImageController : BaseController
    {

        public ConvertImageController(SchoolDbContext db) : base(db)
        {

        }


        // GET: ConvertImage
        public async Task<ActionResult> RenderImage(string studentId)
        {
            Student student = await _db.Students.FindAsync(studentId);

            byte[] photoBack = student.Passport;

            return File(photoBack, "image/png");
        }
    }
}