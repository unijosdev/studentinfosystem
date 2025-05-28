using OfficeOpenXml;
using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampusModel;
using SwiftKampusModel.AddmissionApplicant;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SwiftKampus.Controllers
{
    [Authorize]
   // [Audit(AuditingLevel = 2)]
    public class UnderGraduateRulesController : BaseController
    {

        public UnderGraduateRulesController(SchoolDbContext db) : base(db)
        {

        }
        // GET: UnderGraduateRules
        public ActionResult Index()
        {
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FullName");
            if (User.IsInRole(RoleName.None_Academic) || User.IsInRole(RoleName.Academic))
            {
                var staffDept = _db.Staffs.Include(i => i.Department).Where(x => x.Email.Equals(userId))
                                    .Select(s => s.Department.DepartmentId).FirstOrDefault();
                ViewBag.ProgrammeId = new SelectList(_db.Programmes.Include(i => i.Department).AsNoTracking()
                                    .Where(x => x.Department.DepartmentId.Equals(staffDept)), "ProgrammeId", "ProgrammeName");
            }
            else
            {
                ViewBag.ProgrammeId = new SelectList(_db.Programmes.AsNoTracking(), "ProgrammeId", "ProgrammeName");
            }

            return View();
        }

        public async Task<ActionResult> GetIndex(int? ProgrammeId, int? SchoolProgrammeId)
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

            //var undergraduateRule = new List<UnderGraduateRule>();
            var undergraduateRule = await _db.UnderGraduateRules.AsNoTracking().Include(u => u.Programme)
                                          .Include(u => u.Subject).Include(u => u.SchoolProgramme).ToListAsync();

            if (!string.IsNullOrEmpty(search))
            {

                undergraduateRule = undergraduateRule.Where(x => x.SchoolProgramme.FancyName.Equals(search)
                                    || x.Programme.ProgrammeName.Equals(search))
                                            .ToList();
            }
            if (SchoolProgrammeId != null)
            {
                undergraduateRule = undergraduateRule.Where(x => x.SchoolProgramme.SchoolProgrammeId.Equals((int)SchoolProgrammeId))
                                          .ToList();
            }
            if (ProgrammeId != null)
            {
                undergraduateRule = undergraduateRule.Where(x => x.Programme.ProgrammeId.Equals((int)ProgrammeId)).ToList();
            }

            var sortedData = undergraduateRule.Select(s => new
            {
                ProgrammeName = s.Programme?.ProgrammeName ?? "",
                s.SchoolProgramme.FullName,
                s.Subject.CourseName,
                // s.SubjectGrade,
                s.IsRequired,
                s.UnderGraduateRuleId

            }).ToList();
            totalRecords = sortedData.Count();
            var data = sortedData.Skip(skip).Take(pageSize).ToList();

            return Json(new { draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = sortedData },
                JsonRequestBehavior.AllowGet);

            #endregion Server Side filtering
        }

        public async Task<PartialViewResult> Save(int id)
        {
            var underGraduateRule = await _db.UnderGraduateRules.Include(i => i.Programme).Include(i => i.Subject)
                                .Where(x => x.UnderGraduateRuleId.Equals(id)).FirstOrDefaultAsync();

            var myGrade = from ResultTypeGrade s in Enum.GetValues(typeof(ResultTypeGrade))
                          select new { ID = s, Name = s.ToString() };

            ViewBag.SubjectGrade = new MultiSelectList(myGrade, "Name", "Name");
            if (User.IsInRole(RoleName.None_Academic) || User.IsInRole(RoleName.Academic))
            {
                var staffDept = _db.Staffs.Include(i => i.Department).Where(x => x.Email.Equals(userId))
                                    .Select(s => s.Department.DepartmentId).FirstOrDefault();
                ViewBag.ProgrammeId = new SelectList(_db.Programmes.Include(i => i.Department).AsNoTracking()
                                    .Where(x => x.Department.DepartmentId.Equals(staffDept)), "ProgrammeId", "ProgrammeName", underGraduateRule?.ProgrammeId);
            }
            else
            {
                ViewBag.ProgrammeId = new SelectList(_db.Programmes.AsNoTracking(), "ProgrammeId", "ProgrammeName", underGraduateRule?.ProgrammeId);
            }
            ViewBag.SubjectId = new SelectList(_db.Subjects.AsNoTracking(), "SubjectId", "CourseName", underGraduateRule?.SubjectId);
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FullName", underGraduateRule?.SchoolProgrammeId);
            if (underGraduateRule != null)
            {
                var underGraduateRuleVm = new UnderGraduateRuleVm
                {
                    UnderGraduateRuleId = underGraduateRule.UnderGraduateRuleId
                };
                return PartialView(underGraduateRuleVm);
            }
            return PartialView();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Save(UnderGraduateRuleVm model)
        {
            bool status = false;
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                if (model.UnderGraduateRuleId > 0)
                {
                    var underGraduateRule = await _db.UnderGraduateRules.FindAsync(model.UnderGraduateRuleId);
                    if (underGraduateRule != null)
                    {
                        string[] isRequired = model.IsRequired.Trim().Split(',');
                        bool modelRequired = isRequired[0].ToUpper() == "R";

                        underGraduateRule.ProgrammeId = model.ProgrammeId;
                        underGraduateRule.SubjectId = model.SubjectId[0];
                        underGraduateRule.SchoolProgrammeId = model.SchoolProgrammeId;
                        underGraduateRule.IsRequired = modelRequired;
                        _db.Entry(underGraduateRule).State = EntityState.Modified;

                        await _db.SaveChangesAsync();
                        message = "Subject Grade Updated Successfully...";
                        return new JsonResult { Data = new
                        { status = true, message }
                        };
                    }
                }
                else
                {
                    string[] grades;
                    string[] isRequired = { };
                    if (!string.IsNullOrEmpty(model.IsRequired))
                    {
                       grades = model.IsRequired.Trim().Split(',');
                         isRequired = model.IsRequired.Trim().Split(',');
                        if (model.SubjectId.Length != grades.Length)
                        {
                            return new JsonResult { Data = new { status = false, message = "Subject Selected and Grade must be the same" } };
                        }

                    }
                   
                    for (int i = 0; i < model.SubjectId.Length; i++)
                    {
                        if (model.ProgrammeId != null)
                        {
                            var subjectId = model.SubjectId[i];
                            var checkExist = _db.UnderGraduateRules.Include(d => d.SchoolProgramme).Include(d => d.Programme).AsNoTracking()
                                                .Any(x => x.SchoolProgramme.SchoolProgrammeId.Equals((int)model.SchoolProgrammeId)
                                                && x.Programme.ProgrammeId.Equals((int)model.ProgrammeId)
                                                && x.SubjectId.Equals(subjectId));
                            if (checkExist)
                            {
                                return new JsonResult { Data = new { status = false, message = "Subject has been added for this programme under the school programme" } };
                            }
                        }
                        else
                        {
                            var subjectId = model.SubjectId[i];
                            var checkExist = _db.UnderGraduateRules.Include(d => d.SchoolProgramme).AsNoTracking()
                                                .Any(x => x.SchoolProgramme.SchoolProgrammeId.Equals((int)model.SchoolProgrammeId)
                                                 && x.SubjectId.Equals(subjectId));
                            if (checkExist)
                            {
                                return new JsonResult { Data = new { status = false, message = "Subject has been added under the school programme" } };
                            }
                        }

                        //line added by ema during debugging
                        //grades = model.SubjectGrade.Trim().Split(',');
                        isRequired = model.IsRequired.Trim().Split(',');

                        bool modelRequired = isRequired[i].ToUpper() == "R";
                        var undergraduateRule = new UnderGraduateRule
                        {
                            SchoolProgrammeId = model.SchoolProgrammeId,
                            ProgrammeId = model.ProgrammeId,
                            SubjectId = model.SubjectId[i],
                            //SubjectGrade = grades[i],
                            IsRequired = modelRequired,
                        };
                        _db.UnderGraduateRules.Add(undergraduateRule);
                    }
                    await _db.SaveChangesAsync();
                    message = $"Settings Applied Successfully.";
                    return new JsonResult { Data = new { status = true, message } };
                }
            }
            return new JsonResult { Data = new { status, message } };
            //return View(subject);
        }


        // GET: UnderGraduateRules/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UnderGraduateRule underGraduateRule = await _db.UnderGraduateRules.FindAsync(id);
            if (underGraduateRule == null)
            {
                return HttpNotFound();
            }
            return View(underGraduateRule);
        }

        // GET: UnderGraduateRules/Create
        public ActionResult Create()
        {
            ViewBag.ProgrammeId = new SelectList(_db.Programmes.AsNoTracking(), "ProgrammeId", "ProgrammeName");
            ViewBag.SubjectId = new SelectList(_db.Subjects.AsNoTracking(), "SubjectId", "CourseCode");
            var myGrade = from ResultTypeGrade s in Enum.GetValues(typeof(ResultTypeGrade))
                          select new { ID = s, Name = s.ToString() };

            ViewBag.SubjectGrade = new MultiSelectList(myGrade, "Name", "Name");
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "ProgrammeName");
            return View();
        }

        // POST: UnderGraduateRules/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(UnderGraduateRuleVm model)
        {
            if (ModelState.IsValid)
            {
                string[] grades = model.SubjectGrade.Trim().Split(',');
                string[] isRequired = model.IsRequired.Trim().Split(',');
                if (model.SubjectId.Length != model.SubjectGrade.Length)
                {
                    return View(model);
                }
                for (int i = 0; i < model.SubjectGrade.Length; i++)
                {
                    bool modelRequired = isRequired[i].ToUpper() == "R";
                    var undergraduateRule = new UnderGraduateRule
                    {
                        SchoolProgrammeId = model.SchoolProgrammeId,
                        ProgrammeId = model.ProgrammeId,
                        SubjectId = model.SubjectId[i],
                        //SubjectGrade = grades[i],
                        IsRequired = modelRequired,
                    };
                    _db.UnderGraduateRules.Add(undergraduateRule);
                }
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.ProgrammeId = new SelectList(_db.Programmes.AsNoTracking(), "ProgrammeId", "ProgrammeName");
            ViewBag.SubjectId = new SelectList(_db.Subjects.AsNoTracking(), "SubjectId", "CourseCode");
            var myGrade = from ResultTypeGrade s in Enum.GetValues(typeof(ResultTypeGrade))
                          select new { ID = s, Name = s.ToString() };

            ViewBag.SubjectGrade = new MultiSelectList(myGrade, "Name", "Name");
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "ProgrammeName");
            return View(model);
        }

        // GET: UnderGraduateRules/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UnderGraduateRule underGraduateRule = await _db.UnderGraduateRules.FindAsync(id);
            if (underGraduateRule == null)
            {
                return HttpNotFound();
            }
            ViewBag.ProgrammeId = new SelectList(_db.Programmes, "ProgrammeId", "ProgrammeName", underGraduateRule.ProgrammeId);
            ViewBag.SubjectId = new SelectList(_db.Subjects, "SubjectId", "CourseCode", underGraduateRule.SubjectId);
            var myGrade = from ResultTypeGrade s in Enum.GetValues(typeof(ResultTypeGrade))
                          select new { ID = s, Name = s.ToString() };

            ViewBag.SubjectGrade = new MultiSelectList(myGrade, "Name", "Name");
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes, "SchoolProgrammeId", "ProgrammeName");
            return View(underGraduateRule);
        }

        // POST: UnderGraduateRules/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "UnderGraduateRuleId,SchoolProgrammeId,ProgrammeId,SubjectId,SubjectGrade")] UnderGraduateRule underGraduateRule)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(underGraduateRule).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.ProgrammeId = new SelectList(_db.Programmes.AsNoTracking(), "ProgrammeId", "ProgrammeName");
            ViewBag.SubjectId = new SelectList(_db.Subjects.AsNoTracking(), "SubjectId", "CourseCode");
            var myGrade = from ResultTypeGrade s in Enum.GetValues(typeof(ResultTypeGrade))
                          select new { ID = s, Name = s.ToString() };

            ViewBag.SubjectGrade = new MultiSelectList(myGrade, "Name", "Name");
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "ProgrammeName");
            return View(underGraduateRule);
        }

        // GET: UnderGraduateRules/Delete/5
        public async Task<PartialViewResult> Delete(int? id)
        {
            UnderGraduateRule underGraduateRule = await _db.UnderGraduateRules.Include(i => i.Programme).Include(i => i.Subject)
                .Include(i => i.SchoolProgramme).FirstOrDefaultAsync( x => x.UnderGraduateRuleId.Equals((int)id));
            return PartialView(underGraduateRule);
        }

        // POST: UnderGraduateRules/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            UnderGraduateRule underGraduateRule = await _db.UnderGraduateRules.FindAsync(id);
            if (underGraduateRule != null) _db.UnderGraduateRules.Remove(underGraduateRule);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }


        public PartialViewResult UploadUndergraduateRule()
        {
            return PartialView();
        }


        [HttpPost]
        public async Task<ActionResult> UploadUndergraduateRule(HttpPostedFileBase excelfile)
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
                    int requiredField = 3;
                    var schoolProgrammeId = _db.SchoolProgrammes.AsNoTracking()
                             .Where(x => x.ProgrammeCategory.Equals(ProgrammeCategory.UnderGraduate.ToString()) &&
                             x.ProgrammeType.Equals(ProgrammeType.Full_Time.ToString()))
                             .Select(s => s.SchoolProgrammeId).FirstOrDefault();

                    string validCheck = myExcel.ValidateExcel(noOfRow, workSheet, requiredField);
                    if (!validCheck.Equals("Success"))
                    {
                        //string row = "";
                        //string column = "";
                        string[] ssizes = validCheck.Split(' ');
                        string[] myArray = new string[2];
                        for (int i = 0; i < ssizes.Length; i++)
                        {
                            myArray[i] = ssizes[i];
                            // myArray[i] = ssizes[];
                        }
                        string lineError = $"Line/Row number {myArray[0]}  and column {myArray[1]} is not rightly formatted, Please Check for anomalies ";
                        //ViewBag.LineError = lineError;
                        ViewBag.Message = lineError;
                        return View("Index");
                    }
                    for (int row = 2; row <= noOfRow; row++)
                    {
                        var programmeName = workSheet.Cells[row, 1].Value.ToString().Trim();
                        var subjectName = workSheet.Cells[row, 2].Value.ToString().Trim();
                        var required = workSheet.Cells[row, 3].Value.ToString().Trim();
                        var programme = await _db.Programmes.AsNoTracking().Where(x => x.ProgrammeCode.ToUpper().Trim().Equals(programmeName.ToUpper())).FirstOrDefaultAsync();

                        if (programme == null)
                        {

                            ViewBag.ErrorInfo = $"Please Leave no column or row Empty/Blank in row {row}";
                            ViewBag.ErrorMessage = $" The \"{programme}\" specified in the excel doesn't exist on the portal. " +
                                                   $"Please add the programme code first before uploading or correct the excel sheet...";
                            return View("ErrorException");
                        }

                        string[] subjects = subjectName.Split(',');
                        string[] isRequired = required.Split(',');
                        if (subjects.Length < 5)
                        {
                            ViewBag.ErrorInfo = "Please make sure the subjects is equal or grater than five";
                            ViewBag.ErrorMessage = $" The no of subject specified asubject name is less than five, " +
                                                    $"Please check row {row} to fix this ";
                            return View("ErrorException");
                        }

                        if (subjects.Length != isRequired.Length)
                        {
                            ViewBag.ErrorInfo = "Please make sure the No of Requirment for both match";
                            ViewBag.ErrorMessage = $" The no of subject specified and the required no is not thesame, " +
                                                    $"Please check row {row} to fix this ";
                            return View("ErrorException");
                        }
                        try
                        {
                            for (int i = 0; i < subjects.Length; i++)
                            {
                                bool modelRequired = isRequired[i].ToUpper().Trim() == "R";

                                var subjectCode = subjects[i].Trim();

                                var mySubject = _db.Subjects.AsNoTracking()
                                                .Where(x => x.CourseCode.ToUpper().Trim().Equals(subjectCode.ToUpper()))
                                                .FirstOrDefault();
                                if (mySubject == null)
                                {
                                    ViewBag.ErrorInfo = "Please make sure the subject code used is available";
                                    ViewBag.ErrorMessage = $"Please check row {row} to fix this ";
                                    return View("ErrorException");
                                }
                                var underGraduates = _db.UnderGraduateRules.Include(d => d.SchoolProgramme).Include(p => p.Programme).AsNoTracking()
                                                    .Where(x => x.SchoolProgramme.SchoolProgrammeId.Equals(schoolProgrammeId)
                                                     && x.SubjectId.Equals(mySubject.SubjectId)
                                                     && x.Programme.ProgrammeId.Equals(programme.ProgrammeId)).FirstOrDefault();
                                if (underGraduates != null)
                                {
                                    underGraduates.IsRequired = modelRequired;
                                    _db.Entry(underGraduates).State = EntityState.Modified;
                                }
                                else
                                {
                                    var undergraduateRule = new UnderGraduateRule
                                    {
                                        SchoolProgrammeId = schoolProgrammeId,
                                        ProgrammeId = programme.ProgrammeId,
                                        SubjectId = mySubject.SubjectId,
                                        IsRequired = modelRequired,
                                    };
                                    _db.UnderGraduateRules.Add(undergraduateRule);
                                }
                            }


                        }
                        catch (Exception ex)
                        {
                            ViewBag.ErrorInfo = $"Error Saving the Utme Subjects in row {row} of the excel";
                            ViewBag.ErrorMessage = ex.Message;
                            return View("ErrorException");
                        }
                    }
                    await _db.SaveChangesAsync();
                    message = $"You have successfully Uploaded {recordCount} records...  and {lastrecord}";
                    ViewBag.Message = message;

                }
                return RedirectToAction("Index", "UnderGraduateRules", new { message });
            }

            ViewBag.Error = $"File type is Incorrect <br/>";
            return View("Index");
        }


        public async Task<ActionResult> DeleteBuplicate()
        {
            var subjectRules = await _db.UnderGraduateRules.AsNoTracking().ToListAsync();
            var duplicateRules = new List<UnderGraduateRule>();
            foreach (var subjectRule in subjectRules)
            {
                var ruleCount = _db.UnderGraduateRules.AsNoTracking()
                                .Where(x => x.SchoolProgrammeId.Equals(subjectRule.SchoolProgrammeId) &&
                                x.Programme.ProgrammeId.Equals((int)subjectRule.ProgrammeId) &&
                                x.SubjectId.Equals(subjectRule.SubjectId)).ToList();
                if (ruleCount.Count() > 1)
                {
                    duplicateRules.Add(ruleCount.FirstOrDefault());
                }
            }
            foreach (var duplicateRule in duplicateRules)
            {
                _db.UnderGraduateRules.Remove(duplicateRule);
                _db.SaveChanges();
            }

            ViewBag.Message = $"{duplicateRules.Count} removed successfully";

            return View();
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
