using SwiftKampus.Controllers;
using SwiftKampus.Models;
using SwiftKampus.ViewModels.MisconductVm;
using SwiftKampusModel.Misconduct;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace Unijos.Web.Controllers
{
    public class MisconductController : BaseController
    {
        public MisconductController(SchoolDbContext db) : base(db)
        {
        }

        // GET: Misconduct
        public ActionResult Index()
        {
            var misconducts = _db.Misconducts
                .Select(mc => new MisconductViewModel
                {
                    MisconductId = mc.MisconductId,
                    MisconductName = mc.MisconductName
                }).ToList();

            return View(misconducts);
        }

        // GET: Misconduct/Details/5
        public ActionResult Details(int id)
        {
            var misconduct = _db.Misconducts
                                .Where(ms => ms.MisconductId == id)
                                .Select(ms => new MisconductViewModel
                                {
                                    MisconductId = ms.MisconductId,
                                    MisconductName = ms.MisconductName
                                }).First();

            return View(misconduct);
        }

        // GET: Misconduct/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Misconduct/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(string MisconductName)
        {
            try
            {
                // TODO: Add insert logic here
                Misconduct misconduct = new Misconduct
                {
                    MisconductName = MisconductName
                };

                _db.Misconducts.Add(misconduct);
                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Misconduct/Edit/5
        public ActionResult Edit(int id)
        {
            var misconduct = _db.Misconducts.Find(id);

            if (misconduct == null)
            {
                return HttpNotFound();
            }

            var editMisconduct = new MisconductViewModel
            {
                MisconductId = misconduct.MisconductId,
                MisconductName = misconduct.MisconductName
            };
            return View(editMisconduct);
        }

        // POST: Misconduct/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, string misconductName)
        {
            try
            {
                // TODO: Add update logic here
                var misconduct = _db.Misconducts.Find(id);
                misconduct.MisconductName = misconductName;
                misconduct.MisconductId = id;

                _db.Entry(misconduct).State = EntityState.Modified;
                _db.SaveChanges();

                ViewBag.Message = "Record Edited Successfully!";
                return RedirectToAction("Index");
            }
            catch
            {
                ViewBag.Message = "Edit failed";
                return View();
            }
        }

        // GET: Misconduct/Delete/5
        public ActionResult Delete(int id)
        {
            var misconduct = _db.Misconducts
                               .Where(ms => ms.MisconductId == id)
                               .Select(ms => new MisconductViewModel
                               {
                                   MisconductId = ms.MisconductId,
                                   MisconductName = ms.MisconductName
                               }).First();
            return View(misconduct);
        }

        // POST: Misconduct/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, string misconductName)
        {
            try
            {
                //Delete the defaulter
                Misconduct misconduct = _db.Misconducts.Find(id);
                _db.Misconducts.Remove(misconduct);
                _db.SaveChanges();
                ViewBag.Message = "Record Deleted Successfully!";

                //Delete corresponding file

                return RedirectToAction("Index");
            }
            catch
            {
                ViewBag.Message = "Edit failed";
                return View();
            }
        }
    }
}
