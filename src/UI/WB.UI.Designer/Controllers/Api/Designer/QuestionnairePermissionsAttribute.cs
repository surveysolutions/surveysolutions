using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using WB.Core.BoundedContexts.Designer;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.UI.Designer.Extensions;

namespace WB.UI.Designer.Controllers.Api.Designer
{
    public class QuestionnairePermissionsAttribute : ActionFilterAttribute
    {
        private readonly bool write;

        public QuestionnairePermissionsAttribute(bool write = false)
        {
            this.write = write;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ActionArguments.ContainsKey("id"))
                return;
                
            var id = context.ActionArguments["id"];

            Guid questionnaireid;
            if (id != null && id is QuestionnaireRevision rev)
            {
                questionnaireid = rev.QuestionnaireId;
            } 
            else if (id == null || !Guid.TryParse(id.ToString(), out questionnaireid))
            {
                context.Result = new NotFoundResult();
                return;
            }

            var viewFactory = context.HttpContext.RequestServices.GetRequiredService<IQuestionnaireViewFactory>();
            var httpContextUser = context.HttpContext.User;

            bool hasAnonymousAccess = viewFactory.IsAnonymousQuestionnaire(questionnaireid, out var originQuestionnaireId);
            if (hasAnonymousAccess && originQuestionnaireId.HasValue && id is QuestionnaireRevision r)
                r.MarkAsAnonymousQuestionnaire(originQuestionnaireId.Value);

            if (write)
            {
                bool hasWriteAccess = viewFactory.HasUserChangeAccessToQuestionnaire(questionnaireid, httpContextUser.GetId());
                if (!hasWriteAccess)
                {
                    context.Result = new JsonResult(new { message = ExceptionMessages.NoPremissionsToEditQuestionnaire })
                    {
                        StatusCode = StatusCodes.Status403Forbidden
                    };
                }
                return;
            }
            
            if (!httpContextUser.Identity!.IsAuthenticated && !hasAnonymousAccess)
            {
                context.Result = new JsonResult(new { message = ExceptionMessages.NoPremissionsToEditQuestionnaire })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
                return;
            }

            bool hasAccess = hasAnonymousAccess ||
                  httpContextUser.IsAdmin() ||
                  viewFactory.HasUserAccessToQuestionnaire(questionnaireid, httpContextUser.GetId());
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
