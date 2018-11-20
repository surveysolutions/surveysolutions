using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Resources;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Resources;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.UI.Headquarters.Code;

namespace ASP
{
    [Localizable(false)]
    public static partial class HtmlExtensions
    {
        public static IHtmlString  MainMenuItem(this HtmlHelper html, string actionName, string controllerName, string linkText, MenuItem renderedPage)
        {
            var page = html.ViewBag.ActivePage ?? MenuItem.Logon;
            string isActive = page == renderedPage ? "active" : String.Empty;

            string liStartTag = $"<li class='{isActive}'>\r\n";
            MvcHtmlString part2 = html.ActionLink(linkText, actionName, controllerName, new {area = ""}, new { title = linkText });

            return new MvcHtmlString(liStartTag + part2 + "</li>");
        }

        public static IHtmlString ActivePage(this HtmlHelper html)
        {
            MenuItem page = html.ViewBag.ActivePage ?? MenuItem.Logon;
            return new MvcHtmlString(GetMenuItemTitle(page));
        }

        public static string QuestionnaireName(this HtmlHelper html, string name, long version)
        {
            return string.Format(Pages.QuestionnaireNameFormat, name, version);
        }

        public static string QuestionnaireNameVerstionFirst(this HtmlHelper html, string name, long version)
        {
            return string.Format(Pages.QuestionnaireNameVersionFirst, name, version);
        }

        public static IHtmlString SubstituteQuestionnaireName(this HtmlHelper html,
            string template,
            string questionnaireName)
        {
            if (string.IsNullOrWhiteSpace(template)) return MvcHtmlString.Empty;

            return new MvcHtmlString(template.Replace("%QUESTIONNAIRE%", questionnaireName));
        }

        public static MvcHtmlString HasErrorClassFor<TModel, TProperty>(
            this HtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, TProperty>> expression)
        {
            string expressionText = ExpressionHelper.GetExpressionText(expression);

            string htmlFieldPrefix = htmlHelper.ViewData.TemplateInfo.HtmlFieldPrefix;

            string fullyQualifiedName;

            if (htmlFieldPrefix.Length > 0)
            {
                fullyQualifiedName = string.Join(".", htmlFieldPrefix, expressionText);
            }
            else
            {
                fullyQualifiedName = expressionText;
            }

            bool isValid = htmlHelper.ViewData.ModelState.IsValidField(fullyQualifiedName);

            if (!isValid)
            {
                return MvcHtmlString.Create("has-error");
            }

            return MvcHtmlString.Empty;
        }

        private static string GetMenuItemTitle(MenuItem page)
        {
            switch (page)
            {
                case MenuItem.Surveys: return @MainMenu.SurveysAndStatuses;
                case MenuItem.ApiUsers: return @MainMenu.ApiUsers;
                case MenuItem.DataExport: return @MainMenu.DataExport;
                case MenuItem.Docs: return @MainMenu.Interviews;
                case MenuItem.Headquarters: return @MainMenu.Headquarters;
                case MenuItem.Interviewers: return @MainMenu.Interviewers;
                case MenuItem.InterviewsChart: return @MainMenu.CumulativeChart;
                case MenuItem.ManageAccount: return Strings.SurverManagement_MainMenu_ManageAccount;
                case MenuItem.MapReport: return @MainMenu.MapReport;
                case MenuItem.NumberOfCompletedInterviews: return @MainMenu.Quantity;
                case MenuItem.SpeedOfCompletingInterviews: return @MainMenu.Speed;
                case MenuItem.Observers: return @MainMenu.Observers;
                case MenuItem.Questionnaires: return @MainMenu.SurveySetup;
                case MenuItem.Settings: return Resources.Common.Settings;
                case MenuItem.Summary: return @MainMenu.TeamsAndStatuses;
                case MenuItem.Teams: return @MainMenu.Supervisors;
                case MenuItem.UserBatchUpload: return @MainMenu.UserBatchUpload;
                case MenuItem.Statuses: return @MainMenu.SurveysAndStatuses;
                case MenuItem.Logon: return "Logon";
                case MenuItem.Devices: return "Devices";
                case MenuItem.SyncLog: return "SyncLog";
                case MenuItem.CreateNew: return MainMenu.CreateNew;
                case MenuItem.Started: return MainMenu.Started;
                case MenuItem.Rejected: return MainMenu.Rejected;
                case MenuItem.Completed: return MainMenu.Completed;
                case MenuItem.SurveyAndStatuses: return MainMenu.SurveysAndStatuses;
                case MenuItem.StatusDuration: return MainMenu.StatusDuration;
                case MenuItem.DevicesInterviewers: return MainMenu.DevicesInterviewers;
                case MenuItem.Assignments: return MainMenu.Assignments;
                case MenuItem.AuditLog: return AuditLog.PageTitle;
                case MenuItem.Maps: return MainMenu.Maps;
                case MenuItem.SurveyStatistics: return MainMenu.SurveyStatistics;
                default: return String.Empty;
            }
        }

        private static readonly JsonSerializerSettings asJsonValueSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        public static IHtmlString AsJsonValue(this object obj)
        {
            return new HtmlString(JsonConvert.SerializeObject(obj, asJsonValueSettings));
        }
        
        public static IHtmlString RenderHqConfig(this HtmlHelper helper, object model, string title = null)
        {
            string titleString = title ?? (string) helper.ViewBag.Title?.ToString() ?? null;

            string script = "";

            if (!string.IsNullOrWhiteSpace(titleString))
            {
                script += $"window.CONFIG.title=\"{helper.ToSafeJavascriptMessage(titleString)}\"";
            }

            return new HtmlString($@"<script>{script};window.CONFIG.model={ model?.AsJsonValue() ?? new HtmlString(@"null") }</script>");
        }

        /*public static IHtmlString AuthorizedUserInfoJson(this HtmlHelper helper)
        {
            var authorizedUser = ServiceLocator.Current.GetInstance<IAuthorizedUser>();
            if (authorizedUser == null) return new HtmlString("{}");
            return new HtmlString(JsonConvert.SerializeObject(authorizedUser));
        }*/
    }
}
