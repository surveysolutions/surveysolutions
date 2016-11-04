using System;
using System.Linq;
using MvvmCross.Core.ViewModels;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Repositories;


namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public class VariableViewModel : 
        MvxNotifyPropertyChanged,
        IInterviewEntityViewModel,
        ILiteEventHandler<VariablesChanged>,
        IDisposable
    {
        public Identity Identity { get; private set; }

        private readonly IQuestionnaireStorage questionnaireRepository;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly ILiteEventRegistry registry;
        private readonly IEnumeratorSettings settings;

        private string variableName;
        private string variableDescription;
        private object variableValue;

        public VariableViewModel(IQuestionnaireStorage questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            ILiteEventRegistry registry,
            IEnumeratorSettings settings)
        {
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;
            this.registry = registry;
            this.settings = settings;
        }

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            if (interviewId == null) throw new ArgumentNullException(nameof(interviewId));
            if (entityIdentity == null) throw new ArgumentNullException(nameof(entityIdentity));

            if (!this.IsVisible)
                return;

            var interview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);

            this.Identity = entityIdentity;
            variableDescription = questionnaire.GetVariableDescription(entityIdentity.Id);
            variableName        = questionnaire.GetVariableName(entityIdentity.Id);
            variableValue       = interview.GetVariableValueByOrDeeperRosterLevel(entityIdentity.Id, entityIdentity.RosterVector);
            this.RaisePropertyChanged(nameof(Text));

            this.registry.Subscribe(this, interviewId);
        }

        public string Text => this.variableName + ": " + (this.variableValue.ToString() ?? UIResources.VariableEmptyValue);
        public string Description => this.variableDescription;
        public bool IsShowDescription => !string.IsNullOrEmpty(this.variableDescription);

        public bool IsVisible => settings.ShowVariables;

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