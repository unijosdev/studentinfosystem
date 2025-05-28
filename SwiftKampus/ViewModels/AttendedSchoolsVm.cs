using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SwiftKampusModel.AddmissionApplicant;

namespace SwiftKampus.ViewModels
{
    public class AttendedSchoolsVm
    {
        public AttendedSchool AttendedSchool { get; set; }
        public List <AttendedSchool> AttendedSchools { get; set; }
        public SchoolProgramme SchoolProgramme { get; set; }
    }
}