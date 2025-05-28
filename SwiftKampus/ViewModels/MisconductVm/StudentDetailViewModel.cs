using SwiftKampus.ViewModels.MisconductVm;
using SwiftKampusModel;
using SwiftKampusModel.Misconduct;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace SwiftKampus.ViewModels.MisconductVm
{
    public class StudentDetailViewModel
    {
        public string StudentId { get; set; }

        [Display(Name = "Matriculation Number")]
        public string MatriculationNumber { get; set; }

        [Display(Name = "Fullname")]
        public string FullName { get; set; }

        [Display(Name = "Faculty")]
        public string FacultyName { get; set; }

        [Display(Name = "Department")]
        public string DepartmentName { get; set; }

        [Display(Name = "Programme")]
        public string DepartmentOptionName { get; set; }

        [Display(Name ="Level")]
        public string LevelName { get; set; }

        public int SessionName { get; set; }

        public Session Session { get; set; }

        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        public ICollection<Defaulter> Defaulters { get; set; }

        [Display(Name = "Disciplinary Status")]
        public StudentDisciplinaryStatus StudentDisciplinaryStatus { get; set; }
        public DefaulterMisconductViewModel DefaulterMisconductSingleViewModel { get; set; }

        public ICollection<DefaulterMisconductViewModel> DefaulterMisconductViewModel{ get; set; }

    }
}