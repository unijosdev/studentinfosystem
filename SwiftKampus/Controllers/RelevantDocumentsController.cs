using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampus.ViewModels;
using SwiftKampus.ViewModels.StudentBioData;
using SwiftKampusModel.AddmissionApplicant;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SwiftKampus.Controllers
{
    [Authorize]
    [Audit(AuditingLevel = 2)]
    public class RelevantDocumentsController : BaseController
    {
        private readonly FileStorage _storageServices;

        public RelevantDocumentsController(SchoolDbContext db) : base(db)
        {
            _storageServices = new FileStorage();
        }
        // GET: RelevantDocuments
        public ActionResult Index()
        {
            ViewBag.ApplicantType = _applicantType;
            return View();
        }

        // GET: Return view to display upload of jamb admission letter for UG
        public ActionResult JambAdmissionLetterUpload()
        {
            var modeOfEntry = _studentQuery.GetStudent(userId).ModeOfEntry;
            ViewBag.ModeOfEntry = modeOfEntry.ToUpper();
            ViewBag.ApplicantType = _applicantType;
            return View();
        }
        public ActionResult AIndex()
        {
            ViewBag.ApplicantType = _applicantType;
            return View();
        }

        public async Task<ActionResult> GetIndex()
        {
            // dc.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var data = await _db.RelevantDocuments.AsNoTracking().Where(x => x.UserId.Equals(userId))
                .Select(s => new
                {
                    s.RelevantDocumentId,
                    s.FileName,
                    s.FileAddress,
                }).ToListAsync();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        // GET: RelevantDocuments/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RelevantDocument relevantDocument = await _db.RelevantDocuments.FindAsync(id);
            if (relevantDocument == null)
            {
                return HttpNotFound();
            }
            return View(relevantDocument);
        }

        public ActionResult Save()
        {
            ViewBag.ApplicantType = _applicantType.ApplicantType;
            return PartialView();
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<JsonResult> Save(RelevantDocument model)
        {
            if (ModelState.IsValid)
            {
                var fileName = string.Empty;
                if (model.File.ContentLength > 0)
                {
                    var result = _storageServices.SaveFile(model.File);
                    var relevantDocument = new RelevantDocument()
                    {
                        UserId = userId,
                        FileName = model.FileName,
                        FileAddress = result.FileName
                    };
                    _db.RelevantDocuments.Add(relevantDocument);
                }               
                await _db.SaveChangesAsync();
                return new JsonResult { Data = new { status = true, message = "File is uploaded Successfully" } };
            }
            return new JsonResult { Data = new { status = false, message = "Ooops... Files are uploaded Successfully" } };
        }

        // GET: RelevantDocuments/Create
        public ActionResult Create()
        {
            return View();
        }

        //POST: RelevantDocuments/Create
        //To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<JsonResult> Create(FileUploadVm model)
        {
            if (ModelState.IsValid)
            {
                var fileName = string.Empty;
                //t modelCount = 0;
                for (int i = 0; i < model.FileName.Length; i++)
                {
                    if (model.FileUpload[i].ContentLength > 0)
                    {
                        //fileName = $"{userId}{model.FileName[i]}.pdf";
                        //CloudBlobContainer blobContainer = _blobServices.GetCloudBlobContainer();
                        //CloudBlockBlob blob = blobContainer.GetBlockBlobReference(fileName);
                        //blob.UploadFromStream(model.FileUpload[i].InputStream);

                        //var relevantDocument = new RelevantDocument()
                        //{
                        //    UserId = userId,
                        //    FileName = model.FileName[i],
                        //    FileAddress = $"{SchoolSetUp.BlobAddress}{fileName}"
                        //};
                        //_db.RelevantDocuments.Add(relevantDocument);
                    }
                }
                await _db.SaveChangesAsync();
                return new JsonResult { Data = new { status = true, message = "Files are uploaded Successfully" } };
            }
            return new JsonResult { Data = new { status = false, message = "Ooops... Files are uploaded Successfully" } };
        }

        public ActionResult PCreate()
        {
            ViewBag.ApplicantType = _applicantType;
            var relevantDocuments = _db.RelevantDocuments.AsNoTracking().Where(x => x.UserId.Equals(userId)).ToList();
            var model = new UploadedDocumentVm()
            {
                RelevantDocument = new RelevantDocument(),
                RelevantDocuments = relevantDocuments
            };
            return View(model);
            //return View();
        }

        //POST: RelevantDocuments/Create
        //To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<JsonResult> PCreate(FileUploadVm model)
        {
            if (ModelState.IsValid)
            {
                var fileName = string.Empty;
                for (int i = 0; i < model.FileName.Length; i++)
                {
                    if (model.FileUpload[i].ContentLength > 0)
                    {
                        var result = _storageServices.SaveFile(model.FileUpload[i]);
                        var relevantDocument = new RelevantDocument()
                        {
                            UserId = userId,
                            FileName = model.FileName[i],
                            FileAddress = result.FileName
                        };
                        _db.RelevantDocuments.Add(relevantDocument);
                    }
                }

                var documents = _db.RelevantDocuments.AsNoTracking().Where(x => x.UserId.Equals(userId)).ToList();
                if (documents != null && documents.Any())
                {
                    foreach (var item in documents)
                    {
                        _storageServices.DeleteFile(item.FileAddress);
                        _db.Entry(item).State = EntityState.Deleted;
                    }
                }
                await _db.SaveChangesAsync();
                return new JsonResult { Data = new { status = true, message = "Files are uploaded Successfully" } };
            }
            return new JsonResult { Data = new { status = false, message = "Ooops... Files are uploaded Successfully" } };
        }

        // GET: RelevantDocuments/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RelevantDocument relevantDocument = await _db.RelevantDocuments.FindAsync(id);
            if (relevantDocument == null)
            {
                return HttpNotFound();
            }
            return View(relevantDocument);
        }

        // POST: RelevantDocuments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "RelevantDocumentId,FileName,FileAddress")] RelevantDocument relevantDocument)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(relevantDocument).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(relevantDocument);
        }

        // GET: RelevantDocuments/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RelevantDocument relevantDocument = await _db.RelevantDocuments.FindAsync(id);
            if (relevantDocument == null)
            {
                return HttpNotFound();
            }
            return View(relevantDocument);
        }

        // POST: RelevantDocuments/Delete/5
        [HttpPost, ActionName("Delete")]
        public async Task<ActionResult> Delete(int id)
        {
            bool status = false;
            string message = string.Empty;
            RelevantDocument relevantDocument = await _db.RelevantDocuments.FindAsync(id);
            if(relevantDocument != null)
            {
                if (!string.IsNullOrEmpty(relevantDocument?.FileAddress))
                {
                    _storageServices.DeleteFile(relevantDocument.FileAddress);
                }
                _db.RelevantDocuments.Remove(relevantDocument);
                await _db.SaveChangesAsync();
                message = "Document Deleted Successfully...";
                status = true;
                return new JsonResult { Data = new { status, message } };
            }
           
            return new JsonResult { Data = new { status, message = $"{id } not deleted" } };
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
