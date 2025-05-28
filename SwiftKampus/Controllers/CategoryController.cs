using SwiftKampus.Controllers;
using SwiftKampus.Models;
using SwiftKampusModel.MarketPlace;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SwiftKampus.Abstractions.Controllers
{
    public class CategoryController : BaseController
    {

        public CategoryController(SchoolDbContext db) : base(db)
        {

        }
        // GET: Category
        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> GetIndex()
        {
            // dc.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var categories = await _db.ProductCategories.AsNoTracking().ToListAsync();
            var data = categories.Select(s => new
            {
                s.Id,
                s.CategoryName,
                s.Code,
                s.Visible,
                //s.Amount,

            }).ToList();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        public async Task<PartialViewResult> Save(int? id)
        {
            var category = await _db.ProductCategories.FindAsync(id);
            return PartialView(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Save(ProductCategory model)
        {
            bool status = false;
            string message = string.Empty;
            //string[] ssizes = model.ChargeName.Trim().Split('-', '/');

            if (ModelState.IsValid)
            {
                if (model.Id > 0)
                {
                    var productCategory = await _db.ProductCategories.FindAsync(model.Id);
                    if (productCategory != null)
                    {
                        try
                        {
                            productCategory.CategoryName = model.CategoryName.Trim();
                            productCategory.Code = model.Code;
                            productCategory.Visible = model.Visible;
                            _db.Entry(productCategory).State = System.Data.Entity.EntityState.Modified;
                            await _db.SaveChangesAsync();
                            message = $"{model.CategoryName} Updated Successfully...";
                            return new JsonResult { Data = new { status = true, message } };
                        }
                        catch (Exception ex)
                        {
                            return new JsonResult { Data = new { status = false, message = ex.Message } };
                        }
                    }
                }
                else
                {
                    model.CategoryName = model.CategoryName;
                    model.Code = model.Code;
                    model.Visible = model.Visible;
                   
                    _db.ProductCategories.Add(model);
                    await _db.SaveChangesAsync();
                    message = $"{model.CategoryName} Added Successfully.";
                    return new JsonResult { Data = new { status = true, message } };
                }
            }
            return new JsonResult { Data = new { status, message } };
            //return View(subject);
        }

        // GET: Sessions/Delete/5
        [HttpGet]
        public async Task<ActionResult> Delete(int id)
        {
            bool status = false;
            string message = string.Empty;
            var productCategory = await _db.ProductCategories.FindAsync(id);
            if (productCategory != null)
            {
                _db.ProductCategories.Remove(productCategory);
                await _db.SaveChangesAsync();
                status = true;
                message = "DataS Deleted Successfully...";
                //return new JsonResult { Data = new { status, message } };
                return this.Json(status, JsonRequestBehavior.AllowGet);
            }

            return new JsonResult { Data = new { status = false, message = $"{id } not deleted" } };
        }


        // GET: Category/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Category/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Category/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Category/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Category/Edit/5
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

        //// GET: Category/Delete/5
        //public ActionResult Delete(int id)
        //{
        //    return View();
        //}

        // POST: Category/Delete/5
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
