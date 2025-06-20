using System;

namespace DomainModels.ResultModule;

public class Department
    {
        public int DepartmentId { get; set; }


        [Index(IsUnique = true)]
        [MaxLength(20)]
        [Display(Name = "Department Code")]
        //[Required(ErrorMessage = "Your Department Code is required")]
        public string DeptCode { get; set; }

        [Display(Name = "Department Name")]
        //[Required(ErrorMessage = "Your Department Name is required")]
        public string DeptName { get; set; }

        [Display(Name = "Department Location")]
        public string DeptLocation { get; set; }

        [Display(Name = "Faculty Name")]
        public int FacultyId { get; set; }

        public Faculty Faculty { get; set; }
        public ICollection<DeptResultType> DeptResultType { get; set; }
        public ICollection<Programme> Programmes { get; set; }
        public ICollection<Staff> Staff { get; set; }
        public ICollection<Course> Courses { get; set; }
        public ICollection<DeptPosition> DeptPositions { get; set; }
        public ICollection<Student> Students { get; set; }
        public ICollection<CourseRegistration> CourseRegistrations { get; set; }
        public ICollection<DepartmentFeeType> DepartmentFeeTypes { get; set; }
        public ICollection<DepartmentFeePayment> DepartmentFeePayments { get; set; }
        public ICollection<DepartmentRemitaSetting> DepartmentRemitaSettings { get; set; }
        public ICollection<SchoolFeeType> SchoolFeeTypes { get; set; }
        public ICollection<CourseCategory> CourseCategories { get; set; }

    }
