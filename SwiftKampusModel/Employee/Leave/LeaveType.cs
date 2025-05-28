using System.Collections.Generic;

namespace SwiftKampusModel.Employee.Leave
{
    public class LeaveType
    {
        public int LeaveTypeId { get; set; }
        public string LeaveTypeName { get; set; }
        public int MaximumLeaveCount { get; set; }
        public bool EbanbleCarryForward { get; set; }
        public bool IsActive { get; set; }
        public LeaveTime LeaveTime { get; set; }
        public virtual ICollection<LeaveApplication> LeaveApplications { get; set; }
    }
}
