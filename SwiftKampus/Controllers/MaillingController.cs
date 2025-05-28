using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampusModel.SchoolMail;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SwiftKampus.Controllers
{
    [Authorize]
    [Audit(AuditingLevel = 2)]
    public class MaillingController : BaseController
    {

        public MaillingController(SchoolDbContext db) : base(db)
        {

        }
        // GET: Mailling
        public ActionResult Index()
        {
            var message = _db.SchoolMailMessages.AsNoTracking().Where(x => x.SenderId.Equals(userId))
                                      .OrderBy(x => x.MessageDataTime).ToList();
            ViewBag.DraftMessages = _db.SchoolDraftMessages.AsNoTracking().Where(x => x.SenderId.Equals(userId)).Count();
            ViewBag.SentMessages = message.Count();
            ViewBag.TrashMessages = message.Count(x => x.IsDeleted.Equals(true));
            ViewBag.ImportantMessages = message.Count(x => x.IsImportant.Equals(true));
            return View();
        }

        [ValidateInput(false)]
        public PartialViewResult ComposeMail()
        {
            return PartialView();
        }

        public async Task<PartialViewResult> ReplyMail(string senderId, int? messageId)
        {
            ViewBag.SenderId = senderId;
            if (messageId != null)
            {
                ViewBag.MessageBody = await _db.SchoolMailMessages.Where(x => x.SchoolMailMessageId.Equals((int)messageId))
                            .Select(x => x.MessageBody).FirstOrDefaultAsync();
            }

            return PartialView();
        }

        public async Task<PartialViewResult> ForwardMail(string senderId, int? messageId)
        {
            ViewBag.SenderId = senderId;
            if (messageId != null)
            {
                ViewBag.MessageBody = await _db.SchoolMailMessages.Where(x => x.SchoolMailMessageId.Equals((int)messageId))
                            .Select(x => x.MessageBody).FirstOrDefaultAsync();
            }

            return PartialView();
        }

        public PartialViewResult SentMail(string userId)
        {
            var sentMessage = _db.SchoolMailMessages.AsNoTracking().Where(x => x.SenderId.Equals(userId))
                .OrderBy(x => x.MessageDataTime).ToList();
            ViewBag.SentMessages = sentMessage.Count();
            return PartialView(sentMessage);
        }

        public PartialViewResult TrashMail(string userId)
        {
            var trashMessage = _db.SchoolMailMessages.AsNoTracking().Where(x => (x.SenderId.Equals(userId)
                                || x.RecieverId.Equals(userId)) && x.IsDeleted.Equals(true))
                                .OrderBy(x => x.MessageDataTime).ToList();
            ViewBag.TrashMessages = trashMessage.Count();
            return PartialView(trashMessage);
        }

        public PartialViewResult DraftMail(string userId)
        {
            var draftMessage = _db.SchoolDraftMessages.AsNoTracking().Where(x => x.SenderId.Equals(userId))
                .OrderBy(x => x.MessageDataTime).ToList();
            ViewBag.DraftMessages = draftMessage.Count();
            return PartialView(draftMessage);
        }

        public PartialViewResult ImportantMail(string userId)
        {
            var importantMessage = _db.SchoolMailMessages.AsNoTracking().Where(x => (x.SenderId.Equals(userId)
                                    || x.RecieverId.Equals(userId)) && x.IsImportant.Equals(true))
                                .OrderBy(x => x.MessageDataTime).ToList();
            ViewBag.ImportantMessages = importantMessage.Count();
            return PartialView(importantMessage);
        }

        public PartialViewResult InboxMail()
        {
            var inboxMessage = _db.SchoolMailMessages.AsNoTracking().Where(x => x.RecieverId.Equals(userId)
                                    && x.IsDeleted.Equals(false)).OrderByDescending(x => x.MessageDataTime)
                                    .ToList();
            ViewBag.UnReadMessages = inboxMessage.Count(x => x.HasRead.Equals(false));

            return PartialView(inboxMessage);
        }

        public async Task<PartialViewResult> DetailMail(int schoolMailMessagesId)
        {
            var schoolMailMessage = await _db.SchoolMailMessages.FindAsync(schoolMailMessagesId);
            if (schoolMailMessage != null)
            {
                schoolMailMessage.HasRead = true;
                _db.Entry(schoolMailMessage).State = EntityState.Modified;
                await _db.SaveChangesAsync();
            }
            return PartialView(schoolMailMessage);
        }

        public async Task<PartialViewResult> OtherDetailMail(int schoolMailMessagesId)
        {
            var schoolMailMessage = await _db.SchoolMailMessages.FindAsync(schoolMailMessagesId);
            if (schoolMailMessage != null)
            {
                schoolMailMessage.HasRead = true;
                _db.Entry(schoolMailMessage).State = EntityState.Modified;
                await _db.SaveChangesAsync();
            }
            return PartialView(schoolMailMessage);
        }

        public async Task<PartialViewResult> DraftDetailMail(int schoolMailMessagesId)
        {
            var schoolMailMessage = await _db.SchoolDraftMessages.FindAsync(schoolMailMessagesId);
            return PartialView(schoolMailMessage);
        }

        [HttpPost]
        [ValidateInput(false)]
        public async Task<ActionResult> SendMessage(SendMessageVm model)
        {
            string fullName = string.Empty;
            string recieverId = string.Empty;
            if (Request.IsAuthenticated)
            {
                if (ModelState.IsValid)
                {
                    if (User.IsInRole("Student"))
                    {
                        var student = await _db.Students.FindAsync(userId);
                        fullName = student?.FullName;
                    }
                    else
                    {
                        var staff = await _db.Staffs.FindAsync(userId);
                        fullName = staff?.FullName;
                    }
                    recieverId = GetStudentId(model.RecieverId.ToUpper().Trim());
                    if (string.IsNullOrEmpty(recieverId) || string.IsNullOrEmpty(fullName))
                    {
                        return new JsonResult { Data = new { status = false, message = "User Not identified" } };
                    }
                    var schoolMailMessage = new SchoolMailMessage()
                    {
                        SenderId = userId,
                        RecieverId = recieverId,
                        MessageSubject = model.MessageSubject,
                        MessageBody = model.MessageBody,
                        MessageDataTime = DateTime.Now,
                        RecieverFullName = fullName,
                        AttachmentLocation1 = model.AttachmentLocation1,
                        AttachmentLocation2 = model.AttachmentLocation2,
                        AttachmentLocation3 = model.AttachmentLocation3,
                        AttachmentLocation4 = model.AttachmentLocation4,
                        AttachmentLocation5 = model.AttachmentLocation5
                    };
                    _db.SchoolMailMessages.Add(schoolMailMessage);
                    await _db.SaveChangesAsync();
                    var message = "Message Sent Successfully.";
                    return new JsonResult { Data = new { status = true, message } };
                }
            }
            return new JsonResult { Data = new { status = false, message = "User Need to Login" } };
        }

        [HttpPost]
        public async Task<ActionResult> TrasList(List<string> messages)
        {
            foreach (var messageId in messages)
            {
                int id = Convert.ToInt16(messageId);
                var schoolMailMessage = await _db.SchoolMailMessages.FindAsync(id);
                if (schoolMailMessage != null)
                {
                    schoolMailMessage.IsDeleted = true;
                    _db.Entry(schoolMailMessage).State = EntityState.Modified;
                }
                await _db.SaveChangesAsync();
                var message = $"{messages.Count} Message(s) Trashed Successfully.";
                return new JsonResult { Data = new { status = true, message } };
            }
            return new JsonResult { Data = new { status = false, message = "Data not existing" } };
        }

        [HttpPost]
        public async Task<ActionResult> MarkAsRead(List<string> messages)
        {
            foreach (var messageId in messages)
            {
                int id = Convert.ToInt16(messageId);
                var schoolMailMessage = await _db.SchoolMailMessages.FindAsync(id);
                if (schoolMailMessage != null)
                {
                    schoolMailMessage.HasRead = true;
                    _db.Entry(schoolMailMessage).State = EntityState.Modified;
                }
                await _db.SaveChangesAsync();
                var message = $"{messages.Count} Message(s) Marked as Read Successfully.";
                return new JsonResult { Data = new { status = true, message } };
            }
            return new JsonResult { Data = new { status = false, message = "Data not existing" } };
        }

        [HttpPost]
        public async Task<ActionResult> MarkAsImportant(List<string> messages)
        {
            foreach (var messageId in messages)
            {
                int id = Convert.ToInt16(messageId);
                var schoolMailMessage = await _db.SchoolMailMessages.FindAsync(id);
                if (schoolMailMessage != null)
                {
                    schoolMailMessage.IsImportant = true;
                    _db.Entry(schoolMailMessage).State = EntityState.Modified;
                }
                await _db.SaveChangesAsync();
                var message = $"{messages.Count} Message(s) Matked as Important Successfully.";
                return new JsonResult { Data = new { status = true, message } };
            }
            return new JsonResult { Data = new { status = false, message = "Data not existing" } };
        }

        [ValidateInput(false)]
        public async Task<ActionResult> SaveDraft(SendMessageVm model)
        {
            if (Request.IsAuthenticated && ModelState.IsValid)
            {
                var recieverId = GetStudentId(model.RecieverId.ToUpper().Trim());
                var schoolDraftMessage = new SchoolDraftMessage()
                {
                    SenderId = userId,
                    RecieverId = recieverId,
                    MessageSubject = model.MessageSubject,
                    MessageBody = model.MessageBody,
                    MessageDataTime = DateTime.Now
                };
                _db.SchoolDraftMessages.Add(schoolDraftMessage);
                await _db.SaveChangesAsync();
                var message = "Message Saved as Draft Successfully.";
                return new JsonResult { Data = new { status = true, message } };
            }
            return new JsonResult { Data = new { status = false, message = "User Need to Login" } };
        }

        private string GetStudentId(string recieverId)
        {
            var checkStudent = _db.Students.Where(x => x.MatricNo.Equals(recieverId)
                                    || x.Email.Equals(recieverId) || x.StudentId.Equals(recieverId))
                                    .Select(s => s.StudentId)
                                    .FirstOrDefault();
            var checkStaff = _db.Staffs.Where(x => x.StaffId.Equals(recieverId)
                                    || x.Email.Equals(recieverId)).Select(s => s.StaffId)
                                    .FirstOrDefault();
            if (!string.IsNullOrEmpty(checkStudent))
            {
                return checkStudent;
            }
            if (!string.IsNullOrEmpty(checkStaff))
            {
                return checkStaff;
            }
            return String.Empty;
        }
    }
}