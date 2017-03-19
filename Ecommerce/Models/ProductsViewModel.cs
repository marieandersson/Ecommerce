using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Ecommerce.Models
{
    public class ProductsViewModel
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Format { get; set; }
        public string Description { get; set; }
        public string ImgUrl { get; set; }
        public string Price { get; set; }
        public string Stock { get; set; }
        public DateTime Released { get; set; }
    }
}