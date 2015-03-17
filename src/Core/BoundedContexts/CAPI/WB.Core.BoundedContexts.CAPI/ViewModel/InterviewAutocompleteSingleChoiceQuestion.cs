using Cirrious.MvvmCross.ViewModels;

namespace WB.Core.BoundedContexts.Capi.ViewModel
{
    public class InterviewAutocompleteSingleChoiceQuestion : InterviewSingleChoiceQuestion
    {
        private InterviewStaticOption answer;
        public InterviewStaticOption Answer
        {
            get { return answer; }
            set
            {
                answer = value;
                RaisePropertyChanged(() => Answer);
            }
        }

        private IMvxCommand selectAnswerCommand;
        public IMvxCommand SelectAnswerCommand
        {
            get { return selectAnswerCommand ?? (selectAnswerCommand = new MvxCommand<InterviewStaticOption>(this.SelectAnswer)); }
        }

        private void SelectAnswer(InterviewStaticOption selectedAnswer)
        {
            this.Answer = selectedAnswer;
        }
    }
}