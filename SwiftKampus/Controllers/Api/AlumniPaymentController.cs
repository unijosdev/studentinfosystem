using SwiftKampus.Models;
using SwiftKampusModel;
using System;
using System.Linq;
using System.Web.Http;
using SwiftKampusModel.MarketPlace;
using System.Data.Entity;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Windows;
using System.Net;

namespace SwiftKampus.Controllers.Api
{
    public class AlumniPaymentController : ApiController
    {
        public readonly SchoolDbContext _db;

        public AlumniPaymentController(SchoolDbContext db)
        {
            _db = db;
        }

        //GET /api/AlumniPayment/create
        // create the payment, then redirect to \Controllers\AlumniPaymentController
        [Route("api/AlumniPayment/create")]
        [HttpGet, HttpPost]
        public IHttpActionResult Create(string email, string productName, string payerName, string phoneNumber)
        {
            var transactions = _db.Transactions.Include(x => x.Customer).Include(x => x.Order).Include(x => x.Order).Where(x => x.Customer.Email == email).FirstOrDefault();
            var customer = _db.Customers.Where(x => x.Email == email).FirstOrDefault();
            var product = _db.Products.Where(x => x.ProductName.Trim().ToUpper() == productName.Trim().ToUpper()).FirstOrDefault();
            long milliseconds = DateTime.Now.Ticks;

            if (transactions == null)
            {
                if (customer == null)
                {
                    var alumniDetails = new Customer
                    {
                        Fullname = payerName,
                        Email = email,
                        PhoneNumber = phoneNumber,
                        TransactionDate = DateTime.Now.ToString(),
                    };
                    _db.Customers.Add(alumniDetails);
                }
                
                //_db.SaveChanges();

                var customerId = _db.Customers.Where(x => x.Email == email).Select(x => x.Id).FirstOrDefault();
                var order = new Order
                {
                    CustomerId = customerId,
                    OrderDate = DateTime.Now.ToString(),
                    Total = product.ProductPrice,
                };
                _db.Orders.Add(order);
                _db.SaveChanges();

                var orderId = _db.Orders.Where(x => x.CustomerId == customerId).Select(x => x.OrderId).FirstOrDefault();
                var orderDetails = new OrderDetail
                {
                    OrderId = orderId,
                    ProductId = product.Id,
                    Quantity = 1,
                    UnitPrice = product.ProductPrice
                };
                _db.OrderDetails.Add(orderDetails);
                _db.SaveChanges();

                var transaction = new Transaction
                {
                    TransactionOrderId = $"UJID{milliseconds.ToString()}",
                    OrderId = orderId,
                    Amount = product.ProductPrice,
                    TransactionDate = DateTime.Now.ToString(),
                    CustomerId = customerId,
                };

                _db.Transactions.Add(transaction);

                var log = new RemitaPaymentLog
                {
                    OrderId = orderId.ToString(),
                    PaymentName = "Transcript Payment",
                    PaymentDate = DateTime.Now,
                    Amount = product.ProductPrice.ToString(),
                    PayerName = payerName
                };
                _db.RemitaPaymentLogs.Add(log);
                _db.SaveChanges();

                //return Redirect(new Uri("http://localhost:4084/SchoolFeePayments/SubmitRemita", UriKind.Absolute));
                var transactionId = _db.Transactions.Include(x => x.Customer).Where(x => x.Customer.Email == email).Select(x => x.Id).FirstOrDefault();

                return RedirectToRoute("Default", new
                {
                    controller = "AlumniPayment",
                    action = "Index",
                    id = transactionId
                });

            }
            else if (transactions != null && transactions.HasMadePayment == true)
            {
                return Ok("Payment is already successfull for this user");
            }
            else
            {
                var transactionId = _db.Transactions.Include(x => x.Customer).Where(x => x.Customer.Email == email).Select(x => x.Id).FirstOrDefault();

                return RedirectToRoute("Default", new
                {
                    controller = "AlumniPayment",
                    action = "Index",
                    id = transactionId
                });
            }
        }

        // Returns the payment response after it has returned from remita
        [HttpGet]
        public HttpResponseMessage PaymentResponse(string paymentStatus, string rrr, string paymentDate, double amount, string email)
        {
            string URL = "http://convocation.unijos.edu.ng/api/getPaymentParameters";
            string urlParameters = "?email=" + email + "&refNumber=" + rrr + "&statusMsg=" + paymentStatus + "&paymentDate=" + paymentDate + "&amount=" + amount;

            var response = Request.CreateResponse(HttpStatusCode.Moved);
            response.Headers.Location = new Uri(URL + urlParameters);
            return response;

            //HttpClient client = new HttpClient();
            //client.BaseAddress = new Uri(URL);

            //// Add an Accept header for JSON format.
            //client.DefaultRequestHeaders.Accept.Add(
            //new MediaTypeWithQualityHeaderValue("application/json"));

            //// List data response.
            //HttpResponseMessage response = client.GetAsync(urlParameters).Result;  // Blocking call! Program will wait here until a response is received or a timeout occurs.
            //if (response.IsSuccessStatusCode)
            //{
            //    // Parse the response body.
            //    return Ok(new { paymentStatus, rrr, paymentDate });

            //}
            //else
            //{
            //    return Ok(new { response.ReasonPhrase });
            //}

            //// Make any other calls using HttpClient here.

            //// Dispose once all HttpClient calls are complete. This is not necessary if the containing object will be disposed of; for example in this case the HttpClient instance will be disposed automatically when the application terminates so the following call is superfluous.
            //client.Dispose();

        }

        // Returns a message when no transcript payment was found for the alumnus
        [HttpGet]
        public IHttpActionResult NoTranscriptPayment(string message)
        {
            return Ok(message);
        }
    }
}
