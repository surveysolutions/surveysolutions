using System;
using System.Linq;
using MvvmCross.ViewModels;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;


namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public class VariableViewModel : 
        MvxNotifyPropertyChanged,
        IInterviewEntityViewModel,
        IViewModelEventHandler<VariablesChanged>,
        IDisposable
    {
        public Identity Identity { get; private set; }

        private readonly IQuestionnaireStorage questionnaireRepository;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IViewModelEventRegistry registry;

        private string variableName;
        private string variableLabel;
        private object variableValue;

        public VariableViewModel(IQuestionnaireStorage questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            IViewModelEventRegistry registry)
        {
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;
            this.registry = registry;
        }

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            if (interviewId == null) throw new ArgumentNullException(nameof(interviewId));
            if (entityIdentity == null) throw new ArgumentNullException(nameof(entityIdentity));

            this.NavigationState = navigationState;
            var interview = this.interviewRepository.GetOrThrow(interviewId);
            var questionnaire = this.questionnaireRepository.GetQuestionnaireOrThrow(interview.QuestionnaireIdentity, interview.Language);

            this.Identity = entityIdentity;
            this.variableLabel  = questionnaire.GetVariableLabel(entityIdentity.Id);
            this.variableName   = questionnaire.GetVariableName(entityIdentity.Id);
            this.variableValue  = interview.GetVariableValueByOrDeeperRosterLevel(entityIdentity.Id, entityIdentity.RosterVector);
            this.RaisePropertyChanged(nameof(Text));

            this.registry.Subscribe(this, interviewId);
        }

        public string Text => $"{this.variableName} : {this.variableValue?.ToString() ?? UIResources.VariableEmptyValue}";

        public string Label => this.variableLabel;
        public bool HasLabel => !string.IsNullOrEmpty(this.variableLabel);

        private NavigationState NavigationState { get; set; }
        
        public bool IsCoverVariable => this.NavigationState.CurrentScreenType == ScreenType.Cover;
        
        public void Handle(VariablesChanged @event)
        {
            var changedVariable = @event.ChangedVariables.FirstOrDefault(v => v.Identity == this.Identity);
            bool isVariableChanged = changedVariable != null;
            if (isVariableChanged)
            {
                this.variableValue = changedVariable.NewValue;
                this.RaisePropertyChanged(nameof(Text));
            }
        }

        public void Dispose()
        {
            this.registry.Unsubscribe(this);
        }
    }
}
