using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Ecommerce.Models
{
    public class CheckoutViewModel
    {
        // order info
        public string OrderId { get; set; }
        public string ProductId { get; set; }
        public string Title { get; set; }
        public int Qty { get; set; }
        public float Price { get; set; }
        public float Sum { get; set; }
        // customer info
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Street { get; set; }
        public string PostalCode { get; set; }
        public string City { get; set; }
        
    }
}