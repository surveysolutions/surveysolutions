using System.Threading.Tasks;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    public class SendAnswerViewModel : MvxNotifyPropertyChanged
    {
        private readonly ICommandService commandService;

        public SendAnswerViewModel(ICommandService commandService)
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
            await Task.Run(() => SendAnswerTextQuestionCommandImpl(answerCommand));
        }

        private void SendAnswerTextQuestionCommandImpl(AnswerQuestionCommand answerCommand)
        {
            try
            {
                InProgress = true;

                commandService.Execute(answerCommand);
            }
            finally
            {
                InProgress = false;
            }
        }
    }
}