using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Ecommerce.Models
{
    public class CartViewModel
    {
        public string CartId { get; set; }
        public int Qty { get; set; }

        public string Id { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public string ImgUrl { get; set; }
        public float Price { get; set; }
        public string Stock { get; set; }
        
        public float Sum { get; set; }
    }
}