using System;
using System.ComponentModel.DataAnnotations;

namespace SwiftKampus.ViewModels
{
    public class UpgradeDeptLevelVm
    {
        public string StudentId { get; set; }
        public int LevelId { get; set; }
        public int SchoolProgrammeId { get; set; }
        public int SessionId { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string MatricNo { get; set; }
        public string PrimaryEmail { get; set; }
        public string ModeOfEntry { get; set; }
        public string Email { get; set; }
        public string Gender { get; set; }

        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }
        public string StudentStatus { get; set; }
        public bool IsPhysicallyChallenged { get; set; }
    }

    public class MakeSchoolarshipVm
    {
        public string StudentId { get; set; }
        public bool? IsSchoolarshipStudent { get; set; }
        public string Schoolarship { get; set; }
    }
}