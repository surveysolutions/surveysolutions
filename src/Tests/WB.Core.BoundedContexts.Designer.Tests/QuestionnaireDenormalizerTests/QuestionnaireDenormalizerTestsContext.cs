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
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Implementation.Factories;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Document;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireDenormalizerTests
{
    [Subject(typeof(QuestionnaireDenormalizer))]
    internal class QuestionnaireDenormalizerTestsContext
    {
        protected static IPublishedEvent<T> ToPublishedEvent<T>(T @event)
            where T : class
        {
            return Mock.Of<IPublishedEvent<T>>(publishedEvent
                => publishedEvent.Payload == @event);
        }

        protected static QuestionnaireDenormalizer CreateQuestionnaireDenormalizer(
            IReadSideRepositoryWriter<QuestionnaireDocument> documentStorage = null,
            IQuestionFactory questionFactory = null, ILogger logger = null, IQuestionnaireDocumentUpgrader upgrader = null)
        {
            return new QuestionnaireDenormalizer(
                documentStorage ?? Mock.Of<IReadSideRepositoryWriter<QuestionnaireDocument>>(),
                questionFactory ?? Mock.Of<IQuestionFactory>(),
                logger ?? Mock.Of<ILogger>(),
                upgrader ?? Mock.Of<IQuestionnaireDocumentUpgrader>());
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

        protected static IPublishedEvent<NewQuestionAdded> CreateNewQuestionAddedEvent(Guid questionId, Guid? groupId = null, string title = "New Question X", List<Guid> triggers = null)
        {
            return ToPublishedEvent(new NewQuestionAdded
            {
                PublicKey = questionId,
                GroupPublicKey = groupId,
                QuestionText = title,
                QuestionType = QuestionType.Numeric,
                Triggers = triggers ?? new List<Guid>()
            });
        }

        protected static IPublishedEvent<QuestionChanged> CreateQuestionChangedEvent(Guid questionId, Guid targetGroupId, string title, QuestionType questionType = QuestionType.Numeric, List<Guid> triggers = null)
        {
            return ToPublishedEvent(new QuestionChanged
            {
                PublicKey = questionId,
                QuestionType = questionType,
                QuestionText = title,
                Triggers = triggers ?? new List<Guid>()
            });
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

        protected static IPublishedEvent<RosterChanged> CreateRosterChangedEvent(Guid groupId, Guid rosterSizeQuestionId)
        {
            return ToPublishedEvent(new RosterChanged(Guid.NewGuid(), groupId, rosterSizeQuestionId));
        }

        protected static IPublishedEvent<NumericQuestionAdded> CreateNumericQuestionAddedEvent(
            Guid questionId, Guid? parentGroupId = null, int? maxValue = null, List<Guid> triggers = null)
        {
            return ToPublishedEvent(new NumericQuestionAdded
            {
                PublicKey = questionId,
                GroupPublicKey = parentGroupId ?? Guid.NewGuid(),
                MaxAllowedValue = maxValue,
                Triggers = triggers ?? new List<Guid>()
            });
        }

        protected static IPublishedEvent<NumericQuestionChanged> CreateNumericQuestionChangedEvent(
            Guid questionId, int? maxValue = null, List<Guid> triggers = null)
        {
            return ToPublishedEvent(new NumericQuestionChanged
            {
                PublicKey = questionId,
                MaxAllowedValue = maxValue,
                Triggers = triggers ?? new List<Guid>()
            });
        }

        protected static IPublishedEvent<NumericQuestionCloned> CreateNumericQuestionClonedEvent(
            Guid questionId, Guid? sourceQuestionId = null, Guid? parentGroupId = null, int? maxValue = null, List<Guid> triggers = null)
        {
            return ToPublishedEvent(new NumericQuestionCloned
            {
                PublicKey = questionId,
                SourceQuestionId = sourceQuestionId ?? Guid.NewGuid(),
                GroupPublicKey = parentGroupId ?? Guid.NewGuid(),
                MaxAllowedValue = maxValue,
                Triggers = triggers ?? new List<Guid>()
            });
        }

        protected static IPublishedEvent<QuestionCloned> CreateQuestionClonedEvent(
            Guid questionId, QuestionType questionType = QuestionType.Numeric, Guid? sourceQuestionId = null, Guid? parentGroupId = null, int? maxValue = null, List<Guid> triggers = null)
        {
            return ToPublishedEvent(new QuestionCloned
            {
                PublicKey = questionId,
                QuestionType = questionType,
                SourceQuestionId = sourceQuestionId ?? Guid.NewGuid(),
                GroupPublicKey = parentGroupId ?? Guid.NewGuid(),
                Triggers = triggers ?? new List<Guid>()
            });
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
    }
}
