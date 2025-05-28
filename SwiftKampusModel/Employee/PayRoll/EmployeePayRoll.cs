using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwiftKampusModel.Employee.PayRoll
{
    public class EmployeePayRoll
    {
        [Key, ForeignKey("PayRollCategory")]
        public int PayRollCategoryId { get; set; }
        public decimal Amount { get; set; }
        public virtual PayRollCategory PayRollCategory { get; set; }

    }
}