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
    public class PriceController : BaseController
    {
        public PriceController(SchoolDbContext db) : base(db)
        {

        }
        // GET: Price
        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> GetIndex()
        {
            // dc.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var products = await _db.Prices.Include(p => p.Product).AsNoTracking().ToListAsync();

            var data = products.Select(s => new
            {
                s.Id,
                s.Product.ProductName,
                s.BuyPrice,
                s.SalePrice,
                s.Discount,
                s.Visible,
                //s.Amount,

            }).ToList();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        public async Task<PartialViewResult> Save(int? id)
        {
            var Price = await _db.Prices.FindAsync(id);
            var priceId = Price != null ? Price.Id : 0;
            ViewBag.ProductsId = new SelectList(_db.Products.AsNoTracking().Where(x => x.Visible == 1), "Id", "ProductName", priceId);

            return PartialView(Price);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Save(Price model)
        {
            bool status = false;
            string message = string.Empty;
            //string[] ssizes = model.ChargeName.Trim().Split('-', '/');

            if (ModelState.IsValid)
            {
                if (model.Id > 0)
                {
                    var price = await _db.Prices.FindAsync(model.Id);
                    if (price != null)
                    {
                        try
                        {
                            price.ProductId = model.ProductId;
                            price.BuyPrice = model.BuyPrice;
                            price.SalePrice = model.SalePrice;
                            price.Discount = model.Discount;
                            price.Visible = model.Visible;
                            _db.Entry(price).State = System.Data.Entity.EntityState.Modified;
                            await _db.SaveChangesAsync();
                            message = $"{model.Product.ProductName} Updated Successfully...";
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
                    model.ProductId = model.ProductId;
                    model.BuyPrice = model.BuyPrice;
                    model.SalePrice = model.SalePrice;
                    model.Discount = model.Discount;
                    model.Visible = model.Visible;
                    //model.Visible = model.Visible;

                    _db.Prices.Add(model);
                    await _db.SaveChangesAsync();
                    message = $"Price Added Successfully.";
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
            var price = await _db.Prices.FindAsync(id);
            if (price != null)
            {
                _db.Prices.Remove(price);
                await _db.SaveChangesAsync();
                status = true;
                message = "Data Deleted Successfully...";
                //return new JsonResult { Data = new { status, message } };
                return this.Json(status, JsonRequestBehavior.AllowGet);
            }

            return new JsonResult { Data = new { status = false, message = $"{id } not deleted" } };
        }
    }
}