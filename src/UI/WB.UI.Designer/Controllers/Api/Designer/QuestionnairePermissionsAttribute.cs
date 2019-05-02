using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.UI.Designer1.Extensions;

namespace WB.UI.Designer.Controllers.Api.Designer
{
    public class QuestionnairePermissionsAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var id = context.ActionArguments["id"];
            if (id == null || !Guid.TryParse(id.ToString(), out var parsedId))
            {
                context.Result = new NotFoundResult();
            }
            else
            {
                var viewFactory = context.HttpContext.RequestServices.GetService<IQuestionnaireViewFactory>();
                bool hasAccess = context.HttpContext.User.IsAdmin() || 
                                 viewFactory.HasUserAccessToQuestionnaire(parsedId, context.HttpContext.User.GetId());
                if (!hasAccess)
                {
                    context.Result = new JsonResult(new { message = ExceptionMessages.NoPremissionsToEditQuestionnaire })
                    {
                        StatusCode = StatusCodes.Status403Forbidden
                    };                                        
                }
            }
        }        
    }
}
