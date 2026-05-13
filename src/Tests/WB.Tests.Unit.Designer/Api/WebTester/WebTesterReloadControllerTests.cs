using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Tests.Unit.Designer;
using WB.UI.Designer.Controllers.Api.WebTester;
using WB.UI.Designer.Services;

namespace WB.Tests.Unit.Designer.Api.WebTester;

[TestFixture]
[TestOf(typeof(WebTesterReloadController))]
public class WebTesterReloadControllerTests
{
    [Test]
    public void when_reloading_questionnaire_should_preserve_target_and_hash_in_redirect_url()
    {
        var questionnaireId = Guid.NewGuid();
        var interviewId = Guid.NewGuid().ToString();
        var token = Guid.NewGuid().ToString();
        var revision = new QuestionnaireRevision(questionnaireId, version: 1);
        var questionnaireView = Create.QuestionnaireView();

        var questionnaireViewFactory = Mock.Of<IQuestionnaireViewFactory>(x => x.Load(revision) == questionnaireView);
        var webTesterService = Mock.Of<IWebTesterService>(x => x.CreateTestQuestionnaire(questionnaireId) == token);
        var controller = new WebTesterReloadController(
            Mock.Of<IOptions<WebTesterSettings>>(x => x.Value == new WebTesterSettings { BaseUri = "https://webtester.example/WebTester/Run" }),
            questionnaireViewFactory,
            webTesterService);

        var result = controller.Index(revision, interviewId, "/Section/11111111-1111-1111-1111-111111111111", "#22222222-2222-2222-2222-222222222222");

        var redirect = (RedirectResult)result;
        Assert.That(redirect.Url, Is.EqualTo(
            $"https://webtester.example/WebTester/Run/{token}?sid={interviewId}&target=%2FSection%2F11111111-1111-1111-1111-111111111111&hash=%2322222222-2222-2222-2222-222222222222"));
    }

    [Test]
    public void when_reloading_without_location_should_redirect_with_interview_only()
    {
        var questionnaireId = Guid.NewGuid();
        var interviewId = Guid.NewGuid().ToString();
        var token = Guid.NewGuid().ToString();
        var revision = new QuestionnaireRevision(questionnaireId, version: 1);
        var questionnaireView = Create.QuestionnaireView();

        var questionnaireViewFactory = Mock.Of<IQuestionnaireViewFactory>(x => x.Load(revision) == questionnaireView);
        var webTesterService = Mock.Of<IWebTesterService>(x => x.CreateTestQuestionnaire(questionnaireId) == token);
        var controller = new WebTesterReloadController(
            Mock.Of<IOptions<WebTesterSettings>>(x => x.Value == new WebTesterSettings { BaseUri = "https://webtester.example/WebTester/Run" }),
            questionnaireViewFactory,
            webTesterService);

        var result = controller.Index(revision, interviewId);

        var redirect = (RedirectResult)result;
        Assert.That(redirect.Url, Is.EqualTo($"https://webtester.example/WebTester/Run/{token}?sid={interviewId}"));
    }
}
