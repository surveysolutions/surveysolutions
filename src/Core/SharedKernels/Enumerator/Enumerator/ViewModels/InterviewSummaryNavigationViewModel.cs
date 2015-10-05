using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;

namespace WB.Core.SharedKernels.Enumerator.ViewModels
{
    public class InterviewSummaryNavigationViewModel : MvxNotifyPropertyChanged, IDisposable
    {
        private NavigationState navigationState;

        private readonly AnswerNotifier answerNotifier;

        private readonly IStatefulInterviewRepository interviewRepository;

        public InterviewStateViewModel InterviewState { get; private set; }

        public InterviewSummaryNavigationViewModel(
            InterviewStateViewModel interviewState, 
            AnswerNotifier answerNotifier, 
            IStatefulInterviewRepository interviewRepository)
        {
            this.InterviewState = interviewState;
            this.answerNotifier = answerNotifier;
            this.interviewRepository = interviewRepository;
        }

        public virtual void Init(string interviewId, Identity groupIdentity, NavigationState navigationState)
        {
            this.navigationState = navigationState;

            var groupWithAnswersToMonitor = this.navigationState.CurrentGroup;

            var interview = this.interviewRepository.Get(interviewId);
            IEnumerable<Identity> questionsToListen = interview.GetChildQuestions(groupWithAnswersToMonitor);

            this.InterviewState.Init(interviewId, null);

            this.answerNotifier.Init(interviewId, questionsToListen.ToArray());
            this.answerNotifier.QuestionAnswered += this.QuestionAnswered;
        }

        private void QuestionAnswered(object sender, EventArgs e)
        {
            this.InterviewState.UpdateFromGroupModel();
            this.RaisePropertyChanged(() => this.InterviewState);
        }

        public IMvxCommand NavigateCommand
        {
            get { return new MvxCommand(async () => await this.NavigateAsync()); }
        }

        private async Task NavigateAsync()
        {
            await this.navigationState.NavigateToAsync(NavigationIdentity.CreateForCompleteScreen());
        }

        public void Dispose()
        {
            this.answerNotifier.QuestionAnswered -= this.QuestionAnswered;
        }
    }
}