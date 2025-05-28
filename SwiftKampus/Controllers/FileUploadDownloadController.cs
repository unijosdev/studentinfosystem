using SwiftKampus.Models;
using SwiftKampus.Services;
using System;
using System.Linq;
using System.Web.Mvc;

namespace SwiftKampus.Controllers
{
    [Authorize]
    [Audit(AuditingLevel = 2)]
    public class FileUploadDownloadController : BaseController
    {
        readonly DownloadFiles obj;
        public FileUploadDownloadController(SchoolDbContext db) : base(db)
        {
            obj = new DownloadFiles();
        }

        public ActionResult Index()
        {
            var filesCollection = obj.GetFiles();
            return View(filesCollection);
        }

        public FileResult Download(string FileId)
        {
            int CurrentFileID = Convert.ToInt32(FileId);
            var filesCol = obj.GetFiles();
            string CurrentFileName = (from fls in filesCol
                                      where fls.FileId == CurrentFileID
                                      select fls.FilePath).First();

            string contentType = string.Empty;

            if (CurrentFileName.Contains(".pdf"))
            {
                contentType = "application/pdf";
            }

            else if (CurrentFileName.Contains(".docx"))
            {
                contentType = "application/docx";
            }
            return File(CurrentFileName, contentType, CurrentFileName);
        }

        //public ActionResult SchoolSetUp()
        //{
        //    return View();
        //}

        //[HttpPost]
        //public ActionResult SchoolSetUp(UploadVm model)
        //{
        //    string _FileName = String.Empty;
        //    try
        //    {
        //        if (model.File.ContentLength > 0)
        //        {
        //            _FileName = Path.GetFileName(model.File.FileName);
        //            string _path = HostingEnvironment.MapPath("~/Content/Images/") + _FileName;
        //            var directory = new DirectoryInfo(HostingEnvironment.MapPath("~/Content/Images/"));
        //            if (directory.Exists == false)
        //            {
        //                directory.Create();
        //            }
        //            model.File.SaveAs(_path);
        //        }
        //    }
        //    catch
        //    {
        //        ViewBag.Message = "File upload failed!!";
        //        return View(model);
        //    }
        //    Configuration objConfig = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~");
        //    AppSettingsSection objAppsettings = (AppSettingsSection)objConfig.GetSection("appSettings");
        //    //Edit
        //    if (objAppsettings != null)
        //    {
        //        objAppsettings.Settings["SchoolName"].Value = model.SchoolName;
        //        if (!String.IsNullOrEmpty(_FileName))
        //        {
        //            objAppsettings.Settings["SchoolImage"].Value = _FileName;
        //        }
        //        objConfig.Save();
        //    }
        //    return View("Index");
        //}
    }
}
