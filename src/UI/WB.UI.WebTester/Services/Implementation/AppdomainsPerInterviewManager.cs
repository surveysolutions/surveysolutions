using Main.Core.Documents;
using Ncqrs.Eventing;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.Questionnaire.Translations;

namespace WB.UI.WebTester.Services.Implementation
{
    public class AppdomainsPerInterviewManager : IAppdomainsPerInterviewManager
    {
        private readonly string binFolderPath;
        private readonly ILogger logger;

        private readonly ConcurrentDictionary<Guid, Lazy<RemoteInterviewContainer>> appDomains = new ConcurrentDictionary<Guid, Lazy<RemoteInterviewContainer>>();

        public AppdomainsPerInterviewManager(string binFolderPath,
            ILogger logger)
        {
            this.binFolderPath = binFolderPath ?? throw new ArgumentNullException(nameof(binFolderPath));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        public void SetupForInterview(Guid interviewId, 
            QuestionnaireDocument questionnaireDocument,
            List<TranslationDto> translations,
            string supportingAssembly)
        {
            logger.Debug($"[SetupForInterview]Creating remote interview: {interviewId} for q: {questionnaireDocument.Title}[{questionnaireDocument.PublicKey}]");
            appDomains.GetOrAdd(interviewId, new Lazy<RemoteInterviewContainer>(() =>
            {
                logger.Debug($"[lazy]Creating remote interview: {interviewId} for q: {questionnaireDocument.Title}[{questionnaireDocument.PublicKey}]");
                return new RemoteInterviewContainer(interviewId,
                    binFolderPath, questionnaireDocument, translations, supportingAssembly);
            }));
        }
        
        public void TearDown(Guid interviewId)
        {
            logger.Debug($"TearDown remote interview: {interviewId}");
            if (appDomains.TryRemove(interviewId, out var remote))
            {
                logger.Debug($"TearDown remote interview: {interviewId}. Disposing");
                remote.Value.Dispose();
                logger.Debug($"TearDown remote interview: {interviewId}. Disposed");
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
                logger.Debug($"Execute remote interview command: {interviewCommand.InterviewId} # {command.GetType().Name}");
                return interview.Value.Execute(command);
            }
            
            throw new ArgumentException("Cannot execute command. No remote domain were setted up");
        }

        public List<CategoricalOption> GetFirstTopFilteredOptionsForQuestion(Guid interviewId,
            Identity questionIdentity,
            int? parentQuestionValue, string filter, int itemsCount = 200)
        {
            if (appDomains.TryGetValue(interviewId, out var interview))
            {
                logger.Debug($"Execute remote interview GetFirstTopFilteredOptionsForQuestion: {interviewId}");
                return interview.Value.GetFirstTopFilteredOptionsForQuestion(questionIdentity, parentQuestionValue, filter, itemsCount);
            }

            throw new ArgumentException("Cannot get first top filtered options. No remote domain were setted up");
        }
    }
}