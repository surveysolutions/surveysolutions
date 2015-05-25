using System.Threading.Tasks;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    public class AnsweringViewModel : MvxNotifyPropertyChanged
    {
        private readonly ICommandService commandService;

        public AnsweringViewModel(ICommandService commandService)
        {
            this.commandService = commandService;
        }

        private bool inProgress;
        public bool InProgress
        {
            get { return inProgress; }
            private set { inProgress = value; RaisePropertyChanged(); }
        }

        public async Task SendAnswerQuestionCommand(AnswerQuestionCommand answerCommand)
        {
            try
            {
                this.InProgress = true;

                await this.commandService.ExecuteAsync(answerCommand);
            }
            finally
            {
                this.InProgress = false;
            }
        }
    }
}