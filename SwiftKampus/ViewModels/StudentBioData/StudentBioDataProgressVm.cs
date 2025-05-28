using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SwiftKampus.ViewModels.StudentBioData
{
    public class StudentBioDataProgressVm
    {
        public string StudentCategory { get; set; }

        public string Status { get; set; }

        public bool BioData { get; set; }

        public bool PassportSignature { get; set; }

        public bool Olevel { get; set; }

        public bool NextKin { get; set; }

        public bool AddressFilled { get; set; }

        public bool Sponsors { get; set; }

        public bool Deexam { get; set; }

        public bool Employment { get; set; }

        public bool SubjectCombination { get; set; }

        public bool UploadDocument { get; set; }

        public bool Aqualification { get; set; }

        public bool AawardPrices { get; set; }

        public bool RelevantQualification { get; set; }

        public bool Publications { get; set; }

        public bool RefereeFilled { get; set; }

        public bool Thesis { get; set; }

        public bool Completed { get; set; }
    }
}