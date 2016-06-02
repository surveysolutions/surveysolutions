using System;
using System.Linq;
using System.Windows.Input;
using MvvmCross.Core.ViewModels;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State
{
    public class QuestionHeaderViewModel : MvxNotifyPropertyChanged,
        ILiteEventHandler<SubstitutionTitlesChanged>,
        ILiteEventHandler<VariablesChanged>, 
        IDisposable
    {
        public string Instruction { get; set; }
        private string title;
        public string Title
        {
            get { return this.title; }
            set { this.title = value; this.RaisePropertyChanged(); }
        }

        public bool IsInstructionsHidden
        {
            get { return this.isInstructionsHidden; }
            set
            {
                this.isInstructionsHidden = value;
                this.RaisePropertyChanged();
            }
        }

        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IPlainQuestionnaireRepository questionnaireRepository;
        private readonly ILiteEventRegistry registry;
        private readonly SubstitutionViewModel substitutionViewModel;

        private Identity questionIdentity;
        private string interviewId;
        private bool isInstructionsHidden;

        public void Init(string interviewId, Identity questionIdentity)
        {
            if (interviewId == null) throw new ArgumentNullException(nameof(interviewId));
            if (questionIdentity == null) throw new ArgumentNullException(nameof(questionIdentity));

            var interview = this.interviewRepository.Get(interviewId);
            IQuestionnaire questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity);
            
            this.IsInstructionsHidden = questionnaire.GetHideInstructions(questionIdentity.Id);
            this.Instruction = questionnaire.GetQuestionInstruction(questionIdentity.Id);
            this.questionIdentity = questionIdentity;
            this.interviewId = interviewId;

            this.substitutionViewModel.Init(interviewId, questionIdentity, questionnaire.GetQuestionTitle(questionIdentity.Id));

            this.Title = this.substitutionViewModel.ReplaceSubstitutions();

            this.registry.Subscribe(this, interviewId);
        }

        protected QuestionHeaderViewModel() { }

        public QuestionHeaderViewModel(
            IPlainQuestionnaireRepository questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            ILiteEventRegistry registry,
            SubstitutionViewModel substitutionViewModel)
        {
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;
            this.registry = registry;
            this.substitutionViewModel = substitutionViewModel;
        }

        public ICommand ShowInstructions
        {
            get
            {
                return new MvxCommand(() => IsInstructionsHidden = false);
            }
        }

        public void Handle(VariablesChanged @event)
        {
            if (!this.substitutionViewModel.HasVariablesInText(
                @event.ChangedVariables.Select(variable => variable.Identity))) return;

            this.Title = this.substitutionViewModel.ReplaceSubstitutions();
        }

        public void Handle(SubstitutionTitlesChanged @event)
        {
            bool thisQuestionChanged = @event.Questions.Any(x => this.questionIdentity.Equals(x));

            if (thisQuestionChanged)
            {
                this.Title = this.substitutionViewModel.ReplaceSubstitutions();
            }
        }

        public void Dispose()
        {
            this.registry.Unsubscribe(this);
        }
    }
}