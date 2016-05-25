using Main.Core.Events.Questionnaire;
using Main.Core.Events.User;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.EventBus;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Events.User;
using WB.Core.Synchronization.Events.Sync;

namespace WB.Core.BoundedContexts.Headquarters.EventHandler
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
        IEventHandler<InterviewApproved>,
        IEventHandler<InterviewRejected>,
        IEventHandler<InterviewDeclaredInvalid>,
        IEventHandler<QuestionnaireAssemblyImported>,
        IEventHandler<TabletRegistered>,
        IEventHandler<SubstitutionTitlesChanged>,
        IEventHandler<TemplateImported>, 
        IEventHandler<QuestionnaireDisabled>, 
        IEventHandler<QuestionnaireDeleted>, 
        IEventHandler<PlainQuestionnaireRegistered>, 
        IEventHandler<NewUserCreated>,
        IEventHandler<UserChanged>,
        IEventHandler<UserLocked>,
        IEventHandler<UserUnlocked>,
        IEventHandler<UserLockedBySupervisor>,
        IEventHandler<UserUnlockedBySupervisor>,
        IEventHandler<UserLinkedToDevice>,
        IEventHandler<UserArchived>,
        IEventHandler<UserUnarchived>,
        IEventHandler<StaticTextsDeclaredInvalid>,
        IEventHandler<StaticTextsDeclaredValid>
    {
        public override string Name => "Dummy event handler";

        public override object[] Writers => new object[0];

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
        public void Handle(IPublishedEvent<InterviewApproved> evnt) { }
        public void Handle(IPublishedEvent<InterviewRestarted> evnt) { }
        public void Handle(IPublishedEvent<InterviewRejected> evnt) { }
        public void Handle(IPublishedEvent<InterviewDeclaredInvalid> evnt) { }
        public void Handle(IPublishedEvent<QuestionnaireAssemblyImported> evnt){}
        public void Handle(IPublishedEvent<TabletRegistered> evnt){}
        public void Handle(IPublishedEvent<SubstitutionTitlesChanged> evnt){}
        public void Handle(IPublishedEvent<PlainQuestionnaireRegistered> evnt) { }
        public void Handle(IPublishedEvent<TemplateImported> evnt) { }
        public void Handle(IPublishedEvent<QuestionnaireDisabled> evnt) { }
        public void Handle(IPublishedEvent<QuestionnaireDeleted> evnt) { }
        public void Handle(IPublishedEvent<NewUserCreated> evnt) { }
        public void Handle(IPublishedEvent<UserChanged> evnt) { }
        public void Handle(IPublishedEvent<UserLocked> evnt) { }
        public void Handle(IPublishedEvent<UserUnlocked> evnt) { }
        public void Handle(IPublishedEvent<UserLockedBySupervisor> evnt) { }
        public void Handle(IPublishedEvent<UserUnlockedBySupervisor> evnt) { }
        public void Handle(IPublishedEvent<UserLinkedToDevice> evnt) { }
        public void Handle(IPublishedEvent<UserArchived> evnt) { }
        public void Handle(IPublishedEvent<UserUnarchived> evnt) { }
        public void Handle(IPublishedEvent<StaticTextsDeclaredInvalid> evnt) { }
        public void Handle(IPublishedEvent<StaticTextsDeclaredValid> evnt) { }
    }
}
