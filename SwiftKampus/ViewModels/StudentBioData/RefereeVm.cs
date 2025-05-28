using SwiftKampusModel;
using System.Collections.Generic;

namespace SwiftKampus.ViewModels.StudentBioData
{
    public class RefereeVm
    {
        public Referee Referee { get; set; }
        public List<Referee> Referees { get; set; }
    }

    public class RefereeFormVm
    {
        public string ApplicantId { get; set; }
        public int RefereeId { get; set; }
        public string ApplicantName { get; set; }
        public string ApplicantPhoneNumber { get; set; }
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
    }
}