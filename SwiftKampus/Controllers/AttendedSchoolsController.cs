using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampus.ViewModels;
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
    public class AttendedSchoolsController : BaseController
    {

        public AttendedSchoolsController(SchoolDbContext db) : base(db)
        {

        }


        // GET: AttendedSchools
        public async Task<ActionResult> Index()
        {
            var result = ConfirmApplicationFee();
            if (result != null)
                return result;

            //var attendedSchools = _db.AttendedSchools.AsNoTracking();
            var attendedSchools = _db.AttendedSchools.AsNoTracking().Where(x => x.ApplicantId.Equals(userId));

            return View(await attendedSchools.ToListAsync());
        }

        public async Task<PartialViewResult> RegIndex()
        {
            var result = ConfirmAcceptanceFee();
            if (result != null)
                return (PartialViewResult)result;

            //var attendedSchools = await _db.AttendedSchools.AsNoTracking()
            //                              .Where(x => x.ApplicantId.Equals(userId)).ToListAsync();

            //ViewBag.schoolProgramme = await _db.Applicants.Include(s => s.SchoolProgramme)
            //                              .Where(s => s.ApplicantEmail == userId).Select(s => s.SchoolProgramme.ProgrammeCategory).FirstOrDefaultAsync();

            var attendedSchoolsVm = new AttendedSchoolsVm
            {
                AttendedSchool = new AttendedSchool(),
                AttendedSchools = await _db.AttendedSchools.AsNoTracking().Where(x => x.ApplicantId.Equals(userId)).ToListAsync(),
                SchoolProgramme = await _db.Applicants.Include(s => s.SchoolProgramme).Where(s => s.ApplicantEmail == userId).Select(s => s.SchoolProgramme).FirstOrDefaultAsync()
            };


            return PartialView(attendedSchoolsVm);
        }

        // Show Transcript request view for attended schools
        public PartialViewResult RequestTranscript()
        {
            var result = ConfirmAcceptanceFee();
            if (result != null)
                return (PartialViewResult)result;

            var attendedSchoolsVm = new AttendedSchoolsVm
            {
                AttendedSchool = new AttendedSchool(),
                AttendedSchools = _db.AttendedSchools.AsNoTracking().Where(x => x.ApplicantId.Equals(userId)).ToList(),
                SchoolProgramme = _db.Applicants.Include(s => s.SchoolProgramme).Where(s => s.ApplicantEmail == userId).Select(s => s.SchoolProgramme).FirstOrDefault()
            };


            return PartialView("RegIndex",attendedSchoolsVm);
        }

        // GET: AttendedSchools/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AttendedSchool attendedSchool = await _db.AttendedSchools.FindAsync(id);
            if (attendedSchool == null)
            {
                return HttpNotFound();
            }
            return View(attendedSchool);
        }

        // GET: AttendedSchools/Create
        public ActionResult Create(string IsEdit)
        {
            var schoolProgrammeName = _db.Applicants.Include(s => s.SchoolProgramme)
                                          .Where(s => s.ApplicantEmail == userId).FirstOrDefault();

            ViewBag.schoolProgrammeName = schoolProgrammeName.SchoolProgramme.ProgrammeCategory;

            var result = ConfirmApplicationFee();
            if (result != null)
                return result;

            if (string.IsNullOrEmpty(IsEdit))
            {
                if (_db.AttendedSchools.Any(x => x.ApplicantId.Equals(userId)))
                {
                    var attendedSchoolsVm = new AttendedSchoolsVm
                    {
                        AttendedSchool = new AttendedSchool(),
                        AttendedSchools = _db.AttendedSchools.AsNoTracking().Where(x => x.ApplicantId.Equals(userId)).ToList(),
                        SchoolProgramme = _db.Applicants.Include(s => s.SchoolProgramme).Where(s => s.ApplicantEmail == userId).Select(s => s.SchoolProgramme).FirstOrDefault()
                    };

                    var attendedSchools = _db.AttendedSchools.AsNoTracking()
                                            .Where(x => x.ApplicantId.Equals(userId)).ToList();

                    //return PartialView("RegIndex", attendedSchoolsVm);
                    return View("EditAttendedSchools", attendedSchoolsVm);
                }
                else
                {
                    return View();
                }
            }

            return View();
        }

        // POST: AttendedSchools/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(List<AttendedSchool> attendedSchool)
        {
            string message = "";
            if (ModelState.IsValid)
            {
                foreach (var item in attendedSchool)
                {
                    item.ApplicantId = userId;
                    _db.AttendedSchools.Add(item);
                }
                await _db.SaveChangesAsync();
                message = $"{attendedSchool} Schools are added successfully";
                return new JsonResult { Data = new { status = true, message } };
            }

            message = $"Something is wrong with the data, Please check and try again";
            return new JsonResult { Data = new { status = false, message } };
        }

        // GET: AttendedSchools/Create
        public ActionResult PCreate()
        {
            var attendedSchool = _db.AttendedSchools.AsNoTracking().Where(x => x.ApplicantId.Equals(userId))
                                       .ToList();

            var model = new AttendedSchoolVm()
            {
                AttendedSchool = new AttendedSchool(),
                AttendedSchools = attendedSchool
            };
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> PCreate(List<AttendedSchool> attendedSchool)
        {
            string message = "";
            if (ModelState.IsValid)
            {
                foreach (var item in attendedSchool)
                {
                    item.ApplicantId = userId;
                    _db.AttendedSchools.Add(item);
                }

                var checkAttendedSchool = _db.AttendedSchools.AsNoTracking()
                                  .Where(x => x.ApplicantId.Equals(userId)).ToList();
                if (checkAttendedSchool != null && checkAttendedSchool.Any())
                {
                    foreach (var item in checkAttendedSchool)
                    {
                        _db.Entry(item).State = EntityState.Deleted;
                    }
                }
                await _db.SaveChangesAsync();
                message = $"{attendedSchool.Count()} Schools are added successfully";
                return new JsonResult { Data = new { status = true, message } };
            }

            message = $"Something is wrong with the data, Please check and try again";
            return new JsonResult { Data = new { status = false, message } };
        }

        // GET: AttendedSchools/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            var result = ConfirmApplicationFee();
            if (result != null)
                return result;

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AttendedSchool attendedSchool = await _db.AttendedSchools.FindAsync(id);
            if (attendedSchool == null)
            {
                return HttpNotFound();
            }

            return View(attendedSchool);
        }

        // POST: AttendedSchools/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(AttendedSchool attendedSchool)
        {
            if (ModelState.IsValid)
            {
                attendedSchool.ApplicantId = userId;
                _db.Entry(attendedSchool).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(attendedSchool);
        }

        // GET: AttendedSchools/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AttendedSchool attendedSchool = await _db.AttendedSchools.FindAsync(id);
            if (attendedSchool == null)
            {
                return HttpNotFound();
            }
            return View(attendedSchool);
        }

        // POST: AttendedSchools/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            AttendedSchool attendedSchool = await _db.AttendedSchools.FindAsync(id);
            if (attendedSchool != null) _db.AttendedSchools.Remove(attendedSchool);
            await _db.SaveChangesAsync();
            //return RedirectToAction("Index");
            return RedirectToAction("Create");
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
