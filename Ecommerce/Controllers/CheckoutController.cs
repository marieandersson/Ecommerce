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
            if (Request.Cookies["CartId"] == null)
            {
                return View();
            }
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
            int OrderId;
     
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
                var query = "SELECT Carts.ProductId, Carts.Qty, Products.Price FROM Carts JOIN Products ON Carts.ProductId = Products.Id WHERE Carts.CartId = @cartId";
                var queryParameters = new { cartId = cartId }; 
                Products = connection.Query<CheckoutViewModel>(query, queryParameters).ToList();

                // add order items to database
                foreach (CheckoutViewModel CartItem in Products)
                {
                    var insertOrderItem = "INSERT INTO OrderItems (OrderId, ProductId, Qty, Price) VALUES ((SELECT MAX(Id) FROM Orders), @productId, @qty, @price)";
                    var orderItemParameters = new { productId = CartItem.ProductId, qty = CartItem.Qty, price = CartItem.Price };
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

                // get order id for order confirmation
                var getOrderId = "SELECT MAX(Id) FROM Orders";
                OrderId = connection.QueryFirstOrDefault<int>(getOrderId);
            }

            // delete cart cookie
            var CartCookie = new HttpCookie("CartId");
            CartCookie.Expires = DateTime.Now.AddDays(-1);
            Response.Cookies.Add(CartCookie);

            return RedirectToAction("ConfirmOrder", new { OrderId = OrderId });
        }

        [HttpGet]
        public ActionResult ConfirmOrder(int OrderId)
        {
            List<CheckoutViewModel> order;
            using (var connection = new SqlConnection(this.connectionString))
            {
                var getOrderInfo = "SELECT OrderItems.Qty, OrderItems.Price, Orders.Id, Products.Title, Customers.FirstName FROM OrderItems JOIN Products ON OrderItems.ProductId = Products.Id JOIN Orders ON OrderItems.OrderId = Orders.Id JOIN Customers ON Orders.CustomerId = Customers.Id WHERE OrderItems.OrderId = @orderId";
                var orderIdParameter = new { orderId = OrderId };
                order = connection.Query<CheckoutViewModel>(getOrderInfo, orderIdParameter).ToList();
                ViewBag.CheckoutSum = order.Sum(x => x.Price * x.Qty);
            }
      
            //var viewOrder = "SELECT Orders.Id, OrdersItems.Qty, Products.Title FROM Orders JOIN OrderItems ON Orders.Id = OrderItems.OrderId JOIN Products ON OrdersItems.ProductId = Products.Id WHERE Orders.Id";
            return View(order);
        }
    }
}