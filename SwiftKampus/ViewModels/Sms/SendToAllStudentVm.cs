using System.ComponentModel.DataAnnotations;

namespace SwiftKampus.ViewModels.Sms
{
    public class SendToAllStudentVm
    {
        [Display(Name = "Message")]
        [DataType(DataType.MultilineText)]
        public string Message { get; set; }
        public string StudentCategory { get; set; }
        public int[] Department { get; set; }
    }
}