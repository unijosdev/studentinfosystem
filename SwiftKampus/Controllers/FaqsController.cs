using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampusModel;
using System;
using System.Data.Entity;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SwiftKampus.Controllers
{
    [Authorize]
    [Audit(AuditingLevel = 2)]
    [Authorize(Roles = RoleName.Admin)]
    public class FaqsController : BaseController
    {

        public FaqsController(SchoolDbContext db) : base(db)
        {

        }
        // GET: Faqs
        public async Task<ActionResult> Index()
        {
            return View(await _db.Faqs.ToListAsync());
        }

        [AllowAnonymous]
        public async Task<ActionResult> Faq()
        {
            return View(await _db.Faqs.ToListAsync());
        }

        // GET: Faqs/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Faq faq = await _db.Faqs.FindAsync(id);
            if (faq == null)
            {
                return HttpNotFound();
            }
            return View(faq);
        }

        // GET: Faqs/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Faqs/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Faq faq)
        {
            if (ModelState.IsValid)
            {
                faq.AskedDate = DateTime.Now;
                _db.Faqs.Add(faq);
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(faq);
        }

        public async Task<PartialViewResult> Save(int id)
        {
            var faq = await _db.Faqs.FindAsync(id);
            return PartialView(faq);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Save(Faq model)
        {
            bool status = false;
            string message = string.Empty;
            model.AskedDate = DateTime.Now;
            if (ModelState.IsValid)
            {
                if (model.FaqId > 0)
                {
                    var faq = await _db.Faqs.FindAsync(model.FaqId);
                    if (faq != null)
                    {
                        faq.Answer = model.Answer;
                        faq.AnswerBy = model.AnswerBy;
                        faq.RepliedDate = DateTime.Now.ToString();

                        _db.Entry(faq).State = EntityState.Modified;
                        await _db.SaveChangesAsync();
                        message = $"{faq.Question} Replied Successfully...";
                        return new JsonResult { Data = new { status = true, message } };
                    }
                }
                else
                {
                    _db.Faqs.Add(model);
                    await _db.SaveChangesAsync();
                    message = $"{model.Question} Added Successfully.";
                    return new JsonResult { Data = new { status = true, message } };
                }
            }
            return new JsonResult { Data = new { status, message } };
            //return View(subject);
        }

        // GET: Faqs/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Faq faq = await _db.Faqs.FindAsync(id);
            if (faq == null)
            {
                return HttpNotFound();
            }
            return View(faq);
        }

        // POST: Faqs/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "FaqId,Question,QuestionBy,AskedDate,Answer,AnswerBy,RepliedDate")] Faq faq)
        {
            if (ModelState.IsValid)
            {
                faq.AskedDate = DateTime.Now;
                _db.Entry(faq).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(faq);
        }

        // GET: Faqs/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Faq faq = await _db.Faqs.FindAsync(id);
            if (faq == null)
            {
                return HttpNotFound();
            }
            return View(faq);
        }

        // POST: Faqs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Faq faq = await _db.Faqs.FindAsync(id);
            _db.Faqs.Remove(faq);
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
