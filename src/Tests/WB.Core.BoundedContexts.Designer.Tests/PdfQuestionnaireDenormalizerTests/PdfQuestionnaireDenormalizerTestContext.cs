using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Implementation.Factories;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.QuestionnaireUpgrader.Services;

namespace WB.Core.BoundedContexts.Designer.Tests.PdfQuestionnaireDenormalizerTests
{
    internal class PdfQuestionnaireDenormalizerTestContext
    {
        protected static PdfQuestionnaireDenormalizer CreatePdfQuestionnaireDenormalizer(
            IReadSideRepositoryWriter<PdfQuestionnaireView> documentStorage = null,
            ILogger logger = null,
            IQuestionnaireDocumentUpgrader upgrader = null)
        {
            return new PdfQuestionnaireDenormalizer(documentStorage, logger, null, upgrader);
        }

        protected static IPublishedEvent<T> CreatePublishedEvent<T>(T @event)
           where T : class
        {
            return Mock.Of<IPublishedEvent<T>>(publishedEvent
                => publishedEvent.Payload == @event);
        }

        protected static PdfQuestionnaireView CreatePdfQuestionnaire(params PdfGroupView[] pdfGroupViews)
        {
            var pdfQuestionnaireView = new PdfQuestionnaireView();
            foreach (var pdfGroupView in pdfGroupViews)
            {
                    pdfQuestionnaireView.AddGroup(pdfGroupView, null);
            }
            return pdfQuestionnaireView;
        }

        protected static PdfGroupView CreateGroup(Guid? groupId = null, string title = "Group X", int depth = 1,
            List<PdfEntityView> children = null)
        {
            return new PdfGroupView
            {
                PublicId = groupId ?? Guid.NewGuid(),
                Title = title,
                Depth = depth,
                Children = children?? new List<PdfEntityView>()
            };
        }

        protected static PdfQuestionView CreateQuestion(Guid? questionId = null, string title = "Question X",
            QuestionType type = QuestionType.Numeric)
        {
            return new PdfQuestionView
            {
                PublicId = questionId ?? Guid.NewGuid(),
                Title = title,
                QuestionType = type
            };
        }
    }
}
