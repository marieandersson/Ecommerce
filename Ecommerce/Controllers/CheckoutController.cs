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
                var query = "SELECT ProductId FROM Carts WHERE CartId = @cartId";
                var queryParameters = new { cartId = cartId }; 
                Products = connection.Query<CheckoutViewModel>(query, queryParameters).ToList();

            }
                // add customer to database. 
                // get id (last inserted?)
                // get cart id from cookie
                // add order info to db
                // update stock
                // remove from cart db and cookie
                // return thank you for your order page
                // add try and catch - allt måste gå igenom
                return View("ConfirmOrder");
        }

        [HttpGet]
        public ActionResult ConfirmOrder()
        {
           
            return View();
        }
    }
}