using System.Web.Optimization;

namespace InfoSpace.DeviceLab.Web
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/content/sitejs")
                .Include("~/Scripts/DevicesAjax.js")
                .Include("~/Scripts/JobAjax.js")
            );

            bundles.Add(new LessBundle("~/content/sitecss")
                .Include("~/Content/css/site.less")
            );
        }
    }
}
