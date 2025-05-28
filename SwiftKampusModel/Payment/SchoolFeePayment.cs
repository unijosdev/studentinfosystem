using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwiftKampusModel.Payment
{
    public class SchoolFeePayment
    {
        public int SchoolFeePaymentId { get; set; }

        [Display(Name = "Student's Name")]
        [Required(ErrorMessage = "Student's Name is required")]
        public string StudentId { get; set; }
        public int? LevelId { get; set; }

        public string ReferenceNo { get; set; }

        [Index(IsUnique = true)]
        [MaxLength(50)]
        [Required]
        public string OrderId { get; set; }

        [Display(Name = "Fees Type")]
        public string FeeCategory { get; set; }

        [Display(Name = "Semester")]
        [Required(ErrorMessage = "Semester is required")]
        public int SemesterId { get; set; }

        [Display(Name = "Session")]
        [Required(ErrorMessage = "Session is required")]
        public int SessionId { get; set; }

        [Display(Name = "Amount Paid")]
        public decimal PaidFee { get; set; }


        [Display(Name = "Total Amount")]
        public decimal TotalAmount { get; set; }

        [Display(Name = "Payment Method")]
        public PMode PaymentMode { get; set; }

        [Display(Name = "Date of Payment")]
        public DateTime Date { get; set; }

        [Display(Name = "Fee Status")]
        public bool Status { get; set; }
        public bool IsPartPaymet { get; set; }
        public bool IsFullPayment { get; set; }
        public string PaymentStatus { get; set; }
        public decimal RemainingBalance
        {
            get
            {
                return TotalAmount - PaidFee;
            }
        }
        public Student Students { get; set; }
        public Semester Semester { get; set; }
        public Session Session { get; set; }
        public Level Level { get; set; }

    }


    public class FacultyFeePayment
    {
        public int FacultyFeePaymentId { get; set; }

        [Display(Name = "Student's Name")]
        [Required(ErrorMessage = "Student's Name is required")]
        public string StudentId { get; set; }
        public string ReferenceNo { get; set; }

        [Index(IsUnique = true)]
        [MaxLength(50)]
        [Required]
        public string OrderId { get; set; }

        [Display(Name = "Faculty Id")]
        public int FacultyId { get; set; }

        [Display(Name = "Semester")]
        [Required(ErrorMessage = "Semester is required")]
        public int SemesterId { get; set; }

        [Display(Name = "Session")]
        [Required(ErrorMessage = "Session is required")]
        public int SessionId { get; set; }

        [Display(Name = "Amount Paid")]
        public decimal PaidFee { get; set; }


        [Display(Name = "Total Amount")]
        public decimal TotalAmount { get; set; }


        [Display(Name = "Payment Method")]
        public PMode PaymentMode { get; set; }

        [Display(Name = "Date of Payment")]
        public DateTime Date { get; set; }

        public bool Status { get; set; }

        [Display(Name = "Fee Status")]
        public string PaymentStatus { get; set; }
        public  Student Students { get; set; }
        public  Semester Semester { get; set; }
        public  Session Session { get; set; }
        public  Faculty Faculty { get; set; }
        public string TransactionMessage { get; set; }
    }

    public class DepartmentFeePayment
    {
        public int DepartmentFeePaymentId { get; set; }

        [Display(Name = "Student's Name")]
        [Required(ErrorMessage = "Student's Name is required")]
        public string StudentId { get; set; }
        public string ReferenceNo { get; set; }

        [Index(IsUnique = true)]
        [MaxLength(50)]
        [Required]
        public string OrderId { get; set; }

        [Display(Name = "Department Id")]
        public int DepartmentId { get; set; }

        [Display(Name = "Semester")]
        [Required(ErrorMessage = "Semester is required")]
        public int SemesterId { get; set; }

        [Display(Name = "Session")]
        [Required(ErrorMessage = "Session is required")]
        public int SessionId { get; set; }

        [Display(Name = "Amount Paid")]
        public decimal PaidFee { get; set; }


        [Display(Name = "Total Amount")]
        public decimal TotalAmount { get; set; }


        [Display(Name = "Payment Method")]
        public PMode PaymentMode { get; set; }

        [Display(Name = "Date of Payment")]
        public DateTime Date { get; set; }
        public string TransactionMessage { get; set; }

        [Display(Name = "Fee Status")]
        public bool Status { get; set; }

        public string PaymentStatus { get; set; }
        public List<DepartmentFeeType> DepartmentFeeTypes { get; set; }

        public virtual Student Students { get; set; }
        public virtual Semester Semester { get; set; }
        public virtual Session Session { get; set; }
        public virtual Department Department { get; set; }

    }
}