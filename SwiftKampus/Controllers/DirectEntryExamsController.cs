using System.Data.Entity;
using System.Threading.Tasks;
using System.Net;
using System.Web.Mvc;
using SwiftKampus.Models;
using SwiftKampusModel.AddmissionApplicant;
using System.Linq;
using SwiftKampusModel;
using System;
using SwiftKampus.ViewModels.StudentBioData;

namespace SwiftKampus.Controllers
{
    public class DirectEntryExamsController : BaseController
    {
        public DirectEntryExamsController(SchoolDbContext db) : base(db)
        {

        }

        // GET: DirectEntryExams
        public async Task<ActionResult> Index()
        {
            return View(await _db.DirectEntryExams.ToListAsync());
        }

        // GET: DirectEntryExams/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DirectEntryExam directEntryExam = await _db.DirectEntryExams.FindAsync(id);
            if (directEntryExam == null)
            {
                return HttpNotFound();
            }
            return View(directEntryExam);
        }

        // GET: DirectEntryExams/Create
        public ActionResult PCreate()
        {
            var deExam = _db.DirectEntryExams.AsNoTracking()
                          .Where(x => x.UserId.Trim().ToUpper().Equals(userId.Trim().ToUpper()))
                          .FirstOrDefault();
            var qualificationType = from Qualifications s in Enum.GetValues(typeof(Qualifications))
                                    select new { ID = s, Name = s.ToString() };

            ViewBag.CertificateType = new SelectList(qualificationType, "Name", "Name");
            return View(deExam);
        }

        // POST: DirectEntryExams/PCreate
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> PCreate(DirectEntryExamVm model)
        {
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                var deExam = await _db.DirectEntryExams.AsNoTracking().Where(x => x.DirectEntryExamId.Equals(model.DirectEntryExamId) ||
                                        x.UserId.Trim().ToUpper().Equals(userId.Trim().ToUpper())).FirstOrDefaultAsync();

                var directEntryExam = new DirectEntryExam
                {
                    DirectEntryExamId = model.DirectEntryExamId,
                    CertificateType = model.CertificateType,
                    ExamDate = model.ExamDate,
                    ExamNumber = model.ExamNumber,
                    NameOfInstitution = model.NameOfInstitution,
                    OverallGrade = model.OverallGrade,
                    UserId = userId
                };

                if (deExam != null)
                {

                    if (deExam != null)
                    {
                        deExam.UserId = userId;
                        deExam.ExamDate = model.ExamDate;
                        deExam.NameOfInstitution = model.NameOfInstitution;
                        deExam.ExamNumber = model.ExamNumber;
                        deExam.CertificateType = model.CertificateType;
                        deExam.OverallGrade = model.OverallGrade;
                        _db.Entry(deExam).State = EntityState.Modified;
                        _db.SaveChanges();

                        var student = _db.Students.AsNoTracking().Where(x => x.Email.Equals(userId)).FirstOrDefault();
                        message = $"{model.CertificateType} result Updated Successfully...";
                        return new JsonResult { Data = new { status = true, message } };
                    }
                }
                else
                {
                    //model.UserId = userId;
                    _db.DirectEntryExams.Add(directEntryExam);
                    await _db.SaveChangesAsync();
                    message = $"{model.CertificateType} result Added Successfully.";
                    return new JsonResult { Data = new { status = true, message } };
                }
            }
            return new JsonResult { Data = new { status = false, message = "Data entered is not valid" } };
        }



        // GET: DirectEntryExams/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: DirectEntryExams/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "DirectEntryExamId,UserId,CertificateType,NameOfInstitution,ExamNumber,OverallGrade,ExamDate")] DirectEntryExam directEntryExam)
        {
            if (ModelState.IsValid)
            {
                _db.DirectEntryExams.Add(directEntryExam);
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(directEntryExam);
        }

        // GET: DirectEntryExams/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DirectEntryExam directEntryExam = await _db.DirectEntryExams.FindAsync(id);
            if (directEntryExam == null)
            {
                return HttpNotFound();
            }
            return View(directEntryExam);
        }

        // POST: DirectEntryExams/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "DirectEntryExamId,UserId,CertificateType,NameOfInstitution,ExamNumber,OverallGrade,ExamDate")] DirectEntryExam directEntryExam)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(directEntryExam).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(directEntryExam);
        }

        // GET: DirectEntryExams/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DirectEntryExam directEntryExam = await _db.DirectEntryExams.FindAsync(id);
            if (directEntryExam == null)
            {
                return HttpNotFound();
            }
            return View(directEntryExam);
        }

        // POST: DirectEntryExams/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            DirectEntryExam directEntryExam = await _db.DirectEntryExams.FindAsync(id);
            _db.DirectEntryExams.Remove(directEntryExam);
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
