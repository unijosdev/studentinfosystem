using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampus.ViewModels;
using SwiftKampusModel;
using SwiftKampusModel.AddmissionApplicant;
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
    public class ApplicantOLevelResultsController : BaseController
    {

        public ApplicantOLevelResultsController(SchoolDbContext db) : base(db)
        {

        }

        // GET: ApplicantOLevelResults
        public async Task<ActionResult> Index()
        {
            var result = ConfirmApplicationFee();
            if (result != null)
                return result;

            var applicantOLevelResults = await _db.ApplicantOLevelResults.AsNoTracking().Include(a => a.Subject)
                                           .Where(x => x.ApplicantId.Equals(userId)).ToListAsync();

            return View(applicantOLevelResults);
        }

        public async Task<PartialViewResult> RegIndex()
        {
            var result = ConfirmAcceptanceFee();
            if (result != null)
                return (PartialViewResult)result;

            //ViewBag.ApplicantType = await _db.Students.Where( x=userId);

            var applicantOLevelResults = await _db.ApplicantOLevelResults.AsNoTracking().Include(a => a.Subject)
                                          .Where(x => x.ApplicantId.Equals(userId)).ToListAsync();

            return PartialView(applicantOLevelResults);
        }



        public async Task<ActionResult> GetIndex()
        {
            var schoolProgramme = await _db.ApplicantOLevelResults.AsNoTracking().Include(a => a.Subject)
                .AsNoTracking().ToListAsync();
            var data = schoolProgramme.Select(s => new
            {
                s.ApplicantOLevelResultId,
                s.ResultName,
                s.Subject.CourseName,
                s.SubjectGrade,
                s.ApplicantId

            }).ToList();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        // GET: ApplicantOLevelResults/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicantOLevelResult applicantOLevelResult = await _db.ApplicantOLevelResults.FindAsync(id);
            if (applicantOLevelResult == null)
            {
                return HttpNotFound();
            }
            return View(applicantOLevelResult);
        }

        // GET: ApplicantOLevelResults/Details/5

        // GET: ApplicantOLevelResults/Create
        public async Task<ActionResult> Create()
        {

            var result = ConfirmApplicationFee();
            if (result != null)
                return result;

            var myGrade = from ResultTypeGrade s in Enum.GetValues(typeof(ResultTypeGrade))
                          select new { ID = s, Name = s.ToString() };
            var resultName = from ResultType s in Enum.GetValues(typeof(ResultType))
                             select new { ID = s, Name = s.ToString() };
            ViewBag.ResultName = new SelectList(resultName, "Name", "Name");
            ViewBag.SubjectGrade = new SelectList(myGrade, "Name", "Name");

            var undergraduateRule = await _query.GetUnderGraduateRule(userId);
            ViewBag.SubjectId = new SelectList(undergraduateRule.Where(x => x.IsRequired.Equals(false)).Select(s => s.Subject).ToList(), "SubjectId", "CourseName");

            var model = new ApplicantOLevelResultVm()
            {
                UnderGraduateRules = undergraduateRule.Where(x => x.IsRequired.Equals(true)).ToList(),
            };
            ViewBag.IsDirectEntry = _db.UtmeApplicants.AsNoTracking().Where(x => x.Email.Equals(userId))
                                   .Select(s => s.IsDirectEntry).FirstOrDefault().ToString();

            return View(model);
        }


        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(List<ApplicantOLevelVm> model)
        {
            var result = ConfirmApplicationFee();
            if (result != null)
                return result;

            string message = "";
            double sumGradePoint = 0.0;
            if (ModelState.IsValid)
            {
                var noOfresultType = model.DistinctBy(x => x.ResultName.Trim().ToUpper()).ToList().Count();
                if (noOfresultType > 2)
                {
                    message = $"You are only expected to add two results types";
                    return new JsonResult { Data = new { status = false, message } };
                }
                var modelCount = model.Count;

                if (modelCount < model[1].MaximumCount)
                {
                    var remaning = model[1].MaximumCount - modelCount;
                    message = $"You are expected to add {model[1].MaximumCount}, Please add {remaning} more result(s)";
                    return new JsonResult { Data = new { status = false, message } };
                }
                var oLevelResults = await _db.ApplicantOLevelResults.AsNoTracking()
                                  .Where(x => x.ApplicantId.Trim().ToUpper().Equals(userId.Trim().ToUpper())).ToListAsync();
                foreach (var foundResult in oLevelResults)
                {
                    _db.Entry(foundResult).State = EntityState.Deleted;
                    _db.SaveChanges();
                }

                for (int i = 0; i < model.Count(); i++)
                {
                    var mySubjectId = model[i].SubjectId;
                    var checkResult = _db.ApplicantOLevelResults.AsNoTracking()
                                    .Where(x => x.ApplicantId.Equals(userId)
                                    && x.SubjectId.Equals(mySubjectId)).ToList();
                    if (checkResult.Any() || model.Count(x => x.SubjectId.Equals(mySubjectId)) > 1)
                    {
                        message = "You can't add the same result twice";
                        return new JsonResult { Data = new { status = false, message } };
                    }
                    var gradePoint = await _query.GetAdmissionGradePoint(model[i].SubjectGrades);
                    var olevelresult = new ApplicantOLevelResult
                    {
                        ApplicantId = userId,
                        ResultName = model[i].ResultName.ToString().Trim(),
                        SubjectId = model[i].SubjectId,
                        SubjectGrade = model[i].SubjectGrades,
                        GradePoint = gradePoint
                    };
                    _db.ApplicantOLevelResults.Add(olevelresult);
                    sumGradePoint += gradePoint;
                }

                await SaveUtmeOlevelScore(sumGradePoint);

                await _db.SaveChangesAsync();
                message = $"{modelCount} results is added successfully";
                return new JsonResult { Data = new { status = true, message } };
            }
            message = "Something went wrong... Please complete the result form properly";
            return new JsonResult { Data = new { status = false, message } };
        }

        private async Task SaveUtmeOlevelScore(double sumGradePoint)
        {
            var applicant = await _db.UtmeApplicants.Include(i => i.SchoolProgramme)
                                .Where(x => x.Email.Equals(userId))
                          .FirstOrDefaultAsync();

            var utmeScreeningPolicy = await _db.UtmeScreningPolicies.AsNoTracking()
                                         .Where(x => x.SessionId.Equals(applicant.SessionId))
                                         .FirstOrDefaultAsync();

            int jambScore = Convert.ToInt16(applicant.ResultGrade);

            var jambPercentage = (jambScore * utmeScreeningPolicy.JambPercentage) / utmeScreeningPolicy.JambMaximumScore;
            var olevelPercentage = (sumGradePoint * utmeScreeningPolicy.OLevelPercentage) / utmeScreeningPolicy.OLevelMaximumScore;
            var cummulative = jambPercentage + olevelPercentage;

            applicant.OlevelScore = sumGradePoint;
            applicant.JambPercentage = jambPercentage;
            applicant.OlevelPercentage = olevelPercentage;           
            applicant.Cummulative = cummulative;
            _db.Entry(applicant).State = EntityState.Modified;
        }

        public PartialViewResult DisplaySave()
        {
            return PartialView();
        }

        public PartialViewResult Save(int? id, string IsEdit)
        {
            var applicantOLevelResult = new ApplicantOLevelResult();
            ViewBag.ApplicantType = _applicantType.ApplicantType;
            if (string.IsNullOrEmpty(IsEdit))
            {
                if (_db.ApplicantOLevelResults.Any(x => x.ApplicantId.Equals(userId)))
                {
                    var applicantOLevelResults = _db.ApplicantOLevelResults.AsNoTracking().Include(a => a.Subject)
                                            .Where(x => x.ApplicantId.Equals(userId)).ToList();

                    var stdGrade = from ResultTypeGrade s in Enum.GetValues(typeof(ResultTypeGrade))
                                   select new { ID = s, Name = s.ToString() };
                    var stdResultName = from ResultType s in Enum.GetValues(typeof(ResultType))
                                        select new { ID = s, Name = s.ToString() };
                    ViewBag.StdSubjectId = new SelectList(_db.Subjects.AsNoTracking(), "SubjectId", "CourseName");
                    ViewBag.SubjectGrade = new SelectList(stdGrade, "Name", "Name");

                    ViewBag.ResultName = applicantOLevelResults.DistinctBy(x => x.ResultName).Select(s => s.ResultName);
                    return PartialView("RegIndex", applicantOLevelResults);
                }
            }
            if(id != null)
            {
                applicantOLevelResult = _db.ApplicantOLevelResults.Find((int)id);
            }

            var username = User.Identity.GetUserId();
            var applicant = _db.Applicants.AsNoTracking().Where(c => c.ApplicantId.Equals(username)).ToList();
            var myGrade = from ResultTypeGrade s in Enum.GetValues(typeof(ResultTypeGrade))
                          select new { ID = s, Name = s.ToString() };
            var resultName = from ResultType s in Enum.GetValues(typeof(ResultType))
                             select new { ID = s, Name = s.ToString() };
            ViewBag.ResultName = new SelectList(resultName, "Name", "Name");
            ViewBag.SubjectGrade = new SelectList(myGrade, "Name", "Name");
            
            ViewBag.ApplicantEmail = new SelectList(applicant, "ApplicantEmail", "ApplicantEmail");
            ViewBag.SubjectId = new SelectList(_db.Subjects.AsNoTracking(), "SubjectId", "CourseName");

            return PartialView(applicantOLevelResult);
        }

        [HttpPost]
        public async Task<ActionResult> Save(List<SaveApplicantOLevelVm> modelList)
         {
            var result = ConfirmApplicationFee();
            if (result != null)
                return result;

            string message = "";
            if (ModelState.IsValid)
            {
                var checkResult = _db.ApplicantOLevelResults.AsNoTracking()
                                    .Where(x => x.ApplicantId.Trim().ToUpper().Equals(userId.Trim().ToUpper())).ToList();

                foreach (var item in checkResult)
                {
                    _db.Entry(item).State = EntityState.Deleted;
                }
                _db.SaveChanges();
                foreach (var model in modelList)
                {                  
                    foreach (var item in model.SubjectGrades)
                    {
                        if (model.SubjectGrades.Count(x => x.subject.Equals(item.subject)) > 1)
                        {
                            message = $"You already have this {item.subject} results, You are not expected to add thesame subject twice";
                            return new JsonResult { Data = new { status = false, message } };
                        }
                        var olevelresult = new ApplicantOLevelResult
                        {
                            ApplicantId = userId,
                            ResultName = model.ResultName.ToString().Trim(),
                            ExaminationCenter = model.ExaminationCenter.Trim(),
                            Year = model.Year.Trim(),
                            SubjectId = item.subject,
                            SubjectGrade = item.grade,
                            ExamNumber = model.ExamNumber,
                            GradePoint = await _query.GetAdmissionGradePoint(item.grade)
                        };
                        _db.ApplicantOLevelResults.Add(olevelresult);
                    }
                }               
                _db.SaveChanges();
                message = $"{modelList.Count()} result type  is added successfully";
                return new JsonResult { Data = new { status = true, message } };
            }
            return new JsonResult { Data = new { status = false, message = "Incomplete Data" } };
        }

        // GET: ApplicantOLevelResults/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            var result = ConfirmApplicationFee();
            if (result != null)
                return result;

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicantOLevelResult applicantOLevelResult = await _db.ApplicantOLevelResults.FindAsync(id);
            if (applicantOLevelResult == null)
            {
                return HttpNotFound();
            }
            var username = User.Identity.GetUserId();
            var applicant = await _db.Applicants.AsNoTracking().Where(c => c.ApplicantId.Equals(username)).ToListAsync();
            ViewBag.ApplicantEmail = new SelectList(applicant, "ApplicantEmail", "ApplicantEmail");
            ViewBag.SubjectId = new SelectList(_db.Subjects.AsNoTracking(), "SubjectId", "CourseCode", applicantOLevelResult.SubjectId);
            return View(applicantOLevelResult);
        }

        // POST: ApplicantOLevelResults/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(ApplicantOLevelResult applicantOLevelResult)
        {
            var result = ConfirmApplicationFee();
            if (result != null)
                return result;

            if (ModelState.IsValid)
            {
                _db.Entry(applicantOLevelResult).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                await ValidateApplicant();
                return RedirectToAction("Details", "Applicants");
            }
            var username = User.Identity.GetUserId();
            var applicant = await _db.Applicants.AsNoTracking().Where(c => c.ApplicantId.Equals(username)).ToListAsync();
            ViewBag.ApplicantEmail = new SelectList(applicant, "ApplicantEmail", "ApplicantEmail");
            ViewBag.SubjectId = new SelectList(_db.Subjects.AsNoTracking(), "SubjectId", "CourseCode", applicantOLevelResult.SubjectId);
            return View(applicantOLevelResult);
        }

        // GET: ApplicantOLevelResults/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicantOLevelResult applicantOLevelResult = await _db.ApplicantOLevelResults.FindAsync(id);
            if (applicantOLevelResult == null)
            {
                return HttpNotFound();
            }
            return View(applicantOLevelResult);
        }

        // POST: ApplicantOLevelResults/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            ApplicantOLevelResult applicantOLevelResult = await _db.ApplicantOLevelResults.FindAsync(id);
            if (applicantOLevelResult != null) _db.ApplicantOLevelResults.Remove(applicantOLevelResult);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private async Task ValidateApplicant()
        {

            var username = User.Identity.GetUserId();
            var applicant = _db.Applicants.Include(i => i.AvailableCourse).Include(i => i.SchoolProgramme).AsNoTracking()
                                .FirstOrDefault(c => c.ApplicantId.Equals(username));

            var rule = await _db.UnderGraduateRules.Include(i => i.SchoolProgramme).AsNoTracking()
                        .Where(x => x.ProgrammeId.Equals(applicant.AvailableCourse.ProgrammeId)
                            && x.SchoolProgramme.SchoolProgrammeId.Equals(applicant.SchoolProgramme.SchoolProgrammeId)).ToListAsync();
            int countResult = 0;
            foreach (var item in rule.Where(x => x.IsRequired.Equals(true)))
            {
                var applicantSubject = await _db.ApplicantOLevelResults.AsNoTracking().Where(x => x.ApplicantId.Equals(applicant.ApplicantEmail)
                                        && x.SubjectId.Equals(item.SubjectId)).ToListAsync();
                if (applicantSubject.Any())
                {
                    countResult++;
                }
            }
            foreach (var item in rule.Where(x => x.IsRequired.Equals(false)))
            {
                var applicantSubject = await _db.ApplicantOLevelResults.AsNoTracking().Where(x => x.ApplicantId.Equals(applicant.ApplicantEmail)
                                                        && x.SubjectId.Equals(item.SubjectId)).ToListAsync();
                if (applicantSubject.Any())
                {
                    countResult++;
                }
            }
            //var myApplicant = await _db.Applicants.AsNoTracking()
            //                            .Where(x => x.ApplicantEmail.Equals(applicant.ApplicantEmail))
            //                            .FirstOrDefaultAsync();
            //var noOfSubjectRequired = rule.Select(s => s.NoOfRequiredSubject).FirstOrDefault();
            //if (noOfSubjectRequired >= countResult)
            //{
            //    if (applicant != null) applicant.IsQualified = true;
            //}
            //else
            //{
            //    if (applicant != null) applicant.IsQualified = false;
            //}

            _db.Entry(applicant).State = EntityState.Modified;
            await _db.SaveChangesAsync();

        }

        [HttpPost]
        public async Task<ActionResult> Update(int id, string content, string elementName)
        {
            var applicantOLevelResults = await _db.ApplicantOLevelResults.Where(n => n.ApplicantId.Equals(userId)).ToListAsync();
            if (applicantOLevelResults != null)
            {
                foreach (var applicantOLevelResult in applicantOLevelResults)
                {
                    switch (elementName)
                    {
                        case "resultName":
                            applicantOLevelResult.ResultName = content;
                            break;

                        case "centerName":
                            applicantOLevelResult.ExaminationCenter = content;
                            break;

                        case "year":
                            applicantOLevelResult.Year = content;
                            break;

                        default:
                            applicantOLevelResult.ExamNumber = content;
                            break;
                    }

                    _db.Entry(applicantOLevelResult).State = EntityState.Modified;

                }

                await _db.SaveChangesAsync();
                return Json(new { success = true });
            }

            return Json(new { success = false });
        }

        [HttpPost]
        public async Task<ActionResult> SaveRowData(ApplicantOLevelResult rowData)
        {
            try
            {
                // Here, you can access the rowData.SubjectId and rowData.SubjectGrade
                var checkResult = _db.ApplicantOLevelResults.AsNoTracking()
                                    .Where(x => x.ApplicantId.Equals(userId)
                                    && x.SubjectId.Equals(rowData.SubjectId)
                                    && x.ApplicantOLevelResultId.Equals(rowData.ApplicantOLevelResultId)).ToList();

                if (checkResult.Any())
                {
                    var message = "You can't add the same result twice";
                    return new JsonResult { Data = new { status = false, message } };
                }
                // Perform your data saving logic here, e.g., save to the database
                var ApplicantResult = await _db.ApplicantOLevelResults.FindAsync(rowData.ApplicantOLevelResultId);

                if (string.IsNullOrEmpty(ApplicantResult.ResultName.ToString()) || string.IsNullOrEmpty(ApplicantResult.ExaminationCenter.ToString())
                    || string.IsNullOrEmpty(ApplicantResult.ExamNumber.ToString()) || string.IsNullOrEmpty(ApplicantResult.Year.ToString()))
                {
                    var message = "Please fill out resultname and exam year and center name and exam number for adding subject grade";
                    return new JsonResult { Data = new { status = false, message } };
                }

                var olevelresult = new ApplicantOLevelResult
                {
                    ApplicantId = userId,
                    ResultName = ApplicantResult.ResultName.ToString().Trim(),
                    ExaminationCenter = ApplicantResult.ExaminationCenter.Trim(),
                    Year = ApplicantResult.Year.Trim(),
                    SubjectId = rowData.SubjectId,
                    SubjectGrade = rowData.SubjectGrade,
                    ExamNumber = ApplicantResult.ExamNumber,
                    //GradePoint = _query.GetAdmissionGradePoint(rowData.SubjectGrade)
                };
                _db.ApplicantOLevelResults.Add(olevelresult);

                await _db.SaveChangesAsync();

                // Return a JSON response indicating success
                var response = new { success = true, message = "Data saved successfully." };
                return Json(response);
            }
            catch (Exception ex)
            {
                // Handle exceptions or errors here
                var response = new { success = false, message = ex.Message };
                return Json(response);
            }
        }

        //USED TO REVERT BAD UPDATE ON THE ENGLISH GRADE OF THE DB
        public string UpdateEnglishGrade()
        {

            var applicants = _db.UtmeApplicants.Where(x => x.SessionId == 29 && x.HasRegistered == true && x.OlevelScore != null).ToList();
            int count = 0;

            foreach (var item in applicants)
            {
                // Find the applicant by JambNumber
                var applicant = _db.UtmeApplicants.Include(i => i.SchoolProgramme)
                                    .Where(x => x.JambRegNo.Equals(item.JambRegNo)).FirstOrDefault();

                if (applicant == null)
                {
                    throw new Exception("Applicant not found.");
                }

                // Fetch O-Level grades for this applicant
                var olevelGrades = _db.ApplicantOLevelResults.AsNoTracking()
                                    .Where(x => x.ApplicantId.Trim().ToUpper().Equals(applicant.Email.Trim().ToUpper())).ToList();

                // Fetch grade mapping from AdmissionGrades (assuming points is the mapping for grades like A1 - 8, B2 - 7, etc.)
                var admissionGrades = _db.AdmissionGrades.ToDictionary(g => g.GradeName, g => g.GradePoint);

                int englishSubjectId = 11;  // Assuming subject ID 11 is for English
                double totalFourGrades = 0.0;

                // Calculate the sum of the points for four subjects excluding English
                foreach (var grade in olevelGrades)
                {
                    if (grade.SubjectId != englishSubjectId)
                    {
                        totalFourGrades += admissionGrades[grade.SubjectGrade];
                    }
                }

                // Find the difference between the OlevelScore and the total of the other four grades
                double difference = (Double)applicant.OlevelScore - totalFourGrades;

                // Get possible grade for English from AdmissionGrades that matches the difference
                var possibleEnglishGrade = admissionGrades.FirstOrDefault(x => x.Value == difference).Key;

                // Update the English grade if the grade exists, else default it to 'C6'
                var englishGrade = olevelGrades.FirstOrDefault(g => g.SubjectId == englishSubjectId);
                if (englishGrade != null)
                {
                    englishGrade.SubjectGrade = possibleEnglishGrade ?? "C6";  // If no match, default to C6
                    _db.Entry(englishGrade).State = EntityState.Modified;
                    count++;
                }
            }
            _db.SaveChanges();

            return $" You have successfully processes {count} records!";
        }

        public async Task<ActionResult> getStudentOlevel()
        {
            //ViewBag.SessionId = new SelectList(_db.Sessions.AsNoTracking(), "SessionId", "SessionName");
            //ViewBag.LevelId = new SelectList(_db.Levels.AsNoTracking(), "LevelId", "LevelName");
            return View();
        }

        public async Task<ActionResult> AdminEdit(PreviousCourseRegistrationVm model)
        {
            var applicant = await _db.Students.AsNoTracking().FirstOrDefaultAsync(a => a.Email.ToUpper().Equals(model.MatNum.ToUpper()));

            if (applicant == null)
            {
                ViewBag.Message = "Student with the provided email does not exist.";
                return PartialView("ErrorView");
            }

            var applicantOLevelResults = _db.ApplicantOLevelResults
                .AsNoTracking()
                .Include(r => r.Subject)
                .Where(r => r.ApplicantId == applicant.Email)
                .ToList();

            var grades = from ResultTypeGrade s in Enum.GetValues(typeof(ResultTypeGrade))
                         select new { ID = s, Name = s.ToString() };

            // Pass ResultType enum values to the view
            //ViewBag.ResultTypeOptions = Enum.GetValues(typeof(ResultType))
            //                                .Cast<ResultType>()
            //                                .Select(r => new SelectListItem
            //                                {
            //                                    Value = r.ToString(),
            //                                    Text = r.ToString()
            //                                });

            var resultTypes = Enum.GetValues(typeof(ResultType))
                          .Cast<ResultType>()
                          .Select(rt => new SelectListItem
                          {
                              Value = rt.ToString(),
                              Text = rt.ToString()
                          });

            ViewBag.ResultTypeOptions = new SelectList(resultTypes, "Value", "Text");

            ViewBag.StdSubjectId = new SelectList(_db.Subjects.AsNoTracking(), "SubjectId", "CourseName");
            ViewBag.SubjectGrade = new SelectList(grades, "Name", "Name");
            ViewBag.ApplicantEmail = model.MatNum;

            return View(applicantOLevelResults);
        }

        [HttpPost]
        public async Task<JsonResult> AdminSaveRowData(ApplicantOLevelResult updatedResult)
        {
            try
            {
                var existingResult = _db.ApplicantOLevelResults.Find(updatedResult.ApplicantOLevelResultId);

                //if (existingResult == null || existingResult.GradePoint > 0)
                //{
                //    //return Json(new { status = false, message = "Cannot edit this result." });
                //}

                existingResult.ExaminationCenter = updatedResult.ExaminationCenter;
                existingResult.Year = updatedResult.Year;
                existingResult.ExamNumber = updatedResult.ExamNumber;
                //existingResult.SubjectGrade = updatedResult.SubjectGrade;

                await _db.SaveChangesAsync();

                return Json(new { status = true, message = "Result saved successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = "Error: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult> SaveNewRow(ApplicantOLevelResult newRow)
        {
            if (ModelState.IsValid)
            {
                var olevelresult = new ApplicantOLevelResult
                {
                    ApplicantId = newRow.ApplicantId,
                    ResultName = newRow.ResultName.ToString().Trim(),
                    ExaminationCenter = newRow.ExaminationCenter.Trim(),
                    Year = newRow.Year.Trim(),
                    SubjectId = newRow.SubjectId,
                    SubjectGrade = newRow.SubjectGrade,
                    ExamNumber = newRow.ExamNumber,
                    //GradePoint = _query.GetAdmissionGradePoint(rowData.SubjectGrade)
                };
                _db.ApplicantOLevelResults.Add(olevelresult);

                await _db.SaveChangesAsync();
                // Save new row to database
                return Json(new { status = true, message = "Row added successfully!" });
            }

            return Json(new { status = false, message = "Failed to add row." });
        }

        [HttpPost]
        public JsonResult DeleteRow(DeleteRowRequest request)
        {
            if (request == null)
            {
                return Json(new { status = false, message = "Invalid data provided." });
            }

            try
            {
                //using (var context = new YourDbContext())
                //{
                // Find the record(s) to delete
                var oLevelResults = _db.ApplicantOLevelResults
                    .Where(r => r.ApplicantOLevelResultId == request.ApplicantOlevelResultId)
                    .FirstOrDefault();

                // Check if deletable (GradePoint validation)
                if (oLevelResults.GradePoint > 0)
                {
                    return Json(new { status = false, message = "Cannot delete subjects with a grade point." });
                }

                // Remove all matching records
                _db.ApplicantOLevelResults.Remove(oLevelResults);
                _db.SaveChanges();
                //}

                return Json(new { status = true, message = "Row(s) deleted successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = $"Error: {ex.Message}" });
            }
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

