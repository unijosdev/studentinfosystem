using System;
using System.Web;

namespace SwiftKampus.ViewModels
{
    public class FileUploadVm
    {
        public string[] FileName { get; set; }
        public HttpPostedFileBase[] FileUpload { get; set; }
    }

    public class ApplicantEditVm
    {
        public string FileName { get; set; }
        public HttpPostedFileBase FileUpload { get; set; }
    }

    public class ApplicanEditUpload
    {
        public string ApplicantId { get; set; }
        public int SchoolProgrammeId { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string PhoneNumber { get; set; }
        public string Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string StateOfOrigin { get; set; }
        public string TownOfBirth { get; set; }
        public string Lga { get; set; }
        public string Country { get; set; }
        public int AvailableCourseId { get; set; }
        public string Address { get; set; }
        public string HealthStatus { get; set; }
        public string HealthCondition { get; set; }
    }
}