using System.Threading;
using System.Threading.Tasks;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    public class AnsweringViewModel : MvxNotifyPropertyChanged
    {
        private readonly ICommandService commandService;
        private int inProgressDepth = 0;

        public AnsweringViewModel(ICommandService commandService)
        {
            this.commandService = commandService;
        }

        private bool inProgress;
        public bool InProgress
        {
            get { return this.inProgress; }

            private set
            {
                if (value == this.inProgress) return;

                this.inProgress = value;
                this.RaisePropertyChanged();
            }
        }

        public async Task SendAnswerQuestionCommand(AnswerQuestionCommand answerCommand)
        {
            try
            {
                this.StartInProgressIndicator();

                await this.commandService.ExecuteAsync(answerCommand);
            }
            finally
            {
                this.FinishInProgressIndicator();
            }
        }

        private void StartInProgressIndicator()
        {
            Interlocked.Increment(ref this.inProgressDepth);
            this.UpdateInProgressFlag();
        }

        private void FinishInProgressIndicator()
        {
            Interlocked.Decrement(ref this.inProgressDepth);
            this.UpdateInProgressFlag();
        }

        private void UpdateInProgressFlag()
        {
            this.InProgress = this.inProgressDepth > 0;
        }
    }
}