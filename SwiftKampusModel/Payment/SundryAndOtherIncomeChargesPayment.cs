using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwiftKampusModel.Payment
{
    public class SundryAndOtherIncomeChargesPayment
    {
        public int Id { get; set; }

        [Display(Name = "Payment's Name")]
        [Required(ErrorMessage = "Payment's Name is required")]
        public int SundryAndOtherIncomeChargeId { get; set; }

        public string ApplicationUserId { get; set; }

        public string ReferenceNo { get; set; }

        [Index(IsUnique = true)]
        [MaxLength(50)]
        [Required]
        public string OrderId { get; set; }

        public string  PaymentDate { get; set; }

        public decimal  AmountPayment { get; set; }

        public bool   PaymentStatus { get; set; }

        public bool IsPayed { get; set; }

        public bool IsProcessed { get; set; }


        public SundryAndOtherIncomeCharge SundryAndOtherIncomeCharges { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
    }
}
