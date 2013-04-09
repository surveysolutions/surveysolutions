// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BundleConfig.cs" company="">
//   
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace WB.UI.Designer
{
    using System.Web.Optimization;

    /// <summary>
    /// The bundle config.
    /// </summary>
    public class BundleConfig
    {
        #region Public Methods and Operators

        /// <summary>
        /// The register bundles.
        /// </summary>
        /// <param name="bundles">
        /// The bundles.
        /// </param>
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.UseCdn = false;

            bundles.IgnoreList.Clear();
            bundles.IgnoreList.Ignore("*-vsdoc.js");
            bundles.IgnoreList.Ignore("*intellisense.js");

            bundles.Add(
                new StyleBundle("~/content/edit").Include(
                    "~/Content/details.css", "~/Content/jquery.pnotify.css", "~/Content/jquery.pnotify.icons.css"));

            bundles.Add(
                new StyleBundle("~/content/css").Include(
                    "~/Content/bootstrap.css", 
                    "~/Content/body.css", 
                    "~/Content/bootstrap-responsive.css", 
                    "~/Content/bootstrap-mvc-validation.css"));

            #region [Scripts]

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
                    "~/Scripts/knockout-2.2.1.js", 
                    "~/Scripts/common.js"));

            bundles.Add(
                new ScriptBundle("~/designer").Include(
                    "~/Scripts/jquery-{version}.js", 
                    "~/Scripts/jquery.validate.js", 
                    "~/Scripts/jquery.validate.unobtrusive-custom-for-bootstrap.js", 
                    "~/Scripts/jquery-ui-{version}.js", 
                    "~/Scripts/bootstrap.js", 
                    "~/Scripts/modernizr-{version}.js", 
                    "~/Scripts/lib/json2.js", // IE7 needs this

                    // jQuery plugins
                    "~/Scripts/lib/activity-indicator.js", 
                    "~/Scripts/TrafficCop.js", 
                    "~/Scripts/infuser.js", // depends on TrafficCop

                    // Knockout and its plugins
                    "~/Scripts/knockout-{version}.js", 
                    "~/Scripts/lib/knockout.activity.js", 
                    "~/Scripts/lib/knockout.asyncCommand.js", 
                    "~/Scripts/lib/knockout.dirtyFlag.js", 
                    "~/Scripts/knockout.validation.debug.js", 
                    "~/Scripts/lib/koExternalTemplateEngine.js", 
                    "~/Scripts/lib/knockout-sortable.js", 
                    // Other 3rd party libraries
                    "~/Scripts/lodash.js", 
                    "~/Scripts/moment.js", 
                    "~/Scripts/lib/sammy.js", 
                    "~/Scripts/lib/sammy.title.js", 
                    "~/Scripts/amplify.*", 
                    "~/Scripts/jquery.pnotify.js", 
                    "~/Scripts/bootbox.js", 
                    // Plugins
                    "~/Scripts/lib/jquery.autogrow-textarea.js", 
                    "~/Scripts/lib/Math.uuid.js",
 
                    "~/Scripts/require.js", 
                    "~/Scripts/details/utils.js", 
                    "~/Scripts/details/binder.js", 
                    "~/Scripts/details/bootstrapper.js", 
                    "~/Scripts/details/config.js", 
                    "~/Scripts/details/datacontext.js", 
                    "~/Scripts/details/dataprimer.js", 
                    "~/Scripts/details/dataservice.js", 
                    "~/Scripts/details/ko.bindingHandlers.js", 
                    "~/Scripts/details/ko.debug.helpers.js", 
                    "~/Scripts/details/messenger.js", 
                    "~/Scripts/details/model.answerOption.js", 
                    "~/Scripts/details/model.group.js", 
                    "~/Scripts/details/model.js", 
                    "~/Scripts/details/model.mapper.js", 
                    "~/Scripts/details/model.question.js", 
                    "~/Scripts/details/model.questionnaire.js", 
                    "~/Scripts/details/presenter.js", 
                    "~/Scripts/details/route-mediator.js", 
                    "~/Scripts/details/router.js", 
                    "~/Scripts/details/route-config.js", 
                    "~/Scripts/details/store.js", 
                    "~/Scripts/details/vm.js", 
                    "~/Scripts/details/vm.questionnaire.js", 

                    "~/Scripts/lib/ace/*.js", 

                    "~/Scripts/details/main.js"));

            #endregion
        }

        #endregion
    }
}