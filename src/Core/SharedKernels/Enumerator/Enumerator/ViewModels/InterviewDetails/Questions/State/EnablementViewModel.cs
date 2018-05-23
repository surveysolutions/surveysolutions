using System;
using System.Linq;
using MvvmCross.ViewModels;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State
{
    public class EnablementViewModel : MvxNotifyPropertyChanged,
        ILiteEventHandler<GroupsEnabled>,
        ILiteEventHandler<GroupsDisabled>,
        ILiteEventHandler<QuestionsEnabled>,
        ILiteEventHandler<QuestionsDisabled>, 
        ILiteEventHandler<StaticTextsDisabled>,
        ILiteEventHandler<StaticTextsEnabled>,
        IDisposable
    {
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly ILiteEventRegistry eventRegistry;
        private readonly IQuestionnaireStorage questionnaireRepository;

        public event EventHandler EntityEnabled;
        public event EventHandler EntityDisabled;

        protected EnablementViewModel() { }

        public EnablementViewModel(IStatefulInterviewRepository interviewRepository, ILiteEventRegistry eventRegistry, 
            IQuestionnaireStorage questionnaireRepository)
        {
            if (interviewRepository == null) throw new ArgumentNullException(nameof(interviewRepository));
            if (eventRegistry == null) throw new ArgumentNullException(nameof(eventRegistry));
            if (questionnaireRepository == null) throw new ArgumentNullException(nameof(questionnaireRepository));

            this.interviewRepository = interviewRepository;
            this.eventRegistry = eventRegistry;
            this.questionnaireRepository = questionnaireRepository;
        }

        private string interviewId;
        private Identity entityIdentity;

        public void Init(string interviewId, Identity entityIdentity)
        {
            if (interviewId == null) throw new ArgumentNullException(nameof(interviewId));
            if (entityIdentity == null) throw new ArgumentNullException(nameof(entityIdentity));

            this.interviewId = interviewId;
            this.entityIdentity = entityIdentity;

            var interview = this.interviewRepository.Get(this.interviewId);
            this.HideIfDisabled = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language)
                    .ShouldBeHiddenIfDisabled(entityIdentity.Id);

            this.UpdateSelfFromModel();
            this.eventRegistry.Subscribe(this, interviewId);
        }

        public bool HideIfDisabled { get; private set; }

        private bool enabled;
        public bool Enabled
        {
            get { return this.enabled; }
            private set
            {
                this.RaiseAndSetIfChanged(ref this.enabled, value);
            }
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
                this.OnEnabled();
            }
        }

        public void Handle(GroupsDisabled @event)
        {
            if (@event.Groups.Contains(this.entityIdentity))
            {
                this.UpdateSelfFromModel();
                this.OnOnDisabled();
            }
        }

        public void Handle(QuestionsEnabled @event)
        {
            if (@event.Questions.Contains(this.entityIdentity))
            {
                this.UpdateSelfFromModel();
                this.OnEnabled();
            }
        }

        public void Handle(QuestionsDisabled @event)
        {
            if (@event.Questions.Contains(this.entityIdentity))
            {
                this.UpdateSelfFromModel();
                this.OnOnDisabled();
            }
        }

        public void Dispose()
        {
            this.eventRegistry.Unsubscribe(this);
        }

        public void Handle(StaticTextsDisabled @event)
        {
            if (@event.StaticTexts.Contains(this.entityIdentity))
            {
                this.UpdateSelfFromModel();
                this.OnOnDisabled();
            }
        }

        public void Handle(StaticTextsEnabled @event)
        {
            if (@event.StaticTexts.Contains(this.entityIdentity))
            {
                this.UpdateSelfFromModel();
                this.OnEnabled();
            }
        }

        protected virtual void OnEnabled()
        {
            var handler = this.EntityEnabled;
            handler?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnOnDisabled()
        {
            this.EntityDisabled?.Invoke(this, EventArgs.Empty);
        }
    }
}