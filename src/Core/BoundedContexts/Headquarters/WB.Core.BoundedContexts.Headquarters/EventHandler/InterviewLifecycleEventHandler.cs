using System.Linq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Headquarters.Services.WebInterview;
using WB.Core.Infrastructure.EventBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Utils;

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
        IEventHandler<LinkedOptionsChanged>,
        IEventHandler<AnswersRemoved>,
        IEventHandler<StaticTextsDeclaredInvalid>,
        IEventHandler<StaticTextsDeclaredValid>,
        IEventHandler<RosterInstancesAdded>,
        IEventHandler<RosterInstancesRemoved>,
        IEventHandler<RosterInstancesTitleChanged>,
        IEventHandler<GroupsEnabled>,
        IEventHandler<GroupsDisabled>
    {
        public override object[] Writers => new object[0];

        private readonly IWebInterviewNotificationService webInterviewNotificationService;

        public InterviewLifecycleEventHandler(IWebInterviewNotificationService webInterviewNotificationService)
        {
            this.webInterviewNotificationService = webInterviewNotificationService;
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
        }

        public void Handle(IPublishedEvent<AnswersRemoved> evnt)
        {
            this.webInterviewNotificationService.RefreshEntities(evnt.EventSourceId, evnt.Payload.Questions);
        }

        public void Handle(IPublishedEvent<SingleOptionQuestionAnswered> evnt)
        {
            this.webInterviewNotificationService.RefreshEntities(evnt.EventSourceId, new Identity(evnt.Payload.QuestionId, evnt.Payload.RosterVector));
        }

        public void Handle(IPublishedEvent<MultipleOptionsQuestionAnswered> evnt)        
        {
            this.webInterviewNotificationService.RefreshEntities(evnt.EventSourceId, new Identity(evnt.Payload.QuestionId, evnt.Payload.RosterVector));
        }
        
        public void Handle(IPublishedEvent<NumericIntegerQuestionAnswered> evnt)
        {
            this.webInterviewNotificationService.RefreshEntities(evnt.EventSourceId, new Identity(evnt.Payload.QuestionId, evnt.Payload.RosterVector));
        }

        public void Handle(IPublishedEvent<NumericRealQuestionAnswered> evnt)
        {
            this.webInterviewNotificationService.RefreshEntities(evnt.EventSourceId, new Identity(evnt.Payload.QuestionId, evnt.Payload.RosterVector));
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
            => this.webInterviewNotificationService.RefreshEntities(@event.EventSourceId,
                @event.Payload.Instances.Select(x => x.GetIdentity()).ToArray());

        public void Handle(IPublishedEvent<RosterInstancesTitleChanged> @event)
            => this.webInterviewNotificationService.RefreshEntities(@event.EventSourceId,
                @event.Payload.ChangedInstances.Select(x => x.RosterInstance.GetIdentity()).ToArray());

        public void Handle(IPublishedEvent<GroupsEnabled> @event)
            => this.webInterviewNotificationService.RefreshEntities(@event.EventSourceId, @event.Payload.Groups);

        public void Handle(IPublishedEvent<GroupsDisabled> @event)
            => this.webInterviewNotificationService.RefreshEntities(@event.EventSourceId, @event.Payload.Groups);

        public void Handle(IPublishedEvent<DateTimeQuestionAnswered> evnt) 
            => this.webInterviewNotificationService.RefreshEntities(evnt.EventSourceId, new Identity(evnt.Payload.QuestionId, evnt.Payload.RosterVector));

        public void Handle(IPublishedEvent<TextListQuestionAnswered> evnt)
            => this.webInterviewNotificationService.RefreshEntities(evnt.EventSourceId, new Identity(evnt.Payload.QuestionId, evnt.Payload.RosterVector));

        public void Handle(IPublishedEvent<YesNoQuestionAnswered> evnt)
            => this.webInterviewNotificationService.RefreshEntities(evnt.EventSourceId, new Identity(evnt.Payload.QuestionId, evnt.Payload.RosterVector));

        public void Handle(IPublishedEvent<GeoLocationQuestionAnswered> evnt)
            => this.webInterviewNotificationService.RefreshEntities(evnt.EventSourceId, new Identity(evnt.Payload.QuestionId, evnt.Payload.RosterVector));
            
        public void Handle(IPublishedEvent<SingleOptionLinkedQuestionAnswered> evnt)
            => this.webInterviewNotificationService.RefreshEntities(evnt.EventSourceId, new Identity(evnt.Payload.QuestionId, evnt.Payload.RosterVector));

        public void Handle(IPublishedEvent<MultipleOptionsLinkedQuestionAnswered> evnt)
            => this.webInterviewNotificationService.RefreshEntities(evnt.EventSourceId, new Identity(evnt.Payload.QuestionId, evnt.Payload.RosterVector));

        public void Handle(IPublishedEvent<LinkedOptionsChanged> evnt) 
            => this.webInterviewNotificationService.RefreshEntities(evnt.EventSourceId, evnt.Payload.ChangedLinkedQuestions.Select(x => x.QuestionId).ToArray());
    }
}