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
        [HttpGet]
        public ActionResult Index()
        {
            if (this.Session["login"] == null)
            {
                return View();
            }
            return RedirectToAction("AddProduct");
        }

        [HttpPost]
        public ActionResult Index(AdminViewModel model)
        {
            AdminViewModel admin;
            try
            {
                using (var connection = new SqlConnection(this.connectionString))
                {
                    var getAdmin = "SELECT * FROM Admins WHERE username = @username AND password = @password";
                    var adminParameters = new { username = model.Username, password = model.Password };
                    admin = connection.QuerySingleOrDefault<AdminViewModel>(getAdmin, adminParameters);
                }

                if (admin != null)
                {
                    this.Session["login"] = model.Username;
                    return RedirectToAction("AddProduct");
                }
                return View();
            } 
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpGet]
        public ActionResult AddProduct()
        {
            if (this.Session["login"] == null)
            {
                return RedirectToAction("Index");
            }
            return View();
        }

        [HttpPost]
        public ActionResult AddProduct(ProductsViewModel model)
        {
            try
            {
                using (var connection = new SqlConnection(this.connectionString))
                {
                    var insert = "INSERT INTO products (Title, Artist, Format, Description, ImgUrl, Price, Stock, Released) VALUES (@title, @artist, @format, @description, @imgurl, @price, @stock, @released)";
                    var parameters = new { title = model.Title, artist = model.Artist, format = model.Format, description = model.Description, imgurl = model.ImgUrl, price = model.Price, stock = model.Stock, released = model.Released };
                    connection.Execute(insert, parameters);
                }
                return RedirectToAction("AddProduct");
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpPost]
        public ActionResult Logout()
        {
            Session.Remove("login");
            return RedirectToAction("Index");
        }
    }
}