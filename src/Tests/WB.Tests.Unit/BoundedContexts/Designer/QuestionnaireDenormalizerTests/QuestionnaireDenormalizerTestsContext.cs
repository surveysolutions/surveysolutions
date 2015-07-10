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
using Ncqrs.Eventing.ServiceModel.Bus;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Implementation.Factories;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Document;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using TemplateImported = designer::Main.Core.Events.Questionnaire.TemplateImported;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireDenormalizerTests
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
            IReadSideKeyValueStorage<QuestionnaireDocument> documentStorage = null,
            IQuestionnaireEntityFactory questionnaireEntityFactory = null,
            ILogger logger = null)
        {
            return new QuestionnaireDenormalizer(
                documentStorage ?? Mock.Of<IReadSideKeyValueStorage<QuestionnaireDocument>>(),
                questionnaireEntityFactory ?? Mock.Of<IQuestionnaireEntityFactory>(),
                logger ?? Mock.Of<ILogger>());
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

        protected static QRBarcodeQuestion CreateQRBarcodeQuestion(Guid questionId, string enablementCondition, bool isMandatory, string instructions, string title, string variableName)
        {
            return new QRBarcodeQuestion
            {
                PublicKey = questionId,
                QuestionText = title,
                QuestionType = QuestionType.QRBarcode,
                ConditionExpression = enablementCondition,
                Mandatory = isMandatory,
                StataExportCaption = variableName,
                Instructions = instructions
            };
        }

        protected static IQuestion CreateMultimediaQuestion(Guid questionId, string enablementCondition, bool isMandatory, string instructions, string title, string variableName)
        {
            return new MultimediaQuestion()
            {
                PublicKey = questionId,
                QuestionText = title,
                QuestionType = QuestionType.Multimedia,
                ConditionExpression = enablementCondition,
                Mandatory = isMandatory,
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

        protected static StaticText CreateStaticText(Guid entityId, string text)
        {
            return new StaticText(publicKey: entityId, text: text);
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
            return ToPublishedEvent(new NewQuestionAdded
            {
                PublicKey = questionId,
                GroupPublicKey = groupId,
                QuestionText = title,
                QuestionType = QuestionType.Numeric
            });
        }

        protected static IPublishedEvent<QuestionChanged> CreateQuestionChangedEvent(Guid questionId, Guid targetGroupId, string title, QuestionType questionType = QuestionType.Numeric)
        {
            return ToPublishedEvent(new QuestionChanged
            {
                PublicKey = questionId,
                QuestionType = questionType,
                QuestionText = title
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

        protected static IPublishedEvent<NumericQuestionAdded> CreateNumericQuestionAddedEvent(
            Guid questionId, Guid? parentGroupId = null)
        {
            return ToPublishedEvent(new NumericQuestionAdded
            {
                PublicKey = questionId,
                GroupPublicKey = parentGroupId ?? Guid.NewGuid()
            });
        }

        protected static IPublishedEvent<TextListQuestionAdded> CreateTextListQuestionAddedEvent(
            Guid questionId, Guid parentGroupId)
        {
            return ToPublishedEvent(new TextListQuestionAdded
            {
                PublicKey = questionId,
                GroupId = parentGroupId
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
            return ToPublishedEvent(new NumericQuestionChanged
            {
                PublicKey = questionId
            });
        }

        protected static IPublishedEvent<NumericQuestionCloned> CreateNumericQuestionClonedEvent(
            Guid questionId, Guid? sourceQuestionId = null, Guid? parentGroupId = null)
        {
            return ToPublishedEvent(new NumericQuestionCloned
            {
                PublicKey = questionId,
                SourceQuestionId = sourceQuestionId ?? Guid.NewGuid(),
                GroupPublicKey = parentGroupId ?? Guid.NewGuid()
            });
        }

        protected static IPublishedEvent<QuestionCloned> CreateQuestionClonedEvent(
            Guid questionId, QuestionType questionType = QuestionType.Numeric, Guid? sourceQuestionId = null, Guid? parentGroupId = null, int? maxValue = null)
        {
            return ToPublishedEvent(new QuestionCloned
            {
                PublicKey = questionId,
                QuestionType = questionType,
                SourceQuestionId = sourceQuestionId ?? Guid.NewGuid(),
                GroupPublicKey = parentGroupId ?? Guid.NewGuid()
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


        protected static IPublishedEvent<TextListQuestionAdded> CreateTextListQuestionAddedEvent(
            Guid questionId, Guid? parentGroupId = null, int? maxAnswerCount = null)
        {
            return ToPublishedEvent(new TextListQuestionAdded
            {
                PublicKey = questionId,
                GroupId = parentGroupId ?? Guid.NewGuid(),
                MaxAnswerCount = maxAnswerCount
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
            return ToPublishedEvent(new StaticTextAdded()
            {
                EntityId = entityId,
                ParentId = parentId,
                Text = text
            });
        }

        protected static IPublishedEvent<StaticTextUpdated> CreateStaticTextUpdatedEvent(Guid entityId, string text = null)
        {
            return ToPublishedEvent(new StaticTextUpdated()
            {
                EntityId = entityId,
                Text = text
            });
        }

        protected static IPublishedEvent<StaticTextCloned> CreateStaticTextClonedEvent(Guid targetEntityId,
            Guid sourceEntityId, Guid parentId, string text = null, int targetIndex = 0)
        {
            return ToPublishedEvent(new StaticTextCloned()
            {
                EntityId = targetEntityId,
                SourceEntityId = sourceEntityId,
                ParentId = parentId,
                Text = text,
                TargetIndex = targetIndex
            });
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
