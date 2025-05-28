using SwiftKampus.Models;
using SwiftKampus.ViewModels;
using SwiftKampusModel.TimeTable;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SwiftKampus.Controllers
{
    public class ClassRoomAllocationsController : BaseController
    {
        public ClassRoomAllocationsController(SchoolDbContext db) : base(db)
        {

        }

        // GET: ClassRoomAllocations
        public async Task<ActionResult> Index()
        {
            var classRoomAllocations = _db.ClassRoomAllocations.Include(c => c.Course).Include(c => c.LectureRoom);
            return View(await classRoomAllocations.ToListAsync());
        }

        // GET: ClassRoomAllocations/StudentTimeTable
        public ActionResult StudentTimeTable()
        {
            return View();
        }

        public ActionResult SetSchoolProgramme()
        {
            ViewBag.SchoolProgrammeId = new SelectList(_db.SchoolProgrammes, "SchoolProgrammeId", "FancyName");
            return View();
        }

        // GET: ClassRoomAllocations/StudentTimeTableDetail
        public async Task<JsonResult> StudentTimeTableDetail()
        {
            if (User.IsInRole(RoleName.Student))
            {
                // get time table period based on current Semester and Session
                var timeTablePeriodId = await _db.TimeTablePeriods.AsNoTracking()
                                        .Where(ttp => ttp.SemesterId.Equals(semesterId)
                                        && ttp.SessionId.Equals(sessionId)
                                        && ttp.SchoolProgrammeId.Equals(studentSchoolProgrammeId))
                                        .Select(s => s.TimeTablePeriodId).FirstOrDefaultAsync();

                // get student course registration
                var studentId = _studentQuery.GetStudentId(userId);
                var registeredCourseIds = await _db.CourseRegistrations.AsNoTracking().Include(i => i.Course)
                                        .Where(x => x.StudentId.Equals(studentId)
                                        && x.SessionId.Equals(sessionId) && x.SemesterId.Equals(semesterId))
                                        .Select(s => s.Course.CourseId).ToListAsync();

                // get allocation based on course registration and time table period
                var allocations = new List<ClassRoomAllocation>();
                foreach (var registeredCourseId in registeredCourseIds)
                {
                    var myAllocation = _db.ClassRoomAllocations
                                    .Include(i => i.LectureRoom).Include(i => i.Course).AsNoTracking()
                                    .Where(x => x.TimeTablePeriodId.Equals(timeTablePeriodId) &&
                                    x.CourseId.Equals(registeredCourseId)).FirstOrDefault();
                    if (myAllocation != null)
                    {
                        allocations.Add(myAllocation);
                    }

                }

                var myModel = allocations.Select(s => new ClassroomAllocationVm()
                {
                    TimeTableAllocationId = s.ClassRoomAllocationId,
                    TimeTableId = s.TimeTablePeriodId,
                    CourseCode = s.Course.CourseCode,
                    EndDate = s.EndTime.ToString(),
                    StartDate = s.StartTime.ToString(),
                    LectureRoomCode = s.LectureRoom.LectureRoomCode
                });
                return Json(myModel, JsonRequestBehavior.AllowGet);
            }

            var errorList = new List<string>()
            {

            };

            return Json(errorList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult TimeTableList(int id)
        {
            var allocations = _db.ClassRoomAllocations.Include(i => i.LectureRoom).Include(i => i.Course)
                                .AsNoTracking().Where(x => x.TimeTablePeriodId.Equals(id))
                                .ToList();

            var myModel = allocations.Select(s => new ClassroomAllocationVm()
            {
                TimeTableAllocationId = s.ClassRoomAllocationId,
                TimeTableId = s.TimeTablePeriodId,
                CourseCode = s.Course.CourseCode,
                EndDate = s.EndTime.ToString(),
                StartDate = s.StartTime.ToString(),
                LectureRoomCode = s.LectureRoom.LectureRoomCode
            });
            return Json(myModel, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> SetTimeTable(SetSchoolProgrammeVm setSchoolProgramme)
        {
            var model = new SetTimeTableVm();
            if (User.IsInRole(RoleName.Hod) || User.IsInRole(RoleName.Academic))
            {
                var staff = await _db.Staffs.Include(i => i.Department).AsNoTracking().
                                 Where(x => x.Email.Equals(userId))
                                 .Select(x => new
                                 {
                                     x.Department.DepartmentId,
                                     x.Department.FacultyId
                                 }).FirstOrDefaultAsync();
                var deptCourses = await _db.Courses.Include(i => i.Programme.Department).AsNoTracking()
                                    .Where(x => x.Programme.Department.DepartmentId.Equals(staff.DepartmentId))
                                    .ToListAsync();

                var buildingIds = await _db.AssignFacultyBuildings.AsNoTracking()
                                        .Where(x => x.FacultyId.Equals(staff.FacultyId))
                                        .Select(s => s.BuildingId).ToListAsync();


                model.SelectableCourses = deptCourses;
                var lecturerooms = new List<LectureRoom>();
                foreach (var building in buildingIds)
                {
                    lecturerooms.AddRange(_db.LectureRooms.AsNoTracking()
                                                            .Where(x => x.BuildingId.Equals(building))
                                                            .ToList());
                }
                model.SelectableLectureRooms = lecturerooms;
            }
            else
            {
                model.SelectableCourses = _db.Courses.AsNoTracking().ToList();
                model.SelectableLectureRooms = _db.LectureRooms.AsNoTracking().ToList();
            }
            sessionId = _query.GetCurrentSessionId(setSchoolProgramme.SchoolProgrammeId);
            semesterId = _query.GetCurrentSemesterId(setSchoolProgramme.SchoolProgrammeId);
            var timeTablePeriodId = await _db.TimeTablePeriods.AsNoTracking()
                                       .Where(ttp => ttp.SemesterId.Equals(semesterId)
                                       && ttp.SessionId.Equals(sessionId)
                                       && ttp.SchoolProgrammeId.Equals(setSchoolProgramme.SchoolProgrammeId))
                                       .Select(s => s.TimeTablePeriodId).FirstOrDefaultAsync();

            model.TimeTableId = timeTablePeriodId;



            return View(model);
        }

        // POST: ClassRoomAllocations/SetAllocation
        [HttpPost]
        public JsonResult SetAllocation(ClassRoomAllocation allocation)
        {
            if (ModelState.IsValid)
            {
                if (allocation.ClassRoomAllocationId == 0)
                {
                    _db.ClassRoomAllocations.Add(allocation);
                }
                else
                {
                    ClassRoomAllocation allocationInDB = _db.ClassRoomAllocations.Single(cra => cra.ClassRoomAllocationId.Equals(allocation.ClassRoomAllocationId));
                    allocationInDB.CourseId = allocation.CourseId;
                    allocationInDB.LectureRoomId = allocation.LectureRoomId;
                }
                _db.SaveChanges();
            }

            var allocationRecord = _db.ClassRoomAllocations
                                        .Include(i => i.Course)
                                        .Include(i => i.LectureRoom).AsNoTracking().
                                        Where(x => x.ClassRoomAllocationId.Equals(allocation.ClassRoomAllocationId))
                                        .Select(s => new
                                        {
                                            s.ClassRoomAllocationId,
                                            s.Course.CourseCode,
                                            s.LectureRoom.LectureRoomCode,
                                            StartTime = s.StartTime.ToString(),
                                            EndTime = s.EndTime.ToString(),
                                            s.LectureRoom.SittingCapacity
                                        }).Single();

            return Json(allocationRecord, JsonRequestBehavior.AllowGet);
        }

        // ClassRoomAllocation/AllocationDetail/5
        public JsonResult AllocationDetail(int id)
        {
            var allocation = _db.ClassRoomAllocations
                                .Include(cra => cra.Course)
                                .Include(cra => cra.LectureRoom).AsNoTracking()
                                .Where(cra => cra.ClassRoomAllocationId.Equals(id))
                                .Select(cra => new
                                {
                                    cra.ClassRoomAllocationId,
                                    cra.Course.CourseCode,
                                    cra.Course.CourseId,
                                    cra.LectureRoom.LectureRoomCode,
                                    cra.LectureRoom.LectureRoomId,
                                    cra.StartTime,
                                    cra.EndTime
                                }).FirstOrDefault();

            return Json(allocation, JsonRequestBehavior.AllowGet);
        }

        // ClassRoomAllocation/AllocationDelete/5
        public JsonResult AllocationDelete(int id)
        {
            ClassRoomAllocation allocation = _db.ClassRoomAllocations.Single(cra => cra.ClassRoomAllocationId.Equals(id));
            _db.ClassRoomAllocations.Remove(allocation);
            _db.SaveChanges();

            return Json(allocation, JsonRequestBehavior.AllowGet);
        }

        // POST: ClassRoomAllocation/ExtendAllocatedTime/5
        [HttpPost]
        public JsonResult ExtendAllocatedTime(int classRoomAllocationId, string endTime, string startTime)
        {
            if (ModelState.IsValid)
            {
                ClassRoomAllocation allocation = _db.ClassRoomAllocations.Single(cra => cra.ClassRoomAllocationId.Equals(classRoomAllocationId));
                allocation.EndTime = endTime;
                allocation.StartTime = startTime;
                _db.SaveChanges();
            }

            var allocationRecord = _db.ClassRoomAllocations
                                        .Include(i => i.Course)
                                        .Include(i => i.LectureRoom).AsNoTracking().
                                        Where(x => x.ClassRoomAllocationId.Equals(classRoomAllocationId))
                                        .Select(s => new
                                        {
                                            s.ClassRoomAllocationId,
                                            s.Course.CourseCode,
                                            s.LectureRoom.LectureRoomCode,
                                            StartTime = s.StartTime.ToString(),
                                            EndTime = s.EndTime.ToString(),
                                            s.LectureRoom.SittingCapacity
                                        }).Single();

            return Json(allocationRecord, JsonRequestBehavior.AllowGet);
        }

        // GET: ClassRoomAllocations/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ClassRoomAllocation classRoomAllocation = await _db.ClassRoomAllocations.FindAsync(id);
            if (classRoomAllocation == null)
            {
                return HttpNotFound();
            }
            return View(classRoomAllocation);
        }

        // GET: ClassRoomAllocations/Create
        public async Task<ActionResult> Create()
        {
            var staff = await _db.Staffs.Include(i => i.Department).AsNoTracking().
                                Where(x => x.Email.Equals(userId))
                                .Select(x => new
                                {
                                    x.Department.DepartmentId,
                                    x.Department.FacultyId
                                }).FirstOrDefaultAsync();
            var deptCourses = await _db.Courses.Include(i => i.Programme.Department).AsNoTracking().
                                Where(x => x.Programme.Department.DepartmentId.Equals(staff.DepartmentId))
                                .ToListAsync();

            var buildingId = await _db.AssignFacultyBuildings.AsNoTracking().
                                    Where(x => x.FacultyId.Equals(staff.FacultyId))
                                    .Select(s => s.BuildingId).ToListAsync();

            ViewBag.CourseId = new SelectList(deptCourses, "CourseId", "CourseCode");
            ViewBag.LectureRoomId = new SelectList(_db.LectureRooms.AsNoTracking()
                                    .Where(x => x.BuildingId.Equals(buildingId)).ToList(), "LectureRoomId", "LectureRoomName");
            return View();
        }

        public async Task<List<LectureRoom>> GetClassPopulation(int courseId, bool isGeneralCourse)
        {

            var staffFacultyId = await _db.Staffs.Include(i => i.Department).AsNoTracking().
                                Where(x => x.Email.Equals(userId))
                                .Select(s => s.Department.FacultyId)
                                .FirstOrDefaultAsync();


            var buildingIds = await _db.AssignFacultyBuildings.AsNoTracking().
                                    Where(x => x.FacultyId.Equals(staffFacultyId))
                                    .Select(s => s.BuildingId).ToListAsync();
            var course = await _db.Courses.Include(i => i.Programme).Include(i => i.Level).AsNoTracking()
                            .Where(x => x.CourseId.Equals(courseId)).
                               Select(s => new
                               {
                                   s.Programme.ProgrammeId,
                                   s.Level.LevelId
                               }).FirstOrDefaultAsync();
            var studentCounts = await _db.Students.Include(i => i.Programme).Include(i => i.Level).AsNoTracking().
                            Where(x => x.Programme.ProgrammeId.Equals(course.ProgrammeId)
                            && x.Level.LevelId.Equals(course.LevelId)).CountAsync();

            var lectureRooms = new List<LectureRoom>();
            foreach (var building in buildingIds)
            {
                if (isGeneralCourse == false)
                {
                    lectureRooms.AddRange(_db.LectureRooms.AsNoTracking()
                                       .Where(x => x.BuildingId.Equals(building)
                                           && x.SittingCapacity >= studentCounts).ToList());
                }
                else if (isGeneralCourse == true)
                {
                    lectureRooms.AddRange(_db.LectureRooms.Where(x => x.BuildingId.Equals(building) &&
                                       x.IsGeneralLectureHall.Equals(true)));
                    lectureRooms.AddRange(_db.LectureRooms.Include(i => i.Building)
                                            .Where(x => x.Building.IsMultiPurpose.Equals(true)));
                }

            }

            return lectureRooms;
        }

        // POST: ClassRoomAllocations/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "ClassRoomAllocationId,TimeTablePeriodId,CourseId,LectureRoomId,StartTime,EndTime,IsApproved")] ClassRoomAllocation classRoomAllocation)
        {
            if (ModelState.IsValid)
            {
                _db.ClassRoomAllocations.Add(classRoomAllocation);
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.CourseId = new SelectList(_db.Courses, "CourseId", "CourseCode", classRoomAllocation.CourseId);
            ViewBag.LectureRoomId = new SelectList(_db.LectureRooms, "LectureRoomId", "LectureRoomName", classRoomAllocation.LectureRoomId);
            return View(classRoomAllocation);
        }

        // GET: ClassRoomAllocations/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ClassRoomAllocation classRoomAllocation = await _db.ClassRoomAllocations.FindAsync(id);
            if (classRoomAllocation == null)
            {
                return HttpNotFound();
            }
            ViewBag.CourseId = new SelectList(_db.Courses, "CourseId", "CourseCode", classRoomAllocation.CourseId);
            ViewBag.LectureRoomId = new SelectList(_db.LectureRooms, "LectureRoomId", "LectureRoomName", classRoomAllocation.LectureRoomId);
            return View(classRoomAllocation);
        }

        // POST: ClassRoomAllocations/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "ClassRoomAllocationId,TimeTablePeriodId,CourseId,LectureRoomId,StartTime,EndTime,IsApproved")] ClassRoomAllocation classRoomAllocation)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(classRoomAllocation).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.CourseId = new SelectList(_db.Courses, "CourseId", "CourseCode", classRoomAllocation.CourseId);
            ViewBag.LectureRoomId = new SelectList(_db.LectureRooms, "LectureRoomId", "LectureRoomName", classRoomAllocation.LectureRoomId);
            return View(classRoomAllocation);
        }

        // GET: ClassRoomAllocations/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ClassRoomAllocation classRoomAllocation = await _db.ClassRoomAllocations.FindAsync(id);
            if (classRoomAllocation == null)
            {
                return HttpNotFound();
            }
            return View(classRoomAllocation);
        }

        // POST: ClassRoomAllocations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            ClassRoomAllocation classRoomAllocation = await _db.ClassRoomAllocations.FindAsync(id);
            _db.ClassRoomAllocations.Remove(classRoomAllocation);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
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
