using Microsoft.AspNet.Identity;
using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampusModel;
using SwiftKampusModel.Classroom;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Web.Mvc;

namespace SwiftKampus.Controllers
{
    [Authorize]
    [Audit(AuditingLevel = 2)]
    public class ELearningController : BaseController
    {

        public ELearningController(SchoolDbContext db) : base(db)
        {

        }


        // GET: ELearning
        public ActionResult Index(string message)
        {
            ViewBag.Message = message;
            return View();
        }

        public ActionResult StudentsIndex(string message)
        {
            ViewBag.Message = message;
            return View();
        }

        public async Task<PartialViewResult> GetStudentCourses()
        {
            var courseList = new List<Course>();

            if (User.IsInRole(RoleName.Student))
            {
                var id = User.Identity.GetUserId();
                var courseReg = await _db.CourseRegistrations.AsNoTracking().Where(x => x.StudentId.Equals(id)
                                                && x.Semester.SemesterId.Equals(semesterId) && x.Session.SessionId.Equals(sessionId))
                    .Select(s => s.CourseId).ToListAsync();
                foreach (var courseId in courseReg)
                {
                    var course = await _db.Courses.Include(c => c.Modules).AsNoTracking()
                        .Where(x => x.CourseId.Equals(courseId)).FirstOrDefaultAsync();
                    courseList.Add(course);
                }
            }
            return PartialView(courseList);
        }
        public async Task<ActionResult> MyCourses()
        {
            var courseList = new List<Course>();
            //var courses = await _db.Courses.AsNoTracking().Include(c => c.Department)
            //    .Include(c => c.Level).Include(c => c.Programme).Include(c => c.Semester).ToListAsync();
            if (User.IsInRole(RoleName.Academic) || User.IsInRole(RoleName.Hod) || User.IsInRole(RoleName.Dean))
            {
                var id = User.Identity.GetUserId();
                var assignedCourse = await _db.AssignedCourses.Include(i => i.Semester).Include(i => i.Session).AsNoTracking()
                                        .Where(x => x.StaffId.Equals(id)
                                        && x.Semester.SemesterId.Equals(semesterId)
                                        && x.Session.SessionId.Equals(sessionId))
                                        .Select(s => s.CourseId).ToListAsync();
                foreach (var courseId in assignedCourse)
                {
                    var course = await _db.Courses.Include(c => c.Level).Include(c => c.Programme).AsNoTracking()
                                        .Where(x => x.CourseId.Equals(courseId)).FirstOrDefaultAsync();
                    courseList.Add(course);
                }
            }
            if (User.IsInRole(RoleName.Student))
            {
                var id = User.Identity.GetUserId();
                var courseReg = await _db.CourseRegistrations.AsNoTracking().Where(x => x.StudentId.Equals(id)
                                                && x.Semester.SemesterId.Equals(semesterId)
                                                && x.Session.SessionId.Equals(sessionId))
                    .Select(s => s.CourseId).ToListAsync();
                foreach (var courseId in courseReg)
                {
                    var course = await _db.Courses.Include(c => c.Level).Include(c => c.Programme).AsNoTracking()
                        .Where(x => x.CourseId.Equals(courseId)).FirstOrDefaultAsync();
                    courseList.Add(course);
                }
            }
            var data = courseList.Select(s => new
            {
                s.CourseId,
                s.Level.LevelName,
                s.CourseName,
                s.CourseCode,
                s.Programme.ProgrammeName,
                s.CourseDescription,
                s.Credits
            }).ToList();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        public async Task<PartialViewResult> CourseDetails(int id)
        {
            var course = await _db.Courses.AsNoTracking()
                .Include(c => c.Level).Include(c => c.Programme).Include(c => c.Semester)
                .Include(i => i.Modules)
                .Where(x => x.CourseId.Equals(id)).FirstOrDefaultAsync();

            return PartialView(course);
        }


        public async Task<PartialViewResult> SaveModule(int id)
        {
            var course = await _db.Courses.AsNoTracking().Where(x => x.CourseId.Equals(id))
                     .ToListAsync();
            ViewBag.CourseId = new SelectList(course, "CourseId", "CourseName");
            return PartialView();
        }
        public async Task<PartialViewResult> EditModule(int id)
        {
            var module = await _db.Modules.FindAsync(id);

            var course = await _db.Courses.AsNoTracking().Where(x => x.CourseId.Equals(module.CourseId))
                .ToListAsync();
            ViewBag.CourseId = new SelectList(course, "CourseId", "CourseName");
            return PartialView(module);
        }

        // POST: Modules/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SaveModule(Module model)
        {
            if (ModelState.IsValid)
            {
                if (model.ModuleId > 0)
                {
                    var module = await _db.Modules.FindAsync(model.ModuleId);
                    if (module != null)
                    {
                        module.CourseId = model.CourseId;
                        module.ModuleName = model.ModuleName;
                        module.ModuleDescription = model.ModuleDescription;
                        module.ExpectedTime = model.ExpectedTime;
                        _db.Entry(module).State = EntityState.Modified;
                    }
                    await _db.SaveChangesAsync();
                    var message = $"{model.ModuleName} Updated Successfully";
                    return new JsonResult { Data = new { status = true, message, id = model.CourseId } };

                }
                else
                {
                    _db.Modules.Add(model);
                    await _db.SaveChangesAsync();
                    var message = $"{model.ModuleName} Added Successfully";
                    return new JsonResult { Data = new { status = true, message, id = model.CourseId } };
                }

            }
            return new JsonResult { Data = new { status = false, message = "Model Not Correct", id = model.CourseId } };
        }

        public async Task<PartialViewResult> SaveTopic(int id)
        {

            //var topic = await _db.Topics.AsNoTracking().Where(x => x.TopicId.Equals(id)).FirstOrDefaultAsync();
            //if (topic != null)
            //{
            //    var modules = await _db.Modules.AsNoTracking().Where(x => x.ModuleId.Equals(topic.ModuleId)).ToListAsync();
            //    ViewBag.ModuleId = new SelectList(modules, "ModuleId", "ModuleName");
            //    return PartialView(topic);
            //}

            var module = await _db.Modules.AsNoTracking().Where(x => x.ModuleId.Equals(id)).ToListAsync();
            ViewBag.ModuleId = new SelectList(module, "ModuleId", "ModuleName");
            return PartialView();
        }
        public async Task<PartialViewResult> EditTopic(int id)
        {
            var topic = await _db.Topics.AsNoTracking().Where(x => x.TopicId.Equals(id)).FirstOrDefaultAsync();
            if (topic != null)
            {
                var modules = await _db.Modules.AsNoTracking().Where(x => x.ModuleId.Equals(topic.ModuleId)).ToListAsync();
                ViewBag.ModuleId = new SelectList(modules, "ModuleId", "ModuleName");

            }
            return PartialView(topic);
        }

        // POST: Modules/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SaveTopic(Topic model)
        {
            if (ModelState.IsValid)
            {
                if (model.TopicId > 0)
                {
                    var topic = await _db.Topics.AsNoTracking().Where(x => x.TopicId.Equals(model.TopicId)).FirstOrDefaultAsync();
                    topic.TopicName = model.TopicName;
                    topic.ExpectedTime = model.ExpectedTime;
                    topic.ModuleId = model.ModuleId;

                    _db.Entry(topic).State = EntityState.Modified;
                    await _db.SaveChangesAsync();
                    return new JsonResult { Data = new { status = true, message = $"{model.TopicName} is updated successfully", id = model.ModuleId } };
                }
                _db.Topics.Add(model);
                await _db.SaveChangesAsync();
                var message = $"{model.TopicName} Added Successfully";
                return new JsonResult { Data = new { status = true, message, id = model.ModuleId } };
            }
            return new JsonResult { Data = new { status = false, message = "Model Not Correct" } };
        }

        public async Task<PartialViewResult> DetailTopic(int id)
        {
            var topics = await _db.Modules.Include(i => i.Topics)
                            .AsNoTracking().Where(x => x.ModuleId.Equals(id)).FirstOrDefaultAsync();
            return PartialView(topics);
        }

        public async Task<PartialViewResult> StudentDetailTopic(int id)
        {
            var topics = await _db.Topics.Include(i => i.TopicMaterials).Include(i => i.Module)
                            .AsNoTracking().Where(x => x.Module.ModuleId.Equals(id)).ToListAsync();
            if (!topics.Any())
            {
                ViewBag.Message = "Module Content is not Available at the moment, Please try again";
            }
            return PartialView(topics);
        }


        public async Task<PartialViewResult> SaveTopicContent(int id)
        {
            var topic = await _db.Topics.AsNoTracking().Where(x => x.TopicId.Equals(id)).ToListAsync();
            ViewBag.TopicId = new SelectList(topic, "TopicId", "TopicName");
            return PartialView();
        }

        // POST: Modules/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SaveTopicContent(TopicMaterial model)
        {
            if (ModelState.IsValid)
            {
                string _FileName = String.Empty;
                if (model.File.ContentLength > 0)
                {
                    _FileName = Path.GetFileName(model.File.FileName);
                    string _path = HostingEnvironment.MapPath("~/MaterialUpload/") + _FileName;
                    model.FileLocation = _path;
                    var directory = new DirectoryInfo(HostingEnvironment.MapPath("~/MaterialUpload/"));
                    if (directory.Exists == false)
                    {
                        directory.Create();
                    }
                    model.File.SaveAs(_path);
                }
                model.FileLocation = _FileName;
                if (model.TopicMaterialId > 0)
                {
                    var topicMaterial = await _db.TopicMaterials.AsNoTracking().Where(x => x.TopicId.Equals(model.TopicMaterialId)).FirstOrDefaultAsync();
                    topicMaterial.Author = model.Author;
                    topicMaterial.Description = model.Description;
                    topicMaterial.Name = model.Name;
                    topicMaterial.NoteBody = model.NoteBody;
                    topicMaterial.FileLocation = model.FileLocation;
                    topicMaterial.TopicId = model.TopicId;
                    _db.Entry(topicMaterial).State = EntityState.Modified;
                    await _db.SaveChangesAsync();
                    return new JsonResult { Data = new { status = true, message = $"{model.Name} is updated successfully", id = model.TopicId } };
                }
                _db.TopicMaterials.Add(model);
                await _db.SaveChangesAsync();
                var message = $"{model.Name} Added Successfully";
                //return new JsonResult { Data = new { status = true, message = message, id = model.TopicId } };
                return RedirectToAction("Index", new { message });
            }
            return new JsonResult { Data = new { status = false, message = "Model Not Correct" } };
        }

        public async Task<PartialViewResult> ViewTopic(int id)
        {
            var topic = await _db.Topics.Include(i => i.TopicMaterials).Include(i => i.Module).AsNoTracking()
                                .Where(x => x.TopicId.Equals(id)).FirstOrDefaultAsync();
            return PartialView(topic);
        }

        public async Task<PartialViewResult> DetailTopicContent(int id)
        {
            var content = await _db.TopicMaterials.Include(i => i.Topic).Include(i => i.Topic.Module).AsNoTracking()
                            .Where(x => x.TopicMaterialId.Equals(id)).FirstOrDefaultAsync();
            return PartialView(content);
        }

        //public ActionResult AssignmentTab()
        //{
        //    return View();
        //}


    }
}