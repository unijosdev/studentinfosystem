using SwiftKampus.ViewModels.MisconductVm;
using SwiftKampusModel;
using SwiftKampusModel.Misconduct;

using System.ComponentModel.DataAnnotations;


namespace SwiftKampus.ViewModels.MisconductVm
{
    public class ShowStudentDetailViewModel
    {
        public string StudentId { get; set; }

        public int DefaulterId { get; set; }

        [Display(Name = "Matriculation Number")]
        public string MatriculationNumber { get; set; }

        [Display(Name = "Fullname")]
        public string FullName { get; set; }

        public string LastName { get; set; }

        public string FirstName { get; set; }

        public string MiddleName { get; set; }

        [Display(Name = "Faculty")]
        public string FacultyName { get; set; }

        [Display(Name = "Department")]
        public string DepartmentName { get; set; }

        [Display(Name = "Programme")]
        public string DepartmentOptionName { get; set; }

        [Display(Name = "Level")]
        public string LevelName { get; set; }

        public int SessionName { get; set; }

        public Session Session { get; set; }

        [Display(Name = "Disciplinary Status")]
        public StudentDisciplinaryStatus StudentDisciplinaryStatus { get; set; }

        public DefaulterMisconductViewModel DefaulterMisconductViewModel { get; set; }

        public string MisconductName { get; set; }

        public string IncidentDate { get; set; }

        public bool Status { get; set; }

        public Punishment Punishment { get; set; }

        public string PunishmentName { get; set; }

        public string EvidenceAdress { get; set; }

        public double PriceOfVadalisedProperty { get; set; }

    }
}