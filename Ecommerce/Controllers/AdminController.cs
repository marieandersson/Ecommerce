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
    public class AdminController : Controller
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["EcommerceDatabase"].ConnectionString;
        // GET: Admin
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult AddProduct()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddProduct(ProductsViewModel model)
        {
            using (var connection = new SqlConnection(this.connectionString))
            {
                var insert = "INSERT INTO products (Title, Artist, Format, Description, ImgUrl, Price, Stock, Released) VALUES (@title, @artist, @format, @description, @imgurl, @price, @stock, @released)";
                var parameters = new { title = model.Title, artist = model.Artist, format = model.Format, description = model.Description, imgurl = model.ImgUrl, price = model.Price, stock = model.Stock, released = model.Released };
                connection.Execute(insert, parameters);
            }
            return View();
        }
    }
}