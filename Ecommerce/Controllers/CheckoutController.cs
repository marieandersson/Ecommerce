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
            int orderId;
     
            using (var connection = new SqlConnection(this.connectionString))
            {
                // Start a transaction
                connection.Open();
                SqlCommand command = connection.CreateCommand();
                SqlTransaction transaction;
                transaction = connection.BeginTransaction("checkoutTransaction");
                command.Connection = connection;
                command.Transaction = transaction;
                try
                {
                    // add customer to database
                    command.CommandText = "INSERT INTO Customers (FirstName, LastName, Email, Phone, Street, PostalCode, City) VALUES (@firstName, @lastName, @email, @phone, @street, @postalCode, @city)";
                    command.Parameters.AddWithValue("@firstName", firstName);
                    command.Parameters.AddWithValue("@lastName", lastName);
                    command.Parameters.AddWithValue("@email", email);
                    command.Parameters.AddWithValue("@phone", phone);
                    command.Parameters.AddWithValue("@street", street);
                    command.Parameters.AddWithValue("@postalCode", postalCode);
                    command.Parameters.AddWithValue("@city", city);
                    command.ExecuteNonQuery();

                    // add order to database
                    command.CommandText = "INSERT INTO Orders (CustomerId, OrderDate, OrderStatus) VALUES ((SELECT MAX(Id) FROM Customers), GETDATE(), 'new')";
                    command.ExecuteNonQuery();

                    // get products from cart
                    var products = new List<CheckoutViewModel>();
                    command.CommandText = "SELECT Carts.ProductId, Carts.Qty, Products.Price FROM Carts JOIN Products ON Carts.ProductId = Products.Id WHERE Carts.CartId = @cartId";
                    command.Parameters.AddWithValue("@cartId", cartId);
                    SqlDataReader productsReader = command.ExecuteReader();
                    while (productsReader.Read())
                    {
                        var product = new CheckoutViewModel();
                        product.ProductId = productsReader["ProductId"].ToString();
                        product.Qty = Int32.Parse(productsReader["Qty"].ToString());
                        product.Price = float.Parse(productsReader["Price"].ToString());
                        products.Add(product);
                  
                    }
                    productsReader.Close();

                    // add order items to database
                    foreach (CheckoutViewModel CartItem in products)
                    {
                        command.CommandText = "INSERT INTO OrderItems (OrderId, ProductId, Qty, Price) VALUES ((SELECT MAX(Id) FROM Orders), @productId, @qty, @price)";
                        command.Parameters.AddWithValue("@productId", CartItem.ProductId);
                        command.Parameters.AddWithValue("@qty", CartItem.Qty);
                        command.Parameters.AddWithValue("@price", CartItem.Price);
                        command.ExecuteNonQuery();

                        // update product stock              
                        command.CommandText = "UPDATE Products SET Stock = Stock - @soldQty WHERE Id = @productStockId";
                        command.Parameters.AddWithValue("@soldQty", CartItem.Qty);
                        command.Parameters.AddWithValue("@productStockId", CartItem.ProductId);
                        command.ExecuteNonQuery();
                    }
                    // remove cart from database
                    command.CommandText = "DELETE FROM Carts WHERE CartId = @cartIdToDelete";
                    command.Parameters.AddWithValue("@cartIdToDelete", cartId);
                    command.ExecuteNonQuery();

                    // get order id for order confirmation
                    command.CommandText = "SELECT Id = MAX(Id) FROM Orders";
                    orderId = Convert.ToInt32(command.ExecuteScalar());
                        

                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    return View("Error");
                }
            }

   
            // delete cart cookie
            var CartCookie = new HttpCookie("CartId");
            CartCookie.Expires = DateTime.Now.AddDays(-1);
            Response.Cookies.Add(CartCookie);

            return RedirectToAction("ConfirmOrder", new { OrderId = orderId });
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
            }

            return View(order);
        }

        public ActionResult Error()
        {
            return View();
        } 
    }
}