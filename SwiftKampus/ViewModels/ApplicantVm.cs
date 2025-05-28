using SwiftKampusModel;
using SwiftKampusModel.AddmissionApplicant;
using SwiftKampusModel.ChangeOfCourse;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Web;

namespace SwiftKampus.ViewModels
{
    public class ApplicantVm
    {
        public string ApplicantId { get; set; }
        public int? SessionId { get; set; }

        [Key]
        public string ApplicantEmail { get; set; }

        public int? SchoolProgrammeId { get; set; }

        public int? AvailableCourseId { get; set; }
        public int? DepartmentId { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "First name cannot be longer than 50 characters.")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Display(Name = "Middle Name")]
        public string MiddleName { get; set; }

        public bool HasPayed { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "Last name cannot be longer than 50 characters.")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        public string Gender { get; set; }

        [Required]
        [DataType(DataType.PhoneNumber)]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Required]
        [Display(Name = "Town Of Birth")]
        public string TownOfBirth { get; set; }


        [Display(Name = "Marital Status")]
        public string MaritalStatus { get; set; }

        [Display(Name = "State of Origin")]
        [Required(ErrorMessage = "State of Origin is Required")]
        public string StateOfOrigin { get; set; }

        [Required(ErrorMessage = "Country is Required")]
        [Display(Name = "Country")]
        public string Country { get; set; }

        [Display(Name = "Date Of Birth")]
        [Required(ErrorMessage = "Date of Birth is Required")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Address is Required")]
        [DataType(DataType.MultilineText)]
        public string Address { get; set; }

        public int Age
        {
            get
            {
                var t = DateTime.Now - DateOfBirth;
                return t.Days / 365;
            }
        }

        [Display(Name = "Full Name")]
        public string UserName => LastName + " " + FirstName;

        [Display(Name = "Full Name")]
        public string FullName => LastName + " " + FirstName + " " + MiddleName;
        public byte[] ApplicatPassport { get; set; }

        public bool IsQualified { get; set; }
        public int NoOfApplication { get; set; }
        public bool? IsDeptApproved { get; set; }
        public string ReasonDeptForRejection { get; set; }

        public bool? IsFacultyApproved { get; set; }
        public string ReasonForFacultyRejection { get; set; }

        public bool? IsPGApproved { get; set; }
        public string ReasonForPGRejection { get; set; }

        public bool? IsVcApproved { get; set; }
        public DateTime? DateVcApproved { get; set; }
        public bool? IsDownloaded { get; set; }
        public bool? IsStudent { get; set; }
        public bool? IsNotify { get; set; }
        public bool IsForeignStudent { get; set; }

        [Display(Name = "Health Status")]
        public HealthStatus HealthStatus { get; set; }


        [Display(Name = "Health Condition")]
        [DataType(DataType.MultilineText)]
        public string HealthCondition { get; set; }

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
                    MemoryStream target = new MemoryStream();

                    if (value.InputStream == null)
                        return;

                    value.InputStream.CopyTo(target);
                    ApplicatPassport = target.ToArray();
                }
                catch (Exception ex)
                {
                    Console.Write(ex.Message);
                }
            }
        }

        public SchoolProgramme SchoolProgramme { get; set; }
        public AvailableCourse AvailableCourse { get; set; }
        public Session Session { get; set; }
        public ICollection<AttendedSchool> AttendedSchools { get; set; }
        public ICollection<ApplicantOLevelResult> ApplicantOLevelResults { get; set; }
        public ICollection<RefreeResponse> RefreeResponses { get; set; }
        public ICollection<ApplicantWaiverPayment> ApplicantWaiverPayments { get; set; }
    }
}