//namespace SwiftKampus.Services
//{
//    public class FeePaymentAttribute : ActionFilterAttribute
//    {
//        public bool HasPayed { get; set; }
//        public override void OnActionExecuting(ActionExecutingContext filterContext)
//        {
//            if (!HasPayed)
//            {
//                var controller = (BaseController)filterContext.Controller;
//                filterContext.Result = controller.RedirectToAction("Index", "Home");
//            }

//            // Finishes executing the Action as normal 
//            base.OnActionExecuting(filterContext);
//        }
//    }
//}