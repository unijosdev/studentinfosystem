using OfficeOpenXml;
using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampusModel.AddmissionApplicant;
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
    public class AvailableCoursesController : BaseController
    {

        public AvailableCoursesController(SchoolDbContext db) : base(db)
        {

        }

        // GET: AvailableCourses
        public async Task<ActionResult> Index()
        {
            var availableCourses = _db.AvailableCourses.AsNoTracking().Include(a => a.Programme).Include(a => a.SchoolProgramme);
            return View(await availableCourses.OrderBy(o => o.Programme.ProgrammeName).ToListAsync());
        }

        public async Task<ActionResult> GetIndex()
        {
            // dc.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var schoolProgramme = await _db.AvailableCourses.Include(a => a.Programme).Include(a => a.SchoolProgramme)
                        .AsNoTracking().ToListAsync();
            var data = schoolProgramme.Select(s => new
            {
                s.AvailableCourseId,
                s.SchoolProgrammeId,
                s.Programme.ProgrammeName,
                s.SchoolProgramme.FancyName,
                s.IsActive,

            }).ToList();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }


        public async Task<PartialViewResult> Save(int id)
        {
            var availableCourse = await _db.AvailableCourses.FindAsync(id);
            var availableCourseVm = new AvailableCourseVm();

            if (availableCourse != null)
            {
                availableCourseVm.AvailableCourseId = availableCourse.AvailableCourseId;
                availableCourseVm.ProgrammeName = availableCourse.ProgrammeName;
                availableCourseVm.SchoolProgrammeId = availableCourse.SchoolProgrammeId;
                availableCourseVm.IsActive = availableCourse.IsActive;
                ViewBag.ProgrammeId = new SelectList(_db.Programmes.AsNoTracking(), "ProgrammeId", "ProgrammeName", availableCourse?.ProgrammeId);

            }
            else
            {
                ViewBag.ProgrammeId = new MultiSelectList(_db.Programmes.AsNoTracking(), "ProgrammeId", "ProgrammeName");
            }
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FancyName", availableCourse?.SchoolProgrammeId);
            return PartialView(availableCourseVm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Save(AvailableCourseVm model, int? id)
        {
            bool status = false;
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                if (model.AvailableCourseId > 0)
                {
                    var availableCourse = await _db.AvailableCourses.FindAsync(model.AvailableCourseId);
                    var progId = model.ProgrammeId[0];
                    var myProgramme = await _db.Programmes.AsNoTracking().Where(x => x.ProgrammeId.Equals(progId))
                        .Select(s => s.ProgrammeName).FirstOrDefaultAsync();
                    if (availableCourse != null)
                    {
                        availableCourse.ProgrammeId = model.ProgrammeId[0];
                        availableCourse.SchoolProgrammeId = model.SchoolProgrammeId;
                        availableCourse.ProgrammeName = myProgramme;
                        availableCourse.IsActive = model.IsActive;
                        _db.Entry(availableCourse).State = EntityState.Modified;
                        await _db.SaveChangesAsync();
                        message = $"{model.ProgrammeName} Updated Successfully...";
                        return new JsonResult { Data = new { status = true, message } };
                    }
                }
                else
                {
                    foreach (var programme in model.ProgrammeId)
                    {
                        var myProgramme = await _db.Programmes.AsNoTracking().Where(x => x.ProgrammeId.Equals(programme))
                            .Select(s => s.ProgrammeName).FirstOrDefaultAsync();

                        var availableCourse = new AvailableCourse()
                        {
                            SchoolProgrammeId = model.SchoolProgrammeId,
                            ProgrammeId = programme,
                            ProgrammeName = myProgramme,
                            IsActive = true
                        };
                        _db.AvailableCourses.Add(availableCourse);
                    }
                    await _db.SaveChangesAsync();
                    message = $"Programme Course Added Successfully.";
                    return new JsonResult { Data = new { status = true, message } };
                }
            }
            return new JsonResult { Data = new { status, message } };
            //return View(subject);
        }


        // GET: AvailableCourses/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AvailableCourse availableCourse = await _db.AvailableCourses.FindAsync(id);
            if (availableCourse == null)
            {
                return HttpNotFound();
            }
            return View(availableCourse);
        }

        // GET: AvailableCourses/Create
        public ActionResult Create()
        {
            ViewBag.ProgrammeId = new MultiSelectList(_db.Programmes.AsNoTracking(), "ProgrammeId", "ProgrammeName");
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "FancyName");
            return View();
        }

        // POST: AvailableCourses/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(AvailableCourseVm model)
        {
            if (ModelState.IsValid)
            {

                foreach (var programme in model.ProgrammeId)
                {
                    var myProgramme = await _db.Programmes.AsNoTracking().Where(x => x.ProgrammeId.Equals(programme))
                        .Select(s => s.ProgrammeName).FirstOrDefaultAsync();

                    var availableCourse = new AvailableCourse()
                    {
                        SchoolProgrammeId = model.SchoolProgrammeId,
                        ProgrammeId = programme,
                        ProgrammeName = myProgramme,
                        IsActive = true
                    };
                    _db.AvailableCourses.Add(availableCourse);
                }
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.ProgrammeId = new MultiSelectList(_db.Programmes.AsNoTracking(), "ProgrammeId", "ProgrammeName", model.ProgrammeId);
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "ProgrammeName", model.SchoolProgrammeId);
            return View(model);
        }

        // GET: AvailableCourses/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AvailableCourse availableCourse = await _db.AvailableCourses.FindAsync(id);
            if (availableCourse == null)
            {
                return HttpNotFound();
            }
            ViewBag.ProgrammeId = new SelectList(_db.Programmes.AsNoTracking(), "ProgrammeId", "ProgrammeName", availableCourse.ProgrammeId);
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "ProgrammeName", availableCourse.SchoolProgrammeId);
            return View(availableCourse);
        }

        // POST: AvailableCourses/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "AvailableCourseId,SchoolProgrammeId,ProgrammeId")] AvailableCourse availableCourse)
        {
            if (ModelState.IsValid)
            {
                var myProgramme = await _db.Programmes.AsNoTracking().Where(x => x.ProgrammeId.Equals(availableCourse.ProgrammeId))
                    .Select(s => s.ProgrammeName).FirstOrDefaultAsync();
                availableCourse.ProgrammeName = myProgramme;
                _db.Entry(availableCourse).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.ProgrammeId = new SelectList(_db.Programmes.AsNoTracking(), "ProgrammeId", "ProgrammeName", availableCourse.ProgrammeId);
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes.AsNoTracking(), "SchoolProgrammeId", "ProgrammeName", availableCourse.SchoolProgrammeId);
            return View(availableCourse);
        }

        // GET: AvailableCourses/Delete/5
        public async Task<PartialViewResult> Delete(int id)
        {
            var availableCourse = await _db.AvailableCourses.Include(a => a.Programme).Include(a => a.SchoolProgramme).AsNoTracking()
                                                .Where(x => x.AvailableCourseId.Equals(id)).FirstOrDefaultAsync();
            return PartialView(availableCourse);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            bool status = false;
            string message = string.Empty;
            var availableCourse = await _db.AvailableCourses.FindAsync(id);
            if (availableCourse != null)
            {
                _db.AvailableCourses.Remove(availableCourse);
                await _db.SaveChangesAsync();
                status = true;
                message = "Courses Deleted Successfully...";
                return new JsonResult { Data = new { status, message } };
            }

            return new JsonResult { Data = new { status = false, message = $"{id } not deleted" } };
        }


        public PartialViewResult UploadAvailableProgramme()
        {
            return PartialView();
        }


        [HttpPost]
        public async Task<ActionResult> UploadAvailableProgramme(HttpPostedFileBase excelfile)
        {
            if (excelfile == null || excelfile.ContentLength == 0)
            {
                ViewBag.Error = "Please Select a excel file <br/>";
                ViewBag.Message = "You must select an excel file before you click Upload button";
                TempData["Title"] = "Error.";

                return View("Index");
            }
            HttpPostedFileBase file = Request.Files["excelfile"];
            if (excelfile.FileName.EndsWith("xls", StringComparison.CurrentCulture) ||
                excelfile.FileName.EndsWith("xlsx", StringComparison.CurrentCulture))
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
                        ViewBag.ErrorInfo = "Please Leave no column or row Empty/Blank";
                        ViewBag.ErrorMessage = $" The \"{lineError}\" is not formatted properly ";
                        return View("ErrorException");

                    }
                    for (int row = 2; row <= noOfRow; row++)
                    {
                        var schoolProgrammeCode = workSheet.Cells[row, 1].Value.ToString().Trim();
                        var programmeCode = workSheet.Cells[row, 2].Value.ToString().Trim();


                        var programme = await _db.Programmes.AsNoTracking()
                                           .Where(x => x.ProgrammeCode.Trim().ToUpper().Equals(programmeCode.ToUpper()))
                                           .FirstOrDefaultAsync();

                        var schoolProgramme = await _db.SchoolProgrammes.AsNoTracking()
                                               .Where(x => x.SchoolProgrammeCode.Trim().ToUpper().Equals(schoolProgrammeCode.ToUpper()))
                                               .FirstOrDefaultAsync();

                        if (schoolProgramme == null)
                        {
                            ViewBag.ErrorInfo = $"The School Programme code uploaded  {schoolProgramme} at row {row} is not supported on the portal.";
                            ViewBag.ErrorMessage = "Please check the Student type spelling very well ";
                            return View("ErrorException");
                        }
                        if (programme == null)
                        {
                            ViewBag.ErrorInfo = $"The Department Option code uploaded  {programmeCode} at row {row} is not supported on the portal.";
                            ViewBag.ErrorMessage = "Please check the Student type spelling very well ";
                            return View("ErrorException");
                        }

                        try
                        {
                            var model = new AvailableCourse()
                            {
                                ProgrammeId = programme.ProgrammeId,
                                SchoolProgrammeId = schoolProgramme.SchoolProgrammeId,
                            };
                            _db.AvailableCourses.Add(model);
                            recordCount++;
                            lastrecord = $"The last record uploaded has the School Programme code {schoolProgrammeCode} and Dept Option Name {programmeCode}";
                        }
                        catch (Exception ex)
                        {
                            ViewBag.ErrorInfo = $"There is possible Dept code duplicate at row ({row}), Please check and try again";
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
                        ViewBag.ErrorInfo = "Erro saving available course, try again";
                        ViewBag.ErrorMessage = ex.Message;
                        return View("ErrorException");
                    }

                    message = $"You have successfully Uploaded {recordCount} records...  and {lastrecord}";
                    ViewBag.Message = message;

                }
                return RedirectToAction("Index", "Programmes", new { message });
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
