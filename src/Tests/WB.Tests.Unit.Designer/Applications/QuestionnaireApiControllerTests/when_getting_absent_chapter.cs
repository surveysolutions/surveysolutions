using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using System;
using WB.Core.BoundedContexts.Designer.Implementation.Repositories;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.GenericSubdomains.Portable;
using WB.Tests.Abc;
using WB.UI.Designer.Controllers.Api.Designer;

namespace WB.Tests.Unit.Designer.Applications.QuestionnaireApiControllerTests
{
    internal class when_getting_absent_chapter : QuestionnaireApiControllerTestContext
    {
        [Test]
        public void should_throw_HttpResponseException_exception()
        {
            var controller = CreateQuestionnaireController();
            var actionResult = controller.Chapter(questionnaireId, chapterId);
            Assert.That(actionResult, Is.InstanceOf(typeof(NotFoundResult)));
        }

        private static string chapterId = "22222222222222222222222222222222";
    }
}
