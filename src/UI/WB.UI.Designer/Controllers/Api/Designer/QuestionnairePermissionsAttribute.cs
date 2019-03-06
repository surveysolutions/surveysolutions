using System;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using WB.Core.BoundedContexts.Designer.Implementation.Services.Accounts.Membership;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.UI.Designer.Resources;

namespace WB.UI.Designer.Api
{
    public class QuestionnairePermissionsAttribute : ActionFilterAttribute
    {
        private IMembershipUserService UserHelper => ServiceLocator.Current.GetInstance<IMembershipUserService>();

        private IQuestionnaireViewFactory QuestionnaireViewFactory => ServiceLocator.Current.GetInstance<IQuestionnaireViewFactory>();

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var id = actionContext.ControllerContext.RouteData.Values["id"];
            Guid parsedId;
            if (id == null || !Guid.TryParse(id.ToString(), out parsedId))
            {
                actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.NotFound, ErrorMessages.NoQuestionnaireIdToValidatePremissions);
            }
            else
            {
                IMembershipWebUser user = this.UserHelper.WebUser;
                bool hasAccess = user.IsAdmin || this.QuestionnaireViewFactory.HasUserAccessToQuestionnaire(parsedId, user.UserId);
                if (!hasAccess)
                {
                    actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.Forbidden, ErrorMessages.NoAccessToQuestionnaire);
                }
            }
        }
    }
}