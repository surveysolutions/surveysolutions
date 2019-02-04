﻿using System.Web.Mvc;
using System.Web.Routing;
using WB.UI.Designer.Code.Helpers.Routes;

namespace WB.UI.Designer
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            
            routes.MapRouteLowercase(
                name: "QuestionnaireDetailsNoSection",
                url: "questionnaire/details/{id}/nosection/{entityType}/{entityId}",
                defaults: new { controller = "Questionnaire", action = "DetailsNoSection" });

            routes.MapRouteLowercase(
                name: "QuestionnaireDetails",
                url: "questionnaire/details/{id}/chapter/{chapterId}/{entityType}/{entityId}",
                defaults: new { controller = "Questionnaire", action = "Details" });

            routes.MapRouteLowercase(
                name: "AdminUsersView",
                url: "admin/users",
                defaults: new { controller = "Admin", action = "Index", id = UrlParameter.Optional });

            routes.MapRouteLowercase(
                name: "AdminUsersDelete",
                url: "admin/users/delete",
                defaults: new { controller = "Admin", action = "Index", id = UrlParameter.Optional });

            routes.MapRouteLowercase(
                name: "AdminUsersDetails",
                url: "admin/users/details",
                defaults: new { controller = "Admin", action = "Index", id = UrlParameter.Optional });

            routes.MapRouteLowercase(
                name: "AdminUsersEdit",
                url: "admin/users/edit",
                defaults: new { controller = "Admin", action = "Index", id = UrlParameter.Optional });

            routes.MapRouteLowercase(
                name: "PublicQuestionnaires",
                url: "public",
                defaults: new { controller = "Questionnaire", action = "Public", id = UrlParameter.Optional });

            routes.MapRouteLowercase(
                name: "Default", 
                url: "{controller}/{action}/{id}", 
                defaults: new { controller = "Questionnaire", action = "Index", id = UrlParameter.Optional });
        }
    }
}
