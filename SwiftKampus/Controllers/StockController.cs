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
    public class StockController : BaseController
    {
        public StockController(SchoolDbContext db) : base(db)
        {

        }
        // GET: Stock
        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> GetIndex()
        {
            // dc.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var products = await _db.Stocks.Include(p => p.Product).AsNoTracking().ToListAsync();

            var data = products.Select(s => new
            {
                s.Id,
                s.Product.ProductName,
                s.QuantityBeforeOrder,
                s.QuantityOrder,
                s.SalesQuantity,
                s.CurrentQuantity,
                s.CollectedQuantity,
                s.NotCollectedQuantity,
                //s.,
            }).ToList();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }


        public async Task<PartialViewResult> Save(int? id)
        {
            var Stock = await _db.Stocks.FindAsync(id);
            var stockId = Stock != null ? Stock.Id : 0;
            ViewBag.ProductsId = new SelectList(_db.Products.AsNoTracking().Where(x => x.Visible == 1), "Id", "ProductName", stockId);

            return PartialView(Stock);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Save(Stock model)
        {
            bool status = false;
            string message = string.Empty;
            //string[] ssizes = model.ChargeName.Trim().Split('-', '/');

            if (ModelState.IsValid)
            {
                var stock = await _db.Stocks.Where(p => p.ProductId == model.ProductId).FirstOrDefaultAsync();

                if (model.ProductId > 0)
                {
                    var newStockOder = new StockOrder() {
                        ProductId = model.ProductId,
                        OrderQuantity = model.QuantityOrder,
                        OrderDate = DateTime.Now.ToString(),
                        Visible = 1
                    };

                    _db.StockOrders.Add(newStockOder);


                    if (stock != null)
                    {
                        try
                        {
                            stock.ProductId = stock.ProductId;
                            stock.QuantityBeforeOrder = stock.CurrentQuantity;
                            stock.QuantityOrder = model.QuantityOrder;
                            stock.SalesQuantity = stock.SalesQuantity;
                            stock.CollectedQuantity = stock.CollectedQuantity;
                            stock.NotCollectedQuantity = stock.NotCollectedQuantity;
                            stock.CurrentQuantity = stock.CurrentQuantity + model.QuantityOrder;
                            _db.Entry(stock).State = System.Data.Entity.EntityState.Modified;
                            await _db.SaveChangesAsync();
                            message = $"Record Updated Successfully...";
                            return new JsonResult { Data = new { status = true, message } };
                        }
                        catch (Exception ex)
                        {
                            return new JsonResult { Data = new { status = false, message = ex.Message } };
                        }
                    }
                    else
                    {
                        model.ProductId = model.ProductId;
                        model.QuantityBeforeOrder = 0;
                        model.QuantityOrder = model.QuantityOrder;
                        model.SalesQuantity = 0;
                        model.CollectedQuantity = 0;
                        model.NotCollectedQuantity = 0;
                        model.CurrentQuantity = 0 + model.QuantityOrder;
                        //model.Visible = model.Visible;

                        _db.Stocks.Add(model);
                        await _db.SaveChangesAsync();
                        message = $"Price Added Successfully.";
                        return new JsonResult { Data = new { status = true, message } };
                    }
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
            var stock = await _db.Stocks.FindAsync(id);
            if (stock != null)
            {
                _db.Stocks.Remove(stock);
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