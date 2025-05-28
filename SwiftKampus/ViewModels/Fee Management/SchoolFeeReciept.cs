using SwiftKampusModel;
using SwiftKampusModel.Payment;
using System.Collections.Generic;

namespace SwiftKampus.ViewModels.Fee_Management
{
    public class SchoolFeeReciept
    {
        public Student Student { get; set; }
        public string FeeCategory { get; set; }
        public SchoolFeePayment SchoolFeePayment { get; set; }
        public List<FeeList> FeeLists { get; set; }
    }
    public class FacultyReciept
    {
        public Student Student { get; set; }
        public FacultyFeePayment FacultyFeePayment { get; set; }
        public List<FacultyFeeType> FacultyFeeTypes { get; set; }
    }
    public class DepartmentReciept
    {
        public Student Student { get; set; }
        public DepartmentFeePayment DepartmentFeePayment { get; set; }
        public List<DepartmentFeeType> DepartmentFeeTypes { get; set; }
    }
}