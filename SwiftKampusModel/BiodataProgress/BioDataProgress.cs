using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwiftKampusModel.BiodataProgress
{
    public class BioDataProgress
    {
        public int BioDataProgressId { get; set; }

        public string StudentId { get; set; }

        public string StudentCategory { get; set; }

        public string Status { get; set; }

        public bool BioData { get; set; }

        public bool PassportSignature { get; set; }

        public bool Olevel { get; set; }

        public bool NextKin { get; set; }

        public bool Address { get; set; }

        public bool Sponsors { get; set; }

        public bool Deexam { get; set; }

        public bool Employment { get; set; }

        public bool SubjectCombination { get; set; }

        public bool UploadDocument { get; set; }

        public bool Aqualification { get; set; }

        public bool AawardPrices { get; set; }

        public bool RelevantQualification { get; set; }

        public bool Publications { get; set; }

        public bool Referee { get; set; }

        public bool Thesis { get; set; }

        public bool Completed { get; set; }

        public Student Student { get; set; }
    }
}
