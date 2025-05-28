using System.Collections.Generic;

namespace SwiftKampusModel.Payment
{
    public class FeeCategory
    {
        public int FeeCategoryId { get; set; }
        public string CategoryName { get; set; }
        public virtual ICollection<SchoolFeeType> SchoolFeeTypes { get; set; }
        public virtual ICollection<SchoolFeePayment> SchoolFeePayments { get; set; }
    }
}
