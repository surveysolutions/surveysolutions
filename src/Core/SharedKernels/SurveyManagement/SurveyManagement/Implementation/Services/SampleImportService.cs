using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Main.Core.Documents;
using Microsoft.Practices.ServiceLocation;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Storage;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.PreloadedData;
using WB.Core.SharedKernels.SurveyManagement.Views.SampleImport;
using WB.Core.SharedKernels.SurveyManagement.Views.User;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services
{
    internal class SampleImportService : ISampleImportService
    {
        private static readonly Dictionary<string, SampleCreationStatus> preLoadingStatuses = new Dictionary<string, SampleCreationStatus>();

        private readonly IReadSideKeyValueStorage<QuestionnaireDocumentVersioned> questionnaireDocumentVersionedStorage;
        private readonly IReadSideKeyValueStorage<QuestionnaireExportStructure> questionnaireExportStructureStorage;
        private readonly IReadSideKeyValueStorage<QuestionnaireRosterStructure> questionnaireRosterStructureStorage;
        private readonly IPreloadedDataServiceFactory preloadedDataServiceFactory;
        private readonly ITransactionManagerProvider transactionManager;
        private readonly SampleImportSettings sampleImportSettings;

        private static ILogger Logger
        {
            get { return ServiceLocator.Current.GetInstance<ILogger>(); }
        }

        public SampleImportService(IReadSideKeyValueStorage<QuestionnaireDocumentVersioned> questionnaireDocumentVersionedStorage,
            IPreloadedDataServiceFactory preloadedDataServiceFactory,
            IReadSideKeyValueStorage<QuestionnaireExportStructure> questionnaireExportStructureStorage,
            IReadSideKeyValueStorage<QuestionnaireRosterStructure> questionnaireRosterStructureStorage, 
            ITransactionManagerProvider transactionManager,
            SampleImportSettings sampleImportSettings)
        {
            this.questionnaireDocumentVersionedStorage = questionnaireDocumentVersionedStorage;
            this.preloadedDataServiceFactory = preloadedDataServiceFactory;
            this.questionnaireExportStructureStorage = questionnaireExportStructureStorage;
            this.questionnaireRosterStructureStorage = questionnaireRosterStructureStorage;
            this.transactionManager = transactionManager;
            this.sampleImportSettings = sampleImportSettings;
        }

        public void CreatePanel(Guid questionnaireId, long version, string id, PreloadedDataByFile[] data, Guid responsibleHeadquarterId,
            Guid? responsibleSupervisorId)
        {
            preLoadingStatuses[id] = new SampleCreationStatus(id);

            this.CreateInterviewInternal(
                questionnaireId,
                version,
                id,
                preloadedDataService => preloadedDataService.CreatePreloadedDataDtosFromPanelData(data),
                responsibleHeadquarterId,
                responsibleSupervisorId);
        }

        public void CreateSample(Guid questionnaireId, long version, string id, PreloadedDataByFile data, Guid responsibleHeadquarterId,
            Guid? responsibleSupervisorId)
        {
            preLoadingStatuses[id] = new SampleCreationStatus(id);

            this.CreateInterviewInternal(
                questionnaireId, 
                version, 
                id, 
                preloadedDataService => preloadedDataService.CreatePreloadedDataDtoFromSampleData(data), 
                responsibleHeadquarterId, 
                responsibleSupervisorId);
        }

        public SampleCreationStatus GetSampleCreationStatus(string id)
        {
            if (preLoadingStatuses.ContainsKey(id))
                return preLoadingStatuses[id];

            return new SampleCreationStatus() { StatusMessage = "Import not found or completed." };
        }

        void CreateInterviewInternal(Guid questionnaireId,
            long version,
            string id,
            Func<IPreloadedDataService, PreloadedDataRecord[]> preloadedDataDtoCreator,
            Guid responsibleHeadquarterId,
            Guid? responsibleSupervisorId)
        {
            var result = this.GetSampleCreationStatus(id);

            QuestionnaireDocumentVersioned bigTemplateObject;
            QuestionnaireExportStructure questionnaireExportStructure;
            QuestionnaireRosterStructure questionnaireRosterStructure;

            this.transactionManager.GetTransactionManager().BeginQueryTransaction();
            try
            {
                bigTemplateObject =
                    this.questionnaireDocumentVersionedStorage.AsVersioned().Get(questionnaireId.FormatGuid(), version);
                questionnaireExportStructure =
                    this.questionnaireExportStructureStorage.AsVersioned().Get(questionnaireId.FormatGuid(), version);
                questionnaireRosterStructure =
                    this.questionnaireRosterStructureStorage.AsVersioned().Get(questionnaireId.FormatGuid(), version);
            }
            finally
            {
                this.transactionManager.GetTransactionManager().RollbackQueryTransaction();
            }

            var bigTemplate = bigTemplateObject?.Questionnaire;

            if (bigTemplate == null || questionnaireExportStructure == null || questionnaireRosterStructure == null)
            {
                result.SetErrorMessage("Questionnaire is absent");
                //this.preLoadingStatuses.Store(result, id);
                return;
            }

            var preloadedDataService =
                this.preloadedDataServiceFactory.CreatePreloadedDataService(questionnaireExportStructure,
                    questionnaireRosterStructure, bigTemplateObject.Questionnaire);

            result.SetStatusMessage("Data parsing");
            //this.preLoadingStatuses.Store(result, id);

            var interviewForCreate = preloadedDataDtoCreator(preloadedDataService);

            if (interviewForCreate == null)
            {
                result.SetErrorMessage("Data parsing error");
                //this.preLoadingStatuses.Store(result, id);
                return;
            }

            int errorCountOccuredOnInterviewsCreaition = 0;
            bool interviewLimitReached = false;
            DateTime startTime = DateTime.Now;
            int totalInterviewsProcessed = 0;

            var commandInvoker = ServiceLocator.Current.GetInstance<ICommandService>();
            
            var cancellationToken = new CancellationToken();

            Parallel.ForEach(interviewForCreate,
                new ParallelOptions
                {
                    CancellationToken = cancellationToken,
                    MaxDegreeOfParallelism = sampleImportSettings.InterviewsImportParallelTasksLimit
                },
                preloadedDataRecord => {
                    cancellationToken.ThrowIfCancellationRequested();

                    this.CreatePreloadedInterview(preloadedDataRecord, version, id, responsibleHeadquarterId, responsibleSupervisorId, commandInvoker, bigTemplate, ref errorCountOccuredOnInterviewsCreaition);

                    Interlocked.Increment(ref totalInterviewsProcessed);
                    result.SetStatusMessage(string.Format(
                        @"Processed {0} interview(s) out of {1}. Spent time: {2:d\.hh\:mm\:ss}. Total estimated time: {3:d\.hh\:mm\:ss}.",
                        totalInterviewsProcessed,
                        interviewForCreate.Length,
                        DateTime.Now - startTime,
                        CalculateEstimatedTime(totalInterviewsProcessed, interviewForCreate.Length, startTime, DateTime.Now)));

                    //this.preLoadingStatuses.Store(result, id);
                });

            if (errorCountOccuredOnInterviewsCreaition > 0)
            {
                if (interviewLimitReached)
                {
                    result.SetErrorMessage("You can't create more interviews because limit has been reached");
                }
                else
                {
                    result.SetErrorMessage(string.Format("Error{0} occurred during interview creation",
                        errorCountOccuredOnInterviewsCreaition == 1 ? "" : "s"));
                }
            }
            else
                result.CompleteProcess();

            //this.preLoadingStatuses.Store(result, id);
        }

        private void CreatePreloadedInterview(PreloadedDataRecord preloadedDataRecord,long version, string id, Guid responsibleHeadquarterId,
            Guid? responsibleSupervisorId,
            ICommandService commandInvoker, QuestionnaireDocument bigTemplate,
            ref int errorCountOccuredOnInterviewsCreation)
        {
            ThreadMarkerManager.MarkCurrentThreadAsIsolated();
            ThreadMarkerManager.MarkCurrentThreadAsNoTransactional();

            try
            {
                Guid responsibleId = preloadedDataRecord.SupervisorId ?? responsibleSupervisorId.Value;

                commandInvoker.Execute(
                    new CreateInterviewWithPreloadedData(Guid.NewGuid(), responsibleHeadquarterId,
                        bigTemplate.PublicKey, version,
                        preloadedDataRecord.PreloadedDataDto,
                        DateTime.UtcNow,
                        responsibleId));
            }
            catch (Exception e)
            {
                Logger.Error(e.Message, e);

                errorCountOccuredOnInterviewsCreation ++;
                var interviewException = e as InterviewException;
                if (interviewException != null &&
                    interviewException.ExceptionType == InterviewDomainExceptionType.InterviewLimitReached)
                {
                    throw new ArgumentException("interviewLimitReached");
                }
            }
            finally
            {
                ThreadMarkerManager.ReleaseCurrentThreadFromIsolation();
                ThreadMarkerManager.RemoveCurrentThreadFromNoTransactional();
            }
        }

        private static TimeSpan CalculateEstimatedTime(int interviewIndex, int totalInterviews, DateTime startTime, DateTime currentTime)
        {
            double spentMilliseconds = (currentTime - startTime).TotalMilliseconds;
            double estimatedMilliseconds = spentMilliseconds / (interviewIndex + 1) * totalInterviews;

            return TimeSpan.FromMilliseconds(estimatedMilliseconds);
        }
    }
}
