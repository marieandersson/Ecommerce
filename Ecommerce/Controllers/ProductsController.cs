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
    public class ProductsController : Controller
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["EcommerceDatabase"].ConnectionString;
        // GET: Products
        [HttpGet]
        public ActionResult Index()
        {
            List<ProductsViewModel> Products;
            try
            {
                using (var connection = new SqlConnection(this.connectionString))
                {
                    Products = connection.Query<ProductsViewModel>("select * from Products").ToList();
                }
                return View(Products);
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }

        public ActionResult Details(string id)
        {
            ProductsViewModel singleProduct;
            try
            {
                using (var connection = new SqlConnection(this.connectionString))
                {
                    var query = "select * from Products where id = @productId";
                    var parameters = new { productId = id };
                    singleProduct = connection.QuerySingleOrDefault<ProductsViewModel>(query, parameters);
                }
                if (singleProduct == null)
                {
                    return HttpNotFound();
                }
                return View(singleProduct);
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }

    }

}