using Microsoft.Ajax.Utilities;
using OfficeOpenXml;
using Rotativa;
using SwiftKampus.BusinessLogic;
using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampus.ViewModels;
using SwiftKampus.ViewModels.Result;
using SwiftKampusModel;
using SwiftKampusModel.AddmissionApplicant;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using ResultVm = SwiftKampusModel.ResultVm;

namespace SwiftKampus.Controllers
{
    [Authorize]
    [Audit(AuditingLevel = 2)]
    public class ResultsController : BaseController
    {
        private readonly ResultCommand _resultCommand;

        public ResultsController(SchoolDbContext db) : base(db)
        {
            _resultCommand = new ResultCommand(_db);
        }

        public ActionResult ProbabtionError(string cgpa)
        {
            ViewBag.Message = cgpa;
            return View();
        }

        public ActionResult GenerateNyscList()
        {

            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FancyName");
            ViewBag.FacultyId = new SelectList(_db.Faculties.AsNoTracking(), "FacultyId", "FacultyName");
            ViewBag.DepartmentId = new SelectList(_db.Departments.AsNoTracking(), "DepartmentId", "DeptName");
            ViewBag.ProgrammeId = new SelectList(_db.Programmes.AsNoTracking(), "ProgrammeId", "ProgrammeName");
            ViewBag.LevelId = new SelectList(_db.Levels.AsNoTracking(), "LevelId", "LevelName");
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            return View();
        }

        public async Task<ActionResult> DisplayNyscList(int? programmeId, int? sessionId)
        {
            string message = string.Empty;
            if (programmeId != null && sessionId != null)
            {
                var programme = _db.Programmes.Include(i => i.FinalLevel).AsNoTracking()
                                    .Where(x => x.ProgrammeId.Equals((int)programmeId)).FirstOrDefault();
                var model = await _resultCommand.GetNyscList((int)programmeId, programme.FinalLevel.LevelId, (int)sessionId);
                if (model != null)
                {
                    return PartialView(model);
                }
                ViewBag.Message = "Result is not found for computation";
                return PartialView();
            }
            else
            {
                ViewBag.Message = "Please make sure you select both programme and session for the list to be generated";
            }
            ViewBag.Message = message;
            return PartialView();
        }

        public async Task DownloadNyscList(int? programmeId, int? sessionId)
        {
            if (programmeId != null && sessionId != null)
            {
                var programme = _db.Programmes.Include(i => i.FinalLevel).Include(i => i.Department).Include(i => i.Department.Faculty)
                                    .AsNoTracking()
                                    .Where(x => x.ProgrammeId.Equals((int)programmeId)).FirstOrDefault();
                var myCalist = await _resultCommand.GetNyscList((int)programmeId, programme.FinalLevel.LevelId, (int)sessionId);
                if (myCalist != null)
                {
                    char c1 = 'A';
                    ExcelPackage package = new ExcelPackage();
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Report");


                    var Rng = worksheet.Cells["B1"];
                    Rng.Value = "UNIVERSITY OF JOS";
                    Rng.Style.Font.Size = 14;
                    Rng.Style.Font.Bold = true;
                    var Rng1 = worksheet.Cells["B2"];
                    Rng1.Value = $"FACULTY OF {programme.Department.Faculty.FacultyName.ToUpper()}";
                    Rng1.Style.Font.Bold = true;
                    var Rng2 = worksheet.Cells["B3"];
                    Rng2.Value = $"DEPARTMENT OF {programme.Department.DeptName.ToUpper()} ";
                    Rng2.Style.Font.Bold = true;


                    worksheet.Cells[$"{c1++}5"].Value = "";
                    worksheet.Cells[$"{c1++}5"].Value = "Matric No";
                    worksheet.Cells[$"{c1++}5"].Value = "Surname";
                    worksheet.Cells[$"{c1++}5"].Value = "Other Names";
                    worksheet.Cells[$"{c1++}5"].Value = "Date of Birth";
                    worksheet.Cells[$"{c1++}5"].Value = "JAMB NO";
                    worksheet.Cells[$"{c1++}5"].Value = "State of Origin";
                    worksheet.Cells[$"{c1++}5"].Value = "Course";
                    worksheet.Cells[$"{c1++}5"].Value = "Class of Degree";
                    worksheet.Cells[$"{c1++}5"].Value = "Sex";
                    worksheet.Cells[$"{c1++}5"].Value = "Marital Status";
                    worksheet.Cells[$"{c1++}5"].Value = "Qualification";
                    worksheet.Cells[$"{c1++}5"].Value = "Service Year";
                    worksheet.Cells[$"{c1++}5"].Value = "Programme Mode";
                    worksheet.Cells[$"{c1++}5"].Value = "Phone No";
                    worksheet.Cells[$"{c1++}5"].Value = "Year of Result";

                    int rowStart = 9;
                    //char c2 = 'A';

                    for (var i = 0; i < myCalist.Count; i++)
                    {

                        worksheet.Cells[$"A{rowStart}"].Value = i + 1;
                        worksheet.Cells[$"B{rowStart}"].Value = myCalist[i].MatricNo;
                        worksheet.Cells[$"C{rowStart}"].Value = myCalist[i].Surname;
                        worksheet.Cells[$"D{rowStart}"].Value = myCalist[i].OtherName;
                        worksheet.Cells[$"E{rowStart}"].Value = myCalist[i].DateOfBirth.ToString("dd/MM/yyyy");
                        worksheet.Cells[$"F{rowStart}"].Value = myCalist[i].JambRegNo;
                        worksheet.Cells[$"G{rowStart}"].Value = myCalist[i].StateOfOrigin;
                        worksheet.Cells[$"H{rowStart}"].Value = myCalist[i].Course;
                        worksheet.Cells[$"I{rowStart}"].Value = myCalist[i].ClassOfDegree;
                        worksheet.Cells[$"J{rowStart}"].Value = myCalist[i].Gender;
                        worksheet.Cells[$"K{rowStart}"].Value = myCalist[i].MaritalStatus;
                        worksheet.Cells[$"L{rowStart}"].Value = myCalist[i].Qualification;
                        worksheet.Cells[$"M{rowStart}"].Value = myCalist[i].ServiceYear;
                        worksheet.Cells[$"N{rowStart}"].Value = myCalist[i].ProgrammeMode;
                        worksheet.Cells[$"O{rowStart}"].Value = myCalist[i].PhoneNumber;
                        worksheet.Cells[$"P{rowStart}"].Value = myCalist[i].YearOfResult;
                        rowStart++;
                    }
                    var info = myCalist.FirstOrDefault();
                    //worksheet.Cells["A:AZ"].AutoFitColumns();
                    worksheet.Column(1).Style.Locked = true;
                    worksheet.Column(2).Style.Locked = true;
                    worksheet.Column(3).Style.Locked = true;
                    Response.Clear();
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.AddHeader("content-disposition", "attachment: filename=" + $"{info.Course}NyscList.xlsx");
                    Response.BinaryWrite(package.GetAsByteArray());
                    Response.End();
                }
                Response.End();

            }
            Response.End();

        }

        public ActionResult GenerateGraduationList()
        {

            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FancyName");
            ViewBag.FacultyId = new SelectList(_db.Faculties.AsNoTracking(), "FacultyId", "FacultyName");
            ViewBag.DepartmentId = new SelectList(_db.Departments.AsNoTracking(), "DepartmentId", "DeptName");
            ViewBag.ProgrammeId = new SelectList(_db.Programmes.AsNoTracking(), "ProgrammeId", "ProgrammeName");
            ViewBag.LevelId = new SelectList(_db.Levels.AsNoTracking(), "LevelId", "LevelName");
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            return View();
        }

        public async Task<ActionResult> DisplayGraduationList(int? programmeId, int? sessionId)
        {
            string message = string.Empty;
            if (programmeId != null && sessionId != null)
            {
                var graduatioList = new GraduantsListVm();
                var programme = _db.Programmes.Include(i => i.FinalLevel).Include(i => i.Department.Faculty).AsNoTracking()
                                    .Where(x => x.ProgrammeId.Equals((int)programmeId)).FirstOrDefault();
                var model = await _resultCommand.GetNyscList((int)programmeId, programme.FinalLevel.LevelId, (int)sessionId);
                if (model != null)
                {
                    graduatioList.NyscListVms = model;
                    int schoolProgramId = model[0].SchoolProgrammeId;
                    graduatioList.ClassOfHonour = _db.ClassDegrees.AsNoTracking().Where(x => x.SchoolProgrammeId.Equals(schoolProgramId))
                                                    .Select(s => s.Remark).ToList();
                    return PartialView(graduatioList);
                }
                ViewBag.Message = "Result is not found for computation";
                return PartialView();
            }
            else
            {
                ViewBag.Message = "Please make sure you select both programme and session for the list to be generated";
            }
            ViewBag.Message = message;
            return PartialView();
        }

        public async Task DownloadGraduationList(int? programmeId, int? sessionId)
        {
            if (programmeId != null && sessionId != null)
            {
                var graduatioList = new GraduantsListVm();
                var programme = _db.Programmes.Include(i => i.FinalLevel).Include(i => i.Department.Faculty).AsNoTracking()
                                    .Where(x => x.ProgrammeId.Equals((int)programmeId)).FirstOrDefault();
                var model = await _resultCommand.GetNyscList((int)programmeId, programme.FinalLevel.LevelId, (int)sessionId);
                if (model != null)
                {
                    graduatioList.NyscListVms = model;
                    int schoolProgramId = model[0].SchoolProgrammeId;
                    graduatioList.ClassOfHonour = _db.ClassDegrees.AsNoTracking().Where(x => x.SchoolProgrammeId.Equals(schoolProgramId))
                                                    .Select(s => s.Remark).ToList();
                    char c1 = 'A';
                    ExcelPackage package = new ExcelPackage();
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Report");


                    var Rng = worksheet.Cells["B1"];
                    Rng.Value = "UNIVERSITY OF JOS";
                    Rng.Style.Font.Size = 14;
                    Rng.Style.Font.Bold = true;
                    var Rng1 = worksheet.Cells["B2"];
                    Rng1.Value = $"FACULTY OF {programme.Department.Faculty.FacultyName.ToUpper()}";
                    Rng1.Style.Font.Bold = true;
                    var Rng2 = worksheet.Cells["B3"];
                    Rng2.Value = $"DEPARTMENT OF {programme.Department.DeptName.ToUpper()} ";
                    Rng2.Style.Font.Bold = true;


                    worksheet.Cells[$"{c1++}5"].Value = "";
                    worksheet.Cells[$"{c1++}5"].Value = "Matric No";
                    worksheet.Cells[$"{c1++}5"].Value = "Surname";
                    worksheet.Cells[$"{c1++}5"].Value = "Other Names";


                    int rowStart = 6;
                    //char c2 = 'A';
                    foreach (var item in graduatioList.ClassOfHonour)
                    {                       
                        if (graduatioList.NyscListVms.Any(x => x.ClassOfDegree.Equals(item)))
                        {
                            worksheet.Cells[$"B{rowStart}"].Value = item;
                            rowStart++;
                            foreach (var list in graduatioList.NyscListVms.Where(x => x.ClassOfDegree.Equals(item)))
                            {
                                worksheet.Cells[$"A{rowStart}"].Value = "";
                                worksheet.Cells[$"B{rowStart}"].Value = list.MatricNo;
                                worksheet.Cells[$"C{rowStart}"].Value = list.Surname;
                                var wsb = worksheet.Cells[$"C{rowStart}"];
                                wsb.Style.Font.Bold = true;
                                worksheet.Cells[$"D{rowStart}"].Value = list.OtherName;
                                rowStart++;
                            }
                        }
                       
                    }
                    var info = graduatioList.NyscListVms.FirstOrDefault();
                    //worksheet.Cells["A:AZ"].AutoFitColumns();
                    worksheet.Column(1).Style.Locked = true;
                    worksheet.Column(2).Style.Locked = true;
                    worksheet.Column(3).Style.Locked = true;
                    Response.Clear();
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.AddHeader("content-disposition", "attachment: filename=" + $"{info.Course}NyscList.xlsx");
                    Response.BinaryWrite(package.GetAsByteArray());
                    Response.End();
                    Response.End();

                }
                Response.End();
            }
        }

        public ActionResult DeptResultFormat()
        {
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FancyName");
            ViewBag.FacultyId = new SelectList(_db.Faculties.AsNoTracking(), "FacultyId", "FacultyName");
            ViewBag.DepartmentId = new SelectList(_db.Departments.AsNoTracking(), "DepartmentId", "DeptName");
            ViewBag.ProgrammeId = new SelectList(_db.Programmes.AsNoTracking(), "ProgrammeId", "ProgrammeName");
            ViewBag.LevelId = new SelectList(_db.Levels.AsNoTracking().OrderBy(x => x.LevelId), "LevelId", "LevelName");
            ViewBag.SessionId = new SelectList(GetAllSession().OrderByDescending(x => x.SessionName), "SessionId", "SessionName");
            return View();
        }


        public ActionResult DeptSummaryResultFormat()
        {
            var staff = _db.Staffs.AsNoTracking().Where(x => x.Email.Equals(userId)).Select(x => new 
            { 
                deptId = x.Department.DepartmentId,
                factultyId = x.Department.FacultyId
            }).FirstOrDefault();

            if (User.IsInRole(RoleName.SuperAdmin) || User.IsInRole(RoleName.Admin) || User.IsInRole(RoleName.DAPM_Sup) || User.IsInRole(RoleName.DAPM_Officer))
            {
                ViewBag.FacultyId = new SelectList(_db.Faculties.AsNoTracking(), "FacultyId", "FacultyName");
                ViewBag.DepartmentId = new SelectList(_db.Departments.AsNoTracking(), "DepartmentId", "DeptName");
                ViewBag.ProgrammeId = new SelectList(_db.Programmes.AsNoTracking(), "ProgrammeId", "ProgrammeName");
            }
            else
            {
                ViewBag.FacultyId = new SelectList(_db.Faculties.AsNoTracking().Where(x => x.FacultyId == staff.factultyId), "FacultyId", "FacultyName");
                ViewBag.DepartmentId = new SelectList(_db.Departments.AsNoTracking().Where(x => x.DepartmentId == staff.deptId), "DepartmentId", "DeptName");
                ViewBag.ProgrammeId = new SelectList(_db.Programmes.AsNoTracking().Where(x => x.DepartmentId == staff.deptId), "ProgrammeId", "ProgrammeName");
            }
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.OrderBy(x => x.SchoolProgrammeId).AsNoTracking(), "SchoolProgrammeId", "FancyName");
            ViewBag.LevelId = new SelectList(_db.Levels.AsNoTracking().OrderBy(x => x.LevelId), "LevelId", "LevelName");
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            return View();
        }

        public ActionResult SummaryResultFormat()
        {
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FancyName");
            ViewBag.FacultyId = new SelectList(_db.Faculties.AsNoTracking(), "FacultyId", "FacultyName");
            ViewBag.DepartmentId = new SelectList(_db.Departments.AsNoTracking(), "DepartmentId", "DeptName");
            ViewBag.ProgrammeId = new SelectList(_db.Programmes.AsNoTracking(), "ProgrammeId", "ProgrammeName");
            ViewBag.LevelId = new SelectList(_db.Levels.AsNoTracking(), "LevelId", "LevelName");
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            return View();
        }

        [AllowAnonymous]
        public void Sample()
        {
            ExcelPackage package = new ExcelPackage();
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Report");

            var Rng = worksheet.Cells["A1"];
            Rng.Value = "Welcome to Everyday be coding - tutorials for beginners";
            Rng.Style.Font.Size = 16;
            Rng.Style.Font.Bold = true;
            Rng.Style.Font.Italic = true;
            var Rng2 = worksheet.Cells["A2"];
            Rng2.Value = "Department";
            Rng2.Style.Font.Size = 16;
            Rng2.Style.Font.Bold = true;
            Rng2.Style.Font.Italic = true;
            var Rng3 = worksheet.Cells["A3"];
            Rng3.Value = "Level";
            Rng3.Style.Font.Size = 16;
            Rng3.Style.Font.Bold = true;
            Rng3.Style.Font.Italic = true;

            var Rng4 = worksheet.Cells["A4"];
            Rng4.Value = "Course";
            Rng4.Style.Font.Size = 16;
            Rng4.Style.Font.Bold = true;
            Rng4.Style.Font.Italic = true;
            var Rng5 = worksheet.Cells["A5"];
            Rng5.Value = "Semester";
            Rng5.Style.Font.Size = 16;
            Rng5.Style.Font.Bold = true;
            Rng5.Style.Font.Italic = true;

            var Rng6 = worksheet.Cells["A6"];
            Rng6.Value = "Session";
            Rng6.Style.Font.Size = 16;
            Rng6.Style.Font.Bold = true;
            Rng6.Style.Font.Italic = true;

            worksheet.Column(1).Style.Locked = true;
            worksheet.Column(2).Style.Locked = true;
            worksheet.Column(3).Style.Locked = true;
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment: filename=" + "Senate Formart Result.xlsx");
            Response.BinaryWrite(package.GetAsByteArray());
            Response.End();

        }

        public async Task<PartialViewResult> DisplaySummaryDeptSenateFormat(int? programmeId, int? levelId, int? sessionId)
        {
            if (programmeId != null && levelId != null && sessionId != null)
            {
                //var model = await GetSummaryFormatData(programmeId, levelId, sessionId);
                var model = await _resultCommand.GetSummaryCompleteFormatData(programmeId, levelId, sessionId);

                if (model != null)
                {
                    model.OrderByDescending(x => x.ContinuousAssessments.Count());
                    return PartialView(model);
                }
                ViewBag.Message = "Please Set the Dept Result Template";
            }
            return PartialView();
        }

        public async Task<PartialViewResult> DisplaySummarySenateFormat(int? programmeId, int? levelId, int? sessionId)
        {
            if (programmeId != null && levelId != null && sessionId != null)
            {
                var model = await _resultCommand.GetSummaryCompleteFormatData(programmeId, levelId, sessionId);
                //var model = await GetSummaryFormatData(programmeId, levelId, sessionId);

                if (model != null)
                {
                    return PartialView(model);
                }
                ViewBag.Message = "Please Set the Dept Result Template";
            }
            return PartialView();
        }

        public async Task<PartialViewResult> DisplayDeptSenateFormat(int? programmeId, int? levelId, int? sessionId)
        {
            if (programmeId != null && levelId != null && sessionId != null)
            {
                var model = await GetFormatData(programmeId, levelId, sessionId);
                if (model != null)
                {
                    return PartialView(model);
                }
                ViewBag.Message = "Please Set the Dept Result Template";
            }
            return PartialView();
        }
        //public async Task DownloadDeptSenateFormat(int? programmeId, int? levelId, int? sessionId)
        //{
        //    var senateFormat = await GetFormatData(programmeId, levelId, sessionId);

        //    char c1 = 'A';
        //    ExcelPackage package = new ExcelPackage();
        //    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Report");

        //    worksheet.Cells[$"{c1++}1"].Value = "S/N";
        //    worksheet.Cells[$"{c1++}1"].Value = "Matric No";
        //    worksheet.Cells[$"{c1++}1"].Value = "Student Name";
        //    worksheet.Cells[$"{c1++}1"].Value = "ME";
        //    worksheet.Cells[$"{c1++}1"].Value = "MNSA";
        //    worksheet.Cells[$"{c1++}1"].Value = "NSS";

        //    foreach (var course in senateFormat.CourseCode)
        //    {
        //        worksheet.Cells[$"{c1++}1"].Value = course;
        //        worksheet.Cells[$"{c1++}1"].Value = "GP";
        //    }

        //    int rowStart = 2;


        //    for (var i = 0; i < senateFormat.DeptSanateFormatDetailVms.Count; i++)
        //    {
        //        char c2 = 'A';

        //        worksheet.Cells[$"{c2++}{rowStart}"].Value = senateFormat.DeptSanateFormatDetailVms[i].Sn;
        //        worksheet.Cells[$"{c2++}{rowStart}"].Value = senateFormat.DeptSanateFormatDetailVms[i].MatricNo;
        //        worksheet.Cells[$"{c2++}{rowStart}"].Value = senateFormat.DeptSanateFormatDetailVms[i].FullName;
        //        worksheet.Cells[$"{c2++}{rowStart}"].Value = senateFormat.DeptSanateFormatDetailVms[i].ModeOfEntry;
        //        worksheet.Cells[$"{c2++}{rowStart}"].Value = senateFormat.DeptSanateFormatDetailVms[i].MNSA;
        //        worksheet.Cells[$"{c2++}{rowStart}"].Value = senateFormat.DeptSanateFormatDetailVms[i].NSS;
        //        foreach (var code in senateFormat.CourseCode)
        //        {
        //            foreach (var item in senateFormat.DeptSanateFormatDetailVms[i].ContinuousAssessments.Where(x => x.Course.CourseCode.Equals(code)))
        //            {
        //                worksheet.Cells[$"{c2++}{rowStart}"].Value = item.Total;
        //                worksheet.Cells[$"{c2++}{rowStart}"].Value = item.GradePoint;
        //            }
        //        }

        //        rowStart++;
        //    }
        //    var info = senateFormat;
        //    worksheet.Cells["A:AZ"].AutoFitColumns();
        //    worksheet.Column(1).Style.Locked = true;
        //    worksheet.Column(2).Style.Locked = true;
        //    worksheet.Column(3).Style.Locked = true;
        //    Response.Clear();
        //    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        //    Response.AddHeader("content-disposition", "attachment: filename=" + "Senate Formart Result.xlsx");
        //    Response.BinaryWrite(package.GetAsByteArray());
        //    Response.End();

        //}


        // Excel column helper method to convert numbers to column names (A, B, ..., Z, AA, AB, etc.)
        public string GetExcelColumnName(int columnNumber)
        {
            int dividend = columnNumber;
            string columnName = String.Empty;
            while (dividend > 0)
            {
                int modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo) + columnName;
                dividend = (dividend - modulo) / 26;
            }
            return columnName;
        }

        public async Task DownloadDeptSenateFormat(int? programmeId, int? levelId, int? sessionId)
        {
            var senateFormat = await GetFormatData(programmeId, levelId, sessionId);

            ExcelPackage package = new ExcelPackage();
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Report");

            int columnNumber = 1; // Start with the first column (Excel column 'A')

            // Add headers
            worksheet.Cells[$"{GetExcelColumnName(columnNumber++)}1"].Value = "S/N";
            worksheet.Cells[$"{GetExcelColumnName(columnNumber++)}1"].Value = "Matric No";
            worksheet.Cells[$"{GetExcelColumnName(columnNumber++)}1"].Value = "Student Name";
            worksheet.Cells[$"{GetExcelColumnName(columnNumber++)}1"].Value = "ME";
            worksheet.Cells[$"{GetExcelColumnName(columnNumber++)}1"].Value = "MNSA";
            worksheet.Cells[$"{GetExcelColumnName(columnNumber++)}1"].Value = "NSS";

            // Loop through course codes and add them to headers along with "GP"
            foreach (var course in senateFormat.CourseCode)
            {
                worksheet.Cells[$"{GetExcelColumnName(columnNumber++)}1"].Value = course;
                worksheet.Cells[$"{GetExcelColumnName(columnNumber++)}1"].Value = "GP";
            }

            int rowStart = 2; // Start from the second row for data

            // Loop through the student data and add them to rows
            for (var i = 0; i < senateFormat.DeptSanateFormatDetailVms.Count; i++)
            {
                int dataColumnNumber = 1; // Start from the first column again for data

                worksheet.Cells[$"{GetExcelColumnName(dataColumnNumber++)}{rowStart}"].Value = senateFormat.DeptSanateFormatDetailVms[i].Sn;
                worksheet.Cells[$"{GetExcelColumnName(dataColumnNumber++)}{rowStart}"].Value = senateFormat.DeptSanateFormatDetailVms[i].MatricNo;
                worksheet.Cells[$"{GetExcelColumnName(dataColumnNumber++)}{rowStart}"].Value = senateFormat.DeptSanateFormatDetailVms[i].FullName;
                worksheet.Cells[$"{GetExcelColumnName(dataColumnNumber++)}{rowStart}"].Value = senateFormat.DeptSanateFormatDetailVms[i].ModeOfEntry;
                worksheet.Cells[$"{GetExcelColumnName(dataColumnNumber++)}{rowStart}"].Value = senateFormat.DeptSanateFormatDetailVms[i].MNSA;
                worksheet.Cells[$"{GetExcelColumnName(dataColumnNumber++)}{rowStart}"].Value = senateFormat.DeptSanateFormatDetailVms[i].NSS;

                // Loop through the course codes for each student
                foreach (var code in senateFormat.CourseCode)
                {
                    foreach (var item in senateFormat.DeptSanateFormatDetailVms[i].ContinuousAssessments.Where(x => x.Course.CourseCode.Equals(code)))
                    {
                        worksheet.Cells[$"{GetExcelColumnName(dataColumnNumber++)}{rowStart}"].Value = item.Total;
                        worksheet.Cells[$"{GetExcelColumnName(dataColumnNumber++)}{rowStart}"].Value = item.GradePoint;
                    }
                }

                rowStart++; // Move to the next row for the next student
            }

            // Auto fit columns and lock the first few columns as necessary
            worksheet.Cells["A:AZ"].AutoFitColumns();
            worksheet.Column(1).Style.Locked = true;
            worksheet.Column(2).Style.Locked = true;
            worksheet.Column(3).Style.Locked = true;

            // Send the Excel file as a downloadable response
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment; filename=" + "Senate_Format_Result.xlsx");
            Response.BinaryWrite(package.GetAsByteArray());
            Response.End();
        }

        //public async Task DownloadSummarySenateFormat(int? programmeId, int? levelId, int? sessionId)
        //{
        //    var summarySenate = await _resultCommand.GetSummaryCompleteFormatData(programmeId, levelId, sessionId);
        //    var cA = summarySenate.FirstOrDefault();

        //    char c1 = 'A';
        //    ExcelPackage package = new ExcelPackage();
        //    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Report");

        //    worksheet.Cells[$"{c1++}1"].Value = "S/N";
        //    worksheet.Cells[$"{c1++}1"].Value = "Matric No";
        //    worksheet.Cells[$"{c1++}1"].Value = "Student Name";
        //    worksheet.Cells[$"{c1++}1"].Value = "ME";
        //    worksheet.Cells[$"{c1++}1"].Value = "MNSA";
        //    worksheet.Cells[$"{c1++}1"].Value = "NSS";
        //    if (cA.SessionParameters.Count.Equals(4))
        //    {
        //        worksheet.Cells[$"{c1++}1"].Value = "100 LEVEL TCR";
        //        worksheet.Cells[$"{c1++}1"].Value = "TCE";
        //        worksheet.Cells[$"{c1++}1"].Value = "TGP";
        //        worksheet.Cells[$"{c1++}1"].Value = "GPA";

        //        worksheet.Cells[$"{c1++}1"].Value = "200 LEVEL TCR";
        //        worksheet.Cells[$"{c1++}1"].Value = "TCE";
        //        worksheet.Cells[$"{c1++}1"].Value = "TGP";
        //        worksheet.Cells[$"{c1++}1"].Value = "GPA";
        //        worksheet.Cells[$"{c1++}1"].Value = "CTCR";
        //        worksheet.Cells[$"{c1++}1"].Value = "CTCE";
        //        worksheet.Cells[$"{c1++}1"].Value = "CTGP";
        //        worksheet.Cells[$"{c1++}1"].Value = "CGPA";

        //        worksheet.Cells[$"{c1++}1"].Value = "300 LEVEL TCR";
        //        worksheet.Cells[$"{c1++}1"].Value = "TCE";
        //        worksheet.Cells[$"{c1++}1"].Value = "TGP";
        //        worksheet.Cells[$"{c1++}1"].Value = "GPA";
        //        worksheet.Cells[$"{c1++}1"].Value = "CTCR";
        //        worksheet.Cells[$"{c1++}1"].Value = "CTCE";
        //        worksheet.Cells[$"{c1++}1"].Value = "CTGP";
        //        worksheet.Cells[$"{c1++}1"].Value = "CGPA";

        //        worksheet.Cells[$"{c1++}1"].Value = "400 LEVEL TCR";
        //        worksheet.Cells[$"{c1++}1"].Value = "TCE";
        //        worksheet.Cells[$"{c1++}1"].Value = "TGP";
        //        worksheet.Cells[$"{c1++}1"].Value = "GPA";
        //        worksheet.Cells[$"{c1++}1"].Value = "CTCR";
        //        worksheet.Cells[$"{c1++}1"].Value = "CTCE";
        //        worksheet.Cells[$"{c1++}1"].Value = "CTGP";
        //        worksheet.Cells[$"{c1++}1"].Value = "CGPA";
        //    }
        //    if (cA.SessionParameters.Count.Equals(3))
        //    {
        //        worksheet.Cells[$"{c1++}1"].Value = "100 LEVEL TCR";
        //        worksheet.Cells[$"{c1++}1"].Value = "TCE";
        //        worksheet.Cells[$"{c1++}1"].Value = "TGP";
        //        worksheet.Cells[$"{c1++}1"].Value = "GPA";

        //        worksheet.Cells[$"{c1++}1"].Value = "200 LEVEL TCR";
        //        worksheet.Cells[$"{c1++}1"].Value = "TCE";
        //        worksheet.Cells[$"{c1++}1"].Value = "TGP";
        //        worksheet.Cells[$"{c1++}1"].Value = "GPA";
        //        worksheet.Cells[$"{c1++}1"].Value = "CTCR";
        //        worksheet.Cells[$"{c1++}1"].Value = "CTCE";
        //        worksheet.Cells[$"{c1++}1"].Value = "CTGP";
        //        worksheet.Cells[$"{c1++}1"].Value = "CGPA";

        //        worksheet.Cells[$"{c1++}1"].Value = "300 LEVEL TCR";
        //        worksheet.Cells[$"{c1++}1"].Value = "TCE";
        //        worksheet.Cells[$"{c1++}1"].Value = "TGP";
        //        worksheet.Cells[$"{c1++}1"].Value = "GPA";
        //        worksheet.Cells[$"{c1++}1"].Value = "CTCR";
        //        worksheet.Cells[$"{c1++}1"].Value = "CTCE";
        //        worksheet.Cells[$"{c1++}1"].Value = "CTGP";
        //        worksheet.Cells[$"{c1++}1"].Value = "CGPA";
        //    }
        //    if (cA.SessionParameters.Count.Equals(2))
        //    {
        //        worksheet.Cells[$"{c1++}1"].Value = "100 LEVEL TCR";
        //        worksheet.Cells[$"{c1++}1"].Value = "TCE";
        //        worksheet.Cells[$"{c1++}1"].Value = "TGP";
        //        worksheet.Cells[$"{c1++}1"].Value = "GPA";

        //        worksheet.Cells[$"{c1++}1"].Value = "200 LEVEL TCR";
        //        worksheet.Cells[$"{c1++}1"].Value = "TCE";
        //        worksheet.Cells[$"{c1++}1"].Value = "TGP";
        //        worksheet.Cells[$"{c1++}1"].Value = "GPA";
        //        worksheet.Cells[$"{c1++}1"].Value = "CTCR";
        //        worksheet.Cells[$"{c1++}1"].Value = "CTCE";
        //        worksheet.Cells[$"{c1++}1"].Value = "CTGP";
        //        worksheet.Cells[$"{c1++}1"].Value = "CGPA";
        //    }
        //    if (cA.SessionParameters.Count.Equals(2))
        //    {
        //        worksheet.Cells[$"{c1++}1"].Value = "TCR";
        //        worksheet.Cells[$"{c1++}1"].Value = "TCE";
        //        worksheet.Cells[$"{c1++}1"].Value = "TGP";
        //        worksheet.Cells[$"{c1++}1"].Value = "GPA";
        //    }
        //    foreach (var course in cA.ContinuousAssessments)
        //    {
        //        worksheet.Cells[$"{c1++}1"].Value = course.Course.CourseCode;
        //        worksheet.Cells[$"{c1++}1"].Value = "GP";
        //    }
        //    worksheet.Cells[$"{c1++}1"].Value = "Remark";

        //    int rowStart = 2;
        //    for (var i = 0; i < summarySenate.Count; i++)
        //    {
        //        char c2 = 'A';

        //        worksheet.Cells[$"{c2++}{rowStart}"].Value = summarySenate[i].Sn;
        //        worksheet.Cells[$"{c2++}{rowStart}"].Value = summarySenate[i].MatricNo;
        //        worksheet.Cells[$"{c2++}{rowStart}"].Value = summarySenate[i].FullName;
        //        worksheet.Cells[$"{c2++}{rowStart}"].Value = summarySenate[i].ModeOfEntry;
        //        worksheet.Cells[$"{c2++}{rowStart}"].Value = summarySenate[i].MNSA;
        //        worksheet.Cells[$"{c2++}{rowStart}"].Value = summarySenate[i].NSS;
        //        if (summarySenate[i].SessionParameters.Count.Equals(4))
        //        {
        //            worksheet.Cells[$"{c2++}{rowStart}"].Value = summarySenate[i].SessionParameters[3].TCR;
        //            worksheet.Cells[$"{c2++}{rowStart}"].Value = summarySenate[i].SessionParameters[3].TCE;
        //            worksheet.Cells[$"{c2++}{rowStart}"].Value = summarySenate[i].SessionParameters[3].TGP;
        //            worksheet.Cells[$"{c2++}{rowStart}"].Value = summarySenate[i].SessionParameters[3].TGP;

        //            worksheet.Cells[$"{c2++}{rowStart}"].Value = summarySenate[i].SessionParameters[2].TCR;
        //            worksheet.Cells[$"{c2++}{rowStart}"].Value = summarySenate[i].SessionParameters[2].TCE;
        //            worksheet.Cells[$"{c2++}{rowStart}"].Value = summarySenate[i].SessionParameters[2].TGP;
        //            worksheet.Cells[$"{c2++}{rowStart}"].Value = summarySenate[i].SessionParameters[2].TGP;
        //            worksheet.Cells[$"{c2++}{rowStart}"].Value = summarySenate[i].SessionParameters[2].CTCR;
        //            worksheet.Cells[$"{c2++}{rowStart}"].Value = summarySenate[i].SessionParameters[2].CTCE;
        //            worksheet.Cells[$"{c2++}{rowStart}"].Value = summarySenate[i].SessionParameters[2].CTGP;
        //            worksheet.Cells[$"{c2++}{rowStart}"].Value = summarySenate[i].SessionParameters[2].CTGP;

        //            worksheet.Cells[$"{c2++}{rowStart}"].Value = summarySenate[i].SessionParameters[1].TCR;
        //            worksheet.Cells[$"{c2++}{rowStart}"].Value = summarySenate[i].SessionParameters[1].TCE;
        //            worksheet.Cells[$"{c2++}{rowStart}"].Value = summarySenate[i].SessionParameters[1].TGP;
        //            worksheet.Cells[$"{c2++}{rowStart}"].Value = summarySenate[i].SessionParameters[1].TGP;
        //            worksheet.Cells[$"{c2++}{rowStart}"].Value = summarySenate[i].SessionParameters[1].CTCR;
        //            worksheet.Cells[$"{c2++}{rowStart}"].Value = summarySenate[i].SessionParameters[1].CTCE;
        //            worksheet.Cells[$"{c2++}{rowStart}"].Value = summarySenate[i].SessionParameters[1].CTGP;
        //            worksheet.Cells[$"{c2++}{rowStart}"].Value = summarySenate[i].SessionParameters[1].CTGP;

        //            worksheet.Cells[$"{c2++}{rowStart}"].Value = summarySenate[i].SessionParameters[0].TCR;
        //            worksheet.Cells[$"{c2++}{rowStart}"].Value = summarySenate[i].SessionParameters[0].TCE;
        //            worksheet.Cells[$"{c2++}{rowStart}"].Value = summarySenate[i].SessionParameters[0].TGP;
        //            worksheet.Cells[$"{c2++}{rowStart}"].Value = summarySenate[i].SessionParameters[0].TGP;
        //            worksheet.Cells[$"{c2++}{rowStart}"].Value = summarySenate[i].SessionParameters[0].CTCR;
        //            worksheet.Cells[$"{c2++}{rowStart}"].Value = summarySenate[i].SessionParameters[0].CTCE;
        //            worksheet.Cells[$"{c2++}{rowStart}"].Value = summarySenate[i].SessionParameters[0].CTGP;
        //            worksheet.Cells[$"{c2++}{rowStart}"].Value = summarySenate[i].SessionParameters[0].CTGP;
        //        }
        //        if (summarySenate[i].SessionParameters.Count.Equals(3))
        //        {
        //            worksheet.Cells[$"{c2++}{rowStart}"].Value = summarySenate[i].SessionParameters[2].TCR;
        //            worksheet.Cells[$"{c2++}{rowStart}"].Value = summarySenate[i].SessionParameters[2].TCE;
        //            worksheet.Cells[$"{c2++}{rowStart}"].Value = summarySenate[i].SessionParameters[2].TGP;
        //            worksheet.Cells[$"{c2++}{rowStart}"].Value = summarySenate[i].SessionParameters[2].TGP;

        //            worksheet.Cells[$"{c2++}{rowStart}"].Value = summarySenate[i].SessionParameters[1].TCR;
        //            worksheet.Cells[$"{c2++}{rowStart}"].Value = summarySenate[i].SessionParameters[1].TCE;
        //            worksheet.Cells[$"{c2++}{rowStart}"].Value = summarySenate[i].SessionParameters[1].TGP;
        //            worksheet.Cells[$"{c2++}{rowStart}"].Value = summarySenate[i].SessionParameters[1].TGP;
        //            worksheet.Cells[$"{c2++}{rowStart}"].Value = summarySenate[i].SessionParameters[1].CTCR;
        //            worksheet.Cells[$"{c2++}{rowStart}"].Value = summarySenate[i].SessionParameters[1].CTCE;
        //            worksheet.Cells[$"{c2++}{rowStart}"].Value = summarySenate[i].SessionParameters[1].CTGP;
        //            worksheet.Cells[$"{c2++}{rowStart}"].Value = summarySenate[i].SessionParameters[1].CTGP;

        //            worksheet.Cells[$"{c2++}{rowStart}"].Value = summarySenate[i].SessionParameters[0].TCR;
        //            worksheet.Cells[$"{c2++}{rowStart}"].Value = summarySenate[i].SessionParameters[0].TCE;
        //            worksheet.Cells[$"{c2++}{rowStart}"].Value = summarySenate[i].SessionParameters[0].TGP;
        //            worksheet.Cells[$"{c2++}{rowStart}"].Value = summarySenate[i].SessionParameters[0].TGP;
        //            worksheet.Cells[$"{c2++}{rowStart}"].Value = summarySenate[i].SessionParameters[0].CTCR;
        //            worksheet.Cells[$"{c2++}{rowStart}"].Value = summarySenate[i].SessionParameters[0].CTCE;
        //            worksheet.Cells[$"{c2++}{rowStart}"].Value = summarySenate[i].SessionParameters[0].CTGP;
        //            worksheet.Cells[$"{c2++}{rowStart}"].Value = summarySenate[i].SessionParameters[0].CTGP;
        //        }
        //        if (summarySenate[i].SessionParameters.Count.Equals(2))
        //        {
        //            worksheet.Cells[$"{c2++}{rowStart}"].Value = summarySenate[i].SessionParameters[1].TCR;
        //            worksheet.Cells[$"{c2++}{rowStart}"].Value = summarySenate[i].SessionParameters[1].TCE;
        //            worksheet.Cells[$"{c2++}{rowStart}"].Value = summarySenate[i].SessionParameters[1].TGP;
        //            worksheet.Cells[$"{c2++}{rowStart}"].Value = summarySenate[i].SessionParameters[1].TGP;

        //            worksheet.Cells[$"{c2++}{rowStart}"].Value = summarySenate[i].SessionParameters[0].TCR;
        //            worksheet.Cells[$"{c2++}{rowStart}"].Value = summarySenate[i].SessionParameters[0].TCE;
        //            worksheet.Cells[$"{c2++}{rowStart}"].Value = summarySenate[i].SessionParameters[0].TGP;
        //            worksheet.Cells[$"{c2++}{rowStart}"].Value = summarySenate[i].SessionParameters[0].TGP;
        //            worksheet.Cells[$"{c2++}{rowStart}"].Value = summarySenate[i].SessionParameters[0].CTCR;
        //            worksheet.Cells[$"{c2++}{rowStart}"].Value = summarySenate[i].SessionParameters[0].CTCE;
        //            worksheet.Cells[$"{c2++}{rowStart}"].Value = summarySenate[i].SessionParameters[0].CTGP;
        //            worksheet.Cells[$"{c2++}{rowStart}"].Value = summarySenate[i].SessionParameters[0].CTGP;
        //        }
        //        if (summarySenate[i].SessionParameters.Count.Equals(1))
        //        {
        //            worksheet.Cells[$"{c2++}{rowStart}"].Value = summarySenate[i].SessionParameters[0].TCR;
        //            worksheet.Cells[$"{c2++}{rowStart}"].Value = summarySenate[i].SessionParameters[0].TCE;
        //            worksheet.Cells[$"{c2++}{rowStart}"].Value = summarySenate[i].SessionParameters[0].TGP;
        //            worksheet.Cells[$"{c2++}{rowStart}"].Value = summarySenate[i].SessionParameters[0].TGP;
        //        }

        //        foreach (var ca in summarySenate[i].ContinuousAssessments)
        //        {
        //            worksheet.Cells[$"{c2++}{rowStart}"].Value = ca.Total;
        //            worksheet.Cells[$"{c2++}{rowStart}"].Value = ca.GradePoint;
        //        }
        //        worksheet.Cells[$"{c2++}{rowStart}"].Value = summarySenate[i].Remarks;

        //        rowStart++;
        //    }
        //    var info = summarySenate;
        //    worksheet.Cells["A:AZ"].AutoFitColumns();
        //    worksheet.Column(1).Style.Locked = true;
        //    worksheet.Column(2).Style.Locked = true;
        //    worksheet.Column(3).Style.Locked = true;
        //    Response.Clear();
        //    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        //    Response.AddHeader("content-disposition", "attachment: filename=" + "Senate Summary Formart Result.xlsx");
        //    Response.BinaryWrite(package.GetAsByteArray());
        //    Response.End();

        //}

        public async Task DownloadSummarySenateFormat(int? programmeId, int? levelId, int? sessionId)
        {
            var summarySenate = await _resultCommand.GetSummaryCompleteFormatData(programmeId, levelId, sessionId);
            var senateFormat = await GetFormatData(programmeId, levelId, sessionId);
            var cA = summarySenate.FirstOrDefault();

            int columnIndex = 1; // Start with 1 for column A
            ExcelPackage package = new ExcelPackage();
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Report");

            worksheet.Cells[$"{GetExcelColumnName(columnIndex++)}1"].Value = "S/N";
            worksheet.Cells[$"{GetExcelColumnName(columnIndex++)}1"].Value = "Matric No";
            worksheet.Cells[$"{GetExcelColumnName(columnIndex++)}1"].Value = "Student Name";
            worksheet.Cells[$"{GetExcelColumnName(columnIndex++)}1"].Value = "ME";
            worksheet.Cells[$"{GetExcelColumnName(columnIndex++)}1"].Value = "MNSA";
            worksheet.Cells[$"{GetExcelColumnName(columnIndex++)}1"].Value = "NSS";

            if (cA.SessionParameters.Count == 4)
            {
                worksheet.Cells[$"{GetExcelColumnName(columnIndex++)}1"].Value = "100 LEVEL TCR";
                worksheet.Cells[$"{GetExcelColumnName(columnIndex++)}1"].Value = "TCE";
                worksheet.Cells[$"{GetExcelColumnName(columnIndex++)}1"].Value = "TGP";
                worksheet.Cells[$"{GetExcelColumnName(columnIndex++)}1"].Value = "GPA";

                worksheet.Cells[$"{GetExcelColumnName(columnIndex++)}1"].Value = "200 LEVEL TCR";
                worksheet.Cells[$"{GetExcelColumnName(columnIndex++)}1"].Value = "TCE";
                worksheet.Cells[$"{GetExcelColumnName(columnIndex++)}1"].Value = "TGP";
                worksheet.Cells[$"{GetExcelColumnName(columnIndex++)}1"].Value = "GPA";
                worksheet.Cells[$"{GetExcelColumnName(columnIndex++)}1"].Value = "CTCR";
                worksheet.Cells[$"{GetExcelColumnName(columnIndex++)}1"].Value = "CTCE";
                worksheet.Cells[$"{GetExcelColumnName(columnIndex++)}1"].Value = "CTGP";
                worksheet.Cells[$"{GetExcelColumnName(columnIndex++)}1"].Value = "CGPA";

                worksheet.Cells[$"{GetExcelColumnName(columnIndex++)}1"].Value = "300 LEVEL TCR";
                worksheet.Cells[$"{GetExcelColumnName(columnIndex++)}1"].Value = "TCE";
                worksheet.Cells[$"{GetExcelColumnName(columnIndex++)}1"].Value = "TGP";
                worksheet.Cells[$"{GetExcelColumnName(columnIndex++)}1"].Value = "GPA";
                worksheet.Cells[$"{GetExcelColumnName(columnIndex++)}1"].Value = "CTCR";
                worksheet.Cells[$"{GetExcelColumnName(columnIndex++)}1"].Value = "CTCE";
                worksheet.Cells[$"{GetExcelColumnName(columnIndex++)}1"].Value = "CTGP";
                worksheet.Cells[$"{GetExcelColumnName(columnIndex++)}1"].Value = "CGPA";

                worksheet.Cells[$"{GetExcelColumnName(columnIndex++)}1"].Value = "400 LEVEL TCR";
                worksheet.Cells[$"{GetExcelColumnName(columnIndex++)}1"].Value = "TCE";
                worksheet.Cells[$"{GetExcelColumnName(columnIndex++)}1"].Value = "TGP";
                worksheet.Cells[$"{GetExcelColumnName(columnIndex++)}1"].Value = "GPA";
                worksheet.Cells[$"{GetExcelColumnName(columnIndex++)}1"].Value = "CTCR";
                worksheet.Cells[$"{GetExcelColumnName(columnIndex++)}1"].Value = "CTCE";
                worksheet.Cells[$"{GetExcelColumnName(columnIndex++)}1"].Value = "CTGP";
                worksheet.Cells[$"{GetExcelColumnName(columnIndex++)}1"].Value = "CGPA";
            }

            if (cA.SessionParameters.Count == 3)
            {
                worksheet.Cells[$"{GetExcelColumnName(columnIndex++)}1"].Value = "100 LEVEL TCR";
                worksheet.Cells[$"{GetExcelColumnName(columnIndex++)}1"].Value = "TCE";
                worksheet.Cells[$"{GetExcelColumnName(columnIndex++)}1"].Value = "TGP";
                worksheet.Cells[$"{GetExcelColumnName(columnIndex++)}1"].Value = "GPA";

                worksheet.Cells[$"{GetExcelColumnName(columnIndex++)}1"].Value = "200 LEVEL TCR";
                worksheet.Cells[$"{GetExcelColumnName(columnIndex++)}1"].Value = "TCE";
                worksheet.Cells[$"{GetExcelColumnName(columnIndex++)}1"].Value = "TGP";
                worksheet.Cells[$"{GetExcelColumnName(columnIndex++)}1"].Value = "GPA";
                worksheet.Cells[$"{GetExcelColumnName(columnIndex++)}1"].Value = "CTCR";
                worksheet.Cells[$"{GetExcelColumnName(columnIndex++)}1"].Value = "CTCE";
                worksheet.Cells[$"{GetExcelColumnName(columnIndex++)}1"].Value = "CTGP";
                worksheet.Cells[$"{GetExcelColumnName(columnIndex++)}1"].Value = "CGPA";

                worksheet.Cells[$"{GetExcelColumnName(columnIndex++)}1"].Value = "300 LEVEL TCR";
                worksheet.Cells[$"{GetExcelColumnName(columnIndex++)}1"].Value = "TCE";
                worksheet.Cells[$"{GetExcelColumnName(columnIndex++)}1"].Value = "TGP";
                worksheet.Cells[$"{GetExcelColumnName(columnIndex++)}1"].Value = "GPA";
                worksheet.Cells[$"{GetExcelColumnName(columnIndex++)}1"].Value = "CTCR";
                worksheet.Cells[$"{GetExcelColumnName(columnIndex++)}1"].Value = "CTCE";
                worksheet.Cells[$"{GetExcelColumnName(columnIndex++)}1"].Value = "CTGP";
                worksheet.Cells[$"{GetExcelColumnName(columnIndex++)}1"].Value = "CGPA";
            }

            if (cA.SessionParameters.Count == 2)
            {
                worksheet.Cells[$"{GetExcelColumnName(columnIndex++)}1"].Value = "100 LEVEL TCR";
                worksheet.Cells[$"{GetExcelColumnName(columnIndex++)}1"].Value = "TCE";
                worksheet.Cells[$"{GetExcelColumnName(columnIndex++)}1"].Value = "TGP";
                worksheet.Cells[$"{GetExcelColumnName(columnIndex++)}1"].Value = "GPA";

                worksheet.Cells[$"{GetExcelColumnName(columnIndex++)}1"].Value = "200 LEVEL TCR";
                worksheet.Cells[$"{GetExcelColumnName(columnIndex++)}1"].Value = "TCE";
                worksheet.Cells[$"{GetExcelColumnName(columnIndex++)}1"].Value = "TGP";
                worksheet.Cells[$"{GetExcelColumnName(columnIndex++)}1"].Value = "GPA";
                worksheet.Cells[$"{GetExcelColumnName(columnIndex++)}1"].Value = "CTCR";
                worksheet.Cells[$"{GetExcelColumnName(columnIndex++)}1"].Value = "CTCE";
                worksheet.Cells[$"{GetExcelColumnName(columnIndex++)}1"].Value = "CTGP";
                worksheet.Cells[$"{GetExcelColumnName(columnIndex++)}1"].Value = "CGPA";
            }

            if (cA.SessionParameters.Count.Equals(1))
            {
                worksheet.Cells[$"{GetExcelColumnName(columnIndex++)}1"].Value = "TCR";
                worksheet.Cells[$"{GetExcelColumnName(columnIndex++)}1"].Value = "TCE";
                worksheet.Cells[$"{GetExcelColumnName(columnIndex++)}1"].Value = "TGP";
                worksheet.Cells[$"{GetExcelColumnName(columnIndex++)}1"].Value = "GPA";
            }

            // Loop through course codes and add them to headers along with "GP"
            foreach (var course in senateFormat.CourseCode)
            {
                worksheet.Cells[$"{GetExcelColumnName(columnIndex)}1"].Value = course;  // Add course code to header
                columnIndex++;
                worksheet.Cells[$"{GetExcelColumnName(columnIndex)}1"].Value = "GP";    // Add GP to header
                columnIndex++;
            }

            // Start adding student data
            int rowStart = 2; // Start from the second row (since the first is for headers)
            foreach (var student in summarySenate)
            {
                int rowColumnIndex = 1; // Start again for each row

                worksheet.Cells[$"{GetExcelColumnName(rowColumnIndex++)}{rowStart}"].Value = student.Sn;
                worksheet.Cells[$"{GetExcelColumnName(rowColumnIndex++)}{rowStart}"].Value = student.MatricNo;
                worksheet.Cells[$"{GetExcelColumnName(rowColumnIndex++)}{rowStart}"].Value = student.FullName;
                worksheet.Cells[$"{GetExcelColumnName(rowColumnIndex++)}{rowStart}"].Value = student.ModeOfEntry;
                worksheet.Cells[$"{GetExcelColumnName(rowColumnIndex++)}{rowStart}"].Value = student.MNSA;
                worksheet.Cells[$"{GetExcelColumnName(rowColumnIndex++)}{rowStart}"].Value = student.NSS;

                if (student.SessionParameters.Count.Equals(4))
                {
                    worksheet.Cells[$"{GetExcelColumnName(rowColumnIndex++)}{rowStart}"].Value = student.SessionParameters[3].TCR;
                    worksheet.Cells[$"{GetExcelColumnName(rowColumnIndex++)}{rowStart}"].Value = student.SessionParameters[3].TCE;
                    worksheet.Cells[$"{GetExcelColumnName(rowColumnIndex++)}{rowStart}"].Value = student.SessionParameters[3].TGP;
                    worksheet.Cells[$"{GetExcelColumnName(rowColumnIndex++)}{rowStart}"].Value = student.SessionParameters[3].TGP;

                    worksheet.Cells[$"{GetExcelColumnName(rowColumnIndex++)}{rowStart}"].Value = student.SessionParameters[2].TCR;
                    worksheet.Cells[$"{GetExcelColumnName(rowColumnIndex++)}{rowStart}"].Value = student.SessionParameters[2].TCE;
                    worksheet.Cells[$"{GetExcelColumnName(rowColumnIndex++)}{rowStart}"].Value = student.SessionParameters[2].TGP;
                    worksheet.Cells[$"{GetExcelColumnName(rowColumnIndex++)}{rowStart}"].Value = student.SessionParameters[2].TGP;
                    worksheet.Cells[$"{GetExcelColumnName(rowColumnIndex++)}{rowStart}"].Value = student.SessionParameters[2].CTCR;
                    worksheet.Cells[$"{GetExcelColumnName(rowColumnIndex++)}{rowStart}"].Value = student.SessionParameters[2].CTCE;
                    worksheet.Cells[$"{GetExcelColumnName(rowColumnIndex++)}{rowStart}"].Value = student.SessionParameters[2].CTGP;
                    worksheet.Cells[$"{GetExcelColumnName(rowColumnIndex++)}{rowStart}"].Value = student.SessionParameters[2].CTGP;

                    worksheet.Cells[$"{GetExcelColumnName(rowColumnIndex++)}{rowStart}"].Value = student.SessionParameters[1].TCR;
                    worksheet.Cells[$"{GetExcelColumnName(rowColumnIndex++)}{rowStart}"].Value = student.SessionParameters[1].TCE;
                    worksheet.Cells[$"{GetExcelColumnName(rowColumnIndex++)}{rowStart}"].Value = student.SessionParameters[1].TGP;
                    worksheet.Cells[$"{GetExcelColumnName(rowColumnIndex++)}{rowStart}"].Value = student.SessionParameters[1].TGP;
                    worksheet.Cells[$"{GetExcelColumnName(rowColumnIndex++)}{rowStart}"].Value = student.SessionParameters[1].CTCR;
                    worksheet.Cells[$"{GetExcelColumnName(rowColumnIndex++)}{rowStart}"].Value = student.SessionParameters[1].CTCE;
                    worksheet.Cells[$"{GetExcelColumnName(rowColumnIndex++)}{rowStart}"].Value = student.SessionParameters[1].CTGP;
                    worksheet.Cells[$"{GetExcelColumnName(rowColumnIndex++)}{rowStart}"].Value = student.SessionParameters[1].CTGP;

                    worksheet.Cells[$"{GetExcelColumnName(rowColumnIndex++)}{rowStart}"].Value = student.SessionParameters[0].TCR;
                    worksheet.Cells[$"{GetExcelColumnName(rowColumnIndex++)}{rowStart}"].Value = student.SessionParameters[0].TCE;
                    worksheet.Cells[$"{GetExcelColumnName(rowColumnIndex++)}{rowStart}"].Value = student.SessionParameters[0].TGP;
                    worksheet.Cells[$"{GetExcelColumnName(rowColumnIndex++)}{rowStart}"].Value = student.SessionParameters[0].TGP;
                    worksheet.Cells[$"{GetExcelColumnName(rowColumnIndex++)}{rowStart}"].Value = student.SessionParameters[0].CTCR;
                    worksheet.Cells[$"{GetExcelColumnName(rowColumnIndex++)}{rowStart}"].Value = student.SessionParameters[0].CTCE;
                    worksheet.Cells[$"{GetExcelColumnName(rowColumnIndex++)}{rowStart}"].Value = student.SessionParameters[0].CTGP;
                    worksheet.Cells[$"{GetExcelColumnName(rowColumnIndex++)}{rowStart}"].Value = student.SessionParameters[0].CTGP;
                }

                if (student.SessionParameters.Count.Equals(3))
                {
                    worksheet.Cells[$"{GetExcelColumnName(rowColumnIndex++)}{rowStart}"].Value = student.SessionParameters[2].TCR;
                    worksheet.Cells[$"{GetExcelColumnName(rowColumnIndex++)}{rowStart}"].Value = student.SessionParameters[2].TCE;
                    worksheet.Cells[$"{GetExcelColumnName(rowColumnIndex++)}{rowStart}"].Value = student.SessionParameters[2].TGP;
                    worksheet.Cells[$"{GetExcelColumnName(rowColumnIndex++)}{rowStart}"].Value = student.SessionParameters[2].TGP;

                    worksheet.Cells[$"{GetExcelColumnName(rowColumnIndex++)}{rowStart}"].Value = student.SessionParameters[1].TCR;
                    worksheet.Cells[$"{GetExcelColumnName(rowColumnIndex++)}{rowStart}"].Value = student.SessionParameters[1].TCE;
                    worksheet.Cells[$"{GetExcelColumnName(rowColumnIndex++)}{rowStart}"].Value = student.SessionParameters[1].TGP;
                    worksheet.Cells[$"{GetExcelColumnName(rowColumnIndex++)}{rowStart}"].Value = student.SessionParameters[1].TGP;
                    worksheet.Cells[$"{GetExcelColumnName(rowColumnIndex++)}{rowStart}"].Value = student.SessionParameters[1].CTCR;
                    worksheet.Cells[$"{GetExcelColumnName(rowColumnIndex++)}{rowStart}"].Value = student.SessionParameters[1].CTCE;
                    worksheet.Cells[$"{GetExcelColumnName(rowColumnIndex++)}{rowStart}"].Value = student.SessionParameters[1].CTGP;
                    worksheet.Cells[$"{GetExcelColumnName(rowColumnIndex++)}{rowStart}"].Value = student.SessionParameters[1].CTGP;

                    worksheet.Cells[$"{GetExcelColumnName(rowColumnIndex++)}{rowStart}"].Value = student.SessionParameters[0].TCR;
                    worksheet.Cells[$"{GetExcelColumnName(rowColumnIndex++)}{rowStart}"].Value = student.SessionParameters[0].TCE;
                    worksheet.Cells[$"{GetExcelColumnName(rowColumnIndex++)}{rowStart}"].Value = student.SessionParameters[0].TGP;
                    worksheet.Cells[$"{GetExcelColumnName(rowColumnIndex++)}{rowStart}"].Value = student.SessionParameters[0].TGP;
                    worksheet.Cells[$"{GetExcelColumnName(rowColumnIndex++)}{rowStart}"].Value = student.SessionParameters[0].CTCR;
                    worksheet.Cells[$"{GetExcelColumnName(rowColumnIndex++)}{rowStart}"].Value = student.SessionParameters[0].CTCE;
                    worksheet.Cells[$"{GetExcelColumnName(rowColumnIndex++)}{rowStart}"].Value = student.SessionParameters[0].CTGP;
                    worksheet.Cells[$"{GetExcelColumnName(rowColumnIndex++)}{rowStart}"].Value = student.SessionParameters[0].CTGP;
                }
                if (student.SessionParameters.Count.Equals(2))
                {
                    worksheet.Cells[$"{GetExcelColumnName(rowColumnIndex++)}{rowStart}"].Value = student.SessionParameters[1].TCR;
                    worksheet.Cells[$"{GetExcelColumnName(rowColumnIndex++)}{rowStart}"].Value = student.SessionParameters[1].TCE;
                    worksheet.Cells[$"{GetExcelColumnName(rowColumnIndex++)}{rowStart}"].Value = student.SessionParameters[1].TGP;
                    worksheet.Cells[$"{GetExcelColumnName(rowColumnIndex++)}{rowStart}"].Value = student.SessionParameters[1].TGP;

                    worksheet.Cells[$"{GetExcelColumnName(rowColumnIndex++)}{rowStart}"].Value = student.SessionParameters[0].TCR;
                    worksheet.Cells[$"{GetExcelColumnName(rowColumnIndex++)}{rowStart}"].Value = student.SessionParameters[0].TCE;
                    worksheet.Cells[$"{GetExcelColumnName(rowColumnIndex++)}{rowStart}"].Value = student.SessionParameters[0].TGP;
                    worksheet.Cells[$"{GetExcelColumnName(rowColumnIndex++)}{rowStart}"].Value = student.SessionParameters[0].TGP;
                    worksheet.Cells[$"{GetExcelColumnName(rowColumnIndex++)}{rowStart}"].Value = student.SessionParameters[0].CTCR;
                    worksheet.Cells[$"{GetExcelColumnName(rowColumnIndex++)}{rowStart}"].Value = student.SessionParameters[0].CTCE;
                    worksheet.Cells[$"{GetExcelColumnName(rowColumnIndex++)}{rowStart}"].Value = student.SessionParameters[0].CTGP;
                    worksheet.Cells[$"{GetExcelColumnName(rowColumnIndex++)}{rowStart}"].Value = student.SessionParameters[0].CTGP;
                }
                if (student.SessionParameters.Count.Equals(1))
                {
                    worksheet.Cells[$"{GetExcelColumnName(rowColumnIndex++)}{rowStart}"].Value = student.SessionParameters[0].TCR;
                    worksheet.Cells[$"{GetExcelColumnName(rowColumnIndex++)}{rowStart}"].Value = student.SessionParameters[0].TCE;
                    worksheet.Cells[$"{GetExcelColumnName(rowColumnIndex++)}{rowStart}"].Value = student.SessionParameters[0].TGP;
                    worksheet.Cells[$"{GetExcelColumnName(rowColumnIndex++)}{rowStart}"].Value = student.SessionParameters[0].TGP;
                }

                // Handle Continuous Assessments for the student
                foreach (var ca in student.ContinuousAssessments)
                {
                    worksheet.Cells[$"{GetExcelColumnName(rowColumnIndex++)}{rowStart}"].Value = ca.Total;
                    worksheet.Cells[$"{GetExcelColumnName(rowColumnIndex++)}{rowStart}"].Value = ca.GradePoint;
                }

                rowStart++;
            }

            // Determine the last column dynamically
            int lastColumn = columnIndex + 1; // This gives us the first empty column index after writing all the other data.

            // Insert the "Remarks" header in the last column
            worksheet.Cells[$"{GetExcelColumnName(lastColumn)}1"].Value = "Remark";

            // Insert each student's "Remark" value in the last column
            rowStart = 2; // Reset row start to 2, since row 1 is the header row
            foreach (var student in summarySenate)
            {
                worksheet.Cells[$"{GetExcelColumnName(lastColumn)}{rowStart}"].Value = student.Remarks;
                rowStart++; // Move to next row for the next student
            }

            // Auto-fit columns and lock relevant columns
            worksheet.Cells["A:AZ"].AutoFitColumns();
            worksheet.Column(1).Style.Locked = true;
            worksheet.Column(2).Style.Locked = true;
            worksheet.Column(3).Style.Locked = true;

            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment; filename=" + "Senate_Summary_Format.xlsx");
            Response.BinaryWrite(package.GetAsByteArray());
            Response.End();
        }

        public async Task<DeptSanateFormatVm> GetFormatData(int? programmeId, int? levelId, int? sessionId)
        {
            var model = new DeptSanateFormatVm
            {
                DeptSanateFormatDetailVms = new List<DeptSanateFormatDetailVm>(),
                CourseCode = new List<string>()
            };
            var courseList = new List<string>();
            //var courseRegs = _db.CourseRegistrations.Include(i => i.Students)
            //            .Include(i => i.Course).Include(i => i.Programme).AsNoTracking()
            //            .Where(x => x.ProgrammeId.Equals((int)programmeId) &&
            //            x.LevelId.Equals((int)levelId) && x.SessionId.Equals((int)sessionId) &&
            //            x.IsApproved.Equals(true)).ToList().DistinctBy(d => d.StudentId).ToList();
            var courseRegs = _db.Students.Include(i => i.Session).Include(i => i.Programme).AsNoTracking()
                                    .Where(x => x.Session.SessionId.Equals((int)sessionId)
                                    && x.Programme.ProgrammeId.Equals((int)programmeId) && x.IsGraduated.Equals(false)
                                    && x.Active.Equals(true)).ToList();
            int count = 1;
            foreach (var courseReg in courseRegs.OrderBy(o => o.MatricNo).ToList())
            {
                var ca = await _db.ContinuousAssessments.Include(i => i.Level).Include(i => i.Course).AsNoTracking()
                                    .Where(x => x.StudentId.Equals(courseReg.StudentId) &&
                                    x.Level.LevelId.Equals((int)levelId) && x.SessionId.Equals((int)sessionId))
                                    .ToListAsync();

                model.DeptSanateFormatDetailVms.Add(new DeptSanateFormatDetailVm()
                {
                    Sn = count,
                    FullName = courseReg.FullName,
                    MatricNo = courseReg.MatricNo,
                    ModeOfEntry = courseReg.ModeOfEntry,
                    MNSA = courseReg.Programme.NoOfSemesters.ToString(),
                    NSS = ca.OrderBy(o => o.SessionId).DistinctBy(d => new { d.SessionId, d.SemesterId }).Count().ToString(),
                    ContinuousAssessments = ca,
                });
                count = count + 1;
                courseList.AddRange(ca.Select(s => s.Course.CourseCode));
            }

            model.CourseCode = courseList.Distinct().ToList();
            model.DeptSanateFormatDetailVms = model.DeptSanateFormatDetailVms.OrderBy(s => s.MatricNo).ToList();
            return model;
        }


        //public async Task<List<DeptSanateFormatSummaryVm>> GetSummaryFormatData(int? programmeId, int? levelId, int? sessionId)
        //{         
        //    var model = new List<DeptSanateFormatSummaryVm>();
        //    var courseRegs = _db.CourseRegistrations.Include(i => i.Students)
        //                .Include(i => i.Course).Include(i => i.Programme).AsNoTracking()
        //                .Where(x => x.ProgrammeId.Equals((int)programmeId) &&
        //                x.LevelId.Equals((int)levelId) && x.SessionId.Equals((int)sessionId) &&
        //                x.IsApproved.Equals(true)).ToList().DistinctBy(d => d.StudentId).ToList();
        //    var deptId = _db.Programmes.Include(i => i.Department).AsNoTracking()
        //                    .Where(x => x.ProgrammeId.Equals((int)programmeId))
        //                    .Select(s => s.Department.DepartmentId).FirstOrDefault();
        //    var deptResultTemplate = _db.DeptResultTypes.AsNoTracking()
        //                                .Where(x => x.DepartmentId.Equals(deptId)).FirstOrDefault();
        //    if (deptResultTemplate != null)
        //    {
        //        int count = 1;
        //        foreach (var courseReg in courseRegs)
        //        {
        //            var summary = await ProcessSessionalResult(count,(int)levelId, (int)sessionId, deptResultTemplate.FailMark, courseReg);
        //            model.Add(summary);
        //            count = count + 1;
        //        }
        //        return model;
        //    }
        //    return null;
        //}

        //private async Task<DeptSanateFormatSummaryVm> ProcessSessionalResult(int count, int levelId, int SessionId, int failMark, CourseRegistration courseReg)
        //{
        //    var builder = new StringBuilder();
        //    var ca = await _db.ContinuousAssessments.Include(i => i.Level).Include(i => i.Course).AsNoTracking()
        //                                            .Where(x => x.StudentId.Equals(courseReg.StudentId) &&
        //                                            x.Level.LevelId.Equals(levelId) && x.SessionId.Equals(sessionId))
        //                                            .ToListAsync();
        //    var tgp = ca.Sum(x => x.QualityPoint);
        //    var tcr = ca.Sum(s => s.Course.Credits);
        //    double gpa = Convert.ToDouble(tgp) / Convert.ToDouble(tcr);
        //    var summary = new DeptSanateFormatSummaryVm()
        //    {
        //        Sn = count,
        //        FullName = courseReg.Students.FullName,
        //        MatricNo = courseReg.Students.MatricNo,
        //        ModeOfEntry = courseReg.Students.ModeOfEntry,
        //        MNSA = courseReg.Programme.NoOfSemesters.ToString(),
        //        NSS = ca.OrderBy(o => o.SessionId).DistinctBy(d => new { d.SessionId, d.SemesterId }).Count().ToString(),
        //        ContinuousAssessments = ca,
        //        //TGP = tgp.ToString(),
        //        //TCR = tcr.ToString(),
        //        //TCE = ca.Where(c => c.Total > deptResultTemplate.FailMark).Sum(s => s.Course.Credits).ToString(),
        //        //GPA = Math.Round(gpa, 2)
        //    };

        //    if (ca.Any(x => x.Total <= failMark))
        //    {
        //        var failedCourse = ca.Where(c => c.Total <= failMark).ToList();                

        //        foreach (var course in failedCourse)
        //        {
        //            var checkForPassedCourse = ca.Where(x => x.CourseId.Equals(course.CourseId)).ToList();
        //            if(checkForPassedCourse.Any(x => x.Total > failMark))
        //            {
        //                summary.Remarks = "PASS";
        //            }
        //            else
        //            {
        //                builder.Append("RPT  ");
        //                builder.Append(course.Course.CourseCode);
        //                builder.Append(", ");
        //            }

        //        }
        //        summary.Remarks = builder.ToString();
        //    }
        //    else
        //    {
        //        summary.Remarks = "PASS";
        //    }

        //    return summary;
        //}


        public ActionResult ResultHistory()
        {
            return View();
        }
        public ActionResult GetHistoryIndex()
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

            var regCourses = new List<ContinuousAssessment>();
            if (Request.IsAuthenticated && User.IsInRole(RoleName.Student))
            {
                var studentId = _studentQuery.GetStudentId(userId);
                regCourses = _db.ContinuousAssessments.Include(c => c.Course).Include(s => s.Student)
                                    .Include(c => c.Level).Include(c => c.Programme).Include(c => c.Semester)
                                    .Include(c => c.Sessions).AsNoTracking()
                                    .Where(x => x.StudentId.Equals(studentId)
                                    && x.IsFacultyApproved.Equals(true))
                                    .OrderBy(o => o.Sessions.SessionName)
                                    .DistinctBy(d => new { d.SessionId, d.SemesterId }).ToList();
            }

            var data = regCourses.Select(s => new
            {
                s.ContinuousAssessmentId,
                s.Student.FullName,
                s.Level.LevelName,
                s.Sessions.SessionName,
                s.Semester.SemesterName,
                s.Programme.ProgrammeName,
                IsSenateApproved = s.IsSenateApproved ? "Senate Approved" : "Provisional Result Approved by Faculty",
            });

            totalRecords = data.Count();
            var newData = data.Skip(skip).Take(pageSize).ToList();

            return Json(new { draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = newData },
                JsonRequestBehavior.AllowGet);

            #endregion Server Side filtering

        }


        public async Task<PartialViewResult> PartialDetails(int id)
        {
            var cr = await _db.ContinuousAssessments.AsNoTracking().Where(x => x.ContinuousAssessmentId.Equals(id))
                            .Select(s => new { s.StudentId, s.SessionId, s.SemesterId, s.LevelId }).FirstOrDefaultAsync();

            var courseList = await _db.ContinuousAssessments
                                    .Include(c => c.Semester).Include(i => i.Student)
                                 .Include(c => c.Sessions).Include(i => i.Course).Include(i => i.Level).AsNoTracking()
                                 .Where(x => x.StudentId.Equals(cr.StudentId)
                                 && x.SessionId.Equals(cr.SessionId)
                                 && x.SemesterId.Equals(cr.SemesterId))
                                 .ToListAsync();
            double gpa = courseList.Sum(s => s.QualityPoint) / (double)courseList.Sum(s => s.CourseUnit);
            ViewBag.Gpa = Math.Round(gpa, 2);
            ViewBag.Course = id;
            ViewBag.Cgpa = await _resultCommand.CalculateCgpaByLevel(userId, cr.SessionId, (int)cr.LevelId);

            return PartialView(courseList);
        }

        public async Task<ActionResult> PrintDetails(int id)
        {
            var cr = await _db.ContinuousAssessments.AsNoTracking().Where(x => x.ContinuousAssessmentId.Equals(id))
                            .Select(s => new { s.StudentId, s.SessionId, s.SemesterId, s.LevelId }).FirstOrDefaultAsync();

            var courseList = await _db.ContinuousAssessments
                                    .Include(c => c.Semester).Include(i => i.Student)
                                 .Include(c => c.Sessions).Include(i => i.Course)
                                 .Include(i => i.Level).Include(i => i.Programme)
                                 .Include(i => i.Programme.Department).AsNoTracking()
                                 .Where(x => x.StudentId.Equals(cr.StudentId)
                                 && x.SessionId.Equals(cr.SessionId)
                                 && x.SemesterId.Equals(cr.SemesterId))
                                 .ToListAsync();
            double gpa = courseList.Sum(s => s.QualityPoint) / (double)courseList.Sum(s => s.CourseUnit);
            ViewBag.Gpa = Math.Round(gpa, 2);

            ViewBag.Cgpa = await _resultCommand.CalculateCgpaByLevel(userId, cr.SessionId, (int)cr.LevelId);

            // ViewBag.Cgpa = "";
            return new ViewAsPdf(courseList);
        }

        // GET: Results
        public ActionResult Index()
        {
            //var results = _db.Results.AsNoTracking().Include(r => r.Programme).Include(r => r.Semester).Include(r => r.Sessions);
            //return View(await results.ToListAsync());
            return View();
        }

        public async Task<ActionResult> StudentResult()
        {
            var result = ConfirmSchoolFee();
            if (result != null)
                return result;

            var studentId = _studentQuery.GetStudentId(userId);
            var results = await _db.Results.AsNoTracking().Include(r => r.Programme).Include(r => r.Semester)
                            .Include(r => r.Sessions).Include(i => i.Student)
                            .AsNoTracking().Where(x => x.StudentId.Equals(studentId)).ToListAsync();
            return View(results);

        }
        public ActionResult GetIndex()
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

            var results = _db.Results.Include(i => i.Programme).Include(i => i.Semester)
                        .Include(i => i.Sessions).AsNoTracking().ToList();

            var v = results.Select(s => new
            {
                s.StudentId,
                s.Sessions.SessionName,
                s.Semester.SemesterName,
                s.Programme.ProgrammeName,
                s.Cgpa,
                s.Gpa,
                s.TotalCourseUnit,
                s.ResultId,
                s.TotalGradePoint,
                s.TotalQualityPoint
            }).ToList();

            if (!string.IsNullOrEmpty(search))
            {
                v = v.Where(x => x.StudentId.Equals(search) || x.ProgrammeName.Equals(search)).ToList();
            }

            totalRecords = v.Count();
            var data = v.Skip(skip).Take(pageSize).ToList();

            return Json(new { draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data },
                JsonRequestBehavior.AllowGet);

            #endregion Server Side filtering
        }

        // GET: Results/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Result result = await _db.Results.FindAsync(id);
            if (result == null)
            {
                return HttpNotFound();
            }
            return View(result);
        }

        // GET: Results/Create
        public ActionResult Create()
        {
            ViewBag.ProgrammeId = new SelectList(_db.Programmes.AsNoTracking(), "ProgrammeId", "ProgrammeCode");
            ViewBag.SemesterId = new SelectList(_db.Semesters.AsNoTracking(), "SemesterId", "SemesterName");
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            return View();
        }

        // POST: Results/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(ResultVm model)
        {
            if (ModelState.IsValid)
            {

                string studentId = String.Empty;
                var registeredStudent = await _db.CourseRegistrations.AsNoTracking().Include(i => i.Students.Level)
                                                .Where(x => x.ProgrammeId.Equals(model.ProgrammeId)
                                                && x.SemesterId.Equals(model.SemesterId)
                                                && x.SessionId.Equals(model.SessionId)
                                                && x.IsApproved.Equals(true)).ToListAsync();

                foreach (var item in registeredStudent)
                {

                    var cA = await _db.ContinuousAssessments.AsNoTracking().Where(x => x.StudentId.Equals(item.StudentId)
                                                        && x.SemesterId.Equals(model.SemesterId)
                                                        && x.SessionId.Equals(model.SessionId)
                                                        && x.Programme.ProgrammeId.Equals(model.ProgrammeId))
                                                        .GroupBy(g => g.StudentId)
                                                        .Select(s => new
                                                        {
                                                            courseUnit = s.Sum(k => k.CourseUnit),
                                                            gradePoint = s.Sum(k => k.GradePoint),
                                                            qualityPoint = s.Sum(k => k.QualityPoint)
                                                        }).FirstOrDefaultAsync();
                    var checkResult = await _db.Results.AsNoTracking().Where(x => x.StudentId.Equals(item.StudentId) &&
                                                                    x.SemesterId.Equals(model.SemesterId) &&
                                                                    x.SessionId.Equals(model.SessionId))
                                                                    .FirstOrDefaultAsync();
                    if (checkResult != null)
                    {

                        checkResult.StudentId = item.StudentId;
                        checkResult.TotalCourseUnit = cA.courseUnit;
                        checkResult.TotalQualityPoint = cA.qualityPoint;
                        checkResult.TotalGradePoint = cA.gradePoint;
                        checkResult.ProgrammeId = model.ProgrammeId;
                        checkResult.SemesterId = model.SemesterId;
                        checkResult.SessionId = model.SessionId;
                        checkResult.LevelName = item.Students.Level.LevelName;
                        //_db.Entry(checkResult).State = EntityState.Modified;
                        _db.Set<Result>().AddOrUpdate(checkResult);
                        await _db.SaveChangesAsync();
                        studentId = item.StudentId;

                    }
                    else
                    {
                        var result = new Result()
                        {
                            StudentId = item.StudentId,
                            TotalCourseUnit = cA.courseUnit,
                            TotalQualityPoint = cA.qualityPoint,
                            TotalGradePoint = cA.gradePoint,
                            ProgrammeId = model.ProgrammeId,
                            SemesterId = model.SemesterId,
                            SessionId = model.SessionId,
                            LevelName = item.Students.Level.LevelName
                        };
                        _db.Results.Add(result);
                        studentId = item.StudentId;
                        await _db.SaveChangesAsync();

                    }
                    await SaveCgpa(model, studentId);
                }

                return RedirectToAction("Create");
            }

            ViewBag.ProgrammeId = new SelectList(_db.Programmes.AsNoTracking(), "ProgrammeId", "ProgrammeCode");
            ViewBag.SemesterId = new SelectList(_db.Semesters.AsNoTracking(), "SemesterId", "SemesterName");
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            return View();
        }

        private async Task SaveCgpa(ResultVm result, string studentId)
        {
            var myresult = await _db.Results.AsNoTracking().Where(x => x.StudentId.Equals(studentId))
                                        .GroupBy(g => g.StudentId).Select(s => new
                                        {
                                            totalcredit = s.Sum(k => k.TotalCourseUnit),
                                            totalQuality = s.Sum(k => k.TotalQualityPoint)
                                        }).FirstOrDefaultAsync();
            var editResult = await _db.Results.Where(x => x.StudentId.Equals(studentId)
                                        && x.SemesterId.Equals(result.SemesterId)
                                        && x.SessionId.Equals(result.SessionId)
                                        && x.ProgrammeId.Equals(result.ProgrammeId))
                                        .FirstOrDefaultAsync();
            editResult.Cgpa = myresult.totalQuality / myresult.totalcredit;
            _db.Entry(editResult).State = EntityState.Modified;
            // _db.Set<Result>().AddOrUpdate(editResult);
            await _db.SaveChangesAsync();
        }

        //// GET: Results/Edit/5
        //public async Task<ActionResult> Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Result result = await _db.Results.FindAsync(id);
        //    if (result == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    ViewBag.ProgrammeId = new SelectList(_db.Programmes.AsNoTracking(), "ProgrammeId", "ProgrammeCode", result.ProgrammeId);
        //    ViewBag.SemesterId = new SelectList(_db.Semesters.AsNoTracking(), "SemesterId", "SemesterName", result.SemesterId);
        //    ViewBag.SessionId = new SelectList(_db.Sessions.AsNoTracking(), "SessionId", "SessionName", result.SessionId);
        //    return View(result);
        //}

        //// POST: Results/Edit/5
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Edit([Bind(Include = "ResultId,StudentId,ProgrammeId,SemesterId,SessionId,TotalCourseUnit,TotalGradePoint,TotalQualityPoint,Gpa,Cgpa")] Result result)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _db.Entry(result).State = EntityState.Modified;
        //        await _db.SaveChangesAsync();

        //        await SaveCgpa(result, result.StudentId);
        //        TempData["UserMessage"] = "Result is Updated Successfully.";
        //        TempData["Title"] = "Success.";
        //        return RedirectToAction("Index");
        //    }
        //    ViewBag.ProgrammeId = new SelectList(_db.Programmes.AsNoTracking(), "ProgrammeId", "ProgrammeCode", result.ProgrammeId);
        //    ViewBag.SemesterId = new SelectList(_db.Semesters.AsNoTracking(), "SemesterId", "SemesterName", result.SemesterId);
        //    ViewBag.SessionId = new SelectList(_db.Sessions.AsNoTracking(), "SessionId", "SessionName", result.SessionId);
        //    return View(result);
        //}

        // GET: Results/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Result result = await _db.Results.FindAsync(id);
            if (result == null)
            {
                return HttpNotFound();
            }
            return View(result);
        }

        // POST: Results/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Result result = await _db.Results.FindAsync(id);
            if (result != null) _db.Results.Remove(result);
            await _db.SaveChangesAsync();
            TempData["UserMessage"] = "Result is  Deleted Successfully.";
            TempData["Title"] = "Error.";

            return RedirectToAction("Index");
        }

        public ActionResult CalculateCgpa()
        {
            var result = ConfirmSchoolFee();
            if (result != null)
                return result;

            return View();
        }

        public async Task<ActionResult> DeleteBuplicate()
        {
            var subjectRules = await _db.UnderGraduateRules.Include(i => i.Programme).AsNoTracking().ToListAsync();
            var duplicateRules = new List<UnderGraduateRule>();
            foreach (var subjectRule in subjectRules)
            {
                var ruleCount = _db.UnderGraduateRules.Include(i => i.Programme).AsNoTracking()
                                .Where(x => x.Programme.ProgrammeId.Equals((int)subjectRule.ProgrammeId) &&
                                x.SubjectId.Equals(subjectRule.SubjectId)).ToList();
                if (ruleCount.Count() > 1)
                {
                    duplicateRules.Add(ruleCount.FirstOrDefault());
                }
            }
            duplicateRules = duplicateRules.DistinctBy(d => new { d.Programme.ProgrammeId, d.SubjectId }).ToList();
            var myList = duplicateRules;
            foreach (var duplicateRule in myList)
            {
                UnderGraduateRule underGraduateRule = await _db.UnderGraduateRules.FindAsync(duplicateRule.UnderGraduateRuleId);
                if (underGraduateRule != null) _db.UnderGraduateRules.Remove(underGraduateRule);
                await _db.SaveChangesAsync();
            }

            ViewBag.Message = $"{duplicateRules.Count} removed successfully";

            return View();
        }

        public ActionResult ProcessResult(List<CgpaData> model)
        {
            if (model.Any())
            {
                var cgpaList = new List<CgpaViewModel>();
                foreach (var scoreList in model)
                {
                    var suppliedModel = new CgpaViewModel(_db)
                    {
                        Score = Convert.ToDouble(scoreList.score),
                        CourseCredit = scoreList.unit
                    };

                    cgpaList.Add(suppliedModel);
                }

                var totalCourseUnit = cgpaList.Sum(k => k.CourseCredit);
                var gradePoint = cgpaList.Sum(k => k.GradePoint);
                var totalQualityPoint = cgpaList.Sum(k => k.QualityPoint);
                double gpa = totalQualityPoint / (double)totalCourseUnit;
                gpa = Math.Round(gpa, 2);
                return new JsonResult { Data = gpa.ToString(CultureInfo.InvariantCulture) };
                // RedirectToAction("CalculateCgpa", new { gpa = gpa.ToString(CultureInfo.InvariantCulture) });
            }
            return new JsonResult { Data = "" };

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
