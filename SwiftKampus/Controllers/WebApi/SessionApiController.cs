using SwiftKampus.Models;
using SwiftKampusModel;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace SwiftKampus.Controllers.WebApi
{
    public class SessionApiController : ApiController
    {
        private readonly SchoolDbContext db;

        public SessionApiController(SchoolDbContext context)
        {
            db = context;
        }

        // GET: api/SessionApi
        public IQueryable<Session> GetSessions()
        {
            return db.Sessions;
        }

        // GET: api/SessionApi/5
        [ResponseType(typeof(Session))]
        public async Task<IHttpActionResult> GetSession(int id)
        {
            Session session = await db.Sessions.FindAsync(id);
            if (session == null)
            {
                return NotFound();
            }

            return Ok(session);
        }

        // PUT: api/SessionApi/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutSession(int id, Session session)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != session.SessionId)
            {
                return BadRequest();
            }

            db.Entry(session).State = System.Data.Entity.EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SessionExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/SessionApi
        [ResponseType(typeof(Session))]
        public async Task<IHttpActionResult> PostSession(Session session)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Sessions.Add(session);
            await db.SaveChangesAsync();

            return CreatedAtRoute("DefaultApi", new { id = session.SessionId }, session);
        }

        // DELETE: api/SessionApi/5
        [ResponseType(typeof(Session))]
        public async Task<IHttpActionResult> DeleteSession(int id)
        {
            Session session = await db.Sessions.FindAsync(id);
            if (session == null)
            {
                return NotFound();
            }

            db.Sessions.Remove(session);
            await db.SaveChangesAsync();

            return Ok(session);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool SessionExists(int id)
        {
            return db.Sessions.Count(e => e.SessionId == id) > 0;
        }
    }
}