using System;
using System.ServiceModel;
using Machine.Specifications;
using Machine.Specifications.Utility;
using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.SharedKernels.DataCollection;
using WB.UI.Designer.WebServices;
using WB.UI.Designer.WebServices.Questionnaire;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Applications.Designer.PublicServiceTests
{
    internal class when_downloading_questionnaire_and_template_info_is_null : PublicServiceTestContext
    {
        Establish context = () =>
        {
            var supportedQuestionnaireVersion = new QuestionnaireVersion(0, 0, 1);

            var questionnaireId = Guid.Parse("11111111111111111111111111111111");

            request = CreateDownloadQuestionnaireRequest(questionnaireId, supportedQuestionnaireVersion);
            
            var exportService = new Mock<IQuestionnaireExportService>();
            exportService.Setup(x => x.GetQuestionnaireTemplateInfo(Moq.It.IsAny<QuestionnaireDocument>())).Returns((TemplateInfo) null);

            var questionnaireViewFactory = CreateQuestionnaireViewFactory(questionnaireId);

            var localizationServiceMock = new Mock<ILocalizationService>();
            localizationServiceMock.Setup(_ => _.GetString(Moq.It.IsAny<string>())).Returns(errorMessage);

            service = CreatePublicService(exportService: exportService.Object, questionnaireViewFactory: questionnaireViewFactory, localizationService: localizationServiceMock.Object);
        };

        Because of = () => 
             exception = Catch.Exception(() => service.DownloadQuestionnaire(request));

        It should_throw_exception_of_type_InconsistentVersionException = () =>
            exception.ShouldBeOfExactType<FaultException>();

        It should_throw_exception_that_contains_such_words = () =>
            (exception as FaultException).Message.ShouldEqual(errorMessage);

        private static QuestionnaireVersion version = new QuestionnaireVersion(1,0,0);
        private static DownloadQuestionnaireRequest request;
        private static IPublicService service;
        private static Exception exception;
        private static string errorMessage = "Requested questionnaire id=11111111-1111-1111-1111-111111111111 was not found";
    }
}
