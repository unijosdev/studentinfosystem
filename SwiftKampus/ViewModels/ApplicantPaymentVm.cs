using SwiftKampusModel;

namespace SwiftKampus.ViewModels
{
    public class ApplicantPaymentVm : RemitaPostVm
    {
        public string ApplicantEmail { get; set; }
        public string ApplicantId { get; set; }
        public int SessionId { get; set; }
        public int SchoolProgrammeId { get; set; }
        public string SessionName { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal ExpectedAmount { get; set; }
        public RemitaPaymentType RemitaPaymentType { get; set; }

    }

    public class ChangeOfCoursePaymentVm : RemitaPostVm
    {
        public string FullName { get; set; }
        public string StudentId { get; set; }
        public string ChangeOfCourseType { get; set; }
        public int SessionId { get; set; }
        public string SessionName { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal ExpectedAmount { get; set; }
        public RemitaPaymentType RemitaPaymentType { get; set; }

    }


    public class IdCardPaymentVm : RemitaPostVm
    {
        public string FullName { get; set; }
        public string StudentId { get; set; }
        public string StudentType { get; set; }
        public int SessionId { get; set; }
        public string SessionName { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal ExpectedAmount { get; set; }
        public RemitaPaymentType RemitaPaymentType { get; set; }

    }

    public class ApplicantPaymentIndexVm
    {
        public string ApplicantEmail { get; set; }
        public string JambRegNo { get; set; }
        public string FullName { get; set; }
        public string Amount { get; set; }
        public string RRR { get; set; }
        public string PaymentDate { get; set; }
        public string Message { get; set; }

    }

    public class SundryAndOtherIncomePaymentVm : RemitaPostVm
    {
        public int SundryIncomeItemId { get; set; }
        public string FullName { get; set; }
        public string ApplicationUserId { get; set; }
        public string ChangeOfCourseType { get; set; }
        public string PaymentDate { get; set; }
        public string SessionName { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal ExpectedAmount { get; set; }
        public RemitaPaymentType RemitaPaymentType { get; set; }

    }
}