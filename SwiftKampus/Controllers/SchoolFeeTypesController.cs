using NumberToWordConverter;
using OfficeOpenXml;
using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampus.ViewModels;
using SwiftKampusModel;
using SwiftKampusModel.Payment;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SwiftKampus.Controllers
{
    [Authorize]
    [Audit(AuditingLevel = 2)]
    public class SchoolFeeTypesController : BaseController
    {

        public SchoolFeeTypesController(SchoolDbContext db) : base(db)
        {

        }
        // GET: SchoolFeeTypes
        public ActionResult Index(string message)
        {
            ViewBag.Message = message;

            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FancyName");
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");


            var schoolFeeCategory = from SchoolFeeCategory s in Enum.GetValues(typeof(SchoolFeeCategory))
                                    select new { ID = s, Name = s.ToString() };
            ViewBag.SchoolFeeCategory = new SelectList(schoolFeeCategory, "Name", "Name");
            return View();
        }
        public async Task<ActionResult> GetIndex(int? SchoolProgrammeId, int? SessionId, string SchoolFeeCategory)
        {

            var schoolFeeType = await _db.SchoolFeeTypes.Include(i => i.SchoolProgramme)
                                        .Include(i => i.Session).ToListAsync();
            if(SessionId != null)
            {
                schoolFeeType = schoolFeeType.Where(x => x.Session.SessionId.Equals((int)SessionId)).ToList();
            }

            if (!string.IsNullOrEmpty(SchoolFeeCategory))
            {
                schoolFeeType = schoolFeeType.Where(x => x.FeeCategory.ToUpper().Equals(SchoolFeeCategory.ToUpper())).ToList();
            }
            if (SchoolProgrammeId != null)
            {
                schoolFeeType = schoolFeeType.Where(x => x.SchoolProgrammeId.Equals((int)SchoolProgrammeId)).ToList();
            }

            var data = schoolFeeType.Select(s => new
            {
                s.SchoolFeeTypeId,
                s.FeeCategory,
                s.StudentType,
                s.Amount,
                s.AmountInWords,
                s.FeeName,
                s.Indegine,
                s.Session.SessionName,
                s.SchoolProgramme.FancyName,

            }).ToList();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        public async Task<PartialViewResult> Save(int id)
        {
            var schoolFeeType = await _db.SchoolFeeTypes.FindAsync(id);

            var schoolfeetypeModel = new SchoolFeeTypeVm();
            if (schoolFeeType != null)
            {
                schoolfeetypeModel.SchoolFeeTypeId = schoolFeeType.SchoolFeeTypeId;
                schoolfeetypeModel.FeeName = schoolFeeType.FeeName;
                schoolfeetypeModel.Amount = schoolFeeType.Amount;
                schoolfeetypeModel.AmountInWords = WordConverter.GetNumberConverter(Convert.ToInt32(schoolfeetypeModel.Amount).ToString(), " Naira Only");
                schoolfeetypeModel.SchoolProgrammeId = schoolFeeType.SchoolProgrammeId;
                schoolfeetypeModel.FeeCode = schoolFeeType.FeeCode;
                schoolfeetypeModel.Description = schoolFeeType.Description;

            }
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FancyName");
            ViewBag.SessionId = new SelectList(GetAllSession(), "SessionId", "SessionName");
            var studentStaus = from StudentStatus s in Enum.GetValues(typeof(StudentStatus))
                               select new { ID = s, Name = s.ToString() };

            ViewBag.StudentType = new SelectList(studentStaus, "Name", "Name");
            var indigene = from IndegineStatus s in Enum.GetValues(typeof(IndegineStatus))
                               select new { ID = s, Name = s.ToString() };

            ViewBag.Indegine = new SelectList(indigene, "Name", "Name");

            return PartialView(schoolfeetypeModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Save(SchoolFeeTypeVm model)
        {
            bool status = false;
            string message = string.Empty;
            if (ModelState.IsValid)
            {               
                if (model.SchoolFeeTypeId > 0)
                {

                    var schoolFeeType = await _db.SchoolFeeTypes.FindAsync(model.SchoolFeeTypeId);
                    if (schoolFeeType != null)
                    {
                        schoolFeeType.FeeCategory = model.FeeCategory.ToString();
                        schoolFeeType.FeeName = model.FeeName;
                        schoolFeeType.Amount = model.Amount;
                        schoolFeeType.AmountInWords = $"{WordConverter.ConverterToCurrency(Convert.ToInt32(model.Amount).ToString()).Replace(".", " ")} Naira Only";
                        schoolFeeType.SchoolProgrammeId = model.SchoolProgrammeId;
                        schoolFeeType.Indegine = model.Indegine;
                        schoolFeeType.SessionId = model.SessionId;
                        schoolFeeType.Description = model.Description;
                        schoolFeeType.StudentType = model.StudentType;
                        schoolFeeType.FeeCode = model.FeeCode.Trim();

                        _db.Entry(schoolFeeType).State = EntityState.Modified;
                        await _db.SaveChangesAsync();
                        message = $"{model.FeeName} Updated Successfully...";
                        return new JsonResult { Data = new { status = true, message } };
                    }
                }
                else
                {
                    //var checkFeeCode = _db.SchoolFeeTypes.AsNoTracking()
                    //                     .Any(x => x.FeeCode.Equals(model.FeeCode.Trim()));
                    //if (checkFeeCode)
                    //{
                    //    message = $"{model.FeeCode} already exist, Please check and try again.";
                    //    return new JsonResult { Data = new { status = false, message } };
                    //}
                    var schoolFeeType = new SchoolFeeType
                    {
                        FeeCategory = model.FeeCategory.ToString(),
                        FeeName = model.FeeName,
                        Amount = model.Amount,
                        AmountInWords = $"{WordConverter.ConverterToCurrency(Convert.ToInt32(model.Amount).ToString()).Replace(".", " ")} Naira Only",
                        SchoolProgrammeId = model.SchoolProgrammeId,
                        Indegine = model.Indegine,
                        Description = model.Description,
                        SessionId = model.SessionId,
                        StudentType = model.StudentType,
                        FeeCode = model.FeeCode.Trim()
                    };
                    _db.SchoolFeeTypes.Add(schoolFeeType);
                    await _db.SaveChangesAsync();
                    message = $"{model.FeeName} Added Successfully.";
                    return new JsonResult { Data = new { status = true, message } };
                }
            }
            return new JsonResult { Data = new { status, message = "Please enter the data required Correctly" } };
            //return View(subject);
        }

        public PartialViewResult UploadSchoolFeeType()
        {
            return PartialView();
        }

        [HttpPost]
        public async Task<ActionResult> UploadSchoolFeeType(HttpPostedFileBase excelfile)
        {
            if (excelfile == null || excelfile.ContentLength == 0)
            {

                ViewBag.ErrorInfo = "Please Select a excel file <br/>";
                ViewBag.ErrorMessage = "You must select an excel file before you click Upload button";
                return View("ErrorException");
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
                    int requiredField = 9;

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
                        }

                        string lineError = $"Line/Row number {myArray[0]}  and column {myArray[1]} is not rightly formatted, Please Check for anomalies ";

                        ViewBag.ErrorInfo = lineError;
                        ViewBag.ErrorMessage = lineError;
                        return View("ErrorException");
                    }
                    try
                    {
                        for (int row = 2; row <= noOfRow; row++)
                        {
                            string isIndigene = string.Empty;
                            var schoolProgrammeCode = workSheet.Cells[row, 1].Value.ToString().Trim();
                            var feeCategory = workSheet.Cells[row, 2].Value.ToString().Trim();
                            var feeName = workSheet.Cells[row, 3].Value.ToString().Trim();
                            var feeCode = workSheet.Cells[row, 4].Value.ToString().Trim();
                            var studentType = workSheet.Cells[row, 5].Value.ToString().Trim();
                            var amount = workSheet.Cells[row, 6].Value.ToString().Trim();
                            var description = workSheet.Cells[row, 7].Value.ToString().Trim();
                            var indigene = workSheet.Cells[row, 8].Value.ToString().Trim();
                            var sessionName = workSheet.Cells[row, 9].Value.ToString().Trim();


                            var schoolProgramme = await _db.SchoolProgrammes.AsNoTracking()
                                            .Where(x => x.SchoolProgrammeCode.ToUpper().Equals(schoolProgrammeCode.ToUpper()))
                                            .FirstOrDefaultAsync();
                            var uploadedSession = await _db.Sessions.AsNoTracking().Where(x => x.SessionName.ToUpper().Equals(sessionName.ToUpper()))
                                                 .FirstOrDefaultAsync();

                            if (feeCategory.ToUpper().Equals("SCHOOL CHARGES") || feeCategory.ToUpper().Equals("SCHOOL-CHARGES"))
                            {
                                feeCategory = SchoolFeeCategory.School_Charges.ToString();
                            }
                            else if (feeCategory.ToUpper().Equals("ACCEPTANCE"))
                            {
                                feeCategory = SchoolFeeCategory.Acceptance.ToString();
                            }
                            else
                            {
                                ViewBag.ErrorInfo = "School Fee Category type supported is \"School Fee\"  and \"Acceptance\" ";
                                ViewBag.ErrorMessage = "Please check the School Fee Category spelling very well ";
                                return View("ErrorException");
                            }

                            if (studentType.ToUpper().Equals("NEW STUDENT") || studentType.ToUpper().Equals("NEW-STUDENT"))
                            {
                                studentType = StudentStatus.New_Student.ToString();
                            }
                            else if (studentType.ToUpper().Equals("RETURNING"))
                            {
                                studentType = StudentStatus.Returning.ToString();
                            }
                            else
                            {
                                ViewBag.ErrorInfo = "Student Type Supported is \"New-Student\" and \"Returning\"  ";
                                ViewBag.ErrorMessage = "Please check the Student type spelling very well ";
                                return View("ErrorException");
                            }

                            if (uploadedSession == null)
                            {
                                ViewBag.ErrorInfo = "Please Leave no column or row Empty/Blank";
                                ViewBag.ErrorMessage = $" The Session Name \"{sessionName}\"  specified in the excel at row {row} doesn't exist on the portal. " +
                                                       $"Please add the Session Name first before uploading or correct the excel sheet...";
                                return View("ErrorException");
                            }

                            if (schoolProgramme == null)
                            {
                                ViewBag.ErrorInfo = "School programme Code in the excel doesn't exit";
                                ViewBag.ErrorMessage = $"School Programmme code {schoolProgrammeCode} at row {row} is not valid. Please check the Student type spelling very well ";
                                return View("ErrorException");
                            }

                            if (indigene.ToUpper().Equals("NIGERIAN") )
                            {
                                isIndigene = IndegineStatus.Nigerian.ToString();
                            }
                            else if (indigene.ToUpper().Equals("AFRICAN"))
                            {
                                isIndigene = IndegineStatus.African.ToString();
                            }
                            else if (indigene.ToUpper().Equals("OTHERS"))
                            {
                                isIndigene = IndegineStatus.Others.ToString();
                            }
                            else
                            {
                                ViewBag.ErrorInfo = "Indigene type supported is \"Nigerian\", \"African\",  and \"Others\"  ";
                                ViewBag.ErrorMessage = "Please check the Indigene type spelling very well ";
                                return View("ErrorException");
                            }


                            var schoolFeeType = new SchoolFeeType
                            {
                                SchoolProgrammeId = schoolProgramme.SchoolProgrammeId,
                                FeeCategory = feeCategory,
                                FeeName = feeName,
                                FeeCode = feeCode,
                                Amount = Convert.ToDecimal(amount),
                                AmountInWords = WordConverter.GetNumberConverter(Convert.ToInt32(amount).ToString(), " Naira Only"),
                                StudentType = studentType,
                                Indegine = isIndigene,
                                Description = description,
                                SessionId = uploadedSession.SessionId
                            };
                            _db.SchoolFeeTypes.Add(schoolFeeType);
                            recordCount++;
                            lastrecord = $"The last Updated record has the Fee Category of {feeCategory} and Fee Name {feeName}";

                        }
                        await _db.SaveChangesAsync();
                        message = $"You have successfully Uploaded {recordCount} records...  and {lastrecord}";
                        ViewBag.Message = message;

                    }
                    catch (Exception ex)
                    {
                        ViewBag.ErrorInfo = $"The Fee Code is duplicated. Please check and try again";
                        ViewBag.ErrorMessage = ex.Message;
                        return View("ErrorException");
                    }
                }
                return RedirectToAction("Index", "SchoolFeeTypes", new { message });
            }

            ViewBag.Error = $"File type is Incorrect <br/>";
            return View("Index");
        }

        [HttpPost]
        public async Task<ActionResult> UploadFacultySchoolFeeType(HttpPostedFileBase excelfile)
        {
            if (excelfile == null || excelfile.ContentLength == 0)
            {

                ViewBag.ErrorInfo = "Please Select a excel file <br/>";
                ViewBag.ErrorMessage = "You must select an excel file before you click Upload button";
                return View("ErrorException");
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
                    int requiredField = 10;

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
                        }

                        string lineError = $"Line/Row number {myArray[0]}  and column {myArray[1]} is not rightly formatted, Please Check for anomalies ";

                        ViewBag.ErrorInfo = lineError;
                        ViewBag.ErrorMessage = lineError;
                        return View("ErrorException");
                    }
                    try
                    {
                        for (int row = 2; row <= noOfRow; row++)
                        {
                            string isIndigene = string.Empty;
                            var schoolProgrammeCode = workSheet.Cells[row, 1].Value.ToString().Trim();
                            var facultyCode = workSheet.Cells[row, 2].Value.ToString().Trim();
                            var feeCategory = workSheet.Cells[row, 3].Value.ToString().Trim();
                            var feeName = workSheet.Cells[row, 4].Value.ToString().Trim();
                            var feeCode = workSheet.Cells[row, 5].Value.ToString().Trim();
                            var studentType = workSheet.Cells[row, 6].Value.ToString().Trim();
                            var amount = Convert.ToDecimal(workSheet.Cells[row, 7].Value.ToString().Trim());
                            var amountInWords = workSheet.Cells[row, 8].Value.ToString().Trim();
                            var description = workSheet.Cells[row, 9].Value.ToString().Trim();
                            var indigene = workSheet.Cells[row, 10].Value.ToString().Trim();


                            var schoolProgramme = await _db.SchoolProgrammes.AsNoTracking()
                                            .Where(x => x.SchoolProgrammeCode.ToUpper().Equals(schoolProgrammeCode.ToUpper()))
                                            .FirstOrDefaultAsync();

                            var faculty = await _db.Faculties.AsNoTracking()
                                                .Where(x => x.FacultyCode.ToUpper().Equals(facultyCode.ToUpper()))
                                                .FirstOrDefaultAsync();

                            if (feeCategory.ToUpper().Equals("SCHOOL CHARGES") || feeCategory.ToUpper().Equals("SCHOOL-CHARGES"))
                            {
                                feeCategory = SchoolFeeCategory.School_Charges.ToString();
                            }
                            else if (feeCategory.ToUpper().Equals("ACCEPTANCE"))
                            {
                                feeCategory = SchoolFeeCategory.Acceptance.ToString();
                            }
                            else
                            {
                                ViewBag.ErrorInfo = "School Fee Category type supported is \"School Charges\"  and \"Acceptance\" ";
                                ViewBag.ErrorMessage = "Please check the School Fee Category spelling very well ";
                                return View("ErrorException");
                            }

                            if (studentType.ToUpper().Equals("NEW STUDENT") || studentType.ToUpper().Equals("NEW-STUDENT"))
                            {
                                studentType = StudentStatus.New_Student.ToString();
                            }
                            else if (studentType.ToUpper().Equals("RETURNING"))
                            {
                                studentType = StudentStatus.Returning.ToString();
                            }
                            else
                            {
                                ViewBag.ErrorInfo = "Student Type Supported is \"New-Student\" and \"Returning\"  ";
                                ViewBag.ErrorMessage = "Please check the Student type spelling very well ";
                                return View("ErrorException");
                            }


                            if (schoolProgramme == null)
                            {
                                ViewBag.ErrorInfo = "School programme Code in the excel doesn't exit";
                                ViewBag.ErrorMessage = $"School Programmme code {schoolProgrammeCode} at row {row} is not valid. Please check the Student type spelling very well ";
                                return View("ErrorException");
                            }
                            if (faculty == null)
                            {
                                ViewBag.ErrorInfo = "Faculty Code in the excel doesn't exit";
                                ViewBag.ErrorMessage = $"Faculty code {schoolProgrammeCode} at row {row} is not valid. Please check the faculty code spelling very well ";
                                return View("ErrorException");
                            }


                            if (indigene.ToUpper().Equals("NIGERIAN"))
                            {
                                isIndigene = IndegineStatus.Nigerian.ToString();
                            }
                            else if (indigene.ToUpper().Equals("AFRICAN"))
                            {
                                isIndigene = IndegineStatus.African.ToString();
                            }
                            else if (indigene.ToUpper().Equals("OTHERS"))
                            {
                                isIndigene = IndegineStatus.Others.ToString();
                            }
                            else
                            {
                                ViewBag.ErrorInfo = "Indigene type supported is \"Nigerain\", \"African\",  and \"Others\"  ";
                                ViewBag.ErrorMessage = "Please check the Indigene type spelling very well ";
                                return View("ErrorException");
                            }


                            var schoolFeeType = new SchoolFeeType
                            {
                                SchoolProgrammeId = schoolProgramme.SchoolProgrammeId,
                                FeeCategory = feeCategory,
                                FeeName = feeName,
                                FeeCode = feeCode,
                                Amount = amount,
                                AmountInWords = $"{WordConverter.ConverterToCurrency(Convert.ToInt32(amount).ToString()).Replace(".", " ")} Naira Only",
                                StudentType = studentType,
                                Indegine = isIndigene,
                                Description = description,
                                FacultyId = faculty.FacultyId
                            };
                            _db.SchoolFeeTypes.Add(schoolFeeType);
                            recordCount++;
                            lastrecord = $"The last Updated record has the Fee Category of {feeCategory} and Fee Name {feeName}";

                        }
                        await _db.SaveChangesAsync();
                        message = $"You have successfully Uploaded {recordCount} records...  and {lastrecord}";
                        ViewBag.Message = message;

                    }
                    catch (Exception ex)
                    {
                        ViewBag.ErrorInfo = $"The Fee Code is duplicated. Please check and try again";
                        ViewBag.ErrorMessage = ex.Message;
                        return View("ErrorException");
                    }
                }
                return RedirectToAction("Index", "SchoolFeeTypes", new { message });
            }

            ViewBag.Error = $"File type is Incorrect <br/>";
            return View("Index");
        }
        // GET: SchoolFeeTypes/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SchoolFeeType schoolFeeType = await _db.SchoolFeeTypes.FindAsync(id);
            if (schoolFeeType == null)
            {
                return HttpNotFound();
            }
            return View(schoolFeeType);
        }

        #region Create And Edit Code
        //// GET: SchoolFeeTypes/Create
        //public ActionResult Create()
        //{
        //    ViewBag.SemesterId = new SelectList(_db.Semesters, "SemesterId", "SemesterName");
        //    var studentType = from StudentType s in Enum.GetValues(typeof(StudentType))
        //                      select new { ID = s, Name = s.ToString() };

        //    ViewBag.StudentType = new MultiSelectList(studentType, "Name", "Name");
        //    ViewBag.LevelId = new MultiSelectList(_db.Levels.AsNoTracking(), "LevelId", "LevelName");
        //    ViewBag.FacultyId = new MultiSelectList(_db.Faculties.AsNoTracking(), "FacultyId", "FacultyName");
        //    return View();
        //}

        //// POST: SchoolFeeTypes/Create
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Create(SchoolFeeTypeVm model)
        //{
        //    if (ModelState.IsValid)
        //    {

        //        var schoolFeeType = new SchoolFeeType
        //        {
        //            FeeCategory = model.FeeCategory.ToString(),
        //            FeeName = model.FeeName,
        //            Amount = model.Amount,
        //            AmountInWords = model.AmountInWords,
        //            SchoolProgrammeId = model.SchoolProgrammeId,
        //            Description = model.Description
        //        };
        //        _db.SchoolFeeTypes.Add(schoolFeeType);

        //        await _db.SaveChangesAsync();
        //        return RedirectToAction("Index");
        //    }

        //    //ViewBag.FeeCategoryId = new SelectList(_db.FeeCategories, "FeeCategoryId", "CategoryName", model.FeeCategoryId);
        //    //ViewBag.SemesterId = new SelectList(_db.Semesters, "SemesterId", "SemesterName", schoolFeeType.SemesterId);
        //    var studentType = from StudentType s in Enum.GetValues(typeof(StudentType))
        //                      select new { ID = s, Name = s.ToString() };

        //    ViewBag.StudentType = new MultiSelectList(studentType, "Name", "Name");
        //    ViewBag.LevelId = new MultiSelectList(_db.Levels.AsNoTracking(), "LevelId", "LevelName");
        //    return View(model);
        //}

        //// GET: SchoolFeeTypes/Edit/5
        //public async Task<ActionResult> Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    SchoolFeeType schoolFeeType = await _db.SchoolFeeTypes.FindAsync(id);
        //    if (schoolFeeType == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    //ViewBag.FeeCategoryId = new SelectList(_db.FeeCategories, "FeeCategoryId", "CategoryName", schoolFeeType.FeeCategoryId);
        //    // ViewBag.SemesterId = new SelectList(_db.Semesters, "SemesterId", "SemesterName", schoolFeeType.SemesterId);
        //    var studentType = from StudentType s in Enum.GetValues(typeof(StudentType))
        //                      select new { ID = s, Name = s.ToString() };

        //    ViewBag.StudentType = new MultiSelectList(studentType, "Name", "Name");
        //    ViewBag.LevelId = new SelectList(_db.Levels.AsNoTracking(), "LevelId", "LevelName");

        //    return View(schoolFeeType);
        //}

        //// POST: SchoolFeeTypes/Edit/5
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Edit(SchoolFeeType schoolFeeType)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _db.Entry(schoolFeeType).State = EntityState.Modified;
        //        await _db.SaveChangesAsync();
        //        return RedirectToAction("Index");
        //    }
        //    //ViewBag.SemesterId = new SelectList(_db.Semesters, "SemesterId", "SemesterName", schoolFeeType.SemesterId);
        //    var studentType = from StudentType s in Enum.GetValues(typeof(StudentType))
        //                      select new { ID = s, Name = s.ToString() };

        //    ViewBag.StudentType = new MultiSelectList(studentType, "Name", "Name");
        //    ViewBag.LevelId = new SelectList(_db.Levels.AsNoTracking(), "LevelId", "LevelName");
        //    return View(schoolFeeType);
        //} 
        #endregion

        public async Task<PartialViewResult> Delete(int id)
        {
            var schoolFeeType = await _db.SchoolFeeTypes.FindAsync(id);
            return PartialView(schoolFeeType);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            bool status = false;
            string message = string.Empty;
            var schoolFeeType = await _db.SchoolFeeTypes.FindAsync(id);
            if (schoolFeeType != null)
            {
                _db.SchoolFeeTypes.Remove(schoolFeeType);
                await _db.SaveChangesAsync();
                status = true;
                message = "School Fee Type Deleted Successfully...";
                return new JsonResult { Data = new { status, message } };
            }

            return new JsonResult { Data = new { status = false, message = $"{id } not deleted" } };
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
