using System;
using System.Linq;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionStateViewModels
{
    public class EnablementViewModel : MvxNotifyPropertyChanged,
        IInterviewEntityViewModel,
        ILiteEventHandler<GroupsEnabled>,
        ILiteEventHandler<GroupsDisabled>,
        ILiteEventHandler<QuestionsEnabled>,
        ILiteEventHandler<QuestionsDisabled>
    {
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly ILiteEventRegistry eventRegistry;

        protected EnablementViewModel() { }

        public EnablementViewModel(IStatefulInterviewRepository interviewRepository, ILiteEventRegistry eventRegistry)
        {
            if (interviewRepository == null) throw new ArgumentNullException("interviewRepository");
            if (eventRegistry == null) throw new ArgumentNullException("eventRegistry");

            this.interviewRepository = interviewRepository;
            this.eventRegistry = eventRegistry;
        }

        private string interviewId;
        private Identity entityIdentity;
        private SharedKernels.DataCollection.Events.Interview.Dtos.Identity identityForEvents;

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            if (interviewId == null) throw new ArgumentNullException("interviewId");
            if (entityIdentity == null) throw new ArgumentNullException("entityIdentity");

            this.interviewId = interviewId;
            this.entityIdentity = entityIdentity;
            this.identityForEvents = entityIdentity.ToIdentityForEvents();

            this.UpdateSelfFromModel();

            this.eventRegistry.Subscribe(this);
        }

        private bool enabled;
        public bool Enabled
        {
            get { return enabled; }
            private set { enabled = value; this.RaisePropertyChanged(); }
        }

        private void UpdateSelfFromModel()
        {
            var interview = this.interviewRepository.Get(this.interviewId);

            this.Enabled = interview.IsEnabled(this.entityIdentity);
        }

        public void Handle(GroupsEnabled @event)
        {
            if (@event.Groups.Contains(this.identityForEvents))
            {
                this.UpdateSelfFromModel();
            }
        }

        public void Handle(GroupsDisabled @event)
        {
            if (@event.Groups.Contains(this.identityForEvents))
            {
                this.UpdateSelfFromModel();
            }
        }

        public void Handle(QuestionsEnabled @event)
        {
            if (@event.Questions.Contains(this.identityForEvents))
            {
                this.UpdateSelfFromModel();
            }
        }

        public void Handle(QuestionsDisabled @event)
        {
            if (@event.Questions.Contains(this.identityForEvents))
            {
                this.UpdateSelfFromModel();
            }
        }
    }
}