using System.Linq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.Implementation.Aggregates;
using WB.Core.Infrastructure.Ncqrs.Eventing;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Utils;

namespace WB.Enumerator.Native.WebInterview
{
    [ReceivesIgnoredEvents]
    public class InterviewLifecycleEventHandler :
        BaseDenormalizer,
        IEventHandler<AnswersDeclaredInvalid>,
        IEventHandler<AnswersDeclaredValid>,
        IEventHandler<QuestionsDisabled>,
        IEventHandler<QuestionsEnabled>,
        IEventHandler<StaticTextsDisabled>,
        IEventHandler<StaticTextsEnabled>,
        IEventHandler<TextQuestionAnswered>,
        IEventHandler<TextListQuestionAnswered>,
        IEventHandler<SingleOptionQuestionAnswered>,
        IEventHandler<MultipleOptionsQuestionAnswered>,
        IEventHandler<DateTimeQuestionAnswered>,
        IEventHandler<SubstitutionTitlesChanged>,
        IEventHandler<NumericIntegerQuestionAnswered>,
        IEventHandler<NumericRealQuestionAnswered>,
        IEventHandler<YesNoQuestionAnswered>,
        IEventHandler<GeoLocationQuestionAnswered>,
        IEventHandler<SingleOptionLinkedQuestionAnswered>,
        IEventHandler<MultipleOptionsLinkedQuestionAnswered>,
        IEventHandler<PictureQuestionAnswered>,
        IEventHandler<QRBarcodeQuestionAnswered>,
        IEventHandler<LinkedOptionsChanged>,
        IEventHandler<LinkedToListOptionsChanged>,
        IEventHandler<AnswersRemoved>,
        IEventHandler<StaticTextsDeclaredInvalid>,
        IEventHandler<StaticTextsDeclaredValid>,
        IEventHandler<AnswersDeclaredImplausible>,
        IEventHandler<AnswersDeclaredPlausible>,
        IEventHandler<StaticTextsDeclaredImplausible>,
        IEventHandler<StaticTextsDeclaredPlausible>,
        IEventHandler<RosterInstancesAdded>,
        IEventHandler<RosterInstancesRemoved>,
        IEventHandler<RosterInstancesTitleChanged>,
        IEventHandler<GroupsEnabled>,
        IEventHandler<GroupsDisabled>,
        IEventHandler<TranslationSwitched>,
        IEventHandler<InterviewCompleted>,
        IEventHandler<InterviewDeleted>,
        IEventHandler<InterviewHardDeleted>,
        IEventHandler<InterviewerAssigned>,
        IEventHandler<AreaQuestionAnswered>,
        IEventHandler<AudioQuestionAnswered>,
        IEventHandler<AnswerCommented>

    {
        public override object[] Writers => new object[0];

        private readonly IWebInterviewNotificationService webInterviewNotificationService;
        private readonly IAggregateRootCacheCleaner aggregateRootCacheCleaner;

        public InterviewLifecycleEventHandler(IWebInterviewNotificationService webInterviewNotificationService,
            IAggregateRootCacheCleaner aggregateRootCacheCleaner)
        {
            this.webInterviewNotificationService = webInterviewNotificationService;
            this.aggregateRootCacheCleaner = aggregateRootCacheCleaner;
        }

        public void Handle(IPublishedEvent<AnswersDeclaredInvalid> evnt)
        {
            this.webInterviewNotificationService.RefreshEntities(evnt.EventSourceId, evnt.Payload.FailedValidationConditions.Keys.ToArray());
        }

        public void Handle(IPublishedEvent<AnswersDeclaredValid> evnt)
        {
            this.webInterviewNotificationService.RefreshEntities(evnt.EventSourceId, evnt.Payload.Questions);
        }

        public void Handle(IPublishedEvent<AnswersDeclaredImplausible> evnt)
        {
            this.webInterviewNotificationService.RefreshEntities(evnt.EventSourceId, evnt.Payload.GetFailedValidationConditionsDictionary().Keys.ToArray());
        }

        public void Handle(IPublishedEvent<AnswersDeclaredPlausible> evnt)
        {
            this.webInterviewNotificationService.RefreshEntities(evnt.EventSourceId, evnt.Payload.Questions);
        }

        public void Handle(IPublishedEvent<StaticTextsDeclaredImplausible> evnt)
        {
            this.webInterviewNotificationService.RefreshEntities(evnt.EventSourceId, evnt.Payload.GetFailedValidationConditionsDictionary().Keys.ToArray());
        }

        public void Handle(IPublishedEvent<StaticTextsDeclaredPlausible> evnt)
        {
            this.webInterviewNotificationService.RefreshEntities(evnt.EventSourceId, evnt.Payload.StaticTexts);
        }

        public void Handle(IPublishedEvent<QuestionsDisabled> evnt)
        {
            this.webInterviewNotificationService.RefreshEntities(evnt.EventSourceId, evnt.Payload.Questions);
        }

        public void Handle(IPublishedEvent<QuestionsEnabled> evnt)
        {
            this.webInterviewNotificationService.RefreshEntities(evnt.EventSourceId, evnt.Payload.Questions);
        }

        public void Handle(IPublishedEvent<StaticTextsDisabled> evnt)
        {
            this.webInterviewNotificationService.RefreshEntities(evnt.EventSourceId, evnt.Payload.StaticTexts);
        }

        public void Handle(IPublishedEvent<StaticTextsEnabled> evnt)
        {
            this.webInterviewNotificationService.RefreshEntities(evnt.EventSourceId, evnt.Payload.StaticTexts); 
        }

        public void Handle(IPublishedEvent<TextQuestionAnswered> evnt)
        {
            this.webInterviewNotificationService.RefreshEntities(evnt.EventSourceId, new Identity(evnt.Payload.QuestionId, evnt.Payload.RosterVector));
            this.webInterviewNotificationService.RefreshEntitiesWithFilteredOptions(evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<AnswersRemoved> evnt)
        {
            this.webInterviewNotificationService.RefreshEntities(evnt.EventSourceId, evnt.Payload.Questions);
            this.webInterviewNotificationService.RefreshEntitiesWithFilteredOptions(evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<SingleOptionQuestionAnswered> evnt)
        {
            this.webInterviewNotificationService.RefreshEntities(evnt.EventSourceId, new Identity(evnt.Payload.QuestionId, evnt.Payload.RosterVector));
            this.webInterviewNotificationService.RefreshEntitiesWithFilteredOptions(evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<MultipleOptionsQuestionAnswered> evnt)        
        {
            this.webInterviewNotificationService.RefreshEntities(evnt.EventSourceId, new Identity(evnt.Payload.QuestionId, evnt.Payload.RosterVector));
            this.webInterviewNotificationService.RefreshEntitiesWithFilteredOptions(evnt.EventSourceId);
        }
        
        public void Handle(IPublishedEvent<NumericIntegerQuestionAnswered> evnt)
        {
            this.webInterviewNotificationService.RefreshEntities(evnt.EventSourceId, new Identity(evnt.Payload.QuestionId, evnt.Payload.RosterVector));
            this.webInterviewNotificationService.RefreshEntitiesWithFilteredOptions(evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<NumericRealQuestionAnswered> evnt)
        {
            this.webInterviewNotificationService.RefreshEntities(evnt.EventSourceId, new Identity(evnt.Payload.QuestionId, evnt.Payload.RosterVector));
            this.webInterviewNotificationService.RefreshEntitiesWithFilteredOptions(evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<SubstitutionTitlesChanged> evnt)
        {
            this.webInterviewNotificationService.RefreshEntities(evnt.EventSourceId, evnt.Payload.Questions);
            this.webInterviewNotificationService.RefreshEntities(evnt.EventSourceId, evnt.Payload.Groups);
            this.webInterviewNotificationService.RefreshEntities(evnt.EventSourceId, evnt.Payload.StaticTexts);
        }

        public void Handle(IPublishedEvent<StaticTextsDeclaredInvalid> evnt)
        {
            this.webInterviewNotificationService.RefreshEntities(evnt.EventSourceId, evnt.Payload.GetFailedValidationConditionsDictionary().Keys.ToArray());
        }

        public void Handle(IPublishedEvent<StaticTextsDeclaredValid> evnt)
        {
            this.webInterviewNotificationService.RefreshEntities(evnt.EventSourceId, evnt.Payload.StaticTexts);
        }

        public void Handle(IPublishedEvent<RosterInstancesAdded> evnt)
            => this.webInterviewNotificationService.RefreshEntities(evnt.EventSourceId,
                evnt.Payload.Instances.Select(x => x.GetIdentity()).ToArray());

        public void Handle(IPublishedEvent<RosterInstancesRemoved> evnt)
            => this.webInterviewNotificationService.RefreshRemovedEntities(evnt.EventSourceId,
                evnt.Payload.Instances.Select(x => x.GetIdentity()).ToArray());

        public void Handle(IPublishedEvent<RosterInstancesTitleChanged> evnt)
        {
            var rosterIdentities = evnt.Payload.ChangedInstances.Select(x => x.RosterInstance.GetIdentity()).ToArray();
            this.webInterviewNotificationService.RefreshEntities(evnt.EventSourceId, rosterIdentities);
            this.webInterviewNotificationService.RefreshLinkedToRosterQuestions(evnt.EventSourceId, rosterIdentities);
        }

        public void Handle(IPublishedEvent<GroupsEnabled> evnt)
            => this.webInterviewNotificationService.RefreshEntities(evnt.EventSourceId, evnt.Payload.Groups);

        public void Handle(IPublishedEvent<GroupsDisabled> evnt)
            => this.webInterviewNotificationService.RefreshEntities(evnt.EventSourceId, evnt.Payload.Groups);

        public void Handle(IPublishedEvent<DateTimeQuestionAnswered> evnt)
        {
            this.webInterviewNotificationService.RefreshEntities(evnt.EventSourceId, new Identity(evnt.Payload.QuestionId, evnt.Payload.RosterVector));
            this.webInterviewNotificationService.RefreshEntitiesWithFilteredOptions(evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<TextListQuestionAnswered> evnt)
        {
            var textListIdentity = new Identity(evnt.Payload.QuestionId, evnt.Payload.RosterVector);
            this.webInterviewNotificationService.RefreshEntities(evnt.EventSourceId, textListIdentity);
            this.webInterviewNotificationService.RefreshLinkedToListQuestions(evnt.EventSourceId, new[] {textListIdentity});
            this.webInterviewNotificationService.RefreshEntitiesWithFilteredOptions(evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<YesNoQuestionAnswered> evnt)
        {
            this.webInterviewNotificationService.RefreshEntities(evnt.EventSourceId, new Identity(evnt.Payload.QuestionId, evnt.Payload.RosterVector));
            this.webInterviewNotificationService.RefreshEntitiesWithFilteredOptions(evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<GeoLocationQuestionAnswered> evnt)
        {
            this.webInterviewNotificationService.RefreshEntities(evnt.EventSourceId, new Identity(evnt.Payload.QuestionId, evnt.Payload.RosterVector));
            this.webInterviewNotificationService.RefreshEntitiesWithFilteredOptions(evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<SingleOptionLinkedQuestionAnswered> evnt)
        {
            this.webInterviewNotificationService.RefreshEntities(evnt.EventSourceId, new Identity(evnt.Payload.QuestionId, evnt.Payload.RosterVector));
            this.webInterviewNotificationService.RefreshEntitiesWithFilteredOptions(evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<MultipleOptionsLinkedQuestionAnswered> evnt)
        {
            this.webInterviewNotificationService.RefreshEntities(evnt.EventSourceId, new Identity(evnt.Payload.QuestionId, evnt.Payload.RosterVector));
            this.webInterviewNotificationService.RefreshEntitiesWithFilteredOptions(evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<PictureQuestionAnswered> evnt)
        {
            this.webInterviewNotificationService.RefreshEntities(evnt.EventSourceId, new Identity(evnt.Payload.QuestionId, evnt.Payload.RosterVector));
            this.webInterviewNotificationService.RefreshEntitiesWithFilteredOptions(evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<QRBarcodeQuestionAnswered> evnt)
        {
            this.webInterviewNotificationService.RefreshEntities(evnt.EventSourceId, new Identity(evnt.Payload.QuestionId, evnt.Payload.RosterVector));
            this.webInterviewNotificationService.RefreshEntitiesWithFilteredOptions(evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<LinkedOptionsChanged> evnt) 
            => this.webInterviewNotificationService.RefreshEntities(evnt.EventSourceId, evnt.Payload.ChangedLinkedQuestions.Select(x => x.QuestionId).ToArray());

        public void Handle(IPublishedEvent<TranslationSwitched> evnt)
            => this.webInterviewNotificationService.ReloadInterview(evnt.EventSourceId);

        public void Handle(IPublishedEvent<InterviewerAssigned> evnt)
            => this.webInterviewNotificationService.ReloadInterview(evnt.EventSourceId);

        public void Handle(IPublishedEvent<InterviewCompleted> evnt)
            => this.webInterviewNotificationService.FinishInterview(evnt.EventSourceId);

        public void Handle(IPublishedEvent<LinkedToListOptionsChanged> evnt)
            => this.webInterviewNotificationService.RefreshEntities(evnt.EventSourceId, evnt.Payload.ChangedLinkedQuestions.Select(x => x.QuestionId).ToArray());
        
        public void Handle(IPublishedEvent<InterviewDeleted> evnt)
        {
            this.aggregateRootCacheCleaner.Evict(evnt.EventSourceId);
            this.webInterviewNotificationService.ReloadInterview(evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<InterviewHardDeleted> evnt)
        {
            this.aggregateRootCacheCleaner.Evict(evnt.EventSourceId);
            this.webInterviewNotificationService.ReloadInterview(evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<AreaQuestionAnswered> evnt)
        {
            this.webInterviewNotificationService.RefreshEntities(evnt.EventSourceId, new Identity(evnt.Payload.QuestionId, evnt.Payload.RosterVector));
        }

        public void Handle(IPublishedEvent<AudioQuestionAnswered> evnt)
        {
            this.webInterviewNotificationService.RefreshEntities(evnt.EventSourceId, new Identity(evnt.Payload.QuestionId, evnt.Payload.RosterVector));
        }

        public void Handle(IPublishedEvent<AnswerCommented> evnt)
        {
            this.webInterviewNotificationService.RefreshEntities(evnt.EventSourceId, new Identity(evnt.Payload.QuestionId, evnt.Payload.RosterVector));
        }
    }
}
