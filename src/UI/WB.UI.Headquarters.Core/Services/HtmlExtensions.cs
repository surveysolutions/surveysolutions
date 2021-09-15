using System;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Infrastructure.Native.Workspaces;
using WB.UI.Headquarters.Resources;

namespace WB.UI.Headquarters.Services
{
    public static class HtmlExtensions
    {
        private static readonly JsonSerializerSettings asJsonValueSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };
        
        public static IHtmlContent MainMenuItem(this IHtmlHelper html, string actionName, string controllerName, string linkText, MenuItem renderedPage)
        {
            var page = html.ViewBag.ActivePage ?? MenuItem.Logon;
            var actionLink = html.ActionLink(linkText, actionName, controllerName, new { area = "", id = "" }, new { title = linkText });

            TagBuilder tag = new TagBuilder("li");

            if (page == renderedPage)
            {
                tag.AddCssClass("active");
            }

            tag.InnerHtml.AppendHtml(actionLink);
            return tag;
        }
        
        public static IHtmlContent MainMenuItem(this IHtmlHelper html, 
            WorkspaceContext workspace, string actionName, string controllerName,
            string linkText, MenuItem renderedPage)
        {
            var urlFactory = html.ViewContext.HttpContext.RequestServices.GetRequiredService<IUrlHelperFactory>();
            var urlHelper = urlFactory.GetUrlHelper(html.ViewContext);
            var actionLinkUrl = urlHelper.Action(actionName, controllerName, new { id = "" });

            var context = html.ViewContext.HttpContext.RequestServices.GetWorkspaceContext();
            
            if (context != null)
            {
                var prefix = $"{context.PathBase}/{context.Name}";
                
                if (actionLinkUrl.StartsWith(prefix))
                {
                    actionLinkUrl = $"{context.PathBase}/{workspace.Name}/" +
                                    $"{actionLinkUrl.Substring(prefix.Length).TrimStart('/')}";
                }
            }
            
            var page = html.ViewBag.ActivePage ?? MenuItem.Logon;
         
            var actionLink = new TagBuilder("a");
            actionLink.Attributes.Add("title", linkText);
            actionLink.Attributes.Add("href", actionLinkUrl);
            actionLink.InnerHtml.Append(linkText);

            TagBuilder tag = new TagBuilder("li");

            if (page == renderedPage)
            {
                tag.AddCssClass("active");
            }

            tag.InnerHtml.AppendHtml(actionLink);
            return tag;
        }
        
        public static HtmlString ActivePage(this IHtmlHelper html)
        {
            MenuItem page = html.ViewBag.ActivePage ?? MenuItem.Logon;
            return new HtmlString(GetMenuItemTitle(page));
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
                case MenuItem.ChangePassword: return Strings.SurverManagement_MainMenu_ChangePassword;
                default: return String.Empty;
            }
        }

        public static IHtmlContent AsJsonValue(this object obj)
        {
            return new HtmlString(JsonConvert.SerializeObject(obj, asJsonValueSettings));
        }
    }
}
