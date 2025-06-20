using System;

namespace DomainModels;

public class Student : Person
    {
        public AfricanCountries AfricanCountries;
        public Student()
        {
            AfricanCountries = new AfricanCountries();
        }
        [Key]
        public string StudentId { get; set; }
        public int? SessionId { get; set; }

        public string MatricNo { get; set; }

        public string JambRegNo { get; set; }
        public string PrimaryEmail { get; set; }
        public string ImeiNo { get; set; }

        [Display(Name = "Hobbies")]
        public string Hobby { get; set; }

        [DataType(DataType.Date)]
        // [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Enrollment Date")]
        public DateTime? EnrollmentDate { get; set; }

        public int? ProgrammeId { get; set; }

        public int SchoolProgrammeId { get; set; }

        public bool Active { get; set; }
        [Display(Name = "Physically Challenged?")]
        public bool? IsPhysicallyChallenged { get; set; }
        [Display(Name = "If Yes Please Specify")]
        public string PChallengedDetail { get; set; }
        public string BloodGroup { get; set; }

        //[Required]
        public string StudentStatus { get; set; }        
        public bool IsGraduated { get; set; }
        public bool? IsSchoolarshipStudent { get; set; }
        public bool? IsDownloaded { get; set; }
        public bool? IsRemedialStudent { get; set; }
        public bool IsStaff { get; set; }

        public bool NationalityStatus
        {
            get
            {
                if (!string.IsNullOrEmpty(Nationality))
                {
                    if (Nationality.Trim().ToUpper().Equals("NIGERIA") || Nationality.Trim().ToUpper().Equals("NIGERIAN"))
                    {
                        return true;
                    }
                }
                return false;
            }
        }
        public string Indegine
        {
            get
            {
                var africanCountries = AfricanCountries.Names;
                if (!string.IsNullOrEmpty(Nationality))
                {
                    var isAfrican = africanCountries.Any(x => x.ToUpper().Equals(Nationality.ToUpper()));
                    if (!string.IsNullOrEmpty(Nationality))
                    {
                        if (Nationality.Trim().ToUpper().Equals("NIGERIA") || Nationality.Trim().ToUpper().Equals("NIGERIAN"))
                        {
                            return IndegineStatus.Nigerian.ToString();
                        }
                        else if (isAfrican)
                        {
                            return IndegineStatus.African.ToString();
                        }
                        else
                        {
                            return IndegineStatus.Others.ToString();
                        }
                    }
                }                
                return "Others";
            }
        }

        public bool IsClearedDepartment { get; set; }
        public bool IsClearedAcademics { get; set; }
        public bool IsClearedFaculty { get; set; }
        public bool IsClearedAll
        {
            get
            {
                if (IsClearedAcademics.Equals(true) && IsClearedDepartment.Equals(true) &&
                    IsClearedFaculty.Equals(true))
                {
                    return true;
                }
                return false;
            }
        }

        // Method to log clearance details
        public void LogClearance(string officerName, string clearanceType)
        {
            string logFilePath = "ClearanceLog.txt";
            string path = HttpContext.Current.Server.MapPath("~/" + logFilePath);
            string logMessage = $"{JambRegNo},{clearanceType},{officerName},{DateTime.Now}";

            try
            {
                // Append log to the file
                using (StreamWriter writer = new StreamWriter(path, true))
                {
                    writer.WriteLine(logMessage);
                }
                Console.WriteLine("Clearance details logged successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while logging clearance: {ex.Message}");
            }
        }

        public string ModeOfEntry { get; set; }
        public bool IsDelete { get; set; }
        public int? LevelId { get; set; }
        public Level Level { get; set; }
        public Programme Programme { get; set; }
        public Session Session { get; set; }
        public SchoolProgramme SchoolProgramme { get; set; }

        public ICollection<Enrollment> Enrollments { get; set; }
        public ICollection<CourseRegistration> CourseRegistrations { get; set; }
        public ICollection<SchoolFeePayment> SchoolFeePayments { get; set; }
        public ICollection<DepartmentFeePayment> DepartmentFeePayments { get; set; }
        public ICollection<FacultyFeePayment> FacultyFeePayments { get; set; }
        public ICollection<Result> Results { get; set; }
        public ICollection<ExamLog> ExamLogs { get; set; }
        public ICollection<StudentAssignment> StudentAssignments { get; set; }
        public ICollection<BookIssue> BookIssues { get; set; }
        public ICollection<StudentAssesmentQuestion> StudentAssesmentQuestions { get; set; }
        public ICollection<StudentTestLog> StudentTestLogs { get; set; }
        public ICollection<StudentTopicQuiz> StudentTopicQuizzes { get; set; }
        public ICollection<HostelApplication> HostelApplications { get; set; }
        //public ICollection<AccommodationFeePayment> AccommodationFeePayments { get; set; }
        public ICollection<StudentAccommodationFeePayment> StudentAccommodationFeePayments { get; set; }
        public ICollection<QuizLog> QuizLogs { get; set; }
        public ICollection<ForumQuestion> ForumQuestions { get; set; }
        public ICollection<StudentAttendance> StudentAttendances { get; set; }
        public ICollection<StudentPaymentDetail> StudentPaymentDetails { get; set; }
        public ICollection<ContinuousAssessment> ContinuousAssessments { get; set; }
        public ICollection<ContinuousAssessmentHistory> ContinuousAssessmentHistories { get; set; }
        public ICollection<Defaulter> Defaulters { get; set; }
        public ICollection<StudentRecallRequest> StudentRecallRequests { get; set; }
        public ICollection<Transfer> Transfers { get; set; }
        public ICollection<StudentTransferData> StudentTransferDatas { get; set; }
        public ICollection<Deferment> Deferments { get; set; }
        public ICollection<Extension> Extensions { get; set; }
        public ICollection<MedContiniousAssesment> MedContiniousAssesments { get; set; }
        public ICollection<SiwesPlacement> SiwesPlacements { get; set; }
        public ICollection<SiwesAttendance> SiwesAttendances { get; set; }
        public ICollection<ChangeDetailPayment> ChangeDetailPayments { get; set; }
        public ICollection<ChangeOfCoursePayment> ChangeOfCoursePayment { get; set; }


        // Methods to handle each specific clearance
        public void ClearAtAcademic(string officerName)
        {
            IsClearedAcademics = true;
            LogClearance(officerName, "Academic Clearance");
        }

        public void ClearAtDepartment(string officerName)
        {
            IsClearedDepartment = true;
            LogClearance(officerName, "Department Clearance");
        }

        public void ClearAtFaculty(string officerName)
        {
            IsClearedFaculty = true;
            LogClearance(officerName, "Faculty Clearance");
        }

    }

    public class ClearanceLog
    {
        public string JambRegNo { get; set; }
        public string ClearanceType { get; set; }
        public string Officer { get; set; }
        public DateTime DateTime { get; set; }
    }

    public class DepartmentLevelGenderCountViewModel
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public int LevelId { get; set; }
        public int MaleCount { get; set; }
        public int FemaleCount { get; set; }
        public int TotalCount => MaleCount + FemaleCount;
    }

    public class AfricanCountries
    {
        public AfricanCountries()
        {
            Names = new List<string> {
            "Algeria", "Angola", "Benin", "Botswana", "Burkina Faso", "Burundi", "Cabo Verde",
            "Cameroon", "Central African Republic (CAR)","Chad", "Comoros", "Democratic Republic of the Congo",
            "Republic of the Congo","Ivory Coast", "Djibouti", "Egypt", "Equatorial Guinea",
            "Eritrea", "Eswatini", "Ethiopia", "Gabon", "Gambia", "Ghana", "Guinea",
            "Guinea-Bissau", "Kenya", "Lesotho", "Liberia", "Libya", "Madagascar",
            "Malawi", "Mali", "Mauritania", "Mauritius", "Morocco", "Mozambique",
            "Namibia", "Niger", "Nigeria", "Rwanda", "Sao Tome", "Senegal", "Seychelles",
            "Sierra Leone", "Somalia", "South Africa", "South Sudan", "Sudan", "Tanzania",
            "Togo", "Tunisia", "Uganda", "Zambia", "Zimbabwe"};
        }
        public static List<string> Names { get; set; }
    }
