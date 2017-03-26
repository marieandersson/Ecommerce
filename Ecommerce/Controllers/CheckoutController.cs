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
        public ActionResult submitOrder(CheckoutViewModel model)
        {
            return View();
        }
    }
}