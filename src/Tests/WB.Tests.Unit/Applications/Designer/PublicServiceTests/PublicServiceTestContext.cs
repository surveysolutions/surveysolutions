using System;
using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.SurveySolutions;
using WB.Core.SharedKernels.SurveySolutions.Services;
using WB.UI.Designer.WebServices;
using WB.UI.Designer.WebServices.Questionnaire;
using WB.UI.Shared.Web.Membership;

namespace WB.Tests.Unit.Applications.Designer.PublicServiceTests
{
    internal class PublicServiceTestContext
    {
        protected static PublicService CreatePublicService(
            IStringCompressor zipUtils = null,
            IMembershipUserService userHelper = null,
            IQuestionnaireListViewFactory viewFactory = null,
            IViewFactory<QuestionnaireViewInputModel, QuestionnaireView> questionnaireViewFactory = null,
            IQuestionnaireVerifier questionnaireVerifier = null,
            IExpressionProcessorGenerator expressionProcessorGenerator = null,
            bool? isClientVersionSupported=null)
        {
            return new PublicService(
                zipUtils ?? Mock.Of<IStringCompressor>(),
                userHelper ?? Mock.Of<IMembershipUserService>(),
                viewFactory ?? Mock.Of<IQuestionnaireListViewFactory>(),
                questionnaireViewFactory ?? Mock.Of<IViewFactory<QuestionnaireViewInputModel, QuestionnaireView>>(),
                questionnaireVerifier ?? Mock.Of<IQuestionnaireVerifier>(),
                expressionProcessorGenerator ?? Mock.Of<IExpressionProcessorGenerator>(),
                Mock.Of<IExpressionsEngineVersionService>(_ => _.IsClientVersionSupported(Moq.It.IsAny<Version>()) == (isClientVersionSupported??false)),
                Mock.Of<IJsonUtils>());
        }

        protected static DownloadQuestionnaireRequest CreateDownloadQuestionnaireRequest(Guid questionnaireId,
            Version supportedExpressionsEngineVersion)
        {
            return new DownloadQuestionnaireRequest
            {
                SupportedQuestionnaireVersion = supportedExpressionsEngineVersion,
                QuestionnaireId = questionnaireId
            };
        }

        protected static IViewFactory<QuestionnaireViewInputModel, QuestionnaireView> CreateQuestionnaireViewFactory(Guid id, string title = "title")
        {
            var questionnaire = new QuestionnaireDocument() { Title = title };
            var questionnaireView = new QuestionnaireView(questionnaire);
            var questionnaireViewFactory = Mock.Of<IViewFactory<QuestionnaireViewInputModel, QuestionnaireView>>(x => x.Load(Moq.It.IsAny<QuestionnaireViewInputModel>()) == questionnaireView);

            return questionnaireViewFactory;
        }
    }
}
