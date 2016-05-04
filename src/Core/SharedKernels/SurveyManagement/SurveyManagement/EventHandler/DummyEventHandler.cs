﻿using System;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.EventBus;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Commands;
using WB.Core.Synchronization.Documents;
using WB.Core.Synchronization.Events.Sync;

namespace WB.Core.SharedKernels.SurveyManagement.EventHandler
{
    public class DummyEventHandler :BaseDenormalizer,
        IEventHandler<AnswerCommented>,
        IEventHandler<FlagRemovedFromAnswer>,
        IEventHandler<FlagSetToAnswer>,
        IEventHandler<AnswersDeclaredValid>,
        IEventHandler<AnswersDeclaredInvalid>,
        IEventHandler<GroupPropagated>,
        IEventHandler<QuestionsEnabled>,
        IEventHandler<QuestionsDisabled>,
        IEventHandler<GroupsDisabled>,
        IEventHandler<GroupsEnabled>,
        IEventHandler<InterviewSynchronized>,
        IEventHandler<InterviewSentToHeadquarters>,
        IEventHandler<InterviewRestarted>,
        IEventHandler<InterviewCompleted>,
        IEventHandler<InterviewDeclaredValid>,
        IEventHandler<SingleOptionLinkedQuestionAnswered>,
        IEventHandler<MultipleOptionsLinkedQuestionAnswered>,
        //IEventHandler<SynchronizationMetadataApplied>,
        IEventHandler<InterviewApproved>,
        IEventHandler<InterviewRejected>,
        IEventHandler<InterviewDeclaredInvalid>,
        IEventHandler<QuestionnaireAssemblyImported>,
        IEventHandler<TabletRegistered>,
        IEventHandler<SubstitutionTitlesChanged>,
        IEventHandler<TemplateImported>, 
        IEventHandler<QuestionnaireDisabled>, 
        IEventHandler<QuestionnaireDeleted>, 
        IEventHandler<PlainQuestionnaireRegistered>
    {
        public void Handle(IPublishedEvent<AnswersDeclaredValid> evnt) { }

        public void Handle(IPublishedEvent<AnswersDeclaredInvalid> evnt) { }

        public void Handle(IPublishedEvent<GroupPropagated> evnt) { }

        public void Handle(IPublishedEvent<QuestionsEnabled> evnt) {}

        public void Handle(IPublishedEvent<QuestionsDisabled> evnt) {}

        public void Handle(IPublishedEvent<InterviewCompleted> evnt) {}

        public void Handle(IPublishedEvent<AnswerCommented> evnt) {}

        public void Handle(IPublishedEvent<FlagRemovedFromAnswer> evnt) {}

        public void Handle(IPublishedEvent<FlagSetToAnswer> evnt) {}

        public void Handle(IPublishedEvent<GroupsDisabled> evnt) { }

        public void Handle(IPublishedEvent<GroupsEnabled> evnt) { }

        public void Handle(IPublishedEvent<InterviewSynchronized> evnt) {}

        public void Handle(IPublishedEvent<InterviewSentToHeadquarters> evnt) {}

        public void Handle(IPublishedEvent<InterviewDeclaredValid> evnt) {}

        public void Handle(IPublishedEvent<SingleOptionLinkedQuestionAnswered> evnt) {}

        public void Handle(IPublishedEvent<MultipleOptionsLinkedQuestionAnswered> evnt) {}

        //public void Handle(IPublishedEvent<SynchronizationMetadataApplied> evnt) {}

        public void Handle(IPublishedEvent<InterviewApproved> evnt) { }

        public override string Name
        {
            get { return "Dummy event handler"; }
        }

        public override object[] Writers
        {
            get { return new object[0]; }
        }

        public void Handle(IPublishedEvent<InterviewRestarted> evnt){}
        public void Handle(IPublishedEvent<InterviewRejected> evnt)
        {
        }

        public void Handle(IPublishedEvent<InterviewDeclaredInvalid> evnt)
        {
        }

        public void Handle(IPublishedEvent<QuestionnaireAssemblyImported> evnt){}
        public void Handle(IPublishedEvent<TabletRegistered> evnt){}
        public void Handle(IPublishedEvent<SubstitutionTitlesChanged> evnt){}

        public void Handle(IPublishedEvent<PlainQuestionnaireRegistered> evnt)
        {
        }

        public void Handle(IPublishedEvent<TemplateImported> evnt)
        {
        }

        public void Handle(IPublishedEvent<QuestionnaireDisabled> evnt)
        {
        }

        public void Handle(IPublishedEvent<QuestionnaireDeleted> evnt)
        {
        }
    }
}
