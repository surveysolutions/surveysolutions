using System;
using System.Linq;
using MvvmCross.ViewModels;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State
{
    public class EnablementViewModel : MvxNotifyPropertyChanged,
        IViewModelEventHandler<GroupsEnabled>,
        IViewModelEventHandler<GroupsDisabled>,
        IViewModelEventHandler<QuestionsEnabled>,
        IViewModelEventHandler<QuestionsDisabled>, 
        IViewModelEventHandler<StaticTextsDisabled>,
        IViewModelEventHandler<StaticTextsEnabled>,
        IDisposable
    {
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IViewModelEventRegistry eventRegistry;
        private readonly IQuestionnaireStorage questionnaireRepository;

        public event EventHandler EntityEnabled;
        public event EventHandler EntityDisabled;

        protected EnablementViewModel() { }

        public EnablementViewModel(IStatefulInterviewRepository interviewRepository, IViewModelEventRegistry eventRegistry, 
            IQuestionnaireStorage questionnaireRepository)
        {
            if (interviewRepository == null) throw new ArgumentNullException(nameof(interviewRepository));
            if (eventRegistry == null) throw new ArgumentNullException(nameof(eventRegistry));
            if (questionnaireRepository == null) throw new ArgumentNullException(nameof(questionnaireRepository));

            this.interviewRepository = interviewRepository;
            this.eventRegistry = eventRegistry;
            this.questionnaireRepository = questionnaireRepository;
        }

        public string InterviewId { get; private set; }
    
        private Identity entityIdentity;

        public void Init(string interviewId, Identity entityIdentity)
        {
            if (interviewId == null) throw new ArgumentNullException(nameof(interviewId));
            if (entityIdentity == null) throw new ArgumentNullException(nameof(entityIdentity));

            this.InterviewId = interviewId;
            this.entityIdentity = entityIdentity;

            var interview = this.interviewRepository.Get(this.InterviewId);
            var questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);
            this.HideIfDisabled = questionnaire.ShouldBeHiddenIfDisabled(entityIdentity.Id);

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
            this.Enabled = GetEnablementFromInterview();
        }

        public bool GetEnablementFromInterview()
        {
            var interview = this.interviewRepository.Get(this.InterviewId);
            return interview.IsEnabled(this.entityIdentity);
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
