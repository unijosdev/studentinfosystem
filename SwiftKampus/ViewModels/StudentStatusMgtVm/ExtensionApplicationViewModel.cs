using SwiftKampusModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace SwiftKampus.ViewModels.StudentStatusMgtVm
{
    public class ExtensionApplicationViewModel
    {
        public int ExtensionId { get; set; }
        public string StudentId { get; set; }
        public int FacultyId { get; set; }
        public int DepartmentId { get; set; }
        public int? ProgrammeId { get; set; }
        public string FullName { get; set; }
        public Gender Gender { get; set; }
        public string MatricNumber { get; set; }
        public Faculty Faculty { get; set; }
        public Department Department { get; set; }
        public Programme Programme { get; set; }
        public string PrimaryEmail { get; set; }
        public string SecondaryEmail { get; set; }
        public string PhoneNumber { get; set; }
        public string SessionName { get; set; }

        public DateTime YearOfEntry { get; set; }
    }
}