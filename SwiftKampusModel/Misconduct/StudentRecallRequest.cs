using System;
using System.ComponentModel.DataAnnotations;

namespace SwiftKampusModel.Misconduct
{
    public class StudentRecallRequest
    {
        public int StudentRecallRequestId { get; set; }
        public string StudentId { get; set; }
        public int DefualterId { get; set; }
        public ReasonForRequest ReasonForRequest { set; get; }
        public DateTime? DateCreated { get; set; }
        public bool RequestTreated { get; set; }
        public Defaulter Defaulter { get; set; }
        public Student Student { get; set; }

    }

    public enum ReasonForRequest
    {
        select,
        [Display(Name ="Recall By the University")]
        Recall_by_the_varsity,
        [Display(Name = "Suspension Time Elapsed")]
        Suspension_Time_elapsed,
    }
}
