using System;
using FluentAssertions;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Implementation.Factories;
using WB.UI.Headquarters.API.PublicApi;
using WB.UI.Headquarters.API.PublicApi.Models;


namespace WB.Tests.Unit.Applications.Headquarters.PublicApiTests
{
    internal class when_questionnaires_controller_questionnaries_by_id : ApiTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaireBrowseViewFactoryMock = new Mock<IQuestionnaireBrowseViewFactory>();

            controller = CreateQuestionnairesController(questionnaireBrowseViewViewFactory: questionnaireBrowseViewFactoryMock.Object);
            BecauseOf();
        }
        
        public void BecauseOf() 
        {
            actionResult = controller.Questionnaires(questionnaireId, 10, 1);
        }

        [NUnit.Framework.Test] public void should_return_QuestionnaireApiView () =>
            actionResult.Should().BeOfType<QuestionnaireApiView>();

        [NUnit.Framework.Test] public void should_call_factory_load_once () =>
            questionnaireBrowseViewFactoryMock.Verify(x => x.Load(Moq.It.IsAny<QuestionnaireBrowseInputModel>()), Times.Once());

        private static Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static QuestionnaireApiView actionResult;
        private static QuestionnairesController controller;
        private static Mock<IQuestionnaireBrowseViewFactory> questionnaireBrowseViewFactoryMock;
    }
}
