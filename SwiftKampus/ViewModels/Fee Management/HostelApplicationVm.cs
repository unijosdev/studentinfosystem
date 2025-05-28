using SwiftKampusModel;

namespace SwiftKampus.ViewModels.Fee_Management
{
    public class HostelApplicationVm : RemitaPostVm
    {
        public string StudentId { get; set; }
        public int SessionId { get; set; }
        public double ExpectedAmount { get; set; }
        public RemitaPaymentType RemitaPaymentType { get; set; }
    }

    public class PrintHostelAcceptanceLetterVm 
    {
        public string StudentId { get; set; }
        public string firstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string MatriculationNumber { get; set; }
        public string BlockName { get; set; }
        public string RoomName { get; set; }
        public string SessionName { get; set; }
        public string ProgrammeName { get; set; }
        public string Hostel { get; set; }
        public string Gender { get; set; }
        public string Department { get; set; }
        public string DateAssigned { get; set; }
        public string ExpectedAmount { get; set; }
        public RemitaPaymentType RemitaPaymentType { get; set; }
    }
}