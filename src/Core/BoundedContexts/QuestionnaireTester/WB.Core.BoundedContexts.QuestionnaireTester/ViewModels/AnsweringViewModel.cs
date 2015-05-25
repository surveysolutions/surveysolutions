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
        private int inProgressDeep = 0;

        public AnsweringViewModel(ICommandService commandService)
        {
            this.commandService = commandService;
        }

        private bool inProgress;
        public bool InProgress
        {
            get { return inProgress; }
            private set
            {
                if (value != InProgress)
                {
                    inProgress = value;
                    RaisePropertyChanged();
                }
            }
        }

        public async Task SendAnswerQuestionCommand(AnswerQuestionCommand answerCommand)
        {
            try
            {
                StartInProgressIndicator();

                await this.commandService.ExecuteAsync(answerCommand);
            }
            finally
            {
                FinishInProgressIndicator();
            }
        }

        private void StartInProgressIndicator()
        {
            Interlocked.Increment(ref inProgressDeep);
            this.InProgress = true;
        }

        private void FinishInProgressIndicator()
        {
            Interlocked.Decrement(ref inProgressDeep);

            if (inProgressDeep == 0)
            {
                this.InProgress = false;
            }
        }
    }
}