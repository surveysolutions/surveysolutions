using System;
using System.ServiceModel;
using Machine.Specifications;
using Machine.Specifications.Utility;
using Main.Core.Documents;
using Main.Core.View;
using Moq;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects;
using WB.UI.Designer.Services;
using WB.UI.Designer.Services.Questionnaire;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Applications.Designer.PublicServiceTests
{
    internal class when_downloading_questionnaire_with_higher_version : PublicServiceTestContext
    {
        Establish context = () =>
        {
            var supportedQuestionnaireVersion = new QuestionnaireVersion(0, 0, 1);

            var questionnaireId = Guid.Parse("11111111111111111111111111111111");

            request = CreateDownloadQuestionnaireRequest(questionnaireId, supportedQuestionnaireVersion);

            var templateInfo = CreateTemplateInfo(version);

            exportService = Mock.Of<IQuestionnaireExportService>(x => x.GetQuestionnaireTemplateInfo(Moq.It.IsAny<QuestionnaireDocument>()) == templateInfo);

            var questionnaireViewFactory = CreateQuestionnaireViewFactory(questionnaireId);            
            service = CreatePublicService(exportService: exportService, questionnaireViewFactory: questionnaireViewFactory);
        };

        Because of = () => 
             exception = Catch.Exception(() => service.DownloadQuestionnaire(request));

        It should_throw_exception_of_type_InconsistentVersionException = () =>
            exception.ShouldBeOfExactType<FaultException>();

        It should_throw_exception_that_contains_such_words = () =>
            (new[] { "requested questionnaire", "supports versions" }).Each(x => (exception as FaultException).Message.ToLower().ShouldContain(x));

        private static QuestionnaireVersion version = new QuestionnaireVersion(1,0,0);
        private static DownloadQuestionnaireRequest request;
        private static IQuestionnaireExportService exportService;
        private static IPublicService service;
        private static Exception exception;
    }
}
