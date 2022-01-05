using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MvvmCross;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Utils;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public class AnsweringViewModel : MvxNotifyPropertyChanged, ICompositeEntity
    {
        private readonly ICommandService commandService;
        readonly IUserInterfaceStateService userInterfaceStateService;
        private readonly IMvxMessenger messenger;
        private readonly ILogger logger;

        private int inProgressDepth = 0;

        public Guid Id { get; } = Guid.NewGuid();

        protected AnsweringViewModel() { }

        public AnsweringViewModel(ICommandService commandService,
            IUserInterfaceStateService userInterfaceStateService,
            ILogger logger)
        {
            this.commandService = commandService;
            this.userInterfaceStateService = userInterfaceStateService;
            this.messenger = Mvx.IoCProvider.GetSingleton<IMvxMessenger>();
            this.logger = logger;
        }

        private bool inProgress;
        private readonly object cancellationLockObject = new object();
        private CancellationTokenSource currentCancellationTokenSource;

        public bool InProgress
        {
            get => this.inProgress;
            private set => this.RaiseAndSetIfChanged(ref this.inProgress, value);
        }

        public virtual async Task SendQuestionCommandAsync(QuestionCommand answerCommand)
        {
            Stopwatch commandTime = Stopwatch.StartNew();
            try
            {
                await this.ExecuteCommandAsync(answerCommand);
            }
            catch (Exception e)
            {
                e.Data.Add("Failed to answer question", answerCommand.Question.ToString());
                e.Data.Add("CommandType", answerCommand.GetType());
                e.Data.Add("Interview Id", answerCommand.InterviewId.ToString());
                throw;
            }
            finally
            {
                commandTime.Stop();
                if (commandTime.Elapsed.TotalSeconds > 2)
                {
                    logger.Warn($"Answering {answerCommand.GetType()}: {answerCommand.Question} took {commandTime.Elapsed.TotalMilliseconds}ms");
                }
                messenger.Publish(new AnswerAcceptedMessage(this, commandTime.Elapsed));
            }
        }

        private async Task ExecuteActionAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default)
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
            return ExecuteActionAsync(async token => await this.commandService.ExecuteAsync(answerCommand, cancellationToken: token));
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

        readonly ActionThrottler throttler = new ActionThrottler();

        private void UpdateInProgressFlag(bool useDelay)
        {
            if (Delay != TimeSpan.Zero && useDelay && this.inProgress == false && this.inProgressDepth > 0)
            {
                this.throttler.RunDelayed(() => this.InProgress = this.inProgressDepth > 0, Delay).ConfigureAwait(false);
            }
            else
            {
                this.InProgress = this.inProgressDepth > 0;
            }
        }

        public TimeSpan Delay { get; set; } = TimeSpan.Zero;
    }
}
