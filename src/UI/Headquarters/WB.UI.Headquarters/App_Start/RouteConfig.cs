using System;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace WB.UI.Headquarters
{
    public class InterviewIdConstraint : IRouteConstraint
    {
        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values,
            RouteDirection routeDirection)
        {
            if (routeDirection == RouteDirection.IncomingRequest && parameterName == @"id")
            {
                var sInterviewId = values[@"id"].ToString();
                // If interviewId param is an Guid
                Guid id;
                if (Guid.TryParse(sInterviewId, out id))
                    return true;

                //If interviewId param is an humanID like 55-44-33-22-11
                return Regex.IsMatch(sInterviewId, @"^\d{2}-\d{2}-\d{2}-\d{2}-\d{2}?$");
            }

            return false;
        }
    }

    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute(@"{resource}.axd/{*pathInfo}");

            routes.MapRoute(@"WebInterview.Start", @"WebInterview/Start/{id}", new { controller = @"WebInterview", action = @"Start" },
                constraints: new { id = new InterviewIdConstraint() });
            routes.MapRoute(@"WebInterview.Finish", @"WebInterview/Finish/{id}", new { controller = @"WebInterview", action = @"Finish" },
                constraints: new { id = new InterviewIdConstraint() });
            routes.MapRoute(@"WebInterview.Resume", @"WebInterview/{id}/Section/{sectionId}", 
                defaults: new { controller = @"WebInterview", action = @"Section" },
                constraints: new { id = new InterviewIdConstraint() });
            routes.MapRoute(@"WebInterview", @"WebInterview/{id}/Cover",
                defaults: new { controller = @"WebInterview", action = @"Cover" },
                constraints: new { id = new InterviewIdConstraint() });
            routes.MapRoute(@"WebInterview.Complete", @"WebInterview/{id}/Complete",
                defaults: new { controller = @"WebInterview", action = @"Complete" },
                constraints: new { id = new InterviewIdConstraint() });

            routes.MapRoute(@"WebInterview.ImageAnswering", @"WebInterview/image", new { controller = @"WebInterview", action = @"Image" });

            routes.MapRoute(@"Default", @"{controller}/{action}/{id}",
                new { controller = @"Account", action = @"Index", id = UrlParameter.Optional });
        }
    }
}