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
            string cartId = Request.Cookies["CartId"].Value;
            List<CartViewModel> CartProducts;
            using (var connection = new SqlConnection(this.connectionString))
            {
                var query = "SELECT * FROM Carts JOIN Products ON Carts.ProductId = Products.Id WHERE CartId = @cartId";
                var parameters = new { cartId = cartId };
                CartProducts = connection.Query<CartViewModel>(query, parameters).ToList();
            }
            return View(CartProducts);
        }

        // Add product to Cart
        [HttpPost]
        public ActionResult AddToCart(int productId, int orderQty)
        {
            string cookieCartId;
            // check if customer already has a cart, then use existing cart id and add to same cart
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
                    var insert = "UPDATE Carts SET Qty = Qty +1 WHERE ProductId = @productId AND CartId = @cartId";
                    var parameters = new { cartId = cookieCartId, productId = productId};
                    connection.Execute(insert, parameters);
                }

                
            }
            return RedirectToAction("Index", "Products");
        }

        // Remove product from Cart
        [HttpPost]
        public ActionResult RemoveFromCart(int productId, string cartId)
        {
            using (var connection = new SqlConnection(this.connectionString))
            {
                var remove = "DELETE FROM Carts WHERE CartId = @cartId AND ProductId = productId";
                var parameters = new { productId = productId, cartId = cartId };
                connection.Execute(remove, parameters);
            }

            return RedirectToAction("Index");
        }

    }
}