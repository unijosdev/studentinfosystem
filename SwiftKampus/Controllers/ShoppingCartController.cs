using SwiftKampus.Controllers;
using SwiftKampus.Models;
using SwiftKampus.ViewModels.ShoppingCart;
using SwiftKampusModel.MarketPlace;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static SwiftKampus.ViewModels.ShoppingCart.ShoppingCartVm;

namespace SwiftKampus.Abstractions.Controllers
{
    public class ShoppingCartController : BaseController
    {
        public ShoppingCartController(SchoolDbContext db) : base(db)
        {

        }

        //
        // GET: /ShoppingCart/
        public ActionResult Index()
        {
            var cart = ShoppingCart.GetCart(this.HttpContext);

            // Set up our ViewModel
            var viewModel = new ShoppingCartVm
            {
                CartItems = cart.GetCartItems(),
                CartTotal = cart.GetTotal()
            };
            // Return the view
            return PartialView(viewModel);
        }
        //
        // GET: /Store/AddToCart/5
        public ActionResult AddToCart(ProductCartVm productCartVm)
        {
            // Retrieve the album from the database
            //var addedAlbum = _db.Products
            //    .Single(product => product.Id == productCartVm.ProductId);

            // Add it to the shopping cart
            var cart = ShoppingCart.GetCart(this.HttpContext);

            cart.AddToCart(productCartVm);

            TempData["Message"] = "Product(s) Added to your cart successfully!";
            // Go back to the main store page for more shopping
            return RedirectToAction("Index", "MarketPlace");
        }
        //
        // AJAX: /ShoppingCart/RemoveFromCart/5
        [HttpPost]
        public ActionResult RemoveFromCart(int id)
        {
            // Remove the item from the cart
            var cart = ShoppingCart.GetCart(this.HttpContext);

            // Get the name of the album to display confirmation
            string productName = _db.Carts.Include(c => c.Produt)
                .Single(item => item.RecordId == id).Produt.ProductName;

            // Remove from cart
            int itemCount = cart.RemoveFromCart(id);

            // Display the confirmation message
            var results = new ShoppingCartRemoveViewModel
            {
                Message = Server.HtmlEncode(productName) +
                    " has been removed from your shopping cart.",
                CartTotal = cart.GetTotal(),
                CartCount = cart.GetCount(),
                ItemCount = itemCount,
                DeleteId = id
            };
            return Json(results);
        }

        // AJAX: /ShoppingCart/RemoveFromCart/5
        //[HttpPost]
        //public ActionResult IssueItem(int id)
        //{
        //    // Remove the item from the cart
        //    var orderTrasact = _db.OrderDetails.Include(o => o.Order.Transactions)
        //                                       .Where(o => o.OrderDetailId.Equals(id)).FirstOrDefault();
        //    orderTrasact.

        //    // Get the name of the album to display confirmation
        //    string productName = _db.Carts.Include(c => c.Produt)
        //        .Single(item => item.RecordId == id).Produt.ProductName;

        //    // Remove from cart
        //    int itemCount = cart.RemoveFromCart(id);

        //    // Display the confirmation message
        //    var results = new ShoppingCartRemoveViewModel
        //    {
        //        Message = Server.HtmlEncode(productName) +
        //            " has been removed from your shopping cart.",
        //        CartTotal = cart.GetTotal(),
        //        CartCount = cart.GetCount(),
        //        ItemCount = itemCount,
        //        DeleteId = id
        //    };
        //    return Json(results);
        //}
        //
        // GET: /ShoppingCart/CartSummary
        [ChildActionOnly]
        public ActionResult CartSummary()
        {
            var cart = ShoppingCart.GetCart(this.HttpContext);

            ViewData["CartCount"] = cart.GetCount();
            return PartialView("CartSummary");
        }
    }
}