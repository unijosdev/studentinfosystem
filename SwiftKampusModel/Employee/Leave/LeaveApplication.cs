using System;
using System.ComponentModel.DataAnnotations;

namespace SwiftKampusModel.Employee.Leave
{
    public class LeaveApplication
    {
        public int LeaveApplicationId { get; set; }
        public string StaffId { get; set; }
        public int LeaveTypeId { get; set; }
        [Display(Name = "Starting Date")]
        [Required(ErrorMessage = "Starting date is Required")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Display(Name = "Ending Date")]
        [Required(ErrorMessage = "Ending date is Required")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }
        public DateTime ApplicationDate { get; set; }

        [DataType(DataType.MultilineText)]
        public string ReasonForLeave { get; set; }
        public bool IsHalfDay { get; set; }
        public LeaveStatus LeaveStatus { get; set; }
        public string ReasonForDisApproval { get; set; }
        public bool IsHodApproved { get; set; }
        public bool IsHrApproved { get; set; }
        public string Staus
        {
            get
            {
                if (IsHodApproved.Equals(true) && IsHrApproved.Equals(true))
                {
                    return "Leave Approved";
                }
                if (IsHodApproved.Equals(true) && IsHrApproved.Equals(false))
                {
                    return "Awaiting Registrar's Approval";
                }
                if (IsHodApproved.Equals(false))
                {
                    return "Awaiting HOD's Approval";
                }
                return "Pending Approval";
            }
        }
        public Staff Staff { get; set; }
        public LeaveType LeaveType { get; set; }


    }
}