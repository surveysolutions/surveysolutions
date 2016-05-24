extern alias designer;

using System;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Main.Core.Events.Questionnaire;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Questionnaire;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using IEvent = WB.Core.Infrastructure.EventBus.IEvent;
using QuestionnaireDeleted = WB.Core.SharedKernels.DataCollection.Events.Questionnaire.QuestionnaireDeleted;
using WB.Core.SharedKernels.QuestionnaireEntities;
using TemplateImported = designer::Main.Core.Events.Questionnaire.TemplateImported;

namespace WB.Tests.Unit.TestFactories
{
    internal class PublishedEventFactory
    {
        private static QuestionnaireDocument CreateQuestionnaireDocument(string questionnaireId,
            string questionnaireTitle,
            string chapter1Id,
            string chapter1Title,
            string chapter2Id,
            string chapter2Title,
            string chapter1GroupId,
            string chapter1GroupTitle,
            string chapter2QuestionId,
            string chapter2QuestionTitle,
            string chapter2QuestionVariable,
            string chapter2QuestionConditionExpression,
            string chapter1StaticTextId,
            string chapter1StaticText,
            bool isPublic)
        {
            return new QuestionnaireDocument()
            {
                PublicKey = Guid.Parse(questionnaireId),
                Title = questionnaireTitle,
                IsPublic = isPublic,
                Children = new List<IComposite>()
                {
                    new Group()
                    {
                        PublicKey = Guid.Parse(chapter1Id),
                        Title = chapter1Title,
                        Children = new List<IComposite>()
                        {
                            Create.Other.StaticText(publicKey: GetQuestionnaireItemId(chapter1StaticTextId), text: chapter1StaticText),
                            new Group()
                            {
                                PublicKey = GetQuestionnaireItemId(chapter1GroupId),
                                Title = chapter1GroupTitle,
                                Children = new List<IComposite>()
                                {
                                    new Group()
                                    {
                                        IsRoster = true
                                    }
                                }
                            }
                        }
                    },
                    new Group()
                    {
                        PublicKey = Guid.Parse(chapter2Id),
                        Title = chapter2Title,
                        Children = new List<IComposite>()
                        {
                            new TextQuestion()
                            {
                                PublicKey = GetQuestionnaireItemId(chapter2QuestionId),
                                QuestionText = chapter2QuestionTitle,
                                StataExportCaption = chapter2QuestionVariable,
                                QuestionType = QuestionType.Text,
                                ConditionExpression = chapter2QuestionConditionExpression
                            }
                        }
                    }
                }
            };
        }

        private static Guid GetGuidIdByStringId(string stringId)
        {
            return string.IsNullOrEmpty(stringId) ? Guid.NewGuid() : Guid.Parse(stringId);
        }

        private static Guid GetQuestionnaireItemId(string questionnaireItemId)
        {
            return string.IsNullOrEmpty(questionnaireItemId) ? Guid.NewGuid() : Guid.Parse(questionnaireItemId);
        }

        private static Guid? GetQuestionnaireItemParentId(string questionnaireItemParentId)
        {
            return string.IsNullOrEmpty(questionnaireItemParentId)
                ? (Guid?)null
                : Guid.Parse(questionnaireItemParentId);
        }


        public IPublishedEvent<GroupBecameARoster> GroupBecameARosterEvent(string groupId)
        {
            return ToPublishedEvent(new GroupBecameARoster(responsibleId: new Guid(), groupId: Guid.Parse(groupId)));
        }

        public IPublishedEvent<GroupCloned> GroupClonedEvent(string groupId, string groupTitle = null,
            string parentGroupId = null)
        {
            return ToPublishedEvent(new GroupCloned()
            {
                PublicKey = Guid.Parse(groupId),
                ParentGroupPublicKey = GetQuestionnaireItemParentId(parentGroupId),
                GroupText = groupTitle,
                TargetIndex = 0
            });
        }

        public IPublishedEvent<GroupDeleted> GroupDeletedEvent(string groupId)
        {
            return ToPublishedEvent(new GroupDeleted()
            {
                GroupPublicKey = Guid.Parse(groupId)
            });
        }

        public IPublishedEvent<GroupStoppedBeingARoster> GroupStoppedBeingARosterEvent(string groupId)
        {
            return ToPublishedEvent(new GroupStoppedBeingARoster(responsibleId: new Guid(), groupId: Guid.Parse(groupId)));
        }

        public IPublishedEvent<GroupUpdated> GroupUpdatedEvent(string groupId, string groupTitle)
        {
            return ToPublishedEvent(new GroupUpdated()
            {
                GroupPublicKey = Guid.Parse(groupId),
                GroupText = groupTitle
            });
        }

        public IPublishedEvent<InterviewApprovedByHQ> InterviewApprovedByHQEvent(Guid? interviewId = null, string userId = null, string comment = null)
        {
            return ToPublishedEvent(new InterviewApprovedByHQ(userId: GetGuidIdByStringId(userId), comment: comment), eventSourceId: interviewId);
        }

        public IPublishedEvent<InterviewApproved> InterviewApprovedEvent(Guid? interviewId = null, string userId = null, string comment = null)
        {
            return ToPublishedEvent(new InterviewApproved(userId: GetGuidIdByStringId(userId), comment: comment, approveTime: DateTime.Now), eventSourceId: interviewId);
        }

        public IPublishedEvent<InterviewCompleted> InterviewCompletedEvent(Guid? interviewId = null, string userId = null, string comment = null, Guid? eventId = null)
        {
            return ToPublishedEvent(new InterviewCompleted(userId: GetGuidIdByStringId(userId), completeTime: DateTime.Now, comment: comment), eventSourceId: interviewId, eventId: eventId);
        }

        public IPublishedEvent<InterviewCreated> InterviewCreatedEvent(Guid? interviewId = null, string userId = null,
            string questionnaireId = null, long questionnaireVersion = 0)
        {
            return
                ToPublishedEvent(new InterviewCreated(userId: GetGuidIdByStringId(userId),
                    questionnaireId: GetGuidIdByStringId(questionnaireId), questionnaireVersion: questionnaireVersion), eventSourceId: interviewId);
        }

        public IPublishedEvent<InterviewDeleted> InterviewDeleted(Guid? interviewId = null)
            => Create.Event.InterviewDeleted().ToPublishedEvent(eventSourceId: interviewId);

        public IPublishedEvent<InterviewDeleted> InterviewDeletedEvent(string userId = null, string origin = null, Guid? interviewId = null)
        {
            return ToPublishedEvent(new InterviewDeleted(userId: GetGuidIdByStringId(userId)), origin: origin, eventSourceId: interviewId);
        }

        public IPublishedEvent<InterviewerAssigned> InterviewerAssignedEvent(Guid? interviewId = null, string userId = null,
            string interviewerId = null)
        {
            return
                ToPublishedEvent(new InterviewerAssigned(userId: GetGuidIdByStringId(userId),
                    interviewerId: GetGuidIdByStringId(interviewerId), assignTime: DateTime.Now), eventSourceId: interviewId);
        }

        public IPublishedEvent<InterviewFromPreloadedDataCreated> InterviewFromPreloadedDataCreatedEvent(Guid? interviewId = null, string userId = null,
            string questionnaireId = null, long questionnaireVersion = 0)
        {
            return
                ToPublishedEvent(new InterviewFromPreloadedDataCreated(userId: GetGuidIdByStringId(userId),
                    questionnaireId: GetGuidIdByStringId(questionnaireId), questionnaireVersion: questionnaireVersion), eventSourceId: interviewId);
        }

        public IPublishedEvent<InterviewHardDeleted> InterviewHardDeleted(Guid? interviewId = null)
            => Create.Event.InterviewHardDeleted().ToPublishedEvent(eventSourceId: interviewId);

        public IPublishedEvent<InterviewHardDeleted> InterviewHardDeletedEvent(string userId = null, Guid? interviewId = null)
        {
            return ToPublishedEvent(new InterviewHardDeleted(userId: GetGuidIdByStringId(userId)), eventSourceId: interviewId);
        }

        public IPublishedEvent<InterviewOnClientCreated> InterviewOnClientCreatedEvent(Guid? interviewId = null, string userId = null,
            string questionnaireId = null, long questionnaireVersion = 0)
        {
            return
                ToPublishedEvent(new InterviewOnClientCreated(userId: GetGuidIdByStringId(userId),
                    questionnaireId: GetGuidIdByStringId(questionnaireId), questionnaireVersion: questionnaireVersion), eventSourceId: interviewId);
        }

        public IPublishedEvent<InterviewRejectedByHQ> InterviewRejectedByHQEvent(Guid? interviewId = null, string userId = null, string comment = null)
        {
            return ToPublishedEvent(new InterviewRejectedByHQ(userId: GetGuidIdByStringId(userId), comment: comment), eventSourceId: interviewId);
        }

        public IPublishedEvent<InterviewRejected> InterviewRejectedEvent(Guid? interviewId = null, string userId = null, string comment = null)
        {
            return ToPublishedEvent(new InterviewRejected(userId: GetGuidIdByStringId(userId), comment: comment, rejectTime: DateTime.Now), eventSourceId: interviewId);
        }

        public IPublishedEvent<InterviewRestarted> InterviewRestartedEvent(Guid? interviewId = null, string userId = null, string comment = null)
        {
            return ToPublishedEvent(new InterviewRestarted(userId: GetGuidIdByStringId(userId), restartTime: DateTime.Now, comment: comment), eventSourceId: interviewId);
        }

        public IPublishedEvent<InterviewRestored> InterviewRestoredEvent(Guid? interviewId = null, string userId = null,
            string origin = null)
        {
            return ToPublishedEvent(new InterviewRestored(userId: GetGuidIdByStringId(userId)), origin: origin, eventSourceId: interviewId);
        }

        public IPublishedEvent<InterviewStatusChanged> InterviewStatusChanged(
            Guid interviewId, InterviewStatus status, string comment = "hello", Guid? eventId = null)
            => Create.Event.InterviewStatusChanged(status, comment).ToPublishedEvent(eventSourceId: interviewId, eventId: eventId);

        public IPublishedEvent<InterviewStatusChanged> InterviewStatusChangedEvent(InterviewStatus status,
            string comment = null,
            Guid? interviewId = null)
        {
            return ToPublishedEvent(new InterviewStatusChanged(status, comment), interviewId ?? Guid.NewGuid());
        }

        public IPublishedEvent<MultimediaQuestionUpdated> MultimediaQuestionUpdatedEvent(string questionId, string questionVariable = null, string questionTitle = null, string questionConditionExpression = null)
        {
            return ToPublishedEvent(new MultimediaQuestionUpdated()
            {
                QuestionId = Guid.Parse(questionId),
                VariableName = questionVariable,
                Title = questionTitle,
                EnablementCondition = questionConditionExpression
            });
        }

        public IPublishedEvent<NewGroupAdded> NewGroupAddedEvent(string groupId, string parentGroupId = null,
            string groupTitle = null)
        {
            return ToPublishedEvent(new NewGroupAdded()
            {
                PublicKey = Guid.Parse(groupId),
                ParentGroupPublicKey = GetQuestionnaireItemParentId(parentGroupId),
                GroupText = groupTitle
            });
        }

        public IPublishedEvent<NewQuestionAdded> NewQuestionAddedEvent(string questionId = null,
            string parentGroupId = null, QuestionType questionType = QuestionType.Text, string questionVariable = null,
            string questionTitle = null, string questionConditionExpression = null)
        {
            return ToPublishedEvent(Create.Event.NewQuestionAdded(
                publicKey: GetQuestionnaireItemId(questionId),
                groupPublicKey: GetQuestionnaireItemId(parentGroupId),
                questionType: questionType,
                stataExportCaption: questionVariable,
                questionText: questionTitle,
                conditionExpression: questionConditionExpression
            ));
        }

        public IPublishedEvent<NewQuestionnaireCreated> NewQuestionnaireCreatedEvent(string questionnaireId,
            string questionnaireTitle = null,
            bool? isPublic = null)
        {
            return ToPublishedEvent(new NewQuestionnaireCreated()
            {
                PublicKey = new Guid(questionnaireId),
                Title = questionnaireTitle,
                IsPublic = isPublic ?? false
            }, new Guid(questionnaireId));
        }

        public IPublishedEvent<NumericQuestionChanged> NumericQuestionChangedEvent(string questionId,
            string questionVariable = null, string questionTitle = null, string questionConditionExpression = null)
        {
            return ToPublishedEvent(Create.Event.NumericQuestionChanged(
                publicKey: Guid.Parse(questionId),
                stataExportCaption: questionVariable,
                questionText: questionTitle,
                conditionExpression: questionConditionExpression
            ));
        }

        public IPublishedEvent<NumericQuestionCloned> NumericQuestionClonedEvent(string questionId = null,
            string parentGroupId = null, string questionVariable = null, string questionTitle = null,
            string questionConditionExpression = null, string sourceQuestionId = null)
        {
            return ToPublishedEvent(Create.Event.NumericQuestionCloned(
                publicKey: GetQuestionnaireItemId(questionId),
                groupPublicKey: GetQuestionnaireItemId(parentGroupId),
                stataExportCaption: questionVariable,
                questionText: questionTitle,
                conditionExpression: questionConditionExpression,
                sourceQuestionId: GetQuestionnaireItemId(sourceQuestionId),
                targetIndex: 0
            ));
        }

        public IPublishedEvent<QRBarcodeQuestionCloned> QRBarcodeQuestionClonedEvent(string questionId = null,
            string parentGroupId = null, string questionVariable = null, string questionTitle = null,
            string questionConditionExpression = null, string sourceQuestionId = null,
            IList<ValidationCondition> validationConditions = null)
        {
            return ToPublishedEvent(new QRBarcodeQuestionCloned()
            {
                QuestionId = GetQuestionnaireItemId(questionId),
                ParentGroupId = GetQuestionnaireItemId(parentGroupId),
                VariableName = questionVariable,
                Title = questionTitle,
                EnablementCondition = questionConditionExpression,
                SourceQuestionId = GetQuestionnaireItemId(sourceQuestionId),
                TargetIndex = 0,
                ValidationConditions = validationConditions ?? new List<ValidationCondition>()
            });
        }

        public IPublishedEvent<QRBarcodeQuestionUpdated> QRBarcodeQuestionUpdatedEvent(string questionId,
            string questionVariable = null, string questionTitle = null, string questionConditionExpression = null)
        {
            return ToPublishedEvent(new QRBarcodeQuestionUpdated()
            {
                QuestionId = Guid.Parse(questionId),
                VariableName = questionVariable,
                Title = questionTitle,
                EnablementCondition = questionConditionExpression
            });
        }

        public IPublishedEvent<QuestionnaireDeleted> QuestionaireDeleted(Guid questionnaireId, long version)
        {
            return ToPublishedEvent(new QuestionnaireDeleted { QuestionnaireVersion = version }, eventSourceId: questionnaireId);
        }

        public IPublishedEvent<QuestionChanged> QuestionChangedEvent(string questionId, string parentGroupId = null,
            string questionVariable = null, string questionTitle = null, QuestionType? questionType = null, string questionConditionExpression = null)
        {
            return ToPublishedEvent(Create.Event.QuestionChanged(
                publicKey: Guid.Parse(questionId),
                groupPublicKey: Guid.Parse(parentGroupId ?? Guid.NewGuid().ToString()),
                stataExportCaption: questionVariable,
                questionText: questionTitle,
                questionType: questionType ?? QuestionType.Text,
                conditionExpression: questionConditionExpression
            ));
        }

        public IPublishedEvent<QuestionCloned> QuestionClonedEvent(string questionId = null,
            string parentGroupId = null, string questionVariable = null, string questionTitle = null,
            QuestionType questionType = QuestionType.Text, string questionConditionExpression = null,
            string sourceQuestionId = null,
            IList<ValidationCondition> validationConditions = null, bool hideIfDisabled = false,
            QuestionProperties properties = null)
        {
            return ToPublishedEvent(new QuestionCloned(
                publicKey: GetQuestionnaireItemId(questionId),
                groupPublicKey: GetQuestionnaireItemId(parentGroupId),
                stataExportCaption: questionVariable,
                questionText: questionTitle,
                questionType: questionType,
                conditionExpression: questionConditionExpression,
                hideIfDisabled: hideIfDisabled,
                sourceQuestionId: GetQuestionnaireItemId(sourceQuestionId),
                targetIndex: 0,
                featured: false,
                instructions: null,
                properties: properties ?? new QuestionProperties(false, false),
                responsibleId: Guid.NewGuid(),
                capital: false,
                questionScope: QuestionScope.Interviewer,
                variableLabel: null,
                validationExpression: null,
                validationMessage: null,
                answerOrder: null,
                answers: null,
                linkedToQuestionId: null,
                linkedToRosterId: null,
                isInteger: null,
                areAnswersOrdered: null,
                yesNoView: null,
                mask: null,
                maxAllowedAnswers: null,
                isFilteredCombobox: null,
                cascadeFromQuestionId: null,
                sourceQuestionnaireId: null,
                maxAnswerCount: null,
                countOfDecimalPlaces: null,
                validationConditions: validationConditions,
                linkedFilterExpression: null
            ));
        }

        public IPublishedEvent<QuestionDeleted> QuestionDeletedEvent(string questionId = null)
        {
            return ToPublishedEvent(new QuestionDeleted(GetQuestionnaireItemId(questionId)));
        }

        public IPublishedEvent<QuestionnaireAssemblyImported> QuestionnaireAssemblyImported(Guid questionnaireId, long version)
        {
            return ToPublishedEvent(new QuestionnaireAssemblyImported { Version = version }, eventSourceId: questionnaireId);
        }

        public IPublishedEvent<QuestionnaireCloned> QuestionnaireClonedEvent(string questionnaireId,
            string chapter1Id = null, string chapter1Title = "", string chapter2Id = null, string chapter2Title = "",
            string questionnaireTitle = null, string chapter1GroupId = null, string chapter1GroupTitle = null,
            string chapter2QuestionId = null, string chapter2QuestionTitle = null,
            string chapter2QuestionVariable = null,
            string chapter2QuestionConditionExpression = null,
            string chapter1StaticTextId = null, string chapter1StaticText = null,
            bool? isPublic = null,
            Guid? clonedFromQuestionnaireId = null)
        {
            var result = ToPublishedEvent(new QuestionnaireCloned()
            {
                QuestionnaireDocument =
                    CreateQuestionnaireDocument(questionnaireId: questionnaireId, questionnaireTitle: questionnaireTitle,
                        chapter1Id: chapter1Id ?? Guid.NewGuid().FormatGuid(), chapter1Title: chapter1Title, chapter2Id: chapter2Id ?? Guid.NewGuid().FormatGuid(),
                        chapter2Title: chapter2Title, chapter1GroupId: chapter1GroupId,
                        chapter1GroupTitle: chapter1GroupTitle, chapter2QuestionId: chapter2QuestionId,
                        chapter2QuestionTitle: chapter2QuestionTitle, chapter2QuestionVariable: chapter2QuestionVariable,
                        chapter2QuestionConditionExpression: chapter2QuestionConditionExpression,
                        chapter1StaticTextId: chapter1StaticTextId, chapter1StaticText: chapter1StaticText,
                        isPublic: isPublic ?? false),
                ClonedFromQuestionnaireId = clonedFromQuestionnaireId ?? Guid.NewGuid()
            }, new Guid(questionnaireId));
            return result;
        }

        public IPublishedEvent<Main.Core.Events.Questionnaire.QuestionnaireDeleted> QuestionnaireDeleted(Guid questionnaireId)
        {
            return ToPublishedEvent(new Main.Core.Events.Questionnaire.QuestionnaireDeleted(), eventSourceId: questionnaireId);
        }

        public IPublishedEvent<QuestionnaireItemMoved> QuestionnaireItemMovedEvent(string itemId,
            string targetGroupId = null, int? targetIndex = null, string questionnaireId = null)
        {
            return ToPublishedEvent(new QuestionnaireItemMoved()
            {
                PublicKey = Guid.Parse(itemId),
                GroupKey = GetQuestionnaireItemParentId(targetGroupId),
                TargetIndex = targetIndex ?? 0
            }, Guid.Parse(questionnaireId ?? Guid.NewGuid().ToString()));
        }

        public IPublishedEvent<QuestionnaireUpdated> QuestionnaireUpdatedEvent(string questionnaireId,
            string questionnaireTitle,
            bool isPublic = false)
        {
            return ToPublishedEvent(new QuestionnaireUpdated() { Title = questionnaireTitle, IsPublic = isPublic }, new Guid(questionnaireId));
        }

        public IPublishedEvent<RosterChanged> RosterChanged(string groupId)
        {
            return ToPublishedEvent(new RosterChanged(responsibleId: new Guid(), groupId: Guid.Parse(groupId)));
        }

        public IPublishedEvent<SharedPersonFromQuestionnaireRemoved> SharedPersonFromQuestionnaireRemoved(Guid questionnaireId, Guid personId)
        {
            return ToPublishedEvent(new SharedPersonFromQuestionnaireRemoved() { PersonId = personId }, questionnaireId);
        }

        public IPublishedEvent<SharedPersonToQuestionnaireAdded> SharedPersonToQuestionnaireAdded(Guid questionnaireId, Guid personId)
        {
            return ToPublishedEvent(new SharedPersonToQuestionnaireAdded() { PersonId = personId }, questionnaireId);
        }

        public IPublishedEvent<StaticTextAdded> StaticTextAddedEvent(string entityId = null, Guid? responsibleId = null, string parentId = null, string text = null)
        {
            return ToPublishedEvent(new StaticTextAdded(
                GetQuestionnaireItemId(entityId),
                responsibleId ?? Guid.NewGuid(),
                GetQuestionnaireItemId(parentId),
                text,
                null,
                false,
                null));
        }

        public IPublishedEvent<StaticTextCloned> StaticTextClonedEvent(string entityId = null,
            string parentId = null, string sourceEntityId = null, string text = null, int targetIndex = 0)
        {
            return ToPublishedEvent(Create.Event.StaticTextCloned(
                publicKey: GetQuestionnaireItemId(entityId),
                parentId: GetQuestionnaireItemId(parentId),
                sourceEntityId: GetQuestionnaireItemId(sourceEntityId),
                text: text,
                targetIndex: targetIndex));
        }

        public IPublishedEvent<StaticTextDeleted> StaticTextDeletedEvent(string entityId = null)
        {
            return ToPublishedEvent(new StaticTextDeleted()
            {
                EntityId = GetQuestionnaireItemId(entityId)
            });
        }

        public IPublishedEvent<StaticTextUpdated> StaticTextUpdatedEvent(string entityId = null, string text = null,
            string enablementCondition = null, bool? hideIfDisabled = null, IList<ValidationCondition> validationConditions = null)
        {
            return ToPublishedEvent(Create.Event.StaticTextUpdated(

                GetQuestionnaireItemId(entityId),
                text));
        }

        public IPublishedEvent<SupervisorAssigned> SupervisorAssignedEvent(Guid? interviewId = null, string userId = null,
            string supervisorId = null)
        {
            return
                ToPublishedEvent(new SupervisorAssigned(userId: GetGuidIdByStringId(userId),
                    supervisorId: GetGuidIdByStringId(supervisorId)), eventSourceId: interviewId);
        }

        public IPublishedEvent<SynchronizationMetadataApplied> SynchronizationMetadataAppliedEvent(string userId = null,
            InterviewStatus status = InterviewStatus.Created, string questionnaireId = null,
            AnsweredQuestionSynchronizationDto[] featuredQuestionsMeta = null, bool createdOnClient = false)
        {
            return
                ToPublishedEvent(new SynchronizationMetadataApplied(userId: GetGuidIdByStringId(userId), status: status,
                    questionnaireId: GetGuidIdByStringId(questionnaireId), questionnaireVersion: 1, featuredQuestionsMeta: featuredQuestionsMeta,
                    createdOnClient: createdOnClient, comments: null, rejectedDateTime: null, interviewerAssignedDateTime: null));
        }

        public IPublishedEvent<TemplateImported> TemplateImportedEvent(
            string questionnaireId,
            string chapter1Id = null,
            string chapter1Title = null,
            string chapter2Id = null,
            string chapter2Title = null,
            string questionnaireTitle = null,
            string chapter1GroupId = null, string chapter1GroupTitle = null,
            string chapter2QuestionId = null,
            string chapter2QuestionTitle = null,
            string chapter2QuestionVariable = null,
            string chapter2QuestionConditionExpression = null,
            string chapter1StaticTextId = null, string chapter1StaticText = null,
            bool? isPublic = null)
        {
            return ToPublishedEvent(new TemplateImported()
            {
                Source =
                    CreateQuestionnaireDocument(questionnaireId: questionnaireId, questionnaireTitle: questionnaireTitle,
                        chapter1Id: chapter1Id ?? Guid.NewGuid().FormatGuid(), chapter1Title: chapter1Title,
                        chapter2Id: chapter2Id ?? Guid.NewGuid().FormatGuid(),
                        chapter2Title: chapter2Title, chapter1GroupId: chapter1GroupId,
                        chapter1GroupTitle: chapter1GroupTitle, chapter2QuestionId: chapter2QuestionId,
                        chapter2QuestionTitle: chapter2QuestionTitle, chapter2QuestionVariable: chapter2QuestionVariable,
                        chapter2QuestionConditionExpression: chapter2QuestionConditionExpression,
                        chapter1StaticTextId: chapter1StaticTextId, chapter1StaticText: chapter1StaticText,
                        isPublic: isPublic ?? false)
            }, new Guid(questionnaireId));
        }

        public IPublishedEvent<TextListQuestionChanged> TextListQuestionChangedEvent(string questionId,
            string questionVariable = null, string questionTitle = null, string questionConditionExpression = null)
        {
            return ToPublishedEvent(new TextListQuestionChanged()
            {
                PublicKey = Guid.Parse(questionId),
                StataExportCaption = questionVariable,
                QuestionText = questionTitle,
                ConditionExpression = questionConditionExpression
            });
        }

        public IPublishedEvent<TextListQuestionCloned> TextListQuestionClonedEvent(string questionId = null,
            string parentGroupId = null, string questionVariable = null, string questionTitle = null,
            string questionConditionExpression = null, string sourceQuestionId = null)
        {
            return ToPublishedEvent(new TextListQuestionCloned()
            {
                PublicKey = GetQuestionnaireItemId(questionId),
                GroupId = GetQuestionnaireItemId(parentGroupId),
                StataExportCaption = questionVariable,
                QuestionText = questionTitle,
                ConditionExpression = questionConditionExpression,
                SourceQuestionId = GetQuestionnaireItemId(sourceQuestionId),
                TargetIndex = 0
            });
        }

        public IPublishedEvent<TextQuestionAnswered> TextQuestionAnsweredEvent(Guid? interviewId = null, string userId = null)
        {
            return
                ToPublishedEvent(new TextQuestionAnswered(GetGuidIdByStringId(userId), Guid.NewGuid(), new decimal[0],
                    DateTime.Now, "tttt"));
        }

        private static IPublishedEvent<T> ToPublishedEvent<T>(
            T @event,
            Guid? eventSourceId = null,
            string origin = null,
            DateTime? eventTimeStamp = null,
            Guid? eventId = null)
            where T : class, IEvent
        {
            var mock = new Mock<IPublishedEvent<T>>();
            var eventIdentifier = eventId ?? Guid.NewGuid();
            mock.Setup(x => x.Payload).Returns(@event);
            mock.Setup(x => x.EventSourceId).Returns(eventSourceId ?? Guid.NewGuid());
            mock.Setup(x => x.Origin).Returns(origin);
            mock.Setup(x => x.EventIdentifier).Returns(eventIdentifier);
            mock.Setup(x => x.EventTimeStamp).Returns((eventTimeStamp ?? DateTime.Now));
            var publishableEventMock = mock.As<IUncommittedEvent>();
            publishableEventMock.Setup(x => x.Payload).Returns(@event);
            return mock.Object;
        }

        public IPublishedEvent<UnapprovedByHeadquarters> UnapprovedByHeadquartersEvent(Guid? interviewId = null, string userId = null, string comment = null)
        {
            return ToPublishedEvent(new UnapprovedByHeadquarters(userId: GetGuidIdByStringId(userId), comment: comment), eventSourceId: interviewId);
        }
    }
}