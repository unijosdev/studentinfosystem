using System.ComponentModel;

namespace SwiftKampusModel
{
    public enum Salutation
    {
        Dr = 1, Nurse, Mr, Mrs, Miss, Engr, Pastor
    }

    public enum Relationship
    {
        Father = 1, Mother, Sister, Brother, Nephew, Uncle, Cousin, Niece, Friend, Spouse, Others
    }
   
    public enum Vote
    {
        Like, Dislike
    }

    public enum Religion
    {
        Christianity = 1, Muslim, Others
    }

    public enum Gender
    {
        Male = 1, Female
    }

    public enum Title
    {
        Mr = 1, Mrs, Miss, Dr, Prof, 
    }
    public enum PMode
    {
        Cash = 1, Cheque, Teller, OnlinePayment, NELFUND
    }

    public enum HealthStatus
    {
        Excellent, Good, Poor
    }

    public enum ResultTypeGrade
    {
        A1, B2,A2, A3, B3, C4, C5, C6, D7, E8, F9, ABS
    }

    public enum Status
    {
        GivenOut = 1, Returned
    }

    public enum PositionType
    {
        Executive = 1, Faculty, Department, Administrative
    }
    public enum Maritalstatus
    {
        Single = 1, Married, Divorced, Others
    }

    public enum Qualifications
    {
        NCE,
        IJMB,
        NURSING_CERTIFACTE,
        ND,
        OND,
        HND,
        Degree,
        Others,
    }

    public enum QualificationNormalGrade
    {
        Distinction,
        Merit,
        Upper_Credit,
        Second_Class_Upper,
        Lower_Credit,
        Second_Class_Lower,
        Pass,

    }
   


    public enum ThemeColor
    {
        Purple = 1, LightBlue, NavyBlue, ArmyGreen, LightRed, DeepRed
    }

    public enum State
    {
        select_state,Abia, Adamawa, AkwaIbom, Anambra, Bauchi, Bayelsa, Benue, Borno, CrossRiver, Delta, Ebonyi, Edo, Enugu, Ekiti,
        Gombe, Imo, Jigawa, Kaduna, Kano, Katsina, Kebbi, Kogi, Kwara, Lagos, Nasarawa, Niger, Ogun, Ondo, Osun,
        Oyo, Plateau, Rivers, Sokoto, Taraba, Yobe, Zamfara, FCT, Foreigner
    }

    public enum QuestionType
    {
        SingleChoice, MultiChoice, BlankChoice
    }

    public enum ResultType
    {
        WAEC, NECO, NABTEB, TEACHERS_GRADE_II, OTHERS, WAEC_2nd, NECO_2nd, NABTEB_2nd, 
    }

    public enum ResultNameType
    {
        Regular_Result, Pharmacy, Medicine_Sugery, Fresh_Std_Nursing_Sc,
    }

    public enum ProgrammeCategory
    {
        UnderGraduate = 1, Masters, Post_Graduate, MBA, Phd, Diploma, Remedial_Science, Preliminary_French, Institute_Of_Education, IJMB, CCE, ICT, Affiliate
    }

    public enum ProgrammeType
    {
        Full_Time = 1, Part_Time
    }

    public enum StudentType
    {
        Full_Time = 1, Part_Time
    }


    public enum StudentSms
    {
        All_Student = 1, Registered_Student, All_Staff
    }

    public enum StaffRole
    {
        Academic = 1, None_Academic
    }

    public enum FileType
    {
        NOTE = 1, MP4, MP3, PDF, DOC, PPT, XLS
    }

    public enum FileTypes
    {
        PDF = 1, DOC, IMAGE
    }
    public enum ApplicantFileType
    {
        PDF = 1, IMAGE
    }
    public enum DayOfTheWeek
    {
        Monday = 1, Teusday, Wednessday, Thursday, Friday, Saturday
    }

    public enum LeaveTime
    {
        Monthly, Yearly
    }
    public enum LeaveStatus
    {
        Approved = 1, DisApproved
    }

    public enum CourseType
    {
        Core = 1, Madatory, Elective
    }
    public enum SchoolFeeCategory
    {
        Acceptance = 1, School_Charges
    }

    public enum PaymentFeeCategory
    {
        Acceptance = 1, School_Charges, Utme_Screeing, Sales_Of_Forms, Id_Card_Charges, Accommodation,Accommodation_application, Change_of_course, Waiver_MSc, 
    }

    public enum SchoolFeeCategory2
    {
        School_Charges = 1,
    }
    public enum SchoolFeePaymentType
    {
        Full_Payment = 0, Part_Payment
    }
    public enum AddressType
    {
        Residential = 1, Office
    }

    public enum EventType
    {
        Schoool = 1, Student, Lecturer, None_Teaching,
    }
    public enum RemitaPaymentType
    {
        MasterCard = 1, Visa, Verve, PocketMoni, POS, BANK_BRANCH, BANK_INTERNET, REMITA_PAY, RRRGEN
    }
    public enum AdmissionPayment
    {
        Acceptance, Supplementary
    }
    public enum CourseChoice
    {
        First, Second
    }
    public enum ServiceType
    {
        Admission_Application, School_Charges = 1, Accepatance_Fee, Supplementary_List_Payment, Hostel_Application_Fee, Acommodation_Payment_Fee,
    }

    public enum StudentStatus
    {
        New_Student = 1, Returning
    }

    public enum StudentCategory
    {
        Student = 1, Utme_Applicant, Sales_Of_Form
    }

    public enum ModeOfEntry
    {
        DE, UTME, PG, RS, PF, PT, IJMB, IOE, DIP, ICT
    }

    public enum PhysicallyChallenge
    {
        Blind, Crippled, Deaf, Dumb
    }

    public enum IndegineStatus
    {
        Nigerian = 1, African, Others
    }

    public enum EmailType
    {
        Sent, Draft
    }

    public enum LgaEnum
    {
        Select_Lga = 1
    }

    public enum DetailPayment
    {
        Change_Email, Remove_Account, Reset_Password, Edit_BioData, ID_Card
    }

    public enum ChangeOfCourseType
    {
        Change_Of_Course, Inter_Faculty_Transfer, Waiver_Phd, Deferment, Downgrade_charge
    }

    public enum SubjectCombination
    {
        [Description("Accounting, Business Management, A/L Maths")]
        Accounting_BusinessManagement_ALMaths,

        [Description("Accounting, Business Management, Economics")]
        Accounting_BusinessManagement_Economics,

        [Description("Accounting, Business Management, Geography")]
        Accounting_BusinessManagement_Geography,

        [Description("Accounting, Business Management, Government")]
        Accounting_BusinessManagement_Government,

        [Description("Accounting, Business Management, Sociology")]
        Accounting_BusinessManagement_Sociology,

        [Description("Accounting, Economics, Geography")]
        Accounting_Economics_Geography,

        [Description("Accounting, Economics, Government")]
        Accounting_Economics_Government,

        [Description("Accounting, Economics, Sociology")]
        Accounting_Economics_Sociology,

        [Description("Accounting, Geography, A/L Maths")]
        Accounting_Geography_ALMaths,

        [Description("Accounting, Geography, Government")]
        Accounting_Geography_Government,

        [Description("CRS, Hausa, Literature")]
        CRS_Hausa_Literature,

        [Description("CRS, History, Literature")]
        CRSHistoryLiterature,

        [Description("Economics, Geography, A/L Maths")]
        Economics_Geography_ALMaths,

        [Description("Economics, Geography, Government")]
        Economics_Geography_Government,

        [Description("Economics, Geography, Sociology")]
        Economics_Geography_Sociology,

        [Description("Economics, Government, A/L Maths")]
        Economics_Government_ALMaths,

        [Description("Economics, Government, Sociology")]
        Economics_Government_Sociology,

        [Description("Geography, Government, A/L Maths")]
        Geography_Government_ALMaths,

        [Description("Geography, Government, Sociology")]
        Geography_Government_Sociology,

        [Description("Geography, History, Sociology")]
        Geography_History_Sociology,

        [Description("Geography, Physics, A/L Maths")]
        Geography_Physics_ALMaths,

        [Description("Government, Hausa, Islamic Studies")]
        Government_Hausa_IslamicStudies,

        [Description("Government, Islamic Studies, Literature")]
        Government_IslamicStudiesLiterature,

        [Description("Hausa, History, Literature")]
        Hausa_History_Literature,

        [Description("Hausa, Islamic Studies, History")]
        Hausa_IslamicStudies_History,

        [Description("Hausa, Islamic Studies, Literature")]
        Hausa_IslamicStudies_Literature,

        [Description("History, Islamic Studies, Literature")]
        History_IslamicStudies_Literature,

        //[Description("Sociology, Arabic, Literature")]
        //Sociology_Arabic_Literature,

        //[Description("Sociology, Hausa, Literature")]
        //Sociology_Hausa_Literature,

        [Description("Accounting, Geography, Sociology")]
        Accounting_Geography_Sociology,

        [Description("Accounting, Government, A/L Maths")]
        Accounting_Government_ALMaths,

        [Description("Accounting, Government, Sociology")]
        Accounting_Government_Sociology,

        //[Description("Arabic, Government, Hausa")]
        //Arabic_Government_Hausa,

        //[Description("Arabic, Government, Islamic Studies")]
        //Arabic_Government_IslamicStudies,

        //[Description("Arabic, Hausa, History")]
        //Arabic_Hausa_History,

        //[Description("Arabic, Hausa, Islamic Studies")]
        //Arabic_HausaIslamic_Studies,

        //[Description("Arabic, Hausa, Literature")]
        //Arabic_Hausa_Literature,

        //[Description("Arabic, History, Islamic Studies")]
        //Arabic_HistoryIslamic_Studies,

        //[Description("Arabic, History, Literature")]
        //Arabic_History_Literature,

        [Description("Biology, Chemistry, A/L Maths")]
        Biology_Chemistry_ALMaths,

        [Description("Biology, Chemistry, Geography")]
        Biology_Chemistry_Geography,

        [Description("Biology, Chemistry, Geology")]
        Biology_Chemistry_Geology,

        [Description("Biology, Chemistry, Physics")]
        Biology_Chemistry_Physics,

        [Description("Business Management, Economics, A/L Maths")]
        BusinessManagement_Economics_ALMaths,

        [Description("Business Management, Economics, Geography")]
        BusinessManagement_Economics_Geography,

        [Description("Business Management, Economics, Government")]
        BusinessManagement_Economics_Government,

        [Description("Business Management, Economics, Sociology")]
        BusinessManagement_Economics_Sociology,

        [Description("Business Management, Geography, A/L Maths")]
        BusinessManagement_Geography_ALMaths,

        [Description("Business Management, Geography, Government")]
        BusinessManagement_Geography_Government,

        [Description("Business Management, Geography, Sociology")]
        BusinessManagement_Geography_Sociology,

        [Description("Business Management, Government, Sociology")]
        BusinessManagement_Government_Sociology,

        [Description("Chemistry, Geography, A/L Maths")]
        Chemistry_Geography_ALMaths,

        [Description("Chemistry, Geography, Physics")]
        Chemistry_Geography_Physics,

        [Description("Chemistry, Geology, A/L Maths")]
        Chemistry_Geology_ALMaths,

        [Description("Chemistry, Geology, Physics")]
        Chemistry_Geology_Physics,

        [Description("Chemistry, Physics, A/L Maths")]
        Chemistry_Physics_ALMaths,

        [Description("CRS, Government, Hausa")]
        CRS_Government_Hausa,

        [Description("CRS, Government, Literature")]
        CRS_Government_Literature,

        [Description("CRS, Hausa, History")]
        CRS_Hausa_History
    }

}