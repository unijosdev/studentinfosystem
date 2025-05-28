using System;
using System.ComponentModel.DataAnnotations;

namespace SwiftKampusModel
{
    public class Qualification
    {
        public int QualificationId { get; set; }
        public string UserId { get; set; }

        [Display(Name = "Name of Institution")]
        [Required(ErrorMessage = "Name of Institution is Required")]
        public string NameOfInstitution { get; set; }

        [Display(Name = "Qualification")]
        [Required(ErrorMessage = "Qualification is Required")]
        public string QualificationName { get; set; }

        //[Required(ErrorMessage = "Discipline is Required")]
        public string Discipline { get; set; }

        [Required(ErrorMessage = "Grade is Required")]
        public string Grade { get; set; }

        [Display(Name = "From")]
        [DataType(DataType.Date)]
        public DateTime? FromDate { get; set; }

        [Display(Name = "To")]
        [DataType(DataType.Date)]
        public DateTime? ToDate { get; set; }
    }


    public class Address
    {
        public int AddressId { get; set; }

        public string UserId { get; set; }

        public string AddressType { get; set; }

        [Required]
        public string State { get; set; }

        public string Lga { get; set; }

        [Display(Name = "Name of Town")]
        [StringLength(50, ErrorMessage = "Your Town Name is too long")]
        public string Town { get; set; }

        [Display(Name = "Street Name")]
        [StringLength(70, ErrorMessage = "Your street name is too long")]
        public string Street { get; set; }

        [Display(Name = "House Number")]
        [StringLength(15, ErrorMessage = "Your House number is too long")]
        public string HouseNo { get; set; }

    }


}
