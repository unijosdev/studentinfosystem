
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwiftKampusModel.Convocation
{
    public class ConvocationList
    {
        public int Id { get; set; }

        public string StudentId { get; set; }

        [Display(Name = "Firstname")]
        public string Corrected_FirstName { get; set; }

        [Display(Name = "Middlename")]
        public string Corrected_MiddleName { get; set; }

        [Display(Name = "LastName")]
        public string Corrected_LastName { get; set; }

        [Display(Name = "Class of Degree")]
        public string ClassOfDegree { get; set; }

        public int ProgrammeId { get; set; }

        public int? OTP { get; set; }

        public int SessionId { get; set; }

        public bool WillAttend { get; set; }

        public bool WillLeaseGown { get; set; }

        public Session Session { get; set; }

        public Student Student { get; set; }

        public Programme Programme { get; set; }
    }
}
