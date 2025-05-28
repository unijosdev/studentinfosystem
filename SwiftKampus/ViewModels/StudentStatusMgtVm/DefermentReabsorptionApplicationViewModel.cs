using SwiftKampusModel;

namespace Unijos.Web.ViewModels.StudentStatus
{
    public class DefermentReabsorptionApplicationViewModel
    {
        public int DefermentReabsorptionId { get; set; }
        public Student Student { get; set; }
        public string SessionName { get; set; }
        public int ChangeOfCoursePaymentId { get; set; }

        public string ReasonForDeferment { get; set; }
    }
}