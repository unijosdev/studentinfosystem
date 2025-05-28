using NumberToWordConverter;
using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampusModel;
using SwiftKampusModel.Payment;
using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SwiftKampus.Controllers
{
    [Authorize]
    [Audit(AuditingLevel = 2)]
    public class ChangeOfCourseFeesController : BaseController
    {

        public ChangeOfCourseFeesController(SchoolDbContext db) : base(db)
        {

        }

        // GET: ChangeOfCourseFees
        public ActionResult Index()
        {
            return View();
        }
        public async Task<ActionResult> GetIndex()
        {
            // dc.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var applicationfee = await _db.ChangeOfCourseFees.Include(i => i.SchoolProgramme).Include(a => a.Session)
                                                .AsNoTracking().ToListAsync();
            var data = applicationfee.Select(s => new
            {
                s.ChangeOfCourseFeeId,
                s.AmountInWords,
                s.SchoolProgramme.FullName,
                s.Session.SessionName,
                s.ApplicationFee,
                s.ChangeOfCourseType,
            }).ToList();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        public async Task<PartialViewResult> Save(int id)
        {
            var changeOfCourseFee = await _db.ChangeOfCourseFees.FindAsync(id);
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes, "SchoolProgrammeId", "FullName");
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            var changeOfCoureType = from ChangeOfCourseType s in Enum.GetValues(typeof(ChangeOfCourseType))
                                    select new { ID = s, Name = s.ToString() };

            ViewBag.ChangeOfCourseType = new SelectList(changeOfCoureType, "Name", "Name");
            return PartialView(changeOfCourseFee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Save(ChangeOfCourseFee model)
        {
            bool status = false;
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                if (model.ChangeOfCourseFeeId > 0)
                {
                    var changeOfCourseFee = await _db.ChangeOfCourseFees.FindAsync(model.ChangeOfCourseFeeId);
                    if (changeOfCourseFee != null)
                    {
                        changeOfCourseFee.AmountInWords = WordConverter.GetNumberConverter(Convert.ToInt32(model.ApplicationFee).ToString(), "naira only.");
                        changeOfCourseFee.SchoolProgrammeId = model.SchoolProgrammeId;
                        changeOfCourseFee.ApplicationFee = model.ApplicationFee;
                        changeOfCourseFee.SessionId = model.SessionId;
                        changeOfCourseFee.ChangeOfCourseType = model.ChangeOfCourseType;
                        _db.Entry(changeOfCourseFee).State = EntityState.Modified;
                        await _db.SaveChangesAsync();
                        message = "Change of Course Fee Setting Updated Successfully...";
                        return new JsonResult { Data = new { status = true, message } };
                    }
                }
                else
                {
                    model.AmountInWords = WordConverter.GetNumberConverter(Convert.ToInt32(model.ApplicationFee).ToString(), "naira only.");
                    _db.ChangeOfCourseFees.Add(model);
                    await _db.SaveChangesAsync();
                    message = "Change of Course fee Added Successfully.";
                    return new JsonResult { Data = new { status = true, message } };
                }
            }
            return new JsonResult { Data = new { status, message = "Invalid Post" } };
            //return View(subject);
        }



        // GET: ChangeOfCourseFees/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ChangeOfCourseFee changeOfCourseFee = await _db.ChangeOfCourseFees.FindAsync(id);
            if (changeOfCourseFee == null)
            {
                return HttpNotFound();
            }
            return View(changeOfCourseFee);
        }

        // GET: ChangeOfCourseFees/Create
        public ActionResult Create()
        {
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes, "SchoolProgrammeId", "ProgrammeType");
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            return View();
        }

        // POST: ChangeOfCourseFees/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "ChangeOfCourseFeeId,SchoolProgrammeId,SessionId,ApplicationFee,AmountInWords")] ChangeOfCourseFee changeOfCourseFee)
        {
            if (ModelState.IsValid)
            {
                _db.ChangeOfCourseFees.Add(changeOfCourseFee);
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes, "SchoolProgrammeId", "ProgrammeType", changeOfCourseFee.SchoolProgrammeId);
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName", changeOfCourseFee.SessionId);
            return View(changeOfCourseFee);
        }

        // GET: ChangeOfCourseFees/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ChangeOfCourseFee changeOfCourseFee = await _db.ChangeOfCourseFees.FindAsync(id);
            if (changeOfCourseFee == null)
            {
                return HttpNotFound();
            }
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes, "SchoolProgrammeId", "ProgrammeType", changeOfCourseFee.SchoolProgrammeId);
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName", changeOfCourseFee.SessionId);
            return View(changeOfCourseFee);
        }

        // POST: ChangeOfCourseFees/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "ChangeOfCourseFeeId,SchoolProgrammeId,SessionId,ApplicationFee,AmountInWords")] ChangeOfCourseFee changeOfCourseFee)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(changeOfCourseFee).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes, "SchoolProgrammeId", "ProgrammeType", changeOfCourseFee.SchoolProgrammeId);
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName", changeOfCourseFee.SessionId);
            return View(changeOfCourseFee);
        }

        // GET: ChangeOfCourseFees/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ChangeOfCourseFee changeOfCourseFee = await _db.ChangeOfCourseFees.FindAsync(id);
            if (changeOfCourseFee == null)
            {
                return HttpNotFound();
            }
            return View(changeOfCourseFee);
        }

        // POST: ChangeOfCourseFees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            ChangeOfCourseFee changeOfCourseFee = await _db.ChangeOfCourseFees.FindAsync(id);
            _db.ChangeOfCourseFees.Remove(changeOfCourseFee);
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
