using System;
using System.ServiceModel;
using Machine.Specifications;
using Machine.Specifications.Utility;
using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.SharedKernels.DataCollection;
using WB.UI.Designer.WebServices;
using WB.UI.Designer.WebServices.Questionnaire;
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
            (exception as FaultException).Message.ShouldEqual(errorMessage);

        private static QuestionnaireVersion version = new QuestionnaireVersion(1,0,0);
        private static DownloadQuestionnaireRequest request;
        private static IQuestionnaireExportService exportService;
        private static IPublicService service;
        private static Exception exception;
        private static string errorMessage = "Failed to import questionnaire. Your questionnaire \"aaaa.tmpl\" has 1.0.0 version. Headquarters application supports only up to version 0.0.1.";
    }
}
