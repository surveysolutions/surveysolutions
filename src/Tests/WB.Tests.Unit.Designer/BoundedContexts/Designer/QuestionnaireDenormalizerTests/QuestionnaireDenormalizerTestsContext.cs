using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using Ncqrs;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.EventBus;
using WB.Core.SharedKernels.Questionnaire.Translations;
using Group = Main.Core.Entities.SubEntities.Group;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireDenormalizerTests
{
    internal class QuestionnaireDenormalizerTestsContext
    {
        protected static IPublishedEvent<T> ToPublishedEvent<T>(T @event)
            where T : class, IEvent
        {
            return Mock.Of<IPublishedEvent<T>>(publishedEvent
                => publishedEvent.Payload == @event);
        }

        public static Questionnaire CreateQuestionnaireDenormalizer(QuestionnaireDocument questionnaire,
            IExpressionProcessor expressionProcessor = null,
            IEnumerable<Guid> sharedPersons = null 
            )
        {
            var questAr = new Questionnaire(
                Mock.Of<ILogger>(),
                Mock.Of<IClock>(),
                Mock.Of<ILookupTableService>(),
                Mock.Of<IAttachmentService>(),
                Mock.Of<ITranslationsService>(),
                Mock.Of<IQuestionnaireHistoryVersionsService>());
            var persons = sharedPersons?.Select(id => new SharedPerson() {UserId = id}) ?? new List<SharedPerson>();
            questAr.Initialize(questionnaire.PublicKey, questionnaire, persons);
            return questAr;
        }

        protected static T GetEntityById<T>(QuestionnaireDocument document, Guid entityId)
            where T : class, IComposite
        {
            return document.FirstOrDefault<T>(entity => entity.PublicKey == entityId);
        }

        protected static QuestionnaireDocument CreateQuestionnaireDocument(params IComposite[] children)
        {
            return CreateQuestionnaireDocument(children.AsEnumerable());
        }

        protected static QuestionnaireDocument CreateQuestionnaireDocument(IEnumerable<IComposite> children = null, Guid? createdBy = null)
        {
            return new QuestionnaireDocument()
            {
                Children = children?.ToReadOnlyCollection() ?? new ReadOnlyCollection<IComposite>(new List<IComposite>()),
                CreatedBy = createdBy
            };
        }

        protected static Group CreateGroup(Guid? groupId = null, string title = "Group X",
            IEnumerable<IComposite> children = null, Action<Group> setup = null)
        {
            var group = new Group
            {
                PublicKey = groupId ?? Guid.NewGuid(),
                Title = title,
                Children = children?.ToReadOnlyCollection() ?? new ReadOnlyCollection<IComposite>(new List<IComposite>())
            };
            
            setup?.Invoke(@group);

            return group;
        }

        protected static QRBarcodeQuestion CreateQRBarcodeQuestion(Guid questionId, string enablementCondition, string instructions, string title, string variableName)
        {
            return new QRBarcodeQuestion
            {
                PublicKey = questionId,
                QuestionText = title,
                QuestionType = QuestionType.QRBarcode,
                ConditionExpression = enablementCondition,
                StataExportCaption = variableName,
                Instructions = instructions
            };
        }

        protected static IQuestion CreateMultimediaQuestion(Guid questionId, string enablementCondition, string instructions, string title, string variableName)
        {
            return new MultimediaQuestion()
            {
                PublicKey = questionId,
                QuestionText = title,
                QuestionType = QuestionType.Multimedia,
                ConditionExpression = enablementCondition,
                StataExportCaption = variableName,
                Instructions = instructions
            };
        }

        protected static TextQuestion CreateTextQuestion(Guid? questionId = null, string title = null)
        {
            return new TextQuestion
            {
                PublicKey = questionId ?? Guid.NewGuid(),
                QuestionText = title,
                QuestionType = QuestionType.Text,
            };
        }

        protected static NumericQuestion CreateNumericQuestion(Guid? questionId = null, string title = null)
        {
            return new NumericQuestion
            {
                PublicKey = questionId ?? Guid.NewGuid(),
                QuestionText = title,
                QuestionType = QuestionType.Numeric
            };
        }


        protected static TextListQuestion CreateTextListQuestion(Guid? questionId = null)
        {
            return new TextListQuestion
            {
                PublicKey = questionId ?? Guid.NewGuid(),
                QuestionType = QuestionType.TextList
            };
        }
    }
}
