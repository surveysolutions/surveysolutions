using System;
using Quartz;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Enumerator.Native.WebInterview;

namespace WB.Core.BoundedContexts.Headquarters.WebInterview.Jobs
{
    [DisallowConcurrentExecution]
    internal class PauseResumeJob : IJob
    {
        private readonly ILogger logger;
        private readonly IPauseResumeQueue queue;

        public PauseResumeJob(IPauseResumeQueue queue, ILogger logger)
        {
            this.queue = queue;
            this.logger = logger;
        }

        public void Execute(IJobExecutionContext context)
        {
            var allCommands = queue.DeQueueForPublish();

            foreach (var interviewCommand in allCommands)
            {
                try
                {
                    InScopeExecutor.Current.ExecuteActionInScope(serviceLocatorLocal =>
                    {
                        var commandService = serviceLocatorLocal.GetInstance<ICommandService>();
                        commandService.Execute(interviewCommand);
                    });
                }
                catch(InterviewException interviewException) when (interviewException.ExceptionType == InterviewDomainExceptionType.StatusIsNotOneOfExpected)
                {
                }
                catch (Exception e)
                {
                    this.logger.Error($"Failed to log command {interviewCommand.GetType().Name} for interview {interviewCommand.InterviewId}", e);
                }
            }
        }
    }
}
