using System.Collections.Generic;
using SwiftKampusModel.Employee.PayRoll;

namespace SwiftKampusModel.Employee
{
    public class EmployeeType
    {
        public int EmployeeTypeId { get; set; }
        public string EmployeeTypeName { get; set; }
        public int EmployeeCategoryId { get; set; }
        public virtual EmployeeCategory EmployeeCategory { get; set; }
        public ICollection<PayRollCategory> PayRollCategories { get; set; }
    }
}