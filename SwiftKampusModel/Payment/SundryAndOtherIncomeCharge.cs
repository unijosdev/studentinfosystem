using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwiftKampusModel.Payment
{
    public class SundryAndOtherIncomeCharge
    {
        public int Id { get; set; }

        [Display(Name = "Payment's Name")]
        [Required(ErrorMessage = "Payment's Name is required")]
        public string ChargeName { get; set; }

        [Display(Name = "Payment's Description")]
        public string ChargeDescription { get; set; }

        [Display(Name = "Payment's Code eg.IDC")]
        [Required(ErrorMessage = "Payment's Code is required")]
        public string ChargeCode { get; set; }

        [Display(Name = "Payment's Amount")]
        [Required(ErrorMessage = "Payment's Amount is required")]
        public decimal Amount { get; set; }

        [Display(Name = "Payment's Amount in Words")]
        public string AmountInWords { get; set; }

        public bool Status { get; set; }

        public ICollection<SundryAndOtherIncomeChargesPayment> SundryAndOtherIncomeChargesPayments { get; set; }
        public ICollection<ApplicationUser> ApplicationUser { get; set; }
    }
}
