using Ncqrs.Eventing;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using Autofac;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.UI.WebTester.Services.Implementation
{
    public class AppdomainsPerInterviewManager : IAppdomainsPerInterviewManager
    {
        private readonly string binFolderPath;
        private readonly ILogger<AppdomainsPerInterviewManager> logger;
        private readonly ILifetimeScope rootScope;

        private readonly ConcurrentDictionary<Guid, Lazy<RemoteInterviewContainer>> appDomains = new ConcurrentDictionary<Guid, Lazy<RemoteInterviewContainer>>();

        public AppdomainsPerInterviewManager(IWebHostEnvironment hosting,
            ILogger<AppdomainsPerInterviewManager> logger,
            ILifetimeScope rootScope)
        {

            this.binFolderPath = Path.Combine(hosting.ContentRootPath, "bin");
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.rootScope = rootScope;
        }
        
        public void SetupForInterview(Guid interviewId,
            QuestionnaireIdentity questionnaireIdentity,
            string supportingAssembly)
        {
            logger.LogDebug($"[SetupForInterview]Creating remote interview: {interviewId} for q: [{questionnaireIdentity}]");
            appDomains.GetOrAdd(interviewId, new Lazy<RemoteInterviewContainer>(() =>
            {
                logger.LogDebug($"[lazy]Creating remote interview: {interviewId} for q: {questionnaireIdentity}");
                return new RemoteInterviewContainer(
                    rootScope,
                    interviewId,
                    binFolderPath, 
                    questionnaireIdentity, 
                    supportingAssembly);
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

            if(appDomains.TryGetValue(interviewCommand.InterviewId, out var interview))
            {
                logger.LogDebug($"Execute remote interview command: {interviewCommand.InterviewId} # {command.GetType().Name}");
                return interview.Value.Execute(command);
            }
            
            throw new ArgumentException("Cannot execute command. No remote domain were setted up");
        }

        public List<CategoricalOption> GetFirstTopFilteredOptionsForQuestion(Guid interviewId,
            Identity questionIdentity,
            int? parentQuestionValue, string filter, int itemsCount = 200, int[] excludedOptionIds = null)
        {
            if (appDomains.TryGetValue(interviewId, out var interview))
            {
                logger.LogDebug($"Execute remote interview GetFirstTopFilteredOptionsForQuestion: {interviewId}");
                return interview.Value.statefulInterview.GetFirstTopFilteredOptionsForQuestion(questionIdentity, parentQuestionValue, filter, itemsCount, excludedOptionIds);
            }

            throw new ArgumentException("Cannot get first top filtered options. No remote domain were setted up");
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
