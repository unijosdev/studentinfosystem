using System;
using System.Runtime.Serialization;

namespace SwiftKampus.Services
{

    public class RemitaResponse
    {
        [DataMember(Name = "orderId")]
        public string OrderId { get; set; }

        [DataMember(Name = "RRR")]
        public string Rrr { get; set; }

        [DataMember(Name = "status")]
        public string Status { get; set; }

        [DataMember(Name = "message")]
        public string Message { get; set; }

        [DataMember(Name = "amount")]
        public decimal Amount { get; set; }

        [DataMember(Name = "paymentdate")]
        public DateTime PaymentDate { get; set; }
    }

}