using System;
using System.Collections.Generic;

namespace SwiftKampusModel
{
    public class Correspondence
    {
        public Correspondence()
        {
            CorrespondenceId = Guid.NewGuid();
        }

        public Guid CorrespondenceId { get; set; }

        public Student Student { get; set; }

        public string StudentId { get; set; }

        public Department Department { get; set; }

        public int DepartmentId { get; set; }

        public string CorrespondenceBody { get; set; }

        public int CorrespondenceTypeId { get; set; }

        public CorrespondenceType CorrespondenceType { get; set; }

        public string CorrespondenceSubject { get; set; }

        public DateTime CorrespondenceDate { get; set; }

        public bool Approved { get; set; } = false;

        public List<Staff> CorrespondenceReciepients { get; set; }

    }
}
