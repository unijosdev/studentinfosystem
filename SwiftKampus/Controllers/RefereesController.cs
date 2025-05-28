using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampus.ViewModels.StudentBioData;
using SwiftKampusModel;
using System;
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
    public class RefereesController : BaseController
    {

        public RefereesController(SchoolDbContext db) : base(db)
        {

        }
        // GET: Referees
        public async Task<ActionResult> Index()
        {
            return View(await _db.Referees.AsNoTracking().Where(x => x.UserId.Equals(userId)).ToListAsync());
        }

        // GET: Referees/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Referee referee = await _db.Referees.FindAsync(id);
            if (referee == null)
            {
                return HttpNotFound();
            }
            return View(referee);
        }

        // GET: Referees/Create
        public ActionResult Create()
        {
            var mygender = from Gender s in Enum.GetValues(typeof(Gender))
                           select new { ID = s, Name = s.ToString() };

            ViewBag.Gender = new SelectList(mygender, "Name", "Name");
            return View();
        }

        // POST: Referees/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(List<Referee> referee)
        {
            if (ModelState.IsValid)
            {
                string message = string.Empty;
                var countFromDb = await _db.Referees.AsNoTracking()
                                   .CountAsync(x => x.Email.Equals(userId));

                var modelCount = referee.Count;
                int expectedCount = 3 - countFromDb;
                if (expectedCount < modelCount)
                {
                    message = $"You already have {countFromDb} results, You are only expected to add {expectedCount}";
                    return new JsonResult { Data = new { status = false, message } };
                }
                foreach (var item in referee)
                {
                    item.UserId = userId;
                    _db.Referees.Add(item);
                }
                await _db.SaveChangesAsync();
                message = $"{referee.Count} Referee/Sponsor(s) are added successfully";
                return new JsonResult { Data = new { status = true, message } };
            }

            return new JsonResult { Data = new { status = true, message = "Please complete the form completely" } };
        }


        // GET: Referees/Create
        public ActionResult PCreate()
        {
            var mygender = from Gender s in Enum.GetValues(typeof(Gender))
                           select new { ID = s, Name = s.ToString() };

            ViewBag.Gender = new SelectList(mygender, "Name", "Name");
            ViewBag.ApplicantType = _applicantType;
            var referees = _db.Referees.AsNoTracking().Where(x => x.UserId.Equals(userId)).ToList();
            var model = new RefereeVm()
            {
                Referee = new Referee(),
                Referees = referees
            };

            return View(model);
        }

        // POST: Referees/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> PCreate(List<Referee> referee)
        {
            if (ModelState.IsValid)
            {
                string message = string.Empty;
                foreach (var item in referee)
                {
                    item.UserId = userId;
                    _db.Referees.Add(item);
                }
                var oldReferees = _db.Referees.AsNoTracking()
                              .Where(x => x.UserId.Equals(userId)).ToList();
                if (oldReferees != null && oldReferees.Any())
                {
                    foreach (var item in oldReferees)
                    {
                        _db.Entry(item).State = EntityState.Deleted;
                    }
                }
                await _db.SaveChangesAsync();
                var firstRefereeRecord = referee.FirstOrDefault();
                if (firstRefereeRecord != null)
                {
                    var applicant = _db.Applicants.AsNoTracking().Where(x => x.ApplicantEmail.Equals(firstRefereeRecord.UserId)).FirstOrDefault();

                    //var refereeUrl = Url.Action("RefreeForm", "Referees", new { applicantId = applicant.ApplicantId }, protocol: Request.Url.Scheme);
                    var refereeUrl = string.Empty;

                    foreach (var refere in referee)
                    {
                        refereeUrl = Url.Action("RefreeForm", "Referees", new { applicantId = applicant.ApplicantId, refereeId = refere.RefereeId }, protocol: Request.Url.Scheme);
                        var body = $"The applicant, {applicant.FullName} with application number {applicant.ApplicantId} has choosen you to be his referee. Please click on the link below to proceed <br /> <a href='{refereeUrl}' class='btn' id='close'>click here</a>";

                        await NotifyApplicantByEmail(refere.Email.Trim(), refere.FullName, "Applicant Referee Form", body, applicant.ApplicantId);
                    }
                }

                
                message = $"{referee.Count} Referee/Sponsor(s) are added successfully";
                return new JsonResult { Data = new { status = true, message } };
            }

            return new JsonResult { Data = new { status = false, message = "Please complete the form completely" } };
        }


        // GET: Referees/Create
        public ActionResult UGCreate()
        {
            var mygender = from Gender s in Enum.GetValues(typeof(Gender))
                           select new { ID = s, Name = s.ToString() };

            ViewBag.Gender = new SelectList(mygender, "Name", "Name");
            ViewBag.ApplicantType = _applicantType;
            var referees = _db.Referees.AsNoTracking().Where(x => x.UserId.Equals(userId)).ToList();
            var model = new RefereeVm()
            {
                Referee = new Referee(),
                Referees = referees
            };
            var student = _studentQuery.GetStudent(userId);
            ViewBag.ModeOfEntry = student.ModeOfEntry.ToUpper();
            return View(model);
        }

        // POST: Referees/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> UGCreate(List<Referee> referee)
        {
            if (ModelState.IsValid)
            {
                string message = string.Empty;
                var countFromDb = await _db.Referees.AsNoTracking()
                                   .CountAsync(x => x.UserId.Trim().ToUpper().Equals(userId.ToUpper().ToUpper()));

                var modelCount = referee.Count;
                int expectedCount = 3 - countFromDb;
                if (expectedCount < modelCount)
                {
                    message = $"You already have {countFromDb} results, You are only expected to add {expectedCount}";
                    return new JsonResult { Data = new { status = false, message } };
                }
                foreach (var item in referee)
                {
                    item.UserId = userId;
                    _db.Referees.Add(item);
                }
                await _db.SaveChangesAsync();
                message = $"{referee.Count} Referee/Sponsor(s) are added successfully";
                return new JsonResult { Data = new { status = true, message } };
            }

            return new JsonResult { Data = new { status = false, message = "Please complete the form completely" } };
        }

        public async Task<ActionResult> RemoveMultipleSponsor(string email)
        {
            string message = "";
            var sponsors = await _db.Referees.Where(x => x.UserId.Trim().ToUpper().Equals(email.Trim().ToUpper())).ToListAsync();
            foreach (var item in sponsors)
            {
                if (sponsors.Count > 1)
                {
                    _db.Entry(item).State = EntityState.Deleted;
                }
            }
            _db.SaveChanges();
            return new JsonResult { Data = new { status = true, message } };
        }


            // GET: Referees/Edit/5
            public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Referee referee = await _db.Referees.FindAsync(id);
            if (referee == null)
            {
                return HttpNotFound();
            }
            var mygender = from Gender s in Enum.GetValues(typeof(Gender))
                           select new { ID = s, Name = s.ToString() };

            ViewBag.Gender = new SelectList(mygender, "Name", "Name");
            return View(referee);
        }

        // POST: Referees/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Referee referee)
        {
            if (ModelState.IsValid)
            {
                referee.UserId = userId;
                _db.Entry(referee).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            var mygender = from Gender s in Enum.GetValues(typeof(Gender))
                           select new { ID = s, Name = s.ToString() };

            ViewBag.Gender = new SelectList(mygender, "Name", "Name");
            return View(referee);
        }

        // GET: Referees/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Referee referee = await _db.Referees.FindAsync(id);
            if (referee == null)
            {
                return HttpNotFound();
            }
            return View(referee);
        }

        // POST: Referees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Referee referee = await _db.Referees.FindAsync(id);
            _db.Referees.Remove(referee);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> TranscriptRequestForm(int attendedSchoolId)
        {
            var attendedSchool = await _db.AttendedSchools.AsNoTracking()
                                      .Where(x => x.AttendedSchoolId.Equals(attendedSchoolId)).FirstOrDefaultAsync();

            var applicant = await _db.Applicants.Include(i => i.AvailableCourse.Programme.Department.Faculty).Include(i => i.SchoolProgramme)
                                    .AsNoTracking().Where(x => x.ApplicantEmail.Equals(attendedSchool.ApplicantId))
                                    .FirstOrDefaultAsync();

            var model = new TranscriptFormVm
            {
                FullName = applicant.FullName,
                ProgrameName = applicant.AvailableCourse?.Programme?.ProgrammeName,
                DeptName = applicant.AvailableCourse?.Programme?.Department.DeptName,
                FacultyName = applicant.AvailableCourse?.Programme?.Department?.Faculty.FacultyName,
                RegNo = applicant.ApplicantId,
                ClassOfDegree = attendedSchool.ClassOfDegree,
                Qualification = attendedSchool.Degree,
                Cgpa = attendedSchool.ResultGrade,
                YearGraduated = attendedSchool.ToDate.ToString("dd MMM yyyy"),
                ProgrammeCode = applicant.SchoolProgramme.Description
            };
            return View(model);


        }

        [AllowAnonymous]
        public async Task<ActionResult> RefreeForm(string applicantId, int refereeId)
        {
            if (!string.IsNullOrEmpty(applicantId))
            {             
                var applicant = await _db.Applicants.Include(i => i.AvailableCourse.Programme.Department.Faculty)
                                        .AsNoTracking().Where(x => x.ApplicantId.Equals(applicantId))
                                        .FirstOrDefaultAsync();
                var referee = await _db.Referees.AsNoTracking()
                                    .Where(x => x.UserId.Equals(applicant.ApplicantEmail) && x.RefereeId.Equals(refereeId)).FirstOrDefaultAsync();

                var model = new RefereeFormVm
                {
                    ApplicantName = applicant.FullName,
                    ApplicantPhoneNumber = applicant.PhoneNumber,
                    RefereeName = referee.UserName,
                    ApplicantId = applicant.ApplicantId,
                    RefereeId = referee.RefereeId
                };
                return View(model);
            }
            return View();
        }

        // POST: RefreeResponse/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RefreeResponseCreate(RefereeFormVm refereeResponse)
        {
            string message = "";
            if (ModelState.IsValid)
            {
                RefreeResponse existRefResponse = await _db.RefreeResponses.Include(r => r.Applicant).AsNoTracking()
                                             .Where(r => r.ApplicantId.Trim().ToUpper().Equals(refereeResponse.ApplicantId)
                                             && r.RefreeId.Equals(refereeResponse.RefereeId)).FirstOrDefaultAsync();

                if (existRefResponse != null)
                {
                    existRefResponse.RefreeId = refereeResponse.RefereeId;
                    existRefResponse.ApplicantId = refereeResponse.ApplicantId;
                    existRefResponse.RefereeName = refereeResponse.RefereeName;
                    existRefResponse.Profession = refereeResponse.Profession;
                    existRefResponse.PreviousWork = refereeResponse.PreviousWork;
                    existRefResponse.OralWritting = refereeResponse.OralWritting;
                    existRefResponse.KnownLong = refereeResponse.KnownLong;
                    existRefResponse.JobDescription = refereeResponse.JobDescription;
                    existRefResponse.IntellectualCapacity = refereeResponse.IntellectualCapacity;
                    existRefResponse.ImaginativeThought = refereeResponse.ImaginativeThought;
                    existRefResponse.Scholarship = refereeResponse.Scholarship;
                    existRefResponse.Address = refereeResponse.Address;
                    existRefResponse.AcademicStudy = refereeResponse.AcademicStudy;
                    existRefResponse.Capacity = refereeResponse.Capacity;
                    existRefResponse.Comment = refereeResponse.Comment;

                    _db.Entry(existRefResponse).State = EntityState.Modified;
                    message = $"Thank you for Updating applicant reference form";
                }
                else
                {
                    RefreeResponse newRefreeResponse = new RefreeResponse()
                    {
                        RefreeId = refereeResponse.RefereeId,
                        ApplicantId = refereeResponse.ApplicantId,
                        RefereeName = refereeResponse.RefereeName,
                        Profession = refereeResponse.Profession,
                        PreviousWork = refereeResponse.PreviousWork,
                        OralWritting = refereeResponse.OralWritting,
                        KnownLong = refereeResponse.KnownLong,
                        JobDescription = refereeResponse.JobDescription,
                        IntellectualCapacity = refereeResponse.IntellectualCapacity,
                        ImaginativeThought = refereeResponse.ImaginativeThought,
                        Scholarship = refereeResponse.Scholarship,
                        Address = refereeResponse.Address,
                        AcademicStudy = refereeResponse.AcademicStudy,
                        Capacity = refereeResponse.Capacity,
                        Comment = refereeResponse.Comment
                    };

                    _db.RefreeResponses.Add(newRefreeResponse);
                    message = $"Thank you for Updating applicant reference form";
                }

                await _db.SaveChangesAsync();
                return new JsonResult { Data = new { status = true, message } };

            }

            return new JsonResult { Data = new { status = true, message = "Please complete the form completely" } };
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
