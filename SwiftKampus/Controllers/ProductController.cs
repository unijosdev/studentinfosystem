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
    public class ProductController : BaseController
    {
        public ProductController(SchoolDbContext db) : base(db)
        {

        }
        // GET: Product
        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> GetIndex()
        {
            // dc.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var products = await _db.Products.Include(p => p.ProductCategory).AsNoTracking().ToListAsync();

            var data = products.Select(s => new
            {
                s.Id,
                s.ProductName,
                s.ProductCategory.CategoryName,
                s.Visible,
                s.ProductPrice,

            }).ToList();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        public async Task<PartialViewResult> Save(int? id)
        {
            var Product = await _db.Products.FindAsync(id);
            var productCatId = Product != null ? Product.Id : 0;
            ViewBag.ProductCategoriesId = new SelectList(_db.ProductCategories.AsNoTracking().Where(x => x.Visible == 1), "Id", "CategoryName", productCatId);

            return PartialView(Product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Save(Product model)
        {
            bool status = false;
            string message = string.Empty;
            //string[] ssizes = model.ChargeName.Trim().Split('-', '/');

            if (ModelState.IsValid)
            {
                if (model.Id > 0)
                {
                    var product = await _db.Products.FindAsync(model.Id);
                    if (product != null)
                    {
                        try
                        {
                            product.ProductName = model.ProductName.Trim();
                            product.ProductCategoryId = model.ProductCategoryId;
                            product.ProductPrice = model.ProductPrice;
                            product.Visible = model.Visible;
                            _db.Entry(product).State = System.Data.Entity.EntityState.Modified;
                            await _db.SaveChangesAsync();
                            message = $"{model.ProductName} Updated Successfully...";
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
                    model.ProductName = model.ProductName;
                    model.ProductCategoryId = model.ProductCategoryId;
                    model.ProductPrice = model.ProductPrice;
                    model.Visible = model.Visible;

                    _db.Products.Add(model);
                    await _db.SaveChangesAsync();
                    message = $"{model.ProductName} Added Successfully.";
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
            var product = await _db.Products.FindAsync(id);
            if (product != null)
            {
                _db.Products.Remove(product);
                await _db.SaveChangesAsync();
                status = true;
                message = "DataS Deleted Successfully...";
                //return new JsonResult { Data = new { status, message } };
                return this.Json(status, JsonRequestBehavior.AllowGet);
            }

            return new JsonResult { Data = new { status = false, message = $"{id } not deleted" } };
        }
    }
}