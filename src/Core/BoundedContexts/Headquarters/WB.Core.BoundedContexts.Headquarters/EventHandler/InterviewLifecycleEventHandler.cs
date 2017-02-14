using System;
using System.Linq;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Eventing.ServiceModel.Bus;
using Quartz.Util;
using WB.Core.BoundedContexts.Headquarters.Services.WebInterview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Infrastructure.Native.Storage;

namespace WB.Core.BoundedContexts.Headquarters.EventHandler
{
    internal class InterviewLifecycleEventHandler :
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
        IEventHandler<RosterInstancesAdded>,
        IEventHandler<RosterInstancesRemoved>,
        IEventHandler<RosterInstancesTitleChanged>,
        IEventHandler<GroupsEnabled>,
        IEventHandler<GroupsDisabled>,
        IEventHandler<TranslationSwitched>,
        IEventHandler<InterviewCompleted>,
        IEventHandler<InterviewDeleted>,
        IEventHandler<InterviewHardDeleted>
    {
        public override object[] Writers => new object[0];

        private readonly IWebInterviewNotificationService webInterviewNotificationService;
        private readonly IStatefulInterviewRepository statefulInterviewRepository;
        private readonly IAggregateRootCacheCleaner aggregateRootCacheCleaner;
        private readonly IQuestionnaireStorage questionnaireRepository;

        public InterviewLifecycleEventHandler(IWebInterviewNotificationService webInterviewNotificationService,
            IStatefulInterviewRepository statefulInterviewRepository,
            IQuestionnaireStorage questionnaireRepository,
            IAggregateRootCacheCleaner aggregateRootCacheCleaner)
        {
            this.webInterviewNotificationService = webInterviewNotificationService;
            this.statefulInterviewRepository = statefulInterviewRepository;
            this.aggregateRootCacheCleaner = aggregateRootCacheCleaner;
            this.questionnaireRepository = questionnaireRepository;
        }

        public void Handle(IPublishedEvent<AnswersDeclaredInvalid> @event)
        {
            this.webInterviewNotificationService.RefreshEntities(@event.EventSourceId, @event.Payload.Questions);
        }

        public void Handle(IPublishedEvent<AnswersDeclaredValid> @event)
        {
            this.webInterviewNotificationService.RefreshEntities(@event.EventSourceId, @event.Payload.Questions);
        }

        public void Handle(IPublishedEvent<QuestionsDisabled> evnt)
        {
            this.webInterviewNotificationService.RefreshEntities(evnt.EventSourceId, evnt.Payload.Questions);
            this.RefreshLinkedToListQuestions(evnt.EventSourceId, evnt.Payload.Questions);
        }

        public void Handle(IPublishedEvent<QuestionsEnabled> evnt)
        {
            this.webInterviewNotificationService.RefreshEntities(evnt.EventSourceId, evnt.Payload.Questions);
            this.RefreshLinkedToListQuestions(evnt.EventSourceId, evnt.Payload.Questions);
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
            this.RefreshEntitiesWithFilteredOptions(evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<AnswersRemoved> evnt)
        {
            this.webInterviewNotificationService.RefreshEntities(evnt.EventSourceId, evnt.Payload.Questions);
            this.RefreshEntitiesWithFilteredOptions(evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<SingleOptionQuestionAnswered> evnt)
        {
            this.webInterviewNotificationService.RefreshEntities(evnt.EventSourceId, new Identity(evnt.Payload.QuestionId, evnt.Payload.RosterVector));
            this.RefreshEntitiesWithFilteredOptions(evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<MultipleOptionsQuestionAnswered> evnt)        
        {
            this.webInterviewNotificationService.RefreshEntities(evnt.EventSourceId, new Identity(evnt.Payload.QuestionId, evnt.Payload.RosterVector));
            this.RefreshEntitiesWithFilteredOptions(evnt.EventSourceId);
        }
        
        public void Handle(IPublishedEvent<NumericIntegerQuestionAnswered> evnt)
        {
            this.webInterviewNotificationService.RefreshEntities(evnt.EventSourceId, new Identity(evnt.Payload.QuestionId, evnt.Payload.RosterVector));
            this.RefreshEntitiesWithFilteredOptions(evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<NumericRealQuestionAnswered> evnt)
        {
            this.webInterviewNotificationService.RefreshEntities(evnt.EventSourceId, new Identity(evnt.Payload.QuestionId, evnt.Payload.RosterVector));
            this.RefreshEntitiesWithFilteredOptions(evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<SubstitutionTitlesChanged> evnt)
        {
            this.webInterviewNotificationService.RefreshEntities(evnt.EventSourceId, evnt.Payload.Questions);
            this.webInterviewNotificationService.RefreshEntities(evnt.EventSourceId, evnt.Payload.Groups);
            this.webInterviewNotificationService.RefreshEntities(evnt.EventSourceId, evnt.Payload.StaticTexts);
        }

        public void Handle(IPublishedEvent<StaticTextsDeclaredInvalid> @event)
        {
            this.webInterviewNotificationService.RefreshEntities(@event.EventSourceId, @event.Payload.GetFailedValidationConditionsDictionary().Keys.ToArray());
        }

        public void Handle(IPublishedEvent<StaticTextsDeclaredValid> @event)
        {
            this.webInterviewNotificationService.RefreshEntities(@event.EventSourceId, @event.Payload.StaticTexts);
        }

        public void Handle(IPublishedEvent<RosterInstancesAdded> @event)
            => this.webInterviewNotificationService.RefreshEntities(@event.EventSourceId,
                @event.Payload.Instances.Select(x => x.GetIdentity()).ToArray());

        public void Handle(IPublishedEvent<RosterInstancesRemoved> @event)
            => this.webInterviewNotificationService.RefreshRemovedEntities(@event.EventSourceId,
                @event.Payload.Instances.Select(x => x.GetIdentity()).ToArray());

        public void Handle(IPublishedEvent<RosterInstancesTitleChanged> @event)
            => this.webInterviewNotificationService.RefreshEntities(@event.EventSourceId,
                @event.Payload.ChangedInstances.Select(x => x.RosterInstance.GetIdentity()).ToArray());

        public void Handle(IPublishedEvent<GroupsEnabled> @event)
            => this.webInterviewNotificationService.RefreshEntities(@event.EventSourceId, @event.Payload.Groups);

        public void Handle(IPublishedEvent<GroupsDisabled> @event)
            => this.webInterviewNotificationService.RefreshEntities(@event.EventSourceId, @event.Payload.Groups);

        public void Handle(IPublishedEvent<DateTimeQuestionAnswered> evnt)
        {
            this.webInterviewNotificationService.RefreshEntities(evnt.EventSourceId, new Identity(evnt.Payload.QuestionId, evnt.Payload.RosterVector));
            this.RefreshEntitiesWithFilteredOptions(evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<TextListQuestionAnswered> evnt)
        {
            var textListIdentity = new Identity(evnt.Payload.QuestionId, evnt.Payload.RosterVector);
            this.webInterviewNotificationService.RefreshEntities(evnt.EventSourceId, textListIdentity);
            this.RefreshLinkedToListQuestions(evnt.EventSourceId, new[] {textListIdentity});
            this.RefreshEntitiesWithFilteredOptions(evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<YesNoQuestionAnswered> evnt)
        {
            this.webInterviewNotificationService.RefreshEntities(evnt.EventSourceId, new Identity(evnt.Payload.QuestionId, evnt.Payload.RosterVector));
            this.RefreshEntitiesWithFilteredOptions(evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<GeoLocationQuestionAnswered> evnt)
        {
            this.webInterviewNotificationService.RefreshEntities(evnt.EventSourceId, new Identity(evnt.Payload.QuestionId, evnt.Payload.RosterVector));
            this.RefreshEntitiesWithFilteredOptions(evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<SingleOptionLinkedQuestionAnswered> evnt)
        {
            this.webInterviewNotificationService.RefreshEntities(evnt.EventSourceId, new Identity(evnt.Payload.QuestionId, evnt.Payload.RosterVector));
            this.RefreshEntitiesWithFilteredOptions(evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<MultipleOptionsLinkedQuestionAnswered> evnt)
        {
            this.webInterviewNotificationService.RefreshEntities(evnt.EventSourceId, new Identity(evnt.Payload.QuestionId, evnt.Payload.RosterVector));
            this.RefreshEntitiesWithFilteredOptions(evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<PictureQuestionAnswered> evnt)
        {
            this.webInterviewNotificationService.RefreshEntities(evnt.EventSourceId, new Identity(evnt.Payload.QuestionId, evnt.Payload.RosterVector));
            this.RefreshEntitiesWithFilteredOptions(evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<QRBarcodeQuestionAnswered> evnt)
        {
            this.webInterviewNotificationService.RefreshEntities(evnt.EventSourceId, new Identity(evnt.Payload.QuestionId, evnt.Payload.RosterVector));
            this.RefreshEntitiesWithFilteredOptions(evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<LinkedOptionsChanged> evnt) 
            => this.webInterviewNotificationService.RefreshEntities(evnt.EventSourceId, evnt.Payload.ChangedLinkedQuestions.Select(x => x.QuestionId).ToArray());

        public void Handle(IPublishedEvent<TranslationSwitched> evnt)
            => this.webInterviewNotificationService.ReloadInterview(evnt.EventSourceId);

        public void Handle(IPublishedEvent<InterviewCompleted> evnt)
            => this.webInterviewNotificationService.FinishInterview(evnt.EventSourceId);

        public void Handle(IPublishedEvent<LinkedToListOptionsChanged> evnt)
            => this.webInterviewNotificationService.RefreshEntities(evnt.EventSourceId, evnt.Payload.ChangedLinkedQuestions.Select(x => x.QuestionId).ToArray());
        
        private void RefreshLinkedToListQuestions(Guid interviewId, Identity[] questionIdentities)
        {
            var interview = this.statefulInterviewRepository.Get(interviewId.FormatGuid());
            var questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);

            foreach (var questionIdentity in questionIdentities)
            {
                if(interview.GetTextListQuestion(questionIdentity) == null) continue;
                
                Guid[] listQuestionIds = questionnaire.GetLinkedToSourceQuestions(questionIdentity.Id).ToArray();
                if (!listQuestionIds.Any())
                    return;

                foreach (var listQuestionId in listQuestionIds)
                {
                    var questionsToRefresh = interview.FindQuestionsFromSameOrDeeperLevel(listQuestionId, questionIdentity);
                    this.webInterviewNotificationService.RefreshEntities(interviewId, questionsToRefresh.ToArray());
                }
            }
        }

        private void RefreshEntitiesWithFilteredOptions(Guid interviewId)
        {
            var interview = this.statefulInterviewRepository.Get(interviewId.FormatGuid());
            var document = this.questionnaireRepository.GetQuestionnaireDocument(interview.QuestionnaireIdentity);

            var entityIds = document.Find<IComposite>(IsSupportFilterOptionCondition)
                .Select(e => e.PublicKey).ToHashSet();

            foreach (var entityId in entityIds)
            {
                var identities = interview.GetAllIdentitiesForEntityId(entityId).ToArray();
                this.webInterviewNotificationService.RefreshEntities(interviewId, identities);
            }
        }

        private bool IsSupportFilterOptionCondition(IComposite documentEntity)
        {
            var question = documentEntity as IQuestion;
            if (question != null && !question.Properties.OptionsFilterExpression.IsNullOrWhiteSpace())
                return true;

            return false;
        }

        public void Handle(IPublishedEvent<InterviewDeleted> evnt)
        {
            this.aggregateRootCacheCleaner?.Evict(evnt.EventSourceId);
            this.webInterviewNotificationService.ReloadInterview(evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<InterviewHardDeleted> evnt)
        {
            this.aggregateRootCacheCleaner?.Evict(evnt.EventSourceId);
            this.webInterviewNotificationService.ReloadInterview(evnt.EventSourceId);
        }
    }
}