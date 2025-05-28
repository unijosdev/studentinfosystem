using System.Collections.Generic;

namespace SwiftKampusModel.Employee
{
    public class EmployeeCategory
    {
        public int EmployeeCategoryId { get; set; }
        public string CategoryName { get; set; }
        public string CategoryCode { get; set; }
        public bool IsActive { get; set; }
        public virtual ICollection<EmployeeType> EmployeeTypes { get; set; }
    }
}
