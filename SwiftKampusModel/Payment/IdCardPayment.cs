using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwiftKampusModel.Payment
{
    public class IdCardPayment
    {
        public int IdCardPaymentId { get; set; }
        public string StudentId { get; set; }
        public string ReferenceNo { get; set; }

        [Index(IsUnique = true)]
        [MaxLength(90)]
        public string OrderId { get; set; }
        public int SessionId { get; set; }

        public decimal TotalAmount { get; set; }

        [Display(Name = "Date of Payment")]
        public DateTime PaymentDateTime { get; set; }
        public bool IsPayed { get; set; }
        public bool IsExpired { get; set; }
        public bool IsProcessed { get; set; }
        public bool? IsDownloaded { get; set; }
        public string ProcessedStatus { get; set; }
        public string TransactionMessage { get; set; }
        public string CardType { get; set; }
        public bool? IsPrinted { get; set; }
        public int? NoOfCount { get; set; }
        public DateTime? DatePrinted { get; set; }
        public bool? IsDispatched { get; set; }
        public DateTime? DateDispatched { get; set; }
        public Session Session { get; set; }
        public Student Student { get; set; }

    }
}
