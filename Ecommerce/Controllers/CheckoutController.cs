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
    public class CheckoutController : Controller
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["EcommerceDatabase"].ConnectionString;
        
        // GET: Checkout
        [HttpGet]
        public ActionResult Index()
        {
            string cartId = Request.Cookies["CartId"].Value;
            List<CheckoutViewModel> CheckoutProducts;
            using (var connection = new SqlConnection(this.connectionString))
            {
                var query = "SELECT Products.Id, Products.Title, Products.Price, Carts.Qty FROM Carts JOIN Products ON Carts.ProductId = Products.Id WHERE CartId = @cartId";
                var parameters = new { cartId = cartId };
                CheckoutProducts = connection.Query<CheckoutViewModel>(query, parameters).ToList();
                ViewBag.Sum = CheckoutProducts.Sum(x => x.Price * x.Qty);
            }
            return View(CheckoutProducts);
          
        }

        [HttpPost]
        public ActionResult SubmitOrder(string firstName, string lastName, string email, string phone, string street, string postalCode, string city)
        {
            var cartId = Request.Cookies["CartId"].Value;   
            List<CheckoutViewModel> Products;
     
            using (var connection = new SqlConnection(this.connectionString))
            {
                // add customer to database
                var insertCustomer = "INSERT INTO Customers (FirstName, LastName, Email, Phone, Street, PostalCode, City) VALUES (@firstName, @lastName, @email, @phone, @street, @postalCode, @city)";
                var customerParameters = new { firstName = firstName, lastName = lastName, email = email, phone = phone, street = street, postalCode = postalCode, city = city };
                connection.Execute(insertCustomer, customerParameters);

                // add order to database
                var insertOrder = "INSERT INTO Orders (CustomerId, OrderDate, OrderStatus) VALUES ((SELECT MAX(Id) FROM Customers), GETDATE(), 'new')";
                connection.Execute(insertOrder);

                // get products from cart
                var query = "SELECT ProductId, Qty FROM Carts WHERE CartId = @cartId";
                var queryParameters = new { cartId = cartId }; 
                Products = connection.Query<CheckoutViewModel>(query, queryParameters).ToList();

                // add order items to database
                foreach (CheckoutViewModel CartItem in Products)
                {
                    var insertOrderItem = "INSERT INTO OrderItems (OrderId, ProductId, Qty) VALUES ((SELECT MAX(Id) FROM Orders), @productId, @qty)";
                    var orderItemParameters = new { productId = CartItem.ProductId, qty = CartItem.Qty };
                    connection.Execute(insertOrderItem, orderItemParameters);

                    // update product stock
                    var updateStock = "UPDATE Products SET Stock = Stock - @soldQty WHERE Id = @productId";
                    var stockParameters = new { soldQty = CartItem.Qty, productId = CartItem.ProductId };
                    connection.Execute(updateStock, stockParameters);
                }

                // remove cart from database
                var deleteCart = "DELETE FROM Carts WHERE CartId = @cartId";
                var cartIdParameter = new { cartId = cartId };
                connection.Execute(deleteCart, cartIdParameter);
            }

            // delete cart cookie
            var CartCookie = new HttpCookie("CartId");
            CartCookie.Expires = DateTime.Now.AddDays(-1);
            Response.Cookies.Add(CartCookie);

            return View("ConfirmOrder");
        }

        [HttpGet]
        public ActionResult ConfirmOrder()
        {
            // how to only show after placing order?
            //var viewOrder = "SELECT Orders.Id, OrdersItems.Qty, Products.Title FROM Orders JOIN OrderItems ON Orders.Id = OrderItems.OrderId JOIN Products ON OrdersItems.ProductId = Products.Id WHERE Orders.Id";
            return View();
        }
    }
}