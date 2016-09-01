using System;
using System.Threading;
using System.Threading.Tasks;
using MvvmCross.Core.ViewModels;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public class AnsweringViewModel : MvxNotifyPropertyChanged, ICompositeEntity
    {
        private readonly ICommandService commandService;
        readonly IUserInterfaceStateService userInterfaceStateService;

        private int inProgressDepth = 0;

        protected AnsweringViewModel() { }

        public AnsweringViewModel(ICommandService commandService, IUserInterfaceStateService userInterfaceStateService)
        {
            this.commandService = commandService;
            this.userInterfaceStateService = userInterfaceStateService;
        }

        private bool inProgress;
        private readonly object cancellationLockObject = new object();
        private CancellationTokenSource currentCancellationTokenSource;

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

        public virtual async Task SendAnswerQuestionCommandAsync(AnswerQuestionCommand answerCommand)
        {
            await this.ExecuteCommand(answerCommand).ConfigureAwait(false);
        }

        public virtual async Task SendRemoveAnswerCommandAsync(RemoveAnswerCommand command)
        {
            await this.ExecuteCommand(command);
        }

        private async Task ExecuteCommand(ICommand answerCommand)
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            try
            {
                this.StartInProgressIndicator();

                await this.userInterfaceStateService.WaitWhileUserInterfaceIsRefreshingAsync().ConfigureAwait(false);

                lock (this.cancellationLockObject)
                {
                    this.TryCancelLastExecutedCommand();
                    this.RegisterCancellationTokenOfCurrentCommand(cancellationTokenSource);
                }

                await
                    this.commandService.ExecuteAsync(answerCommand, cancellationToken: cancellationTokenSource.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException) {}
            finally
            {
                lock (this.cancellationLockObject)
                {
                    this.ForgetCancellationToken(cancellationTokenSource);
                }

                this.FinishInProgressIndicator();
            }
        }

        private void TryCancelLastExecutedCommand()
        {
            if (this.currentCancellationTokenSource == null)
                return;

            this.currentCancellationTokenSource.Cancel();
            this.currentCancellationTokenSource = null;
        }

        private void RegisterCancellationTokenOfCurrentCommand(CancellationTokenSource cancellationTokenSource)
        {
            this.currentCancellationTokenSource = cancellationTokenSource;
        }

        private void ForgetCancellationToken(CancellationTokenSource cancellationTokenSource)
        {
            if (this.currentCancellationTokenSource == cancellationTokenSource)
            {
                this.currentCancellationTokenSource = null;
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