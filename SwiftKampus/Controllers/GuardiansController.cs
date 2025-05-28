using Microsoft.AspNet.Identity;
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
    public class GuardiansController : BaseController
    {

        public GuardiansController(SchoolDbContext db) : base(db)
        {

        }
        // GET: Guardians
        public async Task<ActionResult> Index()
        {
            return View(await _db.Guardians.AsNoTracking().Where(x => x.UserId.Equals(userId)).ToListAsync());
        }

        // GET: Guardians/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Guardian guardian = await _db.Guardians.FindAsync(id);
            if (guardian == null)
            {
                return HttpNotFound();
            }
            return View(guardian);
        }

        // GET: Guardians/Create
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

        // POST: Guardians/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Guardian guardian)
        {
            if (ModelState.IsValid)
            {
                guardian.UserId = userId;
                _db.Guardians.Add(guardian);
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            var mygender = from Gender s in Enum.GetValues(typeof(Gender))
                           select new { ID = s, Name = s.ToString() };
            var relationship = from Relationship s in Enum.GetValues(typeof(Relationship))
                               select new { ID = s, Name = s.ToString() };

            ViewBag.Relationship = new SelectList(relationship, "Name", "Name");
            ViewBag.Gender = new SelectList(mygender, "Name", "Name");

            return View(guardian);
        }



        // GET: Guardians/Create
        public ActionResult PCreate()
        {
            var mygender = from Gender s in Enum.GetValues(typeof(Gender))
                           select new { ID = s, Name = s.ToString() };
            var relationship = from Relationship s in Enum.GetValues(typeof(Relationship))
                               select new { ID = s, Name = s.ToString() };

            ViewBag.Relationship = new SelectList(relationship, "Name", "Name");
            ViewBag.Gender = new SelectList(mygender, "Name", "Name");
            var guradian = _db.Guardians.AsNoTracking()
                            .Where(x => x.UserId.Trim().ToUpper().Equals(userId.Trim().ToUpper()))
                            .FirstOrDefault();
            return View(guradian);
        }

        // POST: Guardians/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> PCreate(Guardian model)
        {
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                var guardian = await _db.Guardians.AsNoTracking().Where(x => x.GuardianId.Equals(model.GuardianId) ||
                                      x.UserId.Trim().ToUpper().Equals(userId.Trim().ToUpper())).FirstOrDefaultAsync();
                if (guardian != null)
                {                   
                    if (guardian != null)
                    {
                        guardian.Address = model.Address;
                        guardian.Email = model.Email;
                        guardian.FirstName = model.FirstName;
                        guardian.LastName = model.LastName;
                        guardian.MiddleName = model.MiddleName;
                        guardian.PhoneNumber = model.PhoneNumber;
                        guardian.Relationship = model.Relationship;
                        _db.Entry(guardian).State = EntityState.Modified;
                        _db.SaveChanges();

                        message = $"{model.FullName} Updated Successfully...";
                        return new JsonResult { Data = new { status = true, message } };

                    }
                }
                else
                {
                    model.UserId = userId;
                    _db.Guardians.Add(model);
                    _db.SaveChanges();

                    message = $"{model.FullName} Added Successfully.";
                    return new JsonResult { Data = new { status = true, message } };
                }
            }
            message = "Please fill in all required feild";
            return new JsonResult { Data = new { status = false, message } };
        }

        // GET: Guardians/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Guardian guardian = await _db.Guardians.FindAsync(id);
            if (guardian == null)
            {
                return HttpNotFound();
            }
            var mygender = from Gender s in Enum.GetValues(typeof(Gender))
                           select new { ID = s, Name = s.ToString() };
            var relationship = from Relationship s in Enum.GetValues(typeof(Relationship))
                               select new { ID = s, Name = s.ToString() };

            ViewBag.Relationship = new SelectList(relationship, "Name", "Name");
            ViewBag.Gender = new SelectList(mygender, "Name", "Name");
            return View(guardian);
        }


        // POST: Guardians/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Guardian guardian)
        {
            if (ModelState.IsValid)
            {
                guardian.UserId = userId;
                _db.Entry(guardian).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("Details", "Students");
            }
            var mygender = from Gender s in Enum.GetValues(typeof(Gender))
                           select new { ID = s, Name = s.ToString() };
            var relationship = from Relationship s in Enum.GetValues(typeof(Relationship))
                               select new { ID = s, Name = s.ToString() };

            ViewBag.Relationship = new SelectList(relationship, "Name", "Name");
            ViewBag.Gender = new SelectList(mygender, "Name", "Name");
            return View(guardian);
        }

        // GET: Guardians/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Guardian guardian = await _db.Guardians.FindAsync(id);
            if (guardian == null)
            {
                return HttpNotFound();
            }
            return View(guardian);
        }

        // POST: Guardians/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Guardian guardian = await _db.Guardians.FindAsync(id);
            _db.Guardians.Remove(guardian);
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
