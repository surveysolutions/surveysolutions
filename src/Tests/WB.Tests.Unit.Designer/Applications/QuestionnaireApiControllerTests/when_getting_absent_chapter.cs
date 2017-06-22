using System;
using System.Web.Http;
using Machine.Specifications;
using WB.UI.Designer.Api;

namespace WB.Tests.Unit.Designer.Applications.QuestionnaireApiControllerTests
{
    internal class when_getting_absent_chapter : QuestionnaireApiControllerTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            controller = CreateQuestionnaireController();
            BecauseOf();
        }

        private void BecauseOf() =>
            exception = Catch.Exception(() => controller.Chapter(questionnaireId, chapterId));

        [NUnit.Framework.Test] public void should_throw_exception () =>
            exception.ShouldNotBeNull();

        [NUnit.Framework.Test] public void should_throw_HttpResponseException_exception () =>
            exception.ShouldBeOfExactType(typeof(HttpResponseException));

        private static QuestionnaireController controller;
        private static Exception exception;
        private static string questionnaireId = "11111111111111111111111111111111";
        private static string chapterId = "22222222222222222222222222222222";
    }
}