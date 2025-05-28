using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Web;

namespace SwiftKampusModel.AddmissionApplicant
{
    public class UtmeApplicant
    {
        [Key]
        [Index(IsUnique = true)]
        [MaxLength(20)]
        public string JambRegNo { get; set; }
        public int SessionId { get; set; }
        public int? SchoolProgrammeId { get; set; }
        public string Surname { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string StateOfOrigin { get; set; }
        public string LocalGovtArea { get; set; }
        public string Gender { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public int Age { get; set; }
        public int? ProgrammeId { get; set; }
        public bool IsDirectEntry { get; set; }
        public string ResultGrade { get; set; }
        public bool HasRegistered { get; set; }
        public bool HasPayed { get; set; }
        public double? OlevelScore { get; set; }
        public double? OlevelPercentage { get; set; }
        public double? JambPercentage { get; set; }
        public double? Cummulative { get; set; }
        public string FullName => Surname + " " + FirstName + " " + MiddleName;

        public byte[] Passport { get; set; }
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }
        
        [Display(Name = "Upload A Passport/Picture")]
        [ValidateFile(ErrorMessage = "Please select a PNG/JPEG image smaller than 20kb")]
        [NotMapped]
        public HttpPostedFileBase File
        {
            get
            {
                return null;
            }

            set
            {
                try
                {
                    var target = new MemoryStream();

                    if (value.InputStream == null)
                        return;

                    value.InputStream.CopyTo(target);
                    Passport = target.ToArray();
                }
                catch (Exception ex)
                {
                    var message = ex.Message;
                }
            }
        }

        public SchoolProgramme SchoolProgramme { get; set; }
        public Session Session { get; set; }
        public Programme Programme { get; set; }
        public List<UtmeApplicantSubject> UtmeApplicantSubjects { get; set; }

    }
}