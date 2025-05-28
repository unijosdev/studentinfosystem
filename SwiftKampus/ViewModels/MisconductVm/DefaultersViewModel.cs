using SwiftKampusModel;
using System.Collections.Generic;

namespace SwiftKampus.ViewModels.MisconductVm
{
    public class DefaultersViewModel
    {
        public int DefaulterId { get; set; }
        public string MatriculationNumber { get; set; }

        public string FullName { get; set; }

        public string MisconductName { get; set; }

        public bool CaseTreated { get; set; }

        public string StudentId { get; set; }

        public bool? IsNotified { get; set; }

        public IEnumerable<Student> Students { get; set; }
    }
}