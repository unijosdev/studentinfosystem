using Microsoft.AspNet.Identity;
using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampus.ViewModels;
using SwiftKampusModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SwiftKampus.Controllers
{
    [Authorize]
    [Audit(AuditingLevel = 2)]
    public class CorrespondencesController : BaseController
    {

        public CorrespondencesController(SchoolDbContext db) : base(db)
        {

        }

        // GET: Correspondences
        public async Task<ActionResult> Index()
        {
            var correspondences = await _db.Correspondences.Include(c => c.CorrespondenceType)
                                    .Include(c => c.Department)
                                    .Include(c => c.Student).AsNoTracking()
                                    .Select(c => new CorrespondenceIndexVM
                                    {
                                        StudentFullName = c.Student.FirstName + " " + c.Student.MiddleName + " " + c.Student.LastName,
                                        Reciepient = c.CorrespondenceReciepients.Select(cr => cr.UserName).SingleOrDefault(),
                                        Subject = c.CorrespondenceSubject,
                                        CorrespondenceDate = c.CorrespondenceDate,
                                        CorrespondenceTypeName = c.CorrespondenceType.CorrespondenceTypeName,
                                        Approval = c.Approved,
                                        DeptCode = c.Department.DeptCode,
                                        CorrespondenceId = c.CorrespondenceId
                                    }).ToListAsync();

            return View(correspondences);
        }

        // GET: Correspondences/Details/5
        //[Authorize(Roles ="Student")]
        public async Task<ActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var correspondence = await _db.Correspondences.Include(c => c.Department).Include(c => c.Student)
                                        .Include(c => c.CorrespondenceType).Include(c => c.CorrespondenceReciepients)
                                        .Where(c => c.CorrespondenceId == id.Value)
                                        .Select(c => new CorrespondenceDetailVM
                                        {
                                            StudentFullName = c.Student.FirstName + " " + c.Student.MiddleName + " " + c.Student.LastName,
                                            StudentMatNumber = c.Student.StudentId,
                                            CorrespondenceRecepient = c.CorrespondenceReciepients.Select(cr => cr.UserName).SingleOrDefault(),
                                            RecepientDesignation = c.CorrespondenceReciepients.Select(cr => cr.Designation).SingleOrDefault(),
                                            CorrespondenceBody = c.CorrespondenceBody,
                                            CorrespondenceType = c.CorrespondenceType.CorrespondenceTypeName,
                                            DeptCode = c.Department.DeptCode,
                                            Faculty = c.Department.Faculty.FacultyCode,
                                            Date = c.CorrespondenceDate,
                                            Status = c.Approved,
                                            Subject = c.CorrespondenceSubject
                                        }).SingleOrDefaultAsync();
            if (correspondence == null)
            {
                return HttpNotFound();
            }
            return View(correspondence);
        }

        // GET: Correspondences/Create
        public async Task<ActionResult> Create()
        {
            var studentmatno = User.Identity.GetUserName();

            var corresp = _db.Correspondences.Include(c => c.Department).Include(c => c.Student)
                .Include(c => c.CorrespondenceReciepients);

            var dept = await _db.Departments.Include(d => d.Students).AsNoTracking()
                               .Where(d => d.Students.FirstOrDefault().StudentId == studentmatno)
                               .SingleOrDefaultAsync();

            var createvm = new CorrespondenceCreationVM
            {
                Student = studentmatno,
                DepartmentCode = dept.DeptCode,
                CorrespondenceType = await _db.CorrespondenceTypes.AsNoTracking().ToListAsync(),
                CorrespondenceReciepients = await _db.Staffs.AsNoTracking().ToListAsync()
            };

            return View(createvm);
        }

        // POST: Correspondences/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "CorrespondenceId,StudentId,DepartmentId,CorrespondenceBody,CorrespondenceTypeId,CorrespondenceSubject,CorrespondenceDate,Approved")] Correspondence correspondence)
        {
            if (ModelState.IsValid)
            {
                correspondence.CorrespondenceId = Guid.NewGuid();
                _db.Correspondences.Add(correspondence);
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.CorrespondenceId = new SelectList(_db.CorrespondenceTypes, "CorrespondenceTypeId", "CorrespondenceTypeName", correspondence.CorrespondenceId);
            ViewBag.DepartmentId = new SelectList(_db.Departments, "DepartmentId", "DeptCode", correspondence.DepartmentId);
            ViewBag.StudentId = new SelectList(_db.Students, "StudentId", "FirstName", correspondence.StudentId);
            var createvm = new CorrespondenceCreationVM
            {
                Student = correspondence.StudentId,
                DepartmentCode = correspondence.Department.DeptCode,
                CorrespondenceType = await _db.CorrespondenceTypes.AsNoTracking().ToListAsync(),
                CorrespondenceReciepients = await _db.Staffs.AsNoTracking().ToListAsync()
            };
            return View(createvm);
        }

        // GET: Correspondences/Edit/5
        public async Task<ActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var correspondence = await _db.Correspondences.FindAsync(id);
            if (correspondence == null)
            {
                return HttpNotFound();
            }
            ViewBag.CorrespondenceId = new SelectList(_db.CorrespondenceTypes, "CorrespondenceTypeId", "CorrespondenceTypeName", correspondence.CorrespondenceId);
            ViewBag.DepartmentId = new SelectList(_db.Departments, "DepartmentId", "DeptCode", correspondence.DepartmentId);
            ViewBag.StudentId = new SelectList(_db.Students, "StudentId", "FirstName", correspondence.StudentId);
            return View(correspondence);
        }

        // POST: Correspondences/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "CorrespondenceId,StudentId,DepartmentId,CorrespondenceBody,CorrespondenceTypeId,CorrespondenceSubject,CorrespondenceDate,Approved")] Correspondence correspondence)
        {
            if (ModelState.IsValid)
            {
                //_db.Entry(correspondence).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.CorrespondenceId = new SelectList(_db.CorrespondenceTypes, "CorrespondenceTypeId", "CorrespondenceTypeName", correspondence.CorrespondenceId);
            ViewBag.DepartmentId = new SelectList(_db.Departments, "DepartmentId", "DeptCode", correspondence.DepartmentId);
            ViewBag.StudentId = new SelectList(_db.Students, "StudentId", "FirstName", correspondence.StudentId);
            return View(correspondence);
        }

        // GET: Correspondences/Delete/5
        public async Task<ActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Correspondence correspondence = await _db.Correspondences.FindAsync(id);
            if (correspondence == null)
            {
                return HttpNotFound();
            }
            return View(correspondence);
        }

        // POST: Correspondences/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(Guid id)
        {
            Correspondence correspondence = await _db.Correspondences.FindAsync(id);
            _db.Correspondences.Remove(correspondence);
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
