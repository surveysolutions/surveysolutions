using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Tests.Abc;
using WB.UI.Designer.Controllers.Api.Designer;

namespace WB.Tests.Unit.Designer.Applications;

[TestOf(typeof(AuthorizeOrAnonymousQuestionnaireRequirement))]
public class AuthorizeOrAnonymousQuestionnaireRequirementTests
{
    [Test]
    public async Task when_anonymous_try_to_open_questionnaire_with_anonymous_access_should_allow()
    {
        var context = CreateContext(questionnaireId: new QuestionnaireRevision(Id.g1),
            isAnonymousQuestionnaire: true);
        var requirement = CreateRequirement();
        
        await requirement.HandleAsync(context);
        
        Assert.That(context.HasSucceeded, Is.True);
    }
    
    [Test]
    public async Task when_anonymous_try_to_open_questionnaire_without_page_without_anonymous_access_should_dont_allow()
    {
        var context = CreateContext(questionnaireId: new QuestionnaireRevision(Id.g1));
        var requirement = CreateRequirement();
        
        await requirement.HandleAsync(context);
        
        Assert.That(context.HasFailed, Is.True);
    }

    [Test]
    public async Task when_anonymous_try_to_open_page_without_questionnaire_id_should_dont_allow()
    {
        var context = CreateContext();
        var requirement = CreateRequirement();
        
        await requirement.HandleAsync(context);
        
        Assert.That(context.HasFailed, Is.True);
    }
    
    
    private static AuthorizationHandlerContext CreateContext(QuestionnaireRevision questionnaireId = null,
        bool isAnonymousQuestionnaire = false)
    {
        var routeValueDictionary = new RouteValueDictionary();
        routeValueDictionary.Add("id", questionnaireId);
        var routeData = new RouteData(routeValueDictionary);
        var features = new FeatureCollection();
        var routingFeature = Mock.Of<IRoutingFeature>(f => f.RouteData == routeData);
        features.Set(routingFeature);

        var originId = isAnonymousQuestionnaire ? Id.g2 : (Guid?)null;
        var questionnaireViewFactory = questionnaireId == null
            ? Mock.Of<IQuestionnaireViewFactory>()
            : Mock.Of<IQuestionnaireViewFactory>(q => q.IsAnonymousQuestionnaire(questionnaireId.QuestionnaireId, out originId) == isAnonymousQuestionnaire);
        var requestServices = Mock.Of<IServiceProvider>(s => s.GetService(typeof(IQuestionnaireViewFactory)) == questionnaireViewFactory);
        
        var httpContext = Mock.Of<HttpContext>(c => c.Features == features 
            && c.RequestServices == requestServices);

        var user = Mock.Of<ClaimsPrincipal>();
        var context = new AuthorizationHandlerContext(new IAuthorizationRequirement[] { Mock.Of<AuthorizeOrAnonymousQuestionnaireRequirement>() },
            user, httpContext);
        return context;
    }

    private AuthorizeOrAnonymousQuestionnaireRequirement CreateRequirement()
    {
        return new AuthorizeOrAnonymousQuestionnaireRequirement();
    }
}