using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace SwiftKampusModel.StudentStatusManagement
{
    public class Extension
    {
        [Key]
        public int ExtensionId { get; set; }
        public string StudentId { get; set; }    

        [Display(Name ="Period of extension sought")]
        public string PeriodOfExtensionSought { get; set; }

        [Display(Name ="Session into which extension is sought")]
        public int SessionId { get; set; }

        [Display(Name ="Reason(s) for Extension of Duration of Studies")]
        public string ReasonForExtension { get; set; }
        
        public DateTime ApplicationDate { get; set; }

        public string HODComment { get; set; }

        public bool? HoDApproval { get; set; }

        public DateTime? HODCommentDate { get; set; }

        public string FacultyComment { get; set; }

        public bool? FacultyApproval { get; set; }

        public DateTime? FacultyCommentDate { get; set; }

        public string SenateComment { get; set; }

        public bool? SenateApproval { get; set; }

        public DateTime? SenateApprovalDate { get; set; }
        public bool IsApplicationPending { get; set; }
        public Student Student { get; set; }
        public Session Session { get; set; }
        public ICollection<ExtensionDocument> ExtensionDocuments { get; set; }
    }
}
