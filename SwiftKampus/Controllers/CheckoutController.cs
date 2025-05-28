using Newtonsoft.Json;
using SwiftKampus.Controllers;
using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampus.ViewModels.ShoppingCart;
using SwiftKampusModel;
using SwiftKampusModel.MarketPlace;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SwiftKampus.Abstractions.Controllers
{
    public class CheckoutController : BaseController
    {
        // GET: Checkout
        public CheckoutController(SchoolDbContext db) : base(db)
        {

        }
        //
        // GET: /Checkout/AddressAndPayment
        public ActionResult AddressAndPayment()
        {
            return PartialView();
        }
        //
        // POST: /Checkout/AddressAndPayment
        [HttpPost]
        public async Task<ActionResult> AddressAndPayment(CustomerOrderVm values)
        {
            var cart = ShoppingCart.GetCart(this.HttpContext);
            var order = new Order();
            //TryUpdateModel(order);


            var newCustomer = new Customer();

            var Customer = await _db.Customers.Where(c => c.Email.Trim().ToUpper().Equals(values.Email.Trim().ToUpper())).FirstOrDefaultAsync();
            if (Customer != null)
            {
                Customer.Email = values.Email;
                Customer.Fullname = values.Fullname;
                Customer.PhoneNumber = values.PhoneNumber;
                Customer.TransactionDate = DateTime.Now.ToString();

                _db.Entry(Customer).State = EntityState.Modified;
            }
            else
            {
                newCustomer.Email = values.Email;
                newCustomer.Fullname = values.Fullname;
                newCustomer.PhoneNumber = values.PhoneNumber;
                newCustomer.TransactionDate = DateTime.Now.ToString();
                _db.Customers.Add(newCustomer);
                await _db.SaveChangesAsync();

            }

            var justCreatedCustomer = await _db.Customers.Where(c => c.Email.Trim().ToUpper().Equals(values.Email.Trim().ToUpper())).FirstOrDefaultAsync();

            order.CustomerId = justCreatedCustomer.Id;
            order.OrderDate = DateTime.Now.ToString("dd MMM yyyy");
            order.Total = cart.GetTotal();

            //Save Order
            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            
            //Process the order
            var result = cart.CreateOrder(order);
            // Associate shopping cart items with logged-in user
            MigrateShoppingCart(justCreatedCustomer.Email);

            return new JsonResult { Data = new { status = true, id = order.OrderId } };
        }

        public async Task<ActionResult> CheckOutView(int id)
        {
            long milliseconds = DateTime.Now.Ticks;
            var url = Url.Action("ConfrimProuctPayment", "Checkout", new { },
                                   protocol: Request.Url.Scheme);

            var order = await _db.Orders.Include(i => i.Customer).AsNoTracking()
                        .FirstOrDefaultAsync(x => x.OrderId.Equals(id));
            var checkOut = new CheckoutVm()
            {
                Order = order,
                responseurl = url,
                merchantId = RemitaConfigParams.MERCHANTID,
                //OrderId = $"UJID{milliseconds.ToString()}",
                serviceTypeId = RemitaConfigParams.CHANGEOFCOURSE,
                apiKey = RemitaConfigParams.APIKEY,
            };
            checkOut.OrderDetails = await _db.OrderDetails.Include(i => i.Product).AsNoTracking().Where(x => x.OrderId.Equals(order.OrderId)).ToListAsync();

            return View(checkOut);
        }
        //
        // GET: /Checkout/Complete
        public ActionResult Complete(int id)
        {
            // Validate customer owns this order
            bool isValid = _db.Orders.Include(o => o.Customer).Any(
                o => o.OrderId == id &&
                o.Customer.Username == User.Identity.Name);

            if (isValid)
            {
                return View(id);
            }
            else
            {
                return View("Error");
            }
        }

        private void MigrateShoppingCart(string UserName)
        {
            // Associate shopping cart items with logged-in user
            var cart = ShoppingCart.GetCart(this.HttpContext);

            cart.MigrateCart(UserName);
            Session[ShoppingCart.CartSessionKey] = UserName;

        }

       

        public async Task<ActionResult> ProcessPayment(int id)
        {
            var model = new CheckoutVm();

            var order = await _db.Orders.Include(i => i.Customer).Include(i => i.OrderDetails).AsNoTracking()
                                .FirstOrDefaultAsync(x => x.OrderId.Equals(id));
            var hasTransaction = await _db.Transactions.AsNoTracking()
                                        .Where(x => x.OrderId.Equals(order.OrderId)).FirstOrDefaultAsync();

            //model.paymenttype = model.RemitaPaymentType.ToString().Replace("_", " ").ToLower();
            long milliseconds = DateTime.Now.Ticks;
            var url = Url.Action("ConfrimOrderPayment", "Checkout", new { },
                                  protocol: Request.Url.Scheme);

            

            if (hasTransaction != null)
            {
                var hashed = _query.HashRemitedValidate(hasTransaction.TransactionOrderId, RemitaConfigParams.APIKEY, RemitaConfigParams.MERCHANTID);
                string checkurl = RemitaConfigParams.CHECKSTATUSURL + "/" + RemitaConfigParams.MERCHANTID + "/" + hasTransaction.TransactionOrderId + "/" + hashed + "/" + "orderstatus.reg";
                var checkResult = CheckExistingTransaction(checkurl);

                if (checkResult.Item1.Equals(true))
                {
                    if (string.IsNullOrEmpty(checkResult.Item2.Rrr))
                    {
                        var entry = _db.Entry(hasTransaction);
                        if (entry.State == EntityState.Detached)
                            _db.Transactions.Attach(hasTransaction);
                        _db.Transactions.Remove(hasTransaction);
                        _db.SaveChanges();
                    }
                    else
                    {
                        return RedirectToAction("ConfrimOrderPayment", new { orderID = hasTransaction.TransactionOrderId });
                    }
                }
                else
                {
                    ViewBag.ErrorInfo = $"Check your internet connection and try again";
                    ViewBag.ErrorMessage = "Remita is currently unreachable";
                    return View("RemitaErrorPage");
                }
            }
            var marketPlaceTransact = new Transaction
            {
                TransactionOrderId = $"UJID{milliseconds.ToString()}",
                TransactionDate = DateTime.Now.ToString(),
                OrderId = order.OrderId,
                CustomerId = order.CustomerId,
                Amount = Convert.ToDecimal(order.Total),
                ModeOfPayment = model.ModeOfPayment
            };
            _db.Transactions.Add(marketPlaceTransact);
            var log = new RemitaPaymentLog
            {
                OrderId = marketPlaceTransact.TransactionOrderId,
                PaymentName = "MarketPlace Payment",
                PaymentDate = DateTime.Now,
                Amount = order.Total.ToString(),
                PayerName = order.Customer.Fullname
            };
            _db.RemitaPaymentLogs.Add(log);
            await _db.SaveChangesAsync();


            model.payerName = order.Customer.Fullname;
            model.payerEmail = order.Customer.Email;
            model.payerPhone = order.Customer.PhoneNumber;
            model.amt = order.Total.ToString();
            model.merchantId = RemitaConfigParams.MERCHANTID;
            model.orderId = marketPlaceTransact.TransactionOrderId;
            model.responseurl = url;
            model.serviceTypeId = RemitaConfigParams.CHANGEOFCOURSE;
            model.apiKey = RemitaConfigParams.APIKEY;

            model.hash = _query.HashRemitaRequest(model.merchantId, model.serviceTypeId, marketPlaceTransact.TransactionOrderId, model.amt.ToString(), model.responseurl, RemitaConfigParams.APIKEY);
            return RedirectToAction("SubmitRemita", model);
        }

        [AllowAnonymous]
        public async Task<ActionResult> ConfrimOrderPayment(string RRR, string orderID)
        {
            Transaction orderTransaction;
            RemitaResponse result = new RemitaResponse();

            if (string.IsNullOrEmpty(orderID))
            {
                orderTransaction = await _db.Transactions.Include(i => i.Customer)
                                            .Where(x => x.TransactionReferenceNumber.Equals(RRR.Trim()))
                                            .FirstOrDefaultAsync();
            }
            else
            {
                orderTransaction = await _db.Transactions.Include(i => i.Customer)
                                            .Where(x => x.TransactionOrderId.Equals(orderID))
                                            .FirstOrDefaultAsync();
            }
            if (orderTransaction != null)
            {
                //if (orderTransaction.HasMadePayment.Equals(true))
                //{
                //    result.Message = orderTransaction.TransactionMessage;
                //    result.OrderId = orderTransaction.TransactionOrderId;
                //    result.Rrr = orderTransaction.TransactionReferenceNumber;
                //    result.Status = orderTransaction.HasMadePayment.ToString();
                //    return RedirectToAction("ConfrimRrrPayment", "RemitaPaymentLogs", result);
                //}
                var log = await _db.RemitaPaymentLogs.AsNoTracking()
                                    .Where(x => x.OrderId.Equals(orderTransaction.TransactionOrderId))
                                    .FirstOrDefaultAsync();

                var hashed = _query.HashRemitedValidate(orderID, RemitaConfigParams.APIKEY, RemitaConfigParams.MERCHANTID);
                string url = RemitaConfigParams.CHECKSTATUSURL + "/" + RemitaConfigParams.MERCHANTID + "/" + orderID + "/" + hashed + "/" + "orderstatus.reg";
                string jsondata = new WebClient().DownloadString(url);
                result = JsonConvert.DeserializeObject<RemitaResponse>(jsondata);

                if (result.Status.Equals("00") || result.Status.Equals("01"))
                {
                    var appPayment =
                    orderTransaction.TransactionReferenceNumber = result.Rrr;
                    orderTransaction.HasMadePayment = true;
                    orderTransaction.TransactionMessage = result.Message;
                    _db.Entry(orderTransaction).State = EntityState.Modified;

                    _query.UpdateTransactionLog(log, result);

                    //Get order and update product(s) update available Qty
                    var oderDetail = await _db.OrderDetails.Include(o => o.Order)
                                                            .Include(o => o.Product)
                                                            .Where(o => o.OrderId.Equals(orderTransaction.OrderId)).ToListAsync();
                    foreach (var item in oderDetail)
                    {
                        var productStock = await _db.Stocks.Include(s => s.Product).Where(s => s.ProductId.Equals(item.ProductId)).FirstAsync();
                        productStock.CurrentQuantity -= item.Quantity;
                        _db.Entry(productStock).State = EntityState.Modified;
                    }

                    await _db.SaveChangesAsync();
                }
                else
                {
                    orderTransaction.TransactionReferenceNumber = result.Rrr;
                    orderTransaction.HasMadePayment = false;
                    orderTransaction.TransactionMessage = result.Message;
                    _db.Entry(orderTransaction).State = EntityState.Modified;

                    _query.UpdateTransactionLog(log, result);
                    await _db.SaveChangesAsync();
                    return RedirectToAction("RetryProductPayment", new { rrr = result.Rrr });
                }
                return RedirectToAction("CustomerOrders", "MarketPlace", new { id = orderTransaction.Customer.Id});
            }
            var message = $"There is no payment that has either the RRR {RRR} or" +
                          $" Order Id {orderID} for Hostel Application Payment";
            return RedirectToAction("GetPaymentStatus", "RemitaServices", new { /*message*/ });
        }

        public ActionResult SubmitRemita(CheckoutVm model)
        {
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult RetryProductPayment(string rrr)
        {
            var hashrrr = _query.HashRrrQuery(rrr, RemitaConfigParams.APIKEY, RemitaConfigParams.MERCHANTID);
            string posturl = RemitaConfigParams.CHECKSTATUSURL + "/" + RemitaConfigParams.MERCHANTID + "/" + rrr + "/" + hashrrr + "/" + "status.reg";
            string jsondata = new WebClient().DownloadString(posturl);
            var result = JsonConvert.DeserializeObject<RemitaResponse>(jsondata);
            if (result.Status.Equals("00") || result.Status.Equals("01"))
            {
                return RedirectToAction("ConfrimOrderPayment", "Checkout", new { RRR = result.Rrr, orderID = result.OrderId });
            }
            var hash = _query.HashRemitedRePost(RemitaConfigParams.MERCHANTID, rrr, RemitaConfigParams.APIKEY);
            var url = Url.Action("ConfrimOrderPayment", "Checkout", new { },
                                   protocol: Request.Url.Scheme);
            var model = new RemitaRePostVm
            {
                rrr = rrr,
                merchantId = RemitaConfigParams.MERCHANTID,
                hash = hash,
                responseurl = url
            };
            return View(model);
        }
    }
}