using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampusModel.CourseForum;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SwiftKampus.Controllers
{
    [Authorize]
    [Audit(AuditingLevel = 1)]
    public class ForumsController : BaseController
    {

        public ForumsController(SchoolDbContext db) : base(db)
        {

        }

        // GET: Forums
        public async Task<ActionResult> ForumIndex()
        {
            var froums = await _db.Forums.AsNoTracking().ToListAsync();
            return View(froums);
        }

        public async Task<ActionResult> GetQuestionReply(int id)
        {
            var model = new ForumReplyCommentVm();
            var commentList = new List<ForumComment>();
            model.ForumQuestion = await _db.ForumQuestions.AsNoTracking().Include(i => i.Forum)
                                        .Where(x => x.ForumQuestionId.Equals(id)).FirstOrDefaultAsync();
            var replies = await _db.ForumQuestionReplies.AsNoTracking().Where(x => x.ForumQuestionId.Equals(id))
                            .ToListAsync();
            foreach (var reply in replies)
            {
                var comments = await _db.ForumComments.AsNoTracking()
                                .Where(x => x.ContentId.Equals(reply.ForumQuestionReplyId))
                                .ToListAsync();
                if (comments != null && comments.Any())
                {
                    commentList.AddRange(comments);
                }

            }
            if (replies != null && replies.Any())
            {
                model.ForumQuestionRepy = replies;
            }
            model.ForumComment = commentList;
            ViewBag.UserId = userId;
            return View(model);
        }

        public async Task<ActionResult> GetForumDetails(int id)
        {
            var forumDetails = await _db.Forums.AsNoTracking().Include(i => i.ForumQuestions).Where(x => x.CourseId.Equals(id)).FirstOrDefaultAsync();
            if (forumDetails == null)
            {
                var course = await _db.Courses.Where(x => x.CourseId.Equals(id)).FirstOrDefaultAsync();
                if (course != null)
                {
                    var newForum = new Forum
                    {
                        CourseId = id,
                        Name = course.CourseName,
                        Description = course.CourseDescription
                    };
                    _db.Forums.Add(newForum);
                    await _db.SaveChangesAsync();
                }

            }
            var newForumDetails = await _db.Forums.Include(i => i.ForumQuestions).AsNoTracking()
                                .Where(x => x.CourseId.Equals(id)).FirstOrDefaultAsync();
            var model = new ForumIndexVm()
            {
                Forum = newForumDetails,
                ForumQuestion = await _db.ForumQuestions.AsNoTracking().Where(x => x.CourseId.Equals(id)).ToListAsync(),
            };
            ViewBag.UserId = userId;
            return View(model);
        }

        public async Task<PartialViewResult> GetForum(int id)
        {
            var forumDetails = await _db.Forums.AsNoTracking().Include(i => i.ForumQuestions).Where(x => x.CourseId.Equals(id)).FirstOrDefaultAsync();
            if (forumDetails == null)
            {
                var course = await _db.Courses.Where(x => x.CourseId.Equals(id)).FirstOrDefaultAsync();
                if (course != null)
                {
                    var newForum = new Forum
                    {
                        CourseId = id,
                        Name = course.CourseName,
                        Description = course.CourseDescription
                    };
                    _db.Forums.Add(newForum);
                    await _db.SaveChangesAsync();
                }

            }
            var newForumDetails = await _db.Forums.Include(i => i.ForumQuestions).AsNoTracking()
                .Where(x => x.CourseId.Equals(id)).FirstOrDefaultAsync();
            return PartialView(newForumDetails);
        }
        public async Task<ActionResult> GetQuestion(int id)
        {
            var forumQuestion = await _db.ForumQuestions.AsNoTracking().Where(x => x.CourseId.Equals(id)).ToListAsync();
            var data = forumQuestion.Select(s => new
            {
                s.CourseId,
                s.Title,
                s.Question,
                s.ForumQuestionId
            }).ToList();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }



        public async Task<PartialViewResult> GetQuestionDetails(int id)
        {
            var commentList = new List<ForumComment>();
            var forumQuestion = await _db.ForumQuestions.Include(i => i.Forum).Include(i => i.ForumQuestionReplies).AsNoTracking()
                            .Where(x => x.ForumQuestionId.Equals(id)).FirstOrDefaultAsync();
            foreach (var comment in forumQuestion.ForumQuestionReplies)
            {
                commentList.AddRange(_db.ForumComments.AsNoTracking().Where(x => x.ContentId.Equals(comment.ForumQuestionReplyId)));
            }
            commentList.AddRange(await _db.ForumComments.AsNoTracking()
                                    .Where(x => x.ContentId.Equals(forumQuestion.ForumQuestionId))
                                    .ToListAsync());
            var model = new ForumQuestionVm
            {
                ForumQuestion = forumQuestion,
                ForumComment = commentList,
                Forum = forumQuestion.Forum,
                ForumQuestionReplies = forumQuestion.ForumQuestionReplies.ToList()

            };
            return PartialView(model);
        }

        [HttpGet]
        public async Task<PartialViewResult> SaveForumQuestion(int id)
        {
            var student = await _db.Students.Where(x => x.StudentId.Equals(userId)).ToListAsync();
            var course = await _db.Courses.Where(x => x.CourseId.Equals(id)).ToListAsync();
            ViewBag.CourseId = new SelectList(course, "CourseId", "CourseName");
            ViewBag.StudentId = new SelectList(student, "StudentId", "FullName");
            return PartialView();
        }


        [HttpPost]
        [ValidateInput(false)]
        public async Task<JsonResult> SaveForumQuestionPost(ForumQuestion model)
        {
            var message = string.Empty;
            if (ModelState.IsValid)
            {
                _db.ForumQuestions.Add(model);
                await _db.SaveChangesAsync();
                message = $"You have successfully asked your question.";
                return new JsonResult { Data = new { status = true, message, id = model.CourseId } };
            }

            return new JsonResult { Data = new { status = true, message } };
        }

        [HttpGet]
        public async Task<PartialViewResult> EditForumQuestion(int id)
        {
            var forumQuestion = await _db.ForumQuestions.FindAsync(id);
            var student = await _db.Students.Where(x => x.StudentId.Equals(forumQuestion.StudentId)).ToListAsync();
            var course = await _db.Courses.Where(x => x.CourseId.Equals(forumQuestion.CourseId)).ToListAsync();
            ViewBag.CourseId = new SelectList(course, "CourseId", "CourseName");
            ViewBag.StudentId = new SelectList(student, "StudentId", "FullName");
            return PartialView(forumQuestion);
        }


        [HttpPost]
        [ValidateInput(false)]
        public async Task<JsonResult> EditForumQuestion(ForumQuestion model)
        {
            var message = string.Empty;
            if (ModelState.IsValid)
            {
                var forumQuestion = await _db.ForumQuestions.FindAsync(model.ForumQuestionId);
                forumQuestion.Question = model.Question;
                forumQuestion.Title = model.Title;
                _db.Entry(forumQuestion).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                message = $"You have successfully update your question.";
                return new JsonResult { Data = new { status = true, message, id = model.CourseId } };
            }

            return new JsonResult { Data = new { status = true, message } };
        }


        public PartialViewResult SaveQuestionReply()
        {
            ViewBag.ForumQuestionId = new SelectList(_db.ForumQuestions.AsNoTracking().ToList(), "FormQuestionId", "Title");
            return PartialView();
        }

        [HttpPost]
        public async Task<ActionResult> SaveQuestionReply(string Answer, string ForumQuestionId)
        {
            var message = string.Empty;
            if (ModelState.IsValid)
            {
                var model = new ForumQuestionReply
                {
                    ForumQuestionId = Convert.ToInt16(ForumQuestionId),
                    Answer = Answer,
                    ReplyDate = DateTime.Now,
                    UserId = userId
                };
                _db.ForumQuestionReplies.Add(model);
                await _db.SaveChangesAsync();
                message = $"Thanks for Replying the Question...";
                return new JsonResult { Data = new { status = true, message } };
            }

            return new JsonResult { Data = new { status = true, message, id = ForumQuestionId } };
        }
        [HttpPost]
        public async Task<ActionResult> SaveForumComment(string Body, string ContentId)
        {
            var message = string.Empty;
            if (ModelState.IsValid)
            {
                var model = new ForumComment()
                {
                    ContentId = Convert.ToInt16(ContentId),
                    Body = Body,
                    CommentDateTime = DateTime.Now,
                    UserId = userId
                };
                _db.ForumComments.Add(model);
                await _db.SaveChangesAsync();
                message = $"Your comment has been added successfully.";
                return new JsonResult { Data = new { status = true, message } };
            }

            return new JsonResult { Data = new { status = true, message, id = ContentId } };
        }

        [HttpGet]
        public PartialViewResult ReplyQuestion(int id)
        {
            ViewBag.QuestionId = id;
            return PartialView();
        }


        [HttpPost]
        [ValidateInput(false)]
        public async Task<JsonResult> ReplyQuestion(ForumQuestionReply model)
        {
            var message = string.Empty;

            var forumQuestionReply = new ForumQuestionReply()
            {
                Answer = model.Answer,
                ForumQuestionId = model.ForumQuestionId,
                ReplyDate = DateTime.Now,
                UserId = userId
            };
            _db.ForumQuestionReplies.Add(forumQuestionReply);
            await _db.SaveChangesAsync();
            message = $"You have replied this question successfully.";
            return new JsonResult { Data = new { status = true, message, id = model.ForumQuestionId } };


            //return new JsonResult { Data = new { status = true, message = message } };
        }
        [HttpGet]
        public async Task<PartialViewResult> EditReplyQuestion(int id)
        {
            var model = await _db.ForumQuestionReplies.FindAsync(id);
            return PartialView(model);
        }


        [HttpPost]
        [ValidateInput(false)]
        public async Task<JsonResult> EditReplyQuestion(ForumQuestionReply model)
        {
            var message = string.Empty;
            var forumQuestionReply = await _db.ForumQuestionReplies.FindAsync(model.ForumQuestionReplyId);

            forumQuestionReply.Answer = model.Answer;
            forumQuestionReply.ReplyDate = DateTime.Now;

            _db.Entry(forumQuestionReply).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            message = $"Your response has been updated successfully.";
            return new JsonResult { Data = new { status = true, message, id = model.ForumQuestionId } };


            //return new JsonResult { Data = new { status = true, message = message } };
        }

        [HttpGet]
        public PartialViewResult SaveComment(int id)
        {
            ViewBag.QuestionId = id;
            return PartialView();
        }


        [HttpPost]
        [ValidateInput(false)]
        public async Task<JsonResult> SaveComment(ForumComment model)
        {
            var message = string.Empty;
            var forumQuestionId = _db.ForumQuestionReplies.AsNoTracking()
                                    .Where(x => x.ForumQuestionReplyId.Equals(model.ContentId))
                                    .Select(s => s.ForumQuestionId);

            var forumQuestionReply = new ForumComment()
            {
                Body = model.Body,
                ContentId = model.ContentId,
                CommentDateTime = DateTime.Now,
                UserId = userId
            };
            _db.ForumComments.Add(forumQuestionReply);
            await _db.SaveChangesAsync();
            message = $"Your comment has been submitted successfully.";
            return new JsonResult { Data = new { status = true, message, id = forumQuestionId } };


            //return new JsonResult { Data = new { status = true, message = message } };
        }
        [HttpGet]
        public async Task<PartialViewResult> EditComment(int id)
        {
            var model = await _db.ForumComments.FindAsync(id);
            return PartialView(model);
        }


        [HttpPost]
        [ValidateInput(false)]
        public async Task<JsonResult> EditComment(ForumComment model)
        {
            var message = string.Empty;
            var forumQuestionId = _db.ForumQuestionReplies.AsNoTracking()
                                   .Where(x => x.ForumQuestionReplyId.Equals(model.ContentId))
                                   .Select(s => s.ForumQuestionId);
            var forumQuestionReply = await _db.ForumComments.FindAsync(model.ContentId);

            forumQuestionReply.Body = model.Body;
            forumQuestionReply.CommentDateTime = DateTime.Now;

            _db.Entry(forumQuestionReply).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            message = $"Your response has been updated successfully.";
            return new JsonResult { Data = new { status = true, message, id = forumQuestionId } };


            //return new JsonResult { Data = new { status = true, message = message } };
        }
    }
}