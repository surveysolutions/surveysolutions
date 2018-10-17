using System;
using System.Windows.Input;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State
{
    public class QuestionInstructionViewModel : MvxNotifyPropertyChanged,
        ICompositeEntity, IDisposable
    {
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IQuestionnaireStorage questionnaireRepository;

        public QuestionInstructionViewModel() {}

        public QuestionInstructionViewModel(
            IQuestionnaireStorage questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            DynamicTextViewModel instruction)
        {
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;

            this.Instruction = instruction;
        }

        public DynamicTextViewModel Instruction { get; private set; }

        private bool isInstructionsHidden;
        public bool IsInstructionsHidden
        {
            get { return this.isInstructionsHidden; }
            set { this.RaiseAndSetIfChanged(ref this.isInstructionsHidden, value); }
        }

        public bool HasInstructions => !string.IsNullOrWhiteSpace(this.Instruction.HtmlText);

        public virtual void Init(string interviewId, Identity questionIdentity)
        {
            if (interviewId == null) throw new ArgumentNullException(nameof(interviewId));
            if (questionIdentity == null) throw new ArgumentNullException(nameof(questionIdentity));

            var interview = this.interviewRepository.Get(interviewId);
            IQuestionnaire questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);

            this.IsInstructionsHidden = questionnaire.GetHideInstructions(questionIdentity.Id);
            
            this.Identity = questionIdentity;

            this.Instruction.InitAsInstructions(interviewId, questionIdentity);
        }

        public Identity Identity { get; private set; }

        public ICommand ShowInstructions => new MvxCommand(() => this.IsInstructionsHidden = false);

        public void Dispose()
        {
            Instruction?.Dispose();
        }
    }
}
