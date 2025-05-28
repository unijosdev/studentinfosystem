using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using SwiftKampus.Models;
using System.Threading.Tasks;
using System.Web.Mvc;
using Vereyon.Web;
using SwiftKampus.ViewModels.MisconductVm;
using SwiftKampusModel.Misconduct;
using System.Web;
using SwiftKampus.Services;

namespace SwiftKampus.Controllers
{
    [Authorize]
    [Audit(AuditingLevel = 2)]
    public class StudentDisciplinaryController : BaseController
    {
        public StudentDisciplinaryController(SchoolDbContext db) : base(db)
        {

        }
        // GET: StudentDisciplinary
        public ActionResult Index()
        {
            return View();
        }

        // GET: StudentDisciplinary/FindStudent/(UJ/2012/NS/0005)
        public async Task<ActionResult> FindStudent(string MatricNumber)
        {
            var student = await _db.Students.Where(st => st.MatricNo.Trim().ToUpper().Equals(MatricNumber.Trim().ToUpper()))
                                .FirstOrDefaultAsync();
            if (student == null)
            {
                FlashMessage.Danger("Student not found! Check the Matricultion Number to make sure that it is correct.");
                return RedirectToAction("Index");
            }

            var defaulterMisconductList = await _db.Defaulters
                                             .Include(dt => dt.Misconduct)
                                             .Include(dt => dt.Student)
                                             .Include(dt => dt.Student.Programme.Department.Faculty)
                                             .Where(dt => dt.Student.MatricNo.Trim().ToUpper().Equals(MatricNumber.Trim().ToUpper()))
                                             .Select(dt => new DefaulterMisconductViewModel
                                             {
                                                 DefaulterId = dt.DefaulterId,
                                                 MisconductName = dt.Misconduct.MisconductName,
                                                 IncidentDate = dt.DefaultDate,
                                                 Status = dt.CaseTreated,
                                                 FacultyName = dt.Student.Programme.Department.Faculty.FacultyName
                                             }).ToListAsync();

            //StudentDetailViewModel studentDetail = _db.Defaulters
            //                 .Include(st => st.Student.Programme.Department.Faculty)
            //                 .Include(st => st.Student.Programme.Department)
            //                 .Include(st => st.Student.Programme)
            //                 .Include(st => st.Student.Level)
            //                 .Include(st => st.Student)
            //                 .Where(st => st.Student.MatricNo.Trim().ToUpper().Equals(MatricNumber.Trim().ToUpper())).AsEnumerable()
            //                 .Select(st => new StudentDetailViewModel
            //                 {
            //                     StudentId = st.StudentId,
            //                     MatriculationNumber = st.Student.MatricNo,
            //                     FullName = $"{st.Student.LastName} {st.Student.FirstName} {st.Student.MiddleName}",
            //                     FacultyName = st.Student.Programme.Department.Faculty.FacultyName,
            //                     DepartmentName = st.Student.Programme.Department.DeptName,
            //                     DepartmentOptionName = st.Student.Programme.ProgrammeName,
            //                     LevelName = st.Student.Level.LevelName,
            //                     StudentDisciplinaryStatus = st.StudentDisciplinaryStatus,
            //                     Session = GetCurrentSession,
            //                     PhoneNumber = st.Student.PhoneNumber,
            //                     DefaulterMisconductViewModel = defaulterMisconductList
            //                 }).FirstOrDefault();

            StudentDetailViewModel studentDetail = _db.Students
                                                    .Include(st => st.Programme.Department.Faculty)
                                                    .Include(st => st.Programme.Department)
                                                    .Include(st => st.Programme)
                                                    .Include(st => st.Level)
                                                    .Include(st => st.Defaulters)
                                                    .Where(st => st.MatricNo.Trim().ToUpper().Equals(MatricNumber.Trim().ToUpper())).AsEnumerable()
                                                    .Select(st => new StudentDetailViewModel
                                                    {
                                                        StudentId = st.StudentId,
                                                        MatriculationNumber = st.MatricNo,
                                                        FullName = $"{st.LastName} {st.FirstName} {st.MiddleName}",
                                                        FacultyName = st.Programme.Department.Faculty.FacultyName,
                                                        DepartmentName = st.Programme.Department.DeptName,
                                                        DepartmentOptionName = st.Programme.ProgrammeName,
                                                        LevelName = st.Level.LevelName,
                                                        Defaulters = st.Defaulters,
                                                        //StudentDisciplinaryStatus = st.StudentDisciplinaryStatus,
                                                        //Session = GetCurrentSession,
                                                        PhoneNumber = st.PhoneNumber,
                                                        DefaulterMisconductViewModel = defaulterMisconductList
                                                    }).FirstOrDefault();


            return View(studentDetail);
        }

        // GET: StudentDisciplinary/Details/5
        public async Task<ActionResult> Details(int id)
        {

            var studentDetail = await _db.Defaulters
                             .Include(st => st.Student)
                             .Include(st => st.Student.Programme.Department.Faculty)
                             .Include(st => st.Student.Programme.Department)
                             .Include(st => st.Student.Programme)
                             .Include(st => st.Student.Level)
                             .Include(st => st.Student.StudentStatus)
                             .Where(st => st.DefaulterId == id)
                             .Select(st => new ShowStudentDetailViewModel
                             {
                                 StudentId = st.Student.StudentId,
                                 MatriculationNumber = st.Student.MatricNo,
                                 LastName = st.Student.LastName,
                                 FirstName = st.Student.FirstName,
                                 MiddleName = st.Student.MiddleName,
                                 FacultyName = st.Student.Programme.Department.Faculty.FacultyName,
                                 DepartmentName = st.Student.Programme.Department.DeptName,
                                 DepartmentOptionName = st.Student.Programme.ProgrammeName,
                                 LevelName = st.Student.Level.LevelName,
                                 StudentDisciplinaryStatus = st.StudentDisciplinaryStatus,
                                 //Session =  GetCurrentSession
                                 MisconductName = st.Misconduct.MisconductName,
                                 IncidentDate = st.DefaultDate.ToString(),
                                 Status = st.CaseTreated,
                                 EvidenceAdress = st.EvidenceAdress, /* Server.MapPath("~/Content/Evidence/" + "_" + evidencePath)*/

                             }).SingleOrDefaultAsync();
            return View(studentDetail);
        }

        // GET: StudentDisciplinary/Create
        public async Task<ActionResult> Create(string id)
        {
            var student = await _db.Students.Where(st => st.StudentId.Equals(id)).FirstOrDefaultAsync();
            var misconducts = await _db.Misconducts.ToListAsync();

            var defaulter = new CreateDefaulterViewModel
            {
                StudentId = student.StudentId,
                FullName = student.FullName,
                MatriculationNumber = student.MatricNo,
                Misconducts = misconducts,
                Sessions = _db.Sessions.AsNoTracking().ToList(),
                Semesters = _db.Semesters.AsNoTracking().ToList()
            };
            return View(defaulter);
        }


        // POST: StudentDisciplinary/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateDefaulterViewModel formRecord)
        {
            var now = DateTime.UtcNow;
            var date = new DateTime(now.Year, now.Month, now.Day, now.Hour + 1, now.Minute, now.Second); //Added +1 to hours to compensate for UTC
            //var matricNumber = _db.Students.Where(st => st.Id == formRecord.StudentId).Select(st => st.MatricNumber).First();
            var defaulter = new Defaulter
            {
                // TODO:create the defaulter
                DefaultDate = date,
                StudentId = formRecord.StudentId,
                SessionId = formRecord.SessionId,
                SemesterId = formRecord.SemesterId,
                MisconductId = formRecord.MisconductId
            };
            if (formRecord.EvidenceFile != null)
            {
                if (formRecord.EvidenceFile.ContentLength > 300000)
                {
                    FlashMessage.Warning("File size should be less than or equal to 25kB");
                    return RedirectToAction(formRecord.StudentId, "StudentDisciplinary/Create");
                }

                //check file type
                if (!(formRecord.EvidenceFile.FileName.EndsWith("jpg", StringComparison.CurrentCulture)
                    || formRecord.EvidenceFile.FileName.EndsWith("jpeg", StringComparison.CurrentCulture)
                    || formRecord.EvidenceFile.FileName.EndsWith("png", StringComparison.CurrentCulture)
                    || formRecord.EvidenceFile.FileName.EndsWith("pdf", StringComparison.CurrentCulture)))
                {
                    FlashMessage.Warning("File type must be either jpg, jpeg, png or pdf");
                    return RedirectToAction(formRecord.StudentId, "StudentDisciplinary/Create");
                }
                SaveEvidenceFile(formRecord.EvidenceFile, formRecord.StudentId, formRecord.EvidenceFile.FileName);
                defaulter.EvidenceAdress = $"{formRecord.StudentId}{formRecord?.EvidenceFile?.FileName}";
            }
            //defaulter.StudentDisciplinaryStatusId = null;

            _db.Defaulters.Add(defaulter);
            _db.SaveChanges();


            FlashMessage.Confirmation("Misconduct Created Successfully");
            return RedirectToAction("Index");

        }

        void SaveEvidenceFile(HttpPostedFileBase formRecord, string StudentId, string fileName)
        {
            string path = Server.MapPath("~/Content/Evidence/" + "_" + StudentId + "_" + fileName);
            formRecord.SaveAs(path);
        }

        // GET: StudentDisciplinary/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            var misconducts = await _db.Misconducts.ToListAsync();

            var defaulter = _db.Defaulters
                               .Include(df => df.Student)
                               .Include(df => df.Misconduct)
                               .Where(df => df.DefaulterId.Equals(id)).FirstOrDefault();
                              
            var model = new EditDefaulterViewModel()
            {
                StudentId = defaulter.StudentId,
                FullName = defaulter.Student.FullName,
                MatriculationNumber = defaulter.Student.MatricNo,
                Misconducts = misconducts,
                MisconductName = defaulter.Misconduct.MisconductName,
                MisconductId = defaulter.Misconduct.MisconductId,
                Sessions = _db.Sessions.AsNoTracking().ToList(),
                Semesters = _db.Semesters.AsNoTracking().ToList(),
                DefaulterId = defaulter.DefaulterId
            };
            return View(model);
        }

        // POST: StudentDisciplinary/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, EditDefaulterViewModel formRecord)
        {
            var matricNumber = _db.Students.Where(st => st.StudentId.Equals(formRecord.StudentId)).Select(st => st.MatricNo).First();
            Defaulter defaulter = _db.Defaulters.Find(id);

           // defaulter.DefaultDate = formRecord.Da;
            defaulter.StudentId = formRecord.StudentId;
            defaulter.SessionId = formRecord.SessionId;
            defaulter.SemesterId = formRecord.SemesterId;
            defaulter.MisconductId = formRecord.MisconductId;
            if (formRecord.EvidenceFile != null)
            {
                if (formRecord.EvidenceFile.ContentLength > 300000)
                {
                    FlashMessage.Warning("File size should be less than or equal to 25kB");
                    return RedirectToAction(formRecord.StudentId, "StudentDisciplinary/Create");
                }

                //check file type
                if (!(formRecord.EvidenceFile.FileName.EndsWith("jpg", StringComparison.CurrentCulture)
                    || formRecord.EvidenceFile.FileName.EndsWith("jpeg", StringComparison.CurrentCulture)
                    || formRecord.EvidenceFile.FileName.EndsWith("png", StringComparison.CurrentCulture)
                    || formRecord.EvidenceFile.FileName.EndsWith("pdf", StringComparison.CurrentCulture)))
                {
                    FlashMessage.Warning("File type must be either jpg, jpeg, png or pdf");
                    return RedirectToAction(formRecord.StudentId, "StudentDisciplinary/Create");
                }
                SaveEvidenceFile(formRecord.EvidenceFile, formRecord.StudentId, formRecord.EvidenceFile.FileName);
                defaulter.EvidenceAdress = $"{formRecord.StudentId}{formRecord?.EvidenceFile?.FileName}";
            }


            _db.Entry(defaulter).State = EntityState.Modified;
            _db.SaveChanges();

            FlashMessage.Confirmation("Misconduct Edited Successfully");
            return RedirectToAction("Index");
        }

        // POST: StudentDisciplinary/JSONEditRemoveEvidenceFile/
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult JSONEditRemoveEvidenceFile(int defaulterId, string studentId)
        {
            var evidenceAdress = _db.Defaulters.Where(df => (df.DefaulterId == defaulterId && df.StudentId.Equals(studentId))).Select(df => df.EvidenceAdress).First();

            var path = Server.MapPath("~/Content/Evidence/" + evidenceAdress);

            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);

                var defaulter = _db.Defaulters.Where(df => (df.DefaulterId == defaulterId && df.StudentId.Equals(studentId))).FirstOrDefault();
                defaulter.EvidenceAdress = null;

                _db.Entry(defaulter).State = EntityState.Modified;
                _db.SaveChanges();
                return Json("success");
            }
            return Json("null");

        }

        // GET: StudentDisciplinary/Delete/5
        public async Task<ActionResult> Delete(int id)
        {

            ShowStudentDetailViewModel studentDetail = await _db.Defaulters
                             .Include(st => st.Student)
                             .Include(st => st.Student.Programme.Department.Faculty)
                             .Include(st => st.Student.Programme.Department)
                             .Include(st => st.Student.Programme)
                             .Include(st => st.Student.Level)
                             .Include(st => st.Student.StudentStatus)
                             .Where(st => st.DefaulterId == id)
                             .Select(st => new ShowStudentDetailViewModel
                             {
                                 StudentId = st.Student.StudentId,
                                 DefaulterId = id,
                                 MatriculationNumber = st.Student.MatricNo,
                                 LastName = st.Student.LastName,
                                 FirstName = st.Student.FirstName,
                                 MiddleName = st.Student.MiddleName,
                                 FacultyName = st.Student.Programme.Department.Faculty.FacultyName,
                                 DepartmentName = st.Student.Programme.Department.DeptName,
                                 DepartmentOptionName = st.Student.Programme.ProgrammeName,
                                 LevelName = st.Student.Level.LevelName,
                                 StudentDisciplinaryStatus = st.StudentDisciplinaryStatus,
                                 //Session =  GetCurrentSession
                                 MisconductName = st.Misconduct.MisconductName,
                                 IncidentDate = st.DefaultDate.ToString(),
                                 Status = st.CaseTreated,
                                 EvidenceAdress = st.EvidenceAdress, /* Server.MapPath("~/Content/Evidence/" + "_" + evidencePath)*/

                             }).SingleOrDefaultAsync();

            return View(studentDetail);
        }

        // POST: StudentDisciplinary/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, ShowStudentDetailViewModel collection)
        {

            // TODO: Add delete logic here
            Defaulter defaulter = _db.Defaulters.Find(id);
            var evidenceFile = defaulter.EvidenceAdress;
            _db.Defaulters.Remove(defaulter);
            _db.SaveChanges();

            //Delete default file
            var path = Server.MapPath("~/Content/Evidence/" + evidenceFile);
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }

            FlashMessage.Confirmation("Record Deleted Successfully!");
            return RedirectToAction("Index");
        }

        // GET: StudentDisciplinary/RecallRequest

    }
}
