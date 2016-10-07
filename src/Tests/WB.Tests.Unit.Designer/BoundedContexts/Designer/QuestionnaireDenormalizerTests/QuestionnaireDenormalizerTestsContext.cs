extern alias designer;
using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using Ncqrs;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Implementation.Factories;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.EventBus;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using Group = Main.Core.Entities.SubEntities.Group;
using TemplateImported = WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto.TemplateImported;

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
            IQuestionnaireEntityFactory questionnaireEntityFactory = null,
            IEnumerable<Guid> sharedPersons = null 
            )
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
            var persons = sharedPersons?.Select(id => new SharedPerson() {Id = id}) ?? new List<SharedPerson>();
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
            var questionnaire = new QuestionnaireDocument();

            if (children != null)
            {
                questionnaire.Children.AddRange(children);
            }

            questionnaire.CreatedBy = createdBy;

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
        
        protected static NewGroupAdded CreateNewGroupAddedEvent(Guid groupId,
            string title = "New Group X")
        {
            return new NewGroupAdded
            {
                PublicKey = groupId,
                GroupText = title,
            };
        }

        protected static GroupCloned CreateGroupClonedEvent(Guid groupId,
            string title = "New Cloned Group X")
        {
            return new GroupCloned
            {
                PublicKey = groupId,
                GroupText = title,
            };
        }

        protected static GroupUpdated CreateGroupUpdatedEvent(Guid groupId,
            string title = "Updated Group Title X")
        {
            return new GroupUpdated
            {
                GroupPublicKey = groupId,
                GroupText = title,
            };
        }

        protected static QuestionChanged CreateQuestionChangedEvent(Guid questionId, Guid targetGroupId, string title, QuestionType questionType = QuestionType.Numeric)
        {
            return (Create.Event.QuestionChanged
            (
                publicKey : questionId,
                questionType : questionType,
                questionText : title
            ));
        }

        protected static QuestionnaireItemMoved CreateQuestionnaireItemMovedEvent(Guid itemId, Guid? targetGroupId)
        {
            return (new QuestionnaireItemMoved
            {
                PublicKey = itemId,
                GroupKey = targetGroupId,
            });
        }

        protected static GroupBecameARoster CreateGroupBecameARosterEvent(Guid groupId)
        {
            return (new GroupBecameARoster(Guid.NewGuid(), groupId));
        }

        protected static GroupStoppedBeingARoster CreateGroupStoppedBeingARosterEvent(Guid groupId)
        {
            return (new GroupStoppedBeingARoster(Guid.NewGuid(), groupId));
        }

        protected static RosterChanged CreateRosterChangedEvent(Guid groupId, Guid rosterSizeQuestionId, 
            RosterSizeSourceType rosterSizeSource, FixedRosterTitle[] rosterFixedTitles, Guid? rosterTitleQuestionId)
        {
            return
                (new RosterChanged(Guid.NewGuid(), groupId)
                {
                    RosterSizeQuestionId = rosterSizeQuestionId,
                    RosterSizeSource = rosterSizeSource,
                    FixedRosterTitles = rosterFixedTitles,
                    RosterTitleQuestionId = rosterTitleQuestionId
                });
        }

        protected static TextListQuestionCloned CreateTextListQuestionClonedEvent(Guid questionId, Guid sourceQuestionId)
        {
            return (new TextListQuestionCloned
            {
                PublicKey = questionId,
                SourceQuestionId = sourceQuestionId
            });
        }

        protected static TextListQuestionChanged CreateTextListQuestionChangedEvent(Guid questionId)
        {
            return (new TextListQuestionChanged
            {
                PublicKey = questionId
            });
        }
        protected static NumericQuestionChanged CreateNumericQuestionChangedEvent(
            Guid questionId)
        {
            return (Create.Event.NumericQuestionChanged
            (
                publicKey : questionId
            ));
        }

        protected static NumericQuestionCloned CreateNumericQuestionClonedEvent(
            Guid questionId, Guid? sourceQuestionId = null, Guid? parentGroupId = null)
        {
            return (Create.Event.NumericQuestionCloned
            (
                publicKey : questionId,
                sourceQuestionId : sourceQuestionId ?? Guid.NewGuid(),
                groupPublicKey : parentGroupId ?? Guid.NewGuid()
            ));
        }

        protected static QuestionCloned CreateQuestionClonedEvent(
            Guid questionId, QuestionType questionType = QuestionType.Numeric, Guid? sourceQuestionId = null, Guid? parentGroupId = null, int? maxValue = null)
        {
            return (Create.Event.QuestionCloned(
                publicKey : questionId,
                questionType : questionType,
                sourceQuestionId : sourceQuestionId ?? Guid.NewGuid(),
                groupPublicKey : parentGroupId ?? Guid.NewGuid()
            ));
        }

        protected static TextListQuestionCloned TextListQuestionClonedEvent(
            Guid questionId, Guid? sourceQuestionId = null, Guid? parentGroupId = null, int? maxAnswerCount = null)
        {
            return (new TextListQuestionCloned
            {
                PublicKey = questionId,
                SourceQuestionId = sourceQuestionId ?? Guid.NewGuid(),
                GroupId = parentGroupId ?? Guid.NewGuid(),
                MaxAnswerCount = maxAnswerCount
            });
        }

        protected static TextListQuestionChanged CreateTextListQuestionChangedEvent(
            Guid questionId, int? maxAnswerCount = null)
        {
            return (new TextListQuestionChanged
            {
                PublicKey = questionId,
                MaxAnswerCount = maxAnswerCount
            });
        }
    }
}
