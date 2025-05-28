using System;
using System.ComponentModel.DataAnnotations;

namespace SwiftKampusModel.AddmissionApplicant
{
    public class Publication
    {
        public int PublicationId { get; set; }

        public string UserId { get; set; }

        [Required]
        public string Title { get; set; }
      
        public string Institution { get; set; }
        public string PublicationType { get; set; }
        public string Qualification { get; set; }
        public string Publisher { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "To Date")]
        public DateTime PublishedDate { get; set; }


    }
}
