using System;
using System.Linq;
using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;

namespace WB.Tests.Unit.Designer
{
    internal static class Setup
    {
        public static IAttachmentService AttachmentsServiceForOneQuestionnaire(Guid questionnaireId, params AttachmentView[] attachments)
        {
            var attachmentServiceMock = new Mock<IAttachmentService>();

            attachmentServiceMock.Setup(x => x.GetAttachmentSizesByQuestionnaire(questionnaireId))
                .Returns(attachments.Select(y => new AttachmentSize { AttachmentId = y.Meta.AttachmentId, Size = y.Content.Size}).ToList());
            
            return attachmentServiceMock.Object;
        }

        public static IDesignerEngineVersionService DesignerEngineVersionService(bool isClientVersionSupported = true, bool isQuestionnaireVersionSupported = true, int questionnaireContentVersion = 9)
        {
            return Mock.Of<IDesignerEngineVersionService>(_ 
                => _.IsClientVersionSupported(Moq.It.IsAny<int>()) == isClientVersionSupported
                && _.GetListOfNewFeaturesForClient(Moq.It.IsAny<QuestionnaireDocument>(), Moq.It.IsAny<int>()) == (isQuestionnaireVersionSupported ? new string[0] : new []{"New questionnaire feature"})
                && _.GetQuestionnaireContentVersion(Moq.It.IsAny<QuestionnaireDocument>()) == questionnaireContentVersion);
        }

        public static IQuestionnaireTranslator QuestionnaireTranslator(QuestionnaireDocument questionnaireDocument, ITranslation translation, QuestionnaireDocument translatedQuestionnaireDocument)
        {
            var serviceMock = new Mock<IQuestionnaireTranslator>();
            serviceMock.Setup(x => x.Translate(questionnaireDocument, translation, false))
                 .Returns(translatedQuestionnaireDocument);

            return serviceMock.Object;
        }
    }
}
