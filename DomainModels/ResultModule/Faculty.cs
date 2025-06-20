using System;

namespace DomainModels.ResultModule;

public class Faculty
    {
        public int FacultyId { get; set; }

        [Index(IsUnique = true)]
        [MaxLength(20)]
        [Display(Name = "Faculty Code")]
        [Required(ErrorMessage = "Your Faculty Code is required")]
        public string FacultyCode { get; set; }

        [Display(Name = "Faculty Name")]
        [Required(ErrorMessage = "Your Faculty Name is required")]
        public string FacultyName { get; set; }

        public ICollection<Department> Departments { get; set; }
        public ICollection<SchoolFeeType> SchoolFeeTypes { get; set; }
        public ICollection<FacultyPosition> FacultyPositions { get; set; }
        public ICollection<FacultyFeeType> FacultyFeeTypes { get; set; }
        public ICollection<AssignedHostel> AssignedHostels { get; set; }
        public ICollection<FacultyFeePayment> FacultyFeePayments { get; set; }
        public ICollection<FacultyRemitaSetting> FacultyRemitaSettings { get; set; }
        public ICollection<AssignFacultyBuilding> AssignFacultyBuilding { get; set; }
        public ICollection<AssignStudentLevel> AssignStudentLevels { get; set; }

    }
