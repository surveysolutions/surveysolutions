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
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Tests.Unit.BoundedContexts.Designer.PdfQuestionnaireDenormalizerTests
{
    internal class PdfQuestionnaireDenormalizerTestContext
    {
        protected static PdfQuestionnaireDenormalizer CreatePdfQuestionnaireDenormalizer(
            IReadSideKeyValueStorage<PdfQuestionnaireView> documentStorage = null,
            ILogger logger = null)
        {
            return new PdfQuestionnaireDenormalizer(documentStorage, logger, null);
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

        protected static PdfStaticTextView CreateStaticText(Guid? entityId = null, string title = "Static Text X")
        {
            return new PdfStaticTextView()
            {
                PublicId = entityId ?? Guid.NewGuid(),
                Title = title
            };
        }
    }
}
