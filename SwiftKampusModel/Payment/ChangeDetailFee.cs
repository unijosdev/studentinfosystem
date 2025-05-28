using System.Collections.Generic;

namespace SwiftKampusModel.Payment
{
    public class ChangeDetailFee
    {
        public int ChangeDetailFeeId { get; set; }
        public string FancyName { get; set; }
        public string DetailCategory { get; set; }
        public decimal Amount { get; set; }
        public string AmountInWords { get; set; }
        public ICollection<ChangeDetailPayment> ChangeDetailPayment { get; set; }
    }
}
