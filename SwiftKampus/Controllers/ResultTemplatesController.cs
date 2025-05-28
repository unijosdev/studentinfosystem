using SwiftKampus.Models;
using SwiftKampusModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SwiftKampus.Controllers
{
    public class ResultTemplatesController : BaseController
    {
        public ResultTemplatesController(SchoolDbContext db) : base(db)
        {

        }
        // GET: ResultTemplates
        public ActionResult Index()
        {
            return View();
        }
        public async Task<ActionResult> GetIndex()
        {
            // dc.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var data = await _db.ResultTemplates.AsNoTracking()
                                .Select(s => new
                                {
                                    s.FancyName,
                                    s.ResultType,
                                    s.ResultTemplateId
                                }).ToListAsync();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }


        public async Task<PartialViewResult> Save(int id)
        {
            var resultTemplate = await _db.ResultTemplates.FindAsync(id);

            var resultNameType = from ResultNameType s in Enum.GetValues(typeof(ResultNameType))
                                 select new { ID = s, Name = s.ToString() };
            ViewBag.ResultType = new SelectList(resultNameType, "Name", "Name");
            return PartialView(resultTemplate);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Save(ResultTemplate model)
        {
            bool status = false;
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                if (model.ResultTemplateId > 0)
                {
                    var resultTemplate = await _db.ResultTemplates.FindAsync(model.ResultTemplateId);
                    if (resultTemplate != null)
                    {
                        resultTemplate.FancyName = model.FancyName.Trim();
                        resultTemplate.ResultType = model.ResultType;
                        _db.Entry(resultTemplate).State = EntityState.Modified;
                        await _db.SaveChangesAsync();
                        message = $"{model.FancyName} Updated Successfully...";
                        return new JsonResult { Data = new { status = true, message } };
                    }
                }
                else
                {
                    _db.ResultTemplates.Add(model);
                    await _db.SaveChangesAsync();
                    message = $"{model.FancyName} Added Successfully.";
                    return new JsonResult { Data = new { status = true, message } };
                }
            }
            return new JsonResult { Data = new { status, message } };
            //return View(subject);
        }


        // GET: ResultTemplates/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ResultTemplate resultTemplate = await _db.ResultTemplates.FindAsync(id);
            if (resultTemplate == null)
            {
                return HttpNotFound();
            }
            return View(resultTemplate);
        }

        // GET: ResultTemplates/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ResultTemplates/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "ResultTemplateId,ResultType,FancyName")] ResultTemplate resultTemplate)
        {
            if (ModelState.IsValid)
            {
                _db.ResultTemplates.Add(resultTemplate);
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(resultTemplate);
        }

        // GET: ResultTemplates/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ResultTemplate resultTemplate = await _db.ResultTemplates.FindAsync(id);
            if (resultTemplate == null)
            {
                return HttpNotFound();
            }
            return View(resultTemplate);
        }

        // POST: ResultTemplates/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "ResultTemplateId,ResultType,FancyName")] ResultTemplate resultTemplate)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(resultTemplate).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(resultTemplate);
        }

        // GET: ResultTemplates/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ResultTemplate resultTemplate = await _db.ResultTemplates.FindAsync(id);
            if (resultTemplate == null)
            {
                return HttpNotFound();
            }
            return View(resultTemplate);
        }

        // POST: ResultTemplates/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            ResultTemplate resultTemplate = await _db.ResultTemplates.FindAsync(id);
            _db.ResultTemplates.Remove(resultTemplate);
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
