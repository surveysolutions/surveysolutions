using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.Services;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Enumerator.Native.WebInterview;

namespace WB.Core.BoundedContexts.Headquarters.WebInterview.Jobs
{
    [DisallowConcurrentExecution]
    internal class PauseResumeJob : IJob
    {
        private readonly ILogger<PauseResumeJob> logger;
        private readonly IPauseResumeQueue queue;
        private readonly IAggregateRootPrototypeService prototypeService;

        public PauseResumeJob(IPauseResumeQueue queue, IAggregateRootPrototypeService prototypeService, ILogger<PauseResumeJob> logger)
        {
            this.queue = queue;
            this.prototypeService = prototypeService;
            this.logger = logger;
        }

        public Task Execute(IJobExecutionContext context)
        {
            var allCommands = queue.DeQueueForPublish();

            foreach (var interviewCommand in allCommands)
            {
                if(prototypeService.IsPrototype(interviewCommand.InterviewId)) continue;
                
                try
                {
                    InScopeExecutor.Current.Execute(serviceLocatorLocal =>
                    {
                        var commandService = serviceLocatorLocal.GetInstance<ICommandService>();
                        commandService.Execute(interviewCommand);
                    });
                }
                catch(InterviewException interviewException) 
                    when (interviewException.ExceptionType == InterviewDomainExceptionType.StatusIsNotOneOfExpected ||
                          interviewException.ExceptionType == InterviewDomainExceptionType.QuestionnaireIsMissing)
                {
                }
                catch (Exception e)
                {
                    this.logger.LogError(e, "Failed to log command {command} for interview {interviewId}",
                        interviewCommand.GetType().Name,
                        interviewCommand.InterviewId
                    );
                }
            }

            return Task.CompletedTask;
        }
    }
}
