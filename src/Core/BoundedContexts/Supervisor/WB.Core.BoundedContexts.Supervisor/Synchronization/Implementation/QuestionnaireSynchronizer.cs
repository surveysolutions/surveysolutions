using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Documents;
using Ncqrs.Commanding;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.BoundedContexts.Supervisor.Questionnaires;
using WB.Core.BoundedContexts.Supervisor.Synchronization.Atom;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Commands.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.Supervisor.Synchronization.Implementation
{
    internal class QuestionnaireSynchronizer : IQuestionnaireSynchronizer
    {
        private readonly IAtomFeedReader feedReader;
        private readonly HeadquartersSettings settings;
        private readonly HeadquartersPullContext headquartersPullContext;
        private readonly IPlainQuestionnaireRepository plainQuestionnaireRepository;
        private readonly IQueryablePlainStorageAccessor<LocalQuestionnaireFeedEntry> plainStorage;
        private readonly IHeadquartersQuestionnaireReader headquartersQuestionnaireReader;
        private readonly Action<ICommand> executeCommand;
        private readonly ILogger logger;

        public QuestionnaireSynchronizer(IAtomFeedReader feedReader, HeadquartersSettings settings,
            HeadquartersPullContext headquartersPullContext, IQueryablePlainStorageAccessor<LocalQuestionnaireFeedEntry> plainStorage, ILogger logger, IPlainQuestionnaireRepository plainQuestionnaireRepository,

            ICommandService commandService, IHeadquartersQuestionnaireReader headquartersQuestionnaireReader)
        {
            this.feedReader = feedReader;
            this.settings = settings;
            this.headquartersPullContext = headquartersPullContext;
            this.plainStorage = plainStorage;
            this.logger = logger;
            this.plainQuestionnaireRepository = plainQuestionnaireRepository;
            this.headquartersQuestionnaireReader = headquartersQuestionnaireReader;
            this.executeCommand = command => commandService.Execute(command, origin: Constants.HeadquartersSynchronizationOrigin);
            
        }

        public void Pull()
        {
            var lastStoredEntry = this.plainStorage.Query(_ => _.OrderByDescending(x => x.Timestamp).Select(x => x.EntryId).FirstOrDefault());

            IList<AtomFeedEntry<LocalQuestionnaireFeedEntry>> remoteEvents =
                this.feedReader
                    .ReadAfterAsync<LocalQuestionnaireFeedEntry>(this.settings.QuestionnaireChangedFeedUrl, lastStoredEntry)
                    .Result.ToList();

            this.headquartersPullContext.PushMessage(string.Format("Received {0} events from {1} feed", remoteEvents.Count,
                this.settings.QuestionnaireChangedFeedUrl));

            foreach (var remoteEvent in remoteEvents)
            {
                var questionnaireFeedEntry = remoteEvent.Content;

                try
                {
                    if (this.IsQuestionnnaireAlreadyStoredLocally(questionnaireFeedEntry.QuestionnaireId,
                        questionnaireFeedEntry.QuestionnaireVersion))
                        return;

                    string questionnaireDetailsUrl = this.settings.QuestionnaireDetailsEndpoint
                        .Replace("{id}", questionnaireFeedEntry.QuestionnaireId.FormatGuid())
                        .Replace("{version}", questionnaireFeedEntry.QuestionnaireVersion.ToString());
                    this.headquartersPullContext.PushMessage(string.Format("Loading questionnaire using {0} URL", questionnaireDetailsUrl));
                    QuestionnaireDocument questionnaireDocument =
                        this.headquartersQuestionnaireReader.GetQuestionnaireByUri(new Uri(questionnaireDetailsUrl)).Result;

                    this.plainQuestionnaireRepository.StoreQuestionnaire(questionnaireFeedEntry.QuestionnaireId,
                        questionnaireFeedEntry.QuestionnaireVersion, questionnaireDocument);
                    this.executeCommand(new RegisterPlainQuestionnaire(questionnaireFeedEntry.QuestionnaireId,
                        questionnaireFeedEntry.QuestionnaireVersion, questionnaireFeedEntry.AllowCensusMode));
                }
                catch (AggregateException ex)
                {
                    foreach (var inner in ex.InnerExceptions)
                    {
                        this.MarkAsProcessedWithError(questionnaireFeedEntry, inner);
                    }
                }
                catch (Exception ex)
                {
                    this.MarkAsProcessedWithError(questionnaireFeedEntry, ex);
                    this.logger.Error(string.Format("Questionnaire synchronization error in event {0}.", questionnaireFeedEntry.EntryId), ex);
                }
                finally
                {
                    this.plainStorage.Store(questionnaireFeedEntry, questionnaireFeedEntry.EntryId);
                }
            }
        }

        private bool IsQuestionnnaireAlreadyStoredLocally(Guid id, long version)
        {
            QuestionnaireDocument localQuestionnaireDocument = this.plainQuestionnaireRepository.GetQuestionnaireDocument(id, version);

            return localQuestionnaireDocument != null;
        }

        private void MarkAsProcessedWithError(LocalQuestionnaireFeedEntry questionnaireFeedEntry, Exception ex)
        {
            questionnaireFeedEntry.ProcessedWithError = true;
            this.headquartersPullContext.PushError(string.Format("Error while processing event {0}. ErrorMessage: {1}. Exception messages: {2}",
                questionnaireFeedEntry.EntryId, ex.Message, string.Join(Environment.NewLine, ex.UnwrapAllInnerExceptions().Select(x => x.Message))));
        }
    }
}
