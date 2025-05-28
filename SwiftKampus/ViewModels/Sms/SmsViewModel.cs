using System.ComponentModel.DataAnnotations;

namespace SwiftKampus.ViewModels.Sms
{
    public class SmsViewModel
    {
        public string Destination { get; set; }

        [Display(Name = "Message")]
        [DataType(DataType.MultilineText)]
        public string Message { get; set; }
    }
}