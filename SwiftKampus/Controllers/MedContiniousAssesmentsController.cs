using System.Data.Entity;
using System.Threading.Tasks;
using System.Web.Mvc;
using SwiftKampus.Models;
using SwiftKampus.Services;
using System.Linq;
using System.Web.Script.Serialization;
using System.Collections.Generic;
using SwiftKampus.ViewModels.MedResultVm;
using SwiftKampus.BusinessLogic;
using SwiftKampusModel.MedicalScience;
using System.Data.Entity.Migrations;
using OfficeOpenXml;
using System.Web;
using System;
using SwiftKampusModel;
using Microsoft.Ajax.Utilities;

namespace SwiftKampus.Controllers
{
    [Authorize]
    [Audit(AuditingLevel = 2)]
    public class MedContiniousAssesmentsController : BaseController
    {
        private MedResultCommand _medResultQuery;
        public MedContiniousAssesmentsController(SchoolDbContext db) : base(db)
        {
            _medResultQuery = new MedResultCommand(_db);
        }

        // GET: MedContiniousAssesments
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult SearchAssessment()
        {
            ViewBag.LevelId = new SelectList(_db.Levels.AsNoTracking(), "LevelId", "LevelName");
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            if (User.IsInRole(RoleName.Academic))
            {
                var staffDeptId = _db.Staffs.Include(i => i.Department).AsNoTracking()
                                    .Where(x => x.Email.Equals(userId)).Select(s => s.Department.DepartmentId);
                ViewBag.MedResultCategoryId = new SelectList(_db.MedResultCategories.Include(i => i.Programme.Department).AsNoTracking()
                                                                .Where(x => x.Programme.Department.DepartmentId.Equals(staffDeptId)), "MedResultCategoryId", "CategoryName");
            }
            else
            {
                ViewBag.MedResultCategoryId = new SelectList(_db.MedResultCategories.AsNoTracking(), "MedResultCategoryId", "CategoryName");
            }
            ViewBag.MedResultCaId = new SelectList(_db.MedResultCas.AsNoTracking(), "MedResultCaId", "ResultCaName");

            return View();
        }

        public ActionResult GetMedResultCas(int codeId)
        {
            var item = _db.MedResultCas.AsNoTracking()
                            .Where(x => x.MedResultCategoryId.Equals(codeId))
                .Select(s => new { s.MedResultCaId, s.ResultCaName });

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            var result = javaScriptSerializer.Serialize(item);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetMedResultCategory(int codeId)
        {
            var item = _db.MedResultCategories.AsNoTracking()
                            .Where(x => x.LevelId.Equals(codeId))
                .Select(s => new { s.MedResultCategoryId, s.CategoryName });

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            var result = javaScriptSerializer.Serialize(item);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        public async Task<ActionResult> GetIndex(int? SessionId, int? LevelId, int? MedResultCaId)
        {
            var data = new List<MedCaListVm>();
            var cA = new List<MedContiniousAssesment>();
            if (SessionId != null && MedResultCaId != null)
            {
                 cA = await _db.MedContiniousAssesments.Include(m => m.MedResultCa.MedResultCategory.Level).Include(m => m.Session)
                            .Include(m => m.Student).AsNoTracking()
                            .Where(x => x.SessionId.Equals((int)SessionId) &&
                            x.MedResultCaId.Equals((int)MedResultCaId)).ToListAsync();              
            }
            else
            {
                cA = await _db.MedContiniousAssesments.Include(m => m.MedResultCa.MedResultCategory.Level).Include(m => m.Session)
                          .Include(m => m.Student).AsNoTracking().ToListAsync();
            }
            data.AddRange(cA.Select(s => new MedCaListVm
            {
                MedContiniousAssesmentId = s.MedContiniousAssesmentId,
                MatricNo = s.Student.MatricNo,
                StudentFullName = s.Student.FullName,
                LevelName = s.MedResultCa.MedResultCategory.Level.LevelName,
                Score = s.Score.ToString(),
                SessionName = s.Session.SessionName,
                ResultName = s.MedResultCa.ResultCaName,
                IsAbsent = s.IsAbsentForExam ? "Absent" : "Present"
            }));
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        //[RecurringAuthorize]
        public ActionResult MedCreateCaView(string message)
        {
            ViewBag.LevelId = new SelectList(_db.Levels.AsNoTracking(), "LevelId", "LevelName");

            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            if (User.IsInRole(RoleName.Academic))
            {
                var staffDeptId = _db.Staffs.Include(i => i.Department).AsNoTracking()
                                    .Where(x => x.Email.Equals(userId)).Select(s => s.Department.DepartmentId);
                ViewBag.MedResultCategoryId = new SelectList(_db.MedResultCategories.Include(i => i.Programme.Department).AsNoTracking()
                                                                .Where(x => x.Programme.Department.DepartmentId.Equals(staffDeptId)), "MedResultCategoryId", "CategoryName");
            }
            else
            {
                ViewBag.MedResultCategoryId = new SelectList(_db.MedResultCategories.AsNoTracking(), "MedResultCategoryId", "CategoryName");
            }
            ViewBag.MedResultCaId = new SelectList(_db.MedResultCas.AsNoTracking(), "MedResultCaId", "ResultCaName");
            ViewBag.Message = message;
            return View();
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "MedCreateCa")]
        public async Task<ActionResult> MedCreateCa(MedSelectCaVm model)
        {
            var schoolProgrammeId = await _db.SchoolProgrammes.AsNoTracking()
                                      .Where(x => x.ProgrammeCategory.ToUpper().Equals(ProgrammeCategory.UnderGraduate.ToString().ToUpper())
                                      && x.ProgrammeType.ToUpper().Equals(ProgrammeType.Full_Time.ToString().ToUpper()))
                                      .Select(s => s.SchoolProgrammeId).FirstOrDefaultAsync();
            model.SessionId = _query.GetCurrentSessionId(schoolProgrammeId);
            //var result = await _medResultQuery.GenerateCaList(model, userId);
            var result = await GenerateCaList(model, userId);
            if (result.Item1 != null)
            {
                return View(result.Item1.ToList());
            }
            return RedirectToAction("MedCreateCaView", new { message = result.Item2 });
        }


        [HttpPost]
        [MultipleButton(Name = "action", Argument = "MedDownloadCa")]
        public async Task MedDownloadCa(MedSelectCaVm model)
        {
            var schoolProgrammeId = await _db.SchoolProgrammes.AsNoTracking()
                                      .Where(x => x.ProgrammeCategory.ToUpper().Equals(ProgrammeCategory.UnderGraduate.ToString().ToUpper())
                                      && x.ProgrammeType.ToUpper().Equals(ProgrammeType.Full_Time.ToString().ToUpper()))
                                      .Select(s => s.SchoolProgrammeId).FirstOrDefaultAsync();
            model.SessionId = _query.GetCurrentSessionId(schoolProgrammeId);
            //var myCalist = await _medResultQuery.GenerateCaList(model, userId);
            var myCalist = await GenerateCaList(model, userId);
            if (myCalist.Item1 == null)
            {
                Response.End();
            }

            char c1 = 'A';
            ExcelPackage package = new ExcelPackage();
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Report");
            var caProgrammeId = myCalist.Item1[0].MedResultCa.MedResultCategory.Programme.ProgrammeId;

            var programme = _db.Programmes.Include(i => i.Department).Include(i => i.Department.Faculty)
                            .AsNoTracking().Where(x => x.ProgrammeId.Equals(caProgrammeId)).FirstOrDefault();

            var programmeId = worksheet.Cells["AA2"];
            var courseId = worksheet.Cells["AA3"];
            var levelId = worksheet.Cells["AA4"];
            var semester = worksheet.Cells["AA5"];
            var session = worksheet.Cells["AA6"];

            programmeId.Value = myCalist.Item1[0].MedResultCa.MedResultCategory.Programme.ProgrammeId;
            courseId.Value = myCalist.Item1[0].MedResultCa.MedResultCaId;
            levelId.Value = myCalist.Item1[0].MedResultCa.MedResultCategory.Level.LevelId;
            session.Value = myCalist.Item1[0].Session.SessionId;

            var Rng = worksheet.Cells["B1"];
            Rng.Value = "UNIVERSITY OF JOS";
            Rng.Style.Font.Size = 14;
            Rng.Style.Font.Bold = true;
            var Rng1 = worksheet.Cells["B2"];
            Rng1.Value = $"FACULTY OF {programme.Department.Faculty.FacultyName.ToUpper()}";
            Rng1.Style.Font.Bold = true;
            var Rng2 = worksheet.Cells["B3"];
            Rng2.Value = $"DEPARTMENT OF {programme.Department.DeptName.ToUpper()} ({myCalist.Item1[0].MedResultCa.MedResultCategory.Programme.ProgrammeCode.ToUpper()})";
            Rng2.Style.Font.Bold = true;
            var Rng3 = worksheet.Cells["B4"];
            Rng3.Value = $"{myCalist.Item1[0].MedResultCa.ResultCaName.ToUpper()} ({myCalist.Item1[0].MedResultCa.MedResultCategory.CategoryName.ToUpper()})";
            Rng3.Style.Font.Bold = true;
            var Rng4 = worksheet.Cells["B5"];
            Rng4.Value = $"{myCalist.Item1[0].MedResultCa.MedResultCategory.Level.LevelName.ToUpper()} LEVEL";
            Rng4.Style.Font.Bold = true;
            var Rng6 = worksheet.Cells["B6"];
            Rng6.Value = $"{myCalist.Item1[0].Session.SessionName.ToUpper()} SESSION";
            Rng6.Style.Font.Bold = true;

            worksheet.Cells[$"{c1++}8"].Value = "Id";
            worksheet.Cells[$"{c1++}8"].Value = "Student Name";
            worksheet.Cells[$"{c1++}8"].Value = "Matric Number";
            worksheet.Cells[$"{c1++}8"].Value = "Score(40)";
            worksheet.Cells[$"{c1++}8"].Value = "Is Absent";
            //worksheet.Cells[$"{c1++}7"].Value = "Staff Name";

            int rowStart = 9;
            //char c2 = 'A';

            for (var i = 0; i < myCalist.Item1.Count; i++)
            {

                worksheet.Cells[$"A{rowStart}"].Value = myCalist.Item1[i].MedContiniousAssesmentId;
                worksheet.Cells[$"B{rowStart}"].Value = myCalist.Item1[i].Student.FullName;
                worksheet.Cells[$"C{rowStart}"].Value = myCalist.Item1[i].Student.MatricNo;
                worksheet.Cells[$"D{rowStart}"].Value = myCalist.Item1[i].Score;
                worksheet.Cells[$"F{rowStart}"].Value = myCalist.Item1[i].IsAbsentForExam;
                rowStart++;
            }
            var info = myCalist.Item1.FirstOrDefault();
            //worksheet.Cells["A:AZ"].AutoFitColumns();
            worksheet.Column(1).Style.Locked = true;
            worksheet.Column(2).Style.Locked = true;
            worksheet.Column(3).Style.Locked = true;
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment: filename=" + $"{info.MedResultCa.ResultCaName}{info.Session.SessionName}Result.xlsx");
            Response.BinaryWrite(package.GetAsByteArray());
            Response.End();

        }

        public async Task<Tuple<List<MedContiniousAssesementVm>, string>> GenerateCaList(MedSelectCaVm model, string userId)
        {
            var medResultCa = _db.MedResultCas.Include(i => i.MedResultCategory).Include(i => i.MedResultCategory.Programme)
                                    .Include(i => i.MedResultCategory.Level).AsNoTracking()
                                    .Where(x => x.MedResultCaId.Equals(model.MedResultCaId)).FirstOrDefault();

            //var regStudent = await _db.CourseRegistrations.Include(i => i.Students).Include(i => i.Programme.Department)
            //                                       .Include(i => i.Level).AsNoTracking()
            //                                       .Where(x => x.ProgrammeId.Equals(medResultCa.MedResultCategory.ProgrammeId)
            //                                       && x.LevelId.Equals(medResultCa.MedResultCategory.LevelId)
            //                                       && x.SessionId.Equals(model.SessionId))
            //                                       .Select(x => new
            //                                       {
            //                                           x.Level.LevelOrder,
            //                                           x.Students,
            //                                           deptId = x.Programme.Department.DepartmentId
            //                                       }).ToListAsync();

            var regStudent = await _db.Students.Include(i => i.Level).Include(i => i.Programme.Department)
                                    .Where(x => x.Programme.ProgrammeId.Equals(medResultCa.MedResultCategory.ProgrammeId) &&
                                                    x.Level.LevelId.Equals(medResultCa.MedResultCategory.LevelId))
                                                    .ToListAsync();
            if (!regStudent.Any())
            {
                return new Tuple<List<MedContiniousAssesementVm>, string>(null,
                                    "Student has not registered for this session");

            }

            var calist = _db.MedContiniousAssesments.AsNoTracking().Include(i => i.Student).Include(i => i.MedResultCa.MedResultCategory.Programme)
                                                   .Where(x => x.MedResultCaId.Equals(model.MedResultCaId) &&
                                                   x.MedResultCa.MedResultCategory.ProgrammeId.Equals(medResultCa.MedResultCategory.ProgrammeId) &&
                                                   x.MedResultCa.MedResultCategory.LevelId.Equals(medResultCa.MedResultCategory.LevelId) &&
                                                   x.SessionId.Equals(model.SessionId)).ToList();
            var myCalist = new List<MedContiniousAssesementVm>();
            if (calist.Any())
            {
                if (calist.Any(x => x.IsDeptApproved.Equals(true)))
                {
                    return new Tuple<List<MedContiniousAssesementVm>, string>(null,
                                        $"({medResultCa.ResultCaName}) This result has already been approved by the Department. It can't be edited again until its rejected by the department");

                }
                foreach (var list in calist)
                {
                    var ca = new MedContiniousAssesementVm()
                    {
                        MedContiniousAssesmentId = list.MedContiniousAssesmentId,
                        StudentId = list.StudentId,
                        MedResultCa = medResultCa,
                        SessionId = list.SessionId,
                        Score = list.Score,
                        IsAbsentForExam = list.IsAbsentForExam,
                        StaffName = userId,
                        MedResultCaId = list.MedResultCaId,
                        Submitted = list.Submitted
                    };
                    myCalist.Add(ca);
                }
            }
            else
            {
                foreach (var student in regStudent)
                {

                    var ca = new MedContiniousAssesementVm()
                    {
                        MedContiniousAssesmentId = 0,
                        //StudentId = student.Students.StudentId,
                        StudentId = student.StudentId,
                        MedResultCa = medResultCa,
                        SessionId = model.SessionId,
                        Score = 0,
                        IsAbsentForExam = false,
                        StaffName = userId,
                        MedResultCaId = medResultCa.MedResultCaId,
                        Submitted = false
                    };
                    myCalist.Add(ca);
                }
            }
            return new Tuple<List<MedContiniousAssesementVm>, string>(myCalist, "success");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SaveMedCa(List<MedContiniousAssesementVm> model)
        {
            if (ModelState.IsValid)
            {
                //bool isAvailable = await _resultQuery.CheckForResult(model[0].CourseId, model[0].SessionId, model[0].LevelId, model[0].ProgrammeId);
                //if (isAvailable && model[0].ContinuousAssessmentId == 0)
                //{
                //    return RedirectToAction("CreateCaView", new { message = "This result has already been uploaded. Download the recent version of the result to upload again" });
                //}
                int maximumScore = model.Select(s => s.MedResultCa.MaximumScore).FirstOrDefault();
                if(model.Any(x => x.Score > maximumScore))
                {
                    return new JsonResult { Data = new { status = false, message= "The input Score cannot be more than maximum score." } };
                }
                foreach (var item in model)
                {

                    var continiousAssesment = new MedContiniousAssesment()
                    {
                        MedContiniousAssesmentId = item.MedContiniousAssesmentId,
                        MedResultCaId = item.MedResultCaId,
                        Score = item.Score,
                        SessionId = item.SessionId,
                        StudentId = item.StudentId,
                        StaffName = item.StaffName,
                        Submitted = true,
                        IsAbsentForExam = item.IsAbsentForExam,
                        ReasonForReject = "Lecturer Submit"

                    };
                    _db.MedContiniousAssesments.AddOrUpdate(continiousAssesment);
                }
                await _db.SaveChangesAsync();
                return new JsonResult { Data = new { status = true, message = "Result Saved Successfully" } };
            }

            return View("MedCreateCa");
        }



        [HttpGet]
        public PartialViewResult UploadResult()
        {
            return PartialView();
        }

        [HttpGet]
        public ActionResult UploadResultPage()
        {
            return View();
        }


        [HttpPost]
        public async Task<ActionResult> UploadResult(HttpPostedFileBase excelfile)
        {
            if (excelfile == null || excelfile.ContentLength == 0)
            {
                ViewBag.Error = "Please Select a excel file <br/>";
                TempData["UserMessage"] = "Please Select a excel file.";
                TempData["Title"] = "Error.";

                return RedirectToAction("Index");
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
                    var continiousAssesmentVmList = new List<MedContiniousAssesementVm>();
                    ExcelValidation myExcel = new ExcelValidation();
                    var currentSheet = package.Workbook.Worksheets;
                    var workSheet = currentSheet.First();
                    var noOfCol = workSheet.Dimension.End.Column;
                    var noOfRow = workSheet.Dimension.End.Row;

                    var programmeId = Convert.ToInt32(workSheet.Cells["AA2"].Value.ToString().Trim());
                    var courseId = Convert.ToInt32(workSheet.Cells["AA3"].Value.ToString().Trim());
                    var levelId = Convert.ToInt32(workSheet.Cells["AA4"].Value.ToString().Trim());
                    var semester = Convert.ToInt32(workSheet.Cells["AA5"].Value.ToString().Trim());
                    var session = Convert.ToInt32(workSheet.Cells["AA6"].Value.ToString().Trim());
                    string staffName = userId;
                    for (int row = 9; row <= noOfRow; row++)
                    {
                        int caId = Convert.ToInt32(workSheet.Cells[row, 1].Value.ToString().Trim());
                        string studentName = workSheet.Cells[row, 2].Value.ToString().Trim();
                        string studentId = workSheet.Cells[row, 3].Value.ToString().Trim();
                        int examScore = Convert.ToInt32(workSheet.Cells[row, 4].Value.ToString().Trim());
                        string isAbsent = workSheet.Cells[row, 5].Value.ToString().Trim();

                        bool isAbsentExam = false || (isAbsent.ToUpper().Equals("TRUE") || isAbsent.ToUpper().Equals("YES"));

                        // var model = await GetModelDetails(studentId, levelId, courseId, semester, session, programmeId);

                        //bool isAvailable = await _resultQuery.CheckForResult(model.CourseId, model.SessionId, levelId, programmeId);
                        //if (isAvailable && caId == 0)
                        //{
                        //    return RedirectToAction("CreateCaView", new { message = "This result has already been uploaded. Download the recent version of the result to upload again" });
                        //}
                        //bool isDeptApproved = await _resultQuery.CheckDeptApproval(model.CourseId, model.SessionId, levelId, programmeId);
                        //if (isDeptApproved)
                        //{
                        //    return RedirectToAction("CreateCaView", new { message = "This result has already been approved by the Department. It can't be edited again until its rejected by the department" });
                        //}

                        try
                        {
                            var vm = new MedContiniousAssesementVm()
                            {
                                StudentId = studentId,
                                MedResultCaId = courseId,
                                SessionId = sessionId,
                                StaffName = staffName,
                                Score = examScore,
                                IsAbsentForExam = isAbsentExam,
                            };
                            var continiousAssesment = new MedContiniousAssesment()
                            {
                                MedContiniousAssesmentId = vm.MedContiniousAssesmentId,
                                MedResultCaId = vm.MedResultCaId,
                                Score = vm.Score,
                                SessionId = vm.SessionId,
                                StudentId = vm.StudentId,
                                StaffName = vm.StaffName,
                                Submitted = true,
                                IsAbsentForExam = vm.IsAbsentForExam,
                                ReasonForReject = "Lecturer Submit"
                            };
                            _db.MedContiniousAssesments.AddOrUpdate(continiousAssesment);
                        }
                        catch (Exception ex)
                        {
                            ViewBag.ErrorInfo = "The programme code in the excel doesn't exist";
                            ViewBag.ErrorMessage = ex.Message;
                            return View("ErrorException");
                        }
                    }
                    await _db.SaveChangesAsync();
                    message = $"You have successfully Uploaded {recordCount} records...  and {lastrecord}";
                }
                return RedirectToAction("Index", "ContinuousAssessments");
            }

            ViewBag.Error = $"File type is Incorrect <br/>";
            return RedirectToAction("Index");
        }


        public ActionResult DeptApprovalIndex()
        {
            ViewBag.LevelId = new SelectList(_db.Levels.AsNoTracking(), "LevelId", "LevelName");
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            if (User.IsInRole(RoleName.Academic))
            {
                var staffDeptId = _db.Staffs.Include(i => i.Department).AsNoTracking()
                                    .Where(x => x.Email.Equals(userId)).Select(s => s.Department.DepartmentId);
                ViewBag.MedResultCategoryId = new SelectList(_db.MedResultCategories.Include(i => i.Programme.Department).AsNoTracking()
                                                                .Where(x => x.Programme.Department.DepartmentId.Equals(staffDeptId)), "MedResultCategoryId", "CategoryName");
            }
            else
            {
                ViewBag.MedResultCategoryId = new SelectList(_db.MedResultCategories.AsNoTracking(), "MedResultCategoryId", "CategoryName");
            }
            ViewBag.MedResultCaId = new SelectList(_db.MedResultCas.AsNoTracking(), "MedResultCaId", "ResultCaName");

            return View();
        }

        public ActionResult GetDeptApproval(int? SessionId, int? MedResultCategoryId)
        {
            var data = new List<MedCaListVmApproval>();
            var cA = new List<MedContiniousAssesment>();
            if (SessionId != null && MedResultCategoryId != null)
            {
                cA = _db.MedContiniousAssesments.Include(m => m.MedResultCa.MedResultCategory.Level).Include(m => m.Session)
                           .Include(m => m.Student).AsNoTracking()
                           .Where(x => x.SessionId.Equals((int)SessionId) &&
                           x.MedResultCa.MedResultCategoryId.Equals((int)MedResultCategoryId)).ToList()
                           .DistinctBy(x => x.MedResultCaId).ToList();
            }
            else
            {
                cA = _db.MedContiniousAssesments.Include(m => m.MedResultCa.MedResultCategory.Level).Include(m => m.Session)
                          .Include(m => m.Student).AsNoTracking()
                         .ToList().DistinctBy(x => x.MedResultCaId).ToList();
            }


            if (User.IsInRole(RoleName.Hod) && cA.Count > 0)
            {


            }
            else
            {

            }
            data.AddRange(cA.Select(s => new MedCaListVmApproval
            {
                MedContiniousAssesmentId = s.MedContiniousAssesmentId,
                CategoryName = s.MedResultCa.MedResultCategory.CategoryName,
                CaItem = s.MedResultCa.ResultCaName,
                LevelName = s.MedResultCa.MedResultCategory.Level.LevelName,
                SessionName = s.Session.SessionName,
                IsDeptApproved = s.IsDeptApproved,
                IsFacultyApproved = s.IsFacultyApproved,
                IsSenateApproved = s.IsSenateApproved,
                ReasonForReject = s.ReasonForReject,
                Submitted = s.Submitted
            }));

            return PartialView(data);
        }

        public async Task<ActionResult> DisplayResultDetails(int id)
        {
            var cA = await _db.MedContiniousAssesments.Include(i => i.Student.Programme.Department.Faculty)
                                .Include(i => i.MedResultCa).AsNoTracking()
                                .Where(x => x.MedResultCaId.Equals(id)).FirstOrDefaultAsync();
            var caList = await _db.MedContiniousAssesments.Include(i => i.Student.Programme.Department.Faculty)
                            .Include(i => i.Session).Include(i => i.Student.Level).Include(i => i.MedResultCa.MedResultCategory).AsNoTracking()
                            .Where(x => x.SessionId.Equals(cA.SessionId) &&
                            x.MedResultCaId.Equals(cA.MedResultCaId)).ToListAsync();
            var deptResultTemplate = _db.MedResultCas.Where(x => x.MedResultCaId.Equals(cA.MedResultCaId)).FirstOrDefault();
            ViewBag.FailMark = deptResultTemplate?.PassMark;
            ViewBag.PrintId = id;
            return View(caList);
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
