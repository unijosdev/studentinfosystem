using System;
using System.Collections.Generic;

using SwiftKampus.ViewModels.Fee_Management;
using SwiftKampusModel.AddmissionApplicant;

namespace SwiftKampus.ViewModels
{
    public class StudentDashboardVM
    {
        public string StudentId { get; set; }
        public string MatricNo { get; set; }
        public string JambReg { get; set; }
        public string PrimaryEmail { get; set; }
        public string DepartmentName { get; set; }
        public string FacultyName { get; set; }
        public string ProgrammeName { get; set; }
        public string CurrentLevel { get; set; }
        public string SemesterName { get; set; }
        public string SessionName { get; set; }
        public List<CourseRegVm> Courses { get; set; }
        public List<FeeList> SchoolFee { get; set; }
        public string Assignment { get; set; }
        public int MaleStudents { get; set; }
        public string ClassRoomNotification { get; set; }
        public int DepartmentPopulation { get; set; }
        public int FacultyPopulation { get; set; }
        public List<string> BorrowedBooks { get; set; }
        public int NoOfBorrowedBooks { get; set; }
    }

    public class CourseRegVm
    {
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public string Staus { get; set; }
    }

    public class ExamsLogVm
    {
        public int ExamLogId { get; set; }
        public string StudentId { get; set; }
        public double TotalScore { get; set; }
        public double Score { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string FullName => LastName + " " + FirstName + " " + MiddleName;
        public string CourseName { get; set; }
        public string MatricNumber { get; set; }
        public bool Status { get; set; }
    }

    public class StudentIndexVM
    {
        public string StudentId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string FullName => LastName + " " + FirstName + " " + MiddleName;
        public string MatricNo { get; set; }
        public string ProgrammeName { get; set; }
        public string DeptName { get; set; }
        public string FacultyName { get; set; }
        public string JambRegNo { get; set; }
        public string Gender { get; set; }
        public string LevelName { get; set; }
        public string SchoolProgrammeCode { get; set; }
        public string ModeOfEntry { get; set; }
        public string BloodGroup { get; set; }
        public string IsScholarship { get; set; }

        public string StateOfOrigin { get; set; }

        public List<string> Subjects { get; set; }
    }

    public class ChangeOfCourseReportVm
    {
        public string StudentId { get; set; }
        public int Id { get; set; }
        public string RegNo { get; set; }
        public string MatricNo { get; set; }
        public string FullName { get; set; }
        public string Score { get; set; }
        public string CourseAdmitted { get; set; }
        public string ReasonForRejection { get; set; }
        public string NewCourseApplied { get; set; }
        public string Rocommendation { get; set; }
        public string ModeOfEntry { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
    }

    public class ProcessChangeOfCourseReportVm
    {
        public int Id { get; set; }
        public string RegNo { get; set; }
        public string StudentId { get; set; }
        public string FullName { get; set; }
        public string Score { get; set; }
        public string CourseAdmitted { get; set; }
        public string ReasonForRejection { get; set; }
        public string NewCourseApplied { get; set; }
        public string Rocommendation { get; set; }
        public string ModeOfEntry { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
    }

    public class ChangeOfCoursePaymentHistoryVm
    {
        public int Id { get; set; }
        public string RegNo { get; set; }
        public string FullName { get; set; }
        public string CourseAdmitted { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public string ReferenceNumber { get; set; }
    }

    public class UtmeApplicantVm
    {
        public string JambRegNo { get; set; }
        public string FullName { get; set; }
        public string MiddleName { get; set; }
        public string ResultGrade { get; set; }
        public string StateOfOrigin { get; set; }
        public string Lga { get; set; }
        public string Email { get; set; }

        public string DateofBirth { get; set; }

    }

    public class ApplicantWaiverPaymentVm
    {
        public string JambRegNo { get; set; }
        public string FullName { get; set; }
        public string MiddleName { get; set; }
        public string ResultGrade { get; set; }
        public string StateOfOrigin { get; set; }
        public string Lga { get; set; }
        public string Email { get; set; }
        public string QualificationInstitution { get; set; }
        public string PGRecommendation { get; set; }

    }
    public class ApplicantListVm
    {
        public int Sn { get; set; }
        public string ApplicantId { get; set; }
        public string FullName { get; set; }
        public string SchoolProgrammeName { get; set; }
        public string ProgrammeName { get; set; }
        public string DepartmentName { get; set; }
        public string FacultyName { get; set; }
        public string StateOfOrigin { get; set; }
        public string Lga { get; set; }
        public string Gender { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Status { get; set; }
        public string FacultyStatus { get; set; }
        public string PgStatus { get; set; }
        public string VcStatus { get; set; }
        public string InstittionAttended { get; set; }
        public string Degree { get; set; }
        public string ClassOfDegree { get; set; }
        public string CGPA { get; set; }
        public string CourseOfStudy { get; set; }


        //public List<AttendedSchool> AttendedSchool { get; set; }

    }
    public class UtmeScrenningVm
    {
        public string JambRegNo { get; set; }
        public string FacultyName { get; set; }
        public string DateOfBirth { get; set; }
        public string FullName { get; set; }
        public string JambPercentage { get; set; }
        public string OLevelCummulative { get; set; }
        public string StateOfOrigin { get; set; }
        public string LocalGovtArea { get; set; }
        public string JambScore { get; set; }
        public string SubjectOne { get; set; }
        public string GradeOne { get; set; }
        public string SubjectTwo { get; set; }
        public string GradeTwo { get; set; }
        public string SubjectThree { get; set; }
        public string GradeThree { get; set; }
        public string SubjectFour { get; set; }
        public string GradeFour { get; set; }
        public string SubjectFive { get; set; }
        public string GradeFive { get; set; }
        public string Cummulative { get; set; }
        public string ProgrammeName { get; set; }
        public string JambSubjectOne { get; set; }
        public string JambSubjectScoreOne { get; set; }
        public string JambSubjectTwo { get; set; }
        public string JambSubjectScoreTwo { get; set; }
        public string JambSubjectThree { get; set; }
        public string JambSubjectScoreThree { get; set; }
        public string JambSubjectFour { get; set; }
        public string JambSubjectScoreFour { get; set; }

    }

    public class DeScrenningVm
    {
        public string JambRegNo { get; set; }
        public string FacultyName { get; set; }
        public string DateOfBirth { get; set; }
        public string FullName { get; set; }
        public string JambPercentage { get; set; }
        public string OLevelCummulative { get; set; }
        public string StateOfOrigin { get; set; }
        public string LocalGovtArea { get; set; }
        public string JambScore { get; set; }
        public string SubjectOne { get; set; }
        public string GradeOne { get; set; }
        public string SubjectTwo { get; set; }
        public string GradeTwo { get; set; }
        public string SubjectThree { get; set; }
        public string GradeThree { get; set; }
        public string SubjectFour { get; set; }
        public string GradeFour { get; set; }
        public string SubjectFive { get; set; }
        public string GradeFive { get; set; }
        public string Cummulative { get; set; }
        public string InstitutionName { get; set; }
        public string ResultName { get; set; }
        public string ResultType { get; set; }
        public string Discipline { get; set; }
        public string YearAttended { get; set; }
        public string ProgrammeName { get; set; }
        public string JambSubjectOne { get; set; }
        public string JambSubjectScoreOne { get; set; }
        public string JambSubjectTwo { get; set; }
        public string JambSubjectScoreTwo { get; set; }
        public string JambSubjectThree { get; set; }
        public string JambSubjectScoreThree { get; set; }
        public string JambSubjectFour { get; set; }
        public string JambSubjectScoreFour { get; set; }

    }
}