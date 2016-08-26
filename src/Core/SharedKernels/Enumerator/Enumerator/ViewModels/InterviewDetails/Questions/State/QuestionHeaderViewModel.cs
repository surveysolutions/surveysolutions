using System;
using System.Windows.Input;
using MvvmCross.Core.ViewModels;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State
{
    public class QuestionHeaderViewModel : MvxNotifyPropertyChanged,
        IDisposable
    {
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IQuestionnaireStorage questionnaireRepository;

        public event EventHandler ShowComments;

        public DynamicTextViewModel Title { get; }

        public QuestionHeaderViewModel(
            IQuestionnaireStorage questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            DynamicTextViewModel dynamicTextViewModel)
        {
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;

            this.Title = dynamicTextViewModel;
        }

        public string Instruction { get; private set; }

        private bool isInstructionsHidden;
        public bool IsInstructionsHidden
        {
            get { return this.isInstructionsHidden; }
            set { this.RaiseAndSetIfChanged(ref this.isInstructionsHidden, value); }
        }

        public void Init(string interviewId, Identity questionIdentity)
        {
            if (interviewId == null) throw new ArgumentNullException(nameof(interviewId));
            if (questionIdentity == null) throw new ArgumentNullException(nameof(questionIdentity));

            var interview = this.interviewRepository.Get(interviewId);
            IQuestionnaire questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);
            
            this.IsInstructionsHidden = questionnaire.GetHideInstructions(questionIdentity.Id);
            this.Instruction = questionnaire.GetQuestionInstruction(questionIdentity.Id);

            this.Title.Init(interviewId, questionIdentity, questionnaire.GetQuestionTitle(questionIdentity.Id));
        }

        public ICommand ShowInstructions => new MvxCommand(() => this.IsInstructionsHidden = false);
        public ICommand ShowCommentEditorCommand => new MvxCommand(() => ShowComments?.Invoke(this, EventArgs.Empty));

        public void Dispose()
        {
            this.Title.Dispose();
        }
    }
}