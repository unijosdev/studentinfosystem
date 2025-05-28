using Gma.QrCodeNet.Encoding;
using Gma.QrCodeNet.Encoding.Windows.Render;
using Microsoft.AspNet.Identity;
using Microsoft.WindowsAzure.Storage.Blob;
using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampus.ViewModels;
using SwiftKampusModel;
using SwiftKampusModel.AddmissionApplicant;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SwiftKampus.Controllers
{
    [Audit(AuditingLevel = 2)]
    public class HomeController : BaseController
    {
        AzureBlob _blobServices;
        public HomeController(SchoolDbContext db) : base(db)
        {
            _blobServices = new AzureBlob();
        }

        public ActionResult BarcodeImage(string barcodeText)
        {
            // generating a barcode here. Code is taken from QrCode.Net library
            QrEncoder qrEncoder = new QrEncoder(ErrorCorrectionLevel.H);
            QrCode qrCode = new QrCode();
            qrEncoder.TryEncode(barcodeText, out qrCode);
            GraphicsRenderer renderer = new GraphicsRenderer(new FixedModuleSize(4, QuietZoneModules.Four), Brushes.Black, Brushes.White);

            Stream memoryStream = new MemoryStream();
            renderer.WriteToStream(qrCode.Matrix, ImageFormat.Png, memoryStream);

            // very important to reset memory stream to a starting position, otherwise you would get 0 bytes returned
            memoryStream.Position = 0;

            var resultStream = new FileStreamResult(memoryStream, "image/png")
            {
                FileDownloadName = String.Format("{0}.png", barcodeText)
            };

            return resultStream;
        }

        public async Task<ActionResult> Index()
        {
           
            var fullName = $"{ProgrammeCategory.UnderGraduate.ToString().ToUpper()} {ProgrammeType.Full_Time.ToString().ToUpper()}";

            var schoolProgramme = await _db.SchoolProgrammes.Include(i => i.ApplicantFeeSettings).AsNoTracking()
                                    .Where(x => x.ActiveSale.Equals(true)).ToListAsync();
            //schoolProgramme = schoolProgramme.Where(x => x.FullName.ToUpper() != fullName).ToList();

            var programmeList = new List<SchoolProgramme>();

            schoolProgramme = schoolProgramme.Where(x => x.FullName.ToUpper() != fullName).ToList();
            foreach (var programmes in schoolProgramme)
            {
                programmeList.Add(programmes);
            }
            ViewData.Add("ActionMessage", "View index page");
            ViewBag.Programme = programmeList;

            //var emailService = new EmailService();
            //await emailService.SendAsync(new IdentityMessage
            //{
            //    Destination = "Kunlesymls@gmail.com",
            //    Body = $"Your Matric No  has been generated and UNIJOS Email." +
            //    $"You are now to login with this generated UNIJOS Email on the portal. Visit www.unijos.edu.ng for detail",
            //    Subject = "MARIC NO GENERATED"
            //});

            var no = DateTime.Now.Ticks;
            string value = no.ToString();
            //var sb = new StringBuilder();
            //for (int i =1 ; i < 6; i++)
            //{
            //    sb.Append(value[value.Length - i]);
            //}
            value = value.Substring(value.Length - 5);
            ViewBag.Number = value;
            return View(schoolProgramme);
        }

        public ActionResult PlasuIndex()
        {
            return View();
        }

        public ActionResult GroupChat()
        {
            return View();
        }


        [Authorize]
        public ActionResult ApplicantDashBoard()
        {
            ViewBag.PaymentStatus = _IsPayedApplicationFee;
                
            ViewBag.ScreenningClosed = _db.SchoolProgrammes.AsNoTracking().Where(x => x.ActiveSale.Equals(true)
                                        && x.ProgrammeCategory.Equals(ProgrammeCategory.UnderGraduate.ToString())).FirstOrDefault() == null ? true : false;
            ViewBag.UserId = userId;

            var model = _applicantType;
            var utmeApplicants = _db.UtmeApplicants.AsNoTracking().Where(x => x.Email.Equals(userId)).FirstOrDefault();
            
            if (utmeApplicants != null)
            {
                ViewBag.UtmeType = utmeApplicants.IsDirectEntry;
                var olevelResult = _db.ApplicantOLevelResults.Include(i => i.Subject).AsNoTracking()
                                      .Where(x => x.ApplicantId.Equals(userId)).ToList();
                ViewBag.HasCompleteOLevel = olevelResult.Count > 2 ? "true" : "false";
            }
            Applicant applicant = _db.Applicants.AsNoTracking()
                                    .Where(s => s.ApplicantEmail.Trim().ToUpper().Equals(userId.Trim().ToUpper()))
                                    .FirstOrDefault();
            if (applicant != null)
            {
                if (applicant.IsDeptApproved == null)
                {
                    ViewBag.ProcessStatus = "Pending Approval";
                }
                if (applicant.IsDeptApproved.Equals(true))
                {
                    ViewBag.ProcessStatus = "Approved by Department";
                }
                else
                {
                    ViewBag.ProcessStatus = $"Not Eligible Reason: {applicant.ReasonDeptForRejection}";
                }
            }
            return View(model);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult AcademicofficerDashboard()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }
        public ActionResult DeanDashBoard()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult HodDashboard()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult LecturerDashboard()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }
        public ActionResult LevelcordDashboard()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult SendHotmail()
        {
            MailMessage msg = new MailMessage();
            msg.To.Add(new MailAddress("anaobii@unijos.edu.ng", "Joseph Ajileye"));
            msg.From = new MailAddress("notification@unijos.edu.ng", "UNIJOS");
            msg.Subject = "This is a Test Mail";
            msg.Body = "This is a test message using Exchange OnLine";
            msg.IsBodyHtml = true;


            //SmtpClient client = new SmtpClient("domain-com.mail.protection.outlook.com");
            SmtpClient client = new SmtpClient
            {
                UseDefaultCredentials = false,
                Credentials = new System.Net.NetworkCredential("notification@unijos.edu.ng", "UjPortal098", "MicrosoftOffice365Domain.com"),
                Host = "smtp.office365.com",
                EnableSsl = true,
                TargetName = "STARTTLS/smtp.office365.com",
                Port = 587, // You can use Port 25 if 587 is blocked (mine is!)
                DeliveryMethod = SmtpDeliveryMethod.Network
            };
            try
            {
                client.Send(msg);
                ViewBag.Message = "Sent Successfully";
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
            }
            return View();
        }

        public PartialViewResult Calender()
        {
            ViewBag.Message = "Your contact page.";
            return PartialView();
        }


        [Authorize]
        public async Task<ActionResult> DashBoard()
        {
            int totalMaleStudent = await _db.Students.AsNoTracking().CountAsync(s => s.Gender.Equals("Male"));
            int totalFemaleStudent = await _db.Students.AsNoTracking().CountAsync(s => s.Gender.Equals("Female"));
            int active = await _db.Students.AsNoTracking().CountAsync(s => s.Active.Equals(true));
            int graduatedStudent = await _db.Students.AsNoTracking().CountAsync(s => s.IsGraduated.Equals(true));
            int totalStudent = await _db.Students.AsNoTracking().CountAsync();
            int totalStaff = await _db.Staffs.AsNoTracking().CountAsync();
            int MscTotalStd = await _db.Students.AsNoTracking().CountAsync(s => s.Active.Equals(true) && s.SchoolProgramme.SchoolProgrammeCode.Equals("MP") && s.Session.SessionName.Equals("2022/2023"));
            int PhDSTotalStd = await _db.Students.AsNoTracking().CountAsync(s => s.Active.Equals(true) && s.SchoolProgramme.SchoolProgrammeCode.Equals("DS") && s.Session.SessionName.Equals("2022/2023"));
            int PhDOTotalStd = await _db.Students.AsNoTracking().CountAsync(s => s.Active.Equals(true) && s.SchoolProgramme.SchoolProgrammeCode.Equals("DP") && s.Session.SessionName.Equals("2022/2023"));
            int PGDTotalStd = await _db.Students.AsNoTracking().CountAsync(s => s.Active.Equals(true) && s.SchoolProgramme.SchoolProgrammeCode.Equals("PP") && s.Session.SessionName.Equals("2022/2023"));
            int MBATotalStd = await _db.Students.AsNoTracking().CountAsync(s => s.Active.Equals(true) && s.SchoolProgramme.SchoolProgrammeCode.Equals("MBA") && s.Session.SessionName.Equals("2022/2023"));
            int NewUg = await _db.Students.AsNoTracking().CountAsync(s => s.Active.Equals(true) && s.SchoolProgramme.SchoolProgrammeCode.Equals("UG") && s.Session.SessionName.Equals("2024/2025"));
            int AllUG = await _db.Students.AsNoTracking().CountAsync(s => s.Active.Equals(true) && s.SchoolProgramme.SchoolProgrammeCode.Equals("UG"));
            int RegNewUG = await _db.Students.AsNoTracking().CountAsync(s => s.Active.Equals(true) && s.SchoolProgramme.SchoolProgrammeCode.Equals("UG") && s.Session.SessionName.Equals("2024/2025") && !string.IsNullOrEmpty(s.MatricNo));


            double val1 = totalMaleStudent * 100;

            double val2 = totalFemaleStudent * 100;

            double boysPercentage = Math.Round(val1 / totalStudent, 2);
            double femalePercentage = Math.Round(val2 / totalStudent, 2);

            ViewBag.MaleStudent = totalMaleStudent;
            ViewBag.Femalestudent = totalFemaleStudent;
            ViewBag.TotalStudent = totalStudent;
            ViewBag.TotalStaff = totalStaff;
            ViewBag.BoysPercentage = boysPercentage;
            ViewBag.FemalePercentage = femalePercentage;
            ViewBag.ActiveStudent = active;
            ViewBag.GraduatedStudent = graduatedStudent;
            ViewBag.Faculty = await _db.Faculties.AsNoTracking().CountAsync();
            ViewBag.Department = await _db.Departments.AsNoTracking().CountAsync();
            ViewBag.Programme = await _db.Programmes.AsNoTracking().CountAsync();
            ViewBag.MscTotalStd = MscTotalStd;
            ViewBag.PhDSTotalStd = PhDSTotalStd;
            ViewBag.PhDOTotalStd = PhDOTotalStd;
            ViewBag.PGDTotalStd = PGDTotalStd;
            ViewBag.MBATotalStd = MBATotalStd;
            ViewBag.NewUg = NewUg;
            ViewBag.RegNewUG = RegNewUG;
            ViewBag.AllUG = AllUG;

            ViewBag.Semester = new SelectList(_db.Semesters.AsNoTracking(), "SemesterName", "SemesterName");
            ViewBag.SessionName = new SelectList(_db.Sessions.AsNoTracking(), "SessionName", "SessionName");

            var userActivity = await _query.UserActivityStatistic();
            ViewBag.OnlineUser = userActivity.Item1;
            ViewBag.OnlineUserPercentage = userActivity.Item2;
            ViewBag.AllUsers = userActivity.Item3;

            var model = new AdminDashboardVm();
            var utmeApplicants = await _db.UtmeApplicants.Include(x => x.Session).AsNoTracking().Where(x => x.IsDirectEntry.Equals(false) && x.Session.SessionName.Equals("2023/2024") || x.Session.SessionName.Equals("2024/2025")).ToListAsync();
            model.TotalUtmeScreening = utmeApplicants.Count();
            model.RegisteredUtme = utmeApplicants.Count(x => x.HasRegistered.Equals(true));
            model.RegisteredUtmePercentage = Math.Round(((model.RegisteredUtme * 100) / (double)model.TotalUtmeScreening), 2);
            model.UnRegisteredUtme = model.TotalUtmeScreening - model.RegisteredUtme;
            model.UnRegisteredUtmePercentage = Math.Round(((model.UnRegisteredUtme * 100) / (double)model.TotalUtmeScreening), 2);


            var de = await _db.UtmeApplicants.Include(x => x.Session).AsNoTracking().Where(x => x.IsDirectEntry.Equals(true) && x.Session.SessionName.Equals("2023/2024") || x.Session.SessionName.Equals("2024/2025")).ToListAsync();

            model.TotalDeScreening = de.Count();
            model.RegisteredDe = de.Count(x => x.HasRegistered.Equals(true));
            model.RegisteredDePercentage = Math.Round(((model.RegisteredDe * 100) / (double)model.TotalDeScreening), 2);
            model.UnRegisteredDe = model.TotalDeScreening - model.RegisteredDe;
            model.UnRegisteredDePercentage = Math.Round(((model.UnRegisteredDe * 100) / (double)model.TotalDeScreening), 2);

            return View(model);
        }

        //[RecurringAuthorize]
        public ActionResult SchoolSetUp()
        {
            return View();
        }

        //[HttpPost]
        //public ActionResult SchoolSetUp(SetUpVm model, HttpPostedFileBase File)
        //{
        //    string _FileName = String.Empty;

        //    if (File?.ContentLength > 0)
        //    {
        //        //    _FileName = Path.GetFileName(model.File.FileName);
        //        //    string _path = HostingEnvironment.MapPath("~/Content/Images/") + _FileName;
        //        //    var directory = new DirectoryInfo(HostingEnvironment.MapPath("~/Content/Images/"));
        //        //    if (directory.Exists == false)
        //        //    {
        //        //        directory.Create();
        //        //    }
        //        //    model.File.SaveAs(_path);               

        //        if (File.ContentLength > 0)
        //        {

        //            CloudBlobContainer blobContainer = _blobServices.GetCloudBlobContainer();
        //            CloudBlockBlob blob = blobContainer.GetBlockBlobReference(File.FileName);
        //            blob.UploadFromStream(File.InputStream);
        //        }

        //    }
        //    return View("SchoolSetUp");
        //}
        public string DeleteImg(string Name)
        {
            //Uri uri = new Uri(Name);
            //string filename = System.IO.Path.GetFileName(uri.LocalPath);
            CloudBlobContainer blobContainer = _blobServices.GetCloudBlobContainer();
            CloudBlockBlob blob = blobContainer.GetBlockBlobReference(Name);
            blob.Delete();
            return "File Successfully Deleted";

        }

        public PartialViewResult NewCalender()
        {
            return PartialView();
        }

        public string Init()
        {
            bool rslt = Utils.InitialiseDiary();
            return rslt.ToString();
        }

        public void UpdateEvent(int id, string NewEventStart, string NewEventEnd)
        {
            DiaryEvent.UpdateDiaryEvent(id, NewEventStart, NewEventEnd);
        }

        public bool SaveEvent(string Title, string NewEventDate, string NewEventTime, string NewEventDuration)
        {
            return DiaryEvent.CreateNewEvent(Title, NewEventDate, NewEventTime, NewEventDuration);
        }

        public JsonResult GetDiarySummary(double start, double end)
        {
            var ApptListForDate = DiaryEvent.LoadAppointmentSummaryInDateRange(start, end);
            var eventList = from e in ApptListForDate
                            select new
                            {
                                id = e.ID,
                                title = e.Title,
                                start = e.StartDateString,
                                end = e.EndDateString,
                                someKey = e.SomeImportantKeyID,
                                allDay = false
                            };
            var rows = eventList.ToArray();
            return Json(rows, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetDiaryEvents(double start, double end)
        {
            var ApptListForDate = DiaryEvent.LoadAllAppointmentsInDateRange(start, end);
            var eventList = from e in ApptListForDate
                            select new
                            {
                                id = e.ID,
                                title = e.Title,
                                start = e.StartDateString,
                                end = e.EndDateString,
                                color = e.StatusColor,
                                className = e.ClassName,
                                someKey = e.SomeImportantKeyID,
                                allDay = false
                            };
            var rows = eventList.ToArray();
            return Json(rows, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetEvents()
        {
            using (SchoolDbContext dc = new SchoolDbContext())
            {
                var events = dc.AppointmentDiary.ToList();
                return new JsonResult { Data = events, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
        }

        public ActionResult StudentPortalGuide()
        {
            return View();
        }

        public ActionResult Support()
        {
            return View();
        }
    }
}