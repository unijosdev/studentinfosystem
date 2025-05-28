using System;
using System.Globalization;

namespace SwiftKampus.ViewModels.StudentStatusMgtVm
{
    public class StudentUploadApiVm
    {
        public string MatricNo { get; set; }
        public string JambRegNo { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string StateOfOrigin { get; set; }
        public string Nationality { get; set; }
        public string DateOfBirth { get; set; }
        public string EnrollmentDate { get; set; }
        public string Gender { get; set; }
        public string ProgrammeCode { get; set; }
        public string LevelName { get; set; }
        public string StudentStatus { get; set; }
        public string SchoolProgrammeCode { get; set; }
        public string SessionName { get; set; }
        public string ModeOfEntry { get; set; }
        public string MaritalStatus { get; set; }
        public string ImageUrl { get; set; }
        public string PassportUrl { get; set; }

        public DateTime DateOfBirthOrg
        {
            get
            {
                return (DateTime)ConvertToDateTime(DateOfBirth);
            }
        }
        public DateTime? EnrollmentDateOrg
        {
            get
            {
                return ConvertToDateTime(EnrollmentDate);
            }
        }


        public DateTime? ConvertToDateTime(string passedDateTime)
        {
            CultureInfo provider = CultureInfo.InvariantCulture;
            try
            {
                DateTime dateTime = DateTime.ParseExact(passedDateTime, new string[] { "yyyy.MM.dd", "yyyy-MM-dd", "yyyy/MM/dd" }, provider, DateTimeStyles.None);
                return dateTime;
            }
            catch (Exception)
            {
                return new DateTime(1990, 1, 18);
            }
        }
    }
}