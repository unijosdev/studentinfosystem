using Microsoft.Ajax.Utilities;
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
using System.Data.Entity;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;

namespace SwiftKampus.Controllers
{
    [Authorize]
    [Audit(AuditingLevel = 2)]
    public class UtmeApplicantsController : BaseController
    {


        public UtmeApplicantsController(SchoolDbContext db) : base(db)
        {

        }
        // GET: UtmeApplicants
        public ActionResult Index(string message)
        {
            ViewBag.Message = message;
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            ViewData.Add("ActionMessage", "Utme Applicants list View");
            return View();
        }

        public ActionResult ScrenningIndex(string message)
        {
            ViewBag.Message = message;
            var mystates = from State s in Enum.GetValues(typeof(State))
                           select new { ID = s, Name = s.ToString() };
            ViewBag.StateOfOrigin = new SelectList(mystates, "Name", "Name");
            ViewBag.SessionId = new SelectList(_db.Sessions.AsNoTracking(), "SessionId", "SessionName");

            ViewBag.ProgrammeId = new SelectList(_db.Programmes.AsNoTracking(), "ProgrammeId", "ProgrammeName");
            ViewBag.DepartmentId = new SelectList(_db.Departments.AsNoTracking(), "DepartmentId", "DeptName");
            ViewBag.FacultyId = new SelectList(_db.Faculties.AsNoTracking(), "FacultyId", "FacultyName");

            ViewData.Add("ActionMessage", "Utme Applicant Screening View");
            return View();
        }

        public async Task<ActionResult> GetUtmeIndex(string hasRegistered, string isDirectEntry, int? SessionId)
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
            //Soring direction(either desending or ascending)
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
            string search = Request.Form.GetValues("search[value]").FirstOrDefault();

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int totalRecords = 0;

            var utmeIndex = new List<UtmeApplicantVm>();
            var utmeApplicants = new List<UtmeApplicant>();
            if (SessionId != null)
            {
                utmeApplicants = await _db.UtmeApplicants.Include(i => i.Programme).AsNoTracking()
                                   .Where(x => x.SessionId.Equals((int)SessionId)).ToListAsync();
            }
            else
            {
                utmeApplicants = await _db.UtmeApplicants.Include(i => i.Programme).AsNoTracking()
                                  .Where(x => x.SessionId.Equals(sessionId)).ToListAsync();
            }

            if (!string.IsNullOrEmpty(search))
            {

                var v = utmeApplicants.Where(x => x.JambRegNo.Trim().ToUpper().Contains(search.Trim().ToUpper()))
                                        .ToList();
                // Mapping the student to the correct ViewModel for json display
                utmeIndex = MapToUtmeIndex(v);
            }
            else if (!string.IsNullOrEmpty(isDirectEntry) && !string.IsNullOrEmpty(hasRegistered))
            {
                var v = utmeApplicants.Where(x => x.IsDirectEntry.ToString().ToUpper().Equals(isDirectEntry.ToUpper())
                                        && x.HasRegistered.ToString().ToUpper().Equals(hasRegistered.ToUpper()))
                                       .ToList();
                // Mapping the student to the correct ViewModel for json display
                utmeIndex = MapToUtmeIndex(v);
            }
            else if (!string.IsNullOrEmpty(hasRegistered))
            {
                var v = utmeApplicants.Where(x => x.HasRegistered.ToString().ToUpper().Equals(hasRegistered.ToUpper()))
                                       .ToList();
                // Mapping the student to the correct ViewModel for json display
                utmeIndex = MapToUtmeIndex(v);
            }
            else if (!string.IsNullOrEmpty(isDirectEntry))
            {
                var v = utmeApplicants.Where(x => x.IsDirectEntry.ToString().ToUpper().Equals(isDirectEntry.ToUpper()))
                                       .ToList();
                // Mapping the student to the correct ViewModel for json display
                utmeIndex = MapToUtmeIndex(v);
            }
            else
            {
                var v = utmeApplicants;
                // Mapping the student to the correct ViewModel for json display
                utmeIndex = MapToUtmeIndex(v);
            }

            totalRecords = utmeIndex.Count();
            var data = utmeIndex.Skip(skip).Take(pageSize).ToList();
            ViewData.Add("ActionMessage", $"{data.Count} List of Utme Applicants");

            return Json(new { draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data },
                JsonRequestBehavior.AllowGet);

            #endregion Server Side filtering
        }

        public async Task<ActionResult> GetUtmeScreningIndex(string StateOfOrigin, int? ProgrammeId, int? FacultyId, int? DepartmentId, int? SessionId, string isDirectEntry)
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
            //Soring direction(either desending or ascending)
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
            string search = Request.Form.GetValues("search[value]").FirstOrDefault();

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int totalRecords = 0;

            var utmeIndex = new List<UtmeScrenningVm>();
            bool myDirectEntry = Convert.ToBoolean(isDirectEntry);
            var utmeApplicants = await _db.UtmeApplicants.Include(i => i.Programme).Include(i => i.Programme.Department.Faculty)
                                    .Include(i => i.UtmeApplicantSubjects)/*.AsNoTracking()*/
                                    .Where(x => x.SessionId.Equals((int)SessionId) && x.HasRegistered.Equals(true)
                                    && x.IsDirectEntry.Equals(myDirectEntry)).ToListAsync();

            if (FacultyId != null)
            {
                utmeApplicants = utmeApplicants.Where(x => x.Programme.Department.FacultyId.Equals((int)FacultyId)).ToList();
                //foreach (var item in utmeApplicants)
                //{
                //    var payment = await _db.ApplicantPayments.AsNoTracking().Where(x => x.ApplicantEmail.Equals(item.Email)).FirstOrDefaultAsync();
                //    if (payment.IsPayed == true)
                //    {
                //        item.HasPayed = true;
                //        _db.Entry(item).State = EntityState.Modified;
                //    }
                //}

                //await _db.SaveChangesAsync();

            }
            if (DepartmentId != null)
            {
                utmeApplicants = utmeApplicants.Where(x => x.Programme.Department.DepartmentId.Equals((int)DepartmentId)).ToList();
            }
            if (ProgrammeId != null)
            {
                utmeApplicants = utmeApplicants.Where(x => x.ProgrammeId.Equals((int)ProgrammeId)).ToList();
            }

            if (!string.IsNullOrEmpty(search))
            {
                utmeApplicants = utmeApplicants.Where(x => x.JambRegNo.ToUpper().Equals(search.ToUpper().Trim())).ToList();
            }
            //if (!string.IsNullOrEmpty(StateOfOrigin))
            //{
            //    utmeApplicants = utmeApplicants
            //            .Where(x => x.StateOfOrigin.ToUpper().Equals(StateOfOrigin.ToUpper())).ToList();
            //}
            utmeIndex = await MapToUtmeScrenningIndex(utmeApplicants, (int)SessionId);


            totalRecords = utmeIndex.Count();
            var data = utmeIndex.Skip(skip).Take(pageSize).ToList();
            ViewData.Add("ActionMessage", $"{data.Count} List of Utme Screnning List");

            return Json(new { draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data },
                JsonRequestBehavior.AllowGet);

            #endregion Server Side filtering
        }

        //public async Task<ActionResult> GetDeScreningIndex(string hasRegistered, string isDirectEntry, int? SessionId)
        public async Task<ActionResult> GetDeScreningIndex(int? SessionId)
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
            //Soring direction(either desending or ascending)
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
            string search = Request.Form.GetValues("search[value]").FirstOrDefault();

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int totalRecords = 0;

            var utmeIndex = new List<UtmeApplicantVm>();
            var utmeApplicants = await _db.UtmeApplicants.Include(i => i.Programme).Include(i => i.Programme.Department.Faculty).AsNoTracking()
                                    .Where(x => x.SessionId.Equals((int)SessionId) && x.IsDirectEntry.Equals(false)
                                    && x.HasPayed.Equals(true)).ToListAsync();

            if (!string.IsNullOrEmpty(search))
            {

                var v = utmeApplicants.Where(x => x.JambRegNo.ToUpper().Equals(search.ToUpper().Trim()))
                                        .ToList();
                // Mapping the student to the correct ViewModel for json display
                utmeIndex = MapToUtmeIndex(v);
            }

            //if (!string.IsNullOrEmpty(isDirectEntry) && !string.IsNullOrEmpty(hasRegistered))
            //{
            //    var v = utmeApplicants.Where(x => x.IsDirectEntry.ToString().ToUpper().Equals(isDirectEntry.ToUpper())
            //                            && x.HasRegistered.ToString().ToUpper().Equals(hasRegistered.ToUpper()))
            //                           .ToList();
            //    // Mapping the student to the correct ViewModel for json display
            //    utmeIndex.AddRange(MapToUtmeIndex(v));
            //}
            //else if (!string.IsNullOrEmpty(hasRegistered))
            //{
            //    var v = utmeApplicants.Where(x => x.HasRegistered.ToString().ToUpper().Equals(hasRegistered.ToUpper()))
            //                           .ToList();
            //    // Mapping the student to the correct ViewModel for json display
            //    utmeIndex.AddRange(MapToUtmeIndex(v));
            //}
            //else if (!string.IsNullOrEmpty(isDirectEntry))
            //{
            //    var v = utmeApplicants.Where(x => x.IsDirectEntry.ToString().ToUpper().Equals(isDirectEntry.ToUpper()))
            //                           .ToList();
            //    // Mapping the student to the correct ViewModel for json display
            //    utmeIndex.AddRange(MapToUtmeIndex(v));
            //}

            //var v = utmeApplicants;
            // Mapping the student to the correct ViewModel for json display
            utmeIndex = MapToUtmeIndex(utmeApplicants);


            totalRecords = utmeIndex.Count();
            var data = utmeIndex.Skip(skip).Take(pageSize).ToList();
            ViewData.Add("ActionMessage", $"{data.Count} List of Utme Direct entry Screnning List");


            return Json(new { draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data },
                JsonRequestBehavior.AllowGet);

            #endregion Server Side filtering
        }

        private List<UtmeApplicantVm> MapToUtmeIndex(List<UtmeApplicant> v)
        {
            var utmeApplicant = v.Select(s => new UtmeApplicantVm()
            {
                JambRegNo = s.JambRegNo,
                MiddleName = s.MiddleName,
                StateOfOrigin = s.StateOfOrigin,
                Lga = s.Programme.ProgrammeName,
                ResultGrade = s.ResultGrade,
                FullName = $"{s.Surname} {s.FirstName}",
                Email = s.Email,
                DateofBirth =  s.DateOfBirth?.Date.ToString()

            }).ToList();

            return utmeApplicant;
        }
        async Task<List<UtmeScrenningVm>> MapToUtmeScrenningIndex(List<UtmeApplicant> v, int SessionId)
        {
            var utmeScrenningVmList = new List<UtmeScrenningVm>();
            var utmeScreeningPolicy = await _db.UtmeScreningPolicies.AsNoTracking()
                                          .Where(x => x.SessionId.Equals(SessionId))
                                          .FirstOrDefaultAsync();
            foreach (var item in v)
            {
                var oLevelResults = await _db.ApplicantOLevelResults.Include(i => i.Subject).AsNoTracking()
                                         .Where(x => x.ApplicantId.Trim().ToUpper().Equals(item.Email.Trim().ToUpper()))
                                         .ToListAsync();

                if (oLevelResults.Count >= 1)
                {
                    var subjectGradePoint = oLevelResults.Take(5).Sum(x => x.GradePoint);
                    int jambScore = Convert.ToInt16(item.ResultGrade);
                    var jambPercentage = (jambScore * utmeScreeningPolicy.JambPercentage) / utmeScreeningPolicy.JambMaximumScore;
                    //var olevelPercentage = (subjectGradePoint * utmeScreeningPolicy.OLevelPercentage) / utmeScreeningPolicy.OLevelMaximumScore;
                    var olevelPercentage = (item.OlevelScore * utmeScreeningPolicy.OLevelPercentage) / utmeScreeningPolicy.OLevelMaximumScore;
                    var cummulative = jambPercentage + olevelPercentage;
                    var screnning = new UtmeScrenningVm
                    {
                        JambRegNo = item.JambRegNo,
                        FullName = $"{item.Surname} {item.FirstName} {item.MiddleName}",
                        StateOfOrigin = item.StateOfOrigin,
                        FacultyName = item.Programme.Department.Faculty.FacultyName,
                        JambPercentage = item.JambPercentage.ToString(),
                        DateOfBirth = item.DateOfBirth?.Date.ToString("dd MMM yyyy"),
                        //JambPercentage = jambPercentage.ToString(),
                        //OLevelCummulative = olevelPercentage.ToString(),
                        OLevelCummulative = item.OlevelPercentage.ToString(),
                        Cummulative = item.Cummulative.ToString(),
                        //Cummulative = cummulative.ToString(),
                        ProgrammeName = item.Programme.ProgrammeName,
                        LocalGovtArea = item.LocalGovtArea,
                        JambScore = item.ResultGrade
                    };
                    for (int i = 0; i < oLevelResults.Count; i++)
                    {
                        switch (i)
                        {
                            case 0:
                                screnning.SubjectOne = oLevelResults[i].Subject.CourseName;
                                //screnning.GradeOne = oLevelResults[i].SubjectGrade.ToString();
                                screnning.GradeOne = oLevelResults[i].SubjectGrade != null ? oLevelResults[i].SubjectGrade.ToString() : "";
                                break;
                            case 1:
                                screnning.SubjectTwo = oLevelResults[i].Subject.CourseName;
                                //screnning.GradeTwo = oLevelResults[i].SubjectGrade.ToString();
                                screnning.GradeTwo = oLevelResults[i].SubjectGrade != null ? oLevelResults[i].SubjectGrade.ToString() : "";
                                break;
                            case 2:
                                screnning.SubjectThree = oLevelResults[i].Subject.CourseName;
                                //screnning.GradeThree = oLevelResults[i].SubjectGrade.ToString();
                                screnning.GradeThree = oLevelResults[i].SubjectGrade != null ? oLevelResults[i].SubjectGrade.ToString() : "";
                                break;
                            case 3:
                                screnning.SubjectFour = oLevelResults[i].Subject.CourseName;
                                //screnning.GradeFour = oLevelResults[i].SubjectGrade.ToString();
                                screnning.GradeFour = oLevelResults[i].SubjectGrade != null ? oLevelResults[i].SubjectGrade.ToString() : "";
                                break;
                            case 4:
                                screnning.SubjectFive = oLevelResults[i].Subject.CourseName;
                                //screnning.GradeFive = oLevelResults[i].SubjectGrade.ToString();
                                screnning.GradeFive = oLevelResults[i].SubjectGrade != null ? oLevelResults[i].SubjectGrade.ToString() : "";
                                break;
                        }
                    }
                    for (int i = 0; i < item.UtmeApplicantSubjects.Count; i++)
                    {
                        switch (i)
                        {
                            case 0:
                                screnning.JambSubjectOne = item.UtmeApplicantSubjects[i].SubjectName;
                                screnning.JambSubjectScoreOne = item.UtmeApplicantSubjects[i].Score;
                                break;
                            case 1:
                                screnning.JambSubjectTwo = item.UtmeApplicantSubjects[i].SubjectName;
                                screnning.JambSubjectScoreTwo = item.UtmeApplicantSubjects[i].Score;
                                break;
                            case 2:
                                screnning.JambSubjectThree = item.UtmeApplicantSubjects[i].SubjectName;
                                screnning.JambSubjectScoreThree = item.UtmeApplicantSubjects[i].Score;
                                break;
                            case 3:
                                screnning.JambSubjectFour = item.UtmeApplicantSubjects[i].SubjectName;
                                screnning.JambSubjectScoreFour = item.UtmeApplicantSubjects[i].Score;
                                break;
                        }
                    }
                    utmeScrenningVmList.Add(screnning);
                }

            }

            return utmeScrenningVmList;
        }

        async Task<List<DeScrenningVm>> MapToDeScrenningIndex(List<UtmeApplicant> v, int SessionId)
        {
            var utmeScrenningVmList = new List<DeScrenningVm>();
            var utmeScreeningPolicy = await _db.UtmeScreningPolicies.AsNoTracking()
                                          .Where(x => x.SessionId.Equals(SessionId))
                                          .FirstOrDefaultAsync();
            foreach (var item in v)
            {
                var oLevelResults = await _db.ApplicantOLevelResults.Include(i => i.Subject).AsNoTracking()
                                         .Where(x => x.ApplicantId.Trim().ToUpper().Equals(item.Email.Trim().ToUpper()))
                                         .ToListAsync();
                var qualification = _db.Qualifications.AsNoTracking()
                                    .Where(x => x.UserId.Trim().ToUpper().Equals(item.Email.Trim().ToUpper()))
                                    .FirstOrDefault();

                if (oLevelResults.Count >= 1)
                {
                    var subjectGradePoint = oLevelResults.Take(5).Sum(x => x.GradePoint);
                    int jambScore = Convert.ToInt16(item.ResultGrade);
                    var jambPercentage = (jambScore * utmeScreeningPolicy.JambPercentage) / utmeScreeningPolicy.JambMaximumScore;
                    var olevelPercentage = (subjectGradePoint * utmeScreeningPolicy.OLevelPercentage) / utmeScreeningPolicy.OLevelMaximumScore;
                    var cummulative = jambPercentage + olevelPercentage;
                    var screnning = new DeScrenningVm();


                    screnning.JambRegNo = item.JambRegNo;
                    screnning.FullName = $"{item.Surname} {item.FirstName} {item.MiddleName}";
                    screnning.DateOfBirth = item.DateOfBirth?.ToString("dd MMM yyyy");
                    screnning.FacultyName = item.Programme?.Department?.Faculty.FacultyName;
                    screnning.StateOfOrigin = item.StateOfOrigin;
                    screnning.JambPercentage = jambPercentage.ToString();
                    screnning.OLevelCummulative = olevelPercentage.ToString();
                    screnning.Cummulative = cummulative.ToString();
                    screnning.ProgrammeName = item.Programme?.ProgrammeName;
                    screnning.LocalGovtArea = item.LocalGovtArea;
                    screnning.JambScore = item.ResultGrade;
                    screnning.InstitutionName = qualification?.NameOfInstitution;
                    screnning.ResultName = qualification?.Grade;
                    screnning.ResultType = qualification?.QualificationName;
                    screnning.Discipline = string.IsNullOrEmpty(qualification?.Discipline) 
                                            ? "" : qualification?.Discipline
                                            .Substring(0, qualification.Discipline.Length >= 20 ? 20 : qualification.Discipline.Length);
                    screnning.YearAttended = qualification?.ToDate?.ToString("dd MMM yyyy");

                    for (int i = 0; i < oLevelResults.Count; i++)
                    {
                        switch (i)
                        {
                            case 0:
                                screnning.SubjectOne = oLevelResults[i].Subject?.CourseName;
                                screnning.GradeOne = oLevelResults[i].SubjectGrade?.ToString();
                                break;
                            case 1:
                                screnning.SubjectTwo = oLevelResults[i].Subject?.CourseName;
                                screnning.GradeTwo = oLevelResults[i].SubjectGrade?.ToString();
                                break;
                            case 2:
                                screnning.SubjectThree = oLevelResults[i].Subject?.CourseName;
                                screnning.GradeThree = oLevelResults[i].SubjectGrade?.ToString();
                                break;
                            case 3:
                                screnning.SubjectFour = oLevelResults[i].Subject?.CourseName;
                                screnning.GradeFour = oLevelResults[i].SubjectGrade?.ToString();
                                break;
                            case 4:
                                screnning.SubjectFive = oLevelResults[i].Subject.CourseName;
                                screnning.GradeFive = oLevelResults[i].SubjectGrade?.ToString();
                                break;
                        }
                    }
                    for (int i = 0; i < item.UtmeApplicantSubjects.Count; i++)
                    {
                        switch (i)
                        {
                            case 0:
                                screnning.JambSubjectOne = item.UtmeApplicantSubjects[i]?.SubjectName;
                                screnning.JambSubjectScoreOne = item.UtmeApplicantSubjects[i]?.Score;
                                break;
                            case 1:
                                screnning.JambSubjectTwo = item.UtmeApplicantSubjects[i]?.SubjectName;
                                screnning.JambSubjectScoreTwo = item.UtmeApplicantSubjects[i]?.Score;
                                break;
                            case 2:
                                screnning.JambSubjectThree = item.UtmeApplicantSubjects[i]?.SubjectName;
                                screnning.JambSubjectScoreThree = item.UtmeApplicantSubjects[i]?.Score;
                                break;
                            case 3:
                                screnning.JambSubjectFour = item.UtmeApplicantSubjects[i]?.SubjectName;
                                screnning.JambSubjectScoreFour = item.UtmeApplicantSubjects[i]?.Score;
                                break;
                        }
                    }
                    utmeScrenningVmList.Add(screnning);
                }

            }

            return utmeScrenningVmList;
        }

        public string fixScreeningReg(int SessionId)
        {
            int count = 0;

            // Ensure SessionId is valid
            if (SessionId <= 0)
            {
                return "Invalid SessionId.";
            }

            // Retrieve all applicant payments for the given session
            var applicantPayments = _db.ApplicantPayments.AsNoTracking()
                                  .Where(x => x.SchoolProgrammeId == 1 &&
                                              x.SessionId == SessionId &&
                                              x.IsPayed == true).ToList();

            // Process each applicant payment
            foreach (var item in applicantPayments)
            {
                var utmeApplicant = _db.UtmeApplicants
                                       .Where(x => x.Email.Equals(item.ApplicantEmail))
                                       .FirstOrDefault();

                // If the applicant exists, update their registration status
                if (utmeApplicant != null)
                {
                    utmeApplicant.HasRegistered = true;

                    // Save changes to the database
                    _db.Entry(utmeApplicant).State = EntityState.Modified;
                    _db.SaveChanges();

                    // Increment the count of updated applicants
                    count++;
                }
            }

            // Return a success message with the count of updated records
            return $"{count} UTME Applicants have been registered successfully.";
        }

        public async Task DownloadReport(int? SessionId, int? ProgrammeId, string isDirectEntry)
        {
            var model = new List<UtmeScrenningVm>();
            var utmeApplicants = new List<UtmeApplicant>();
            bool myDirectEntry = Convert.ToBoolean(isDirectEntry);
            if (ProgrammeId != null)
            {
                utmeApplicants = await _db.UtmeApplicants.Include(i => i.Programme).Include(i => i.Programme.Department.Faculty)
                                   .Include(i => i.UtmeApplicantSubjects).AsNoTracking()
                                   .Where(x => x.SessionId.Equals((int)SessionId) && x.HasRegistered.Equals(true)
                                   && x.IsDirectEntry.Equals(myDirectEntry) && x.Programme.ProgrammeId.Equals((int)ProgrammeId)).ToListAsync();
            }
            else
            {
                utmeApplicants = await _db.UtmeApplicants.Include(i => i.Programme).Include(i => i.Programme.Department.Faculty)
                                    .Include(i => i.UtmeApplicantSubjects).AsNoTracking()
                                    .Where(x => x.SessionId.Equals((int)SessionId) && x.HasRegistered.Equals(true)
                                    && x.IsDirectEntry.Equals(myDirectEntry)).ToListAsync();
            }


            model = await MapToUtmeScrenningIndex(utmeApplicants, (int)SessionId);

            char c1 = 'A';
            ExcelPackage package = new ExcelPackage();
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Report");

            worksheet.Cells[$"{c1++}1"].Value = "Jamb Reg No";
            worksheet.Cells[$"{c1++}1"].Value = "FullName";
            worksheet.Cells[$"{c1++}1"].Value = "DoB";
            worksheet.Cells[$"{c1++}1"].Value = "Faculty Name";
            worksheet.Cells[$"{c1++}1"].Value = "Course Name";
            worksheet.Cells[$"{c1++}1"].Value = "State";
            worksheet.Cells[$"{c1++}1"].Value = "LGA";
            worksheet.Cells[$"{c1++}1"].Value = "Jamb Score";
            worksheet.Cells[$"{c1++}1"].Value = "Subject One";
            worksheet.Cells[$"{c1++}1"].Value = "Grade One";
            worksheet.Cells[$"{c1++}1"].Value = "Subject Two";
            worksheet.Cells[$"{c1++}1"].Value = "Grade Two";
            worksheet.Cells[$"{c1++}1"].Value = "Subject Three";
            worksheet.Cells[$"{c1++}1"].Value = "Grade Three";
            worksheet.Cells[$"{c1++}1"].Value = "Subject Four";
            worksheet.Cells[$"{c1++}1"].Value = "Grade Four";
            worksheet.Cells[$"{c1++}1"].Value = "Subject Five";
            worksheet.Cells[$"{c1++}1"].Value = "Grade Five";
            worksheet.Cells[$"{c1++}1"].Value = "Jamb %";
            worksheet.Cells[$"{c1++}1"].Value = "O Level %";
            worksheet.Cells[$"{c1++}1"].Value = "Cumulative";
            worksheet.Cells[$"{c1++}1"].Value = "Jamb Subject One";
            worksheet.Cells[$"{c1++}1"].Value = "Score";
            worksheet.Cells[$"{c1++}1"].Value = "Jamb Subject Two";
            worksheet.Cells[$"{c1++}1"].Value = "Score";
            worksheet.Cells[$"{c1++}1"].Value = "Jamb Subject Three";
            worksheet.Cells[$"AA1"].Value = "Score";
            worksheet.Cells[$"AB1"].Value = "Jamb Subject Four";
            worksheet.Cells[$"AC1"].Value = "Score";


            int rowStart = 2;
            //char c2 = 'A';

            for (var i = 0; i < model.Count; i++)
            {
                worksheet.Cells[$"A{rowStart}"].Value = model[i].JambRegNo;
                worksheet.Cells[$"B{rowStart}"].Value = model[i].FullName;
                worksheet.Cells[$"C{rowStart}"].Value = model[i].DateOfBirth;
                worksheet.Cells[$"D{rowStart}"].Value = model[i].FacultyName;
                worksheet.Cells[$"E{rowStart}"].Value = model[i].ProgrammeName;
                worksheet.Cells[$"F{rowStart}"].Value = model[i].StateOfOrigin;
                worksheet.Cells[$"G{rowStart}"].Value = model[i].LocalGovtArea;
                worksheet.Cells[$"H{rowStart}"].Value = model[i].JambScore;
                worksheet.Cells[$"I{rowStart}"].Value = model[i].SubjectOne;
                worksheet.Cells[$"J{rowStart}"].Value = model[i].GradeOne;
                worksheet.Cells[$"K{rowStart}"].Value = model[i].SubjectTwo;
                worksheet.Cells[$"L{rowStart}"].Value = model[i].GradeTwo;
                worksheet.Cells[$"M{rowStart}"].Value = model[i].SubjectThree;
                worksheet.Cells[$"N{rowStart}"].Value = model[i].GradeThree;
                worksheet.Cells[$"O{rowStart}"].Value = model[i].SubjectFour;
                worksheet.Cells[$"P{rowStart}"].Value = model[i].GradeFour;
                worksheet.Cells[$"Q{rowStart}"].Value = model[i].SubjectFive;
                worksheet.Cells[$"R{rowStart}"].Value = model[i].GradeFive;
                worksheet.Cells[$"S{rowStart}"].Value = model[i].JambPercentage;
                worksheet.Cells[$"T{rowStart}"].Value = model[i].OLevelCummulative;
                worksheet.Cells[$"U{rowStart}"].Value = model[i].Cummulative;
                worksheet.Cells[$"V{rowStart}"].Value = model[i].JambSubjectOne;
                worksheet.Cells[$"W{rowStart}"].Value = model[i].JambSubjectScoreOne;
                worksheet.Cells[$"X{rowStart}"].Value = model[i].JambSubjectTwo;
                worksheet.Cells[$"Y{rowStart}"].Value = model[i].JambSubjectScoreTwo;
                worksheet.Cells[$"Z{rowStart}"].Value = model[i].JambSubjectThree;
                worksheet.Cells[$"AA{rowStart}"].Value = model[i].JambSubjectScoreThree;
                worksheet.Cells[$"AB{rowStart}"].Value = model[i].JambSubjectFour;
                worksheet.Cells[$"AC{rowStart}"].Value = model[i].JambSubjectScoreFour;
                rowStart++;
            }

            worksheet.Cells["A:AZ"].AutoFitColumns();
            worksheet.Column(1).Style.Locked = true;
            worksheet.Column(2).Style.Locked = true;
            worksheet.Column(3).Style.Locked = true;
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment: filename=" +
                                                      $"ApplicantPayment.xlsx");
            Response.BinaryWrite(package.GetAsByteArray());
            Response.End();
        }

        public async Task DownloadDeReport(int? SessionId, int? ProgrammeId)
        {
            var model = new List<DeScrenningVm>();
            var utmeApplicants = new List<UtmeApplicant>();

            if (ProgrammeId != null)
            {
                utmeApplicants = await _db.UtmeApplicants.Include(i => i.Programme.Department.Faculty).Include(i => i.UtmeApplicantSubjects).AsNoTracking()
                                   .Where(x => x.SessionId.Equals((int)SessionId) && x.HasRegistered.Equals(true)
                                   && x.IsDirectEntry.Equals(true) && x.Programme.ProgrammeId.Equals((int)ProgrammeId)).ToListAsync();
            }
            else
            {
                utmeApplicants = await _db.UtmeApplicants.Include(i => i.Programme.Department.Faculty).Include(i => i.UtmeApplicantSubjects).AsNoTracking()
                                   .Where(x => x.SessionId.Equals((int)SessionId) && x.HasRegistered.Equals(true)
                                   && x.IsDirectEntry.Equals(true)).ToListAsync();
            }

            model = await MapToDeScrenningIndex(utmeApplicants, (int)SessionId);

            char c1 = 'A';
            ExcelPackage package = new ExcelPackage();
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Report");

            worksheet.Cells[$"{c1++}1"].Value = "Jamb Reg No";
            worksheet.Cells[$"{c1++}1"].Value = "FullName";
            worksheet.Cells[$"{c1++}1"].Value = "DoB";
            worksheet.Cells[$"{c1++}1"].Value = "Faculty Name";
            worksheet.Cells[$"{c1++}1"].Value = "Course Name";
            worksheet.Cells[$"{c1++}1"].Value = "State";
            worksheet.Cells[$"{c1++}1"].Value = "LGA";
            worksheet.Cells[$"{c1++}1"].Value = "Jamb Score";
            worksheet.Cells[$"{c1++}1"].Value = "Subject One";
            worksheet.Cells[$"{c1++}1"].Value = "Grade One";
            worksheet.Cells[$"{c1++}1"].Value = "Subject Two";
            worksheet.Cells[$"{c1++}1"].Value = "Grade Two";
            worksheet.Cells[$"{c1++}1"].Value = "Subject Three";
            worksheet.Cells[$"{c1++}1"].Value = "Grade Three";
            worksheet.Cells[$"{c1++}1"].Value = "Subject Four";
            worksheet.Cells[$"{c1++}1"].Value = "Grade Four";
            worksheet.Cells[$"{c1++}1"].Value = "Subject Five";
            worksheet.Cells[$"{c1++}1"].Value = "Grade Five";
            //worksheet.Cells[$"{c1++}1"].Value = "Jamb %";
            //worksheet.Cells[$"{c1++}1"].Value = "O Level %";
            worksheet.Cells[$"{c1++}1"].Value = "Cumulative";
            worksheet.Cells[$"{c1++}1"].Value = "Institution";
            worksheet.Cells[$"{c1++}1"].Value = "Discipline";
            worksheet.Cells[$"{c1++}1"].Value = "Result Type";
            worksheet.Cells[$"{c1++}1"].Value = "Result Grade";            
            worksheet.Cells[$"{c1++}1"].Value = "Year Attended";            

            int rowStart = 2;

            for (var i = 0; i < model.Count; i++)
            {
                worksheet.Cells[$"A{rowStart}"].Value = model[i].JambRegNo;
                worksheet.Cells[$"B{rowStart}"].Value = model[i].FullName;
                worksheet.Cells[$"C{rowStart}"].Value = model[i].DateOfBirth;
                worksheet.Cells[$"D{rowStart}"].Value = model[i].FacultyName;
                worksheet.Cells[$"E{rowStart}"].Value = model[i].ProgrammeName;
                worksheet.Cells[$"F{rowStart}"].Value = model[i].StateOfOrigin;
                worksheet.Cells[$"G{rowStart}"].Value = model[i].LocalGovtArea;
                worksheet.Cells[$"H{rowStart}"].Value = model[i].ResultName;
                worksheet.Cells[$"I{rowStart}"].Value = model[i].SubjectOne;
                worksheet.Cells[$"J{rowStart}"].Value = model[i].GradeOne;
                worksheet.Cells[$"K{rowStart}"].Value = model[i].SubjectTwo;
                worksheet.Cells[$"L{rowStart}"].Value = model[i].GradeTwo;
                worksheet.Cells[$"M{rowStart}"].Value = model[i].SubjectThree;
                worksheet.Cells[$"N{rowStart}"].Value = model[i].GradeThree;
                worksheet.Cells[$"O{rowStart}"].Value = model[i].SubjectFour;
                worksheet.Cells[$"P{rowStart}"].Value = model[i].GradeFour;
                worksheet.Cells[$"Q{rowStart}"].Value = model[i].SubjectFive;
                worksheet.Cells[$"R{rowStart}"].Value = model[i].GradeFive;
                //worksheet.Cells[$"R{rowStart}"].Value = model[i].JambPercentage;
                //worksheet.Cells[$"S{rowStart}"].Value = model[i].OLevelCummulative;
                worksheet.Cells[$"S{rowStart}"].Value = model[i].Cummulative;
                worksheet.Cells[$"T{rowStart}"].Value = model[i].InstitutionName;
                worksheet.Cells[$"U{rowStart}"].Value = model[i].Discipline;
                worksheet.Cells[$"V{rowStart}"].Value = model[i].ResultType;
                worksheet.Cells[$"W{rowStart}"].Value = model[i].ResultName;
                worksheet.Cells[$"X{rowStart}"].Value = model[i].YearAttended;
                
                rowStart++;
            }

            worksheet.Cells["A:AZ"].AutoFitColumns();
            worksheet.Column(1).Style.Locked = true;
            worksheet.Column(2).Style.Locked = true;
            worksheet.Column(3).Style.Locked = true;
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment: filename=" +
                                                      $"ApplicantPayment.xlsx");
            Response.BinaryWrite(package.GetAsByteArray());
            Response.End();
        }

        //[AllowAnonymous]
        //public async Task<PartialViewResult> PartialDetails(string id)
        //{
        //    var utmeApplicants = await _db.UtmeApplicants.AsNoTracking()
        //                                .Where(x => x.JambRegNo.ToUpper().Equals(id.ToUpper()))
        //                                .ToListAsync();
        //    var utmeApplicantEmail = utmeApplicants.Select(s => s.Email).LastOrDefault();
        //    var oLevelSubjects = await _db.ApplicantOLevelResults.Include(i => i.Subject)
        //                        .AsNoTracking()
        //                        .Where(x => x.ApplicantId.ToUpper().Equals(utmeApplicantEmail.ToUpper()))
        //                        .ToListAsync();
        //    ViewData.Add("ActionMessage", $"Partial detail view of utme applicants with jamb No {id}");

        //    return PartialView(oLevelSubjects);
        //}


        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> CheckUtmeAdmission()
        {
            SchoolProgramme schoolProgramme = await CheckUtmeScrenningAvailability();
            if (schoolProgramme != null)
            {
                ViewBag.Status = "true";
            }
            else
            {
                ViewBag.Status = "false";
            }
            ViewData.Add("ActionMessage", "Check admission View");
            return View();
        }

       

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> CheckUtmeAdmission(string JambRegNo)
        {
            //SchoolProgramme schoolProgramme = await CheckUtmeScrenningAvailability();
            if (JambRegNo != null)
            {
                ViewBag.Status = "true";
            }
            else
            {
                ViewBag.Status = "false";
            }
            if (!string.IsNullOrEmpty(JambRegNo))
            {              
                var utmeStudent = await _db.UtmeApplicants.Include(i => i.Programme).Include(i => i.Session).AsNoTracking()
                                    .Where(x => x.JambRegNo.ToUpper().Equals(JambRegNo.Trim().ToUpper()))
                                    .FirstOrDefaultAsync();

                if (utmeStudent != null)
                {
                    SchoolProgramme schoolProgramme = await CheckUtmeScrenningAvailability(utmeStudent.SessionId);

                    int jambScore = Convert.ToInt16(utmeStudent.ResultGrade);

                    var cutOff = _db.UtmeScreeningCutOffs.Include(i => i.Programme).Include(i => i.Session)
                              .AsNoTracking().FirstOrDefault(s => s.Session.SessionId.Equals(utmeStudent.SessionId)
                              && s.Programme.ProgrammeId.Equals(utmeStudent.Programme.ProgrammeId));
                    if (schoolProgramme != null && schoolProgramme.SessionId != null && (int)schoolProgramme.SessionId == utmeStudent.SessionId)
                    {
                        if (utmeStudent.IsDirectEntry.Equals(true))
                        {
                            if (!utmeStudent.HasRegistered.Equals(true))
                            {
                                var model = new SignUpUtmeViewModel()
                                {
                                    JambRegNo = utmeStudent.JambRegNo,
                                    FirstChoice = utmeStudent.Programme.ProgrammeName,
                                    FirstName = utmeStudent.FirstName,
                                    LastName = utmeStudent.Surname,
                                    SessionId = utmeStudent.SessionId
                                };
                                ViewData.Add("ActionMessage", "");
                                return RedirectToAction("RegisterUtmeApplicant", "Account", model);
                            }
                            ViewBag.Message = $"You have registered before with ( {utmeStudent.Email} ) before." +
                                          $" Please confirm your email or reset password to continue";
                        }
                        else if (cutOff != null && jambScore >= cutOff.CutOffMark)
                        {
                            if (!utmeStudent.HasRegistered.Equals(true))
                            {
                                var model = new SignUpUtmeViewModel()
                                {
                                    JambRegNo = utmeStudent.JambRegNo,
                                    FirstChoice = utmeStudent.Programme.ProgrammeName,
                                    FirstName = utmeStudent.FirstName,
                                    LastName = utmeStudent.Surname,
                                    SessionId = utmeStudent.SessionId
                                };
                                ViewData.Add("ActionMessage", "");
                                return RedirectToAction("RegisterUtmeApplicant", "Account", model);
                            }
                            ViewBag.Message = $"You have registered before with ( {utmeStudent.Email} ) before." +
                                          $" Please confirm your email or reset password to continue";
                        }
                        else
                        {
                            ViewBag.Message = $"Your JAMB score is less than the required {cutOff.CutOffMark} CutOff for {utmeStudent.Programme.ProgrammeName}," +
                                $"You may wish to change your Course. Visit the website to see the various courses cutoff";
                        }
                    }
                    else
                    {
                        ViewBag.Message = $"UTME Screening is not available for this session {utmeStudent.Session.SessionName}. " +
                            $"Please contact the school support desk for more information.";
                    }


                    return View();

                }
                ViewBag.Message = $"Your JAMB No ({JambRegNo}) cannot be found at the moment, either your JAMB Score is less than 180 " +
                                          $"or your change of Institution from JAMB office has not been effected yet. Please try again later";
                return View();
            }
            ViewBag.Message = $"JAMB Registration Number Cannot be empty; Please enter your JAMB Registration Number No and Try Again...";
            return View();
        }


        // GET: UtmeApplicants/Details/5
        public async Task<ActionResult> Details(string id)
        {
            if (id == null)
            {
                id = userId;
            }
            var utmeApplicants = await _db.UtmeApplicants.Include(i => i.Programme).AsNoTracking()
                                        .Where(x => x.Email.Equals(id)).ToListAsync();
            var utmeApplicant = utmeApplicants.LastOrDefault();
            var olevelResult = await _db.ApplicantOLevelResults.Include(i => i.Subject).AsNoTracking()
                                        .Where(x => x.ApplicantId.Equals(id)).ToListAsync();
            if (utmeApplicant == null)
            {
                return HttpNotFound();
            }
            var model = new UtmeApplicantDetailVm()
            {
                UtmeApplicant = utmeApplicant,
                ApplicantOlevelResult = olevelResult,
                Qualification = _db.Qualifications.AsNoTracking().Where(x => x.UserId.Equals(id)).FirstOrDefault()
            };
            return View(model);
        }

        public async Task<ActionResult> PrintDetails(string id)
        {
            if (id == null)
            {
                id = userId;
            }
            var utmeApplicants = await _db.UtmeApplicants.Include(i => i.Programme).AsNoTracking()
                                        .Where(x => x.Email.Trim().ToUpper().Equals(id.Trim().ToUpper())).ToListAsync();
            var utmeApplicant = utmeApplicants.LastOrDefault();
            var olevelResult = await _db.ApplicantOLevelResults.Include(i => i.Subject).AsNoTracking()
                                        .Where(x => x.ApplicantId.Trim().ToUpper().Equals(id.Trim().ToUpper())).ToListAsync();
            var payment = await _db.ApplicantPayments.AsNoTracking()
                                       .Where(x => x.ApplicantEmail.Trim().ToUpper().Equals(id.Trim().ToUpper())).FirstOrDefaultAsync();
            if (utmeApplicant == null)
            {
                return HttpNotFound();
            }
            var model = new UtmeApplicantDetailVm()
            {
                UtmeApplicant = utmeApplicant,
                ApplicantOlevelResult = olevelResult,
                ApplicantPayment = payment,
                Qualification = _db.Qualifications.AsNoTracking().Where(x => x.UserId.Trim().ToUpper().Equals(id.Trim().ToUpper())).FirstOrDefault()

            };
            //return new (model);
            return new ViewAsPdf(model);
        }

        public async Task<ActionResult> RemoveDuplicate(string email, string message)
        {
            var Qualification = await _db.Qualifications.AsNoTracking().Where(x => x.UserId.Trim().ToUpper().Equals(email.Trim().ToUpper())).FirstOrDefaultAsync();

            if (Qualification != null)
            {

                Qualification.Discipline = message;
                _db.Entry(Qualification).State = EntityState.Modified;
                

                await _db.SaveChangesAsync();
            }

            return Json("succes", JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> GetUtmeApplicant()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> DoGetUtmeApplicant(string JambRegNo)
        {
            var id = await _db.UtmeApplicants.Where(x => x.JambRegNo.Trim().ToUpper().Equals(JambRegNo.Trim().ToUpper())).Select(x => x.Email).FirstOrDefaultAsync();

            var utmeApplicants = await _db.UtmeApplicants.Include(i => i.Programme).AsNoTracking()
                                        .Where(x => x.Email.Equals(id)).ToListAsync();
            var utmeApplicant = utmeApplicants.LastOrDefault();
            var olevelResult = await _db.ApplicantOLevelResults.Include(i => i.Subject).AsNoTracking()
                                        .Where(x => x.ApplicantId.Equals(id)).ToListAsync();
            var payment = await _db.ApplicantPayments.AsNoTracking()
                                       .Where(x => x.ApplicantEmail.Equals(id)).FirstOrDefaultAsync();
            if (utmeApplicant == null)
            {
                return HttpNotFound();
            }
            var model = new UtmeApplicantDetailVm()
            {
                UtmeApplicant = utmeApplicant,
                ApplicantOlevelResult = olevelResult,
                ApplicantPayment = payment,
                Qualification = _db.Qualifications.AsNoTracking().Where(x => x.UserId.Trim().ToUpper().Equals(id.Trim().ToUpper())).FirstOrDefault()

            };
            //return new (model);
            return new ViewAsPdf(model);
        }


            // GET: UtmeApplicants/Create
            public ActionResult Create()
        {
            var mystates = from State s in Enum.GetValues(typeof(State))
                           select new { ID = s, Name = s.ToString() };

            var mygender = from Gender s in Enum.GetValues(typeof(Gender))
                           select new { ID = s, Name = s.ToString() };
            var studentType = from StudentType s in Enum.GetValues(typeof(StudentType))
                              select new { ID = s, Name = s.ToString() };

            var studentStaus = from StudentStatus s in Enum.GetValues(typeof(StudentStatus))
                               select new { ID = s, Name = s.ToString() };
            var lga = from LgaEnum s in Enum.GetValues(typeof(LgaEnum))
                      select new { ID = s, Name = s.ToString() };

            ViewBag.LocalGovtArea = new SelectList(lga, "Name", "Name");
            ViewBag.Gender = new SelectList(mygender, "Name", "Name");
            ViewBag.StateOfOrigin = new SelectList(mystates, "Name", "Name");
            ViewBag.ProgrammeId = new SelectList(_db.Programmes.AsNoTracking(), "ProgrammeId", "ProgrammeName");
            ViewBag.SessionId = new SelectList(_db.Sessions.AsNoTracking(), "SessionId", "SessionName");

            return View();
        }

        // POST: UtmeApplicants/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<ActionResult> Create(UtmeApplicant utmeApplicant)
        {
            var schoolProgramme = await _query.GetUndergraduateCurrentSession();

            if (ModelState.IsValid && schoolProgramme != null)
            {
                utmeApplicant.SchoolProgrammeId = schoolProgramme.SchoolProgrammeId;
                _db.UtmeApplicants.Add(utmeApplicant);
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            var mystates = from State s in Enum.GetValues(typeof(State))
                           select new { ID = s, Name = s.ToString() };

            var mygender = from Gender s in Enum.GetValues(typeof(Gender))
                           select new { ID = s, Name = s.ToString() };
            var studentType = from StudentType s in Enum.GetValues(typeof(StudentType))
                              select new { ID = s, Name = s.ToString() };

            var studentStaus = from StudentStatus s in Enum.GetValues(typeof(StudentStatus))
                               select new { ID = s, Name = s.ToString() };
            var lga = from LgaEnum s in Enum.GetValues(typeof(LgaEnum))
                      select new { ID = s, Name = s.ToString() };

            ViewBag.LocalGovtArea = new SelectList(lga, "Name", "Name");
            ViewBag.Gender = new SelectList(mygender, "Name", "Name");
            ViewBag.StateOfOrigin = new SelectList(mystates, "Name", "Name");
            ViewBag.ProgrammeId = new SelectList(_db.Programmes.AsNoTracking(), "ProgrammeId", "ProgrammeName");
            ViewBag.Message = $"School Programmme for Undergraduate Full Time is not available at the moment.";
            return View(utmeApplicant);
        }

        // GET: UtmeApplicants/Edit/5
        public ActionResult Edit(string id)
        {
            var result = ConfirmApplicationFee();
            if (result != null)
                return result;

            if (id == null)
            {
                id = userId;
            }
            var utmeApplicants = _db.UtmeApplicants.AsNoTracking()
                                            .Where(x => x.Email.Trim().ToUpper().Equals(id.Trim().ToUpper())).ToList();
            if (utmeApplicants.LastOrDefault() == null)
            {
                return HttpNotFound();
            }

            // Return an error page if this function is hit directly to evade the constraint of editing screening results only once
            if (utmeApplicants.LastOrDefault().OlevelScore != null)
            {
                string errorMessage = "<div style='color: red; font-family: Arial, sans-serif; font-size: 24px; padding: 100px; background-color: #ffecee; text-align: center;'>You can no longer make changes to your screening form</div>";
                return Content(errorMessage, "text/html");
            }

            var mystates = from State s in Enum.GetValues(typeof(State))
                           select new { ID = s, Name = s.ToString() };

            var mygender = from Gender s in Enum.GetValues(typeof(Gender))
                           select new { ID = s, Name = s.ToString() };
            var studentType = from StudentType s in Enum.GetValues(typeof(StudentType))
                              select new { ID = s, Name = s.ToString() };

            var studentStaus = from StudentStatus s in Enum.GetValues(typeof(StudentStatus))
                               select new { ID = s, Name = s.ToString() };
            var lga = from LgaEnum s in Enum.GetValues(typeof(LgaEnum))
                      select new { ID = s, Name = s.ToString() };

            ViewBag.LocalGovtArea = new SelectList(lga, "Name", "Name");
            ViewBag.Gender = new SelectList(mygender, "Name", "Name");
            ViewBag.StateOfOrigin = new SelectList(mystates, "Name", "Name");
            ViewBag.ProgrammeId = new SelectList(_db.Programmes.AsNoTracking(), "ProgrammeId", "ProgrammeName");
            return View(utmeApplicants.LastOrDefault());
        }

        // POST: UtmeApplicants/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(UtmeApplicant model)
        {
            if (ModelState.IsValid)
            {
                var utmeApplicant = await _db.UtmeApplicants.FindAsync(model.JambRegNo);
                if (utmeApplicant != null)
                {
                    utmeApplicant.Passport = model.Passport;
                    _db.Entry(utmeApplicant).State = EntityState.Modified;
                }

                await _db.SaveChangesAsync();
                return RedirectToAction("Create", "ApplicantOLevelResults");
            }
            var mystates = from State s in Enum.GetValues(typeof(State))
                           select new { ID = s, Name = s.ToString() };

            var mygender = from Gender s in Enum.GetValues(typeof(Gender))
                           select new { ID = s, Name = s.ToString() };
            var studentType = from StudentType s in Enum.GetValues(typeof(StudentType))
                              select new { ID = s, Name = s.ToString() };

            var studentStaus = from StudentStatus s in Enum.GetValues(typeof(StudentStatus))
                               select new { ID = s, Name = s.ToString() };
            var lga = from LgaEnum s in Enum.GetValues(typeof(LgaEnum))
                      select new { ID = s, Name = s.ToString() };

            ViewBag.LocalGovtArea = new SelectList(lga, "Name", "Name");
            ViewBag.Gender = new SelectList(mygender, "Name", "Name");
            ViewBag.StateOfOrigin = new SelectList(mystates, "Name", "Name");
            ViewBag.ProgrammeId = new SelectList(_db.Programmes.AsNoTracking(), "ProgrammeId", "ProgrammeName");
            return View(model);
        }

        // GET: UtmeApplicants/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UtmeApplicant utmeApplicant = await _db.UtmeApplicants.FindAsync(id);
            if (utmeApplicant == null)
            {
                return HttpNotFound();
            }
            return View(utmeApplicant);
        }

        // POST: UtmeApplicants/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            UtmeApplicant utmeApplicant = await _db.UtmeApplicants.FindAsync(id);
            _db.UtmeApplicants.Remove(utmeApplicant);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }


        [AllowAnonymous]
        public async Task<ActionResult> RenderImage(string userId)
        {
            var utmeApplicants = await _db.UtmeApplicants.AsNoTracking()
                                .Where(x => x.Email.Equals(userId)).ToListAsync();
            var utmeApplicant = utmeApplicants.LastOrDefault();
            byte[] result = null;
            byte[] photoBack = null;

            //if (utmeApplicant != null && utmeApplicant.Passport == null)
            //{
            //    result = _query.MapUtmeDePicture(utmeApplicant.JambRegNo);
            //    photoBack = result;

            //}
            //else
            //{
            //    photoBack = utmeApplicant.Passport;

            //}

            if (utmeApplicant != null)
            {
                //photoBack = result;
                photoBack = utmeApplicant.Passport;
            }

            return File(photoBack, "image/png");
        }

        public PartialViewResult UploadUtmeApplicants()
        {
            return PartialView();
        }


        [HttpPost]
        public async Task<ActionResult> UploadUtmeApplicants(HttpPostedFileBase excelfile)
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

                // Read data from excel file
                using (var package = new ExcelPackage(file.InputStream))
                {
                    ExcelValidation myExcel = new ExcelValidation();
                    var currentSheet = package.Workbook.Worksheets;
                    var workSheet = currentSheet.First();
                    var noOfCol = workSheet.Dimension.End.Column;
                    var noOfRow = workSheet.Dimension.End.Row;
                    int requiredField = 12;

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

                    var schoolProgramme = await _query.GetUndergraduateCurrentSession();

                    for (int row = 2; row <= noOfRow; row++)
                    {
                        var jambRegNo = workSheet.Cells[row, 1].Value.ToString().Trim();
                        var surname = workSheet.Cells[row, 2].Value.ToString().Trim();
                        var firstName = workSheet.Cells[row, 3].Value.ToString().Trim();
                        var middleName = workSheet.Cells[row, 4].Value.ToString().Trim();
                        var stateOfOrigin = workSheet.Cells[row, 5].Value.ToString().Trim();
                        var lga = workSheet.Cells[row, 6].Value.ToString().Trim();
                        var gender = workSheet.Cells[row, 7].Value.ToString().Trim();
                        var age = Convert.ToDateTime(workSheet.Cells[row, 8].Value.ToString().Trim());
                        var resultGrade = workSheet.Cells[row, 9].Value.ToString().Trim();
                        var deptOptionCode = workSheet.Cells[row, 10].Value.ToString().Trim();
                        var isDirect = workSheet.Cells[row, 11].Value.ToString().Trim();
                        var sessionName = workSheet.Cells[row, 12].Value.ToString().Trim();
                        bool isDirectEntry = false;

                        int programmeId = await _db.Programmes.AsNoTracking()
                                            .Where(x => x.ProgrammeCode.ToUpper().Equals(deptOptionCode.ToUpper()))
                                            .Select(x => x.ProgrammeId).FirstOrDefaultAsync();

                        if (programmeId < 1)
                        {
                            ViewBag.ErrorInfo = "Please Leave no column or row Empty/Blank";
                            ViewBag.ErrorMessage = $" The department Option \"{deptOptionCode}\" at row {row}  specified in the excel doesn't exist on the portal. " +
                                                   $"Please add the Department Option first before uploading or correct the excel sheet...";
                            return View("ErrorException");
                        }
                        var uploadedSession = await _db.Sessions.AsNoTracking().Where(x => x.SessionName.ToUpper().Equals(sessionName.ToUpper()))
                                                .FirstOrDefaultAsync();
                        if (uploadedSession == null)
                        {
                            ViewBag.ErrorInfo = "Please Leave no column or row Empty/Blank";
                            ViewBag.ErrorMessage = $" The Session Name \"{sessionName}\"  specified in the excel at row {row} doesn't exist on the portal. " +
                                                   $"Please add the Session Name first before uploading or correct the excel sheet...";
                            return View("ErrorException");
                        }

                        if (schoolProgramme == null)
                        {
                            ViewBag.ErrorInfo = "Please Leave no column or row Empty/Blank";
                            ViewBag.ErrorMessage = $"School Programmme for Undergraduate Full Time is not available at the moment.";
                            return View("ErrorException");
                        }

                        if (gender.ToUpper().Equals("M") || gender.ToUpper().Equals("MALE"))
                        {
                            gender = "Male";
                        }
                        else if (gender.ToUpper().Equals("F") || gender.ToUpper().Equals("FEMALE"))
                        {
                            gender = "Female";
                        }
                        else
                        {
                            ViewBag.ErrorInfo = "Gender supported is \"Male\"  and \"Female\" ";
                            ViewBag.ErrorMessage = $"Please check \"{gender}\" at row {row}  Gender type spelling very well ";
                            return View("ErrorException");
                        }

                        if (isDirect.ToUpper().Equals("1"))
                        {
                            isDirectEntry = false;
                        }
                        else if (isDirect.ToUpper().Equals("2"))
                        {
                            isDirectEntry = true;
                        }
                        else if (isDirect.ToUpper().Equals("TRUE") || isDirect.ToUpper().Equals("T"))
                        {
                            isDirectEntry = true;
                        }
                        else if (isDirect.ToUpper().Equals("FALSE") || isDirect.ToUpper().Equals("F"))
                        {
                            isDirectEntry = false;
                        }
                        else
                        {
                            ViewBag.ErrorInfo = "Is Direct Entry Supported is \"1\" for false, \"2\" for true, \"true\", and \"false\"  ";
                            ViewBag.ErrorMessage = $"Please check \"{isDirect}\" at row {row} the Direct entry type spelling very well ";
                            return View("ErrorException");
                        }
                        if (middleName.Equals("."))
                        {
                            middleName = "";
                        }

                        var checkApplicant = _db.UtmeApplicants.Where(x => x.JambRegNo.Equals(jambRegNo)).FirstOrDefault();
                        try
                        {
                            var utmeApplicant = new UtmeApplicant()
                            {
                                JambRegNo = jambRegNo,
                                Surname = surname,
                                FirstName = firstName,
                                MiddleName = middleName != null ? middleName : "" ,
                                Gender = gender,
                                StateOfOrigin = stateOfOrigin,
                                LocalGovtArea = lga,
                                DateOfBirth = age,
                                ResultGrade = resultGrade,
                                ProgrammeId = programmeId,
                                IsDirectEntry = isDirectEntry,
                                SessionId = uploadedSession.SessionId,
                                SchoolProgrammeId = schoolProgramme.SchoolProgrammeId,
                                Age = 0
                            };
                            if (checkApplicant == null)
                            {
                                _db.UtmeApplicants.Add(utmeApplicant);
                                recordCount++;
                                lastrecord = $"The last Updated record has the Surname  {surname} and First Name {firstName} with Jamb Reg No {jambRegNo}";
                            }
                            
                        }
                        catch (Exception ex)
                        {
                            ViewBag.ErrorMessage = ex.Message;
                            return View("ErrorException");
                        }
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
                    message = $"You have successfully Uploaded {recordCount} records...  and {lastrecord}";
                    ViewBag.Message = message;

                }
                return RedirectToAction("Index", "UtmeApplicants", new { message });
            }

            ViewBag.Error = $"File type is Incorrect <br/>";
            return View("Index");
        }

        public async Task<ViewResult> UploadUtmeApplicantsDoB()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> UploadUtmeApplicantsDoB(HttpPostedFileBase excelfile)
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

                // Read data from excel file
                using (var package = new ExcelPackage(file.InputStream))
                {
                    ExcelValidation myExcel = new ExcelValidation();
                    var currentSheet = package.Workbook.Worksheets;
                    var workSheet = currentSheet.First();
                    var noOfCol = workSheet.Dimension.End.Column;
                    var noOfRow = workSheet.Dimension.End.Row;
                    int requiredField = 2;

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

                    var schoolProgramme = await _query.GetUndergraduateCurrentSession();

                    for (int row = 2; row <= noOfRow; row++)
                    {
                        var jambRegNo = workSheet.Cells[row, 1].Value.ToString().Trim();
                        
                        var DoB = Convert.ToDateTime(workSheet.Cells[row, 2].Value.ToString().Trim());
                        
                                          

                        try
                        {
                            var utmeApplicant = await _db.Students.Where(x => x.JambRegNo.ToUpper().Equals(jambRegNo)).FirstOrDefaultAsync();
                            if (utmeApplicant !=null )
                            {
                                utmeApplicant.DateOfBirth = DoB;
                                _db.Entry(utmeApplicant).State = EntityState.Modified;
                                recordCount++;
                                lastrecord = $"The last Updated record has the Surname  {utmeApplicant.LastName} and First Name {utmeApplicant.FirstName} with Jamb Reg No {jambRegNo}";
                            }
                           
                        }
                        catch (Exception ex)
                        {
                            ViewBag.ErrorMessage = ex.Message;
                            return View("ErrorException");
                        }
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
                    message = $"You have successfully Uploaded {recordCount} records...  and {lastrecord}";
                    ViewBag.Message = message;

                }
                return RedirectToAction("Index", "UtmeApplicants", new { message });
            }

            ViewBag.Error = $"File type is Incorrect <br/>";
            return View("Index");
        }

        public PartialViewResult ChangeOfCourseUtmeApplicants()
        {
            return PartialView();
        }


        [HttpPost]
        public async Task<ActionResult> ChangeOfCourseUtmeApplicants(HttpPostedFileBase excelfile)
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

                // Read data from excel file
                using (var package = new ExcelPackage(file.InputStream))
                {
                    ExcelValidation myExcel = new ExcelValidation();
                    var currentSheet = package.Workbook.Worksheets;
                    var workSheet = currentSheet.First();
                    var noOfCol = workSheet.Dimension.End.Column;
                    var noOfRow = workSheet.Dimension.End.Row;
                    int requiredField = 2;

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

                    var utmeUploadError = new List<StudentUtmeUploadErrorVm>();
                    bool hasUtmeUploadError = false;

                    for (int row = 2; row <= noOfRow; row++)
                    {
                        var jambRegNo = workSheet.Cells[row, 1].Value.ToString().Trim();
                        var programmeCode = workSheet.Cells[row, 2].Value.ToString().Trim();

                        int programmeId = await _db.Programmes.AsNoTracking()
                                            .Where(x => x.ProgrammeCode.Trim().ToUpper().Equals(programmeCode.ToUpper()))
                                            .Select(x => x.ProgrammeId).FirstOrDefaultAsync();

                        if (programmeId < 1)
                        {
                            ViewBag.ErrorInfo = "Please Leave no column or row Empty/Blank";
                            ViewBag.ErrorMessage = $" The department Option \"{programmeCode}\" at row {row}  specified in the excel doesn't exist on the portal. " +
                                                   $"Please add the Department Option first before uploading or correct the excel sheet...";
                            return View("ErrorException");
                        }
                        var utmeApplicant = await _db.UtmeApplicants.AsNoTracking()
                                            .Where(x => x.JambRegNo.Trim().ToUpper().Equals(jambRegNo.ToUpper()))
                                            .FirstOrDefaultAsync();
                        //string body = $"Dear {utmeApplicant.FirstName}, your change of course is successful; you can proceed with " +
                        //       $"your screening exercise on the unijos portal";

                        if (utmeApplicant != null)
                        {
                            utmeApplicant.ProgrammeId = programmeId;
                            _db.Entry(utmeApplicant).State = EntityState.Modified;
                            recordCount++;
                            //await SMSClass.SendSMS("UNIJOS SIS", body, utmeApplicant.PhoneNumber);
                        }
                        else
                        {
                            utmeUploadError.Add(new StudentUtmeUploadErrorVm { JambRegNo = jambRegNo, Row = row, Message = "Jamb Reg No cannot be found" });
                            hasUtmeUploadError = true;
                        }
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
                        ViewBag.ErrorInfo = $"These applicants wasn't found on the system";
                        ViewBag.ErrorMessage = $"You have successfully Uploaded {recordCount} records...";
                        return View("ErrorException", utmeUploadError);
                    }
                    message = $"You have successfully Uploaded {recordCount} records...  and {lastrecord}";
                    return RedirectToAction("Index", "UtmeApplicants", new { message });
                }
            }
            ViewBag.Error = $"File type is Incorrect <br/>";
            return View("Index");
        }



        [Authorize(Roles = RoleName.Admin + "," + RoleName.DatabaseAdmin + "," + RoleName.SuperAdmin)]
        public ActionResult UpdateUtmeSessionName(string message)
        {
            ViewBag.Message = message;
            return View();
        }

        [Authorize(Roles = RoleName.Admin + "," + RoleName.DatabaseAdmin + "," + RoleName.SuperAdmin)]
        [HttpPost]
        public async Task<ActionResult> UpdateUtmeSessionName(HttpPostedFileBase excelfile)
        {
            if (excelfile == null || excelfile.ContentLength == 0)
            {
                ViewBag.Error = "Please Select a excel file <br/>";
                TempData["UserMessage"] = "Please Select a excel file.";
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

                // Read data from excel file
                using (var package = new ExcelPackage(file.InputStream))
                {
                    ExcelValidation myExcel = new ExcelValidation();
                    var currentSheet = package.Workbook.Worksheets;
                    var workSheet = currentSheet.First();
                    var noOfCol = workSheet.Dimension.End.Column;
                    var noOfRow = workSheet.Dimension.End.Row;
                    int requiredField = 2;

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

                    var utmeUploadError = new List<StudentUtmeUploadErrorVm>();
                    bool hasUtmeUploadError = false;

                    for (int row = 2; row <= noOfRow; row++)
                    {
                        var jambRegNo = workSheet.Cells[row, 1].Value.ToString().Trim();
                        var sessionName = workSheet.Cells[row, 2].Value.ToString().Trim();

                        int sessionId = await _db.Sessions.AsNoTracking()
                                            .Where(x => x.SessionName.Trim().ToUpper().Equals(sessionName.ToUpper()))
                                            .Select(x => x.SessionId).FirstOrDefaultAsync();

                        if (sessionId < 1)
                        {
                            ViewBag.ErrorInfo = "Please Leave no column or row Empty/Blank";
                            ViewBag.ErrorMessage = $" The Session Name \"{sessionName}\" at row {row}  specified in the excel doesn't exist on the portal. " +
                                                   $"Please add the Session Name first before uploading or correct the excel sheet...";
                            return View("ErrorException");
                        }
                        var utmeApplicant = await _db.UtmeApplicants.AsNoTracking()
                                            .Where(x => x.JambRegNo.Trim().ToUpper().Equals(jambRegNo.ToUpper()))
                                            .FirstOrDefaultAsync();

                        if (utmeApplicant != null)
                        {
                            utmeApplicant.SessionId = sessionId;
                            _db.Entry(utmeApplicant).State = EntityState.Modified;
                            recordCount++;
                        }
                        else
                        {
                            utmeUploadError.Add(new StudentUtmeUploadErrorVm { JambRegNo = jambRegNo, Row = row, Message = "Jamb Reg No cannot be found" });
                            hasUtmeUploadError = true;
                        }
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
                        ViewBag.ErrorInfo = $"These applicants wasn't found on the system";
                        ViewBag.ErrorMessage = $"You have successfully Uploaded {recordCount} records...";
                        return View("ErrorException", utmeUploadError);
                    }
                    message = $"You have successfully Uploaded {recordCount} records...  and {lastrecord}";
                    return RedirectToAction("Index", "UtmeApplicants", new { message });
                }
            }

            ViewBag.Error = $"File type is Incorrect <br/>";
            return View("Index");
        }



        //public async Task<ActionResult> MapUtmeDePicture()
        //{
        //    int processedCount = 0;
        //    DirectoryInfo dirInfo = new DirectoryInfo(HostingEnvironment.MapPath("~/UtmeDePictures"));
        //    var files = dirInfo.GetFiles().ToList();
        //    foreach (var file in files)
        //    {
        //        var jambReg = file.Name.Replace("_Face", "").Trim();
        //        jambReg = jambReg.Replace(".jpg", "");
        //        var jambApplicant = _db.UtmeApplicants.Where(x => x.JambRegNo.ToUpper().Trim().Equals(jambReg.ToUpper())).FirstOrDefault();
        //        if (jambApplicant != null && jambApplicant.Passport == null)
        //        {
        //            System.Drawing.Image img = System.Drawing.Image.FromFile(file.FullName);
        //            using (MemoryStream ms = new MemoryStream())
        //            {
        //                img.Save(ms, ImageFormat.Jpeg);
        //                jambApplicant.Passport = ms.ToArray();
        //                _db.Entry(jambApplicant).State = EntityState.Modified;
        //                processedCount += 1;
        //            }
        //        }
        //    }
        //    await _db.SaveChangesAsync();

        //    ViewBag.Message = $"You have successfully Map {processedCount} for ({processedCount}) Student(s)";
        //    return View();
        //}

        public async Task<ActionResult> MapUtmeDePicture(int facID)
        {
            int processedCount = 0;
            DirectoryInfo dirInfo = new DirectoryInfo(HostingEnvironment.MapPath("~/UtmeDePictures"));
            var files = dirInfo.GetFiles().ToList(); 
            var applicants = _db.Students.Include(x => x.Programme.Department.Faculty)
                                               .Include(x => x.Session)
                                               .Where(x => x.Session.SessionId.Equals(29) || x.Session.SessionId.Equals(29) && x.Passport == null ).ToList();

            foreach (var applicant in applicants)
            {
                //var jambReg = file.Name.Replace("_Face", "").Trim();
                var jambReg = applicant.JambRegNo.Trim();
                var filesInDir = dirInfo.GetFiles("*" + jambReg + "*.*");

                //var jambApplicant = _db.UtmeApplicants.Where(x => x.JambRegNo.ToUpper().Trim().Equals(jambReg.ToUpper())).FirstOrDefault();
                foreach (var item in filesInDir)
                {
                    if (applicant != null && applicant.Passport == null)
                    {
                        System.Drawing.Image img = System.Drawing.Image.FromFile(item.FullName);
                        using (MemoryStream ms = new MemoryStream())
                        {
                            img.Save(ms, ImageFormat.Jpeg);
                            applicant.Passport = ms.ToArray();
                            _db.Entry(applicant).State = EntityState.Modified;
                            processedCount += 1;
                        }
                    }
                }
            }
            await _db.SaveChangesAsync();

            ViewBag.Message = $"You have successfully Map {processedCount} for ({processedCount}) Student(s)";
            return View();
        }

        //public async Task<ActionResult> TempChangeUtmeApplicantEmail(string jambReg, string email)
        //{
        //    var utmeStudent = await _db.UtmeApplicants.Include(i => i.Programme).Include(i => i.Session)
        //                           .Where(x => x.JambRegNo.ToUpper().Equals(jambReg.Trim().ToUpper()))
        //                           .FirstOrDefaultAsync();

        //    if (utmeStudent !=null)
        //    {
        //        utmeStudent.Email = null;
        //        _db.Entry(utmeStudent).State = EntityState.Modified;
        //        await _db.SaveChangesAsync();

        //        var model = new SignUpUtmeViewModel()
        //        {
        //            JambRegNo = utmeStudent.JambRegNo,
        //            FirstChoice = utmeStudent.Programme.ProgrammeName,
        //            FirstName = utmeStudent.FirstName,
        //            LastName = utmeStudent.Surname,
        //            SessionId = utmeStudent.SessionId
        //        };
        //        ViewData.Add("ActionMessage", "");
        //        return RedirectToAction("RegisterUtmeApplicant", "Account", model);
        //    }
        //    return Json("Success!", JsonRequestBehavior.AllowGet);
        //}

        // Fixing issue with utme applicants with wrong school programme
        public async Task<ActionResult> UtmeApplicantFix()
        {
            var applicants = await _db.UtmeApplicants.Include(x => x.Session)
                                .Where(x => x.SchoolProgrammeId != 1)
                                .Where(x => x.Session.SessionName == "2023/2024" || x.Session.SessionName == "2024/2025")
                                .ToListAsync();

            foreach (var applicant in applicants)
            {
                applicant.SchoolProgrammeId = 1;
                _db.Entry(applicant).State = EntityState.Modified;
            }
            await _db.SaveChangesAsync();


            return Json("success");
        }

        [HttpGet]
        [Authorize(Roles = RoleName.Admin + "," + RoleName.DatabaseAdmin + "," + RoleName.SuperAdmin + "," + RoleName.AdmissionUploadOfficer)]
        public ActionResult DeleteUtmeAdmission(string message)
        {
            ViewBag.Message = message;
            return View();
        }

        [Authorize(Roles = RoleName.Admin + "," + RoleName.DatabaseAdmin + "," + RoleName.SuperAdmin + "," + RoleName.AdmissionUploadOfficer)]
        public async Task<ActionResult> DeleteUtmeAdmission(HttpPostedFileBase excelfile)
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
                    int requiredField = 2;

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
                        var jambNo = workSheet.Cells[row, 1].Value.ToString().Trim();
                        var progId = Convert.ToInt32(workSheet.Cells[row, 2].Value);



                        var utmeApplicant = await _db.UtmeApplicants
                                            .Where(x => x.JambRegNo.Trim().ToUpper().Equals(jambNo.ToUpper()))
                                            .FirstOrDefaultAsync();

                        if (utmeApplicant != null)
                        {
                            _db.Entry(utmeApplicant).State = EntityState.Deleted;
                            recordCount++;
                        }
                        else
                        {
                            utmeUploadError.Add(new StudentUtmeUploadErrorVm { JambRegNo = jambNo, Row = row, Message = "Jamb Reg No cannot be found" });
                            hasUtmeUploadError = true;
                        }
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
