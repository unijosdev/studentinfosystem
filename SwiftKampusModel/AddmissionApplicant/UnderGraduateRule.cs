namespace SwiftKampusModel.AddmissionApplicant
{
    public class UnderGraduateRule
    {
        public int UnderGraduateRuleId { get; set; }

        public int? SchoolProgrammeId { get; set; }
        public int? ProgrammeId { get; set; }
        public int SubjectId { get; set; }
        //public string SubjectGrade { get; set; }
        public bool IsRequired { get; set; }

        //public int Remark
        //{
        //    #region Checking grade
        //    get { return GetGradeValue(SubjectGrade); }
        //    private set { }

        //    #endregion
        //}

        //private int GetGradeValue(string gradeInput)
        //{
        //    if (gradeInput.Equals("A1"))
        //    {
        //        return 1;
        //    }
        //    if (gradeInput.Equals("B2"))
        //    {
        //        return 2;
        //    }
        //    if (gradeInput.Equals("B3"))
        //    {
        //        return 3;
        //    }
        //    if (gradeInput.Equals("C4"))
        //    {
        //        return 4;
        //    }
        //    if (gradeInput.Equals("C5"))
        //    {
        //        return 5;
        //    }
        //    if (gradeInput.Equals("C6"))
        //    {
        //        return 6;
        //    }
        //    if (gradeInput.Equals("D7"))
        //    {
        //        return 7;
        //    }
        //    if (gradeInput.Equals("E8"))
        //    {
        //        return 8;
        //    }
        //    if (gradeInput.Equals("F9"))
        //    {
        //        return 9;
        //    }
        //    return 0;
        //}

        public Programme Programme { get; set; }
        public Subject Subject { get; set; }
        public SchoolProgramme SchoolProgramme { get; set; }
    }

    public class UnderGraduateRuleVm
    {
        public int UnderGraduateRuleId { get; set; }

        public int? SchoolProgrammeId { get; set; }

        public int? ProgrammeId { get; set; }

        public int[] SubjectId { get; set; }

        public string IsRequired { get; set; }
        public string SubjectGrade { get; set; }
        public int NoOfRequiredSubject { get; set; }
    }
}
