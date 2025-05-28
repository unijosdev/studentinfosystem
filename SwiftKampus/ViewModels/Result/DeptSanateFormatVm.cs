using SwiftKampusModel;
using System;
using System.Collections.Generic;

namespace SwiftKampus.ViewModels.Result
{
    public class DeptSanateFormatVm
    {
        public List<string> CourseCode { get; set; }
        public List<DeptSanateFormatDetailVm> DeptSanateFormatDetailVms { get; set; }
    }

    public class DeptSanateFormatDetailVm
    {
        public int Sn { get; set; }
        public string MatricNo { get; set; }
        public string FullName { get; set; }
        public string ModeOfEntry { get; set; }
        public string MNSA { get; set; }
        public string NSS { get; set; }
        public List<ContinuousAssessment> ContinuousAssessments { get; set; }
    }

    public class DeptSanateFormatSummaryVm
    {
        public int Sn { get; set; }
        public string MatricNo { get; set; }
        public string FullName { get; set; }
        public string ModeOfEntry { get; set; }
        public string MNSA { get; set; }
        public string NSS { get; set; }
        public List<SessionParameters> SessionParameters { get; set; }
        public string Remarks { get; set; }
        public List<ContinuousAssessment> ContinuousAssessments { get; set; }
    }
    public class SessionParameters
    {
        public string LevelName { get; set; }
        public int TCR { get; set; }
        public int TCE { get; set; }
        public int TGP { get; set; }
        public string GPA { get; set; }
        public double DGPA { get; set; }
        public int CTCR { get; set; }
        public int CTCE { get; set; }
        public int CTGP { get; set; }
        public string CGPA { get; set; }
        public List<ContinuousAssessment> ContinuousAssessments { get; set; }
    }
    public class PreviousModel
    {
        public int LevelId { get; set; }
        public int SessionId { get; set; }
    }

    public class SanateFormatSummaryVm
    {
        public int Sn { get; set; }
        public string MatricNo { get; set; }
        public string FullName { get; set; }
        public string ModeOfEntry { get; set; }
        public string MNSA { get; set; }
        public string NSS { get; set; }
        public string TCR { get; set; }
        public string TCE { get; set; }
        public string TGP { get; set; }
        public double GPA { get; set; }
        public string Remarks { get; set; }
        public List<ContinuousAssessment> ContinuousAssessments { get; set; }
    }


    public class NyscListVm
    {
        public int Sn { get; set; }
        public string MatricNo { get; set; }
        public string Surname { get; set; }
        public string OtherName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string JambRegNo { get; set; }
        public string StateOfOrigin { get; set; }
        public string Course { get; set; }
        public string ClassOfDegree { get; set; }
        public string Gender { get; set; }
        public string MaritalStatus { get; set; }
        public string Qualification { get; set; }
        public string ServiceYear { get; set; }
        public string ProgrammeMode { get; set; }
        public string PhoneNumber { get; set; }
        public string YearOfResult { get; set; }
        public int SchoolProgrammeId { get; set; }

    }

    public class GraduantsListVm
    {
       public List<NyscListVm> NyscListVms { get; set; }
        public List<string> ClassOfHonour { get; set; }
    }
}