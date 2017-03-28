using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Ecommerce.Models;
using Dapper;
using System.Configuration;
using System.Data.SqlClient;


namespace Ecommerce.Controllers
{
    public class CartController : Controller
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["EcommerceDatabase"].ConnectionString;
        // GET: Cart
        [HttpGet]
        public ActionResult Index()
        {
            if (Request.Cookies["CartId"] != null)
            {
                string cartId = Request.Cookies["CartId"].Value;
                List<CartViewModel> CartProducts;
                using (var connection = new SqlConnection(this.connectionString))
                {
                    var query = "SELECT * FROM Carts JOIN Products ON Carts.ProductId = Products.Id WHERE CartId = @cartId";
                    var parameters = new { cartId = cartId };
                    CartProducts = connection.Query<CartViewModel>(query, parameters).ToList();
                    ViewBag.CartSum = CartProducts.Sum(x => x.Price * x.Qty);
                }
                return View(CartProducts);
            }
            return View();
        }

        // Add product to Cart
        [HttpPost]
        public ActionResult AddToCart(int productId, int orderQty)
        {
            string cookieCartId;
            // check if customer already has a cart
            if (Request.Cookies["CartId"] != null)
            {
                cookieCartId = Request.Cookies["CartId"].Value;
            }
            // if no existing cart, create one
            else
            {
                cookieCartId = Guid.NewGuid().ToString();
                // save cart id in cookie
                HttpCookie ecommerceCookie = new HttpCookie("CartId");
                ecommerceCookie.Value = cookieCartId;
                ecommerceCookie.Expires = DateTime.Now.AddDays(14d);
                Response.Cookies.Add(ecommerceCookie);
            }

            using (var connection = new SqlConnection(this.connectionString))
            {
                var checkStock = "SELECT Stock FROM Products WHERE Id = @productId";
                var parameters = new { productId = productId };
                var stock = connection.QuerySingleOrDefault<int>(checkStock, parameters);

                if (stock < orderQty)
                {
                    // error msg not only "qty" in stock, change value
                    return Redirect(Request.UrlReferrer.AbsolutePath);
                }
            }



            using (var connection = new SqlConnection(this.connectionString))
            {
                var query = "SELECT Qty FROM Carts WHERE ProductId = @productId AND CartId = @cartId";
                var p = new { productId = productId, cartId = cookieCartId };
                var existingProduct = connection.QuerySingleOrDefault<ProductsViewModel>(query, p);

                if (existingProduct == null)
                {
                    var insert = "INSERT INTO Carts (CartId, ProductId, Qty) VALUES (@cartId, @productId, @qty)";
                    var parameters = new { cartId = cookieCartId, productId = productId, qty = orderQty };
                    connection.Execute(insert, parameters);
                }
                else
                {
                    var update = "UPDATE Carts SET Qty = Qty +1 WHERE ProductId = @productId AND CartId = @cartId";
                    var parameters = new { cartId = cookieCartId, productId = productId };
                    connection.Execute(update, parameters);
                }


            }
            return Redirect(Request.UrlReferrer.AbsolutePath);
        }

        // Remove product from Cart
        [HttpPost]
        public ActionResult RemoveFromCart(int productId, string cartId)
        {
            using (var connection = new SqlConnection(this.connectionString))
            {
                var remove = "DELETE FROM Carts WHERE CartId = @cartId AND ProductId = @productId";
                var parameters = new { productId = productId, cartId = cartId };
                connection.Execute(remove, parameters);
            }

            return RedirectToAction("Index");
        }

        // Update Quantity on Cart View
        [HttpPost]
        public ActionResult UpdateProductQty(int productId, string cartId, int qty)
        {
            using (var connection = new SqlConnection(this.connectionString))
            {
                var update = "UPDATE Carts SET Qty = @qty WHERE CartId = @cartId AND ProductId = @productId";
                var parameters = new { qty = qty, productId = productId, cartId = cartId };
                connection.Execute(update, parameters);
            }
            return RedirectToAction("Index");
        }
    }
}