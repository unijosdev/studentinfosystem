using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SwiftKampusModel.AddmissionApplicant
{
    public class ApplicantOLevelResult
    {
        public int ApplicantOLevelResultId { get; set; }
        public string ApplicantId { get; set; }
        [Required]
        public string ResultName { get; set; }
        public string Year { get; set; }
        public string ExaminationCenter { get; set; }
        public string ExamNumber { get; set; }

        [Required]
        public int SubjectId { get; set; }


        public string SubjectGrade { get; set; }

        public double GradePoint { get; set; }

        private int GetGradeValue(string gradeInput)
        {
            if (gradeInput.Equals("A1"))
            {
                return 8;
            }
            if (gradeInput.Equals("B2"))
            {
                return 7;
            }
            if (gradeInput.Equals("B3"))
            {
                return 6;
            }
            if (gradeInput.Equals("C4"))
            {
                return 5;
            }
            if (gradeInput.Equals("C5"))
            {
                return 4;
            }
            if (gradeInput.Equals("C6"))
            {
                return 3;
            }
            if (gradeInput.Equals("D7"))
            {
                return 2;
            }
            if (gradeInput.Equals("E8"))
            {
                return 1;
            }
            if (gradeInput.Equals("F9"))
            {
                return 0;
            }
            return 0;
        }
        public virtual Subject Subject { get; set; }
    }

    public class ApplicantOLevelResultVm
    {

        public List<ApplicantOLevelVm> ApplicantOLevelVm { get; set; }
        public List<UnderGraduateRule> UnderGraduateRules { get; set; }

    }

    public class ApplicantOLevelVm
    {
        [Required]
        public string ResultName { get; set; }
        //public string Year { get; set; }
        //public string ExaminationCenter { get; set; }
        public int MaximumCount { get; set; }
        [Required]
        public string SubjectGrades { get; set; }
        [Required]
        public int SubjectId { get; set; }

    }
    public class SaveApplicantOLevelVm
    {
        [Required]
        public string ResultName { get; set; }
        public string Year { get; set; }
        public string ExaminationCenter { get; set; }
        public string ExamNumber { get; set; }
        public int MaximumCount { get; set; }
        public SubjectGrade[] SubjectGrades { get; set; }

    }

    public class SubjectGrade
    {
        public int subject { get; set; }
        public string grade { get; set; }
    }

    public class DeleteRowRequest
    {
        public int ApplicantOlevelResultId { get; set; }
        public int SubjectId { get; set; }
    }
}
