using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Documents;
using Ncqrs.Commanding;

using WB.Core.BoundedContexts.Supervisor.Questionnaires;
using WB.Core.BoundedContexts.Supervisor.Synchronization.Atom;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization;
using WB.Core.SharedKernels.SurveyManagement.Services.DeleteQuestionnaireTemplate;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.BoundedContexts.Supervisor.Synchronization.Implementation
{
    internal class QuestionnaireSynchronizer : IQuestionnaireSynchronizer
    {
        private readonly IAtomFeedReader feedReader;
        private readonly IHeadquartersSettings settings;
        private readonly HeadquartersPullContext headquartersPullContext;
        private readonly IPlainQuestionnaireRepository plainQuestionnaireRepository;
        private readonly IQueryablePlainStorageAccessor<LocalQuestionnaireFeedEntry> plainStorage;
        private readonly IDeleteQuestionnaireService deleteQuestionnaireService;
        private readonly IHeadquartersQuestionnaireReader headquartersQuestionnaireReader;
        private readonly Action<ICommand> executeCommand;
        private readonly ILogger logger;

        public QuestionnaireSynchronizer(IAtomFeedReader feedReader, IHeadquartersSettings settings,
            HeadquartersPullContext headquartersPullContext, IQueryablePlainStorageAccessor<LocalQuestionnaireFeedEntry> plainStorage, ILogger logger, IPlainQuestionnaireRepository plainQuestionnaireRepository,

            ICommandService commandService, IHeadquartersQuestionnaireReader headquartersQuestionnaireReader, IDeleteQuestionnaireService deleteQuestionnaireService)
        {
            this.feedReader = feedReader;
            this.settings = settings;
            this.headquartersPullContext = headquartersPullContext;
            this.plainStorage = plainStorage;
            this.logger = logger;
            this.plainQuestionnaireRepository = plainQuestionnaireRepository;
            this.headquartersQuestionnaireReader = headquartersQuestionnaireReader;
            this.deleteQuestionnaireService = deleteQuestionnaireService;
            this.executeCommand = command => commandService.Execute(command, origin: Constants.HeadquartersSynchronizationOrigin);
            
        }

        public void Pull()
        {
            this.StoreEventsToLocalStorage();

            IEnumerable<LocalQuestionnaireFeedEntry> events =
                this.plainStorage.Query(_ => _.Where(x => !x.Processed));

            this.headquartersPullContext.PushMessage(string.Format("Synchronizing questionnaires. Events count: {0}", events.Count()));

            foreach (var questionnaireFeedEntry in events)
            {
                try
                {
                    switch (questionnaireFeedEntry.EntryType)
                    {
                        case QuestionnaireEntryType.QuestionnaireDeleted:
                            DeleteQuestionnaire(questionnaireFeedEntry.QuestionnaireId,
                                questionnaireFeedEntry.QuestionnaireVersion);
                            break;
                        case QuestionnaireEntryType.QuestionnaireCreated:
                        case QuestionnaireEntryType.QuestionnaireCreatedInCensusMode:

                            if (this.IsQuestionnaireAlreadyStoredLocally(questionnaireFeedEntry.QuestionnaireId,
                                questionnaireFeedEntry.QuestionnaireVersion))
                                break;

                            string questionnaireDetailsUrl = this.settings.QuestionnaireDetailsEndpoint
                                .Replace("{id}", questionnaireFeedEntry.QuestionnaireId.FormatGuid())
                                .Replace("{version}", questionnaireFeedEntry.QuestionnaireVersion.ToString());

                            string questionnaireAssemblyUrl = this.settings.QuestionnaireAssemblyEndpoint
                                .Replace("{id}", questionnaireFeedEntry.QuestionnaireId.FormatGuid())
                                .Replace("{version}", questionnaireFeedEntry.QuestionnaireVersion.ToString());

                            this.headquartersPullContext.PushMessage(string.Format("Loading questionnaire using {0} URL", questionnaireDetailsUrl));

                            QuestionnaireDocument questionnaireDocument = this.headquartersQuestionnaireReader.GetQuestionnaireByUri(new Uri(questionnaireDetailsUrl)).Result;

                            this.headquartersPullContext.PushMessage(string.Format("Loading questionnaire assembly using {0} URL", questionnaireAssemblyUrl));

                            byte[] questionnaireAssembly = this.headquartersQuestionnaireReader.GetAssemblyByUri(new Uri(questionnaireAssemblyUrl)).Result;

                            string questionnaireAssemblyInBase64 = questionnaireAssembly != null ? Convert.ToBase64String(questionnaireAssembly) : null;

                            this.plainQuestionnaireRepository.StoreQuestionnaire(questionnaireFeedEntry.QuestionnaireId,
                                questionnaireFeedEntry.QuestionnaireVersion, questionnaireDocument);

                            this.executeCommand(new RegisterPlainQuestionnaire(questionnaireFeedEntry.QuestionnaireId,
                                questionnaireFeedEntry.QuestionnaireVersion,
                                questionnaireFeedEntry.EntryType == QuestionnaireEntryType.QuestionnaireCreatedInCensusMode,
                                questionnaireAssemblyInBase64));

                            break;
                    }
                    questionnaireFeedEntry.Processed = true;
                    questionnaireFeedEntry.ProcessedWithError = false;
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

        private void StoreEventsToLocalStorage()
        {
            var lastStoredEntry = this.plainStorage.Query(_ => _.OrderByDescending(x => x.Timestamp).Select(x => x.EntryId).FirstOrDefault());

            IList<AtomFeedEntry<LocalQuestionnaireFeedEntry>> remoteEvents =
                this.feedReader
                    .ReadAfterAsync<LocalQuestionnaireFeedEntry>(this.settings.QuestionnaireChangedFeedUrl, lastStoredEntry)
                    .Result.ToList();

            this.headquartersPullContext.PushMessage(string.Format("Received {0} events from {1} feed", remoteEvents.Count,
                this.settings.QuestionnaireChangedFeedUrl));

            var newEvents = new List<LocalQuestionnaireFeedEntry>();
            foreach (AtomFeedEntry<LocalQuestionnaireFeedEntry> remoteEvent in remoteEvents)
            {
                newEvents.Add(remoteEvent.Content);
            }

            this.plainStorage.Store(newEvents.Select(x => Tuple.Create(x, (object)x.EntryId)));
        }

        private void DeleteQuestionnaire(Guid id, long version)
        {
            this.headquartersPullContext.PushMessage(string.Format("Deleting questionnaire '{0}' version '{1}'", id, version));

            deleteQuestionnaireService.DeleteQuestionnaire(id, version, null);
        }

        private bool IsQuestionnaireAlreadyStoredLocally(Guid id, long version)
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
