using EndWellJobApplication.Models;
using SwiftKampusModel.Accomodation;
using SwiftKampusModel.AddmissionApplicant;
using SwiftKampusModel.CBTE;
using SwiftKampusModel.ChangeOfCourse;
using SwiftKampusModel.Convocation;
using SwiftKampusModel.MedicalScience;
using SwiftKampusModel.Misconduct;
using SwiftKampusModel.Payment;
using SwiftKampusModel.Siwes;
using SwiftKampusModel.StudentStatusManagement;
using SwiftKampusModel.TimeTable;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwiftKampusModel
{
    public class Session
    {
        public int SessionId { get; set; }

        [MaxLength(20)]
        [Index(IsUnique = true)]
        [Display(Name = "Session Name")]
        [Required(ErrorMessage = "Session Name is required")]
        public string SessionName { get; set; }

        [Display(Name = "Session Start")]
        [Required(ErrorMessage = "Session Start is required")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Display(Name = "Session End")]
        [Required(ErrorMessage = "Session End is required")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }
        public ICollection<ExamRule> ExamRules { get; set; }
        public ICollection<ScreeningRule> ScreeningRules { get; set; }
        public ICollection<DeptPosition> DeptPositions { get; set; }
        public ICollection<SchoolFeeType> SchoolFeeTypes { get; set; }
        public ICollection<SchoolFeePayment> SchoolFeePayments { get; set; }
        public ICollection<FacultyFeePayment> FacultyFeePayments { get; set; }
        public ICollection<DepartmentFeePayment> DepartmentFeePayments { get; set; }
        public ICollection<CourseRegistration> CourseRegistrations { get; set; }

        public ICollection<Result> Results { get; set; }
        public ICollection<ExamSetting> ExamSettings { get; set; }
        public ICollection<ExamLog> ExamLogs { get; set; }
        public ICollection<StudentAssignment> StudentAssignments { get; set; }
        public ICollection<SessionAccomodation> SessionAccomodations { get; set; }
        public ICollection<HostelApplication> HostelApplications { get; set; }
        //public ICollection<AccommodationFeePayment> AccommodationFeePayments { get; set; }
        public ICollection<StudentAccommodationFeePayment> StudentAccommodationFeePayments { get; set; }
        public ICollection<AssignedCourse> AssignedCourses { get; set; }
        public ICollection<TimeTablePeriod> TimeTablePeriods { get; set; }
        public ICollection<ExamTimeTable> ExamTimeTables { get; set; }
        public ICollection<SchoolFeeSetting> SchoolFeeSettings { get; set; }
        public ICollection<FacultyFeeSetting> FacultyFeeSettings { get; set; }
        public ICollection<DepartmentFeeSetting> DepartmentFeeSettings { get; set; }
        public ICollection<ApplicantPayment> ApplicantPayments { get; set; }
        public ICollection<ApplicantFeeSetting> ApplicantFeeSettings { get; set; }
        public ICollection<PaymentSetting> PaymentSettings { get; set; }
        public ICollection<UtmeApplicant> UtmeApplicants { get; set; }
        public ICollection<UtmeScreningPolicy> UtmeScreningPolicies { get; set; }
        public ICollection<Applicant> Applicants { get; set; }
        public ICollection<CourseLoadSetting> CourseLoadSettings { get; set; }
        public ICollection<ChangeOfCoursePayment> ChangeOfCoursePayments { get; set; }
        public ICollection<ApplicantWaiverPayment> ApplicantWaiverPayments { get; set; }
        public ICollection<ChangeOfCourseFee> ChangeOfCourseFees { get; set; }
        public ICollection<StudentPaymentDetail> StudentPaymentDetails { get; set; }
        public ICollection<ContinuousAssessment> ContinuousAssessments { get; set; }
        public ICollection<ContinuousAssessmentHistory> ContinuousAssessmentHistories { get; set; }
        public ICollection<DeptResultType> DeptResultTypes { get; set; }
        public ICollection<Defaulter> Defaulters { get; set; }
        public ICollection<Transfer> Transfers { get; set; }
        public ICollection<Extension> Extensions { get; set; }
        public ICollection<CourseRegSetting> CourseRegSettings { get; set; }
        public ICollection<AssignStudentLevel> AssignStudentLevels { get; set; }
        public ICollection<MedContiniousAssesment> MedContiniousAssesments { get; set; }
        public ICollection<SiwesSetting> SiwesSettings { get; set; }
        public ICollection<SiwesPlacement> SiwesPlacements { get; set; }
        public ICollection<SiwesAttendance> SiwesAttendances { get; set; }
        public ICollection<ChangeDetailPayment> ChangeDetailPayments { get; set; }
        public ICollection<UtmeScreeningCutOff> UtmeScreeningCutOffs { get; set; }
        public ICollection<Deferment> Deferments { get; set; }

    }

}