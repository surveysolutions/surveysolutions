using System;
using Main.Core.View;
using Moq;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.SharedKernel.Utils.Compression;
using WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects;
using WB.UI.Designer.Services;
using WB.UI.Designer.Services.Questionnaire;
using WB.UI.Shared.Web.Membership;

namespace WB.Tests.Unit.Applications.Designer.PublicServiceTests
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
}
