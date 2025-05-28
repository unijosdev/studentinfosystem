using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampusModel;
using SwiftKampusModel.Library;
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
    public class BookIssuesController : BaseController
    {

        public BookIssuesController(SchoolDbContext db) : base(db)
        {

        }

        // GET: BookIssues
        public async Task<ActionResult> Index()
        {
            return View(await _db.BookIssues.ToListAsync());
        }

        // GET: BookIssues/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BookIssue bookIssue = await _db.BookIssues.FindAsync(id);
            if (bookIssue == null)
            {
                return HttpNotFound();
            }
            return View(bookIssue);
        }

        // GET: BookIssues/Create
        public ActionResult Create()
        {
            var status = from Status s in Enum.GetValues(typeof(Status))
                         select new { ID = s, Name = s.ToString() };

            ViewBag.Status = new SelectList(status, "Name", "Name");
            return View();
        }

        // POST: BookIssues/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "BookIssueId,StudentId,AccessionNo,IssueDate,DueDate,Status")] BookIssue bookIssue)
        {
            if (ModelState.IsValid)
            {

                //var bookId = _db.Books.Where(c => c.AccessionNo.Equals(bookIssue.AccessionNo)).Select(c => c.BookId).FirstOrDefault();
                _db.BookIssues.Add(bookIssue);

                var book = await _db.Books.FirstOrDefaultAsync(x => x.AccessionNo.Equals(bookIssue.AccessionNo));
                if (book.TotalBook == 0)
                {
                    ViewBag.BookNotPresent = "The Requested book/books is not present in the library...please check tommorow";
                }
                book.BorrowedBook += 1;
                _db.Entry(book).State = EntityState.Modified;
                await _db.SaveChangesAsync();

                return RedirectToAction("Index");
            }
            var status = from Status s in Enum.GetValues(typeof(Status))
                         select new { ID = s, Name = s.ToString() };

            ViewBag.Status = new SelectList(status, "Name", "Name");
            return View(bookIssue);
        }

        // GET: BookIssues/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BookIssue bookIssue = await _db.BookIssues.FindAsync(id);
            if (bookIssue == null)
            {
                return HttpNotFound();
            }
            var status = from Status s in Enum.GetValues(typeof(Status))
                         select new { ID = s, Name = s.ToString() };

            ViewBag.Status = new SelectList(status, "Name", "Name");
            return View(bookIssue);
        }

        // POST: BookIssues/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "BookIssueId,StudentId,AccessionNo,IssueDate,DueDate,Status")] BookIssue bookIssue)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(bookIssue).State = EntityState.Modified;
                //  await _db.SaveChangesAsync();

                if (bookIssue.Status.ToString().Equals("GivenOut"))
                {
                    var book = await _db.Books.FirstOrDefaultAsync(x => x.AccessionNo.Equals(bookIssue.AccessionNo));
                    book.BorrowedBook += 1;
                    _db.Entry(book).State = EntityState.Modified;
                    await _db.SaveChangesAsync();
                }
                if (bookIssue.Status.ToString().Equals("Returned"))
                {
                    var book = await _db.Books.FirstOrDefaultAsync(x => x.AccessionNo.Equals(bookIssue.AccessionNo));
                    //check to see if the number of book is not zero inorder to avoid a negative number/////////////
                    if (book.BorrowedBook != 0)
                    {
                        book.BorrowedBook -= 1;
                    }

                    _db.Entry(book).State = EntityState.Modified;
                    await _db.SaveChangesAsync();
                }
                return RedirectToAction("Index");
            }
            var status = from Status s in Enum.GetValues(typeof(Status))
                         select new { ID = s, Name = s.ToString() };

            ViewBag.Status = new SelectList(status, "Name", "Name");
            return View(bookIssue);
        }

        // GET: BookIssues/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BookIssue bookIssue = await _db.BookIssues.FindAsync(id);
            if (bookIssue == null)
            {
                return HttpNotFound();
            }
            return View(bookIssue);
        }

        // POST: BookIssues/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            BookIssue bookIssue = await _db.BookIssues.FindAsync(id);
            _db.BookIssues.Remove(bookIssue);
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
