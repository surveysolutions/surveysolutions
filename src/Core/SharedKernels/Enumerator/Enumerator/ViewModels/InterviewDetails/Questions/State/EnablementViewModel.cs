using System;
using System.Linq;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.Enumerator.Repositories;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State
{
    public class EnablementViewModel : MvxNotifyPropertyChanged,
        ILiteEventHandler<GroupsEnabled>,
        ILiteEventHandler<GroupsDisabled>,
        ILiteEventHandler<QuestionsEnabled>,
        ILiteEventHandler<QuestionsDisabled>, IDisposable
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

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            if (interviewId == null) throw new ArgumentNullException("interviewId");
            if (entityIdentity == null) throw new ArgumentNullException("entityIdentity");

            this.interviewId = interviewId;
            this.entityIdentity = entityIdentity;

            this.UpdateSelfFromModel();
            this.eventRegistry.Subscribe(this, interviewId);
        }

        private bool enabled;
        public bool Enabled
        {
            get { return this.enabled; }
            private set { this.enabled = value; this.RaisePropertyChanged(); }
        }

        private void UpdateSelfFromModel()
        {
            var interview = this.interviewRepository.Get(this.interviewId);

            this.Enabled = interview.IsEnabled(this.entityIdentity);
        }

        public void Handle(GroupsEnabled @event)
        {
            if (@event.Groups.Contains(this.entityIdentity))
            {
                this.UpdateSelfFromModel();
            }
        }

        public void Handle(GroupsDisabled @event)
        {
            if (@event.Groups.Contains(this.entityIdentity))
            {
                this.UpdateSelfFromModel();
            }
        }

        public void Handle(QuestionsEnabled @event)
        {
            if (@event.Questions.Contains(this.entityIdentity))
            {
                this.UpdateSelfFromModel();
            }
        }

        public void Handle(QuestionsDisabled @event)
        {
            if (@event.Questions.Contains(this.entityIdentity))
            {
                this.UpdateSelfFromModel();
            }
        }

        public void Dispose()
        {
            this.eventRegistry.Unsubscribe(this, interviewId);
        }
    }
}