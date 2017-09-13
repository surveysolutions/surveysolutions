using System;
using System.Threading;
using System.Threading.Tasks;
using MvvmCross.Core.ViewModels;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Utils;

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
            get => this.inProgress;
            private set => this.RaiseAndSetIfChanged(ref this.inProgress, value);
        }

        /// <exception cref="InterviewException">All consumers of this method should gracefully handle InterviewException's</exception>
        /// <exception cref="Exception">All other exceptions will be wrapped into Exception with readable message</exception>
        public virtual async Task SendAnswerQuestionCommandAsync(AnswerQuestionCommand answerCommand)
        {
            try
            {
                await this.ExecuteCommandAsync(answerCommand);
            }
            catch (InterviewException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to answer question {answerCommand.Question}. " +
                                    $"CommandType: {answerCommand.GetType()}. " +
                                    $"Interview Id {answerCommand.InterviewId}", e);
            }
        }

        public virtual Task SendRemoveAnswerCommandAsync(RemoveAnswerCommand command)
        {
            return this.ExecuteCommandAsync(command);
        }

        public async Task ExecuteActionAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default(CancellationToken))
        {
            CancellationTokenSource cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            try
            {
                this.StartInProgressIndicator();

                await this.userInterfaceStateService.WaitWhileUserInterfaceIsRefreshingAsync();

                lock (this.cancellationLockObject)
                {
                    this.TryCancelLastExecutedCommand();
                    this.RegisterCancellationTokenOfCurrentCommand(cancellationTokenSource);
                }

                cancellationTokenSource.Token.ThrowIfCancellationRequested();

                await action(cancellationTokenSource.Token);
            }
            catch (OperationCanceledException) { }
            finally
            {
                lock (this.cancellationLockObject)
                {
                    this.ForgetCancellationToken(cancellationTokenSource);
                }

                this.FinishInProgressIndicator();
            }
        }

        private Task ExecuteCommandAsync(ICommand answerCommand)
        {
            return ExecuteActionAsync(token => this.commandService.ExecuteAsync(answerCommand, cancellationToken: token));
        }

        private void TryCancelLastExecutedCommand()
        {
            this.currentCancellationTokenSource?.Cancel();
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

        public void StartInProgressIndicator()
        {
            Interlocked.Increment(ref this.inProgressDepth);
            this.UpdateInProgressFlag(true);
        }

        public void FinishInProgressIndicator()
        {
            Interlocked.Decrement(ref this.inProgressDepth);
            this.UpdateInProgressFlag(false);
        }

        readonly ActionThrottler trottler = new ActionThrottler();

        private void UpdateInProgressFlag(bool useDelay)
        {
            if (Delay != TimeSpan.Zero && useDelay && this.inProgress == false && this.inProgressDepth > 0)
            {
                this.trottler.RunDelayed(() => this.InProgress = this.inProgressDepth > 0, Delay).ConfigureAwait(false);
            }
            else
            {
                this.InProgress = this.inProgressDepth > 0;
            }
        }

        public TimeSpan Delay { get; set; } = TimeSpan.Zero;
    }
}