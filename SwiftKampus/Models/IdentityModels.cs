using Microsoft.AspNet.Identity.EntityFramework;
using SwiftKampusModel;
using SwiftKampusModel.Accomodation;
using SwiftKampusModel.AddmissionApplicant;
using SwiftKampusModel.Attendance;
using SwiftKampusModel.BlogPost;
using SwiftKampusModel.Calender;
using SwiftKampusModel.CBTE;
using SwiftKampusModel.Classroom;
using SwiftKampusModel.Classroom.Quiz;
using SwiftKampusModel.CourseForum;
using SwiftKampusModel.Employee;
using SwiftKampusModel.Employee.Leave;
using SwiftKampusModel.StudentStatusManagement;
using SwiftKampusModel.Library;
using SwiftKampusModel.Misconduct;
using SwiftKampusModel.Payment;
using SwiftKampusModel.SchoolMail;
using SwiftKampusModel.TimeTable;
using System.Data.Entity;
using SwiftKampusModel.MedicalScience;
using SwiftKampusModel.Siwes;
using SwiftKampusModel.ChangeOfCourse;
using SwiftKampusModel.Convocation;
using SwiftKampusModel.MarketPlace;

namespace SwiftKampus.Models
{
    public class SchoolDbContext : IdentityDbContext<ApplicationUser>
    {
        
        public SchoolDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
            //this.Configuration.LazyLoadingEnabled = false;
            this.Configuration.ProxyCreationEnabled = false;

        }

        public static SchoolDbContext Create()
        {
            return new SchoolDbContext();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {  
            modelBuilder.Entity<RolePermissions>()
                .HasKey(rp => new { rp.RoleId, rp.PermissionId });

            modelBuilder.Entity<CorrespondenceType>()
                .HasKey(ct => ct.CorrespondenceTypeId);

            modelBuilder.Entity<Correspondence>()
                .HasRequired(c => c.CorrespondenceType)
                .WithOptional(ct => ct.Correspondence);

            modelBuilder.HasDefaultSchema("public");
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Correspondence> Correspondences { get; set; }
        public DbSet<CorrespondenceType> CorrespondenceTypes { get; set; }
        public DbSet<AppointmentDiary> AppointmentDiary { get; set; }
        public DbSet<Semester> Semesters { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<AssignSessionToSchool> AssignSessionToSchools { get; set; }
        public DbSet<AssignSemesterToSchool> AssignSemesterToSchools { get; set; }
        public DbSet<Programme> Programmes { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Faculty> Faculties { get; set; }
        public DbSet<Executive> Executives { get; set; }
        public DbSet<Staff> Staffs { get; set; }
        public DbSet<StaffPosition> StaffPositions { get; set; }
        public DbSet<DeptPosition> DeptPositions { get; set; }
        public DbSet<FacultyPosition> FacultyPositions { get; set; }
        public DbSet<Level> Levels { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<CourseRegistration> CourseRegistrations { get; set; }
        public DbSet<ChangeOfCourseHistory> ChangeOfCourseHistory { get; set; }
        public DbSet<ContinuousAssessment> ContinuousAssessments { get; set; }
        public DbSet<Grade> Grades { get; set; }
        public DbSet<ExamRule> ExamRules { get; set; }
        public DbSet<QuestionAnswer> QuestionAnswers { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<StudentQuestion> StudentQuestions { get; set; }
        public DbSet<ExamSetting> ExamSettings { get; set; }
        public DbSet<ExamType> ExamTypes { get; set; }
        public DbSet<ExamLog> ExamLogs { get; set; }
        public DbSet<Result> Results { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<CourseUpload> CourseUploads { get; set; }
        public DbSet<OfficeAssignment> OfficeAssignments { get; set; }
        public DbSet<StudentAssignment> StudentAssignments { get; set; }
        public DbSet<SchoolFeeType> SchoolFeeTypes { get; set; }
        public DbSet<Block> Blocks { get; set; }
        public DbSet<Hostel> Hostels { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<AssignedRoom> AssignedRooms { get; set; }
        public DbSet<StudentAssignedRoom> StudentAssignedRooms { get; set; }
        public DbSet<SchoolProgramme> SchoolProgrammes { get; set; }
        public DbSet<Applicant> Applicants { get; set; }
        public DbSet<AttendedSchool> AttendedSchools { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<ApplicantOLevelResult> ApplicantOLevelResults { get; set; }
        public DbSet<AvailableCourse> AvailableCourses { get; set; }
        public DbSet<UnderGraduateRule> UnderGraduateRules { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<BookCategory> BookCategories { get; set; }
        public DbSet<BookIssue> BookIssues { get; set; }
        public DbSet<LibraryRegistration> LibraryRegistrations { get; set; }
        public DbSet<MembershipType> MembershipTypes { get; set; }
        public DbSet<SessionAccomodation> SessionAccomodations { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermissions> RolePermissions { get; set; }
        public DbSet<PreDegreeStudent> PreDegreeStudents { get; set; }
        public DbSet<PreDegreeExam> PreDegreeExams { get; set; }
        public DbSet<Audit> AuditRecords { get; set; }
        public DbSet<SchoolFeePayment> SchoolFeePayments { get; set; }
        public DbSet<DepartmentFeeType> DepartmentFeeTypes { get; set; }
        public DbSet<FacultyFeeType> FacultyFeeTypes { get; set; }
        public DbSet<DepartmentFeePayment> DepartmentFeePayments { get; set; }
        public DbSet<FacultyFeePayment> FacultyFeePayments { get; set; }
        public DbSet<Module> Modules { get; set; }
        public DbSet<Topic> Topics { get; set; }
        public DbSet<CarryOverCourse> CarryOverCourses { get; set; }
        public DbSet<HostelApplication> HostelApplications { get; set; }
        public DbSet<AccommodationFeePayment> AccommodationFeePayments { get; set; }
        public DbSet<StudentAccommodationFeePayment> StudentAccommodationFeePayments { get; set; }
        public DbSet<AssignedCourse> AssignedCourses { get; set; }
        public DbSet<TopicMaterial> TopicMaterials { get; set; }
        public DbSet<TopicQuiz> TopicQuizs { get; set; }
        public DbSet<QuizRule> QuizRules { get; set; }
        public DbSet<StudentTopicQuiz> StudentTopicQuizes { get; set; }
        public DbSet<QuizLog> QuizLogs { get; set; }
        public DbSet<PhoneImei> PhoneImeis { get; set; }
        public DbSet<Guardian> Guardians { get; set; }
        public DbSet<NextOfKin> NextOfKins { get; set; }
        public DbSet<Forum> Forums { get; set; }
        public DbSet<ForumView> ForumViews { get; set; }
        public DbSet<ForumQuestion> ForumQuestions { get; set; }
        public DbSet<ForumComment> ForumComments { get; set; }
        public DbSet<ForumQuestionReply> ForumQuestionReplies { get; set; }
        public DbSet<ForumQuestionView> ForumQuestionViews { get; set; }
        public DbSet<CommentReply> CommentReplies { get; set; }
        public DbSet<TimeTablePeriod> TimeTablePeriods { get; set; }
        public DbSet<EmployeeType> EmployeeTypes { get; set; }
        public DbSet<AssignedHostel> AssignedHostels { get; set; }
        public DbSet<SchoolFeeSetting> SchoolFeeSettings { get; set; }
        public DbSet<FacultyFeeSetting> FacultyFeeSettings { get; set; }
        public DbSet<DepartmentFeeSetting> DepartmentFeeSettings { get; set; }
        public DbSet<Building> Buildings { get; set; }
        public DbSet<LectureRoom> LectureRooms { get; set; }

        public DbSet<ApplicantFeeSetting> ApplicantFeeSettings { get; set; }
        public DbSet<ApplicantPayment> ApplicantPayments { get; set; }
        public DbSet<Referee> Referees { get; set; }
        public DbSet<Qualification> Qualifications { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<LeaveType> LeaveTypes { get; set; }
        public DbSet<LeaveApplication> LeaveApplications { get; set; }
        public DbSet<ReservedRoom> ReservedRooms { get; set; }
        public DbSet<RemitaPaymentLog> RemitaPaymentLogs { get; set; }
        public DbSet<SupplementaryList> SupplementaryList { get; set; }
        public DbSet<ChatConnection> ChatConnections { get; set; }
        public DbSet<PrivateMessage> PrivateMessages { get; set; }
        public DbSet<GroupMessage> GroupMessages { get; set; }
        public DbSet<FacultyRemitaSetting> FacultyRemitaSettings { get; set; }
        public DbSet<DepartmentRemitaSetting> DepartmentRemitaSettings { get; set; }
        public DbSet<Faq> Faqs { get; set; }
        public DbSet<SchoolMailMessage> SchoolMailMessages { get; set; }
        public DbSet<SchoolDraftMessage> SchoolDraftMessages { get; set; }
        public DbSet<PaymentSetting> PaymentSettings { get; set; }
        public DbSet<IdCardPayment> IdCardPayments { get; set; }
        public DbSet<IdCardPaymentSetting> IdCardPaymentSettings { get; set; }

        public DbSet<UtmeApplicant> UtmeApplicants { get; set; }

        public DbSet<AdmissionGrade> AdmissionGrades { get; set; }

        public DbSet<AwardPrice> AwardPrices { get; set; }

        public DbSet<EmploymentDetail> EmploymentDetails { get; set; }

        public DbSet<ThesisProposal> ThesisProposals { get; set; }

        public DbSet<Publication> Publications { get; set; }

        public DbSet<RelevantDocument> RelevantDocuments { get; set; }

        public DbSet<UtmeScreningPolicy> UtmeScreningPolicies { get; set; }
        public DbSet<RejectedStudent> RejectedStudents { get; set; }

        public DbSet<CourseLoadSetting> CourseLoadSettings { get; set; }

        public DbSet<ChangeOfCourseFee> ChangeOfCourseFees { get; set; }
        public DbSet<ChangeOfCoursePayment> ChangeOfCoursePayments { get; set; }

        public DbSet<CoursePrerequisite> CoursePrerequisites { get; set; }
        public DbSet<StudentPaymentDetail> StudentPaymentDetails { get; set; }

        public DbSet<ClassRoomAllocation> ClassRoomAllocations { get; set; }
        public DbSet<AssignFacultyBuilding> AssignFacultyBuildings { get; set; }
        public DbSet<StudentAttendance> StudentAttendances { get; set; }

        public DbSet<ClassDegree> ClassDegrees { get; set; }
        public DbSet<ContinuousAssessmentHistory> ContinuousAssessmentHistories { get; set; }
        public DbSet<DeptResultType> DeptResultTypes { get; set; }
        public DbSet<ResultTemplate> ResultTemplates { get; set; }

        //Emma's add-ups
        public DbSet<Defaulter> Defaulters { get; set; }
        public DbSet<StudentDisciplinaryStatus> StudentDisciplinaryStatus { get; set; }
        public DbSet<Misconduct> Misconducts { get; set; }
        public DbSet<StudentRecallRequest> StudentRecallRequests { get; set; }

        public DbSet<Extension> Extensions { get; set; }
        public DbSet<ExtensionDocument> ExtensionDocuments { get; set; }

        public DbSet<Deferment> Deferments { get; set; }
        public DbSet<DefermentDocument> DefermentDocuments { get; set; }

        public DbSet<CourseCategory> CourseCategories { get; set; }
               
        public DbSet<AssignCourseToCategory> AssignCourseToCategories { get; set; }               
        public DbSet<UtmeApplicantSubject> UtmeApplicantSubjects { get; set; }
        public DbSet<Reabsorption>  Reabsorptions { get; set; }
        public DbSet<Transfer>  Transfers { get; set; }
        public DbSet<TransferProcess>  TransferProcesses { get; set; }
        public DbSet<CourseRegSetting> CourseRegSettings { get; set; }
        public DbSet<AssignStudentLevel> AssignStudentLevels { get; set; }
        public DbSet<DeCoreCourse> DeCoreCourses { get; set; }
        public DbSet<MedResultCategory> MedResultCategories { get; set; }
        public DbSet<MedResultCa> MedResultCas { get; set; }
        public DbSet<MedContiniousAssesment> MedContiniousAssesments { get; set; }
        public DbSet<SiwesSetting> SiwesSettings { get; set; }
        public DbSet<ChangeDetailFee> ChangeDetailFees { get; set; }
        public DbSet<ChangeDetailPayment> ChangeDetailPayments { get; set; }
        public DbSet<DirectEntryExam> DirectEntryExams { get; set; }
        public DbSet<ConvocationList> ConvocationLists { get; set; }
        public DbSet<AcademicGownLease> AcademicGownLeases { get; set; }
        public DbSet<SundryAndOtherIncomeCharge> SundryAndOtherIncomeCharges { get; set; }
        public DbSet<SundryAndOtherIncomeChargesPayment> SundryAndOtherIncomeChargesPayments { get; set; }
        public DbSet<AssignSessionToProgramme> AssignSessionToProgrammes { get; set; }
        public DbSet<AssignSemesterToProgramme> AssignSemesterToProgrammes { get; set; }
        public DbSet<ApplicantWaiverPayment> ApplicantWaiverPayments { get; set; }


        //Models For Market Place
        public DbSet<Delivery> Deliveries { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Price> Prices { get; set; }
        public DbSet<StockOrder> StockOrders { get; set; }
        public DbSet<Stock> Stocks { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<UtmeScreeningCutOff> UtmeScreeningCutOffs { get; set; }
        public DbSet<RefreeResponse> RefreeResponses { get; set; }

    }
}
