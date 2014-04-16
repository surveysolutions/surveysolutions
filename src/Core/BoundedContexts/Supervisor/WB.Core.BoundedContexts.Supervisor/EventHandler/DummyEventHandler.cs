﻿using System;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.FunctionalDenormalization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Core.BoundedContexts.Supervisor.EventHandler
{
    public class DummyEventHandler :
        IEventHandler<AnswerCommented>,
        IEventHandler<FlagRemovedFromAnswer>,
        IEventHandler<FlagSetToAnswer>,
        IEventHandler<AnswerDeclaredValid>,
        IEventHandler<AnswerDeclaredInvalid>,
        IEventHandler<AnswersDeclaredValid>,
        IEventHandler<AnswersDeclaredInvalid>,
        IEventHandler<GroupPropagated>,
        IEventHandler<QuestionEnabled>,
        IEventHandler<QuestionDisabled>,
        IEventHandler<QuestionsEnabled>,
        IEventHandler<QuestionsDisabled>,
        IEventHandler<GroupDisabled>,
        IEventHandler<GroupEnabled>,
        IEventHandler<GroupsDisabled>,
        IEventHandler<GroupsEnabled>,
        IEventHandler<InterviewSynchronized>,
        IEventHandler<InterviewRestarted>,
        IEventHandler<InterviewCompleted>,
        IEventHandler<InterviewDeclaredValid>,
        IEventHandler<SingleOptionLinkedQuestionAnswered>,
        IEventHandler<MultipleOptionsLinkedQuestionAnswered>,
        IEventHandler<SynchronizationMetadataApplied>,
        IEventHandler<InterviewRejected>,
    IEventHandler
    {
        public void Handle(IPublishedEvent<AnswerDeclaredValid> evnt) {}

        public void Handle(IPublishedEvent<AnswerDeclaredInvalid> evnt) { }

        public void Handle(IPublishedEvent<AnswersDeclaredValid> evnt) { }

        public void Handle(IPublishedEvent<AnswersDeclaredInvalid> evnt) { }

        public void Handle(IPublishedEvent<GroupPropagated> evnt) { }

        public void Handle(IPublishedEvent<QuestionEnabled> evnt) {}

        public void Handle(IPublishedEvent<QuestionDisabled> evnt) {}

        public void Handle(IPublishedEvent<QuestionsEnabled> evnt) {}

        public void Handle(IPublishedEvent<QuestionsDisabled> evnt) {}

        public void Handle(IPublishedEvent<InterviewCompleted> evnt) {}

        public void Handle(IPublishedEvent<AnswerCommented> evnt) {}

        public void Handle(IPublishedEvent<FlagRemovedFromAnswer> evnt) {}

        public void Handle(IPublishedEvent<FlagSetToAnswer> evnt) {}

        public void Handle(IPublishedEvent<GroupDisabled> evnt) { }

        public void Handle(IPublishedEvent<GroupEnabled> evnt) { }

        public void Handle(IPublishedEvent<GroupsDisabled> evnt) { }

        public void Handle(IPublishedEvent<GroupsEnabled> evnt) { }

        public void Handle(IPublishedEvent<InterviewSynchronized> evnt) {}

        public void Handle(IPublishedEvent<InterviewDeclaredValid> evnt) {}

        public void Handle(IPublishedEvent<SingleOptionLinkedQuestionAnswered> evnt) {}

        public void Handle(IPublishedEvent<MultipleOptionsLinkedQuestionAnswered> evnt) {}

        public void Handle(IPublishedEvent<SynchronizationMetadataApplied> evnt) {}

        public string Name
        {
            get { return "Dummy event handler"; }
        }

        public Type[] UsesViews
        {
            get { return new Type[0]; }
        }

        public Type[] BuildsViews
        {
            get { return new Type[0]; }
        }

        public void Handle(IPublishedEvent<InterviewRestarted> evnt){}
        public void Handle(IPublishedEvent<InterviewRejected> evnt)
        {
        }
    }
}
