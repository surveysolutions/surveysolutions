extern alias designer;
using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Main.Core.Events.Questionnaire;
using Moq;
using Ncqrs;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Implementation.Factories;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.EventBus;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using TemplateImported = designer::Main.Core.Events.Questionnaire.TemplateImported;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireDenormalizerTests
{
    [Subject(typeof(Questionnaire))]
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
            IQuestionnaireEntityFactory questionnaireEntityFactory = null)
        {
            var questAr = new Questionnaire(
                questionnaireEntityFactory ?? new QuestionnaireEntityFactory(),
                Mock.Of<ILogger>(),
                Mock.Of<IClock>(),
                expressionProcessor ?? Mock.Of<IExpressionProcessor>(),
                Create.SubstitutionService(),
                Create.KeywordsProvider(),
                Mock.Of<ILookupTableService>(),
                Mock.Of<IAttachmentService>(),
                Mock.Of<ITranslationsService>());
            questAr.Initialize(questionnaire.PublicKey, questionnaire);
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

        protected static QuestionnaireDocument CreateQuestionnaireDocument(IEnumerable<IComposite> children = null)
        {
            var questionnaire = new QuestionnaireDocument();

            if (children != null)
            {
                questionnaire.Children.AddRange(children);
            }

            return questionnaire;
        }

        protected static Group CreateGroup(Guid? groupId = null, string title = "Group X",
            IEnumerable<IComposite> children = null, Action<Group> setup = null)
        {
            var group = new Group
            {
                PublicKey = groupId ?? Guid.NewGuid(),
                Title = title,
            };

            if (children != null)
            {
                group.Children.AddRange(children);
            }

            if (setup != null)
            {
                setup(group);
            }

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
        
        protected static IPublishedEvent<GroupDeleted> CreateGroupDeletedEvent(Guid groupId)
        {
            return ToPublishedEvent(new GroupDeleted
            {
                GroupPublicKey = groupId,
            });
        }

        protected static IPublishedEvent<QuestionDeleted> CreateQuestionDeletedEvent(Guid questionId)
        {
            return ToPublishedEvent(new QuestionDeleted
            {
                QuestionId = questionId,
            });
        }

        protected static IPublishedEvent<NewGroupAdded> CreateNewGroupAddedEvent(Guid groupId,
            string title = "New Group X")
        {
            return ToPublishedEvent(new NewGroupAdded
            {
                PublicKey = groupId,
                GroupText = title,
            });
        }

        protected static IPublishedEvent<GroupCloned> CreateGroupClonedEvent(Guid groupId,
            string title = "New Cloned Group X")
        {
            return ToPublishedEvent(new GroupCloned
            {
                PublicKey = groupId,
                GroupText = title,
            });
        }

        protected static IPublishedEvent<GroupUpdated> CreateGroupUpdatedEvent(Guid groupId,
            string title = "Updated Group Title X")
        {
            return ToPublishedEvent(new GroupUpdated
            {
                GroupPublicKey = groupId,
                GroupText = title,
            });
        }

        protected static IPublishedEvent<NewQuestionAdded> CreateNewQuestionAddedEvent(Guid questionId, Guid? groupId = null, string title = "New Question X")
        {
            return ToPublishedEvent(Create.Event.NewQuestionAdded
            (
                publicKey : questionId,
                groupPublicKey : groupId,
                questionText : title,
                questionType : QuestionType.Numeric
            ));
        }

        protected static IPublishedEvent<QuestionChanged> CreateQuestionChangedEvent(Guid questionId, Guid targetGroupId, string title, QuestionType questionType = QuestionType.Numeric)
        {
            return ToPublishedEvent(Create.Event.QuestionChanged
            (
                publicKey : questionId,
                questionType : questionType,
                questionText : title
            ));
        }

        protected static IPublishedEvent<QuestionnaireItemMoved> CreateQuestionnaireItemMovedEvent(Guid itemId, Guid? targetGroupId)
        {
            return ToPublishedEvent(new QuestionnaireItemMoved
            {
                PublicKey = itemId,
                GroupKey = targetGroupId,
            });
        }

        protected static IPublishedEvent<GroupBecameARoster> CreateGroupBecameARosterEvent(Guid groupId)
        {
            return ToPublishedEvent(new GroupBecameARoster(Guid.NewGuid(), groupId));
        }

        protected static IPublishedEvent<GroupStoppedBeingARoster> CreateGroupStoppedBeingARosterEvent(Guid groupId)
        {
            return ToPublishedEvent(new GroupStoppedBeingARoster(Guid.NewGuid(), groupId));
        }

        protected static IPublishedEvent<RosterChanged> CreateRosterChangedEvent(Guid groupId, Guid rosterSizeQuestionId, 
            RosterSizeSourceType rosterSizeSource, FixedRosterTitle[] rosterFixedTitles, Guid? rosterTitleQuestionId)
        {
            return
                ToPublishedEvent(new RosterChanged(Guid.NewGuid(), groupId)
                {
                    RosterSizeQuestionId = rosterSizeQuestionId,
                    RosterSizeSource = rosterSizeSource,
                    FixedRosterTitles = rosterFixedTitles,
                    RosterTitleQuestionId = rosterTitleQuestionId
                });
        }

        protected static IPublishedEvent<TextListQuestionCloned> CreateTextListQuestionClonedEvent(Guid questionId, Guid sourceQuestionId)
        {
            return ToPublishedEvent(new TextListQuestionCloned
            {
                PublicKey = questionId,
                SourceQuestionId = sourceQuestionId
            });
        }

        protected static IPublishedEvent<TextListQuestionChanged> CreateTextListQuestionChangedEvent(Guid questionId)
        {
            return ToPublishedEvent(new TextListQuestionChanged
            {
                PublicKey = questionId
            });
        }
        protected static IPublishedEvent<NumericQuestionChanged> CreateNumericQuestionChangedEvent(
            Guid questionId)
        {
            return ToPublishedEvent(Create.Event.NumericQuestionChanged
            (
                publicKey : questionId
            ));
        }

        protected static IPublishedEvent<NumericQuestionCloned> CreateNumericQuestionClonedEvent(
            Guid questionId, Guid? sourceQuestionId = null, Guid? parentGroupId = null)
        {
            return ToPublishedEvent(Create.Event.NumericQuestionCloned
            (
                publicKey : questionId,
                sourceQuestionId : sourceQuestionId ?? Guid.NewGuid(),
                groupPublicKey : parentGroupId ?? Guid.NewGuid()
            ));
        }

        protected static IPublishedEvent<QuestionCloned> CreateQuestionClonedEvent(
            Guid questionId, QuestionType questionType = QuestionType.Numeric, Guid? sourceQuestionId = null, Guid? parentGroupId = null, int? maxValue = null)
        {
            return ToPublishedEvent(Create.Event.QuestionCloned(
                publicKey : questionId,
                questionType : questionType,
                sourceQuestionId : sourceQuestionId ?? Guid.NewGuid(),
                groupPublicKey : parentGroupId ?? Guid.NewGuid()
            ));
        }

        protected static IPublishedEvent<TemplateImported> CreateTemplateImportedEvent(QuestionnaireDocument questionnaireDocument = null)
        {
            return ToPublishedEvent(new TemplateImported
            {
                Source = questionnaireDocument ?? new QuestionnaireDocument()
            });
        }

        protected static IPublishedEvent<QuestionnaireCloned> CreateQuestionnaireClonedEvent(QuestionnaireDocument questionnaireDocument = null)
        {
            return ToPublishedEvent(new QuestionnaireCloned
            {
                QuestionnaireDocument = questionnaireDocument ?? new QuestionnaireDocument()
            });
        }

        protected static IPublishedEvent<TextListQuestionCloned> TextListQuestionClonedEvent(
            Guid questionId, Guid? sourceQuestionId = null, Guid? parentGroupId = null, int? maxAnswerCount = null)
        {
            return ToPublishedEvent(new TextListQuestionCloned
            {
                PublicKey = questionId,
                SourceQuestionId = sourceQuestionId ?? Guid.NewGuid(),
                GroupId = parentGroupId ?? Guid.NewGuid(),
                MaxAnswerCount = maxAnswerCount
            });
        }

        protected static IPublishedEvent<TextListQuestionChanged> CreateTextListQuestionChangedEvent(
            Guid questionId, int? maxAnswerCount = null)
        {
            return ToPublishedEvent(new TextListQuestionChanged
            {
                PublicKey = questionId,
                MaxAnswerCount = maxAnswerCount

            });
        }

        protected static IPublishedEvent<StaticTextAdded> CreateStaticTextAddedEvent(Guid entityId, Guid parentId, string text = null)
        {
            return ToPublishedEvent(
                Create.Event.StaticTextAdded(entityId : entityId,
                    parentId : parentId,
                    text : text));
        }

        protected static IPublishedEvent<StaticTextUpdated> CreateStaticTextUpdatedEvent(Guid entityId, string text = null, string attachmentName = null)
        {
            return ToPublishedEvent(Create.Event.StaticTextUpdated(
                entityId : entityId,
                text : text,
                attachmentName : attachmentName));
        }

        protected static IPublishedEvent<StaticTextCloned> CreateStaticTextClonedEvent(Guid targetEntityId,
            Guid sourceEntityId, Guid parentId, string text = null, int targetIndex = 0)
        {
            return ToPublishedEvent(Create.Event.StaticTextCloned(
                publicKey: targetEntityId,
                sourceEntityId : sourceEntityId,
                parentId : parentId,
                text : text,
                targetIndex : targetIndex));
        }

        protected static IPublishedEvent<StaticTextDeleted> CreateStaticTextDeletedEvent(Guid entityId)
        {
            return ToPublishedEvent(new StaticTextDeleted()
            {
                EntityId = entityId
            });
        }
        
    }
}
