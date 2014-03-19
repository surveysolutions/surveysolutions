using System;
using System.ServiceModel;
using Machine.Specifications;
using Machine.Specifications.Utility;
using Main.Core.View;
using Moq;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.SharedKernel.Utils.Compression;
using WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects;
using WB.UI.Designer.WebServices.Questionnaire;
using WB.UI.Shared.Web.Membership;
using It = Machine.Specifications.It;
using it = Moq.It;

namespace WB.UI.Designer.WebServices.Tests
{
    internal class PublicServiceTestContext
    {
        protected static PublicService CreatePublicService(IJsonExportService exportService = null,
            IStringCompressor zipUtils = null,
            IMembershipUserService userHelper = null,
            IViewFactory<QuestionnaireListInputModel, QuestionnaireListView> viewFactory = null)
        {
            return new PublicService(exportService ?? Mock.Of<IJsonExportService>(),
                zipUtils ?? Mock.Of<IStringCompressor>(),
                userHelper ?? Mock.Of<IMembershipUserService>(),
                viewFactory ?? Mock.Of<IViewFactory<QuestionnaireListInputModel, QuestionnaireListView>>());
        }

        protected static TemplateInfo CreateTemplateInfo(QuestionnaireVersion version)
        {
            return new TemplateInfo
            {
                Version = version,
                Source = "aaaa",
                Title = "aaaa"
            };
        }

        protected static DownloadQuestionnaireRequest CreateDownloadQuestionnaireRequest(Guid questionnaireId,
            QuestionnaireVersion supportedQuestionnaireVersion)
        {
            return new DownloadQuestionnaireRequest
            {
                SupportedQuestionnaireVersion = supportedQuestionnaireVersion,
                QuestionnaireId = questionnaireId
            };
        }
    }

    internal class when_dowloading_questionnaire_with_higher_version : PublicServiceTestContext
    {
        Establish context = () =>
        {
            var supportedQuestionnaireVersion = new QuestionnaireVersion(0, 0, 1);

            var questionnaireId = Guid.Parse("11111111111111111111111111111111");

            request = CreateDownloadQuestionnaireRequest(questionnaireId, supportedQuestionnaireVersion);

            var templateInfo = CreateTemplateInfo(version);

            exportService = Mock.Of<IJsonExportService>(x => x.GetQuestionnaireTemplate(questionnaireId) == templateInfo);

            service = CreatePublicService(exportService: exportService);
        };

        Because of = () => 
             exception = Catch.Exception(() => service.DownloadQuestionnaire(request));

        It should_throw_exception_of_type_InconsistentVersionException = () =>
            exception.ShouldBeOfType<FaultException>();

        It should_throw_exception_that_contains_such_words = () =>
            (new[] { "requested questionnaire", "supports versions" }).Each(x => (exception as FaultException).Message.ToLower().ShouldContain(x));

        private static QuestionnaireVersion version = new QuestionnaireVersion(1,0,0);
        private static DownloadQuestionnaireRequest request;
        private static IJsonExportService exportService;
        private static IPublicService service;
        private static Exception exception;

    }
}
