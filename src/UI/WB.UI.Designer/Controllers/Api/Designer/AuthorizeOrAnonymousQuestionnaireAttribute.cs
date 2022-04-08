using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;

namespace WB.UI.Designer.Controllers.Api.Designer
{
    public class AuthorizeOrAnonymousQuestionnaireAttribute : TypeFilterAttribute
    {
        public AuthorizeOrAnonymousQuestionnaireAttribute(string claimType, string claimValue) : base(typeof(ClaimRequirementFilter))
        {
            Arguments = new object[] { new Claim(claimType, claimValue) };
        }
    }

    public class ClaimRequirementFilter : IAuthorizationFilter
    {
        readonly Claim _claim;

        public ClaimRequirementFilter(Claim claim)
        {
            _claim = claim;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var id = context.ModelState["id"];
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

            bool isAnonymousQuestionnaire =
                viewFactory.IsAnonymousQuestionnaire(questionnaireid, out var originQuestionnaireId);


        }
    }
}