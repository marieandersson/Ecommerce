using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Ecommerce.Models
{
    public class CheckoutViewModel
    {
        // order info
        public int OrderId { get; set; }      
        public string Title { get; set; }
        public string OrderStatus { get; set; }
        // order item info
        public string ProductId { get; set; }
        public float Price { get; set; }
        public int Qty { get; set; }
        // customer info
        public int CustomerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Street { get; set; }
        public string PostalCode { get; set; }
        public string City { get; set; }
        
    }
}