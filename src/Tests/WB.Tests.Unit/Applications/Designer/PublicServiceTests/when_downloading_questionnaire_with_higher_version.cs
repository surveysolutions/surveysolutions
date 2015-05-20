using System;
using System.ServiceModel;
using Machine.Specifications;
using Machine.Specifications.Utility;
using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.SurveySolutions;
using WB.UI.Designer.WebServices;
using WB.UI.Designer.WebServices.Questionnaire;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Applications.Designer.PublicServiceTests
{
    internal class when_downloading_questionnaire_with_higher_version : PublicServiceTestContext
    {
        Establish context = () =>
        {
            var supportedQuestionnaireVersion = new Version(0, 0, 1);

            var questionnaireId = Guid.Parse("11111111111111111111111111111111");

            request = CreateDownloadQuestionnaireRequest(questionnaireId, supportedQuestionnaireVersion);

            var questionnaireViewFactory = CreateQuestionnaireViewFactory(questionnaireId, "aaaa");
            service = CreatePublicService(questionnaireViewFactory: questionnaireViewFactory, isClientVersionSupported: false);
        };

        Because of = () => 
             exception = Catch.Exception(() => service.DownloadQuestionnaire(request));

        It should_throw_exception_of_type_InconsistentVersionException = () =>
            exception.ShouldBeOfExactType<FaultException>();

        It should_throw_exception_that_contains_such_words = () =>
            (exception as FaultException).Message.ShouldEqual(errorMessage);

        private static DownloadQuestionnaireRequest request;
        private static IPublicService service;
        private static Exception exception;
        private static string errorMessage = "Failed to open the questionnaire. Your version is 0.0.1. Please update.";
    }
}
