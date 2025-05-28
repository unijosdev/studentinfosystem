using SwiftKampusModel.AddmissionApplicant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwiftKampusModel
{
    public class RefreeResponse
    {
        public int RefreeResponseId { get; set; }
        public string ApplicantId { get; set; }
        public int RefreeId { get; set; }
        public string RefereeName { get; set; }
        public string Profession { get; set; }
        public string Address { get; set; }
        public string JobDescription { get; set; }
        public string KnownLong { get; set; }
        public string Capacity { get; set; }
        public string Comment { get; set; }
        public string IntellectualCapacity { get; set; }
        public string AcademicStudy { get; set; }
        public string ImaginativeThought { get; set; }
        public string Scholarship { get; set; }
        public string PreviousWork { get; set; }
        public string OralWritting { get; set; }

        public Applicant Applicant { get; set; }
        public Referee Referee { get; set; }
    }
}
