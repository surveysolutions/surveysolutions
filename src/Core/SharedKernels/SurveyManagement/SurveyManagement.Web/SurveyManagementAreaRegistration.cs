using System.Web.Mvc;

namespace WB.Core.SharedKernels.SurveyManagement.Web
{
    public class SurveyManagementAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "SurveyManagement";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "SurveyManagement_default",
                "SurveyManagement/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );

            context.MapRoute(
                "Interview",
                "Interview/{action}/{id}",
                new { controller = "Interview", id = UrlParameter.Optional }
            );
        }
    }
}