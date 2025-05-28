using OfficeOpenXml;
using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampusModel;
using SwiftKampusModel.Accomodation;
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
    public class BlocksController : BaseController
    {

        public BlocksController(SchoolDbContext db) : base(db)
        {

        }

        // GET: Blocks
        public ActionResult Index(string message)
        {
            ViewBag.Message = message;
            return View();
        }
        public async Task<ActionResult> GetIndex()
        {
            // dc.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var data = await _db.Blocks.Include(i => i.Hostel).AsNoTracking().Select(s => new
            {
                s.BlockId,
                s.BlockName,
                s.Gender,
                s.Hostel.HostelName,
                s.Capacity
            }).ToListAsync();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        public async Task<PartialViewResult> Save(int id)
        {
            var block = await _db.Blocks.FindAsync(id);
            ViewBag.HostelId = new SelectList(_db.Hostels.AsNoTracking(), "HostelId", "HostelCode");
            var gender = from Gender s in Enum.GetValues(typeof(Gender))
                         select new { ID = s, Name = s.ToString() };

            ViewBag.Gender = new MultiSelectList(gender, "Name", "Name");
            return PartialView(block);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Save(Block model)
        {
            bool status = false;
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                if (model.BlockId > 0)
                {
                    var block = await _db.Blocks.FindAsync(model.BlockId);
                    if (block != null)
                    {
                        block.HostelId = model.HostelId;
                        block.BlockName = model.BlockName;
                        block.Capacity = model.Capacity;
                        block.Gender = model.Gender;
                        _db.Entry(block).State = EntityState.Modified;
                        await _db.SaveChangesAsync();
                        message = $"{model.BlockName} Updated Successfully...";
                        return new JsonResult { Data = new { status = true, message } };

                    }
                }
                else
                {
                    _db.Blocks.Add(model);
                    await _db.SaveChangesAsync();
                    message = $"{model.BlockName} Added Successfully.";
                    return new JsonResult { Data = new { status = true, message } };
                }
            }
            return new JsonResult { Data = new { status, message } };
            //return View(subject);
        }



        // GET: Blocks/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Block block = await _db.Blocks.FindAsync(id);
            if (block == null)
            {
                return HttpNotFound();
            }
            return View(block);
        }

        // GET: Blocks/Create
        public ActionResult Create()
        {
            ViewBag.HostelId = new SelectList(_db.Hostels.AsNoTracking(), "HostelId", "HostelName");
            return View();
        }

        // POST: Blocks/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "BlockId,HostelId,BlockName,Capacity")] Block block)
        {
            if (ModelState.IsValid)
            {
                _db.Blocks.Add(block);
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.HostelId = new SelectList(_db.Hostels.AsNoTracking(), "HostelId", "HostelName", block.HostelId);
            return View(block);
        }

        // GET: Blocks/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Block block = await _db.Blocks.FindAsync(id);
            if (block == null)
            {
                return HttpNotFound();
            }
            ViewBag.HostelId = new SelectList(_db.Hostels.AsNoTracking(), "HostelId", "HostelName", block.HostelId);
            return View(block);
        }

        // POST: Blocks/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "BlockId,HostelId,BlockName,Capacity")] Block block)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(block).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.HostelId = new SelectList(_db.Hostels.AsNoTracking(), "HostelId", "HostelName", block.HostelId);
            return View(block);
        }

        // GET: Blocks/Delete/5
        public async Task<PartialViewResult> Delete(int id)
        {
            var block = await _db.Blocks.FindAsync(id);
            return PartialView(block);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            bool status = false;
            string message = string.Empty;
            var block = await _db.Blocks.FindAsync(id);
            if (block != null)
            {
                _db.Blocks.Remove(block);
                await _db.SaveChangesAsync();
                status = true;
                message = "Block Deleted Successfully...";
                return new JsonResult { Data = new { status, message } };
            }

            return new JsonResult { Data = new { status = false, message = $"{id } not deleted" } };
        }

        public PartialViewResult UploadHostelBlock()
        {
            return PartialView();
        }


        [HttpPost]
        public async Task<ActionResult> UploadHostelBlock(HttpPostedFileBase excelfile)
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
                    int requiredField = 4;

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
                        return RedirectToAction("Index", "Blocks", new { message = lineError });
                    }
                    for (int row = 2; row <= noOfRow; row++)
                    {
                        var hostelCode = workSheet.Cells[row, 1].Value.ToString().Trim();
                        var gender = workSheet.Cells[row, 4].Value.ToString().Trim();
                        var hostel = await _db.Hostels.AsNoTracking()
                            .Where(x => x.HostelCode.ToUpper().Trim().Equals(hostelCode.ToUpper()))
                            .FirstOrDefaultAsync();

                        if (hostel == null)
                        {
                            ViewBag.ErrorInfo = "Hostel Code not valid";
                            ViewBag.ErrorMessage = $" The \"{hostelCode}\" specified in the excel doesn't exist on the portal. " +
                                                   $"Please add/update the Hostel first before uploading or correct the excel sheet...";
                            return View("ErrorException");
                        }

                        if(gender.ToUpper().Equals("M") || gender.ToUpper().Equals("MALE"))
                        {
                            gender = "Male";
                        }
                        else if (gender.ToUpper().Equals("F") || gender.ToUpper().Equals("FEMALE"))
                        {
                            gender = "Female";
                        }
                        else
                        {

                            ViewBag.ErrorInfo = "Hostel gender not valid yet";
                            ViewBag.ErrorMessage = $" The \"{gender}\" specified in the excel doesn't exist. " +
                                                   $"Please add/update the gender first before uploading or correct the excel sheet...";
                            return View("ErrorException");
                        }

                        try
                        {
                            var block = new Block()
                            {
                                HostelId = hostel.HostelId,
                                BlockName = workSheet.Cells[row, 2].Value.ToString().Trim(),
                                Gender = gender,
                                Capacity = Convert.ToUInt16(workSheet.Cells[row, 3].Value.ToString().Trim())
                            };

                            _db.Blocks.Add(block);
                            recordCount++;
                            lastrecord = $"The last Updated record has the Hostel Name {block.BlockName}";
                        }
                        catch (Exception ex)
                        {
                            ViewBag.ErrorInfo = $"Error Saving the hostels in row {row} of the excel";
                            ViewBag.ErrorMessage = ex.Message;
                            return View("ErrorException");
                        }
                    }
                    await _db.SaveChangesAsync();
                    message = $"You have successfully Uploaded {recordCount} records...  and {lastrecord}";
                    ViewBag.Message = message;
                    return RedirectToAction("Index", "Blocks", new { message });
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
