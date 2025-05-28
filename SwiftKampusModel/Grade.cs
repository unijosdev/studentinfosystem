using SwiftKampusModel.AddmissionApplicant;
using System.ComponentModel.DataAnnotations;

namespace SwiftKampusModel
{
    public class Grade
    {
        [Key]
        public int GradeId { get; set; }

        public int SchoolProgrammeId { get; set; }
        public int ResultTemplateId { get; set; }

        [Display(Name = "Grade Name")]
        [Required(ErrorMessage = "Grade Name is required")]
        public string GradeName { get; set; }

        [Range(0, 100)]
        [Display(Name = "Minimum Value")]
        public int MinimumValue { get; set; }


        [Range(0, 100)]
        [Display(Name = "Maximum Value")]
        public int MaximumValue { get; set; }

        [Display(Name = "Grade Point")]
        [Range(0, 6)]
        public double GradePoint { get; set; }


        [Display(Name = "Remark")]
        [Required(ErrorMessage = "Remark is required")]
        public string Remark { get; set; }

        public SchoolProgramme SchoolProgramme { get; set; }
        public ResultTemplate ResultTemplate { get; set; }


        //public int FacultyId { get; set; }

        //public virtual Faculty Faculty { get; set; }
    }


    public class ClassDegree
    {
        [Key]
        public int ClassDegreeId { get; set; }

        public int SchoolProgrammeId { get; set; }

        [Display(Name = "Grade Name")]
        [Required(ErrorMessage = "Grade Name is required")]
        public string DegreeName { get; set; }

        [Range(0.99, 5.0)]
        [Display(Name = "Minimum Point")]
        public double MinimumValue { get; set; }


        [Range(0.99, 5.0)]
        [Display(Name = "Maximum Point")]
        public double MaximumValue { get; set; }


        [Display(Name = "Remark")]
        [Required(ErrorMessage = "Remark is required")]
        public string Remark { get; set; }

        public SchoolProgramme SchoolProgramme { get; set; }

    }
}