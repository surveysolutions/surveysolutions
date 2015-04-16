using System;
using System.ServiceModel;
using Machine.Specifications;
using Machine.Specifications.Utility;
using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.SurveySolutions;
using WB.UI.Designer.WebServices;
using WB.UI.Designer.WebServices.Questionnaire;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Applications.Designer.PublicServiceTests
{
    internal class when_downloading_questionnaire_that_absent : PublicServiceTestContext
    {
        Establish context = () =>
        {
            var supportedQuestionnaireVersion = new EngineVersion(0, 0, 1);

            var questionnaireId = Guid.Parse("11111111111111111111111111111111");

            request = CreateDownloadQuestionnaireRequest(questionnaireId, supportedQuestionnaireVersion);

            var questionnaireViewFactory = new Mock<IViewFactory<QuestionnaireViewInputModel, QuestionnaireView>>();
            questionnaireViewFactory.Setup(x => x.Load(Moq.It.IsAny<QuestionnaireViewInputModel>())).Returns((QuestionnaireView)null);

            service = CreatePublicService(questionnaireViewFactory: questionnaireViewFactory.Object);
        };

        Because of = () => 
             exception = Catch.Exception(() => service.DownloadQuestionnaire(request));

        It should_throw_exception_of_type_InconsistentVersionException = () =>
            exception.ShouldBeOfExactType<FaultException>();

        It should_throw_exception_that_contains_such_words = () =>
            (new[] { "questionnaire", "cannot be found" }).Each(x => (exception as FaultException).Message.ToLower().ShouldContain(x));

        private static EngineVersion version = new EngineVersion(1,0,0);
        private static DownloadQuestionnaireRequest request;
        private static IPublicService service;
        private static Exception exception;
    }
}
