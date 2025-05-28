using System;

namespace SwiftKampusModel.Employee.PayRoll
{
    public class PayRollCategory
    {
        public int PayRollCategoryId { get; set; }
        public string CategoryName { get; set; }
        public double Percentage { get; set; }
        public double PercentageAmount { get; set; }

        public decimal Total
        {
            get
            {
                var calculatedIncome = (Percentage * PercentageAmount) / 100;
                return Convert.ToDecimal(calculatedIncome);
            }
        }

        public bool IsActive { get; set; }
        public bool IsADeduction { get; set; }
        public int EmployeeTypeId { get; set; }
        public virtual EmployeeType EmployeeType { get; set; }
        public virtual EmployeePayRoll EmployeePayRoll { get; set; }

    }
}
