using System.Web.Optimization;

namespace WB.UI.Headquarters
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.UseCdn = false;

            bundles.IgnoreList.Clear();
            bundles.IgnoreList.Ignore("*-vsdoc.js");
            bundles.IgnoreList.Ignore("*intellisense.js");

            bundles.Add(new StyleBundle("~/Content/main").Include(
                "~/Content/bootstrap.css",
                "~/Content/font-awesome.min.css",
                "~/Content/bootstrap-mvc-validation.css",
                "~/Content/jquery.pnotify.default.css",
                "~/Content/app.css"
                ));

            bundles.Add(new StyleBundle("~/css/main-not-loggedin").Include(
                "~/Content/bootstrap.css",
                "~/Content/bootstrap-mvc-validation.css",
                "~/Content/main-not-logged.css"
                ));
            bundles.Add(new StyleBundle("~/css/list").Include(
                "~/Content/listview.css"
                ));
            bundles.Add(new StyleBundle("~/css/interview-new").Include(
                "~/Content/bootstrap-editable.css",
                "~/Content/datepicker.css"
                ));
            bundles.Add(new StyleBundle("~/css/interview").Include(
                "~/Content/bootstrap-editable.css",
                "~/Content/datepicker.css"
                ));

            bundles.Add(new StyleBundle("~/css/controlpanel").Include(
                "~/Content/bootstrap.css",
                "~/Content/controlpanel.css"
                ));

            bundles.Add(new ScriptBundle("~/js/main").Include(
                "~/Scripts/jquery-{version}.js",
                "~/Scripts/moment.js",
                "~/Scripts/modernizr-2.8.3.js",
                "~/Scripts/bootstrap.js",
                "~/Scripts/director.js",
                "~/Scripts/knockout-3.4.0.js",
                "~/Scripts/knockout.mapping-latest.js",
                "~/Scripts/lodash.underscore.js",
                "~/Scripts/query-string.js",
                "~/Scripts/supervisor.framework.js",
                "~/Scripts/viewmodels/viewmodelbase.js",
                "~/Scripts/viewmodels/pagebase.js",
                "~/Scripts/viewmodels/surveymanagmentheader.js",
                "~/Scripts/bootbox.min.js"
                ));
            bundles.Add(new ScriptBundle("~/validate").Include(
                "~/Scripts/jquery.validate.js",
                "~/Scripts/jquery.validate.unobtrusive-custom-for-bootstrap.js"
                ));

            bundles.Add(new ScriptBundle("~/js/list").Include(
                "~/Scripts/ko.pager.js",
                "~/Scripts/viewmodels/listview.js",
                "~/Scripts/components/bindings.js",
                "~/Scripts/spin.js",
                "~/Scripts/knockout/ko.spin.js"
                ));

            bundles.Add(new ScriptBundle("~/js/interview-general").Include(
                "~/Scripts/bootstrap-datepicker.js",
                "~/Scripts/Math.uuid.js",
                "~/Scripts/knockout/ko.numericformatter.js",
                "~/Scripts/lodash.underscore.js",
                "~/Scripts/jquery.maskedinput.js",
                "~/Scripts/knockout.validation.min.js",
                "~/Scripts/viewmodels/pages/interview/custom.js"
                ));
            bundles.Add(new ScriptBundle("~/js/interview-new").Include(
                "~/Scripts/bootstrap3-typeahead.js",
                "~/Scripts/knockout/ko.typeahead.js",
                "~/Scripts/components/typeahead-extended.js",
                "~/Scripts/viewmodels/pages/interview/new/datacontext.js",
                "~/Scripts/viewmodels/pages/interview/new/mapper.js",
                "~/Scripts/viewmodels/pages/interview/new/model.js",
                "~/Scripts/viewmodels/pages/interview/new/newinterview.js",
                "~/Scripts/spin.js",
                "~/Scripts/knockout/ko.spin.js"
                ));
            bundles.Add(new ScriptBundle("~/js/details").Include(
                "~/Scripts/bloodhound.js",
                "~/Scripts/bootstrap3-typeahead.js", 
                "~/Scripts/components/typeahead-extended.js", 
                "~/Scripts/knockout/ko.typeahead.js",
                "~/Scripts/components/bindings.js",
                "~/Scripts/director.js",
                "~/Scripts/viewmodels/pages/interview/details/config.js",
                "~/Scripts/viewmodels/pages/interview/details/datacontext.js",
                "~/Scripts/viewmodels/pages/interview/details/interviewdetailssettings.js",
                "~/Scripts/viewmodels/pages/interview/details/details.js"
                ));
            bundles.Add(new ScriptBundle("~/js/map-report").Include(
                "~/Scripts/lodash.underscore.js",
                "~/Scripts/markerclusterer_compiled.js",
                "~/Scripts/infobubble.js",
                "~/Scripts/viewmodels/pages/mapreport.js"
                ));
            bundles.Add(new ScriptBundle("~/js/periodicstatusreport").Include(
                "~/Scripts/moment.js",
                "~/Scripts/components/moment-duration-format.js",
                "~/Scripts/bootstrap-datepicker.js",
                "~/Scripts/ko.datepicker.js",
                "~/Scripts/viewmodels/pages/periodicstatusreport.js"
                ));
            bundles.Add(new ScriptBundle("~/js/synclog").Include(
                "~/Scripts/bootstrap-datepicker.js",
                "~/Scripts/ko.datepicker.js",
                "~/Scripts/bootstrap3-typeahead.js",
                "~/Scripts/components/typeahead-extended.js",
                "~/Scripts/knockout/ko.typeahead.js",
                "~/Scripts/spin.js",
                "~/Scripts/knockout/ko.spin.js",
                "~/Scripts/viewmodels/pages/controlpanel/synchronizationlog.js"
                ));
            bundles.Add(new ScriptBundle("~/js/brokeninterviewpackages").Include(
                "~/Scripts/bootstrap-datepicker.js",
                "~/Scripts/ko.datepicker.js",
                "~/Scripts/bootstrap3-typeahead.js",
                "~/Scripts/components/typeahead-extended.js",
                "~/Scripts/knockout/ko.typeahead.js",
                "~/Scripts/spin.js",
                "~/Scripts/knockout/ko.spin.js",
                "~/Scripts/viewmodels/pages/controlpanel/brokeninterviewpackages.js",
                "~/Scripts/dateRangePicker/daterangepicker.js"
                ));
            bundles.Add(new ScriptBundle("~/js/import-interviews").Include(
                "~/Scripts/bootbox.min.js",
                "~/Scripts/knockout.validation.js",
                "~/Scripts/viewmodels/pages/interview/custom.js",
                "~/Scripts/knockout/ko.numericformatter.js",
                "~/Scripts/bootstrap3-typeahead.js",
                "~/Scripts/components/typeahead-extended.js",
                "~/Scripts/knockout/ko.typeahead.js",
                "~/Scripts/knockout/ko.extenders.js",
                "~/Scripts/spin.js",
                "~/Scripts/knockout/ko.spin.js",
                "~/Scripts/pages/import-interviews.js"
                ));

            bundles.Add(new ScriptBundle("~/js/interviews").Include(
                "~/Scripts/bootbox.min.js",
                "~/Scripts/bootstrap3-typeahead.js",
                "~/Scripts/components/typeahead-extended.js",
                "~/Scripts/knockout/ko.typeahead.js",
                "~/Scripts/viewmodels/pages/interviews.base.js",
                "~/Scripts/viewmodels/pages/interview/hq.interviews.js",
                "~/Scripts/knockout/ko.numericformatter.js"
                ));

            bundles.Add(new ScriptBundle("~/js/interviews-sv").Include(
                "~/Scripts/bootstrap3-typeahead.js", 
                "~/Scripts/components/typeahead-extended.js",
                "~/Scripts/knockout/ko.typeahead.js", 
                "~/Scripts/viewmodels/pages/interviews.base.js",
                "~/Scripts/viewmodels/pages/interview/sv.interviews.js",
                "~/Scripts/knockout/ko.numericformatter.js"
                ));

            bundles.Add(new ScriptBundle("~/js/surveysandstatuses").Include(
                "~/Scripts/bootstrap3-typeahead.js", 
                "~/Scripts/components/typeahead-extended.js",
                "~/Scripts/knockout/ko.typeahead.js", 
                "~/Scripts/viewmodels/pages/surveysandstatuses.js"
                ));

            bundles.Add(new ScriptBundle("~/js/interviewer").Include(
                "~/Scripts/bootstrap3-typeahead.js", 
                "~/Scripts/components/typeahead-extended.js",
                "~/Scripts/knockout/ko.typeahead.js", 
                "~/Scripts/viewmodels/pages/create-interviewer.js"
                ));
            bundles.Add(new ScriptBundle("~/js/interviewers").Include(
                "~/Scripts/bootstrap3-typeahead.js", 
                "~/Scripts/components/typeahead-extended.js",
                "~/Scripts/knockout/ko.typeahead.js", 
                "~/Scripts/viewmodels/pages/interviewers.js"
                ));

            bundles.Add(new ScriptBundle("~/js/users").Include(
                "~/Scripts/viewmodels/pages/users.js"
                ));

            bundles.Add(new ScriptBundle("~/js/teamsandstatuses").Include(
                "~/Scripts/viewmodels/pages/teamsandstatuses.js"
                ));

            bundles.Add(new ScriptBundle("~/js/questionnaires").Include(
                "~/Scripts/viewmodels/pages/questionnaires.js"
                ));

            bundles.Add(new ScriptBundle("~/js/exportdata").Include(
                "~/Scripts/viewmodels/pages/interviews.base.js",
                "~/Scripts/pages/exportdata.js"
                ));

            bundles.Add(new ScriptBundle("~/js/statuslistview").Include(
                "~/Scripts/vm/statuslistview.js"
                ));

            bundles.Add(new ScriptBundle("~/js/chart").Include(
                "~/Scripts/query-string.js", 
                "~/Scripts/jqPlot/jquery.jqplot.js", 
                "~/Scripts/jqPlot/plugins/jqplot.dateAxisRenderer.js", 
                "~/Scripts/jqPlot/plugins/jqplot.highlighter.min.js", 
                "~/Scripts/jqPlot/plugins/jqplot.cursor.js", 
                "~/Scripts/jqPlot/plugins/jqplot.enhancedLegendRenderer.js",
                "~/Scripts/bootstrap-datepicker.js",
                "~/Scripts/ko.datepicker.js",
                "~/Scripts/viewmodels/pages/interviews.base.js",
                "~/Scripts/pages/chart.js",
                "~/Scripts/dateRangePicker/daterangepicker.js"
                ));

            bundles.Add(new ScriptBundle("~/js/preloading").Include(
                "~/Scripts/components/bindings.js", 
                "~/Scripts/pages/userpreloading.js"
                ));

            bundles.Add(new ScriptBundle("~/js/batch-user-creation").Include(
                "~/Scripts/pages/batch-user-creation.js"
                ));

            bundles.Add(new ScriptBundle("~/js/export-settings").Include(
                "~/Scripts/pages/export-settings.js"
                ));
            
        }
    }
}