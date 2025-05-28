using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwiftKampusModel.Misconduct
{
    public class StudentDisciplinaryStatus
    {
        [Key, ForeignKey("Defaulter")]
        public int DefaulterId { get; set; }

        [Display(Name = "Disciplinary Status")]
        public string DisciplineName { get; set; }
        public bool StillValid { get; set; }
        public DateTime? DateSetActive { get; set; }
        public DateTime? DateSetInActive { get; set; }
        public string ReasonSetToInActive { get; set; }
        public string StaffId { get; set; }

        [Display(Name = "Amount to pay for vandalised property")]
        public double AmountToPay { get; set; }

        public int recallSession { get; set; }
        public Staff Staff { get; set; }
        public Defaulter Defaulter { get; set; }

    }
}
