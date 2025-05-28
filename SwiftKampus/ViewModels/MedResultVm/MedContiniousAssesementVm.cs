using SwiftKampus.Models;
using SwiftKampusModel;
using SwiftKampusModel.MedicalScience;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SwiftKampus.ViewModels.MedResultVm
{
    public class MedContiniousAssesementVm
    {
        private readonly SchoolDbContext _db = new SchoolDbContext();

        public int MedContiniousAssesmentId { get; set; }
        public int MedResultCaId { get; set; }
        public int SessionId { get; set; }
        public string StudentId { get; set; }
        public int Score { get; set; }
        public bool Submitted { get; set; }
        public string StaffName { get; set; }
        public bool IsAbsentForExam { get; set; }

        public MedResultCa MedResultCa { get; set; }
        public Session Session
        {
            get
            {
                var session = _db.Sessions.Find(SessionId);
                return session;
            }

        }

        public Student Student
        {
            get
            {
                var student = _db.Students.Find(StudentId);
                return student;
            }
        }

    }
}