using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;

namespace WB.UI.Designer.Controllers.Api.Designer
{
    public class AuthorizeOrAnonymousQuestionnaireAttribute : AuthorizeAttribute
    {
        public AuthorizeOrAnonymousQuestionnaireAttribute() : base("AuthorizeOrAnonymousQuestionnaire")
        {
        }
    }

    public class AuthorizeOrAnonymousQuestionnaireRequirement : AuthorizationHandler<AuthorizeOrAnonymousQuestionnaireRequirement>, IAuthorizationRequirement
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AuthorizeOrAnonymousQuestionnaireRequirement requirement)
        {
            if (context.User.Identity != null && context.User.Identity.IsAuthenticated)
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            var httpContext = context.Resource as HttpContext;
            if (httpContext == null)
            {
                context.Fail();
                return Task.CompletedTask;
            }
            
            
            var routeData = httpContext.GetRouteData();
            var id = routeData?.Values["questionnaireId"] ?? routeData?.Values["id"];
            Guid questionnaireId;
            if (id != null && id is QuestionnaireRevision rev)
            {
                questionnaireId = rev.QuestionnaireId;
            }
            else if (id == null || !Guid.TryParse(id.ToString(), out questionnaireId))
            {
                context.Fail();
                return Task.CompletedTask;
            }

            var viewFactory = httpContext.RequestServices.GetRequiredService<IQuestionnaireViewFactory>();
            bool isAnonymousQuestionnaire = viewFactory?.IsAnonymousQuestionnaire(questionnaireId, out var originQuestionnaireId) ?? false;

            if (!isAnonymousQuestionnaire)
            {
                context.Fail();
                return Task.CompletedTask;
            }

            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}