using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampusModel.Library;
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
    public class BooksController : BaseController
    {

        public BooksController(SchoolDbContext db) : base(db)
        {

        }


        // GET: Books
        public async Task<ActionResult> Index(string book)
        {
            var searchResult = new List<Book>();


            if (book != null)
            {
                searchResult = await _db.Books.Where(x => x.Subject.Contains(book)
                                                            || x.AccessionNo.Contains(book)
                                                            || x.Author.Contains(book)
                                                            || x.Title.Contains(book)).ToListAsync();
            }
            else
            {
                searchResult = await _db.Books.Include(b => b.BookCategory).ToListAsync();
            }
            return View(searchResult);
        }
        [HttpGet]
        public async Task<ActionResult> Search(string book)
        {
            var searchResult = await _db.Books.Where(x => x.Subject.Contains(book)
                                                   || x.AccessionNo.Contains(book)
                                                   || x.Author.Contains(book)
                                                   || x.Title.Contains(book)).ToListAsync();
            return View("Index", searchResult);
        }
        // GET: Books/Details/5
        public async Task<ActionResult> Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Book book = await _db.Books.FindAsync(id);
            if (book == null)
            {
                return HttpNotFound();
            }
            return View(book);
        }

        // GET: Books/Create
        public ActionResult Create()
        {
            ViewBag.BookCategoryId = new SelectList(_db.BookCategories, "BookCategoryId", "BookCategoryName");
            return View();
        }

        // POST: Books/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "AccessionNo,BookId,Title,Author,JointAuthor,Subject,ISBN,Edition,Publisher,PlaceOfPublish,BookCategoryId")] Book book)
        {
            if (ModelState.IsValid)
            {
                _db.Books.Add(book);
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.BookCategoryId = new SelectList(_db.BookCategories, "BookCategoryId", "BookCategoryName", book.BookCategoryId);
            return View(book);
        }

        // GET: Books/Edit/5
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Book book = await _db.Books.FindAsync(id);
            if (book == null)
            {
                return HttpNotFound();
            }
            ViewBag.BookCategoryId = new SelectList(_db.BookCategories, "BookCategoryId", "BookCategoryName", book.BookCategoryId);
            return View(book);
        }

        // POST: Books/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "AccessionNo,BookId,Title,Author,JointAuthor,Subject,ISBN,Edition,Publisher,PlaceOfPublish,BookCategoryId")] Book book)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(book).State = System.Data.Entity.EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.BookCategoryId = new SelectList(_db.BookCategories, "BookCategoryId", "BookCategoryName", book.BookCategoryId);
            return View(book);
        }

        // GET: Books/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Book book = await _db.Books.FindAsync(id);
            if (book == null)
            {
                return HttpNotFound();
            }
            return View(book);
        }

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            Book book = await _db.Books.FindAsync(id);
            _db.Books.Remove(book);
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
