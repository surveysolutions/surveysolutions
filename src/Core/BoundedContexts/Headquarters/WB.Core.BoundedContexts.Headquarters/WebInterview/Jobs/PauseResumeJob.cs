using System;
using System.Linq;
using System.Threading.Tasks;
using Quartz;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Exceptions;

namespace WB.Core.BoundedContexts.Headquarters.WebInterview.Jobs
{
    [DisallowConcurrentExecution]
    internal class PauseResumeJob : IJob
    {
        public ILogger Logger => ServiceLocator.Current.GetInstance<ILoggerProvider>().GetFor<PauseResumeJob>();
        
        private IPauseResumeQueue queue;

        public IPauseResumeQueue Queue
        {
            get => queue ?? ServiceLocator.Current.GetInstance<IPauseResumeQueue>();
            set => queue = value;
        }

        public ICommandService CommandService => ServiceLocator.Current.GetInstance<ICommandService>();

#pragma warning disable 1998
        public async Task Execute(IJobExecutionContext context)
#pragma warning restore 1998
        {
            var allCommands = Queue.DeQueueForPublish();

            var transactionManager = ServiceLocator.Current.GetInstance<ITransactionManager>();

            foreach (var interviewCommand in allCommands)
            {
                try
                {
                    transactionManager.BeginCommandTransaction();
                    this.CommandService.Execute(interviewCommand);
                    transactionManager.CommitCommandTransaction();
                }
                catch(InterviewException interviewException) when (interviewException.ExceptionType == InterviewDomainExceptionType.StatusIsNotOneOfExpected)
                {
                }
                catch (Exception e)
                {
                    transactionManager.RollbackCommandTransaction();
                    this.Logger.Error($"Failed to log command {interviewCommand.GetType().Name} for interview {interviewCommand.InterviewId}", e);
                }
            }
        }
    }
}
