using SwiftKampusModel;
using SwiftKampusModel.AddmissionApplicant;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Web;

namespace SwiftKampus.ViewModels
{
    public class StudentViewModel
    {
        [Display(Name = "Student ID")]
        [Required(ErrorMessage = "Your Student ID Number is required")]
        [StringLength(25, ErrorMessage = "Your Student ID is too long")]
        public string StudentId { get; set; }

        [Display(Name = "First Name")]
        [Required(ErrorMessage = "Your First Name is required")]
        [StringLength(50, ErrorMessage = "Your First Name is too long")]
        public string FirstName { get; set; }

        [Display(Name = "Middle Name")]
        public string MiddleName { get; set; }

        [Display(Name = "Last Name")]
        [Required(ErrorMessage = "Your Last Name is required")]
        [StringLength(50, ErrorMessage = "Your Last Name is too long")]
        public string LastName { get; set; }

        public int? ProgrammeId { get; set; }

        [Display(Name = "Mobile Number")]
        [DataType(DataType.PhoneNumber)]
        [Required(ErrorMessage = "Phone Number is required")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Date of Birth")]
        [Required(ErrorMessage = "Your Date of Birth is required")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Display(Name = "Enrollment Date")]
        public DateTime? EnrollmentDate { get; set; }

        [Display(Name = "Place of Birth")]
        public string PlaceOfBirth { get; set; }

        [Display(Name = "State of Origin")]
        public State StateOfOrigin { get; set; }

        public Gender Gender { get; set; }
        public StudentType StudentType { get; set; }

        [Display(Name = "Current Level")]
        public int LevelId { get; set; }

        [Display(Name = "Religion")]
        public Religion Religion { get; set; }

        [Display(Name = "Tribe")]
        public string Tribe { get; set; }

        [Display(Name = "Town Of Birth")]
        public string TownOfBirth { get; set; }

        [Display(Name = "Nationality")]
        public string Nationality { get; set; }

        public string UserName
        {
            get { return $"{LastName} {FirstName}"; }          
        }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare(nameof(Password), ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public byte[] StudentPassport { get; set; }

        [Display(Name = "Upload A Passport/Picture")]
        [ValidateFile(ErrorMessage = "Please select a PNG/JPEG image smaller than 1MB")]
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
                    StudentPassport = target.ToArray();
                }
                catch (Exception ex)
                {
                    Console.Write(ex.Message);
                    //logger.Error(ex.Message);
                    //logger.Error(ex.StackTrace);
                }
            }
        }
    }

    public class StudentPartialVm
    {
        public List<ApplicantOLevelResult> ApplicantOLevelResults { get; set; }
        public Student Student { get; set; }
    }

    public class StudentSchChargesAndCourseRegVm
    {
        public string DeptName { get; set; }
        public string Programme { get; set; }
        public string Level { get; set; }
        public string MatricNum { get; set; }
        public string fullname { get; set; }
        public string PaymentSatus { get; set; }
        public string CourseRegStatus { get; set; }
    }

    public class StudentEditViewModel
    {       
        public string StudentId { get; set; }
        public string MatricNo { get; set; }
        public string JambRegNo { get; set; }
        public int? LevelId { get; set; }
        public DateTime? EnrollmentDate { get; set; }

        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Display(Name = "Middle Name")]
        public string MiddleName { get; set; }

        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        public int? ProgrammeId { get; set; }

        [Display(Name = "Mobile Number")]
        [DataType(DataType.PhoneNumber)]
        [Required(ErrorMessage = "Phone Number is required")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Date of Birth")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        [Required(ErrorMessage = "Your Date of Birth is required")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }


        [Display(Name = "Place of Birth")]
        public string PlaceOfBirth { get; set; }

        [Display(Name = "State of Origin")]
        [Required(ErrorMessage = "Your State of Origin is required")]
        public string StateOfOrigin { get; set; }

        [Display(Name = "Local Govt Area")]
        [Required(ErrorMessage = "Your LGA is required")]
        public string Lga { get; set; } 
        
        [Display(Name = "Marital Status")]
        //[Required()]
        public string MaritalStatus { get; set; }

        [Display(Name = "Gender")]
        [Required(ErrorMessage = "Your Gender is required")]
        public string Gender { get; set; }

        [Display(Name = "Religion")]
        [Required(ErrorMessage = "Your Religion is required")]
        public string Religion { get; set; }

        [Display(Name = "Tribe")]
        public string Tribe { get; set; }

        [Display(Name = "Town Of Birth")]
        //[Required()]
        public string TownOfBirth { get; set; }

        [Display(Name = "Hobby")]
        //[Required(ErrorMessage = "Your hobbies are required")]
        public string Hobby { get; set; }

        [Display(Name = "Nationality")]
        [Required(ErrorMessage = "Your nationality is required")]
        public string Nationality { get; set; }
        
        [Display(Name = "Is Physically Challenged")]
        public bool IsPhysicallyChallenged { get; set; } 
        
        [Display(Name = "Physical Challenge")]
        public string PChallengedDetail { get; set; }

       
       
    }

    public class StudentSpillVm
    {
        public string MatricNum { get; set; }
        public decimal Amount { get; set; }
        public string session { get; set; }

        public StudentSpillVm(string v1, decimal v2, string v3)
        {
            this.MatricNum = v1;
            this.Amount = v2;
            this.session = v3;
        }

        
    }
}