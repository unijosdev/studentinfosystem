using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwiftKampusModel.Convocation
{
    public class AcademicGownLease
    {
        public int Id { get; set; }

        public string StudentId { get; set; }

        public string LeaseType { get; set; }

        public double GownHeight { get; set; }

        public int SessionId { get; set; }

        public bool HasPaid { get; set; }

        public bool HasCollected { get; set; }

        public bool HasReturned { get; set; }

        public Student Student { get; set; }
        public Session Session { get; set; }
    }
}
