using SwiftKampusModel;
using SwiftKampusModel.Payment;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SwiftKampus.ViewModels.Fee_Management
{
    public class SchoolFeePaymentVm
    {
        [Display(Name = "Student's Name")]
        [Required(ErrorMessage = "Student's Name is required")]
        public string StudentId { get; set; }

        [Display(Name = "Fees Type")]
        public string FeeCategory { get; set; }

        [Display(Name = "Payment Type")]
        public string SchoolFeePaymentType { get; set; }

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

        public bool IsPartPayment { get; set; }
        public bool isBalancePayment { get; set; }
        public bool IsFullPayment { get; set; }

    }

    public class FacultyFeePaymentVm
    {
        [Display(Name = "Student's Name")]
        [Required(ErrorMessage = "Student's Name is required")]
        public string StudentId { get; set; }

        [Display(Name = "Faculty Name")]
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
    }
    public class SelectPaymentVm
    {
        [Display(Name = "Student's Name")]
        [Required(ErrorMessage = "Student's Name is required")]
        public string StudentId { get; set; }

        [Display(Name = "Fees Type")]
        public string FeeCategory { get; set; }

        [Display(Name = "Semester")]
        [Required(ErrorMessage = "Semester is required")]
        public int SemesterId { get; set; }

        [Display(Name = "Session")]
        [Required(ErrorMessage = "Session is required")]
        public int SessionId { get; set; }


        [Display(Name = "Total Amount")]
        public decimal TotalAmount { get; set; }

    }
    public class ConfirmPaymentVm : RemitaPostVm
    {
        [Display(Name = "Student's Name")]
        public string StudentId { get; set; }
        public string StudentName { get; set; }

        [Display(Name = "Fees Type")]
        public string FeeCategory { get; set; }

        public string SemesterName { get; set; }
        public string SessionName { get; set; }

        [Display(Name = "Semester")]
        [Required(ErrorMessage = "Semester is required")]
        public int SemesterId { get; set; }

        [Display(Name = "Session")]
        [Required(ErrorMessage = "Session is required")]
        public int SessionId { get; set; }

        public int PaymentId { get; set; }

        [Display(Name = "Total Amount")]
        public decimal TotalAmount { get; set; }

        [Display(Name = "Payment Method")]
        public PMode PaymentMode { get; set; }

        public RemitaPaymentType RemitaPaymentType { get; set; }
        public bool IsPartPayment { get; set; }
        public bool isBalancePayment { get; set; }

        public List<FeeList> FeeLists { get; set; }
    }

    public class ChangeDetailPaymentVm : RemitaPostVm
    {
        [Display(Name = "Student's Name")]
        public string StudentId { get; set; }
        public string StudentName { get; set; }

        [Display(Name = "Fees Type")]
        public string FeeCategory { get; set; }
        public string SessionName { get; set; }

        [Display(Name = "Session")]
        [Required(ErrorMessage = "Session is required")]
        public int SessionId { get; set; }

        public int PaymentId { get; set; }

        [Display(Name = "Total Amount")]
        public decimal TotalAmount { get; set; }

        public RemitaPaymentType RemitaPaymentType { get; set; }

        public ChangeDetailFee ChangeDetailFee { get; set; }
    }


    public class FeeList
    {
        public string FeeTypeName { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
    }

    public class ReportFeeListVm
    {
        public string FeeTypeName { get; set; }
        public decimal Amount { get; set; }
        public decimal TotalAmount { get; set; }
        public string Description { get; set; }
    }


    public class SchoolPaymentFeeListVm
    {
        public List<ReportFeeListVm> ReportFeeListVms { get; set; }
        public decimal OverallTotal { get; set; }
        public int TotalStudent { get; set; }
    }

    public class DepartmentPaymentVm
    {
        [Display(Name = "Department")]
        [Required(ErrorMessage = "Department is required")]
        public int DepartmentId { get; set; }

        [Display(Name = "Semester")]
        [Required(ErrorMessage = "Semester is required")]
        public int SemesterId { get; set; }

        [Display(Name = "Session")]
        [Required(ErrorMessage = "Session is required")]
        public int SessionId { get; set; }


    }

    public class SelectDepartmentPaymentVm
    {
        [Display(Name = "Student's Name")]
        [Required(ErrorMessage = "Student's Name is required")]
        public string StudentId { get; set; }

        public int DepartmentId { get; set; }

        [Display(Name = "Semester")]
        [Required(ErrorMessage = "Semester is required")]
        public int SemesterId { get; set; }

        [Display(Name = "Session")]
        [Required(ErrorMessage = "Session is required")]
        public int SessionId { get; set; }


        [Display(Name = "Total Amount")]
        public decimal TotalAmount { get; set; }

    }

    public class ConfirmDeptPaymentVm
    {
        [Display(Name = "Student's Name")]
        [Required(ErrorMessage = "Student's Name is required")]
        public string StudentId { get; set; }

        [Display(Name = "Department")]
        public int DepartmentId { get; set; }

        [Display(Name = "Semester")]
        [Required(ErrorMessage = "Semester is required")]
        public int SemesterId { get; set; }

        [Display(Name = "Session")]
        [Required(ErrorMessage = "Session is required")]
        public int SessionId { get; set; }


        [Display(Name = "Total Amount")]
        public decimal TotalAmount { get; set; }


        [Display(Name = "Payment Method")]
        public PMode PaymentMode { get; set; }

        public List<FeeList> FeeLists { get; set; }
    }
}