using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
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
            this.interviewRepository = interviewRepository ?? throw new ArgumentNullException(nameof(interviewRepository));
            this.eventRegistry = eventRegistry ?? throw new ArgumentNullException(nameof(eventRegistry));
            this.questionnaireRepository = questionnaireRepository ?? throw new ArgumentNullException(nameof(questionnaireRepository));
        }

        private string InterviewId { get; set; }
    
        private Identity entityIdentity;

        private bool initiated = false;
        
        public void Init(string interviewId, Identity entityIdentity)
        {
            this.InterviewId = interviewId ?? throw new ArgumentNullException(nameof(interviewId));
            this.entityIdentity = entityIdentity ?? throw new ArgumentNullException(nameof(entityIdentity));

            var interview = this.interviewRepository.Get(this.InterviewId);
            
            var questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);
            this.HideIfDisabled = questionnaire.ShouldBeHiddenIfDisabled(entityIdentity.Id);
            
            this.eventRegistry.Subscribe(this, interviewId);
            initiated = true;

            this.UpdateSelfFromModel();
        }

        public void AddPropertyChangedHandler(PropertyChangedEventHandler handler)
        {
            this.propertyChangedHandlers.Add(handler);
            this.PropertyChanged += handler;
        }

        public bool HideIfDisabled { get; private set; }

        private bool enabled;
        private List<PropertyChangedEventHandler> propertyChangedHandlers = new List<PropertyChangedEventHandler>();

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
            if(!initiated)
                throw new InvalidOperationException("Model was not initiated.");
            var interview = this.interviewRepository.Get(this.InterviewId);
            
            if(interview == null)
                throw new InvalidOperationException($"Interview was not found for interview [{this.InterviewId}]");
            
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
            if(propertyChangedHandlers != null)
                propertyChangedHandlers.ForEach(handler => this.PropertyChanged -= handler);
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
