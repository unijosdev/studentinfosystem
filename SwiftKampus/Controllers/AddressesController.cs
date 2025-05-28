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
    public class AddressesController : BaseController
    {

        public AddressesController(SchoolDbContext db) : base(db)
        {

        }

        // GET: Addresses
        public async Task<ActionResult> Index()
        {
            ViewData.Add("ActionMessage", "View list of address");
            return View(await _db.Addresses.AsNoTracking().Where(x => x.UserId.Equals(userId)).ToListAsync());
        }

        // GET: Addresses/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            ViewData.Add("ActionMessage", "View Details of address");
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Address address = await _db.Addresses.FindAsync(id);
            if (address == null)
            {
                return HttpNotFound();
            }
            return View(address);
        }

        // GET: Addresses/Create
        public ActionResult Create()
        {
            var addresstype = from AddressType s in Enum.GetValues(typeof(AddressType))
                              select new { ID = s, Name = s.ToString() };
            var state = from State s in Enum.GetValues(typeof(State))
                        select new { ID = s, Name = s.ToString() };
            var lga = from LgaEnum s in Enum.GetValues(typeof(LgaEnum))
                      select new { ID = s, Name = s.ToString() };

            ViewBag.Lga = new SelectList(lga, "Name", "Name");

            ViewBag.AddressType = new SelectList(addresstype, "Name", "Name");
            ViewBag.State = new SelectList(state, "Name", "Name");
            ViewData.Add("ActionMessage", "View create page for address");
            return View();
        }

        // POST: Addresses/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Address address)
        {
            if (ModelState.IsValid)
            {
                address.UserId = userId;
                _db.Addresses.Add(address);
                await _db.SaveChangesAsync();
                ViewData.Add("ActionMessage", $"Saves address ({address.AddressType})");
                return RedirectToAction("Index");
            }
            var addresstype = from AddressType s in Enum.GetValues(typeof(AddressType))
                              select new { ID = s, Name = s.ToString() };
            var state = from State s in Enum.GetValues(typeof(State))
                        select new { ID = s, Name = s.ToString() };
            var lga = from LgaEnum s in Enum.GetValues(typeof(LgaEnum))
                      select new { ID = s, Name = s.ToString() };

            ViewBag.Lga = new SelectList(lga, "Name", "Name");

            ViewBag.AddressType = new SelectList(addresstype, "Name", "Name");
            ViewBag.State = new SelectList(state, "Name", "Name");
            ViewData.Add("ActionMessage", $"Error: tried to save ({address.AddressType})");
            return View(address);
        }


        public PartialViewResult PCreate()
        {
            var addresstype = from AddressType s in Enum.GetValues(typeof(AddressType))
                              select new { ID = s, Name = s.ToString() };
            var state = from State s in Enum.GetValues(typeof(State))
                        select new { ID = s, Name = s.ToString() };
            var lga = from LgaEnum s in Enum.GetValues(typeof(LgaEnum))
                      select new { ID = s, Name = s.ToString() };

            
            var address = _db.Addresses.AsNoTracking()
                            .Where(x => x.UserId.Trim().ToUpper().Equals(userId.Trim().ToUpper()))
                            .FirstOrDefault();
            var model = _applicantType;
            ViewBag.Model = model;
            ViewBag.Lga = new SelectList(lga, "Name", "Name", address?.Lga);

            ViewBag.AddressType = new SelectList(addresstype, "Name", "Name", address?.AddressType);
            ViewBag.State = new SelectList(state, "Name", "Name", address?.State);
            ViewData.Add("ActionMessage", "View create page for address");
            return PartialView(address);
        }

        // POST: Addresses/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> PCreate(Address model)
        {
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                var address = await _db.Addresses.AsNoTracking().Where(x => x.AddressId.Equals(model.AddressId) ||
                                        x.UserId.Trim().ToUpper().Equals(userId.Trim().ToUpper())).FirstOrDefaultAsync();
                if (address != null)
                {
                    address.AddressType = model.AddressType;
                    address.HouseNo = model.HouseNo;
                    address.Lga = model.Lga;
                    address.State = model.State;
                    address.Town = model.Town;
                    address.Street = model.Street;
                    _db.Entry(address).State = EntityState.Modified;
                    _db.SaveChanges();                   
                    message = $"{model.AddressType} Updated Successfully...";
                    return new JsonResult { Data = new { status = true, message } };
                }
                else
                {
                    model.UserId = userId;
                    _db.Addresses.Add(model);
                    _db.SaveChanges();                   
                    message = $"{model.AddressType} Added Successfully.";
                    return new JsonResult { Data = new { status = true, message } };
                }
            }
            message = "Please fill in all required field";
            return new JsonResult { Data = new { status = false, message } };
        }

        // GET: Addresses/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                ViewData.Add("ActionMessage", "Error: Editing address page requires Id");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Address address = await _db.Addresses.FindAsync(id);
            if (address == null)
            {
                return HttpNotFound();
            }
            var addresstype = from AddressType s in Enum.GetValues(typeof(AddressType))
                              select new { ID = s, Name = s.ToString() };
            var state = from State s in Enum.GetValues(typeof(State))
                        select new { ID = s, Name = s.ToString() };
            var lga = from LgaEnum s in Enum.GetValues(typeof(LgaEnum))
                      select new { ID = s, Name = s.ToString() };

            ViewBag.Lga = new SelectList(lga, "Name", "Name");

            ViewBag.AddressType = new SelectList(addresstype, "Name", "Name");
            ViewBag.State = new SelectList(state, "Name", "Name");
            ViewData.Add("ActionMessage", $"View Address edit page with id:{id}");
            return View(address);
        }

        // POST: Addresses/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Address address)
        {
            if (ModelState.IsValid)
            {
                address.UserId = userId;
                _db.Entry(address).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                ViewData.Add("ActionMessage", $"Address is edited successfully: {address.AddressType}");

                return RedirectToAction("Index");
            }
            var addresstype = from AddressType s in Enum.GetValues(typeof(AddressType))
                              select new { ID = s, Name = s.ToString() };
            var state = from State s in Enum.GetValues(typeof(State))
                        select new { ID = s, Name = s.ToString() };
            var lga = from LgaEnum s in Enum.GetValues(typeof(LgaEnum))
                      select new { ID = s, Name = s.ToString() };

            ViewBag.Lga = new SelectList(lga, "Name", "Name");

            ViewBag.AddressType = new SelectList(addresstype, "Name", "Name");
            ViewBag.State = new SelectList(state, "Name", "Name");
            ViewData.Add("ActionMessage", $"Error Editing Address: Address is edited successfully: {address.AddressType}");

            return View(address);
        }

        // GET: Addresses/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            ViewData.Add("ActionMessage", $"Delete addresss with id: {id}");

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Address address = await _db.Addresses.FindAsync(id);
            if (address == null)
            {
                return HttpNotFound();
            }
            return View(address);
        }

        // POST: Addresses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Address address = await _db.Addresses.FindAsync(id);
            if (address != null) _db.Addresses.Remove(address);
            await _db.SaveChangesAsync();
            ViewData.Add("ActionMessage", $"Address is deleted successfully: {address.AddressType}");
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
