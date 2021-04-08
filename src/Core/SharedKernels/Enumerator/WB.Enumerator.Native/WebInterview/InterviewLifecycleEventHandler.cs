using System.Collections.Generic;
using System.Linq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.Infrastructure.Ncqrs.Eventing;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Enumerator.Native.WebInterview.LifeCycle;

namespace WB.Enumerator.Native.WebInterview
{
    [ReceivesIgnoredEvents]
    [ReceivesPrototypeEvents]
    public class InterviewLifecycleEventHandler : FunctionalEventHandlerBase<InterviewLifecycle>,
        IFunctionalEventHandler,
        IUpdateHandler<InterviewLifecycle, AnswersDeclaredInvalid>,
        IUpdateHandler<InterviewLifecycle, AnswersDeclaredValid>,
        IUpdateHandler<InterviewLifecycle, QuestionsDisabled>,
        IUpdateHandler<InterviewLifecycle, QuestionsEnabled>,
        IUpdateHandler<InterviewLifecycle, StaticTextsDisabled>,
        IUpdateHandler<InterviewLifecycle, StaticTextsEnabled>,
        IUpdateHandler<InterviewLifecycle, TextQuestionAnswered>,
        IUpdateHandler<InterviewLifecycle, TextListQuestionAnswered>,
        IUpdateHandler<InterviewLifecycle, SingleOptionQuestionAnswered>,
        IUpdateHandler<InterviewLifecycle, MultipleOptionsQuestionAnswered>,
        IUpdateHandler<InterviewLifecycle, DateTimeQuestionAnswered>,
        IUpdateHandler<InterviewLifecycle, SubstitutionTitlesChanged>,
        IUpdateHandler<InterviewLifecycle, NumericIntegerQuestionAnswered>,
        IUpdateHandler<InterviewLifecycle, NumericRealQuestionAnswered>,
        IUpdateHandler<InterviewLifecycle, YesNoQuestionAnswered>,
        IUpdateHandler<InterviewLifecycle, GeoLocationQuestionAnswered>,
        IUpdateHandler<InterviewLifecycle, SingleOptionLinkedQuestionAnswered>,
        IUpdateHandler<InterviewLifecycle, MultipleOptionsLinkedQuestionAnswered>,
        IUpdateHandler<InterviewLifecycle, PictureQuestionAnswered>,
        IUpdateHandler<InterviewLifecycle, QRBarcodeQuestionAnswered>,
        IUpdateHandler<InterviewLifecycle, LinkedOptionsChanged>,
        IUpdateHandler<InterviewLifecycle, LinkedToListOptionsChanged>,
        IUpdateHandler<InterviewLifecycle, AnswersRemoved>,
        IUpdateHandler<InterviewLifecycle, StaticTextsDeclaredInvalid>,
        IUpdateHandler<InterviewLifecycle, StaticTextsDeclaredValid>,
        IUpdateHandler<InterviewLifecycle, AnswersDeclaredImplausible>,
        IUpdateHandler<InterviewLifecycle, AnswersDeclaredPlausible>,
        IUpdateHandler<InterviewLifecycle, StaticTextsDeclaredImplausible>,
        IUpdateHandler<InterviewLifecycle, StaticTextsDeclaredPlausible>,
        IUpdateHandler<InterviewLifecycle, RosterInstancesAdded>,
        IUpdateHandler<InterviewLifecycle, RosterInstancesRemoved>,
        IUpdateHandler<InterviewLifecycle, RosterInstancesTitleChanged>,
        IUpdateHandler<InterviewLifecycle, GroupsEnabled>,
        IUpdateHandler<InterviewLifecycle, GroupsDisabled>,
        IUpdateHandler<InterviewLifecycle, TranslationSwitched>,
        IUpdateHandler<InterviewLifecycle, InterviewCompleted>,
        IUpdateHandler<InterviewLifecycle, InterviewDeleted>,
        IUpdateHandler<InterviewLifecycle, InterviewStatusChanged>,
        IUpdateHandler<InterviewLifecycle, InterviewHardDeleted>,
        IUpdateHandler<InterviewLifecycle, InterviewerAssigned>,
        IUpdateHandler<InterviewLifecycle, AreaQuestionAnswered>,
        IUpdateHandler<InterviewLifecycle, AudioQuestionAnswered>,
        IUpdateHandler<InterviewLifecycle, AnswerCommented>,
        IUpdateHandler<InterviewLifecycle, AnswerCommentResolved>,
        IUpdateHandler<InterviewLifecycle, VariablesChanged>,
        IUpdateHandler<InterviewLifecycle, VariablesEnabled>,
        IUpdateHandler<InterviewLifecycle, VariablesDisabled>,
        IUpdateHandler<InterviewLifecycle, InterviewModeChanged>

    {
        private readonly IAggregateRootCache aggregateRootCache;
        private readonly IWebInterviewNotificationService webInterviewNotificationService;
        private readonly IWebInterviewNotificationBuilder notificationBuilder;

        public InterviewLifecycleEventHandler(IAggregateRootCache aggregateRootCache,
            IWebInterviewNotificationService webInterviewNotificationService, 
            IWebInterviewNotificationBuilder notificationBuilder)
        {
            this.aggregateRootCache = aggregateRootCache;
            this.webInterviewNotificationService = webInterviewNotificationService;
            this.notificationBuilder = notificationBuilder;
        }

        public void Handle(IEnumerable<IPublishableEvent> publishableEvents)
        {
            var state = new InterviewLifecycle();

            foreach (var publishableEvent in publishableEvents)
            {
                if (this.Handles(publishableEvent))
                {
                    state = ApplyEventOnEntity(publishableEvent, state);
                }
            }

            foreach (var store in state.Store)
            {
                var aggId = store.Key;
                var actionStore = store.Value;

                if (actionStore.FinishInterview)
                {
                    webInterviewNotificationService.FinishInterview(aggId);
                    continue;
                }

                if (actionStore.ReloadInterview)
                {
                    webInterviewNotificationService.ReloadInterview(aggId);
                }

                if (actionStore.RefreshFilteredOptions)
                {
                    notificationBuilder.RefreshEntitiesWithFilteredOptions(state, aggId);
                }
                
                if (actionStore.RefreshRemovedEntities.Any())
                {
                    webInterviewNotificationService.RefreshRemovedEntities(aggId,
                        actionStore.RefreshRemovedEntities.ToArray());
                }
                else
                {
                    webInterviewNotificationService.RefreshEntities(aggId, actionStore.RefreshEntities.ToArray());
                }
            }
        }

        public InterviewLifecycle Update(InterviewLifecycle cycle, IPublishedEvent<AnswersDeclaredInvalid> evnt)
        {
            return cycle.RefreshEntities(evnt.EventSourceId, evnt.Payload.FailedValidationConditions.Keys);
        }

        public InterviewLifecycle Update(InterviewLifecycle cycle, IPublishedEvent<AnswersDeclaredValid> evnt)
        {
            return cycle.RefreshEntities(evnt.EventSourceId, evnt.Payload.Questions);
        }

        public InterviewLifecycle Update(InterviewLifecycle cycle, IPublishedEvent<AnswersDeclaredImplausible> evnt)
        {
            return cycle.RefreshEntities(evnt.EventSourceId, evnt.Payload.GetFailedValidationConditionsDictionary().Keys.ToArray());
        }

        public InterviewLifecycle Update(InterviewLifecycle cycle, IPublishedEvent<AnswersDeclaredPlausible> evnt)
        {
            return cycle.RefreshEntities(evnt.EventSourceId, evnt.Payload.Questions);
        }

        public InterviewLifecycle Update(InterviewLifecycle cycle, IPublishedEvent<StaticTextsDeclaredImplausible> evnt)
        {
            return cycle.RefreshEntities(evnt.EventSourceId, evnt.Payload.GetFailedValidationConditionsDictionary().Keys.ToArray());
        }

        public InterviewLifecycle Update(InterviewLifecycle cycle, IPublishedEvent<StaticTextsDeclaredPlausible> evnt)
        {
            return cycle.RefreshEntities(evnt.EventSourceId, evnt.Payload.StaticTexts);
        }

        public InterviewLifecycle Update(InterviewLifecycle cycle, IPublishedEvent<QuestionsDisabled> evnt)
        {
            return cycle.RefreshEntities(evnt.EventSourceId, evnt.Payload.Questions);
        }

        public InterviewLifecycle Update(InterviewLifecycle cycle, IPublishedEvent<QuestionsEnabled> evnt)
        {
            return cycle.RefreshEntities(evnt.EventSourceId, evnt.Payload.Questions);
        }

        public InterviewLifecycle Update(InterviewLifecycle cycle, IPublishedEvent<StaticTextsDisabled> evnt)
        {
            return cycle.RefreshEntities(evnt.EventSourceId, evnt.Payload.StaticTexts);
        }

        public InterviewLifecycle Update(InterviewLifecycle cycle, IPublishedEvent<StaticTextsEnabled> evnt)
        {
            return cycle.RefreshEntities(evnt.EventSourceId, evnt.Payload.StaticTexts);
        }

        public InterviewLifecycle Update(InterviewLifecycle cycle, IPublishedEvent<TextQuestionAnswered> evnt)
        {
            cycle.RefreshEntities(evnt.EventSourceId, new Identity(evnt.Payload.QuestionId, evnt.Payload.RosterVector));
            return cycle.RefreshEntitiesWithFilteredOptions(evnt.EventSourceId);
        }

        public InterviewLifecycle Update(InterviewLifecycle cycle, IPublishedEvent<AnswersRemoved> evnt)
        {
            cycle.RefreshEntities(evnt.EventSourceId, evnt.Payload.Questions);
            return cycle.RefreshEntitiesWithFilteredOptions(evnt.EventSourceId);
        }

        public InterviewLifecycle Update(InterviewLifecycle cycle, IPublishedEvent<SingleOptionQuestionAnswered> evnt)
        {
            cycle.RefreshEntities(evnt.EventSourceId, new Identity(evnt.Payload.QuestionId, evnt.Payload.RosterVector));
            cycle.RefreshEntitiesWithFilteredOptions(evnt.EventSourceId);
            notificationBuilder.RefreshCascadingOptions(cycle, evnt.EventSourceId,
                new Identity(evnt.Payload.QuestionId, evnt.Payload.RosterVector));
            return cycle;
        }

        public InterviewLifecycle Update(InterviewLifecycle cycle, IPublishedEvent<MultipleOptionsQuestionAnswered> evnt)
        {
            cycle.RefreshEntities(evnt.EventSourceId, new Identity(evnt.Payload.QuestionId, evnt.Payload.RosterVector));
            return cycle.RefreshEntitiesWithFilteredOptions(evnt.EventSourceId);
        }

        public InterviewLifecycle Update(InterviewLifecycle cycle, IPublishedEvent<NumericIntegerQuestionAnswered> evnt)
        {
            cycle.RefreshEntities(evnt.EventSourceId, new Identity(evnt.Payload.QuestionId, evnt.Payload.RosterVector));
            return cycle.RefreshEntitiesWithFilteredOptions(evnt.EventSourceId);
        }

        public InterviewLifecycle Update(InterviewLifecycle cycle, IPublishedEvent<NumericRealQuestionAnswered> evnt)
        {
            cycle.RefreshEntities(evnt.EventSourceId, new Identity(evnt.Payload.QuestionId, evnt.Payload.RosterVector));
            return cycle.RefreshEntitiesWithFilteredOptions(evnt.EventSourceId);
        }

        public InterviewLifecycle Update(InterviewLifecycle cycle, IPublishedEvent<SubstitutionTitlesChanged> evnt)
        {
            cycle.RefreshEntities(evnt.EventSourceId, evnt.Payload.Questions);
            cycle.RefreshEntities(evnt.EventSourceId, evnt.Payload.Groups);
            return cycle.RefreshEntities(evnt.EventSourceId, evnt.Payload.StaticTexts);
        }

        public InterviewLifecycle Update(InterviewLifecycle cycle, IPublishedEvent<StaticTextsDeclaredInvalid> evnt)
        {
            return cycle.RefreshEntities(evnt.EventSourceId, evnt.Payload.GetFailedValidationConditionsDictionary().Keys.ToArray());
        }

        public InterviewLifecycle Update(InterviewLifecycle cycle, IPublishedEvent<StaticTextsDeclaredValid> evnt)
        {
            return cycle.RefreshEntities(evnt.EventSourceId, evnt.Payload.StaticTexts);
        }

        public InterviewLifecycle Update(InterviewLifecycle cycle, IPublishedEvent<RosterInstancesAdded> evnt)
        {
            return cycle.RefreshEntities(evnt.EventSourceId,
                evnt.Payload.Instances.Select(x => x.GetIdentity()));
        }

        public InterviewLifecycle Update(InterviewLifecycle cycle, IPublishedEvent<RosterInstancesRemoved> evnt)
            => cycle.RefreshRemovedEntities(evnt.EventSourceId, evnt.Payload.Instances.Select(x => x.GetIdentity()));

        public InterviewLifecycle Update(InterviewLifecycle cycle, IPublishedEvent<RosterInstancesTitleChanged> evnt)
        {
            var rosterIdentities = evnt.Payload.ChangedInstances.Select(x => x.RosterInstance.GetIdentity()).ToArray();
            cycle.RefreshEntities(evnt.EventSourceId, rosterIdentities);
            notificationBuilder.RefreshLinkedToRosterQuestions(cycle, evnt.EventSourceId, rosterIdentities);
            return cycle;
        }

        public InterviewLifecycle Update(InterviewLifecycle cycle, IPublishedEvent<GroupsEnabled> evnt)
            => cycle.RefreshEntities(evnt.EventSourceId, evnt.Payload.Groups);

        public InterviewLifecycle Update(InterviewLifecycle cycle, IPublishedEvent<GroupsDisabled> evnt)
            => cycle.RefreshEntities(evnt.EventSourceId, evnt.Payload.Groups);

        public InterviewLifecycle Update(InterviewLifecycle cycle, IPublishedEvent<DateTimeQuestionAnswered> evnt)
        {
            cycle.RefreshEntities(evnt.EventSourceId, new Identity(evnt.Payload.QuestionId, evnt.Payload.RosterVector));
            return cycle.RefreshEntitiesWithFilteredOptions(evnt.EventSourceId);
        }

        public InterviewLifecycle Update(InterviewLifecycle cycle, IPublishedEvent<TextListQuestionAnswered> evnt)
        {
            var textListIdentity = new Identity(evnt.Payload.QuestionId, evnt.Payload.RosterVector);
            cycle.RefreshEntities(evnt.EventSourceId, textListIdentity);
            notificationBuilder.RefreshLinkedToListQuestions(cycle, evnt.EventSourceId, textListIdentity);
            return cycle.RefreshEntitiesWithFilteredOptions(evnt.EventSourceId);
        }

        public InterviewLifecycle Update(InterviewLifecycle cycle, IPublishedEvent<YesNoQuestionAnswered> evnt)
        {
            cycle.RefreshEntities(evnt.EventSourceId, new Identity(evnt.Payload.QuestionId, evnt.Payload.RosterVector));
            return cycle.RefreshEntitiesWithFilteredOptions(evnt.EventSourceId);
        }

        public InterviewLifecycle Update(InterviewLifecycle cycle, IPublishedEvent<GeoLocationQuestionAnswered> evnt)
        {
            cycle.RefreshEntities(evnt.EventSourceId, new Identity(evnt.Payload.QuestionId, evnt.Payload.RosterVector));
            return cycle.RefreshEntitiesWithFilteredOptions(evnt.EventSourceId);
        }

        public InterviewLifecycle Update(InterviewLifecycle cycle, IPublishedEvent<SingleOptionLinkedQuestionAnswered> evnt)
        {
            cycle.RefreshEntities(evnt.EventSourceId, new Identity(evnt.Payload.QuestionId, evnt.Payload.RosterVector));
            return cycle.RefreshEntitiesWithFilteredOptions(evnt.EventSourceId);
        }

        public InterviewLifecycle Update(InterviewLifecycle cycle, IPublishedEvent<MultipleOptionsLinkedQuestionAnswered> evnt)
        {
            cycle.RefreshEntities(evnt.EventSourceId, new Identity(evnt.Payload.QuestionId, evnt.Payload.RosterVector));
            return cycle.RefreshEntitiesWithFilteredOptions(evnt.EventSourceId);
        }

        public InterviewLifecycle Update(InterviewLifecycle cycle, IPublishedEvent<PictureQuestionAnswered> evnt)
        {
            cycle.RefreshEntities(evnt.EventSourceId, new Identity(evnt.Payload.QuestionId, evnt.Payload.RosterVector));
            return cycle.RefreshEntitiesWithFilteredOptions(evnt.EventSourceId);
        }

        public InterviewLifecycle Update(InterviewLifecycle cycle, IPublishedEvent<QRBarcodeQuestionAnswered> evnt)
        {
            cycle.RefreshEntities(evnt.EventSourceId, new Identity(evnt.Payload.QuestionId, evnt.Payload.RosterVector));
            return cycle.RefreshEntitiesWithFilteredOptions(evnt.EventSourceId);
        }

        public InterviewLifecycle Update(InterviewLifecycle cycle, IPublishedEvent<LinkedOptionsChanged> evnt)
            => cycle.RefreshEntities(evnt.EventSourceId, evnt.Payload.ChangedLinkedQuestions.Select(x => x.QuestionId));

        public InterviewLifecycle Update(InterviewLifecycle cycle, IPublishedEvent<TranslationSwitched> evnt)
        {
            if (!evnt.IsPrototype())
            {
                cycle.ReloadInterview(evnt.EventSourceId);
            }

            return cycle;
        }

        public InterviewLifecycle Update(InterviewLifecycle cycle, IPublishedEvent<InterviewerAssigned> evnt)
        {
            if (!evnt.IsPrototype())
            {
                cycle.ReloadInterview(evnt.EventSourceId);
            }

            return cycle;
        }

        public InterviewLifecycle Update(InterviewLifecycle cycle, IPublishedEvent<InterviewCompleted> evnt)
            => cycle.FinishInterview(evnt.EventSourceId);

        public InterviewLifecycle Update(InterviewLifecycle cycle, IPublishedEvent<LinkedToListOptionsChanged> evnt)
            => cycle.RefreshEntities(evnt.EventSourceId, evnt.Payload.ChangedLinkedQuestions.Select(x => x.QuestionId));

        public InterviewLifecycle Update(InterviewLifecycle cycle, IPublishedEvent<InterviewDeleted> evnt)
        {
            this.aggregateRootCache.Evict(evnt.EventSourceId);
            return cycle.ReloadInterview(evnt.EventSourceId);
        }

        public InterviewLifecycle Update(InterviewLifecycle cycle, IPublishedEvent<InterviewHardDeleted> evnt)
        {
            this.aggregateRootCache.Evict(evnt.EventSourceId);
            return cycle.ReloadInterview(evnt.EventSourceId);
        }

        public InterviewLifecycle Update(InterviewLifecycle cycle, IPublishedEvent<InterviewStatusChanged> evnt)
        {
            if (!evnt.IsPrototype() && evnt.Payload.Status != InterviewStatus.Completed)
            {
                cycle.ReloadInterview(evnt.EventSourceId);
            }

            return cycle;
        }

        public InterviewLifecycle Update(InterviewLifecycle cycle, IPublishedEvent<AreaQuestionAnswered> evnt)
        {
            return cycle.RefreshEntities(evnt.EventSourceId, new Identity(evnt.Payload.QuestionId, evnt.Payload.RosterVector));
        }

        public InterviewLifecycle Update(InterviewLifecycle cycle, IPublishedEvent<AudioQuestionAnswered> evnt)
        {
            return cycle.RefreshEntities(evnt.EventSourceId, new Identity(evnt.Payload.QuestionId, evnt.Payload.RosterVector));
        }

        public InterviewLifecycle Update(InterviewLifecycle cycle, IPublishedEvent<AnswerCommented> evnt)
        {
            return cycle.RefreshEntities(evnt.EventSourceId, new Identity(evnt.Payload.QuestionId, evnt.Payload.RosterVector));
        }

        public InterviewLifecycle Update(InterviewLifecycle cycle, IPublishedEvent<AnswerCommentResolved> evnt)
        {
            return cycle.RefreshEntities(evnt.EventSourceId, new Identity(evnt.Payload.QuestionId, evnt.Payload.RosterVector));
        }

        public InterviewLifecycle Update(InterviewLifecycle cycle, IPublishedEvent<VariablesChanged> evnt)
        {
            return cycle.RefreshEntities(evnt.EventSourceId, evnt.Payload.ChangedVariables.Select(x => x.Identity).ToArray());
        }

        public InterviewLifecycle Update(InterviewLifecycle cycle, IPublishedEvent<VariablesEnabled> evnt)
        {
            return cycle.RefreshEntities(evnt.EventSourceId, evnt.Payload.Variables);
        }

        public InterviewLifecycle Update(InterviewLifecycle cycle, IPublishedEvent<VariablesDisabled> evnt)
        {
            return cycle.RefreshEntities(evnt.EventSourceId, evnt.Payload.Variables);
        }

        public InterviewLifecycle Update(InterviewLifecycle cycle, IPublishedEvent<InterviewModeChanged> @event)
        {
            if (!@event.IsPrototype())
            {
                return cycle.FinishInterview(@event.EventSourceId);
            }

            return cycle;
        }
    }
}
