using Ncqrs.Eventing;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Autofac;
using Microsoft.Extensions.Logging;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Questionnaire.Api;

namespace WB.UI.WebTester.Services.Implementation
{
    public class AppdomainsPerInterviewManager : IAppdomainsPerInterviewManager
    {
        private readonly ILogger<AppdomainsPerInterviewManager> logger;
        private readonly ILifetimeScope rootScope;

        private readonly ConcurrentDictionary<Guid, Lazy<RemoteInterviewContainer>> appDomains = new ConcurrentDictionary<Guid, Lazy<RemoteInterviewContainer>>();

        public AppdomainsPerInterviewManager(
            ILogger<AppdomainsPerInterviewManager> logger,
            ILifetimeScope rootScope)
        {

            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.rootScope = rootScope;
        }
        
        public void SetupForInterview(Guid interviewId,
            QuestionnaireIdentity questionnaireIdentity,
            string supportingAssembly,
            QuestionnaireSettings questionnaireSettings)
        {
            logger.LogDebug($"[SetupForInterview]Creating remote interview: {interviewId} for q: [{questionnaireIdentity}]");
            appDomains.GetOrAdd(interviewId, new Lazy<RemoteInterviewContainer>(() =>
            {
                logger.LogDebug($"[lazy]Creating remote interview: {interviewId} for q: {questionnaireIdentity}");
                return new RemoteInterviewContainer(
                    rootScope,
                    interviewId, 
                    questionnaireIdentity, 
                    supportingAssembly,
                    questionnaireSettings);
            }));
        }
        
        public void TearDown(Guid interviewId)
        {
            logger.LogDebug($"TearDown remote interview: {interviewId}");
            if (appDomains.TryRemove(interviewId, out var remote))
            {
                logger.LogDebug($"TearDown remote interview: {interviewId}. Disposing");
                remote.Value.Dispose();
                logger.LogDebug($"TearDown remote interview: {interviewId}. Disposed");
            }
        }

        public void Flush(Guid interviewId)
        {
            if(appDomains.TryGetValue(interviewId, out var interview))
            {
                interview.Value.Flush();
            }
        }

        public List<CommittedEvent> Execute(ICommand command)
        {
            var interviewCommand = command as InterviewCommand;

            if (interviewCommand == null)
                throw new ArgumentException(nameof(command));

            if (appDomains.TryGetValue(interviewCommand.InterviewId, out var interview))
            {
                logger.LogDebug($"Execute remote interview command: {interviewCommand.InterviewId} # {command.GetType().Name}");
                return interview.Value.Execute(command);
            }
            
            throw new ArgumentException("Cannot execute command. No remote domain were setted up");
        }

        public int? GetLastEventSequence(Guid interviewId)
        {
            if (appDomains.TryGetValue(interviewId, out var interview))
            {
                logger.LogDebug($"Execute remote interview GetLastEventSequence: {interviewId}");
                return interview.Value.GetLastEventSequence();
            }

            throw new ArgumentException("Cannot get last event sequence. No remote domain were setted up");
        }
    }
}
