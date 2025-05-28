using SwiftKampusModel;
using System;
using System.Collections.Generic;

namespace SwiftKampus.ViewModels
{
    public class CorrespondenceIndexVM
    {
        public string StudentFullName { get; set; }

        public string CorrespondenceTypeName { get; set; }

        public DateTime CorrespondenceDate { get; set; }

        public string Subject { get; set; }

        public bool Approval { get; set; }

        public string Reciepient { get; set; }

        public string DeptCode { get; set; }

        public Guid CorrespondenceId { get; set; }
    }

    public class CorrespondenceDetailVM
    {
        public string StudentFullName { get; set; }

        public string StudentMatNumber { get; set; }

        public string CorrespondenceRecepient { get; set; }

        public string RecepientDesignation { get; set; }

        public string CorrespondenceBody { get; set; }

        public string Subject { get; set; }

        public DateTime Date { get; set; }

        public string Faculty { get; set; }

        public string DeptCode { get; set; }

        public string CorrespondenceType { get; set; }

        public bool Status { get; set; }
    }

    public class CorrespondenceCreationVM
    {

        public string Student { get; set; }

        //public IEnumerable<Department> Department { get; set; }

        public string DepartmentCode { get; set; }

        public string CorrespondenceBody { get; set; }

        public int CorrespondenceTypeId { get; set; }

        public IEnumerable<CorrespondenceType> CorrespondenceType { get; set; }

        public string CorrespondenceSubject { get; set; }

        public DateTime CorrespondenceDate { get; set; }

        public bool Approved { get; set; } = false;

        public IEnumerable<Staff> CorrespondenceReciepients { get; set; }
    }
}