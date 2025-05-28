using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SwiftKampus.ViewModels.ShoppingCart
{
    public class CustomerOrderVm
    {
        [Required]
        [Display(Name = "Customer's Address")]
        public string UserName { get; set; }

        [Display(Name = "Fullname")]
        [Required]
        public string Fullname { get; set; }
        [Required]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Total Amount")]
        public string CheckoutTotalAmount { get; set; }
        public System.DateTime OrderDate { get; set; }
    }
}