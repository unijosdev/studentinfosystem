using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampusModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SwiftKampus.Controllers
{
    [Authorize]
    [Audit(AuditingLevel = 2)]
    public class AuditsController : BaseController
    {

        public AuditsController(SchoolDbContext db) : base(db)
        {

        }
        // GET: Audits
        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> GetIndex(string UserName, string AccessDate)
        {
            #region Server Side filtering

            //Get parameter for sorting from grid table
            // get Start (paging start index) and length (page size for paging)
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            //Get Sort columns values when we click on Header Name of column
            //getting column name
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            //Soring direction(either desending or ascending)
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
            string search = Request.Form.GetValues("search[value]").FirstOrDefault();

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int totalRecords = 0;

            var auditTrail = new List<AuditVm>();

            if (!string.IsNullOrEmpty(UserName) && !string.IsNullOrEmpty(AccessDate))
            {
                var myStartDate = Convert.ToDateTime(AccessDate);
                var v = await _db.AuditRecords.AsNoTracking()
                                .Where(x => (x.UserName.ToUpper().Equals(UserName.ToUpper().Trim())
                                || x.IpAddress.ToUpper().Equals(UserName.ToUpper().Trim())) &&
                                x.TimeAccessed.Equals(myStartDate))
                                .OrderByDescending(o => o.TimeAccessed).ToListAsync();
                // Mapping the student to the correct ViewModel for json display
                auditTrail.AddRange(MapToAuditIndex(v));
            }
            else if (!string.IsNullOrEmpty(search))
            {
                var v = await _db.AuditRecords.AsNoTracking()
                                .Where(x => x.UserName.ToUpper().Equals(search.ToUpper().Trim())
                                || x.IpAddress.ToUpper().Equals(search.ToUpper().Trim()))
                                .OrderByDescending(o => o.TimeAccessed).ToListAsync();
                // Mapping the student to the correct ViewModel for json display
                auditTrail.AddRange(MapToAuditIndex(v));
            }
            else
            {
                var v = await _db.AuditRecords.AsNoTracking().OrderByDescending(o => o.TimeAccessed).ToListAsync();
                auditTrail.AddRange(MapToAuditIndex(v));
            }

            totalRecords = auditTrail.Count();
            var data = auditTrail.Skip(skip).Take(pageSize).ToList();

            return Json(new { draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data },
                JsonRequestBehavior.AllowGet);

            #endregion Server Side filtering
        }

        private List<AuditVm> MapToAuditIndex(List<Audit> v)
        {
            var auditIndex = new List<AuditVm>();
            foreach (var audit in v)
            {
                var index = new AuditVm()
                {
                    AuditId = audit.AuditId,
                    UserName = audit.UserName,
                    IpAddress = audit.IpAddress,
                    TimeAccessed = audit.TimeAccessed.ToString(),
                    UrlAccessed = audit.UrlAccessed,
                    ActionPerformed = audit.ActionPerformed
                };
                auditIndex.Add(index);
            }
            return auditIndex;
        }

        public ActionResult LoginIndex()
        {
            return View();
        }

        public async Task<ActionResult> GetLoginIndex()
        {
            var users = await _db.Users.AsNoTracking().Where(s => s.IsLogin.Equals(true))
                                .Select(s => s.Email).ToListAsync();
            var data = await GetLoginDetails(users);
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        private async Task<List<LoginDetailVm>> GetLoginDetails(List<string> v)
        {
            var loginUsers = new List<LoginDetailVm>();
            foreach (var user in v)
            {
                var index = await _query.GetUserDetails(user);
                if (index != null)
                {
                    loginUsers.Add(index);
                }
            }
            return loginUsers;
        }

        // GET: Audits/Details/5
        public async Task<ActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Audit audit = await _db.AuditRecords.FindAsync(id);
            if (audit == null)
            {
                return HttpNotFound();
            }
            return View(audit);
        }

        #region CRUD Operation disabled

        //// GET: Audits/Details/5
        //public async Task<ActionResult> Details(Guid? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Audit audit = await _db.AuditRecords.FindAsync(id);
        //    if (audit == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(audit);
        //}

        //// GET: Audits/Create
        //public ActionResult Create()
        //{
        //    return View();
        //}

        //// POST: Audits/Create
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for
        //// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Create([Bind(Include = "AuditId,SessionId,UserName,IpAddress,UrlAccessed,TimeAccessed,Data")] Audit audit)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        audit.AuditId = Guid.NewGuid();
        //        _db.AuditRecords.Add(audit);
        //        await _db.SaveChangesAsync();
        //        return RedirectToAction("Index");
        //    }

        //    return View(audit);
        //}

        //// GET: Audits/Edit/5
        //public async Task<ActionResult> Edit(Guid? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Audit audit = await _db.AuditRecords.FindAsync(id);
        //    if (audit == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(audit);
        //}

        //// POST: Audits/Edit/5
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for
        //// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Edit([Bind(Include = "AuditId,SessionId,UserName,IpAddress,UrlAccessed,TimeAccessed,Data")] Audit audit)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _db.Entry(audit).State = EntityState.Modified;
        //        await _db.SaveChangesAsync();
        //        return RedirectToAction("Index");
        //    }
        //    return View(audit);
        //}

        //// GET: Audits/Delete/5
        //public async Task<ActionResult> Delete(Guid? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Audit audit = await _db.AuditRecords.FindAsync(id);
        //    if (audit == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(audit);
        //}

        //// POST: Audits/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> DeleteConfirmed(Guid id)
        //{
        //    Audit audit = await _db.AuditRecords.FindAsync(id);
        //    _db.AuditRecords.Remove(audit);
        //    await _db.SaveChangesAsync();
        //    return RedirectToAction("Index");
        //}

        #endregion CRUD Operation disabled

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