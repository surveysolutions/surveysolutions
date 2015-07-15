using System;
using System.Threading;
using System.Threading.Tasks;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.BoundedContexts.Tester.ViewModels
{
    public class AnsweringViewModel : MvxNotifyPropertyChanged
    {
        private readonly ICommandService commandService;

        private int inProgressDepth = 0;

        protected AnsweringViewModel() { }

        public AnsweringViewModel(ICommandService commandService)
        {
            this.commandService = commandService;
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
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            try
            {
                this.StartInProgressIndicator();

                lock (this.cancellationLockObject)
                {
                    this.TryCancelLastExecutedCommand();
                    this.RegisterCancellationTokenOfCurrentCommand(cancellationTokenSource);
                }

                await this.commandService.ExecuteAsync(answerCommand, cancellationToken: cancellationTokenSource.Token);
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