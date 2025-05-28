using SwiftKampus.BusinessLogic;
using SwiftKampus.Controllers;
using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampus.ViewModels.MisconductVm;
using SwiftKampusModel.Misconduct;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

using Vereyon.Web;

namespace Unijos.Web.Controllers
{
    [Authorize]
    [Audit(AuditingLevel = 2)]
    public class StudentDisciplinController : BaseController
    {
        public StudentDisciplinController(SchoolDbContext db) : base(db)
        {
        }

        // GET: StudentDisciplin
        public ActionResult Index()
        {
            var defaulters = _db.Defaulters
                                      .Include(df => df.Student)
                                      .Include(df => df.StudentDisciplinaryStatus)
                                      .Include(df => df.Misconduct).AsEnumerable().AsEnumerable()
                                      //.Where(df => df.CaseTreated == false)
                                      .Select(df => new DefaultersViewModel
                                      {
                                          MatriculationNumber = df.Student.MatricNo,
                                          FullName = $"{df.Student.LastName} {df.Student.FirstName} {df.Student.MiddleName}",
                                          MisconductName = df.Misconduct.MisconductName,
                                          StudentId = df.StudentId,
                                          DefaulterId = df.DefaulterId,
                                          CaseTreated = df.CaseTreated,
                                          IsNotified = df.IsNotified
                                      }).ToList();

            return View(defaulters);
        }

        // GET: StudentDisciplin/Details/5
        // [Route("StudentDisciplin/Details/{id}")]
        public async Task<ActionResult> Details(int id)
        {

            var studentDetail = await _db.Defaulters
                             .Include(st => st.Student)
                             .Include(st => st.Student.Programme.Department.Faculty)
                             .Include(st => st.Student.Programme.Department)
                             .Include(st => st.Student.Programme)
                             .Include(st => st.Student.Level)
                             //.Include(st => st.Student.StudentDisciplinaryStatus)
                             .Where(st => st.DefaulterId == id)
                             .Select(st => new ShowStudentDetailViewModel
                             {
                                 DefaulterId = st.DefaulterId,
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

        // GET: StudentDisciplin/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: StudentDisciplin/Create
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> JASONCreateMisconduct(ShowStudentDetailViewModel showStudentDetailViewModel)
        //{

        //    // TODO: Add insert logic here
        //    var now = DateTime.UtcNow;
        //    var date = new DateTime(now.Year, now.Month, now.Day, now.Hour + 1, now.Minute, now.Second); //Added +1 to hours to compensate for UTC

        //    var student = _db.Defaulters.Include(i => i.Student)
        //                        .AsNoTracking().Where(x => x.Student.StudentId.Equals(showStudentDetailViewModel.StudentId))
        //                        .Select(s => s.Student).FirstOrDefault();

        //    var currentSessionId = _query.GetCurrentSessionId(student.SchoolProgrammeId);
        //    var ActiveSession = _db.Sessions.Where(ss => ss.SessionId.Equals(currentSessionId)).Select(ss => ss.EndDate).FirstOrDefault();
        //    var defaulterId = showStudentDetailViewModel.DefaulterId;
        //    var studentDisciplinary = await _db.StudentDisciplinaryStatus.Where(st => st.Defaulter.StudentDisciplinaryStatus.DefaulterId.Equals(showStudentDetailViewModel.DefaulterId)).FirstOrDefaultAsync();

        //    //check if there is NOT an existing record, create new
        //    if (studentDisciplinary == null)
        //    {
        //        StudentDisciplinaryStatus newStudentDisciplinary = new StudentDisciplinaryStatus
        //        {
        //            DisciplineName = showStudentDetailViewModel.Punishment.ToString(),
        //            DefaulterId = showStudentDetailViewModel.DefaulterId,
        //            StillValid = true,
        //            DateSetActive = date,

        //        };

        //        //update fields as appropriate to the disciplinary action taken
        //        switch ((Int32)showStudentDetailViewModel.Punishment)
        //        {
        //            case 3:
        //                newStudentDisciplinary.recallSession = ActiveSession.Year + 1;
        //                break;
        //            case 4:
        //                newStudentDisciplinary.recallSession = ActiveSession.Year + 2;
        //                break;
        //            case 5:
        //                newStudentDisciplinary.AmountToPay = showStudentDetailViewModel.PriceOfVadalisedProperty;
        //                break;
        //        }

        //        _db.StudentDisciplinaryStatus.Add(newStudentDisciplinary);
        //        _db.SaveChanges();

        //        //Update defauter record with status=caseTreated and disciplinaryStatusId
        //        var defaulter = _db.Defaulters.Where(dft => dft.DefaulterId == defaulterId).First();

        //        defaulter.StudentDisciplinaryStatusId = newStudentDisciplinary.DefaulterId;
        //        defaulter.CaseTreated = true; //set case as true
        //        _db.Entry(defaulter).State = EntityState.Modified;
        //        _db.SaveChanges();

        //        FlashMessage.Confirmation("Disciplinary Action Entered Successfully");
        //        return RedirectToAction("Index");
        //    }
        //    else
        //    {
        //        //update an existing record
        //        studentDisciplinary.DisciplineName = showStudentDetailViewModel.Punishment.ToString();
        //        studentDisciplinary.StillValid = true;
        //        studentDisciplinary.DateSetActive = date;
        //        studentDisciplinary.DefaulterId = showStudentDetailViewModel.DefaulterId;

        //        //update fields as appropriate to the disciplinary action taken
        //        switch ((Int32)showStudentDetailViewModel.Punishment)
        //        {
        //            case 3:
        //                studentDisciplinary.recallSession = ActiveSession.Year + 1;
        //                break;
        //            case 4:
        //                studentDisciplinary.recallSession = ActiveSession.Year + 2;
        //                break;
        //            case 5:
        //                studentDisciplinary.AmountToPay = showStudentDetailViewModel.PriceOfVadalisedProperty;
        //                break;
        //        }
        //        _db.Entry(studentDisciplinary).State = EntityState.Modified;
        //        //_db.SaveChanges();

        //        var defaulter = await _db.Defaulters.Where(dft => dft.DefaulterId == defaulterId).FirstAsync();

        //        defaulter.StudentDisciplinaryStatusId = studentDisciplinary.DefaulterId;
        //        defaulter.CaseTreated = true; //set case as true
        //        _db.Entry(defaulter).State = EntityState.Modified;
        //        _db.SaveChanges();

        //        FlashMessage.Confirmation("Disciplinary Action Updated Successfully");
        //        return RedirectToAction("Index");
        //    }


        //}


        // POST: StudentDisciplin/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(ShowStudentDetailViewModel showStudentDetailViewModel)
        {

            // TODO: Add insert logic here
            var now = DateTime.UtcNow;
            var date = new DateTime(now.Year, now.Month, now.Day, now.Hour + 1, now.Minute, now.Second); //Added +1 to hours to compensate for UTC

            var student = _db.Defaulters.Include(i => i.Student)
                                .AsNoTracking().Where(x => x.Student.StudentId.Equals(showStudentDetailViewModel.StudentId))
                                .Select(s => s.Student).FirstOrDefault();

            var currentSessionId = _query.GetCurrentSessionId(student.SchoolProgrammeId);
            var ActiveSession = _db.Sessions.Where(ss => ss.SessionId.Equals(currentSessionId)).Select(ss => ss.EndDate).FirstOrDefault();
            var defaulterId = showStudentDetailViewModel.DefaulterId;
            var studentDisciplinary = await _db.StudentDisciplinaryStatus.Where(st => st.Defaulter.StudentDisciplinaryStatus.DefaulterId.Equals(showStudentDetailViewModel.DefaulterId)).FirstOrDefaultAsync();

            //check if there is NOT an existing record, create new
            if (studentDisciplinary == null)
            {
                StudentDisciplinaryStatus newStudentDisciplinary = new StudentDisciplinaryStatus
                {
                    DisciplineName = showStudentDetailViewModel.Punishment.ToString(),
                    DefaulterId = showStudentDetailViewModel.DefaulterId,
                    StillValid = true,
                    DateSetActive = date,

                };

                //update fields as appropriate to the disciplinary action taken
                switch ((Int32)showStudentDetailViewModel.Punishment)
                {
                    case 3:
                        newStudentDisciplinary.recallSession = ActiveSession.Year + 1;
                        break;
                    case 4:
                        newStudentDisciplinary.recallSession = ActiveSession.Year + 2;
                        break;
                    case 5:
                        newStudentDisciplinary.AmountToPay = showStudentDetailViewModel.PriceOfVadalisedProperty;
                        break;
                }

                //update student detail to prevent academic activities
                var studentUpdate = _db.Students.Where(x => x.StudentId.Equals(showStudentDetailViewModel.StudentId)).FirstOrDefault();
                studentUpdate.MatricNo = "SUSP_" + studentUpdate.MatricNo;
                studentUpdate.Email = "SUSP_" + studentUpdate.Email;
                _db.Entry(studentUpdate).State = EntityState.Modified;

                _db.StudentDisciplinaryStatus.Add(newStudentDisciplinary);
                _db.SaveChanges();

                //Update defauter record with status=caseTreated and disciplinaryStatusId
                var defaulter = _db.Defaulters.Where(dft => dft.DefaulterId == defaulterId).First();

                defaulter.StudentDisciplinaryStatusId = newStudentDisciplinary.DefaulterId;
                defaulter.CaseTreated = true; //set case as true

                _db.Entry(defaulter).State = EntityState.Modified;

                try
                {
                    _db.SaveChanges();

                    string body = $"{studentUpdate.FirstName} with MatricNo {studentUpdate.MatricNo}, You are expected to come to the Academic Office to collect your disciplinary letter";

                    await SMSClass.SendSMS("UNIJOS SIS", body, studentUpdate.PhoneNumber); //EBULK SMS API
                }
                catch (Exception e)
                {

                }

                FlashMessage.Confirmation("Disciplinary Action Entered Successfully");
                return RedirectToAction("Index");
            }
            else
            {
                //update an existing record
                studentDisciplinary.DisciplineName = showStudentDetailViewModel.Punishment.ToString();
                studentDisciplinary.StillValid = true;
                studentDisciplinary.DateSetActive = date;
                studentDisciplinary.DefaulterId = showStudentDetailViewModel.DefaulterId;

                //update fields as appropriate to the disciplinary action taken
                switch ((Int32)showStudentDetailViewModel.Punishment)
                {
                    case 3:
                        studentDisciplinary.recallSession = ActiveSession.Year + 1;
                        break;
                    case 4:
                        studentDisciplinary.recallSession = ActiveSession.Year + 2;
                        break;
                    case 5:
                        studentDisciplinary.AmountToPay = showStudentDetailViewModel.PriceOfVadalisedProperty;
                        break;
                }
                _db.Entry(studentDisciplinary).State = EntityState.Modified;
                //_db.SaveChanges();

                var defaulter = await _db.Defaulters.Where(dft => dft.DefaulterId == defaulterId).FirstAsync();

                defaulter.StudentDisciplinaryStatusId = studentDisciplinary.DefaulterId;
                defaulter.CaseTreated = true; //set case as true
                _db.Entry(defaulter).State = EntityState.Modified;

                //update student detail to prevent academic activities
                var studentUpdate = _db.Students.Where(x => x.StudentId.Equals(showStudentDetailViewModel.StudentId)).FirstOrDefault();
                studentUpdate.MatricNo = "SUSP_" + studentUpdate.MatricNo;
                studentUpdate.Email = "SUSP_" + studentUpdate.Email;
                _db.Entry(studentUpdate).State = EntityState.Modified;

                try
                {
                    _db.SaveChanges();

                    string body = $"{studentUpdate.FirstName} You are expected to come to the Academic Office to collect your disciplinary letter";

                    await SMSClass.SendSMS("UNIJOS SIS", body, studentUpdate.PhoneNumber); //EBULK SMS API
                }
                catch (Exception e)
                {
                    
                }

                FlashMessage.Confirmation("Disciplinary Action Updated Successfully");
                return RedirectToAction("Index");
            }
        }

        // GET: StudentDisciplin/UpdateStudentStatus/
        public ActionResult UpdateStudentStatus()
        {

            return View();
        }

        // GET: StudentDisciplin/JSONReqestDetailsStudentStatus/MatNum
        //[ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<ActionResult> JSONReqestDetailsStudentStatus(string matriculationNumber)
        {
            //find student using matriculationNumber
            var student = _db.Defaulters.Include(st => st.Student).Where(st => st.Student.MatricNo.Trim().ToUpper().Equals(matriculationNumber.Trim().ToUpper())).FirstOrDefault();

            if (student == null)
            {
                return Json("null");
            }

            var defaulterMisconductList = await _db.Defaulters
                                            .Include(dt => dt.Misconduct)
                                            .Include(dt => dt.Student)
                                            .Include(dt => dt.Student.Programme.Department.Faculty)
                                            .Where(dt => dt.Student.MatricNo.Trim().ToUpper().Equals(matriculationNumber.Trim().ToUpper()))
                                            .Select(dt => new DefaulterMisconductViewModel
                                            {
                                                DefaulterId = dt.DefaulterId,
                                                MisconductName = dt.Misconduct.MisconductName,
                                                IncidentDate = dt.DefaultDate,
                                                Status = dt.CaseTreated,
                                                FacultyName = dt.Student.Programme.Department.Faculty.FacultyName
                                            }).ToListAsync();

            StudentDetailViewModel studentDetail = _db.Defaulters
                             .Include(st => st.Student.Programme.Department.Faculty)
                             .Include(st => st.Student.Programme.Department)
                             .Include(st => st.Student.Programme)
                             .Include(st => st.Student.Level)
                             .Include(st => st.Student)
                             .Include(st => st.StudentDisciplinaryStatus)
                             .Where(st => st.Student.MatricNo.Trim().ToUpper().Equals(matriculationNumber.Trim().ToUpper())).AsEnumerable()
                             .Select(st => new StudentDetailViewModel
                             {
                                 StudentId = st.StudentId,
                                 MatriculationNumber = st.Student.MatricNo,
                                 FullName = $"{st.Student.LastName} {st.Student.FirstName} {st.Student.MiddleName}",
                                 FacultyName = st.Student.Programme.Department.Faculty.FacultyName,
                                 DepartmentName = st.Student.Programme.Department.DeptName,
                                 DepartmentOptionName = st.Student.Programme.ProgrammeName,
                                 LevelName = st.Student.Level.LevelName,
                                 StudentDisciplinaryStatus = st.StudentDisciplinaryStatus,
                                 //Session =  GetCurrentSession,
                                 PhoneNumber = st.Student.PhoneNumber,
                                 DefaulterMisconductViewModel = defaulterMisconductList
                             }).FirstOrDefault();

            //check if student can olny be activated through application and redirect back with message
            if (studentDetail.StudentDisciplinaryStatus == null)
            {
                return Json("case_not_treated");
            }
            else if (studentDetail.StudentDisciplinaryStatus.DisciplineName.Equals("One_Session_Rustication") || studentDetail.StudentDisciplinaryStatus.DisciplineName.Equals("Two_Session_Rustication"))
            {
                return Json("redirect");
            }

            return View(studentDetail);
        }

        // GET: StudentDisciplin/JSONReqestGetStudentDetail/MatNum
        //[ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<ActionResult> JSONReqestGetStudentDetail(int DefaulterId)
        {
            //find student using matriculationNumber
            //var student = await _db.Students.Where(st => st.MatricNo.Equals(matriculationNumber)).FirstOrDefaultAsync();

            //if (student == null)
            //{
            //    //FlashMessage.Danger("Student not found on the DATABASE! Check the Matricultion Number to make sure it is correct.");
            //    return Json("Student not found! Check the Matricultion Number to make sure it is correct.");
            //}

            var defaulterMisconductList = await _db.Defaulters
                                             .Include(dt => dt.Misconduct)
                                             .Include(dt => dt.Student)
                                             .Where(dt => dt.DefaulterId.Equals(DefaulterId))
                                             .Select(dt => new DefaulterMisconductViewModel
                                             {
                                                 DefaulterId = dt.DefaulterId,
                                                 MisconductName = dt.Misconduct.MisconductName,
                                                 IncidentDate = dt.DefaultDate,
                                                 Status = dt.CaseTreated
                                             }).FirstOrDefaultAsync();

            StudentDetailViewModel studentDetail = _db.Defaulters
                             .Include(st => st.Student.Programme.Department.Faculty)
                             .Include(st => st.Student.Programme.Department)
                             .Include(st => st.Student.Programme)
                             .Include(st => st.Student.Level)
                             .Include(st => st.StudentDisciplinaryStatus)
                             //.Include(st => st.s)
                             .Where(st => st.DefaulterId.Equals(DefaulterId)).AsEnumerable()
                             .Select(st => new StudentDetailViewModel
                             {
                                 StudentId = st.StudentId,
                                 MatriculationNumber = st.Student.MatricNo,
                                 FullName = $"{st.Student.LastName} {st.Student.FirstName} {st.Student.MiddleName}",
                                 FacultyName = st.Student.Programme.Department.Faculty.FacultyName,
                                 DepartmentName = st.Student.Programme.Department.DeptName,
                                 DepartmentOptionName = st.Student.Programme.ProgrammeName,
                                 LevelName = st.Student.Level.LevelName,
                                 StudentDisciplinaryStatus = st.StudentDisciplinaryStatus,
                                 //Session =  GetCurrentSession,
                                 PhoneNumber = st.Student.PhoneNumber,
                                 DefaulterMisconductSingleViewModel = defaulterMisconductList
                             }).SingleOrDefault();


            return View(studentDetail);
        }

        // POST: /StudentDisciplin/JSONUpdateStudentStatus/
        public async Task<ActionResult> JSONUpdateStudentStatus(string studentId, string Reason)
        {
            var now = DateTime.UtcNow;
            var date = new DateTime(now.Year, now.Month, now.Day, now.Hour + 1, now.Minute, now.Second); //Added +1 to hours to compensate for UTC
            var updateStudentDisciplinaryStatus = await _db.StudentDisciplinaryStatus.Where(sdt => sdt.Defaulter.StudentId.Equals(studentId)).FirstOrDefaultAsync();

            //Update studentDiscipillinaryStatus
            updateStudentDisciplinaryStatus.StillValid = false;
            updateStudentDisciplinaryStatus.ReasonSetToInActive = Reason;
            updateStudentDisciplinaryStatus.DateSetInActive = date;
            updateStudentDisciplinaryStatus.StaffId = _db.Staffs.Where(us => us.Email.Trim().ToUpper().Equals(userId.Trim().ToUpper()))
                                                .Select(us => us.StaffId).FirstOrDefault(); //get loged-in staffId in place of this
            updateStudentDisciplinaryStatus.DisciplineName = null;

            _db.Entry(updateStudentDisciplinaryStatus).State = EntityState.Modified;
            _db.SaveChanges();

            //update requestRecall as treated
            var updateRequestStatus = await _db.StudentRecallRequests.Where(st => st.StudentId == studentId && st.RequestTreated == false).FirstOrDefaultAsync();
            updateRequestStatus.RequestTreated = true;
            _db.Entry(updateRequestStatus).State = EntityState.Modified;
            _db.SaveChanges();

            var message = "Student's Status has been successfully updated!";
            return Json(message, JsonRequestBehavior.AllowGet);
        }

        // GET: StudentDisciplin/ShowRecallRequestList/
        public ActionResult ShowRecallRequestList()
        {

            var recallRequestLists = _db.StudentRecallRequests
                                    //.Include(sr => sr.Defaulter)
                                    //.Include(sr => sr.Defaulter.Student)
                                    //.Include(sr => sr.Defaulter.StudentDisciplinaryStatus)
                                    .Where(sr => sr.RequestTreated == false)
                                    .ToList();
            var modelList = new List<RecallRequestListViewModel>();
            foreach (var recallRequestList in recallRequestLists)
            {
                var defaulter = _db.Defaulters.AsNoTracking().Include(i => i.Student).Include(i => i.StudentDisciplinaryStatus)
                    .Where(x => x.DefaulterId.Equals(recallRequestList.DefualterId))
                            .FirstOrDefault();

                var model = new RecallRequestListViewModel()
                {
                    MatriculationNumber = defaulter.Student.MatricNo,
                    FullName = $"{defaulter.Student.LastName} {defaulter.Student.FirstName} {defaulter.Student.MiddleName}",
                    //MisconductName = sr.Defaulter.Misconduct.MisconductName,
                    StudentId = defaulter.StudentId,
                    RecallRequestId = recallRequestList.StudentRecallRequestId,
                    ApplicationTreated = recallRequestList.RequestTreated,
                    DisciplineName = defaulter.StudentDisciplinaryStatus.DisciplineName,
                    DefaulterId = defaulter.DefaulterId,
                    RecallSession = defaulter.StudentDisciplinaryStatus.recallSession,
                    ReasonForRequest = recallRequestList.ReasonForRequest.ToString()
                };
                modelList.Add(model);

            }

            return View(modelList);
        }

        // GET: StudentDisciplin/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: StudentDisciplin/Edit/5
        //[HttpPost]
        public ActionResult JSONSendNotification(string mesgSubject, string meetingDate, string meetingTime, string meetingVenue, string messageText)
        {
            // TODO: Add update logic here
            //return RedirectToAction("Index");

            return Json(mesgSubject, JsonRequestBehavior.AllowGet);

        }

        // POST: StudentDisciplin/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: StudentDisciplin/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: StudentDisciplin/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
