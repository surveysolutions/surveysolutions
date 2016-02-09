using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Tests.Unit.BoundedContexts.Designer.PdfQuestionnaireDenormalizerTests
{
    internal class PdfQuestionnaireDenormalizerTestContext
    {
        protected static PdfQuestionnaireDenormalizer CreatePdfQuestionnaireDenormalizer(
            IReadSideKeyValueStorage<PdfQuestionnaireView> documentStorage = null,
            ILogger logger = null)
        {
            var pdfQuestionTypeConverter = new PdfQuestionTypeConverter();
            return new PdfQuestionnaireDenormalizer(documentStorage, logger, null, pdfQuestionTypeConverter);
        }

        protected static IPublishedEvent<T> CreatePublishedEvent<T>(T @event)
           where T : class, IEvent
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
            PdfQuestionType type = PdfQuestionType.Numeric)
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

        protected static QuestionCloned CreateQuestionCloned(QuestionType questionType, Guid publicKey, Guid groupPublicKey, 
            string questionText, string validationExpression)
        {
            return new QuestionCloned (
                publicKey: publicKey,
                questionType:questionType,
                groupPublicKey:groupPublicKey,
                questionText: questionText,
                validationExpression: validationExpression,
                responsibleId: Guid.NewGuid(),
                conditionExpression: null,
                hideIfDisabled: false,
                featured:false,
                instructions: null,
                capital: false,
                questionScope: QuestionScope.Interviewer, 
                stataExportCaption: null,
                variableLabel: null,
                validationMessage: null,
                answerOrder: null,
                answers: null,
                linkedToQuestionId: null,
                linkedToRosterId: null,
                isInteger: null,
                areAnswersOrdered: null,
                yesNoView: null,
                maxAllowedAnswers: null,
                mask: null,
                maxAnswerCount: null,
                isFilteredCombobox: null,
                cascadeFromQuestionId: null,
                sourceQuestionnaireId: null,
                sourceQuestionId: Guid.NewGuid(), 
                targetIndex: 0,
                countOfDecimalPlaces: null,
                validationConditions: new List<ValidationCondition>()
                );
        }
    }
}
