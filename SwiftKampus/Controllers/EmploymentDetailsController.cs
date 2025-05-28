using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampus.ViewModels.StudentBioData;
using SwiftKampusModel.AddmissionApplicant;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SwiftKampus.Controllers
{
    [Authorize]
    [Audit(AuditingLevel = 2)]
    public class EmploymentDetailsController : BaseController
    {

        public EmploymentDetailsController(SchoolDbContext db) : base(db)
        {

        }

        // GET: EmploymentDetails
        public async Task<ActionResult> Index()
        {
            return View(await _db.EmploymentDetails.ToListAsync());
        }

        // GET: EmploymentDetails/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EmploymentDetail employmentDetail = await _db.EmploymentDetails.FindAsync(id);
            if (employmentDetail == null)
            {
                return HttpNotFound();
            }
            return View(employmentDetail);
        }

        // GET: EmploymentDetails/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: EmploymentDetails/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(List<EmploymentDetail> employmentDetail)
        {
            var message = "";
            if (ModelState.IsValid)
            {
                foreach (var item in employmentDetail)
                {
                    item.ApplicantId = userId;
                    _db.EmploymentDetails.Add(item);

                }
                await _db.SaveChangesAsync();
                message = $"{employmentDetail} Employment Detail added successfully";
                return new JsonResult { Data = new { status = true, message } };
            }
            message = "Bad Data Supplied, Please check and try again...";
            return new JsonResult { Data = new { status = false, message } };
        }

        // GET: EmploymentDetails/Create
        public ActionResult PCreate()
        {
            var user = User.IsInRole(RoleName.Student);
            if (user) ViewBag.isStudent = "Student";

            ViewBag.ApplicantType = _applicantType;
            var emplymentHistory = _db.EmploymentDetails.AsNoTracking().Where(x => x.ApplicantId.Equals(userId))
                                        .ToList();
            var model = new EmploymentDetailVm()
            {
                EmploymentDetail = new EmploymentDetail(),
                EmploymentDetails = emplymentHistory
            };
            return View(model);
        }

        // POST: EmploymentDetails/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> PCreate(List<EmploymentDetail> employmentDetail)
        {
            var message = "";
            if (ModelState.IsValid)
            {
                foreach (var item in employmentDetail)
                {
                    item.ApplicantId = userId;
                    _db.EmploymentDetails.Add(item);
                }
                var checkEmp = _db.EmploymentDetails.AsNoTracking()
                                   .Where(x => x.ApplicantId.Equals(userId)).ToList();
                if (checkEmp != null && checkEmp.Any())
                {
                    foreach (var item in checkEmp)
                    {
                        _db.Entry(item).State = EntityState.Deleted;
                    }
                }
                await _db.SaveChangesAsync();
                message = $"{employmentDetail.Count} Employment Detail added successfully";
                return new JsonResult { Data = new { status = true, message } };
            }
            message = "Bad Data Supplied, Please check and try again...";
            return new JsonResult { Data = new { status = false, message } };
        }

        // GET: EmploymentDetails/Create
        public PartialViewResult PGCreate()
        {
            ViewBag.ApplicantType = _applicantType;
            var emplymentHistory = _db.EmploymentDetails.AsNoTracking().Where(x => x.ApplicantId.Equals(userId))
                                        .ToList();
            return PartialView(emplymentHistory);
        }


        // GET: EmploymentDetails/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EmploymentDetail employmentDetail = await _db.EmploymentDetails.FindAsync(id);
            if (employmentDetail == null)
            {
                return HttpNotFound();
            }
            return View(employmentDetail);
        }

        // POST: EmploymentDetails/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "EmploymentDetailId,ApplicantId,EmployeeName,Address,FromDate,ToDate")] EmploymentDetail employmentDetail)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(employmentDetail).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(employmentDetail);
        }

        // GET: EmploymentDetails/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EmploymentDetail employmentDetail = await _db.EmploymentDetails.FindAsync(id);
            if (employmentDetail == null)
            {
                return HttpNotFound();
            }
            return View(employmentDetail);
        }

        // POST: EmploymentDetails/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            EmploymentDetail employmentDetail = await _db.EmploymentDetails.FindAsync(id);
            _db.EmploymentDetails.Remove(employmentDetail);
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
