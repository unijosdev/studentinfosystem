using System.Web.Optimization;

namespace SwiftKampus
{
    public static class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862

        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/scripts").Include(

               //"~/Scripts/jquery-{version}.js",
               "~/AdminTemplate/js/bootstrap.min.js",
               "~/AdminTemplate/js/plugins/metisMenu/jquery.metisMenu.js",
               "~/AdminTemplate/js/plugins/slimscroll/jquery.slimscroll.min.js",
               "~/AdminTemplate/js/plugins/dataTables/datatables.min.js",
               "~/AdminTemplate/js/inspinia.js",
               "~/AdminTemplate/js/plugins/pace/pace.min.js",
               "~/AdminTemplate/js/plugins/jquery-ui/jquery-ui.min.js",
               "~/AdminTemplate/js/plugins/toastr/toastr.min.js"

            ));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                "~/Scripts/jquery.unobtrusive*",
                "~/Scripts/jquery.validate*",
                "~/Scripts/chosen.jquery.js"));

            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                "~/Scripts/modernizr-*"));

            bundles.Add(new StyleBundle("~/Content/css").Include(

                "~/Content/chosen.css",
               "~/AdminTemplate/css/bootstrap.min.css",
               "~/AdminTemplate/css/plugins/dataTables/datatables.min.css",
               "~/AdminTemplate/css/plugins/toastr/toastr.min.css",
               "~/AdminTemplate/js/plugins/gritter/jquery.gritter.css",
               "~/AdminTemplate/css/animate.css",
               "~/AdminTemplate/css/style.css"

            ));

            bundles.Add(new StyleBundle("~/Content/fullcalendarcss").Include(
                "~/Content/themes/jquery.ui.all.css",
                "~/Content/fullcalendar.css"));

            //Calendar Script file

            bundles.Add(new ScriptBundle("~/bundles/fullcalendarjs").Include(
                "~/Scripts/jquery-ui-{version}.js",

                "~/Scripts/bootstrap.js",
                "~/Scripts/bootstrap-modal.js",
                "~/Scripts/fullcalendar.min.js",
                "~/Scripts/alertify.min.js"));

            BundleTable.EnableOptimizations = true;
        }


    }
}
