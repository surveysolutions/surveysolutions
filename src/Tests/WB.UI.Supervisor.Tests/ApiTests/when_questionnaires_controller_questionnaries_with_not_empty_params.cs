﻿using Machine.Specifications;
using Main.Core.View;
using Moq;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire.BrowseItem;
using WB.UI.Supervisor.API;
using WB.UI.Supervisor.Models.API;
using It = Machine.Specifications.It;

namespace Web.Supervisor.Tests.ApiTests
{
    internal class when_questionnaires_controller_questionnaries_with_not_empty_params : ApiTestContext
    {
        private Establish context = () =>
        {
            questionnaireBrowseViewFactoryMock = new
                Mock<IViewFactory<QuestionnaireBrowseInputModel, QuestionnaireBrowseView>>();
            controller = CreateQuestionnairesController(questionnaireBrowseViewViewFactory: questionnaireBrowseViewFactoryMock.Object);
        };
        
        Because of = () =>
        {
            actionResult = controller.Questionnaires(10, 1);
        };

        It should_return_QuestionnaireApiView = () =>
            actionResult.ShouldBeOfExactType<QuestionnaireApiView>();

        It should_call_factory_load_once = () =>
            questionnaireBrowseViewFactoryMock.Verify(x => x.Load(Moq.It.IsAny<QuestionnaireBrowseInputModel>()), Times.Once());

        private static QuestionnaireApiView actionResult;
        private static QuestionnairesController controller;
        private static Mock<IViewFactory<QuestionnaireBrowseInputModel, QuestionnaireBrowseView>> questionnaireBrowseViewFactoryMock;
    }
}
