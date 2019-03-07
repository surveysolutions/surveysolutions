using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;

namespace WB.UI.Designer.Api
{
    public class QuestionnairePermissionsAttribute : ActionFilterAttribute
    {
        private IQuestionnaireViewFactory QuestionnaireViewFactory => ServiceLocator.Current.GetInstance<IQuestionnaireViewFactory>();
        private ILoggedInUser loggedInUser => ServiceLocator.Current.GetInstance<ILoggedInUser>();

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var id = context.ActionArguments["id"];
            if (id == null || !Guid.TryParse(id.ToString(), out var parsedId))
            {
                context.Result = new NotFoundResult();
            }
            else
            {
                bool hasAccess = loggedInUser.IsAdmin || this.QuestionnaireViewFactory.HasUserAccessToQuestionnaire(parsedId, loggedInUser.Id);
                if (!hasAccess)
                {
                    context.Result = new ForbidResult();
                }
            }
        }
    }
}
