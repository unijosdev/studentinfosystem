using SwiftKampus.Controllers;
using SwiftKampus.Models;
using SwiftKampus.ViewModels.ShoppingCart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Data.Entity;
using System.Web.Mvc;
using SwiftKampusModel.MarketPlace;
using SwiftKampus.ViewModels;

namespace SwiftKampus.Abstractions.Controllers
{
    public class MarketPlaceController : BaseController
    {
        private readonly ShoppingCart _shoppingCart;
        public MarketPlaceController(SchoolDbContext db) : base(db)
        {
            _shoppingCart = new ShoppingCart();
        }
        // GET: MarketPlace
        public async Task<ActionResult> Index()
        {
            ViewBag.ProductCategoryId = new SelectList(_db.ProductCategories.AsNoTracking(), "Id", "CategoryName");
            return View();
        }

        public async Task<ActionResult> GetIndex(int? categoryId)
        {
            // dc.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var data = new List<ProductCartVm>();

            if (categoryId == null)
            {
                data = await _db.Stocks.Include(p => p.Product).Select(x => new ProductCartVm
                {
                    ProductName = x.Product.ProductName,
                    Price = x.Product.ProductPrice,
                    ProductId = x.Product.Id,
                    availableQty = x.CurrentQuantity
                }).OrderByDescending(x => x.ProductId).ToListAsync();
            }
            else
            {
                data = await _db.Stocks.Include(p => p.Product).Include(p => p.Product.ProductCategory)
                                              .Where(x => x.Product.ProductCategory.Id.Equals((int)categoryId))
                                              .Select(x => new ProductCartVm
                                              {
                                                  ProductName = x.Product.ProductName,
                                                  Price = x.Product.ProductPrice,
                                                  ProductId = x.Product.Id,
                                                  availableQty = x.CurrentQuantity
                                              }).OrderByDescending(x => x.ProductId).ToListAsync();
            }

            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }
        public async Task<ActionResult> Graduants()
        {
            var products = await _db.Stocks.Include(p => p.Product).Select(x => new ProductCartVm
            {
                ProductName = x.Product.ProductName,
                Price = x.Product.ProductPrice,
                ProductId = x.Product.Id,
                availableQty = x.CurrentQuantity
            }).ToListAsync();

            return View(products);
        }

        public ActionResult CustomerOrders(int? id)
        {
            ViewBag.OrderId = id;
            return View();
        }
        public async Task<ActionResult> GetCustomerOrder(string id, int? customerId)
        {
            if (!string.IsNullOrEmpty(id))
            {
                id = id.Trim().ToUpper();
                var products = await _db.Orders.Include(p => p.Customer).Include(p => p.OrderDetails)
                    .Include(i => i.Transactions).Where(x => x.Customer.Email.Trim().ToUpper().Equals(id)
                    || x.Customer.PhoneNumber.Trim().ToUpper().Equals(id)).ToListAsync();
                
                return View(products);
            }
            else if(customerId != null)
            {
                var products = await _db.Orders.Include(p => p.Customer).Include(p => p.OrderDetails)
                    .Include(i => i.Transactions).Where(x => x.Customer.Id.Equals((int)customerId)).ToListAsync();

                return View(products);
            }
            else
            {
                ViewBag.Message = "Please type a valid email or password";
                return View();
            }          
           
        }

        public async Task<ActionResult> DeleteCustomerOrder(int orderId)
        {
            var product = await _db.Orders.Where(x => x.OrderId.Equals((int)orderId)).FirstOrDefaultAsync();

            _db.Entry(product).State = EntityState.Deleted;
            _db.Orders.Remove(product);
            _db.SaveChanges();

            return new JsonResult { Data = new { status = true, message = "Order has been deleted successfully" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public async Task<ActionResult> ProductDetail(int id)
        {
            var product = await _db.Products.Include(p => p.StockOrders).Where(p => p.Id == id).FirstOrDefaultAsync();
            var productCartView = new ProductCartVm
            {
                ProductId = product.Id,
                ProductName = product.ProductName,
                Price = product.ProductPrice,
                availableQty =  _db.Stocks.Include(s => s.Product).Where(x=>x.ProductId.Equals(id)).Select(x => x.CurrentQuantity).FirstOrDefault(),
            };
            return PartialView(productCartView);
        }

        public JsonResult AddToCart(ProductCartVm product)
        {
            var availableQuantity = _db.Stocks.Include(s => s.Product).Where(x => x.ProductId.Equals(product.ProductId))
                                         .Select(x => x.CurrentQuantity).FirstOrDefault();
            if(availableQuantity >= product.CustomerOrderedCount)
            {
                var cart = ShoppingCart.GetCart(this.HttpContext);
                cart.AddToCart(product);
                return new JsonResult { Data = new { status = true, message = "Product has been added to cart successfully" } };
            }
            else
            {
                return new JsonResult { Data = new { status = false, message = $"Oops , Quantity selected not available; we have only {availableQuantity} quantity left in stock." } };
            }           
        }

        public ActionResult CartSummary()
        {
            var cart = ShoppingCart.GetCart(this.HttpContext);
            return PartialView(cart.GetCartItems());
        }

        public ActionResult showOrderDetail(int id)
        {
            var orderDetail = _db.OrderDetails.Include(o => o.Product).AsNoTracking()
                                                    .Include(o => o.Order)
                                                    .Include(o => o.Order.Customer)
                                                    .Where(x => x.OrderId.Equals(id)).ToList();
            return PartialView(orderDetail);
        }

        public ActionResult setCollected(int id)
        {
            var transact = _db.Transactions.Include(o => o.Order)
                                           .Include(o => o.Customer)
                                                    .Where(x => x.OrderId.Equals(id)).ToList();
            Delivery deliver = new Delivery();

            deliver.DateRecieved = DateTime.Today.ToString();
            deliver.TransactionId = transact.First().Id;
            deliver.Recipient = transact.First().Order.Customer.Fullname;
            deliver.UserId = 1111; //change to userId but first change deliver.userId to string and run migration

            _db.Deliveries.Add(deliver);
            _db.SaveChanges();

            foreach (var item in transact)
            {
                item.DeliverId = deliver.Id;
                _db.Entry(item).State = EntityState.Modified;

            }

            _db.SaveChanges();
            var message = $"Item(s) Issued Succesfully";
        return Json(message, JsonRequestBehavior.AllowGet);
    }

        public ActionResult TransactionList()
        {
            {
                ViewBag.CategoryId = new SelectList(_db.ProductCategories.AsNoTracking(), "Id", "CategoryName");               
                return View();
            }
        }

        public async Task<ActionResult> getTransactionList(bool Status, int CategoryId)
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

            var transactionIndex = new List<PickUpsVm>();
            bool activetransaction = false;
            //if (!string.IsNullOrEmpty(hasRegistered) && hasRegistered.Equals("True"))
            //{
            //    activetransaction = true;
            //}

            var tansactionList = await GetAllTransactionList(CategoryId, Status);


            totalRecords = tansactionList.Count();
            var data = tansactionList.Skip(skip).Take(pageSize).ToList();

            return Json(new { draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data },
                JsonRequestBehavior.AllowGet);

            #endregion Server Side filtering
        }

        public async Task<List<PickUpsVm>> GetAllTransactionList(int? CategoryId, bool Status)
        {
            //var model;
            var pickList = new List<PickUpsVm>();
            if (Status == null)
            {
                var OrdersTransaction = await _db.Transactions.Include(t => t.Customer).AsNoTracking()
                                                              .Include(t => t.Order)
                                                   .Include(t => t.Order.OrderDetails).ToListAsync();


                foreach (var item in OrdersTransaction)
                {
                    var pickUp = new PickUpsVm()
                    {
                        TransactionId = item.Id,
                        CustomerName = item.Customer.Fullname,
                        Phone = item.Customer.PhoneNumber,
                        ProductCount = item.Order.OrderDetails.Count(),
                        ProductPrice = item.Amount,
                        TransactionReference = item.TransactionReferenceNumber,
                        IssueStatus = item.DeliverId == 1 ? true : false,
                        OrderId = item.OrderId,

                    };
                    pickList.Add(pickUp);
                }

                return pickList;

            }
            else
            {
                var OrdersTransaction = await _db.Transactions.Include(t => t.Customer).AsNoTracking()
                                                   .Include(t => t.Order.OrderDetails)
                                                   .Include(t => t.Order)
                                                   .Where(t => t.HasMadePayment.Equals(Status)).ToListAsync();


                foreach (var item in OrdersTransaction)
                {
                    var pickUp = new PickUpsVm()
                    {
                        TransactionId = item.Id,
                        CustomerName = item.Customer.Fullname,
                        Phone = item.Customer.PhoneNumber,
                        ProductCount = item.Order.OrderDetails.Count(),
                        ProductPrice = item.Amount,
                        TransactionReference = item.TransactionReferenceNumber,
                        IssueStatus = item.DeliverId > 0 ? true : false,
                        OrderId = item.OrderId,

                    };
                    pickList.Add(pickUp);
                }

                return pickList;
            }
        }
    }
}