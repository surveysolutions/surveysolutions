﻿using System.Web.Optimization;

namespace WB.UI.Designer
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.UseCdn = false;

            bundles.IgnoreList.Clear();
            bundles.IgnoreList.Ignore("*-vsdoc.js");
            bundles.IgnoreList.Ignore("*intellisense.js");

            bundles.Add(
                new StyleBundle("~/content/css").Include(
                    "~/Content/bootstrap.css",
                    "~/Content/font-awesome.min.css",
                    "~/Content/body.css", 
                    "~/Content/bootstrap-responsive.css", 
                    "~/Content/bootstrap-mvc-validation.css"));

            bundles.Add(
                new ScriptBundle("~/simplepage").Include("~/Scripts/jquery-{version}.js", "~/Scripts/bootstrap.js"));

            bundles.Add(
                new ScriptBundle("~/editform").Include(
                    "~/Scripts/jquery-{version}.js", 
                    "~/Scripts/jquery.validate.js", 
                    "~/Scripts/jquery.validate.unobtrusive-custom-for-bootstrap.js", 
                    "~/Scripts/bootstrap.js"));

            bundles.Add(
                new ScriptBundle("~/list").Include(
                    "~/Scripts/jquery-{version}.js", 
                    "~/Scripts/bootstrap.js",
                    "~/Scripts/knockout-{version}.js", 
                    "~/Scripts/common.js"));

            bundles.Add(
                new ScriptBundle("~/readside").Include(
                    "~/Scripts/knockout-{version}.js",
                    "~/Scripts/lodash.underscore.js",
                    "~/Scripts/moment.js",
                    "~/Scripts/components/moment-duration-format.js",
                    "~/Scripts/components/bindings.js",
                    "~/Scripts/viewmodels/designer.framework.js",
                    "~/Scripts/viewmodels/viewmodelbase.js",
                    "~/Scripts/viewmodels/pagebase.js",
                    "~/Scripts/viewmodels/pages/controlpanel/readside.js"));
        }
    }
}