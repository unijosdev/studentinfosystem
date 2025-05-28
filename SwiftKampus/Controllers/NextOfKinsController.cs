using SwiftKampus.Models;
using SwiftKampus.Services;

using SwiftKampusModel;

using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SwiftKampus.Controllers
{
    [Authorize]
    [Audit(AuditingLevel = 2)]
    public class NextOfKinsController : BaseController
    {

        public NextOfKinsController(SchoolDbContext db) : base(db)
        {

        }
        // GET: NextOfKins
        public async Task<ActionResult> Index()
        {
            return View(await _db.NextOfKins.AsNoTracking().Where(x => x.UserId.Equals(userId)).ToListAsync());
        }

        // GET: NextOfKins/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            NextOfKin nextOfKin = await _db.NextOfKins.FindAsync(id);
            if (nextOfKin == null)
            {
                return HttpNotFound();
            }
            return View(nextOfKin);
        }

        // GET: NextOfKins/Create
        public ActionResult Create()
        {
            var mygender = from Gender s in Enum.GetValues(typeof(Gender))
                           select new { ID = s, Name = s.ToString() };
            var relationship = from Relationship s in Enum.GetValues(typeof(Relationship))
                               select new { ID = s, Name = s.ToString() };

            ViewBag.Relationship = new SelectList(relationship, "Name", "Name");
            ViewBag.Gender = new SelectList(mygender, "Name", "Name");
            return View();
        }

        // POST: NextOfKins/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(NextOfKin nextOfKin)
        {
            if (ModelState.IsValid)
            {
                nextOfKin.UserId = userId;
                _db.NextOfKins.Add(nextOfKin);
                await _db.SaveChangesAsync();
                if (User.IsInRole(RoleName.Student)) { }
                return RedirectToAction("Index");
            }
            var mygender = from Gender s in Enum.GetValues(typeof(Gender))
                           select new { ID = s, Name = s.ToString() };
            var relationship = from Relationship s in Enum.GetValues(typeof(Relationship))
                               select new { ID = s, Name = s.ToString() };

            ViewBag.Relationship = new SelectList(relationship, "Name", "Name");
            ViewBag.Gender = new SelectList(mygender, "Name", "Name");
            return View(nextOfKin);
        }

        // GET: NextOfKins/Create
        public ActionResult PCreate()
        {
            var mygender = from Gender s in Enum.GetValues(typeof(Gender))
                           select new { ID = s, Name = s.ToString() };
            var relationship = from Relationship s in Enum.GetValues(typeof(Relationship))
                               select new { ID = s, Name = s.ToString() };

            ViewBag.Relationship = new SelectList(relationship, "Name", "Name");
            ViewBag.Gender = new SelectList(mygender, "Name", "Name");
            ViewBag.ApplicantType = _applicantType;

            var student = _db.Students.Where(z => z.Email.Equals(userId)).FirstOrDefault();
            if (student != null)
            {
                ViewBag.Student = true;
            }
            var nextofKin = _db.NextOfKins.AsNoTracking()
                           .Where(x => x.UserId.Trim().ToUpper().Equals(userId.Trim().ToUpper()))
                           .FirstOrDefault();
            return View(nextofKin);
        }

        // POST: NextOfKins/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> PCreate(NextOfKin model)
        {
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                var nextOfKin = await _db.NextOfKins.AsNoTracking().Where(x => x.NextOfKinId.Equals(model.NextOfKinId) ||
                                        x.UserId.Trim().ToUpper().Equals(userId.Trim().ToUpper())).FirstOrDefaultAsync();
                if (nextOfKin != null)
                {

                    if (nextOfKin != null)
                    {
                        nextOfKin.FirstName = model.FirstName;
                        nextOfKin.LastName = model.LastName;
                        nextOfKin.MiddleName = model.MiddleName;
                        nextOfKin.Gender = model.Gender;
                        nextOfKin.Email = model.Email;
                        nextOfKin.PhoneNumber = model.PhoneNumber;
                        nextOfKin.Relationship = model.Relationship;
                        nextOfKin.UserId = userId;
                        nextOfKin.Address = model.Address;
                        _db.Entry(nextOfKin).State = EntityState.Modified;
                        _db.SaveChanges();

                        var student = _db.Students.AsNoTracking().Where(x => x.Email.Equals(userId)).FirstOrDefault();                       
                        message = $"{model.FullName} Next of Kin Details Updated Successfully...";
                        return new JsonResult { Data = new { status = true, message } };
                    }
                }
                else
                {
                    model.UserId = userId;
                    _db.NextOfKins.Add(model);
                    _db.SaveChanges();
                    message = $"{model.FullName} Added Successfully.";
                    return new JsonResult { Data = new { status = true, message } };
                }
            }
            return new JsonResult { Data = new { status = false, message = "Data entered is not valid" } };
        }


        // GET: NextOfKins/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            NextOfKin nextOfKin = await _db.NextOfKins.FindAsync(id);
            if (nextOfKin == null)
            {
                return HttpNotFound();
            }
            var mygender = from Gender s in Enum.GetValues(typeof(Gender))
                           select new { ID = s, Name = s.ToString() };
            var relationship = from Relationship s in Enum.GetValues(typeof(Relationship))
                               select new { ID = s, Name = s.ToString() };

            ViewBag.Relationship = new SelectList(relationship, "Name", "Name");
            ViewBag.Gender = new SelectList(mygender, "Name", "Name");
            return View(nextOfKin);
        }

        // POST: NextOfKins/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(NextOfKin nextOfKin)
        {
            if (ModelState.IsValid)
            {
                nextOfKin.UserId = userId;
                _db.Entry(nextOfKin).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            var mygender = from Gender s in Enum.GetValues(typeof(Gender))
                           select new { ID = s, Name = s.ToString() };
            var relationship = from Relationship s in Enum.GetValues(typeof(Relationship))
                               select new { ID = s, Name = s.ToString() };

            ViewBag.Relationship = new SelectList(relationship, "Name", "Name");
            ViewBag.Gender = new SelectList(mygender, "Name", "Name");
            return View(nextOfKin);
        }

        // GET: NextOfKins/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            NextOfKin nextOfKin = await _db.NextOfKins.FindAsync(id);
            if (nextOfKin == null)
            {
                return HttpNotFound();
            }
            return View(nextOfKin);
        }

        // POST: NextOfKins/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            NextOfKin nextOfKin = await _db.NextOfKins.FindAsync(id);
            if (nextOfKin != null) _db.NextOfKins.Remove(nextOfKin);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
