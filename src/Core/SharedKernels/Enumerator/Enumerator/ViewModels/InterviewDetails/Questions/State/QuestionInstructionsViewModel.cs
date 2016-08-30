using System;
using System.Windows.Input;
using MvvmCross.Core.ViewModels;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Repositories;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State
{
    public class QuestionInstructionViewModel : MvxNotifyPropertyChanged
    {
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IQuestionnaireStorage questionnaireRepository;

        public QuestionInstructionViewModel() {}

        public QuestionInstructionViewModel(
            IQuestionnaireStorage questionnaireRepository,
            IStatefulInterviewRepository interviewRepository)
        {
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;
        }

        public string Instruction { get; private set; }

        private bool isInstructionsHidden;
        public bool IsInstructionsHidden
        {
            get { return this.isInstructionsHidden; }
            set { this.RaiseAndSetIfChanged(ref this.isInstructionsHidden, value); }
        }

        public bool HasInstructions => !string.IsNullOrWhiteSpace(Instruction);

        public virtual void Init(string interviewId, Identity questionIdentity)
        {
            if (interviewId == null) throw new ArgumentNullException(nameof(interviewId));
            if (questionIdentity == null) throw new ArgumentNullException(nameof(questionIdentity));

            var interview = this.interviewRepository.Get(interviewId);
            IQuestionnaire questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);

            this.IsInstructionsHidden = questionnaire.GetHideInstructions(questionIdentity.Id);
            this.Instruction = questionnaire.GetQuestionInstruction(questionIdentity.Id);
        }

        public ICommand ShowInstructions => new MvxCommand(() => this.IsInstructionsHidden = false);
    }
}