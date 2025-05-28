using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using OfficeOpenXml;
using Rotativa;
using SwiftKampus.BusinessLogic;
using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampus.ViewModels;
using SwiftKampusModel;
using SwiftKampusModel.AddmissionApplicant;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SwiftKampus.Controllers
{
    [Authorize]
    [Audit(AuditingLevel = 2)]
    public class ApplicantsController : BaseController
    {
        private ApplicationUserManager _userManager;

        public ApplicantsController(ApplicationUserManager userManager, SchoolDbContext db) : base(db)
        {
            UserManager = userManager;
        }

        public ApplicationUserManager UserManager
        {
            get => _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            private set => _userManager = value;
        }

        // GET: Applicants
        public ActionResult Index()
        {
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FancyName");
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            ViewBag.ProgrammeId = new SelectList(_db.Programmes.AsNoTracking(), "ProgrammeId", "ProgrammeName");

            return View();
        }

        public ActionResult HodRecommedationIndex()
        {
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FancyName");
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            return View();
        }

        // Shows the list of all remedial students that have purchased the form
        public ActionResult HodRemedialIndex()
        {
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking().Where(x => x.SchoolProgrammeId == 9).ToList(), "SchoolProgrammeId", "FancyName");
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            return View();
        }

        public ActionResult FacultyRecommedationIndex()
        {
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FancyName");
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            return View();
        }
        public ActionResult PgRecommedationIndex()
        {
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FancyName");
            ViewBag.FacultyId = new SelectList(_db.Faculties.AsNoTracking(), "FacultyId", "FacultyName");
            ViewBag.DepartmentId = new SelectList(_db.Departments.AsNoTracking(), "DepartmentId", "DeptName");
            ViewBag.ProgrammeId = new SelectList(_db.Programmes.AsNoTracking(), "ProgrammeId", "ProgrammeName");
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            return View();
        }

        public async Task<ActionResult> GetIndex(int? SchoolProgrammeId, string hasRegistered,
            int? SessionId, int? ProgrammeId)
        {
            #region Server Side filtering

            //Get parameter for sorting from grid table
            // get Start (paging start index) and length (page size for paging)
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            //Get Sort columns values when we click on Header Name of column
            //getting column name
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            //Soring direction(either descending or ascending)
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
            string search = Request.Form.GetValues("search[value]").FirstOrDefault();

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int totalRecords = 0;

            var applicantIndexVm = new List<ApplicantListVm>();
            var applicantList = new List<Applicant>();
            var applicantPayments = new List<ApplicantPayment>();
            var payedApplicantList = new List<Applicant>();

            if (SchoolProgrammeId != null)
            {
                applicantList = await _db.Applicants.Include(i => i.SchoolProgramme).Include(i => i.AvailableCourse.Programme)
                            .Include(i => i.AvailableCourse.Programme.Department)
                            .Include(i => i.AvailableCourse.Programme.Department.Faculty)
                            .Include(i => i.Session).AsNoTracking()
                            .Include(i => i.AttendedSchools)
                            .Where(x => x.SchoolProgramme.SchoolProgrammeId.Equals((int)SchoolProgrammeId))
                            .ToListAsync();              

            }
            else
            {
                applicantList = await _db.Applicants.Include(i => i.SchoolProgramme)
                    .Include(i => i.AvailableCourse.Programme)
                    .Include(i => i.AvailableCourse.Programme.Department)
                    .Include(i => i.AvailableCourse.Programme.Department.Faculty)
                    .Include(i => i.Session).AsNoTracking().ToListAsync();
            }

            if (applicantList.Count() >= 1 && SessionId != null)
            {
                var firstApplicant = applicantList.FirstOrDefault();
                applicantPayments = _db.ApplicantPayments.Include(x => x.SchoolProgramme).Include(i => i.Session).AsNoTracking()
                                    .Where(x => x.SchoolProgrammeId.Equals(firstApplicant.SchoolProgramme.SchoolProgrammeId) 
                                    && x.Session.SessionId.Equals((int)SessionId))
                                    .ToList();

            }

            if (SessionId != null)
            {
                applicantList = applicantList.Where(x => x.Session.SessionId.Equals((int)SessionId)).ToList();
            }
            if(ProgrammeId != null)
            {
                applicantList = applicantList.Where(x => x.AvailableCourse != null && x.AvailableCourse.ProgrammeId.Equals((int)ProgrammeId)).ToList();
            }
            if (!string.IsNullOrEmpty(hasRegistered))
            {
                bool myRegistered;
                if (hasRegistered.ToLower().Equals("true"))
                {
                    myRegistered = true;
                    foreach (var applicant in applicantList)
                    {
                        if (SessionId != null)
                        {
                            if (applicantPayments.Any(x => x.ApplicantEmail.Equals(applicant.ApplicantEmail) && x.IsPayed.Equals(myRegistered)))
                            {
                                payedApplicantList.Add(applicant);
                            }
                        }
                    }
                }
                else
                {
                    myRegistered = false;
                    foreach (var applicant in applicantList)
                    {
                        if (SessionId != null)
                        {
                            if (applicantPayments.Any(x => x.ApplicantEmail.Equals(applicant.ApplicantEmail) && x.IsPayed.Equals(myRegistered)))
                            {
                                payedApplicantList.Add(applicant);
                            }
                            else
                            {
                                payedApplicantList.Add(applicant);
                            }
                        }
                    }
                }
                
            }           

            if (!string.IsNullOrEmpty(search))
            {
                payedApplicantList = payedApplicantList.Where(x => x.FullName.ToUpper().Contains(search.ToUpper().Trim())
                            || x.ApplicantId.Trim().ToUpper().Contains(search.ToUpper().Trim())).ToList();
                if (payedApplicantList.Count() == 0)
                {
                    payedApplicantList = payedApplicantList.Where(x => !string.IsNullOrEmpty(x.ApplicantEmail) &&
                            x.ApplicantEmail.ToUpper().Contains(search.ToUpper().Trim())).ToList();
                }
            }

            applicantIndexVm = MapToApplicantIndex(payedApplicantList);

            totalRecords = applicantIndexVm.Count();
            var data = applicantIndexVm.Skip(skip).Take(pageSize).ToList();

            return Json(new { draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data },
                JsonRequestBehavior.AllowGet);

            #endregion Server Side filtering
        }
        public async Task<ActionResult> GetHodIndex(int SchoolProgrammeId, int SessionId, bool? IsApproved, bool IsVcApproved, bool IsDownloaded)
         {
            #region Server Side filtering

            //Get parameter for sorting from grid table
            // get Start (paging start index) and length (page size for paging)
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            //Get Sort columns values when we click on Header Name of column
            //getting column name
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            //Soring direction(either descending or ascending)
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
            string search = Request.Form.GetValues("search[value]").FirstOrDefault();

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int totalRecords = 0;

            var applicantIndexVm = new List<ApplicantListVm>();
            var applicantList = new List<Applicant>();
            var payedApplicantList = new List<Applicant>();
            var applicantPayments = new List<ApplicantPayment>();

            var staffDept = await _db.Staffs.AsNoTracking().Where(x => x.Email.Equals(userId))
                                    .Select(s => s.Department).FirstOrDefaultAsync();

            if (staffDept.DeptCode.Trim().ToUpper().Equals("NS_REM"))
            {
                applicantList = await _db.Applicants.Include(i => i.SchoolProgramme).Include(i => i.Session)
                       .Include(i => i.AvailableCourse.Programme.Department).AsNoTracking()
                       .Include(i => i.AvailableCourse.Programme)
                       .Include(i => i.AvailableCourse.Programme.Department.Faculty)
                       .Where(x => (x.SchoolProgramme.ProgrammeCategory.Trim().ToUpper().Equals(ProgrammeCategory.Remedial_Science.ToString().Trim().ToUpper())
                       || x.SchoolProgramme.ProgrammeCategory.Trim().ToUpper().Equals(ProgrammeCategory.Preliminary_French.ToString().Trim().ToUpper()))
                       && x.Session.SessionId.Equals(SessionId)).ToListAsync();

                applicantPayments = _db.ApplicantPayments.Include(x => x.SchoolProgramme).AsNoTracking()
                                    .Where(x => (x.SchoolProgramme.ProgrammeCategory.Trim().ToUpper().Equals(ProgrammeCategory.Remedial_Science.ToString().Trim().ToUpper())
                                            || x.SchoolProgramme.ProgrammeCategory.Trim().ToUpper().Equals(ProgrammeCategory.Preliminary_French.ToString().Trim().ToUpper())))
                                    .ToList();
            }
            else
            {
                applicantList = await _db.Applicants.Include(i => i.SchoolProgramme).Include(i => i.Session)
                       .Include(i => i.AvailableCourse.Programme)
                       .Include(i => i.AvailableCourse.Programme.Department)
                       .Include(i => i.AvailableCourse.Programme.Department.Faculty)
                       .Where(x => x.AvailableCourse.Programme.Department.DepartmentId.Equals(staffDept.DepartmentId)
                       && x.Session.SessionId.Equals(SessionId) && x.SchoolProgramme.SchoolProgrammeId.Equals(SchoolProgrammeId)).ToListAsync();

                if (applicantList.Count() >= 1)
                {
                    var firstApplicant = applicantList.FirstOrDefault();
                    applicantPayments = _db.ApplicantPayments.Include(x => x.SchoolProgramme).AsNoTracking()
                                        .Where(x => x.SchoolProgrammeId.Equals(SchoolProgrammeId) 
                                        && x.Session.SessionId.Equals(SessionId))
                                        .ToList();
                }
            }           
          

            foreach (var applicant in applicantList)
            {
                if (applicantPayments.Any(x => x.ApplicantEmail.Equals(applicant.ApplicantEmail) && x.IsPayed.Equals(true)))
                {
                    if (!applicant.SchoolProgramme.ProgrammeCategory.Trim().ToUpper().Equals(ProgrammeCategory.Remedial_Science.ToString().Trim().ToUpper()) && applicant.HasPayed == false ) { //Added to flag applicants payment status
                        applicant.HasPayed = true;
                        _db.Entry(applicant).State = EntityState.Modified;
                        await _db.SaveChangesAsync();
                    }
                    payedApplicantList.Add(applicant);
                }
            }

            if (IsVcApproved)
            {
                payedApplicantList = payedApplicantList.Where(x => x.IsDeptApproved.Equals(IsApproved)
                           && x.IsVcApproved.Equals(IsVcApproved)).ToList();
            }
            else
            {
                payedApplicantList = payedApplicantList.Where(x => x.IsDeptApproved.Equals(IsApproved)
                            && (x.IsVcApproved.Equals(false) || x.IsVcApproved == null)).ToList();
            }

            if (IsDownloaded)
            {
                payedApplicantList = payedApplicantList.Where(x => x.IsDownloaded.Equals(IsDownloaded)).ToList();
            }
            else
            {
                payedApplicantList = payedApplicantList.Where(x => x.IsDownloaded.Equals(true) || x.IsDownloaded == null).ToList();
            }

            if (!string.IsNullOrEmpty(search))
            {
                payedApplicantList = payedApplicantList.Where(x => x.FullName.ToUpper().Contains(search.ToUpper().Trim())
                            || x.ApplicantId.Trim().ToUpper().Contains(search.ToUpper().Trim())).ToList();
                if (payedApplicantList.Count() == 0)
                {
                    payedApplicantList = payedApplicantList.Where(x => !string.IsNullOrEmpty(x.ApplicantEmail) &&
                            x.ApplicantEmail.ToUpper().Contains(search.ToUpper().Trim())).ToList();
                }
            }

            applicantIndexVm = MapToApplicantIndex(payedApplicantList);

            totalRecords = applicantIndexVm.Count();
            var data = applicantIndexVm.Skip(skip).Take(pageSize).ToList();

            return Json(new { draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data },
                JsonRequestBehavior.AllowGet);

            #endregion Server Side filtering
        }

        // Get the list of all remedial students that have purchased the form for a particula session
        public async Task<ActionResult> GetHodRemedialIndex(int SchoolProgrammeId, int SessionId)
        {
            #region Server Side filtering

            //Get parameter for sorting from grid table
            // get Start (paging start index) and length (page size for paging)
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            //Get Sort columns values when we click on Header Name of column
            //getting column name
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            //Soring direction(either descending or ascending)
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
            string search = Request.Form.GetValues("search[value]").FirstOrDefault();

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int totalRecords = 0;

            var applicantIndexVm = new List<ApplicantListVm>();
            var applicantList = new List<Applicant>();
            var payedApplicantList = new List<Applicant>();
            var applicantPayments = new List<ApplicantPayment>();

            var staffDept = await _db.Staffs.AsNoTracking().Where(x => x.Email.Equals(userId))
                                    .Select(s => s.Department).FirstOrDefaultAsync();

            if (staffDept.DeptCode.Trim().ToUpper().Equals("NS_REM"))
            {
                applicantList = await _db.Applicants.Include(i => i.SchoolProgramme).Include(i => i.Session)
                       .Include(i => i.AvailableCourse.Programme.Department).AsNoTracking()
                       .Include(i => i.AvailableCourse.Programme)
                       .Include(i => i.AvailableCourse.Programme.Department.Faculty)
                       .Where(x => (x.SchoolProgramme.ProgrammeCategory.Trim().ToUpper().Equals(ProgrammeCategory.Remedial_Science.ToString().Trim().ToUpper())
                       || x.SchoolProgramme.ProgrammeCategory.Trim().ToUpper().Equals(ProgrammeCategory.Preliminary_French.ToString().Trim().ToUpper()))
                       && x.Session.SessionId.Equals(SessionId)).ToListAsync();

                applicantPayments = _db.ApplicantPayments.Include(x => x.SchoolProgramme).AsNoTracking()
                                    .Where(x => (x.SchoolProgramme.ProgrammeCategory.Trim().ToUpper().Equals(ProgrammeCategory.Remedial_Science.ToString().Trim().ToUpper())
                                            || x.SchoolProgramme.ProgrammeCategory.Trim().ToUpper().Equals(ProgrammeCategory.Preliminary_French.ToString().Trim().ToUpper())))
                                    .ToList();
            }
            
            foreach (var applicant in applicantList)
            {
                if (applicantPayments.Any(x => x.ApplicantEmail.Equals(applicant.ApplicantEmail) && x.IsPayed.Equals(true)))
                {
                    if (!applicant.SchoolProgramme.ProgrammeCategory.Trim().ToUpper().Equals(ProgrammeCategory.Remedial_Science.ToString().Trim().ToUpper()) && applicant.HasPayed == false)
                    { //Added to flag applicants payment status
                        applicant.HasPayed = true;
                        _db.Entry(applicant).State = EntityState.Modified;
                        await _db.SaveChangesAsync();
                    }
                    payedApplicantList.Add(applicant);
                }
            }

            if (!string.IsNullOrEmpty(search))
            {
                payedApplicantList = payedApplicantList.Where(x => x.FullName.ToUpper().Contains(search.ToUpper().Trim())
                            || x.ApplicantId.Trim().ToUpper().Contains(search.ToUpper().Trim())).ToList();
                if (payedApplicantList.Count() == 0)
                {
                    payedApplicantList = payedApplicantList.Where(x => !string.IsNullOrEmpty(x.ApplicantEmail) &&
                            x.ApplicantEmail.ToUpper().Contains(search.ToUpper().Trim())).ToList();
                }
            }

            applicantIndexVm = MapToApplicantIndex(payedApplicantList);

            totalRecords = applicantIndexVm.Count();
            var data = applicantIndexVm.Skip(skip).Take(pageSize).ToList();

            return Json(new { draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data },
                JsonRequestBehavior.AllowGet);

            #endregion Server Side filtering
        }

        public async Task<string> setIsdownload(string remNo)
        {
            var applicant = await _db.Applicants.Include(i => i.SchoolProgramme).Include(i => i.Session)
                                    .Include(i => i.AvailableCourse.Programme.Department).AsNoTracking()
                                    .Where(x => (x.SchoolProgramme.ProgrammeCategory.Trim().ToUpper().Equals(ProgrammeCategory.Remedial_Science.ToString().Trim().ToUpper())) && x.ApplicantId.Equals(remNo)).FirstOrDefaultAsync();
            applicant.IsVcApproved = true;

            _db.Entry(applicant).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            return "Success!";
        }
        public async Task<ActionResult> GetFacultyIndex(int SchoolProgrammeId, int SessionId, bool? IsApproved, bool IsVcApproved, bool IsDownloaded)
        {
            #region Server Side filtering

            //Get parameter for sorting from grid table
            // get Start (paging start index) and length (page size for paging)
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            //Get Sort columns values when we click on Header Name of column
            //getting column name
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            //Soring direction(either descending or ascending)
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
            string search = Request.Form.GetValues("search[value]").FirstOrDefault();

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int totalRecords = 0;

            var applicantIndexVm = new List<ApplicantListVm>();
            var applicantList = new List<Applicant>();

            var staffFacultyId = await _db.Staffs.AsNoTracking().Include(i => i.Department).Where(x => x.Email.Equals(userId))
                                    .Select(s => s.Department.FacultyId).FirstOrDefaultAsync();

            applicantList = await _db.Applicants.Include(i => i.SchoolProgramme).Include(i => i.Session)
                        .Include(i => i.AvailableCourse.Programme.Department.Faculty).AsNoTracking()
                        .Include(i => i.AvailableCourse.Programme.Department)
                        .Include(i => i.AvailableCourse.Programme)
                        .Where(x => x.AvailableCourse.Programme.Department.FacultyId.Equals(staffFacultyId)
                        && x.Session.SessionId.Equals(SessionId) && x.HasPayed.Equals(true))
                                        .OrderBy(s => s.AvailableCourse.Programme.Department.Faculty.FacultyName)
                                        .ThenBy(s => s.AvailableCourse.Programme.Department.DeptName)
                                        .ThenBy(s => s.AvailableCourse.ProgrammeName).ToListAsync();

            applicantList = await _db.Applicants.Include(i => i.SchoolProgramme).Include(i => i.Session)
                       .Include(i => i.AvailableCourse.Programme.Department.Faculty).AsNoTracking()
                       .Include(i => i.AvailableCourse.Programme.Department)
                       .Include(i => i.AvailableCourse.Programme)
                       .Where(x => x.AvailableCourse.Programme.Department.Faculty.FacultyId.Equals(staffFacultyId)
                       && x.Session.SessionId.Equals(SessionId) && x.SchoolProgramme.SchoolProgrammeId.Equals(SchoolProgrammeId)
                       && x.HasPayed.Equals(true))
                       .OrderBy(s => s.AvailableCourse.Programme.Department.Faculty.FacultyName)
                                        .ThenBy(s => s.AvailableCourse.Programme.Department.DeptName)
                                        .ThenBy(s => s.AvailableCourse.ProgrammeName).ToListAsync();

            applicantList = applicantList.Where(x => x.IsFacultyApproved.Equals(IsApproved)).ToList();

            if (IsVcApproved)
            {
                applicantList = applicantList.Where(x => x.IsVcApproved.Equals(IsVcApproved)).ToList();
            }
            else
            {
                applicantList = applicantList.Where(x => x.IsVcApproved == false || x.IsVcApproved == null).ToList();
            }
            if (IsDownloaded)
            {
                applicantList = applicantList.Where(x => x.IsDownloaded.Equals(IsDownloaded)).ToList();
            }
            else
            {
                applicantList = applicantList.Where(x => x.IsDownloaded.Equals(false) || x.IsDownloaded == null).ToList();
            }

            applicantIndexVm = MapToApplicantIndex(applicantList);

            totalRecords = applicantList.Count();
            var data = applicantIndexVm.Skip(skip).Take(pageSize).ToList();

            return Json(new { draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data },
                JsonRequestBehavior.AllowGet);

            #endregion Server Side filtering
        }

        public async Task<ActionResult> GetPgIndex(int? SchoolProgrammeId, int? FacultyId, int? DepartmentId, int? ProgrammeId, int SessionId, bool? IsApproved, bool IsVcApproved, bool IsDownloaded)
        {
            #region Server Side filtering

            //Get parameter for sorting from grid table
            // get Start (paging start index) and length (page size for paging)
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            //Get Sort columns values when we click on Header Name of column
            //getting column name
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            //Soring direction(either descending or ascending)
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
            string search = Request.Form.GetValues("search[value]").FirstOrDefault();

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int totalRecords = 0;

            var applicantIndexVm = new List<ApplicantListVm>();
            var applicantList = new List<Applicant>();

            //var staffFacultyId = await _db.Staffs.AsNoTracking().Include(i => i.Department).Where(x => x.Email.Equals(userId))
            //                        .Select(s => s.Department.FacultyId).FirstOrDefaultAsync();

            applicantList = await _db.Applicants.Include(i => i.SchoolProgramme).Include(i => i.Session)
                        .Include(i => i.AvailableCourse.Programme.Department.Faculty).AsNoTracking()
                        .Include(i => i.AvailableCourse.Programme.Department)
                        .Include(i => i.AvailableCourse.Programme)
                        .Where(x => x.Session.SessionId.Equals(SessionId) && x.HasPayed.Equals(true) && x.AvailableCourseId != null 
                        )
                        .OrderBy(s => s.AvailableCourse.Programme.Department.Faculty.FacultyName)
                                        .ThenBy(s => s.AvailableCourse.Programme.Department.DeptName)
                                        .ThenBy(s => s.AvailableCourse.ProgrammeName).ToListAsync();

            //applicantList = applicantList.Where(x => x.SchoolProgramme.ProgrammeCategory.Equals(ProgrammeCategory.Phd.ToString())
            //                || x.SchoolProgramme.ProgrammeCategory.Equals(ProgrammeCategory.Masters.ToString())
            //                || x.SchoolProgramme.ProgrammeCategory.Equals(ProgrammeCategory.Post_Graduate.ToString())
            //                || x.SchoolProgramme.ProgrammeCategory.Equals(ProgrammeCategory.Remedial_Science.ToString())).ToList();

            applicantList = applicantList.Where(x => x.SchoolProgramme.SchoolProgrammeId.Equals((Int32)SchoolProgrammeId)).ToList();

            if (ProgrammeId != null)
            {
                applicantList = applicantList.Where(x => x.AvailableCourse.Programme.ProgrammeId.Equals((Int32)ProgrammeId)).ToList();
            }
            else if (DepartmentId != null)
            {
                applicantList = applicantList.Where(x => x.AvailableCourse.Programme.Department.DepartmentId.Equals((Int32)DepartmentId)).ToList();
            }
            else if (FacultyId != null)
            {
                applicantList = applicantList.Where(x => x.AvailableCourse.Programme.Department.FacultyId.Equals((Int32)FacultyId)).ToList();
            }

            applicantList = applicantList.Where(x => x.IsPGApproved.Equals(IsApproved) && (x.IsDeptApproved.Equals(true) || x.IsFacultyApproved.Equals(true))).ToList();

            if (IsVcApproved)
            {
                applicantList = applicantList.Where(x => x.IsVcApproved.Equals(IsVcApproved)).ToList();
            }
            else
            {
                applicantList = applicantList.Where(x => x.IsVcApproved == false || x.IsVcApproved == null).ToList();
            }

            if (IsDownloaded)
            {
                applicantList = applicantList.Where(x => x.IsDownloaded.Equals(IsDownloaded)).ToList();
            }
            else
            {
                applicantList = applicantList.Where(x => x.IsDownloaded.Equals(false) || x.IsDownloaded == null).ToList();
            }
            applicantList = applicantList.OrderBy(x => x.SchoolProgramme.ProgrammeCategory)
                            .ThenBy(x => x.AvailableCourse.Programme.Department.Faculty.FacultyName).ToList();

            if (!string.IsNullOrEmpty(search))
            {
                applicantList = applicantList.Where(x => x.FullName.ToUpper().Contains(search.ToUpper().Trim())
                            || x.ApplicantId.Trim().ToUpper().Contains(search.ToUpper().Trim())).ToList();
                if (applicantList.Count() == 0)
                {
                    applicantList = applicantList.Where(x => !string.IsNullOrEmpty(x.ApplicantEmail) &&
                            x.ApplicantEmail.ToUpper().Contains(search.ToUpper().Trim())).ToList();
                }
            }
            applicantIndexVm = MapToApplicantIndex(applicantList);

            totalRecords = applicantList.Count();
            var data = applicantIndexVm.Skip(skip).Take(pageSize).ToList();

            return Json(new { draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data },
                JsonRequestBehavior.AllowGet);

            #endregion Server Side filtering
        }

        [AllowAnonymous]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateApplicantProgrammeForForms(int? SchoolProgrammeId, string ApplicantEmail)
        {
        
            if (string.IsNullOrEmpty(ApplicantEmail))
            {
                ViewBag.Message = "Applicant Email is Required To Procceed";
                ViewData.Add("ActionMessage", "Error, Applicant Email is Required To Procceed");
                ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FancyName");
                return View();

            }

            //var getApplicant = await _db.Applicants.AsNoTracking().Where(x => x.ApplicantId.Trim().ToUpper().Equals(ApplicantId.Trim().ToUpper())
            //|| x.ApplicantId.Trim().ToUpper().Equals(ApplicantId.Trim().ToUpper()))
            //.FirstOrDefaultAsync();


            var getApplicant = await _db.Applicants.AsNoTracking().Where(x => x.ApplicantEmail.Trim().ToUpper().Equals(ApplicantEmail.Trim().ToUpper())).FirstOrDefaultAsync();

            if(getApplicant != null)
            {
                getApplicant.SchoolProgrammeId = SchoolProgrammeId;
                getApplicant.ApplicantId = "";
                getApplicant.AvailableCourseId = null;

                _db.Entry(getApplicant).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                ViewBag.Message = "Programme Changed Successfully";
                ViewData.Add("ActionMessage", $"Programme Changed Successfully for {getApplicant.ApplicantEmail}");
                ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FancyName");
                return View();

            }

            ViewBag.Message = "The Registered Email was Not Found";
            ViewData.Add("ActionMessage", "Error: The Registered Email was Not Found");
            return View();
        }


        private List<ApplicantListVm> MapToApplicantIndex(List<Applicant> v)
            {
            int count = 0;

            List<ApplicantListVm> applicantList = new List<ApplicantListVm>();

            foreach (var s in v)
            {
                var AttendedSchool = _db.AttendedSchools.AsNoTracking()
                                        .Where(x => x.ApplicantId.Equals(s.ApplicantEmail)).FirstOrDefault();
                applicantList.Add(
                    new ApplicantListVm()
                    {
                        Sn = ++count,
                        //Status = GetApplicantStatus(level.Equals("Dept") ? s.IsDeptApproved : level.Equals("Faculty") ? s.IsFacultyApproved : level.Equals("Pg") ? s.IsPGApproved : null),
                        Status = GetApplicantStatus(s.IsDeptApproved),
                        FacultyStatus = GetApplicantStatus(s.IsFacultyApproved),
                        PgStatus = GetApplicantStatus(s.IsPGApproved),
                        ApplicantId = s.ApplicantId,
                        Email = s.ApplicantEmail,
                        PhoneNumber = s.PhoneNumber,
                        Gender = s.Gender,
                        ProgrammeName = string.IsNullOrEmpty(s.AvailableCourse?.Programme?.ProgrammeName) ? " " : s.AvailableCourse.Programme.ProgrammeName,
                        DepartmentName = string.IsNullOrEmpty(s.AvailableCourse?.Programme?.Department.DeptName) ? " " : s.AvailableCourse.Programme.Department.DeptName,
                        FacultyName = string.IsNullOrEmpty(s.AvailableCourse?.Programme?.Department.Faculty.FacultyName) ? " " : s.AvailableCourse.Programme.Department.Faculty.FacultyName,
                        StateOfOrigin = s.StateOfOrigin,
                        Lga = s.TownOfBirth,
                        SchoolProgrammeName = s.SchoolProgramme.FancyName,
                        FullName = $"{s.LastName} {s.FirstName}",
                        VcStatus = s.IsVcApproved.Equals(true) ? "Approved" : "Not Approved",
                        InstittionAttended = AttendedSchool != null ? AttendedSchool.SchoolName ?? "": " ",
                        Degree = AttendedSchool != null ? AttendedSchool.Degree ?? "": " ",
                        ClassOfDegree = AttendedSchool != null ? AttendedSchool.ClassOfDegree ?? "": " ",
                        CGPA = AttendedSchool != null ? AttendedSchool.ResultGrade ?? "": " ",
                        CourseOfStudy = AttendedSchool != null ? AttendedSchool.CourseOfStudy ?? "": "",
                    }
                   );
            }
            //var applicants = v.Select(s => new ApplicantListVm()
            //{
            //    Sn = ++count,
            //    //Status = GetApplicantStatus(level.Equals("Dept") ? s.IsDeptApproved : level.Equals("Faculty") ? s.IsFacultyApproved : level.Equals("Pg") ? s.IsPGApproved : null),
            //    Status = GetApplicantStatus(s.IsDeptApproved),
            //    FacultyStatus = GetApplicantStatus(s.IsFacultyApproved),
            //    PgStatus = GetApplicantStatus(s.IsPGApproved),
            //    ApplicantId = s.ApplicantId,
            //    Email = s.ApplicantEmail,
            //    PhoneNumber = s.PhoneNumber,
            //    Gender = s.Gender,
            //    ProgrammeName = string.IsNullOrEmpty(s.AvailableCourse?.Programme?.ProgrammeName) ? " " : s.AvailableCourse.Programme.ProgrammeName,
            //    StateOfOrigin = s.StateOfOrigin,
            //    Lga = s.TownOfBirth,
            //    SchoolProgrammeName = s.SchoolProgramme.FancyName,
            //    FullName = $"{s.LastName} {s.FirstName}",
            //    VcStatus = s.IsVcApproved.Equals(true) ? "Approved" : "Not Approved",
            //    AttendedSchool = _db.AttendedSchools.AsNoTracking()
            //                            .Where(x => x.ApplicantId.Equals(v.)).ToListAsync()
            //}).ToList();

            return applicantList;
        }

        private string GetApplicantStatus(bool? isDeptApproved)
        {
            if (isDeptApproved == null)
            {
                return "Pending";
            }
            if (isDeptApproved.Equals(true))
            {
                return "Recommended";
            }
            else
            {
                return "Not Recommended";
            }
        }

        public ActionResult ProcessForm()
        {
            var result = ConfirmApplicationFee();
            if (result != null)
                return (PartialViewResult)result;

            var model = _applicantType;
            return View(model);
        }

        public ActionResult InstructionPage()
        {
            return View();
        }
        public ActionResult UnderGraduateIndex()
        {
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FullName");
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> UnderGraduateIndex(string type)
        {
            var applicants = _db.Applicants.AsNoTracking().Include(a => a.SchoolProgramme).Include(a => a.ApplicantOLevelResults)
                                    .Where(x => x.SchoolProgramme.ProgrammeCategory.Equals(type));
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FullName");
            return View(await applicants.ToListAsync());
        }

        // GET: Applicants/Details/5
        public async Task<ActionResult> Details(string id)
        {
            if (id == null)
            {
                id = userId;
            }
            Applicant applicant = await _db.Applicants.Include(i => i.SchoolProgramme).Include(i => i.AvailableCourse.Programme)
                                        .AsNoTracking().Where(s => s.ApplicantEmail.Equals(id))
                                        .FirstOrDefaultAsync();


            if (applicant == null)
            {
                return HttpNotFound();
            }
            var model = new ApplicantDetailVm()
            {
                Applicant = applicant,
                ApplicantOLevelResults = await _db.ApplicantOLevelResults.Include(i => i.Subject).AsNoTracking()
                                        .Where(x => x.ApplicantId.Equals(id)).ToListAsync(),
                EmploymentDetails = await _db.EmploymentDetails.AsNoTracking()
                                        .Where(x => x.ApplicantId.Equals(id)).ToListAsync(),
                NextOfKins = await _db.NextOfKins.AsNoTracking()
                                        .Where(x => x.UserId.Equals(id)).ToListAsync(),
                AttendedSchools = await _db.AttendedSchools.AsNoTracking()
                                        .Where(x => x.ApplicantId.Equals(id)).ToListAsync(),
                AwardPrices = await _db.AwardPrices.AsNoTracking()
                                        .Where(x => x.ApplicantId.Equals(id)).ToListAsync(),
                Publications = await _db.Publications.AsNoTracking()
                                        .Where(x => x.UserId.Equals(id)).ToListAsync(),
                Qualifications = await _db.Qualifications.AsNoTracking()
                                        .Where(x => x.UserId.Equals(id)).ToListAsync(),
                Referees = await _db.Referees.AsNoTracking()
                                        .Where(x => x.UserId.Equals(id)).ToListAsync(),
                ThesisProposals = await _db.ThesisProposals.AsNoTracking()
                                        .Where(x => x.UserId.Equals(id)).ToListAsync(),


                SubjetCombination   = GetEnumDescription(((SubjectCombination)applicant.NoOfApplication)),
            };
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> SaveHodApproval(string applicantId, string hodApproval, string ReasonForReject)
        {
            string message = string.Empty;
            bool isdeptApproved;
            if (!string.IsNullOrEmpty(applicantId))
            {
                if (!string.IsNullOrEmpty(hodApproval))
                {
                    isdeptApproved = false || hodApproval.ToUpper().Equals("APPROVE");
                }
                else
                {
                    isdeptApproved = false;
                    hodApproval = "Rejected";
                }
                Applicant applicant = await _db.Applicants.AsNoTracking().Where(s => s.ApplicantEmail.Equals(applicantId))
                                      .FirstOrDefaultAsync();
                if (applicant != null)
                {
                    applicant.IsDeptApproved = isdeptApproved;
                    applicant.ReasonDeptForRejection = ReasonForReject;
                    _db.Entry(applicant).State = EntityState.Modified;
                    await _db.SaveChangesAsync();
                    message = $"Applicant has been {hodApproval} successfully";
                    return new JsonResult { Data = new { status = true, message } };
                }
                message = "Applicant not found";
                return new JsonResult { Data = new { status = false, message } };
            }
            message = "Please ensure that HOD approval is selected";
            return new JsonResult
            {
                Data = new
                {
                    status = false,
                    message
                }
            };

        }

        [HttpPost]
        public async Task<ActionResult> SaveFacultyApproval(string applicantId, string deanApproval, string ReasonForReject)
        {
            string message = string.Empty;
            bool isdeptApproved;
            if (!string.IsNullOrEmpty(applicantId))
            {
                if (!string.IsNullOrEmpty(deanApproval))
                {
                    isdeptApproved = false || deanApproval.ToUpper().Equals("APPROVE");
                }
                else
                {
                    isdeptApproved = false;
                    deanApproval = "Rejected";
                }
                Applicant applicant = await _db.Applicants.AsNoTracking().Where(s => s.ApplicantEmail.Equals(applicantId))
                                      .FirstOrDefaultAsync();
                if (applicant != null)
                {
                    applicant.IsFacultyApproved = isdeptApproved;
                    applicant.ReasonDeptForRejection = ReasonForReject;
                    _db.Entry(applicant).State = EntityState.Modified;
                    await _db.SaveChangesAsync();
                    message = $"Applicant has been {deanApproval} successfully";
                    return new JsonResult { Data = new { status = true, message } };
                }
                message = "Applicant not found";
                return new JsonResult { Data = new { status = false, message } };
            }
            message = "Please ensure that Dean approval is selected";
            return new JsonResult
            {
                Data = new
                {
                    status = false,
                    message
                }
            };
        }

        [HttpPost]
        public async Task<ActionResult> SavePGApproval(string applicantId, string pgApproval, string ReasonForReject)
        {
            string message = string.Empty;
            bool isdeptApproved;
            if (!string.IsNullOrEmpty(applicantId))
            {
                if (!string.IsNullOrEmpty(pgApproval))
                {
                    isdeptApproved = false || pgApproval.ToUpper().Equals("APPROVE");
                }
                else
                {
                    isdeptApproved = false;
                    pgApproval = "Rejected";
                }
                Applicant applicant = await _db.Applicants.AsNoTracking().Where(s => s.ApplicantEmail.Equals(applicantId))
                                      .FirstOrDefaultAsync();
                if (applicant != null)
                {
                    applicant.IsPGApproved = isdeptApproved;
                    applicant.IsDeptApproved = isdeptApproved;
                    applicant.IsFacultyApproved = isdeptApproved;
                    applicant.ReasonDeptForRejection = ReasonForReject;
                    _db.Entry(applicant).State = EntityState.Modified;
                    await _db.SaveChangesAsync();
                    message = $"Applicant has been {pgApproval} successfully";
                    return new JsonResult { Data = new { status = true, message } };
                }
                message = "Applicant not found";
                return new JsonResult { Data = new { status = false, message } };
            }
            message = "Please ensure that PG approval is selected";
            return new JsonResult
            {
                Data = new
                {
                    status = false,
                    message
                }
            };
        }

        public PartialViewResult RejectAdmission(string applicantId, string level)
        {
            if (!string.IsNullOrEmpty(applicantId) && !string.IsNullOrEmpty(level))
            {
                var model = new RejectAdmissionVm()
                {
                    applicantId = applicantId,
                    LevelOfReject = level
                };
                if (level.ToUpper().Equals("FACULTY"))
                {
                    return PartialView("FacultyReject", model);
                }
                if (level.ToUpper().Equals("PG"))
                {
                    return PartialView("PgReject", model);
                }
                return PartialView(model);
            }
            ViewBag.Message = "Empty Student Id";
            return PartialView();
        }

        public PartialViewResult FacultyReject(RejectAdmissionVm model)
        {
            return PartialView(model);
        }
        public PartialViewResult PgReject(RejectAdmissionVm model)
        {
            return PartialView(model);
        }

        // GET: Applicants/PrintDetails/5
        public async Task<ActionResult> PrintDetails(string id)
        {
            if (id == null)
            {
                id = userId;
            }
            Applicant applicant = await _db.Applicants.Include(i => i.SchoolProgramme).Include(i => i.AvailableCourse.Programme)
                                        .AsNoTracking().Where(s => s.ApplicantEmail.Equals(id))
                                        .FirstOrDefaultAsync();
            if (applicant == null)
            {
                return HttpNotFound();
            }
            var model = new ApplicantDetailVm()
            {
                Applicant = applicant,
                ApplicantOLevelResults = await _db.ApplicantOLevelResults.Include(i => i.Subject).AsNoTracking()
                                        .Where(x => x.ApplicantId.Equals(id)).ToListAsync(),
                EmploymentDetails = await _db.EmploymentDetails.AsNoTracking()
                                        .Where(x => x.ApplicantId.Equals(id)).ToListAsync(),
                NextOfKins = await _db.NextOfKins.AsNoTracking()
                                        .Where(x => x.UserId.Equals(id)).ToListAsync(),
                AttendedSchools = await _db.AttendedSchools.AsNoTracking()
                                        .Where(x => x.ApplicantId.Equals(id)).ToListAsync(),
                AwardPrices = await _db.AwardPrices.AsNoTracking()
                                        .Where(x => x.ApplicantId.Equals(id)).ToListAsync(),
                Publications = await _db.Publications.AsNoTracking()
                                        .Where(x => x.UserId.Equals(id)).ToListAsync(),
                Qualifications = await _db.Qualifications.AsNoTracking()
                                        .Where(x => x.UserId.Equals(id)).ToListAsync(),
                Referees = await _db.Referees.AsNoTracking()
                                        .Where(x => x.UserId.Equals(id)).ToListAsync(),
                ThesisProposals = await _db.ThesisProposals.AsNoTracking()
                                        .Where(x => x.UserId.Equals(id)).ToListAsync(),
                ApplicantPayment = await _db.ApplicantPayments.AsNoTracking()
                                        .Where(x => x.ApplicantEmail.Trim().ToUpper().Equals(id.Trim().ToUpper())).FirstOrDefaultAsync(),
                RelevantDocuments = await _db.RelevantDocuments.AsNoTracking()
                                        .Where(x => x.UserId.Trim().ToUpper()
                                        .Equals(id.Trim().ToUpper())).ToListAsync(),
                SubjetCombination = GetEnumDescription(((SubjectCombination)applicant.NoOfApplication)),


            };
            // return View(model);
            return new ViewAsPdf(model);
        }

        //GET: Remedials Admission Letter
        public async Task<ActionResult> ShowAdmissionLetter(string id)
        {
            if (id == null)
            {
                id = userId;
            }
            Applicant applicant = await _db.Applicants.Include(i => i.SchoolProgramme).Include(i => i.AvailableCourse.Programme)
                                                       .Include(i => i.Session)
                                        .AsNoTracking().Where(s => s.ApplicantEmail.Equals(id))
                                        .FirstOrDefaultAsync();


            if (applicant == null)
            {
                return HttpNotFound();
            }
            // return View(model);
            return View(applicant);
        }

        //GET: PG Admission Letter
        public async Task<ActionResult> ShowPGAdmissionLetter(string id)
        {
            if (id == null)
            {
                id = userId;
            }
            Applicant applicant = await _db.Applicants.Include(i => i.SchoolProgramme).Include(i => i.AvailableCourse.Programme)
                                                       .Include(i => i.Session)
                                                       //.Include(i => i.Address)
                                        .AsNoTracking().Where(s => s.ApplicantEmail.Equals(id))
                                        .FirstOrDefaultAsync();


            if (applicant == null)
            {
                return HttpNotFound();
            }
            // return View(model);
            return View(applicant);
        }

        public async Task<ActionResult> ApproveDetails(string id)
        {
            if (id == null)
            {
                id = userId;
            }
            Applicant applicant = await _db.Applicants.Include(i => i.SchoolProgramme)
                                        .Include(i => i.AvailableCourse.Programme).AsNoTracking()
                                        .Where(s => s.ApplicantEmail.Trim().ToUpper().Equals(id.Trim().ToUpper()))
                                        .FirstOrDefaultAsync();
            if (applicant == null)
            {
                return HttpNotFound();
            }
            var model = new ApplicantDetailVm()
            {
                Applicant = applicant,
                ApplicantOLevelResults = await _db.ApplicantOLevelResults.Include(i => i.Subject).AsNoTracking()
                                        .Where(x => x.ApplicantId.Equals(id)).ToListAsync(),
                EmploymentDetails = await _db.EmploymentDetails.AsNoTracking()
                                        .Where(x => x.ApplicantId.Equals(id)).ToListAsync(),
                NextOfKins = await _db.NextOfKins.AsNoTracking()
                                        .Where(x => x.UserId.Equals(id)).ToListAsync(),
                AttendedSchools = await _db.AttendedSchools.AsNoTracking()
                                        .Where(x => x.ApplicantId.Equals(id)).ToListAsync(),
                AwardPrices = await _db.AwardPrices.AsNoTracking()
                                        .Where(x => x.ApplicantId.Equals(id)).ToListAsync(),
                Publications = await _db.Publications.AsNoTracking()
                                        .Where(x => x.UserId.Equals(id)).ToListAsync(),
                Qualifications = await _db.Qualifications.AsNoTracking()
                                        .Where(x => x.UserId.Equals(id)).ToListAsync(),
                Referees = await _db.Referees.AsNoTracking()
                                        .Where(x => x.UserId.Equals(id)).ToListAsync(),
                ThesisProposals = await _db.ThesisProposals.AsNoTracking()
                                        .Where(x => x.UserId.Equals(id)).ToListAsync(),
                ApplicantPayment = await _db.ApplicantPayments.AsNoTracking()
                                        .Where(x => x.ApplicantEmail.Trim().ToUpper()
                                        .Equals(id.Trim().ToUpper())).FirstOrDefaultAsync(),
                RelevantDocuments = await _db.RelevantDocuments.AsNoTracking()
                                        .Where(x => x.UserId.Trim().ToUpper()
                                        .Equals(id.Trim().ToUpper())).ToListAsync(),

            };
            return View(model);
            //return new ViewAsPdf(model);
        }
        public PartialViewResult ApplicantOLevelResults()
        {
            var username = User.Identity.GetUserId();
            var id = _db.Users.AsNoTracking().Where(c => c.Id.Equals(username)).Select(c => c.Email)
                                .FirstOrDefault();
            var applicantOLevelResults = _db.ApplicantOLevelResults.AsNoTracking().Include(a => a.Subject)
                                    .Where(x => x.ApplicantId.Equals(id)).ToList();
            return PartialView(applicantOLevelResults);
        }

        // GET: Applicants/Create
        public ActionResult Create()
        {
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FullName");
            var mystates = from State s in Enum.GetValues(typeof(State))
                           select new { ID = s, Name = s.ToString() };
            var religion = from Religion s in Enum.GetValues(typeof(Religion))
                           select new { ID = s, Name = s.ToString() };
            var mygender = from Gender s in Enum.GetValues(typeof(Gender))
                           select new { ID = s, Name = s.ToString() };
            var lga = from LgaEnum s in Enum.GetValues(typeof(LgaEnum))
                      select new { ID = s, Name = s.ToString() };


            ViewBag.Lga = new SelectList(lga, "Name", "Name");
            ViewBag.Religion = new SelectList(religion, "Name", "Name");
            ViewBag.StateOfOrigin = new SelectList(mystates, "Name", "Name");
            ViewBag.Gender = new SelectList(mygender, "Name", "Name");
            return View();
        }

        // POST: Applicants/Create
        // To protect from over-posting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Applicant applicant)
        {
            if (ModelState.IsValid)
            {
                _db.Applicants.Add(applicant);
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FullName", applicant.SchoolProgrammeId);
            var mystates = from State s in Enum.GetValues(typeof(State))
                           select new { ID = s, Name = s.ToString() };
            var religion = from Religion s in Enum.GetValues(typeof(Religion))
                           select new { ID = s, Name = s.ToString() };
            var mygender = from Gender s in Enum.GetValues(typeof(Gender))
                           select new { ID = s, Name = s.ToString() };
            var lga = from LgaEnum s in Enum.GetValues(typeof(LgaEnum))
                      select new { ID = s, Name = s.ToString() };


            ViewBag.Lga = new SelectList(lga, "Name", "Name");
            ViewBag.Religion = new SelectList(religion, "Name", "Name");
            ViewBag.StateOfOrigin = new SelectList(mystates, "Name", "Name");
            ViewBag.Gender = new SelectList(mygender, "Name", "Name");
            return View(applicant);
        }

        // GET: Applicants/Edit/5
        public ActionResult Edit(string id)
        {
            //var result = ConfirmApplicationFee();
            //if (result != null)
            //{
            //    return result;
            //}
            if (id == null)
            {
                id = userId;
            }
            Applicant applicant = _db.Applicants.Include(i => i.AvailableCourse.Programme).AsNoTracking()
                                        .Where(x => x.ApplicantEmail.Equals(id)).FirstOrDefault();
            if (applicant == null)
            {
                return HttpNotFound();
            }
            var availableCourse = _db.AvailableCourses.AsNoTracking().Include(a => a.Programme).AsNoTracking()
                                    .Where(x => x.SchoolProgrammeId.Equals((int)applicant.SchoolProgrammeId) && x.IsActive.Equals(true))
                                    .Select(s => new { s.AvailableCourseId, s.Programme.ProgrammeName }).ToList();

            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FullName", applicant.SchoolProgrammeId);
            ViewBag.AvailableCourseId = new SelectList(availableCourse, "AvailableCourseId", "ProgrammeName", applicant?.AvailableCourseId);
            var mystates = from State s in Enum.GetValues(typeof(State))
                           select new { ID = s, Name = s.ToString() };
            var religion = from Religion s in Enum.GetValues(typeof(Religion))
                           select new { ID = s, Name = s.ToString() };
            var mygender = from Gender s in Enum.GetValues(typeof(Gender))
                           select new { ID = s, Name = s.ToString() };
            var lga = from LgaEnum s in Enum.GetValues(typeof(LgaEnum))
                      select new { ID = s, Name = s.ToString() };


            ViewBag.TownOfBirth = new SelectList(lga, "Name", "Name", applicant.TownOfBirth);
            ViewBag.Religion = new SelectList(religion, "Name", "Name");
            ViewBag.StateOfOrigin = new SelectList(mystates, "Name", "Name");
            ViewBag.Gender = new SelectList(mygender, "Name", "Name");
            return View(applicant);
        }

        // POST: Applicants/Edit/5
        // To protect from over posting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Edit(Applicant model)
        {
            if (ModelState.IsValid && model.AvailableCourseId != null)
            {
                //var applicant = await _db.Applicants.FindAsync(model.ApplicantId);

                //if (model.ApplicatPassport == null)
                //{
                //    if (applicant.ApplicatPassport == null)
                //    {
                //        return new JsonResult { Data = new { status = false, message = "Please upload both Passport." } };
                //    }
                //}

                //if (model.ApplicatPassport != null)
                //{
                //    applicant.ApplicatPassport = model.ApplicatPassport;
                //}
                _db.Entry(model).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return new JsonResult { Data = new { status = true, message = "Information updated successfully" } };
            }
            return new JsonResult { Data = new { status = false, message = "Please complete all fields and select the programme of study to proceed." } };
        }

        public JsonResult GetAvailableCourses(int deptId, int schoolProgrammeId)
        {
            var availableCourses = _db.AvailableCourses.AsNoTracking().Include(a => a.Programme)
                                    .Where(x => x.SchoolProgrammeId == schoolProgrammeId && x.Programme.DepartmentId == deptId && x.IsActive.Equals(true))
                                    .Select(s => new { s.AvailableCourseId, s.Programme.ProgrammeName }).ToList();

            //return new JsonResult { Data = new { availableCourses = availableCourses, JsonRequestBehavior.AllowGet } };
            return Json(availableCourses, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Edit_2(string id)
        {
            var result = ConfirmApplicationFee();
            if (result != null)
            {
                return result;
            }
            if (id == null)
            {
                id = userId;
            }

            Applicant applicant = _db.Applicants.Include(i => i.AvailableCourse.Programme).AsNoTracking()
                                        .Where(x => x.ApplicantEmail.Equals(id)).FirstOrDefault();
            if (applicant == null)
            {
                return HttpNotFound();
            }
            var availableCourse = _db.AvailableCourses.AsNoTracking().Include(a => a.Programme).AsNoTracking()
                                    .Where(x => x.SchoolProgrammeId.Equals((int)applicant.SchoolProgrammeId) && x.IsActive.Equals(true))
                                    .Select(s => new { s.AvailableCourseId, s.Programme.ProgrammeName }).ToList();

            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FullName", applicant.SchoolProgrammeId);
            ViewBag.AvailableCourseId = new SelectList(availableCourse, "AvailableCourseId", "ProgrammeName", applicant?.AvailableCourseId);
            
            // Handle first instance of filling form
            if (applicant.AvailableCourseId == null)
            {
                ViewBag.DepartmentId = new SelectList(_db.Departments.AsNoTracking(), "DepartmentId", "DeptName");
            }
            else ViewBag.DepartmentId = new SelectList(_db.Departments.AsNoTracking(), "DepartmentId", "DeptName", applicant?.AvailableCourse.Programme.DepartmentId);

            var mystates = from State s in Enum.GetValues(typeof(State))
                           select new { ID = s, Name = s.ToString() };
            var religion = from Religion s in Enum.GetValues(typeof(Religion))
                           select new { ID = s, Name = s.ToString() };
            var mygender = from Gender s in Enum.GetValues(typeof(Gender))
                           select new { ID = s, Name = s.ToString() };
            var lga = from LgaEnum s in Enum.GetValues(typeof(LgaEnum))
                      select new { ID = s, Name = s.ToString() };
            var country = from IndegineStatus s in Enum.GetValues(typeof(IndegineStatus))
                          select new { ID = s, Name = s.ToString() };

            ViewBag.TownOfBirth = new SelectList(lga, "Name", "Name", applicant.TownOfBirth);
            ViewBag.Religion = new SelectList(religion, "Name", "Name");
            ViewBag.StateOfOrigin = new SelectList(mystates, "Name", "Name");
            ViewBag.Gender = new SelectList(mygender, "Name", "Name");
            ViewBag.Country = new SelectList(country, "Name", "Name");

            ApplicantVm applicantVm = new ApplicantVm
            {
                Address = applicant.Address,
                ApplicantEmail = applicant.ApplicantEmail,
                ApplicantId = applicant.ApplicantId,
                //ApplicantOLevelResults = applicant.ApplicantOLevelResults,
                //ApplicantWaiverPayments = applicant.ApplicantWaiverPayments,
                ApplicatPassport = applicant.ApplicatPassport,
                //AttendedSchools = applicant.AttendedSchools,
                AvailableCourse = applicant.AvailableCourse,
                AvailableCourseId = applicant.AvailableCourseId,
                Country = applicant.Country,
                DateOfBirth = applicant.DateOfBirth,
                FirstName = applicant.FirstName,
                Gender = applicant.Gender,
                HealthCondition = applicant.HealthCondition,
                HealthStatus = applicant.HealthStatus,
                LastName = applicant.LastName,
                MaritalStatus = applicant.MaritalStatus,
                MiddleName = applicant.MiddleName,
                PhoneNumber = applicant.PhoneNumber,
                SchoolProgramme = applicant.SchoolProgramme,
                SchoolProgrammeId = applicant.SchoolProgrammeId,
                Session = applicant.Session,
                SessionId = applicant.SessionId,
                StateOfOrigin = applicant.StateOfOrigin,
                TownOfBirth = applicant.TownOfBirth
            };

            return View(applicantVm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Edit_2(ApplicantVm model)
        {
            if (ModelState.IsValid && model.AvailableCourseId != null)
            {
                //var applicant = await _db.Applicants.FindAsync(model.ApplicantId);

                //if (model.ApplicatPassport == null)
                //{
                //    if (applicant.ApplicatPassport == null)
                //    {
                //        return new JsonResult { Data = new { status = false, message = "Please upload both Passport." } };
                //    }
                //}

                //if (model.ApplicatPassport != null)
                //{
                //    applicant.ApplicatPassport = model.ApplicatPassport;
                //}

                if (model.StateOfOrigin == "select_state")
                {
                    return new JsonResult { Data = new { status = false, message = "Please enter your state of origin" } };
                }

                Applicant applicant = new Applicant
                {
                    Address = model.Address,
                    ApplicantEmail = model.ApplicantEmail,
                    ApplicantId = model.ApplicantId,
                    //ApplicantOLevelResults = model.ApplicantOLevelResults,
                    //ApplicantWaiverPayments = model.ApplicantWaiverPayments,
                    ApplicatPassport = model.ApplicatPassport,
                    //AttendedSchools = model.AttendedSchools,
                    AvailableCourse = model.AvailableCourse,
                    AvailableCourseId = model.AvailableCourseId,
                    Country = model.Country,
                    DateOfBirth = model.DateOfBirth,
                    FirstName = model.FirstName,
                    Gender = model.Gender,
                    HealthCondition = model.HealthCondition,
                    HealthStatus = model.HealthStatus,
                    LastName = model.LastName,
                    MaritalStatus = model.MaritalStatus,
                    MiddleName = model.MiddleName,
                    PhoneNumber = model.PhoneNumber,
                    SchoolProgramme = model.SchoolProgramme,
                    SchoolProgrammeId = model.SchoolProgrammeId,
                    Session = model.Session,
                    SessionId = model.SessionId,
                    StateOfOrigin = model.StateOfOrigin,
                    TownOfBirth = model.TownOfBirth
                };

                _db.Entry(applicant).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return new JsonResult { Data = new { status = true, message = "Information updated successfully" } };
            }
            
            return new JsonResult { Data = new { status = false, message = "Please complete all fields and select the programme of study to proceed." } };
        }

        public async Task<ActionResult> ApplicantSubjectCombo()
        {
            //var sujectCombo = from SubjectCombination s in Enum.GetValues(typeof(SubjectCombination))
            //               select new { ID = s, Name = s.ToString() };
            //ViewBag.sujectCombo = new SelectList(sujectCombo, "Name", "Name");
            var sujectCombo = from SubjectCombination s in Enum.GetValues(typeof(SubjectCombination))
                              select new
                              {
                                  ID = (int)s,
                                  Name = GetEnumDescription(s)
                              };

            ViewBag.sujectCombo = new SelectList(sujectCombo, "ID", "Name");

            return View();
        }

        [HttpPost]
        public async Task<ActionResult> SaveSubjectCombination(int subjectCombo)
        {
            if (subjectCombo != null)
            {
                var username = User.Identity.GetUserId();

                Applicant applicant = await _db.Applicants.Include(i => i.AvailableCourse.Programme).AsNoTracking()
                                        .Where(x => x.ApplicantId.Equals(username)).FirstOrDefaultAsync();
                //SubjectCombination subjectCombo = (SubjectCombination)subjectCombo;

                if (applicant == null)
                {
                    return HttpNotFound();
                }
                applicant.NoOfApplication = subjectCombo;
                _db.Entry(applicant).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return new JsonResult { Data = new { status = true, message = "Information updated successfully" } };

            }
            return new JsonResult { Data = new { status = false, message = "Please complete all fields and select the programme of study to proceed." } };


        }



        // GET: Applicants/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Applicant applicant = await _db.Applicants.FindAsync(id);
            if (applicant == null)
            {
                return HttpNotFound();
            }
            return View(applicant);
        }

        // POST: Applicants/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            Applicant applicant = await _db.Applicants.FindAsync(id);
            if (applicant != null) _db.Applicants.Remove(applicant);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }



        [AllowAnonymous]
        public ActionResult RenderImage(string studentId)
        {
            Applicant applicant = _db.Applicants.Where(x => x.ApplicantEmail.Trim().ToUpper().Equals(studentId.Trim().ToUpper())
                                    || x.ApplicantId.Trim().ToUpper().Equals(studentId.Trim().ToUpper())).FirstOrDefault();

            byte[] photoBack = applicant.ApplicatPassport;
             
            return File(photoBack, "image/png");
        }

        public async Task<ActionResult> ShowRefereeResponse(int Id)
        {
            //var refList = await _db.RefreeResponses.ToListAsync();
            var RefResponse = await _db.RefreeResponses.AsNoTracking()
                                        .Where(x => x.RefreeId.Equals(Id)).FirstOrDefaultAsync();
            return View(RefResponse);
        }
        public async Task DownloadRecommededReport(int SessionId)
        {
            var applicantIndexVm = new List<ApplicantListVm>();
            var applicantList = new List<Applicant>();

            var staffDept = await _db.Staffs.Include(i => i.Department).AsNoTracking().Where(x => x.Email.Equals(userId))
                                    .Select(s => s.Department).FirstOrDefaultAsync();

            if (User.IsInRole(RoleName.HodAdmissionOfficer))
            {
                if (staffDept.DeptCode.Trim().ToUpper().Equals("NS_REM")  )
                {
                    applicantList = await _db.Applicants.Include(i => i.SchoolProgramme).Include(i => i.Session)
                            .Include(i => i.AvailableCourse.Programme.Department)
                            .Include(i => i.AvailableCourse.Programme)
                            .Include(i => i.AvailableCourse.Programme.Department.Faculty)
                           .Where(x => (x.SchoolProgramme.ProgrammeCategory.Trim().ToUpper().Equals(ProgrammeCategory.Remedial_Science.ToString().Trim().ToUpper())
                           || x.SchoolProgramme.ProgrammeCategory.Trim().ToUpper().Equals(ProgrammeCategory.Preliminary_French.ToString().Trim().ToUpper()))
                           && x.Session.SessionId.Equals(SessionId) && x.IsVcApproved != true)
                           .ToListAsync();
                }
                else
                {
                    applicantList = await _db.Applicants.Include(i => i.SchoolProgramme).Include(i => i.Session)
                       .Include(i => i.AvailableCourse.Programme.Department)
                       .Include(i => i.AvailableCourse.Programme)
                       .Include(i => i.AvailableCourse.Programme.Department.Faculty)
                       .Where(x => x.AvailableCourse.Programme.Department.DepartmentId.Equals(staffDept.DepartmentId)
                       && x.Session.SessionId.Equals(SessionId)
                      && x.IsVcApproved != true).ToListAsync();
                }

                applicantList = applicantList.Where(x => x.IsDeptApproved.Equals(true) && x.IsDownloaded != true).ToList();
                applicantIndexVm = MapToApplicantIndex(applicantList);
            }

            if (User.IsInRole(RoleName.PGAdmissionOfficer))
            {
                applicantList = await _db.Applicants.Include(i => i.SchoolProgramme).Include(i => i.Session)
                        .Include(i => i.AvailableCourse.Programme.Department.Faculty)
                       .Include(i => i.AvailableCourse.Programme)
                       .Include(i => i.AvailableCourse.Programme.Department.Faculty)
                        .Where(x => x.Session.SessionId.Equals(SessionId)).ToListAsync();

                applicantList = applicantList.Where(x => x.SchoolProgramme.ProgrammeCategory.Equals(ProgrammeCategory.Phd.ToString())
                                || x.SchoolProgramme.ProgrammeCategory.Equals(ProgrammeCategory.Masters.ToString())
                                || x.SchoolProgramme.ProgrammeCategory.Equals(ProgrammeCategory.Post_Graduate.ToString())
                                || x.SchoolProgramme.ProgrammeCategory.Equals(ProgrammeCategory.MBA.ToString())).ToList();

                applicantList = applicantList.Where(x => x.IsPGApproved.Equals(true) && x.IsDownloaded != true).ToList();
                applicantIndexVm = MapToApplicantIndex(applicantList);
            }

            char c1 = 'A';
            ExcelPackage package = new ExcelPackage();
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Report");

            worksheet.Cells[$"{c1++}1"].Value = "S/N";
            worksheet.Cells[$"{c1++}1"].Value = "Form No";
            worksheet.Cells[$"{c1++}1"].Value = "Programme Name";
            worksheet.Cells[$"{c1++}1"].Value = "FullName";
            worksheet.Cells[$"{c1++}1"].Value = "State";
            worksheet.Cells[$"{c1++}1"].Value = "LGA";
            worksheet.Cells[$"{c1++}1"].Value = "Gender";
            worksheet.Cells[$"{c1++}1"].Value = "Status";

            int rowStart = 2;
            //char c2 = 'A';

            for (var i = 0; i < applicantIndexVm.Count; i++)
            {
                worksheet.Cells[$"A{rowStart}"].Value = applicantIndexVm[i].Sn;
                worksheet.Cells[$"B{rowStart}"].Value = applicantIndexVm[i].ApplicantId;
                worksheet.Cells[$"C{rowStart}"].Value = applicantIndexVm[i].SchoolProgrammeName;
                worksheet.Cells[$"D{rowStart}"].Value = applicantIndexVm[i].FullName;
                worksheet.Cells[$"E{rowStart}"].Value = applicantIndexVm[i].StateOfOrigin;
                worksheet.Cells[$"F{rowStart}"].Value = applicantIndexVm[i].Lga;
                worksheet.Cells[$"G{rowStart}"].Value = applicantIndexVm[i].Gender;
                worksheet.Cells[$"H{rowStart}"].Value = applicantIndexVm[i].Status;
                rowStart++;
            }

            foreach (var applicant in applicantList)
            {
                applicant.IsDownloaded = true;
                _db.Entry(applicant).State = EntityState.Modified;
            }
            await _db.SaveChangesAsync();

            worksheet.Cells["A:AZ"].AutoFitColumns();
            worksheet.Column(1).Style.Locked = true;
            worksheet.Column(2).Style.Locked = true;
            worksheet.Column(3).Style.Locked = true;
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment: filename=" +
                                                      $"Dept Recommended List.xlsx");
            Response.BinaryWrite(package.GetAsByteArray());
            Response.End();
        }

        [HttpPost]
        public async Task<ActionResult> ApproveDeptAdmission(int SessionId)
        {
            var applicantIndexVm = new List<ApplicantListVm>();
            var applicantList = new List<Applicant>();

            var staffDept = await _db.Staffs.Include(i => i.Department).AsNoTracking().Where(x => x.Email.Equals(userId))
                                    .Select(s => s.Department).FirstOrDefaultAsync();

            if (staffDept.DeptCode.Trim().ToUpper().Equals("NS_REM"))
            {
                applicantList = await _db.Applicants.Include(i => i.SchoolProgramme).Include(i => i.Session)
                       .Include(i => i.AvailableCourse.Programme.Department)
                       .Where(x => (x.SchoolProgramme.ProgrammeCategory.Trim().ToUpper().Equals(ProgrammeCategory.Remedial_Science.ToString().Trim().ToUpper())
                       || x.SchoolProgramme.ProgrammeCategory.Trim().ToUpper().Equals(ProgrammeCategory.Preliminary_French.ToString().Trim().ToUpper()))
                       && x.Session.SessionId.Equals(SessionId)).ToListAsync();
            }
            else
            {
                applicantList = await _db.Applicants.Include(i => i.SchoolProgramme).Include(i => i.Session)
                       .Include(i => i.AvailableCourse.Programme.Department)
                       .Where(x => x.AvailableCourse.Programme.Department.DepartmentId.Equals(staffDept.DepartmentId)
                       && x.Session.SessionId.Equals(SessionId))
                       .ToListAsync();
            }


            applicantList = applicantList.Where(x => x.IsDeptApproved.Equals(true)
                            && x.IsVcApproved != true && x.IsDownloaded.Equals(true)).ToList();

            if (applicantList.Count() >= 1 && User.IsInRole(RoleName.HodAdmissionOfficer))
            {
                var applicantSchoolProgramme = applicantList.FirstOrDefault();
                if (applicantSchoolProgramme.SchoolProgramme.ProgrammeCategory.Equals(ProgrammeCategory.IJMB.ToString())
                    || applicantSchoolProgramme.SchoolProgramme.ProgrammeCategory.Equals(ProgrammeCategory.Remedial_Science.ToString())
                    || applicantSchoolProgramme.SchoolProgramme.ProgrammeCategory.Equals(ProgrammeCategory.Preliminary_French.ToString())
                    || applicantSchoolProgramme.SchoolProgramme.ProgrammeCategory.Equals(ProgrammeCategory.Institute_Of_Education.ToString())
                    || applicantSchoolProgramme.SchoolProgramme.ProgrammeCategory.Equals(ProgrammeCategory.Diploma.ToString())
                    || applicantSchoolProgramme.SchoolProgramme.ProgrammeCategory.Equals(ProgrammeCategory.ICT.ToString()))
                {
                    var successfulMigration = await MigrateApplicantToStudent(applicantList); /////////////////////Migrates REMS Applicants to Students /////////////////////////////
                    var message = $"{successfulMigration} Applicant's Admission Approved Successfully";
                    return new JsonResult { Data = new { status = true, message } };
                }

            }
            return new JsonResult { Data = new { status = false, message = "No record Found" } };
        }

        public async Task<ActionResult> NotifyAdmissionStatus(int SessionId)
        {
            var applicantIndexVm = new List<ApplicantListVm>();
            var applicantList = new List<Applicant>();
            //var applicantIndexVm = new List<ApplicantListVm>();
            //var applicantList = new List<Applicant>();
            var payedApplicantList = new List<Applicant>();
            var applicantPayments = new List<ApplicantPayment>();
            bool IsVcApproved = true;
            bool IsDownloaded = true;
            bool IsApproved = true;


            var staffDept = await _db.Staffs.Include(i => i.Department).AsNoTracking().Where(x => x.Email.Equals(userId))
                                    .Select(s => s.Department).FirstOrDefaultAsync();

            if (staffDept.DeptCode.Trim().ToUpper().Equals("NS_REM"))
            {
                //applicantList = await _db.Applicants.Include(i => i.SchoolProgramme).Include(i => i.Session)
                //       .Include(i => i.AvailableCourse.Programme.Department)
                //       .Where(x => (x.SchoolProgramme.ProgrammeCategory.Trim().ToUpper().Equals(ProgrammeCategory.Remedial_Science.ToString().Trim().ToUpper())
                //       || x.SchoolProgramme.ProgrammeCategory.Trim().ToUpper().Equals(ProgrammeCategory.Preliminary_French.ToString().Trim().ToUpper()))
                //       && x.Session.SessionId.Equals(SessionId)).ToListAsync();
                applicantList = await _db.Applicants.Include(i => i.SchoolProgramme).Include(i => i.Session)
                       .Include(i => i.AvailableCourse.Programme.Department)
                       .Where(x => (x.SchoolProgramme.ProgrammeCategory.Trim().ToUpper().Equals(ProgrammeCategory.Remedial_Science.ToString().Trim().ToUpper())
                       || x.SchoolProgramme.ProgrammeCategory.Trim().ToUpper().Equals(ProgrammeCategory.Preliminary_French.ToString().Trim().ToUpper()))
                       && x.Session.SessionId.Equals(SessionId)).ToListAsync();

                applicantPayments = _db.ApplicantPayments.Include(x => x.SchoolProgramme)
                                    .Where(x => (x.SchoolProgramme.ProgrammeCategory.Trim().ToUpper().Equals(ProgrammeCategory.Remedial_Science.ToString().Trim().ToUpper())
                                            || x.SchoolProgramme.ProgrammeCategory.Trim().ToUpper().Equals(ProgrammeCategory.Preliminary_French.ToString().Trim().ToUpper())))
                                    .ToList();
            }
            else
            {
                applicantList = await _db.Applicants.Include(i => i.SchoolProgramme).Include(i => i.Session)
                       .Include(i => i.AvailableCourse.Programme.Department)
                       .Where(x => x.AvailableCourse.Programme.Department.DepartmentId.Equals(staffDept.DepartmentId)
                       && x.Session.SessionId.Equals(SessionId))
                       .ToListAsync();
            }
            foreach (var applicant in applicantList)
            {
                if (applicantPayments.Any(x => x.ApplicantEmail.Equals(applicant.ApplicantEmail) && x.IsPayed.Equals(true)))
                {
                    payedApplicantList.Add(applicant);
                }
            }

            if (IsVcApproved)
            {
                payedApplicantList = payedApplicantList.Where(x => x.IsDeptApproved.Equals(IsApproved)
                           && x.IsVcApproved.Equals(IsVcApproved)).ToList();
            }
            else
            {
                payedApplicantList = payedApplicantList.Where(x => x.IsDeptApproved.Equals(IsApproved)
                            && (x.IsVcApproved.Equals(false) || x.IsVcApproved == null)).ToList();
            }

            if (IsDownloaded)
            {
                payedApplicantList = payedApplicantList.Where(x => x.IsDownloaded.Equals(IsDownloaded) && x.IsNotify.Equals(null)).ToList();
            }
            else
            {
                payedApplicantList = payedApplicantList.Where(x => x.IsDownloaded.Equals(false) || x.IsDownloaded == null).ToList();
            }

            //applicantList = applicantList.Where(x => x.IsStudent.Equals(true) && x.IsNotify != true).ToList();

            if (applicantList.Count() >= 1 && User.IsInRole(RoleName.HodAdmissionOfficer))
            {
                var applicantSchoolProgramme = applicantList.FirstOrDefault();
                if (applicantSchoolProgramme.SchoolProgramme.ProgrammeCategory.Equals(ProgrammeCategory.IJMB.ToString())
                    || applicantSchoolProgramme.SchoolProgramme.ProgrammeCategory.Equals(ProgrammeCategory.Remedial_Science.ToString())
                    || applicantSchoolProgramme.SchoolProgramme.ProgrammeCategory.Equals(ProgrammeCategory.Preliminary_French.ToString())
                    || applicantSchoolProgramme.SchoolProgramme.ProgrammeCategory.Equals(ProgrammeCategory.Institute_Of_Education.ToString())
                    || applicantSchoolProgramme.SchoolProgramme.ProgrammeCategory.Equals(ProgrammeCategory.Diploma.ToString())
                    || applicantSchoolProgramme.SchoolProgramme.ProgrammeCategory.Equals(ProgrammeCategory.ICT.ToString()))
                {
                    var successfulMigration = await NotifyApplicantAdmission(payedApplicantList);
                    var message = $"{successfulMigration} Notification Admission Message Sent Successfully";
                    return new JsonResult { Data = new { status = true, message } };
                }

            }
            return new JsonResult { Data = new { status = false, message = "No record Found" } };
        }


        private async Task<int> NotifyApplicantAdmission(List<Applicant> applicantList)
        {
            int count = 0;
            var (modeOfEntry, levelId) = GetSchoolProgrammeEntryLevelAndMode(applicantList.Select(s => s.SchoolProgramme)
                                       .FirstOrDefault());
            string programmeName = string.Empty;
            var programme = new Programme();

            if (modeOfEntry.Equals("RS"))
            {
                programmeName = _db.Programmes.AsNoTracking().Where(x => x.ProgrammeCode.Trim().Equals("NS_REM_REM"))
                                    .Select(s => s.ProgrammeName).FirstOrDefault();
            }

            foreach (var applicant in applicantList)
            {
                var user = await _db.Users.AsNoTracking()
                                    .Where(x => x.Email.Trim().ToUpper().Equals(applicant.ApplicantEmail.Trim().ToUpper()))
                                    .FirstOrDefaultAsync();
                if (user != null)
                {
                    //string body = "You've been offered admission" +
                    //   $" to {applicant.SchoolProgramme.FullName} by Unijos. Visit www.unijos.edu.ng for detail";

                    //string body = $"Unijos has offered you provisional admission into the Pre-degree Science Programme in the 2019/2020 " +
                    //    $"Academic Session. Visit portal.unijos.edu.ng Click CHECK ADMISSION and " +
                    //    $"following the prompt to accept and to register immediately. Lectures commences on 15/10/19. Congratulations";

                    string body = $" Dear {applicant.FirstName}, Unijos has offered you provisional admission into the Department of Remedial Science in the {applicant.Session.SessionName} Session." +
                        $" Visit https://portal.unijos.edu.ng and log into your dashboard to print admission letter and pay acceptance fee." +
                        $" Congratulation";


                    var roles = await UserManager.GetRolesAsync(user.Id);

                    await UserManager.RemoveFromRolesAsync(user.Id, roles.ToArray());
                    await UserManager.AddToRoleAsync(user.Id, RoleName.Student);

                    //PUT YOUR SMS HIA!!!!!
                    var customSms = new CustomSms();
                    if (!string.IsNullOrEmpty(applicant.PhoneNumber))
                    {
                        //await customSms.SendUnknowMsgAsync(new SmsToStudent() { Body = body, Destination = applicant.PhoneNumber });
                        await SMSClass.SendSMS("UNIJOS SIS", body, applicant.PhoneNumber); //EBULK SMS API
                    }

                    if (!modeOfEntry.Equals("RS"))
                    {
                        programmeName = applicant.AvailableCourse.ProgrammeName;
                    }

                    //await NotifyByEmail(applicant.ApplicantEmail, applicant.LastName, applicant.FirstName, programmeName, applicant.ApplicantEmail,
                    //    applicant.ApplicantId, applicant.Session.SessionName, applicant.SchoolProgramme.FullName);

                    count += 1;
                    applicant.IsNotify = true;
                    _db.Entry(applicant).State = EntityState.Modified;

                    await _db.SaveChangesAsync();
                }
            }
            return count;
        }

        [HttpPost]
        public async Task<ActionResult> ApprovePgAdmission(int SessionId)
        {
            var applicantIndexVm = new List<ApplicantListVm>();
            var applicantList = new List<Applicant>();

            var staffDept = await _db.Staffs.AsNoTracking().Where(x => x.Email.Equals(userId))
                                    .Select(s => s.DepartmentId).FirstOrDefaultAsync();

            applicantList = await _db.Applicants.Include(i => i.SchoolProgramme).Include(i => i.Session)
                        .Include(i => i.AvailableCourse.Programme.Department.Faculty)
                        .Where(x => x.Session.SessionId.Equals(SessionId) && x.HasPayed.Equals(true) && x.IsFacultyApproved == true && x.IsVcApproved == null).ToListAsync();

            applicantList = applicantList.Where(x => x.SchoolProgramme.ProgrammeCategory.Equals(ProgrammeCategory.Phd.ToString())
                            || x.SchoolProgramme.ProgrammeCategory.Equals(ProgrammeCategory.Masters.ToString())
                            || x.SchoolProgramme.ProgrammeCategory.Equals(ProgrammeCategory.Post_Graduate.ToString())
                            || x.SchoolProgramme.ProgrammeCategory.Equals(ProgrammeCategory.MBA.ToString())
                            || x.SchoolProgramme.ProgrammeCategory.Equals(ProgrammeCategory.Remedial_Science.ToString())).ToList();

            applicantList = applicantList.Where(x => x.IsPGApproved.Equals(true) && x.IsDownloaded.Equals(true)).ToList();

            if (applicantList.Count() >= 1 && User.IsInRole(RoleName.PGAdmissionOfficer))
            {
                var successfulMigration = await MigrateApplicantToStudent(applicantList);
                var message = $"{successfulMigration} Applicant's Admission Approved Successfully";
                return new JsonResult { Data = new { status = true, message } };
            }
            return new JsonResult { Data = new { status = false, message = "No record Found" } };
        }

        private async Task<int> MigrateApplicantToStudent(List<Applicant> applicantList)
        {
            int count = 0;
            bool isRemedial = false;

            var (modeOfEntry, levelId) = GetSchoolProgrammeEntryLevelAndMode(applicantList.Select(s => s.SchoolProgramme)
                                        .FirstOrDefault());
            var programme = new Programme();
            if (modeOfEntry.Equals("RS"))
            {
                isRemedial = true;
                programme = _db.Programmes.AsNoTracking().Where(x => x.ProgrammeCode.Trim().Equals("NS_REM_REM")).FirstOrDefault();
            }
            foreach (var applicant in applicantList)
            {
                //var (modeOfEntry, levelId) = GetSchoolProgrammeEntryLevelAndMode(applicant.SchoolProgramme);

                var studentExist = _db.Students.AsNoTracking()
                                    .Any(x => x.JambRegNo.ToUpper().Trim().Equals(applicant.ApplicantId.Trim().ToUpper()));
                //var AppliedProgramme = _db.Programmes.Where(x => x.ProgrammeCode.Equals(applicant.AvailableCourse.Programme.ProgrammeCode)).FirstOrDefault();
                if (!studentExist)
                {
                    var studentId = DateTime.Now.Ticks;
                    var student = new Student
                    {
                        StudentId = $"{SchoolSetUp.CurrentSchoolName}{studentId}",
                        Email = applicant.ApplicantEmail,
                        Active = true,
                        StudentStatus = StudentStatus.New_Student.ToString(),
                        PhoneNumber = applicant.PhoneNumber,
                        JambRegNo = applicant.ApplicantId,
                        FirstName = applicant.FirstName,
                        MiddleName = applicant.MiddleName,
                        LastName = applicant.LastName,
                        StateOfOrigin = applicant.StateOfOrigin,
                        DateOfBirth = applicant.DateOfBirth,
                        Gender = applicant.Gender,
                        EnrollmentDate = DateTime.Now,
                        ProgrammeId = modeOfEntry.Equals("RS") ? programme?.ProgrammeId : applicant.AvailableCourse.ProgrammeId,
                        LevelId = levelId,
                        SchoolProgrammeId = (int)applicant.SchoolProgrammeId,
                        SessionId = applicant.SessionId, //applicant.SessionId   use this for next pg admission processing
                        Passport = applicant.ApplicatPassport,
                        Nationality = applicant.Country,
                        ModeOfEntry = modeOfEntry
                    };
                    _db.Students.Add(student);

                    // This check only applies to remedial
                    if (isRemedial)
                    {
                        applicant.IsVcApproved = true;
                    }

                    applicant.DateVcApproved = DateTime.Now;
                    applicant.IsStudent = true;
                    _db.Entry(applicant).State = EntityState.Modified;

                    count += 1;
                }
            }
            await _db.SaveChangesAsync();
            return count;
        }

        public (string modeOfEntry, int levelId) GetSchoolProgrammeEntryLevelAndMode(SchoolProgramme schoolProgramme)
        {
            var levels = _db.Levels.AsNoTracking().ToList();
            if (schoolProgramme.ProgrammeCategory.Equals(ProgrammeCategory.Remedial_Science.ToString()))
            {
                return (ModeOfEntry.RS.ToString(), GetLevelIdByName("000", levels));
            }
            else if (schoolProgramme.ProgrammeCategory.Equals(ProgrammeCategory.Preliminary_French.ToString()))
            {
                return (ModeOfEntry.PF.ToString(), GetLevelIdByName("000", levels));
            }
            else if (schoolProgramme.ProgrammeCategory.Equals(ProgrammeCategory.IJMB.ToString()))
            {
                return (ModeOfEntry.IJMB.ToString(), GetLevelIdByName("000", levels));
            }
            else if (schoolProgramme.ProgrammeCategory.Equals(ProgrammeCategory.Diploma.ToString()))
            {
                return (ModeOfEntry.DIP.ToString(), GetLevelIdByName("000", levels));
            }
            else if (schoolProgramme.ProgrammeCategory.Equals(ProgrammeCategory.ICT.ToString()))
            {
                return (ModeOfEntry.ICT.ToString(), GetLevelIdByName("000", levels));
            }
            else if (schoolProgramme.ProgrammeCategory.Equals(ProgrammeCategory.Institute_Of_Education.ToString())
                && schoolProgramme.ProgrammeType.Equals(ProgrammeType.Full_Time.ToString()))
            {
                return (ModeOfEntry.IOE.ToString(), GetLevelIdByName("000", levels));
            }
            else if (schoolProgramme.ProgrammeCategory.Equals(ProgrammeCategory.UnderGraduate.ToString())
                && schoolProgramme.ProgrammeType.Equals(ProgrammeType.Part_Time.ToString()))
            {
                return (ModeOfEntry.PT.ToString(), GetLevelIdByName("100", levels));
            }
            else if (schoolProgramme.ProgrammeCategory.Equals(ProgrammeCategory.Masters.ToString()) || schoolProgramme.ProgrammeCategory.Equals(ProgrammeCategory.MBA.ToString())
                //||schoolProgramme.ProgrammeCategory.Equals(ProgrammeCategory.Post_Graduate.ToString()) ||
                //schoolProgramme.ProgrammeCategory.Equals(ProgrammeCategory.Phd.ToString())
                )
            {
                return (ModeOfEntry.PG.ToString(), GetLevelIdByName("800", levels));
            }
            else if (/*schoolProgramme.ProgrammeCategory.Equals(ProgrammeCategory.Masters.ToString()) ||*/
                schoolProgramme.ProgrammeCategory.Equals(ProgrammeCategory.Post_Graduate.ToString())
                //|| schoolProgramme.ProgrammeCategory.Equals(ProgrammeCategory.Phd.ToString())
                )
            {
                return (ModeOfEntry.PG.ToString(), GetLevelIdByName("700", levels));
            }
            else if (
                //schoolProgramme.ProgrammeCategory.Equals(ProgrammeCategory.Masters.ToString()) ||
                //schoolProgramme.ProgrammeCategory.Equals(ProgrammeCategory.Post_Graduate.ToString()) ||
                schoolProgramme.ProgrammeCategory.Equals(ProgrammeCategory.Phd.ToString()))
            {
                return (ModeOfEntry.PG.ToString(), GetLevelIdByName("900", levels));
            }
            else
            {
                return ("", 0);
            }
        }


        [Authorize]
        [HttpGet]
        public ActionResult NotifyApplicant()
        {
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FancyName");
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateInput(false)]
        public async Task<ActionResult> NotifyApplicant(ApplicantNotificationVm model)
        {
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FancyName");

            if(ModelState.IsValid)
            {
                var applicants = await _db.Applicants.AsNoTracking()
                                .Where(x => x.SchoolProgramme.SchoolProgrammeId.Equals(model.SchoolProgrammeId)
                                && x.Session.SessionId.Equals(model.SessionId)).ToListAsync();
                foreach (var applicant in applicants)
                {
                    await NotifyApplicantByEmail(applicant.ApplicantEmail, applicant.FullName, model.Subject,
                                model.Body, applicant.ApplicantId);
                }
                //await NotifyApplicantByEmail("kunlesymls@gmail.com", "Joseph Ajileye", model.Subject,
                //                model.Body, "AbC123");
                //return new JsonResult { Data = new { status = true, message = $" Email Message has been sent successfully" } };
                return new JsonResult { Data = new { status = true, message = $"{applicants.Count} Email Message has been sent successfully" } };
            }

            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FancyName");
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            return View(model);
        }

        [Authorize]
        [ValidateInput(false)]
        public async Task DownloadNotifyApplicant(int SchoolProgrammeId, int SessionId)
        {
            var applicants = await _db.Applicants.Include(i => i.AvailableCourse.Programme).AsNoTracking()
                                .Where(x => x.SchoolProgramme.SchoolProgrammeId.Equals(SchoolProgrammeId)
                                && x.Session.SessionId.Equals(SessionId)).ToListAsync();

            char c1 = 'A';
            ExcelPackage package = new ExcelPackage();
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Report");

            worksheet.Cells[$"{c1++}1"].Value = "S/N";
            worksheet.Cells[$"{c1++}1"].Value = "Form No";
            worksheet.Cells[$"{c1++}1"].Value = "Programme Name";
            worksheet.Cells[$"{c1++}1"].Value = "FullName";
            worksheet.Cells[$"{c1++}1"].Value = "Email";
            worksheet.Cells[$"{c1++}1"].Value = "Phone Number";

            int rowStart = 2;
            //char c2 = 'A';

            for (var i = 0; i < applicants.Count; i++)
            {
                worksheet.Cells[$"A{rowStart}"].Value = i + 1;
                worksheet.Cells[$"B{rowStart}"].Value = applicants[i].ApplicantId;
                worksheet.Cells[$"C{rowStart}"].Value = applicants[i].AvailableCourse?.Programme?.ProgrammeName;
                worksheet.Cells[$"D{rowStart}"].Value = applicants[i].FullName;
                worksheet.Cells[$"E{rowStart}"].Value = applicants[i].ApplicantEmail;
                worksheet.Cells[$"F{rowStart}"].Value = applicants[i].PhoneNumber;
                rowStart++;
            }           

            worksheet.Cells["A:AZ"].AutoFitColumns();
            worksheet.Column(1).Style.Locked = true;
            worksheet.Column(2).Style.Locked = true;
            worksheet.Column(3).Style.Locked = true;
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment: filename=" +
                                                      $"Dept Recommended List.xlsx");
            Response.BinaryWrite(package.GetAsByteArray());
            Response.End();
        }

        public ActionResult ChangeApplicantSchoolProgramme()
        {
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FancyName");
            ViewBag.Session = new SelectList(_db.Sessions.OrderByDescending(s => s.SessionName).AsNoTracking(), "SessionId", "SessionName");

            return View();
        }

        [HttpPost]
        public async Task<ActionResult> ChangeApplicantSchoolProgramme(string email, int id, int sessionId)
        {
            var message = "";

            var applicant = await _db.Applicants.Include(x => x.SchoolProgramme).Where(a => a.ApplicantEmail.Trim().ToUpper().Equals(email.ToUpper().Trim())).FirstOrDefaultAsync();

            if (applicant != null)
            {
                var applicantPayments = await _db.ApplicantPayments.Include(x => x.SchoolProgramme).Include(i => i.Session)
                                    .Where(x => x.ApplicantEmail.Trim().ToUpper().Equals(applicant.ApplicantEmail.Trim().ToUpper()))
                                    .FirstOrDefaultAsync();

                //applicant.ApplicantEmail = "Balamubarak01@gmail.com";
                //applicant.LastName = "Kabonbwok ";

                if(applicantPayments != null)
                {
                    applicantPayments.IsPayed = true;
                    applicantPayments.SchoolProgrammeId = id;
                    applicantPayments.SessionId = sessionId;
                    _db.Entry(applicantPayments).State = EntityState.Modified;
                }

                applicant.SchoolProgrammeId = id;
                applicant.SessionId = sessionId;
                applicant.HasPayed = true;

                _db.Entry(applicant).State = EntityState.Modified;
                await _db.SaveChangesAsync();

                message = $"{applicant.FullName} school programme changed successfully";
                return new JsonResult { Data = new { status = true, message } };
            }
            message = "No applicant with such email address found";
            return new JsonResult { Data = new { status = false, message } };
        }

        public async Task<ActionResult> updateLastYearAppPayment(string email)
         {
            var applicatPayments = await _db.ApplicantPayments.Include(x => x.Session)
                                        .Where(x => x.ApplicantEmail.Trim().ToUpper().Equals(email.Trim().ToUpper()))
                                        .ToListAsync();
            if (applicatPayments.Count > 0 )
            {
                foreach (var item in applicatPayments)
                {
                    item.ApplicantEmail = item.Session.SessionName + item.ApplicantEmail;
                    _db.Entry(item).State = EntityState.Modified;
                }

                await _db.SaveChangesAsync();
            }

            return Json("succes", JsonRequestBehavior.AllowGet);
         }

        [HttpGet]
        //[Authorize(Roles = RoleName.Admin + "," + RoleName.DatabaseAdmin + "," + RoleName.SuperAdmin + "," + RoleName.AdmissionUploadOfficer)]
        public ActionResult UploadPGRecommendation(string message)
        
        {
            ViewBag.Message = message;
            return View();
        }

        //[Authorize(Roles = RoleName.Admin + "," + RoleName.DatabaseAdmin + "," + RoleName.SuperAdmin + "," + RoleName.AdmissionUploadOfficer)]
        public async Task<ActionResult>UploadPGRecommendation(HttpPostedFileBase excelfile)
        {
            if (excelfile == null || excelfile.ContentLength == 0)
            {
                ViewBag.Error = "Please Select a excel file <br/>";
                ViewBag.Message = "You must select an excel file before you click Upload button";
                TempData["Title"] = "Error.";

                return View("Index");
            }
            HttpPostedFileBase file = Request.Files["excelfile"];
            if (excelfile.FileName.EndsWith("xls", StringComparison.CurrentCulture) || excelfile.FileName.EndsWith("xlsx", StringComparison.CurrentCulture))
            {
                string lastrecord = "";
                int recordCount = 0;
                string message = "";
                string fileContentType = file.ContentType;
                byte[] fileBytes = new byte[file.ContentLength];
                var data = file.InputStream.Read(fileBytes, 0, Convert.ToInt32(file.ContentLength));

                var utmeUploadError = new List<StudentUtmeUploadErrorVm>();
                bool hasUtmeUploadError = false;


                // Read data from excel file
                using (var package = new ExcelPackage(file.InputStream))
                {
                    ExcelValidation myExcel = new ExcelValidation();
                    var currentSheet = package.Workbook.Worksheets;
                    var workSheet = currentSheet.First();
                    var noOfCol = workSheet.Dimension.End.Column;
                    var noOfRow = workSheet.Dimension.End.Row;
                    int requiredField = 1;

                    string validCheck = myExcel.ValidateExcel(noOfRow, workSheet, requiredField);
                    if (!validCheck.Equals("Success"))
                    {
                        string[] ssizes = validCheck.Split(' ');
                        string[] myArray = new string[2];
                        for (int i = 0; i < ssizes.Length; i++)
                        {
                            myArray[i] = ssizes[i];
                        }
                        string lineError = $"Line/Row number {myArray[0]}  and column {myArray[1]} is not rightly formatted, Please Check for anomalies ";
                        ViewBag.Message = lineError;
                        return View("ErrorException");
                    }

                    for (int row = 2; row <= noOfRow; row++)
                    {
                        var matNo = workSheet.Cells[row, 1].Value.ToString().Trim();

                        //var student = await _db.Students.AsNoTracking()
                        //                    .Where(x => x.MatricNo.Trim().ToUpper().Equals(matNo.ToUpper()))
                        //                    .ToListAsync();

                        Applicant applicant = await _db.Applicants.Where(s => s.ApplicantId.Equals(matNo))
                                      .FirstOrDefaultAsync();

                        //foreach (var item in student)
                        //{
                            if (applicant != null)
                            {
                            applicant.IsDeptApproved = true;
                            applicant.IsFacultyApproved = true;
                            applicant.IsPGApproved = true;
                            applicant.IsDownloaded = true;
                            //applicant.IsVcApproved = null;
                            //applicant.SessionId = sessionId;

                            var applicantPayments = await _db.ApplicantPayments.Include(x => x.SchoolProgramme).Include(i => i.Session)
                                    .Where(x => x.ApplicantEmail.Trim().ToUpper().Equals(applicant.ApplicantEmail.Trim().ToUpper()) && x.IsPayed == true)
                                    .FirstOrDefaultAsync();

                            if (applicantPayments != null)
                            {
                                applicant.SchoolProgrammeId = applicant.SchoolProgrammeId;
                                applicant.HasPayed = true;
                                applicantPayments.IsPayed = true;
                                applicantPayments.SchoolProgrammeId = (Int32)applicant.SchoolProgrammeId;
                                //applicantPayments.SessionId = sessionId;
                                _db.Entry(applicantPayments).State = EntityState.Modified;
                            }


                            _db.Entry(applicant).State = EntityState.Modified;

                               
                            }
                            else
                            {
                                ViewBag.ErrorMessage = "Applicant not found";
                            utmeUploadError.Add(new StudentUtmeUploadErrorVm { JambRegNo = matNo, Row = row });
                            hasUtmeUploadError = true;
                            //return View("ErrorException");
                        }
                            recordCount++;
                            //lastrecord = $"The last Updated record has the Surname  {applicant.LastName} and " +
                            //    $"First Name {applicant.FirstName} with  Reg No {applicant.ApplicantId}";
                        //}

                        
                    }
                    try
                    {
                        await _db.SaveChangesAsync();

                    }
                    catch (Exception ex)
                    {
                        ViewBag.ErrorMessage = ex.Message;
                        return View("ErrorException");
                    }
                    if (hasUtmeUploadError)
                    {
                        ViewBag.ErrorInfo = $"Success!";
                        ViewBag.ErrorMessage = $"You have successfully Removed {recordCount} records...";
                        return View("ErrorException", utmeUploadError);
                    }
                    message = $"You have successfully Deleted {recordCount} records...  and {lastrecord}";
                    ViewBag.Message = message;
                    return RedirectToAction("Index", "Students", new { message });
                }
            }
            ViewBag.Error = $"File type is Incorrect <br/>";
            return View("Index");
        }

        public static string GetEnumDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (attributes != null && attributes.Length > 0)
                return attributes[0].Description;
            else
                return value.ToString();
        }

        //public static class EnumHelper
        //{
        //    public static string GetEnumDescription(Enum value)
        //    {
        //        var fieldInfo = value.GetType().GetField(value.ToString());
        //        var attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

        //        return attributes.Length > 0 ? attributes[0].Description : value.ToString();
        //    }
        //}

    //public static string GetEnumDescription(Enum value)
    //{
    //    FieldInfo fi = value.GetType().GetField(value.ToString());
    //    DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

    //    return attributes.Length > 0 ? attributes[0].Description : value.ToString();
    //}
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
