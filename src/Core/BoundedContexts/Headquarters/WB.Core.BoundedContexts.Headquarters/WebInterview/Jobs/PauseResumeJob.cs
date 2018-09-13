using System;
using Quartz;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Exceptions;

namespace WB.Core.BoundedContexts.Headquarters.WebInterview.Jobs
{
    [DisallowConcurrentExecution]
    internal class PauseResumeJob : IJob
    {
        public PauseResumeJob(IServiceLocator serviceLocator)
        {
            this.serviceLocator = serviceLocator;
        }
        public ILogger Logger => serviceLocator.GetInstance<ILoggerProvider>().GetFor<PauseResumeJob>();
        
        private IPauseResumeQueue queue;
        private IServiceLocator serviceLocator;

        public IPauseResumeQueue Queue
        {
            get => queue ?? serviceLocator.GetInstance<IPauseResumeQueue>();
            set => queue = value;
        }

        public ICommandService CommandService => serviceLocator.GetInstance<ICommandService>();

        public void Execute(IJobExecutionContext context)
        {
            var allCommands = Queue.DeQueueForPublish();

            foreach (var interviewCommand in allCommands)
            {
                try
                {
                    this.CommandService.Execute(interviewCommand);
                }
                catch(InterviewException interviewException) when (interviewException.ExceptionType == InterviewDomainExceptionType.StatusIsNotOneOfExpected)
                {
                }
                catch (Exception e)
                {
                    this.Logger.Error($"Failed to log command {interviewCommand.GetType().Name} for interview {interviewCommand.InterviewId}", e);
                }
            }
        }
    }
}
